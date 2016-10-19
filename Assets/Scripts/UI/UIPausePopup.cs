using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIPausePopup")]
public class UIPausePopup : MonoBehaviour
{
    //public Transform[] missionRows;
    public Transform closeButton;
    public Transform replayButton;
    public Transform resumeButton;
    public Transform helpButton;

    public GameObject layout1;
    public GameObject layout2;

    protected UIRoot uiRoot;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected List<OnTheRunSingleRunMissions.DriverMission> missionsList;
    protected UIButton restartButton;
    //protected UIButton btCheckSlipStream;
    //protected GameObject checkIcoYes;
    //protected GameObject checkIcoNo;

    private UISharedData uiSharedData;
    protected GameObject missionPanel;

    protected OnTheRunGameplay gameplayManager;

    void Awake()
    {
        //btCheckSlipStream = transform.FindChild("btCheckSleepStream").GetComponent<UIButton>();
        //checkIcoYes = btCheckSlipStream.gameObject.transform.FindChild("icon_check_yes").gameObject;
        //checkIcoNo = btCheckSlipStream.gameObject.transform.FindChild("icon_check_no").gameObject;
        //checkIcoYes.SetActive(false);

        missionPanel = transform.FindChild("Layout1/MissionPanelPopup").gameObject;
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("bg_black_gradient"));


    }

    void OnEnable()
    {
        uiRoot = Manager<UIRoot>.Get();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        
        restartButton = transform.FindChild("ReplayButton").GetComponent<UIButton>();

        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();

    }

    void OnShow(UIPopup popup)
    {
        if(gameplayManager==null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        AdaptText[] myList = GetComponentsInChildren<AdaptText>();
        foreach (AdaptText component in myList)
            component.SendMessage("Update");

        layout1.SetActive(!OnTheRunTutorialManager.Instance.TutorialActive);
        layout2.SetActive(OnTheRunTutorialManager.Instance.TutorialActive);
        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            //
            closeButton.localPosition = new Vector3(closeButton.localPosition.x, 0.0f, closeButton.localPosition.z);
            replayButton.localPosition = new Vector3(replayButton.localPosition.x, 0.0f, replayButton.localPosition.z);
            resumeButton.localPosition = new Vector3(resumeButton.localPosition.x, 0.0f, resumeButton.localPosition.z);
            helpButton.localPosition = new Vector3(helpButton.localPosition.x, 0.0f, helpButton.localPosition.z);
        }
        else
        {
            closeButton.localPosition = new Vector3(closeButton.localPosition.x, -0.9386783f, closeButton.localPosition.z);
            replayButton.localPosition = new Vector3(replayButton.localPosition.x, -0.9386783f, replayButton.localPosition.z);
            resumeButton.localPosition = new Vector3(resumeButton.localPosition.x, -0.9386783f, resumeButton.localPosition.z);
            helpButton.localPosition = new Vector3(helpButton.localPosition.x, -0.9386783f, helpButton.localPosition.z);
        }

        helpButton.gameObject.SetActive(false);

#if UNITY_WEBPLAYER
        helpButton.gameObject.SetActive(true);
        transform.FindChild("CloseButton").transform.localPosition += Vector3.left * 0.85f;
        transform.FindChild("ReplayButton").transform.localPosition += Vector3.right * 0.9f;
        transform.FindChild("ResumeButton").transform.localPosition += Vector3.right * 0.85f;
#endif

        //missionPanel.SetActive(!OnTheRunTutorialManager.Instance.TutorialActive);

        /*missionsList = OnTheRunSingleRunMissions.Instance.ActiveMissions;
        
       
       //reset rows...
       for (int i = 0; i < 3; ++i)
       {
           Transform currentRow = missionRows[i];
           currentRow.FindChild("tfDescription").GetComponent<UITextField>().text = " ";
           currentRow.FindChild("tfValue").gameObject.SetActive(false);
           currentRow.FindChild("icon_mission_completed").gameObject.SetActive(false);
       }

       //setup rows
       int currentRowIndex = 0;
       for (int i = 0; i < 1; ++i)
       {
           OnTheRunSingleRunMissions.DriverMission currMission = currentMission;// missionsList[i];
           Transform currentRow = missionRows[currentRowIndex];
           if (currMission != null)
           {
               bool missionPassed = currMission.checkCounter <= currMission.Counter || currMission.JustPassed || currMission.Done;
               currentRow.FindChild("tfDescription").GetComponent<UITextField>().text = currMission.description;
               currentRow.FindChild("tfValue").GetComponent<UITextField>().text = uiRoot.FormatTextNumber((int)(Mathf.Floor(currMission.Counter))) + "/" + uiRoot.FormatTextNumber(currMission.checkCounter);
               currentRow.FindChild("tfValue").gameObject.SetActive(!missionPassed);
               currentRow.FindChild("icon_mission_completed").gameObject.SetActive(missionPassed);
               ++currentRowIndex;
           }
       }
       */
    }

    void Signal_OnResumeRelease(UIButton button)
    {
        Manager<UIManager>.Get().ActivePage.SendMessage("ActivateResumeButton", false);
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();

        //OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);

		if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame )
		{
//			Debug.LogWarning("devo resumare musica");
			OnTheRunSoundsManager.Instance.ResumeForced();
		}
    }

    void Signal_OnRestartRelease(UIButton button)
    {
        OnTheRunSingleRunMissions.Instance.SendMessage("OnEndSession");

        OnTheRunTutorialManager.Instance.StopRallenty();

        //OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        gameplayManager.FirstTimeSaveMeShown = true;


#if UNITY_WEBPLAYER
        Manager<UIRoot>.Get().ShowAreYouSurePopup(UIRoot.AreYouSureType.Restart);
#else
        uiRoot.RestartFromGame();
#endif
    }

    void Signal_OnQuitRelease(UIButton button)
    {
        OnTheRunSingleRunMissions.Instance.SendMessage("OnEndSession");

        OnTheRunTutorialManager.Instance.StopRallenty();

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

#if UNITY_WEBPLAYER
        Manager<UIRoot>.Get().ShowAreYouSurePopup(UIRoot.AreYouSureType.Exit);
#else
        OnTheRunFBLoginPopupManager.Instance.justEnteredInGarage = true;
        uiRoot.QuitFromGame();
#endif
    }

    /*void QuitFromGame()
    {
        Manager<UIManager>.Get().PopPopup();

        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            OnTheRunTutorialManager.Instance.StopTutorial();
        }

        IntrusiveList<UIFlyer> flyers = Manager<UIManager>.Get().GetActiveFlyers("SimpleFlyer");
        UIFlyer node = flyers.Head;
        while (node != null)
        {
            UIFlyer tmp = node;
            node = node.next;
            tmp.onEnd.RemoveAllTargets();
            tmp.Stop();
        }

        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Offgame);
        Manager<UIRoot>.Get().StopGoingRewardIfNecessary();
        Manager<UIManager>.Get().GoToPage("GaragePage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("GaragePage", "InGamePage");
        GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("ResetEnvironment");

        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.OnGameplayFinished();

        //if (OnTheRunFuelManager.Instance.Fuel <= 0 && !OnTheRunFuelManager.Instance.IsFuelFreezeActive())
        //   Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
    }

    void RestartFromGame()
    {
        if (OnTheRunFuelManager.Instance.Fuel <= 0)
        {
            Manager<UIRoot>.Get().StopGoingRewardIfNecessary();

            if (OnTheRunTutorialManager.Instance.TutorialActive)
            {
                OnTheRunTutorialManager.Instance.StopTutorial();
            }

            Signal_OnQuitRelease(null);
            if (!OnTheRunFuelManager.Instance.IsFuelFreezeActive())
            {
                if (PlayerPersistentData.Instance.FirstTimeFuelFinished && OnTheRunDataLoader.Instance.GetFirstFuelGiftEnabled())
                    OnTheRunUITransitionManager.Instance.OpenPopup("FuelGiftPopup");
                else
                    Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
            }
        }
        else
        {
            Manager<UIRoot>.Get().StopGoingRewardIfNecessary();
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            Manager<UIManager>.Get().PopPopup();

            if (OnTheRunTutorialManager.Instance.TutorialActive)
            {
                OnTheRunTutorialManager.Instance.StopTutorial();
            }

            Manager<UIManager>.Get().GetPage("GaragePage").GetComponent<UIGaragePage>().RestartSession();

            if (OnTheRunIngameHiScoreCheck.Instance)
            {
                //long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresIngameRanks() - 1; //99;
                //McSocialApiManager.Instance.GetScoresForIngame(true, true, McSocialApiUtils.ScoreType.Latest, scoresSpread, OnTheRunMcSocialApiData.Instance.GetLeaderboardId(uiSharedData.LastGarageLocationIndex));
                McSocialApiManager.Instance.GetScoresForIngame(OnTheRunMcSocialApiData.Instance.GetLeaderboardId(uiSharedData.LastGarageLocationIndex));

                OnTheRunIngameHiScoreCheck.Instance.OnGameplayRestart();
            }
        }
    }*/

    void OnBackButtonAction()
    {
        UIButton pauseButton = transform.FindChild("ResumeButton").GetComponent<UIButton>();
        pauseButton.onReleaseEvent.Invoke(pauseButton);
    }

    void Signal_OnHelpRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIRoot>.Get().OpenHelpPopup();
    }
}