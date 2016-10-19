using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("SBS/Racing/SimplePhysics/WheelRay")]
public class WheelRay : MonoBehaviour
{
    protected static float RayLenght = 10.0f;
    protected static float RayOffset = 1.0f;
    public static float MaxNormalAngle = 75.0f;

    #region Public members
    public float radius = 0.0f;
    public Vector3[] positions;
    public Transform[] nodes;

    public float bounce = 1500.0f;
    public float rebound = 1000.0f;
    public float restitution = -1.0f;
    public float penalty = 0.0f;
    public float maxOvertravel = 1.0e-2f;
    public float springCoeff = 5000.0f;

    public float travelMin = 0.0f;
    public float travelMax = 0.2f;
    public float restingPos = 0.2f;

    public LayerMask layerMask; // = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
    #endregion

    #region Protected members
    protected Rigidbody rb = null;
    protected SBSMatrix4x4 rbMatrix;

    protected float[] displacements = null;
    protected float[] prevDisplacements = null;
    protected float[] overTravels = null;

    protected bool[] onGround = null;
    protected SBSVector3[] groundNormals = null;
    protected SBSVector3[] groundPositions = null;
    protected RaycastHit[] raycastHits = null;
    protected float[] groundDistances = null;

    protected SBSVector3[] lastForces;
    protected SBSVector3[] lastTorques;
    #endregion

    #region Public methods
    public SBSVector3 GetWorldPosition(int index)
    {
        return rbMatrix.MultiplyPoint3x4((SBSVector3)positions[index] + SBSVector3.down * displacements[index]);
    }

    public bool GetWheelOnGround(int index)
    {
        return onGround[index];
    }

    public SBSVector3 GetGroundNormal(int index)
    {
        return groundNormals[index];
    }

    public RaycastHit GetRaycastHit(int index)
    {
        return raycastHits[index];
    }

    public SBSVector3 GetGroundPosition(int index)
    {
        return groundPositions[index];
    }

    public float GetGroundDistance(int index)
    {
        return groundDistances[index];
    }

    public float GetDisplacement(int index)
    {
        return displacements[index];
    }

    public SBSVector3 GetLastForce(int index)
    {
        return lastForces[index];
    }

    public SBSVector3 GetLastTorque(int index)
    {
        return lastTorques[index];
    }

    public void AccumForcesNoRays(float dt, ref float[] _displacements, ref SBSVector3 force, ref SBSVector3 torque)
    {
        SBSVector3 up = rbMatrix.MultiplyVector(SBSVector3.up);
        for (int i = positions.Length - 1; i >= 0; --i)
        {
            SBSVector3 p = positions[i],
                       r = rbMatrix.MultiplyVector(p - (SBSVector3)rb.centerOfMass);
            float f = 0.0f,
                  v = Vector3.Dot(SBSVector3.Cross(rb.angularVelocity, r), up);

            _displacements[i] += v * dt;

            float displ = _displacements[i];
            if (displ < restingPos)
            {
                if (v >= 0.0f)
                    f -= rebound * v;
                else
                    f -= bounce * v;
                f += (restingPos - displ) * springCoeff;
            }

            force += up * f;
            torque += SBSVector3.Cross(r, up) * f;
        }
    }

    public void AccumForces(float dt, ref SBSVector3 force, ref SBSVector3 torque)
    {
        if (null == rb)
            return;

        rbMatrix = SBSMatrix4x4.TRS(rb.position, rb.rotation, SBSVector3.one);

        for (int i = positions.Length - 1; i >= 0; --i)
        {
            onGround[i] = false;
            SBSVector3 p = positions[i],
                       r = p - (SBSVector3)rb.centerOfMass;

            RaycastHit[] hits = Physics.RaycastAll(
                rbMatrix.MultiplyPoint3x4(p + SBSVector3.up * RayOffset),
                rbMatrix.MultiplyVector(SBSVector3.down), RayLenght,
                layerMask);

            if (0 == hits.Length)
                continue;

            int nearestIdx = -1, j = 0;
            float nearestDist = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == GetComponent<Collider>() || SBSVector3.Angle(hit.normal, SBSVector3.up) > MaxNormalAngle)
                {
                    ++j;
                    continue;
                }
                if (hit.distance < nearestDist)
                {
                    nearestIdx = j;
                    nearestDist = hit.distance;
                }

                ++j;
            }

            if (-1 == nearestIdx)
                continue;

