using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSounds : MonoBehaviour 
{
    #region Public members
    public AudioClip[] hits;
    public AudioClip engineClip;
    public AudioClip turboStartClip;
    //public AudioClip turboClip;
    #endregion
    
    #region Protected members
    protected AudioSource[] playerSources;
    protected AudioSource engineSource;
    protected float engineSourceDelayTimer = -1.0f;
    protected float playerSpeed = 0f;
    protected PlayerKinematics playerKinematics;
    protected float minEnginePitch = 0.5f;
    #endregion
    
    #region public enums
    public enum PlayerSoundsType
    {
        Hit,
        TurboStart
    }
    #endregion

    #region Public properties
    #endregion
    
    #region Functions
    public AudioSource FreeAudioSource(AudioSource[] sources)
    {
        for (int i = 0; i < sources.Length; i++)
            if (!sources[i].isPlaying)
                return sources[i];
        return null;
    }

    public AudioClip GetRandomClip(AudioClip[] clipsArray)
    {
        AudioClip currentClip =clipsArray[Random.Range(0, 10000 % clipsArray.Length)];
        return currentClip;
    }
    #endregion

    #region Messages
    void OnStartGame()
    {
        //Debug.Log("engineSource Resetted");
        engineSource.Stop();
        engineSource.volume = 1.0f;
        engineSource.pitch = minEnginePitch;
    }

    void OnChangePlayerCar()
    {
        //Debug.Log("OnChangePlayerCar");
        engineSource.Stop();
        engineSource.pitch = minEnginePitch;
        engineSourceDelayTimer = 1.0f;
        engineSource.volume = 1.0f;
    }

    void OnPausePlayerEngine()
    {
        engineSource.volume = 0.0f;
    }

    void OnRestartPlayerEngine()
    {
        //Debug.Log("OnRestartPlayerEngine");
        engineSource.volume = 1.0f;
    }

    public void UpdatePlayerEngineVolume(float vol)
    {
        //Debug.Log("UpdatePlayerEngineVolume");
        engineSource.volume = vol;
    }

    void PlayPlayerSound(PlayerSoundsType soundType)
    {
        //Debug.Log("PlayPlayerSound " + soundType);
        AudioSource currentSource = FreeAudioSource(playerSources);
        if(currentSource != null)
        {
            switch(soundType)
            {
                case PlayerSoundsType.Hit:
                    currentSource.clip = GetRandomClip(hits);
                    break;
                case PlayerSoundsType.TurboStart:
                    currentSource.clip = turboStartClip;
                    break;
            }
            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
        }
    }

    void OnTurboActive(bool active)
    {
        if (active)
        {
            PlayPlayerSound(PlayerSoundsType.TurboStart);
        }
    }

    void OnSlipstreamActive(bool active)
    {
        if (active)
        {
            PlayPlayerSound(PlayerSoundsType.TurboStart);
        }
    }
    #endregion
    
    #region Unity Callbacks
    void Awake()
    {
        playerSources = new AudioSource[3];
        for (int i = 0; i< playerSources.Length; i++)
        {
            playerSources[i] = gameObject.AddComponent<AudioSource>();
            playerSources[i].volume = 1.0f;
            playerSources[i].loop = false;
            playerSources[i].playOnAwake = false;

            playerSources[i].rolloffMode = AudioRolloffMode.Linear;
            playerSources[i].minDistance = 10.0f;
            playerSources[i].maxDistance = 150.0f;
            playerSources[i].velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        }

        engineSource = gameObject.AddComponent<AudioSource>();
        engineSource.volume = 0.0f;
        engineSource.playOnAwake = false;
        engineSource.loop = true;
        engineSource.clip = engineClip;
        engineSource.pitch = minEnginePitch;
        
        engineSource.clip = engineClip;
        engineSource.rolloffMode = AudioRolloffMode.Linear;
        engineSource.minDistance = 1.0f;
        engineSource.maxDistance = 60.0f;
        engineSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
    }

    void Start()
    {
        playerKinematics = gameObject.GetComponent<PlayerKinematics>();
    }

    void Update()
    {
        //Debug.Log("player speed: " + playerKinematics.Speed);

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        /*
        if (playerKinematics.TurboOn && engineSource.clip != turboClip)
            engineSource.clip = turboClip;
        else if (!playerKinematics.TurboOn && engineSource.clip != engineClip)
            engineSource.clip = engineClip;
        */

        if (playerKinematics.Speed > 0f && !engineSource.isPlaying && !playerKinematics.PlayerIsDead && !TimeManager.Instance.MasterSource.IsPaused)
        {
            OnTheRunSoundsManager.Instance.PlaySource(engineSource);
        }

        if (engineSource.isPlaying && !playerKinematics.PlayerIsDead)
        {
            //engineSource.pitch = minEnginePitch + playerKinematics.Speed * dt; // Ma che e' sta' roba????
            //engineSource.pitch = minEnginePitch + playerKinematics.Speed * 0.015f;
            //if (engineSource.clip == turboClip)
            //    engineSource.pitch = 0.5f;
            //else
            engineSource.pitch = Mathf.Clamp(minEnginePitch + playerKinematics.Speed * 0.015f, minEnginePitch, 2.4f);
            //Debug.Log("pitch " + engineSource.pitch + " SPEED: " + playerKinematics.Speed);
        }

        if (playerKinematics.PlayerIsDead)
        {
            engineSource.pitch = minEnginePitch;
        }

        if (engineSourceDelayTimer >= 0.0f && !TimeManager.Instance.MasterSource.IsPaused)
        {
            engineSourceDelayTimer -= dt;
            if (engineSourceDelayTimer < 0.0f)
            {
                engineSourceDelayTimer = -1.0f;
                OnRestartPlayerEngine();
            }
        }
    }
    #endregion
}
