using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using McSocialApiUtils;

public class EditorMcSocialApiImplementation : MonoBehaviour, McSocialApiImplementation
{
    const float responseDelay = 0.25f;

    public ErrorCode forcedResponse = ErrorCode.Ok;



    List<ScoreData> testScores = new List<ScoreData>() {
        new ScoreData("TEST-001", "Amy",      "PT", 1001,  1, LoginType.Guest, string.Empty),
        new ScoreData("TEST-002", "Bob",      "PT",  902,  2, LoginType.Guest, string.Empty),
        new ScoreData("TEST-003", "Charlie",  "PT",  803,  3, LoginType.Guest, string.Empty),
        new ScoreData("TEST-004", "Dan",      "PT",  704,  4, LoginType.Guest, string.Empty),
        new ScoreData("TEST-005", "Ellen",    "PT",  605,  5, LoginType.Guest, string.Empty),
        new ScoreData("TEST-006", "Frank",    "PT",  506,  6, LoginType.Guest, string.Empty),
        new ScoreData("TEST-007", "Guybrush", "PT",  407,  7, LoginType.Guest, string.Empty),
        new ScoreData("TEST-008", "Henry",    "PT",  308,  8, LoginType.Guest, string.Empty),
        new ScoreData("TEST-009", "Isaac",    "PT",  209,  9, LoginType.Guest, string.Empty),
        new ScoreData("TEST-010", "Jim",      "PT",   99, 10, LoginType.Guest, string.Empty),
        new ScoreData("TEST-011", "Kim",      "PT",   91, 11, LoginType.Guest, string.Empty),
        new ScoreData("TEST-012", "Larry",    "PT",   82, 12, LoginType.Guest, string.Empty),
        new ScoreData("TEST-013", "Mike",     "PT",   73, 13, LoginType.Guest, string.Empty),
        new ScoreData("TEST-014", "Nigel",    "PT",   64, 14, LoginType.Guest, string.Empty),
        new ScoreData("TEST-015", "Orson",    "PT",   55, 15, LoginType.Guest, string.Empty),
        new ScoreData("TEST-016", "Paul",     "PT",   46, 16, LoginType.Guest, string.Empty),
        new ScoreData("TEST-017", "Robin",    "PT",   37, 17, LoginType.Guest, string.Empty),
        new ScoreData("TEST-018", "Stella",   "PT",   28, 18, LoginType.Guest, string.Empty),
        new ScoreData("TEST-019", "Uma",      "PT",   19, 19, LoginType.Guest, string.Empty),
        new ScoreData("TEST-020", "Vicky",    "PT",   10, 20, LoginType.Guest, string.Empty),
        new ScoreData("TEST-020", "Zed",      "PT",   1,  21, LoginType.Guest, string.Empty)
    };


