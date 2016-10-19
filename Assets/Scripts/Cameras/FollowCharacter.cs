using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("Cameras/FollowCharacter")]
public class FollowCharacter : MonoBehaviour
{
    public GameObject player;
    public Vector3 sourceOffset = new Vector3(0.0f, 2.5f, -3.4f);
    public Vector3 targetOffset = new Vector3(0.0f, 1.7f, 0.0f);

    public float heightStep = 1.0f;
    public float heightSmoothing = 4.0f;
    public float heightSmoothing2 = 40.0f;
    public float offsetSmoothing = 1.0f;
    public float horizontalOffset = 0.75f;
    public float forwardOffset = 0.25f;
    public float backwardOffset = 0.75f;

    protected PlayerKinematics kinematics;
    protected Transform playerRoot;
    protected float lastPivotHeight;
    protected Vector3 offset = Vector3.zero;
    protected Vector3 prevPlayerPivot = Vector3.zero;

    protected float heightVelocity = 0.0f;

    protected Vector3 defaultSourceOffset;
    protected Vector3 defaultTargetOffset;

    protected bool interpolating = false;
    protected float t0, t1;
    protected Quaternion r0, r1;
    protected float m0, m1;
    protected Vector3 v0;
    protected Tuple<Vector3, Vector3> interpOffsets;

    protected bool firstFrame;
    protected float currHeightSmoothing;

    protected float groundHeightTest;

    protected Vector3 noise = SBSVector3.zero;
    protected float noiseStrength = 0.0f;
    protected float noiseStrengthVel = 0.0f;
    protected float noiseTarget = 0.0f;

    protected bool shakeCameraActive = false;
    protected float shakeStartTime = 0.0f;
    protected ShakeData currentShakeData;

    protected Vector3 lastSourceOffset, sourceOffsetAtInterpBegin;
    protected Vector3 lastTargetOffset;
    protected float smoothTime = 0.1f;
    protected Vector3 noiseTremor = Vector3.zero;
    protected bool actionTaken = false;
    protected float deadTime = -1.0f;

    protected bool slideshowActive = false;
    protected float slideshowEnterTime = 0.0f;
    protected float slideshowExitTime = 0.0f;

    protected bool turboCameraActive = false;
    protected float turboCameraEnterTime = 0.0f;
    protected float turboCameraExitTime = 0.0f;

    protected bool finalCameraActive = false;
    protected float finalCameraEnterTime = 0.0f;
    protected float finalCameraExitTime = 0.0f;

    protected bool oldCameraActive = true;

    protected float oldFov = 70.0f;
    protected Vector3 oldCamSourceOffset = new Vector3(0.0f, 8.5f, -4.5f);
    protected Vector3 oldCamTargetOffset = new Vector3(0.0f, 0.9f, 5.3f);

    protected float startEaseTime = 0.0f;
    protected float easeTimeDuration = 1.0f;
    protected Vector3 goalSourceOffset;
    protected Vector3 goalTargetOffset;

    protected int cameraIndex = 3;
    protected float[] cameraFovs = { 55.0f, 60.0f, 55.0f };
    protected Vector3[] cameraSourceOffsets = {
                                                  new Vector3(0.0f, 5.8f, -3.8f),
                                                  new Vector3(0.0f, 6.04f, -4.0f),
                                                  new Vector3(0.0f, 8.5f, -6.7f)
                                              };
    protected Vector3[] cameraTargetOffsets = {
                                                  new Vector3(0.0f, 2.2f, 2.5f),
                                                  new Vector3(0.0f, 1.35f, 3.36f),
                                                  new Vector3(0.0f, 1.45f, 5.3f)
                                              };

    //protected Vector3 newCamSourceOffset = new Vector3(0.0f, 5.8f, -3.8f);//Camera 1
    //protected Vector3 newCamTargetOffset = new Vector3(0.0f, 2.2f, 2.5f);//Camera 1
    protected Vector3 newCamSourceOffset = new Vector3(0.0f, 6.04f, -4.0f);//Camera 2
    protected Vector3 newCamTargetOffset = new Vector3(0.0f, 1.35f, 3.36f);//Camera 2
    //protected Vector3 newCamSourceOffset = new Vector3(0.0f, 8.5f, -6.7f); //Camera 3 (Alma)
    //protected Vector3 newCamTargetOffset = new Vector3(0.0f, 1.45f, 5.3f); //Camera 3 (Alma)
    protected Vector3 testNewTurboSourceOffset = new Vector3(0.0f, 5.8f, -4.0f);
    protected Vector3 testNewTurboTargetOffset = new Vector3(0.0f, 2.1f, 2.5f);
    protected Vector3 testNewFinalSourceOffset = new Vector3(-6.5f, 5.0f, -5.5f);
    protected Vector3 testNewFinalTargetOffset = new Vector3(-4.5f, 1.7f, 0.0f);

