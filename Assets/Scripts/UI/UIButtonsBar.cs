using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIButtonsBar")]
public class UIButtonsBar: MonoBehaviour
{
    public LayerMask uiLayersGarage;
     
    public GameObject RankingItem;
    public GameObject SpecialItem;

    public GameObject FBFriendsItem;

    protected GameObject SpinWheelItem;
    protected GameObject OptionsWebItem;
    protected GameObject HelpWebItem;

    protected UITextField tfLogin;
    private UIManager uiManager;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected float[] buttonsStartYPosition = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    UIButton[] buttons;
    protected float selectedDeltaPos = 0.15f;

    void Awake()
    {
        buttons = gameObject.GetComponentsInChildren<UIButton>();
        for (int i = 0; i < buttons.Length; ++i)
        {
            buttonsStartYPosition[i] = buttons[i].gameObject.transform.localPosition.y;
        }

        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        SpinWheelItem = transform.FindChild("SpinWheelButton").gameObject;
        OptionsWebItem = transform.FindChild("btOptionsWeb").gameObject;
        HelpWebItem = transform.FindChild("btHelpWeb").gameObject;
#if !UNITY_WEBPLAYER
        OptionsWebItem.SetActive(false);
        HelpWebItem.SetActive(false);
#else
        transform.FindChild("btTiers").localPosition = SpecialItem.transform.localPosition;
        SpecialItem.transform.localPosition = SpinWheelItem.transform.localPosition;
#endif
    }

    void OnEnable()
    {
        transform.FindChild("SpinWheelButton/text").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("spin");
        transform.FindChild("btRankings/tfLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ranking");
        transform.FindChild("btTrucks/tfLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("special_btn");
        transform.FindChild("btTiers/tfLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("tiers_btn");
#if UNITY_WEBPLAYER
        transform.FindChild("btOptionsWeb/tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("options");
        transform.FindChild("btHelpWeb/tfOptionsLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_help");
#endif
        tfLogin = FBFriendsItem.transform.FindChild("tfOptionsLabel").GetComponent<UITextField>();
        tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("friends");
        uiManager = Manager<UIManager>.Get();
    }

    public void UpdateFacebookButtonText()
    {
        if (OnTheRunFacebookManager.Instance == null || OnTheRunDataLoader.Instance == null)
            return;

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.FB_LOGIN);
            //tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("logged");
            //tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onHover = Color.white;
            //tfLogin.gameObject.transform.parent.GetComponent<UIButtonsColorModified>().onPressed = Color.white;
            FBFriendsItem.SetActive(true);
            Manager<UIRoot>.Get().UpdateAvatarPicture();

            int pendingInvites = OnTheRunNotificationManager.Instance.PendingNotifications;
            if (pendingInvites > 0)
            {
                FBFriendsItem.transform.FindChild("Remaining").gameObject.SetActive(true);
                FBFriendsItem.transform.FindChild("Remaining/tfRemaining").GetComponent<UITextField>().text = pendingInvites.ToString();
            }
            else
                FBFriendsItem.transform.FindChild("Remaining").gameObject.SetActive(false);
        }
        else
        {
            //tfLogin.text = OnTheRunDataLoader.Instance.GetLocaleString("login");
            FBFriendsItem.SetActive(true);
            FBFriendsItem.transform.FindChild("Remaining").gameObject.SetActive(false);
        }

        FBFriendsItem.SetActive(true);
    }

    void OnShowButtonsBar()
    {
        foreach (UIButton b in buttons)
            b.gameObject.SetActive(true);

        UpdateFacebookButtonText();        
        RefreshSpinWheelButton();

#if UNITY_WEBPLAYER
        RankingItem.SetActive(false);
        
        FBFriendsItem.SetActive(false);
        SpinWheelItem.SetActive(false);
        
#else
        OptionsWebItem.SetActive(false);
        HelpWebItem.SetActive(false);
#endif
    }

    void RefreshSpinWheelButton()
    {
        UpdateSpinWheelButton();
        Manager<UIRoot>.Get().SetupWheelButtonBounce(transform.FindChild("SpinWheelButton").gameObject);
    }
    
    void OnButtonActive(UIButton button)
    {
        button.transform.localScale = Vector3.one;
        button.transform.localPosition += new Vector3(0.0f, selectedDeltaPos, 0.0f);
    }

    void OnReset( )
    {
        for (int i = 0; i < buttons.Length; ++i)
        {
            buttons[i].transform.localScale = Vector3.one;
            buttons[i].transform.localPosition = new Vector3(buttons[i].transform.localPosition.x, buttonsStartYPosition[i], buttons[i].transform.localPosition.z);
        }
    }

    #region Signals
    void Signal_OnOver(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

        button.transform.localScale = Vector3.one * 1.1f;
    }

    void Signal_OnOut(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

        button.transform.localScale = Vector3.one;
        /*UIToggleButton uiToggle = gameObject.transform.FindChild("Toggle").GetComponent<UIToggleButton>();
        foreach (UIButton b in buttons)
        {
            if (!uiToggle.IsActiveButton(b))
                b.transform.localScale = Vector3.one;
        }*/
    }

    void Signal_OnChangePage(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

        Manager<UIRoot>.Get().lastPageShown = "GaragePage";// "StartPage";
        Manager<UIRoot>.Get().lastPageVisited = uiManager.ActivePageName;

        Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().DisableFloatingStuff();

        switch (button.gameObject.name)
        {
            case "btConsumables":
                interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
                Debug.Log("CONSUMABLES: Signal_OnChangePage");
                break;
            case "btGarage":
                if (uiManager.ActivePageName != "GaragePage")
                    StartCoroutine(GoToNextPage("GaragePage"));
                break;
            case "btRankings":
                string activePageName = Manager<UIManager>.Get().ActivePageName;
                Manager<UIRoot>.Get().lastPageShown = activePageName;
                if (activePageName.Equals("GaragePage"))
                    UIRankingsPage.RankingsPageWorldId = Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIGaragePage>().CurrentSetIndex;
                else if (activePageName.Equals("RewardPage"))
                    UIRankingsPage.RankingsPageWorldId = Manager<UIRoot>.Get().GetComponent<UISharedData>().LastGarageLocationIndex;
                StartCoroutine(GoToNextPage("RankingsPage"));
                break;
            case "btTrucks":
                StartCoroutine(GoToNextPage("TrucksPage"));
                break;
            case "btTiers":
                StartCoroutine(GoToNextPage("StartPage"));
                break;
            case "SpinWheelButton":
                Manager<UIRoot>.Get().lastPageShown = "GaragePage";
                StartCoroutine(GoToNextPage("WheelPopup"));
                break;
            case "btFBFriends":
                if (OnTheRunFacebookManager.Instance.IsLoggedIn)
                {
                    Manager<UIRoot>.Get().lastPageShown = "StartPage";
                    StartCoroutine(GoToNextPage("FBFriendsPopup"));
                }
                else
                    Manager<UIRoot>.Get().ShowFBLoginSequence(SuccessCallback);
                break;
            case "btFBLogin":
                if (!OnTheRunFacebookManager.Instance.IsLoggedIn)
                {
                    ShowLoadingPopup();

                    OnTheRunFacebookManager.Instance.Login(
                        () =>
                        {
                            OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                            McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, null);
                            UpdateFacebookButtonText();
                            HideLoadingPopup();
                        },
                        () => { HideLoadingPopup(); },
                        () => { HideLoadingPopup(); });
                }
                break;
        }

        UpdateBarStatus();
    }

