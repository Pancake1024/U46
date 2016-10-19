using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UITrucksAdvisePopup")]
public class UITrucksAdvisePopup : MonoBehaviour
{
    public UITextField tfTextBig;
    public UITextField tfTextSmall;
    public UITextField tfTap;

    OnTheRunInterfaceSounds interfaceSounds;

    protected bool closed = false;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));

        tfTextBig.text = OnTheRunDataLoader.Instance.GetLocaleString("you_know");
        tfTextSmall.text = OnTheRunDataLoader.Instance.GetLocaleString("trucks_advise");

#if UNITY_WEBPLAYER
        tfTap.text = OnTheRunDataLoader.Instance.GetLocaleString("web_pressspacetocontinue");
#else
        tfTap.text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_tap");
#endif
        //transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        closed = false;
    }

    void StartFireworks( )
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("fireworks"));
    }

    void Signal_OnCancelRelease(UIButton button)
    {
        if (closed)
            return;

#if !UNITY_WEBPLAYER
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
        closed = true;
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
            Manager<UIManager>.Get().PushPopup("SaveMePopup");
        }
        else
        {
            OnTheRunUITransitionManager.Instance.ClosePopup();
        }
#else
        OnTheRunUITransitionManager.Instance.ClosePopup();
#endif
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
        if (Manager<UIManager>.Get().disableInputs)
            return;

        UIButton blockButton = null;
        if (transform.FindChild("BlockButton") != null)
            blockButton = transform.FindChild("BlockButton").GetComponent<UIButton>();

        UIButton cancelButton = null;
        if (transform.FindChild("content/CancelButton") != null)
            cancelButton = transform.FindChild("content/CancelButton").GetComponent<UIButton>();

        UIButton okButton = null;
        if (transform.FindChild("content/OkButton") != null)
            okButton = transform.FindChild("content/OkButton").GetComponent<UIButton>();

        if (blockButton != null && blockButton.State != UIButton.StateType.Disabled)
            blockButton.onReleaseEvent.Invoke(blockButton);
        else if (cancelButton != null && cancelButton.State != UIButton.StateType.Disabled)
            cancelButton.onReleaseEvent.Invoke(cancelButton);
        else if (okButton != null && okButton.State != UIButton.StateType.Disabled)
            okButton.onReleaseEvent.Invoke(okButton);
    }

    void Update()
    {

#if UNITY_WEBPLAYER
        if (Input.GetKeyDown(KeyCode.Space) && !closed)
        {
            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
            OnTheRunUITransitionManager.Instance.ClosePopup();
            closed = true;
        }
#endif
    }
}