    public float CurrentFov
    {
        get 
        {
            float retValue = 0.0f;
            if (cameraIndex == 0)
                retValue = oldFov;
            else
                retValue = cameraFovs[cameraIndex - 1];
            return retValue;
        }
    }

    #region public Classes
    public class ShakeData
    {
        public float duration;
        public float noise;
        public float smoothTime;

        public ShakeData(float _duration, float _noise, float _smoothTime)
        {
            duration = _duration;
            noise = _noise;
            smoothTime = _smoothTime;
        }
    }
    #endregion

    void Start()
    {
        kinematics = player.GetComponent<PlayerKinematics>();
        lastPivotHeight = 0.0f;// kinematics.GroundHeight;
        playerRoot = player.transform.FindChild("player/ROOT");

        lastSourceOffset = defaultSourceOffset = sourceOffset;
        lastTargetOffset = defaultTargetOffset = targetOffset;
        goalSourceOffset = sourceOffset;
        goalTargetOffset = targetOffset;

        currentShakeData = new ShakeData(0.0f, 0.0f, 0.0f);

        slideshowActive = (int)OnTheRunDataLoader.Instance.GetSlideshowData()[0] == 1;
        slideshowEnterTime = OnTheRunDataLoader.Instance.GetSlideshowData()[1];
        slideshowExitTime = OnTheRunDataLoader.Instance.GetSlideshowData()[2];

        turboCameraActive = (int)OnTheRunDataLoader.Instance.GetTurboCameraData()[0] == 1;
        turboCameraEnterTime = OnTheRunDataLoader.Instance.GetTurboCameraData()[1];
        turboCameraExitTime = OnTheRunDataLoader.Instance.GetTurboCameraData()[2];

        finalCameraActive = (int)OnTheRunDataLoader.Instance.GetFinalCameraData()[0] == 1;
        finalCameraEnterTime = OnTheRunDataLoader.Instance.GetFinalCameraData()[1];
        finalCameraExitTime = OnTheRunDataLoader.Instance.GetFinalCameraData()[2];

        ChangeCamera(); 
    }

    void RestoreDefaultCamera(float duration = 1.0f)
    {
        this.StartEasing(defaultSourceOffset, defaultTargetOffset, duration);
        //this.InterpolateTo(new Tuple<Vector3, Vector3>(defaultSourceOffset, defaultTargetOffset), duration);
    }

    public Vector3 slideshowSourceOffset = new Vector3(4.0f, 5.0f, -2.0f);
    public Vector3 slideshowTargetOffset = new Vector3(0.0f, 0.0f, 2.5f);

    protected bool defaultPosition = true;
    void OnResetCameraEvent()
    {
        defaultPosition = true;

        lastSourceOffset = sourceOffset = defaultSourceOffset;
        lastTargetOffset = targetOffset = defaultTargetOffset;
    }

    void OnSlideshowEvent()
    {
        if (slideshowActive)
        {
            if (defaultPosition)
                this.StartEasing(slideshowSourceOffset, slideshowTargetOffset, slideshowEnterTime); //this.InterpolateTo(new Tuple<Vector3, Vector3>(slideshowSourceOffset, slideshowTargetOffset), slideshowEnterTime);
            else
                RestoreDefaultCamera(slideshowExitTime);
            defaultPosition = !defaultPosition;
        }
    }

    public Vector3 testTurboSourceOffset = new Vector3(0.0f, 6.0f, -2.0f);
    public Vector3 testTurboTargetOffset = new Vector3(0.0f, 2.6f, 2.5f);
    void OnTurboCameraEvent(bool active)
    {
        Vector3 sourceTemp = cameraIndex == 0 ? testTurboSourceOffset : testNewTurboSourceOffset;
        Vector3 targetTemp = cameraIndex == 0 ? testTurboTargetOffset : testNewTurboTargetOffset;
        if (turboCameraActive)
        {
            if (active)
                this.StartEasing(sourceTemp, targetTemp, turboCameraEnterTime); //this.InterpolateTo(new Tuple<Vector3, Vector3>(sourceTemp, targetTemp), turboCameraEnterTime);
            else
                RestoreDefaultCamera(turboCameraExitTime);
        }
    }

