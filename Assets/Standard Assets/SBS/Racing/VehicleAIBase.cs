using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/VehicleAIBase")]
public class VehicleAIBase : VehicleInputs
{
    #region Public members
    public bool enableAtStart = false;
    public VehicleAISensor sensor = null;
    public float skill = 1.0f;
    public Vector3 targetPosition;
    public float targetSpeed = -1.0f;
    public float minSpeedDelta = 1.0f;
    public float maxSpeedDelta = 4.0f;
    public float brakeDistanceThreshold = 10.0f;
    public bool forceBrake = false; // set to TRUE when avoiding
    public float timeInRearAfterStuck = 3.5f;
    public float timeToStartStuck = 2.0f;
    public float _gainDebug = 0.0f;
    #endregion

    #region Protected members
    protected VehicleTrackData trackData = null;
    protected IVehiclePhysics vehiclePhysics = null;

    protected float throttle = 0.0f;
    protected float steering = 0.0f;

    protected float isStuckTimer = 0.0f;
    protected bool isStuck = false;
    protected float rearTime = 0.0f;
    #endregion

    #region Protected methods
    protected float GetLimitSpeed(Token token, float latFriction)
    {
        float curvatureRadius = (token != null && Token.TokenType.Curve == token.type) ? token.lengthOrRadius : 10000.0f;

        float d = (1.0f - SBSMath.Min(1.01f, -vehiclePhysics.AerodynamicDownforceCoeff * curvatureRadius * latFriction / vehiclePhysics.Mass));
        float vSquare = (latFriction * vehiclePhysics.Gravity * curvatureRadius) / d;

        return vSquare > 0.0f ? SBSMath.Sqrt(vSquare) : 1000.0f;
    }

    protected float GetBrakeDistance(float vehicleSpeed, float limitSpeed, float longFriction)
    {
        float frictionForce = longFriction * vehiclePhysics.Gravity;
        float d = (-longFriction * vehiclePhysics.AerodynamicDownforceCoeff + vehiclePhysics.AerodynamicDragCoeff) / vehiclePhysics.Mass;
        float v1Square = vehicleSpeed * vehicleSpeed;
        float v2Square = limitSpeed * limitSpeed;

        return -SBSMath.Log((frictionForce + v2Square * d) / (frictionForce + v1Square * d)) / (2.0f * d);
    }

    protected float ComputeThrottle(float prevThrottle, float brake, float gain)
    {
        float newThrottle;
        if (brake > 0.0f)
            newThrottle = -brake;
        else
        {
            gain = gain * (0.9f + (skill - 0.4f) * 0.5f);
            newThrottle = gain;
        }

        if (isStuck)
            newThrottle = -newThrottle * 0.9f;

        if (newThrottle * throttle < 0.0f)
            throttle = 0.0f;

        _gainDebug = gain;
        float throttleChangeRate = 5.0f * Time.fixedDeltaTime;
        return SBSMath.RateLimit(prevThrottle, newThrottle, throttleChangeRate, throttleChangeRate);
    }

    protected float ComputeSteering(SBSVector3 targetPos)
    {
        if (isStuck)
            return 0.0f;

        SBSVector3 vehiclePos = vehiclePhysics.Transform.position;
        SBSVector3 vehicleOrientation = vehiclePhysics.Transform.zAxis;

        SBSVector3 targetOrientation = targetPos - vehiclePos;
        vehicleOrientation.y = 0.0f;
        targetOrientation.y = 0.0f;

        float angle = SBSVector3.Angle(vehicleOrientation, targetOrientation);
        if (float.IsNaN(angle))
            angle = 0.0f;

        angle *= SBSMath.Sign(SBSVector3.Cross(vehicleOrientation, targetOrientation).y);

        float bestSteeringRange = vehiclePhysics.BestSteeringAngle;
        angle = Mathf.Clamp(angle, -bestSteeringRange, bestSteeringRange);

        return Mathf.Clamp(angle / vehiclePhysics.MaxSteeringAngle, -1.0f, 1.0f);
    }
    #endregion

    #region Public properties
    public override float Throttle
    {
        get
        {
            return throttle;
        }
        set
        {
            throttle = value;
        }
    }

    public override float Steering
    {
        get
        {
            return steering;
        }
    }

    public bool IsStuck
    {
        get
        {
            return isStuck;
        }
    }
    #endregion

    #region Public methods
    public void Reset()
    {
        throttle = 0.0f;
        steering = 0.0f;
        ResetStuck();
    }

    public void ResetStuck()
    {
        isStuckTimer = 0.0f;
        isStuck = false;
        rearTime = 0.0f;
    }

