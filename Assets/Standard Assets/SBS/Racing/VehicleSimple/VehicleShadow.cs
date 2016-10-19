using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Racing/SimplePhysics/VehicleShadow")]
public class VehicleShadow : MonoBehaviour
{
    #region Public members
    public WheelRay wheelRay;
    public float offset = 0.05f;
    #endregion

    #region Unity callbacks
    void Update()
    {
        if (null == wheelRay)
            return;

        SBSVector3 pos = SBSVector3.zero, norm = SBSVector3.zero;

        int numWheels = wheelRay.positions.Length;
        if (0 == numWheels)
            return;

        for (int i = wheelRay.positions.Length - 1; i >= 0; --i)
        {
            pos += wheelRay.GetGroundPosition(i);
            norm += wheelRay.GetGroundNormal(i);
        }

        float oon = 1.0f / (float)numWheels;
        pos *= oon;
        norm *= oon;
        pos += norm * offset;

        transform.position = pos;
        SBSVector3 worldForward = wheelRay.transform.TransformDirection(Vector3.forward);
        worldForward -= norm * SBSVector3.Dot(worldForward, norm);
        transform.LookAt(pos + worldForward, norm);
    }
    #endregion
}
