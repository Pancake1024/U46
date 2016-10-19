using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections;

[AddComponentMenu("Opponent/Kinematics")]
[RequireComponent(typeof(Rigidbody))]
public class OpponentKinematics : IntrusiveListNode<OpponentKinematics>
{
    public static float lateralCollisionTime = -1.0f;
    public GameObject[] tunnelLights;

    #region Protected members
    protected OnTheRunSpawnManager spawnManager;
    protected NeverendingPlayer playerData;
    protected bool isDead = false;
    protected float lastCollisionTime = -1.0f;
    protected float lastLateralCollisionTime = -1.0f;
    protected float lastFxCollisionTime = -1.0f;
    protected int prevLane;
    protected Vector3 startPosition;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunEnvironment environmentManager;
    protected bool isChangingLane = false;
    protected bool isChangingLaneCollision = false;
    protected float backCollisionTime = -1.0f;
    protected float backupSideAcc = -1.0f;
    protected GameObject playerRef;
    protected bool exploded = false;
    protected bool wandering = false;
    protected bool isBonus = false;
    protected bool isTruck = false;
    protected int forwardDirectionOnLane = 1;
    protected LayerMask ignoreLayers = 0;
    protected int coinsForCrashNormal;
    protected int coinsForCrashSpecial;

    protected float minPlayerDistance = float.MaxValue;

    //change lane
    protected bool canChangeLane = true;
    protected GameObject arrowLeft;
    protected GameObject arrowRight;
    protected int finalTargetLane = -1;
    protected bool laneChoosen = false;
    protected bool beginArrowBlink = false;
    protected bool beginMovement = false;
    protected float changeLaneArrowTimer = -1.0f;
    protected float blinkArrowTimer = -1.0f;
    protected int blinkArrowSide = 0;
    protected bool laneAlreadyChanged = true;
    protected float changeLaneDistance = -1.0f;
    protected float arrowChangeLaneTime = 1.0f;
    protected float blinkArrowTime = 0.2f; //Arrows blink frequency time
    protected float minChangeLaneDistance = 100.0f; //Min distance from player to change lane
    protected float maxChangeLaneDistance = 150.0f; //Max distance from player to change lane
    protected float changeLaneTime = -1.0f;
    protected bool forceChangeLane = false;

    protected bool collidedWithPlayer = false;
    protected int currentLane;
    protected PoliceBehaviour PoliceBehaviour = null;

    protected bool insideTankShotArea = false;
    protected float currentMaxSpeed;

    protected Rigidbody rb;
    #endregion

    #region Public members
    public OpponentId vehicleId;
    public float forwardAcc = 5.0f;
    public float maxSpeed = 150.0f;
    public float maxSpeedBackwards = 120.0f;
    public float grip = 16.0f;
    public float sideAcc = 180.0f;
    public float sideAcc2 = 40.0f;
    public float laneDelta = 0.5f;
    public float laneDelta2 = 0.10f;
    public float rotCoeff = 0.01f;
    #endregion

    #region Public properties
    public Rigidbody OpponentRigidbody
    {
        get { return rb; }
    }

    public class PhysicParameters
    {
        public float acceleration;
        public float maxSpeed;
        public float maxSpeedWrongDirection;
        public float sideAcceleration;

        public PhysicParameters(float _acceleration, float _maxSpeed, float _maxSpeedWrongDirection, float _sideAcceleration)
        {
            acceleration = _acceleration;
            maxSpeed = _maxSpeed;
            maxSpeedWrongDirection = _maxSpeedWrongDirection;
            sideAcceleration = _sideAcceleration;
        }
    }

    public enum OpponentId
    {
        TrafficGeneric = -1,
        TrafficAsia1 = 0,
        TrafficAsia2,
        TrafficAsia3,
        TrafficAsia4,
        TrafficAsia5,
        TrafficAsia6,
        TrafficEu1,
        TrafficEu2,
        TrafficEu3,
        TrafficEu4,
        TrafficEu5,
        TrafficEu6,
        TrafficPolice,
        TrafficNy1,
        TrafficNy2,
        TrafficNy3,
        TrafficNy4,
        TrafficNy5,
        TrafficNy6,
        TrafficUs1,
        TrafficUs2,
        TrafficUs3,
        TrafficUs4,
        TrafficUs5,
        TrafficUs6,
        TrafficAsia7,
        TrafficAsia8,
        TrafficAsia9,
        TrafficAsia10,
        TrafficEu7,
        TrafficEu8,
        TrafficEu9,
        TrafficEu10,
        TrafficNy7,
        TrafficNy8,
        TrafficNy9,
        TrafficNy10,
        TrafficUs7,
        TrafficUs8,
        TrafficUs9,
        TrafficUs10,
        TrafficAsia11,
        TrafficEu11,
        TrafficNy11,
        TrafficUs11
    }

