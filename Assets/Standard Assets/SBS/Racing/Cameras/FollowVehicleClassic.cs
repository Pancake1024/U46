using System;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/Cameras/FollowVehicleClassic")]
public class FollowVehicleClassic : MonoBehaviour
{
    static public float MaxAngle = SBSMath.PI * 0.3f;

    public GameObject vehicle = null;
    public VehicleTrackData trackData = null;

    public Vector3 pivotOffset;
    public Vector3 sourceOffset;
    public Vector3 targetOffset;

    public float yawMultiplier = 0.0009f * 500.0f * 0.35f;
    public float latOffsetMult = 0.05f * 0.036f * 7.5f;
    public float rollMultiplier = 0.125f;
    public float offsetBase = 0.95f;
    public float offsetMultiplier = 0.05f * 0.036f * 1.75f;
    public float offsetMaxSpeed = 205.0f * VehicleUtils.ToMetersPerSecs;

    public bool tokenOrientation = true;

    public float pitchValue = 0.36f;
    public float rollValue = 0.1f;

    protected float yawAngle = 0.0f;
    protected float rollAngle = 0.0f;
    protected float offset = 1.0f;
    protected float latOffset = 0.0f;

    protected SBSQuaternion prevRotation;
    protected bool firstFrame = true;

    protected SBSVector3 noise = new SBSVector3();
    protected float noiseTimer = -1.0f;
    protected float noiseIntensity = 1.0f;
    protected float noiseDuration = 1.0f;
    protected bool noiseProgressive = false;

    protected SBSVector3 tokenTangent;

    public bool useWheels = true;

    public bool rayCheck = false;
    public float rayCheckRadius = 0.0f;
    public float rayCheckOffset = 0.02f;
    public LayerMask rayCheckMask;

    public float smoothing = 6.0f;

    protected SBSVector3 lastRaycheckSourceOffset;

    public void StartNoise(float now, float duration, float intensity, bool progressive)
    {
        if ((now - noiseTimer) < 0 && (intensity < noiseIntensity))
                return;

        noiseTimer       = now + duration;
        noiseIntensity   = intensity;
        noiseProgressive = progressive;
        noiseDuration    = duration;
    }

    void OnFollowVehicleClassicEnter(float time)
    {
        //Debug.Log("OnFollowVehicleClassicEnter");

        yawAngle = 0.0f;
        rollAngle = 0.0f;
        offset = offsetBase;
        latOffset = 0.0f;
        firstFrame = true;

        lastRaycheckSourceOffset = sourceOffset;
    }

