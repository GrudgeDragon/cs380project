using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPetals : MonoBehaviour
{
    // All mesh generators should have these
    public bool RebuildLive = false;
    public bool Rebuild = false;
    private int seed; // used for seeding rand consistantly

    // these fields are custom to this mesh generator
    public AnimationCurve LeafWidthCurve;
    public AnimationCurve LeafCrossSectionCurve;
    public AnimationCurve LeafCrossSectionHeight;
    public AnimationCurve LeafCurve;
    public int WidthDivisions = 15;
    public int HeightDivisions = 15;
    public float InnerPush = 1.8f;
    public float MidPush = 3.69f;
    public float OutterPush = 7.82f;
    public float InnerTwist = -0.64f;
    public float MidTwist = 1.42f;
    public float OuterTwist = 1;
    public float InnerLength = 2.91f;
    public float MidLength = 3.01f;
    public float OuterLength = 3.14f;


    void MakePetal(Matrix4x4 local, float length)
    {
        // front and back of petal
        MeshBuilder.AddLatticeWithCurves(local, length, HeightDivisions, WidthDivisions, false, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve);
        MeshBuilder.AddLatticeWithCurves(local, length, HeightDivisions, WidthDivisions, true, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve);
    }

    void MakeBombAssFlower()
    {
        int numPetals = 8;
        for (int i = 0; i < numPetals; ++i)
        {
            float r = 2 * Mathf.PI * (float)i / numPetals;
            float rp = 2 * Mathf.PI * (float)(i + InnerTwist) / numPetals;
            Vector3 lookPos = new Vector3(-Mathf.Cos(rp), -Mathf.Sin(rp), InnerPush);
            Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.LookRotation(lookPos, up));
            MakePetal(rot, InnerLength);
        }

        float f = 1.2f; // scale factor
        numPetals = 13;
        for (int i = 0; i < numPetals; ++i)
        {
            float r = 2 * Mathf.PI * (i + 0.33f) / numPetals;
            float rp = 2 * Mathf.PI * (float)(i + MidTwist) / numPetals;
            Vector3 lookPos = new Vector3(-Mathf.Cos(rp), -Mathf.Sin(rp), MidPush);
            Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
            MakePetal(Matrix4x4.Rotate(Quaternion.LookRotation(lookPos, up)) * Matrix4x4.Scale(new Vector3(f, f, f)), MidLength);
        }

        f = 1.4f; // scale factor
        numPetals = 21;
        for (int i = 0; i < numPetals; ++i)
        {
            float r = 2 * Mathf.PI * (i + 0.66f) / numPetals;
            float rp = 2 * Mathf.PI * (float)(i + OuterTwist) / numPetals;
            Vector3 lookPos = new Vector3(-Mathf.Cos(rp), -Mathf.Sin(rp), OutterPush);
            Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
            MakePetal(Matrix4x4.Rotate(Quaternion.LookRotation(lookPos, up)) * Matrix4x4.Scale(new Vector3(f, f, f)), OuterLength);
        }
    }

    // this gets called when we want to to build a mesh
    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        MakeBombAssFlower(); // custom mesh building function
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
