using System;
using System.Collections.Generic;
using UnityEngine;

public class BonusBehaviour : IntrusiveListNode<BonusBehaviour>
{
    #region Public Members
    public float bonusSpeed = 125.0f;
    public float rotationSpeed = 0.0f;
    public float verticalSpeed = 0.0f;
    public OnTheRunObjectsPool.ObjectType bonusType;
    public int coinsGiven;
    #endregion

    #region Protected Members
    protected GameObject playerRef;
    protected OnTheRunGameplay gameplayManager;
    protected Vector3 startPosition;
    protected float startTime;
    protected float verticalDistance = 0.2f;
    protected Vector3 startMagnetPoint = Vector3.zero;
    protected float startMagnetTime = 0.0f;
    protected bool magnetActivated = false;
    protected bool activateSlowingDown = false;
    protected bool resetted = false;

    //magnet stuff
    protected float magnetMovementDuration = 0.30f; // 0.5f
    protected float magnetRadius = 25.0f; // 45.0f

    protected bool underUfo = false;
    public bool isInRoadWorks = false;

    protected Rigidbody rb;
    protected Rigidbody playerRb;
    protected PlayerKinematics playerKinematics;
    protected SpawnableObjectType spawnObjType;
    protected OnTheRunSpawnManager spawnManager;
    #endregion

    public bool IsInRoadWorks
    {
        set { 
                isInRoadWorks = value;
                if(isInRoadWorks)
                    rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    public class BonusData
    {
        public OnTheRunObjectsPool.ObjectType type;
        public int coinsToGain;
    }

    #region Unity callbacks
    void OnTriggerEnter(Collider other)
    {
        if (null == spawnObjType)
            spawnObjType = gameObject.GetComponent<SpawnableObjectType>();
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("BonusSpecialTrigger"))
        {
            OnTheRunObjectsPool.ObjectType type = spawnObjType.type;
            Vector3 Offset = Vector3.forward * 1.7f + Vector3.up;
            if (gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Plane || gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Ufo)
                Offset = Vector3.forward * 0.0f - Vector3.up * playerRef.GetComponent<FlyingBehaviour>().flyingHeight * 0.45f;
            
                switch (type)
            {
                case OnTheRunObjectsPool.ObjectType.BonusMoney:
                case OnTheRunObjectsPool.ObjectType.BonusMoneyBig:
                    OnTheRunObjectsPool.Instance.RequestEffect(other.gameObject.transform, Offset, OnTheRunObjectsPool.ObjectType.PickupCoins, true);
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusMagnet:
                    OnTheRunObjectsPool.Instance.RequestEffect(other.gameObject.transform, Offset, OnTheRunObjectsPool.ObjectType.PickupMagnet, true);
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusShield:
                    OnTheRunObjectsPool.Instance.RequestEffect(other.gameObject.transform, Offset, OnTheRunObjectsPool.ObjectType.PickupShield, true);
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusTurbo:
                    OnTheRunObjectsPool.Instance.RequestEffect(other.gameObject.transform, Offset, OnTheRunObjectsPool.ObjectType.PickupBolt, true);
                    break;
            }

            BonusData data = new BonusData();
            data.type = type;
            data.coinsToGain = coinsGiven;
            LevelRoot.Instance.BroadcastMessage("OnBonusCollected", data);
            OnTheRunObjectsPool.Instance.NotifyDestroyingParent(gameObject, type);
        }
    }

    void Awake()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        playerRb = playerRef.GetComponent<Rigidbody>();
        playerKinematics = playerRef.GetComponent<PlayerKinematics>();
        rb = GetComponent<Rigidbody>();
        spawnObjType = gameObject.GetComponent<SpawnableObjectType>();

        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.gameObject.GetComponent<OnTheRunSpawnManager>();
    }

    void OnEnable()
    {
        spawnManager.__bonuses.Add(this);
    }

    void OnDisable()
    {
        if (spawnManager != null)
            spawnManager.__bonuses.Remove(this);
    }

