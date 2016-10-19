using UnityEngine;
using System.Collections;
using System;

public class VoidFyberImplementation : MonoBehaviour, FyberImplementation
{
    public bool RewardedVideoIsReady { get { return false; } }
    public bool InterstitialIsReady { get { return false; } }
    public bool VideoRequestResponseIsReceived { get { return true; } }

    public void Initialize(string appId, string securityToken, string userId) { }

    public void LaunchOfferWall() { }

    public void CacheRewardedVideo() { }
    public void CacheAndLaunchRewardedVideo(Action<bool> callback) { callback(false); }
    public void LaunchRewardedVideo(Action<bool> callback) { callback(false); }

    public void RequestInterstitial(string placement) { }
    public void ShowInterstitial() { }

    public void AbortAnyPendigVideoRequest() { }
}