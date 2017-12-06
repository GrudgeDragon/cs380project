using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvyCone : MonoBehaviour
{
    // All mesh generators should have these
    public bool RebuildLive = false;
    public bool Rebuild = false;
    private int seed;

    // these fields are custom to this mesh generator
    public AnimationCurve XCurve;
    public AnimationCurve YCurve;
    public AnimationCurve ZCurve;
    public int numSegments = 50;
    public float Height = 5;
    public float Factor = 1;
    public float ConeRadius = 1;
    public int minorSubdivisions = 10;

    // builds the mesh
    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        GenerateCurvySpine(); // custom mesh building function goes here
        MeshBuilder.PostBuild(mesh);
    }
    
    // gets called when the object is created
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

    void GenerateCurvySpine()
    {


        int prevRing = -1;
        //Vector3 prevPos = Vector3.zero;
        for (int i = 0; i < numSegments; ++i)
        {
            float t = (float)i / numSegments;
            float tp = (float)(i + 1) / numSegments; // `t, next t 
            float tc = 1 - t; // compliment of t

            // use position and direction to create our local matrices
            Vector3 pos = Vector3.up * t * Height;
            Vector3 dir;

            float radius = tc * ConeRadius;

            pos = Factor * new Vector3(XCurve.Evaluate(t), YCurve.Evaluate(t), ZCurve.Evaluate(t));
            dir = new Vector3(XCurve.Evaluate(tp) - XCurve.Evaluate(t), YCurve.Evaluate(tp) - YCurve.Evaluate(t),
                ZCurve.Evaluate(tp) - ZCurve.Evaluate(t));

            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation; // = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up));
            rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, dir));

            Matrix4x4 local = trans * rotation;

            // add a ring to the mesh
            int ring; // index of the first vert in a ring

            ring = MeshBuilder.AddRing(local, minorSubdivisions, radius, 0);


            // if this is not the first ring
            if (prevRing != -1)
                MeshBuilder.AddBand(ring, prevRing,
                    minorSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;

        }

    }
}
