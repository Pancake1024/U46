using System;
using System.Collections.Generic;
using UnityEngine;

public class AmbientController : MonoBehaviour
{
    #region Public Members
    public AudioClip ingameMusic;
    public List<StageConfiguration> ambientConfigurationList;

    [System.Serializable]
    public class StageConfiguration
    {
        public List<NeverendingTrackModified.AmbientName> ambientIdList;
        public float minDurationInMeters;
        public float maxDurationInMeters;
        public float minSingleAmbientDurationInMeters = -1;
        public float maxSingleAmbientDurationInMeters = -1;
    }
    #endregion

    #region Protected Members
    protected OnTheRunGameplay gameplayManager;
    protected PlayerKinematics playerKinematics;
    protected StageConfiguration currentConfiguration;
    protected GameObject trackGO;
    protected int currentConfigurationIndex = 0;
    protected int currentAmbientIndex = 0;
    protected float currentAmbientLength;
    protected float nextSingleAmbientLength;
    #endregion

    #region Public Properties
    public NeverendingTrackModified.AmbientName CurrentAmbient
    {
        get { return currentConfiguration.ambientIdList[currentAmbientIndex]; }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        trackGO = GameObject.Find("Track");

        currentConfigurationIndex = 0;
        currentAmbientIndex = 0;
        currentConfiguration = ambientConfigurationList[currentConfigurationIndex];
        currentAmbientLength = UnityEngine.Random.Range(currentConfiguration.minDurationInMeters, currentConfiguration.maxDurationInMeters);
        if (currentConfiguration.minSingleAmbientDurationInMeters != -1 && currentConfiguration.maxSingleAmbientDurationInMeters != -1)
            nextSingleAmbientLength = UnityEngine.Random.Range(currentConfiguration.minSingleAmbientDurationInMeters, currentConfiguration.maxSingleAmbientDurationInMeters);

        OnTheRunSounds.Instance.ingameMusic = ingameMusic;
    }

    void Update()
    {
        if (Manager<UIManager>.Get().ActivePageName != "IngamePage")
            return;

        if (Manager<UIManager>.Get() != null && Manager<UIManager>.Get().ActivePageName != "IngamePage")
            return;

        if (playerKinematics == null)
            playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (currentAmbientLength > 0.0f && playerKinematics.Distance > currentAmbientLength)
        {
            UpdateAmbient();
        }
        else if (nextSingleAmbientLength > 0.0f && playerKinematics.Distance > nextSingleAmbientLength) //currentAmbientLength > 400.0f && 
        {
            switchAmbient();
        }
    }
    #endregion

    #region Functions
    void switchAmbient()
    {
        currentAmbientIndex = UnityEngine.Random.Range(0, ambientConfigurationList[currentConfigurationIndex].ambientIdList.Count);
        currentConfiguration = ambientConfigurationList[currentConfigurationIndex];
        nextSingleAmbientLength += UnityEngine.Random.Range(currentConfiguration.minSingleAmbientDurationInMeters, currentConfiguration.maxSingleAmbientDurationInMeters);
        
        trackGO.SendMessage("ChangeAmbient", CurrentAmbient);
    }

    void UpdateAmbient()
    {
        if (currentAmbientLength > 0)
        {
            //++currentConfigurationIndex;
            currentConfigurationIndex = (currentConfigurationIndex + 1) % ambientConfigurationList.Count;
            currentAmbientIndex = UnityEngine.Random.Range(0, ambientConfigurationList[currentConfigurationIndex].ambientIdList.Count);
            currentConfiguration = ambientConfigurationList[currentConfigurationIndex];
            if (currentConfiguration.minSingleAmbientDurationInMeters != -1 && currentConfiguration.maxSingleAmbientDurationInMeters != -1)
                nextSingleAmbientLength += UnityEngine.Random.Range(currentConfiguration.minSingleAmbientDurationInMeters, currentConfiguration.maxSingleAmbientDurationInMeters);
            else
                nextSingleAmbientLength = -1.0f;
            float nextDistanceRange = UnityEngine.Random.Range(currentConfiguration.minDurationInMeters, currentConfiguration.maxDurationInMeters);
            if (nextDistanceRange > 0.0f)
                currentAmbientLength += nextDistanceRange;
            else
                currentAmbientLength = -1;
            trackGO.SendMessage("ChangeAmbient", CurrentAmbient);
        }
    }
    #endregion

    #region Messages
    void OnStartGame()
    {
        currentConfigurationIndex = 0;
        currentAmbientIndex = 0;
        currentConfiguration = ambientConfigurationList[currentConfigurationIndex];
        currentAmbientLength = UnityEngine.Random.Range(currentConfiguration.minDurationInMeters, currentConfiguration.maxDurationInMeters);
        nextSingleAmbientLength = -1.0f;
    }

    void OnReset()
    {
        OnStartGame();
    }

    void OnGameover()
    {
        OnStartGame();
    }

    void OnChangePlayerCar()
    {
        playerKinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
    }
    #endregion
}

