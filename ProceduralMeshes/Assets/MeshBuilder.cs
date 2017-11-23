using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour
{

    public static List<Vector3> VertList = new List<Vector3>();
    public static List<int> FaceList = new List<int>();

    // creates a ring in the x-y plane, perpindicular to unity's forward direction, z
    public static int AddRing(Matrix4x4 trans, int numPoints, float radius)
    {
        // remember the index of the first vert
        int firstVert = VertList.Count;

        // how much to increment current angle at
        float angleIncrement = Mathf.PI * 2 / numPoints;

        // start at half increment instead of 0 if even number of verts
        float currentAngle = (numPoints % 2 != 0) ? 0 : angleIncrement / 2;

        for (int i = 0; i < numPoints; ++i)
        {
            Vector4 pos = new Vector4(radius * Mathf.Cos(currentAngle), radius * Mathf.Sin(currentAngle), 0, 1);
            pos = trans * pos;
            VertList.Add(pos);
            currentAngle += angleIncrement;
        }
        // return the index of the first new vert
        return firstVert;

    }

    /* creates a band of quads between two sets of points.
       Be carelful, top and bottom edges are not arbitrary, you may flip the normals the wrong way */
    public static int AddBand(int topEdge, int bottomEdge, int num)
    {
        // get what will be the index of the first face to be added
        int firstFace = FaceList.Count;

        // for quad that will make up the band
        for (int i = 0; i < num; ++i)
        {
            // first quad triangle
            FaceList.Add(topEdge + i);
            FaceList.Add(bottomEdge + i);
            FaceList.Add(bottomEdge + (i + 1) % num);

            // second quad triangle
            FaceList.Add(topEdge + i);
            FaceList.Add(bottomEdge + (i + 1) % num);
            FaceList.Add(topEdge + (i + 1) % num);
        }
        return firstFace;
    }

    // make a cone along the forward vector I think
    public static void MakeCone(Matrix4x4 localSpace, float baseRadius, float height)
    {
        Vector4 starPos = Vector3.zero;
        Vector4 dir = Vector3.up;
        int heightSubdivisions = 3;
        int radialSubdivisions = 6;

        starPos.w = 1;
        dir.Normalize();

        int prevRing = -1;

        for (int i = 0; i < heightSubdivisions; ++i)
        {
            // normalized parameter
            float t = (float)i / heightSubdivisions;

            // get the current position along the height of the cone
            Vector3 pos = starPos + t * height * dir;

            // use position and direction to create our local matrices
            Matrix4x4 trans = Matrix4x4.Translate(pos);
            Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.LookRotation(dir, Vector3.up));

            // add a ring to the mesh
            int ring; // index of the first vert in a ring
            ring = AddRing(localSpace * trans * rotation, radialSubdivisions, (1 - t) * baseRadius);

            // if this is not the first ring
            if (prevRing != -1)
                AddBand(ring, prevRing, radialSubdivisions); // make a band of triangles from this and the last ring

            prevRing = ring;
        }

        // cap the cone
        int tipVert = VertList.Count;
        Vector4 /*just the*/ tip = localSpace * starPos + localSpace * dir * height;
        VertList.Add(tip);
        AddFanCone(tipVert, prevRing, radialSubdivisions, false);
    }




    // not tested
    public static int AddFan(int baseVert, int first, int numSpines, bool flip)
    {
        int firstFace = FaceList.Count;
        for (int i = 0; i < numSpines - 1; ++i)
        {
            if (flip)
            {
                FaceList.Add(first + i);
                FaceList.Add(baseVert);
                FaceList.Add(first + i + 1);

            }
            else
            {
                FaceList.Add(baseVert);
                FaceList.Add(first + i);
                FaceList.Add(first + i + 1);
            }
        }
        return firstFace;
    }

    public static int AddFanCone(int baseVert, int first, int numSpines, bool flip = false)
    {
        int firstFace = FaceList.Count;
        for (int i = 0; i < numSpines; ++i)
        {
            if (flip)
            {
                FaceList.Add(first + i);
                FaceList.Add(baseVert);
                FaceList.Add(first + (i + 1) % numSpines);
            }
            else
            {
                FaceList.Add(baseVert);
                FaceList.Add(first + i);
                FaceList.Add(first + (i + 1) % numSpines);
            }
        }
        return firstFace;
    }

}
