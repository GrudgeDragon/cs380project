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
    private Stack<UnityEngine.Random.State> seeds = new Stack<UnityEngine.Random.State>(); // used for seeding rand consistantly

    // these fields are custom to this mesh generator
    public float HelixHeight = 12;
    public float HelixMajorRadius = 4;
    public float HelixMinorRadius = 2;
    public float HelixTurns = 1;
    public float SegmentLength = 1; // aproximate length of spine between segments
    int minorSubdivisions = 10;     // number of verts in the rings
    public bool RecursiveVine = false;
    public Transform flowerPrefab;
    public Transform spinePrefab;
    public String flower;
    public int seedFudge;
    public AnimationCurve ConeXCurve;
    public AnimationCurve ConeYCurve;
    public AnimationCurve ConeZCurve;
    private List<Transform> spawnedObjects = new List<Transform>();
    private List<Transform> spawnedSpines = new List<Transform>();

    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        int spawnCounter = 0;
        int spineCounter = 0;
        MakeASweetAssHelix(Matrix4x4.identity, 
            HelixMajorRadius, 
            HelixMinorRadius, 
            HelixHeight, 
            HelixTurns, 
            true,
            RecursiveVine, 
            SegmentLength, minorSubdivisions,
            ref spawnCounter, 0, 1, 1, ref spineCounter);
        MeshBuilder.PostBuild(mesh);
    }

    void Start()
    {
        Vector3 startPosition = transform.position;
        UnityEngine.Random.InitState((int)(startPosition.x + startPosition.y + startPosition.z + seedFudge));
        seeds.Push(UnityEngine.Random.state);
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


    void MakeASweetAssHelix(Matrix4x4 vineToModel, float majorRadius, float minorRadius, float height, float revolutions, bool recursive, bool usingRecursive, float segmentLength, int minorSubdivisions, ref int spawnCounter, float startingUVHeight, float endUVHeight, float flip, ref int spineCounter)
    {
        UnityEngine.Random.state = seeds.Peek();

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

        //int numSegments = 1 + (int)(totalArcLen / segmentLength); // number of segments in the helix
        //float spd = height / numSegments; // vertical speed

        float effecitveSegmentLength = segmentLength;
        if (effecitveSegmentLength > totalArcLen / 5)
            effecitveSegmentLength = totalArcLen / 5;

        // for each ring that will make up the helix
        for (float i = 0; i <= totalArcLen + effecitveSegmentLength; i += effecitveSegmentLength)
        {
            // t is our main paremeter      
            float t = Mathf.Min(i / totalArcLen, 1);
            float tc = 1 - t; // compliment of t

            // radians of revolution
            float r = flip * t * arc;

            // this f(x) and `f(x) are used for determining the radius. f(x) is an arbitrary function. outputs [0,1] for t [0,1]
            float x = flip * (-(t * t) + 2 * t); //+ RandomTools.Gaussian(0.0f, -tc, tc, tc/4.0f);
            float dx = flip * -2 * t + 2;

            // helix equation and its derivitive are used to get position and direction. we make the helix start straight up by applying t to minimize early twisting.
            Vector3 pos = new Vector3(flip * t * x * majorRadius * Mathf.Cos(r),         t * height, flip * t * x * majorRadius * Mathf.Sin(r));
            Vector3 dir = new Vector3(flip * t * dx * majorRadius * -Mathf.Sin(r) * arc, height,     flip * t * dx * majorRadius * Mathf.Cos(r) * arc);

            
            //pos = pos + RandomTools.Gaussian(0.0f, t-0.1f, t+0.1f, t/4.0f) * Vector3.up;

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos); // position in helix
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, flip * Vector3.up)); // direction of helix
            Matrix4x4 helixToModel = vineToModel * trans * rotation; 

            // make spikes and other decorators
            for (int j = 0; j < minorSubdivisions; ++j)
            {
                // hard break if we have too many verts
                if (MeshBuilder.VertList.Count > 65000)
                {
                    Debug.LogError("Too many verts");
                    // break out of double nested loops
                    i = totalArcLen + 1;
                    break;
                }


                // ~15% chance to make a decorator, less likely at the begining
                float chanceToSkip = 0.85f;
                float roll = UnityEngine.Random.value;
                if (roll < chanceToSkip)
                    continue;

                roll = (roll - chanceToSkip) / (1 - chanceToSkip);

                float u = ((float)j / minorSubdivisions) * 2 * Mathf.PI; // normalized param

                float fudge = 0.7f; // becase the bases of cones and helices aren't tight, have to pull them back in a bit
                //fudge = 1.0f; //todo: put fudge back

                // the normal in helix space
                Vector3 surfaceNormal = new Vector3(tc * minorRadius * fudge * Mathf.Cos(u),
                                                    tc * minorRadius * fudge * Mathf.Sin(u)); // direction spike will point
                //Vector4 surfacePos = surfaceNormal;
                //surfacePos.w = 1;

                Matrix4x4 surfaceToModel = helixToModel * Matrix4x4.Translate(surfaceNormal) *
                                           Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, surfaceNormal));
                // start bad
                // exists in vine space space
                Vector3 normal_model = helixToModel * surfaceNormal; // put the spike direction into local space
                // Vector3 adjustedNormal_model = normal_model + 1.2f*Vector3.up;
                
                // up in this space is normal to the vine
                Matrix4x4 normalRot = Matrix4x4.Rotate(Quaternion.LookRotation(normal_model, dir));
                Matrix4x4 normalToModel = vineToModel * trans * normalRot; // normal space to model space
                // end bad
           
                if (usingRecursive && recursive && roll < 0.1) // doesn't use spawn new vines in the bottom third
                {
                    seeds.Push(UnityEngine.Random.state);
                    MakeASweetAssHelix(surfaceToModel, majorRadius * tc, minorRadius * tc * tc * 0.75f, height * tc * tc * 0.5f, revolutions * tc * tc * 0.5f, false, false, 
                        effecitveSegmentLength / 2 * tc, minorSubdivisions, ref spawnCounter, startingUVHeight, Mathf.Lerp(startingUVHeight, endUVHeight, tc), -1, ref spineCounter);
                    UnityEngine.Random.state = seeds.Pop();
                    roll = 1;
                }
                else
                {
                    roll = (roll - 0.1f) / 0.9f;
                }

                // if we should attempt to make a flower instead of a spike
                if (roll < 0.1f)
                {
                    float s;// = 0.2f * minorRadius * tc;
                    s = (-20 * (t - 0.75f) * (t - 0.75f) + 1) * 0.1f * minorRadius;
                    if (s > 0)
                    {
                        //Debug.Log("size: " + s);
                        // get the object to spawn
                        Transform other;
                        if (spawnedObjects.Count < spawnCounter + 1)
                        {
                            // spawn one
                            other = Instantiate(flowerPrefab, Vector3.zero, Quaternion.identity, transform);
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
                        //d = transform.localToWorldMatrix * normalToModel * Vector3.up;

                        // set rotation of spawned object
                        other.rotation = Quaternion.LookRotation(d, Vector3.up);

                        // set position of spawned object
                        Vector4 otherPos = (pos + (normal_model * 1 / fudge));
                        otherPos.w = 1;
                        other.position = transform.localToWorldMatrix * vineToModel * otherPos;
                        //other.position = spineSpace * new Vector4(0,0,0,1);

                        // set scale of spawned object

                        other.localScale = new Vector3(s, s, s);
                        if (!(s > 0))
                            other.gameObject.SetActive(false);
                        else
                            other.gameObject.SetActive(true);

                    }
                }
                else
                {
                    //MeshBuilder.MakeCone(surfaceToModel, 0.25f * minorRadius * tc, 2 * minorRadius * tc, tc*tc, 1);

                    Transform other;
                    if (spawnedSpines.Count < spineCounter + 1)
                    {
                        // spawn one
                        other = Instantiate(spinePrefab, Vector3.zero, Quaternion.identity, transform);
                        spawnedSpines.Add(other);
                    }
                    else
                    {
                        // recycle one from the pool
                        other = spawnedSpines[spineCounter];
                        spawnedSpines[spineCounter].gameObject.SetActive(true);
                    }
                    spineCounter++;

                    Vector3 d = normal_model;
                    d = transform.localToWorldMatrix * normal_model; // works on first vine, not recursive vine
                    //d = transform.localToWorldMatrix * normalToModel * Vector3.up;

                    // set rotation of spawned object
                    other.rotation = Quaternion.LookRotation(d, Vector3.up);

                    fudge = 1;
                    // set position of spawned object
                    Vector4 otherPos = (pos + (normal_model * fudge));
                    otherPos.w = 1;
                    other.position = transform.localToWorldMatrix * vineToModel * otherPos;
                    //other.position = spineSpace * new Vector4(0,0,0,1);

                    // set scale of spawned object
                    float s = 5.0f * minorRadius * tc;
                    other.localScale = new Vector3(s, s, s);
                }
            }


            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = MeshBuilder.AddRing(helixToModel, minorSubdivisions, minorRadius * tc, Mathf.Lerp(startingUVHeight, endUVHeight, tc));

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

        // disable any unused objects
        for (int i = spineCounter; i < spawnedSpines.Count; ++i)
        {
            spawnedSpines[i].gameObject.SetActive(false);
        }
    }


}
