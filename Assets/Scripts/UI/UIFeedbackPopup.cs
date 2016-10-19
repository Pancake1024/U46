using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIFeedbackPopup")]
public class UIFeedbackPopup : MonoBehaviour
{
    public GameObject facebookIcon;
    public GameObject shareIcon;
    public UITextField descriptionText;
    public UITextField titleText;
    public UITextField okButtonText;
	
	UIButton shareButton;
	
	void Awake()
	{
		shareButton = transform.FindChild("content/ShareButton").GetComponent<UIButton>();
	}

    public void SetPopupText(string title, string descr)
    {
        titleText.text = title;
        descriptionText.text = descr;
    }

    public void SetButtonIcons(bool shareActive, bool facebookActive)
    {
        shareIcon.SetActive(shareActive);
        facebookIcon.SetActive(facebookActive);
    }
    
    void Signal_OnResumeButtonRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();
    }

    void Signal_OnShareButtonRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
            SendFBMessage();
        else
            Manager<UIRoot>.Get().ShowFBLoginSequence(SuccessCallback);

    }

    void SuccessCallback(bool success)
    {
        Manager<UIRoot>.Get().HideLoadingPopup();

        if (success)
            SendFBMessage();
    }

    void SendFBMessage()
    {
        Debug.Log("SHARE ON FACEBOOK PRESSED");
        //Manager<UIManager>.Get().PopPopup();

        // This sould be unused
        string feedMessage = string.Empty;
        OnTheRunFacebookManager.Instance.Feed(feedMessage, success => { shareButton.State = UIButton.StateType.Disabled; });
    }

    void OnEnable()
    {
        if(okButtonText != null)
            okButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");

        //shareButton.State = OnTheRunFacebookManager.Instance.IsLoggedIn ? UIButton.StateType.Normal : UIButton.StateType.Disabled;
        shareButton.State = UIButton.StateType.Normal;

    }

    void OnBackButtonAction()
    {
        UIButton okButton = transform.FindChild("content/ResumeButton").GetComponent<UIButton>();
        okButton.onReleaseEvent.Invoke(okButton);
        
        //OnTheRunUITransitionManager.Instance.SendMessage("OnBuyPopupClosed", okButton);
    }
}