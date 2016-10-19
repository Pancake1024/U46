using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UISingleButtonPopup")]
public class UISingleButtonPopup : MonoBehaviour
{
    protected GameObject bottomBar;
    public bool goToRewardPageFlag = false;
    public UITextField tfOk;

    OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        //transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        if (null != tfOk)
            tfOk.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");

#if UNITY_WEBPLAYER
        if (null != tfOk)
        {
            if (gameObject.name.Equals("HelpPopup") || gameObject.name.Equals("HighscoresPopup"))
                tfOk.text = OnTheRunDataLoader.Instance.GetLocaleString("web_back");
        }
#endif
        gameObject.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    void StartFireworks( )
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("fireworks"));
    }

    void Signal_OnCancelRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
#if UNITY_WEBPLAYER
        if (TimeManager.Instance.MasterSource.IsPaused)
            Manager<UIManager>.Get().PopPopup();
        else
            OnTheRunUITransitionManager.Instance.ClosePopup();
#else
        OnTheRunUITransitionManager.Instance.ClosePopup();
#endif
    }

    void Signal_OnOkRelease(UIButton button)
    {
    }

    void Signal_OnOkSingleButtonRelease(UIButton button)
    {
        OnTheRunFireworks.Instance.ClearFireworks();
        Manager<UIRoot>.Get().PauseExperienceBarAnimation(false);
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

#if UNITY_WEBPLAYER
        if (Manager<UIManager>.Get().IsPopupInStack("SaveMePopup") && Manager<UIManager>.Get().ActivePageName == "IngamePage")
        {
            Manager<UIManager>.Get().FrontPopup.pausesGame = false;
            Manager<UIManager>.Get().PopPopup();
            Manager<UIManager>.Get().ClearPopups();
            Manager<UIManager>.Get().PushPopup("SaveMePopup");
        }
        else
        {
            OnTheRunUITransitionManager.Instance.ClosePopup();
        }
#else
        OnTheRunUITransitionManager.Instance.ClosePopup();
#endif

        if (goToRewardPageFlag)
        {
            goToRewardPageFlag = false;
            if (bottomBar==null)
                bottomBar = Manager<UIRoot>.Get().transform.FindChild("RewarBottomBar").gameObject;


            bottomBar.GetComponent<UIRewardBar>().StartCoroutine(Manager<UIRoot>.Get().SkipMissionsPage());
        }
    }

    void Signal_OnOkTutorialButtonRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunTutorialManager.Instance.ActivateIngameArrows(true);
        Manager<UIManager>.Get().PopPopup();
    }

    void Signal_OnHelpButtonRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        if (TimeManager.Instance.MasterSource.IsPaused)
            Manager<UIManager>.Get().PopPopup();
        else
            OnTheRunUITransitionManager.Instance.ClosePopup( );
    }

    void Signal_OnShow(UIPopup popup)
    {
    }

    void Signal_OnHide(UIPopup popup)
    {
    }

    void OnBackButtonAction()
    {
        UIButton cancelButton = null;
        if (transform.FindChild("content/CancelButton") != null)
            cancelButton = transform.FindChild("content/CancelButton").GetComponent<UIButton>();

        UIButton okButton = null;
        if (transform.FindChild("content/OkButton") != null)
            okButton = transform.FindChild("content/OkButton").GetComponent<UIButton>();

        if (cancelButton != null && cancelButton.State != UIButton.StateType.Disabled)
            cancelButton.onReleaseEvent.Invoke(cancelButton);
        else if (okButton != null && okButton.State != UIButton.StateType.Disabled)
            okButton.onReleaseEvent.Invoke(okButton);
        else
            Signal_OnCancelRelease(null);
    }
}