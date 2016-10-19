using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;

[AddComponentMenu("OnTheRun/Traffic")]
public class OnTheRunTraffic : MonoBehaviour
{
    public List<GameObject> trafficVehicles = null;
    public PlayerKinematics player = null;
    public List<GameObject> opponents = null;
    public float startMinSpawnDistance = 15.0f;
    public float startMaxSpawnDistance = 40.0f;
    public float endMinSpawnDistance = 50.0f;
    public float endMaxSpawnDistance = 95.0f;
    public float startDistanceLane = 500.0f;
    public float endDistanceLane = 3500.0f;

    public GameObject bonusPrefab = null;
    public float startBonusMinSpawnDistance = 15.0f;
    public float startBonusMaxSpawnDistance = 40.0f;
    public float endBonusMinSpawnDistance = 50.0f;
    public float endBonusMaxSpawnDistance = 95.0f;

    protected float newVehicleDistance;
    protected float newBonusDistance;

    protected bool started = false;

    void Awake()
    {
        newVehicleDistance = -1.0f;
        newBonusDistance = -1.0f;
        opponents = new List<GameObject>();
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        newVehicleDistance = -1.0f;
        float dt = Time.fixedDeltaTime,
              time = TimeManager.Instance.MasterSource.TotalTime;
        if (started && (newVehicleDistance == -1.0f || player.GetComponent<Rigidbody>().position.z >= newVehicleDistance))
        {
            float ratio = 1.0f - Mathf.Clamp01((player.GetComponent<Rigidbody>().position.z - startDistanceLane) / (endDistanceLane - startDistanceLane)),
                  minDistance = startMinSpawnDistance + ratio * (endMinSpawnDistance - startMinSpawnDistance),
                  maxDistance = startMaxSpawnDistance + ratio * (endMaxSpawnDistance - startMaxSpawnDistance);
            //Debug.Log("d: " + player.rigidbody.position.z + " minDistance: " + minDistance + " maxDistance: " + maxDistance);

            bool spawnBonus = false;
            if (newVehicleDistance != -1.0f)
            {
                spawnBonus = (player.GetComponent<Rigidbody>().position.z >= newBonusDistance && newBonusDistance != -1.0f && !player.TurboOn);
                GameObject instance = (GameObject)Instantiate((spawnBonus ? bonusPrefab : trafficVehicles[UnityEngine.Random.Range(0, trafficVehicles.Count)]), new Vector3(player.GetComponent<Rigidbody>().position.x, 0.0f, player.GetComponent<Rigidbody>().position.z + 200.0f), Quaternion.identity);
                OpponentKinematics opponentKinematics = instance.GetComponent<OpponentKinematics>();
                opponentKinematics.CurrentLane = UnityEngine.Random.Range(0, 5);
                opponentKinematics.IsBonus = spawnBonus;
                opponentKinematics.gameObject.SendMessage("OnReset");
                opponents.Add(instance);
            }

            newVehicleDistance = player.GetComponent<Rigidbody>().position.z + UnityEngine.Random.Range(minDistance, minDistance);

            if (newBonusDistance == -1.0f || spawnBonus)
            {
                minDistance = startBonusMinSpawnDistance + ratio * (endBonusMinSpawnDistance - startBonusMinSpawnDistance);
                maxDistance = startBonusMaxSpawnDistance + ratio * (endBonusMaxSpawnDistance - startBonusMaxSpawnDistance);
                newBonusDistance = player.GetComponent<Rigidbody>().position.z + UnityEngine.Random.Range(minDistance, minDistance);
            }
        }

        float playerZ = player.GetComponent<Rigidbody>().position.z;
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                if (playerZ - opponent.GetComponent<Rigidbody>().position.z > 20.0f || opponent.GetComponent<Rigidbody>().position.y > 10.0f || (player.TurboOn && opponent.GetComponent<Collider>().isTrigger))
                {
                    //OnTheRunObjectsPool.Instance.NotifyDestroyingParent();
                    OnTheRunObjectsPool.Instance.NotifyDestroyingParent(opponent, OnTheRunObjectsPool.ObjectType.TrafficVehicle);
                    opponents.RemoveAt(i);
                }
            }
        }
    }

    void OnPause()
    {
    }

    void OnResume()
    {
    }

    void OnStartGame()
    {
        newVehicleDistance = -1.0f;
        started = true;
        newVehicleDistance = -1.0f;
        newBonusDistance = -1.0f;
    }

    void OnGameover()
    {
        started = false;
        DestroyAllVehicles();
    }

    void DestroyAllVehicles()
    {
        newVehicleDistance = -1.0f;
        //OnTheRunObjectsPool.Instance.NotifyDestroyingParent();
        for (int i = opponents.Count - 1; i >= 0; --i)
        {
            GameObject opponent = opponents[i];
            if (opponent != null)
            {
                OnTheRunObjectsPool.Instance.NotifyDestroyingParent(opponent, opponent.GetComponent<SpawnableObjectType>().type);
                opponents.RemoveAt(i);
            }
        }
    }
}