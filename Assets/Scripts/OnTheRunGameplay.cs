using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.UI;
using System.Collections;

public class OnTheRunGameplay : MonoBehaviour
{
	public static OnTheRunGameplay Instance = null;

    public static bool oldSystemActive = false;
    public static bool stopTimeDuringSpecialCars = false;
    public static int availableCarsNumber = 20;

    public static string GetCarLocalizedStr(CarId carId)
    {
        string res = "";
        switch (carId)
        {
            case CarId.Bigfoot:
                res = OnTheRunDataLoader.Instance.GetLocaleString("bigfoot");
                break;

            case CarId.Plane:
                res = OnTheRunDataLoader.Instance.GetLocaleString("plane");
                break;

            case CarId.Tank:
                res = OnTheRunDataLoader.Instance.GetLocaleString("tank");
                break;

            case CarId.Ufo:
                res = OnTheRunDataLoader.Instance.GetLocaleString("ufo");
                break;

            case CarId.Firetruck:
                res = OnTheRunDataLoader.Instance.GetLocaleString("firetruck");
                break;

        }
        return res;
    }

    public static string GetCarLocalizedDescrStr(TruckBehaviour.TrasformType carId)
    {
        string res = "";
        switch (carId)
        {
            case TruckBehaviour.TrasformType.Bigfoot:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_bigfoot_short_descr");
                break;

            case TruckBehaviour.TrasformType.Plane:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_plane_short_descr");
                break;

            case TruckBehaviour.TrasformType.Tank:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_tank_short_descr");
                break;

            case TruckBehaviour.TrasformType.Ufo:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_ufo_short_descr");
                break;

            case TruckBehaviour.TrasformType.Firetruck:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_firetruck_short_descr");
                break;

        }
        return res;
    }

    public static string GetCarLocalizedLongDescrStr(CarId carId)
    {
        string res = "";
        switch (carId)
        {
            case CarId.Bigfoot:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_bigfoot_long_descr");
                break;

            case CarId.Plane:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_plane_long_descr");
                break;

            case CarId.Tank:
#if UNITY_WEBPLAYER
                res = OnTheRunDataLoader.Instance.GetLocaleString("web_special_tank_long_descr");
#else
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_tank_long_descr");
#endif
                break;

            case CarId.Ufo:
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_ufo_long_descr");
                break;

            case CarId.Firetruck:
#if UNITY_WEBPLAYER
                res = OnTheRunDataLoader.Instance.GetLocaleString("web_special_firetruck_long_descr");
#else
                res = OnTheRunDataLoader.Instance.GetLocaleString("special_firetruck_long_descr");
#endif
                break;

        }
        return res;
    }

    #region Public Members
    protected float[] checkpointTimeList;
    protected float checkpointInitTime = 120.0f;
    protected float checkpointDecreaseTime = 1.0f;
    protected float checkpointMinTime = 10.0f;
    protected float collisionProtectionTime = 1.0f;
    public List<GameObject> carsList;
    protected float specialCarTime = 5.0f;
    protected float specialCarDeltaTime = 1.0f;
    protected float specialCarBoostTime = 4.0f;
    public AudioClip blinkCarSound;
    [HideInInspector]
    public float coinsExponent = 2.0f;
    [HideInInspector]
    public float coinsBase = 1.0f;
    [HideInInspector]
    public float coinsStep = 1.0f;
    public float helpingHandDistance = 30.0f;

    [HideInInspector]
    public bool checkForDailyBonusAfterPause = false;

    public GameObject[] Cars_EU;
    public GameObject[] Cars_NY;
    public GameObject[] Cars_JP;
    public GameObject[] Cars_USA;
    
    [NonSerialized]
    public bool restartFromSaveMe = false;

    public ParticleSystem slipstreamFX_1;
    public ParticleSystem slipstreamFX_2;
    public ParticleSystem slipstreamFX_3;
    public ParticleSystem slipstreamFX_4;
    public ParticleSystem slipstreamFX_5;
    public ParticleSystem slipstreamFX_6;
    #endregion

    #region Protected Members
    protected UIManager uiManager;
    protected AudioSource blinkCarSource;
    protected UISharedData uiSharedData;
    protected GameObject EnvironmentSceneGo;
    protected OnTheRunSpawnManager spawnManager;
    protected OnTheRunEnemiesManager enemiesManager;
    protected OnTheRunEnvironment environmentManager;
    protected GameObject cameraManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UIIngamePage ingamePage;
    protected GameplayStates gameState = GameplayStates.Start;
    protected PlayerKinematics player;
    protected CarId specialCarInUse = CarId.None;
    protected bool tankActivateFlag = false;
    protected int turboCounter = 0;
    protected float turboRatio = -1.0f;
    protected float truckTurboRatio = -1.0f;
    protected float checkpointShowTime;
    protected float gameplayTime;
    protected float initGameplayTime;
    protected float freezeGameplayTimeTimer = -1.0f;
    protected bool gameplaystarted = false;
    protected bool wasgameplaystarted = true;
    protected bool isPlayerSpecialCar = false;
    protected float carChangedTimer = -1.0f;
    protected float carTurboTimer = -1.0f;
    protected float blinkCarTimer = -1.0f;
    protected bool isMagnetActive = false;
    protected bool isBoltMagnetActive = false;
    protected float magnetBonusTimer = -1.0f;
    protected int checkpointCounter = 0;
    protected float prevFixedDeltaTime;
    protected bool endingTimePlayed = false;
    protected bool endingTimeSpeechPlayed = false;
    protected bool isBadGuyActive = false;
    protected bool isHelicopterActive = false;
    protected int coinsForSession = 0;
    protected int scoreForSession = 0;
    protected GameObject carBaseSelectedRef;
    protected GameObject carSelectedRef;
    protected string envPostfixLoaded = "none";
    protected bool resumeEndTimeSound = false;
    protected int boosterMoreCheckpointTimeSeconds = 0;

    protected float refreshFBNotificationTime = 30.0f;
    protected float refreshFBNotificationTimer;

    protected int slipstreamCounter = 0;
    protected int maxCombo = 0;
    protected int hitsCounter = 0;
    protected bool[] initboosters;

    //boosters-----
    protected bool startWithTurbo = false;
    protected int moneyMultiplier = 1;
    protected int expMultiplier = 1;
    protected bool startWithSpecialCar = false;
    protected bool wasStartWithSpecialCar = false;

    //Save me popup
    protected bool firstTimeSaveMeShown = true;
    protected int saveMeCounter = 0;

    #endregion

    #region Public Properties
    public bool logIsEnabled = false;
    protected float turboDuration;
    protected float slipstreamDuration;
    #endregion

    #region Public Enums
    public enum RunParameters
    {
        Slipstream,
        MaxCombo,
        HitsCount,
        Booster
    }

    public enum GameplayStates
    { 
        Start,
        Paused,
        Racing,
        GameOver,
        ReadyToRace,
        Offgame
    }

    public enum CarId
    {
        None = 0,
        Bigfoot,
        Tank,
        Car_1_europe,
        Car_2_europe,
        Car_3_europe,
        Car_4_europe,
        Car_5_europe,
        Car_1_american,
        Car_2_american,
        Car_3_american,
        Car_4_american,
        Car_5_american,
        Car_1_oriental,
        Car_2_oriental,
        Car_3_oriental,
        Car_4_oriental,
        Car_5_oriental,
        Car_1_muscle,
        Car_2_muscle,
        Car_3_muscle,
        Car_4_muscle,
        Car_5_muscle,
        Firetruck,
        Ufo,
        Plane
    }
    #endregion

    #region Public Properties
    public int SlipstreamRunCount
    {
        get { return slipstreamCounter; }
    }

    public int MaxRunCombo
    {
        get { return maxCombo; }
    }

    public int HitsRunCount
    {
        get { return hitsCounter; }
    }

