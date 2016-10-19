using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIWheelFeedbackPopup")]
public class UIWheelFeedbackPopup: MonoBehaviour
{
    public SpriteRenderer icon;
    public UITextField numText;
    public UITextField youWinText;
    public UITextField greatText;
    public UITextField collectButtonText;
    public UITextField shareButtonText;
    public GameObject FBIcon;
    public GameObject FBIconGhost;
	
	UIButton shareButton;
	
	void Awake()
	{
		shareButton = transform.FindChild("content/ShareButton").GetComponent<UIButton>();
	}

    void OnEnable()
    {
        greatText.text = OnTheRunDataLoader.Instance.GetLocaleString("great_spin");
        youWinText.text = OnTheRunDataLoader.Instance.GetLocaleString("you_win");
        collectButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("collect");
        shareButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("share");
        collectButtonText.ApplyParameters();
        shareButtonText.ApplyParameters();

        //shareButton.State = OnTheRunFacebookManager.Instance.IsLoggedIn ? UIButton.StateType.Normal : UIButton.StateType.Disabled;
        shareButton.State = UIButton.StateType.Normal;
        FBIcon.SetActive(true);
        FBIconGhost.SetActive(false);

        if (FBIcon != null)
        {
            float xPos = shareButton.transform.position.x - shareButton.GetComponent<BoxCollider2D>().size.x * 0.40f;
            Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
            FBIcon.transform.position = newPos;
            FBIconGhost.transform.position = newPos;
        }
    }

    public void SetText(string titleText, string descrText)
    {
        greatText.text = titleText;
        youWinText.text = descrText;
    }

    void Signal_OnOkRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();
    }

    void Signal_OnShareRelease(UIButton button)
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
        string feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_share_wheel");
        Debug.Log("SHARE ON FACEBOOK PRESSED - " + feedMessage);
        OnTheRunFacebookManager.Instance.Feed(feedMessage, success =>
		{
			shareButton.State = UIButton.StateType.Disabled;
            FBIcon.SetActive(false);
            FBIconGhost.SetActive(true);

			if (success)
				OnTheRunOmniataManager.Instance.TrackFacebookShare(OmniataIds.FacebookShare_SpinPrize);
		});
    }

    void OnBackButtonAction()
    {
        UIButton collectButton = transform.FindChild("content/CollectButton").GetComponent<UIButton>();
        collectButton.onReleaseEvent.Invoke(collectButton);
    }
}