    public Vector3 finalSourceOffset = new Vector3(-5.5f, 5.0f, -4.0f);
    public Vector3 finalTargetOffset = new Vector3(-4.5f, 1.7f, 0.0f);
    protected GameObject playerRef;
    void OnFinalCameraEvent()
    {
        Vector3 sourceTemp = cameraIndex == 0 ? finalSourceOffset : testNewFinalSourceOffset;
        Vector3 targetTemp = cameraIndex == 0 ? finalTargetOffset : testNewFinalTargetOffset;
        if (finalCameraActive)
        {
            if (playerRef == null)
                playerRef = GameObject.FindGameObjectWithTag("Player");

            Vector3 playerPos = new Vector3(playerRef.transform.position.x, 0.0f, 0.0f);
            Vector3 realFinalSourceOffset = new Vector3(sourceTemp.x, sourceTemp.y, sourceTemp.z);
            Vector3 realFinalTargetOffset = new Vector3(targetTemp.x, targetTemp.y, targetTemp.z);
            if (playerPos.x < 0.0f)
            {
                realFinalSourceOffset = new Vector3(-realFinalSourceOffset.x, realFinalSourceOffset.y, realFinalSourceOffset.z);
                realFinalTargetOffset = new Vector3(-realFinalTargetOffset.x, realFinalTargetOffset.y, realFinalTargetOffset.z);
            }

            if (defaultPosition)
                this.StartEasing(playerPos + realFinalSourceOffset, playerPos + realFinalTargetOffset, finalCameraEnterTime); //this.InterpolateTo(new Tuple<Vector3, Vector3>(playerPos + realFinalSourceOffset, playerPos + realFinalTargetOffset), finalCameraEnterTime);
            else
                RestoreDefaultCamera(finalCameraExitTime);
            defaultPosition = !defaultPosition;
        }
    }

    void OnDeadCamera()
    {
        Vector3 deadTargetOffset;
        deadTime = TimeManager.Instance.MasterSource.TotalTime;

        deadTargetOffset = new Vector3(0.0f, 0.22f, 0.0f);

        this.StartEasing(defaultSourceOffset, deadTargetOffset, 1.0f); //this.InterpolateTo(new Tuple<Vector3, Vector3>(defaultSourceOffset, deadTargetOffset));
    }

    void InterpolateTo(Tuple<Vector3, Vector3> sourceTarget, float duration = 1.0f)
    {
        interpolating = true;
        interpOffsets = sourceTarget;

        t0 = TimeManager.Instance.MasterSource.TotalTime;
        t1 = t0 + duration;

        sourceOffsetAtInterpBegin = lastSourceOffset;

        v0 = lastTargetOffset - lastSourceOffset;
        Vector3 v1 = sourceTarget.Second - sourceTarget.First;

        r0 = Quaternion.identity;
        r1 = Quaternion.FromToRotation(v0, v1);

        m0 = 1.0f;
        m1 = v1.magnitude / v0.magnitude;
    }

    void StartEasing(Vector3 goalSource, Vector3 goalTarget, float duration)
    {
        interpolating = true;
        startEaseTime = TimeManager.Instance.MasterSource.TotalTime;
        easeTimeDuration = duration;
        goalSourceOffset = goalSource;
        goalTargetOffset = goalTarget;
    }

    Vector3 EaseTo(Vector3 source, Vector3 goalSource, Vector3 defaultGoal)
    {
        if (interpolating)
        {
            float now = TimeManager.Instance.MasterSource.TotalTime;
            //source.x = SBSEasing.EaseInOutCirc(now - startEaseTime, source.x, goalSource.x - source.x, easeTimeDuration);
            //source.y = SBSEasing.EaseInOutCirc(now - startEaseTime, source.y, goalSource.y - source.y, easeTimeDuration);
            //source.z = SBSEasing.EaseInOutCirc(now - startEaseTime, source.z, goalSource.z - source.z, easeTimeDuration);
            source.x = SBSEasing.EaseInOutQuad(now - startEaseTime, source.x, goalSource.x - source.x, easeTimeDuration);
            source.y = SBSEasing.EaseInOutQuad(now - startEaseTime, source.y, goalSource.y - source.y, easeTimeDuration);
            source.z = SBSEasing.EaseInOutQuad(now - startEaseTime, source.z, goalSource.z - source.z, easeTimeDuration);

            if (now - startEaseTime >= easeTimeDuration)
                interpolating = false;
        }
        else
            source = goalSource;

        return source;
    }

