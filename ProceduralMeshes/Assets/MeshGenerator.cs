using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    //public Vector3[] verts;
    public List<Vector3> vertList;
    //public Vector2[] uvs;
    //public int[] triangles;
    public List<int> faceList;

    void Start()
    {
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        int ringVertCount = 10;
        int numHeightSegments = 60;
        float tubeWidth = 1f;
        float heightStep = 0.2f;
        //Matrix4x4 rotateBy = Matrix4x4.Rotate(Quaternion.AngleAxis(5, new Vector3(1,0,0)));
        Matrix4x4 rotation = Matrix4x4.identity;
        float helixRadius = 2;
 
        int prevRing = -1;
        for (int i = 0; i < numHeightSegments; ++i)
        {
            float t = i * heightStep;
            Vector3 pos = new Vector3(helixRadius * Mathf.Cos(t), t, helixRadius * Mathf.Sin(t));
            Vector3 dir = new Vector3(helixRadius * -Mathf.Sin(t), 0, helixRadius * Mathf.Cos(t));
            rotation = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up, dir));
            
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            int ring = AddRing(trans * rotation, ringVertCount, tubeWidth);
            //secondRing = AddRing(Matrix4x4.Translate(new Vector3(0, i + 1, 0)) * rotation, ringVertCount, 1);
            if (prevRing != -1)
                AddBand(prevRing, ring, ringVertCount);

            prevRing = ring;
            //rotation *= rotateBy;
        }

 

        Vector3[] thing = (vertList.ToArray());
        mesh.vertices = thing;
        mesh.triangles = faceList.ToArray();
        mesh.RecalculateNormals();

        Debug.Log("Hi!");

    }

    // Update is called once per frame
    void Update()
    {

    }

    int AddRing(Matrix4x4 trans, int numPoints, float radius)
    {
        Debug.Log(trans);

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
    int AddBand(int topEdge, int bottomEdge, int num)
    {
        int firstFace = faceList.Count;
        for (int i = 0; i < num; ++i)
        {
            faceList.Add(topEdge + i);
            faceList.Add(bottomEdge + i);
            faceList.Add(bottomEdge + (i + 1) % num);
            faceList.Add(topEdge + i);
            faceList.Add(bottomEdge + (i + 1) % num);
            faceList.Add(topEdge + (i + 1) % num);
        }
        return firstFace;


    }




}

