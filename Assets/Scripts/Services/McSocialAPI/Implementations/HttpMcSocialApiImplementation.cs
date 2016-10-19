//#define TEST_ON_SBS_SERVER

using UnityEngine;
using System.Collections;
using SBS_MiniJSON;
using System.Collections.Generic;
using System.Text;
using System;
using McSocialApiUtils;

#if UNITY_WP8
using xBrainLab.Security.Cryptography;
#else
using System.Security.Cryptography;
#endif

public class HttpMcSocialApiImplementation : MonoBehaviour, McSocialApiImplementation
{
#if TEST_ON_SBS_SERVER
        const string URL = "http://172.19.6.254/www_test/services/mc_social_api_test/";
#else
    //const string URL = "https://api.miniclippt.com/v1/";
    //const string URL = "http://api.miniclippt.com/v1/";
    const string URL = "http://api.ontherun.miniclippt.com/v1/";
#endif

    public static string GEOIP_COUNTRY = "current";
    public static string ALL_COUNTRIES = "all";

    const string METHOD_LOGIN_USER = "user/authenticate";
    const string METHOD_GET_FRIENDS = "user/friends";
    const string METHOD_GET_SCORES = "scores/retrieve";
    const string METHOD_POST_SCORES = "scores/store";
    const string METHOD_STORE_DATA = "data/store";
    const string METHOD_RETRIEVE_DATA = "data/retrieve";
    const string METHOD_SEND_NOTIFICATION = "mailbox/send";
    const string METHOD_LIST_NOTIFICATIONS = "mailbox/list";
    const string METHOD_RETRIEVE_NOTIFICATION = "mailbox/retrieve";
    const string METHOD_GET_UTC_TIME = "timestamp";

    McSocialApiCallbacksHandler callbacksHandler;

    public void SetCallbacksHandler(McSocialApiCallbacksHandler callbacksHandler)
    {
        this.callbacksHandler = callbacksHandler;
    }

    #region Public Interface

    public void Login(string apiKey, LoginType loginType, string token = "")
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("api_key", apiKey);
        switch (loginType)
        {
            case LoginType.Facebook:
                payloadDict.Add("facebook_token", token);
                break;

            case LoginType.Google:
                payloadDict.Add("googleplus_token", token);
                break;
        }
        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_LOGIN_USER, payload);
    }

    public void GetFriends(string token)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);
        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_GET_FRIENDS, payload);
    }

    public void GetScores(string token, bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "")
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);
        payloadDict.Add("topscores", showTopScores);
        payloadDict.Add("friends", showFriends);
        switch (scoresType)
        {
            case ScoreType.Latest:
                payloadDict.Add("type", "latest");
                break;

            case ScoreType.Weekly:
                payloadDict.Add("type", "weekly");
                break;

            case ScoreType.Monthly:
                payloadDict.Add("type", "monthly");
                break;
        }
        payloadDict.Add("spread", spread);

        if (!string.IsNullOrEmpty(country))
            payloadDict.Add("country", country);
        else
            payloadDict.Add("country", ALL_COUNTRIES);

        // TODO: insert the new leaderboard param here
        payloadDict.Add("leaderboard", leaderboard);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_GET_SCORES, payload);
    }

    public void PostScore(string token, string hmacKey, string userId, long score, string leaderboard, string country = "")
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);
        payloadDict.Add("score", score);

        string messageToEncode = userId + ":" + score;

#if UNITY_WP8
        byte[] messageByteArray = Encoding.UTF8.GetBytes(messageToEncode);
        byte[] hmacKeyByteArray = Encoding.UTF8.GetBytes(hmacKey);
#else
        byte[] messageByteArray = Encoding.ASCII.GetBytes(messageToEncode);
        byte[] hmacKeyByteArray = Encoding.ASCII.GetBytes(hmacKey);
