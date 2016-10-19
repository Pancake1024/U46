using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections;

[AddComponentMenu("OnTheRun/OnTheRunTutorialManager")]
public class OnTheRunTutorialManager : Manager<OnTheRunTutorialManager>
{
    static public bool TUTORIAL_WITH_SLOWMOTION = true;

    #region Protected Members
    protected bool wasTutorialActive = false;
    protected bool tutorialActive = false;
    protected bool locationAndCarChanged = false;
    protected UIManager uiManager;
    protected UIPopup popupWithButton;
    protected bool tutorialOnScreen = false;
    protected bool inRallenty = false;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunSpawnManager spawnManager;

    protected float unskippablePopupTime = 1.0f;
    protected float unskippableTimer = -1.0f;

    protected int currentTutorialStep = 0;
    protected bool carAlreadySpawned = false;
    protected bool freezeControls = false;
    protected TutorialType currentTutorial = TutorialType.None;
    protected TutorialType nextTutorial = TutorialType.None;
    protected TutorialActions currentAction = TutorialActions.None;
    protected bool haveToTap = false;
    protected List<OpponentKinematics> carsList = new List<OpponentKinematics>();
    protected int carsListIndex = 0;
    protected TutorialActions playerAction = TutorialActions.None;
    protected int playerLaneDelta = 0;
    protected bool stopped = false;
    protected int inputDxCounter = 0;
    protected int inputSxCounter = 0;

    protected bool nextTutorialSpawnFlag = false;
    protected int nextTutorialSpawnIndex = 0;
    protected float[] nextTutorialTimerRef = { 0.0f, 0.5f, 2.5f };
    protected List<float> nextTutorialTimer;

    protected PlayerKinematics playerKin;
    public GameObject tutPopup;
    #endregion

    #region Public Enums
    public enum TutorialType
    {
        None = -1,
        FreeDrive = 0,
        AvoidFirstCar,
        SingleSlipstream,
        Turbo,
        Trucks,
        End
    }
    
    public enum TutorialActions
    {
        None = -1,
        TapEverywhere = 0,
        TapRight,
        TapLeft
    }
    #endregion

    #region Singleton instance
    public static OnTheRunTutorialManager Instance
    {
        get
        {
            return Manager<OnTheRunTutorialManager>.Get();
        }
    }
    #endregion

    #region Public properties
    public bool IsInRallenty
    {
        get
        {
            return tutorialActive && inRallenty;
        }
    }

    public int PlayerLaneDelta
    {
        get { return playerLaneDelta; }
    }

    public TutorialActions CurrentActionAllowed
    {
        get { return currentAction; }
    }

    public bool TutorialStopped
    {
        get { return stopped; }
    }
    
    public TutorialType NextTutorial
    {
        get { return nextTutorial; }
    }
    
    public int NextTutorialIndex
    {
        get { return nextTutorialSpawnFlag ? nextTutorialSpawnIndex : -1; }
    }
    
    public bool TutorialActive
    {
        get { return tutorialActive; }
        set { tutorialActive = value; }
    }

	public bool TutorialActiveAndIngame
	{
		get { return TutorialActive && UIManager.Instance.ActivePageName == "IngamePage"; }
	}

    public bool LocationAndCarChanged
    {
        get { return locationAndCarChanged; }
        set { locationAndCarChanged = value; }
    }

    public bool AvoidSpawnTraffic
    {
        get { return tutorialActive && (nextTutorial == TutorialType.SingleSlipstream || nextTutorial == TutorialType.Turbo); }
    }

    public bool WasTutorialActive
    {
        get { return wasTutorialActive; }
        set { wasTutorialActive = value; }
    }
    
    public bool IsTutorialOnScreen
    {
        get { return tutorialOnScreen; }
    }

    public bool DeactivatePlayerInputs
    {
        get { return freezeControls; }
        set { freezeControls = value; }
    }

    public bool CanBeSkipped
    {
        get { return unskippableTimer < 0.0f; }
    }
    #endregion

    #region Public methods
    public void ShowTutorial()
    {
        stopped = false;
        ShowPopup( );
    }

    public void ResetTutorial()
    {
        currentTutorialStep = 0;
    }

    public void StopTutorial()
    {
        stopped = true;
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();
        uiManager.PopPopup();
        currentAction = TutorialActions.None;
        carAlreadySpawned = false;
        tutorialOnScreen = false;
        currentTutorialStep = 0;
        uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrowBlink(false, 1);
        uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrowBlink(false, -1);
    }
    
    public void EndTutorial()
    {
        ShowTutorial();
    }

