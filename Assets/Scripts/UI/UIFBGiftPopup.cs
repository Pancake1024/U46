using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIFBGiftPopup")]
public class UIFBGiftPopup : MonoBehaviour
{
    public UITextField tfTitle;
    public UITextField tfYellowText;
    public UITextField tfWhiteText;
    public UITextField tfAcceptButtonText;
    public SpriteRenderer icon;
    public UIEnterExitAnimations blackAlphaBG;

    protected string friendId;
    protected bool sendFuelBack = false;

    SpriteRenderer friendPictureRenderer;

    #region Unity Callbacks
    void Awake()
    {
        friendPictureRenderer = transform.FindChild("content/UserGroup/fb_user").GetComponent<SpriteRenderer>();
    }
    #endregion

    void OnEnable()
    {
        if (UIManager.Instance.ActivePage != null && UIManager.Instance.ActivePageName.Equals("StartPage"))
        {
            blackAlphaBG.animations[0].endPoint = 0.5f;
            blackAlphaBG.animations[1].endPoint = 0.5f;
            blackAlphaBG.animations[0].coordinateToMove = "alpha";
            blackAlphaBG.animations[1].coordinateToMove = "alpha";
        }
    }

    void OnDisable()
    {
        blackAlphaBG.animations[0].endPoint = 1.0f;
        blackAlphaBG.animations[1].endPoint = 1.0f;
        blackAlphaBG.animations[0].coordinateToMove = "";
        blackAlphaBG.animations[1].coordinateToMove = "";
    }
    #region Functions
    public void Initialize(string senderId, string friendName, Sprite friendPicture, bool sendFuel)
    {
        sendFuelBack = sendFuel;
        friendId = senderId;
        tfTitle.text = OnTheRunDataLoader.Instance.GetLocaleString("notification_popup_title");
        tfYellowText.text = OnTheRunMcSocialApiData.Instance.TrimStringAtMaxChars(friendName, 20) + " " + OnTheRunDataLoader.Instance.GetLocaleString("notification_popup_text1");

        friendPictureRenderer.sprite = friendPicture;
        McSocialApiManager.ScaleAvatarSpriteRenderer(friendPictureRenderer);

        if (sendFuelBack)
        {
            icon.gameObject.SetActive(true);
            tfWhiteText.text = OnTheRunDataLoader.Instance.GetLocaleString("notification_popup_text2");
            tfAcceptButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("notification_popup_button");
        }
        else
        {
            icon.gameObject.SetActive(true);
            tfWhiteText.text = "";
            tfAcceptButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("notification_popup_button_accept");
        }

        tfWhiteText.ApplyParameters();
        tfAcceptButtonText.ApplyParameters();

        /*if (icon != null)
        {
            float xPos = tfAcceptButtonText.transform.parent.transform.position.x + tfAcceptButtonText.transform.parent.GetComponent<BoxCollider2D>().size.x * 0.5f - icon.bounds.size.x * 0.25f;
            Vector3 newPos = new Vector3(xPos, icon.transform.position.y, icon.transform.position.z);
            icon.transform.position = newPos;
        }*/
    }
    #endregion

    #region Signals

    void Signal_OnAcceptGiftRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        OnTheRunFuelManager.Instance.Fuel++;
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
		OnTheRunOmniataManager.Instance.TrackFuelReceived();
        if (sendFuelBack)
            OnTheRunNotificationManager.Instance.SendFuelGift(friendId);
        OnTheRunNotificationManager.Instance.PopupClosed();
    }

    #endregion
}