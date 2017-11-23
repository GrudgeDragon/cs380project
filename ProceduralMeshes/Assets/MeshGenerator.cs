using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class MeshGenerator : MonoBehaviour
{
    // Note, Y is up, Z is forward


    // temp lists to hold the verts and faces
    public List<Vector3> VertList;
    public List<int> FaceList;
    public float HelixHeight = 12;
    public float HelixMajorRadius = 4;
    public float HelixMinorRadius = 2;
    public float HelixTurns = 1;
    public float SegmentLength = 1; // aproximate length of spine between segments
    public bool RebuildLive = false;
    public bool Rebuild = false;
    public bool RecursiveVine = false;
    public AnimationCurve TestCurve;

    private int seed;

    void GenerateCurvySpine()
    {

    }

    // this is the function where we scrap everything and rebuild from scratch
    void GenerateSweetNewMesh()
    {
        Vector3 pos = GetComponent<Transform>().position;
        UnityEngine.Random.seed = seed;

        // get the mesh and reset empty it of data
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        VertList.Clear();
        FaceList.Clear();

        MakeASweetAssHelix(Matrix4x4.identity, HelixMajorRadius, HelixMinorRadius, HelixHeight, HelixTurns, true);

        // assign to the mesh
        mesh.vertices = VertList.ToArray();
        mesh.triangles = FaceList.ToArray();
        mesh.RecalculateNormals(); // without this we get no light
    }

    // this gets called once at the begining
    void Start()
    {
        Vector3 startPosition = GetComponent<Transform>().position;
        seed = (int)(startPosition.x + startPosition.y + startPosition.z);
        GenerateSweetNewMesh();
    }

    // this gets called every frame
    void Update()
    {
        if (RebuildLive)
            GenerateSweetNewMesh();

        if (Rebuild)
        {
            GenerateSweetNewMesh();
            Rebuild = false;
        }
    }

    void MakeASweetAssHelix(Matrix4x4 helixSpace, float majorRadius, float minorRadius, float height, float revolutions, bool recursive)
    {
        // some variables for tweaking. These can be made properties for the editor, or parameters for the function
        int ringVertCount = 10;     // number of verts in the rings

        int prevRing = -1;// index of the last ring added to the mesh
        float arc = revolutions * 2 * Mathf.PI; // the entire arc 

        // calculate the length of the spine of the vine
        Vector3 a, b;
        float numHalfTurns = revolutions * 2;
        {
            // this is a copy past of the helix stuff used below
            float t = 0; // start of interval
            float r = t * arc;
            float x = (-(t * t) + 2 * t);
            float dx = -2 * t + 2;
            a = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height, t * dx * majorRadius * Mathf.Cos(r) * arc);
        }
        {
            float t = 1.0f / numHalfTurns; // end of interval
            float r = t * arc;
            float x = (-(t * t) + 2 * t);
            float dx = -2 * t + 2;
            b = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height, t * dx * majorRadius * Mathf.Cos(r) * arc);
        }
        float totalArcLen = numHalfTurns * (b - a).magnitude; // fundamental theory of calculus Sf(x)dx = F(b)-F(a)
        
        int numSegments = 1 + (int)(totalArcLen / SegmentLength); // number of segments in the helix
        float spd = height / numSegments; // vertical speed
        // Debug.Log(a);
        // Debug.Log(b);
        // Debug.Log(totalArcLen);

        // for each ring that will make up the helix
        for (int i = 0; i <= numSegments; ++i)
        {
            // t is our main paremeter      
            float t = (float) i / numSegments + 0.001f;
            float tc = 1 - t; // compliment of t

            // radians of revolution
            float r = t * arc;
 
            // this f(x) and `f(x) are used for determining the radius. f(x) is an arbitrary function. outputs [0,1] for t [0,1]
            float x = (-(t*t) + 2*t);
            float dx = -2 * t + 2;

            // helix equation and its derivitive are used to get position and direction. we make the helix start straight up by applying t to minimize early twisting.
            Vector3 pos = new Vector3(t * x * majorRadius *  Mathf.Cos(r),        t * height,   t * x * majorRadius * Mathf.Sin(r));
            Vector3 dir = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height,       t * dx * majorRadius * Mathf.Cos(r) * arc);

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation( dir, Vector3.up));
            Matrix4x4 local = helixSpace * trans * rotation;

            // make spikes
            for (int j = 0; j < ringVertCount; ++j)
            {
                // hard break if we have too many verts
                if (VertList.Count > 65000)
                {
                    Debug.LogError("Too many verts");
                    // break out of double nested loops
                    i = numSegments + 1;
                    break;
                }


                // but not all the spikes
                if (UnityEngine.Random.value > 0.15)
                    continue;

                float u = ((float) j / ringVertCount) * 2 * Mathf.PI; // normalized param

                float fudge = 0.8f; // becase the bases of cones and helices aren't tight, have to pull them back in a bit
                Vector3 subDir = new Vector3(tc * minorRadius * fudge * Mathf.Cos(u), 
                                             tc * minorRadius * fudge * Mathf.Sin(u)); // direction spike will point

                //Vector3 randDir = UnityEngine.Random.insideUnitSphere * 0.75f; // just a dash of random
                subDir = local * subDir; // put the spike direction into local space

                Matrix4x4 subRot = Matrix4x4.Rotate(Quaternion.LookRotation(dir, subDir));
                Matrix4x4 spineSpace = helixSpace * Matrix4x4.Translate(subDir) * trans * subRot;
                if (RecursiveVine && recursive && UnityEngine.Random.value > .9f && t > 0.3) // doesn't use spawn new vines in the bottom third
                {
                    MakeASweetAssHelix(spineSpace, majorRadius * tc, minorRadius * tc * tc, height * tc, revolutions, false);
                }
                else
                {
                    MakeCone(spineSpace, 0.25f * minorRadius * tc, 2 * minorRadius * tc);
                }
            }


            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = AddRing(local, ringVertCount, minorRadius * tc);
            
            // if this is not the first ring
            if (prevRing != -1)
                AddBand(ring, prevRing, ringVertCount); // make a band of triangles from this and the last ring

            prevRing = ring;
        }
    }

    // make a cone along the forward vector I think
    void MakeCone(Matrix4x4 localSpace, float baseRadius, float height)
    {
        Vector4 starPos = Vector3.zero;
        Vector4 dir = Vector3.up;
        int heightSubdivisions = 3;
        int radialSubdivisions = 6;

        starPos.w = 1;
        dir.Normalize();

        int prevRing = -1;

        for (int i = 0; i < heightSubdivisions; ++i)
        {
            // normalized parameter
            float t = (float) i / heightSubdivisions;

            // get the current position along the height of the cone
            Vector3 pos = starPos + t * height * dir;

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up));

            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = AddRing(localSpace * trans * rotation, radialSubdivisions, (1 - t) * baseRadius);

            // if this is not the first ring
            if (prevRing != -1)
                AddBand(ring, prevRing, radialSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;
        }

        // cap the cone
        int tipVert = VertList.Count;
        Vector4 /*just the*/ tip = localSpace * starPos + localSpace * dir * height;
        VertList.Add(tip);
        AddFanCone(tipVert, prevRing, radialSubdivisions, false);
    }


    // creates a ring in the x-y plane, perpindicular to unity's forward direction, z
    int AddRing(Matrix4x4 trans, int numPoints, float radius)
    {
        // remember the index of the first vert
        int firstVert = VertList.Count;

        // how much to increment current angle at
        float angleIncrement = Mathf.PI * 2 / numPoints;

        // start at half increment instead of 0 if even number of verts
        float currentAngle = (numPoints % 2 != 0) ? 0 : angleIncrement / 2;

        for (int i = 0; i < numPoints; ++i)
        {
            Vector4 pos = new Vector4(radius * Mathf.Cos(currentAngle), radius * Mathf.Sin(currentAngle), 0, 1);
            pos = trans * pos;
            VertList.Add(pos);
            currentAngle += angleIncrement;
        }
        // return the index of the first new vert
        return firstVert;

    }

    /* creates a band of quads between two sets of points.
       Be carelful, top and bottom edges are not arbitrary, you may flip the normals the wrong way */
    int AddBand(int topEdge, int bottomEdge, int num)
    {
        // get what will be the index of the first face to be added
        int firstFace = FaceList.Count;

        // for quad that will make up the band
        for (int i = 0; i < num; ++i)
        {
            // first quad triangle
            FaceList.Add(topEdge + i);
            FaceList.Add(bottomEdge + i);
            FaceList.Add(bottomEdge + (i + 1) % num);

            // second quad triangle
            FaceList.Add(topEdge + i);
            FaceList.Add(bottomEdge + (i + 1) % num);
            FaceList.Add(topEdge + (i + 1) % num);
        }
        return firstFace;
    }

    // not tested
    int AddFan(int baseVert, int first, int numSpines, bool flip)
    {
        int firstFace = FaceList.Count;
        for (int i = 0; i < numSpines - 1; ++i)
        {
            if (flip)
            {
                FaceList.Add(first + i);
                FaceList.Add(baseVert);
                FaceList.Add(first + i + 1);

            }
            else
            {
                FaceList.Add(baseVert);
                FaceList.Add(first + i);
                FaceList.Add(first + i + 1);
            }
        }
        return firstFace;
    }

    int AddFanCone(int baseVert, int first, int numSpines, bool flip = false)
    {
        int firstFace = FaceList.Count;
        for (int i = 0; i < numSpines; ++i)
        {
            if (flip)
            {
                FaceList.Add(first + i);
                FaceList.Add(baseVert);
                FaceList.Add(first + (i + 1) % numSpines);
            }
            else
            {
                FaceList.Add(baseVert);
                FaceList.Add(first + i);
                FaceList.Add(first + (i + 1) % numSpines);
            }
        }
        return firstFace;
    }

}

