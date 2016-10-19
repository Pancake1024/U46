using UnityEngine;
using SBS.Core;
using SBS.Miniclip;
using System;
using System.Collections;

public class OnTheRunCoinsService : Manager<OnTheRunCoinsService>
{
    #region Singleton instance
    protected static OnTheRunCoinsService instance = null;

    public static OnTheRunCoinsService Instance
    {
        get
        {
            return Manager<OnTheRunCoinsService>.Get();
        }
    }
    #endregion

    const bool LOGS_ARE_ENABLED = false;

    public const int EDITOR_FREE_COINS_REWARD = 100;

    public enum WatchVideoPlacement
    {
        MainMenuVideoAdsPlacement,
        IncentivisePopupVideoAdsPlacement,
        NotEnoughCoinsVideoAdsPlacement,
        ShopCoinsVideoAdsPlacement,
        SaveMeAdsPlacement
    }

    public enum VideoType
    {
        FreeCoins,
        SaveMe,
        Booster,
        FreeFuel
    }

	Action<bool> coinsRewardCallback;

    bool videoAdsAreEnabled;
    int coinsRewardPerVideo;
    int maxVideosPerDay;
    int maxBoosterVideosPerDay;
    int maxFreeFuelVideosPerDay;
    int maxFreeSaveMeVideosPerDay;

    bool Videos_ReachedMaxPerDay { get { return OnTheRunDaysCounterForAdvertising.Instance.VideosShownToday >= maxVideosPerDay; } }
    bool BoosterVideos_ReachedMaxPerDay { get { return OnTheRunDaysCounterForAdvertising.Instance.BoosterVideosShownToday >= maxBoosterVideosPerDay; } }
    bool FreeFuelVideos_ReachedMaxPerDay { get { return OnTheRunDaysCounterForAdvertising.Instance.FreeFuelVideosShownToday >= maxFreeFuelVideosPerDay; } }
    bool FreeSaveMeVideos_ReachedMaxPerDay { get { return OnTheRunDaysCounterForAdvertising.Instance.FreeSaveMeVideosShownToday >= maxFreeSaveMeVideosPerDay; } }
    
    int TodaysRemainingVideos { get { return maxVideosPerDay - OnTheRunDaysCounterForAdvertising.Instance.VideosShownToday; } }

    public int FreeCoinsReward { get { return coinsRewardPerVideo; } }

    #region Initialization Methods

    new void Awake()
	{
        base.Awake();
        DontDestroyOnLoad(gameObject);
		
		Init();
	}
    	
	void Init()
	{
        Log("Init");

#if UNITY_IPHONE
#    if MC_COINS_SERVICE
		CoinsServiceBindings.Start();
		ObjCDispatcher.Instance.RegisterEvent("csReward", onCoinsServiceReward);
		ObjCDispatcher.Instance.RegisterEvent("csCloseNoReward", onCoinsServiceCloseWithoutReward);
		ObjCDispatcher.Instance.RegisterEvent("csDisableSound", (str) => { AudioListener.volume = 0.0f; });
		ObjCDispatcher.Instance.RegisterEvent("csEnableSound", (str) => { AudioListener.volume = 1.0f; });
#   endif
#elif UNITY_ANDROID
        gameObject.AddComponent<AndroidVideoAdsManager>();
#endif
	}

    public void SetConfigParameters(bool _videoAdsAreEnabled, int _coinsRewardPerVideo, int _maxVideosPerDay, int _maxBoosterVideosPerDay, int _maxFreeFuelVideosPerDay, int _maxFreeSaveMeVideosPerDay)
    {
        Log("SetConfigParameters - videosEnabled: " + _videoAdsAreEnabled + " - coinsReward: " + _coinsRewardPerVideo + " - maxVideos/Day: " + _maxVideosPerDay + " - maxBooster/Day: " + _maxBoosterVideosPerDay + " - maxFreeFuel/Day: " + _maxFreeFuelVideosPerDay + " - maxFreeSaveMe/Day: " + _maxFreeSaveMeVideosPerDay);

        this.videoAdsAreEnabled = _videoAdsAreEnabled;
        this.coinsRewardPerVideo = _coinsRewardPerVideo;
        this.maxVideosPerDay = _maxVideosPerDay;
        this.maxBoosterVideosPerDay = _maxBoosterVideosPerDay;
        this.maxFreeFuelVideosPerDay = _maxFreeFuelVideosPerDay;
        this.maxFreeSaveMeVideosPerDay = _maxFreeSaveMeVideosPerDay;
    }

