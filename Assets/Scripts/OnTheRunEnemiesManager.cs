using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

public class OnTheRunEnemiesManager : MonoBehaviour
{
    #region Public Members
    protected float minDistanceFromCheckpoint;
    public List<PoliceSpawnData> policeSpawnData;
    public List<PoliceSpawnData> helicopterSpawnData;

    public float minRoadBlockCarDistance;
    public float maxRoadBlockCarDistance;
    #endregion

    #region Protected Members
    protected float minPoliceSpawnDistance = 2200.0f; // 4000.0f;
    protected float maxPoliceSpawnDistance = 4500.0f;// 8000.0f;
    protected float minHelicopterSpawnDistance;
    protected float maxHelicopterSpawnDistance;
    protected float nextPoliceDistance = -1.0f;
    protected float nextHelicopterDistance = -1.0f;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunSpawnManager spawnManager;
    protected OnTheRunObjectsPool.ObjectType[] enemiesList = { OnTheRunObjectsPool.ObjectType.Police };//, OnTheRunObjectsPool.ObjectType.Helicopter OnTheRunObjectsPool.ObjectType.BlackCar, OnTheRunObjectsPool.ObjectType.BlackVan, OnTheRunObjectsPool.ObjectType.Helicopter }; //, OnTheRunObjectsPool.ObjectType.BlackPickup
    protected int nextEnemyIdx = 0;
    protected bool spawningActive = true;
    protected PlayerKinematics playerKin;
    protected bool noPoliceBoostActivated = false;
    protected int policeConfigurationIndex = 0;
    protected int helicopterConfigurationIndex = 0;
    protected int policeCount = -1;
    protected int helicopterCount = -1;
    protected int policeAlreadyActive = 0;

    protected int[] roadBlockLanes = { 0, 1, 2, 3, 4 };
    protected int roadBlockLaneIndex = 0;
    protected float roadBlockLaneDistance = -1.0f;
    protected float nextRoadWorkCarDistance = -1.0f;

    protected bool helicopterActive = false;

    protected float spawnPolicePercentage = 100.0f;
    protected bool forcePoliceSpawn = false;
    #endregion

    #region Public properties
    public bool isPoliceActive 
    {
        get { return policeAlreadyActive>0; } 
    }

    [System.Serializable]
    public class PoliceSpawnData
    {
        public string id;
        public float minSpawnDistance;
        public float maxSpawnDistance;
        public float percentage;
    }

    public struct EnemyData
    {
        public OnTheRunObjectsPool.ObjectType type;
        public PoliceBehaviour.EnterStyle enterStyle;
        public int quantity;
    }

    public enum EnemyType
    {
        Helicopter,
        PoliceSingle,
        PoliceTriple,
        RoadBlock
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        minDistanceFromCheckpoint = OnTheRunDataLoader.Instance.GetEnemiesMinDistanceFromCheckpoint();
        spawnPolicePercentage = OnTheRunDataLoader.Instance.GetPoliceSpawnPercentage();

        string dataId = "one";
        for (int i = 0; i < 2; ++i)
        {
            float[] policeData = OnTheRunDataLoader.Instance.GetPoliceData(dataId);
            policeSpawnData[i].minSpawnDistance = policeData[0];
            policeSpawnData[i].maxSpawnDistance = policeData[1];
            policeSpawnData[i].percentage = policeData[2];
            if (dataId == "one") dataId = "three";
        }

        dataId = "one";
        for (int i = 0; i < 3; ++i)
        {
            float[] helicopterData = OnTheRunDataLoader.Instance.GetHelicopterData(dataId);
            helicopterSpawnData[i].minSpawnDistance = helicopterData[0];
            helicopterSpawnData[i].maxSpawnDistance = helicopterData[1];
            if (dataId == "one") dataId = "two";
            else if (dataId == "two") dataId = "three";
        }
    }

    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.GetComponent<OnTheRunSpawnManager>();

        spawningActive = true;

