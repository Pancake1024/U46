using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Racing/VehicleRespawner")]
public class VehicleRespawner : MonoBehaviour
{
    #region Public members
    public float maxDeltaTrackPos = 15.0f;
    public float fadeOutTime = 1.0f;
    public float fadeInTime = 1.0f;
    #endregion

    #region Protected members
    protected VehiclePhysics vehiclePhysics = null;
    protected VehicleTrackData trackData = null;
    protected float prevTrackPos = 0.0f;
    protected float maxTrackPos = 0.0f;
    protected bool inRespawn = false;
    #endregion

    public void OnEnabled()
    {
        maxTrackPos = trackData.TrackPosition;
    }

    #region Messages
    void StartRespawn()
    {
        inRespawn = true;
        if (UIManager_OLD.Instance != null)
            UIManager_OLD.Instance.gameObject.SendMessage("StartFadeOut", fadeOutTime);
    }

    void OnFadeCompleted()
    {
        if (inRespawn)
        {
            if (vehiclePhysics != null)
            {
                SBSVector3 p, t;
                RacingManager.Instance.track.EvaluateAt(trackData.TrackBranch, maxTrackPos, 0.25f, trackData.branchesMask, out p, out t);

                RaycastHit hit;
                Physics.Raycast(p + SBSVector3.up * 500.0f, SBSVector3.down, out hit, 1000.0f, ~(1 << LayerMask.NameToLayer("Ignore Raycast")));

                vehiclePhysics.GetComponent<Rigidbody>().position = hit.point + Vector3.up * 1.0f;
                vehiclePhysics.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(t);

                vehiclePhysics.DynamicStart = 80.0f;
            }

            this.EndRespawn();
        }
    }

    void EndRespawn()
    {
        if (UIManager_OLD.Instance != null)
            UIManager_OLD.Instance.gameObject.SendMessage("StartFadeIn", fadeInTime);

        maxTrackPos = trackData.TrackPosition;
        inRespawn = false;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        vehiclePhysics = gameObject.GetComponent<VehiclePhysics>();
        trackData = gameObject.GetComponent<VehicleTrackData>();
    }

    void LateUpdate()
    {
        if (null == trackData || inRespawn)
            return;

        maxTrackPos = SBSMath.Max(maxTrackPos, trackData.TrackPosition);

        float lastFrameDelta = trackData.TrackPosition - prevTrackPos;
        prevTrackPos = trackData.TrackPosition;

        float deltaTrackPos = maxTrackPos - trackData.TrackPosition;
        if (SBSMath.Abs(lastFrameDelta) >= RacingManager.Instance.track.Length * 0.9f)
        {
            deltaTrackPos = 0.0f;
            maxTrackPos = trackData.TrackPosition;
        }

        if (deltaTrackPos >= maxDeltaTrackPos || Input.GetKeyDown(KeyCode.R)) //CHEAT
            gameObject.SendMessage("StartRespawn");
    }
    #endregion
}