using UnityEngine;
using System.Collections;
using SBS.Core;

public class OnTheRunAppsFlyerManager : Manager<OnTheRunAppsFlyerManager>
{
    #region Singleton instance
    protected static OnTheRunAppsFlyerManager instance = null;

    public static OnTheRunAppsFlyerManager Instance
    {
        get
        {
            return Manager<OnTheRunAppsFlyerManager>.Get();
        }
    }
    #endregion

    public string sbsDeveloperKey;
    public string mcDeveloperKey;
    public string sbsAppleAppId;
    public string mcAppleAppId;
    
    AppsFlyerImplementation implementation;
    string developerKey;
    string appleAppId;

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
        SetKeys();
        SetImplementation();
        implementation.Initialize(developerKey, appleAppId);
    }

    void SetKeys()
    {
#if UNITY_EDITOR || (UNITY_ANDROID && UNITY_KINDLE) || UNITY_WP8
        developerKey = sbsDeveloperKey;
        appleAppId = sbsAppleAppId;
#else
        developerKey = mcDeveloperKey;
        appleAppId = mcAppleAppId;
#endif
    }

    void SetImplementation()
    {
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorAppsFlyerImplementation>();
#elif UNITY_IPHONE
        implementation = gameObject.AddComponent<UnitySdkAppsFlyerImplementation>();
#elif UNITY_ANDROID && !DEVELOPMENT_BUILD
        implementation = gameObject.AddComponent<UnitySdkAppsFlyerImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<VoidAppsFlyerImplementation>();
#else
        implementation = gameObject.AddComponent<EditorAppsFlyerImplementation>();
#endif
    }

    void AppsFlyerSessionBegin()
    {
    }

    public void OnConversionDataLoaded(string json)
    {
        
    }

    public void OnInAppDone(string currencyCode, float expense, bool hackedPurchase)
    {
        const string NON_REVENUE_VALUE_HEADER = "value-";

        if (hackedPurchase)
            implementation.TrackEvent(AppsFlyerIds.Event_Hacker_InApp, currencyCode, NON_REVENUE_VALUE_HEADER + expense.ToString());
        else
            implementation.TrackEvent(AppsFlyerIds.Event_InApp, currencyCode, expense.ToString());
    }
}