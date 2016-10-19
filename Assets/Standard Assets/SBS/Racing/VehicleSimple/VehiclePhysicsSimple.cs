using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("SBS/Racing/SimplePhysics/VehiclePhysicsSimple")]
public class VehiclePhysicsSimple : MonoBehaviour, IVehiclePhysics
{
    #region Public enums
    public enum Version
    {
        v0 = 0, // old version before On The Run 2013
        v1
    }
    #endregion

    #region Public members
    public Version version = Version.v0;

    public float frontTraction = 0.2f;
    public float rearTraction = 0.8f;

    public float gravity = 12.0f;
    public float traction = 3000.0f;
    public float drag = 4.0f;
    public float friction = 0.4f;

    public float backTraction = 1500.0f;
    public float backDrag = 4.0f;

    public Vector3 frontPoint;
    public float frontWeight;
    public bool frontWheelsOnGround = true;
    public Vector3 rearPoint;
    public float rearWeight;
    public bool rearWheelsOnGround = true;

    public float slipAnglePeak = 9.0f;
    public float maxGripFront = 3.0f;
    //public float gripSlopeFront = 0.0f;
    //public float minGripFront = 0.0f;
    public float maxGripRear = 3.0f;
    //public float gripSlopeRear = 0.0f;
    //public float minGripRear = 0.0f;

    public bool airTraction = false;
    public bool airGrip = false;

    public float linVelRestThreshold = 3.5f;
    public float angVelRestThreshold = 0.4f;

    public float steeringAngleHi = 30.0f;
    public float steeringAngleMinSpeed = 30.0f;
    public float steeringAngleLo = 15.0f;
    public float steeringAngleMaxSpeed = 120.0f;

    public bool rotateInPlace = true;
    public float rotateInPlaceMaxVelocity = 1.0f;
    public float rotateInPlaceAcc = 4.0f;
    public float rotateInPlaceDec = 4.0f;
    public float latForceSpeedMult = 0.25f;

    public bool isWaterVehicle = false;
    /*
    public AnimationCurve gripCurve;
    public bool useGripCurve = false;*/
    #endregion

    #region Protected members
    protected Rigidbody rb = null;
    protected SBSMatrix4x4 rbMatrix;

    protected float currentSpeed = 0.0f;
    protected float absCurrentSpeed = 0.0f;

    protected float latForceFront = 0.0f;
    protected float latForceRear = 0.0f;

    protected float maxSteeringAngle = 0.0f;

    protected float throttle = 0.0f;
    protected float brake = 0.0f;
    protected float steering = 0.0f;

    protected VehicleInputs[] inputs = null;

    protected SBSVector3 prevSpeed;

    protected SBSVector3[] lastForces = new SBSVector3[2];
    protected SBSVector3[] lastTorques = new SBSVector3[2];
    #endregion

    #region Public properties
    public VehicleType Type
    {
        get
        {
            return isWaterVehicle ? VehicleType.WaterRacing : VehicleType.SimpleRacing;
        }
    }

    public float Mass
    {
        get
        {
            return rb.mass;
        }
    }

    public SBSVector3 Velocity
    {
        get
        {
            return rb.velocity;
        }
    }

    public SBSVector3 PrevSpeed
    {
        get
        {
            return prevSpeed;
        }
    }

    public float Gravity
    {
        get
        {
            return gravity;
        }
    }

    public SBSMatrix4x4 Transform
    {
        get
        {
            return rbMatrix;
        }
    }

    public bool IsOnGround
    {
        get
        {
            return frontWheelsOnGround || rearWheelsOnGround;
        }
    }

    public float Throttle
    {
        get
        {
            return throttle - brake;
        }

        set
        {
            if (value >= 0.0f)
            {
                throttle = SBSMath.Min(1.0f, value);
                brake = 0.0f;
            }
            else
            {
                throttle = 0.0f;
                brake = SBSMath.Min(1.0f, -value);
            }
        }
    }

    public float Steering
    {
        get
        {
            return steering;
        }
        set
        {
            steering = Mathf.Clamp(value, -1.0f, 1.0f);
        }
    }

    public float LateralForceFront
    {
        get
        {
            return latForceFront;
        }
    }

    public float LateralForceRear
    {
        get
        {
            return latForceRear;
        }
    }

    public float AerodynamicDragCoeff
    {
        get
        {
            return friction * gravity * rb.mass;
        }
    }

    public float AerodynamicDownforceCoeff
    {
        get
        {
            return 0.0f;
        }
    }

    public float LongitudinalFrictionCoeff
    {
        get
        {
            float normalForce = gravity * rb.mass;
            return (traction / normalForce);
        }
    }

    public float LateralFrictionCoeff
    {
        get
        {
            return (maxGripFront + maxGripRear) * 0.5f;
        }
    }

