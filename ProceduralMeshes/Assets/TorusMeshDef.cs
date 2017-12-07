using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TorusMeshDef : MonoBehaviour, IMeshDef
{
    // http://www.mi.sanu.ac.rs/vismath/taylor2009/index.html
    public Vector4 ParameterizedPosition(float t)
    {
        return new Vector4(0, 5*Mathf.Sin(t*2*Mathf.PI), 5*Mathf.Cos(t* 2 * Mathf.PI), 1);
    }

    public Vector4 ParameterizedDirection(float t)
    {
        return new Vector4(0, 2 * Mathf.PI*5 * Mathf.Cos(t*2*Mathf.PI), -2 * Mathf.PI*5 * Mathf.Sin(t* 2 * Mathf.PI), 0);
    }

    public float ParameterizedRadius(float t)
    {
        return 1;
    }

    public int MajorSubdivisions
    {
        get { return 10; }
    }

    public int MinorSubdivisions
    {
        get { return 10; }
    }

    public bool HasParameterizedDirection
    {
        get { return true; }
    }
}
