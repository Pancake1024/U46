using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections;

[AddComponentMenu("OnTheRun/OnTheRunUITransitionManager")]
public class OnTheRunUITransitionManager : Manager<OnTheRunUITransitionManager>
{
    private bool SHOW_LOGS = false;

    public static float delayFactor = 0.0f;
    public static float changePageDelay = 0.5f;

    public GameObject[] StartPageNavBar;
    public GameObject[] TopNavBar;
    public GameObject[] BottomNavBar;

    public bool missionsFromStartPage = false;
    public bool isInTransition = false;
    public bool popupOpened = false;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UIRoot uiRoot;
    protected UIManager uiManager;
    protected string openAnotherPopup = "";
    protected string anotherPopupText = "";
    protected bool anotherPopupFireworks = false;

    protected Action closePopupCallback = null;

    public bool ButtonsCantWork
    {
        get { return isInTransition || popupOpened; }
    }

    #region Singleton instance
    public static OnTheRunUITransitionManager Instance
    {
        get
        {
            return Manager<OnTheRunUITransitionManager>.Get();
        }
    }
    #endregion

    #region Functions
    void Awake()
    {
        StartPageNavBar = new GameObject[7];
        StartPageNavBar[0] = transform.FindChild("StartPage/AnchorBottomLeft").gameObject;
        StartPageNavBar[1] = transform.FindChild("StartPage/AnchorTop").gameObject;
        StartPageNavBar[2] = transform.FindChild("StartPage/BgDown").gameObject;
        StartPageNavBar[3] = transform.FindChild("StartPage/Logo/otr_logo_main").gameObject;
        StartPageNavBar[4] = transform.FindChild("StartPage/AnchorBottomRight").gameObject;
        StartPageNavBar[5] = transform.FindChild("StartPage/CenterAnchor").gameObject;
        StartPageNavBar[6] = transform.FindChild("StartPage/Logo/bg_black_alpha").gameObject;

        TopNavBar[0] = transform.FindChild("CurrencyBar/CoinsItem").gameObject;
        TopNavBar[1] = transform.FindChild("CurrencyBar/DiamondsItem").gameObject;
        TopNavBar[2] = transform.FindChild("CurrencyBar/FuelItem").gameObject;
        TopNavBar[3] = transform.FindChild("OffgameBG/BGup").gameObject;
        TopNavBar[4] = transform.FindChild("TopLeftObjects/btHome").gameObject;
        TopNavBar[5] = transform.FindChild("TopLeftObjects/LevelBar").gameObject;
        TopNavBar[6] = transform.FindChild("CurrencyBar/linksBar").gameObject;

        BottomNavBar = new GameObject[7];
#if UNITY_WEBPLAYER
        BottomNavBar[0] = transform.FindChild("ButtonsBar/btOptionsWeb").gameObject;
        BottomNavBar[1] = transform.FindChild("ButtonsBar/btHelpWeb").gameObject;
#else
        BottomNavBar[0] = transform.FindChild("ButtonsBar/btRankings").gameObject;
        BottomNavBar[1] = transform.FindChild("ButtonsBar/SpinWheelButton").gameObject;
#endif
        BottomNavBar[2] = transform.FindChild("ButtonsBar/btFBFriends").gameObject;
        BottomNavBar[3] = transform.FindChild("ButtonsBar/btTrucks").gameObject;
        BottomNavBar[4] = transform.FindChild("GaragePage/AnchorBottomRight").gameObject;
        BottomNavBar[5] = transform.FindChild("OffgameBG/BgDown").gameObject;
        BottomNavBar[6] = transform.FindChild("ButtonsBar/btTiers").gameObject;

        transform.FindChild("RewardPage").GetComponent<UIRewardPage>().bottomBar = transform.FindChild("RewarBottomBar").gameObject;
    }
  