    protected void GetSourceAndTarget(out Vector3 source, out Vector3 target)
    {
        if (interpolating)
        {
            float t = TimeManager.Instance.MasterSource.TotalTime,
                  s = (t - t0) / (t1 - t0);

            if (s >= 1.0f)
            {
                sourceOffset = interpOffsets.First;
                targetOffset = interpOffsets.Second;
                interpolating = false;
            }

            s = Mathf.Clamp01(s);

            Quaternion r = Quaternion.Slerp(r0, r1, s);
            float m = m0 + (m1 - m0) * s;

            source = Vector3.Lerp(sourceOffsetAtInterpBegin, interpOffsets.First, s);
            target = source + r * v0 * m;
        }
        else
        {
            source = sourceOffset;
            target = targetOffset;
        }
    }

    void NoFirstFrame()
    {
        firstFrame = false;
    }

    public void OnFollowCharaEnter(float time)
    {
        //Debug.Log("follow chara enter" + transform.position);
        prevPlayerPivot = player.transform.position;
        firstFrame = true;
        currHeightSmoothing = heightSmoothing;
        deadTime = -1.0f;
        actionTaken = false;
        //this.RestoreDefaultCamera();
    }

    public void OnFollowCharaExec(float time)
    {
        //if (TimeManager.Instance.MasterSource.IsPaused)
        //    return;

        if (player == null)
            return;

        //if (kinematics.LastCollisionTime != -1)
        //    return;

        float dt = Time.fixedDeltaTime;// TimeManager.Instance.MasterSource.DeltaTime;
        float now = TimeManager.Instance.MasterSource.TotalTime;
        Vector3 playerPivot = player.transform.position;
        playerPivot.x = 0.0f;
        playerPivot.y = 0.0f;

        float targetHeight = playerPivot.y;

        if (firstFrame)
        {
            lastPivotHeight = targetHeight;
            prevPlayerPivot = playerPivot;
            heightVelocity = 0.0f;
            firstFrame = false;
        }
        else
        {
            float targetSmoothTime = 0.1f;

            smoothTime = Mathf.MoveTowards(smoothTime, targetSmoothTime, 2.5f * dt);

            lastPivotHeight = Mathf.SmoothDamp(lastPivotHeight, targetHeight, ref heightVelocity, smoothTime, 50.0f, dt);
            prevPlayerPivot = playerPivot;
        }

        /*
        Vector3 vel = kinematics.Velocity;
        if (vel.x > 1.0f)
            offset.x += (horizontalOffset - offset.x) * offsetSmoothing * dt;
        else if (vel.x < -1.0f)
            offset.x += (-horizontalOffset - offset.x) * offsetSmoothing * dt;
        else
            offset.x -= offset.x * offsetSmoothing * dt;

        if (vel.z > 1.0f)
            offset.z += (forwardOffset - offset.z) * offsetSmoothing * dt;
        else if (vel.z < -1.0f)
            offset.z += (-backwardOffset - offset.z) * offsetSmoothing * dt;
        else
            offset.z -= offset.z * offsetSmoothing * dt;
        */
        offset.z = -kinematics.PlayerRigidbody.velocity.z * 0.5f;//-kinematics.LinearVelocity;

        Vector3 camPivot = new Vector3(prevPlayerPivot.x * 0.8f, lastPivotHeight, prevPlayerPivot.z);

        //this.GetSourceAndTarget(out lastSourceOffset, out lastTargetOffset);
        lastSourceOffset = this.EaseTo(lastSourceOffset, goalSourceOffset, sourceOffset);
        lastTargetOffset = this.EaseTo(lastTargetOffset, goalTargetOffset, targetOffset);

        transform.position = camPivot + lastSourceOffset + offset * 0.1f + noise * noiseStrength; // +noise * noiseStrength + noiseTremor * 0.00069f * kinematics.PlayerRigidbody.velocity.z; //PIETRO

        transform.LookAt(camPivot + lastTargetOffset + offset * 0.1f, Vector3.up);

        if (!TimeManager.Instance.MasterSource.IsPaused)
        {
            //Camera Shake
            if (shakeCameraActive)
                ShakeCamera(dt);

            //tremor (always active
            this.UpdateTremor(dt);
        }

        //check if is dead
        if (now - deadTime > 3.6f && !actionTaken && deadTime > 0.0f)
        {
            actionTaken = true;
            //Debug.Log("GO TO REWARD");
            LevelRoot.Instance.BroadcastMessage("GoToOffgame");     //GoToReward");
        }
    }