        InitializeEnemiesList();
    }

    void Update() //FixedUpdate
    {
        if (!gameplayManager.Gameplaystarted)
            return;

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            return;

        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        //Next enemy logic--------------
        bool distanceCheck = false;
        bool checkForPlayer = !(gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Tank) && !(gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Ufo) && !(gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Plane) && !(gameplayManager.CurrentSpecialCar == OnTheRunGameplay.CarId.Firetruck);
        bool canBeSpawned = /*CheckpointCheck() &&*/ !playerKin.PlayerIsStopping && !playerKin.PlayerIsDead && checkForPlayer; //&& !playerKin.TurboOn // !gameplayManager.IsSpecialCarActive && 

        if (nextHelicopterDistance > 0.0f && canBeSpawned)
        {
            distanceCheck = playerKin.Distance > nextHelicopterDistance;
            if (distanceCheck && ((policeAlreadyActive == 0 && (nextPoliceDistance - playerKin.Distance) >= 500.0f) || helicopterMultiplier < 1.0f) && !gameplayManager.IsHelicopterActive)
            {
                if (helicopterCount <= 0)
                    nextHelicopterDistance = -1.0f;
                else
                {
                    ComputeNextHelicopterDistance();
                    SpawnEnemy(EnemyType.Helicopter);
                }
            }
            else if (distanceCheck && (policeAlreadyActive != 0 || (nextPoliceDistance - playerKin.Distance) < 500.0f))
            {
                nextHelicopterDistance += Random.Range(minHelicopterSpawnDistance * 0.5f, maxHelicopterSpawnDistance * 0.5f);
            }
        }

        if (spawningActive && nextPoliceDistance > 0.0f && !gameplayManager.IsHelicopterActive)
        {
            distanceCheck = playerKin.Distance > nextPoliceDistance;
            if (distanceCheck && canBeSpawned)
            {
                if (policeCount <= 0)
                    nextPoliceDistance = -1.0f;
                else
                {
                    ComputeNextPoliceDistance();
                    bool spawnTriplePolice = Random.Range(0, 10000000) % 100 < policeSpawnData[1].percentage;
                    if (spawnTriplePolice || forcePoliceSpawn)
                        SpawnEnemy(EnemyType.PoliceTriple);
                    else
                        SpawnEnemy(EnemyType.PoliceSingle);
                }
            }
        }
        //------------------------------

        if (roadBlockLaneDistance >= 0.0f)
        {
            bool checkDistance = (playerKin.PlayerRigidbody.position.z - roadBlockLaneDistance) > nextRoadWorkCarDistance;
            if (checkDistance)
            {
                ++roadBlockLaneIndex;
                if (roadBlockLaneIndex <= 2)
                {
                    nextRoadWorkCarDistance = UnityEngine.Random.Range(minRoadBlockCarDistance, maxRoadBlockCarDistance);
                    roadBlockLaneDistance = playerKin.PlayerRigidbody.position.z;
                    LevelRoot.Instance.Root.BroadcastMessage("ActivateBlockRoad", roadBlockLanes[roadBlockLaneIndex]);
                }
                else
                {
                    roadBlockLaneDistance = -1.0f;
                }
            }
        }
    }
    #endregion

    #region Functions
    void InitializeEnemiesList()
    {
        nextEnemyIdx = 0;
        //CoreUtils.Shuffle<OnTheRunObjectsPool.ObjectType>(enemiesList, new System.Random(iOSUtils.GetNetworkDate().Millisecond));
        CoreUtils.Shuffle<OnTheRunObjectsPool.ObjectType>(enemiesList, new System.Random(System.Environment.TickCount));
    }

    IEnumerator DelayedHelicopter()
    {
        if (Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>()!=null)
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("OnLiveNews", 99999.0f);

        yield return new WaitForSeconds(2.5f);

        EnemyData enData = new EnemyData();
        enData.type = OnTheRunObjectsPool.ObjectType.Helicopter;
        LevelRoot.Instance.Root.BroadcastMessage("ActivateBadGuy", enData);
    }

    void SpawnEnemy(EnemyType type)
    {
        bool avoidEnemy = noPoliceBoostActivated && enemiesList[nextEnemyIdx] == OnTheRunObjectsPool.ObjectType.Police;
        if (type == EnemyType.Helicopter)
        {
            OnTheRunSounds.Instance.StartReduceMusicVolume();
           // playerKin.gameObject.GetComponent<PlayerSounds>().SendMessage("OnPausePlayerEngine");

            OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.JingleTv);
            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.HelicopterEnter);
            StartCoroutine(DelayedHelicopter());
        }
        else if (policeAlreadyActive==0)
        {
            if (!avoidEnemy)
            {
                //Debug.Log("******** POLICE ENTER!!!!!!!!!!");
                if (type == EnemyType.PoliceSingle)
                {
                    EnemyData enData = new EnemyData();
                    enData.type = OnTheRunObjectsPool.ObjectType.Police;
                    enData.quantity = 1;
                    enData.enterStyle = PoliceBehaviour.EnterStyle.FromFront;
                    policeAlreadyActive = enData.quantity;
                    LevelRoot.Instance.Root.BroadcastMessage("ActivateBadGuy", enData);
                }
                else if (type == EnemyType.PoliceTriple)
                {
                    OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.PoliceEnter);
                    EnemyData enData = new EnemyData();
                    enData.type = OnTheRunObjectsPool.ObjectType.Police;
                    enData.quantity = 3;
                    enData.enterStyle = PoliceBehaviour.EnterStyle.FromBehind;
                    policeAlreadyActive = enData.quantity;
                    LevelRoot.Instance.Root.BroadcastMessage("ActivateBadGuy", enData);
                }
                else if (type == EnemyType.RoadBlock)
                {
                    if (playerKin == null)
                        playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

                    OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.PoliceEnter);
                    CoreUtils.Shuffle<int>(roadBlockLanes, new System.Random(System.Environment.TickCount)); //iOSUtils.GetNetworkDate().Millisecond));
                    roadBlockLaneIndex = 0;
                    nextRoadWorkCarDistance = UnityEngine.Random.Range(minRoadBlockCarDistance, maxRoadBlockCarDistance);
                    roadBlockLaneDistance = playerKin.PlayerRigidbody.position.z;
                    //List<int> lanesOccupied = spawnManager.GetLanesOccuppiedByTrucks(); // ??
                    LevelRoot.Instance.Root.BroadcastMessage("ActivateBlockRoad", roadBlockLanes[roadBlockLaneIndex]);
                }
            }
        }
       

        ++nextEnemyIdx;
        if (nextEnemyIdx >= enemiesList.Length)
        {
            InitializeEnemiesList();
        }
    }
    
    bool CheckpointCheck()
    {
        float nextCheckpointdistance = spawnManager.NextCheckpointPosition - playerKin.gameObject.transform.position.z;
        return nextCheckpointdistance > minDistanceFromCheckpoint;
    }

    protected float initialPlayerDistance;
    void SaveDistancesForSpecialCar(float playerCurrentDistance)
    {
        initialPlayerDistance = playerCurrentDistance;
    }

    void RestoreDistancesForSpecialCar(float playerCurrentDistance)
    {
        float metersRunned = playerCurrentDistance - initialPlayerDistance;

        if (nextPoliceDistance > 0.0f)
            nextPoliceDistance += metersRunned;

        if (nextHelicopterDistance > 0.0f)
            nextHelicopterDistance += metersRunned;
    }

    void ComputeNextPoliceDistance(bool resetGame=false)
    {
        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        minPoliceSpawnDistance = policeSpawnData[policeConfigurationIndex - 1].minSpawnDistance * policeMultiplier;
        maxPoliceSpawnDistance = policeSpawnData[policeConfigurationIndex - 1].maxSpawnDistance * policeMultiplier;
        --policeCount;
        float offsetDistance = resetGame ? 0.0f : playerKin.Distance;
        nextPoliceDistance = offsetDistance + Random.Range(minPoliceSpawnDistance, maxPoliceSpawnDistance);
    }

    void ComputeNextHelicopterDistance(bool resetGame = false)
    {
        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        minHelicopterSpawnDistance = helicopterSpawnData[helicopterConfigurationIndex - 1].minSpawnDistance * helicopterMultiplier;
        maxHelicopterSpawnDistance = helicopterSpawnData[helicopterConfigurationIndex - 1].maxSpawnDistance * helicopterMultiplier;
        --helicopterCount;
        float offsetDistance = resetGame ? 0.0f : playerKin.Distance;
        nextHelicopterDistance = offsetDistance + Random.Range(minHelicopterSpawnDistance, maxHelicopterSpawnDistance);
    }

    public void ResetAllForceParameters()
    {
        forcePoliceSpawn = false;
        helicopterMultiplier = 1.0f;
        policeMultiplier = 1.0f;
    }

    protected float helicopterMultiplier = 1.0f;
    protected float policeMultiplier = 1.0f;
    public void ForceHelicopterSpawn( )
    {
        helicopterMultiplier = OnTheRunDataLoader.Instance.GetMissionsModificationsData("helicopter_distance_multiplier", 0.8f);
        helicopterConfigurationIndex = 2;//?
        helicopterCount = 6;
        ComputeNextHelicopterDistance();
    }

    public void ForcePoliceSpawn()
    {
        policeMultiplier = OnTheRunDataLoader.Instance.GetMissionsModificationsData("police_distance_multiplier", 0.8f);
        forcePoliceSpawn = true;
        policeConfigurationIndex = 2;
        policeCount = 3;
        spawningActive = true;
        ComputeNextPoliceDistance( );
        nextHelicopterDistance = -1.0f;
    }
    #endregion
    
    #region Messages
    void OnEndPoliceCar()
    {
        --policeAlreadyActive;
        if (policeAlreadyActive < 0)
            policeAlreadyActive = 0;
    }

    void OnEquipBooster(OnTheRunBooster.BoosterType type)
    {
        switch (type)
        {
            case OnTheRunBooster.BoosterType.SpecialCar:
                //noPoliceBoostActivated = true;
                break;
        }
    }

    void OnStartGame()
    {
        spawningActive = false;
        nextPoliceDistance = -1.0f;
        nextHelicopterDistance = -1.0f;

        //Police initialization------------------
        float policeActivationPerc = Random.Range(0, 100000) % 100.0f;
        if(policeActivationPerc < spawnPolicePercentage)
        {
            policeConfigurationIndex = (int)(Random.Range(0, 100000) % (policeSpawnData.Count));
            ++policeConfigurationIndex;
            policeCount = (int)(Random.Range(0, 100000) % 2) + 1;//policeConfigurationIndex + 1;
            spawningActive = true;
            ComputeNextPoliceDistance(true);
        }

        //Helicopter initialization------------------
        helicopterConfigurationIndex = (int)(Random.Range(0, 100000) % (helicopterSpawnData.Count));
        ++helicopterConfigurationIndex;
        helicopterCount = helicopterConfigurationIndex + 1;
        ComputeNextHelicopterDistance(true);

        policeAlreadyActive = 0;

        //Debug.Log("HELICOPTERS NUMBER: " + helicopterConfigurationIndex + " -- POLICE NUMBER: " + policeConfigurationIndex);
        //Debug.Log("HELICOPTERS DISTANCE: " + (nextHelicopterDistance * OnTheRunUtils.ToOnTheRunMeters) + " -- POLICE DISTANCE: " + (nextPoliceDistance * OnTheRunUtils.ToOnTheRunMeters));
         
    }

    void OnGameover()
    {
        noPoliceBoostActivated = false;
    }

    void RestartEnemiesManager(bool restart)
    {
        if(this.enabled)
        {
            spawningActive = true;
        }
    }

    public void ResetForRestart()
    {
        StopAllCoroutines();
    }

    void OnReset()
    {
        StopAllCoroutines();
    }
    #endregion

}