    public void ForceStuck()
    {
        isStuck = true;
        rearTime = 0.0f;
    }

    public override void UpdateInputs()
    {
        float latFrictionCoeff = vehiclePhysics.LateralFrictionCoeff,
              vehicleSpeed     = vehiclePhysics.Velocity.magnitude,
              limitSpeed       = this.GetLimitSpeed(trackData.Token, latFrictionCoeff) * skill,
              gain             = 0.5f,
              brake            = 0.0f;

        if (targetSpeed >= 0.0f)
            limitSpeed = SBSMath.Min(targetSpeed, limitSpeed);

        if (vehicleSpeed < 2.0f && vehiclePhysics.IsOnGround)
        {
            if (!isStuck)
            {
                isStuckTimer += Time.fixedDeltaTime;

                if (isStuckTimer > timeToStartStuck)
                {
                    isStuck = true;
                    rearTime = 0.0f;
                }
            }
        }
        else
        {
            if (!isStuck)
                isStuckTimer = 0.0f;
        }

        float speedDelta = limitSpeed - vehicleSpeed;
        if (speedDelta < 0.0f)
        {
            gain = 0.0f;
            if (-speedDelta < minSpeedDelta && !forceBrake)
                brake = 0.0f;
            else
                brake = SBSMath.Min(-speedDelta / maxSpeedDelta, 1.0f);
        }
        else if (float.IsNaN(speedDelta) || speedDelta > maxSpeedDelta)
        {
            gain  = 1.0f;
            brake = 0.0f;
        }
        else
        {
            gain  = speedDelta / maxSpeedDelta;
            brake = 0.0f;
        }

        float longFrictionCoeff  = vehiclePhysics.LongitudinalFrictionCoeff,
              brakeLookaheadDist = GetBrakeDistance(vehicleSpeed, 0.0f, longFrictionCoeff) + brakeDistanceThreshold,
              checkDist          = 0.0f;

        SBSVector3 checkPos = new SBSVector3(),
                   checkTan = new SBSVector3();

        int checkNextCount = 0;
        float driveBrake = brake;

        while (checkDist < brakeLookaheadDist)
        {
            checkDist += 40.0f;
            float checkTrackPos = trackData.TrackPosition + checkDist;
            Token checkToken = null;
            if (trackData.TrackBranch != null)
#if UNITY_FLASH
                checkToken = RacingManager.Instance.track.EvaluateAt(trackData.TrackBranch, checkTrackPos, 0.0f, trackData.branchesMask, checkPos, checkTan);
#else
                checkToken = RacingManager.Instance.track.EvaluateAt(trackData.TrackBranch, checkTrackPos, 0.0f, trackData.branchesMask, out checkPos, out checkTan);
#endif

            float limitSpeed2 = GetLimitSpeed(checkToken, latFrictionCoeff) * skill,
                  brakeDist   = GetBrakeDistance(vehicleSpeed, limitSpeed2, longFrictionCoeff);

            if (brakeDist > checkDist)
            {
                gain = 0.0f;
                brake = 1.0f;
                checkNextCount++;
            }
            else
            {
                if (checkNextCount != 0)
                {
                    checkNextCount++;
                    if (driveBrake <= 0.0f && !forceBrake)
                        brake = 0.0f;
                }
            }

            if (checkNextCount >= 2)
                break;
        }

        if (speedDelta < 0.0f)
        {
            if (Vector3.Dot(vehiclePhysics.Velocity, transform.forward) < 0.0f)
            {
                throttle = 1.0f;
                brake = 0.0f;
                gain = 1.0f;
            }
        }

        throttle = this.ComputeThrottle(throttle, brake, gain);
        steering = this.ComputeSteering(targetPosition);

        if (isStuck)
        {
            rearTime += Time.fixedDeltaTime;

            if (rearTime > timeInRearAfterStuck)  // 3.5f
            {
                rearTime = 0.0f;
                isStuck = false;
                isStuckTimer = 0.0f;
            }
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        vehiclePhysics = gameObject.GetComponent<VehiclePhysics>();
        if (null == vehiclePhysics)
            vehiclePhysics = gameObject.GetComponent<VehiclePhysicsSimple>();

        trackData = gameObject.GetComponent<VehicleTrackData>();

        this.enabled = enableAtStart;
        gameObject.SendMessage("SetAIEnabled", this.enabled);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, 1.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.forward * throttle * 5.0f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(Vector3.right * steering * 5.0f));
        Gizmos.color = Color.white;
    }
#endif
    #endregion

    #region Messages
    void SetAIEnabled(bool flag)
    {
        this.enabled = flag;
    }

    void DisableInputs()
    {
        this.enabled = false;
    }
    #endregion
}