    public bool[] InitBoosters
    {
        get { return initboosters; }
    }

    public bool FirstTimeSaveMeShown
    {
        get { return firstTimeSaveMeShown; }
        set { firstTimeSaveMeShown = value;}
    }

    public float CollisionProtectionTime
    {
        get
        {
            return collisionProtectionTime;
        }
    }

    public int MoneyMultiplier
    {
        get
        {
            return moneyMultiplier;
        }
    }

    public int ExperienceMultiplier
    {
        get
        {
            return expMultiplier;
        }
    }
    
    public GameObject BaseParkingLotCar
    {
        get
        {
            return carBaseSelectedRef;
        }
    }

    public GameObject LastUsedCar
    {
        get
        {
            return carSelectedRef;
        }
    }

    public Camera MainCamera
    {
        get {
            if (cameraManager == null)
                cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
            return cameraManager.GetComponent<Camera>(); 
        }
    }
    
    public int CoinsCollected
    {
        get
        {
            return coinsForSession;
        }
        set
        {
            float deltaCoins = value - coinsForSession;

			//if( OnTheRunSingleRunMissions.Instance != null )
			//	OnTheRunSingleRunMissions.Instance.OnMissionCoinsCollected( (int)deltaCoins );

            coinsForSession = value;

            ingamePage.SendMessage("UpdateIngameCoinsText", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    public int CurrentRunScore
    {
        get
        { 
            return scoreForSession;
        }
        set
        {
            scoreForSession = value;
        }
    }
    
    public GameplayStates GameState
    {
        get
        {
            return gameState;
        }
    }

    public CarId CurrentSpecialCar
    {
        get
        {
            return specialCarInUse;
        }
    }

    public bool Gameplaystarted
    {
        get
        {
            return gameplaystarted;
        }
    }

    public bool MagnetActive
    {
        set
        {
            isMagnetActive = value;
        }
        get
        {
            return isMagnetActive;
        }
    }

    public bool TankMagnetActive
    {
        set
        {
            isBoltMagnetActive = value;
        }
        get
        {
            return isBoltMagnetActive;
        }
    }
    
    public float GameplayTime
    {
        get
        {
            return gameplayTime;
        }
        set
        {
            gameplayTime = value;
        }
    }
    
    public float GameplayTimeForCheckpoint
    {
        get
        {
            return checkpointShowTime;
        }
    }
    
    public float InitGameplayTime
    {
        get
        {
            return initGameplayTime;
        }
    }

    public int TurboCounter
    {
        get
        { 
            return turboCounter; 
        }
    }

    public bool IsTurboActive
    {
        get
        {
            return turboRatio >= 0.0f;
        }
    }

    public bool IsSpecialCarActive
    {
        get
        {
            return isPlayerSpecialCar;
        }
    }

    public bool IsBadGuyActive
    {
        get
        {
            return isBadGuyActive;
        }
        set
        {
            isBadGuyActive = value;
        }
    }

    public bool IsHelicopterActive
    {
        get
        {
            return isHelicopterActive;
        }
        set
        {
            if (!value)
                gameObject.GetComponent<OnTheRunSpawnManager>().MoreTrucksDectivated( );
            isHelicopterActive = value;
        }
    }

    public PlayerKinematics PlayerKinematics
    {
        get
        {
            return player;
        }
    }

    public int CheckpointCounter
    {
        get { return checkpointCounter; }
    }

    public bool IsSpecialCar(CarId carId)
    {
        return carId == CarId.Bigfoot || carId == CarId.Firetruck || carId == CarId.Plane || carId == CarId.Tank || carId == CarId.Ufo;
    }

    public int SaveMeCounter
    {
        get { return saveMeCounter; } 
    }
    #endregion

    #region Messages
    public void StartReadyGoSequence()
    {
        OnTheRunTutorialManager.Instance.DeactivatePlayerInputs = false;
        ingamePage.DectivateTapToContinue( );
        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            OnTheRunTutorialManager.Instance.Initialize();
            return;
        }
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Ready);

        Manager<UIManager>.Get().ActivePage.SendMessage("SetProgressBarVisibility", true, SendMessageOptions.DontRequireReceiver);

        UIFlyer ready = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero, Quaternion.identity);

        UITextField tf = ready.GetComponentInChildren<UITextField>();
        tf.text = OnTheRunDataLoader.Instance.GetLocaleString("item_ready");
        tf.ApplyParameters();

        GameObject uiRoot = GameObject.Find("UI");
        ready.onEnd.AddTarget(uiRoot, "OnReadyEnded");

        //LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.StartTheGame);
        OnTheRunBooster.Instance.ActivateEquippedBoosters();

        spawnManager.SpawnTrafficRandomly(0.0f, 4, 28);
    }

    void OnEquipBooster(OnTheRunBooster.BoosterType type)
    {
        switch (type)
        {
            case OnTheRunBooster.BoosterType.Turbo:
                startWithTurbo = true;
                break;
            case OnTheRunBooster.BoosterType.DoubleCoins:
                moneyMultiplier = 2;
                break;
            case OnTheRunBooster.BoosterType.DoubleExp:
                expMultiplier = 2;
                break;
            case OnTheRunBooster.BoosterType.MoreCheckpointTime:
                boosterMoreCheckpointTimeSeconds = 1;
                break;
            case OnTheRunBooster.BoosterType.SpecialCar:
                startWithSpecialCar = true;
                wasStartWithSpecialCar = true;
                break;
        }
    }

    void TankCheat()
    {
        isPlayerSpecialCar = true;
        GameObject newCar = null;
        ingamePage.ActivateSpecialPowerText(true, TruckBehaviour.TrasformType.Tank);
        newCar = Instantiate(carsList[2]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        specialCarInUse = CarId.Tank;
        TankMagnetActive = true;
        isMagnetActive = true;
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        tankActivateFlag = true;
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }
        
        gameObject.SendMessage("OnTankActivated", true);
        ChangePlayerCar(newCar, player.gameObject.transform.position);
    }

    void BigfootCheat()
    {
        isPlayerSpecialCar = true;
        GameObject newCar = null;
        ingamePage.ActivateSpecialPowerText(true, TruckBehaviour.TrasformType.Bigfoot);
        newCar = Instantiate(carsList[1]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        newCar.GetComponent<PlayerDraft>().SendMessage("ActivateDraftParticles", true);
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        specialCarInUse = CarId.Bigfoot;
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }
        ChangePlayerCar(newCar, player.gameObject.transform.position);
    }

    void FiretruckCheat()
    {
        isPlayerSpecialCar = true;
        GameObject newCar = null;
        ingamePage.ActivateSpecialPowerText(true, TruckBehaviour.TrasformType.Firetruck);
        newCar = Instantiate(carsList[3]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        newCar.GetComponent<PlayerDraft>().SendMessage("ActivateDraftParticles", true);
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        specialCarInUse = CarId.Firetruck;
        tankActivateFlag = true;
        TankMagnetActive = true;
        isMagnetActive = true;
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }
        ChangePlayerCar(newCar, player.gameObject.transform.position);
    }

    void UfoCheat()
    {
        isPlayerSpecialCar = true;
        GameObject newCar = null;
        ingamePage.ActivateSpecialPowerText(true, TruckBehaviour.TrasformType.Ufo);
        newCar = Instantiate(carsList[4]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        specialCarInUse = CarId.Ufo;
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }
        ChangePlayerCar(newCar, player.gameObject.transform.position);
        GameObject.FindGameObjectWithTag("Player").SendMessage("StartFlying");
        player.GetComponent<PlayerDraft>().SendMessage("ActivateDraftParticles", false);
    }

