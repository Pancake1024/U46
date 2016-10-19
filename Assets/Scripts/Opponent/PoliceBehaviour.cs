using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

public class PoliceBehaviour : MonoBehaviour
{
    #region Public members
    public EnterStyle enterStyle;
    public OnTheRunObjectsPool.ObjectType behaviour;
    public float keepDistance;
    public float lifeTime;
    public float clampSpeedAt;
    public GameObject policeLights;
    public GameObject shadow;
    public Material[] policeMaterials;
    #endregion

    #region Protected members
    protected OnTheRunSpawnManager spawnManager;
    protected OnTheRunEnemiesManager enemiesManager;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunEnvironment environmentManager;
    protected PlayerKinematics playerKin;
    protected OpponentKinematics opponentK;
    protected EnemyState state;
    protected List<float> badGuyBackupValues;
    protected float lifeTimer = -1.0f;
    protected bool isChangingLane = false;
    protected float badGuyRayCastTime = 0.3f;
    protected float badGuyRayCastTimer = -1.0f;
    protected bool backwardLaneFlag = false;
    protected ParticleSystem policeLightsParticles;
    protected AudioSource policeSound;
    protected LayerMask ignoreLayers = 0;
    protected int isMyTurn = 0;
    protected bool isNowSuperable = false;
    protected Rigidbody rb;
    #endregion

    #region Public Porperties
    public enum EnterStyle
    {
        FromBehind,
        FromFront
    }

    public enum EnemyState
    {
        Approaching,
        KeepDistance,
        GoAway,
        CanBeDestroyed,
        WaitingMyTurn
    }

    public int MyTurn
    {
        get { return isMyTurn; }
        set { isMyTurn = value; }
    }

    public EnemyState State
    {
        get { return state; }
    }

    public bool CanBeDestroyed
    {
        get { return state == EnemyState.CanBeDestroyed; }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.GetComponent<OnTheRunSpawnManager>();
        opponentK = gameObject.GetComponent<OpponentKinematics>();
        enemiesManager = gameplayManager.GetComponent<OnTheRunEnemiesManager>();
        //behaviour = OnTheRunObjectsPool.ObjectType.None;
        state = EnemyState.Approaching;
        
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        switch (environmentManager.currentEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe:
                gameObject.transform.FindChild("police_car").GetComponent<Renderer>().material = policeMaterials[2];
                break;
            case OnTheRunEnvironment.Environments.Asia:
                gameObject.transform.FindChild("police_car").GetComponent<Renderer>().material = policeMaterials[1];
                break;
            case OnTheRunEnvironment.Environments.USA:
                gameObject.transform.FindChild("police_car").GetComponent<Renderer>().material = policeMaterials[3];
                break;
            case OnTheRunEnvironment.Environments.NY:
                gameObject.transform.FindChild("police_car").GetComponent<Renderer>().material = policeMaterials[0];
                break;
        }

        Initialize(false);
    }

    void Start()
    {
        ResetPoliceLights();
        shadow.SetActive(true);
        isNowSuperable = false;

        if (behaviour == OnTheRunObjectsPool.ObjectType.RoadBlock || enterStyle==EnterStyle.FromBehind)
            StartPoliceLights(true);
    }

    void ResetPoliceLights( )
    {
        if (policeLights != null)
        {
            shadow.SetActive(true);
            policeLightsParticles = policeLights.GetComponent<ParticleSystem>();
            policeSound = gameObject.GetComponent<AudioSource>();
            if (policeSound!=null)
                OnTheRunSoundsManager.Instance.StopSource(policeSound);
            StartPoliceLights(false);
        }
    }

    void StartPoliceLights(bool active)
    {
        if (policeLightsParticles != null)
        {
            if (active)
            {
                policeLightsParticles.Clear(true);
                policeLightsParticles.Play(true);
                if (policeSound != null)
                    OnTheRunSoundsManager.Instance.PlaySource(policeSound);
            }
            else
            {
                policeLightsParticles.Stop(true);
                if (policeSound != null)
                    OnTheRunSoundsManager.Instance.StopSource(policeSound);
            }
        }
    }

