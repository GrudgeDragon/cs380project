using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour {

    // All mesh generators should have these
    public bool RebuildLive = false;
    public bool Rebuild = false;
    private int seed; // used for seeding rand consistantly

    public float Radius = 2;
    public int HeightDivisions = 10;
    public int RadialSubdivisions = 10;

    void MakeASphere()
    {
        int prev = -1;
        for (int i = 1; i < HeightDivisions; ++i)
        {
            float t = (float) i / HeightDivisions;
            float r = Mathf.Sin(t * Mathf.PI) * Radius;
            //r = Radius;
            float h = Mathf.Cos(t * Mathf.PI) * Radius;
            Matrix4x4 trans = Matrix4x4.Translate(Vector3.forward * h);
            int ring = MeshBuilder.AddRing(trans, RadialSubdivisions, r, t);

            if (prev != -1)
                MeshBuilder.AddBand(prev, ring,   RadialSubdivisions);
            prev = ring;
        }

        // endcaps
        Vector3 begining = new Vector3(0, 0, Radius);
        MeshBuilder.VertList.Add(begining);
        MeshBuilder.UVList.Add(new Vector2(0, 0));
        MeshBuilder.AddFanCone(MeshBuilder.VertList.Count - 1, 0, RadialSubdivisions); // 0 is index of first ring

        Vector3 end = new Vector3(0, 0, -Radius);
        MeshBuilder.VertList.Add(end);
        MeshBuilder.UVList.Add(new Vector2(1, 1));
        MeshBuilder.AddFanCone(MeshBuilder.VertList.Count - 1, prev, RadialSubdivisions, true); // prev is index of last ring
    }

    // this gets called when we want to to build a mesh
    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        MakeASphere(); // custom mesh building function
        MeshBuilder.PostBuild(mesh);
    }

    // this gets called when object is spawned
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

}
