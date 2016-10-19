using UnityEngine;
using System.Collections;
#if UNITY_IPHONE && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public static class ChartboostBinding
{
#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void chartboostCacheInterstitialForLocation(string stringLocation);

	[DllImport("__Internal")]
	private static extern void chartboostShowInterstitialForLocation(string stringLocation);

	[DllImport("__Internal")]
	private static extern void chartboostLockInterstitials();

	[DllImport("__Internal")]
	private static extern void chartboostUnlockInterstitials();
#endif

    public static void CacheInterstitial(string location)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        chartboostCacheInterstitialForLocation(location);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("unityCacheInterstitial", location);
#endif
    }

    public static void ShowInterstitial(string location)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        chartboostShowInterstitialForLocation(location);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("unityShowInterstitial", location);
#endif
    }

    public static void LockInterstitials()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		chartboostLockInterstitials();
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("unitySetIngame", true);
#endif
    }

    public static void UnlockInterstitials()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		chartboostUnlockInterstitials();
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("unitySetIngame", false);
#endif
    }
}