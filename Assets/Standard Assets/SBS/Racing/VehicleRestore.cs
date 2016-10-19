using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Racing/VehicleRestore")]
public class VehicleRestore : MonoBehaviour
{
    #region Public members
    public float maxAngle = 30.0f;
    public float restoreStrength = 800.0f;
    public float restoreDamping = 0.4f;
    #endregion

    #region Protected members
    protected Rigidbody rigidBody = null;
    #endregion

    #region Unity callbacks
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        SBSVector3 upVec = rigidBody.rotation * Vector3.up;
        float angle = SBSVector3.Angle(upVec, SBSVector3.up);
        SBSVector3 axis = SBSVector3.Cross(upVec, SBSVector3.up);
//      axis.y = 0.0f;
        if (angle > maxAngle && axis.Normalize() > 0.0f)
        {
            rigidBody.AddTorque((angle - maxAngle) * restoreStrength * axis, ForceMode.Force);
            GetComponent<Rigidbody>().AddTorque(-SBSVector3.Dot(GetComponent<Rigidbody>().angularVelocity, axis) * SBSMath.Pow(1.0f - restoreDamping, Time.fixedDeltaTime) * axis, ForceMode.VelocityChange);
        }
    }
    #endregion
}
