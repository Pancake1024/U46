using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

public class PTFMeshData : ScriptableObject
{
    // figure extrusion
    public Vector3[] figurePoints;
    public Vector2[] uv0;
    public Vector2[] uv1;
    public bool closeFigure = false;

    // mesh cloning
    public Mesh mesh = null;
    public Vector3[] anchorPoints;
    public bool orientMesh = false;
    public bool skipLast = true;

    // curve segment
    public float spaceStep = 1.0f;
    public float spaceBegin = 0.0f;
    public float spaceEnd = 0.0f;
    public float maxError = 1.0e-2f;
}
