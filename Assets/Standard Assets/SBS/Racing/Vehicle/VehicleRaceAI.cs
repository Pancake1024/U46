using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Racing;
using SBS.Math;

[AddComponentMenu("SBS/Racing/VehicleRaceAI")]
public class VehicleRaceAI : MonoBehaviour
{
    #region Public members
    public float lookAheadCoeff = 0.35f;
    public float lookAheddCoeffSpeed = 0.9f;
    public float evadeWidth = 0.45f;
    #endregion

    #region EvadeDir enum
    public enum EvadeDir
    {
        None,
        Right,
        Left
    }
    #endregion

    #region Protected members
    protected IVehiclePhysics vehiclePhysics;
    protected VehicleAIBase aiBase;
    protected VehicleTrackData trackData;
    protected EvadeDir evadeDir = EvadeDir.None;
    #endregion

    #region Unity callbacks
    void Awake()
    {
        vehiclePhysics = gameObject.GetComponent<VehiclePhysics>();
        if (null == vehiclePhysics)
            vehiclePhysics = gameObject.GetComponent<VehiclePhysicsSimple>();

        aiBase = gameObject.GetComponent<VehicleAIBase>();
        trackData = gameObject.GetComponent<VehicleTrackData>();
    }

    void Update()
    {
        TrackBranch trackBranch = trackData.TrackBranch;
        Token trackToken = trackData.Token;
        if (null == trackBranch || null == trackToken)
            return;

        float trackWidth = trackData.Token.width,
              lookAhead  = trackWidth * lookAheadCoeff + vehiclePhysics.CurrentSpeed * lookAheddCoeffSpeed;

        float targetLat = 0.0f;
        GameObject frNeighbour = aiBase.sensor.GetNearestNeighbour(VehicleAISensor.Region.FrontRight),
                   fcNeighbour = aiBase.sensor.GetNearestNeighbour(VehicleAISensor.Region.FrontCenter),
                   flNeighbour = aiBase.sensor.GetNearestNeighbour(VehicleAISensor.Region.FrontLeft);
        VehicleTrackData frTrackData = (null == frNeighbour ? null : frNeighbour.GetComponent<VehicleTrackData>()),
                         fcTrackData = (null == fcNeighbour ? null : fcNeighbour.GetComponent<VehicleTrackData>()),
                         flTrackData = (null == flNeighbour ? null : flNeighbour.GetComponent<VehicleTrackData>());

        if (null == frNeighbour && null == fcNeighbour && null == flNeighbour)
        {
            if (Token.TokenType.Curve == trackData.Token.type)
            {
                if (trackData.Token.curveDir == -1.0f)
                    targetLat = 0.40f;
                else
                    targetLat = -0.40f;
            }

            evadeDir = EvadeDir.None;
        }
        else if (fcNeighbour != null)
        {
            if (EvadeDir.Right == evadeDir || (EvadeDir.None == evadeDir && trackData.Trasversal >= fcTrackData.Trasversal))
            {
                targetLat = fcTrackData.Trasversal + evadeWidth;

                if (frNeighbour != null)
                {
                    float rightDist = (targetLat - fcTrackData.Trasversal) * trackWidth * 0.5f;
                    if (SBSMath.Abs(rightDist) < 2.0f)
                        targetLat = frTrackData.Trasversal + evadeWidth;
                }

                evadeDir = EvadeDir.Right;
            }
            else if (EvadeDir.Left == evadeDir || (EvadeDir.None == evadeDir && trackData.Trasversal < fcTrackData.Trasversal))
            {
                targetLat = fcTrackData.Trasversal - evadeWidth;

                if (flNeighbour != null)
                {
                    float leftDist = (targetLat - flTrackData.Trasversal) * trackWidth * 0.5f;
                    if (SBSMath.Abs(leftDist) < 2.0f)
                        targetLat = flTrackData.Trasversal - evadeWidth;
                }

                evadeDir = EvadeDir.Left;
            }

            lookAhead *= 0.33f;
            targetLat = Mathf.Clamp(targetLat, -0.90f, 0.90f);
        }
        else
            evadeDir = EvadeDir.None;

        SBSVector3 targetPos = new SBSVector3(),
                   targetTan = new SBSVector3();

#if UNITY_FLASH
        RacingManager.Instance.track.EvaluateAt(trackBranch, trackData.TrackPosition + lookAhead, targetLat, trackData.branchesMask, targetPos, targetTan);
#else
        RacingManager.Instance.track.EvaluateAt(trackBranch, trackData.TrackPosition + lookAhead, targetLat, trackData.branchesMask, out targetPos, out targetTan);
#endif

        aiBase.targetSpeed = -1.0f;
        aiBase.targetPosition = targetPos;
        aiBase.forceBrake = false;

        if (fcNeighbour != null)
        {
            if ((fcTrackData.TrackPosition - trackData.TrackPosition) < 15.0f)
            {
                IVehiclePhysics otherPhys = fcNeighbour.GetComponent<VehiclePhysics>();
                if (null == otherPhys)
                    otherPhys = fcNeighbour.GetComponent<VehiclePhysicsSimple>();

                if (otherPhys != null)
                {
                    aiBase.targetSpeed = otherPhys.CurrentSpeed - 1.0f;
                    aiBase.forceBrake = true;
                }
            }
        }
    }
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
