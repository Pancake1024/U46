using UnityEngine;
using System.Collections;

public class OnTheRunInterfaceSounds : MonoBehaviour
{
    public static OnTheRunInterfaceSounds Instance = null;

    #region Public members
    public AudioClip scroll_car;
    public AudioClip popup;
    public AudioClip buy;
    public AudioClip change_location;
    public AudioClip counter_star;
    public AudioClip counter_coin;
    public AudioClip panel_exit;
    public AudioClip panel_enter;
    public AudioClip padlock_open;

    public AudioClip spin_peg;
    public AudioClip spin_coin;
    public AudioClip spin_powerup;
    public AudioClip spin_main;
    public AudioClip fireworks;
    public AudioClip click;
    public AudioClip offgameMusic;
    
    public AudioClip jinglePos;
    public AudioClip jingleNeg;
    public AudioClip hurryUp;
    public AudioClip ready;
    public AudioClip go;
    public AudioClip swish;
    public AudioClip endingTime;
    public AudioClip turboExplosion;
    public AudioClip[] checkPointClips;
    #endregion

    #region Protected members
    protected AudioSource[] interfaceSources;
    protected AudioSource musicSource;
    protected AudioSource jingleSource;
    protected AudioSource endingTimeSource;
    protected AudioSource spinwheelSource;
    #endregion

    #region public enums

    public enum InterfaceSoundsType
    {
        Click,
        HurryUp,
        Ready,
        Go,
        SwishButton,
        Turbo
    }
    #endregion

    #region Public properties

    #endregion

    #region Functions