    public void OnTranslationsReady()
    {
        Log("Translations Ready");

        //LOCALIZATION
        
#if UNITY_IPHONE && MC_COINS_SERVICE
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.TitleCongratulations,            ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_congrat")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.TitleSorry,                      ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_sorry")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.MessageWatchMore,                ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_another_video")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.MessageCurrentlyNoMoreAvailable, ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_not_available")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.MessageCurrentlyNotAvailable,    ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_not_available")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.ButtonLater,                     ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_later")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.ButtonWatchNow,                  ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_watch_now")));
        CoinsServiceBindings.SetDialogLocalizedText(CoinsServiceBindings.SystemDialogTextType.ButtonOk,                        ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_ok")));
#endif
    }

    public string ReplaceCoinsSymbolWithValue(string str)
    {
        const string coins_symbol = "$coins$";
        return str.Replace(coins_symbol, coinsRewardPerVideo.ToString());
    }

    #endregion

    #region Video Request Public Methods

    public void OnFreeCoinsSelected()
    {
        Log("OnFreeCoinsSelected");

        coinsRewardCallback = FreeCoinsRewardCallback;

#if UNITY_EDITOR
        onCoinsServiceReward(string.Empty);
#elif UNITY_IPHONE
#    if MC_COINS_SERVICE
		CoinsServiceBindings.Show_withDialogAtEnd(TodaysRemainingVideos);
#    else
		OnTheRunFyberManager.Instance.LaunchRewardedVideo(coinsRewardCallback);
#    endif
#elif UNITY_ANDROID
        AndroidVideoAdsManager.Instance.RequestVideoAd(() => { FreeCoinsRewardCallback(true); });
#endif
    }

    public void WatchVideoForCallback(Action<bool> callback)
    {
        Log("OnWatchVideoForCallback");

        coinsRewardCallback = callback;

#if UNITY_EDITOR
        onCoinsServiceReward(string.Empty);
#elif UNITY_IPHONE
#    if MC_COINS_SERVICE
        CoinsServiceBindings.Show_noDialogAtEnd();
#    else
		OnTheRunFyberManager.Instance.LaunchRewardedVideo(callback);
#    endif
#elif UNITY_ANDROID
        AndroidVideoAdsManager.Instance.RequestVideoAd(callback);
#endif
    }

    #endregion

    #region Reward Callback Methods

    void onCoinsServiceReward(string param)
    {
        Log("onCoinsServiceReward - param: " + param);

        if (coinsRewardCallback != null)
            coinsRewardCallback(true);
    }

    void onCoinsServiceCloseWithoutReward(string param)
    {
        Log("onCoinsServiceCloseWithoutReward - param: " + param);

        if (coinsRewardCallback != null)
            coinsRewardCallback(false);
    }

    void FreeCoinsRewardCallback(bool success)
    {
        Log("FreeCoinsRewardCallback - success: " + success);

        if (success)
        {
            PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.FirstCurrency, coinsRewardPerVideo);
            Manager<UIRoot>.Get().UpdateCurrenciesItem();

#if UNITY_ANDROID
            AndroidVideoAdsManager.Instance.NotifyCoinsReward();
#endif
            OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(VideoType.FreeCoins);
        }

#if UNITY_IPHONE
        TriggerFreeCoinsAlertDialog(success);
#endif
    }

    #endregion

    #region iOS Free Coins Feedback Alert
    
