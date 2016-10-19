using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRankFeedbackPopup")]
public class UIRankFeedbackPopup: MonoBehaviour
{
    public SpriteRenderer icon;
    public UITextField numText;
    public UITextField youWinText;
    public UITextField greatText;
    public UITextField collectButtonText;
    public UITextField shareButtonText;
    public GameObject diamond;
    public GameObject FBIcon;
    public GameObject FBIconGhost;

    public GameObject normalItem;
    public GameObject bonusItem;

    public UITextField bonusYouWinText;
    public UITextField bonusRewardText;
    public SpriteRenderer bonusSprite;

    protected bool pressed = false;
    protected Vector3 backupCollectButtonPos;
    protected bool isBonusAvailable= false;
	
	UIButton shareButton;
	
	void Awake()
	{
		shareButton = transform.FindChild("content/ShareButton").GetComponent<UIButton>();
	}

    void OnEnable()
    {
        if (backupCollectButtonPos == null)
            backupCollectButtonPos = collectButtonText.transform.parent.transform.localPosition;
        collectButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("collect");
        shareButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("share");
        collectButtonText.ApplyParameters();
        shareButtonText.ApplyParameters();

        shareButton.State = UIButton.StateType.Normal;
        FBIcon.SetActive(true);
        FBIconGhost.SetActive(false);

        if (FBIcon != null)
        {
            float xPos = shareButton.transform.position.x - shareButton.GetComponent<BoxCollider2D>().size.x * 0.4f;
            Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
            FBIcon.transform.position = newPos;
            FBIconGhost.transform.position = newPos;
        }

#if UNITY_WEBPLAYER
        /* dani sposto su onShow
        transform.FindChild("content/Icon").gameObject.SetActive(false);
        transform.FindChild("content/popupNumValue").gameObject.SetActive(false);
        transform.FindChild("content/popupTextField").gameObject.SetActive(false);
        //transform.FindChild("content/popupGreatText").transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        collectButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        shareButtonText.text = "";
        shareButtonText.ApplyParameters();
        shareButton.gameObject.SetActive(false);
        FBIcon.SetActive(false);
        FBIconGhost.SetActive(false);
        Transform collectButTr = collectButtonText.transform.parent.transform;
        collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);
        */
#endif
    }

    public void SetText(string titleText, string descrText, string num, bool shareActive=true)
    {
        greatText.text = titleText;
        youWinText.text = descrText;
        numText.text = Manager<UIRoot>.Get().FormatTextNumber(int.Parse(num, CultureInfo.InvariantCulture));
        ActivateSharedButton(shareActive);
    }

    public void ActivateSharedButton(bool value)
    {
#if UNITY_WEBPLAYER
        return;
#endif
        shareButton.gameObject.SetActive(value);
        FBIcon.SetActive(value);
        FBIconGhost.SetActive(false);
        //Transform collectButTr = collectButtonText.transform.parent.transform;

        //if (!value)
        //    collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);
        //else
        //    collectButTr.localPosition = backupCollectButtonPos;

/*#if UNITY_WEBPLAYER
        collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);
#endif*/

    }

    void Signal_OnShow(UIPopup popup)
    {
        pressed = false;

        iTween.Stop(bonusSprite.gameObject);
        bonusSprite.gameObject.transform.localScale = Vector3.one;
        setBonusAlpha(255.0f);
        //iTween.ScaleTo(bonusSprite.gameObject, iTween.Hash("scale", new Vector3(1.0f, 1.0f, 1.0f), "easeType", "easeOutBounce", "time", 0.01f));
        //iTween.ValueTo(bonusSprite.gameObject, iTween.Hash("from", 1.0f, "to", 1.0f, "time", 0.0f, "easetype", "easeOutCirc", "onupdate", "setBonusAlpha", "onupdatetarget", gameObject));

        collectButtonText.transform.parent.gameObject.SetActive(true);

        OnTheRunConsumableBonusManager.ConsumableBonus bonusAvailable = OnTheRunConsumableBonusManager.Instance.IsConsumableBounsAvailable();
        isBonusAvailable = bonusAvailable != null && bonusAvailable.type != OnTheRunConsumableBonusManager.ConsumableType.None && bonusAvailable.type != OnTheRunConsumableBonusManager.ConsumableType.OnlyDiamonds;
        normalItem.SetActive(!isBonusAvailable);
        bonusItem.SetActive(isBonusAvailable);
        
        if (isBonusAvailable)
        {
            bonusYouWinText.text = "";// OnTheRunDataLoader.Instance.GetLocaleString("you_win");
            string txtBonus = "";
            switch (bonusAvailable.type)
            {
                case OnTheRunConsumableBonusManager.ConsumableType.CoinsPack:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("coins_pack");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.GemsPack:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("gems_pack");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.FreeSpinsPack:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("free_spin_pack");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.FuelPack:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("fuel_pack");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.FuelFreeze:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("fuel_freeze");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.Car:
                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("car_unlocked");
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.Other:
                    List<OnTheRunRankManager.Unlockable> unlockableList = OnTheRunRankManager.Instance.UnlockableItems;
                    for (int i = unlockableList.Count - 1; i >= 0; --i)
                    {
                        OnTheRunRankManager.Unlockable currUnlockable = unlockableList[i];
                        if (currUnlockable.rankLevel == PlayerPersistentData.Instance.Level)
                        {
                            switch(currUnlockable.category)
                            {
                                case OnTheRunRankManager.UnlockableType.SpecialVehicle:
                                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("special_vehicles");
                                    break;
                                case OnTheRunRankManager.UnlockableType.Tier:
                                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("tier_unlocked");
                                    break;
                                case OnTheRunRankManager.UnlockableType.Car:
                                    txtBonus = OnTheRunDataLoader.Instance.GetLocaleString("car_unlocked");
                                    break;
                            }
                        }
                    }
                    break;
                case OnTheRunConsumableBonusManager.ConsumableType.None:
                    break;
            }
            bonusRewardText.text = txtBonus;
            bonusSprite.sprite = OnTheRunConsumableBonusManager.Instance.GetConsumableSprite(bonusAvailable.type);
        }

        collectButtonText.ApplyParameters();
        shareButtonText.ApplyParameters();

#if UNITY_WEBPLAYER
        collectButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        Transform collectButTr = collectButtonText.transform.parent.transform;
        collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);