    public void UpdateTutorial(float dt)
    {
        if (nextTutorialSpawnIndex > nextTutorialTimer.Count - 1)
            return;

        if (nextTutorialTimer[nextTutorialSpawnIndex] > 0.0f)
        {
            nextTutorialTimer[nextTutorialSpawnIndex] -= dt;
            if (nextTutorialTimer[nextTutorialSpawnIndex]<0.0f)
                nextTutorialSpawnFlag = true;
        }
    }

    public void UpdateSkippableTutorialTimer( )
    {
        if (tutorialActive && unskippableTimer >= 0.0f && uiManager.ActivePageName == "IngamePage")
        {
            UIPage page = uiManager.ActivePage;
            unskippableTimer -= uiManager.UITimeSource.DeltaTime;

            if (unskippableTimer < 0.0f)
            {
                switch (currentTutorialStep)
                {
                    case 0:
                        page.GetComponent<UIIngamePage>().ActivateTapToContinue(true);
                        break;
                    case 1:
                        if (playerKin.CurrentLane == 0)
                        {
                            page.GetComponent<UIIngamePage>().ActivateArrows(false, -1);
                            page.GetComponent<UIIngamePage>().ActivateArrows(true, 1);
                            currentAction = TutorialActions.TapRight;
                        }
                        else if (playerKin.CurrentLane == 4)
                        {
                            page.GetComponent<UIIngamePage>().ActivateArrows(true, -1);
                            page.GetComponent<UIIngamePage>().ActivateArrows(false, 1);
                            currentAction = TutorialActions.TapLeft;
                        }
                        else
                        {
                            page.GetComponent<UIIngamePage>().ActivateArrows(true, -1);
                            page.GetComponent<UIIngamePage>().ActivateArrows(true, 1);
                            currentAction = TutorialActions.TapEverywhere;
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                        page.GetComponent<UIIngamePage>().ActivateTapToContinue(true);
                        break;
                }
            }
        }
    }

    public void ResetTutorialTimer( )
    {
        nextTutorialSpawnFlag = false;
        ++nextTutorialSpawnIndex;
    }

    public void CheckTutorialEnded()
    {
        //if (currentTutorialStep != 3 && wasTutorialActive)
        //    tutorialActive = true;

        wasTutorialActive = false;
    }

    public bool CanSpawnTrucks()
    {
        bool retValue = tutorialActive && nextTutorial == TutorialType.Trucks && !nextTutorialSpawnFlag;
        if (retValue)
        {
            //spawnManager.ResetAllDistances();
            nextTutorialSpawnFlag = true;
        }

        return retValue;
    }

    IEnumerator StartReadyGoSequenceDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        
        gameplayManager.StartReadyGoSequence();
    }

