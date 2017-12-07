using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

interface IMeshDef
{
    Vector4 ParameterizedPosition(float t);
    Vector4 ParameterizedDirection(float t);
    float ParameterizedRadius(float t);
    int MajorSubdivisions { get; }
    int MinorSubdivisions { get; }
    bool HasParameterizedDirection { get; }
}
