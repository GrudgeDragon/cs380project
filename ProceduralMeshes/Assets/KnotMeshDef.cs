using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[ExecuteInEditMode]
class KnotMeshDef : MonoBehaviour, IMeshDef
{
    public float m;
    public float n;
    public float p;
    public float q;
    public float h;
    public float v;
    public float tMax;
    public KnotMeshDef()
    {
        // Simple one
        /*
        m = 1.0f;
        n = 1.5f;
        h = 0.35f;
        p = 1.0f;
        q = -2.0f;
        v = 3.0f;
        */

        // P dank
        m = 1;
        n = 0.25f;
        h = 0.35f;
        p = 2;
        q = -7;
        v = 9;
        tMax = 2 * Mathf.PI;
    }

    public Vector4 ParameterizedPosition(float t)
    {
        float theta = t * tMax;
        return new Vector4(
            m * Mathf.Cos(p * theta) + n * Mathf.Cos(q * theta),
            m * Mathf.Sin(p * theta) + n * Mathf.Sin(q * theta),
            h * Mathf.Sin(v * theta),
            1);
    }

    public Vector4 ParameterizedDirection(float t)
    {
        float theta = t * tMax;
        return new Vector4(
            -m * p * Mathf.Sin(p * theta) - n * q * Mathf.Sin(q * theta),
             m * p * Mathf.Cos(p * theta) + n * q * Mathf.Cos(q * theta),
             h * v * Mathf.Cos(v * theta),
             1);
    }
    public float ParameterizedRadius(float t)
    {
        return 0.2f;
    }

    public int MajorSubdivisions { get { return 100; } }
    public int MinorSubdivisions { get { return 10; } }
    public bool HasParameterizedDirection { get { return true; } }
}
