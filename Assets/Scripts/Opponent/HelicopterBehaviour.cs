using UnityEngine;
using System.Collections;
using SBS.Core;

public class HelicopterBehaviour : MonoBehaviour
{
    enum State
    {
        Entering = 0,
        Following,
        Exiting,
        Inactive
    }

    public float helicopterAltitude = 4.0f;
    public float helicopterDistance = 20.0f;
    public float followingDuration = 25.0f;
    public float exitingDuration = 2.0f;
    public float propellerRotationSpeed = 1000.0f;
    public float stayOnLaneTime = 0.5f;
    public float floatingBaseSpeed = 20.0f;
    public GameObject propellerGO;

    protected OnTheRunEnemiesManager enemiesManager;
    protected OnTheRunGameplay gameplayManager;
    PlayerKinematics playerKinematics;
    State currentState = State.Inactive;
    Vector3 desiredPos;
    float waitToExitTimer = -1.0f;
    float exitingTimer = 0.0f;
    float propellerRotation = 0.0f;

    int targetLane = 0;
    float floatingTimer = 0.0f;
    float maxLaneDistance;

    void Awake()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
    }

    void FindPlayerKinematics()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        Asserts.Assert(playerGO != null, "Could not find GameObject with tag \"Player\"");

        if (playerGO!=null)
        { 
            playerKinematics = playerGO.GetComponent<PlayerKinematics>();
            Asserts.Assert(playerKinematics != null, "Could not find PlayerKinematics component in Player GameObject");
        }
    }

    void Initialize()
    {
        if (enemiesManager==null)
            enemiesManager = gameplayManager.GetComponent<OnTheRunEnemiesManager>();

        FindPlayerKinematics();

        //int playerLane = playerKinematics.CurrentLane;
        Transform playerTr = playerKinematics.transform;

        int enteringLane = 2; // Random.Range(0, 5);
        targetLane = (Random.value * 10000) % 100 > 50 ? 0: 4; // Random.Range(0, 5);
        float enteringX = 17.5f * 0.5f * OnTheRunEnvironment.lanes[enteringLane];
        float desiredX = 17.5f * 0.5f * OnTheRunEnvironment.lanes[targetLane];

        float lastLanePos = 17.5f * 0.5f * OnTheRunEnvironment.lanes[4];
        float firstLanePos = 17.5f * 0.5f * OnTheRunEnvironment.lanes[0];
        maxLaneDistance = Mathf.Abs(lastLanePos - firstLanePos);

        gameObject.transform.position = new Vector3(enteringX, 0.0f, playerTr.position.z) + new Vector3(0.0f, helicopterAltitude, -helicopterDistance);
        gameObject.transform.LookAt(playerTr);

        //desiredPos = playerTr.position + new Vector3(desiredX, helicopterAltitude, helicopterDistance);
        desiredPos = new Vector3(desiredX, helicopterAltitude, playerTr.position.z + helicopterDistance); //new Vector3(playerTr.position.x + desiredX, helicopterAltitude, playerTr.position.z + helicopterDistance);
        currentState = State.Entering;
        exitingTimer = 0.0f;
        propellerRotation = 0.0f;

        gameObject.SetActive(true);
        gameplayManager.IsHelicopterActive = true;

        float breakingNewsMomentDuration = 13.0f;

        //playerKinematics.gameObject.GetComponent<PlayerSounds>().SendMessage("OnPausePlayerEngine");
        OnTheRunSoundsManager.Instance.PlaySource(gameObject.GetComponent<AudioSource>());
        StartCoroutine(RestoreSounds(breakingNewsMomentDuration));
    }
    
    IEnumerator RestoreSounds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //playerKinematics.gameObject.GetComponent<PlayerSounds>().SendMessage("OnRestartPlayerEngine");
    }

    void FixedUpdate()
    {
        CheckPlayerKinematicsReference();

        if (playerKinematics == null)
            return;

        float dt = Time.fixedDeltaTime;

        bool checkSpecialCar = gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Tank || gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Firetruck || gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Ufo || gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Plane;
        if ((playerKinematics.PlayerIsStopping || checkSpecialCar || enemiesManager.isPoliceActive) && currentState == State.Following)// && waitToExitTimer < 0.0f)
        {
            gameObject.GetComponent<HelicopterAreaBehaviour>().StopShooting();
            waitToExitTimer = -1.0f;
            OnTheRunSounds.Instance.SendMessage("FadeOutReporterSpeech");
            OnTheRunSounds.Instance.StopReduceMusicVolume();
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("DestroyLiveNewsFlyer");
            StopCoroutine("SwitchToExitingStateCoroutine");
            currentState = State.Exiting;
        }

        switch (currentState)
        {
            case State.Inactive:
                return;

            case State.Entering:
                UpdateEnteringState(dt);
                break;

            case State.Following:
                UpdateFollowingState(dt);
                break;

            case State.Exiting:
                UpdateExitingState(dt);
                break;
        }

        SmoothLookAtPlayer(dt);
        UpdatePropellerGraphics(dt);
    }

    void CheckPlayerKinematicsReference()
    {
        if (playerKinematics != null)
            return;

        FindPlayerKinematics();
    }

    void SmoothLookAtPlayer(float deltaTime)
    {
        //gameObject.transform.LookAt(playerKinematics.transform.position);

        const float damping = 1.2f;

        const float xDeviationMultiplier = 4.0f;
        Vector3 up = Vector3.up + Vector3.right * smallSinXdeviation * xDeviationMultiplier;

        Quaternion rotation = Quaternion.LookRotation(playerKinematics.transform.position - transform.position, up);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, deltaTime * damping);
    }

    void UpdateEnteringState(float deltaTime)
    {
        desiredPos = new Vector3(desiredPos.x, desiredPos.y, playerKinematics.transform.position.z + helicopterDistance);
        
        Vector3 deltaToDesiredPos = desiredPos - transform.position;
        if (deltaToDesiredPos.magnitude <= 0.5f)
        {
            currentState = State.Following;
            //gameObject.GetComponent<HelicopterShootBehaviour>().StartShooting();
            gameObject.GetComponent<HelicopterAreaBehaviour>().StartShooting();
            StartCoroutine(SwitchToExitingStateCoroutine(followingDuration));
            return;
        }

        // speed = 2 * playerSpeed, when over player (slightly more)
        // speed = 0, when reached desired pos
        float speedFactor = playerKinematics.Speed * (1.1f + Mathf.Abs(deltaToDesiredPos.magnitude) / helicopterDistance);
        transform.position += Vector3.Normalize(deltaToDesiredPos) * speedFactor * deltaTime;
    }

    void UpdateFollowingState(float deltaTime)
    {
        // FOLLOWING ----------------------------------------------------------------------------------------------
        Rigidbody playerRb = playerKinematics.GetComponent<Rigidbody>();

        // FLOATING X ---------------------------------------------------------------------------------------------
        float targetX = 17.5f * 0.5f * OnTheRunEnvironment.lanes[targetLane];
        float distance = targetX - transform.position.x;

        float floatingXincrement = 0.0f;
        
        if (Mathf.Abs(distance) <= 0.5f)
        {
            floatingTimer += deltaTime;
            if (floatingTimer >= stayOnLaneTime)
            {
                floatingTimer = 0.0f;
                /*int newRandomLane = Random.Range(0, 5);
                if (newRandomLane == targetLane)
                    targetLane = 4 - newRandomLane;
                else
                    targetLane = newRandomLane;*/
            }
        }
        else
            floatingXincrement = floatingBaseSpeed * distance / maxLaneDistance * deltaTime;
        
        // FLOATING Y ---------------------------------------------------------------------------------------------
        floatingYangle += 2.0f * Mathf.PI * floatingYfrequency * deltaTime;
        float floatingYincrement = Mathf.Sin(floatingYangle) * 0.5f;

        // FLOATING X - small sin ---------------------------------------------------------------------------------
        //const float sinXdeviationMultiplier = 0.03f;
        //smallSinXdeviation = Mathf.Sin(floatingYangle) * sinXdeviationMultiplier;
        //floatingXincrement += smallSinXdeviation;

        smallSinXdeviation = floatingXincrement;

        float zPositionDelta = -(Mathf.Clamp(playerRb.position.y - 0.06f, 0.0f, 4.0f)) * deltaTime * 25.0f;
        
        // FINAL TRANSFORM POSITION -------------------------------------------------------------------------------
        desiredPos = new Vector3(desiredPos.x + floatingXincrement, desiredPos.y, playerRb.position.z + helicopterDistance);
        transform.position = desiredPos + Vector3.up * floatingYincrement + Vector3.forward * zPositionDelta;

        //EXITING -------------------
        if (waitToExitTimer >= 0.0f)
        {
            waitToExitTimer -= deltaTime;
            if (waitToExitTimer < 0.0f)
            {
                currentState = State.Exiting;
                gameObject.GetComponent<HelicopterAreaBehaviour>().StopShooting();
            }
        }
    }

    float floatingYangle = 0.0f;
    float floatingYfrequency = 0.5f;
    float smallSinXdeviation = 0.0f;

    /*
    void UpdateFloating(float deltaTime)
    {
        float targetX = 17.5f * 0.5f * OnTheRunEnvironment.lanes[desiredLane];
        float distance = targetX - transform.position.x;

        //if (currentLane == desiredLane)
        if (distance <= 0.05f)
        {
            floatingTimer += deltaTime;
            if (floatingTimer >= stayOnLaneTime)
            {
                floatingTimer = 0.0f;
                desiredLane = Random.Range(0, 5);
            }
        }
        else
        {
            float newX = transform.position.x + floatingBaseSpeed * Mathf.Sign(distance) * deltaTime;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }
    */

    void UpdateExitingState(float deltaTime)
    {
        exitingTimer += deltaTime;
        if (exitingTimer >= exitingDuration)
        {
            LevelRoot.Instance.Root.BroadcastMessage("RemoveHelicopter");
            gameObject.GetComponent<AudioSource>().Stop();
            exitingTimer = 0.0f;
            currentState = State.Inactive;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            gameObject.SetActive(false);
            gameplayManager.IsHelicopterActive = false;
            //gameObject.GetComponent<HelicopterShootBehaviour>().StopShooting();
            gameObject.GetComponent<HelicopterAreaBehaviour>().StopShooting();
            LevelRoot.Instance.Root.BroadcastMessage("RestartEnemiesManager", false);
        }
    }

    IEnumerator SwitchToExitingStateCoroutine(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);

        Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("DestroyLiveNewsFlyer");
        OnTheRunSounds.Instance.StopReduceMusicVolume();
        currentState = State.Exiting;
        //gameObject.GetComponent<HelicopterShootBehaviour>().StopShooting();
        gameObject.GetComponent<HelicopterAreaBehaviour>().StopShooting();
    }

    void UpdatePropellerGraphics(float deltaTime)
    {
        Material propellerMaterial = propellerGO.GetComponent<Renderer>().material;

        propellerRotation += deltaTime * propellerRotationSpeed;
        Quaternion rotation = Quaternion.Euler(0, 0, propellerRotation);
        Vector2 pivotPoint = new Vector2(0.5f, 0.5f);

        Matrix4x4 firstTranslationMtx = Matrix4x4.TRS(-pivotPoint, Quaternion.identity, Vector3.one);
        Matrix4x4 rotationMtx = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Matrix4x4 secondTranslationMtx = Matrix4x4.TRS(pivotPoint, Quaternion.identity, Vector3.one);

        Matrix4x4 transformationMatrix = secondTranslationMtx * rotationMtx * firstTranslationMtx;

        propellerMaterial.SetMatrix("_Transform", transformationMatrix);
    }
}