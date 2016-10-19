using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("OnTheRun/OnTheRunFBLoginPopupManager")]
public class OnTheRunFBLoginPopupManager : Manager<OnTheRunFBLoginPopupManager>
{
    public static bool SHOW_WITH_DAILY_BONUS = false;

    #region Singleton instance
    public static OnTheRunFBLoginPopupManager Instance
    {
        get
        {
            return Manager<OnTheRunFBLoginPopupManager>.Get();
        }
    }
    #endregion

    [HideInInspector]
    public bool justEnteredInGarage = false;

    public int initialCounter = 1;
    public int deltaCounter = 1;
    public int maxDisplayCount = 3;

    protected bool initialized = false;
    protected UIManager uiManager;
    protected int counterThreshold;
    protected int firstTimeShowed = 1;
    protected int rewardPopupShowed = 1;
    protected const string kShowFBPopupKey = "fbp_sk";
    protected const string kShowFBPopupCounter = "fbp_c";
    protected const string kShowFBFirstTime = "fbp_ft";
    protected const string kShowFBRewardShowed = "fbp_rs";
    protected const string kTotalShownCounter = "fbp_tsc";
    protected const string kFirstTimeEnteredKey = "ftek_g";

    public void CheckFBPopupCanShow(bool gameJustLoaded)
    {
        if (!justEnteredInGarage)
            return;

        //initialization
        if (!initialized)
        {
            initialized = true;
            initialCounter = OnTheRunDataLoader.Instance.GetFacebookPopupData()[0];
            deltaCounter = OnTheRunDataLoader.Instance.GetFacebookPopupData()[1];
            maxDisplayCount = OnTheRunDataLoader.Instance.GetFacebookPopupData()[2];
        }


        bool canBeShown = true; // gameJustLoaded ? (!OnTheRunDailyBonusManager.Instance.DailyPopupHasToShow && !OnTheRunDailyBonusManager.Instance.DailyBonusSequencePopupHasToShow) : SHOW_WITH_DAILY_BONUS;
        bool rightMoment = false;

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            canBeShown = false;
            EncryptedPlayerPrefs.SetInt(kShowFBPopupCounter, initialCounter);
            EncryptedPlayerPrefs.SetInt(kShowFBPopupKey, 0);

            if (UIFacebookLoggedPopup.CanBeShown(false)) //true))
                OnTheRunUITransitionManager.Instance.OpenPopup("FacebookLoggedPopup");
        }
        else
        {
            if (EncryptedPlayerPrefs.GetInt(kFirstTimeEnteredKey, 0) == 0)
                rightMoment = true;
            else
            {
                counterThreshold = EncryptedPlayerPrefs.GetInt(kShowFBPopupCounter, initialCounter);
                int currCounter = EncryptedPlayerPrefs.GetInt(kShowFBPopupKey, 0);
                int totalShownCounter = EncryptedPlayerPrefs.GetInt(kTotalShownCounter, 0);
                int maxTimesThreshold = (initialCounter + (maxDisplayCount - 1) * deltaCounter);
                rightMoment = currCounter >= counterThreshold && totalShownCounter < maxDisplayCount;//counterThreshold <= maxTimesThreshold;

                if (rightMoment && canBeShown)
                {
                    EncryptedPlayerPrefs.SetInt(kShowFBPopupCounter, deltaCounter - 1);// + deltaCounter);
                    EncryptedPlayerPrefs.SetInt(kShowFBPopupKey, 0);
                    EncryptedPlayerPrefs.SetInt(kTotalShownCounter, totalShownCounter + 1);
                }
                else
                    EncryptedPlayerPrefs.SetInt(kShowFBPopupKey, currCounter + 1);
            }
        }

        if (uiManager==null)
            uiManager = Manager<UIManager>.Get();

        if (canBeShown ? rightMoment : false)
        {
            ShowFBPopup();
        }
    }

    void ShowFBPopup( )
    {
        if (EncryptedPlayerPrefs.GetInt(kFirstTimeEnteredKey, 0) == 0)
        {
            OnTheRunDailyBonusManager.Instance.SendMessage("SetupDailyBonus", true);
            EncryptedPlayerPrefs.SetInt(kFirstTimeEnteredKey, 1);
        }
        else
        {
            firstTimeShowed = EncryptedPlayerPrefs.GetInt(kShowFBFirstTime, 0);
            rewardPopupShowed = EncryptedPlayerPrefs.GetInt(kShowFBRewardShowed, 0);
            int showRewardAfter = OnTheRunDataLoader.Instance.GetFacebookRewardAppearAfter();
            if (firstTimeShowed < showRewardAfter || rewardPopupShowed == 1)
                OnTheRunUITransitionManager.Instance.OpenPopup("FacebookLoginPopup");
            else
                Manager<UIRoot>.Get().ShowFBLoginSequence(SuccessCallback);

            EncryptedPlayerPrefs.SetInt(kShowFBFirstTime, firstTimeShowed + 1);
        }
    }

    void SuccessCallback(bool success)
    {
        Manager<UIRoot>.Get().HideLoadingPopup();

        if (success)
        {
        }
    }

    void OnFBopupClosed(UIButton button)
    {
        OnTheRunUITransitionManager.Instance.ClosePopup();
        button.onReleaseEvent.RemoveTarget(gameObject);
    }
}