using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlowerBase : MonoBehaviour {

    // All mesh generators should have these
    public bool RebuildLive = false;
    public bool Rebuild = false;
    private int seed;

    // these fields are custom to this mesh generator
    public AnimationCurve LeafWidthCurve;
    public AnimationCurve LeafCrossSectionCurve;
    public AnimationCurve LeafCrossSectionHeight;
    public AnimationCurve LeafCurve;
    public int WidthDivisions = 15;
    public int HeightDivisions = 15;

    public FlowerPetals.PetalAttributes Leaves;

    public float Push = 1.8f;
    public float Twist = -0.64f;
    public float Length = 2.91f;
    public float Width = 2.91f;
    public float ProfileFactor = 1;
    public float BendFactor = 1;

    void MakePetals(FlowerPetals.PetalAttributes petal)
    {
        for (int i = 0; i < petal.NumPetals; ++i)
        {
            float r = 2 * Mathf.PI * (float)i / petal.NumPetals;
            float t = (float)i / petal.NumPetals;
            Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
            Matrix4x4 trans = Matrix4x4.Rotate(Quaternion.AngleAxis(petal.Push, up))
                              * Matrix4x4.Rotate(Quaternion.AngleAxis(t * 360.0f, Vector3.forward))
                              * Matrix4x4.Rotate(Quaternion.AngleAxis(petal.Twist, Vector3.up))
                              * Matrix4x4.Scale(new Vector3(petal.Scale, petal.Scale, petal.Scale));
            // front and back of petal
            MeshBuilder.AddLatticeWithCurves(trans, petal.Length, 1, ProfileFactor, HeightDivisions, WidthDivisions, false, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve, BendFactor);
            MeshBuilder.AddLatticeWithCurves(trans, petal.Length, 1, ProfileFactor, HeightDivisions, WidthDivisions, true, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve, BendFactor);
        }

    }

    void MakeFlowerBase()
    {
        MakePetals(Leaves);

        //int numPetals = 8;
        //for (int i = 0; i < numPetals; ++i)
        //{
        //    float r = 2 * Mathf.PI * (float)i / numPetals;
        //    float rp = 2 * Mathf.PI * (float)(i + Twist) / numPetals;
        //    Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
        //    Matrix4x4 rot =
        //            Matrix4x4.Rotate(Quaternion.AngleAxis(Twist, up))
        //            * Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, up))
        //            * Matrix4x4.Rotate(Quaternion.AngleAxis(Push, Vector3.left))
        //        ;
        //    MakePetal(rot, Length, Width);
        //}
    }

    // this gets called when we want to to build a mesh
    void GenerateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MeshBuilder.PreBuild(mesh);
        MakeFlowerBase(); // custom mesh building function
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