    public void OnFollowVehicleClassicExec(float time)
    {
        if (null == vehicle || !VehiclesManager.Instance.HasVehicle(vehicle) || TimeManager.Instance.MasterSource.IsPaused)
            return;

        float dt = Time.fixedDeltaTime;

        SBSVector3 zAxisWheels,
                   xAxisWheels,
                   center;
        if (useWheels)
        {
            SBSVector3 wheelFrontLeft = VehiclesManager.Instance.GetWheelWorldPosition(vehicle, (int)VehicleUtils.WheelPos.FR_LEFT),
                        wheelRearLeft = VehiclesManager.Instance.GetWheelWorldPosition(vehicle, (int)VehicleUtils.WheelPos.RR_LEFT),
                        wheelRearRight = VehiclesManager.Instance.GetWheelWorldPosition(vehicle, (int)VehicleUtils.WheelPos.RR_RIGHT);
            zAxisWheels = wheelFrontLeft - wheelRearLeft;
            xAxisWheels = wheelRearRight - wheelRearLeft;
            center = 0.5f * (wheelFrontLeft + wheelRearRight);
        }
        else
        {
            zAxisWheels = vehicle.transform.forward;
            xAxisWheels = vehicle.transform.right;
            center = vehicle.transform.position;
        }

        zAxisWheels.Normalize();
        xAxisWheels.Normalize();

        SBSVector3 xAxisPlanar = xAxisWheels - SBSVector3.Dot(xAxisWheels, SBSVector3.up) * SBSVector3.up;

        if (xAxisPlanar.sqrMagnitude < SBSMath.SqrEpsilon)
            xAxisPlanar = SBSVector3.Cross(zAxisWheels, SBSVector3.up);
        xAxisPlanar.Normalize();

        SBSVector3 xAxis = SBSVector3.Rotate(xAxisPlanar, xAxisWheels, rollValue),
                   zAxisPlanar = SBSVector3.Cross(xAxis, SBSVector3.up);
        zAxisPlanar.Normalize();

        SBSVector3 pivot = pivotOffset;
        if (trackData != null && tokenOrientation)
        {
            if (firstFrame || trackData.TokenTangent.sqrMagnitude > SBSMath.SqrEpsilon)
                tokenTangent = trackData.TokenTangent;

            float cos0 = (tokenTangent.x * zAxisWheels.x) + (tokenTangent.z * zAxisWheels.z),
                  sin0 = (tokenTangent.x * zAxisWheels.z) - (tokenTangent.z * zAxisWheels.x),
                  cos1 = (tokenTangent.x * zAxisPlanar.x) + (tokenTangent.z * zAxisPlanar.z),
                  sin1 = (tokenTangent.x * zAxisPlanar.z) - (tokenTangent.z * zAxisPlanar.x),
                  a0 = SBSMath.Atan2(sin0, cos0),
                  a1 = SBSMath.Atan2(sin1, cos1),
                  aNeg = 0.0f,
                  dist = 0.0f;

            SBSQuaternion r0 = new SBSQuaternion(0.0f, 0.0f, 0.0f, 1.0f),
                          r1 = new SBSQuaternion(0.0f, 0.0f, 0.0f, 1.0f);

            if (a0 > MaxAngle || a0 < -MaxAngle)
            {
                dist = (SBSMath.Abs(a0) - MaxAngle) / (SBSMath.PI - MaxAngle);
                aNeg = SBSMath.PI - SBSMath.Abs(a0);

                if (aNeg < MaxAngle)
                {
                    if (a0 < 0.0f)
                        a0 -= (MaxAngle - aNeg);
                    else
                        a0 += (MaxAngle - aNeg);
                }

                a0 = (a0 < 0.0f ? (a0 + MaxAngle) : (a0 - MaxAngle));
                a0 *= 0.5f;

                r0.y = SBSMath.Sin(a0);
                r0.w = SBSMath.Cos(a0);
                r0.Normalize();
            }

            if (a1 > MaxAngle || a1 < -MaxAngle)
            {
                dist = SBSMath.Max(dist, (SBSMath.Abs(a1) - MaxAngle) / (SBSMath.PI - MaxAngle));
                aNeg = SBSMath.PI - SBSMath.Abs(a1);

                if (aNeg < MaxAngle)
                {
                    if (a1 < 0.0f)
                        a1 -= (MaxAngle - aNeg);
                    else
                        a1 += (MaxAngle - aNeg);
                }

                a1 = (a1 < 0.0f ? (a1 + MaxAngle) : (a1 - MaxAngle));
                a1 *= 0.5f;

                r1.y = SBSMath.Sin(a1);
                r1.w = SBSMath.Cos(a1);
                r1.Normalize();
            }

            pivot.z -= dist * 1.25f;

#if UNITY_FLASH
            r0.Rotate(zAxisWheels, zAxisWheels);
            r1.Rotate(zAxisPlanar, zAxisPlanar);
            SBSQuaternion.SLERP(r0, r1, 0.5f, r0);
            r0.Rotate(xAxis, xAxis);
#else
            r0.Rotate(zAxisWheels, ref zAxisWheels);
            r1.Rotate(zAxisPlanar, ref zAxisPlanar);
            r0 = SBSQuaternion.Slerp(r0, r1, 0.5f);
            r0.Rotate(xAxis, ref xAxis);
#endif
        }

        SBSVector3 zAxis = SBSVector3.Rotate(zAxisPlanar, zAxisWheels, pitchValue),
                   yAxis = SBSVector3.Cross(zAxis, xAxis);

        SBSMatrix4x4 vehFrame = new SBSMatrix4x4();
        vehFrame.xAxis = xAxis;
        vehFrame.yAxis = yAxis;
        vehFrame.zAxis = zAxis;

        SBSVector3 vehPosition = vehFrame.MultiplyVector(pivot) + center;
        vehFrame.position = vehPosition;

        SBSQuaternion vehRotation = vehFrame.rotation;
        if (firstFrame)
        {
            prevRotation = vehRotation;
        }
        else
        {
#if UNITY_FLASH
            SBSQuaternion.Slerp(prevRotation, vehRotation, SBSMath.Min(dt * smoothing, 1.0f), vehRotation);
#else
            vehRotation = SBSQuaternion.Slerp(prevRotation, vehRotation, SBSMath.Min(dt * smoothing, 1.0f));
#endif
            prevRotation = vehRotation;

            vehFrame.rotation = vehRotation;
        }

        IVehiclePhysics vehPhysics = VehiclesManager.Instance.GetVehicleInterface(vehicle);
        SBSVector3 vel = vehPhysics.Velocity;
        float sideVel = SBSVector3.Dot(vel, xAxis),
              speed = SBSVector3.Dot(vel, zAxis);

        float newYawAngle = -sideVel * yawMultiplier;
        yawAngle += (newYawAngle - yawAngle) * SBSMath.Min(1.0f, dt * 4.0f);

        float newRollAngle = sideVel * rollMultiplier;
        rollAngle += (newRollAngle - rollAngle) * SBSMath.Min(1.0f, dt * 4.0f);

        float newOffset = offsetBase + SBSMath.Abs(SBSMath.Min(speed, offsetMaxSpeed)) * offsetMultiplier;
        offset += (newOffset - offset) * SBSMath.Min(1.0f, dt * 4.0f);

        float newLatOffset = -sideVel * latOffsetMult;
        latOffset += (newLatOffset - latOffset) * SBSMath.Min(1.0f, dt * smoothing);

        float extraOffset = offset - 1.0f;
        SBSVector3 target = SBSVector3.up * (SBSVector3.Dot(targetOffset, SBSVector3.up) * extraOffset);
        target.IncrementBy(targetOffset);
        target = vehFrame * target;

        SBSVector3 localSource = SBSVector3.forward * (SBSVector3.Dot(sourceOffset, SBSVector3.forward) * extraOffset);
        localSource.IncrementBy(sourceOffset);
        localSource.x += latOffset;

        SBSQuaternion q = SBSQuaternion.AngleAxis(yawAngle, SBSVector3.up) * vehRotation;
        SBSVector3 source = vehPosition + q * localSource;

        bool isNoiseActive = false;
        if( time - noiseTimer <= 0 )
            isNoiseActive = true;

        if (isNoiseActive)
        {
            float noiseT = 1.0f;
            if (noiseProgressive)
            {
                noiseT = (noiseTimer - time) / noiseDuration;
            }
            SBSVector3 newNoise = new SBSVector3();
            newNoise.x = UnityEngine.Random.Range(-noiseIntensity, noiseIntensity);
            newNoise.y = UnityEngine.Random.Range(-noiseIntensity, noiseIntensity);
            newNoise.z = UnityEngine.Random.Range(-noiseIntensity, noiseIntensity);
            newNoise = newNoise * noiseT;
            noise = SBSVector3.Lerp(noise, newNoise, SBSMath.Min(dt * 3.5f, 1.0f));
        }
        else
        {
            noise = SBSVector3.Lerp(noise, new SBSVector3(), SBSMath.Min(dt * 5.0f, 1.0f));
        }

        if (rayCheck)
        {
            SBSVector3 origin    = target + noise,
                       direction = source - origin;

            float distance = direction.magnitude;
            direction /= distance;

            float pivotDist = SBSVector3.Dot(vehPosition - origin, direction);

            RaycastHit[] hits = null;
            if (rayCheckRadius > Mathf.Epsilon)
                hits = Physics.SphereCastAll(origin, rayCheckRadius, direction, distance, rayCheckMask);
            else
                hits = Physics.RaycastAll(origin, direction, distance, rayCheckMask);

            RaycastHit nearestHit = new RaycastHit();
            bool found = false;
            foreach (RaycastHit hit in hits)
            {
                float dist = SBSVector3.Dot((SBSVector3)hit.point - origin, direction);
                if (dist > pivotDist && Vector3.Dot(hit.normal, direction) < -Mathf.Epsilon)
                {
                    if (!found)
                    {
                        found = true;
                        nearestHit = hit;
                    }
                    else
                    {
                        if (hit.distance < nearestHit.distance)
                            nearestHit = hit;
                    }
                }
            }

            if (found)
            {
                source = origin + direction * (nearestHit.distance + (rayCheckRadius > Mathf.Epsilon ? rayCheckRadius : 0.0f) - rayCheckOffset);
                lastRaycheckSourceOffset = (q.inverse * (source - vehPosition));
            }
            else
            {
                SBSVector3 newLocalSourceOffset = (q.inverse * (source - vehPosition));
                lastRaycheckSourceOffset = SBSVector3.Lerp(lastRaycheckSourceOffset, newLocalSourceOffset, Mathf.Min(1.0f, smoothing * dt));
                source = vehPosition + q * lastRaycheckSourceOffset;
            }
        }

        q.SetAngleAxis(rollAngle, zAxis);

        gameObject.transform.position = source;
        gameObject.transform.LookAt(target + noise, q * SBSVector3.up);

        firstFrame = false;
    }

    void OnFollowVehicleClassicExit(float time)
    {
        Debug.Log("OnFollowVehicleClassicExit");
    }
};