            RaycastHit nearestHit = hits[nearestIdx];

            onGround[i] = true;
            groundNormals[i] = nearestHit.normal;
            groundPositions[i] = nearestHit.point;
            raycastHits[i] = nearestHit;
            prevDisplacements[i] = displacements[i];
            float groundDist = SBSMath.Max(0.0f, nearestHit.distance - RayOffset);
            groundDistances[i] = groundDist;
            float displ = groundDist - radius;
            displacements[i] = displ;
            overTravels[i] = 0.0f;

            if (displ > travelMax)
            {
                onGround[i] = false;
                displ = displacements[i] = travelMax;
            }

            if (displ < travelMin)
            {
                overTravels[i] = travelMin - displ;
                displ = displacements[i] = travelMin;
            }

            float f = 0.0f,
                  v = (displ - prevDisplacements[i]) / dt;
            if (displ < restingPos)
            {
                if (v >= 0.0f)
                    f -= rebound * v;
                else
                    f -= bounce * v;
                f += (restingPos - displ) * springCoeff;
            }
            if (overTravels[i] > SBSMath.Epsilon)
            {
                SBSMatrix4x4 invR = rbMatrix.inverseFast,
                             invI = new SBSMatrix4x4(
                                 rb.inertiaTensor.x, 0.0f, 0.0f, 0.0f,
                                 0.0f, rb.inertiaTensor.y, 0.0f, 0.0f,
                                 0.0f, 0.0f, rb.inertiaTensor.z, 0.0f,
                                 0.0f, 0.0f, 0.0f, 1.0f);

                invI.Prepend(invR);
                invI.Append(rbMatrix);
                invI.Invert();

                SBSVector3 lv = invR.MultiplyVector(rb.velocity),
                           n = SBSVector3.down,
                           w = invR.MultiplyVector(rb.angularVelocity),
                           rxn = SBSVector3.Cross(r, n);

                float denom = (1.0f / rb.mass) + SBSVector3.Dot(rxn, invI * rxn),
                      J = SBSVector3.Dot(lv, n) + SBSVector3.Dot(rxn, w) / (denom * dt);

                J = SBSMath.Max(0.0f, (1.0f + restitution) * J);
                J += SBSMath.Min(maxOvertravel, overTravels[i]) * penalty * rb.mass;

                f += J;
            }

            SBSVector3 up = rbMatrix.yAxis;

            lastForces[i] = up * f;
            lastTorques[i] = SBSVector3.Cross(rbMatrix.MultiplyVector(r), up) * f;

            force += lastForces[i];
            torque += lastTorques[i];
        }
    }
    #endregion

    #region Unity callbacks
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (null == positions || null == lastForces || null == lastTorques)
            return;
        for (int i = positions.Length - 1; i >= 0; --i)
        {
            Vector3 worldPt = transform.TransformPoint(positions[i]);
            Gizmos.DrawRay(worldPt, lastForces[i] * 0.01f);
            Gizmos.DrawRay(worldPt, lastTorques[i] * 0.01f);
        }
    }
#endif
    #endregion

    #region Messages
    void ResetWheels()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        float tmin = travelMin, tmax = travelMin;
        travelMin = SBSMath.Min(tmin, tmax);
        travelMin = SBSMath.Max(tmin, tmax);
        restingPos = Mathf.Clamp(restingPos, travelMin, travelMax);

        int wheelsCount = positions.Length;

        displacements = new float[wheelsCount];
        prevDisplacements = new float[wheelsCount];
        overTravels = new float[wheelsCount];
        onGround = new bool[wheelsCount];
        groundNormals = new SBSVector3[wheelsCount];
        groundPositions = new SBSVector3[wheelsCount];
        raycastHits = new RaycastHit[wheelsCount];
        groundDistances = new float[wheelsCount];
        lastForces = new SBSVector3[wheelsCount];
        lastTorques = new SBSVector3[wheelsCount];

        for (int i = 0; i < wheelsCount; ++i)
        {
            displacements[i] = 0.0f;
            prevDisplacements[i] = 0.0f;
            overTravels[i] = 0.0f;
            onGround[i] = false;
            groundNormals[i] = SBSVector3.up;
            groundPositions[i] = SBSVector3.zero;
            groundDistances[i] = 0.0f;
            lastForces[i] = SBSVector3.zero;
            lastTorques[i] = SBSVector3.zero;
        }
    }
    #endregion
}
