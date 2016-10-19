using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIVideoFeedbackPopup")]
public class UIVideoFeedbackPopup : MonoBehaviour
{
    protected GameObject bottomBar;
    public bool goToRewardPageFlag = false;
    public UITextField tfOk;
    public UITextField tfWatch;
    public UITextField tfCancel;
    public UITextField tfTitle;
    public UITextField tfDescr;
    public UITextField tfCoinsAmount;
    public UIButton cancelButton;
    public UIButton watchButton;
    public UIButton okButton;

    OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        //transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
		tfCoinsAmount.text = ""+OnTheRunCoinsService.Instance.FreeCoinsReward;

        gameObject.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    void StartFireworks( )
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(18, transform.FindChild("content/fireworks"));
    }

    void Signal_OnCancelRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        //OnTheRunUITransitionManager.Instance.ClosePopup();
        UIManager.Instance.PopPopup();
    }

    void Signal_OnOkRelease(UIButton button)
    {
    }

    void Signal_OnWatchNowRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        //OnTheRunUITransitionManager.Instance.ClosePopup();
        UIManager.Instance.PopPopup();
        OnTheRunCoinsService.Instance.OnFreeCoinsSelected();
    }

    void Signal_OnShow(UIPopup popup)
    {
        StartFireworks();
        if (null != tfOk && okButton.gameObject.activeInHierarchy)
            tfOk.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        if (null != tfWatch && watchButton.gameObject.activeInHierarchy)
            tfWatch.text = OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_watch_now");
        if (null != tfCancel && cancelButton.gameObject.activeInHierarchy)
            tfCancel.text = OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_later");
    }

    public void SetupPopup(int buttonsNum, string title, string descr)
    {
        okButton.gameObject.SetActive(buttonsNum == 1);
        cancelButton.gameObject.SetActive(buttonsNum != 1);
        watchButton.gameObject.SetActive(buttonsNum != 1);

        tfTitle.text = title;
        tfDescr.text = descr;
    }

    void OnBackButtonAction()
    {
        if (cancelButton != null && cancelButton.gameObject.activeInHierarchy)
            cancelButton.onReleaseEvent.Invoke(cancelButton);
        else if (okButton != null && okButton.gameObject.activeInHierarchy)
            okButton.onReleaseEvent.Invoke(okButton);
        else
            Signal_OnCancelRelease(null);
    }
}