using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Assertions.Must;

public class MeshTester : MonoBehaviour
{
    // Note, Y is up, Z is forward

    [Header("Helix Parameters")]
    public float HelixHeight = 12;
    public float HelixMajorRadius = 4;
    public float HelixMinorRadius = 2;
    public float HelixTurns = 1;
    public float SegmentLength = 1; // aproximate length of spine between segments
    int minorSubdivisions = 10;     // number of verts in the rings

    public bool RebuildLive = false;
    public bool Rebuild = false;
    public bool RecursiveVine = false;


    public AnimationCurve XCurve;
    public AnimationCurve YCurve;
    public AnimationCurve ZCurve;
    //public AnimationCurve RadiusCurve;
    public int numSegments = 50;
    public float height = 5;
    public float Factor = 1;
    public float ConeRadius = 1;

    private int seed; // used for seeding rand consistantly

    void GenerateCurvySpine()
    {


        int prevRing = -1;
        //Vector3 prevPos = Vector3.zero;
        for (int i = 0; i < numSegments; ++i)
        {
            float t = (float) i / numSegments;
            float tp = (float) (i + 1) / numSegments; // `t, next t 
            float tc = 1 - t; // compliment of t

            // use position and direction to create our local matrices
            Vector3 pos = Vector3.up * t * height;
            Vector3 dir = Vector3.up * t * height;
            if (dir == Vector3.zero)
                dir = Vector3.up;

            float radius = tc * ConeRadius;

            pos = Factor * new Vector3(XCurve.Evaluate(t), YCurve.Evaluate(t), ZCurve.Evaluate(t));
            dir =  new Vector3(XCurve.Evaluate(tp) - XCurve.Evaluate(t), YCurve.Evaluate(tp) - YCurve.Evaluate(t), ZCurve.Evaluate(tp) - ZCurve.Evaluate(t));
            Vector3 crossDir = new Vector3(dir.z, dir.y, dir.x);
            
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation;// = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up));
            rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, dir));

            Matrix4x4 local = trans * rotation;

            // add a ring to the mesh
            int ring; // index of the first vert in a ring
 
            ring = MeshBuilder.AddRing(local, minorSubdivisions, radius);


            // if this is not the first ring
            if (prevRing != -1)
                MeshBuilder.AddBand(ring, prevRing, minorSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;
        }
    }

    // this is the function where we scrap everything and rebuild from scratch
    void GenerateSweetNewMesh()
    {
        Vector3 pos = GetComponent<Transform>().position;
        UnityEngine.Random.seed = seed;

        // get the mesh and reset empty it of data
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        MeshBuilder.VertList.Clear();
        MeshBuilder.FaceList.Clear();

        Helix.MakeASweetAssHelix(Matrix4x4.identity, HelixMajorRadius, HelixMinorRadius, HelixHeight, HelixTurns, true, RecursiveVine, SegmentLength, minorSubdivisions);
        //GenerateCurvySpine();

        // assign to the mesh
        mesh.vertices = MeshBuilder.VertList.ToArray();
        mesh.triangles = MeshBuilder.FaceList.ToArray();
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

    

    

}