    void Initialize()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        CheckForManagers();
    }

    void CheckForManagers()
    {
        if (uiRoot == null)
            uiRoot = Manager<UIRoot>.Get();
        if (uiManager == null)
            uiManager = Manager<UIManager>.Get();

    }
    #endregion

    #region Page Changes
    public void OnPageExiting(string currentPageName, string nextPageName)
    {
        //SBS.Miniclip.UtilsBindings.ConsoleLog("***************************************************DEBUG LOG -- UITransitionManager OnPageExiting - curr: " + currentPageName + " - next: " + nextPageName);

        CheckForManagers();

        if (!uiRoot.IsMissionAnimationPlaying)
            uiManager.disableInputs = true;
        PlayExitPanelSound();

        UIEnterExitAnimations.activeAnimationsCounter = 0;
        if (SHOW_LOGS)
            Debug.Log("************** OnPageExiting: " + currentPageName + " " + nextPageName);
        switch (currentPageName)
        {
            case "StartPage":
                OnMoveStartPageNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName != "GaragePage" && nextPageName != "RankingsPage" && nextPageName != "WheelPopup" && nextPageName != "OptionsPopup" && nextPageName != "FBFriendsPopup")
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                
                if (nextPageName == "InGamePageDirectly")
                {
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }
                break;
            case "RankingsPage":
                OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                if (nextPageName != "StartPage" && nextPageName != "CurrencyPopup" && nextPageName != "FBFriendsPopup")
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                break;
            case "GaragePage":

				uiManager.ActivePage.GetComponent<UIGaragePage>().missionPanel.SkipNewMissionAnim();

				uiManager.ActivePage.GetComponent<UIGaragePage>().ClearSteppers();

                if (nextPageName == "StartPage")
                {
                    //OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }
                if (nextPageName == "RankingsPage")
                {
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }

                if (nextPageName == "InGamePage")
                {
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }
                if (nextPageName == "InGamePageDirectly")
                {
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }
                if (nextPageName == "OptionsPopup")
                {
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                }

                if (nextPageName == "TrucksPage")
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);

                //if (nextPageName == "WheelPopup")
                //    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);

                if (nextPageName != "CurrencyPopup")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                uiManager.ActivePage.BroadcastMessage("DeactivateWebArrows");
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                break;
            case "TrucksPage":

				uiManager.ActivePage.GetComponent<UITrucksPage>().ClearSteppers();

                if (nextPageName == "RankingsPage")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName == "StartPage")
                {
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);
                }
                else if (nextPageName != "CurrencyPopup" && nextPageName != "OptionsPopup")
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);

                if (nextPageName == "OptionsPopup")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);

                if (nextPageName == "WheelPopup")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);

                if (OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate)
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);

                uiManager.ActivePage.BroadcastMessage("DeactivateWebArrows");
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                break;
            case "InAppPage":
                if (nextPageName != "CurrencyPopup")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName == "InGamePage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                else if (nextPageName == "GaragePage")
                    StartCoroutine(FadeDelayed(0.3f));

                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                break;
            case "RewardPage":
                if (nextPageName != "WheelPopup")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);

                if (nextPageName == "TrucksPage")
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);

                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                uiManager.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitRewardNavBar);
                break;
            case "WheelPopup":
                if (nextPageName != "StartPage" && nextPageName != "CurrencyPopup" && nextPageName != "RewardPage" && nextPageName != "GaragePage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName == "RewardPage" || nextPageName == "StartPage")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOutPopup);
                break;
            case "FBFriendsPopup":
                if (nextPageName != "GaragePage" && nextPageName != "CurrencyPopup" && nextPageName != "StartPage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);

                if (nextPageName == "StartPage")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);

                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOutPopup);
                break;
            case "OptionsPopup":
                if (nextPageName != "StartPage" && nextPageName != "CurrencyPopup" && nextPageName != "GaragePage" && nextPageName != "TrucksPage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName == "StartPage")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOutPopup);
                break;
            case "CurrencyPopup":
                if (nextPageName == "StartPage" || nextPageName == "RewardPage")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.ExitNavBar);
                if (nextPageName == "SaveMePopup" || nextPageName == "DailyBonusSequencePopup")
                {
                    UIEnterExitAnimations.activeAnimationsCounter = 0;
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                }
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOutPopup);
                break;
        }

        switch (nextPageName)
        {
            case "CurrencyPopup":
                OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.ExitNavBar);
                break;
        }
        isInTransition = true;
    }

    public void OnPageChanged(string currentPageName, string pastPageName)
    {
        //SBS.Miniclip.UtilsBindings.ConsoleLog("***************************************************DEBUG LOG -- UITransitionManager OnPageChanged - curr: " + currentPageName + " - past: " + pastPageName);

        CheckForManagers();

        if (!uiRoot.IsMissionAnimationPlaying)
            uiManager.disableInputs = false;
        PlayEnterPanelSound();

        if (OnTheRunChartboostManager.Instance != null)
            OnTheRunChartboostManager.Instance.OnPageChanged(currentPageName);

        if (OnTheRunMoPubManager.Instance != null)
            OnTheRunMoPubManager.Instance.OnPageChanged();

        if (SHOW_LOGS)
            Debug.Log("************** OnPageChanged: " + currentPageName + " " + pastPageName + " " + OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate);
        switch (currentPageName)
        {
            case "StartPage":
                delayFactor = -0.7f;//-0.45f;
                if (pastPageName == "init")
                {
                    uiRoot.ShowRewardBar(false);

                    OnMoveStartPageNavBar(UIEnterExitAnimations.AnimationType.EnterPage);
                    OnMoveStartPageNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    if (pastPageName != "RankingsPage" && pastPageName != "WheelPopup" && pastPageName != "OptionsPopup" && pastPageName != "FBFriendsPopup" && !missionsFromStartPage)
                        uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);

                    Manager<UIRoot>.Get().ShowPageBorders(false);
                    Manager<UIRoot>.Get().ShowMainLogo(true);
                    Manager<UIRoot>.Get().ShowTopLeftObjects(false, false);
                    Manager<UIRoot>.Get().ShowCommonPageElements(true, false, true, false, false);
                    uiRoot.ShowUpperPageBorders(true);
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar, 1.5f);

                    StartCoroutine(PlayEnterPanelSoundDelayed(1.5f + delayFactor));
                }
                else //(pastPageName == "CurrencyPopup" || pastPageName != "init")
                {
                    Manager<UIRoot>.Get().ShowCommonPageElements(true, false, true, false, false);
                    uiRoot.ShowUpperPageBorders(true);
                    Manager<UIRoot>.Get().ShowMainLogo(false);
                    OnMoveStartPageNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar, -1.5f);
                    if(pastPageName == "CurrencyPopup" )
                        uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                    else
                        uiManager.ActivePage.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);

                    OnTheRunInterstitialsManager.Instance.TriggerInterstitial(OnTheRunInterstitialsManager.TriggerPoint.BackToMainScreen);
                    uiManager.ActivePage.SendMessage("RefreshVideoButton", SendMessageOptions.DontRequireReceiver);
                }

                uiManager.ActivePage.SendMessage("RefreshSpinWheelButton");
                missionsFromStartPage = false;
                delayFactor = 0.0f;
                break;
            case "RankingsPage":
                uiRoot.ShowCommonPageElements(false, true, false, false, false);
                OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);

                uiManager.ActivePage.SendMessage("ResetPageBeforeFriendsPopup");

                if (pastPageName != "StartPage" && pastPageName != "CurrencyPopup")
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                else
                    uiManager.ActivePage.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                break;
            case "GaragePage":
                if (pastPageName == "StartPage")
                {
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);

                    if (!OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate && !OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup)
                        uiManager.ActivePage.SendMessage("InitializeButtonsBarStatus");
                }
                if (pastPageName == "InGamePage")
                {
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);

                    if (!OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate && !OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup)
                        uiManager.ActivePage.SendMessage("InitializeButtonsBarStatus");
                }
                if (pastPageName == "RankingsPage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                if (pastPageName != "OptionsPopup" && pastPageName != "WheelPopup" && pastPageName != "FBFriendsPopup")
                {
                    if (pastPageName == "TrucksPage")
                    {
                        //uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                    }
                    else if (pastPageName != "CurrencyPopup")
                    {
                        uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                        //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    }
                }
                if (pastPageName == "RewardPage")
                {
                    uiRoot.ShowTopLeftObjects(true, true);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                if (pastPageName == "OptionsPopup")
                {
                    uiRoot.ShowCommonPageElements(true, true, true, true, false);
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                if (pastPageName == "InAppPage")
                    uiRoot.ShowTopLeftObjects(true, true);

                if (pastPageName == "WheelPopup" || pastPageName == "FBFriendsPopup")
                {
                    Manager<UIRoot>.Get().ShowPageBorders(true);
                    uiRoot.ShowCommonPageElements(true, true, true, true, false);
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    uiRoot.ShowOffgameBG(true);
                }

                if (pastPageName == "TrucksPage")
                    uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);

                OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                break;
            case "TrucksPage":
                if (pastPageName == "RankingsPage")
                {
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                if (pastPageName != "CurrencyPopup" && pastPageName != "OptionsPopup")
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                if (pastPageName == "OptionsPopup")
                {
                    uiRoot.ShowCommonPageElements(true, true, true, true, false);
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                if (pastPageName == "RewardPage")
                {
                    //OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    uiRoot.ShowTopLeftObjects(true, true);
                    uiRoot.ShowCommonPageElements(true, true, true, false, false);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                }

                //if (pastPageName == "CurrencyPopup")
                //    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);

                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                break;
            case "InAppPage":
                uiRoot.ShowTopLeftObjects(true, true);
                //Debug.Log("pastPageName: " + pastPageName);
                if (pastPageName == "RewardPage")
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);

                if (pastPageName == "InGamePage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);

                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOut);

                //uiRoot.ShowBottomPageBorders(true);

                OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                break;
            case "RewardPage":
                if (UIRewardPage.firstStep && !UIRewardPage.alreadyEntered)
                {
                    UIRewardPage.alreadyEntered = true;
                    uiRoot.ShowRewardBarLeftButtons(true);
                    if (pastPageName == "CurrencyPopup")
                    {
                        uiRoot.ShowTopLeftObjects(false, true);
                        uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    }
                    else if (pastPageName != "WheelPopup")
                    {
                        OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                        uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                    }

                    //uiRoot.ShowRewardBar(true);
                    uiManager.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterRewardNavBar);
                    uiManager.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterRewardNavBar);

                    if (pastPageName == "WheelPopup")
                    {
                        uiRoot.ShowCommonPageElements(true, false, true, false, false);
                        uiRoot.ShowUpperPageBorders(true);
                        uiRoot.ShowOffgameBG(true);
                    }
                    uiRoot.ShowBottomPageBorders(false);
                    uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                }
                else
                {
                    if (pastPageName == "CurrencyPopup")
                    {
                        uiRoot.ShowTopLeftObjects(false, true);
                        uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    }
                    else if (pastPageName != "WheelPopup")
                    {
                        OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                        uiManager.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
                    }
                    //uiRoot.ShowRewardBar(true);
                    if (pastPageName == "WheelPopup")
                    {
                        uiRoot.ShowCommonPageElements(true, false, true, false, false);
                        uiRoot.ShowUpperPageBorders(true);
                        uiRoot.ShowOffgameBG(true);
                    }
                    uiRoot.ShowBottomPageBorders(false);
                    UIManager.Instance.ActivePage.GetComponent<UIRewardPage>().transform.FindChild("CenterScreenAnchor/AnimAnchor").SendMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                    UIManager.Instance.ActivePage.GetComponent<UIRewardPage>().transform.FindChild("CenterScreenAnchor/MissionRewardPanel").SendMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                    UIManager.Instance.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.NextPageStepEnter);
                    Manager<UIRoot>.Get().GetComponentInChildren<UIRewardBar>().BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.NextPageStepEnter);
                }
                uiManager.ActivePage.SendMessage("RefreshSpinWheelButton");
                break;
            case "WheelPopup":
                if (pastPageName != "StartPage" && pastPageName != "CurrencyPopup" && pastPageName != "RewardPage" && pastPageName != "GaragePage" && pastPageName != "TrucksPage")
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                if (pastPageName == "RewardPage" || pastPageName == "StartPage")
                {
                    uiRoot.ShowTopLeftObjects(true, true);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);

                if (pastPageName == "CurrencyPopup")
                    uiManager.FrontPopup.BroadcastMessage("ShowNoMoreSpinPopup", false);
                break;
            case "FBFriendsPopup":
                if (pastPageName != "GaragePage" && pastPageName != "StartPage")
                {
                    uiRoot.ShowUpperPageBorders(true);
                    uiRoot.ShowCommonPageElements(true, true, true, false, false);

                    if (pastPageName != "CurrencyPopup")
                        OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                else if (pastPageName == "StartPage")
                {
                    uiRoot.ShowTopLeftObjects(true, true);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }

                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
                break;
            case "OptionsPopup":
                if (pastPageName != "CurrencyPopup" && pastPageName != "GaragePage" && pastPageName != "TrucksPage")
                {
                    if (pastPageName == "StartPage")
                    {
                        uiRoot.ShowCommonPageElements(true, true, true, false, false);
                        uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    }
                    else
                    {
                        uiRoot.ShowUpperPageBorders(true);
                        uiRoot.ShowCommonPageElements(true, true, true, false, false);
                        OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                    }
                }
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
                break;
            case "CurrencyPopup":
                if (pastPageName == "RewardPage" || pastPageName == "StartPage")
                {
                    uiRoot.ShowTopLeftObjects(true, true);
                    uiRoot.MoveHomeButton(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                
                if (pastPageName == "SaveMePopup" || pastPageName == "DailyBonusSequencePopup")
                {
                    uiRoot.ShowCommonPageElements(true, true, true, false, false);
                    OnMoveTopNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                }
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
                uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
                break;
            case "SaveMePopup":
                if (pastPageName.Equals("CurrencyPopup"))
                {
                    uiRoot.ShowTopLeftObjects(false, false);
                    uiRoot.ShowCommonPageElements(false, false, false, false, false);
                }
                break;
            case "InGamePage":
                uiManager.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage, SendMessageOptions.DontRequireReceiver);
                break;
        }


        switch (pastPageName)
        {
            case "CurrencyPopup":
                if (currentPageName == "GaragePage")
                    OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType.EnterNavBar);
                break;
        }

        isInTransition = false;
    }

    public void OpenPopup(string popupName)
    {
        if (popupName == "SingleButtonPopup" || popupName == "OptionsPopup" || popupName == "SinglePopup" || popupName == "HelpPopup")
            popupOpened = true;

        PlayEnterPanelSound();
        uiManager.gameObject.BroadcastMessage("ResetAnimationsCounter");

        if (popupName.Equals("WheelFeedbackPopup") || popupName.Equals("SingleButtonPopup"))
            uiManager.PushPopup(popupName, false);
        else
            uiManager.PushPopup(popupName, true);

        if (TimeManager.Instance.MasterSource.IsPaused)
            TimeManager.Instance.MasterSource.Resume();

        uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPopup);
        uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    public void ClosePopup(Action callback = null)
    {
        closePopupCallback = callback;

        string popupName = uiManager.FrontPopup.name;
        if (popupName == "SingleButtonPopup" || popupName == "OptionsPopup" || popupName == "SinglePopup" || popupName == "HelpPopup")
            popupOpened = false;

        if (popupName == "SingleButtonPopup" && OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup)
        {
            OnTheRunDailyBonusManager.Instance.ShowFirstTimeMissionPopup = false;
            Manager<UIRoot>.Get().lastPageShown = "GaragePage"; // FORCE THIS TO AVOID GOING TO START PAGE AFTER CLOSING THE MISSIONS PAGE
        }

        uiManager.disableInputs = true;

        PlayExitPanelSound();
        uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPopup);
        uiManager.FrontPopup.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeOutPopup);
        this.StartCoroutine(this.ClosePopupDelayed());
    }

    public void ClosePopupAndOpen(string nextPopup, string nextPopupText, bool startfireworks = false)
    {
        openAnotherPopup = nextPopup;
        anotherPopupText = nextPopupText;
        anotherPopupFireworks = startfireworks;
        ClosePopup();
    }

    IEnumerator ClosePopupDelayed()
    {
        string popupName = uiManager.FrontPopup.name;

        //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }
        uiManager.PopPopup();
        uiManager.disableInputs = false;

        if (closePopupCallback != null)
            closePopupCallback();

        if (openAnotherPopup != "")
        {
            if (openAnotherPopup.Equals("NoMoreDiamonds"))
            {
                uiRoot.ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
            }
            else if (openAnotherPopup.Equals("SaveMe"))
            {
                OpenPopup(openAnotherPopup);
            }
            else
            {
                OpenPopup(openAnotherPopup);
                uiManager.FrontPopup.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = anotherPopupText;
                if (anotherPopupFireworks)
                    uiManager.FrontPopup.SendMessage("StartFireworks");
            }
            openAnotherPopup = "";
        }

        if (uiManager.PopupsInStack >= 1)
            uiManager.FrontPopup.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    public void OpenBuyPopup(string titleText, string descriptionText)
    {
        OpenPopup("SingleButtonFeedbackPopup");
        if (uiManager == null)
            uiManager = uiManager;

        uiManager.FrontPopup.transform.FindChild("content/titletText").GetComponent<UITextField>().text = titleText;
        uiManager.FrontPopup.transform.FindChild("content/descriptionTextField").GetComponent<UITextField>().text = descriptionText;
        uiManager.FrontPopup.transform.FindChild("content/ResumeButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        uiManager.FrontPopup.transform.FindChild("content/ResumeButton").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "OnBuyPopupClosed");
    }


    public void OnBuyPopupClosed(UIButton button)
    {
        ClosePopup();
        button.onReleaseEvent.RemoveTarget(gameObject);
    }

    protected void PlayEnterPanelSound()
    {
        if (interfaceSounds == null)
            Initialize();

        interfaceSounds.PanelEnter();
    }

    protected void PlayExitPanelSound()
    {
        if (interfaceSounds == null)
            Initialize();

        interfaceSounds.PanelExit();
    }

    IEnumerator PlayEnterPanelSoundDelayed(float time)
    {
        yield return new WaitForSeconds(time);

        interfaceSounds.PanelEnter();
    }

    IEnumerator FadeDelayed(float time)
    {
        yield return new WaitForSeconds(time);

        Manager<UIManager>.Get().ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeIn);
    }
    #endregion

    #region Messages
    public void OnResetStartPageNavBar(UIEnterExitAnimations.AnimationType type)
    {
        foreach (GameObject go in StartPageNavBar)
        {
            if (go.activeInHierarchy)
                go.SendMessage("ResetEnterExitAnimation", type);
        }
    }

    public void OnMoveStartPageNavBar(UIEnterExitAnimations.AnimationType type, float delay = 0.0f)
    {
        foreach (GameObject go in StartPageNavBar)
        {
            if (go.activeInHierarchy)
            {
                if (delay != 0.0f)
                    go.GetComponent<UIEnterExitAnimations>().StartEnterExitAnimationWithParameters(type, -1.5f);
                else
                    go.SendMessage("StartEnterExitAnimation", type);
            }
        }
    }

    public void OnMoveTopNavBar(UIEnterExitAnimations.AnimationType type, float delay = 0.0f)
    {
        foreach (GameObject go in TopNavBar)
        {
            if (go.activeInHierarchy)
            {
                if (delay != 0.0f)
                    go.GetComponent<UIEnterExitAnimations>().StartEnterExitAnimationWithParameters(type, delay);
                else
                    go.SendMessage("StartEnterExitAnimation", type);
            }
        }
    }

    public void OnMoveBottomNavBar(UIEnterExitAnimations.AnimationType type)
    {
        foreach (GameObject go in BottomNavBar)
        {
            if (go.activeInHierarchy)
            {
                go.SendMessage("StartEnterExitAnimation", type);
            }
        }
    }

    public void InitializeBackground(Transform obj)
    {
        if (obj.GetComponent<Renderer>() != null)
        {
            foreach (Material mObj in obj.GetComponent<Renderer>().materials)
            {
                mObj.color = new Color(mObj.color.r, mObj.color.g, mObj.color.b, 0.0f);
            }
        }
        else
        {
            Color mObjColor = obj.GetComponent<UINineSlice>().color;
            mObjColor = new Color(mObjColor.r, mObjColor.g, mObjColor.b, 0.0f);
            obj.GetComponent<UINineSlice>().color = mObjColor;
            obj.GetComponent<UINineSlice>().ApplyParameters();
        }
    }
    #endregion
}