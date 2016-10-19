using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Racing;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[AddComponentMenu("SBS/Racing/SimplePhysics/Buoyancy")]
public class Buoyancy : MonoBehaviour
{
   
    public Vector3[] samplePoints;
    public float waterMassRatio = 4.0f;
    public float bounce = 240.0f;
    public float rebound = 360.0f;
    public float gravity = 9.8f;
    public bool isInWater = true;

    protected float[] precomputedHeights;
    protected Vector3[] bottomPoints;
    protected float[] prevVelocities;
    protected OceanBase ocean;

    void Awake()
    {
        ocean = GameObject.Find("Ocean").GetComponent<OceanBase>();
    }

    void Start()
    {
        int i = 0, c = samplePoints.Length;

        precomputedHeights = new float[c];
        bottomPoints = new Vector3[c];
        prevVelocities = new float[c];

        Vector3[] worldSamplePoints = new Vector3[c];
        for (; i < c; ++i)
            worldSamplePoints[i] = GetComponent<Rigidbody>().position + GetComponent<Rigidbody>().rotation * samplePoints[i];

        i = 0;
        foreach (Vector3 pt in worldSamplePoints)
        {
            bottomPoints[i] = samplePoints[i];

            RaycastHit hit;
            if (GetComponent<Collider>().Raycast(new Ray(pt - 10.0f * Vector3.up, Vector3.up), out hit, 10.0f))
                precomputedHeights[i] += (10.0f - hit.distance);
            if (GetComponent<Collider>().Raycast(new Ray(pt + 10.0f * Vector3.up, Vector3.down), out hit, 10.0f))
            {
                precomputedHeights[i] += (10.0f - hit.distance);
                bottomPoints[i] += (10.0f - hit.distance) * Vector3.down;
            }

            ++i;
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime,
              oodt = 1.0f / dt;

        int i = 0, c = samplePoints.Length;
        Vector3[] worldBottomPoints = new Vector3[c],
                  worldApplyPoint = new Vector3[c];
        float[] accelerations = new float[c];
        for (; i < c; ++i)
        {
            worldApplyPoint[i] = GetComponent<Rigidbody>().position + GetComponent<Rigidbody>().rotation * samplePoints[i];
            worldBottomPoints[i] = GetComponent<Rigidbody>().position + GetComponent<Rigidbody>().rotation * bottomPoints[i];

            float newVel = GetComponent<Rigidbody>().GetPointVelocity(worldApplyPoint[i]).y;
            accelerations[i] = (newVel - prevVelocities[i]) * oodt;
            //Debug.DrawRay(worldApplyPoint[i], Vector3.up * accelerations[i]);
            prevVelocities[i] = newVel;
        }

        i = 0;
        float sampleMass = GetComponent<Rigidbody>().mass / (float)c;

        Vector3 worldUp = Vector3.up;
        float[] ratios = new float[c];
        float ratioSum = 0.0f;
        isInWater = false;

        foreach (Vector3 pt in worldBottomPoints)
        {
            Vector3 waterPt = new Vector3(pt.x, ocean.GetWaterHeightAtLocation(pt.x, pt.z) - 100.0f, pt.z);
            RaycastHit hit;

            float ratio = ratios[i] = 0.0f;
            if (GetComponent<Collider>().Raycast(new Ray(waterPt, Vector3.up), out hit, 100.0f))
                ratio = ratios[i] = Mathf.Clamp01((100.0f + waterPt.y - pt.y) / precomputedHeights[i]);
            Debug.DrawRay(pt, Vector3.up * ratio);
            ratioSum += ratio;

            if (ratio > 0.0f)
                isInWater = true;

            ++i;
        }

        for (i = 0; i < c; ++i)
        {
            float r = ratios[i];
            Vector3 force = worldUp * Mathf.Max(-accelerations[i], gravity) * sampleMass * r * waterMassRatio;
            float vel = prevVelocities[i];
            if (vel < 0.0f)
                force -= worldUp * vel * bounce * r;
            else
                force -= worldUp * vel * rebound * r;
            GetComponent<Rigidbody>().AddForceAtPosition(force, worldApplyPoint[i]);
        }
    }
}
