using UnityEngine;
using System.Collections;

public class OnTheRunAdjustManager : Manager<OnTheRunAdjustManager>
{
    #region Singleton instance
    protected static OnTheRunAdjustManager instance = null;

    public static OnTheRunAdjustManager Instance
    {
        get
        {
            return Manager<OnTheRunAdjustManager>.Get();
        }
    }
    #endregion

    public const string Evt_Purchase = "revenue";
    public const string Evt_Hacker_Purchase = "wrong_purchase";

    const bool ENABLE_EVENT_BUFFERING = false;

    bool IS_IN_PRODUCTION = false;
    public string iosAppToken;
    public string androidAppToken;

    AdjustImplementation implementation;
    string appToken;

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
        SetImplementation();
        SetProductionFlag();
        SetAppToken();
        implementation.Initialize(appToken, IS_IN_PRODUCTION, ENABLE_EVENT_BUFFERING);
    }

    void SetProductionFlag()
    {
#if UNITY_IPHONE
		if (SBS.Miniclip.MCUtilsBindings.isMassiveTestBuild())
            IS_IN_PRODUCTION = false;
        else
            IS_IN_PRODUCTION = true;
#elif UNITY_ANDROID
#   if DEVELOPMENT_BUILD
        IS_IN_PRODUCTION = false;
#   else
        IS_IN_PRODUCTION = true;
#   endif
#else
        IS_IN_PRODUCTION = false;
#endif
    }

    void SetAppToken()
    {
#if UNITY_IPHONE
        appToken = iosAppToken;
#elif UNITY_ANDROID
        appToken = androidAppToken;
#else
        appToken = string.Empty;
#endif
    }

    void SetImplementation()
    {
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorAdjustImplementation>();
#elif UNITY_IPHONE
        implementation = gameObject.AddComponent<UnitySdkAdjustImplementation>();
#elif UNITY_ANDROID && !UNITY_KINDLE
        implementation = gameObject.AddComponent<UnitySdkAdjustImplementation>();
#elif UNITY_ANDROID && UNITY_KINDLE
        implementation = gameObject.AddComponent<VoidAdjustImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<UnitySdkAdjustImplementation>();
#elif UNITY_WEBPLAYER
        implementation = gameObject.AddComponent<VoidAdjustImplementation>();
#else
        implementation = gameObject.AddComponent<VoidAdjustImplementation>();
#endif
    }

    public void TrackPurchase(string currencyCode, double expense, bool hackedPurchase)
    {
        double amountInCents = expense * 100.0;

        if (hackedPurchase)
            implementation.TrackRevenue(Evt_Hacker_Purchase, currencyCode, amountInCents);
        else
            implementation.TrackRevenue(Evt_Purchase, currencyCode, amountInCents);
    }
}