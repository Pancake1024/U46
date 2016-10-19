using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackObstaclesManager : MonoBehaviour
{
    protected float roadWorksMinLenght;
    protected float roadWorksMaxLenght;
    protected float roadWorksMinDistance;
    protected float roadWorksMaxDistance;

    public GameObject startGuardRail;
    public GameObject centerGuardRail;
    public GameObject endGuardRail;

    protected OnTheRunSpawnManager spawnManager;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunEnvironment environmentManager;
    protected PlayerKinematics playerKin;

    protected float nextRoadWorksDistance;
    protected float nextObstacleDistance = -1.0f;
    protected float roadWorksEndDistance = -1.0f;
    protected int roadWorksLane = -1;
    protected float roadWorksObstaclesFixedDistance = 14.0f;

    protected float guardRailMinLenght;
    protected float guardRailMaxLenght;
    protected float guardRailHoleMinDistance;
    protected float guardRailHoleMaxDistance;

    protected float roadGuardRailFixedDistance = 49.0f;
    protected float lastGuardRailPos = 0.0f;

    protected int firstTwoBigCoinCounter = 0;

    public struct RoadWorksData
    {
        public float length;
        public float obstaclesDistance;
        public int lane;
    }

    public struct SpawnObstacleData
    {
        public OnTheRunObjectsPool.ObjectType obstaclesType;
        public int selectedLane;
        public float zOffset;
    }

    public int RoadWorksLane
    {
        get { return roadWorksLane; }
    }

    #region Unity callbacks
    void Awake()
    {
        roadWorksMinLenght = OnTheRunDataLoader.Instance.GetRoadworksData("min_duration");
        roadWorksMaxLenght = OnTheRunDataLoader.Instance.GetRoadworksData("max_duration");
        roadWorksMinDistance = OnTheRunDataLoader.Instance.GetRoadworksData("min_distance");
        roadWorksMaxDistance = OnTheRunDataLoader.Instance.GetRoadworksData("max_distance");

        guardRailMinLenght = OnTheRunDataLoader.Instance.GetCentralMudData("min_duration");
        guardRailMaxLenght = OnTheRunDataLoader.Instance.GetCentralMudData("max_duration");
        guardRailHoleMinDistance = OnTheRunDataLoader.Instance.GetCentralMudData("min_hole_duration");
        guardRailHoleMaxDistance = OnTheRunDataLoader.Instance.GetCentralMudData("max_hole_duration");
    }

    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
        playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
        spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();

    }

    void Update() //FixedUpdate
    {
        if ((!gameplayManager.Gameplaystarted && !playerKin.PlayerIsStopping) || gameplayManager.GameState==OnTheRunGameplay.GameplayStates.Offgame)
            return;

        if (playerKin == null && GameObject.FindGameObjectWithTag("Player")!=null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (playerKin == null)
            return;

        float playerDistance = playerKin.PlayerRigidbody.position.z;

        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.NY)
        {
            bool canRoadWorksBeActivated = roadWorksEndDistance < 0.0f && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Tank && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Firetruck;
            
            //START ROAD WORKS--------------------------------------------------------------
            if (playerDistance > nextRoadWorksDistance && canRoadWorksBeActivated)
                StartRoadWorks(playerDistance);

            //Spawn obstacles--------------
            if (nextObstacleDistance > 0.0f)
            {
                bool checkDistance = playerDistance > nextObstacleDistance;
                if (checkDistance)
                {
                    SpawnRoadWorkObstacle(playerDistance);
                }
            }

            //END ROAD WORKS--------------------------------------------------------------
            if (roadWorksEndDistance > 0.0f && playerDistance >= roadWorksEndDistance)
                StopRoadWorks( );
        }
        else if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
        {
            bool canRoadWorksBeActivated = roadWorksEndDistance < 0.0f && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Tank && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Firetruck;
            
            //START GUARDRAIL--------------------------------------------------------------
            if (playerDistance > nextRoadWorksDistance && canRoadWorksBeActivated)
                StartGuardRail(playerDistance - 1.0f);

            //Spawn obstacles--------------
            if (nextObstacleDistance > 0.0f)
            {
                bool checkDistance = playerDistance > nextObstacleDistance;
                if (checkDistance)
                {
                    SpawnGuardRailObstacle(playerDistance - 1.0f, OnTheRunObjectsPool.ObjectType.CentralGuardRail, roadGuardRailFixedDistance);

                    if (playerDistance + roadGuardRailFixedDistance >= roadWorksEndDistance)
                    {
                        SpawnGuardRailObstacle(playerDistance - 2.0f, OnTheRunObjectsPool.ObjectType.EndGuardRail, -1.0f);
                        StopGuardRail();
                    }
                }
            }

            //END GUARDRAIL--------------------------------------------------------------
            //if (roadWorksEndDistance > 0.0f && playerDistance >= roadWorksEndDistance)
            //{
            //    StopGuardRail();
            //}
        }
    }
    #endregion

    #region Messages
    void RaceStarted()
    {
        nextRoadWorksDistance = 0.0f;
        nextObstacleDistance = -1;
        roadWorksEndDistance = -1.0f;
        roadWorksLane = -1;

        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.NY)
            ComuputeNextRoadWorksDistance();
        else if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
            ComuputeNextGuardRailDistance(true);
    }
    #endregion

    #region Road Works
    void ComuputeNextRoadWorksDistance()
    {
        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        nextRoadWorksDistance = playerKin.PlayerRigidbody.position.z + UnityEngine.Random.Range(roadWorksMinDistance, roadWorksMaxDistance);

        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
            nextRoadWorksDistance = 0.0f;
    }

    void StartRoadWorks(float playerDistance)
    {
        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.NY)
        {
            List<int> lanesOccupied = spawnManager.GetLanesOccuppiedByTrucks();
            if (lanesOccupied.Count < 5)
            {
                float currentLenght = UnityEngine.Random.Range(roadWorksMinLenght, roadWorksMaxLenght);
                roadWorksEndDistance = playerDistance + currentLenght;
                do
                {
                    roadWorksLane = UnityEngine.Random.Range(0, 5);
                } while (lanesOccupied.Contains(roadWorksLane));

                firstTwoBigCoinCounter = 0;
                SpawnRoadWorkObstacle(playerDistance);
                spawnManager.SendMessage("AvoidLaneForRoadWork", roadWorksLane);
            }
        }
    }

    void SpawnRoadWorkObstacle(float playerDistance)
    {
        SpawnObstacleData data = new SpawnObstacleData();
        data.selectedLane = roadWorksLane;
        ++firstTwoBigCoinCounter;
        if (firstTwoBigCoinCounter <= 2)
            data.obstaclesType = OnTheRunObjectsPool.ObjectType.BonusMoneyBig;
        else
            data.obstaclesType = OnTheRunObjectsPool.ObjectType.RoadWork;
        data.zOffset = 0.0f;
        spawnManager.SendMessage("SpawnSingleObstacle", data);
        nextObstacleDistance = playerDistance + roadWorksObstaclesFixedDistance;
    }

    void StopRoadWorks( )
    {
        nextObstacleDistance = -1;
        roadWorksEndDistance = -1.0f;
        roadWorksLane = -1;
        ComuputeNextRoadWorksDistance();
    }
    #endregion

    #region Central Lane Walls
    void ComuputeNextGuardRailDistance(bool startRace=false)
    {
        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        nextRoadWorksDistance = playerKin.PlayerRigidbody.position.z + UnityEngine.Random.Range(guardRailHoleMinDistance, guardRailHoleMaxDistance);
        
        if(startRace)
            nextRoadWorksDistance = 0.0f;
    }

    void StartGuardRail(float playerDistance)
    {
        if (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA)
        {
            float currentLenght = UnityEngine.Random.Range(guardRailMinLenght, guardRailMaxLenght);
            roadWorksEndDistance = playerDistance + currentLenght;
            roadWorksLane = 2;
            SpawnGuardRailObstacle(playerDistance, OnTheRunObjectsPool.ObjectType.StartGuardRail, 24.0f);
        }
    }

    void SpawnGuardRailObstacle(float playerDistance, OnTheRunObjectsPool.ObjectType type, float nextDistance)
    {
        SpawnObstacleData data = new SpawnObstacleData();
        data.selectedLane = roadWorksLane;
        data.obstaclesType = type;
        data.zOffset = 0.0f;
        if (nextDistance<0.0f)
            data.zOffset = 24.0f;
        spawnManager.SendMessage("SpawnSingleObstacle", data);
        nextObstacleDistance = playerDistance + nextDistance;
    }

    void StopGuardRail()
    {
        nextObstacleDistance = -1;
        roadWorksEndDistance = -1.0f;
        roadWorksLane = -1;
        ComuputeNextGuardRailDistance();
    }
    #endregion
}