    //bool CheckLaneBySide(string side, Vector3 startPos, Vector3 endPos)
	bool CheckLaneBySide(bool isRightLane, Vector3 startPos, Vector3 endPos)
	{
        Vector3 offset = Vector3.left * 3.33f;
		if (isRightLane)//side.Equals("right"))
            offset = Vector3.right * 3.33f;

        RaycastHit hit;
        Physics.Raycast(startPos + offset, endPos + offset, out hit, 20.0f, ~ignoreLayers);
        if (hit.collider)
        {
            return true;
        }

        return false;
    }
    
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (behaviour == OnTheRunObjectsPool.ObjectType.RoadBlock)
        {
            if(!policeSound.isPlaying)
                OnTheRunSoundsManager.Instance.PlaySource(policeSound);
            float deltaZ = playerKin.PlayerRigidbody.position.z - gameObject.transform.position.z;
            float deltaY = playerKin.PlayerRigidbody.position.y - gameObject.transform.position.y;
            if (deltaZ > 30 || deltaY < -10.0f)
            {
                OnTheRunObjectsPool.Instance.NotifyDestroyingExplosion(gameObject);
                OnTheRunObjectsPool.Instance.NotifyDestroyingParent(gameObject, OnTheRunObjectsPool.ObjectType.RoadBlock);
            }
        }
        else
        {
            //Avoid traffic-------------------------
            badGuyRayCastTimer -= dt;
            if (!isChangingLane) // && badGuyRayCastTimer < 0.0f)
            {
                badGuyRayCastTimer = badGuyRayCastTime;
                float forwardDistance = 4.0f;
                if ((opponentK.OpponentRigidbody.velocity.z / VehicleUtils.ToMetersPerSecs) > 250.0f)
                    forwardDistance = 15.0f;

                if (gameplayManager.IsInWrongDirection(opponentK.CurrentLane))
                {
                   forwardDistance = 100.0f;
                }

                bool rightHit = false,
                     leftHit = false;
                Vector3 startPos = gameObject.transform.position + Vector3.forward * 3.4f + Vector3.up * 1.0f;
                Vector3 endPos = startPos + Vector3.forward * forwardDistance;

                RaycastHit hit;
                bool hitted = Physics.Raycast(startPos, Vector3.forward, out hit, forwardDistance, ~ignoreLayers);
                if (hitted && hit.collider.gameObject.GetComponent<PoliceBehaviour>() == null)
                {
                    bool lookFirstDx = UnityEngine.Random.Range(0, 100) > 50;
                    if ((lookFirstDx || opponentK.CurrentLane == 0) && opponentK.CurrentLane != 4)
                    {
                        rightHit = CheckLaneBySide(/*"right"*/true, startPos, endPos); //look dx------------
                        if (rightHit)
                            leftHit = CheckLaneBySide(/*"left"*/false, startPos, endPos); //look sx------------
                    }
                    else
                    {
                        leftHit = CheckLaneBySide(/*"left"*/false, startPos, endPos); //look sx------------
                        if (leftHit)
                            rightHit = CheckLaneBySide(/*"right"*/true, startPos, endPos); //look dx------------
                    }

                    if (!rightHit)
                    {
                        isChangingLane = true;
                        opponentK.SendMessage("OnChangeLaneBadGuy", 1);
                    }
                    else if (!leftHit)
                    {
                        isChangingLane = true;
                        opponentK.SendMessage("OnChangeLaneBadGuy", -1);
                    }
                }
            }
            //--------------------------------------

            //Go away if player is dying------------
            if (playerKin.PlayerIsStopping && state != EnemyState.GoAway)
            {
                state = EnemyState.GoAway;
                opponentK.SetupBadGuyParameters(90.0f, 280.0f);
            }
            //--------------------------------------

            //Generic behaviour---------------------
            float playerDist = gameObject.transform.position.z - playerKin.gameObject.transform.position.z;
            if (state == EnemyState.Approaching)
            {
                bool checkForEndApproach = enterStyle == EnterStyle.FromBehind ? playerDist > keepDistance - 20.0f : playerDist < keepDistance;
                if (checkForEndApproach)
                {
                    if ((isMyTurn == 0 && enterStyle == EnterStyle.FromBehind) || enterStyle == EnterStyle.FromFront)
                    {
                        ActivatePolice();
                    }
                    else
                    {
                        state = EnemyState.WaitingMyTurn;
                        opponentK.SetupBadGuyParameters(playerKin.ForwardAcceleration, playerKin.CurrentSpeed);
                    }
                }
                else if(playerKin.TurboOn)
                {
                    opponentK.SetupBadGuyParameters(70.0f, 50.0f);
                    state = EnemyState.GoAway;
                }
            }
            else if (state == EnemyState.WaitingMyTurn)
            {
                if (playerDist > keepDistance * 1.5f)
                    rb.drag = 0.3f;
                else
                    rb.drag = 0.1f;

                if (playerDist < keepDistance && !playerKin.TurboOn)
                    opponentK.SetupBadGuyParameters(20.0f, 380.0f);
                else
                {
                    float maxSpeed = Mathf.Clamp(playerKin.CurrentSpeed * 0.8f, 0.0f, clampSpeedAt);
                    opponentK.SetupBadGuyParameters(playerKin.ForwardAcceleration, maxSpeed);
                }
            }
            else if (state == EnemyState.KeepDistance)
            {
                if (playerDist > keepDistance)
                    rb.drag = 0.3f;
                else
                    rb.drag = 0.1f;

                float maxSpeed = Mathf.Clamp(playerKin.PlayerRigidbody.velocity.z / VehicleUtils.ToMetersPerSecs, 0.0f, clampSpeedAt);
                opponentK.SetupBadGuyParameters(playerKin.ForwardAcceleration, maxSpeed);
                
                lifeTimer -= dt;
                if (lifeTimer < 0.0f)
                {
                    state = EnemyState.GoAway;
                    opponentK.SetupBadGuyParameters(70.0f, 50.0f);//320.0f);
                    if (enterStyle == EnterStyle.FromBehind)
                        spawnManager.SendMessage("ActivateNextPoliceCar", isMyTurn);
                }

            }
            else if (state == EnemyState.GoAway)
            {
                if (playerDist > 200.0f || playerDist < -30.0f)
                {
                    state = EnemyState.CanBeDestroyed;
                    ResetPoliceLights();
                    if (enterStyle == EnterStyle.FromBehind)
                        spawnManager.SendMessage("ActivateNextPoliceCar", isMyTurn);
                }
            }

            if (enterStyle == EnterStyle.FromBehind && playerDist > 0 && !isNowSuperable)
                isNowSuperable = true;

            //Check if can be destroyed
            if (enterStyle == EnterStyle.FromFront || isNowSuperable)
            {
                if (playerDist < -30.0f || playerDist > 250.0f)
                {
                    state = EnemyState.CanBeDestroyed;
                    ResetPoliceLights();
                    if (enterStyle == EnterStyle.FromBehind)
                        spawnManager.SendMessage("ActivateNextPoliceCar", isMyTurn);
                }
            }

            /*if ((playerDist < -30.0f || playerDist > 250.0f ) && state != EnemyState.Approaching)
            {
                state = EnemyState.CanBeDestroyed;
                ResetPoliceLights();
                if (enterStyle == EnterStyle.FromBehind)
                    spawnManager.SendMessage("ActivateNextPoliceCar", isMyTurn);
            }*/
            //--------------------------------------
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        bool collidedWithPlayer = collision.collider.gameObject.tag.Equals("Player");
        bool lateralCollision = Math.Abs(collision.contacts[0].normal.x) >= 0.9f ? true : false;
        if (collidedWithPlayer && (lateralCollision || behaviour == OnTheRunObjectsPool.ObjectType.RoadBlock))
        {
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("OnPoliceDestroyed");

            LevelRoot.Instance.BroadcastMessage("OnDestroyspecificCar", OpponentKinematics.OpponentId.TrafficPolice);
            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);

            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.BadGuy);
            rb.AddExplosionForce(30.0f, rb.transform.position + rb.transform.forward, 2.0f, 3.0f, ForceMode.VelocityChange);
            rb.constraints = RigidbodyConstraints.None;
            opponentK.IsExploded = true;
            ResetPoliceLights();
            shadow.SetActive(false);
            DectivatePolice();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //for turbo mode
        bool collideWithPlayer = other.gameObject.tag.Equals("Player");
        if (collideWithPlayer)
        {
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("OnPoliceDestroyed");

            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.BadGuy);
            rb.AddExplosionForce(30.0f, rb.transform.position + rb.transform.forward, 2.0f, 3.0f, ForceMode.VelocityChange);
            rb.constraints = RigidbodyConstraints.None;
            opponentK.IsExploded = true;
            ResetPoliceLights();
            shadow.SetActive(false);
            DectivatePolice();
        }
    }
    #endregion

    #region Messages
    void ActivatePolice( )
    {
        if(enterStyle==EnterStyle.FromFront)
            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.PoliceEnter);

        float playerDist = gameObject.transform.position.z - playerKin.gameObject.transform.position.z;
        if (playerDist>0)
        {
            state = EnemyState.KeepDistance;
            opponentK.SetupBadGuyParameters(playerKin.ForwardAcceleration, playerKin.CurrentSpeed);
            lifeTimer = lifeTime;
            StartPoliceLights(true);
        }
    }

    void DectivatePolice()
    {
        enemiesManager.SendMessage("OnEndPoliceCar");
        state = EnemyState.Approaching;

        if (behaviour != OnTheRunObjectsPool.ObjectType.RoadBlock && enterStyle == EnterStyle.FromBehind)
            spawnManager.SendMessage("ActivateNextPoliceCar", isMyTurn);
    }

    void Initialize(bool updateGameplay)
    {
        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        //Initialization -> generic for "avoid traffic" behaviour
        state = EnemyState.Approaching;
        badGuyBackupValues = new List<float>();
        badGuyBackupValues.Add(opponentK.forwardAcc);
        badGuyBackupValues.Add(opponentK.maxSpeed);
        isNowSuperable = false;

        if (enterStyle == EnterStyle.FromBehind)
        {
            opponentK.SetupBadGuyParameters(15.0f, 380.0f);//opponentK.SetupBadGuyParameters(20.0f, 380.0f);
            StartPoliceLights(true);
        }
        else
        {
            opponentK.SetupBadGuyParameters(5.0f, 140.0f);
            ResetPoliceLights();
        }

        gameplayManager.IsBadGuyActive = updateGameplay;
    }

    void InitializeDistance(int spawnIndex)
    {
        switch (spawnIndex)
        {
            case 0:
                keepDistance = 50.0f;//45.0f;
                break;
            case 1:
                keepDistance = 30.0f;//30.0f;
                break;
            case 2:
                keepDistance = 20.0f;
                break;
        }
    }

    void ResetBadGuy()
    {
        badGuyRayCastTimer = -1.0f;
        isChangingLane = false;
        gameplayManager.IsBadGuyActive = false;
        opponentK.SetupBadGuyParameters(badGuyBackupValues[0], badGuyBackupValues[1]);
        badGuyBackupValues.Clear();
        state = EnemyState.Approaching;
        LevelRoot.Instance.Root.BroadcastMessage("RestartEnemiesManager", false);
    }
    
    void OnChangeLaneEnd()
    {
        isChangingLane = false;
    }
    #endregion
}
