using UnityEngine;
using System.Collections;
using System;

public class EditorFyberImplementation : MonoBehaviour, FyberImplementation
{
    const bool LOGS_ARE_ENABLED = false;

    public bool RewardedVideoIsReady { get { return videoIsReady; } }
    public bool InterstitialIsReady { get { return true; } }
    public bool VideoRequestResponseIsReceived { get { return videoRequestResponseIsReceived; } }

    bool videoIsReady = false;
    bool showVideoWhenReady = false;
    bool videoRequestResponseIsReceived = true;

    Action<bool> pendingCallback;

    public void Initialize(string appId, string securityToken, string userId)
    {
        Log("Initialize()");
    }

    public void LaunchOfferWall()
    {
        Log("LaunchOfferWall()");
    }

    public void CacheRewardedVideo()
    {
        RequestRewardedVideo(false);
    }
    
    public void CacheAndLaunchRewardedVideo(Action<bool> callback)
    {
        RequestRewardedVideo(true);
        pendingCallback = callback;
    }

    void RequestRewardedVideo(bool showWhenReady)
    {
        Log("RequestRewardedVideo - showWhenReady: " + showWhenReady);
        showVideoWhenReady = showWhenReady;
    }

    public void LaunchRewardedVideo(Action<bool> callback)
    {
        Log("LaunchRewardedVideo");
        
        if (callback != null)
            callback(true);

        pendingCallback = null;
    }

    public void RequestInterstitial(string placement)
    {
        Log("RequestInterstitial() - placement: " + placement);
    }

    public void ShowInterstitial()
    {
        Log("ShowInterstitial()");
    }

    public void AbortAnyPendigVideoRequest()
    {
        Log("AbortAnyPendigVideoRequest()");
        showVideoWhenReady = false;
        pendingCallback = null;
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### Fyber EDITOR - " + logStr);
    }

#if UNITY_EDITOR
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            videoIsReady = false;
            videoRequestResponseIsReceived = false;
            Log("setting video not ready");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Log("faking video loaded");
            onVideoLoadedEvent();
        }
    }*/
#endif

    void onVideoLoadedEvent()
    {
        Log("---------------------------------- onInterstitialLoadedEvent");
        videoIsReady = true;
        videoRequestResponseIsReceived = true;

#if UNITY_IPHONE || UNITY_ANDROID
        if (showVideoWhenReady && OnTheRunFyberManager.Instance.CurrentPageAllowsVideos())
            LaunchRewardedVideo(pendingCallback);
        else if (videoIsReady)
        {
            if (Manager<UIManager>.Get().ActivePage != null)
            {
                Log("------------------ Refreshing Free Coins Button on Active Page");
                Manager<UIManager>.Get().ActivePage.SendMessage("RefreshVideoButton", SendMessageOptions.DontRequireReceiver);
            }
            if (Manager<UIManager>.Get().FrontPopup != null)
            {
                Log("------------------ Refreshing Free Coins Button on Front Popup");
                Manager<UIManager>.Get().FrontPopup.SendMessage("RefreshVideoButton", SendMessageOptions.DontRequireReceiver);
            }
        }
#endif
    }
}