    public AudioSource FreeAudioSource(AudioSource[] sources)
    {
        if (sources == null)
            return null;

        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying)
            {
                return sources[i];
            }
        }
        return null;
    }

    #endregion


    #region Messages
    public void PlayOffGameMusic()
    {
        AudioClip audioClip = offgameMusic;
        
        //Debug.Log("PlayOffGameMusic ");
        OnTheRunSoundsManager.Instance.StopMusicSource();
        OnTheRunSoundsManager.Instance.musicClip = null;

        musicSource.clip = audioClip;
        OnTheRunSoundsManager.Instance.musicClip = audioClip;
        OnTheRunSoundsManager.Instance.PlayMusicSource(musicSource);
        OnTheRunSoundsManager.Instance.MusicVolume = OnTheRunSounds.Instance.offgameMusicBaseVolume;
    }

    void StopOffGameMusic()
    {
        OnTheRunSoundsManager.Instance.StopMusicSource();
    }

    void PlayerIsDead()
    {
        //PlayJingle();
    }


    public void PlayUnlockSound()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);

        currentSource.clip = padlock_open;

        if (currentSource != null)
            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }

    void PlayJingle()
    {
        OnTheRunSounds.Instance.StopIngameMusic();
        jingleSource.clip = jinglePos;
        OnTheRunSoundsManager.Instance.PlaySource(jingleSource);
    }

    void PlayCheckpointSound()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = checkPointClips[Random.Range(0, checkPointClips.Length)];
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);

        //jingleSource.clip = checkPointClips[Random.Range(0, checkPointClips.Length)];
        //OnTheRunSoundsManager.Instance.PlaySource(jingleSource);
    }

    void PlayGeneralInterfaceSound(InterfaceSoundsType soundType)
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        if (currentSource != null)
        {
            switch (soundType)
            {
                case InterfaceSoundsType.Click:
                    currentSource.clip = click;
					OnTheRunSoundsManager.Instance.PlaySource(currentSource, true);
					return;
                    break;
                case InterfaceSoundsType.HurryUp:
                    currentSource.clip = hurryUp;
                    break;
                case InterfaceSoundsType.Ready:
                    currentSource.clip = ready;
                    break;
                case InterfaceSoundsType.Go:
                    currentSource.clip = go;
                    break;
                case InterfaceSoundsType.SwishButton:
                    currentSource.clip = swish;
                    break;
                case InterfaceSoundsType.Turbo:
                    currentSource.clip = turboExplosion;
                    break;
            }
            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
        }
    }

    void PlayEndingTimeSound()
    {
        endingTimeSource.time = 0.0f;
        OnTheRunSoundsManager.Instance.PlaySource(endingTimeSource);
    }

    void StopEndingTimeSound()
    {
        OnTheRunSoundsManager.Instance.StopSource(endingTimeSource);
    }

    protected float endingTimeTime = 0.0f;
    void PauseEndingTimeSound(bool inPause)
    {
        if (inPause)
        {
            endingTimeTime = endingTimeSource.time;
            OnTheRunSoundsManager.Instance.StopSource(endingTimeSource);
        }
        else
        {
            if (endingTimeTime != 0.0f)
            {
                endingTimeSource.time = endingTimeTime;
                OnTheRunSoundsManager.Instance.PlaySource(endingTimeSource);//.Play();
                endingTimeTime = 0.0f;
            }
        }
    }

    void StopAllSources()
    {
        jingleSource.Stop();
    }
    #endregion

    #region Unity Callbacks

    void Awake()
    {
        Instance = this;

        interfaceSources = new AudioSource[20];
        for (int i = 0; i < interfaceSources.Length; i++)
        {
            interfaceSources[i] = gameObject.AddComponent<AudioSource>();
            interfaceSources[i].volume = 1.0f;
            interfaceSources[i].loop = false;
            interfaceSources[i].playOnAwake = false;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.volume = 1.0f;
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        jingleSource = gameObject.AddComponent<AudioSource>();
        jingleSource.volume = 1.0f;
        jingleSource.playOnAwake = false;
        jingleSource.loop = false;

        endingTimeSource = gameObject.AddComponent<AudioSource>();
        endingTimeSource.volume = 1.0f;
        endingTimeSource.playOnAwake = false;
        endingTimeSource.loop = false;
        endingTimeSource.clip = endingTime;
    }

    void Start()
    {
        PlayOffGameMusic();
    }
    /*
    void Update()
    { }
    */
    #endregion

    //-------------------------------------------//
    public void ScrollCar()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = scroll_car;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void Popup()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = popup;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void Buy()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = buy;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void ChangeLocation()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = change_location;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void CounterStar()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = counter_star;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void CounterCoin()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = counter_coin;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void PanelExit()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = panel_exit;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void PanelEnter()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = panel_enter;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }



    protected float lastTickSoundTime = -1.0f;
    protected float minTickInterval = 0.035f;
    // Wheel sounds
    //-------------------------------------------//
    public void WheelTick()
    {
        float now = TimeManager.Instance.MasterSource.TotalTime;
        bool tooNear = now - lastTickSoundTime < minTickInterval;
        if (!tooNear)
        {
            AudioSource currentSource = FreeAudioSource(interfaceSources);
            currentSource.clip = spin_peg;
            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
            lastTickSoundTime = now;
        }
    }
    //-------------------------------------------//
    public void WheelCarOrFuel()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = spin_powerup;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void WheelCoinOrDiamonds()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = spin_coin;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
    public void WheelExtraSpin()
    {
        spinwheelSource = FreeAudioSource(interfaceSources);
        spinwheelSource.clip = spin_main;
        OnTheRunSoundsManager.Instance.PlaySource(spinwheelSource);
    }
    public void StopWheelExtraSpinSound()
    {
        if (spinwheelSource != null)
            OnTheRunSoundsManager.Instance.StopSource(spinwheelSource);
    }
    //-------------------------------------------//
    public void PlayFireworkSound()
    {
        AudioSource currentSource = FreeAudioSource(interfaceSources);
        currentSource.clip = fireworks;
        OnTheRunSoundsManager.Instance.PlaySource(currentSource);
    }
    //-------------------------------------------//
}
