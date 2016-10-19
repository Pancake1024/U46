using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;

public class OnTheRunEnvironment : MonoBehaviour
{
    static public float[] lanes = {-0.74f, -0.37f, 0.0f, 0.37f, 0.74f};

    #region Public members
    public TrafficDirectionConfiguration currentTrafficDirection;
    public Environments currentEnvironment;
    public List<Material> asphaltMaterials;
    #endregion

    #region Protected members
    protected OnTheRunSpawnManager spawnManager;
    #endregion


    #region Public properties
    public enum TrafficDirectionConfiguration
    {
        AllForward = 0,
        ForwardBackward,
        AvoidCentralLane,
        AvoidCentralLaneForwardBackward,
    }

    public enum Environments
    {
        None = -1,
        Europe = 0,
        NY,
        USA,
        Asia,
        Count
    }

    public Material CurrentAsphalt
    {
        get { return asphaltMaterials[(int)currentEnvironment]; }
    }
    #endregion

    #region Unity callbacks
    void Start()
    {
        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.GetComponent<OnTheRunSpawnManager>();
    }
    #endregion

}
