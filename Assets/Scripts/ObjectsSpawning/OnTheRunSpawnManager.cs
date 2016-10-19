using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;
using System.Collections;
using SBS.Core;

[AddComponentMenu("OnTheRun/Traffic")]
public class OnTheRunSpawnManager : MonoBehaviour
{
    #region Public Members
    public PlayerKinematics player = null;
    //public float startDistanceLane = 500.0f;
    //public float endDistanceLane = 3500.0f;
    [HideInInspector]
    public int checkpointDistance = 1000;
    public int objectDistanceForRemoving = 30;
    public float maxStartMinSpawn;
    public float maxStartMaxSpawn;
    public float maxEndMinSpawn;
    public float maxEndMaxSpawn;
    public float deltaStartMinSpawn;
    public float deltaStartMaxSpawn;
    public float deltaEndMinSpawn;
    public float deltaEndMaxSpawn;
    public List<SpawnableObjectClass> objectsToSpawn;
    #endregion

    #region Protected Members
    protected UIManager uiManager;
    protected GameObject cameraManager;
    protected OnTheRunGameplay gameplayManager;
    protected TrackObstaclesManager trackObstaclesManager;
    protected OnTheRunEnvironment environmentManager;
    protected List<GameObject> opponents = null;
    protected GameObject helicopterOpponent = null;
    protected SpawnableObjectClass spawnDelayed = null;
    protected List<SpawnableObjectClass> wantToSpawn = null;
    protected float[] freeLanes;
    protected bool started = false;
    protected float lastPositionSpawn = 0.0f;
    protected OpponentKinematics lastOpponentSpawn = null;
    protected List<float> nextTruckSpawnPosition;
    protected int nextTruckLane = -1;
    protected bool truckIsComing = false;
    protected float nextCheckpointPosition = -1.0f;
    protected bool moreTrucks = false;

    //boosters----
    protected float turboTrafficMultiplier = 1.0f;
    protected float truckMultiplier = 1.0f;
    protected float helicopterMultiplier = 1.0f;
    protected float currentTruckMultiplier = 1.0f;
    protected float backupTruckMultiplier = 1.0f;

    protected int vehicleTrafficIndex;
    protected float[] backupTrafficValues;
    protected float min_startMinSpawn;
    protected float min_startMaxSpawn;
    protected float min_endMinSpawn;
    protected float min_endMaxSpawn;

    protected int isTruckOnScreen = 0;

    [HideInInspector]
    public bool firstSpecialVehicleInSession = true;


    protected OnTheRunEnvironment.TrafficDirectionConfiguration backupLanesConfig;
    #endregion

    #region Public Properties
    public float TurboTrafficMultiplier
    {
        get { return turboTrafficMultiplier; }
        set { turboTrafficMultiplier = value; }
    }

    public List<GameObject> ActiveOpponents
    {
        get { return opponents; }
    }
    
    public int TruckLane
    {
        get { return nextTruckLane; }
    }

