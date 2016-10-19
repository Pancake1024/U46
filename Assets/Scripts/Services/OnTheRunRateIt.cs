using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using SBS.Core;

public class OnTheRunRateIt : MonoBehaviour
{
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern string getRateItAppId();
		
	[DllImport("__Internal")]
	private static extern string getRateItUrl();	
#endif

    protected string rateItAppId
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			return getRateItAppId();
#elif UNITY_ANDROID && !UNITY_KINDLE
            return "com.miniclip.ontherun";
#elif UNITY_ANDROID && UNITY_KINDLE
            return "com.miniclip.ontherunamazon";
#else
            return "";
#endif
        }
    }

    protected string rateItUrl
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			string oldStyleUrl = "itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=" + rateItAppId;
			string url = getRateItUrl();
			if (String.IsNullOrEmpty(url))
				url = oldStyleUrl;			
			return url;
#elif UNITY_ANDROID && !UNITY_KINDLE
            return "https://play.google.com/store/apps/details?id=" + rateItAppId;
#elif UNITY_ANDROID && UNITY_KINDLE
            return "http://www.amazon.com/gp/mas/dl/android?p=" + rateItAppId;
#else
            return "";
#endif
        }
    }

    protected string StoreName
    {
        get
        {
#if UNITY_IPHONE
            return OnTheRunDataLoader.Instance.GetLocaleString("app_store_name");
#elif UNITY_ANDROID && !UNITY_KINDLE
            return OnTheRunDataLoader.Instance.GetLocaleString("google_store_name");
#elif UNITY_ANDROID && UNITY_KINDLE
            return OnTheRunDataLoader.Instance.GetLocaleString("amazon_store_name");
#else
            return OnTheRunDataLoader.Instance.GetLocaleString("store_name");
#endif
        }
    }

    #region Singleton instance
    protected static OnTheRunRateIt instance = null;

    public static OnTheRunRateIt Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    const bool SHOW_ALWAYS_FOR_TEST = false;

    protected static int maxShownPerSession = 5;
    protected static int thresholdIncrement = 20;

    protected string gameVersion = null;
    protected int playsCounter = 0;
    protected int playsThreshold = 10;
    protected int shownCounter = 0;
    protected int sessionCounter = 0;
    protected bool shouldShowPopup = false;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        DontDestroyOnLoad(gameObject);
        Init();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    
    void Init()
    {
        gameVersion = SBS.Core.iOSUtils.GetAppVersion();
#if UNITY_ANDROID
        gameVersion = AndroidUtils.GetVersionName();
#elif UNITY_WP8
        gameVersion = SBS.Miniclip.WP8Bindings.VersionNumber;
#endif
        playsCounter = EncryptedPlayerPrefs.GetInt("rip_playsCounter_" + gameVersion, playsCounter);
        playsThreshold = EncryptedPlayerPrefs.GetInt("rip_playsThreshold_" + gameVersion, playsThreshold);
        shownCounter = EncryptedPlayerPrefs.GetInt("rip_shownCounter_" + gameVersion, shownCounter);

        shouldShowPopup = 1 == EncryptedPlayerPrefs.GetInt("rip_showPopup_" + gameVersion, shouldShowPopup ? 1 : 0);

        sessionCounter = EncryptedPlayerPrefs.GetInt("rip_sessionCounter_" + gameVersion, 0);
        ++sessionCounter;
        EncryptedPlayerPrefs.SetInt("rip_sessionCounter_" + gameVersion, sessionCounter);

#if UNITY_EDITOR
        playsThreshold = 3;
        thresholdIncrement = 1;
#endif
    }

    public void OnRunFinished()
    {
        ++playsCounter;
        if (playsCounter > playsThreshold)
            shouldShowPopup = (shownCounter < maxShownPerSession && sessionCounter > 1);
        this.SaveData();

#if UNITY_EDITOR
        //Debug.Log("####################### RATE IT - ON RUN FINISHED - playsCounter " + playsCounter + ", playsThreshold: " + playsThreshold + ", shownCounter: " + shownCounter +
        //                            ", maxShownPerSession: " + maxShownPerSession + ", sessionCounter: " + sessionCounter + ", shouldShowPopup: " + shouldShowPopup);
#endif

        if (shouldShowPopup || SHOW_ALWAYS_FOR_TEST)
            ShowPopup();
    }

    void ShowPopup()
    {
        Asserts.Assert(shouldShowPopup);
        shouldShowPopup = false;

        ++shownCounter;

        playsThreshold = playsCounter + thresholdIncrement;
        this.SaveData();

        string rateInviteString = InsertSpaceCharAtTheEndOfStringIfMissing(OnTheRunDataLoader.Instance.GetLocaleString("rate_invite"));
#if UNITY_EDITOR
        //Debug.Log("####################### RATE IT - SHOW POPUP");
        return;
#elif UNITY_IPHONE
        SBS.Miniclip.AlertViewBindings.AlertBox(
            OnTheRunDataLoader.Instance.GetLocaleString("rate_descr"),
            rateInviteString + StoreName + "!",
            OnTheRunDataLoader.Instance.GetLocaleString("later"), new string[1] { OnTheRunDataLoader.Instance.GetLocaleString("rate") }, false);
        ObjCDispatcher.Instance.RegisterEvent("alertButtonClicked", onRateItAlertButtonClicked);
#elif  UNITY_ANDROID    // difference with iOS: NativeDispatcher
        SBS.Miniclip.AlertViewBindings.AlertBox(
            OnTheRunDataLoader.Instance.GetLocaleString("rate_descr"),
            rateInviteString + StoreName + "!",
            OnTheRunDataLoader.Instance.GetLocaleString("later"), new string[1] { OnTheRunDataLoader.Instance.GetLocaleString("rate") }, false);
        NativeDispatcher.Instance.RegisterEvent("alertButtonClicked", onRateItAlertButtonClicked);
#elif UNITY_WP8
        SBS.Miniclip.WP8Bindings.OnRateItShowPopup(OnTheRunDataLoader.Instance.GetLocaleString("rate_descr"),
                                                   rateInviteString + StoreName + "!",
                                                   SBS.Miniclip.WP8Bindings.RATE_IT_URL);
#endif

    }

    public string InsertSpaceCharAtTheEndOfStringIfMissing(string originalStr)
    {
        string returnStr = originalStr;

        if (!returnStr.EndsWith(" "))
            returnStr += " ";

        return returnStr;
    }

    void onRateItAlertButtonClicked(string btnIdx)
    {
#if UNITY_IPHONE
        ObjCDispatcher.Instance.RemoveEvent("alertButtonClicked");
#else
        NativeDispatcher.Instance.RemoveEvent("alertButtonClicked");
#endif
        //if (btnIdx == "Rate")
        if (btnIdx == OnTheRunDataLoader.Instance.GetLocaleString("rate"))
        {
            shownCounter = maxShownPerSession;
            this.SaveData();

            Application.OpenURL(rateItUrl);
        }
    }

    protected void SaveData()
    {
        EncryptedPlayerPrefs.SetInt("rip_playsCounter_" + gameVersion, playsCounter);
        EncryptedPlayerPrefs.SetInt("rip_playsThreshold_" + gameVersion, playsThreshold);
        EncryptedPlayerPrefs.SetInt("rip_shownCounter_" + gameVersion, shownCounter);
        EncryptedPlayerPrefs.SetInt("rip_showPopup_" + gameVersion, shouldShowPopup ? 1 : 0);
        EncryptedPlayerPrefs.SetInt("rip_sessionCounter_" + gameVersion, sessionCounter);
    }

}
