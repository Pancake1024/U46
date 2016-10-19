using UnityEngine;
using System.Collections;
using System;

public interface FyberImplementation {

    bool RewardedVideoIsReady { get; }
    bool InterstitialIsReady { get; }
    bool VideoRequestResponseIsReceived { get; }

    void Initialize(string appId, string securityToken, string userId);

    void LaunchOfferWall();

    void CacheRewardedVideo();
    void CacheAndLaunchRewardedVideo(Action<bool> callback);

    void LaunchRewardedVideo(Action<bool> callback);

    void RequestInterstitial(string placement);
    void ShowInterstitial();

    void AbortAnyPendigVideoRequest();
}
