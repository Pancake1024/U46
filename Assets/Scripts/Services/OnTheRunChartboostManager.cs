using UnityEngine;
using System.Collections;
using SBS.Core;
using System;
using System.Collections.Generic;

public class OnTheRunChartboostManager : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunChartboostManager instance = null;

    public static OnTheRunChartboostManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    [Flags]
    public enum TriggerPoint
    {
        GameLaunch = (1 << 0)
    }

    const bool LOGS_ARE_ENABLED = false;
    const string isPurchaser_key = "key_isPurchaser";

    const string DEFAULT_LOCATION = "Default";
    const string TEST_LOCATION_VIDEO = "Test_Video";

    public int numDaysBeforeFirstInterstitial = 7;

    bool ShouldSkipChartboost
    {
        get
        {
#if UNITY_WP8
            return true;
#elif UNITY_IPHONE
			return true;
#else
            return (isPurchaser || !isEnabled || !PassedDaysToWait || ReachedDaysLimit || OnTheRunSessionsManager.Instance.IsFirstSession());
#endif
        }
    }

    bool PassedDaysToWait
    {
        get
        {
            int days;
            OnTheRunSessionsManager.Instance.GetDaysPassedSinceFirstSession(out days);
            return days >= numDaysBeforeFirstInterstitial;
        }
    }

    bool ReachedDaysLimit
    {
        get
        {
            int alreadyShown = OnTheRunDaysCounterForAdvertising.Instance.ChartboostInterstitialsShownToday;
            int limit = OnTheRunDataLoader.Instance.GetChartboostLimitPerDay();

            return alreadyShown >= limit;
        }
    }
    
    bool isPurchaser;
	bool isEnabled = false;
    bool interstitialReadyButNotShownYet;
    bool interstitialHasBeenShown;
    List<string> pagesNoInterstitialPermitted;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);

        isPurchaser = EncryptedPlayerPrefs.GetInt(isPurchaser_key, 0) == 1;

        interstitialReadyButNotShownYet = false;
        interstitialHasBeenShown = false;

        SetupPagesNoInterstitialPermitted();
        RegisterCallbackEvent();
    }

    void RegisterCallbackEvent()
    {
#if !UNITY_WP8
        NativeDispatcher.Instance.RegisterEvent("cbDidDisplayInterstitial", (str) =>
        {
            bool showVideo = ("1" == str);
            if (showVideo)
            {
                //chartboostVideoCounter++;
                //EncryptedPlayerPrefs.SetInt(chartboostVideoCounter_key, chartboostVideoCounter);
            }
            else
            {
                //chartboostInterCounter++;
                //EncryptedPlayerPrefs.SetInt(chartboostCounter_key, chartboostInterCounter);

                interstitialHasBeenShown = true;
                if (OnTheRunDaysCounterForAdvertising.Instance != null)
                    OnTheRunDaysCounterForAdvertising.Instance.OnChartboostInterstitialShown();
            }
            //chartboostSessionCounter++;
            //EncryptedPlayerPrefs.Save();
        });
#endif
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    
    public void OnInappDone()
    {
        Log("On Inapp Done");

        isPurchaser = true;
        EncryptedPlayerPrefs.SetInt(isPurchaser_key, 1);
        EncryptedPlayerPrefs.Save();
    }

	public void SetConfigParameters(bool enable)
	{
		Log ("SetConfigParameters - enable: " + enable);

		if(!isEnabled && enable)
		{
        	if (!interstitialHasBeenShown)
            	this.TriggerInterstitial();
	    }
	}

    void SetupPagesNoInterstitialPermitted()
    {
        pagesNoInterstitialPermitted = new List<string>();
        pagesNoInterstitialPermitted.Add("IngamePage");
		pagesNoInterstitialPermitted.Add("RewardPage");
		pagesNoInterstitialPermitted.Add("InAppPage");
    }

    void ChooseTriggerPoint(bool cache)
    {
        /*if (0 == enabledTriggerPoints)
        {
            interstitialTriggerPoint = 0;
            return;
        }

        int numTriggerPoints = triggerPointNames.Length;
        int randomIndex = UnityEngine.Random.Range(0, numTriggerPoints), counter = 0;
        while (0 == ((TriggerPoint)(1 << randomIndex) & enabledTriggerPoints) && counter++ < numTriggerPoints)
            randomIndex = (randomIndex + 1) % numTriggerPoints;

        interstitialTriggerPoint = (TriggerPoint)(1 << randomIndex);

        if (cache && this.InterstitialCanBePresented)
        {
            bool showVideo = false;
            if (videoFirst)
                showVideo = (chartboostVideoCounter < videoLimitPerDay);
            else
                showVideo = (chartboostInterCounter >= interLimitPerDay);

            ChartboostBinding.CacheInterstitial(interstitialTriggerPoint + (showVideo ? "_video" : string.Empty));
        }*/
    }

    void ChartboostSessionBegin()
	{
        Log("ChartboostSessionBegin - should skip: " + ShouldSkipChartboost);

        if (ShouldSkipChartboost)
            return;
        
        if (!interstitialHasBeenShown)
            this.TriggerInterstitial();
    }

    void TriggerInterstitial()
    {
        Log("TriggerInterstitial");

        if (PassedDaysToWait)
        {
            if (DoesPagePermitInterstitial(Manager<UIManager>.Get().ActivePageName))
                ShowInterstitial();
            else
                interstitialReadyButNotShownYet = true;
        }
    }

    void ShowInterstitial()
    {
        Log("ShowInterstitial - should skip: " + ShouldSkipChartboost);

        if (ShouldSkipChartboost)
            return;

        ChartboostBinding.ShowInterstitial(DEFAULT_LOCATION);
    }

    public void OnPageChanged(string currentPageName)
    {
        if (ShouldSkipChartboost)
            return;

		Log("OnPageChanged - parameter: " + currentPageName + " - activePage: " + Manager<UIManager>.Get().ActivePageName);

		if (DoesPagePermitInterstitial(currentPageName))
			UnlockInterstitials();
		else
			BlockInterstitials();

        if (DoesPagePermitInterstitial(currentPageName) && interstitialReadyButNotShownYet)
        {
            interstitialReadyButNotShownYet = false;
            ShowInterstitial();
        }
    }

	void BlockInterstitials()
	{
		Log("Block Interstitials");

        ChartboostBinding.LockInterstitials();
    }

	void UnlockInterstitials()
	{
		Log("Unlock Interstitials");

        ChartboostBinding.UnlockInterstitials();
    }

    bool DoesPagePermitInterstitial(string pageName)
    {
        if (pagesNoInterstitialPermitted == null)
            return true;

        bool returnValue = true;
        foreach(var noInterstitialPage in pagesNoInterstitialPermitted)
            if (noInterstitialPage.ToLowerInvariant().Equals(pageName.ToLowerInvariant()))
                returnValue = false;

        Log("DoesPagePermitInterstitial - " + pageName + " - " + returnValue);

        return returnValue;
    }

	void Log(string logStr)
	{
		if (LOGS_ARE_ENABLED)
            Debug.Log ("### ChartboostManager - " + logStr);
    }

    /*void OnGUI()
    {
        float btnWidth = Screen.width / 3.0f;
        float btnHeight = Screen.height / 3.0f;
        if (GUI.Button(new Rect(0.0f, 0.0f, btnWidth, btnHeight), "Chartboost"))
        {
            ChartboostBinding.ShowInterstitial(TEST_LOCATION_VIDEO);
        }
    }*/
}