    void SuccessCallback(bool success)
    {
        Manager<UIRoot>.Get().HideLoadingPopup();

        if (success)
        {
            Manager<UIRoot>.Get().lastPageShown = "StartPage";
            StartCoroutine(GoToNextPage("FBFriendsPopup"));
        }
    }

    public void ShowLoadingPopup()
    {
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
            uiManager.PushPopup("LoadingPopup");
            if (uiManager.FrontPopup != null)
                uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
        }
    }

    public void HideLoadingPopup()
    {
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");

        Manager<UIRoot>.Get().ShowDailyBonusPopup();
    }

    void UpdateBarStatus()
    {
        OnReset();
        /*UIToggleButton uiToggle = gameObject.transform.FindChild("Toggle").GetComponent<UIToggleButton>();
        for (int i = 0; i < buttons.Length; ++i)
        {
            if (!uiToggle.IsActiveButton(buttons[i]))
            {
                buttons[i].transform.localScale = Vector3.one;
                buttons[i].transform.localPosition = new Vector3(buttons[i].transform.localPosition.x, buttonsStartYPosition[i], buttons[i].transform.localPosition.z);
            }
            else
            {
                buttons[i].transform.localScale = Vector3.one;
                if (buttons[i].transform.localPosition.y == buttonsStartYPosition[i])
                    buttons[i].transform.localPosition += new Vector3(0.0f, selectedDeltaPos, 0.0f);
                buttons[i].gameObject.GetComponent<UIButtonsColorModified>().SendMessage("UIButtonTextColor_OnPressed", buttons[i], SendMessageOptions.DontRequireReceiver);
            }
        }*/
    }

    void Signal_OnOptionsPage(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //OnTheRunUITransitionManager.Instance.OpenPopup("OptionsPopup");
        StartCoroutine(GoToNextPage("OptionsPopup"));
    }

    void Signal_OnHelpPage(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //OnTheRunUITransitionManager.Instance.OpenPopup("OptionsPopup");
        Manager<UIRoot>.Get().OpenHelpPopup();
    }
    #endregion

    #region Functions
    IEnumerator GoToNextPage(string nextPage)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        string currentPage = Manager<UIManager>.Get().ActivePageName;
        OnTheRunUITransitionManager.Instance.OnPageExiting(currentPage, nextPage);

        //Manager<UIManager>.Get().ActivePage.gameObject.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);

        //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        if (nextPage == "OptionsPopup" || nextPage == "WheelPopup" || nextPage == "FBFriendsPopup")
            Manager<UIManager>.Get().PushPopup(nextPage);
        else
            Manager<UIManager>.Get().GoToPage(nextPage);

        OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
    }

    public void UpdateSpinWheelButton()
    {
        int remainingSpinds = PlayerPersistentData.Instance.ExtraSpin;
        if (remainingSpinds > 0)
        {
            transform.FindChild("SpinWheelButton/Remaining").gameObject.SetActive(true);
            transform.FindChild("SpinWheelButton/Remaining/tfRemaining").GetComponent<UITextField>().text = remainingSpinds.ToString();
        }
        else
            transform.FindChild("SpinWheelButton/Remaining").gameObject.SetActive(false);
    }
    #endregion
}