    void LogResponseError(string method, ErrorCode errorCode)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation Response Error - Method: " + method + " - Code: " + errorCode.ToString());
    }

    McSocialApiCallbacksHandler callbacksHandler;

    public void SetCallbacksHandler(McSocialApiCallbacksHandler callbacksHandler)
    {
        this.callbacksHandler = callbacksHandler;
    }

    public void Login(string apiKey, LoginType loginType, string token = "")
    {
        if (loginType == LoginType.Guest)
            Debug.Log("EditorMCLeaderSocialAPIImplementation - Login As Guest");
        else
            Debug.Log("EditorMCLeaderSocialAPIImplementation - Login With " + loginType.ToString() + " - token: " + token);

        StartCoroutine(LoginResponseCoroutine());
    }

    IEnumerator LoginResponseCoroutine()
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            LoginData loginData = new LoginData("976B-5B0-414-8AD43E", "Elmo", "APA91bd45md_L93w_0GEww", 1394841600, LoginType.Guest);
            Debug.Log("EditorMCLeaderSocialAPIImplementation Login Success - Login Data:\n" + loginData.ToString());
            callbacksHandler.loginSuccessCallback(loginData);
        }
        else
        {
            LogResponseError("Login", forcedResponse);
            callbacksHandler.loginFailCallback();
        }
    }

    public void GetFriends(string token)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - GetFriends");
        StartCoroutine(GetFriendsResponseCoroutine());
    }

    IEnumerator GetFriendsResponseCoroutine()
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            List<FriendData> friends = new List<FriendData>();
            friends.Add(new FriendData("986B-5B0-414-8AD43E", LoginType.Facebook, "100000000", "Elmo2"));
            friends.Add(new FriendData("8B7F-B10-D12-6AB471", LoginType.Facebook, "100000001", "Big Bird"));

            string friendsListString = string.Empty;
            foreach (var friend in friends)
                friendsListString += "\n" + friend.ToString() + "\n";
            Debug.Log("EditorMCLeaderSocialAPIImplementation - GetFriends Success - friends:\n" + friendsListString);

            callbacksHandler.getFriendsSuccessCallback(friends);
        }
        else
        {
            LogResponseError("GetFriends", forcedResponse);
            callbacksHandler.getFriendsFailCallback();
        }
    }

    public void GetScores(string token, bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "")
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - GetScores");
        StartCoroutine(GetScoresCoroutine(showTopScores, showFriends, scoresType, spread, country));
    }

    IEnumerator GetScoresCoroutine(bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string country = "")
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            ScoreFilter filter;
            if (showFriends)
                filter = ScoreFilter.Friends;
            else if (!String.IsNullOrEmpty(country))
                filter = ScoreFilter.Country;
            else
                filter = ScoreFilter.Global;

            int[] indices;
            if (filter == ScoreFilter.Country && scoresType == ScoreType.Latest)
                indices = new int[] { 4, 17, 5, 16, 18, 19, 9, 10, 11, 1, 2, 0, 5, 6, 7, 8, 3, 20 };
            else if (filter == ScoreFilter.Country && scoresType == ScoreType.Monthly)
                indices = new int[] { 0, 14, 5, 6, 19, 8, 9, 12, 13, 20, 15, 16, 7, 17, 18, 10, 11, 14 };
            else if (filter == ScoreFilter.Country && scoresType == ScoreType.Weekly)
                indices = new int[] { 0, 1, 2, 3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 6, 7, 8, 9, 17, 18, 19, 20 };
            else if (filter == ScoreFilter.Friends && scoresType == ScoreType.Latest)
                indices = new int[] { 13, 8, 9, 10, 19, 4, 5, 6, 14, 17, 18, 7, 11, 12, 20 };
            else if (filter == ScoreFilter.Friends && scoresType == ScoreType.Monthly)
                indices = new int[] { 0, 1, 4, 18, 8, 2, 3, 9, 10, 5, 6, 7, 11, 12, 13, 17, 19 };
            else if (filter == ScoreFilter.Friends && scoresType == ScoreType.Weekly)
                indices = new int[] { 0, 8, 9, 5, 6, 10, 11, 15, 16, 1, 2, 17, 20, 12, 14, 4 };
            else if (filter == ScoreFilter.Global && scoresType == ScoreType.Latest)
                indices = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            else if (filter == ScoreFilter.Global && scoresType == ScoreType.Monthly)
                indices = new int[] { 13, 14, 15, 18, 19, 3, 4, 5, 6, 9, 10, 11, 12, 20 };
            else
                indices = new int[] { 3, 4, 5, 17, 18, 0, 1, 6, 13, 14, 15, 16, 2 };

            List<ScoreData> scores = new List<ScoreData>();
            for (int i = 0; i < indices.Length; i++)
                scores.Add(testScores[indices[i]]);
            
            string scoresString = string.Empty;
            foreach (var score in scores)
                scoresString += "\n" + score.ToString() + "\n";
            Debug.Log("EditorMCLeaderSocialAPIImplementation - GetScores Success - scores:\n" + scoresString);

            callbacksHandler.getScoresSuccessCallback(scores);
        }
        else if (forcedResponse == ErrorCode.Undefined)
        {
            callbacksHandler.getScoresSuccessCallback(new List<ScoreData>());
        }
        else
        {
            LogResponseError("GetScores", forcedResponse);
            callbacksHandler.getScoresFailCallback();
        }
    }

    public void PostScore(string token, string hmacKey, string userId, long score, string leaderboard, string country = "")
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - PostScore");
        StartCoroutine(PostScoreCoroutine());
    }

    IEnumerator PostScoreCoroutine()
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - PostScore Success");

            callbacksHandler.postScoreSuccessCallback();
        }
        else
        {
            LogResponseError("PostScore", forcedResponse);
            callbacksHandler.postScoreFailCallback();
        }
    }

    public void StoreData(string token, List<DataToStore> data)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - StoreData");
        StartCoroutine(StoreDataCoroutine(data));
    }

    IEnumerator StoreDataCoroutine(List<DataToStore> data)
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            string dataString = string.Empty;
            foreach (var field in data)
                dataString += "\n" + field.ToString() + "\n";
            Debug.Log("EditorMCLeaderSocialAPIImplementation - StoreData Success - data:\n" + dataString);
            callbacksHandler.storeDataSuccessCallback();
        }
        else
        {
            LogResponseError("StoreData", forcedResponse);
            callbacksHandler.storeDataFailCallback();
        }
    }

    public void GetData(string token, List<string> fieldsName, string userId = "")
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - GetData");
        StartCoroutine(GetDataCoroutine(userId));
    }

    public void GetData(string token, List<string> fieldsName, List<string> userIds)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - GetData - userIds list");
        StartCoroutine(GetDataCoroutine(userIds[0]));
    }

    IEnumerator GetDataCoroutine(string userId)
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - GetData Success");
            callbacksHandler.getDataSuccessCallback(new List<RetrievedData>{new RetrievedData(userId, new List<DataToStore>{
                new DataToStore("games_won", 1, true),
                new DataToStore("nickname", "JohnnyBoy", true),
                new DataToStore("coins", 100, true)})});
        }
        else
        {
            LogResponseError("GetData", forcedResponse);
            callbacksHandler.getDataFailCallback();
        }
    }

    public void SendNotification(string token, string recepientId, string contentType, string notificationPayload)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - SendNotification");
        StartCoroutine(SendNotificationResponseCoroutine());
    }

    IEnumerator SendNotificationResponseCoroutine()
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - SendNotification Success");
            callbacksHandler.sendNotificationSuccessCallback();
        }
        else
        {
            LogResponseError("SendNotification", forcedResponse);
            callbacksHandler.sendNotificationFailCallback();
        }
    }

    public void ListNotifications(string token)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveAllNotifications");
        StartCoroutine(ReceiveNotificationsResponseCoroutine());
    }

    IEnumerator ReceiveNotificationsResponseCoroutine()
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveNotifications Success");
            callbacksHandler.receiveAllNotificationsSuccessCallback(new List<ReceivedNotification> {
                new ReceivedNotification("asdlkfj", "8B7F-B10-D12-6AB471", "Big Bird", 123456789, "fuel", ""),
                new ReceivedNotification("kfdssfj", "97BA-B10-D12-6AEF62", "Kermit", 45556789, "fuel",  "")});
        }
        else
        {
            LogResponseError("ReceiveNotifications", forcedResponse);
            callbacksHandler.receiveAllNotificationsFailCallback();
        }
    }

    public void ReceiveNotification(string token, string messageId)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveNotification");
        StartCoroutine(ReceiveSingleNotificationResponseCoroutine(messageId));
    }

    IEnumerator ReceiveSingleNotificationResponseCoroutine(string messageId)
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveNotifications Success");
            callbacksHandler.receiveSingleNotoficationSuccessCallback(new ReceivedNotification(messageId, "8B7F-B10-D12-6AB471", "Big Bird", 123456789, "fuel", ""));
        }
        else
        {
            LogResponseError("ReceiveNotifications", forcedResponse);
            callbacksHandler.receiveSingleNotificationFailCallback();
        }
    }

    public void DeleteNotification(string token, string messageId)
    {
        Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveNotifications");
        StartCoroutine(DeleteNotificationResponseCoroutine(messageId));
    }

    IEnumerator DeleteNotificationResponseCoroutine(string messageId)
    {
        yield return new WaitForSeconds(responseDelay);

        if (forcedResponse == ErrorCode.Ok)
        {
            Debug.Log("EditorMCLeaderSocialAPIImplementation - ReceiveNotifications Success");
            callbacksHandler.deleteNotificationSuccessCallback(new ReceivedNotification(messageId, "97BA-B10-D12-6AEF62", "Kermit", 45556789, "fuel", ""));
        }
        else
        {
            LogResponseError("ReceiveNotifications", forcedResponse);
            callbacksHandler.deleteNotificationFailCallback();
        }
    }

    public void GetUtcTime(string token)
    {
        if (forcedResponse == ErrorCode.Ok)
        {
            callbacksHandler.getUtcTimeSuccessCallback(DateTime.UtcNow);
        }
        else
        {
            callbacksHandler.getUtcTimeFailCallback();
        }
    }

    public void StopAnyWWWRequest()
    {
        return;
    }
}