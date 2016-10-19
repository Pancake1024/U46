using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using McSocialApiUtils;

public class VoidMcSocialApiImplementation : MonoBehaviour, McSocialApiImplementation
{
    McSocialApiCallbacksHandler callbacksHandler;

    public void SetCallbacksHandler(McSocialApiCallbacksHandler callbacksHandler)
    {
        this.callbacksHandler = callbacksHandler;
    }

    public void Login(string apiKey, LoginType loginType, string token = "")
    {
        callbacksHandler.loginFailCallback();
    }

    public void GetFriends(string token)
    {
        callbacksHandler.getFriendsFailCallback();
    }

    public void GetScores(string token, bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "")
    {
        callbacksHandler.getScoresFailCallback();
    }

    public void PostScore(string token, string hmacKey, string userId, long score, string leaderboard, string country = "")
    {
        callbacksHandler.postScoreFailCallback();
    }

    public void StoreData(string token, List<DataToStore> data)
    {
        callbacksHandler.storeDataFailCallback();
    }

    public void GetData(string token, List<string> fieldsName, string userId = "")
    {
        callbacksHandler.getDataFailCallback();
    }

    public void GetData(string token, List<string> fieldsName, List<string> userIds)
    {
        callbacksHandler.getDataFailCallback();
    }

    public void SendNotification(string token, string recepientId, string contentType, string notificationPayload)
    {
        callbacksHandler.sendNotificationFailCallback();
    }

    public void ListNotifications(string token)
    {
        callbacksHandler.receiveAllNotificationsFailCallback();
    }

    public void ReceiveNotification(string token, string messageId)
    {
        callbacksHandler.receiveSingleNotificationFailCallback();
    }

    public void DeleteNotification(string token, string messageId)
    {
        callbacksHandler.deleteNotificationFailCallback();
    }

    public void GetUtcTime(string token)
    {
        callbacksHandler.getUtcTimeFailCallback();
    }

    public void StopAnyWWWRequest()
    {
        return;
    }
}