    public float NextCheckpointPosition
    {
        get { return nextCheckpointPosition; }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        checkpointDistance = OnTheRunDataLoader.Instance.GetCheckpointDistance();
        UpdateTrafficDataByPlayerLevel();
        /*maxStartMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_start_min_spawn_distance");
        maxStartMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_start_max_spawn_distance");
        maxEndMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_end_min_spawn_distance");
        maxEndMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_end_max_spawn_distance");
        deltaStartMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_start_min_spawn_distance");
        deltaStartMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_start_max_spawn_distance");
        deltaEndMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_end_min_spawn_distance");
        deltaEndMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_end_max_spawn_distance");*/

        opponents = new List<GameObject>();
        wantToSpawn = new List<SpawnableObjectClass>();
        nextTruckSpawnPosition = new List<float>();
        availableSpecialVehicles = new List<TruckBehaviour.TrasformType>();

        /*for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            if (objectsToSpawn[i].type == OnTheRunObjectsPool.ObjectType.Checkpoint)
            {
                objectsToSpawn[i].startMinSpawnDistance = checkpointDistance;
                objectsToSpawn[i].startMaxSpawnDistance = checkpointDistance;
                objectsToSpawn[i].endMinSpawnDistance = checkpointDistance;
                objectsToSpawn[i].endMaxSpawnDistance = checkpointDistance;
            }
            else if (objectsToSpawn[i].type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
            {
                objectsToSpawn[i].startMinSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "start_min_spawn_distance");
                objectsToSpawn[i].startMaxSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "start_max_spawn_distance");
                objectsToSpawn[i].endMinSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "end_min_spawn_distance");
                objectsToSpawn[i].endMaxSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "end_max_spawn_distance");

                vehicleTrafficIndex = i;
                backupTrafficValues = new float[4];
                backupTrafficValues[0] = objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance;
                backupTrafficValues[1] = objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance;
                backupTrafficValues[2] = objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance;
                backupTrafficValues[3] = objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance; 
                min_startMinSpawn = objectsToSpawn[i].startMinSpawnDistance;
                min_startMaxSpawn = objectsToSpawn[i].startMaxSpawnDistance;
                min_endMinSpawn = objectsToSpawn[i].endMinSpawnDistance;
                min_endMaxSpawn = objectsToSpawn[i].endMaxSpawnDistance; 
            }
        }*/

        freeLanes = new float[5] { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f };
    }

    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
        trackObstaclesManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<TrackObstaclesManager>();
        uiManager = Manager<UIManager>.Get();
        ResetAvailableSpecialVehicles();
    }

    void UpdateTrafficDataByPlayerLevel()
    {
        maxStartMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_start_min_spawn_distance");
        maxStartMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_start_max_spawn_distance");
        maxEndMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_end_min_spawn_distance");
        maxEndMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("limit", "max_end_max_spawn_distance");
        deltaStartMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_start_min_spawn_distance");
        deltaStartMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_start_max_spawn_distance");
        deltaEndMinSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_end_min_spawn_distance");
        deltaEndMaxSpawn = OnTheRunDataLoader.Instance.GetTrafficData("increment", "delta_end_max_spawn_distance");

        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            if (objectsToSpawn[i].type == OnTheRunObjectsPool.ObjectType.Checkpoint)
            {
                objectsToSpawn[i].startMinSpawnDistance = checkpointDistance;
                objectsToSpawn[i].startMaxSpawnDistance = checkpointDistance;
                objectsToSpawn[i].endMinSpawnDistance = checkpointDistance;
                objectsToSpawn[i].endMaxSpawnDistance = checkpointDistance;
            }
            else if(IsABonus(objectsToSpawn[i].type))
            {
                objectsToSpawn[i].startMinSpawnDistance = OnTheRunDataLoader.Instance.GetPowerUpsData(objectsToSpawn[i].type.ToString(), "start_min_spawn_distance");
                objectsToSpawn[i].startMaxSpawnDistance = OnTheRunDataLoader.Instance.GetPowerUpsData(objectsToSpawn[i].type.ToString(), "start_max_spawn_distance");
                objectsToSpawn[i].endMinSpawnDistance = OnTheRunDataLoader.Instance.GetPowerUpsData(objectsToSpawn[i].type.ToString(), "end_min_spawn_distance");
                objectsToSpawn[i].endMaxSpawnDistance = OnTheRunDataLoader.Instance.GetPowerUpsData(objectsToSpawn[i].type.ToString(), "end_max_spawn_distance");
            }
            else if (objectsToSpawn[i].type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
            {
                objectsToSpawn[i].startMinSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "start_min_spawn_distance");
                objectsToSpawn[i].startMaxSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "start_max_spawn_distance");
                objectsToSpawn[i].endMinSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "end_min_spawn_distance");
                objectsToSpawn[i].endMaxSpawnDistance = OnTheRunDataLoader.Instance.GetTrafficData("init", "end_max_spawn_distance");

                vehicleTrafficIndex = i;
                backupTrafficValues = new float[4];
                backupTrafficValues[0] = objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance;
                backupTrafficValues[1] = objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance;
                backupTrafficValues[2] = objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance;
                backupTrafficValues[3] = objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance;
                min_startMinSpawn = objectsToSpawn[i].startMinSpawnDistance;
                min_startMaxSpawn = objectsToSpawn[i].startMaxSpawnDistance;
                min_endMinSpawn = objectsToSpawn[i].endMinSpawnDistance;
                min_endMaxSpawn = objectsToSpawn[i].endMaxSpawnDistance;
            }
        }
    }

    public bool IsABonus(OnTheRunObjectsPool.ObjectType checkType)
    {
        return checkType == OnTheRunObjectsPool.ObjectType.BonusTurbo || checkType == OnTheRunObjectsPool.ObjectType.BonusTime || checkType == OnTheRunObjectsPool.ObjectType.BonusMoney || checkType == OnTheRunObjectsPool.ObjectType.BonusMoneyBig || checkType == OnTheRunObjectsPool.ObjectType.BonusMagnet || checkType == OnTheRunObjectsPool.ObjectType.BonusShield;
    }

    void RaceStarted()
    {
        firstSpecialVehicleInSession = true;

        UpdateTrafficDataByPlayerLevel();

        lastPositionSpawn = 80.0f;
        lastOpponentSpawn = null;
        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            bool isTruck = OnTheRunObjectsPool.Instance.IsATruck(objectsToSpawn[i].type);
            if (objectsToSpawn[i].type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
            {
                objectsToSpawn[i].startMinSpawnDistance = backupTrafficValues[0];
                objectsToSpawn[i].startMaxSpawnDistance = backupTrafficValues[1];
                objectsToSpawn[i].endMinSpawnDistance = backupTrafficValues[2];
                objectsToSpawn[i].endMaxSpawnDistance = backupTrafficValues[3];
                objectsToSpawn[i].NextSpawnObjectDistance = -1.0f;
            }
            else if (isTruck)
            {
                objectsToSpawn[i].NextSpawnObjectDistance = -1.0f;
            }
        }
    }

    public void SpawnTrafficRandomly(float startDistance, int quantity, int distanceBetween)
    {
        Vector3 startPositionOffset = Vector3.forward * startDistance;
        for (int i = 0; i < quantity; ++i)
        {
            SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TrafficVehicle, 0, 0, 0, 0, 0), 1, 0.0f, -1, startPositionOffset + Vector3.forward * distanceBetween * i);
        }
    }

    public IntrusiveList<OpponentKinematics> __opponents = new IntrusiveList<OpponentKinematics>();
    public IntrusiveList<BonusBehaviour> __bonuses = new IntrusiveList<BonusBehaviour>();
    void FixedUpdate()
    {
        var opp = __opponents.Head;
        while (opp != null)
        {
            opp.MyFixedUpdate();
            opp = opp.next;
        }
        var bon = __bonuses.Head;
        while (bon != null)
        {
            bon.MyFixedUpdate();
            bon = bon.next;
        }

        float currentPlayerDistance = player.Distance; //player.PlayerRigidbody.position.z

        var tutMng = OnTheRunTutorialManager.Instance;
        if (tutMng.TutorialActive && uiManager.ActivePageName.Equals("IngamePage"))
        {
            tutMng.UpdateTutorial(TimeManager.Instance.MasterSource.DeltaTime);

            int tutorialIndex = tutMng.NextTutorialIndex;
            if (tutorialIndex>=0)
            {
                tutMng.ResetTutorialTimer();
                switch (tutorialIndex)
                {
                    case 3:
                        OpponentKinematics oppKin = SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTurbo, 0, 0, 0, 0, 0), 1, 0.0f, 2, Vector3.back * 5.0f);
                        tutMng.AddTutorialCar(oppKin);

                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTurbo, 0, 0, 0, 0, 0), 1, 0.0f, 0, Vector3.back * 5.0f);
                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTransform, 0, 0, 0, 0, 0), 1, 0.0f, 1, Vector3.back * 5.0f);
                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTransform, 0, 0, 0, 0, 0), 1, 0.0f, 3, Vector3.back * 5.0f);
                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTurbo, 0, 0, 0, 0, 0), 1, 0.0f, 4, Vector3.back * 5.0f);
                        break;
                }
            }
        }

        if (tutMng.AvoidSpawnTraffic)
            return;

        if (!tutMng.TutorialActive)
        {
            if (!player.PlayerIsStopping && !gameplayManager.Gameplaystarted)
                return;
        }
        else
            currentPlayerDistance = player.gameObject.transform.position.z;

        if (!tutMng.TutorialActive)
        {
            truckIsComing = nextTruckSpawnPosition.Count > 0 && (nextTruckSpawnPosition[0] - currentPlayerDistance) < 80.0f && (nextTruckSpawnPosition[0] - currentPlayerDistance) > 0.0f;
            if (truckIsComing && nextTruckLane < 0)
                nextTruckLane = ChooseFreeLane(false, true);
        }

        //Loop checking all the objects wants to be spawned at the same time
		for (int i = 0, c = objectsToSpawn.Count; i < c; ++i)
        {
            SpawnableObjectClass currObj = objectsToSpawn[i];

            if (tutMng.TutorialActive && currObj.type != OnTheRunObjectsPool.ObjectType.TrafficVehicle)
                continue;

            bool specialEventsCase = currObj.avoidSpecialEvents && (player.TurboOn || gameplayManager.IsSpecialCarActive);
            if (specialEventsCase)
                continue;

            if (started && (currObj.NextSpawnObjectDistance == -1.0f || currentPlayerDistance >= currObj.NextSpawnObjectDistance))
            {
                float ratio = 1.0f; // - Mathf.Clamp01((player.PlayerRigidbody.position.z - startDistanceLane) / (endDistanceLane - startDistanceLane));

                float minDistance = currObj.startMinSpawnDistance + ratio * (currObj.endMinSpawnDistance - currObj.startMinSpawnDistance),
                    maxDistance = currObj.startMaxSpawnDistance + ratio * (currObj.endMaxSpawnDistance - currObj.startMaxSpawnDistance);
                
                //more traffic if in turbo
                if (currObj.type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
                {
                    minDistance *= turboTrafficMultiplier; // 0.002f;
                    maxDistance *= turboTrafficMultiplier; //0.002f;
                }

                //more trucks if booster equipped
                bool isTruck = OnTheRunObjectsPool.Instance.IsATruck(currObj.type);
                if (isTruck && truckMultiplier != 1.0f && helicopterMultiplier==1.0f)
                {
                    minDistance *= truckMultiplier;
                    maxDistance *= truckMultiplier;
                }

                if (currObj.type == OnTheRunObjectsPool.ObjectType.TruckTurbo && helicopterMultiplier != 1.0f)
                {
                    minDistance *= helicopterMultiplier;
                    maxDistance *= helicopterMultiplier;
                }


                //Special cases to avoid
                bool noBonusTurbo = (player.TurboOn) && currObj.type == OnTheRunObjectsPool.ObjectType.BonusTurbo; //|| gameplayManager.IsSpecialCarActive || gameplayManager.IsHelicopterActive || gameplayManager.IsBadGuyActive 
                bool noTrucks = (player.TurboOn || gameplayManager.IsSpecialCarActive) && OnTheRunObjectsPool.Instance.IsATruck(currObj.type); // || gameplayManager.IsBadGuyActive
                
                if (currObj.NextSpawnObjectDistance != -1.0f)
                {
                    if (!noBonusTurbo && !noTrucks)
                    {
                        wantToSpawn.Add(currObj);
                    }
                }

                if (currObj.type == OnTheRunObjectsPool.ObjectType.Checkpoint && currObj.NextSpawnObjectDistance == -1)
                {
                    currObj.NextSpawnObjectDistance = currentPlayerDistance + UnityEngine.Random.Range(minDistance, minDistance) - 200.0f; //player.PlayerRigidbody.position.z + UnityEngine.Random.Range(minDistance, minDistance) - 200.0f;
                }
                else
                    currObj.NextSpawnObjectDistance += UnityEngine.Random.Range(minDistance, minDistance); //player.PlayerRigidbody.position.z + UnityEngine.Random.Range(minDistance, minDistance);
                
                if (isTruck)
                {
                    nextTruckSpawnPosition.Add(currObj.NextSpawnObjectDistance);
                }

                if (currObj.type == OnTheRunObjectsPool.ObjectType.Checkpoint)
                    nextCheckpointPosition = currObj.NextSpawnObjectDistance;

            }
        }

        //Check for higher priority object
        bool canSpawn = (currentPlayerDistance - lastPositionSpawn) > 15.0f;
        if (lastOpponentSpawn != null && canSpawn)
            canSpawn = (lastOpponentSpawn.OpponentRigidbody.position.z - player.PlayerRigidbody.position.z) < 195.0f;

        bool alwaysSpawnFound = false;
        int wantToSpawnCount = wantToSpawn.Count;
        int spawnCheckpointIndex = -1;
        if (wantToSpawnCount > 0)
        {
            int higherPriority = 999;
            int higherPriorityIdx = -1;
            for (int i = 0; i < wantToSpawnCount; ++i)
            {
                if (wantToSpawn[i].type == OnTheRunObjectsPool.ObjectType.Checkpoint)
                {
                    spawnCheckpointIndex = i;
                    continue;
                }

                if (wantToSpawn[i].alwaysSpawn)
                {
                    alwaysSpawnFound = true;
                    if (canSpawn)
                    {
                        canSpawn = true;
                        higherPriority = wantToSpawn[i].priority;
                        higherPriorityIdx = i;
                    }
                    else
                        spawnDelayed = wantToSpawn[i];
                }
                else if (wantToSpawn[i].priority < higherPriority && canSpawn && !alwaysSpawnFound)
                {
                    higherPriority = wantToSpawn[i].priority;
                    higherPriorityIdx = i;
                }
            }

            if (higherPriorityIdx >= 0 && spawnDelayed==null)
            {
                int quantity = wantToSpawn[higherPriorityIdx].type == OnTheRunObjectsPool.ObjectType.BonusMoney ? 5 : 1;
                float distanceBetween = wantToSpawn[higherPriorityIdx].type == OnTheRunObjectsPool.ObjectType.BonusMoney ? 5.0f : 0.0f;
                lastOpponentSpawn = SpawnObject(wantToSpawn[higherPriorityIdx], quantity, distanceBetween);
                lastPositionSpawn = currentPlayerDistance;
            }

            if (spawnCheckpointIndex >= 0)
            { 
                SpawnObject(wantToSpawn[spawnCheckpointIndex], 1, 0.0f);
                spawnCheckpointIndex = -1;
            }

            wantToSpawn.Clear();

            if (spawnDelayed != null)
            {
                wantToSpawn.Add(spawnDelayed);
                spawnDelayed = null;
            }
        }
    }

    void ForceSpawn()
    {
        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TruckTransform, 0, 0, 0, 0, 0), 1, 0.0f);
        lastPositionSpawn = player.PlayerRigidbody.position.z;
    }

    protected float updateTimer = 0.1f;

    protected Rigidbody oppRb;
    protected OnTheRunObjectsPool.ObjectType oppType;
    protected PoliceBehaviour oppPol;
    protected OpponentKinematics oppKin;
    protected int carCounter = 0;
    protected int carsEvaluatedPerFrame = 4;
    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        OnTheRunTutorialManager.Instance.UpdateSkippableTutorialTimer();

        updateTimer -= dt;
        if (updateTimer < 0.0f)
        {
            updateTimer = 0.01f;
            //Delete objects too far
            float playerZ = player.PlayerRigidbody.position.z;
            int initIndex = carCounter;
            int endIndex = Mathf.Clamp(initIndex + carsEvaluatedPerFrame, 0, opponents.Count - 1);

            carCounter = endIndex == opponents.Count - 1 ? 0 : endIndex;

            //for (int i = opponents.Count - 1; i >= 0; --i)
            for (int i = initIndex; i < endIndex; ++i)
            {
                if (i < 0 || i >= opponents.Count)
                    continue;

                GameObject opponent = opponents[i];
                oppRb = opponent.GetComponent<Rigidbody>();
                oppType = opponent.GetComponent<SpawnableObjectType>().type;
                oppPol = opponent.GetComponent<PoliceBehaviour>();
                oppKin = opponent.GetComponent<OpponentKinematics>();
                if (opponent != null)
                {
                    //used to remove bonus turbo already spawned during turbo
                    bool disableDuringTurbo = player.TurboOn && oppType == OnTheRunObjectsPool.ObjectType.BonusTurbo;

                    //used to remove bonus turbo already spawned during turbo
                    //bool disableDuringBadGuys = (gameplayManager.IsBadGuyActive || gameplayManager.IsHelicopterActive) && opponent.GetComponent<SpawnableObjectType>().type == OnTheRunObjectsPool.ObjectType.BonusTurbo;

                    //used to remove bonus turbo and trucks already spawned
                    //bool disableDuringSpecialCar = gameplayManager.IsSpecialCarActive && (opponent.GetComponent<SpawnableObjectType>().type == OnTheRunObjectsPool.ObjectType.BonusTurbo); //|| OnTheRunObjectsPool.Instance.IsATruck(opponent.GetComponent<SpawnableObjectType>().type) 

                    //used to remove objects too far
                    bool disableTooFar = false;
                    bool disableTooFarForward = false;
                    if (oppPol == null)
                    {
                        if (oppType == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
                        {
                            disableTooFar = (playerZ - oppRb.position.z > 8.0f || oppRb.position.y > 10.0f);
                            //disableTooFar = (cameraManager.transform.position.z - opponent.rigidbody.position.z > 3.0f || opponent.rigidbody.position.y > 10.0f);

                            if (oppKin.MinDistanceFromFromPlayer > 0.0f && player.Distance > 200.0f)
                            {
                                disableTooFarForward = oppKin.CurrentDistanceFromFromPlayer >= 250.0f; //oppKin.MinDistanceFromFromPlayer < 190.0f && oppKin.CurrentDistanceFromFromPlayer >= 190.0f;
                            }
                        }
                        else if (oppRb != null)
                            disableTooFar = (playerZ - oppRb.position.z > objectDistanceForRemoving || oppRb.position.y > 10.0f);
                    }

                    //used to remove bad guys
                    bool removeBadGuy = oppPol != null && (oppRb.position.y > 10.0f || oppPol.CanBeDestroyed); //gameplayManager.IsBadGuyActive && && opponent.GetComponent<PoliceBehaviour>().CanBeDestroyed;
                    //bool removeBonusByPlayerSpeed = OnTheRunObjectsPool.Instance.IsABonus(opponent.GetComponent<SpawnableObjectType>().type) && player.PlayerIsStopping && player.Speed < 2.0f;

                    if (disableTooFar || disableDuringTurbo || removeBadGuy || disableTooFarForward) //|| disableDuringSpecialCar || removeBonusByPlayerSpeed) //|| disableDuringBadGuys)
                    {
                        if (removeBadGuy)
                            oppPol.SendMessage("DectivatePolice");

                        OnTheRunObjectsPool.Instance.NotifyDestroyingExplosion(opponent);
                        OnTheRunObjectsPool.Instance.NotifyDestroyingParent(opponent, oppType);

                        opponents.RemoveAt(i);
                    }
                }
            }
        }

        //Manage free lanes
        int freeLanesLength = freeLanes.Length;
        for (int i = 0; i < freeLanesLength; ++i)
        {
            if (freeLanes[i] >= 0.0f)
                freeLanes[i] -= dt;
        }
    }

    protected GameObject lastObjectSpawned;
    protected float lastObjectSpawnedInitPos;
    OpponentKinematics SpawnObject(SpawnableObjectClass currObj, int quantity = 0, float distanceBetween = 0.0f, int forceLane=-1, Vector3 positionOffset=default(Vector3))
    {
        GameObject instance = null;

        OnTheRunObjectsPool.ObjectType objType = currObj.type;
        int laneChoosen = forceLane < 0 ? ChooseFreeLane() : forceLane;

        if (forceLane < 0)
        {
            if ((!OnTheRunObjectsPool.Instance.IsATruck(objType) && laneChoosen < 0) || (OnTheRunObjectsPool.Instance.IsATruck(objType) && nextTruckLane < 0)) // laneChoosen < 0 || 
                return null;
        }

        float initPosition = player.PlayerRigidbody.position.z;
        for (int i = 0; i < quantity; ++i)
        {
            //Sometimes the last bonus money i a big onre
            if (objType==OnTheRunObjectsPool.ObjectType.BonusMoney && !OnTheRunTutorialManager.Instance.TutorialActive)
            {
                bool lastMoneyBig = UnityEngine.Random.Range(0, 100) > 50;
                bool isLastMoney = (i == quantity - 1);
                if (objType == OnTheRunObjectsPool.ObjectType.BonusMoney && lastMoneyBig && isLastMoney)
                    objType = OnTheRunObjectsPool.ObjectType.BonusMoneyBig;

                if (gameplayManager.IsInWrongDirection(laneChoosen))
                    objType = OnTheRunObjectsPool.ObjectType.BonusMoneyBig;
            }

            instance = OnTheRunObjectsPool.Instance.RequestObject(objType);
            instance.transform.position = new Vector3(0.0f, 0.0f, initPosition + 200.0f + i * distanceBetween) + positionOffset; //new Vector3(player.rigidbody.position.x, 0.0f, player.rigidbody.position.z + 200.0f);
            
            instance.transform.rotation = Quaternion.identity;

            if (OnTheRunObjectsPool.Instance.IsABonus(objType))
                instance.transform.position += Vector3.forward * 5.0f;

            lastObjectSpawned = instance;
            lastObjectSpawnedInitPos = initPosition;

            if (instance.GetComponent<SpawnableObjectType>() == null)
                instance.AddComponent<SpawnableObjectType>();
            instance.GetComponent<SpawnableObjectType>().type = objType;

            OpponentKinematics opponentKinematics = instance.GetComponent<OpponentKinematics>();
            if (objType != OnTheRunObjectsPool.ObjectType.Checkpoint && opponentKinematics != null)
            {
                if (OnTheRunObjectsPool.Instance.IsATruck(objType))
                {
                    ++IsTruckOnScreen;
                    if (moreTrucks)
                        opponentKinematics.CurrentLane = ChooseFreeLane( );
                    else
                        opponentKinematics.CurrentLane = nextTruckLane;
                }
                else
                    opponentKinematics.CurrentLane = laneChoosen;

                if (OnTheRunTutorialManager.Instance.TutorialActive && forceLane!=-1)
                    opponentKinematics.CurrentLane = forceLane;

                opponentKinematics.gameObject.SendMessage("OnReset");
            }

            if (OnTheRunObjectsPool.Instance.IsABonus(objType))
            {
                if (objType == OnTheRunObjectsPool.ObjectType.BonusMoney || objType == OnTheRunObjectsPool.ObjectType.BonusMoneyBig)
                    freeLanes[laneChoosen] = 2.0f;
                instance.gameObject.SendMessage("ResetBonus", laneChoosen);
            }

            if (OnTheRunObjectsPool.Instance.IsATruck(objType))
            {
                opponentKinematics.IsTruck = true;
                if (!OnTheRunTutorialManager.Instance.TutorialActive)
                {
                    nextTruckSpawnPosition.RemoveAt(0);
                    if (nextTruckSpawnPosition.Count == 0)
                        nextTruckLane = -1;
                    else
                        nextTruckLane = ChooseFreeLane(false, true);
                }
            }

            opponents.Add(instance);
        }

        return instance.GetComponent<OpponentKinematics>();
    }
    
    void SpawnSingleObstacle(TrackObstaclesManager.SpawnObstacleData data)
    {
        GameObject instance = OnTheRunObjectsPool.Instance.RequestObject(data.obstaclesType);
        float zOffset = data.zOffset;
        instance.transform.position = new Vector3(17.5f * 0.5f * OnTheRunEnvironment.lanes[data.selectedLane], 0.0f, player.PlayerRigidbody.position.z + 200.0f + zOffset);
        float rotationAdditionRnd = 0.0f;
        if (data.obstaclesType==OnTheRunObjectsPool.ObjectType.RoadWork)
            rotationAdditionRnd = UnityEngine.Random.Range(-40.0f, 40.0f);
        else if (data.obstaclesType == OnTheRunObjectsPool.ObjectType.StartGuardRail)
            rotationAdditionRnd = -180.0f;
        instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f + rotationAdditionRnd, 0.0f);
        freeLanes[data.selectedLane] = 2.0f;
        if (instance.GetComponent<SpawnableObjectType>() == null)
            instance.AddComponent<SpawnableObjectType>();
        instance.GetComponent<SpawnableObjectType>().type = data.obstaclesType;

        if (data.obstaclesType == OnTheRunObjectsPool.ObjectType.BonusMoneyBig)
        {
            instance.GetComponent<BonusBehaviour>().ResetBonus(data.selectedLane);
            instance.GetComponent<BonusBehaviour>().IsInRoadWorks = true;
        }
        opponents.Add(instance);
    }

    IEnumerator ActivateBlockRoad(int selectedLane)
    {
        freeLanes[selectedLane] = 6.0f;

        yield return new WaitForSeconds(4.0f);

        GameObject instance = OnTheRunObjectsPool.Instance.RequestObject(OnTheRunObjectsPool.ObjectType.RoadBlock);
        instance.transform.position = new Vector3(17.5f * 0.5f * OnTheRunEnvironment.lanes[selectedLane], 0.0f, player.PlayerRigidbody.position.z + 200.0f);
        if(selectedLane<=2)
            instance.transform.rotation = Quaternion.Euler(0.0f, 120.0f, 0.0f);
        else
            instance.transform.rotation = Quaternion.Euler(0.0f, 220.0f, 0.0f);
        if (instance.GetComponent<SpawnableObjectType>() == null)
            instance.AddComponent<SpawnableObjectType>();
        instance.GetComponent<SpawnableObjectType>().type = OnTheRunObjectsPool.ObjectType.RoadBlock;
        opponents.Add(instance);

        AvoidLaneForRoadWork(selectedLane);
    }

    void AvoidLaneForRoadWork(int selectedLane)
    {
        //Traffic avoid lane
        for (int j = 0; j < opponents.Count; ++j)
        {
            GameObject opponent = opponents[j];
            OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
            if (type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
            {
                OpponentKinematics opponentKin = opponent.GetComponent<OpponentKinematics>();
                if (opponentKin.CurrentLane == selectedLane)
                {
                    opponentKin.SendMessage("ChangeLaneForRoadWorks");
                }
            }
        }
    }

    void ActivateNextPoliceCar(int lastPoliceIndex)
    {
        //Traffic avoid lane
        for (int j = 0; j < opponents.Count; ++j)
        {
            GameObject opponent = opponents[j];
            OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
            if (type == OnTheRunObjectsPool.ObjectType.Police)
            {
                PoliceBehaviour opponentPolice = opponent.GetComponent<PoliceBehaviour>();
                if (Mathf.Abs(opponentPolice.MyTurn-lastPoliceIndex)==1)
                {
                    opponentPolice.SendMessage("ActivatePolice");
                }
            }
        }
    }

    protected int ChooseFreeLane(bool isBadGuy = false, bool isTruck = false)
    {
        int laneChoosen = -1,
            minLane = 0,
            maxLane = 4;
        if (isTruck)
        {
            if (environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.ForwardBackward)
            {
                minLane = 0;
                maxLane = 2;
            }
            else if (environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward)
            {
                minLane = 3;
                maxLane = 4;
            }
        }

        if (isBadGuy)
        {
            do
            {
                laneChoosen = UnityEngine.Random.Range(minLane, maxLane + 1);
            } while (freeLanes[laneChoosen] >= 0.0f || !LaneIsFree(laneChoosen) || laneChoosen==player.CurrentLane); // || gameplayManager.IsInWrongDirection(laneChoosen)
        }
        else
        {
            do
            {
                laneChoosen = UnityEngine.Random.Range(minLane, maxLane + 1);
            } while (freeLanes[laneChoosen] >= 0.0f || !LaneIsFree(laneChoosen) || TruckWillSpawnShortlyHere(laneChoosen));
        }
        return laneChoosen;
    }

    public bool LaneIsFree(int index)
    {
        bool truckLane = truckIsComing && index == nextTruckLane;//&& nextTruckLane >= 0 && truckIsComing && index == nextTruckLane;
        bool roadWorksLaneLane = trackObstaclesManager.RoadWorksLane == index;
        bool avoidCentralLane = (environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLane || environmentManager.currentTrafficDirection == OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward) && index == 2;
        return !truckLane && !roadWorksLaneLane && !avoidCentralLane;
    }

    public bool TruckWillSpawnShortlyHere(int index)
    {
        float currentPlayerDistance = player.Distance;
        bool truckIsComing = nextTruckSpawnPosition.Count > 0 && (nextTruckSpawnPosition[0] - currentPlayerDistance) < 150.0f && (nextTruckSpawnPosition[0] - currentPlayerDistance) > 0.0f;
        return truckIsComing && nextTruckLane == index;
    }

    public bool TruckWillSpawnShortly( )
    {
        float currentPlayerDistance = player.Distance;
        bool truckIsComing = nextTruckSpawnPosition.Count > 0 && (nextTruckSpawnPosition[0] - currentPlayerDistance) < 150.0f && (nextTruckSpawnPosition[0] - currentPlayerDistance) > 0.0f;
        return truckIsComing;
    }
    #endregion

    #region Functions
    protected TruckBehaviour.TrasformType moreSpecialVehicle = TruckBehaviour.TrasformType.None;
    public void MoreSpecialVehiclesActivated(TruckBehaviour.TrasformType carId)
    {
        moreSpecialVehicle = carId;
    }

    public void ResetSpecialVehiclesActivated( )
    {
        moreSpecialVehicle = TruckBehaviour.TrasformType.None;
    }

    public void MoreTrucksActivated(float multiplier)
    {
        if (!moreTrucks)
        {
            currentTruckMultiplier = multiplier;
            nextTruckLane = ChooseFreeLane(false ,true);
            SpawnableObjectClass currObj = null;
            for (int i = 0; i < objectsToSpawn.Count; ++i)
            {
                currObj = objectsToSpawn[i];
                if (currObj.type == OnTheRunObjectsPool.ObjectType.TruckTurbo)
                {
                    float minDistance = currObj.startMinSpawnDistance + (currObj.endMinSpawnDistance - currObj.startMinSpawnDistance),
                          maxDistance = currObj.startMaxSpawnDistance + (currObj.endMaxSpawnDistance - currObj.startMaxSpawnDistance);
                    minDistance *= currentTruckMultiplier;
                    maxDistance *= currentTruckMultiplier;
                    currObj.startMinSpawnDistance *= currentTruckMultiplier;
                    currObj.endMinSpawnDistance *= currentTruckMultiplier;
                    currObj.NextSpawnObjectDistance = player.PlayerRigidbody.position.z + UnityEngine.Random.Range(minDistance, minDistance);
                }
            }

            for (int i = 0; i < nextTruckSpawnPosition.Count; ++i)
            {
                nextTruckSpawnPosition[i] *= currentTruckMultiplier;
            }

            backupTruckMultiplier = helicopterMultiplier;
            helicopterMultiplier = currentTruckMultiplier * 10.0f;
            moreTrucks = true;
        }
    }

    public void MoreTrucksDectivated()
    {
        SpawnableObjectClass currObj = null;
        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            currObj = objectsToSpawn[i];
            if (currObj.type == OnTheRunObjectsPool.ObjectType.TruckTurbo)
            {
                float minDistance = currObj.startMinSpawnDistance + (currObj.endMinSpawnDistance - currObj.startMinSpawnDistance),
                      maxDistance = currObj.startMaxSpawnDistance + (currObj.endMaxSpawnDistance - currObj.startMaxSpawnDistance);
                minDistance /= currentTruckMultiplier;
                maxDistance /= currentTruckMultiplier;
                currObj.startMinSpawnDistance /= currentTruckMultiplier;
                currObj.endMinSpawnDistance /= currentTruckMultiplier;
                currObj.NextSpawnObjectDistance = player.PlayerRigidbody.position.z + UnityEngine.Random.Range(minDistance, minDistance);
            }
        }

        for (int i = 0; i < nextTruckSpawnPosition.Count; ++i)
        {
            nextTruckSpawnPosition[i] /= currentTruckMultiplier;
        }

        moreTrucks = false;
        helicopterMultiplier = backupTruckMultiplier;
        //nextTruckLane = -1;
    }

    public int IsTruckOnScreen
    {
        set { isTruckOnScreen = value; }
        get { return isTruckOnScreen; }
    }

    void TruckRemoved()
    {
        --isTruckOnScreen;
    }

    protected float initialPlayerDistance;
    void SaveDistancesForSpecialCar(float playerCurrentDistance)
    {
        initialPlayerDistance = playerCurrentDistance; // player.rigidbody.position.z;

        //for test
        /*SpawnableObjectClass currObj = null;
        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            currObj = objectsToSpawn[i];
            if (OnTheRunObjectsPool.Instance.IsATruck(currObj.type))
            {
                Debug.Log("SaveDistancesForSpecialCar: " + currObj.type + " " + currObj.NextSpawnObjectDistance);
            }
        }*/
    }

    void RestoreDistancesForSpecialCar(float playerCurrentDistance)
    {
        float metersRunned = playerCurrentDistance - initialPlayerDistance;

        SpawnableObjectClass currObj = null;
        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            currObj = objectsToSpawn[i];
            if (currObj.avoidSpecialEvents)
            {
                currObj.NextSpawnObjectDistance += metersRunned;
                //Debug.Log("RestoreDistancesForSpecialCar: " + currObj.type + " " + currObj.NextSpawnObjectDistance);
            }
        }
    }
    #endregion

    #region Messages
    public bool ShotNearestTrafficVehicle(bool shotRight, float minShotDistance, float maxShotDistance)
    {
        float minDistance = 500.0f;
        GameObject hittedVehicle = null;
        for (int i = 0; i < opponents.Count; ++i) //for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            OpponentKinematics opponentKin = opponent.GetComponent<OpponentKinematics>();
            OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
            if (opponent != null && opponentKin!=null && !opponentKin.IsExploded && (type == OnTheRunObjectsPool.ObjectType.TrafficVehicle || type == OnTheRunObjectsPool.ObjectType.Police))
            {
                float opponentDistance = opponent.GetComponent<Rigidbody>().position.z - player.GetComponent<Rigidbody>().position.z;
                bool distanceRange = opponentDistance > minShotDistance && opponentDistance < maxShotDistance;
                if (distanceRange && opponentDistance < minDistance)
                {
                    int oppLane = opponentKin.CurrentLane;
                    bool checkSide = shotRight ? oppLane > 2 : oppLane < 2;
                    if (checkSide)
                    {
                        minDistance = opponentDistance;
                        hittedVehicle = opponent;
                        break;
                    }
                }
            }
        }

        bool someoneHitted = false;
        if (hittedVehicle != null)
        {
            someoneHitted = true;
            hittedVehicle.GetComponent<OpponentKinematics>().SendMessage("OnOpponentDestroyed");
        }

        return someoneHitted;
    }

    void OnTankActivated(bool active)
    {
        if(active)
        {   
            if (backupLanesConfig != OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLane && backupLanesConfig != OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward)
                backupLanesConfig = environmentManager.currentTrafficDirection;
            if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.Asia || environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLaneForwardBackward;    
            else
                environmentManager.currentTrafficDirection = OnTheRunEnvironment.TrafficDirectionConfiguration.AvoidCentralLane;
        }
        else
        {
            environmentManager.currentTrafficDirection = backupLanesConfig;
        }
    }

    void OnChangeVehicleSpawnData( )
    {
        objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance = Mathf.Clamp(objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance - deltaStartMinSpawn, maxStartMinSpawn, min_startMinSpawn);
        objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance = Mathf.Clamp(objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance - deltaStartMaxSpawn, maxStartMaxSpawn, min_startMaxSpawn);
        objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance = Mathf.Clamp(objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance - deltaEndMinSpawn, maxEndMinSpawn, min_endMinSpawn);
        objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance = Mathf.Clamp(objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance - deltaEndMaxSpawn, maxEndMaxSpawn, min_endMaxSpawn);
        /*Debug.Log("NEW DATA -> startMinSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance + 
                           " - startMaxSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance +
                           " - endMinSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance +
                           " - endMaxSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance);*/
    }

    public List<int> GetLanesOccuppiedByTrucks()
    {
        List<int> laneList = new List<int>();
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                bool isTruck = OnTheRunObjectsPool.Instance.IsATruck(opponent.GetComponent<SpawnableObjectType>().type);
                if (isTruck)
                {
                    laneList.Add(opponent.GetComponent<OpponentKinematics>().CurrentLane);
                }
            }
        }

        return laneList;
    }
    
    void OnPauseChangeLaneUpdate()
    {
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
                if (opponent != null && type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
                {
                    opponent.SendMessage("OnPauseChangeLaneUpdate");
                }
            }
        }
    }

    void OnTankShot(bool rightDirection)
    {
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
                if (opponent != null && type == OnTheRunObjectsPool.ObjectType.TrafficVehicle)
                {
                    opponent.SendMessage("OnTankShot", rightDirection);
                }
            }
        }
    }

    void ActivateBadGuy(OnTheRunEnemiesManager.EnemyData enemyData)
    {
        GameObject instance = null;
        OnTheRunObjectsPool.ObjectType objType = enemyData.type;
        if (objType != OnTheRunObjectsPool.ObjectType.Helicopter)
        {
            for (int i = 0; i < enemyData.quantity; ++i)
            {
                instance = OnTheRunObjectsPool.Instance.RequestObject(objType);
                PoliceBehaviour polBehaviour = instance.GetComponent<PoliceBehaviour>();
                polBehaviour.enterStyle = enemyData.enterStyle;
                polBehaviour.MyTurn = enemyData.quantity - i - 1;

                if (enemyData.enterStyle == PoliceBehaviour.EnterStyle.FromBehind)
                {
                    float initPosition = player.PlayerRigidbody.position.z;
                    instance.transform.position = new Vector3(0.0f, 0.0f, initPosition - 10.0f - i * 40.0f); //new Vector3(player.rigidbody.position.x, 0.0f, player.rigidbody.position.z + 200.0f);
                    instance.SendMessage("InitializeDistance", i);
                }
                else
                {
                    instance.transform.position = new Vector3(player.PlayerRigidbody.position.x, 0.0f, player.PlayerRigidbody.position.z + 200.0f + i * 30.0f);
                }

                instance.transform.rotation = Quaternion.identity;

                if (instance.GetComponent<SpawnableObjectType>() == null)
                    instance.AddComponent<SpawnableObjectType>();
                instance.GetComponent<SpawnableObjectType>().type = objType;

                int laneChoosen = ChooseFreeLane(true);
                OpponentKinematics opponentKinematics = instance.GetComponent<OpponentKinematics>();
                if (opponentKinematics != null)
                {
                    opponentKinematics.CurrentLane = laneChoosen;
                    opponentKinematics.gameObject.SendMessage("OnReset");
                }

                instance.SendMessage("Initialize", true);

                opponents.Add(instance);
            }
        }
        else
        {
            if (helicopterOpponent == null)
                instance = OnTheRunObjectsPool.Instance.RequestObject(objType);
            else
                instance = helicopterOpponent;
            instance.SetActive(true);

            instance.transform.rotation = Quaternion.identity;

            if (instance.GetComponent<SpawnableObjectType>() == null)
                instance.AddComponent<SpawnableObjectType>();
            instance.GetComponent<SpawnableObjectType>().type = objType;

            int laneChoosen = ChooseFreeLane(true);
            OpponentKinematics opponentKinematics = instance.GetComponent<OpponentKinematics>();
            if (opponentKinematics != null)
            {
                opponentKinematics.CurrentLane = laneChoosen;
                opponentKinematics.gameObject.SendMessage("OnReset");
            }

            instance.SendMessage("Initialize", true);
            helicopterOpponent = instance;
        }
    }

    void OnChangePlayerCar()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
    }


    void OnPause()
    {
    }

    void OnResume()
    {
    }

    void OnStartGame()
    {
        started = true;
        lastPositionSpawn = 80.0f;//If zero the first car was spawned in a wrong position (sometimes the second one was spawned over the first)
        lastOpponentSpawn = null;
        for (int i = 0; i < objectsToSpawn.Count; ++i)
        {
            SpawnableObjectClass currObj = objectsToSpawn[i];
            currObj.Reset();
        }

        for (int i = 0; i < freeLanes.Length; ++i)
            freeLanes[i] = -1.0f;

        nextTruckSpawnPosition = new List<float>();
        nextTruckLane = -1;

        objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance = backupTrafficValues[0];
        objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance = backupTrafficValues[1];
        objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance = backupTrafficValues[2];
        objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance = backupTrafficValues[3];

        ResetAvailableSpecialVehicles();
        /*Debug.Log("TRAFFIC DATA -> startMinSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].startMinSpawnDistance +
                   " - startMaxSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].startMaxSpawnDistance +
                   " - endMinSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].endMinSpawnDistance +
                   " - endMaxSpawnDistance:" + objectsToSpawn[vehicleTrafficIndex].endMaxSpawnDistance);*/
    }

    void OnGameover()
    {
        started = false;
        DestroyAllObjects();
    }

    void PlayerIsResurrected()
    {
        lastPositionSpawn += 80.0f;
        OnTheRunObjectsPool.Instance.NotifyDestroyingExplosion();
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                OnTheRunObjectsPool.ObjectType type = opponent.GetComponent<SpawnableObjectType>().type;
                bool checkForRoadWorks = type != OnTheRunObjectsPool.ObjectType.RoadWork;
                bool checkForGuardRail = type != OnTheRunObjectsPool.ObjectType.StartGuardRail && type != OnTheRunObjectsPool.ObjectType.CentralGuardRail && type != OnTheRunObjectsPool.ObjectType.EndGuardRail;
                if (opponent != null && type != OnTheRunObjectsPool.ObjectType.Checkpoint && !OnTheRunObjectsPool.Instance.IsABonus(type) && checkForRoadWorks && checkForGuardRail)
                {
                    OnTheRunObjectsPool.Instance.NotifyDestroyingParent(opponent, type);
                    opponents.RemoveAt(i);
                }
            }
        }
    }
    
    void DestroyAllObjects()
    {
        OnTheRunObjectsPool.Instance.NotifyDestroyingExplosion();
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                OnTheRunObjectsPool.Instance.NotifyDestroyingParent(opponent, opponent.GetComponent<SpawnableObjectType>().type);
                opponents.RemoveAt(i);
            }
        }

        if (helicopterOpponent != null)
        {
            helicopterOpponent.GetComponent<HelicopterAreaBehaviour>().StopShooting();
            helicopterOpponent.SetActive(false);
            helicopterOpponent = null;
        }
    }

    void OnTutorialChange(OnTheRunTutorialManager.TutorialType type)
    {
        int currentPlayerLane = player.CurrentLane + OnTheRunTutorialManager.Instance.PlayerLaneDelta;
        switch(type)
        {
            case OnTheRunTutorialManager.TutorialType.SingleSlipstream:
                DestroyAllObjects();
                
                player.CurrentLane = 2;
                player.gameObject.transform.position = new Vector3(0.0f, player.gameObject.transform.position.y, player.gameObject.transform.position.z);
                currentPlayerLane = 2;
                //player.GetComponent<PlayerDraft>().SendMessage("ActivateDraftParticles", false);
                player.GetComponent<PlayerDraft>().SendMessage("PalyerHasCollided");

                float distance = 120.0f;
                OpponentKinematics oppKin = SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.TrafficVehicle, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane, Vector3.back * distance);
                OnTheRunTutorialManager.Instance.AddTutorialCar(oppKin);

                for (int i = 0; i < 5; ++i)
                {
                    if (currentPlayerLane!=0)
                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.BonusMoney, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane - 1, (Vector3.back * distance) + Vector3.forward * 5.0f * i);
                    if (currentPlayerLane != 4)
                        SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.BonusMoney, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane + 1, (Vector3.back * distance) + Vector3.forward * 5.0f * i);
                }
                break;
            case OnTheRunTutorialManager.TutorialType.Turbo:
                float firstTurboDistance = 100.0f;
                SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.BonusTurbo, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane, Vector3.back * firstTurboDistance);
                SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.BonusTurbo, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane, Vector3.back * (firstTurboDistance - 25.0f));
                SpawnObject(new SpawnableObjectClass(OnTheRunObjectsPool.ObjectType.BonusTurbo, 0, 0, 0, 0, 0), 1, 0.0f, currentPlayerLane, Vector3.back * (firstTurboDistance - 50.0f));
                break;
        }
    }

    void RemoveHelicopter()
    {
        //helicopterOpponent = null;
    }
    #endregion

    #region Trucks Management
    protected TruckBehaviour.TrasformType lastSpecialVehiclesSpawned = TruckBehaviour.TrasformType.None;
    protected List<TruckBehaviour.TrasformType> availableSpecialVehicles;
    protected int currentSpecialVehiclesIdx;

    void ResetAvailableSpecialVehicles()
    {
        //Debug.Log("*** RESET");
        currentSpecialVehiclesIdx = 0;
        availableSpecialVehicles.Clear();

        for (int i = 0; i < UnlockingManager.Instance.SpecialCarsNumber; ++i)
        {
            TruckBehaviour.TrasformType currType = (TruckBehaviour.TrasformType)i;
            if (!UnlockingManager.Instance.IsCarLocked(currType))
                availableSpecialVehicles.Add(currType);
        }
        Shuffle<TruckBehaviour.TrasformType>(availableSpecialVehicles);
    }

    public TruckBehaviour.TrasformType GetNextSpecialVehicles()
    {
        TruckBehaviour.TrasformType retValue;
        if (moreSpecialVehicle == TruckBehaviour.TrasformType.None)
        {
            //Debug.Log("--------------------------------------- " + lastSpecialVehiclesSpawned);
            currentSpecialVehiclesIdx = UnityEngine.Random.Range(0, availableSpecialVehicles.Count);
            if (lastSpecialVehiclesSpawned != TruckBehaviour.TrasformType.None)
            {
                if (lastSpecialVehiclesSpawned == availableSpecialVehicles[currentSpecialVehiclesIdx])
                {
                    ++currentSpecialVehiclesIdx;
                    if (currentSpecialVehiclesIdx >= availableSpecialVehicles.Count)
                        currentSpecialVehiclesIdx -= 2;
                }
                lastSpecialVehiclesSpawned = TruckBehaviour.TrasformType.None;
                //Debug.Log("*** USED " + availableSpecialVehicles[currentSpecialVehiclesIdx]);
            }
            if (currentSpecialVehiclesIdx < 0)
                currentSpecialVehiclesIdx = 0;
            retValue = availableSpecialVehicles[currentSpecialVehiclesIdx];
            availableSpecialVehicles.RemoveAt(currentSpecialVehiclesIdx);

            lastSpecialVehiclesSpawned = retValue;
            if (availableSpecialVehicles.Count == 0)
            {
                ResetAvailableSpecialVehicles();
            }
        }
        else
        {
            float specificProbability = OnTheRunDataLoader.Instance.GetPercentageForceSpecialVehicle();
            float othersProbabilities = (100.0f - specificProbability) / (availableSpecialVehicles.Count-1);

            //Debug.Log("specificProbability " + specificProbability + " othersProbabilities " + othersProbabilities);

            List<KeyValuePair<TruckBehaviour.TrasformType, double>> specialVehiclesProbabilities = new List<KeyValuePair<TruckBehaviour.TrasformType, double>>();
            for (int i = 0; i < availableSpecialVehicles.Count; ++i)
            {
                float probability = moreSpecialVehicle==availableSpecialVehicles[i] ? specificProbability : othersProbabilities;
                specialVehiclesProbabilities.Add(new KeyValuePair<TruckBehaviour.TrasformType, double>(availableSpecialVehicles[i], probability));
            }

            TruckBehaviour.TrasformType selected = TruckBehaviour.TrasformType.Bigfoot;
            System.Random r = new System.Random();
            double diceRoll = r.NextDouble() * 100.0f;
            double cumulative = 0.0;
            for (int i = 0; i < specialVehiclesProbabilities.Count; i++)
            {
                cumulative += specialVehiclesProbabilities[i].Value;
                if (diceRoll < cumulative)
                {
                    selected = specialVehiclesProbabilities[i].Key;
                    break;
                }
            }
            retValue = selected;

            //Debug.Log("---> GetNextSpecialVehicles " + retValue + " diceRoll " + diceRoll);
        }

        return retValue;
    }

    public void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion
}