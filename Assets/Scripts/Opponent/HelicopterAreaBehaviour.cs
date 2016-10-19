using UnityEngine;
using System.Collections;
using SBS.Core;

public class HelicopterAreaBehaviour : MonoBehaviour
{
    protected OnTheRunEnvironment environmentManager;

    enum State
    {
        Aiming = 0,
        Shooting,
        Idle,
        Inactive
    }

    public float lightIdleDuration = 1.0f;
    public float idleDuration = 0.35f;
    public GameObject bullseyeTargetPrefab;
    public GameObject coneRef;
    public Material highlightMaterial;
    public Material normalMaterial;

    PlayerKinematics playerKinematics;
    GameObject target;

    State currentState = State.Inactive;
    Vector3 aimPos = Vector3.zero;
    float timer = 0.0f;
    float targetScale = 2.0f;

    bool isInsideArea = false;
    int coinsForTimeStep;
    float timerForCoins = -1.0f;
    float timeSingleCoins = 0.15f;

    void Awake()
    {
        environmentManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GetComponent<OnTheRunEnvironment>();
        target = GameObject.Instantiate(bullseyeTargetPrefab) as GameObject;
        //target.renderer.material.color = Color.yellow;
        target.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        target.GetComponent<Renderer>().enabled = false;
        target.GetComponent<HelicopterTargetBehaviour>().helicopterRef = gameObject;

        coinsForTimeStep = (int)OnTheRunDataLoader.Instance.GetHelicopterData()[0];
        timeSingleCoins = OnTheRunDataLoader.Instance.GetHelicopterData()[1];
    }

    void FindPlayerKinematics()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        Asserts.Assert(playerGO != null, "Could not find GameObject with tag \"Player\"");

        playerKinematics = playerGO.GetComponent<PlayerKinematics>();
        Asserts.Assert(playerKinematics != null, "Could not find PlayerKinematics component in Player GameObject");
    }

    void InitializeTarget()
    {
        int rndLane;
        int invalidLane = -1;
        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
            invalidLane = 2;
        do
        {
            rndLane = (int)Random.Range(0, 100000.0f) % 5;
        }
        while (rndLane == playerKinematics.CurrentLane || rndLane == invalidLane);

        aimPos = new Vector3(17.5f * 0.5f * OnTheRunEnvironment.lanes[rndLane], 0.0f, playerKinematics.transform.position.z) + Vector3.forward * playerKinematics.Speed;
        timer = lightIdleDuration;
    }

    public void StartShooting()
    {
        FindPlayerKinematics();

        currentState = State.Aiming;

        InitializeTarget();
        target.transform.position = aimPos;

        target.GetComponent<Renderer>().enabled = true;
        target.transform.position = new Vector3(aimPos.x, 0.1f, playerKinematics.transform.position.z);
        target.GetComponent<Renderer>().material = normalMaterial;
    }

    public void StopShooting()
    {
        currentState = State.Inactive;
        aimPos = Vector3.zero;
        timer = 0.0f;
        target.GetComponent<Renderer>().material = normalMaterial;
        target.GetComponent<Renderer>().enabled = false;
    }

    void FixedUpdate()
    {
        CheckPlayerKinematicsReference();

        float dt = Time.fixedDeltaTime;

        switch (currentState)
        {
            case State.Inactive:
                coneRef.SetActive(false);
                return;

            case State.Aiming:
                if(!coneRef.activeInHierarchy)
                    coneRef.SetActive(true);
                UpdateAimingState(dt);
                break;

            case State.Shooting:
                break;

            case State.Idle:
                UpdateIdleState(dt);
                break;
        }

    }

    void CheckPlayerKinematicsReference()
    {
        if (playerKinematics != null)
            return;

        FindPlayerKinematics();
    }

    void OnTargetEntered()
    {
        target.GetComponent<Renderer>().material = highlightMaterial;
        isInsideArea = true;
        timerForCoins = -1.0f;
        playerKinematics.IsOnAir = true;
    }

    void OnTargetExit()
    {
        target.GetComponent<Renderer>().material = normalMaterial;
        isInsideArea = false;
        timerForCoins = -1.0f;
        playerKinematics.IsOnAir = false;
    }

    void UpdateAimingState(float deltaTime)
    {
        aimPos = new Vector3(aimPos.x, 0.1f, playerKinematics.transform.position.z +1.0f);

        //target.transform.position = aimPos;
        float deltaPosX = target.transform.position.x - aimPos.x;
        float xPos = target.transform.position.x - 20.0f * deltaTime * Mathf.Sign(deltaPosX);
        if (Mathf.Abs(deltaPosX)<0.5f)
        {
            xPos = aimPos.x;
        }

        target.transform.position = new Vector3(xPos, aimPos.y, aimPos.z);

        timer -= deltaTime;
        if (timer < 0)
        {
            InitializeTarget();
        }

        if (isInsideArea)
        {
            timerForCoins -= deltaTime;

            if (timerForCoins < 0.0f)
            {
                timerForCoins = timeSingleCoins;
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.MoneyOnAir);
                Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(playerKinematics.PlayerRigidbody.position, coinsForTimeStep, true);
            }
            else
            {
                float scaleFactor = 2.0f + Mathf.Sin(timerForCoins) * 3.0f;
                target.transform.localScale = new Vector3(scaleFactor, 0.0f, scaleFactor);
            }
        }

        coneRef.transform.LookAt(target.transform.position + Vector3.up * 3.0f);
        coneRef.transform.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);
    }

    void UpdateIdleState(float deltaTime)
    {
        // test
        //aimPos = playerKinematics.transform.position;
        target.transform.position = aimPos;

        timer += deltaTime;
        if (timer >= idleDuration)
        {
            timer = 0.0f;
            currentState = State.Aiming;
            target.GetComponent<Renderer>().enabled = true;
            target.GetComponent<Renderer>().material = normalMaterial;
        }
    }
}