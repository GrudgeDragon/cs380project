using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ConeMeshDef : MonoBehaviour, IMeshDef
{
    // Radius of the base of the cone
    public float BaseRadius = 2.0f;
    // Height of the cone
    public float Height = 5.0f;
    // Power used in radius calculation
    public float CurvePow = 1.5f;

    public float OffsetIntensity = 0.3f;
    public float OffsetCurvePow = 1.5f;
    public int MajorDivs = 100;
    public int MinorDivs = 10;

    public Vector4 ParameterizedPosition(float t)
    {
        var offsetVec = Vector3.forward;
        return new Vector4(
                0, 
                t*Height, 
                0, 1) 
        + OffsetIntensity * Mathf.Pow(t, OffsetCurvePow) * new Vector4(
                offsetVec.x, 
                offsetVec.y, 
                offsetVec.z, 
                1);
    }

    public Vector4 ParameterizedDirection(float t)
    {
        // Disabled...
        return Vector4.zero;
        //return new Vector4(0, 1.0f, 0, 0);
    }

    public float ParameterizedRadius(float t)
    {
        return BaseRadius * Mathf.Pow((1 - t), CurvePow);
    }

    public int MajorSubdivisions { get { return MajorDivs; } }
    public int MinorSubdivisions { get { return MinorDivs; } }
    public bool HasParameterizedDirection { get { return false; } }
}