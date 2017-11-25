using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helix : MonoBehaviour
{
    // All mesh generators should have these
    public bool RebuildLive = false;
    public bool Rebuild = false;
    private int seed; // used for seeding rand consistantly

    // these fields are custom to this mesh generator
    public float HelixHeight = 12;
    public float HelixMajorRadius = 4;
    public float HelixMinorRadius = 2;
    public float HelixTurns = 1;
    public float SegmentLength = 1; // aproximate length of spine between segments
    int minorSubdivisions = 10;     // number of verts in the rings
    public bool RecursiveVine = false;


    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        UnityEngine.Random.seed = seed;
        MakeASweetAssHelix(Matrix4x4.identity, 
            HelixMajorRadius, HelixMinorRadius, 
            HelixHeight, HelixTurns, true,
            RecursiveVine, SegmentLength, minorSubdivisions);
        MeshBuilder.PostBuild(mesh);
    }

    void Start()
    {
        Vector3 startPosition = GetComponent<Transform>().position;
        seed = (int)(startPosition.x + startPosition.y + startPosition.z);
        GenerateMesh();
    }

    // this gets called every frame
    void Update()
    {
        if (RebuildLive)
            GenerateMesh();

        if (Rebuild)
        {
            GenerateMesh();
            Rebuild = false;
        }
    }


    public static void MakeASweetAssHelix(Matrix4x4 helixSpace, float majorRadius, float minorRadius, float height, float revolutions, bool recursive, bool usingRecursive, float segmentLength, int minorSubdivisions)
    {
        // some variables for tweaking. These can be made properties for the editor, or parameters for the function


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

        int numSegments = 1 + (int)(totalArcLen / segmentLength); // number of segments in the helix
        float spd = height / numSegments; // vertical speed

        // for each ring that will make up the helix
        for (int i = 0; i <= numSegments; ++i)
        {
            // t is our main paremeter      
            float t = (float)i / numSegments + 0.001f;
            float tc = 1 - t; // compliment of t

            // radians of revolution
            float r = t * arc;

            // this f(x) and `f(x) are used for determining the radius. f(x) is an arbitrary function. outputs [0,1] for t [0,1]
            float x = (-(t * t) + 2 * t);
            float dx = -2 * t + 2;

            // helix equation and its derivitive are used to get position and direction. we make the helix start straight up by applying t to minimize early twisting.
            Vector3 pos = new Vector3(t * x * majorRadius * Mathf.Cos(r), t * height, t * x * majorRadius * Mathf.Sin(r));
            Vector3 dir = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height, t * dx * majorRadius * Mathf.Cos(r) * arc);

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up));
            Matrix4x4 local = helixSpace * trans * rotation;

            // make spikes
            for (int j = 0; j < minorSubdivisions; ++j)
            {
                // hard break if we have too many verts
                if (MeshBuilder.VertList.Count > 65000)
                {
                    Debug.LogError("Too many verts");
                    // break out of double nested loops
                    i = numSegments + 1;
                    break;
                }


                // but not all the spikes
                if (UnityEngine.Random.value > 0.15)
                    continue;

                float u = ((float)j / minorSubdivisions) * 2 * Mathf.PI; // normalized param

                float fudge = 0.8f; // becase the bases of cones and helices aren't tight, have to pull them back in a bit
                Vector3 subDir = new Vector3(tc * minorRadius * fudge * Mathf.Cos(u),
                                             tc * minorRadius * fudge * Mathf.Sin(u)); // direction spike will point

                //Vector3 randDir = UnityEngine.Random.insideUnitSphere * 0.75f; // just a dash of random
                subDir = local * subDir; // put the spike direction into local space

                Matrix4x4 subRot = Matrix4x4.Rotate(Quaternion.LookRotation(dir, subDir));
                Matrix4x4 spineSpace = helixSpace * Matrix4x4.Translate(subDir) * trans * subRot;
                if (usingRecursive && recursive && UnityEngine.Random.value > .9f && t > 0.3) // doesn't use spawn new vines in the bottom third
                {
                    MakeASweetAssHelix(spineSpace, majorRadius * tc, minorRadius * tc * tc, height * tc, revolutions, false, usingRecursive, segmentLength, minorSubdivisions);
                }
                else
                {
                    MeshBuilder.MakeCone(spineSpace, 0.25f * minorRadius * tc, 2 * minorRadius * tc);
                }
            }


            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = MeshBuilder.AddRing(local, minorSubdivisions, minorRadius * tc);

            // if this is not the first ring
            if (prevRing != -1)
                MeshBuilder.AddBand(ring, prevRing, minorSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;
        }
    }


}