#endif

        HMACMD5 hmacMD5 = new HMACMD5(hmacKeyByteArray);
        byte[] encodedMessage = hmacMD5.ComputeHash(messageByteArray);
		string bacon = BitConverter.ToString(encodedMessage);
        bacon = System.Text.RegularExpressions.Regex.Replace(bacon, "-", "");
        payloadDict.Add("bacon", bacon.ToLowerInvariant());

        payloadDict.Add("leaderboard", leaderboard);

        if (string.IsNullOrEmpty(country))
            payloadDict.Add("country", country);
        else
            payloadDict.Add("country", GEOIP_COUNTRY);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_POST_SCORES, payload);
    }

    public void StoreData(string token, System.Collections.Generic.List<DataToStore> data)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);

        List<Dictionary<string, object>> fieldsList = new List<Dictionary<string, object>>();
        foreach (DataToStore field in data)
        {
            Dictionary<string, object> fieldDict = new Dictionary<string, object>();
            fieldDict.Add("name", field.Name);
            fieldDict.Add("value", field.Value);
            fieldDict.Add("public", field.IsPublic);

            fieldsList.Add(fieldDict);
        }
        payloadDict.Add("fields", fieldsList);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_STORE_DATA, payload);
    }

    public void GetData(string token, System.Collections.Generic.List<string> fieldsName, string userId = "")
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);

        if (fieldsName != null && fieldsName.Count > 0)
            payloadDict.Add("fields", fieldsName);

        if (!string.IsNullOrEmpty(userId))
            payloadDict.Add("user_id", userId);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_RETRIEVE_DATA, payload);
    }

    public void GetData(string token, System.Collections.Generic.List<string> fieldsName, List<string> userIds)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);

        if (fieldsName != null && fieldsName.Count > 0)
            payloadDict.Add("fields", fieldsName);

        if (userIds != null && userIds.Count > 0)
            payloadDict.Add("user_ids", userIds);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_RETRIEVE_DATA, payload);
    }

    public void SendNotification(string token, string recepientId, string contentType, string notificationPayload)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);
        payloadDict.Add("recipient_id", recepientId);
        payloadDict.Add("content_type", contentType);
        payloadDict.Add("payload", notificationPayload);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_SEND_NOTIFICATION, payload);
    }

    public void ListNotifications(string token)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_LIST_NOTIFICATIONS, payload);
    }

    public void ReceiveNotification(string token, string messageId)
    {
        CallRetrieveNotificationMethod(token, messageId, true);
    }

    public void DeleteNotification(string token, string messageId)
    {
        CallRetrieveNotificationMethod(token, messageId, false);
    }

    void CallRetrieveNotificationMethod(string token, string messageId, bool shouldRetainMessage)
    {
        Dictionary<string, object> payloadDict = new Dictionary<string, object>();
        payloadDict.Add("token", token);
        payloadDict.Add("message_id", messageId);
        payloadDict.Add("retain", shouldRetainMessage);

        string payload = Json.Serialize(payloadDict);

        //Debug.Log(payload);
        CallServerMethod(METHOD_RETRIEVE_NOTIFICATION, payload);
    }

    public void GetUtcTime(string token)
    {
        //Debug.Log("GET UTC TIME");

        CallServerMethod(METHOD_GET_UTC_TIME, string.Empty);
    }

    #endregion

    #region WWW Request Functions

    void CallServerMethod(string apiMethod, string payload)
    {
        // --- Debug.Log("CALLING - apiMethod: " + apiMethod + " - payload: " + payload);

#if UNITY_WP8
        byte[] postData = Encoding.UTF8.GetBytes(payload.ToCharArray());
#else
        byte[] postData = Encoding.ASCII.GetBytes(payload.ToCharArray());
#endif

#if TEST_ON_SBS_SERVER
        string requestUrl = URL + apiMethod + ".php";
#else
        string requestUrl = URL + apiMethod;
#endif
        WWW wwwRequest = null;
        if (payload.Equals(string.Empty))
            wwwRequest = new WWW(requestUrl);
        else
            wwwRequest = new WWW(requestUrl, postData);

        StartCoroutine(WaitServerResponse(wwwRequest, apiMethod, payload));
    }

    IEnumerator WaitServerResponse(WWW wwwRequest, string apiMethod, string payload)
    {
        yield return wwwRequest;

        // --- Debug.Log("DONE - apiMethod: " + apiMethod + " - payload: " + payload);
        //Debug.Log("Error: " + wwwRequest.error);

        if (string.IsNullOrEmpty(wwwRequest.error))
        {
            // --- Debug.Log("Response: " + wwwRequest.text);
            HandleResponse(apiMethod, payload, wwwRequest.text);
        }
        else
        {
            Debug.Log("Response Error: " + wwwRequest.error);
            HandleWwwRequestError(apiMethod, payload);
        }
    }

    void HandleResponse(string apiMethod, string callPayload, string response)
    {
        Dictionary<string, object> responseDict = Json.Deserialize(response) as Dictionary<string, object>;

        ErrorCode errorCode = ErrorCodeUtils.ErrorStringToErrorCode((string)responseDict["error"]);
        if (errorCode == ErrorCode.Ok)
            HandleSuccessfulResponse(apiMethod, callPayload, responseDict);
        else
            HandleErrorResponse(apiMethod, callPayload, responseDict);
    }

    #endregion

    #region Response Handling Functions

    void HandleSuccessfulResponse(string apiMethod, string callPayload, Dictionary<string, object> responseDict)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        if (responseDict.ContainsKey("data"))
            dataDict = responseDict["data"] as Dictionary<string, object>;

        List<object> dataArray = new List<object>();
        if (responseDict.ContainsKey("data"))
            dataArray = responseDict["data"] as List<object>;

        switch (apiMethod)
        {
            case METHOD_LOGIN_USER:
                Dictionary<string, object> loginPayloadDict = Json.Deserialize(callPayload) as Dictionary<string, object>;
                McSocialApiUtils.LoginType loginType;
                if (loginPayloadDict.ContainsKey("facebook_token"))
                    loginType = LoginType.Facebook;
                else if (loginPayloadDict.ContainsKey("googleplus_token"))
                    loginType = LoginType.Google;
                else
                    loginType = LoginType.Guest;
                
                LoginData loginData = new LoginData((string)dataDict["user_id"], (string)dataDict["name"], (string)dataDict["token"], (long)dataDict["expires"], loginType);
                callbacksHandler.loginSuccessCallback(loginData);
                break;

            case METHOD_GET_FRIENDS:
                List<FriendData> friends = new List<FriendData>();
                foreach (Dictionary<string, object> friendObject in dataArray)
                {
                    LoginType friendLoginType = LoginType.Guest;
                    string loginTypeId = string.Empty;
                    if (friendObject.ContainsKey("facebook_id"))
                    {
                        friendLoginType = LoginType.Facebook;
                        loginTypeId = (string)friendObject["facebook_id"];
                    }
                    else if (friendObject.ContainsKey("googleplus_id"))
                    {
                        friendLoginType = LoginType.Google;
                        loginTypeId = (string)friendObject["googleplus_id"];
                    }

                    friends.Add(new FriendData((string)friendObject["user_id"], friendLoginType, loginTypeId, (string)friendObject["name"]));
                }

                callbacksHandler.getFriendsSuccessCallback(friends);
                break;

            case METHOD_GET_SCORES:
                List<ScoreData> scores = new List<ScoreData>();
                foreach (Dictionary<string, object> scoreObject in dataArray)
                {
                    string id = (string)scoreObject["user_id"];
                    string name = (string)scoreObject["name"];
                    string country = (string)scoreObject["country"];
                    long score = (long)scoreObject["score"];
                    long rank = (long)scoreObject["rank"];

                    LoginType scoreLoginType = LoginType.Guest;
                    string loginTypeId = string.Empty;
                    if (scoreObject.ContainsKey("facebook_id"))
                    {
                        scoreLoginType = LoginType.Facebook;
                        loginTypeId = (string)scoreObject["facebook_id"];
                    }
                    else if (scoreObject.ContainsKey("googleplus_id"))
                    {
                        scoreLoginType = LoginType.Google;
                        loginTypeId = (string)scoreObject["googleplus_id"];
                    }

                    scores.Add(new ScoreData(id, name, country, score, rank, scoreLoginType, loginTypeId));
                }

                callbacksHandler.getScoresSuccessCallback(scores);
                break;

            case METHOD_POST_SCORES:
                callbacksHandler.postScoreSuccessCallback();
                break;

            case METHOD_STORE_DATA:
                callbacksHandler.storeDataSuccessCallback();
                break;

            case METHOD_RETRIEVE_DATA:
                /*Dictionary<string, object> getDataPayloadDict = Json.Deserialize(callPayload) as Dictionary<string, object>;
                string userId = string.Empty;
                if (getDataPayloadDict.ContainsKey("user_id"))
                    userId = (string)getDataPayloadDict["user_id"];
                else
                    userId = McSocialApiManager.Instance.UserLoginData.Id;

                List<DataToStore> dataList = new List<DataToStore>();
                foreach (Dictionary<string, object> fieldDict in dataArray)
                {
                    object fieldValue = null;
                    if (fieldDict.ContainsKey("value"))
                        fieldValue = fieldDict["value"];

                    bool isPublic = false;
                    if (fieldValue != null)
                        isPublic = McSocialApiManager.Instance.publicFieldCheck((string)fieldDict["name"]);

                    dataList.Add(new DataToStore((string)fieldDict["name"], fieldValue, isPublic));
                }

                callbacksHandler.getDataSuccessCallback(new RetrievedData(userId, dataList));*/

                List<RetrievedData> retrievedDataList = new List<RetrievedData>();
                foreach (Dictionary<string, object> userData in dataArray)
                {
                    string userId = string.Empty;
                    if (userData.ContainsKey("user_id"))
                        userId = (string)userData["user_id"];
                    
                    List<DataToStore> dataList = new List<DataToStore>();
                    if (userData.ContainsKey("data"))
                    {
                        List<object> userDataFields = userData["data"] as List<object>;
                        foreach (Dictionary<string, object> userDataField in userDataFields)
                        {
                            object fieldValue = null;
                            if (userDataField.ContainsKey("value"))
                                fieldValue = userDataField["value"];

                            bool isPublic = false;
                            if (fieldValue != null)
                                isPublic = McSocialApiManager.Instance.publicFieldCheck((string)userDataField["name"]);

                            dataList.Add(new DataToStore((string)userDataField["name"], fieldValue, isPublic));
                        }
                    }

                    retrievedDataList.Add(new RetrievedData(userId, dataList));
                }

                //foreach (var d in retrievedDataList)
                //    if (d != null)
                //    Debug.Log("########################################################### RETRIEVED DATA\n" + d.ToString());
                //
                //Debug.Log("########################################################### retrievedDataList.Count: " + retrievedDataList.Count);
                callbacksHandler.getDataSuccessCallback(retrievedDataList);
                break;

            case METHOD_SEND_NOTIFICATION:
                callbacksHandler.sendNotificationSuccessCallback();
                break;

            case METHOD_LIST_NOTIFICATIONS:
                List<ReceivedNotification> notificationsList = new List<ReceivedNotification>();
                foreach (Dictionary<string, object> notificationDict in dataArray)
                {
                    string messId = (string)notificationDict["message_id"];
                    string sendId = (string)notificationDict["sender_id"];
                    string sendName = (string)notificationDict["sender_name"];
                    long sendTime = (long)notificationDict["send_time"];
                    string contentType = (string)notificationDict["content_type"];
                    notificationsList.Add(new ReceivedNotification(messId, sendId, sendName, sendTime, contentType, string.Empty));
                }
                callbacksHandler.receiveAllNotificationsSuccessCallback(notificationsList);
                break;

            case METHOD_RETRIEVE_NOTIFICATION:
                ReceivedNotification receivedNotification = null;
                if (dataDict.Count > 0)
                    receivedNotification = new ReceivedNotification((string)dataDict["message_id"], (string)dataDict["sender_id"], (string)dataDict["sender_name"], (long)dataDict["send_time"], (string)dataDict["content_type"], (string)dataDict["payload"]);

                Dictionary<string, object> receiveNotificationPayloadDict = Json.Deserialize(callPayload) as Dictionary<string, object>;
                bool notificatioWasRetained = (bool)receiveNotificationPayloadDict["retain"];
                if (notificatioWasRetained)
                    callbacksHandler.receiveSingleNotoficationSuccessCallback(receivedNotification);
                else
                    callbacksHandler.deleteNotificationSuccessCallback(receivedNotification);
                break;

            case METHOD_GET_UTC_TIME:
                //const bool USE_TEST_TIME = true;
                DateTime time;
                //if (USE_TEST_TIME)
                //    time = DateTime.UtcNow;
                //else
                {
                    long unixTimeStamp = (long)responseDict["data"];
                    time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    time = time.AddSeconds(unixTimeStamp).ToUniversalTime();
                }
                callbacksHandler.getUtcTimeSuccessCallback(time);
                break;

            default:
                Debug.LogError("Unhandled Successful Response for Api Method: " + apiMethod);
                break;
        }
    }

    void HandleErrorResponse(string apiMethod, string callPayload, Dictionary<string, object> responseDict)
    {
        LogErrorList(apiMethod, callPayload, responseDict);

        switch (apiMethod)
        {
            case METHOD_LOGIN_USER:
                callbacksHandler.loginFailCallback();
                break;

            case METHOD_GET_FRIENDS:
                callbacksHandler.getFriendsFailCallback();
                break;

            case METHOD_GET_SCORES:
                callbacksHandler.getScoresFailCallback();
                break;

            case METHOD_POST_SCORES:
                callbacksHandler.postScoreFailCallback();
                break;

            case METHOD_STORE_DATA:
                callbacksHandler.storeDataFailCallback();
                break;

            case METHOD_RETRIEVE_DATA:
                callbacksHandler.getDataFailCallback();
                break;

            case METHOD_SEND_NOTIFICATION:
                callbacksHandler.sendNotificationFailCallback();
                break;

            case METHOD_LIST_NOTIFICATIONS:
                callbacksHandler.receiveAllNotificationsFailCallback();
                break;

            case METHOD_RETRIEVE_NOTIFICATION:
                Dictionary<string, object> payloadDict = Json.Deserialize(callPayload) as Dictionary<string, object>;
                bool notificatioWasRetained = (bool)payloadDict["retain"];
                if (notificatioWasRetained)
                    callbacksHandler.receiveSingleNotificationFailCallback();
                else
                    callbacksHandler.deleteNotificationFailCallback();
                break;

            case METHOD_GET_UTC_TIME:
                callbacksHandler.getUtcTimeFailCallback();
                break;

            default:
                Debug.LogError("Unhandled Error Response for Api Method: " + apiMethod);
                break;
        }
    }

    void HandleWwwRequestError(string apiMethod, string callPayload)
    {
        switch (apiMethod)
        {
            case METHOD_LOGIN_USER:
                callbacksHandler.loginFailCallback();
                break;

            case METHOD_GET_FRIENDS:
                callbacksHandler.getFriendsFailCallback();
                break;

            case METHOD_GET_SCORES:
                callbacksHandler.getScoresFailCallback();
                break;

            case METHOD_POST_SCORES:
                callbacksHandler.postScoreFailCallback();
                break;

            case METHOD_STORE_DATA:
                callbacksHandler.storeDataFailCallback();
                break;

            case METHOD_RETRIEVE_DATA:
                callbacksHandler.getDataFailCallback();
                break;

            case METHOD_SEND_NOTIFICATION:
                callbacksHandler.sendNotificationFailCallback();
                break;

            case METHOD_LIST_NOTIFICATIONS:
                callbacksHandler.receiveAllNotificationsFailCallback();
                break;

            case METHOD_RETRIEVE_NOTIFICATION:
                Dictionary<string, object> payloadDict = Json.Deserialize(callPayload) as Dictionary<string, object>;
                bool notificatioWasRetained = (bool)payloadDict["retain"];
                if (notificatioWasRetained)
                    callbacksHandler.receiveSingleNotificationFailCallback();
                else
                    callbacksHandler.deleteNotificationFailCallback();
                break;

            case METHOD_GET_UTC_TIME:
                callbacksHandler.getUtcTimeFailCallback();
                break;

            default:
                Debug.LogError("Unhandled WWW Request Error for Api Method: " + apiMethod);
                break;
        }
    }

    void LogErrorList(string apiMethod, string callPayload, Dictionary<string, object> responseDict)
    {
        List<string> errorList = null;
        if (responseDict.ContainsKey("error_list"))
            errorList = responseDict["error_list"] as List<string>;

        string debugStr = string.Empty;
        if (errorList != null)
        {
            for (int i = 0; i < errorList.Count; i++)
            {
                debugStr += errorList[i];
                if (i < errorList.Count - 1)
                    debugStr += "\n";
            }
        }

        Debug.Log("ERROR - " + apiMethod + " - error: " + (string)responseDict["error"] + "\nPayload: " + callPayload + "\n" + debugStr);
    }
    #endregion

    public void StopAnyWWWRequest()
    {
        StopAllCoroutines();
    }
}