#if UNITY_IPHONE

    string freeCoinsDialog_congrats = string.Empty;
    string freeCoinsDialog_sorry = string.Empty;

    string freeCoinsDialog_watchMore = string.Empty;
    string freeCoinsDialog_noMoreAvailable = string.Empty;

    string freeCoinsDialog_buttonLater = string.Empty;
    string freeCoinsDialog_buttonWatchNow = string.Empty;
    string freeCoinsDialog_buttonOk = string.Empty;

    void TriggerFreeCoinsAlertDialog(bool success)
    {
        FreeCoinsDialogLazySetup();

        if (success)
            StartCoroutine(DelayedFreeCoinsAlertDialog());
    }

    IEnumerator DelayedFreeCoinsAlertDialog()
    {
        const float MAX_SECONDS_DELAY = 5.0f;

        Manager<UIRoot>.Get().ShowLoadingPopup();

		yield return new WaitForEndOfFrame();
		OnTheRunFyberManager.Instance.CacheRewardedVideo();

        float timeBefore = TimeManager.Instance.MasterSource.TotalTime;
        
		//Debug.Log ("--- --- --- ------------- DelayedFreeCoinsAlertDialog - start");

        while (TimeManager.Instance.MasterSource.TotalTime - timeBefore < MAX_SECONDS_DELAY)
		{
			//Debug.Log ("--- --- --- ------------- DelayedFreeCoinsAlertDialog - waiting - elapsed: " + (TimeManager.Instance.MasterSource.TotalTime - timeBefore).ToString());
            if (OnTheRunFyberManager.Instance.VideoRequestResponseIsReceived)
			{
				//Debug.Log ("--- --- --- ------------- DelayedFreeCoinsAlertDialog - VideoRequestResponseIsReceived - stopping coroutine");
                ShowFreeCoinsAlertView();
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
		
		//Debug.Log ("--- --- --- ------------- DelayedFreeCoinsAlertDialog - time is up - calling ShowFreeCoinsAlertView");
        ShowFreeCoinsAlertView();
    }

    void ShowFreeCoinsAlertView()
	{
		//Debug.Log ("--- --- --- ------------- ShowFreeCoinsAlertView - IsVideoAdAvailable: " + IsVideoAdAvailable());
		//Debug.Log ("--- --- --- ------------- ShowFreeCoinsAlertView - flag details - videoAdsAreEnabled: " + videoAdsAreEnabled + " - AreServiceVideoAdsAvailable: " + AreServiceVideoAdsAvailable () + " -  Videos_ReachedMaxPerDay: " + Videos_ReachedMaxPerDay);
        //ObjCDispatcher.Instance.RegisterEvent("alertButtonClicked", onFreeCoinsAlertDialogClicked);

        Manager<UIRoot>.Get().HideLoadingPopup();

        var uiMan = UIManager.Instance;
        uiMan.PushPopup("VideoFeedbackPopup");
        if (IsVideoAdAvailable())
        {    //SBS.Miniclip.AlertViewBindings.AlertBox(freeCoinsDialog_congrats, freeCoinsDialog_watchMore, freeCoinsDialog_buttonWatchNow, new string[] { freeCoinsDialog_buttonLater });
                
            //uiMan.FrontPopup.GetComponent<UIVideoFeedbackPopup>().SetupPopup(2, ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_congrat")), ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_another_video")));
            uiMan.FrontPopup.GetComponent<UIVideoFeedbackPopup>().SetupPopup(2, freeCoinsDialog_congrats, freeCoinsDialog_watchMore);
        }
        else
        {
            //SBS.Miniclip.AlertViewBindings.AlertBox(freeCoinsDialog_congrats, freeCoinsDialog_noMoreAvailable, freeCoinsDialog_buttonOk);
            //uiMan.FrontPopup.GetComponent<UIVideoFeedbackPopup>().SetupPopup(1, ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_sorry")), ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_sorry")));
            uiMan.FrontPopup.GetComponent<UIVideoFeedbackPopup>().SetupPopup(1, freeCoinsDialog_congrats, freeCoinsDialog_noMoreAvailable);
        }
    }

    void FreeCoinsDialogLazySetup()
    {
        if (string.IsNullOrEmpty(freeCoinsDialog_congrats))
            freeCoinsDialog_congrats = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_congrat"));

        if (string.IsNullOrEmpty(freeCoinsDialog_sorry))
            freeCoinsDialog_sorry = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_sorry"));

        if (string.IsNullOrEmpty(freeCoinsDialog_watchMore))
            freeCoinsDialog_watchMore = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_another_video"));

        if (string.IsNullOrEmpty(freeCoinsDialog_noMoreAvailable))
            freeCoinsDialog_noMoreAvailable = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_not_available"));

        if (string.IsNullOrEmpty(freeCoinsDialog_buttonLater))
            freeCoinsDialog_buttonLater = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_later"));

        if (string.IsNullOrEmpty(freeCoinsDialog_buttonWatchNow))
            freeCoinsDialog_buttonWatchNow = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_watch_now"));

        if (string.IsNullOrEmpty(freeCoinsDialog_buttonOk))
            freeCoinsDialog_buttonOk = ReplaceCoinsSymbolWithValue(OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_ok"));
    }

    /*void onFreeCoinsAlertDialogClicked(string pressedButton)
    {
        ObjCDispatcher.Instance.RemoveEvent("alertButtonClicked");

        if (pressedButton.Equals(freeCoinsDialog_buttonWatchNow))
            StartCoroutine(DelayedWatchOneMoreVideo());
    }

	IEnumerator DelayedWatchOneMoreVideo()
	{
		for (int i = 0; i < 3; i++)
			yield return new WaitForEndOfFrame();
		
		OnTheRunFyberManager.Instance.LaunchRewardedVideo(FreeCoinsRewardCallback);
	}*/
#endif
    #endregion

    #region Videos Availability Check Methods

    bool AreServiceVideoAdsAvailable()
    {
#if UNITY_WP8 || UNITY_WEBPLAYER || UNITY_WEBGL
        return false;
#elif UNITY_EDITOR
        return true;
/*
#    if MC_COINS_SERVICE
        return true;
#   else
        return OnTheRunFyberManager.Instance.VideoIsReady;
#   endif
*/
#elif UNITY_IPHONE
#    if MC_COINS_SERVICE
		return CoinsServiceBindings.AreVideoAdsAvailable();
#   else
        return OnTheRunFyberManager.Instance.VideoIsReady;
#   endif
#elif UNITY_ANDROID
        AndroidVideoAdsManager.Instance.AreVideoAdsAvailable();
#endif
        return false;
	}

    public bool IsVideoAdAvailable()
    {
        return videoAdsAreEnabled && AreServiceVideoAdsAvailable() && !Videos_ReachedMaxPerDay;
    }

    public bool IsBoosterVideoAvailable()
    {
        return IsVideoAdAvailable() && !BoosterVideos_ReachedMaxPerDay;
    }

    public bool IsFreeFuelVideoAvailable()
    {
        return IsVideoAdAvailable() && !FreeFuelVideos_ReachedMaxPerDay;
    }

    public bool IsFreeSaveMeVideoAvailable()
    {
        return IsVideoAdAvailable() && !FreeSaveMeVideos_ReachedMaxPerDay;
    }
    #endregion

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### Coins Service Manager - " + logStr);
    }
}