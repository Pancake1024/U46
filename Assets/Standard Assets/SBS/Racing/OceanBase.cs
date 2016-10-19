using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/OceanBase")]
public class OceanBase : MonoBehaviour, IOcean
{
    public virtual float GetWaterHeightAtLocation(float x, float y)
    {
        return 0.0f;
    }
}