    public float BestSteeringAngle
    {
        get
        {
            return slipAnglePeak;
        }
    }

    public float MaxSteeringAngle
    {
        get
        {
            return maxSteeringAngle;
        }
    }

    public float CurrentSpeed
    {
        get
        {
            return currentSpeed;
        }
    }
    #endregion

    #region Protected methods
    protected void HandleInputs()
    {
        foreach (VehicleInputs input in inputs)
        {
            if (input.enabled)
            {
                input.UpdateInputs();

                this.Throttle = input.Throttle;
                this.Steering = input.Steering;
            }
        }
    }
    #endregion

    #region Public methods
    public void AccumForces(float dt, ref SBSVector3 force, ref SBSVector3 torque)
    {
        prevSpeed = rb.velocity;

        this.HandleInputs();

        rbMatrix = SBSMatrix4x4.TRS(rb.position, rb.rotation, SBSVector3.one);
        if (isWaterVehicle)
        {
            SBSVector3 rbForward = rbMatrix.MultiplyVector(SBSVector3.forward);
            rbForward.y = 0.0f;
            rbMatrix = SBSMatrix4x4.LookAt(rbMatrix.position, rbMatrix.position + rbForward, SBSVector3.up);
        }

        SBSVector3 frontPt = rbMatrix.MultiplyVector(frontPoint - rb.centerOfMass),
                   rearPt = rbMatrix.MultiplyVector(rearPoint - rb.centerOfMass),
                   worldForward = rbMatrix.MultiplyVector(SBSVector3.forward),
                   worldUp = rbMatrix.MultiplyVector(SBSVector3.up),
                   linVel = rb.velocity,
                   angVel = rb.angularVelocity,
                   frontVel = linVel + SBSVector3.Cross(angVel, frontPt),
                   rearVel = linVel + SBSVector3.Cross(angVel, rearPt);

        currentSpeed = SBSVector3.Dot(linVel, worldForward);
        absCurrentSpeed = SBSMath.Abs(currentSpeed);

        if (absCurrentSpeed <= steeringAngleMinSpeed)
            maxSteeringAngle = steeringAngleHi;
        else if (absCurrentSpeed <= steeringAngleMaxSpeed)
        {
            float a = (absCurrentSpeed - steeringAngleMinSpeed) / (steeringAngleMaxSpeed - steeringAngleMinSpeed),
                  b = 1.0f - a;
            maxSteeringAngle = steeringAngleHi * b + steeringAngleLo * a;
        }
        else
            maxSteeringAngle = steeringAngleLo;

        float dot = SBSVector3.Dot(worldUp, frontVel);
        frontVel -= worldUp * dot;

        dot = SBSVector3.Dot(worldUp, rearVel);
        rearVel -= worldUp * dot;

        linVel.y = 0.0f;
        if (throttle < 1.0e-2f && brake < 1.0e-2f)
        {
            dot = SBSVector3.Dot(angVel, worldUp);
            if (SBSVector3.Dot(linVel, linVel) < (linVelRestThreshold * linVelRestThreshold))
            {
                if (SBSMath.Abs(dot) < angVelRestThreshold && (!rotateInPlace || Mathf.Abs(steering) < 1.0e-2f) )
                {
                    angVel -= worldUp * dot;

                    rb.angularVelocity = angVel;
                    rb.velocity -= (Vector3)linVel;
                }

                if (rotateInPlace)
                {
                    SBSVector3 angularVelocityTarget = steering * rotateInPlaceMaxVelocity * worldUp;
                    angVel += (angularVelocityTarget - angVel) * Time.fixedDeltaTime * (angularVelocityTarget.sqrMagnitude >= angVel.sqrMagnitude ? rotateInPlaceAcc : rotateInPlaceDec);
                    rb.angularVelocity = angVel;
                }

                return;
            }
        }
        
        float steerAngle = (Version.v0 == version ? -steering * maxSteeringAngle : steering * maxSteeringAngle);
        if (SBSVector3.Dot(frontVel, frontVel) > SBSMath.Epsilon && SBSVector3.Dot(rearVel, rearVel) > SBSMath.Epsilon)
        {
            float slipAngleFront = SBSMath.Atan2(SBSVector3.Dot(SBSVector3.Cross(frontVel, worldForward), worldUp), SBSMath.Abs(SBSVector3.Dot(worldForward, frontVel))) * SBSMath.ToDegrees,
                  slipAngleRear = SBSMath.Atan2(SBSVector3.Dot(SBSVector3.Cross(rearVel, worldForward), worldUp), SBSMath.Abs(SBSVector3.Dot(worldForward, rearVel))) * SBSMath.ToDegrees;

            if (Version.v0 == version)
            {
                if (this.Throttle >= 0.0f)//currentSpeed >= 0.0f)
                    slipAngleFront -= steerAngle;
                else
                    slipAngleFront += steerAngle;
            }
            else
            {
                if (currentSpeed >= 0.0f)
                    slipAngleFront += steerAngle;
                else
                    slipAngleFront -= steerAngle;
            }
            /*
            if (useGripCurve)
            {
                latForceFront = gripCurve.Evaluate(Mathf.Abs(slipAngleFront / 90.0f)) * maxGripFront;
                if (slipAngleFront < 0.0f)
                    latForceFront = -latForceFront;
                latForceRear = gripCurve.Evaluate(Mathf.Abs(slipAngleRear / 90.0f)) * maxGripRear;
                if (slipAngleRear < 0.0f)
                    latForceRear = -latForceRear;
            }
            else
            {*/
                //float fAngle = slipAngleFront >= 0.0f ? Mathf.Max(0.0f, slipAngleFront - slipAnglePeak) : Mathf.Min(0.0f, slipAngleFront + slipAnglePeak),
                      //rAngle = slipAngleRear >= 0.0f ? Mathf.Max(0.0f, slipAngleRear - slipAnglePeak) : Mathf.Min(0.0f, slipAngleRear + slipAnglePeak);
                latForceFront = Mathf.Clamp(slipAngleFront * maxGripFront / slipAnglePeak, -maxGripFront, maxGripFront);
                //latForceFront = latForceFront - (latForceFront >= 0.0f ? Mathf.Min(latForceFront, gripSlopeFront * fAngle) : Mathf.Max(latForceFront, gripSlopeFront * fAngle));
                latForceRear = Mathf.Clamp(slipAngleRear * maxGripRear / slipAnglePeak, -maxGripRear, maxGripRear);
                //latForceRear = latForceRear - (latForceRear >= 0.0f ? Mathf.Min(latForceRear, gripSlopeRear * rAngle) : Mathf.Max(latForceRear, gripSlopeRear * rAngle));
            //}
        }
        else
        {
            latForceFront = latForceRear = 0.0f;
        }

        float tr = (throttle - brake);

        if (currentSpeed >= 0.0f)
            tr *= traction;
        else
            tr *= backTraction;

        latForceFront *= frontWeight * Mathf.Clamp01(absCurrentSpeed * latForceSpeedMult);
        latForceRear *= rearWeight * Mathf.Clamp01(absCurrentSpeed * latForceSpeedMult);

        SBSQuaternion q = SBSQuaternion.AngleAxis(steerAngle, worldUp);

        SBSVector3 worldRight = rbMatrix.MultiplyVector(SBSVector3.right),
                   frontForward = q * worldForward,
                   frontRight = SBSVector3.Cross(worldUp, frontForward),
                   f;

        if (frontWheelsOnGround || airTraction || airGrip)
        {
            if (!frontWheelsOnGround)
                frontRight.y = 0.0f;

            f = frontForward * (frontWheelsOnGround || airTraction ? tr : 0.0f) * frontTraction +
                frontRight * (frontWheelsOnGround || airGrip ? latForceFront : 0.0f);

            force += f;
            torque += SBSVector3.Cross(frontPt, f);

            lastForces[0] = f;
            lastTorques[0] = SBSVector3.Cross(frontPt, f);
        }

        if (rearWheelsOnGround || airTraction || airGrip)
        {
            if (!rearWheelsOnGround)
                worldRight.y = 0.0f;

            f = worldForward * (rearWheelsOnGround || airTraction ? tr : 0.0f) * rearTraction +
                worldRight * (rearWheelsOnGround || airGrip ? latForceRear : 0.0f);

            force += f;
            torque += SBSVector3.Cross(rearPt, f);

            lastForces[1] = f;
            lastTorques[1] = SBSVector3.Cross(rearPt, f);
        }

        force -= linVel * (currentSpeed >= 0.0f ? drag : backDrag);

        worldForward.y = 0.0f;

        force += worldForward * friction * (frontWeight + rearWeight) * (currentSpeed >= 0.0f ? -1.0f : 1.0f);
    }
    #endregion

    #region Unity callbacks
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector3 frontPt = transform.TransformPoint(frontPoint);
        Gizmos.DrawRay(frontPt, lastForces[0] * 0.01f);
        Gizmos.DrawRay(frontPt, lastTorques[0] * 0.01f);
        Vector3 rearPt = transform.TransformPoint(rearPoint);
        Gizmos.DrawRay(rearPt, lastForces[1] * 0.01f);
        Gizmos.DrawRay(rearPt, lastTorques[1] * 0.01f);
    }
#endif
    #endregion

    #region Messages
    void ResetPhysics()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        inputs = gameObject.GetComponents<VehicleInputs>();
    }

    void DisableInputs()
    {
        throttle = 0.0f;
        brake = 0.0f;
        steering = 0.0f;
    }
    #endregion
}
