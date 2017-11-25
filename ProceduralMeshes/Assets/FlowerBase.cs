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
    public float Push = 1.8f;
    public float Twist = -0.64f;
    public float Length = 2.91f;
    public float Width = 2.91f;
    public float ProfileFactor = 1;


    void MakePetal(Matrix4x4 local, float length, float width)
    {
        // front and back of petal
        MeshBuilder.AddLatticeWithCurves(local, length, width, ProfileFactor, HeightDivisions, WidthDivisions, false, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve);
        MeshBuilder.AddLatticeWithCurves(local, length, width, ProfileFactor, HeightDivisions, WidthDivisions, true, LeafWidthCurve, LeafCrossSectionCurve, LeafCrossSectionHeight, LeafCurve);
    }

    void MakeFlowerBase()
    {
        int numPetals = 8;
        for (int i = 0; i < numPetals; ++i)
        {
            float r = 2 * Mathf.PI * (float)i / numPetals;
            float rp = 2 * Mathf.PI * (float)(i + Twist) / numPetals;
            Vector3 up = new Vector3(Mathf.Cos(r), Mathf.Sin(r));
            Matrix4x4 rot =
                    Matrix4x4.Rotate(Quaternion.AngleAxis(Twist, up))
                    * Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.forward, up))
                    * Matrix4x4.Rotate(Quaternion.AngleAxis(Push, Vector3.left))
                ;
            MakePetal(rot, Length, Width);
        }
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
