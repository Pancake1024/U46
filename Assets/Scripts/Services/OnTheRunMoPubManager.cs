using UnityEngine;
using System.Collections;

public class OnTheRunMoPubManager : Manager<OnTheRunMoPubManager>
{
    #region Singleton instance
    protected static OnTheRunMoPubManager instance = null;

    public static OnTheRunMoPubManager Instance
    {
        get
        {
            return Manager<OnTheRunMoPubManager>.Get();
        }
    }
    #endregion

	const bool LOGS_ARE_ENABLED = false;

    public string phoneInterstitialAdUnit = "cdf225c4226d49b48e51dd0f50909057";
    public string tabletInterstitialAdUnit = "4c78d371fa9441909bf06f668433ddde";

    MoPubImplementation implementation;
    string interstitialAdUnitId;

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
		Log("Initialize()");

        SetAdUnitId();
        SetImplementation();
		implementation.Initialize(InterstitialDismissedCallback);
        RequestInterstitial();
    }

    void SetAdUnitId()
	{
		Log("SetAdUnitId() - Device Is Tablet: " + PluginsUtils.DeviceIsTablet());

        if (PluginsUtils.DeviceIsTablet())
            interstitialAdUnitId = tabletInterstitialAdUnit;
        else
            interstitialAdUnitId = phoneInterstitialAdUnit;
    }

    void SetImplementation()
	{
		Log("SetImplementation()");

#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorMoPubImplementation>();
#elif UNITY_IPHONE
        implementation = gameObject.AddComponent<UnitySdkMoPubImplementation>();
#elif UNITY_ANDROID
        implementation = gameObject.AddComponent<UnitySdkMoPubImplementation>();
#elif UNITY_WP8
        implementation = gameObject.AddComponent<VoidMoPubImplementation>();
#elif UNITY_WEBPLAYER
        implementation = gameObject.AddComponent<VoidMoPubImplementation>();
#else
        implementation = gameObject.AddComponent<VoidMoPubImplementation>();
#endif
    }

    void ReportApplicationOpen()
	{
		Log("ReportApplicationOpen()");
        implementation.ReportApplicationOpen();
    }

    void RequestInterstitial()
	{
		Log("RequestInterstitial()");
        implementation.RequestInterstitial(interstitialAdUnitId, false);
    }

    public void ShowInterstitial()
	{
        Log("ShowInterstitial() - ready: " + implementation.InterstitialIsReady + " - current page permits interstitials: " + OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials());

        if (implementation.InterstitialIsReady)
        {
            if (OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials())
                implementation.ShowInterstitial(interstitialAdUnitId);
        }
        else
        {
            if (OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials())
                implementation.RequestInterstitial(interstitialAdUnitId, true);
            else
                requestInterstitialCachingWhenPossible = true;
        }
    }

    bool requestInterstitialCachingWhenPossible = false;

    public void OnPageChanged()
    {
        bool shouldCacheInterstitial = requestInterstitialCachingWhenPossible && OnTheRunInterstitialsManager.Instance.DoesCurrentPagePermitInterstitials();
        
        Log("OnPageChanged() - shouldCacheInterstitial: " + shouldCacheInterstitial);
        if (shouldCacheInterstitial)
        {
            requestInterstitialCachingWhenPossible = false;
            implementation.RequestInterstitial(interstitialAdUnitId, false);
        }
    }
    
    void InterstitialDismissedCallback()
    {
        Log("InterstitialDismissedCallback");
        implementation.RequestInterstitial(interstitialAdUnitId, false);
        OnTheRunInterstitialsManager.Instance.OnInterstitialDismissed();
    }

	void Log(string logStr)
	{
		if (LOGS_ARE_ENABLED)
			Debug.Log("### °°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°° MoPub Manager - " + logStr);
	}
}