    void Start()
    {
        startPosition = this.transform.localPosition;
        startTime = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnTrappedByUfo()
    {
        underUfo = true;
        rb.rotation = Quaternion.LookRotation(new Vector3(0.0f, 0.0f, 1.0f));
        rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        rb.constraints = RigidbodyConstraints.None;
    }

    public void MyFixedUpdate()
    {
        if (underUfo)
            return;

        float dt = Time.fixedDeltaTime;

        if (playerRef == null)
        {
            playerRef = GameObject.FindGameObjectWithTag("Player");
            playerRb = playerRef.GetComponent<Rigidbody>();
            playerKinematics = playerRef.GetComponent<PlayerKinematics>();
        }

        if (bonusType == OnTheRunObjectsPool.ObjectType.BonusTurbo && OnTheRunTutorialManager.Instance.TutorialActive && !OnTheRunTutorialManager.Instance.IsTutorialOnScreen)
        {
            if (OnTheRunTutorialManager.Instance.NextTutorial==OnTheRunTutorialManager.TutorialType.Turbo)
            {
                float distanceFromPlayer = transform.position.z - playerRef.transform.position.z;
                float distanceThreshold = OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION ? 30.0f : 20.0f;
                if (distanceFromPlayer < distanceThreshold)
                    OnTheRunTutorialManager.Instance.ShowTutorial();
            }
        }

        float speed = bonusSpeed * VehicleUtils.ToMetersPerSecs;
        if(!isInRoadWorks)
        {
            var playVel = playerRb.velocity;
            if (activateSlowingDown)
            {
                float targetSpeed = Mathf.Max(0.0f, playVel.z - 80.0f * VehicleUtils.ToMetersPerSecs);//Mathf.Min(bonusSpeed * VehicleUtils.ToMetersPerSecs, Mathf.Max(0.0f, playVel.z - 35.0f * VehicleUtils.ToMetersPerSecs));
                var rbVel = rb.velocity;
                speed = rbVel.z + (targetSpeed - rbVel.z) * dt * 80.0f;
            }
            rb.velocity = new Vector3(0.0f, 0.0f, speed);

            if (!activateSlowingDown)
            {
                if (playerKinematics.PlayerIsDead || playerKinematics.PlayerIsStopping)
                    activateSlowingDown = true;
            }
            else
            {
                if (playVel.z >= bonusSpeed * VehicleUtils.ToMetersPerSecs && playVel.z >= speed)// bonusSpeed * VehicleUtils.ToMetersPerSecs)
                    activateSlowingDown = false;
            }
        }

        //Magnet
        if (null == spawnObjType)
            spawnObjType = gameObject.GetComponent<SpawnableObjectType>();
        OnTheRunObjectsPool.ObjectType currType = spawnObjType.type;
        bool coinsMagnetActive = (gameplayManager.MagnetActive || magnetActivated) && (currType == OnTheRunObjectsPool.ObjectType.BonusMoney || currType == OnTheRunObjectsPool.ObjectType.BonusMoneyBig);
        bool boltMagnetActive = gameplayManager.TankMagnetActive && currType == OnTheRunObjectsPool.ObjectType.BonusTurbo;
        if (coinsMagnetActive || boltMagnetActive)
        {
            //magnetic force!
            Vector3 magnetField = playerRef.transform.position - transform.position;
            if (magnetField.magnitude < magnetRadius)
            {
                if (!magnetActivated)
                    magnetActivated = true;

                if (startMagnetPoint == Vector3.zero)
                {
                    startMagnetPoint = gameObject.transform.position;
                    startMagnetTime = Time.time;
                }
                Vector3 endPoint = playerRef.transform.position;
                gameObject.transform.position = Vector3.Lerp(startMagnetPoint, endPoint, (Time.time - startMagnetTime) / magnetMovementDuration);
            }
        }

        //rotation
        if (rotationSpeed != 0.0f)
        {
            transform.RotateAround(Vector3.up, rotationSpeed * dt);
        }

        //vertical movement
        if (verticalSpeed != 0.0f)
        {
            float elapsedTime = TimeManager.Instance.MasterSource.TotalTime - startTime;
            float newY = startPosition.y + verticalDistance * Mathf.Sin(elapsedTime * verticalSpeed);
            transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
        }

    }
    #endregion

    #region Messages
    void OnChangePlayerCar()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
    }
    
    public void ResetBonus( int lane )
    {
        gameObject.transform.position = new Vector3(0.0f, gameObject.transform.position.y, gameObject.transform.position.z);
        gameObject.transform.position += new Vector3(17.5f * 0.5f * OnTheRunEnvironment.lanes[lane], 0.0f, 0.0f);

        resetted = true;
        isInRoadWorks = false;
        underUfo = false;
        activateSlowingDown = false;
        magnetActivated = false;
        startMagnetPoint = Vector3.zero;
        startMagnetTime = 0.0f;

        if (bonusType == OnTheRunObjectsPool.ObjectType.BonusMoney || bonusType == OnTheRunObjectsPool.ObjectType.BonusMoneyBig)
        {
            float initRotation = (gameObject.transform.position.z % 360.0f) / 2.0f;
            transform.RotateAround(Vector3.up, -initRotation);
        }
    }
    #endregion
}