    void PlaneCheat()
    {
        isPlayerSpecialCar = true;
        GameObject newCar = null;
        ingamePage.ActivateSpecialPowerText(true, TruckBehaviour.TrasformType.Plane);
        newCar = Instantiate(carsList[5]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        specialCarInUse = CarId.Plane;
        TankMagnetActive = true;
        isMagnetActive = true;
        magnetBonusTimer = -1.0f;
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }
        ChangePlayerCar(newCar, player.gameObject.transform.position);
        GameObject.FindGameObjectWithTag("Player").SendMessage("StartFlying");
    }

    public void CreatePlayerCarByRef(GameObject selectedCar, GameObject baseCar=null)
    {
        carSelectedRef = selectedCar;
        if(baseCar!=null)
            carBaseSelectedRef = baseCar;
        GameObject newCar = Instantiate(selectedCar) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        isPlayerSpecialCar = false;
        ChangePlayerCar(newCar, Vector3.zero);
    }

    public void CreatePlayerCar(int index=0)
    {
        GameObject newCar = Instantiate(carsList[index]) as GameObject;
        newCar.transform.parent = LevelRoot.Instance.Root;
        newCar.tag = "Untagged";
        isPlayerSpecialCar = false;
        ChangePlayerCar(newCar, Vector3.zero);
    }


