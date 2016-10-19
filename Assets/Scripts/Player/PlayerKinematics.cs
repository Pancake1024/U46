using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections;

[AddComponentMenu("Player/Kinematics")]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerKinematics : MonoBehaviour
{
    #region Protected members
    private bool firstFrame = true;
    protected OnTheRunEnvironment environmentManager;
    protected NeverendingPlayer playerData;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunSpawnManager spawnManager;
    protected GameObject cameraRef;
    protected UISharedData uiSharedData;
    protected UIIngamePage ingamePage;

    protected bool inputLeft = false;
    protected bool inputRight = false;
    protected bool isStopping = false;
    protected bool isDead = false;
    protected bool isRunning = false;// ALEX
    protected float lastCollisionTime = -1.0f;
    protected float avoidFrequentCollisionTime = -1.0f;
    protected int prevLane;
    protected Vector3 startPosition;
    protected float distance = 0.0f;
    protected bool gameplayStarted = false;
    protected Vector3 startDistance = new Vector3(0.0f, 0.0f, 0.0f);
    protected float maxSpeed = 0.0f;
    protected float maxBonusSpeed = 0.0f;
    protected bool backupShieldOn = false;
    protected float backupPlayerMass = 100.0f;
    protected bool shieldOn = false;
    protected bool turboOn = false;
    protected bool slipstreamOn = false;
    protected bool draftOn = false;
    protected float turboRatio = 0.0f;
    protected bool goOneLaneDx = false;
    protected bool goOneLaneSx = false;
    protected bool isOnLane = true;
    protected int isChangingLane = -1;
    protected bool wrongDirection = false;
    protected float turboSpeed = 420.0f;
    protected float playerOldSpeed = 0.0f;
    protected Vector3 prevVel;

    //jump
    protected TruckBehaviour truckInUse = null;
    protected bool isLifting = false;
    protected bool isJumping = false;
    protected bool isFirstBounce = true;
    protected bool isDescendingParabola = false;
    protected float liftAngle = 0.0f;
    protected float radiusExplosionEndJump = 10.0f;
    protected float jumpDistance = 7.0f;
    protected float jumpDistanceBounce = 5.0f;
    protected float firingAngle = 45.0f;
    protected float gravityBase = 9.8f;
    protected float currentGravity = 9.8f;
    protected float gravityMultiplier = 3.5f;
    protected float flightDuration;
    protected float playerStartPositionY;
    protected Vector3 jumpTarget;
    protected float elapse_time = 0;
    protected float Vz;
    protected float Vy;
    protected TruckBehaviour.TruckType truckOverType = TruckBehaviour.TruckType.None;
    protected TruckBehaviour.TrasformType transformInto = TruckBehaviour.TrasformType.None;
    protected int currentLane;
    protected float timeLostForHit = -1.0f;
    protected bool startInTurbo = false;
    protected float collisionPosition = 0.0f;
    protected float backwardDistance = 0.0f;
    protected PlayerDraft playerDraft;
    protected bool isOnAir = false;
    protected bool forceJumpDescendingParabola = false;
    protected int isOnCentralMud = 0;
    protected float backupMaxSpeed = 0;

    protected float tempTimer = 0.0f;
    protected float dirtTimer = -1.0f;
    protected float yMaxHeightFactor = 0.065f;
    protected float verticalHeightFrequency = 55.0f;
    protected float yMaxRotationFactor = 1.0f;
    protected float verticalRotationFrequency = 30.0f;

    protected float keepSpeedTimer = -1.0f;

    protected float collisionProtectionTimer = -1.0f;

    protected Rigidbody rb;
    #endregion

    #region Public members
    public bool analogicInput;
    public OnTheRunGameplay.CarId carId;
    public int forceOnLane = -1;
    public float forwardAccMin = 15.0f;
    public float forwardAccMax = 25.0f;
    public float maxSpeedMin = 250.0f;
    public float maxSpeedMax = 350.0f;
    public float resistanceMin = 0.5f;
    public float resistanceMax = 0.15f;
    public float turboSpeedMin = 0.5f;
    public float turboSpeedMax = 0.15f;

    public float forwardAcc = 15.0f;
    public float forwardAccTurbo = 75.0f;
    public float backwardAcc = -75.0f;
    public float initialMaxSpeed = 250.0f;
    public float maxSpeedInc = 0.0f;
    public float limitMaxSpeed = 420.0f;
    public float grip = 16.0f;
    public float sideAcc = 180.0f;
    public float sideAcc2 = 40.0f;
    public float planeSideAcc = 180.0f;
    public float planeSideAcc2 = 40.0f;
    public float laneDelta = 0.5f;
    public float laneDelta2 = 0.10f;
    public float rotCoeff = 0.01f;
    public float minSideAccRatio = 1.5f;
    public float maxSideAccRatio = 1.5f;
    public float startSideAccLane = 400.0f;
    public float endSideAccLane = 4800.0f;

    public class PhysicParameters
    {
        public float[] acceleration;
        public float[] maxSpeed;
        public float[] resistance;
        public float[] turboSpeed;
        public float sideAcceleration;
        public float limitMaxSpeed;

        public PhysicParameters(float[] _acceleration, float[] _maxSpeed, float[] _resistance, float[] _turboSpeed, float _sideAcceleration, float _limitMaxSpeed)
        {
            acceleration = _acceleration;
            maxSpeed = _maxSpeed;
            resistance = _resistance;
            turboSpeed = _turboSpeed;
            sideAcceleration = _sideAcceleration;
            limitMaxSpeed = _limitMaxSpeed;
        }
    }

    enum PlayerDataType
    {
        Acceleration,
        MaxSpeed,
        Resistance,
        TurboSpeed
    }

    public bool IsOnCollisionEffect
    {
        get { return timeLostForHit >= 0.0f; }
    }

    public bool ForceOneLaneDx
    {
        set { goOneLaneDx = value; }
    }

    public bool ForceOneLaneSx
    {
        set { goOneLaneSx = value; }
    }

    public float KeepSpeedTimer
    {
        set { keepSpeedTimer = value; }
    }
    public bool IsRunnig
    {
        get { return isRunning; }
    }

    public bool IsOnAir
    {
        get { return isOnAir; }
        set { isOnAir = value; }
    }

    public bool IsOnDirt
    {
        get { return dirtTimer >= 0.0f; }
    }

    public bool WrongDirection
    {
        get
        {
            return wrongDirection;
        }
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

    public float ForwardAcceleration
    {
        get { return forwardAcc; }
    }

    public float Distance
    {
        get { return distance; }
    }

    public Vector3 StartDistance
    {
        get { return startDistance; }
    }

    public SBSVector3 TokenTangent
    {
        get
        {
            return playerData.TokenTangent;
        }
    }

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
        set
        {
            isDead = value;
        }
    }

    public bool PlayerIsStopping
    {
        get
        {
            return isStopping;
        }
        set
        {
            isStopping = value;
        }
    }

    public float PlayerOldSpeed
    {
        set { playerOldSpeed = value; }
    }

    public bool StatInTurbo
    {
        set
        {
            startInTurbo = value;
        }
    }

    public float LastCollisionTime
    {
        get
        {
            return lastCollisionTime;
        }
    }

    public float Speed
    {
        get
        {
            return rb.velocity.z;
        }
    }

    public bool TurboOn
    {
        get
        {
            return turboOn;
        }
    }

    public bool ShieldOn
    {
        get
        {
            return shieldOn;
        }
    }
    public bool SlipstreamOn
    {
        get
        {
            return slipstreamOn;
        }
    }

    public bool DraftOn
    {
        get
        {
            return draftOn;
        }
        set
        {
            draftOn = value;
        }
    }

    public float CurrentSpeed
    {
        get
        {
            return maxSpeed;
        }
    }

    public bool IsOnLane
    {
        get
        {
            return isOnLane;
        }
    }


    public bool IsLifting
    {
        get
        {
            return isLifting;
        }
    }

    public bool IsJumping
    {
        get
        {
            return isJumping;
        }
    }

    public int PrevLane
    {
        get
        {
            return prevLane;
        }
    }

    public TruckBehaviour TruckTaken
    {
        get { return truckInUse; }
    }


    public TruckBehaviour.TrasformType TransformInto
    {
        get { return transformInto; }
    }

    public Rigidbody PlayerRigidbody
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            return rb;
        }
    }

    #endregion

    #region Protected Properties

    protected OnTheRunGameplay GameplayManager
    {
        get
        {
            if (gameplayManager == null)
                gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

            return gameplayManager;
        }
    }

    #endregion

    #region protected methods
    protected void SetTurboFxSize(float size)
    {
        /*ParticleSystem ps = (ParticleSystem)turboFxRef.transform.GetChild(0).GetComponent<ParticleSystem>();
        ps.startSpeed = size * 1.0f;
        ps.startSize = size * 2.5f;

        ps = (ParticleSystem)turboFxSmokeRef.transform.GetChild(0).GetComponent<ParticleSystem>();
        ps.startSpeed = size * 20.0f;
        ps.startSize = size * 4.0f;

        ps = (ParticleSystem)turboFxSmokeRef.transform.GetChild(1).GetComponent<ParticleSystem>();
        ps.startSpeed = size * 40.0f;
        ps.startSize = size * 4.0f;*/
    }
    #endregion

    void DebugPrintCarParameters()
    {
        PlayerGameData gameData = gameObject.GetComponent<PlayerGameData>();
        Debug.Log("------------------ " + carId);
        string total = "";
        total += "+ acceleration - min: " + GetDataFromPlayerDataIndex(PlayerDataType.Acceleration, gameData.minAcceleration) + " max: " + GetDataFromPlayerDataIndex(PlayerDataType.Acceleration, gameData.maxAcceleration) + "\n";
        total += "+ speed - min: " + GetDataFromPlayerDataIndex(PlayerDataType.MaxSpeed, gameData.minMaxSpeed) + " max: " + GetDataFromPlayerDataIndex(PlayerDataType.MaxSpeed, gameData.maxMaxSpeed) + "\n";
        total += "+ resistance - min: " + GetDataFromPlayerDataIndex(PlayerDataType.Resistance, gameData.minResistance) + " max: " + GetDataFromPlayerDataIndex(PlayerDataType.Resistance, gameData.maxResistance) + "\n";
        total += "+ turbo speed - min: " + GetDataFromPlayerDataIndex(PlayerDataType.TurboSpeed, gameData.minTurboSpeed) + " max: " + GetDataFromPlayerDataIndex(PlayerDataType.TurboSpeed, gameData.maxTurboSpeed) + "\n";
        Debug.Log(total);
    }

    #region Unity callbacks
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerDraft = gameObject.GetComponent<PlayerDraft>();

        startPosition = rb.position;
    }

    void Init()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = GameplayManager.GetComponent<OnTheRunEnvironment>();
        spawnManager = GameplayManager.GetComponent<OnTheRunSpawnManager>();
        cameraRef = GameObject.FindGameObjectWithTag("MainCamera");


        string type = GameplayManager.IsSpecialCar(carId) ? "special_vehicles" : "player";
        PhysicParameters data = OnTheRunDataLoader.Instance.GetPlayerPhysicData(type);
        forwardAccMin = data.acceleration[0];
        forwardAccMax = data.acceleration[1];
        maxSpeedMin = data.maxSpeed[0];
        maxSpeedMax = data.maxSpeed[1];
        resistanceMin = data.resistance[0];
        resistanceMax = data.resistance[1];
        turboSpeedMin = data.turboSpeed[0];
        turboSpeedMax = data.turboSpeed[1];
        sideAcc = data.sideAcceleration;
        limitMaxSpeed = data.limitMaxSpeed;
    }

    void Start()
    {
        Init();

        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();

        PlayerGameData gameData = gameObject.GetComponent<PlayerGameData>();
        if (gameData != null)
        {
            PlayerPersistentData.Instance.GetPlayerData(carId).acceleration = Mathf.Max(PlayerPersistentData.Instance.GetPlayerData(carId).acceleration, gameData.minAcceleration);
            PlayerPersistentData.Instance.GetPlayerData(carId).maxSpeed = Mathf.Max(PlayerPersistentData.Instance.GetPlayerData(carId).maxSpeed, gameData.minMaxSpeed);
            PlayerPersistentData.Instance.GetPlayerData(carId).resistance = Mathf.Max(PlayerPersistentData.Instance.GetPlayerData(carId).resistance, gameData.minResistance);
            PlayerPersistentData.Instance.GetPlayerData(carId).turboSpeed = Mathf.Max(PlayerPersistentData.Instance.GetPlayerData(carId).turboSpeed, gameData.minTurboSpeed);

            forwardAcc = GetDataFromPlayerDataIndex(PlayerDataType.Acceleration, PlayerPersistentData.Instance.GetPlayerData(carId).acceleration);
            initialMaxSpeed = GetDataFromPlayerDataIndex(PlayerDataType.MaxSpeed, PlayerPersistentData.Instance.GetPlayerData(carId).maxSpeed);
            turboSpeed = GetDataFromPlayerDataIndex(PlayerDataType.TurboSpeed, PlayerPersistentData.Instance.GetPlayerData(carId).turboSpeed);
        }
        else
        {
            forwardAcc = forwardAccMin;
            initialMaxSpeed = maxSpeedMin;
            turboSpeed = turboSpeedMin;
        }

        maxSpeed = initialMaxSpeed;

        distance = 0.0f;
        playerData = GetComponent<NeverendingPlayer>();
        prevLane = currentLane;
        backupPlayerMass = 100.0f;

        if (isStopping)
        {
            forwardAcc = -Mathf.Abs(forwardAcc);
            rb.velocity = Vector3.zero + Vector3.forward * playerOldSpeed;
            playerOldSpeed = 0.0f;
        }

        if (GameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.None)
            uiSharedData.ResetScoreMultiplier();

        if (startInTurbo)
            GameplayManager.SendMessage("ActivateTurbo");
        startInTurbo = false;
        isOnAir = false;
    }

    float GetDataFromPlayerDataIndex(PlayerDataType type, int index)
    {
        float retValue = 0.0f,
              baseValue = 0.0f,
              deltaValue = 0.0f,
              maxSteps = 10.0f;

        switch (type)
        {
            case PlayerDataType.Acceleration:
                baseValue = forwardAccMin;
                deltaValue = forwardAccMax - forwardAccMin;
                break;
            case PlayerDataType.MaxSpeed:
                baseValue = maxSpeedMin;
                deltaValue = maxSpeedMax - maxSpeedMin;
                break;
            case PlayerDataType.Resistance:
                baseValue = resistanceMin;
                deltaValue = resistanceMax - resistanceMin;
                break;
            case PlayerDataType.TurboSpeed:
                baseValue = turboSpeedMin;
                deltaValue = turboSpeedMax - turboSpeedMin;
                break;
        }

        retValue = baseValue + (deltaValue / (maxSteps)) * index;

        return retValue;
    }

    void OnStartRunning()
    {
        forwardAcc = Mathf.Abs(forwardAcc);
        gameObject.SendMessage("ActivateTurboFx", false);
    }

    public void Reset(float initSpeed, Vector3 initRBspeed, Vector3 newStartDistance, int currLane)
    {
        startPosition = rb.position;
        playerData = GetComponent<NeverendingPlayer>();
        prevLane = currentLane;
        gameObject.SendMessage("ActivateTurboFx", false);
        gameObject.SendMessage("ActivateSlipstreamFx", false);
        playerDraft.SendMessage("ActivateDraftParticles", false);
        RaceStarted();
        startDistance = newStartDistance;
        maxSpeed = initSpeed;
        rb.velocity = initRBspeed;
        prevVel = rb.velocity;
        currentLane = currLane;
    }

    void Update()
    {
        if (firstFrame)
        {
            firstFrame = false;
            return;
        }

        //if (gameplayStarted)
        if (GameplayManager.Gameplaystarted || isStopping)
            distance = rb.position.z - startDistance.z;

        if (GameplayManager.IsInWrongDirection(CurrentLane))
        {
            if (!wrongDirection)
                uiSharedData.IncreaseWrongDirectionMultiplier();
            wrongDirection = true;
        }
        else
        {
            if (wrongDirection)
                uiSharedData.ResetWrongDirectionMultiplier();
            wrongDirection = false;
        }

        if (collisionProtectionTimer >= 0.0f)
        {
            collisionProtectionTimer -= TimeManager.Instance.MasterSource.DeltaTime;
            if (collisionProtectionTimer < 0.0f)
                OnCollisionProtectionActivation(false);
        }
    }

    void FixedUpdate()
    {
        // if (!isRunning) // ALEX
        //     return;

        if (isDead)
        {
            if (isJumping)
            {
                JumpUpdate(Time.fixedDeltaTime, TimeManager.Instance.MasterSource.TotalTime);
            }
            return;
        }

        if (isStopping && rb.velocity.z <= 20.0f * VehicleUtils.ToMetersPerSecs || GameplayManager.GameState != OnTheRunGameplay.GameplayStates.Racing)
            inputRight = inputLeft = false;

        if (OnTheRunTutorialManager.Instance.DeactivatePlayerInputs)
            inputRight = inputLeft = false;

        if (forceOnLane >= 0)
        {
            inputRight = inputLeft = false;
            currentLane = forceOnLane;
        }

#if UNITY_WEBPLAYER
        if (GameplayManager.Gameplaystarted)
        {
            bool checkLockedDx = inputRight && !Input.GetKey(KeyCode.RightArrow);
            bool checkLockedSx = inputLeft && !Input.GetKey(KeyCode.LeftArrow);
            if (checkLockedDx)
                inputRight = false;
            if (checkLockedSx)
                inputLeft = false;
        }
#endif

        float dt = Time.fixedDeltaTime,
              time = TimeManager.Instance.MasterSource.TotalTime,
              inputDirection = (inputRight ? 1.0f : 0.0f) + (inputLeft ? -1.0f : 0.0f);

        if (goOneLaneDx || goOneLaneSx)
        {
            inputDirection = (goOneLaneDx ? 1.0f : 0.0f) + (goOneLaneSx ? -1.0f : 0.0f);
            goOneLaneDx = goOneLaneSx = false;
        }

        if (isLifting)
        {
            LiftingUpdate(dt, time);
            inputDirection = 0.0f;
        }

        if (isJumping)
        {
            JumpUpdate(dt, time);
            //inputDirection = 0.0f;
        }

        if (playerDraft.SlipstreamFlyierCanBeShown)
        {
            if (inputDirection > 0)
            {
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartPlayerComboFlyier(Vector3.left);
                playerDraft.SlipstreamFlyierCanBeShown = false;
            }
            else if (inputDirection < 0)
            {
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartPlayerComboFlyier(Vector3.right);
                playerDraft.SlipstreamFlyierCanBeShown = false;
            }
        }

        if (lastCollisionTime != -1.0f)
            return;
        if (playerData.lastToken == null)
            return;

        //if (linearVelocity < maxLinearVelocity) && accelerationStarted && !isDead && isReached)

        float targetX = playerData.lastToken.width * 0.5f * OnTheRunEnvironment.lanes[currentLane],
              deltaToTarget = targetX - rb.position.x;

        Vector3 force = new Vector3(0.0f, 0.0f, 0.0f);

        if (analogicInput)
        {
            float lastLanePos = playerData.lastToken.width * 0.5f * OnTheRunEnvironment.lanes[4];
            float firstLanePos = playerData.lastToken.width * 0.5f * OnTheRunEnvironment.lanes[0];

            targetX = rb.position.x;
            if (inputDirection > 0)
                targetX = lastLanePos;
            else if (inputDirection < 0)
                targetX = firstLanePos;

            deltaToTarget = targetX - rb.position.x;

            if (deltaToTarget > laneDelta)
                force.x = planeSideAcc * minSideAccRatio * dt;
            else if (deltaToTarget < -laneDelta)
                force.x = -planeSideAcc * minSideAccRatio * dt;
            else if (deltaToTarget > laneDelta2)
                force.x = planeSideAcc2 * minSideAccRatio * dt;
            else if (deltaToTarget < -laneDelta2)
                force.x = -planeSideAcc2 * minSideAccRatio * dt;

            force.x -= rb.velocity.x * grip * dt;

            if (rb.position.x > lastLanePos)
                rb.position = new Vector3(lastLanePos, rb.position.y, rb.position.z);
            else if (rb.position.x < firstLanePos)
                rb.position = new Vector3(firstLanePos, rb.position.y, rb.position.z);
        }
        else
        {
            isOnLane = Mathf.Abs(deltaToTarget) < 0.5f; // 0.35f;//0.5f;// Mathf.Abs(rb.velocity.x * dt);
            int newLane = currentLane;
            if (inputDirection > 0.0f && currentLane < OnTheRunEnvironment.lanes.Length - 1)
                newLane++;
            else if (inputDirection < 0.0f && currentLane > 0)
                newLane--;

            if (isChangingLane < 0)
            {
                if (newLane != currentLane)
                    isChangingLane = currentLane;
            }
            else
            {
                if (newLane == currentLane)
                {
                    LevelRoot.Instance.BroadcastMessage("OnLaneChanged", Mathf.Abs(isChangingLane - currentLane));
                    isChangingLane = -1;
                }
            }

            if (isOnLane)
                prevLane = currentLane;

            if (currentLane == prevLane && isOnLane)
                currentLane = newLane;
            else if (newLane == prevLane)
                currentLane = newLane;

            float sideAccRatio = minSideAccRatio + Mathf.Clamp01((rb.position.z - startSideAccLane) / (endSideAccLane - startSideAccLane)) * (maxSideAccRatio - minSideAccRatio);

            if (deltaToTarget > laneDelta)
                force.x = sideAcc * sideAccRatio * dt;
            else if (deltaToTarget < -laneDelta)
                force.x = -sideAcc * sideAccRatio * dt;
            else if (deltaToTarget > laneDelta2)
                force.x = sideAcc2 * sideAccRatio * dt;
            else if (deltaToTarget < -laneDelta2)
                force.x = -sideAcc2 * sideAccRatio * dt;

            force.x -= rb.velocity.x * grip * dt;

            if (isJumping && isFirstBounce)
                force.x /= 10.0f;
        }

        //Debug.Log("r: " + sideAccRatio + " s: " + rb.velocity.z * VehicleUtils.ToKmh);
        float targetSpeed = maxSpeed * VehicleUtils.ToMetersPerSecs;
        if (turboOn && turboRatio >= 0.25f || slipstreamOn && turboRatio >= 0.25f)
            targetSpeed = turboSpeed * VehicleUtils.ToMetersPerSecs;
        else if (draftOn)
        {
            cameraRef.GetComponent<Camera>().transform.Translate(new Vector3(0.0f, 0.0f, dt * 400.0f));
            if (PlayerPersistentData.Instance.alternativeSlipstreamModeActive)
            {
                targetSpeed = (maxSpeed + playerDraft.draftDeltaSpeed) * VehicleUtils.ToMetersPerSecs;
            }
            else
            {
                if (OnTheRunDataLoader.ABTesting_Flag)
                    targetSpeed = (maxSpeed + playerDraft.currentDeltaSpeedAdvanced) * VehicleUtils.ToMetersPerSecs;
                else
                    targetSpeed = /*335.0f*/(maxSpeed + playerDraft.draftDeltaSpeed) * VehicleUtils.ToMetersPerSecs;
            }
        }

        //Debug.Log(" targetSpeed: " + targetSpeed + " - current delta speed: " + playerDraft.currentDeltaSpeedAdvanced);
        //Debug.Log(" s: " + rb.velocity.z * VehicleUtils.ToKmh);

        if (rb.velocity.z <= targetSpeed)
        {
            if (PlayerPersistentData.Instance.alternativeSlipstreamModeActive)
            {
                //Debug.Log("draftOn: " + draftOn);
                if (turboOn)
                {
                    force.z = forwardAccTurbo * dt;
                }
                else if (draftOn && !isStopping)
                {
                    if (OnTheRunDataLoader.ABTesting_Flag)
                        force.z = playerDraft.currentAccelerationAdvanced * dt;
                    else
                        force.z = playerDraft.draftAcceleration * dt;
                }
                else
                {
                    force.z = forwardAcc * dt;
                }
            }
            else
            {
                if (OnTheRunDataLoader.ABTesting_Flag && draftOn && !isStopping)
                    force.z = playerDraft.currentAccelerationAdvanced * dt;
                else
                    force.z = ((turboOn || slipstreamOn) ? forwardAccTurbo : forwardAcc) * dt;
            }
        }
        else if (rb.velocity.z >= targetSpeed + 10.0f)
            force.z = backwardAcc * dt;

        //if (OnTheRunDataLoader.ABTesting_Flag && draftOn)
        //    Debug.Log(" targetSpeed: " + targetSpeed + " " + playerDraft.currentDeltaSpeedAdvanced + " - force.z: " + force.z + " " + playerDraft.currentAccelerationAdvanced);

        //Helping hand save me-------
        if (keepSpeedTimer >= 0.0f)
        {
            keepSpeedTimer -= dt;
            if (rb.velocity.z <= maxSpeed * VehicleUtils.ToMetersPerSecs)
                force.z = forwardAccTurbo * dt;
            if (keepSpeedTimer < 0.0f)
                keepSpeedTimer = -1.0f;
        }
        //---------------------------


        if (timeLostForHit >= 0.0f)
        {
            timeLostForHit -= dt;
            force.z *= 0.1f;
        }

        rb.AddForce(force, ForceMode.VelocityChange);

        //Debug.Log(rb.velocity.z);

        maxSpeed = Mathf.Min(maxSpeed + maxSpeedInc * dt, limitMaxSpeed);

        if (!isLifting && !isJumping)
        {
            if (GameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Plane)
            {
                float planeRollRotationCoeff = rotCoeff * Mathf.Rad2Deg;
                float planePitchRotationCoeff = rotCoeff * 0.5f;
                Quaternion planeRotation = Quaternion.AngleAxis(-rb.velocity.x * planeRollRotationCoeff, rb.transform.forward);
                Vector3 rotTan = new Vector3(rb.velocity.x * planePitchRotationCoeff, 0.0f, 1.0f);
                planeRotation *= Quaternion.LookRotation(rotTan);
                rb.MoveRotation(planeRotation);
            }
            else
            {
                Vector3 rotTan = new Vector3(rb.velocity.x * rotCoeff, 0.0f, 1.0f);
                rb.MoveRotation(Quaternion.LookRotation(rotTan));
            }
        }

        if (isStopping && rb.velocity.z < 1.0f && !isDead)
        {
            OnTimeEndedForReal();
        }

        //asia collision
        float oldBackwardDistance = backwardDistance;
        backwardDistance = distance - collisionPosition;
        if (oldBackwardDistance - backwardDistance > 0)
        {
            if (backwardDistance < -2.0f)
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.8f);
        }

        if (isOnCentralMud > 0)
        {
            dirtTimer += dt;
            float yPosition = yMaxHeightFactor * Mathf.Sin(dirtTimer * verticalHeightFrequency);
            transform.position += new Vector3(0.0f, yPosition, 0.0f);

            float yRotation = yMaxRotationFactor * (Mathf.Sin(dirtTimer * verticalRotationFrequency) - 0.5f);
            transform.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f);
        }
        else if (dirtTimer >= 0.0f)
        {
            dirtTimer = -1.0f;
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }

        if (GameplayManager.Gameplaystarted && !PlayerIsStopping && !GameplayManager.IsSpecialCarActive)
        {
            if (currentLane == 2 && environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
            {
                tempTimer += dt;
                if (tempTimer > 0.5f)
                {
                    if (dirtTimer < 0)
                    {
                        playerDraft.SendMessage("StartUSADraftAction");
                    }
                    else
                    {
                        tempTimer = 0.0f;
                        playerDraft.SendMessage("StopUSADraftAction");
                    }
                }
            }
            else if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA && prevLane == 2 && tempTimer != 0.0f)
            {
                playerDraft.SendMessage("StopUSADraftAction");
                tempTimer = 0.0f;
            }
            else
                tempTimer = 0.0f;
        }

        //Avoid to exit form the street...
        float maxXVal = 8.0f;
        if (Mathf.Abs(rb.transform.position.x) > maxXVal)
        {
            float nextX = Mathf.Clamp(rb.transform.position.x, -maxXVal, maxXVal);
            rb.transform.position = new Vector3(nextX, rb.transform.position.y, rb.transform.position.z);
        }

        prevVel = rb.velocity;
    }

    #region Jump update function
    /// <summary>
    /// 3、 计算从地面到车顶上升z，y方向的速度
    /// </summary>
    void InitializeLifting()
    {
        liftAngle = 0.0f;

        float truckHeight = 3.0f;
        float truckLength = 3.0f;

        if (rb.velocity.z < 70.0f)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 70.0f);

        // 车辆z方向速度
        float playerForwardSpeed = Mathf.Max(rb.velocity.z, truckInUse.gameObject.GetComponent<Rigidbody>().velocity.z + 50.0f);
        // 计算车辆上升过程消耗时间
        float liftTime = truckLength / playerForwardSpeed * VehicleUtils.ToKmh; //0.2f;

        //Calculate the target
        jumpTarget = gameObject.transform.position + gameObject.transform.forward * jumpDistance;
        jumpTarget.y = truckHeight;

        
        Vz = truckLength / liftTime;

        float i = truckLength / Mathf.Sin(90.0f - firingAngle * Mathf.Deg2Rad);
        Vy = i / liftTime;
    }

    /// <summary>
    /// 2、player跳跃，
    /// </summary>
    void StartLifting()
    {
        isFirstBounce = true;
        // 向上抬升
        isLifting = true;
        // 是否起跳
        isJumping = false;
        currentGravity = gravityBase;
        // 向下做抛物运动
        isDescendingParabola = false;
        InitializeLifting();
    }

    /// <summary>
    /// 4、车辆上升过程，同事调整车头朝向，当车辆高度大于等于target的y值时，开始执行StartJump
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="now"></param>
    void LiftingUpdate(float dt, float now)
    {
        gameObject.transform.Translate(0, Vy * dt, Vz * dt);

        UpdateJumpingRotation(10.0f, dt);

        // player到达npc车顶
        if (gameObject.transform.position.y >= jumpTarget.y)
        {
            StartJump(jumpDistance);
        }
    }

    /// <summary>
    /// 6、计算车辆落地点的位置，以及起跳各个方向速度
    /// </summary>
    /// <param name="distanceZ"></param>
    void InitializeJump(float distanceZ)
    {
        currentGravity = gravityBase;

        //Calculate the target
        jumpTarget = gameObject.transform.position + gameObject.transform.forward * distanceZ;
        jumpTarget.y = 0.0f;

        // Calculate distance to target
        float target_Distance = Vector3.Distance(gameObject.transform.position, jumpTarget);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravityBase);

        // Extract the X & Y componenent of the velocity
        Vz = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        flightDuration = target_Distance / Vz;
    }

    void CompleteJump(float distanceZ)
    {
        StartJump(distanceZ);
        liftAngle = 45.0f;
    }

    /// <summary>
    /// 5、做起跳的准备工作，并依据相应的起跳平台做相应的工作
    /// </summary>
    /// <param name="distanceZ"></param>
    /// <param name="deactivateDraft"></param>
    void StartJump(float distanceZ, bool deactivateDraft = true)
    {
        rb.velocity = new Vector3(0.0f, rb.velocity.y, rb.velocity.z);
        InitializeJump(distanceZ);
        isLifting = false;
        isJumping = true;
        elapse_time = 0;
        playerStartPositionY = 0.0f;// gameObject.transform.position.y;
        if (deactivateDraft)
            playerDraft.SendMessage("ActivateDraftParticles", false);

        if (truckOverType == TruckBehaviour.TruckType.Turbo && !TurboOn)
        {
            //Truck turbo!!!!!
            LevelRoot.Instance.Root.BroadcastMessage("ActivateTruckTurbo", true);
        }
    }

    /// <summary>
    /// 7、更新车辆挑起到落下过程
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="now"></param>
    void JumpUpdate(float dt, float now)
    {
        float verticalSpeed = (Vy - (currentGravity * elapse_time)) * dt;
        // 如果y速度方向为向下并且不是下降抛物线，或者强制为向下跳抛物线（飞行器用）
        // 再次一个作用为当车辆跳跃到最高点时，增大向下的重力，让其下降速度加快，切该逻辑语块，在同一次起跳中只执行一次
        if ((verticalSpeed < 0 && !isDescendingParabola) || forceJumpDescendingParabola)
        {
            // 如果游戏结束并且是强制向下
            if (forceJumpDescendingParabola && GameplayManager.GameplayTime == 0.0f)
                // ??
                gameObject.transform.position += Vector3.up * 3.5f;
            forceJumpDescendingParabola = false;
            isDescendingParabola = true;
            currentGravity = gravityBase * gravityMultiplier;
        }

        gameObject.transform.Translate(0, verticalSpeed, Vz * dt);
        elapse_time += dt;

        UpdateJumpingRotation(-0.5f, dt);

        // 当车辆的垂直高度小于或等于落地点的水平高度时，结束此次跳跃
        if (gameObject.transform.position.y <= jumpTarget.y)
        {
            EndJump();
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, playerStartPositionY, gameObject.transform.position.z);
        }
    }

    void UpdateJumpingRotation(float coeff, float dt)
    {
        Vector3 rotTan = new Vector3(0.0f, liftAngle, 1.0f);
        rb.MoveRotation(Quaternion.LookRotation(rotTan));
        liftAngle += dt * coeff;
        liftAngle = Mathf.Clamp(liftAngle, 0.0f, 0.5f);
    }

    void FastEndJump()
    {
        isJumping = false;
        currentGravity = gravityBase;
        isFirstBounce = false;
        isDescendingParabola = false;
    }

    /// <summary>
    /// 结束跳跃
    /// </summary>
    void EndJump()
    {
        // 中止跳跃循环逻辑
        isJumping = false;

        if (isFirstBounce)
        {
            if (GameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Tank)
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.LandingTank);
            else
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.Landing);

            currentGravity = gravityBase;
            isFirstBounce = false;
            StartJump(jumpDistanceBounce, false);
            gameObject.SendMessage("OnPlayerLand");

            FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(0.8f, 0.8f, 0.0f);
            cameraRef.SendMessage("StartShakeCamera", sData);

            // 目前认为是当玩家车辆落地时，如果附近一定范围内有其他车辆，则播放特定砸地效果
            for (int i = 0; i < spawnManager.ActiveOpponents.Count; ++i)
            {
                GameObject opponent = spawnManager.ActiveOpponents[i];
                if (opponent != null)
                {
                    OpponentKinematics kin = opponent.GetComponent<OpponentKinematics>();
                    float opponentDistance = (opponent.transform.position - gameObject.transform.position).magnitude;
                    if (kin != null && (opponent.name.Contains("TrafficVehicle") || opponent.tag.Equals("Police")))
                    {
                        if (opponentDistance < radiusExplosionEndJump)
                        {
                            float angle = Vector3.Angle(gameObject.transform.position, opponent.gameObject.transform.position) * SBSMath.ToDegrees;
                            if (gameObject.transform.position.x > opponent.gameObject.transform.position.x)
                                angle = -angle;
                            kin.SendMessage("OnPlayerEndJump", angle);
                        }
                    }
                }
            }
        }
        else
        {
            if (OnTheRunTutorialManager.Instance.TutorialActive)
                OnTheRunTutorialManager.Instance.EndTutorial();

            isDescendingParabola = false;

            if (isStopping)
                forwardAcc = -Math.Abs(forwardAcc);
        }
    }
    #endregion

    void OnCollisionEnter(Collision collision)
    {
        if (lastCollisionTime != -1.0f)
            return;

        float now = TimeManager.Instance.MasterSource.TotalTime;

        if (isJumping)
            return;

        if (now - avoidFrequentCollisionTime < 0.5f)
            return;

        playerDraft.SendMessage("PalyerHasCollided");
        //uiSharedData.ResetScoreMultiplier();

        bool lateralCollision = Math.Abs(collision.contacts[0].normal.x) >= 0.9f ? true : false;
        if (shieldOn && !lateralCollision)
        {
            collision.collider.SendMessage("OnPlayerWithShieldCollison", GetComponent<Collider>(), SendMessageOptions.DontRequireReceiver);
            return;
        }

        if (!lateralCollision)
        {
            if (UnityEngine.Random.Range(0.0f, 100.0f) > 70.0f)
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Collision);
            ActivateFrontCollisionEffect(collision.contacts[0].point);
            collisionPosition = distance;
        }
        else
        {
            ActivateSideCollisionEffect(collision.contacts[0].point, collision.contacts[0].normal.x > 0);
        }
        //ActivateCollisionSparks(collision.contacts[0].point);

        //Debug.Log("OnCollisionEnter---------------------------");
        //       float time = TimeManager.Instance.MasterSource.TotalTime;
        //       if (lastCollisionTime >= 0.0f && (time - lastCollisionTime) <= 0.33f)
        //           return;
        //lastCollisionTime = TimeManager.Instance.MasterSource.TotalTime;

        gameObject.SendMessage("PlayPlayerSound", PlayerSounds.PlayerSoundsType.Hit, SendMessageOptions.DontRequireReceiver);

        if (!lateralCollision)
            gameplayManager.UpdateRunParameter(OnTheRunGameplay.RunParameters.HitsCount, 1);

        if (!turboOn && !isDead && !slipstreamOn && !shieldOn)
        {
            //added OnTheRunGameplay.oldSystemActive check
            if (OnTheRunGameplay.oldSystemActive)
            {
                lastCollisionTime = now;
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.70f);
                rb.AddExplosionForce(5.0f, rb.transform.position + rb.transform.forward, 2.0f, 3.0f, ForceMode.VelocityChange);
                rb.constraints = RigidbodyConstraints.None;
                LevelRoot.Instance.BroadcastMessage("PlayerIsDead");
                isDead = true;
                Manager<UIRoot>.Get().GoToRewardSequence();
            }
            else
            {
                avoidFrequentCollisionTime = now;
                if (!lateralCollision)
                {
                    rb.velocity = prevVel * OnTheRunDataLoader.Instance.GetPlayerSpeedReductionPerHit();

                    timeLostForHit = GetDataFromPlayerDataIndex(PlayerDataType.Resistance, PlayerPersistentData.Instance.GetPlayerData(carId).resistance);//PlayerPersistentData.Instance.GetPlayerData(carId).resistance;

                    bool goRight;
                    if (currentLane == 0) goRight = true;
                    else if (currentLane == OnTheRunEnvironment.lanes.Length - 1) goRight = false;
                    else goRight = UnityEngine.Random.Range(1.0f, 100.0f) > 50.0f;
                    float impulseForce = goRight ? -2500.0f : 2500.0f;
                    rb.AddForce(Vector3.right * impulseForce, ForceMode.Impulse);
                    goOneLaneDx = goRight ? true : false;
                    goOneLaneSx = !goOneLaneDx;
                }
            }
        }
    }
    #endregion

    #region Messages
    void OnFlyingEnded()
    {
        forceJumpDescendingParabola = true;
        StartJump(10.0f);

        for (int i = 0; i < 5; ++i)
        {
            float currLanePos = 17.5f * 0.5f * OnTheRunEnvironment.lanes[i];
            if (Math.Abs(rb.transform.position.x - currLanePos) < 1.5f)
            {
                currentLane = i;
                break;
            }
        }
    }

    public void ResumeInStoppingState()
    {
        isStopping = true;
    }

    void OnShooterDamage()
    {
        bool goRight;
        if (currentLane == 0) goRight = true;
        else if (currentLane == OnTheRunEnvironment.lanes.Length - 1) goRight = false;
        else goRight = UnityEngine.Random.Range(1.0f, 100.0f) > 50.0f;
        float impulseForce = goRight ? -600.0f : 600.0f;
        rb.AddForce(Vector3.right * impulseForce + rb.transform.forward * -350.0f, ForceMode.Impulse);
    }

    void OnHelicopterDamage()
    {
        bool goRight;
        if (currentLane == 0) goRight = true;
        else if (currentLane == OnTheRunEnvironment.lanes.Length - 1) goRight = false;
        else goRight = UnityEngine.Random.Range(1.0f, 100.0f) > 50.0f;
        float impulseForce = goRight ? -2500.0f : 2500.0f;
        rb.AddForce(Vector3.right * impulseForce + rb.transform.forward * -2500.0f, ForceMode.Impulse);
    }

    void OnMineCollision()
    {
        bool goRight;
        if (currentLane == 0) goRight = true;
        else if (currentLane == OnTheRunEnvironment.lanes.Length - 1) goRight = false;
        else goRight = UnityEngine.Random.Range(1.0f, 100.0f) > 50.0f;
        float impulseForce = goRight ? -2500.0f : 2500.0f;
        rb.AddForce(Vector3.right * impulseForce, ForceMode.Impulse);
        goOneLaneDx = goRight ? true : false;
        goOneLaneSx = !goOneLaneDx;

        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z * 0.70f);
    }

    void OnEquipBooster(OnTheRunBooster.BoosterType type)
    {
        switch (type)
        {
            case OnTheRunBooster.BoosterType.Shield:
                OnShieldActive(true);
                break;
        }
    }

    void OnTruckLateralcollision()
    {
        inputRight = inputLeft = false;
        if (currentLane != prevLane)
            currentLane = prevLane;
        else if (currentLane >= 2)
            currentLane--;
        else
            currentLane++;
    }

    void OnEnterCentralMud()
    {
        if (GameplayManager.IsSpecialCarActive)
            return;

        if (isOnCentralMud == 0)
        {
            dirtTimer = 0.0f;
            cameraRef.SendMessage("StopShakeCamera");
            FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(10.0f, 0.3f, 0.0f);
            if (GameplayManager.GameplayTime > 0.0f)
                cameraRef.SendMessage("StartShakeCamera", sData);
            gameObject.SendMessage("ActivateDirtDust", true);

            backupMaxSpeed = maxSpeed;
            maxSpeed *= 0.8f;
        }
        ++isOnCentralMud;
    }

    void OnExitCentralMud()
    {
        if (GameplayManager.IsSpecialCarActive)
            return;

        --isOnCentralMud;
        if (isOnCentralMud == 0)
        {
            cameraRef.SendMessage("StopShakeCamera");
            maxSpeed = backupMaxSpeed;
            gameObject.SendMessage("ActivateDirtDust", false);
        }
    }
    /*void OnGuardRailcollision()
    {
        inputRight = inputLeft = false;
        Vector3 offset = Vector3.zero;
        if (prevLane > 2)
        {
            currentLane = 3;
            offset = Vector3.right * -1.0f;
        }
        else if (prevLane < 2)
        {
            currentLane = 1;
            offset = Vector3.right * 1.0f;
        }
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, offset + Vector3.up * 1.0f, OnTheRunObjectsPool.ObjectType.SideCollision, true);
    }*/

    /// <summary>
    /// 1、 第一步，开始起跳
    /// </summary>
    /// <param name="truckGO"></param>
    public void OnTruckEnter(GameObject truckGO)
    {
        OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.Ramp);
        truckInUse = truckGO.GetComponent<TruckBehaviour>();
        rb.velocity = new Vector3(0.0f, rb.velocity.y, rb.velocity.z);
        StartLifting();
        truckOverType = truckInUse.truckType;
        transformInto = truckInUse.CarOverTruckType;
    }

    void OnCheckpointEnter()
    {
        if (isStopping)
        {
            isStopping = false;
            forwardAcc = Mathf.Abs(forwardAcc);
        }
    }

    void OnTimeEnded()
    {
        if (turboOn)
            OnTurboActive(false);

        if (slipstreamOn)
            OnSlipstreamActive(false);

        uiSharedData.ResetScoreMultiplier();

        isStopping = true;
        if (!IsJumping)
            forwardAcc = -Math.Abs(forwardAcc);
        LevelRoot.Instance.BroadcastMessage("OnTurboEnd");
        LevelRoot.Instance.BroadcastMessage("OnTruckTurboEnd");
    }

    void OnTimeEndedForReal()
    {
        OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.TimeEnd);

        forwardAcc = Math.Abs(forwardAcc);
        isStopping = false;
        rb.AddTorque(0.0f, 10.0f, 0.0f, ForceMode.Impulse);
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0.0f);
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        LevelRoot.Instance.BroadcastMessage("PlayerIsDead");
        Manager<UIRoot>.Get().LaunchTimeOutFlyer();
        gameObject.SendMessage("OnDead");

        cameraRef.SendMessage("StopShakeCamera");

        GameplayManager.CurrentRunScore = (int)Distance;
        PlayerPersistentData.Instance.SetBestMeters((int)(Distance * OnTheRunUtils.ToOnTheRunMeters));

        isDead = true;

        GameplayManager.SendMessage("BackToDefaultCar");
    }

    void PlayerIsResurrected()
    {
        isStopping = false;
        isDead = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        if (GameplayManager.IsSpecialCarActive)
            playerDraft.SendMessage("ActivateDraftParticles", true);
    }

    void OnTurboActive(bool active)
    {
        gameplayManager.MainCamera.SendMessage("OnTurboCameraEvent", active);

        if (active)
        {
            backupShieldOn = shieldOn;
            uiSharedData.ResetScoreMultiplier();
        }

        RemovePreviousEffects();

        turboOn = active;
        if (turboOn)
        {
            turboRatio = 1.0f;
            SetTurboFxSize(1.0f);
        }
        else
            turboRatio = 0.0f;
        gameObject.SendMessage("ActivateTurboFx", active);

        if (GameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.None)
        {
            if (!active)
                OnCollisionProtectionActivation(true);
            else
                GetComponent<Collider>().isTrigger = turboOn;
        }

        if (!active)
        {
            GameplayManager.SendMessage("EndExtendGameplay");

            if (backupShieldOn)
            {
                backupShieldOn = false;
                OnShieldActive(true);
            }
        }
    }

    void OnCollisionProtectionActivation(bool active)
    {
        if (active)
        {
            GetComponent<Collider>().isTrigger = true;
            collisionProtectionTimer = gameplayManager.CollisionProtectionTime;
        }
        else if (!turboOn)
        {
            collisionProtectionTimer = -1.0f;
            GetComponent<Collider>().isTrigger = false;
        }
    }

    void OnShieldActive(bool active)
    {
        if (active && turboOn)
        {
            backupShieldOn = true;
            gameObject.SendMessage("ActivateShieldFx", active);
        }
        else
        {
            shieldOn = active;
            gameObject.SendMessage("ActivateShieldFx", active);

            if (GameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.None)
            {
                //    collider.isTrigger = shieldOn;
                if (active)
                {
                    if (rb.mass != float.PositiveInfinity)
                    {
                        backupPlayerMass = rb.mass;
                    }
                    rb.mass = float.PositiveInfinity;
                }
                else
                    rb.mass = backupPlayerMass;
            }
        }
    }

    void OnSlipstreamActive(bool active)
    {
        slipstreamOn = active;
        if (slipstreamOn)
            turboRatio = 1.0f;
        else
            turboRatio = 0.0f;
        gameObject.SendMessage("ActivateSlipstreamFx", active);

        //if (gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.None)
        //    collider.isTrigger = slipstreamOn;
    }

    void OnTurboUpdate(float ratio)
    {
        turboRatio = ratio;

        if (ratio <= 0.25f)
            SetTurboFxSize(ratio * 4.0f);
    }

    void RemovePreviousEffects()
    {
        if (slipstreamOn)
            OnSlipstreamActive(false);

        if (shieldOn)
            OnShieldActive(false);
    }

    void OnGameover()
    {
        gameObject.SendMessage("ActivateTurboFx", false);
        inputLeft = false;
        inputRight = false;
    }

    void RaceStarted()
    {
        startDistance = rb.position;
        gameplayStarted = true;
        isJumping = false;
        keepSpeedTimer = -1.0f;
    }

    public void OnDraft()
    {
    }

    public void OnDraftEnd()
    {
    }

    void ActivateCollisionSparks(Vector3 collisionPoint)
    {
        OnTheRunObjectsPool.Instance.RequestEffectInPoint(collisionPoint, Vector3.zero, OnTheRunObjectsPool.ObjectType.Sparks);
    }

    void ActivateFrontCollisionEffect(Vector3 collisionPoint)
    {
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, gameObject.transform.forward * 1.5f + Vector3.up * 1.0f, OnTheRunObjectsPool.ObjectType.FrontCollision, true);
    }

    void ActivateSideCollisionEffect(Vector3 collisionPoint, bool rightSide)
    {
        Vector3 offset = rightSide ? Vector3.right * -1.0f : Vector3.right * 1.0f;
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, offset + Vector3.up * 1.0f, OnTheRunObjectsPool.ObjectType.SideCollision, true);
    }

    void OnLeftInputDown()
    {
        inputLeft = true;
    }

    void OnLeftInputUp()
    {
        inputLeft = false;
    }

    void OnRightInputDown()
    {
        inputRight = true;
    }

    void OnRightInputUp()
    {
        inputRight = false;
    }

    void OnReset()
    {
        isStopping = false;
        isDead = false;
        isOnLane = true;
        distance = 0;

        isRunning = false; // ALEX
    }

    void OnDead()
    {
        isDead = true;
    }

    void OnStartGame()
    {
        isRunning = true;// ALEX
        isStopping = false;
        isDead = false;
        lastCollisionTime = -1.0f;
        avoidFrequentCollisionTime = -1.0f;
        inputLeft = false;
        inputRight = false;
        turboOn = false;
        turboRatio = 0.0f;
        slipstreamOn = false;

        gameObject.transform.position = Vector3.zero;
        rb.position = new Vector3(0.0f, 0.0f, 0.0f);
        rb.rotation = Quaternion.LookRotation(new Vector3(0.0f, 0.0f, 1.0f));

        rb.velocity = new Vector3(0.0f, 0.0f, 60.0f);

        //rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        currentLane = 2;
        prevLane = currentLane;
        maxSpeed = initialMaxSpeed;

        gameObject.SendMessage("ActivateTurboFx", false);
        //maxBonusSpeed = initialMaxSpeed * 2.0f;

        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

    }

    void OnChangePlayerCar()
    {
        isRunning = true;// ALEX
    }

    void OnStartPlayerRun()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void ShiftPlayerForward()
    {
        distance = 0;
        const float shiftingDistance = 20.0f;
        gameObject.transform.position = Vector3.forward * shiftingDistance;
    }
    #endregion
}
