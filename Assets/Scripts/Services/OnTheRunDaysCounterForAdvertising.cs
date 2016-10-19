using UnityEngine;
using System.Collections;

public class OnTheRunDaysCounterForAdvertising : Manager<OnTheRunDaysCounterForAdvertising>
{
    #region Singleton instance
    public static OnTheRunDaysCounterForAdvertising Instance
    {
        get
        {
            return Manager<OnTheRunDaysCounterForAdvertising>.Get();
        }
    }
    #endregion

    const bool LOGS_ARE_ENABLED = false;

    const string kElapsedDaysAtTodayKey = "elapsed_days_at_today";

    const string kChartboostInterstitialsShownTodayKey = "chartboost_interstitials_shown_today";
    const string kInterstitialsShownTodayKey = "interstitials_shown_today";
    const string kVideosShownTodayKey ="videos_shown_stoday";
    const string kBoosterVideosShownTodayKey = "booster_videos_shown_today";
    const string kFreeFuelVideosShownTodayKey = "freefuel_videos_shown_today";
    const string kFreeSaveMeVideosShownTodayKey = "freesaveme_videos_shown_today";

    int elapsedDaysAtToday;

    int chartboostInterstitialsShownToday;
    int interstitialsShownToday;
    int videosShownToday;
    int boosterVideosShownToday;
    int freeFuelVideosShownToday;
    int freeSaveMeVideosShownToday;

    public int ChartboostInterstitialsShownToday { get { return chartboostInterstitialsShownToday; } }
    public int InterstitialsShownToday { get { return interstitialsShownToday; } }
    public int VideosShownToday { get { return videosShownToday; } }
    public int BoosterVideosShownToday { get { return boosterVideosShownToday; } }
    public int FreeFuelVideosShownToday { get { return freeFuelVideosShownToday; } }
    public int FreeSaveMeVideosShownToday { get { return freeSaveMeVideosShownToday; } }
    
    public void OnChartboostInterstitialShown()
    {
        chartboostInterstitialsShownToday++;
        SaveCounters();
    }

    public void OnInterstitialShown()
    {
        Log("OnInterstitialShown");

        interstitialsShownToday++;
        SaveCounters();
    }

    public void OnVideoAdShown(OnTheRunCoinsService.VideoType videoType)
    {
        Log("OnVideoAdShown - type: " + videoType);
        
        videosShownToday++;

        switch (videoType)
        {
            case OnTheRunCoinsService.VideoType.Booster:
                boosterVideosShownToday++;
                //Debug.LogError("OnVideoAdShown - type: " + videoType + " boosterVideosShownToday " + boosterVideosShownToday);
                break;

            case OnTheRunCoinsService.VideoType.FreeFuel:
                freeFuelVideosShownToday++;
                //Debug.LogError("OnVideoAdShown - type: " + videoType + " freeFuelVideosShownToday " + freeFuelVideosShownToday);
                break;

            case OnTheRunCoinsService.VideoType.SaveMe:
                freeSaveMeVideosShownToday++;
                //Debug.LogError("OnVideoAdShown - type: " + videoType + " freeSaveMeVideosShownToday " + freeSaveMeVideosShownToday);
                break;

            case OnTheRunCoinsService.VideoType.FreeCoins:
                break;
        }

        SaveCounters();
    }

    /*public void OnVideoAdShown()
    {
        videosShownToday++;
        SaveCounters();
    }

    public void OnBoosterVideoShown()
    {
        videosShownToday++;
        boosterVideosShownToday++;
        SaveCounters();
    }

    public void OnFreeFuelVideoShown()
    {
        videosShownToday++;
        freeFuelVideosShownToday++;
        SaveCounters();
    }*/

    new void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        LoadCounters();
    }

    void AdvertisingDaysCounterSessionBegin()
    {
        LoadLastElapsedDaysAtToday();

        if (IsNewDay())
        {
            Log("AdvertisingDaysCounterSessionBegin - New Day!");
            UpdateAndSaveLastElapsedDaysAtToday();
            ResetDaysCounters();
        }
    }

    void ResetDaysCounters()
    {
        chartboostInterstitialsShownToday = 0;
        interstitialsShownToday = 0;
        videosShownToday = 0;
        boosterVideosShownToday = 0;
        freeFuelVideosShownToday = 0;
        freeSaveMeVideosShownToday = 0;

        SaveCounters();
    }

    bool IsNewDay()
    {
        int days;
        OnTheRunSessionsManager.Instance.GetDaysPassedSinceFirstSession(out days);

        return (days > elapsedDaysAtToday);
    }

    void LoadLastElapsedDaysAtToday()
    {
        elapsedDaysAtToday = EncryptedPlayerPrefs.GetInt(kElapsedDaysAtTodayKey, -1);
    }

    void UpdateAndSaveLastElapsedDaysAtToday()
    {
        int days;
        OnTheRunSessionsManager.Instance.GetDaysPassedSinceFirstSession(out days);

        elapsedDaysAtToday = days;
        EncryptedPlayerPrefs.SetInt(kElapsedDaysAtTodayKey, elapsedDaysAtToday);

        EncryptedPlayerPrefs.Save();

        Log("Days Elapsed At Today: " + elapsedDaysAtToday);
    }

    void LoadCounters()
    {
        chartboostInterstitialsShownToday = EncryptedPlayerPrefs.GetInt(kChartboostInterstitialsShownTodayKey, 0);
        interstitialsShownToday = EncryptedPlayerPrefs.GetInt(kInterstitialsShownTodayKey, 0);
        videosShownToday = EncryptedPlayerPrefs.GetInt(kVideosShownTodayKey, 0);
        boosterVideosShownToday = EncryptedPlayerPrefs.GetInt(kBoosterVideosShownTodayKey, 0);
        freeFuelVideosShownToday = EncryptedPlayerPrefs.GetInt(kFreeFuelVideosShownTodayKey, 0);
        freeSaveMeVideosShownToday = EncryptedPlayerPrefs.GetInt(kFreeSaveMeVideosShownTodayKey, 0);

        Log("Load Counters - interstitials: " + interstitialsShownToday + " - videos: " + videosShownToday + " - boosterVideos: " + boosterVideosShownToday + " - freeFuelVideos: " + freeFuelVideosShownToday + " - freeSaveMeVideos: " + freeSaveMeVideosShownToday);
    }

    void SaveCounters()
    {
        EncryptedPlayerPrefs.SetInt(kChartboostInterstitialsShownTodayKey, chartboostInterstitialsShownToday);
        EncryptedPlayerPrefs.SetInt(kInterstitialsShownTodayKey, interstitialsShownToday);
        EncryptedPlayerPrefs.SetInt(kVideosShownTodayKey, videosShownToday);
        EncryptedPlayerPrefs.SetInt(kBoosterVideosShownTodayKey, boosterVideosShownToday);
        EncryptedPlayerPrefs.SetInt(kFreeFuelVideosShownTodayKey, freeFuelVideosShownToday);
        EncryptedPlayerPrefs.SetInt(kFreeSaveMeVideosShownTodayKey, freeSaveMeVideosShownToday);

        EncryptedPlayerPrefs.Save();

        Log("Save Counters - interstitials: " + interstitialsShownToday + " - videos: " + videosShownToday + " - boosterVideos: " + boosterVideosShownToday + " - freeFuelVideos: " + freeFuelVideosShownToday + " - freeSaveMeVideos: " + freeSaveMeVideosShownToday);
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### Days Counter For Ads - " + logStr);
    }
}