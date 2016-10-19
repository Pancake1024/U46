using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/OnTheRunSounds")]
public class OnTheRunSounds : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunSounds instance = null;

    public static OnTheRunSounds Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public members
    //public AudioClip offgameMusic;
    public float offgameMusicBaseVolume = 1.0f;

    public AudioClip ingameMusic;
    /*public AudioClip ingameMusicEu;
    public AudioClip ingameMusicAs;
    public AudioClip ingameMusicNy;
    public AudioClip ingameMusicUsa;*/
    public AudioClip ingameMusicBadGuys;
    public AudioClip metersOk;
    public AudioClip[] bonusTurbo;
    public AudioClip bonusMoney;
    public AudioClip bonusBigMoney;
    public AudioClip bonusMagnet;
    public AudioClip bonusShield;
    public AudioClip ramp;

    public AudioClip startBigfoot;
    public AudioClip startTank;
    public AudioClip tankCannon;
    public AudioClip landing;
    public AudioClip landingTank;
    public AudioClip backToCar;
    public AudioClip jingleTv;
    public AudioClip moneyOnAir;
    public AudioClip ufoTractorBeam;
    public AudioClip planeGun;
    public AudioClip firetruckSiren;
    //public AudioClip firetruckHydrant;
    public AudioClip woodHit;
    public AudioClip wind;
    public AudioClip missionCompleted;

    public AudioClip[] speechInitial;
    public AudioClip[] speechBolt;
    public AudioClip[] speechTank;
    public AudioClip[] speechBigfoot;
    public AudioClip[] speechCheckpoint;
    public AudioClip[] speechTruckTurbo;
    public AudioClip[] speechCollision;
    public AudioClip[] speechTimeEnded;
    public AudioClip[] speechBadGuy;
    public AudioClip[] speechHurry;

    public AudioClip[] speechBoom;
    public AudioClip[] speechPoliceEnter;
    public AudioClip[] speechHelicopterEnter;
    public AudioClip[] speechPlaneEnter;
    public AudioClip[] speechUFOEnter;
    public AudioClip[] speechFiretruckEnter;
    public List<ListWrapper> speechReporterFirstPart = new List<ListWrapper>();
    public List<ListWrapper> speechReporterSecondPart = new List<ListWrapper>();

    [System.Serializable]
    public class ListWrapper
    {
        public AudioClip[] speechList;
    }

    public class LocalListWrapper
    {
        public List<AudioClip> speechList;
    }
    #endregion
    
    #region Protected members
    protected OnTheRunGameplay gameplayManager;
    protected PlayerSounds playerEngineSound;
    protected AudioSource bgSource;
    protected AudioSource bonusSource;
    protected AudioSource metersSource;
    protected AudioSource badguysSource;
    protected AudioSource speechSource;
    protected AudioSource windSource;
    protected AudioSource[] gameSoundsSources;
    protected AudioSource firetruckSirenSource;
    //protected AudioSource firetruckHydrantSource;
    protected int gameSourcesNumber = 10;

    protected bool changeBgVolume = false;
    protected bool changeBadGuysVolume = false;
	
    protected float fadeStartTime = -1.0f;
    protected float startVolume;
    protected float finalVolume;

    protected float fadeStartTimeBadGuys = -1.0f;
    protected float startVolumeBadGuys;
    protected float finalVolumeBadGuys;

    protected List<LocalListWrapper> nextSpeechFirstPart;
    protected List<LocalListWrapper> nextSpeechSecondPart;
    protected int firstPartLastIndex = -1;
    protected int secondPartLastIndex = -1;

    protected bool isMusicDownActive = false;

    protected Dictionary<Speech, int> speechIndexes = new Dictionary<Speech, int>();

    public enum GeneralGameSounds
    {
        Ramp,
        StartBigfoot,
        StartTank,
        TankCannon,
        Landing,
        LandingTank,
        BackToCar,
        JingleTv,
        MoneyOnAir,
        UfoTractorBeam,
        PlaneGun,
        WoodHit,
        MissionCompleted
    }

    public enum Speech
    {
        Initial,
        Tank,
        Bigfoot,
        Bolt,
        Checkpoint,
        TruckTurbo,
        Collision,
        TimeEnd,
        BadGuy,
        Hurry,
        Boom,
        PoliceEnter,
        HelicopterEnter,
        PlaneEnter,
        UFOEnter,
        FiretruckEnter,
        ReporterFirtstPart,
        ReporterSecondPart
    }
    #endregion

    #region Public methods
    public void GetRandomspeech(ref AudioClip[] speechList, Speech type)
    {
        int listIndex = speechIndexes[type];
        if (listIndex < 0)
        {
            listIndex = 0;
            OnTheRunUtils.Shuffle<AudioClip>(speechList);
        }

        speechSource.clip = speechList[listIndex];
        ++listIndex;
        if (listIndex >= speechList.Length)
        {
            listIndex = 0;
            OnTheRunUtils.Shuffle<AudioClip>(speechList);
        }
        speechIndexes[type] = listIndex;
    }

    public void PlaySpeech(Speech speechType)
    {
        if (speechSource.isPlaying && speechType!=Speech.HelicopterEnter)
            return;

        float delay = -1.0f;

        switch (speechType)
        {
            case Speech.Initial:
                GetRandomspeech(ref speechInitial, speechType); 
                break;
            case Speech.Tank:
                GetRandomspeech(ref speechTank, speechType); 
                break;
            case Speech.Bigfoot:
                GetRandomspeech(ref speechBigfoot, speechType); 
                break;
            case Speech.Checkpoint:
                GetRandomspeech(ref speechCheckpoint, speechType); 
                break;
            case Speech.TruckTurbo:
                GetRandomspeech(ref speechTruckTurbo, speechType); 
                break;
            case Speech.Collision:
                GetRandomspeech(ref speechCollision, speechType);
                break;
            case Speech.TimeEnd:
                GetRandomspeech(ref speechTimeEnded, speechType);
                break;
            case Speech.BadGuy:
                GetRandomspeech(ref speechBadGuy, speechType);
                break;
            case Speech.Hurry:
                GetRandomspeech(ref speechHurry, speechType);
                break;
            case Speech.Bolt:
                GetRandomspeech(ref speechBolt, speechType);
                break;
            case Speech.Boom:
                GetRandomspeech(ref speechBoom, speechType); 
                break;
            case Speech.PoliceEnter:
                GetRandomspeech(ref speechPoliceEnter, speechType); 
                break;
            case Speech.HelicopterEnter:
                delay = 3.0f;
                OnTheRunSoundsManager.Instance.StopSource(speechSource);
                GetRandomspeech(ref speechHelicopterEnter, speechType); 
                StopCoroutine("PlayReporterSpeech");
                StartCoroutine("PlayReporterSpeech", (delay + speechSource.clip.length));
                break;
            case Speech.PlaneEnter:
                GetRandomspeech(ref speechPlaneEnter, speechType); 
                break;
            case Speech.UFOEnter:
                GetRandomspeech(ref speechUFOEnter, speechType); 
                break;
            case Speech.FiretruckEnter:
                GetRandomspeech(ref speechFiretruckEnter, speechType); 
                break;
        }

        if (delay < 0.0f)
            OnTheRunSoundsManager.Instance.PlaySource(speechSource);
        else
            speechSource.PlayDelayed(delay);
    }
    
    public void StopSpeech( )
    {
        StopCoroutine("PlayReporterSpeech");
        OnTheRunSoundsManager.Instance.StopSource(speechSource);
    }

    void RemoveHelicopter()
    {
        StopCoroutine("PlayReporterSpeech");
    }

    protected void InitializeReporterSpeech(int partIndex=-1)
    {
        if (nextSpeechFirstPart == null || nextSpeechSecondPart == null)
        {
            nextSpeechFirstPart = new List<LocalListWrapper>();
            nextSpeechSecondPart = new List<LocalListWrapper>();
        }

        //FIRST PART-------------------------------------------------
        if (partIndex != 2)
        {
            nextSpeechFirstPart.Clear();
            for (int i = 0; i < speechReporterFirstPart.Count; ++i)
            {
                LocalListWrapper listWrapper = new LocalListWrapper();
                listWrapper.speechList = new List<AudioClip>();
                ListWrapper tempWrapper = speechReporterFirstPart[i];
                for (int j = 0; j < tempWrapper.speechList.Length; ++j)
                {
                    listWrapper.speechList.Add(tempWrapper.speechList[j]);
                }
                OnTheRunUtils.Shuffle<AudioClip>(listWrapper.speechList);
                nextSpeechFirstPart.Add(listWrapper);
            }
            firstPartLastIndex = 0;
        }

        //SECOND PART-------------------------------------------------
        if (partIndex != 1)
        {
            nextSpeechSecondPart.Clear();
            for (int i = 0; i < speechReporterSecondPart.Count; ++i)
            {
                LocalListWrapper listWrapper = new LocalListWrapper();
                listWrapper.speechList = new List<AudioClip>();
                ListWrapper tempWrapper = speechReporterSecondPart[i];
                for (int j = 0; j < tempWrapper.speechList.Length; ++j)
                {
                    listWrapper.speechList.Add(tempWrapper.speechList[j]);
                }
                OnTheRunUtils.Shuffle<AudioClip>(listWrapper.speechList);
                nextSpeechSecondPart.Add(listWrapper);
            }
            secondPartLastIndex = 0;
        }
    }

    public IEnumerator PlayReporterSpeech(float delay)
    {
        yield return new WaitForSeconds(delay);

        speechSource.Stop();
        OnTheRunSounds.Instance.StartWind();

        if (nextSpeechFirstPart[firstPartLastIndex].speechList.Count == 0)
            InitializeReporterSpeech(1);

        AudioClip nextSpeech = nextSpeechFirstPart[firstPartLastIndex].speechList[0];
        nextSpeechFirstPart[firstPartLastIndex].speechList.RemoveAt(0);
        speechSource.clip = nextSpeech;
        OnTheRunSoundsManager.Instance.PlaySource(speechSource);
        ++firstPartLastIndex;
        if (firstPartLastIndex >= nextSpeechFirstPart.Count)
            firstPartLastIndex = 0;

        yield return new WaitForSeconds(speechSource.clip.length);

        speechSource.Stop();
        if (nextSpeechSecondPart[secondPartLastIndex].speechList.Count == 0)
            InitializeReporterSpeech(2);

        nextSpeech = nextSpeechSecondPart[secondPartLastIndex].speechList[0];
        nextSpeechSecondPart[secondPartLastIndex].speechList.RemoveAt(0);
        speechSource.clip = nextSpeech;
        OnTheRunSoundsManager.Instance.PlaySource(speechSource);
        StartCoroutine(StopWindDelayed(speechSource.clip.length));
        ++secondPartLastIndex;
        if (secondPartLastIndex >= nextSpeechSecondPart.Count)
            secondPartLastIndex = 0;
    }

    IEnumerator FadeOutReporterSpeech()
    {
        while (speechSource.volume > .1F)
        {
            speechSource.volume = Mathf.Lerp(speechSource.volume, 0F, Time.deltaTime);
            yield return 0;
        }

        StopCoroutine("PlayReporterSpeech");
        OnTheRunSounds.Instance.StopReduceMusicVolume();
        OnTheRunSounds.Instance.StopWind();
        speechSource.Stop();
        speechSource.volume = 1.0f;
    }
    
    IEnumerator StopWindDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        OnTheRunSounds.Instance.StopReduceMusicVolume();
        OnTheRunSounds.Instance.StopWind();
    }
    
    public void PlayInGameMusic( )
    {
        OnTheRunEnvironment.Environments currEnvironment = gameplayManager.GetComponent<OnTheRunEnvironment>().currentEnvironment;

        OnTheRunSoundsManager.Instance.StopMusicSource();
        OnTheRunSoundsManager.Instance.musicClip = null;

        /*switch (currEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe: ingameMusic = ingameMusicEu; break;
            case OnTheRunEnvironment.Environments.Asia: ingameMusic = ingameMusicAs; break;
            case OnTheRunEnvironment.Environments.NY: ingameMusic = ingameMusicNy; break;
            case OnTheRunEnvironment.Environments.USA: ingameMusic = ingameMusicUsa; break;
        }*/

        //fadeStartTime = -1.0f;
        gameObject.SendMessage("StopAllSources");

        AudioClip envClip = ingameMusic; //ambientSounds[(int)RunnerTrack.Instance.gameObject.GetComponent<RunnerEnvironment>().CurrentEnvironment];
        if (null == envClip)
            envClip = ingameMusic;

        bgSource.clip = envClip;// ingameMusic;
        bgSource.loop = true;
        bgSource.mute = false;
        //bgSource.volume = 0.2f;
        OnTheRunSoundsManager.Instance.musicClip = ingameMusic;
        OnTheRunSoundsManager.Instance.PlayMusicSource(bgSource);
    }

    public void PauseInGameMusic()
    {
    }

    public bool IsInGameMusicPlaying( )
    {
        return bgSource.isPlaying;
    }

    public void StartWind()
    {
        windSource.clip = wind;
        OnTheRunSoundsManager.Instance.PlaySource(windSource);
    }

    public void StopWind()
    {
        OnTheRunSoundsManager.Instance.StopSource(windSource);
    }

    public void StartReduceMusicVolume()
    {
        if (!changeBgVolume)
            bgVolumeControl(true);
    }

    public void StopReduceMusicVolume()
    {
        if(!changeBgVolume)
            bgVolumeControl(false);
    }

    public void StartFiretruckSounds()
    {
        OnTheRunSoundsManager.Instance.PlaySource(firetruckSirenSource);
        //OnTheRunSoundsManager.Instance.PlaySource(firetruckHydrantSource);
    }

    public void StopFiretruckSounds()
    {
        OnTheRunSoundsManager.Instance.StopSource(firetruckSirenSource);
        //OnTheRunSoundsManager.Instance.StopSource(firetruckHydrantSource);
    }
    
    public void ResetIngameMusic()
    {
        changeBgVolume = false;
        changeBadGuysVolume = false;
        bgSource.volume = 1.0f;
        badguysSource.volume = 0.0f;
        OnTheRunSoundsManager.Instance.StopSource(badguysSource);
        OnTheRunSoundsManager.Instance.StopMusicSource();
    }

    public void CheckForMusicDownVolume(AudioSource source)
    {
        if (isMusicDownActive)
            source.volume = 0.5f;
        else
            source.volume = 1.0f;
    }
    public AudioSource FreeAudioSource(AudioSource[] sources)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying)
            {
                return sources[i];
            }
        }
        return null;
    }

    public void PlayGeneralGameSound(GeneralGameSounds soundType)
    {
        AudioSource currentSource = FreeAudioSource(gameSoundsSources);
        if (currentSource != null)
        {
            switch (soundType)
            {
                case GeneralGameSounds.Ramp:
                    currentSource.clip = ramp;
                    break;
                case GeneralGameSounds.StartBigfoot:
                    currentSource.clip = startBigfoot;
                    break;
                case GeneralGameSounds.StartTank:
                    currentSource.clip = startTank;
                    break;
                case GeneralGameSounds.TankCannon:
                    currentSource.clip = tankCannon;
                    break;
                case GeneralGameSounds.Landing:
                    currentSource.clip = landing;
                    break;
                case GeneralGameSounds.LandingTank:
                    currentSource.clip = landingTank;
                    break;
                case GeneralGameSounds.BackToCar:
                    currentSource.clip = backToCar;
                    break;
                case GeneralGameSounds.JingleTv:
                    currentSource.clip = jingleTv;
                    break;
                case GeneralGameSounds.MoneyOnAir:
                    currentSource.clip = moneyOnAir;
                    break;
                case GeneralGameSounds.UfoTractorBeam:
                    currentSource.clip = ufoTractorBeam;
                    break;
                case GeneralGameSounds.PlaneGun:
                    currentSource.clip = planeGun;
                    break;
                case GeneralGameSounds.WoodHit:
                    currentSource.clip = woodHit;
                    break;
                case GeneralGameSounds.MissionCompleted:
                    currentSource.clip = missionCompleted;
                    break;
            }

            CheckForMusicDownVolume(currentSource);

            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
        }
    }

    public void PlayBonusCollected(OnTheRunObjectsPool.ObjectType bonusType)
    {
        AudioSource currentSource = FreeAudioSource(gameSoundsSources);
        if (currentSource != null)
        {
            switch (bonusType)
            {
                case OnTheRunObjectsPool.ObjectType.BonusMoney:
                    currentSource.clip = bonusMoney;
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusMoneyBig:
                    currentSource.clip = bonusBigMoney;
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusMagnet:
                    currentSource.clip = bonusMagnet;
                    break;
                case OnTheRunObjectsPool.ObjectType.BonusShield:
                    currentSource.clip = bonusShield;
                    break;
            }

            CheckForMusicDownVolume(currentSource);

            OnTheRunSoundsManager.Instance.PlaySource(currentSource);
        }
    }

    public void PlayTurboBonusCollected(int count)
    {
        bonusSource.clip = bonusTurbo[count - 1];

        CheckForMusicDownVolume(bonusSource);

        OnTheRunSoundsManager.Instance.PlaySource(bonusSource);
    }

    public void PlayMetersOk()
    {
        metersSource.clip = metersOk;
        OnTheRunSoundsManager.Instance.PlaySource(metersSource);
    }
    	
    public void StopIngameMusic()
    {
        OnTheRunSoundsManager.Instance.StopMusicSource();
    }

    public void bgVolumeControl(bool down)
    {
        isMusicDownActive = down;
        fadeStartTime = Time.time;
        changeBgVolume = true;
        startVolume = bgSource.volume;
        if (down)
        {
            finalVolume = 0.1f;
        }
        else
        {
            finalVolume = 1.0f;  //music
        }
    }
    
    public void badguysVolumeControl(bool down)
    {
        fadeStartTimeBadGuys = Time.time;
        changeBadGuysVolume = true;
        startVolumeBadGuys = badguysSource.volume;
        if (down)
        {
            finalVolumeBadGuys = 0.0f;
        }
        else
        {
            finalVolumeBadGuys = 1.0f;
        }
    }
    
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        bgSource = gameObject.AddComponent<AudioSource>();
        bgSource.volume = 1.0f; //music
        bonusSource = gameObject.AddComponent<AudioSource>();
        bonusSource.volume = 1.0f;
        metersSource = gameObject.AddComponent<AudioSource>();
        metersSource.volume = 1.0f;
        badguysSource = gameObject.AddComponent<AudioSource>();
        badguysSource.volume = 0.0f;
        badguysSource.loop = true;
        speechSource = gameObject.AddComponent<AudioSource>();
        speechSource.volume = 1.0f;
        windSource = gameObject.AddComponent<AudioSource>();
        windSource.volume = 1.0f;
        windSource.loop = true;
        startVolume = bgSource.volume;
        finalVolume = 0.1f;

        gameSoundsSources = new AudioSource[gameSourcesNumber];
        for (int i = 0; i < gameSoundsSources.Length; i++)
        {
            gameSoundsSources[i] = gameObject.AddComponent<AudioSource>();
            gameSoundsSources[i].playOnAwake = false;
            gameSoundsSources[i].volume = 1.0f;
            gameSoundsSources[i].loop = false;
        }

        speechIndexes.Add(Speech.BadGuy, -1);
        speechIndexes.Add(Speech.Bigfoot, -1);
        speechIndexes.Add(Speech.Bolt, -1);
        speechIndexes.Add(Speech.Boom, -1);
        speechIndexes.Add(Speech.Checkpoint, -1);
        speechIndexes.Add(Speech.Collision, -1);
        speechIndexes.Add(Speech.FiretruckEnter, -1);
        speechIndexes.Add(Speech.HelicopterEnter, -1);
        speechIndexes.Add(Speech.Hurry, -1);
        speechIndexes.Add(Speech.Initial, -1);
        speechIndexes.Add(Speech.PlaneEnter, -1);
        speechIndexes.Add(Speech.PoliceEnter, -1);
        speechIndexes.Add(Speech.ReporterFirtstPart, -1);
        speechIndexes.Add(Speech.ReporterSecondPart, -1);
        speechIndexes.Add(Speech.Tank, -1);
        speechIndexes.Add(Speech.TimeEnd, -1);
        speechIndexes.Add(Speech.TruckTurbo, -1);
        speechIndexes.Add(Speech.UFOEnter, -1);

        InitializeFiretruckSounds();
        InitializeReporterSpeech();
    }

    void InitializeFiretruckSounds()
    {
        firetruckSirenSource = gameObject.AddComponent<AudioSource>();
        firetruckSirenSource.volume = 1.0f;
        firetruckSirenSource.loop = true;
        if (firetruckSiren != null)
            firetruckSirenSource.clip = firetruckSiren;

        /*
        firetruckHydrantSource = gameObject.AddComponent<AudioSource>();
        firetruckHydrantSource.volume = 1.0f;
        firetruckHydrantSource.loop = true;
        if (firetruckHydrant != null)
            firetruckHydrantSource.clip = firetruckHydrant;
        */
    }

    void Start()
    { 
        startVolume = bgSource.volume;
        //turboCounter = 0;
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
    }
    
    void Update()
    {
        float fadeTime;
        float fadeTimeBadGuys;
        float now = Time.time;
        if (changeBgVolume && !TimeManager.Instance.MasterSource.IsPaused)
        {
            if (playerEngineSound == null)
                playerEngineSound = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>().GetComponent<PlayerSounds>();

            if (startVolume > finalVolume)
            {
                fadeTime = now - fadeStartTime;
                if (bgSource.volume <= finalVolume)
                {
                    playerEngineSound.UpdatePlayerEngineVolume(finalVolume);
                    bgSource.volume = finalVolume;
                    changeBgVolume = false;
                }
            }
            else
            {
                fadeTime = fadeStartTime - now;
                if (bgSource.volume >= finalVolume)
                {
                    playerEngineSound.UpdatePlayerEngineVolume(finalVolume);
                    bgSource.volume = finalVolume;
                    changeBgVolume = false;
                }
            }
            bgSource.volume = startVolume - fadeTime * 0.6f;
            playerEngineSound.UpdatePlayerEngineVolume(bgSource.volume);
        }

        if (changeBadGuysVolume && !TimeManager.Instance.MasterSource.IsPaused)
        {
            if (startVolumeBadGuys > finalVolumeBadGuys)
            {
                fadeTimeBadGuys = now - fadeStartTimeBadGuys;
                if (badguysSource.volume <= finalVolumeBadGuys)
                {
                    badguysSource.volume = finalVolumeBadGuys;
                    changeBadGuysVolume = false;
                    OnTheRunSoundsManager.Instance.StopSource(badguysSource);
                }
            }
            else
            {
                fadeTimeBadGuys = fadeStartTimeBadGuys - now;
                if (badguysSource.volume >= finalVolumeBadGuys)
                {
                    badguysSource.volume = finalVolumeBadGuys;
                    changeBadGuysVolume = false;
                }
            }
            badguysSource.volume = startVolumeBadGuys - fadeTimeBadGuys * 0.6f;
        }
    }

    /*
    void Start()
    {
        
        //this.PlayOffGameMusic();
    }

    void Update()
    {
        if (fadeStartTime > 0.0f)
        {
            AudioClip lavaClip = ambientSounds[(int)RunnerEnvironment.Type.Lava];
            float t = (TimeManager.Instance.MasterSource.TotalTime - fadeStartTime) * (fadeTarget < fadeSource ? (nextBgClip == lavaClip ? 4.0f : 0.66f) : (bgSource.clip == lavaClip ? 4.0f : 2.0f));
            if (t >= 1.0f)
            {
                bgSource.volume = fadeTarget;
                fadeStartTime = -1.0f;

                if (nextBgClip != null)
                {
                    bgSource.clip = nextBgClip;
                    bgSource.loop = true;
                    bgSource.volume = 0.0f;
                    OnTheRunSoundsManager.Instance.PlayMusicSource(bgSource);
                    OnTheRunSoundsManager.Instance.musicClip = nextBgClip;

                    fadeStartTime = TimeManager.Instance.MasterSource.TotalTime;
                    fadeSource = 0.0f;
                    fadeTarget = nextBgVolume;

                    nextBgClip = null;
                }
            }
            else
            {
                t = Mathf.Clamp01(t);
                bgSource.volume = fadeSource * (1.0f - t) + fadeTarget * t;
            }
        }
    }
	*/

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