    public int CurrentLane
    {
        get
        {
            return currentLane;
        }
        set
        {
            currentLane = value;
        }
    }

    public float MinDistanceFromFromPlayer
    {
        get
        {
            return minPlayerDistance;
        }
    }

    public float CurrentDistanceFromFromPlayer
    {
        get
        {
            if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");
            return gameObject.transform.position.z - playerRef.transform.position.z;
        }
    }

    public int PreviousLane
    {
        get
        {
            return prevLane;
        }
        set
        {
            prevLane = value;
        }
    }

    public int LaneDirection
    {
        set
        {
            forwardDirectionOnLane = value;
        }
    }

    public bool PlayerIsDead
    {
        get
        {
            return isDead;
        }
    }

    public bool IsExploded
    {
        set { exploded = value; }
        get { return exploded; }
    }

    public bool IsBadGuy
    {
        get { return PoliceBehaviour != null; }
    }

    public bool IsBonus
    {
        set { isBonus = value; }
        get { return isBonus; }
    }

    public bool IsTruck
    {
        set { isTruck = value; }
        get { return isTruck; }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));
        currentMaxSpeed = maxSpeed;
        startPosition = rb.position;
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.GetComponent<OnTheRunSpawnManager>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        backupSideAcc = sideAcc;
        Transform arrowTr = gameObject.transform.Find("arrow_left");
        if(arrowTr!=null)
        {
            arrowLeft = arrowTr.gameObject;
            arrowRight = gameObject.transform.Find("arrow_right").gameObject;
        }

        PoliceBehaviour = gameObject.GetComponent<PoliceBehaviour>();

        coinsForCrashNormal = OnTheRunDataLoader.Instance.GetCrashData()[0];
        coinsForCrashSpecial = OnTheRunDataLoader.Instance.GetCrashData()[1];

        PhysicParameters data = OnTheRunDataLoader.Instance.GetOpponentPhysicData("opponent");
        forwardAcc = data.acceleration;
        maxSpeed = data.maxSpeed;
        maxSpeedBackwards = data.maxSpeedWrongDirection;
        sideAcc = data.sideAcceleration;
    }

    void OnEnable()
    {
        spawnManager.__opponents.Add(this);
    }

    void OnDisable()
    {
        if (spawnManager != null)
            spawnManager.__opponents.Remove(this);
    }

    void Start()
    {
        playerData = GetComponent<NeverendingPlayer>();
        OnReset();

        //tunnelLights = GameObject.FindGameObjectsWithTag("TunnelLights");
        SetupLights();
    }
    
    #region Special Powers Functions
    void OnPlayerEndJump( float angle )
    {
        //Manager<UIManager>.Get().ActivePage.SendMessage("OnScoreForCollision", rigidbody.position);
        if (gameplayManager.IsSpecialCarActive)
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashSpecial);
        else
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashNormal);

        exploded = true;
        gameObject.BroadcastMessage("ActivateShadow", false, SendMessageOptions.DontRequireReceiver);

        Vector3 vect = new Vector3(0.0f, 2.0f, 3.0f);
        vect = Quaternion.AngleAxis(angle, Vector3.up) * vect;
        rb.AddForceAtPosition(vect * 30.0f, rb.position, ForceMode.VelocityChange);

        rb.constraints = RigidbodyConstraints.None;
        OnTheRunObjectsPool.Instance.RequestEffectInPoint(rb.position, Vector3.zero, OnTheRunObjectsPool.ObjectType.Sparks);
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.zero, OnTheRunObjectsPool.ObjectType.Explosion, true);
        OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.zero, OnTheRunObjectsPool.ObjectType.Smoke, true);

        LevelRoot.Instance.BroadcastMessage("OnDestroyspecificCar", vehicleId);
        LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
    }
    #endregion

    public void MyFixedUpdate()
    {
        //if (lastCollisionTime != -1.0f)
        //    return;

        var rbPos = rb.position;
        if (wandering && !exploded)
        {
            if (Math.Abs(rbPos.x) > 8.0f)
            {
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rbPos, coinsForCrashSpecial);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
                CarExplode(40.0f, transform.position + transform.forward);
            }
        }

        if (exploded || wandering)
            return;

        float dt = Time.fixedDeltaTime;

        float targetX = 17.5f * 0.5f * OnTheRunEnvironment.lanes[currentLane],
              deltaToTarget = targetX - rbPos.x;

        bool isOnLane = Mathf.Abs(deltaToTarget) < 0.5f;
        
        if (isOnLane && isChangingLane)
        {
            isChangingLane = false;
            if (PoliceBehaviour != null)
                PoliceBehaviour.SendMessage("OnChangeLaneEnd");
        }
        else if (isOnLane && isChangingLaneCollision)
        {
            isChangingLaneCollision = false;
        }

        if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");
        float diff = gameObject.transform.position.z - playerRef.transform.position.z;
        if (diff < minPlayerDistance)
            minPlayerDistance = diff;

        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);
		bool isBonus = gameObject.CompareTag("Bonus");
		bool avoidForces = gameplayManager.MagnetActive && isBonus;
        var rbVel = rb.velocity;
        if (!avoidForces)
        {
            if (deltaToTarget > laneDelta)
                force.x = sideAcc * dt;
            else if (deltaToTarget < -laneDelta)
                force.x = -sideAcc * dt;
            else if (deltaToTarget > laneDelta2)
                force.x = sideAcc2 * dt;
            else if (deltaToTarget < -laneDelta2)
                force.x = -sideAcc2 * dt;
            force.x -= rbVel.x * grip * dt;

            //if (rigidbody.velocity.z <= maxSpeed * VehicleUtils.ToMetersPerSecs)
            //    force.z = forwardAcc * dt;

            if (Math.Abs(rbVel.z) <= currentMaxSpeed * VehicleUtils.ToMetersPerSecs)
                force.z = forwardDirectionOnLane * forwardAcc * 10.0f * dt;

            rb.AddForce(force, ForceMode.VelocityChange);
        }

        Vector3 rotTan;
		if (isBonus)
        {
            rotTan = new Vector3(0.0f, 1.0f, 0.0f);
        }
        else
        {
            rotTan = new Vector3(rbVel.x * rotCoeff, 0.0f, forwardDirectionOnLane * 1.0f);
        }
        rb.MoveRotation(Quaternion.LookRotation(rotTan));

        //Backup side acceleration after collision from back
        if (backCollisionTime > 0.0f)
        {
            backCollisionTime -= dt;
            if (backCollisionTime<0.0f)
                sideAcc = backupSideAcc;
        }

        //Change lane ended
        if (changeLaneTime > 0.0f)
        {
            changeLaneTime -= dt;
            if (changeLaneTime < 0.0f)
            {
                EndChangeLane();
            }
        }
        
        //Changing lane logic update
        if (canChangeLane)
        {
            bool truckOnTheWay = spawnManager.TruckWillSpawnShortly();
            if (!OnTheRunTutorialManager.Instance.TutorialActive && !truckOnTheWay)
            {
                if (!laneAlreadyChanged)
                    ChangeLaneUpdate(dt);
                BlinkingArrowsUpdate(dt);
            }
        }

        if (OnTheRunTutorialManager.Instance.TutorialActive && !OnTheRunTutorialManager.Instance.IsTutorialOnScreen && !OnTheRunTutorialManager.Instance.TutorialStopped)
        {
            if (OnTheRunTutorialManager.Instance.IsCurrentTutorialCar(this))
            {
                float distanceFromPlayer = gameObject.transform.position.z - playerRef.transform.position.z;
                float distanceThreshold = 30.0f;
                if(OnTheRunTutorialManager.Instance.NextTutorial == OnTheRunTutorialManager.TutorialType.SingleSlipstream)
                    distanceThreshold = OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION ? 26.1f : 14.5f;
                else if (OnTheRunTutorialManager.Instance.NextTutorial == OnTheRunTutorialManager.TutorialType.Trucks)
                    distanceThreshold = 50.0f;

                if (distanceFromPlayer < distanceThreshold)
                    OnTheRunTutorialManager.Instance.ShowTutorial();
            }
        }
    }

    #region Change Lane
    void ChangeLaneUpdate( float dt )
    {
        if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");
        float distanceFromPlayer = gameObject.transform.position.z - playerRef.transform.position.z;
        if ((distanceFromPlayer < changeLaneDistance && spawnManager.IsTruckOnScreen == 0) || forceChangeLane)
        {
            if (changeLaneArrowTimer >= 0.0f)
            {
                if (!laneChoosen)
                {
                    //choose a target lane
                    bool lookFirstDx = UnityEngine.Random.Range(0, 100) > 50;
                    if ((lookFirstDx || currentLane == 0) && currentLane!=4)
                    {
                        CheckLaneBySide(/*"right"*/true, out finalTargetLane); //look dx------------
                        if (finalTargetLane < 0 && currentLane!=0)
                            CheckLaneBySide(/*"left"*/false, out finalTargetLane); //look sx------------
                    }
                    else
                    {
                        CheckLaneBySide(/*"left"*/false, out finalTargetLane); //look sx------------
                        if (finalTargetLane < 0 && currentLane != 4)
                            CheckLaneBySide(/*"right"*/true, out finalTargetLane); //look dx------------
                    }
                
                    laneChoosen = true;
                    if (!forceChangeLane)
                        LevelRoot.Instance.BroadcastMessage("OnPauseChangeLaneUpdate");

                    if (finalTargetLane >= 0)
                    {
                        beginArrowBlink = true;
                        blinkArrowTimer = blinkArrowTime;
                        blinkArrowSide = currentLane > finalTargetLane ? -1:1;
                    }
                    else if (forceChangeLane)
                    {
                        beginArrowBlink = true;
                        blinkArrowTimer = blinkArrowTime;
                        finalTargetLane = (UnityEngine.Random.value * 100000.0f) % 100.0f > 50.0f ? currentLane-1 : currentLane+1;
                        if (finalTargetLane < 0) finalTargetLane = 1;
                        else if (finalTargetLane > 4) finalTargetLane = 3;
                        blinkArrowSide = currentLane > finalTargetLane ? -1 : 1;
                    }
                }

                changeLaneArrowTimer -= dt;

                if (changeLaneArrowTimer < 0.0f)
                {
                    //Start change lane movement
                    beginMovement = true;
                    if (finalTargetLane >= 0)
                    {
                        forceChangeLane = false;
                        changeLaneDistance = UnityEngine.Random.Range(minChangeLaneDistance, maxChangeLaneDistance);
                        laneAlreadyChanged = true;
                        changeLaneTime = Math.Abs(currentLane - finalTargetLane);
                        currentLane = finalTargetLane;
                        changeLaneArrowTimer = changeLaneTime;
                        sideAcc = 80.0f;
                    }
                }
            }
        }
    }

    void BlinkingArrowsUpdate(float dt)
    {
        if (blinkArrowTimer >= 0.0f && beginArrowBlink)
        {
            blinkArrowTimer -= dt;
            if (blinkArrowTimer < 0.0f)
            {
                blinkArrowTimer = blinkArrowTime;
                if (blinkArrowSide<0)
                    arrowLeft.SetActive(!arrowLeft.activeInHierarchy);
                else
                    arrowRight.SetActive(!arrowRight.activeInHierarchy);
            }
        } 
    }

    //void CheckLaneBySide(string side, out int finalTargetLane)
	void CheckLaneBySide(bool rightLane, out int finalTargetLane)
	{
        finalTargetLane = -1;
        int maxLaneDelta = UnityEngine.Random.Range(1, 5);

		if (rightLane)//side.Equals("right"))
        {
            int endLaneCheck = Mathf.Min(currentLane + maxLaneDelta, 4);
            for (int i = currentLane; i < endLaneCheck; ++i)
            {
                int targetLane = i + 1;
                if (spawnManager.LaneIsFree(targetLane))//targetLane != spawnManager.TruckLane)
                {
                    bool isLaneEmpty = CheckLaneByIndex(targetLane);
                    if (!isLaneEmpty)
                        break;
                    else
                        finalTargetLane = targetLane;
                }
                else
                    break;
            }
        }
        else
        {
            int endLaneCheck = Mathf.Max(currentLane - maxLaneDelta, 0);
            for (int i = currentLane; i > endLaneCheck; --i)
            {
                int targetLane = i - 1;
                if (spawnManager.LaneIsFree(targetLane))//targetLane != spawnManager.TruckLane)
                {
                    bool isLaneEmpty = CheckLaneByIndex(targetLane);
                    if (!isLaneEmpty)
                        break;
                    else
                        finalTargetLane = targetLane;
                }
                else
                    break;
            }
        }
    }

    bool CheckLaneByIndex(int index)
    {
        //check if a lane is empty
        float forwardDistance = 6.0f;
        RaycastHit hit;
        Vector3 startPos = gameObject.transform.position + Vector3.up * 1.0f + Vector3.right * (index - currentLane);
        Vector3 endPos = startPos + Vector3.forward * forwardDistance;
        Physics.Raycast(startPos, endPos, out hit, 50.0f, ~ignoreLayers);
        if (!hit.collider)
        {
            //Empty lane
            return true;
        }

        return false;
    }

    void OnPauseChangeLaneUpdate()
    {
        if (!laneChoosen)
        {
            ResetChangeLane();
            canChangeLane = false;
        }
    }

    void EndChangeLane()
    {
        beginMovement = false;
        beginArrowBlink = false;
        sideAcc = backupSideAcc;
        arrowLeft.SetActive(false);
        arrowRight.SetActive(false);
    }

    void ResetChangeLane( )
    {
        canChangeLane = true;
        finalTargetLane = -1;
        laneChoosen = false;
        if (arrowLeft != null)
        {
            arrowLeft.SetActive(false);
            arrowRight.SetActive(false);
        }
        beginArrowBlink = false;
        blinkArrowTimer = -1.0f;

        //temp
        if (arrowLeft != null)
        {
            arrowLeft.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            arrowLeft.GetComponent<Renderer>().material.color = Color.yellow;
            arrowRight.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            arrowRight.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
    #endregion

    void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;
        
        bool lateralCollision = Math.Abs(collision.contacts[0].normal.x) >= 0.9f ? true : false;
        if (collision.collider.GetComponent<PoliceBehaviour>()!=null && IsTruck && lateralCollision)
        {
            collision.collider.GetComponent<OpponentKinematics>().SendMessage("OnTruckLateralcollision", SendMessageOptions.DontRequireReceiver);
            return;
        }

        bool collidedWithTruck = false;
        if (collision.collider.GetComponent<TruckBehaviour>()!=null)
            collidedWithTruck = true;
        
        //if (OnTheRunTutorialManager.Instance.TutorialActive && !isTruck && collidedWithTruck)
        if (!isTruck && collidedWithTruck && !lateralCollision)
        {
            CarExplode(40.0f, transform.position + transform.forward);
            return;
        }

        if (wandering)
        {
            CarExplode(40.0f, transform.position + transform.forward);
            return;
        }

        bool collidedWithMine = collision.collider.gameObject.name.Contains("mine");
        if (collidedWithMine) return;

        bool collidedWithPlayer = collision.collider.gameObject.tag.Equals("Player");


        if (lastCollisionTime != -1.0f && !collidedWithPlayer) //collision.collider.GetComponent<PlayerKinematics>() == null)
            return;

        if (collidedWithPlayer && collision.collider.GetComponent<PlayerKinematics>().IsJumping && !IsTruck)
        {
            GetComponent<Collider>().isTrigger = true;
            OnPlayerEndJump( 0.0f );
            return;
        }

        float now = TimeManager.Instance.MasterSource.TotalTime;

        //added lateralCollision and OnTheRunGameplay.oldSystemActive
        bool alwaysExplosion = false;
        bool extremeLanes = currentLane == 0 || currentLane == 4;

        //Player has a shield????
        /*if (collidedWithPlayer && !lateralCollision && collision.collider.GetComponent<PlayerKinematics>().ShieldOn)
        {
            OnPlayerWithShieldCollison(collision.collider);
            return;
        }*/

        //Reset collidedWithPlayer variable after a while
        if (collidedWithPlayer && now - lastLateralCollisionTime > 2.0f)
            collidedWithPlayer = false;

        //Avoid wrong explosion while in extremeLanes and collision form back
        if (now - lastLateralCollisionTime < 0.1f && backCollisionTime > 0.0f && extremeLanes) return;

        if (collision.collider.gameObject.tag.Equals("Player"))
        {
            if (!lateralCollision)
            {
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.BumpOnTraffic);
                PlayerKinematics playerKinematics = collision.collider.GetComponent<PlayerKinematics>();
                if (gameplayManager.IsInWrongDirection(CurrentLane) && !playerKinematics.PlayerIsDead)
                    LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
            }
            else
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.MoveAwayTraffic);
        }

        OpponentKinematics oppKin = collision.collider.GetComponent<OpponentKinematics>();
        bool collisionWithBadGuy = oppKin != null && oppKin.IsBadGuy;
        if (collisionWithBadGuy && !isTruck && !IsBadGuy)
        {
            CarExplode(40.0f, transform.position + transform.forward);
            return;
        }

        if (IsTruck)
        {
            if (lateralCollision)
            {
                if(collision.collider.GetComponent<PlayerKinematics>()!=null)
                    collision.collider.GetComponent<PlayerKinematics>().SendMessage("OnTruckLateralcollision", SendMessageOptions.DontRequireReceiver);
            }
            else
                return;
        }
        else if ((!IsBadGuy && (OnTheRunGameplay.oldSystemActive || lateralCollision) && backCollisionTime < 1.3f) ) //|| (lateralCollision && extremeLanes && backCollisionTime < 1.3f))
        {
            backCollisionTime = -1.0f;
            lastCollisionTime = now;
            sideAcc = backupSideAcc;

            bool isCollisionWithPlayer = collidedWithPlayer;
            bool explodeForDoubleCollisionCheck = !isCollisionWithPlayer &&
                                                  collidedWithPlayer &&
                                                  lastCollisionTime - OpponentKinematics.lateralCollisionTime < 0.8f;

            if (extremeLanes || alwaysExplosion || explodeForDoubleCollisionCheck || collisionWithBadGuy)
            {
                bool isGoingOnExtremeLane = isChangingLaneCollision && extremeLanes && !explodeForDoubleCollisionCheck;
                if (!isGoingOnExtremeLane)
                {
                    if (collision.collider.gameObject.tag.Equals("Player"))//isCollisionWithPlayer)
                    {
                        LevelRoot.Instance.BroadcastMessage("OnDestroyspecificCar", vehicleId);
                        LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
                    }
                    Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashNormal);
                    OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
                    OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
                    CarExplode(40.0f, transform.position + transform.forward);
                }
            }
            else if (!isChangingLaneCollision)
            {
                lastLateralCollisionTime = now;
                bool collisionOnLeftSide = collision.contacts[0].normal.x > 0.0f;
                lastCollisionTime = -1.0f;

                //lateral impulse
                float impulseForce = collisionOnLeftSide ? 4000.0f : -4000.0f;
                rb.AddForce(Vector3.right * impulseForce, ForceMode.Impulse);

                OnChangeLane(collisionOnLeftSide);
                if (isCollisionWithPlayer)
                {
                    collidedWithPlayer = true;
                    OpponentKinematics.lateralCollisionTime = now;
                }
                else
                {
                    collidedWithPlayer = false;
                }
            }
        }

        if (!IsBadGuy && !IsTruck && !OnTheRunGameplay.oldSystemActive && !lateralCollision && backCollisionTime<0.0f)
        {
            if (gameplayManager.IsInWrongDirection(CurrentLane) && collision.collider.gameObject.tag.Equals("Player"))
            {
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashNormal);
                rb.velocity = Vector3.zero;
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
                CarExplode(40.0f, transform.position + transform.forward);
            }
            else
            {
                backCollisionTime = 1.5f;
                sideAcc = 20.0f;
                float impulseForce = UnityEngine.Random.Range(0, 100)>50 ? -2500.0f : 2500.0f;
                rb.AddForce(Vector3.right * impulseForce, ForceMode.Impulse);
            }
        }

        bool backCollision = collision.contacts[0].point.z < gameObject.transform.position.z ? true : false;
        if (OnTheRunObjectsPool.Instance != null)
        {
            Vector3 collOffset = Vector3.zero;
            Vector3 carSize = gameObject.GetComponent<BoxCollider>().size;
            if (lateralCollision)
            {
                Vector3 side = collision.contacts[0].normal.x < 0.0f ? Vector3.right : Vector3.left;
                collOffset = side * (carSize.x * 0.5f);
            }
            else if (backCollision)
            {
                collOffset = Vector3.back * (carSize.z * 0.6f);
            }

            if (now - lastFxCollisionTime > 0.5f && collOffset!=Vector3.zero && !exploded)
            {
                lastFxCollisionTime = now;
                OnTheRunObjectsPool.Instance.RequestEffectInPoint(collision.contacts[0].point, Vector3.zero, OnTheRunObjectsPool.ObjectType.Sparks);
                OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, collOffset, OnTheRunObjectsPool.ObjectType.Explosion, true);
                OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
                if(!IsBadGuy)
                    OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.zero, OnTheRunObjectsPool.ObjectType.Smoke, true);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameObject.GetComponent<BonusBehaviour>() != null)
            return;

        bool collideWithPlayer = other.gameObject.tag.Equals("Player");
        bool shieldActive = collidedWithPlayer ? other.gameObject.GetComponent<PlayerKinematics>().ShieldOn : false;
        if (collideWithPlayer || shieldActive)//(!isTruck || shieldActive))
        {
            if (gameplayManager.IsSpecialCarActive)
            {
                OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialWrongLane(gameplayManager.CurrentSpecialCar, CurrentLane);
                OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialVehicle(gameplayManager.CurrentSpecialCar);

                switch (gameplayManager.CurrentSpecialCar)
                {
                    //case OnTheRunGameplay.CarId.Tank: LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithTank); break;
                    //case OnTheRunGameplay.CarId.Bigfoot: LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithBigfoot); break;
                    //case OnTheRunGameplay.CarId.Firetruck: LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithFiretruck); break;
                    case OnTheRunGameplay.CarId.Ufo:
                        Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashSpecial);
                        transform.position += Vector3.back * 300.0f;
                        other.gameObject.GetComponent<PlayerKinematics>().gameObject.BroadcastMessage("OnExitFromUfo", gameObject);
                        //LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithUfo);
                        return;
                        break;
                    case OnTheRunGameplay.CarId.Plane:
                        //LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithPlane);
                        return;
                        break;
                }
            }
            
            if (gameplayManager.IsTurboActive)
            {
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsInTurbo);
                OnTheRunSingleRunMissions.Instance.OnDestroyCarsTurboWrongLane(OnTheRunSingleRunMissions.MissionType.DestroyCarsInTurboWrongLane, CurrentLane);
            }


            if (gameplayManager.IsSpecialCarActive)
            {
                //Manager<UIManager>.Get().ActivePage.SendMessage("OnScoreForCollision", rigidbody.position);
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashSpecial);
            }
            else
            {
                //Manager<UIManager>.Get().ActivePage.SendMessage("OnScoreForCollision", rigidbody.position);
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashNormal);
            }

            //Debug.Log("OnTriggerEnter: " + other.gameObject.tag + " " + collideWithPlayer + " " + other.gameObject.GetComponent<PlayerKinematics>().ShieldOn);

            //Don't collide with the truck you are jumping from
            bool isUsedTruck = false;
            if (IsTruck)
                isUsedTruck = (other.gameObject.GetComponent<PlayerKinematics>().IsLifting || other.gameObject.GetComponent<PlayerKinematics>().IsJumping);
            if (isUsedTruck)
                return;

            if (lastCollisionTime != -1.0f)
                return;

            lastCollisionTime = TimeManager.Instance.MasterSource.TotalTime;
            float deltaX = rb.position.x - other.GetComponent<Rigidbody>().position.x;
            /*float signVx = Mathf.Sign(deltaX);
            if (Mathf.Abs(deltaX) < 0.75f)
            {
                if (Mathf.Abs(rigidbody.position.x) < 0.75f)
                    signVx = Mathf.Sign(UnityEngine.Random.RandomRange(-1.0f, 1.0f));
                else
                    signVx = Mathf.Sign(rigidbody.position.x);
            }*/
            rb.velocity = new Vector3(Mathf.Sign(deltaX) * UnityEngine.Random.Range(12.0f, 25.0f), 0.0f, other.GetComponent<Rigidbody>().velocity.z * 0.94f);

            LevelRoot.Instance.BroadcastMessage("OnDestroyspecificCar", vehicleId);
            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);

            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
            CarExplode(40.0f, (rb.position + other.GetComponent<Rigidbody>().position) * 0.5f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        bool collideWithPlayer = other.gameObject.tag.Equals("Player");
        if (collideWithPlayer && other.gameObject.GetComponent<PlayerKinematics>().ShieldOn)
        {
            if (!gameplayManager.IsSpecialCarActive)
                other.gameObject.SendMessage("OnShieldActive", false);
        }
    }

    void OnPlayerWithShieldCollison(Collider other)
    {
        OnTriggerEnter(other);
        if (!gameplayManager.IsSpecialCarActive)
            other.gameObject.SendMessage("OnShieldActive", false);
    }

    void OnOpponentDestroyed( )
    {
        if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");
        Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashSpecial);
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
        CarExplode(40.0f, transform.position + transform.forward);
        if(gameplayManager.CurrentSpecialCar==OnTheRunGameplay.CarId.Tank)
            OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.up * 0.1f, OnTheRunObjectsPool.ObjectType.AsphaltCrack, false);
        OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialWrongLane(gameplayManager.CurrentSpecialCar, CurrentLane);
        OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialVehicle(gameplayManager.CurrentSpecialCar);
        LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
    }

    void OnFiretruckHit()
    {
        if (!wandering)
        {
            float lateralForce = 20.0f;
            float forwardForce = 60.0f;
            Vector3 impulseVector;
            if (currentLane < 2)
                impulseVector = Vector3.left * lateralForce;
            else if (currentLane > 2)
                impulseVector = Vector3.right * lateralForce;
            else
                impulseVector = UnityEngine.Random.Range(0, 100) > 50 ? Vector3.right * lateralForce : Vector3.left * lateralForce;

            rb.velocity = Vector3.zero;
            rb.AddForceAtPosition(impulseVector, Vector3.zero, ForceMode.Impulse);
            rb.AddForce(Vector3.forward * forwardForce + impulseVector, ForceMode.VelocityChange);
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            wandering = true;
            OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialWrongLane(OnTheRunGameplay.CarId.Firetruck, CurrentLane);
            OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialVehicle(OnTheRunGameplay.CarId.Firetruck);
            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);
        }
    }

    protected void CarExplode(float explosionForce, Vector3 explosionPosition)
    {
        explosionForce = 30.0f;
        rb.AddExplosionForce(explosionForce, explosionPosition, 2.0f, 3.0f, ForceMode.VelocityChange);
        float forwardForce = 60.0f;
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.VelocityChange);
        rb.constraints = RigidbodyConstraints.None;
        gameObject.BroadcastMessage("ActivateShadow", false, SendMessageOptions.DontRequireReceiver);
        exploded = true;

        if (OnTheRunObjectsPool.Instance != null)
        {
            OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
            OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.up, OnTheRunObjectsPool.ObjectType.CarExplosion, true);
        }
        GetComponent<Collider>().enabled = false;
    }

    void SetupLights( )
    {
        OnActivateLights(false);
        if (gameplayManager.GetComponent<OnTheRunEnvironment>().currentEnvironment == OnTheRunEnvironment.Environments.Asia)
        {
            gameObject.SendMessage("OnActivateLights", true);
        }
    }

    void OnActivateLights(bool active)
    {
        if (tunnelLights != null)
        {
            foreach (GameObject light in tunnelLights)
            {
                light.SetActive(active);
            }
        }
    }
    #endregion

    #region Messages
    void OnTruckLateralcollision()
    {
        if (vehicleId == OpponentId.TrafficPolice)
        {
            currentLane = prevLane;
            gameObject.GetComponent<SeekForPlayerBehaviour>().DeactivateTime = 1.0f;
        }
        else
        {
            if (currentLane != prevLane)
                currentLane = prevLane;
            else if (currentLane >= 2)
                currentLane--;
            else
                currentLane++;
        }
    }

    void OnTrappedByUfo()
    {
        exploded = true;
        GetComponent<Collider>().isTrigger = true;
        rb.rotation = Quaternion.LookRotation(new Vector3(0.0f, 0.0f, 1.0f));
        rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false;
    }

    void ChangeLaneForRoadWorks()
    {
        if (!IsTruck)
        {
            forceChangeLane = true;
            canChangeLane = true;
            laneAlreadyChanged = false;
            changeLaneArrowTimer = (UnityEngine.Random.value * 100000.0f) % arrowChangeLaneTime;
            laneChoosen = false;
        }
    }

    public void SetupBadGuyParameters(float _forwardAcc, float _maxspeed)
    {
        forwardAcc = _forwardAcc;
        maxSpeed = _maxspeed;
        currentMaxSpeed = maxSpeed;
    }

    void OnTankShotAreaEnter()
    {
        insideTankShotArea = true;
    }

    void OnTankShotAreaExit()
    {
        insideTankShotArea = false;
    }

    void OnTankShot(bool rightDirection)
    {
        bool checkSide = rightDirection ? currentLane > 2 : currentLane < 2;
        if (insideTankShotArea && checkSide)
        {
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(rb.position, coinsForCrashSpecial);
            OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, Vector3.up * 0.1f, OnTheRunObjectsPool.ObjectType.AsphaltCrack, false);
            /*OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY);
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM);*/
            CarExplode(40.0f, transform.position + transform.forward);
            /*OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialWrongLane(OnTheRunGameplay.CarId.Tank, CurrentLane);
            OnTheRunSingleRunMissions.Instance.OnDestroyCarsSpecialVehicle(OnTheRunGameplay.CarId.Tank);
            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);*/
        }
    }

    void OnChangeLaneBadGuy(int deltaLane=0)
    {
        isChangingLane = true;

        if (currentLane == 4) 
            currentLane = 3;
        else if (currentLane == 0) 
            currentLane = 1;
        else
        {
            int delta = deltaLane;
            if (deltaLane == 0) 
                delta = UnityEngine.Random.Range(0, 100) > 50 ? 1 : -1;
            if (spawnManager.LaneIsFree(currentLane + delta))
            {
                currentLane += delta;
            }
            else if (spawnManager.LaneIsFree(currentLane - delta))
            {
                currentLane -= delta;
            }
        }
    }

    void OnChangeLane(bool collDx)
    {
        isChangingLaneCollision = true;
        if (collDx)
            ++currentLane;
        else
            --currentLane;
    }

    void OnExitGame()
    {
    }

    void OnReset( )
    {
        gameObject.transform.localScale = Vector3.one;

        //Choose direction
        forwardDirectionOnLane = 1;
        if (PoliceBehaviour == null)
        {
            if (gameplayManager.IsInWrongDirection(currentLane)) //(environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.ForwardBackward || environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward) && currentLane > 2)
            {
                forwardDirectionOnLane = -1;
                currentMaxSpeed = maxSpeedBackwards;
            }
            else
                currentMaxSpeed = maxSpeed;
        }

        isDead = false;
        exploded = false;
        wandering = false;
        prevLane = currentLane;
        rb.position = new Vector3(17.5f * 0.5f * OnTheRunEnvironment.lanes[currentLane], rb.position.y, rb.position.z + currentMaxSpeed * VehicleUtils.ToMetersPerSecs * Time.fixedDeltaTime);
        rb.velocity = new Vector3(0.0f, 0.0f, forwardDirectionOnLane * currentMaxSpeed * VehicleUtils.ToMetersPerSecs);
        collidedWithPlayer = false;
        lastCollisionTime = -1.0f;
        backCollisionTime = -1.0f;
        GetComponent<Collider>().isTrigger = false;
        GetComponent<Collider>().enabled = true;
        rb.useGravity = false;
        minPlayerDistance = float.MaxValue;

        ResetChangeLane();

        //The vehicle change lane only sometime
        laneAlreadyChanged = true;
        canChangeLane = false;
        if (UnityEngine.Random.Range(0, 100) > 80 && !IsTruck && environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.AllForward && PoliceBehaviour==null)
        {
            changeLaneDistance = UnityEngine.Random.Range(minChangeLaneDistance, maxChangeLaneDistance);
            changeLaneArrowTimer = arrowChangeLaneTime;
            blinkArrowTimer = blinkArrowTime;
            laneAlreadyChanged = false;
            canChangeLane = true;
        }
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        gameObject.BroadcastMessage("ActivateShadow", true, SendMessageOptions.DontRequireReceiver);
        SetupLights();
    }

    void OnResetVehicle()
    {
        if (PoliceBehaviour != null)
            PoliceBehaviour.SendMessage("ResetBadGuy");

        OnReset();
        OnStartGame();
    }

    void OnDead()
    {
        isDead = true;
    }

    void OnStartGame()
    {
        lastCollisionTime = -1.0f;
        backCollisionTime = -1.0f;
        rb.position = startPosition;
        rb.rotation = Quaternion.LookRotation(new Vector3(0.0f, 0.0f, 1.0f));
        rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }
    #endregion
}
