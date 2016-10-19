using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using McSocialApiUtils;

public interface McSocialApiImplementation
{
    void Login(string apiKey, LoginType loginType, string token = "");
    void GetFriends(string token);
    void GetScores(string token, bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "");
    void PostScore(string token, string hmacKey, string userId, long score, string leaderboard, string country = "");
    void StoreData(string token, List<DataToStore> data);
    void GetData(string token, List<string> fieldsName, string userId = "");
    void GetData(string token, List<string> fieldsName, List<string> userIds);
    void SendNotification(string token, string recepientId, string contentType, string notificationPayload);
    void ListNotifications(string token);
    void ReceiveNotification(string token, string messageId);
    void DeleteNotification(string token, string messageId);

    void GetUtcTime(string token);

    void SetCallbacksHandler(McSocialApiCallbacksHandler callbacksHandler);

    void StopAnyWWWRequest();
}