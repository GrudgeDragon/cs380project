using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    // temp lists to hold the verts and faces
    public List<Vector3> vertList;
    public List<int> faceList;

    void Start()
    {
        // get the mesh and reset empty it of data
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // some variables for tweaking
        int ringVertCount = 10;     // number of verts in the rings
        int numHeightSegments = 60; // number of rings that make up the helix
        float tubeWidth = 1f;       // minor radius
        float heightStep = 0.2f;    // distance to next ring in the helix
        float helixRadius = 2;      // major radius
 
        // index of the last ring added to the mesh
        int prevRing = -1;

        // for each ring that will make up the helix
        for (int i = 0; i < numHeightSegments; ++i)
        {
            // t is our main paremeter
            float t = i * heightStep;

            // helix equation and its derivitive are used to get position and direction
            Vector3 pos = new Vector3(helixRadius * Mathf.Cos(t), t, helixRadius * Mathf.Sin(t));
            Vector3 dir = new Vector3(helixRadius * -Mathf.Sin(t), 0, helixRadius * Mathf.Cos(t));

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up, dir));

            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = AddRing(trans * rotation, ringVertCount, tubeWidth);

            // if this is not the first ring
            if (prevRing != -1)
                AddBand(prevRing, ring, ringVertCount); // make a band of triangles from this and the last ring

            prevRing = ring;
        }
       
        // assign to the mesh
        mesh.vertices = vertList.ToArray();
        mesh.triangles = faceList.ToArray();
        mesh.RecalculateNormals(); // without this we get no light

    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    int AddRing(Matrix4x4 trans, int numPoints, float radius)
    {
        //Debug.Log(trans);

        // remember the index of the first vert
        int firstVert = vertList.Count;

        // how much to increment current angle at
        float angleIncrement = Mathf.PI * 2 / numPoints;

        // start at half increment instead of 0 if even number of verts
        float currentAngle = (numPoints % 2 != 0) ? 0 : angleIncrement / 2;

        for (int i = 0; i < numPoints; ++i)
        {
            Vector4 pos = new Vector4(radius * Mathf.Cos(currentAngle), 0, radius * Mathf.Sin(currentAngle), 1);
            pos = trans * pos;
            vertList.Add(pos);
            currentAngle += angleIncrement;
        }

        // return the index of the first new vert
        return firstVert;

    }

    // creates a band of quads between two sets of points
    int AddBand(int topEdge, int bottomEdge, int num)
    {
        // get what will be the index of the first face to be added
        int firstFace = faceList.Count;

        // for quad that will make up the band
        for (int i = 0; i < num; ++i)
        {
            // first quad triangle
            faceList.Add(topEdge + i);
            faceList.Add(bottomEdge + i);
            faceList.Add(bottomEdge + (i + 1) % num);

            // second quad triangle
            faceList.Add(topEdge + i);
            faceList.Add(bottomEdge + (i + 1) % num);
            faceList.Add(topEdge + (i + 1) % num);
        }
        return firstFace;
    }




}