    public void OnFollowCharaExit(float time)
    {
    }

    void OnReset()
    {
        //Debug.Log("RESET CAM");
        interpolating = false;
        shakeCameraActive = false;
        sourceOffset = defaultSourceOffset;
        targetOffset = defaultTargetOffset;
    }

    void OnResetPivot()
    {
        prevPlayerPivot = player.transform.position;

        firstFrame = true;
        currHeightSmoothing = heightSmoothing;

        this.RestoreDefaultCamera();
    }

    void StartShakeCamera(ShakeData sData)
    {
        if (shakeCameraActive && sData.noise < currentShakeData.noise)
        {
            return;
        }

        currentShakeData = sData;
        shakeStartTime = TimeManager.Instance.MasterSource.TotalTime;
        shakeCameraActive = true;
    }

    void UpdateTremor(float deltaTime)
    {
        Vector3 v = UnityEngine.Random.onUnitSphere;
        noiseTremor += (v - noiseTremor) * deltaTime * 8.0f;
        //if (kinematics.linearVelocity <= 0.5f)
        //  noiseTremor = Vector3.zero;
    }

    void ShakeCamera(float deltaTime)
    {
        if (TimeManager.Instance.MasterSource.TotalTime - shakeStartTime <= currentShakeData.duration)
        {
            if (currentShakeData.smoothTime > 0.0f)
                noiseStrength = Mathf.SmoothDamp(noiseStrength, currentShakeData.noise, ref noiseStrengthVel, currentShakeData.smoothTime, 300.0f, deltaTime);
            else
                noiseStrength = currentShakeData.noise; // go directly

            if (noiseStrength > 0.0f)
            {
                Vector3 v = UnityEngine.Random.onUnitSphere;
                noise += (v - noise) * deltaTime * 8.0f;
            }
            else
                noise = SBSVector3.zero;
        }

        if (TimeManager.Instance.MasterSource.TotalTime - shakeStartTime >= currentShakeData.duration)
            StopShakeCamera();
    }

    void StopShakeCamera()
    {
        currentShakeData = new ShakeData(0.0f, 0.0f, 0.0f);
        noiseStrength = 0.0f;
        noise = SBSVector3.zero;
        shakeCameraActive = false;
    }

    void OnChangePlayerCar()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        kinematics = player.GetComponent<PlayerKinematics>();
    }

    public string ChangeCamera()
    {
        //if (interpolating || kinematics.TurboOn)
        //    return cameraIndex == 0 ? "Old camera on" : "camera " + cameraIndex + " on";

        //++cameraIndex;
        //if (cameraIndex > cameraSourceOffsets.Length)
        //    cameraIndex = 0;

        string buttonText = "";
        buttonText = cameraIndex == 0 ? "Old camera on" : "camera " + cameraIndex + " on";
        if (cameraIndex == 0)
        {
            gameObject.GetComponent<Camera>().fieldOfView = oldFov;
            lastSourceOffset = defaultSourceOffset = sourceOffset = oldCamSourceOffset;
            lastTargetOffset = defaultTargetOffset = targetOffset = oldCamTargetOffset;
            goalSourceOffset = oldCamSourceOffset;
            goalTargetOffset = oldCamTargetOffset;
        }
        else
        {
            gameObject.GetComponent<Camera>().fieldOfView = cameraFovs[cameraIndex - 1];
            lastSourceOffset = defaultSourceOffset = sourceOffset = cameraSourceOffsets[cameraIndex - 1];
            lastTargetOffset = defaultTargetOffset = targetOffset = cameraTargetOffsets[cameraIndex - 1];
            goalSourceOffset = cameraSourceOffsets[cameraIndex - 1];
            goalTargetOffset = cameraTargetOffsets[cameraIndex - 1];
        }
        
        return buttonText;
    }
}
