using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    public Transform flowerPrefab;
    public String flower;
    private List<Transform> spawnedObjects = new List<Transform>();

    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        UnityEngine.Random.seed = seed;
        int spawnCounter = 0;
        MakeASweetAssHelix(Matrix4x4.identity, 
            HelixMajorRadius, HelixMinorRadius, 
            HelixHeight, HelixTurns, true,
            RecursiveVine, SegmentLength, minorSubdivisions,
            ref spawnCounter);
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


    void MakeASweetAssHelix(Matrix4x4 vineToModel, float majorRadius, float minorRadius, float height, float revolutions, bool recursive, bool usingRecursive, float segmentLength, int minorSubdivisions, ref int spawnCounter)
    {
        //int spawnCounter = 0; // this is used for chaching spawned prefabs

        int prevRing = -1;// index of the last ring added to the mesh
        float arc = revolutions * 2 * Mathf.PI; // the entire arc 

        // calculate the length of the spine of the vine
        Vector3 a, b;
        float numHalfTurns = revolutions * 2;
        {
            // this is a copy past of the helix stuff used below
            float t = 0; // start of interval
            float r = t * arc;
            float dx = -2 * t + 2;
            a = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height, t * dx * majorRadius * Mathf.Cos(r) * arc);
        }
        {
            float t = 1.0f / numHalfTurns; // end of interval
            float r = t * arc;
            float dx = -2 * t + 2;
            b = new Vector3(t * dx * majorRadius * -Mathf.Sin(r) * arc, height, t * dx * majorRadius * Mathf.Cos(r) * arc);
        }
        float totalArcLen = numHalfTurns * (b - a).magnitude; // fundamental theory of calculus Sf(x)dx = F(b)-F(a)

        int numSegments = 1 + (int)(totalArcLen / segmentLength); // number of segments in the helix
        //float spd = height / numSegments; // vertical speed

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
            Matrix4x4 trans = Matrix4x4.Translate(pos); // position in helix
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up)); // direction of helix
            Matrix4x4 helixToModel = vineToModel * trans * rotation; 

            // make spikes and other decorators
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


                // ~15% chance to make a decorator, less likely at the begining
                float roll = UnityEngine.Random.value;
                if (roll > 0.15 || roll > t * t)
                    continue;

                float u = ((float)j / minorSubdivisions) * 2 * Mathf.PI; // normalized param

                float fudge = 0.8f; // becase the bases of cones and helices aren't tight, have to pull them back in a bit
                //fudge = 1.0f; //todo: put fudge back

                // the normal in helix space
                Vector3 normal_helix = new Vector3(tc * minorRadius * fudge * Mathf.Cos(u),
                                                    tc * minorRadius * fudge * Mathf.Sin(u)); // direction spike will point
                // exists in vine space space
                Vector3 normal_model = helixToModel * normal_helix; // put the spike direction into local space

                // up in this space is normal to the vine
                Matrix4x4 normalRot = Matrix4x4.Rotate(Quaternion.LookRotation(dir, normal_model));
                Matrix4x4 normalToModel = vineToModel * trans * normalRot; // normal space to model space

                if (usingRecursive && recursive && UnityEngine.Random.value > .9f && t > 0.33f) // doesn't use spawn new vines in the bottom third
                {
                    MakeASweetAssHelix(normalToModel, majorRadius * tc, minorRadius * tc * tc, height * tc, revolutions, false, usingRecursive, segmentLength, minorSubdivisions, ref spawnCounter);
                }
                else if (UnityEngine.Random.value > .75f && t > 0.66f)
                {
                    Transform tran = GetComponent<Transform>();

                    // get the object to spawn
                    Transform other;
                    if (spawnedObjects.Count < spawnCounter + 1)
                    {
                        // spawn one
                        other = Instantiate(flowerPrefab, Vector3.zero, Quaternion.identity, tran);
                        spawnedObjects.Add(other);
                    }
                    else
                    {
                        // recycle one from the pool
                        other = spawnedObjects[spawnCounter];
                        spawnedObjects[spawnCounter].gameObject.SetActive(true);
                    }
                    spawnCounter++;

                    Vector3 d = normal_model;
                    d = transform.localToWorldMatrix * normal_model; // works on first vine, not recursive vine
                    d = transform.localToWorldMatrix * normalToModel * Vector3.up; 

                    // set rotation of spawned object
                    other.rotation = Quaternion.LookRotation(d, Vector3.up);

                    // set position of spawned object
                    Vector4 otherPos = (pos + (normal_model * 1/fudge));
                    otherPos.w = 1;
                     other.position = tran.localToWorldMatrix * vineToModel * otherPos;
                    //other.position = spineSpace * new Vector4(0,0,0,1);

                    // set scale of spawned object
                    float s = 0.2f * minorRadius * tc ;
                    other.localScale = new Vector3(s,s,s);
                }
                else
                {
                    MeshBuilder.MakeCone(normalToModel, 0.25f * minorRadius * tc, 2 * minorRadius * tc);
                }
            }


            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = MeshBuilder.AddRing(helixToModel, minorSubdivisions, minorRadius * tc);

            // if this is not the first ring
            if (prevRing != -1)
                MeshBuilder.AddBand(ring, prevRing, minorSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;
        }

        // disable any unused objects
        for (int i = spawnCounter; i < spawnedObjects.Count; ++i)
        {
            spawnedObjects[i].gameObject.SetActive(false);
        }

    }


}