    protected void ShowPopup( )
    {
        //Debug.Log("***** ShowPopup " + currentTutorialStep);
        unskippableTimer = unskippablePopupTime;
        switch (currentTutorialStep)
        {
            case 0:
                currentTutorial = TutorialType.AvoidFirstCar;
                currentAction = TutorialActions.TapEverywhere;
                ActivateIngameArrows(false);
                ShowTutorialPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_beginningright"), OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_subtitle1"));
                OnTheRunOmniataManager.Instance.TrackTutorialStepFinished(TutorialType.AvoidFirstCar);
                break;
            case 1:
                currentTutorial = TutorialType.SingleSlipstream;
                currentAction = TutorialActions.TapRight;
                ShowTutorialPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_beginningleft"), OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_subtitle2"));
                break;
            case 2:
                currentTutorial = TutorialType.Turbo;
                currentAction = TutorialActions.TapEverywhere;
                ActivateIngameArrows(false);
                ShowTutorialPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_bolts"), OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_subtitle3"));
                OnTheRunOmniataManager.Instance.TrackTutorialStepFinished(TutorialType.SingleSlipstream);
                break;
            case 3:
                currentTutorial = TutorialType.Trucks;
                currentAction = TutorialActions.TapEverywhere;
                ActivateIngameArrows(false);
                ShowTutorialPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_specialvehicles"), OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_subtitle4"));
                OnTheRunOmniataManager.Instance.TrackTutorialStepFinished(TutorialType.Turbo);
                spawnManager.SpawnTrafficRandomly(100.0f, 2, 20);
                break;
            case 4:
                currentTutorial = TutorialType.End;
                currentAction = TutorialActions.TapEverywhere;
                ActivateIngameArrows(false);
                uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateSpecialVehicleTextForTutorial(false);
                ShowTutorialPopup(OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_end"), OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_subtitle5"));
                OnTheRunOmniataManager.Instance.TrackTutorialStepFinished(TutorialType.Trucks);
                break;
        }
    }

    public void OnInputProcessed(TutorialActions type)
    {
        if (!tutorialActive)
            return;

        bool checkInputs = (type == currentAction || currentAction == TutorialActions.TapEverywhere);
        
#if UNITY_WEBPLAYER
        checkInputs = (currentTutorialStep != 1 && type == currentAction) || (currentTutorialStep == 1 && type != TutorialActions.TapEverywhere && currentAction != TutorialActions.None);
#endif

        if (currentTutorial == TutorialType.FreeDrive)
        {
            if(type == TutorialActions.TapLeft)
                ++inputSxCounter;
            else
                ++inputDxCounter;

            if (inputSxCounter > 3 && inputDxCounter > 3)
            {
                ShowTutorial();
            }
        }
        else if (CanBeSkipped && checkInputs)
        {
            playerAction = type;
            GoNextStep();
        }
    }

    public bool CanSpawnNextObject( )
    {
        if (!carAlreadySpawned)
        {
            carAlreadySpawned = true;
            return true;
        }
        else
            return false;
    }

    public void AddTutorialCar(OpponentKinematics car)
    {
        carsList.Add(car);
    }

    public void RemoveTutorialCar()
    {
        carsList.RemoveAt(0);
        carsListIndex = 0;
    }

    public void ResetTutorialCar( )
    {
        carsList = new List<OpponentKinematics>();
        carsListIndex = 0;
    }

    public bool IsCurrentTutorialCar(OpponentKinematics oppKin)
    {
        if (carsListIndex >= carsList.Count)
            return false;
        else
            return carsList[carsListIndex]==oppKin;
    }
    #endregion

    #region Protected methods
    public void Initialize()
    {
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        if (spawnManager == null)
            spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();

        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();

        if (playerKin == null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        unskippableTimer = -1.0f;
        freezeControls = false; // tutorialActive;
        carAlreadySpawned = false;
        stopped = false;
        nextTutorialSpawnFlag = true;
        nextTutorialSpawnIndex = 0;
        nextTutorialTimer = new List<float>();
        nextTutorialTimer.AddRange(nextTutorialTimerRef);
        currentTutorialStep = 0;
        currentTutorial = TutorialType.FreeDrive;
        currentAction = TutorialActions.None;
        inputDxCounter = 0;
        inputSxCounter = 0;
        playerLaneDelta = 0;
        ResetTutorialCar();
        nextTutorial = TutorialType.AvoidFirstCar;

        ActivateIngameArrows(true);

        Manager<UIRoot>.Get().SetupBackground(OnTheRunEnvironment.Environments.Europe);

        spawnManager.SpawnTrafficRandomly(-150.0f, 8, 20);

        if (locationAndCarChanged)
        {
            ActivateIngameArrows(false);
            StartCoroutine(InitialTutorialPopupDelayed());
        }
    }

    IEnumerator InitialTutorialPopupDelayed( )
    {
        yield return new WaitForSeconds(0.1f);

        uiManager.PushPopup("InitialTutorialPopup", false);
        uiManager.FrontPopup.gameObject.transform.FindChild("content/popupTitle").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_warning_title");
        uiManager.FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_warning");
        uiManager.FrontPopup.transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        locationAndCarChanged = false;
    }

    protected void ShowTutorialPopup(string text, string titleText = null)
    {
        tutorialActive = true;
        wasTutorialActive = true;
        string popupToShow = "",
               popupTextfield = "",
               popupTitle = "";
        popupToShow = "TutorialPopup";
        popupTextfield = "content/popupTextField";
        popupTitle = "content/popupTitle";

        tutorialOnScreen = true;

        if (tutPopup==null)
            tutPopup = Manager<UIRoot>.Get().transform.FindChild("TutorialPopup").gameObject;

        tutPopup.GetComponent<UIPopup>().pausesGame = !OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION;

        uiManager.PushPopup(popupToShow);
        uiManager.FrontPopup.gameObject.transform.FindChild(popupTextfield).GetComponent<UITextField>().text = text;
		if (titleText == null)
        	uiManager.FrontPopup.gameObject.transform.FindChild(popupTitle).GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_title");
		else
			uiManager.FrontPopup.gameObject.transform.FindChild(popupTitle).GetComponent<UITextField>().text = titleText;

        if (OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION)
        {
            inRallenty = true;
            InterpolatedTimeEffects.Instance.AddInterpEffect(0.75f, 1.0f, 0.05f);
            StartCoroutine(DelayedPause(0.75f));    //0.4f));
        }

        Manager<UIManager>.Get().BroadcastMessage("OnTutorialPopupShown");
    }

    IEnumerator DelayedPause(float duration)
    {
        //yield return new WaitForSeconds(duration);

        var uiTimesource = UIManager.Instance.UITimeSource;
        float t = uiTimesource.TotalTime;
        while ((uiTimesource.TotalTime - t) < duration)
            yield return null;

        if (inRallenty)
            TimeManager.Instance.MasterSource.Pause();
    }

    public void StopRallenty()
    {
        if (OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION)
        {
            inRallenty = false;
            StopCoroutine("DelayedPause");
            InterpolatedTimeEffects.Instance.RemoveAllInterpEffects();
        }
    }

    protected void GoNextStep( )
    {
        if (OnTheRunTutorialManager.TUTORIAL_WITH_SLOWMOTION)
        {
            inRallenty = false;
            StopCoroutine("DelayedPause");
            InterpolatedTimeEffects.Instance.RemoveAllInterpEffects();

            if (TimeManager.Instance.MasterSource.IsPaused)
                TimeManager.Instance.MasterSource.Resume();

            InterpolatedTimeEffects.Instance.AddInterpEffect(0.25f, 0.2f, 1.0f);
        }

        currentAction = TutorialActions.None;
        carAlreadySpawned = false;
        tutorialOnScreen = false;
        ++currentTutorialStep;

        uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateTapToContinue(false);

        switch (currentTutorial)
        {
            case TutorialType.AvoidFirstCar:
                /*uiManager.PopPopup();
                gameplayManager.PlayerKinematics.ForceOneLaneDx = true;
                nextTutorial = TutorialType.SingleSlipstream;
                RemoveTutorialCar();
                ActivateIngameArrows(false);*/
                
                uiManager.PopPopup();
                nextTutorial = TutorialType.SingleSlipstream;
                ActivateIngameArrows(false);
                DeactivatePlayerInputs = true;
                spawnManager.SendMessage("OnTutorialChange", nextTutorial);
                break;
            case TutorialType.SingleSlipstream:
                /*uiManager.PopPopup();
                gameplayManager.PlayerKinematics.ForceOneLaneSx = true;
                nextTutorial = TutorialType.Turbo;
                ResetTutorialCar();
                ActivateIngameArrows(false);*/

                ResetTutorialCar();
                uiManager.PopPopup();
                nextTutorial = TutorialType.Turbo;
                ActivateIngameArrows(false);
                DeactivatePlayerInputs = true;
                if (playerAction == TutorialActions.TapLeft)
                {
                    playerLaneDelta = -1;
                    gameplayManager.PlayerKinematics.ForceOneLaneSx = true;
                }
                else
                {
                    playerLaneDelta = 1;
                    gameplayManager.PlayerKinematics.ForceOneLaneDx = true;
                }
                spawnManager.SendMessage("OnTutorialChange", nextTutorial);
                break;
            case TutorialType.Turbo:
                uiManager.PopPopup();
                nextTutorial = TutorialType.Trucks;
                uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrowBlink(false, 1);
                uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrowBlink(false, -1);
                break;
            case TutorialType.Trucks:
                uiManager.PopPopup();
                ResetTutorialCar();
                nextTutorial = TutorialType.None;
#if !UNITY_WEBPLAYER
                ActivateIngameArrows(true);
#endif
                break;
            case TutorialType.End:
                uiManager.PopPopup();
                nextTutorial = TutorialType.None;
#if !UNITY_WEBPLAYER
                ActivateIngameArrows(true);
#endif
                uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateSpecialVehicleTextForTutorial(true);
                if (tutorialActive)
                {
                    currentTutorialStep = 0;
                    tutorialActive = false;
                    freezeControls = false;
                    gameplayManager.StartReadyGoSequence();
                    //this.StartCoroutine(this.StartReadyGoSequenceDelayed());
                }
                break;
        }
    }

    public void ActivateIngameArrows(bool value)
    {
        uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrows(value, 1);
        uiManager.ActivePage.GetComponent<UIIngamePage>().ActivateArrows(value, -1);
    }
    #endregion

    #region Load/Save
    public void Load()
    {
        tutorialActive = EncryptedPlayerPrefs.GetInt("tut_act", 1) == 1;
    }

    public void Save()
    {
        EncryptedPlayerPrefs.SetInt("tut_act", tutorialActive ? 1 : 0);
    }
    #endregion
}