    protected GameObject SetupSpecialCar(TruckBehaviour.TrasformType carType)
    {
        GameObject newCar = null;
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        if (stopTimeDuringSpecialCars)
            ingamePage.SendMessage("OnLaunchTimeStoppedFlyer");
        if (interfaceSounds != null)
        {
            resumeEndTimeSound = true;
            interfaceSounds.SendMessage("PauseEndingTimeSound", true);
        }

        switch (carType)
        {
            case TruckBehaviour.TrasformType.None:
                //error
                break;
            case TruckBehaviour.TrasformType.Bigfoot:
                ingamePage.ActivateSpecialPowerText(true, carType);
                specialCarInUse = CarId.Bigfoot;
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Bigfoot);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.StartBigfoot);
                newCar = Instantiate(carsList[1]) as GameObject;
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.UseBigfoot);
                break;
            case TruckBehaviour.TrasformType.Tank:
                ingamePage.ActivateSpecialPowerText(true, carType);
                specialCarInUse = CarId.Tank;
                TankMagnetActive = true;
                tankActivateFlag = true;
                player.gameObject.SendMessage("ActivateMagnetFx", false);
                isMagnetActive = true;
                magnetBonusTimer = -1.0f;
                gameObject.SendMessage("OnTankActivated", true);
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Tank);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.StartTank);
                newCar = Instantiate(carsList[2]) as GameObject;
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.UseTank);
                break;
            case TruckBehaviour.TrasformType.Firetruck:
                ingamePage.ActivateSpecialPowerText(true, carType);
                specialCarInUse = CarId.Firetruck;
                TankMagnetActive = true;
                isMagnetActive = true;
                magnetBonusTimer = -1.0f;
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.FiretruckEnter);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.StartBigfoot);
                newCar = Instantiate(carsList[3]) as GameObject;
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.UseFiretruck);
                break;
            case TruckBehaviour.TrasformType.Ufo:
                ingamePage.ActivateSpecialPowerText(true, carType);
                specialCarInUse = CarId.Ufo;
                newCar = Instantiate(carsList[4]) as GameObject;
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.UFOEnter);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.StartBigfoot);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.UseUFo);
                break;
            case TruckBehaviour.TrasformType.Plane:
                ingamePage.ActivateSpecialPowerText(true, carType);
                specialCarInUse = CarId.Plane;
                TankMagnetActive = true;
                isMagnetActive = true;
                magnetBonusTimer = -1.0f;
                newCar = Instantiate(carsList[5]) as GameObject;
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.PlaneEnter);
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.StartBigfoot);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.UsePlane);
                break;
        }

        return newCar;
    }

    protected void RandomSpecialCar()
    {
        isPlayerSpecialCar = true;
        TruckBehaviour.TrasformType randomCar = TruckBehaviour.randomTruckFromInAppPage;//TruckBehaviour.TrasformType.None;
        if (randomCar != TruckBehaviour.TrasformType.None)
        {
            GameObject newCar = SetupSpecialCar(randomCar);
            newCar.transform.parent = LevelRoot.Instance.Root;
            newCar.tag = "Untagged";
            ChangePlayerCar(newCar, player.gameObject.transform.position);

            if (specialCarInUse == CarId.Ufo || specialCarInUse == CarId.Plane)
            {
                player.SendMessage("StartFlying");
                player.SendMessage("FastEndJump");
            }
        }
    }

    public void OnChangeCarEvent(Vector3 fakeCarPosition)
    {
        bool canChangeCar = (!isPlayerSpecialCar && fakeCarPosition != Vector3.zero) || (isPlayerSpecialCar && fakeCarPosition == Vector3.zero);
        bool wasFlying = false;
        if (canChangeCar)
        {
            isPlayerSpecialCar = !isPlayerSpecialCar;
            GameObject newCar = null;
            if (isPlayerSpecialCar)
            {
                newCar = SetupSpecialCar(player.TransformInto);
                //newCar = SetupSpecialCar(player.TruckTaken.CarOverTruckType);
                //player.TruckTaken.CarOverTruckType = TruckBehaviour.TrasformType.None;
            }
            else
            {
                if (specialCarInUse != CarId.None)
                    gameObject.SendMessage("RestoreDistancesForSpecialCar", player.PlayerRigidbody.position.z);
                if (resumeEndTimeSound)
                    interfaceSounds.SendMessage("PauseEndingTimeSound", false);
                resumeEndTimeSound = false;
                ingamePage.ActivateSpecialPowerText(false);
                switch (specialCarInUse)
                {
                    case CarId.Tank:
                        player.SendMessage("OnDestroyTankArea");
                        break;
                    case CarId.Ufo:
                        wasFlying = true;
                        break;
                    case CarId.Plane:
                        wasFlying = true;
                        break;
                }

                TankMagnetActive = false;
                isMagnetActive = false;

                specialCarInUse = CarId.None;
                newCar = Instantiate(carSelectedRef) as GameObject;
                ingamePage.SendMessage("OnDestroyTimeStoppedFlyer", SendMessageOptions.DontRequireReceiver);
            }
            newCar.transform.parent = LevelRoot.Instance.Root;
            newCar.tag = "Untagged";
            ChangePlayerCar(newCar, fakeCarPosition);

            //end the jump
            if (fakeCarPosition != Vector3.zero)
                player.SendMessage("CompleteJump", 10.0f);

            if (wasFlying)// && gameplayTime > 0.0f)
            {
                player.SendMessage("OnFlyingEnded");
            }

            if (specialCarInUse == CarId.Ufo || specialCarInUse == CarId.Plane)
            {
                player.SendMessage("StartFlying");
                player.SendMessage("FastEndJump");
            }
        }
    }

    void ChangePlayerCar(GameObject newCar, Vector3 fakeCarPosition)
    {
        GameObject currCar = GameObject.FindGameObjectWithTag("Player"); //LevelRoot.Instance.Root.Find("Player").gameObject;
        bool shieldActive = player.ShieldOn;
        bool isDead = player.PlayerIsDead;
        bool isInTurbo = player.TurboOn;
        bool isStopping = player.PlayerIsStopping;
        float oldSpeed = player.PlayerRigidbody.velocity.z;
        currCar.SetActive(false);
        currCar.tag = "Untagged";
        newCar.tag = "Player";
        newCar.name = "Player";
        newCar.SetActive(true);
        
        PlayerKinematics currCarKinematics = currCar.GetComponent<PlayerKinematics>();
        player = newCar.GetComponent<PlayerKinematics>();
        Vector3 initialPlayerSpeed = Vector3.zero;
        if (fakeCarPosition != Vector3.zero)
        {
            newCar.transform.position = fakeCarPosition;

            //Debug.Log("********* Special Vehicle START - DEBUG LOG");
            carChangedTimer = specialCarTime + UnlockingManager.Instance.GetSpecialCarData(player.carId).level * specialCarDeltaTime;
            if (startWithSpecialCar)
                carChangedTimer += specialCarBoostTime;
            initialPlayerSpeed = currCarKinematics.PlayerRigidbody.velocity;
        }
        else
        {
            bool keepPlayerStill = isStopping || isDead;
            if (!keepPlayerStill && (Manager<UIManager>.Get().ActivePageName == "IngamePage" || Manager<UIManager>.Get().ActivePageName == "RewardPage"))
                initialPlayerSpeed = Vector3.forward * 60.0f;
            newCar.transform.position = currCar.transform.position;
        }

        player.Reset(currCarKinematics.CurrentSpeed, initialPlayerSpeed, currCarKinematics.StartDistance, currCarKinematics.CurrentLane);
        Destroy(currCar);
        if (IsSpecialCarActive)
            LevelRoot.Instance.Root.BroadcastMessage("OnPausePlayerEngine");


        GameObject track = GameObject.Find("Track").gameObject;//LevelRoot.Instance.Root.Find("Track").gameObject;
        track.GetComponent<NeverendingTrackModified>().controller = newCar.GetComponent<NeverendingPlayer>();
        if (Manager<UIManager>.Get().ActivePageName == "IngamePage")
            track.GetComponent<NeverendingTrackModified>().controller.StartRunning(track.GetComponent<NeverendingTrackModified>().FirstBranch);

        LevelRoot.Instance.Root.BroadcastMessage("OnChangePlayerCar");
        Manager<UIRoot>.Get().OnChangePlayerCar();

        if (gameplaystarted)
        {
            player.SendMessage("OnChangePlayerEffect");
            newCar.SendMessage("OnShieldActive", shieldActive);
            shieldActive = false;
            if (MagnetActive)
                newCar.SendMessage("ActivateMagnetFx", true);
            if (isInTurbo)
                player.StatInTurbo = true;
        }
        else
        {
            if (isDead)
                player.PlayerIsDead = true;
            if (isStopping)
            {
                player.PlayerIsStopping = true;
                player.PlayerOldSpeed = oldSpeed;
            }

            if (!IsSpecialCarActive && gameplayTime <= 0.0f && fakeCarPosition == Vector3.zero && Manager<UIManager>.Get().ActivePageName == "IngamePage")
                player.ResumeInStoppingState();
        }
    } 

    void OnInitGameplayTime()
    {
        //reset boosters properties
        //Debug.Log("GamePlayManager: OnInitGameplayTime");

        if (!restartFromSaveMe)
        {
            saveMeCounter = 0;
            coinsForSession = 0;
        }
        else
            ++saveMeCounter;

        scoreForSession = 0;
        StartCoroutine(OnResetRestartFromSaveMe());

        if (player.Distance == 0.0f)
        {
            checkpointCounter = 0;
            carChangedTimer = -1.0f;
            carTurboTimer = -1.0f;
            Manager<UIRoot>.Get().GetComponent<UISharedData>().OnRestartRace();
        }
        moneyMultiplier = 1;
        expMultiplier = 1;
        boosterMoreCheckpointTimeSeconds = 1;

        gameplayTime = checkpointTimeList[0];// Mathf.Max((int)(checkpointInitTime - checkpointDecreaseTime * checkpointCounter), (int)checkpointMinTime);
       // initGameplayTime = 0.0f;
        checkpointShowTime = gameplayTime;
        freezeGameplayTimeTimer = 1.5f;
        endingTimePlayed = false;
        endingTimeSpeechPlayed = false;
        if (player.Distance == 0.0f)
            ++checkpointCounter;
        MagnetActive = false;
        TankMagnetActive = false;

        ingamePage = GameObject.Find("IngamePage").GetComponent<UIIngamePage>();
        OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
    }

    IEnumerator OnResetRestartFromSaveMe()
    {
        yield return new WaitForSeconds(0.5f);
        restartFromSaveMe = false;
    }

    void OnResetGameplayTime()
    {
        OnUpdateGameplayTime(true);//true);
        gameplaystarted = true;
        gameplayTime = checkpointTimeList[0];
        initGameplayTime = gameplayTime;// 0.0f;
        checkpointShowTime = gameplayTime;
    }

    void OnUpdateGameplayTime(bool justStarted)
    {
        int timeSaved = 0;
        int nextCheckpointBaseTime = (int)gameplayTime;

        //Give money for time saved
        if (!justStarted)
        {
            resumeEndTimeSound = false;

            timeSaved = (int)gameplayTime;
            Manager<UIManager>.Get().ActivePage.SendMessage("OnTimeSaved", timeSaved);

            //Update Checkpoint time
            if (checkpointCounter < checkpointTimeList.Length)
            {
                nextCheckpointBaseTime = (int)checkpointTimeList[checkpointCounter];
                gameplayTime = (int)gameplayTime + nextCheckpointBaseTime + boosterMoreCheckpointTimeSeconds;
            }
            else
            {
                nextCheckpointBaseTime = Mathf.Max((int)(checkpointInitTime - checkpointDecreaseTime * (checkpointCounter - checkpointTimeList.Length)), (int)checkpointMinTime);
                gameplayTime = (int)gameplayTime + nextCheckpointBaseTime + boosterMoreCheckpointTimeSeconds;
            }
        }

        //Debug.Log("CHECKPOINT -- TIME SAVED: " + timeSaved + " ; CHECKPOINT BASE TIME: " + nextCheckpointBaseTime);
        
        /*if (justStarted)
            gameplayTime = Mathf.Max((int)(checkpointInitTime - checkpointDecreaseTime * checkpointCounter), (int)checkpointMinTime);
        else
        {
            gameplayTime = (int)gameplayTime + Mathf.Max((int)(checkpointInitTime - checkpointDecreaseTime * checkpointCounter), (int)checkpointMinTime);
        }*/

        initGameplayTime = gameplayTime;// 0.0f;
        checkpointShowTime = gameplayTime;
        freezeGameplayTimeTimer = 1.5f;
        endingTimePlayed = false;
        endingTimeSpeechPlayed = false;
        ++checkpointCounter;
        gameplaystarted = true;
        wasgameplaystarted = true;
        if (justStarted)
            player.gameObject.SendMessage("OnTutorialEnd", SendMessageOptions.DontRequireReceiver);
    }

    void OnGameover()
    {
        turboCounter = 0;
        turboRatio = -1.0f;
        truckTurboRatio = -1.0f;
        gameplaystarted = false;
        
        if (interfaceSounds!=null)
            interfaceSounds.SendMessage("StopEndingTimeSound");
    }

    public void OnTurboEnd()
    {
        turboRatio = -1.0f;
    }

    public void OnTruckTurboEnd()
    {
        truckTurboRatio = -1.0f;
    }

    void OnSaveMeHelpingHandPressed(int extraTime)
    {
        gameplayTime = extraTime + 1.0f;
        freezeGameplayTimeTimer = -1.0f;
        endingTimePlayed = false;
        endingTimeSpeechPlayed = false;
        gameplaystarted = true;
        player.gameObject.SendMessage("OnStartRunning");
        LevelRoot.Instance.BroadcastMessage("PlayerIsResurrected");
        
        player.KeepSpeedTimer = 2.0f;
    }

    void OnSaveMeBoltPressed(int extraTime)
    {
        gameplayTime = extraTime + 1.0f;
        freezeGameplayTimeTimer = -1.0f;
        endingTimePlayed = false;
        endingTimeSpeechPlayed = false;
        gameplaystarted = true;
        player.gameObject.SendMessage("OnStartRunning");
        LevelRoot.Instance.BroadcastMessage("PlayerIsResurrected");

        turboCounter = 3;
        ingamePage.OnTurboBonusCollected(turboCounter);
        OnTheRunSounds.Instance.PlayTurboBonusCollected(turboCounter);
        turboCounter = 0;
        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
        ActivateTurbo();
    }

    void OnSaveMeTimePressed(int extraTime)
    {
        gameplayTime = extraTime + 1.0f;
        freezeGameplayTimeTimer = -1.0f;
        endingTimePlayed = false;
        endingTimeSpeechPlayed = false;
        gameplaystarted = true;
        player.gameObject.SendMessage("OnStartRunning");
        LevelRoot.Instance.BroadcastMessage("PlayerIsResurrected");
    }

    void OnBonusCollected(BonusBehaviour.BonusData bonData)
    {
        if (player.PlayerIsDead) //turboRatio >= 0.0f || 
            return;

        int deltaCoins = 0;
        switch (bonData.type)
        {
            case OnTheRunObjectsPool.ObjectType.BonusMoney:
                //Debug.Log("##############BONUS MONEY");
                deltaCoins = bonData.coinsToGain * moneyMultiplier;
                CoinsCollected += deltaCoins;
                //LevelRoot.Instance.BroadcastMessage("OnMissionCoinsCollected", deltaCoins);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CollectCoins);
                OnTheRunSounds.Instance.PlayBonusCollected(bonData.type);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_MEDIUM, deltaCoins);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_HARD, deltaCoins);
                break;
            case OnTheRunObjectsPool.ObjectType.BonusMoneyBig:
                //Debug.Log("##############BONUS MONEY");
                deltaCoins = bonData.coinsToGain * moneyMultiplier;
                CoinsCollected += deltaCoins;
                //LevelRoot.Instance.BroadcastMessage("OnMissionCoinsCollected", deltaCoins);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CollectBigCoins);
                OnTheRunSounds.Instance.PlayBonusCollected(bonData.type);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_MEDIUM, deltaCoins);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COINS_HARD, deltaCoins);
                break;
            case OnTheRunObjectsPool.ObjectType.BonusShield:
                //Debug.Log("##############BONUS SHIELD");
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CollectShield);
                OnTheRunSounds.Instance.PlayBonusCollected(bonData.type);
                player.SendMessage("OnShieldActive", true);
                break;
            case OnTheRunObjectsPool.ObjectType.BonusMagnet:
                //Debug.Log("##############BONUS MAGNET");
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CollectMagnet);
                isMagnetActive = true;
                magnetBonusTimer = 8.0f;
                player.gameObject.SendMessage("ActivateMagnetFx", true);
                OnTheRunSounds.Instance.PlayBonusCollected(bonData.type);
                break;
            case OnTheRunObjectsPool.ObjectType.BonusTurbo:
                //Debug.Log("##############BONUS TURBO " + turboCounter);
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Bolt);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.CollectBolt);
                turboCounter++;
                if (turboCounter <= 3)
                {
                    ingamePage.OnTurboBonusCollected(turboCounter);
                    OnTheRunSounds.Instance.PlayTurboBonusCollected(turboCounter);
                    if (turboCounter >= 3)
                    {
                        turboCounter = 0;
                        gameObject.SendMessage("SaveDistancesForSpecialCar", player.PlayerRigidbody.position.z);
                        ActivateTurbo();
                    }
                }
                break;
        }

    }
    #endregion

    #region Functions
    public OnTheRunEnvironment.Environments EnvironmentFromIdx(int index)
    {
        OnTheRunEnvironment.Environments retValue = OnTheRunEnvironment.Environments.Europe;
        switch (index)
        {
            case 0: retValue = OnTheRunEnvironment.Environments.Europe; break;
            case 1: retValue = OnTheRunEnvironment.Environments.Asia; break;
            case 2: retValue = OnTheRunEnvironment.Environments.NY; break;
            case 3: retValue = OnTheRunEnvironment.Environments.USA; break;
        }

        return retValue;
    }

    public int EnvironmentIdx(OnTheRunEnvironment.Environments env)
    {
        int retValue = -1;
        switch (env)
        {
            case OnTheRunEnvironment.Environments.Europe: retValue = 0; break;
            case OnTheRunEnvironment.Environments.Asia: retValue = 1; break;
            case OnTheRunEnvironment.Environments.NY: retValue = 2; break;
            case OnTheRunEnvironment.Environments.USA: retValue = 3; break;
        }

        return retValue;
    }

    public GameObject[] GetCarsFromIndex(int tierIndex)
    {
        GameObject[] currentCarsList = Cars_EU;
        switch (tierIndex)
        {
            case 1: currentCarsList = Cars_JP; break;
            case 2: currentCarsList = Cars_NY; break;
            case 3: currentCarsList = Cars_USA; break;
        }
        return currentCarsList;
    }

    public static string GetCarName(CarId carId)
    {
        string res = "";
        switch (carId)
        {
            case CarId.Car_1_american:
                res = OnTheRunDataLoader.Instance.GetLocaleString("usaCarName1");
                break;
            case CarId.Car_2_american:
                res = OnTheRunDataLoader.Instance.GetLocaleString("usaCarName2");
                break;
            case CarId.Car_3_american:
                res = OnTheRunDataLoader.Instance.GetLocaleString("usaCarName3");
                break;
            case CarId.Car_4_american:
                res = OnTheRunDataLoader.Instance.GetLocaleString("usaCarName4");
                break;
            case CarId.Car_5_american:
                res = OnTheRunDataLoader.Instance.GetLocaleString("usaCarName5");
                break;

            case CarId.Car_1_europe:
                res = OnTheRunDataLoader.Instance.GetLocaleString("euCarName1");
                break;
            case CarId.Car_2_europe:
                res = OnTheRunDataLoader.Instance.GetLocaleString("euCarName2");
                break;
            case CarId.Car_3_europe:
                res = OnTheRunDataLoader.Instance.GetLocaleString("euCarName3");
                break;
            case CarId.Car_4_europe:
                res = OnTheRunDataLoader.Instance.GetLocaleString("euCarName4");
                break;
            case CarId.Car_5_europe:
                res = OnTheRunDataLoader.Instance.GetLocaleString("euCarName5");
                break;

            case CarId.Car_1_muscle:
                res = OnTheRunDataLoader.Instance.GetLocaleString("nyCarName1");
                break;
            case CarId.Car_2_muscle:
                res = OnTheRunDataLoader.Instance.GetLocaleString("nyCarName2");
                break;
            case CarId.Car_3_muscle:
                res = OnTheRunDataLoader.Instance.GetLocaleString("nyCarName3");
                break;
            case CarId.Car_4_muscle:
                res = OnTheRunDataLoader.Instance.GetLocaleString("nyCarName4");
                break;
            case CarId.Car_5_muscle:
                res = OnTheRunDataLoader.Instance.GetLocaleString("nyCarName5");
                break;

            case CarId.Car_1_oriental:
                res = OnTheRunDataLoader.Instance.GetLocaleString("asiaCarName1");
                break;
            case CarId.Car_2_oriental:
                res = OnTheRunDataLoader.Instance.GetLocaleString("asiaCarName2");
                break;
            case CarId.Car_3_oriental:
                res = OnTheRunDataLoader.Instance.GetLocaleString("asiaCarName3");
                break;
            case CarId.Car_4_oriental:
                res = OnTheRunDataLoader.Instance.GetLocaleString("asiaCarName4");
                break;
            case CarId.Car_5_oriental:
                res = OnTheRunDataLoader.Instance.GetLocaleString("asiaCarName5");
                break;

            case CarId.Bigfoot:
                res = OnTheRunDataLoader.Instance.GetLocaleString("bigfoot");
                break;
            case CarId.Firetruck:
                res = OnTheRunDataLoader.Instance.GetLocaleString("firetruck");
                break;
            case CarId.Plane:
                res = OnTheRunDataLoader.Instance.GetLocaleString("plane");
                break;
            case CarId.Tank:
                res = OnTheRunDataLoader.Instance.GetLocaleString("tank");
                break;
            case CarId.Ufo:
                res = OnTheRunDataLoader.Instance.GetLocaleString("ufo");
                break;
        }
        return res;

    }

    public bool IsInWrongDirection(int currentLane)
    {
        bool retValue = false;
        bool checkAsia = environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.Asia && currentLane > 2;
        bool checkUSA = environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA && currentLane < 2;

        if (checkAsia || checkUSA)
            retValue = true;

        //Debug.Log("IsInWrongDirection currentLane " + currentLane + " ASIA " + (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.Asia) + " USA " + (environmentManager.currentEnvironment == OnTheRunEnvironment.Environments.USA) + " RES " + retValue);

        return retValue;
    }

    void ExtendGameplay()
    {
        if (!gameplaystarted && !OnTheRunTutorialManager.Instance.WasTutorialActive)
        {
            gameplaystarted = true;
            wasgameplaystarted = false;
        }
    }

    void EndExtendGameplay()
    {
        if (!wasgameplaystarted)
        {
            gameplaystarted = false;
        }
    }


    void ActivateTurbo()
    {
        if (IsSpecialCarActive)
        {
            //carChangedTimer += turboDuration;
            OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
            player.gameObject.SendMessage("ActivateBlink", true);
        }
        else if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
#if !UNITY_WEBPLAYER
            OnTheRunTutorialManager.Instance.ActivateIngameArrows(true);
#endif
            OnTheRunTutorialManager.Instance.DeactivatePlayerInputs = false;
            turboDuration = OnTheRunDataLoader.Instance.GetTurboDuration() * 0.7f;
            //carChangedTimer = turboDuration;
        }
        //else
        //    carChangedTimer = turboDuration;

        carTurboTimer = turboDuration;

        ExtendGameplay();
        turboRatio = 1.0f;
        ingamePage.OnTurboActive(true);
        LevelRoot.Instance.Root.BroadcastMessage("ActivateTruckTurbo", false);
        player.SendMessage("OnTurboActive", true, SendMessageOptions.DontRequireReceiver);

        FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(turboDuration, 0.4f, 0.5f);
        cameraManager.SendMessage("StartShakeCamera", sData);

        spawnManager.TurboTrafficMultiplier = 0.002f;
    }

    void ActivateTruckTurbo(bool active)
    {
        ExtendGameplay();
        if (!gameplaystarted && !OnTheRunTutorialManager.Instance.WasTutorialActive)
        {
            gameplaystarted = true;
            wasgameplaystarted = false;
        }

        if (active)
        {
            if (truckTurboRatio < 0.0f)
            {
                truckTurboRatio = 1.0f;
                player.SendMessage("OnSlipstreamActive", true, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            truckTurboRatio = -1.0f;
            player.SendMessage("OnSlipstreamActive", false, SendMessageOptions.DontRequireReceiver);
        }
    }

    void RaceStarted()
    {
        gameplaystarted = true;
        if (!OnTheRunTutorialManager.Instance.WasTutorialActive)
        {
            turboCounter = 0;
            turboRatio = -1.0f;
        }
        truckTurboRatio = -1.0f;
        checkpointCounter = 0;
        freezeGameplayTimeTimer = 1.5f;
        OnResetGameplayTime();
        initGameplayTime = gameplayTime;
        if(startWithTurbo)
            ActivateTurbo();
        startWithTurbo = false;

        if (startWithSpecialCar)
            RandomSpecialCar();

        startWithSpecialCar = false;
        TruckBehaviour.randomTruckFromInAppPage = TruckBehaviour.TrasformType.None;

        spawnManager.ResetSpecialVehiclesActivated();
        enemiesManager.ResetAllForceParameters();
        OnTheRunSingleRunMissions.DriverMission currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        if (currentMission != null && !currentMission.Done)
        {
            //Debug.Log("AAAAAAAAAAAAAA " + currentMission.SpecialVehicleNeeded);
            spawnManager.MoreSpecialVehiclesActivated(currentMission.SpecialVehicleNeeded);

            if (currentMission.category==OnTheRunSingleRunMissions.MissionCategory.OnAir)
                enemiesManager.ForceHelicopterSpawn();

            if (currentMission.category == OnTheRunSingleRunMissions.MissionCategory.DestroyPolice)
                enemiesManager.ForcePoliceSpawn();
        }
    }

    public void ChangeState(GameplayStates newState)
    {
        //Debug.Log("ChangeState() - newState: " + newState);

        if (newState == gameState)
            return;

        if (gameState == GameplayStates.Paused)
            ResumeGameplay();

        switch (newState)
        {
            case GameplayStates.Offgame: SwitchToOffgame(); break;
            case GameplayStates.Start: SwitchToStart(); break;
            case GameplayStates.ReadyToRace: SwitchToReadyToRace(); break;
            case GameplayStates.Racing:   SwitchToRacing();   break;
            case GameplayStates.Paused:   SwitchToPaused();   break;
            case GameplayStates.GameOver: SwitchToGameOver(); break;
        }
    }

    void ResumeGameplay()
    {
        if (TimeManager.Instance.MasterSource.IsPaused)
        {
            blinkCarSource.volume = 1.0f;
            //TimeManager.Instance.MasterSource.Resume();
        }
    }

    void SwitchToOffgame()
    {
        //Debug.Log("SwitchToOffgame()");

        if (Manager<UIManager>.Get().ActivePageName == "IngamePage")
            Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().DestroyLiveNewsFlyer();

        OnTheRunTutorialManager.Instance.CheckTutorialEnded();

        carChangedTimer = -1.0f;
        carTurboTimer = -1.0f;
        OnTheRunSounds.Instance.StopSpeech( );
        OnTheRunSounds.Instance.ResetIngameMusic();
        if (blinkCarSource.isPlaying)
            OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
        isPlayerSpecialCar = false;
        if (specialCarInUse == CarId.Tank)
            player.SendMessage("OnDestroyTankArea");
        specialCarInUse = CarId.None;
        interfaceSounds.SendMessage("PlayOffGameMusic");
        LevelRoot.Instance.gameObject.BroadcastMessage("OnGameover");
        EnvironmentSceneGo.BroadcastMessage("OnGameover");
        player.gameObject.SetActive(false);

        gameState = GameplayStates.Offgame;
    }

    void ReactivatePlayer()
    {
        specialCarInUse = CarId.None;
        player.gameObject.SetActive(true);
    }

    void SwitchToStart()
    {
        LevelRoot.Instance.gameObject.BroadcastMessage("OnGameover");
        EnvironmentSceneGo.BroadcastMessage("OnGameover");

        player.gameObject.SetActive(true);
        LevelRoot.Instance.gameObject.BroadcastMessage("OnStartGame");
        EnvironmentSceneGo.BroadcastMessage("OnStartGame");

        LevelRoot.Instance.gameObject.BroadcastMessage("ShiftPlayerForward");
        LevelRoot.Instance.gameObject.BroadcastMessage("OnStartPlayerRun");

        gameState = GameplayStates.Start;
    }

    void ResetEnvironment()
    {
        LevelRoot.Instance.gameObject.BroadcastMessage("OnGameover");
        EnvironmentSceneGo.BroadcastMessage("OnGameover");
        player.gameObject.SetActive(false);
    }

    void SwitchToReadyToRace()
    {
        gameState = GameplayStates.ReadyToRace;
    }

    void SwitchToRacing()
    {
        switch (gameState)
        {
            case GameplayStates.ReadyToRace:
                //Debug.Log("SwitchToRacing() - sending messages ");
                //LevelRoot.Instance.gameObject.BroadcastMessage("OnStartPlayerRun");
                LevelRoot.Instance.Root.BroadcastMessage("RestartEnemiesManager", true);
                break;
        }

        uiSharedData.ResetScoreMultiplier();
        spawnManager.TurboTrafficMultiplier = 1.0f;
		gameState = GameplayStates.Racing;
    }

    void SwitchToPaused()
    {
        //Log("SwitchToPaused()");

        blinkCarSource.volume = 0.0f;

        //TimeManager.Instance.MasterSource.Pause();

        gameState = GameplayStates.Paused;
    }

    void SwitchToGameOver()
    {
        //Log("SwitchToGameOver()");

        switch (gameState)
        {
            case GameplayStates.Start:
                break;

            case GameplayStates.Racing:
                LevelRoot.Instance.gameObject.BroadcastMessage("OnGameover");
                EnvironmentSceneGo.BroadcastMessage("OnGameover");
                break;
        }

        gameState = GameplayStates.GameOver;
    }
    #endregion

    #region Unity Callbacks
    
    void Awake()
    {
        collisionProtectionTime = OnTheRunDataLoader.Instance.GetCollisionProtectionTime();
        checkpointTimeList = OnTheRunDataLoader.Instance.GetCheckpointTimes();
        checkpointInitTime = OnTheRunDataLoader.Instance.GetCheckpointData("init_time");
        checkpointDecreaseTime = OnTheRunDataLoader.Instance.GetCheckpointData("decrease_time");
        checkpointMinTime = OnTheRunDataLoader.Instance.GetCheckpointData("min_time");

        specialCarTime = OnTheRunDataLoader.Instance.GetSpecialCarData("duration");
        specialCarDeltaTime = OnTheRunDataLoader.Instance.GetSpecialCarData("delta_time");
        specialCarBoostTime = OnTheRunDataLoader.Instance.GetSpecialCarData("boost_duration");

        player = (PlayerKinematics)GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
        Asserts.Assert(player != null, "OnTheRunGameplay.Awake() - Could not find GameObject with Tag \"Player\".");
		Instance = this;

        turboDuration = OnTheRunDataLoader.Instance.GetTurboDuration();
        slipstreamDuration = OnTheRunDataLoader.Instance.GetSlipStreamFromTruckDuration();
        coinsBase = OnTheRunDataLoader.Instance.GetSlipstreamRewardData()[0];
        coinsStep = OnTheRunDataLoader.Instance.GetSlipstreamRewardData()[1];
        coinsExponent = OnTheRunDataLoader.Instance.GetSlipstreamRewardData()[2];

        gameObject.AddComponent<InterpolatedTimeEffects>();

        initboosters = new bool[(int)OnTheRunBooster.BoosterType.Count];
    }

    void Start()
    {
        Shader.WarmupAllShaders();

        environmentManager = GetComponent<OnTheRunEnvironment>();

        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        
        GameObject.DontDestroyOnLoad(LevelRoot.Instance.Root.transform.gameObject);

        blinkCarSource = gameObject.AddComponent<AudioSource>();
        blinkCarSource.volume = 1.0f;
        blinkCarSource.playOnAwake = false;
        blinkCarSource.loop = true;
        blinkCarSource.clip = blinkCarSound;
    }

    IEnumerator LoadEnvironment(bool preload)
    {
#if UNITY_WEBPLAYER
        while (!Application.CanStreamedLevelBeLoaded("scene_" + envPostfixLoaded))
        {
            yield return new WaitForEndOfFrame();
        }
#endif

        if (!preload)
        {
            yield return Resources.UnloadUnusedAssets();
            AsyncOperation async = Application.LoadLevelAdditiveAsync("scene_" + envPostfixLoaded);
            yield return async;
        }

        yield return new WaitForEndOfFrame();

        EnvironmentSceneGo = GameObject.Find("Root_" + envPostfixLoaded).gameObject;
       
        OnTheRunObjectsPool.Instance.AddNewItems(EnvironmentSceneGo.transform.FindChild("AdditionalItems").GetComponent<AdditionalPoolElements>().additionalPoolList);

        if (preload)
            InitializeGameplay();
        else
        {
            Manager<UIManager>.Get().ActivePage.SendMessage("LevelLoaded");
        }
    }

    void LoadNewLevel(bool preload)
    {
        string oldEnv = envPostfixLoaded;
        switch (environmentManager.currentEnvironment)
        {
            case OnTheRunEnvironment.Environments.Europe: envPostfixLoaded = "eu"; break;
            case OnTheRunEnvironment.Environments.Asia: envPostfixLoaded = "as"; break;
            case OnTheRunEnvironment.Environments.USA: envPostfixLoaded = "us"; break;
            case OnTheRunEnvironment.Environments.NY: envPostfixLoaded = "ny"; break;
        }

        bool levelAlreadyLoaded = (oldEnv == envPostfixLoaded);
        if(levelAlreadyLoaded && !preload)
        {
            Manager<UIManager>.Get().ActivePage.SendMessage("LevelLoaded");
        }
        else
        {
            //Unload old environment
            if (EnvironmentSceneGo != null)
            {
                OnTheRunObjectsPool.Instance.DestroyPoolElements();
                GameObject.Find("Track").GetComponent<NeverendingTrackModified>().DestroyAllFromScene();
                Destroy(EnvironmentSceneGo);
            }
            
            StartCoroutine(LoadEnvironment(preload));
        }
    }

    void InitializeGameplay()
    {
        CreatePlayerCar();
        //Log("Start()");
        gameState = GameplayStates.Offgame;
        //SwitchToOffgame();
        turboCounter = 0;
        turboRatio = -1.0f;
        truckTurboRatio = -1.0f;
        checkpointCounter = 0;
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
        spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();
        enemiesManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunEnemiesManager>();
    }

    void Update()
    {

#if UNITY_WEBPLAYER
        if (uiManager==null)
            uiManager = Manager<UIManager>.Get();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool activePopup = uiManager.FrontPopup != null && (uiManager.FrontPopup.name == "SingleButtonFeedbackPopup" || uiManager.FrontPopup.name == "RankFeedbackPopup");
            if (activePopup)
            {
                switch (uiManager.FrontPopup.name)
                {
                    case "SingleButtonFeedbackPopup":
                        OnTheRunUITransitionManager.Instance.OnBuyPopupClosed(uiManager.FrontPopup.transform.FindChild("content/ResumeButton").GetComponent<UIButton>());
                        break;
                    case "RankFeedbackPopup":
                        uiManager.FrontPopup.GetComponent<UIRankFeedbackPopup>().Signal_OnCollectRelease(uiManager.FrontPopup.transform.FindChild("content/CollectButton").GetComponent<UIButton>());
                        break;
                }
            }
            else
            {
                switch (uiManager.ActivePageName)
                {
                    case "IngamePage":
                        if(OnTheRunTutorialManager.Instance.TutorialActive)
                            OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapEverywhere);
                        else if(uiManager.IsPopupInStack("SaveMePopup"))
                            uiManager.FrontPopup.BroadcastMessage("OnSpacePressed");
                        break; 

                    case "StartPage":
                    case "InAppPage":
                    case "GaragePage":
                    case "RewardPage":
                        if(uiManager.FrontPopup == null && !uiManager.disableInputs)
                            uiManager.ActivePage.BroadcastMessage("OnSpacePressed");
                        break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (uiManager.ActivePageName == "IngamePage")
            {
                if (!Manager<UIManager>.Get().IsPopupInStack("PausePopup"))
                {
                    interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
                    ChangeState(OnTheRunGameplay.GameplayStates.Paused);
                    uiManager.ActivePage.SendMessage("ActivateResumeButton", true);
                    Manager<UIManager>.Get().PushPopup("PausePopup");
                }
                else
                {
                    Manager<UIManager>.Get().ActivePage.SendMessage("ActivateResumeButton", false);
                    interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
                    Manager<UIManager>.Get().PopPopup();

                    OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
                    gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if ((uiManager.ActivePageName == "GaragePage" || uiManager.ActivePageName == "TrucksPage") && Manager<UIManager>.Get().FrontPopup == null)
                uiManager.ActivePage.SendMessage("SwapCarFromKeyboard", 1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if ((uiManager.ActivePageName == "GaragePage" || uiManager.ActivePageName == "TrucksPage") && Manager<UIManager>.Get().FrontPopup == null)
                uiManager.ActivePage.SendMessage("SwapCarFromKeyboard", -1);
        }
#endif

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        //PlayerPersistentData.Instance.UpdateFuelTime(dt);

        if (gameplaystarted && !player.PlayerIsDead)
        {
            bool stopTime = (stopTimeDuringSpecialCars && IsSpecialCarActive) || OnTheRunTutorialManager.Instance.TutorialActive;
            if (freezeGameplayTimeTimer < 0.0f && !stopTime)
                gameplayTime -= dt;

            //if (freezeGameplayTimeTimer >= 0.0f)
            //    Debug.Log("freezeGameplayTimeTimer: " + freezeGameplayTimeTimer + " GPT: " + gameplayTime);

            gameplayTime = Mathf.Max(0.0f, gameplayTime);

            if (gameplayTime < 3.0f && !endingTimeSpeechPlayed && CheckpointCheck())
            {
                endingTimeSpeechPlayed = true;
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Hurry);
            }

            if (gameplayTime < 5.0f && !endingTimePlayed)
            {
                endingTimePlayed = true;
                interfaceSounds.SendMessage("PlayEndingTimeSound");
            }
            if (gameplayTime <= 0.0f && wasgameplaystarted && !player.TurboOn)
            {
                gameplaystarted = false;
                LevelRoot.Instance.BroadcastMessage("OnTimeEnded");
            }

            if (freezeGameplayTimeTimer >= 0.0f)
            {
                freezeGameplayTimeTimer -= dt;
            }

            
        }

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            refreshFBNotificationTimer -= dt;

            if (refreshFBNotificationTimer < 0.0f)
            {
                refreshFBNotificationTimer = refreshFBNotificationTime;
                OnTheRunNotificationManager.Instance.OnLoginSuccessuful();
            }
        }

        if (carTurboTimer >= 0.0f)
        {
            carTurboTimer -= dt;

            if (carTurboTimer < 2.0f)
            {
                blinkCarTimer -= dt;
                if (!blinkCarSource.isPlaying)
                {
                    blinkCarSource.volume = 1.0f;
                    OnTheRunSoundsManager.Instance.PlaySource(blinkCarSource);//.Play();
                }

                if (blinkCarTimer < 0.0f)
                {
                    blinkCarTimer = 0.1f;//0.12f;
                    //player.gameObject.SendMessage("ActivateBlink", false);
                }
            }

            if (carTurboTimer < 0.0f)
            {
                carTurboTimer = -1.0f;
                blinkCarSource.Stop();
                //OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
            }
        }
        else if (carChangedTimer >= 0.0f)//blinking---------------------
        {
            carChangedTimer -= dt;

            float tempTimer = carChangedTimer;
            float tempTimeThreshold = 2.0f;//3.6
            if (IsSpecialCarActive && IsTurboActive)
            {
                tempTimer -= specialCarBoostTime;
                tempTimeThreshold += specialCarBoostTime;
                if (wasStartWithSpecialCar)
                    tempTimeThreshold += 2.0f;
            }

            if (specialCarInUse == CarId.Tank && tempTimer < 3.0f && tankActivateFlag)
            {
                tankActivateFlag = false;
                gameObject.SendMessage("OnTankActivated", false);
            }

            if (tempTimer < tempTimeThreshold)
            {
                blinkCarTimer -= dt;
                if (!blinkCarSource.isPlaying)
                {
                    blinkCarSource.volume = 1.0f;
                    OnTheRunSoundsManager.Instance.PlaySource(blinkCarSource);//.Play();
                }

                if (blinkCarTimer < 0.0f)
                {
                    blinkCarTimer = 0.1f;//0.12f;
                    player.gameObject.SendMessage("ActivateBlink", false);
                }
            }

            if (tempTimer < 0.0f)
            {
                OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
                if (gameplaystarted)
                {
                    if (!player.IsJumping && !player.IsLifting)
                    {
                        BackToDefaultCar();
                    }
                    else
                        carChangedTimer = 0.1f;
                }
                else
                {
                    OnChangeCarEvent(Vector3.zero);
                    carChangedTimer = -1.0f;
                }
            }
        }

        //turbo update
        if (turboRatio >= 0.0f)
        {
            ingamePage.OnTurboBarUpdate(turboRatio);
            turboRatio -= dt * (1.0f / turboDuration);
            player.SendMessage("OnTurboUpdate", turboRatio, SendMessageOptions.DontRequireReceiver);
            if (turboRatio < 0.5f)
            {
                spawnManager.TurboTrafficMultiplier = 1.0f;
            }

            if (turboRatio < 0.3f)
                OnTheRunTutorialManager.Instance.CanSpawnTrucks();

            if (turboRatio <= 0.0f)
            {
                turboCounter = 0;
                turboRatio = -1.0f;
                gameObject.SendMessage("RestoreDistancesForSpecialCar", player.PlayerRigidbody.position.z);
                if (IsSpecialCarActive)
                {
                    wasStartWithSpecialCar = false;
                    OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
                }
                player.SendMessage("OnTurboActive", false, SendMessageOptions.DontRequireReceiver);
                ingamePage.OnTurboActive(false);
                ingamePage.ResetTurboIndicators();
                player.gameObject.SendMessage("ActivateBlink", true);
            }
        }

        //turbo from truck turbo update
        if (truckTurboRatio >= 0.0f)
        {
            truckTurboRatio -= dt * (1.0f / slipstreamDuration);
            if (truckTurboRatio <= 0.0f)
            {
                ActivateTruckTurbo(false);
            }
        }

        if (magnetBonusTimer >= 0.0f)
        {
            magnetBonusTimer -= dt;
            if (magnetBonusTimer < 0.0f)
            {
                player.gameObject.SendMessage("ActivateMagnetFx", false, SendMessageOptions.DontRequireReceiver);
                magnetBonusTimer = -1.0f;
                isMagnetActive = false;
            }
        }
    }
    #endregion

    protected void BackToDefaultCar()
    {
        OnTheRunSoundsManager.Instance.StopSource(blinkCarSource);
        OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.BackToCar);
        carChangedTimer = -1.0f;
        bool wasSpecialCarActive = IsSpecialCarActive;
        OnChangeCarEvent(Vector3.zero);

        if (gameplayTime <= 0.0f)
        {
            if (wasSpecialCarActive)
                player.SendMessage("OnChangePlayerEffect");
            player.transform.position = new Vector3(player.transform.position.x, 0.0f, player.transform.position.z);
        }
        else
        {
            player.SendMessage("OnCollisionProtectionActivation", true);
        }
    }

    #region Utils
    void Log(string logStr)
    {
        if (logIsEnabled)
            Debug.Log("### OnTheRunGameplay - " + logStr);
    }

    bool CheckpointCheck()
    {
        float nextCheckpointdistance = spawnManager.NextCheckpointPosition - player.gameObject.transform.position.z;
        return nextCheckpointdistance > 120.0f;
    }
    #endregion

    #region Run parameter
    public void ResetRunParameter( )
    {
        slipstreamCounter = 0;
        maxCombo = 0;
        hitsCounter = 0;

        for (int i = 0; i < initboosters.Length; ++i)
            initboosters[i] = false;
    }

    public void UpdateRunParameter(RunParameters parameter, int value)
    {
        switch (parameter)
        {
            case RunParameters.HitsCount:
                hitsCounter += value;
                break;

            case RunParameters.Slipstream:
                slipstreamCounter += value;
                break;

            case RunParameters.MaxCombo:
                if (value > maxCombo)
                    maxCombo = value;
                break;

            case RunParameters.Booster:
                initboosters[value] = true;
                break;
        }
    }
    #endregion

    void OnApplicationPause(bool paused)
    {
        if (paused)
            checkForDailyBonusAfterPause = true; 
    }
}