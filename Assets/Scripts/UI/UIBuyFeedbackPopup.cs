using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIBuyFeedbackPopup")]
public class UIBuyFeedbackPopup : MonoBehaviour
{
    public enum ItemBought
    {
        Car,
        ParkingLot,
        SpecialUnlock,
        Other
    }

    public UITextField descriptionText;
    public UITextField titleText;
    public UITextField okButtonText;
    public UITextField shareButtonText;
    public GameObject FBIcon;
    public GameObject FBIconGhost;

    GameObject okButton;
	UIButton shareButton;
    ItemBought itemBougth;
    string feedItemName;
    float defaultOkButtonXPos;

	void Awake()
	{
        okButton = transform.FindChild("content/ResumeButton").gameObject;
        shareButton = transform.FindChild("content/ShareButton").GetComponent<UIButton>();
        shareButtonText.onResize.AddTarget(gameObject, "OnResizeDone" );
        defaultOkButtonXPos = okButton.transform.localPosition.x;
	}

    public void SetPopupText(string title, string descr)
    {
        titleText.text = title;
        descriptionText.text = descr;
    }

    public void SetBoughtItem(ItemBought item)
    {
        itemBougth = item;
    }

    public void SetShareButtonVisibility(bool visible)
    {
        shareButton.gameObject.SetActive(visible);
        FBIcon.SetActive(visible);

        if(!visible)
            okButton.transform.localPosition = new Vector3(0.0f, okButton.transform.localPosition.y, okButton.transform.localPosition.z);
        else
            okButton.transform.localPosition = new Vector3(defaultOkButtonXPos, okButton.transform.localPosition.y, okButton.transform.localPosition.z);
    }

    public void SetFeedItemName(string itemName)
    {
        feedItemName = itemName;
    }

    void Signal_OnResumeButtonRelease(UIButton button)
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
        string feedMessage = string.Empty;
        switch (itemBougth)
        {
            case ItemBought.Car: feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_buy_car"); break;
            case ItemBought.ParkingLot: feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_buy_tier") + feedItemName + "!"; break;
            case ItemBought.SpecialUnlock: feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_unlock_special") + feedItemName + "!"; break;
        }

        Debug.Log("SHARE ON FACEBOOK PRESSED - " + feedMessage);

        OnTheRunFacebookManager.Instance.Feed(feedMessage, success =>
		{
			shareButton.State = UIButton.StateType.Disabled;
            FBIcon.SetActive(false);
            FBIconGhost.SetActive(true);

			if (success)
			{
				string shareType = string.Empty;
				switch (itemBougth)
				{
					case ItemBought.Car: shareType = OmniataIds.FacebookShare_Car; break;
					case ItemBought.ParkingLot: shareType = OmniataIds.FacebookShare_ParkingLot; break;
					case ItemBought.SpecialUnlock: shareType = OmniataIds.FacebookShare_SpecialVehicle; break;
				}
				OnTheRunOmniataManager.Instance.TrackFacebookShare(shareType);
			}
		});
    }

    void OnEnable()
    {
        okButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        shareButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("share");

        //shareButton.State = OnTheRunFacebookManager.Instance.IsLoggedIn ? UIButton.StateType.Normal : UIButton.StateType.Disabled;
        shareButton.State = UIButton.StateType.Normal;
        FBIcon.SetActive(true);
        FBIconGhost.SetActive(false);

        /*if (FBIcon != null)
        {
            OnResizeDone();
        }*/

        SetShareButtonVisibility(true);

#if UNITY_WEBPLAYER
        shareButtonText.text = "";
        shareButtonText.ApplyParameters();
        shareButton.gameObject.SetActive(false);
        FBIcon.SetActive(false);
        Transform collectButTr = okButtonText.transform.parent.transform;
        collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);
#endif
    }

    void OnBackButtonAction()
    {
        UIButton okButton = transform.FindChild("content/ResumeButton").GetComponent<UIButton>();
        okButton.onReleaseEvent.Invoke(okButton);
        
        //OnTheRunUITransitionManager.Instance.SendMessage("OnBuyPopupClosed", okButton);
    }

    void OnResizeDone(UITextField tf)
    {
        float xPos = shareButton.transform.position.x - shareButton.GetComponent<BoxCollider2D>().size.x * 0.4f;
        //Debug.Log("xPos: " + xPos + " shareButton.GetComponent<BoxCollider2D>().size.x " + shareButton.GetComponent<BoxCollider2D>().size.x);
        Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
        FBIcon.transform.position = newPos;
        FBIconGhost.transform.position = newPos;
        //Debug.Log("newPos: " + newPos);
    }
}