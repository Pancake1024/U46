using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("Player/Drive")]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerDrive : MonoBehaviour
{
    #region Protected members
    private bool firstFrame = true;
    //protected float startLinearVelocity;
    //protected float maxLinearVelocity;
    protected float wantedTrasversal = 0.0f;
    protected VehiclePhysicsSimple controller;
    protected VehicleAIBase aiBase;

    protected NeverendingPlayer playerData;

    protected bool inputLeft = false;
    protected bool inputRight = false;
    //protected float inputDirection = 0.0f;

    //protected int toLane = 0;
    protected float latSpeed = 0.0f;

    //protected Track track;
    //protected VehicleTrackData trackData;
    protected float trasvVel = 0.0f;
    protected float prevTrasversal = 0.0f;
//    protected ZG_CameraManager cameraManager;
    protected Vector3 velocity = Vector3.zero;
    protected float lastLeftRequestTime = -1.0f;
    protected float lastRightRequestTime = -1.0f;
    protected bool isHit = false;
    protected CollisionTypes lastCollisionType;
    protected bool turnLeft = false;
    protected bool accelerationStarted = false;
    protected bool isDead = false;
    //protected GameObject cameraRef;
    protected ShotDirections shotDirection;
    protected bool isReached = false;
    protected bool returning = false;
    //protected bool laneReached = true;

    protected float lastCollisionTime = -1.0f;

    protected float linearVelocity = 0.0f;
    protected float lateralVelocity = 0.0f;

    protected int prevLane;
    #endregion

    #region Public members
    //public float startLinearVelocity = 10.0f;
    public float maxLinearVelocity = 30.0f;
    public float maxLateralVelocity = 10.0f;

    public int currentLane;
    
    public SBSVector3 TokenTangent
    {
        get
        {
            return playerData.TokenTangent;
        }
    }
	/*
    public SBSVector3 TokenNormal
    {
        get
        {
            return playerData.TokenNormal;
        }
    }
	*/
    public float TokenTrasversal
    {
        get
        {
            return playerData.TokenTrasversal;
        }
    }

    public bool PlayerIsDead
    {
        get 
        {
            return isDead;
        }
    }

    public string lastColliderName;
    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
    }

    public float LinearVelocity
    {
        get
        {
            return linearVelocity;
        }
    }

    public float WantedTrasversal
    {
        get
        {
            return wantedTrasversal;
        }
    }

    public enum CollisionTypes
    {
        obstacleFront,
        obstacleLeft,
        obstacleRight,
        shot,
        enemyCrash
    }

    public enum ShotDirections
    {
        left,
        right,
        center
    }

    public CollisionTypes LastCollisionType
    {
        get
        {
            return lastCollisionType;
        }
    }

    public bool TurnLeft
    {
        get
        {
            return turnLeft;
        }
    }

    public ShotDirections ShotDirection
    {
        get
        {
            return shotDirection;
        }
    }

    #endregion

    #region Public methods
    #endregion

    #region protected methods
    #endregion

    #region Unity callbacks
    void Awake()
    {
        //track = null;
        //trackData = gameObject.GetComponent<VehicleTrackData>();
        //startLinearVelocity = LevelRoot.Instance.GetComponent<LevelSettings>().playerSpeed;
        //maxLinearVelocity = LevelRoot.Instance.GetComponent<LevelSettings>().MaxPlayerSpeed;
        controller = gameObject.GetComponent<VehiclePhysicsSimple>();
        aiBase = gameObject.GetComponent<VehicleAIBase>();

        gameObject.SendMessage("ResetPhysics");

        GetComponent<Rigidbody>().centerOfMass = new Vector3(0.0f, 0.0f, 0.0f);
        //prevVel = new Vector3();

        gameObject.SendMessage("ResetWheels"); // TODO ALEX

        linearVelocity = 0.0f;
        lateralVelocity = 0.0f;
    }

    void Start()
    {
        //cameraRef = GameObject.FindGameObjectWithTag("MainCamera");
        playerData = GetComponent<NeverendingPlayer>();
        //toLane = currentLane;
        prevLane = currentLane;
    }


    void Update()
    {
        if (firstFrame)
        {
            firstFrame = false;
            return;
        }
        //test stuff

       /*
        //handle movement requests
        bool isChangingLane = Mathf.Abs(trasvVel) > 0.55f;
        bool canChangeLane = false;
        float trasversalTo = 0.0f;
        float now = TimeManager.Instance.MasterSource.TotalTime;
        bool isInMiddleLane = Mathf.Abs(trackData.Trasversal) < 0.15f;
     
        if (lastLeftRequestTime > 0.0f && now - lastLeftRequestTime < 0.55f && !isHit)
        {
            canChangeLane = trackData.Trasversal >= -0.2f && !isChangingLane;
            turnLeft = true;
            if (isInMiddleLane)
                trasversalTo = -0.6f;
            else
                trasversalTo = 0.0f;
            
            //if (!canChangeLane)
              //  Debug.Log("CANNOT LEFT" + trackData.Trasversal + "isChangingLane" + isChangingLane);
        }
        if (lastRightRequestTime > 0.0f && now - lastRightRequestTime < 0.55f && !isHit)
        {
            canChangeLane = trackData.Trasversal <= 0.2f && !isChangingLane;
            turnLeft = false;
            if (isInMiddleLane)
                trasversalTo = 0.6f;
            else
                trasversalTo = 0.0f;
           
            //if (!canChangeLane)
              //  Debug.Log("CANNOT RIGHT" + trackData.Trasversal + "isChangingLane" + isChangingLane + "trasv" + trasvVel);
        }

        if (canChangeLane)
        {
            lastLeftRequestTime = -1.0f;
            lastRightRequestTime = -1.0f;
            prevTrasversal = wantedTrasversal;
            wantedTrasversal = trasversalTo;
            gameObject.SendMessage("OnTurn", turnLeft);            
        }
        * */
        //this.GetShootTargetForEnemies(this.GetTrasversalForEnemies());
    }
    
    void FixedUpdate()
    {
        /*
        if (null == track)
            return;

        trackData.ForceUpdate();
        //Debug.Log("PlayerKinematics.FixedUpdate - startLinearVelocity: " + startLinearVelocity);
        float dt = Time.fixedDeltaTime;//TimeManager.Instance.MasterSource.DeltaTime;
        float trackPos = trackData.TrackPosition, trasv = trackData.Trasversal;

        if (linearVelocity < maxLinearVelocity && accelerationStarted && !isDead && isReached)
            linearVelocity = Mathf.MoveTowards(linearVelocity, maxLinearVelocity, dt * 35.0f);

        if (linearVelocity < startLinearVelocity && accelerationStarted && !isDead && !isReached)       
            linearVelocity = Mathf.MoveTowards(linearVelocity, startLinearVelocity, dt * 13.0f);
        //Debug.Log("LA VELOCITA' VALE: " + linearVelocity + " isfinished" + isFinished);
        if (isDead)
        {
            float decrementSpeed = 3.7f + startLinearVelocity * 0.126f;
            linearVelocity = Mathf.MoveTowards(linearVelocity, 0.0f, dt * decrementSpeed);
        }
        
        linearVelocity = Mathf.Clamp(linearVelocity, 0.0f, isReached ? maxLinearVelocity : startLinearVelocity);


        trackPos += linearVelocity * dt;
        trasv = Mathf.SmoothDamp(trasv, wantedTrasversal, ref trasvVel, 0.3f, 2.0f, dt);

        SBSVector3 newPos, newTan;
        track.EvaluateAt(track.GetRootBranch(), trackPos, trasv, -1, out newPos, out newTan);

        SBSVector3 dir = newPos - (SBSVector3)rigidbody.position;
        float distance = dir.Normalize();
        
        //todo if can be hit
        Collider[] coll = Physics.OverlapSphere(transform.position, 1f);
        RaycastHit[] hits = rigidbody.SweepTestAll(dir, distance + 2.0f);

        Collider c = null;
        string coName = string.Empty;
        foreach (Collider co in coll)
        {
            if (co.isTrigger || co == collider)
                continue;
            c = co;
            coName = co.name;
            break;
        }

        velocity = dir * distance / dt;
        
        bool hitAnObject = (hits.Length > 0 && hits[0].distance < distance) || coName.Length > 0;
        
        if (hitAnObject)
        {          
            Collider col = hits.Length > 0 ? hits[0].collider : c;
            string collName = col.name;
            /*
            if (collName.Length > 0)
            {
                if (collName.Contains("LateralWall"))  //special case: hit a lateral wall
                {
                    Debug.Log("lateralwallhit" + prevTrasversal + "want " + wantedTrasversal);
                    if (prevTrasversal < wantedTrasversal)
                        gameObject.SendMessage("OnNotifyHit", CollisionTypes.obstacleRight);
                    else
                        gameObject.SendMessage("OnNotifyHit", CollisionTypes.obstacleLeft);
                    wantedTrasversal = prevTrasversal;  //goes back to previous lane
                }
                else if (col.gameObject.tag.Equals("EnemyBullet"))
                {
                    col.gameObject.SendMessage("OnPlayerHit");
                    //check direction to play custom anims

                    //Vector3 localPoint = this.transform.InverseTransformPoint(col.transform.position);
                    float dot = Vector3.Dot(-col.gameObject.transform.forward.normalized, transform.right);

                    Vector3 sparksEffectSpawnOffset = Vector3.zero;

                    if (Mathf.Abs(dot) < 0.09f)
                    {
                        shotDirection = ShotDirections.center;
                        sparksEffectSpawnOffset = Vector3.forward;
                    }
                    else if (dot > 0.0f)
                    {
                        shotDirection = ShotDirections.right;
                        sparksEffectSpawnOffset = 0.5f * Vector3.right;
                    }
                    else
                    {
                        shotDirection = ShotDirections.left;
                        sparksEffectSpawnOffset = -0.5f * Vector3.right;
                    }

                    EffectsManager.Instance.RequestEffect(transform, sparksEffectSpawnOffset, EffectsManager.EffectType.Sparks, true);
                    FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(0.3f, 0.3f, 0.0f);
                    //cameraRef.SendMessage("StartShakeCamera", sData);

                    //Debug.Log("xDiff" + dot + "foprward" + col.gameObject.transform.forward.normalized);
                    gameObject.SendMessage("OnNotifyHit", CollisionTypes.shot);
                }
                else
                {
                    Debug.Log("HIT NORMAL" + collName);
                    lastColliderName = collName;
                    gameObject.SendMessage("OnNotifyHit", CollisionTypes.obstacleFront);

                    if (collName.Contains("kamikaze"))
                    {
                        col.gameObject.transform.parent.SendMessage("OnEnemyDead");
                        EffectsManager.Instance.RequestEffect(transform, Vector3.zero, EffectsManager.EffectType.EnemyExplosion, true);
                    }
                }
                
                if (!collName.Contains("kamikaze"))
                    col.enabled = false;
            }
            */
        /*
            //Debug.Log("asdasd " + hits[0].collider.name + "dist " + hits[0].distance);
        }
        */
        float dt = Time.fixedDeltaTime;
        float time = TimeManager.Instance.MasterSource.TotalTime;
        //Debug.Log("time - lastCollisionTime: " + (time - lastCollisionTime));
        //Debug.Log("lastCollisionTime: " + lastCollisionTime);
        if (lastCollisionTime >= 0.0f && (time - lastCollisionTime) <= 2.0f)
        {
            //Debug.Log("InCollision: " + time);
            //if (this.rigidbody.isKinematic)
            //    this.rigidbody.isKinematic = false;
            //this.rigidbody.constraints = RigidbodyConstraints.None;
           // return;
            
        }
        else
            linearVelocity = Mathf.MoveTowards(linearVelocity, maxLinearVelocity, dt * 6.0f);
        //this.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        //if (!this.rigidbody.isKinematic)
        //    this.rigidbody.isKinematic = true;

        float inputDirection = 0.0f;
        if (inputRight)
            inputDirection += 1.0f;
        if (inputLeft)
            inputDirection -= 1.0f;

        
        //aiBase.targetSpeed = maxLinearVelocity;
        //aiBase.targetPosition = rigidbody.position + new Vector3(0.0f, 0.0f, 100.0f); ;

        Debug.Log("aiBase.targetSpeed: " + aiBase.targetSpeed);

        if (controller.GetComponent<Rigidbody>() != null)
        {
            controller.Throttle =  0.5f;
            controller.Steering = 1.0f;

            SBSVector3 force = SBSVector3.zero,
                       torque = SBSVector3.zero;

            controller.AccumForces(dt, ref force, ref torque);

            GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
            GetComponent<Rigidbody>().AddTorque(torque, ForceMode.Force);
        }
        if (playerData.lastToken == null)
            return;



        //if (linearVelocity < maxLinearVelocity) && accelerationStarted && !isDead && isReached)

        float targetX = playerData.lastToken.width * 0.5f * OnTheRunEnvironment.lanes[currentLane];
        float deltaX = 0.0f;
        float deltaToTarget = targetX - GetComponent<Rigidbody>().position.x;
        if (deltaToTarget > 0.0f)
        {
            latSpeed = Mathf.MoveTowards(latSpeed, maxLateralVelocity, dt * 60.0f);
            if (deltaToTarget < latSpeed * dt)
            {
                deltaX = deltaToTarget;
                deltaToTarget = 0.0f;
            }
            else
                deltaX = latSpeed * dt;
        }
        else if (deltaToTarget < 0.0f)
        {
            latSpeed = Mathf.MoveTowards(latSpeed, -maxLateralVelocity, dt * 60.0f);
            if (deltaToTarget > latSpeed * dt)
            {
                deltaX = deltaToTarget;
                deltaToTarget = 0.0f;
            }
            else
                deltaX = latSpeed * dt;
        }

        bool isOnLane = Mathf.Abs(deltaToTarget) < SBSMath.Epsilon;
        int newLane = currentLane;
        if (inputDirection > 0.0f && currentLane < OnTheRunEnvironment.lanes.Length - 1)
            newLane++;
        else if (inputDirection < 0.0f && currentLane > 0)
            newLane--;

        if (isOnLane)
        {
            prevLane = currentLane;
            latSpeed = Mathf.MoveTowards(latSpeed, 0.0f, dt * 120.0f);
        }

        if (currentLane == prevLane && isOnLane)
            currentLane = newLane;
        else if (newLane == prevLane)
            currentLane = newLane;

        //Vector3 newTan = new Vector3(latSpeed * dt * 0.6f, 0.0f, 1.0f);
        //rigidbody.rotation = Quaternion.LookRotation(newTan);
        //rigidbody.position = rigidbody.position + new Vector3(deltaX, 0.0f, linearVelocity * dt);


    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter: " + collision.);
        lastCollisionTime = TimeManager.Instance.MasterSource.TotalTime;
        Debug.Log("** lastCollisionTime: " + lastCollisionTime);
        linearVelocity = 10.0f;
    }

    #endregion

    #region Messages
    //void OnFinishedd()
    //{
    //    isFinished = true;
    //}

    void OnGameover()
    {
        inputLeft = false;
        inputRight = false;
    }

    void OnReachedTeleport()
    {
        isReached = true;
    }

    void OnLeftInputDown()
    {
        inputLeft = true;
        if(accelerationStarted)
            lastLeftRequestTime = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnLeftInputUp()
    {
        inputLeft = false;
    }

    void OnRightInputDown()
    {
        inputRight = true;
        if (accelerationStarted)
            lastRightRequestTime = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnRightInputUp()
    {
        inputRight = false;
    }

    void ChangeLaneRequest(float _wantedTrasveral)
    {
        //changing lane
        wantedTrasversal = _wantedTrasveral;
    }

    void OnAccelerationStarted()
    {
        //startLinearVelocity = LevelRoot.Instance.GetComponent<LevelSettings>().playerSpeed;
        accelerationStarted = true;
    }

    void OnExitGame()
    {
        linearVelocity = 0.0f;
        lateralVelocity = 0.0f;
        //startLinearVelocity = 0.0f;
        accelerationStarted = false;
    }

    void OnReset()
    {
        accelerationStarted = false;
        //startLinearVelocity = LevelRoot.Instance.GetComponent<LevelSettings>().playerSpeed;
        isDead = false;
        isReached = false;
        //gameObject.transform.position = GameObject.Find("Start").transform.position;
    }

    void OnNotifyHit(CollisionTypes collisionType)
    {
        Debug.Log("OnNotifyHit " + collisionType);

        //SpawnDustPuffEffect(collisionType);

        lastCollisionType = collisionType;

        if (isHit || isDead)                 
            return;        //already hit
        
        /*
        float damage;
        float shakeIntensity = 0.6f;
        float shakeDuration = 0.34f;

        switch (collisionType)
        {
            default:
            case CollisionTypes.obstacleFront:
            case CollisionTypes.obstacleLeft:
            case CollisionTypes.obstacleRight:
                damage = 15.0f;
                break;          
            case CollisionTypes.shot:
                shakeIntensity = 0.35f;
                shakeDuration = 0.3f;
                damage = 15.0f;
                break;
            case CollisionTypes.enemyCrash:                   //kamikaze?
                damage = 24.0f;
                break;
        }
              
        isHit = true;
        //FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(shakeDuration, shakeIntensity, 0.0f);
        //cameraRef.SendMessage("StartShakeCamera", sData);
        */
    }

    /*void SpawnDustPuffEffect(CollisionTypes collisionType)
    {
        Vector3 spawnOffset = Vector3.zero;

        switch (collisionType)
        {
            case CollisionTypes.obstacleFront:
                spawnOffset = Vector3.forward;
                break;

            case CollisionTypes.obstacleLeft:
                spawnOffset = -0.5f * Vector3.right;
                break;

            case CollisionTypes.obstacleRight:
                spawnOffset =0.5f *  Vector3.right;
                break;
        }

        //EffectsManager.Instance.RequestEffect(transform, spawnOffset, EffectsManager.EffectType.DustPuff, true);
        //FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(0.3f, 0.3f, 0.0f);
        //cameraRef.SendMessage("StartShakeCamera", sData);
    }*/


    void OnDead()
    {
        isDead = true;
        linearVelocity = linearVelocity * 0.7f;
        //cameraRef.SendMessage("OnDeadCamera");
    }


    void OnEndHit()
    {      
        isHit = false;       
    }

    void OnLevelStarted()
    {
        /*
        track = RacingManager.Instance.track;

        SBSVector3 newPos, newTan;
        track.EvaluateAt(track.GetRootBranch(), 0.0f, 0.0f, -1, out newPos, out newTan);

        rigidbody.position = newPos;
        rigidbody.rotation = Quaternion.LookRotation(newTan);
       */
        /*
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ZG_CameraManager>();
        if (cameraManager.CamState != ZG_CameraManager.CamStates.happy)
            LevelRoot.Instance.BroadcastMessage("OnFollowCamera");
        */
        isReached = false;
    }

    #endregion
}