        shareButton.gameObject.SetActive(false);
        FBIcon.SetActive(false);
        FBIconGhost.SetActive(false);

        //TODO feedback text

        /* dani
        transform.FindChild("content/Icon").gameObject.SetActive(false);
        transform.FindChild("content/popupNumValue").gameObject.SetActive(false);
        transform.FindChild("content/popupTextField").gameObject.SetActive(false);
        //transform.FindChild("content/popupGreatText").transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        collectButtonText.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        shareButtonText.text = "";
        shareButtonText.ApplyParameters();
        shareButton.gameObject.SetActive(false);
        FBIcon.SetActive(false);
        Transform collectButTr = collectButtonText.transform.parent.transform;
        collectButTr.localPosition = new Vector3(0.0f, collectButTr.localPosition.y, collectButTr.localPosition.z);
        */
#endif
    }

    void Signal_OnExit(UIPopup popup)
    {
        OnTheRunFireworks.Instance.ClearFireworks();
    }

    public void Signal_OnCollectRelease(UIButton button)
    {
        if (pressed)
            return;

        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        collectButtonText.transform.parent.gameObject.SetActive(false);

        float time = 0.0f;
        float scaleFactor = 1.6f;
        iTween.ScaleTo(bonusSprite.gameObject, iTween.Hash("scale", new Vector3(scaleFactor, scaleFactor, scaleFactor), "easeType", "easeOutBounce", "time", 0.5f));
        iTween.ValueTo(bonusSprite.gameObject, iTween.Hash("from", 1.0f, "to", 0.0f, "time", 1.5f, "easetype", "easeOutCirc", "onupdate", "setBonusAlpha", "onupdatetarget", gameObject));

        if (!isBonusAvailable)
        {
#if !UNITY_WEBPLAYER
            Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(OnTheRunConsumableBonusManager.Instance.GetDiamondsForLevelUp(PlayerPersistentData.Instance.Level), diamond, 5.0f, 0f);
            time = 0.4f;
#endif
        }
        else
        {
            OnTheRunConsumableBonusManager.Instance.TakeConsumableBonusReward();
            time = 0.75f;
        }

        pressed = true;
        this.StartCoroutine(this.CloseDelay(time));
    }

    public void setBonusAlpha(float newAlpha)
    {
        bonusSprite.color = new Color(bonusSprite.color.r, bonusSprite.color.g, bonusSprite.color.b, newAlpha);
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
        string feedMessage = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_share_rank") + PlayerPersistentData.Instance.Level;
        Debug.Log("SHARE ON FACEBOOK PRESSED - " + feedMessage);
        OnTheRunFacebookManager.Instance.Feed(feedMessage, success =>
		{
			shareButton.State = UIButton.StateType.Disabled;
            FBIcon.SetActive(true);
            FBIconGhost.SetActive(false);

			if (success)
				OnTheRunOmniataManager.Instance.TrackFacebookShare(OmniataIds.FacebookShare_LevelUp);
		});
    }

    IEnumerator CloseDelay(float time)
    {
        yield return new WaitForSeconds(time);

        UIManager uiManager = Manager<UIManager>.Get();
        if (uiManager.ActivePageName == "RewardPage")
            uiManager.ActivePage.GetComponent<UIRewardPage>().WillRankUp = false;

        PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, 1.0f);
        if (uiManager.ActivePageName == "GaragePage" && Manager<UIManager>.Get().ActivePage.GetComponent<UIGaragePage>().ShowParkingLotPopup)
        {
            uiManager.PopPopup();
            uiManager.ActivePage.SendMessage("ShowTierUnlockedPopup");
        }
        else if (uiManager.ActivePageName == "GaragePage" && Manager<UIManager>.Get().ActivePage.GetComponent<UIGaragePage>().CanShowAdvicePopup)
        {
            uiManager.PopPopup();
            uiManager.ActivePage.SendMessage("ShowAdvicePopup", SendMessageOptions.DontRequireReceiver);
        }
        else if (uiManager.IsPopupInStack("FBFriendsPopup"))
        {
            OnTheRunUITransitionManager.Instance.ClosePopup(OnTheRunNotificationManager.Instance.CheckForInviteAccepted);
        }
        else
            OnTheRunUITransitionManager.Instance.ClosePopup();

        if (Manager<UIRoot>.Get().advanceToNextPage)
            Manager<UIRoot>.Get().BroadcastMessage("AdvanceToNextPage");

        Manager<UIRoot>.Get().PauseExperienceBarAnimation(false);

        OnTheRunInterstitialsManager.Instance.TriggerInterstitial(OnTheRunInterstitialsManager.TriggerPoint.LevelUp);
    }

    void StartFireworks()
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("fireworks"));
    }

    void OnBackButtonAction()
    {
        UIButton collectButton = transform.FindChild("content/CollectButton").GetComponent<UIButton>();
        collectButton.onReleaseEvent.Invoke(collectButton);
    }
}