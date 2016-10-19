using UnityEngine;
using System.Collections;
using SBS.Core;
using System;

public class OnTheRunSocial : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunSocial instance = null;

    public static OnTheRunSocial Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    SocialImplementation implementation;
   
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);
        
#if !UNITY_WEBPLAYER
        Init();
#endif
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    void Init()
    {
        SetImplementation();
        implementation.Init();
        implementation.LogIn();
    }

    void SetImplementation()
	{
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorSocialImplementation>();
#elif UNITY_IPHONE
		implementation = gameObject.AddComponent<IosSocialImplementation>();
#elif UNITY_ANDROID && !UNITY_KINDLE
        implementation = gameObject.AddComponent<AndroidSocialImplementation>();
#elif UNITY_ANDROID && UNITY_KINDLE
        implementation = gameObject.AddComponent<KindleUnitySocialImplementation>();
        //implementation = gameObject.AddComponent<KindleSocialImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<EditorSocialImplementation>();
#endif
    }

    void OnApplicationPause(bool paused)
    {
#if !UNITY_WEBPLAYER
        if (!paused)
            implementation.OnApplicationResumed();
#endif
    }

    public void ReportLeaderboardTotalMeters(int meters)
    {
        if (implementation.IsLoggedIn)
            implementation.ReportLeaderboardTotalMeters(meters);
    }

    public void ReportLeaderboardBestMeters(int meters)
    {
        if (implementation.IsLoggedIn)
            implementation.ReportLeaderboardBestMeters(meters);
    }

    public void ReportAchievementDone(string id, float perc)
    {
#if !UNITY_EDITOR
        if (implementation.IsLoggedIn)
#endif
            implementation.ReportAchievementDone(id, perc);
    }

    public string GetAchievementID(OnTheRunAchievements.AchievementType type)
    {
        return implementation.GetAchievementID(type);
    }

    public void ShowLeaderboards()
    {
        if (!implementation.IsLoggedIn)
            implementation.LogIn();
        implementation.ShowLeaderboards();
    }

    public void ShowAchievements()
    {
        if (!implementation.IsLoggedIn)
            implementation.LogIn();
        implementation.ShowAchievements();
    }

    public OnTheRunAchievements.AchievementType GetAchievementType(string achievementId)
    {
        for (int i = 0; i < Enum.GetValues(typeof(OnTheRunAchievements.AchievementType)).Length; i++)
            if (implementation.GetAchievementID((OnTheRunAchievements.AchievementType)i).Equals(achievementId))
                return (OnTheRunAchievements.AchievementType)i;

        Asserts.Assert(false, "Could not find achievement type for id: " + achievementId);
        return (OnTheRunAchievements.AchievementType)(-1);
    }
}