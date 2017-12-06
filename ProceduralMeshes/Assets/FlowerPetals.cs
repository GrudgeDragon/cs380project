using System;
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
    public AnimationCurve LeafBendCurve;
    public int WidthDivisions = 15;
    public int HeightDivisions = 15;

    [Serializable]
    public struct PetalAttributes
    {
        public int NumPetals;
        public float Push;
        public float Twist;
        public float Length;
        public float Scale;
    }

    public PetalAttributes InnerPetals;
    public PetalAttributes MidPetals;
    public PetalAttributes OuterPetals;

    public float ProfileFactor = 1;
    public float BendFactor = 1;



    void MakePetals(PetalAttributes petal)
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
            MeshBuilder.AddLatticeWithCurves(trans, petal.Length, 1, ProfileFactor, HeightDivisions, WidthDivisions, false, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafBendCurve, BendFactor);
            MeshBuilder.AddLatticeWithCurves(trans, petal.Length, 1, ProfileFactor, HeightDivisions, WidthDivisions, true, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafBendCurve, BendFactor);
        }

    }

    void MakeBombAssFlower()
    {
        MakePetals(InnerPetals);
        MakePetals(MidPetals);
        MakePetals(OuterPetals);
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
