using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using McSocialApiUtils;
using System;
using SBS.Core;
using System.Globalization;

public class McSocialApiManager : MonoBehaviour
{
    #region Singleton instance
    protected static McSocialApiManager instance = null;

    public static McSocialApiManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    public const string FAKE_ID_00 = "fake_id_00";
    public const string FAKE_ID_01 = "fake_id_01";
    public const string FAKE_ID_02 = "fake_id_02";

    const int MAX_NOTIFICATION_QUOTA = 60000;

    public string apiKey;
    public string hmacKey;

    public bool IsLoggedIn { get { return isLoggedIn; } }
    public LoginData UserLoginData { get { return loginData; } }
    public List<FriendData> Friends { get { return friends; } }
    public List<ScoreData> LastRequestedScores { get { return lastRequestedScores; } }
    public RetrievedData LastRetrievedData { get { return lastRetrievedData; } }
    public List<RetrievedData> LastRetrievedDataList { get { return lastRetrievedDataList; } }
    public List<ReceivedNotification> LastReceivedNotifications { get { return lastReceivedNotifications; } }
    public ReceivedNotification LastDeletedNotification { get { return lastDeletedNotification; } }
    public Dictionary<string, float> LastFriendsExperienceData { get { return lastFriendsExperienceData; } }
    
    public Action OnFriendsAvailable = null;

    public delegate bool PublicFieldCheckDelegate(string field);
    public PublicFieldCheckDelegate publicFieldCheck;

    McSocialApiImplementation implementation;

    LoginData loginData;
    bool isLoggedIn = false;

    List<FriendData> friends = null;
    List<ScoreData> lastRequestedScores = null;
    RetrievedData lastRetrievedData = null;
    List<RetrievedData> lastRetrievedDataList = null;
    List<ReceivedNotification> lastReceivedNotifications = null;
    ReceivedNotification lastDeletedNotification = null;
    Dictionary<string, float> lastFriendsExperienceData = null;

    bool isWaitingForData;

    McSocialApiCallbacksHandler callbacksHandler;

    bool requestsWereAborted;

    ScoreRequest lastRankingPageRequest = null;

    public bool friendsRequestForRankBarReady = false;
    public bool friendsPicturesForRankBarReady = false;
    bool friendsRequestForRankBarStarted = false;
    bool scoresForIngameAreReady;
    public bool ScoresForIngameAreReady { get { return scoresForIngameAreReady; } }

    public int lastStoredLevel = 0;
    public int lastStoredExperience = 0;

    protected DateTime UTCEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    protected const long ticksPerSecond = 1000;
    protected const int numTokenRefreshRetriesOnFail = 5;
    protected const int refreshPeriodInSeconds = 300;
    bool shouldRefreshToken = false;

    DateTime tokenExpirationDate = new DateTime(0);
    float lastTokenCheckRealtimeSinceStartup;   //int lastTokenCheckTicks;
    int tokenRefreshRetiresCounter = 0;
    float tokenRefreshRetriesDelaySeconds = 2.0f;
    
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);

        SetImplementation();
        RegisterCallbacks();

        publicFieldCheck = new PublicFieldCheckDelegate(OTR_CheckPublicFields);
        isWaitingForData = false;
        scoresForIngameAreReady = false;

        requestsWereAborted = false;
		lastStoredLevel = 0;
        lastStoredExperience = 0;
    }

    bool OTR_CheckPublicFields(string field)
    {
        if (field == OnTheRunMcSocialApiData.Instance.levelPropertyName)
            return true;

        return false;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    public void Logout()
    {
        isLoggedIn = false;
		lastStoredLevel = 0;
        lastStoredExperience = 0;

        loginData = LoginData.InvalidLogin();

        if (friends != null)
            friends.Clear();
        friends = null;

        if (lastRequestedScores != null)
            lastRequestedScores.Clear();
        lastRequestedScores = null;

        if (lastReceivedNotifications != null)
            lastReceivedNotifications.Clear();
        lastReceivedNotifications = null;
                
        lastRetrievedData = null;

        if (lastRetrievedDataList != null)
            lastRetrievedDataList.Clear();
        lastRetrievedDataList = null;
        
        lastDeletedNotification = null;

        requestsWereAborted = false;

        ResetTokenExpirationToNone();

        callbacksHandler.loginSuccessCallback = OnLoginSuccess;
        callbacksHandler.loginFailCallback = OnLoginFail;
    }

    #region Initializations

    void SetImplementation()
    {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
        implementation = gameObject.AddComponent<HttpMcSocialApiImplementation>();  //EditorMcSocialApiImplementation>();
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8
		implementation = gameObject.AddComponent<HttpMcSocialApiImplementation>();
#elif UNITY_WEBPLAYER
		implementation = gameObject.AddComponent<VoidMcSocialApiImplementation>();
#else
        implementation = gameObject.AddComponent<HttpMcSocialApiImplementation>();
#endif
    }

    void RegisterCallbacks()
    {
        callbacksHandler = new McSocialApiCallbacksHandler();
        
        callbacksHandler.loginSuccessCallback = OnLoginSuccess;
        callbacksHandler.loginFailCallback = OnLoginFail;
        
        callbacksHandler.getFriendsSuccessCallback = OnGetFriendsSuccess;
        callbacksHandler.getFriendsFailCallback = OnGetFriendsFail;
        
        callbacksHandler.getScoresSuccessCallback = OnGetScoresSuccess;
        callbacksHandler.getScoresFailCallback = OnGetScoresFail;
        
        callbacksHandler.postScoreSuccessCallback = OnPostScoreSuccess;
        callbacksHandler.postScoreFailCallback = OnPostScoreFail;
        
        callbacksHandler.storeDataSuccessCallback = OnStoreDataSuccess;
        callbacksHandler.storeDataFailCallback = OnStoreDataFail;
        
        callbacksHandler.getDataSuccessCallback = OnGetDataSuccess;
        callbacksHandler.getDataFailCallback = OnGetDataFail;
        
        callbacksHandler.sendNotificationSuccessCallback = OnSendNotificationSuccess;
        callbacksHandler.sendNotificationFailCallback = OnSendNotificationFail;
        
        callbacksHandler.receiveAllNotificationsSuccessCallback = OnReceiveAllNotificationsSuccess;
        callbacksHandler.receiveAllNotificationsFailCallback = OnReceiveAllNotificationsFail;
        
        callbacksHandler.receiveSingleNotoficationSuccessCallback = OnReceiveSingleNotificationSuccess;
        callbacksHandler.receiveSingleNotificationFailCallback = OnReceiveSingleNotificationFail;
        
        callbacksHandler.deleteNotificationSuccessCallback = OnDeleteNotificationSuccess;
        callbacksHandler.deleteNotificationFailCallback = OnDeleteNotificationFail;

        callbacksHandler.getUtcTimeSuccessCallback = OnGetUtcTimeSuccess;
        callbacksHandler.getUtcTimeFailCallback = OnGetUctTimeFail;

        implementation.SetCallbacksHandler(callbacksHandler);
    }

    void ResetTokenExpirationToNone()
    {
        shouldRefreshToken = false;
        tokenExpirationDate = new DateTime(0);
        tokenRefreshRetiresCounter = 0;
    }

    void SetTokenExpirationData(long tokenExpirationUnixTimestamp)
    {
        shouldRefreshToken = true;
        tokenExpirationDate = UTCEpoch.AddSeconds(tokenExpirationUnixTimestamp).ToUniversalTime();
        tokenRefreshRetiresCounter = 0;

        //Debug.Log("@#@#@#@#@#@#@#@ SetTokenExpirationData - tokenExpirationDate: " + tokenExpirationDate);

        /*DateTime utcNow;
        bool isValid;
        DateTimeManager.Instance.GetDate(out utcNow, out isValid);
        tokenExpirationDate = utcNow.AddMinutes(2.1);
        Debug.Log("@#@#@#@#@#@#@#@ SetTokenExpirationData - faking token Expiration - now: " + utcNow + " - tokenExpirationDate: " + tokenExpirationDate);*/
    }

    #endregion

    public void GetScoresForRankingPage(bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "")
    {
        //Debug.Log("### ### ### GetScoresForRankingPage - isLoggedIn: " + isLoggedIn);

        if (!isLoggedIn)
        {
            OnGetScoresFail();
            OnGetScoresForRankingPageFail();
            return;
        }

        requestsWereAborted = false;
        lastRankingPageRequest = new ScoreRequest(showTopScores, showFriends, scoresType, spread, leaderboard, country);

        callbacksHandler.getScoresSuccessCallback = OnGetScoresSuccess;
        callbacksHandler.getScoresSuccessCallback += OnGetScoresForRankingPageSuccess;

        callbacksHandler.getScoresFailCallback = OnGetScoresFail;
        callbacksHandler.getScoresFailCallback += OnGetScoresForRankingPageFail;

        StartCoroutine(GetScoresTimeoutCoroutine());
        implementation.GetScores(loginData.Token, showTopScores, showFriends, scoresType, spread, leaderboard, country);
    }

    bool getsScoresResultBeforeTimeout = false;

    IEnumerator GetScoresTimeoutCoroutine()
    {
        const float TIMEOUT_SECONDS = 20.0f;

        //Debug.Log("                                           >>>>>>>>>>>>>>>> STARTING SCORES REQUEST TIMEOUT");
        float timeBefore = TimeManager.Instance.MasterSource.TotalTime;
        getsScoresResultBeforeTimeout = false;

        while (TimeManager.Instance.MasterSource.TotalTime - timeBefore < TIMEOUT_SECONDS)
        {
            if (getsScoresResultBeforeTimeout)
            {
                //Debug.Log("                                           >>>>>>>>>>>>>>>> SCORES RESULT ARRIVED BEFORE TIMEOUT");
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        //Debug.Log("                                           >>>>>>>>>>>>>>>> SCORES REQUEST TIMED OUT - STOPPING ANY REQUEST");
        StopAnyRequest();

        if (callbacksHandler.getScoresFailCallback != null)
        {
            //Debug.Log("                                           >>>>>>>>>>>>>>>> SCORES REQUEST TIMED OUT - FAIL CALLBACK");
            callbacksHandler.getScoresFailCallback();

            //Debug.Log("                                           >>>>>>>>>>>>>>>> SCORES REQUEST TIMED OUT - POPPING LOADING POPUP");
            UIManager uiManager = Manager<UIManager>.Get();
            if (uiManager.IsPopupInStack("LoadingPopup") && uiManager.FrontPopup.name.Equals("LoadingPopup"))
                uiManager.PopPopup();
        }
    }

    public void GetScoresForIngame(string leaderboard)
    {
        if (!isLoggedIn)
        {
            OnGetScoresFail();
            OnGetScoresForIngameFail();
            return;
        }

        long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresIngameRanks() - 1;
        /*bool showFriends = (loginData.Type != LoginType.Guest);
        
        GetScoresForIngame(true, showFriends, ScoreType.Latest, scoresSpread, leaderboard, (retrievedScores) =>
        {
            if (showFriends && retrievedScores.Count <= 0) // OLD_RANKING_LOGIC: 1)
            {
                /*
                // OLD_RANKING_LOGIC: GetScoresForIngame(true, false, ScoreType.Latest, scoresSpread, leaderboard, (newScores) => { OnGetScoresForIngameSuccess(newScores); });
                List<ScoreData> fakeScoresForFriendlessPlayer = new List<ScoreData>();
                fakeScoresForFriendlessPlayer.Add(new ScoreData("fake_id_00", "Best Friend 1", "unknown_country", 5000, 3, LoginType.Guest, string.Empty));
                fakeScoresForFriendlessPlayer.Add(new ScoreData("fake_id_01", "Best Friend 2", "unknown_country", 10000, 2, LoginType.Guest, string.Empty));
                fakeScoresForFriendlessPlayer.Add(new ScoreData("fake_id_02", "Best Friend 3", "unknown_country", 15000, 1, LoginType.Guest, string.Empty));
                lastRequestedScores = fakeScoresForFriendlessPlayer;
                * /

                MergeLastRequestedScoresWithFakeFriends();

                scoresForIngameAreReady = true;
            }
            else
                OnGetScoresForIngameSuccess(retrievedScores);
        });*/

        if (loginData.Type == LoginType.Guest)
        {
            lastRequestedScores = MergeScoresWithFakeFriends(new List<ScoreData>());
            scoresForIngameAreReady = true;
            //LogScores(lastRequestedScores);
        }
        else
        {
            GetScoresForIngame(true, true, ScoreType.Latest, scoresSpread, leaderboard, (retrievedScores) =>
            {
                /*Debug.Log("######### BEFORE");
                LogScores(retrievedScores);*/

                lastRequestedScores = MergeScoresWithFakeFriends(retrievedScores);
                OnGetScoresForIngameSuccess(lastRequestedScores);

                /*Debug.Log("######### AFTER");
                LogScores(lastRequestedScores);*/
            });
        }

        /*
        GetScoresForIngame(true, showFriends, ScoreType.Latest, scoresSpread, leaderboard, (retrievedScores) => {
            if (showFriends)
            {
                retrievedScores.Sort(delegate(ScoreData score1, ScoreData score2)
                {
                    return score1.Rank.CompareTo(score2.Rank);
                });

                bool userHasFriendsOnTop = false;
                for (int i = 0; i < retrievedScores.Count; i++)
                {
                    if (retrievedScores[i].Id.Equals(loginData.Id) && i > 0)
                    {
                        userHasFriendsOnTop = true;
                        break;
                    }
                }

                if (!userHasFriendsOnTop)
                {
                    GetScoresForIngame(true, false, ScoreType.Latest, scoresSpread, leaderboard, (newScores) => { OnGetScoresForIngameSuccess(newScores); });
                    return;
                }
            }
            OnGetScoresForIngameSuccess(retrievedScores);
        });*/
    }

    public void GetFriendsLevelsForRankBar( )
    {
        if (friendsRequestForRankBarStarted)
            return;

        friendsRequestForRankBarReady = false;
        friendsPicturesForRankBarReady = false;
        //McSocialApiManager.Instance.StopAnyRequest();
        GetFriends(success =>
        {
            if (!friendsRequestForRankBarStarted)
            {
                friendsRequestForRankBarStarted = true;

                List<string> facebookIds = new List<string>();
                List<string> googlePlusIds = new List<string>();

                foreach (FriendData data in friends)
                    facebookIds.Add(data.LoginTypeId);

                RequestPictures(facebookIds, googlePlusIds, OnGetPicturesForRankBarSuccess);

                GetLevelsForRankBar((retrievedData) =>
                {
                    OnGetScoresForRankBarSuccess(retrievedData);
                });
            }
        });
    }

    void GetLevelsForRankBar(Action<List<RetrievedData>> additionalCallback)
    {
        requestsWereAborted = false;

        callbacksHandler.getDataSuccessCallback = OnGetDataSuccess;
        callbacksHandler.getDataSuccessCallback += additionalCallback;

        callbacksHandler.getDataFailCallback = OnGetDataFail;
        callbacksHandler.getDataFailCallback += OnGetRankBarDataFail;
        
        List<string> friendsList = new List<string>();
        for(int i=0; i<friends.Count; ++i)
            friendsList.Add(friends[i].Id);

        if (friendsList.Count > 0)
        {
            isWaitingForData = true;
            implementation.GetData(loginData.Token, new List<string> { OnTheRunMcSocialApiData.Instance.levelPropertyName, OnTheRunMcSocialApiData.Instance.experiencePropertyName }, friendsList);
        }
        else
            friendsRequestForRankBarStarted = false;

    }

    List<ScoreData> MergeScoresWithFakeFriends(List<ScoreData> scoresToBeMerged)
    {
        List<ScoreData> fakeScoresForFriendlessPlayer = new List<ScoreData>();
        fakeScoresForFriendlessPlayer.Add(new ScoreData(FAKE_ID_02, OnTheRunDataLoader.Instance.GetLocaleString("fake_username_3"), "unknown_country", 15000, 1, LoginType.Guest, string.Empty));
        fakeScoresForFriendlessPlayer.Add(new ScoreData(FAKE_ID_01, OnTheRunDataLoader.Instance.GetLocaleString("fake_username_2"), "unknown_country", 10000, 2, LoginType.Guest, string.Empty));
        fakeScoresForFriendlessPlayer.Add(new ScoreData(FAKE_ID_00, OnTheRunDataLoader.Instance.GetLocaleString("fake_username_1"), "unknown_country", 5000, 3, LoginType.Guest, string.Empty));

        if (scoresToBeMerged == null || scoresToBeMerged.Count == 0)
            return fakeScoresForFriendlessPlayer;

        scoresToBeMerged.Sort(
            delegate(McSocialApiUtils.ScoreData score1, McSocialApiUtils.ScoreData score2)
            {
                return score1.Rank.CompareTo(score2.Rank);
            });

        foreach (var fakeScore in fakeScoresForFriendlessPlayer)
            scoresToBeMerged.Add(fakeScore);

        scoresToBeMerged.Sort(
            delegate(McSocialApiUtils.ScoreData score1, McSocialApiUtils.ScoreData score2)
            {
                return -1 * score1.Score.CompareTo(score2.Score);
            });

        for (int i = 0; i < scoresToBeMerged.Count; i++)
            scoresToBeMerged[i].SetRank(i + 1);

        return scoresToBeMerged;

        /*bool allFakeScoresWereInserted = false;
        int fakeScoresCounter = 0;
        int currentRank = 1;
        List<ScoreData> integratedList = new List<ScoreData>();
        for (int i = 0; i < scoresToBeMerged.Count; i++)
        {
            for (int j = fakeScoresCounter; j < fakeScoresForFriendlessPlayer.Count; j++)
            {
                if (!allFakeScoresWereInserted && scoresToBeMerged[i].Score <= fakeScoresForFriendlessPlayer[fakeScoresCounter].Score)
                {
                    fakeScoresForFriendlessPlayer[fakeScoresCounter].SetRank(currentRank++);
                    integratedList.Add(fakeScoresForFriendlessPlayer[fakeScoresCounter++]);

                    if (fakeScoresCounter >= fakeScoresForFriendlessPlayer.Count)
                        allFakeScoresWereInserted = true;
                }
            }

            scoresToBeMerged[i].SetRank(currentRank++);
            integratedList.Add(scoresToBeMerged[i]);
        }

        if (fakeScoresCounter < fakeScoresForFriendlessPlayer.Count)
        {
            for(int i = fakeScoresCounter; i < fakeScoresForFriendlessPlayer.Count ; i++)
            {
                fakeScoresForFriendlessPlayer[fakeScoresCounter].SetRank(currentRank++);
                integratedList.Add(fakeScoresForFriendlessPlayer[i]);
            }
        }

        scoresToBeMerged = integratedList;
        
        return scoresToBeMerged;*/
    }

    public void LogScores(List<ScoreData> scores)
    {
        string debugStr = "################################################ SCORES";
        foreach (var s in scores)
            debugStr += "\n" + s.Rank + " - " + s.Score + " - " + s.Name;

        Debug.Log(debugStr);
    }

    /*public*/ void GetScoresForIngame(bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, /*string country = "",*/ Action<List<ScoreData>> additionalCallback)
    {
        if (!isLoggedIn)
        {
            OnGetScoresFail();
            OnGetScoresForIngameFail();
            return;
        }

        requestsWereAborted = false;

        callbacksHandler.getScoresSuccessCallback = OnGetScoresSuccess;
        callbacksHandler.getScoresSuccessCallback += additionalCallback;    //OnGetScoresForIngameSuccess;

        callbacksHandler.getScoresFailCallback = OnGetScoresFail;
        callbacksHandler.getScoresFailCallback += OnGetScoresForIngameFail;

        scoresForIngameAreReady = false;

        implementation.GetScores(loginData.Token, showTopScores, showFriends, scoresType, spread, leaderboard, string.Empty);
    }

    public void StopAnyRequest()
    {
        requestsWereAborted = true;

        callbacksHandler.getScoresSuccessCallback = OnGetScoresSuccess;
        callbacksHandler.getScoresFailCallback = OnGetScoresFail;

        StopAllCoroutines();
        implementation.StopAnyWWWRequest();

        lastRequestedScores = null;
        lastRetrievedData = null;
        if (lastRetrievedDataList != null)
            lastRetrievedDataList.Clear();
        lastRetrievedDataList = null;

        isWaitingForData = false;
        scoresForIngameAreReady = false;
    }

    #region Api calls
    
    public void LoginWithSavedGuest()
    {
        if (!LoginData.GuestLoginDataIsSaved())
            return;

        requestsWereAborted = false;

        /*loginData = McSocialApiUtils.LoginData.LoadPlayerPrefsGuest();
        isLoggedIn = true;*/

        OnLoginSuccess(McSocialApiUtils.LoginData.LoadPlayerPrefsGuest());
    }

    Action<bool> additionalFacebookLoginCallback = null;

    public void LoginWithFacebook(string facebookToken, Action<bool> additionalCallback)
    {
        requestsWereAborted = false;

        Logout();

        additionalFacebookLoginCallback = additionalCallback;

        /*callbacksHandler.loginSuccessCallback = OnLoginSuccess;
        if (additionalCallback != null)
            callbacksHandler.loginSuccessCallback += (loginData) => { additionalCallback(true); };

        callbacksHandler.loginFailCallback = OnLoginFail;
        if (additionalCallback != null)
            callbacksHandler.loginFailCallback += () => { additionalCallback(false); };*/

        implementation.Login(apiKey, LoginType.Facebook, facebookToken);
    }

    public void LoginWithGoogle(string googleToken)
    {
        requestsWereAborted = false;

        Logout();
        implementation.Login(apiKey, LoginType.Google, googleToken);
    }

    public void LoginAsGuest()
    {
        requestsWereAborted = false;

        Logout();

        if (LoginData.GuestLoginDataIsSaved())
            LoginWithSavedGuest();
        else
            implementation.Login(apiKey, LoginType.Guest);
    }

    public void GetFriends(Action<bool> callback)
    {
        if (!isLoggedIn)
        {
            callback(false);
            return;
        }

        callbacksHandler.getFriendsSuccessCallback = OnGetFriendsSuccess;
        callbacksHandler.getFriendsSuccessCallback += ((friends) => { callback(true); });

        callbacksHandler.getFriendsFailCallback = OnGetFriendsFail;
        callbacksHandler.getFriendsFailCallback += () => { callback(false); };


        requestsWereAborted = false;
        implementation.GetFriends(loginData.Token);
    }
       
    public void GetScores(bool showTopScores, bool showFriends, ScoreType scoresType, long spread, string leaderboard, string country = "")
    {
        if (!isLoggedIn)
            return;

        requestsWereAborted = false;
        implementation.GetScores(loginData.Token, showTopScores, showFriends, scoresType, spread, leaderboard, country);
    }

    public void PostScore(string leaderboard, long score, string country = "")
    {
        if (!isLoggedIn)
            return;

        requestsWereAborted = false;
        implementation.PostScore(loginData.Token, hmacKey, loginData.Id, score, leaderboard, country);
    }

    public void StoreData(List<DataToStore> data)
    {
        if (!isLoggedIn)
            return;

        requestsWereAborted = false;
        implementation.StoreData(loginData.Token, data);
    }

    public void GetData(List<string> fieldsName, string userId = "")
    {
        if (isWaitingForData)
            return;

		callbacksHandler.getDataSuccessCallback = OnGetDataSuccess;
		callbacksHandler.getDataFailCallback = OnGetDataFail;

        requestsWereAborted = false;

        isWaitingForData = true;
        implementation.GetData(loginData.Token, fieldsName, userId);
    }

    public void GetData(List<string> fieldsName, List<string> userIds)
    {
        if (isWaitingForData)
            return;
		
		callbacksHandler.getDataSuccessCallback = OnGetDataSuccess;
		callbacksHandler.getDataFailCallback = OnGetDataFail;

        requestsWereAborted = false;

        isWaitingForData = true;
        implementation.GetData(loginData.Token, fieldsName, userIds);
    }

    public void SendNotification(string recepientId, string contentType, string notificationPayload)
    {
        requestsWereAborted = false;

        implementation.SendNotification(loginData.Token, recepientId, contentType, notificationPayload);
    }

    public void ReceiveAllNotifications(Action<List<ReceivedNotification>> additionalSuccessCallback, Action additionalFailedCallback)
    {
        requestsWereAborted = false;

        callbacksHandler.receiveAllNotificationsSuccessCallback = OnReceiveAllNotificationsSuccess;
        callbacksHandler.receiveAllNotificationsSuccessCallback += additionalSuccessCallback;

        callbacksHandler.receiveAllNotificationsFailCallback = OnReceiveAllNotificationsFail;
        callbacksHandler.receiveAllNotificationsFailCallback += additionalFailedCallback;

        implementation.ListNotifications(loginData.Token);
    }

    public string GetLastReceivedNotifications()
    {
        string lastNotificationsString = string.Empty;
        
        for (int i = 0; i < lastReceivedNotifications.Count; i++)
        {
            lastNotificationsString += lastReceivedNotifications[i].ToString();
            if (i < lastReceivedNotifications.Count - 1)
                lastNotificationsString += "\n\n";
        }

        return lastNotificationsString;
    }

    public void ReceiveSingleNotification(string messageId, Action<ReceivedNotification> additionalSuccessCallback, Action additionalFailedCallback)
    {
        requestsWereAborted = false;

        //callbacksHandler.receiveSingleNotoficationSuccessCallback = OnReceiveSingleNotificationSuccess;
        //callbacksHandler.receiveSingleNotoficationSuccessCallback += additionalSuccessCallback;

        callbacksHandler.receiveSingleNotoficationSuccessCallback = (receivedNotification) => { OnReceiveSingleNotificationSuccess_WithAdditionalData(receivedNotification, additionalSuccessCallback); };

        callbacksHandler.receiveSingleNotificationFailCallback = OnReceiveSingleNotificationFail;
        callbacksHandler.receiveSingleNotificationFailCallback += additionalFailedCallback;

        implementation.ReceiveNotification(loginData.Token, messageId);
    }

    public void DeleteNotification(string messageId)
    {
        requestsWereAborted = false;

        implementation.DeleteNotification(loginData.Token, messageId);
    }

    public void GetUtcTime()
    {
        requestsWereAborted = false;

        implementation.GetUtcTime(loginData.Token);
    }

    #endregion

    #region Callbacks

    void OnLoginSuccess(LoginData data)
    {
        loginData = data;

        if (loginData.Type == LoginType.Guest)
        {
            ResetTokenExpirationToNone();
            if (!LoginData.GuestLoginDataIsSaved())
            {
                const int BEGINNING_LEVEL = 1;
                implementation.StoreData(loginData.Token, new List<DataToStore> { new DataToStore(OnTheRunMcSocialApiData.Instance.levelPropertyName, BEGINNING_LEVEL, true) });
            }
            loginData.SaveGuestToPlayerPrefs();
        }
        else
        {
            SetTokenExpirationData(loginData.Expires);
            OnTheRunNotificationManager.Instance.OnLoginSuccessuful();
        }

        if (OnTheRunOmniataManager.Instance != null)
        {
            /*OnTheRunOmniataManager.Instance.TrackUser();
            OnTheRunOmniataManager.Instance.TrackMcInfo(loginData.Type.ToString(), OnTheRunFacebookManager.Instance.FriendsDictionary.Count);*/

            //Debug.Log("************************************************************************************** calling RequestTrackUser, requested: " + OnTheRunOmniataManager.Instance.trackUserWasRequested + " - RequestTrackMcInfo, requested: " + OnTheRunOmniataManager.Instance.trackMcInfoWasRequested);

            OnTheRunOmniataManager.Instance.RequestTrackUser();
            OnTheRunOmniataManager.Instance.RequestTrackMcInfo();
        }

        //isLoggedIn = true;
        FetchPlayerDataAtLogin();
    }

	void FetchPlayerDataAtLogin()
	{
		callbacksHandler.getDataSuccessCallback = OnFetchPlayerDataAtLoginSuccess;
		callbacksHandler.getDataFailCallback = OnFetchPlayerDataAtLoginFail;

		List<string> fieldsToRequest = new List<string> { OnTheRunMcSocialApiData.Instance.levelPropertyName };
		List<string> userIds = new List<string> { loginData.Id };

		implementation.GetData(loginData.Token, fieldsToRequest, userIds);
	}
	
	void OnFetchPlayerDataAtLoginSuccess(List<RetrievedData> retrievedData)
    {
		foreach (var userData in retrievedData)
		{
			foreach(var dataEntry in userData.Data)
			{
				if (dataEntry.Name.Equals(OnTheRunMcSocialApiData.Instance.levelPropertyName)
				    && userData.UserId.Equals(loginData.Id)
				    && dataEntry.Value != null)
				{
					int.TryParse(dataEntry.Value.ToString(), out lastStoredLevel);
                }

                if (dataEntry.Name.Equals(OnTheRunMcSocialApiData.Instance.experiencePropertyName)
                    && userData.UserId.Equals(loginData.Id)
                    && dataEntry.Value != null)
                {
                    int.TryParse(dataEntry.Value.ToString(), out lastStoredExperience);
                }
			}
		}
		isLoggedIn = true;

        if (loginData.Type == LoginType.Facebook && additionalFacebookLoginCallback != null)
        {
            additionalFacebookLoginCallback(true);
            additionalFacebookLoginCallback = null;
        }

        if (loginData.Type == LoginType.Facebook)
            GetFriendsLevelsForRankBar();
	}

	void OnFetchPlayerDataAtLoginFail()
	{
		lastStoredLevel = 0;
        lastStoredExperience = 0;
		isLoggedIn = true;

        if (additionalFacebookLoginCallback != null)
        {
            additionalFacebookLoginCallback(false);
            additionalFacebookLoginCallback = null;
        }
	}

    void OnLoginFail()
    {
        loginData = LoginData.InvalidLogin();
        isLoggedIn = false;

        if (additionalFacebookLoginCallback != null)
        {
            additionalFacebookLoginCallback(false);
            additionalFacebookLoginCallback = null;
        }
    }

    void OnGetFriendsSuccess(List<FriendData> friends)
    {
        this.friends = friends;

        if (OnFriendsAvailable != null)
            OnFriendsAvailable();
    }

    void OnGetFriendsFail()
    {
        this.friends = null;
    }

    void OnGetScoresSuccess(List<ScoreData> scores)
    {
        lastRequestedScores = scores;
    }

    void OnGetScoresForRankingPageSuccess(List<ScoreData> scores)
    {
        bool userScoreIsIncluded = false;
        foreach (ScoreData score in scores)
        {
            if (score.Id.Equals(UserLoginData.Id))
            {
                userScoreIsIncluded = true;
                break;
            }
        }

        if (userScoreIsIncluded)
            RequestPicturesForRankingPage(scores);
        else
        {
            callbacksHandler.getScoresSuccessCallback = OnGetMissingUserScoreForRankingPageSuccess;
            callbacksHandler.getScoresFailCallback = () => { RequestPicturesForRankingPage(scores); };

            implementation.GetScores(loginData.Token, false, lastRankingPageRequest.showFriends, lastRankingPageRequest.scoresType, 0, lastRankingPageRequest.leaderboard, lastRankingPageRequest.country);
        }
    }

    void RequestPicturesForRankingPage(List<ScoreData> scores)
    {
        List<string> facebookIds = new List<string>();
        List<string> googlePlusIds = new List<string>();

        foreach (ScoreData score in scores)
        {
            switch (score.LoginType)
            {
                case LoginType.Facebook: facebookIds.Add(score.LoginTypeId); break;
                case LoginType.Google: googlePlusIds.Add(score.LoginTypeId); break;
            }
        }

        //GetFBScoresPicturesForRankings(facebookIds);

        StartCoroutine(DownloadPicturesCoroutine(facebookIds, googlePlusIds, () => { OnPicturesForRankingsDownloaded(); }));      //StartCoroutine(DownloadPicturesForRankingPageCoroutine(facebookIds, googlePlusIds));
    }

    /*IEnumerator DownloadPicturesForRankingPageCoroutine(List<string> facebookIds, List<string> googlePlusIds)
    {
        bool facebookPicturesDownloaded = false;
        OnTheRunFacebookManager.Instance.DownloadPictures(facebookIds, () => { facebookPicturesDownloaded = true; });

        bool googlePlusPicturesDownloaded = false;
#if UNITY_EDITOR || !UNITY_ANDROID
        //googlePlusPicturesDownloaded = true;
#endif
        if (googlePlusIds.Count > 0)
            GooglePlusManager.Instance.RequestOtherUsersPicure(googlePlusIds, (wasSuccessful) => { googlePlusPicturesDownloaded = true; });
        else
            googlePlusPicturesDownloaded = true;

        while (true)
        {
            if (requestsWereAborted)
                yield break;

            if (facebookPicturesDownloaded && googlePlusPicturesDownloaded)
                break;
            else
                yield return new WaitForEndOfFrame();
        }

        OnPicturesForRankingsDownloaded();
    }*/

    void OnGetMissingUserScoreForRankingPageSuccess(List<ScoreData> scores)
    {
        ScoreData userScore = null;
        foreach (ScoreData score in scores)
        {
            if (score.Id.Equals(UserLoginData.Id))
            {
                userScore = score;
            }
        }

        if (userScore != null)
            lastRequestedScores.Add(userScore);

        RequestPicturesForRankingPage(lastRequestedScores);
    }

    void OnGetScoresForRankingPageFail()
    {
        OnReadyForRankings(new Dictionary<string, int>());
        lastRankingPageRequest = null;
    }

    void OnGetScoresForIngameSuccess(List<ScoreData> scores)
    {
        List<string> facebookIds = new List<string>();
        List<string> googlePlusIds = new List<string>();

        foreach (ScoreData score in scores)
        {
            switch (score.LoginType)
            {
                case LoginType.Facebook: facebookIds.Add(score.LoginTypeId); break;
                case LoginType.Google: googlePlusIds.Add(score.LoginTypeId); break;
            }
        }

        GetScoresPicturesForIngame(facebookIds, googlePlusIds);
    }

    void OnGetPicturesForRankBarSuccess( )
    {
        friendsPicturesForRankBarReady = true;
    }

    void OnGetScoresForRankBarSuccess(List<RetrievedData> data)
    {
        lastFriendsExperienceData = new Dictionary<string, float>();
        for (int i = 0; i < data.Count; ++i)
        {
            float friendLevel = 0;
            foreach (var field in data[i].Data)
            {
                if (field.Name.Equals(OnTheRunMcSocialApiData.Instance.levelPropertyName))
                {
                    if (field.Value != null)
                    {
                        friendLevel += int.Parse(field.Value.ToString(), CultureInfo.InvariantCulture);
                        break;
                    }
                }
            }

            foreach (var field in data[i].Data)
            {
                if (field.Name.Equals(OnTheRunMcSocialApiData.Instance.experiencePropertyName))
                {
                    if (field.Value != null)
                    {
                        int currExp = int.Parse(field.Value.ToString(), CultureInfo.InvariantCulture);
                        //Debug.Log("--Z " + friendLevel + " " + currExp + " " + PlayerPersistentData.Instance.GetExperienceThresholdByLevel((int)friendLevel));
                        friendLevel += (float)currExp / (float)PlayerPersistentData.Instance.GetExperienceThresholdByLevel((int)friendLevel);
                        break;
                    }
                }
            }

            lastFriendsExperienceData.Add(data[i].UserId, friendLevel);
        }

        friendsRequestForRankBarReady = true;
        friendsRequestForRankBarStarted = false;
    }

    void OnGetScoresFail()
    {
        lastRequestedScores = null;
    }

    void OnGetScoresForIngameFail()
    {
        friendsRequestForRankBarStarted = false;
        scoresForIngameAreReady = true;
    }

    void OnPostScoreSuccess() { }

    void OnPostScoreFail() { }

    void OnStoreDataSuccess() { }

    void OnStoreDataFail() { }

    /*void OnGetDataSuccess(RetrievedData data)
    {
        lastRetrievedData = data;
        isWaitingForData = false;
    }*/

    void OnGetDataSuccess(List<RetrievedData> data)
    {
        lastRetrievedDataList = data;
        isWaitingForData = false;
    }

    void OnGetDataFail()
    {
        lastRetrievedData = null;
        if (lastRetrievedDataList != null)
            lastRetrievedDataList.Clear();
        lastRetrievedDataList = null;

        isWaitingForData = false;
    }
    
    void OnGetRankBarDataFail()
    {
        friendsRequestForRankBarStarted = false;
    }

    void OnSendNotificationSuccess() { }

    void OnSendNotificationFail() { }

    void OnReceiveAllNotificationsSuccess(List<ReceivedNotification> notifications)
    {
        lastReceivedNotifications = notifications;
    }

    void OnReceiveAllNotificationsFail()
    {
        lastReceivedNotifications = null;
    }
    
    void OnReceiveSingleNotificationSuccess(ReceivedNotification receivedNotification)
    {
        lastReceivedNotifications = new List<ReceivedNotification> { receivedNotification };
    }

    void OnReceiveSingleNotificationSuccess_WithAdditionalData(ReceivedNotification receivedNotification, Action<ReceivedNotification> callback)
    {
        receivedNotification.SetSenderPicture(OnTheRunMcSocialApiData.Instance.defaultUserPicture);

        if (friends != null)
            GetSuccessfulNotification_AdditionalSenderData(receivedNotification, callback);
        else
            GetFriends(success => { GetSuccessfulNotification_AdditionalSenderData(receivedNotification, callback); });
    }

    void GetSuccessfulNotification_AdditionalSenderData(ReceivedNotification receivedNotification, Action<ReceivedNotification> callback)
    {
        if (friends != null)
        {
            bool senderIsFriend = false;
            FriendData friendData = new FriendData();

            foreach (var friend in friends)
            {
                if (friend.Id.Equals(receivedNotification.SenderId))
                {
                    senderIsFriend = true;
                    friendData = friend;
                    break;
                }
            }

            if (senderIsFriend)
            {
                if (friendData.Type == LoginType.Facebook && OnTheRunFacebookManager.Instance.IsLoggedIn)
                {
                    // get name
                    string friendName = receivedNotification.SenderName;
                    if (OnTheRunFacebookManager.Instance.FriendsDictionary != null)
                    {
                        if (OnTheRunFacebookManager.Instance.FriendsDictionary.ContainsKey(friendData.LoginTypeId))
                            friendName = OnTheRunFacebookManager.Instance.FriendsDictionary[friendData.LoginTypeId];
                    }
                    receivedNotification.SetSenderName(friendName);

                    // get picture
                    RequestPictures(new List<string> { friendData.LoginTypeId }, new List<String>(), () =>
                    {
                        Sprite userPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(friendData.LoginTypeId);
                        if (userPicture != null)
                            receivedNotification.SetSenderPicture(userPicture);

                        FinalizeNotificationSuccess(receivedNotification, callback);
                    });
                }
                else
                    FinalizeNotificationSuccess(receivedNotification, callback);
            }
            else
                FinalizeNotificationSuccess(receivedNotification, callback);
        }
        else
            FinalizeNotificationSuccess(receivedNotification, callback);
    }

    void FinalizeNotificationSuccess(ReceivedNotification receivedNotification, Action<ReceivedNotification> callback)
    {
        OnReceiveSingleNotificationSuccess(receivedNotification);
        callback(receivedNotification);
    }

    void OnReceiveSingleNotificationFail() { }

    void OnDeleteNotificationSuccess(ReceivedNotification deletedNotification)
    {
        lastDeletedNotification = deletedNotification;
    }

#if UNITY_EDITOR
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            OnTheRunFacebookManager.Instance.Feed("This is a test feed", () => { });
        }
    }*/
#endif

    /*void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.5f - 50.0f, 0.0f, 100.0f, 100.0f), "FEED"))
            OnTheRunFacebookManager.Instance.Feed("This is a test feed", () => { });
    }*/

    void OnDeleteNotificationFail()
    {
        lastDeletedNotification = null;
    }

    void OnGetUtcTimeSuccess(DateTime retrievedUtcTime)
    {
        Debug.Log("############################################### TIME: " + retrievedUtcTime);
    }

    void OnGetUctTimeFail()
    {
        Debug.Log("############################################### TIME FAIL");
    }

    #endregion

    public void RequestPictures(List<string> facebookIds, List<string> googlePlusIds, Action downloadCompletedCallback)
    {
        StartCoroutine(DownloadPicturesCoroutine(facebookIds, googlePlusIds, downloadCompletedCallback));
    }
    
    public void GetScoresPicturesForIngame(List<string> facebookIds, List<string> googlePlusIds)
    {
        //OnTheRunFacebookManager.Instance.DownloadPictures(facebookIds, () => { scoresForIngameAreReady = true; });

        StartCoroutine(DownloadPicturesCoroutine(facebookIds, googlePlusIds, () => { scoresForIngameAreReady = true; }));
    }


    IEnumerator DownloadPicturesCoroutine(List<string> facebookIds, List<string> googlePlusIds, Action downloadCompletedCallback)
    {
        bool facebookPicturesDownloaded = false;
        OnTheRunFacebookManager.Instance.DownloadPictures(facebookIds, () => { facebookPicturesDownloaded = true; });

        bool googlePlusPicturesDownloaded = false;
#if UNITY_EDITOR || !UNITY_ANDROID
        //googlePlusPicturesDownloaded = true;
#endif
        if (googlePlusIds.Count > 0 && GooglePlusManager.Instance != null)
            GooglePlusManager.Instance.RequestOtherUsersPicure(googlePlusIds, (wasSuccessful) => { googlePlusPicturesDownloaded = true; });
        else
            googlePlusPicturesDownloaded = true;

        while (true)
        {
            if (requestsWereAborted)
                yield break;

            if (facebookPicturesDownloaded && googlePlusPicturesDownloaded)
                break;
            else
                yield return new WaitForEndOfFrame();
        }

        downloadCompletedCallback();
    }

    /*void GetFBScoresPicturesForRankings(List<string> facebookIds)
    {
        OnTheRunFacebookManager.Instance.DownloadPictures(facebookIds, OnPicturesForRankingsDownloaded);
    }*/

    void OnPicturesForRankingsDownloaded()
    {
        if (requestsWereAborted)
            return;

        //StartCoroutine(GetLevelsForRankingsCoroutine());
        StartCoroutine(GetLevelsForRankingsUserListCoroutine());
    }

    IEnumerator GetLevelsForRankingsCoroutine()
    {
        Dictionary<string, int> levelsById = new Dictionary<string, int>();
        foreach (var score in lastRequestedScores)
        {
            lastRetrievedData = null;
            string id = score.Id;
            
            List<string> fieldsToRequest = new List<string>();
            fieldsToRequest.Add(OnTheRunMcSocialApiData.Instance.levelPropertyName);
            GetData(fieldsToRequest, id);

            while (isWaitingForData == true)
                yield return new WaitForEndOfFrame();
            
            if (lastRetrievedData == null)
                continue;

            int level = -1;
            foreach (var datum in lastRetrievedData.Data)
            {
                if (datum.Name.Equals(OnTheRunMcSocialApiData.Instance.levelPropertyName))
                {
                    if (datum.Value != null)
                        level = int.Parse(datum.Value.ToString(), CultureInfo.InvariantCulture);
                    break;
                }
            }

            levelsById.Add(id, level);
            lastRetrievedData = null;
        }

        OnReadyForRankings(levelsById);
    }


    IEnumerator GetLevelsForRankingsUserListCoroutine()
    {
        Dictionary<string, int> levelsById = new Dictionary<string, int>();

        List<string> lastRequestedScoresIds = new List<string>();
        int counter = 0;
        foreach (var score in lastRequestedScores)
        {
            lastRequestedScoresIds.Add(score.Id);
            counter++;
            if (counter >= 50)
                break;
        }
        
        List<string> fieldsToRequest = new List<string>();
        fieldsToRequest.Add(OnTheRunMcSocialApiData.Instance.levelPropertyName);

        /*bool userLevelIsAlreadyAvailable = lastRequestedScoresIds.Contains(UserLoginData.Id) || levelsById.ContainsKey(UserLoginData.Id);
        if (!userLevelIsAlreadyAvailable)
            lastRequestedScoresIds.Add(UserLoginData.Id);*/

        GetData(fieldsToRequest, lastRequestedScoresIds);

        while (isWaitingForData == true)
            yield return new WaitForEndOfFrame();

        if (lastRetrievedDataList != null)
        {
            int level = -1;
            foreach (var userData in lastRetrievedDataList)
            {
                string id = userData.UserId;

                foreach (var field in userData.Data)
                {
                    if (field.Name.Equals(OnTheRunMcSocialApiData.Instance.levelPropertyName))
                    {
                        if (field.Value != null)
                            level = int.Parse(field.Value.ToString(), CultureInfo.InvariantCulture);
                        break;
                    }
                }

                levelsById.Add(id, level);
            }

            lastRetrievedDataList = null;
        }

		bool userLevelIsAlreadyAvailable = lastRequestedScoresIds.Contains(UserLoginData.Id) || levelsById.ContainsKey(UserLoginData.Id);
        //Debug.Log("################################ User Level is available? ..." + userLevelIsAlreadyAvailable);
        if (!userLevelIsAlreadyAvailable)
        {
            GetData(fieldsToRequest, new List<string> { UserLoginData.Id });

            while (isWaitingForData == true)
                yield return new WaitForEndOfFrame();

            if (lastRetrievedDataList != null)
            {
                int level = -1;
                foreach (var userData in lastRetrievedDataList)
                {
                    if (userData.UserId.Equals(UserLoginData.Id))
                    {
                        foreach (var field in userData.Data)
                        {
                            if (field.Name.Equals(OnTheRunMcSocialApiData.Instance.levelPropertyName))
                            {
                                if (field.Value != null)
                                    level = int.Parse(field.Value.ToString(), CultureInfo.InvariantCulture);
                                break;
                            }
                        }
                        levelsById.Add(UserLoginData.Id, level);
                    }
                }
                lastRetrievedDataList = null;
            }
        }

        OnReadyForRankings(levelsById);
    }

    /*void OnPicturesDownloaded()
    {/*
        List<Texture2D> TEST_pictureList = new List<Texture2D>();

        // Add My fb picture
        if (UserLoginData.Type == LoginType.Facebook && OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            Texture2D userPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(OnTheRunFacebookManager.Instance.UserId);
            Debug.Log("############################ user Picture is null ? " + (userPicture == null).ToString());
            TEST_pictureList.Add(userPicture);
        }* /

        /*
        if (lastRequestedScores != null)
        {
            foreach (var score in lastRequestedScores)
            {
                if (score.LoginType == LoginType.Facebook)
                {
                    Texture2D picture = OnTheRunFacebookManager.Instance.GetUsersPicture(score.LoginTypeId);
                    score.SetPicture(picture);
                    //if (picture != null)
                    //    TEST_pictureList.Add(picture);
                }
            }
        }
        * /

        OnReadyForRankings();

        /*
        Debug.Log("############### On Pictures Downloaded - TEST_pictureList.Count: " + TEST_pictureList.Count);
        OnTheRun_TEST_SocialNetworksLogin.Instance.pictures = TEST_pictureList;
        * /
    }*/

    void OnReadyForRankings(Dictionary<string, int> levels)
    {
        //Debug.Log("                                           >>>>>>>>>>>>>>>> OnReadyForRankings - setting timeout flag to TRUE");
        getsScoresResultBeforeTimeout = true;

        UIPage page = Manager<UIManager>.Get().ActivePage;
        if (page != null)
        {
            UIRankingsPage rankingsPage = page.GetComponent<UIRankingsPage>();
            if (rankingsPage != null && rankingsPage.gameObject.activeInHierarchy)
                rankingsPage.OnRankingsAvailable(levels);
        }
    }

    public static void ScaleAvatarSpriteRenderer(SpriteRenderer rend)
    {
        if (rend == null || rend.sprite == null)
            return;

        const float DESIRED_PIXEL_SIZE = 58.0f;

        float widthScale = DESIRED_PIXEL_SIZE / (float)rend.sprite.rect.width;
        float heightScale = DESIRED_PIXEL_SIZE / (float)rend.sprite.rect.height;

        rend.transform.localScale = new Vector3(widthScale, heightScale, 1.0f);
    }

    void Update()
    {
        if (shouldRefreshToken)
        {
            if (Time.realtimeSinceStartup - lastTokenCheckRealtimeSinceStartup >= refreshPeriodInSeconds)
                DoTokenExpirationCheck();
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused && shouldRefreshToken)
            DoTokenExpirationCheck();
    }

    void DoTokenExpirationCheck()
    {
        DateTime utcNow;
        bool isDateValid;
        DateTimeManager.Instance.GetDate(out utcNow, out isDateValid);

        //Debug.Log("@#@#@#@#@#@#@#@ DOING TOKEN EXPIRATION CHECK \n now: " + utcNow + "\n nextTime: " + utcNow.AddSeconds(refreshPeriodInSeconds) + "\n tokenExpirationDate: " + tokenExpirationDate);

        if (utcNow.AddSeconds(refreshPeriodInSeconds) >= tokenExpirationDate)
            RequestTokenRefresh();

        lastTokenCheckRealtimeSinceStartup = Time.realtimeSinceStartup;
    }

    void RequestTokenRefresh()
    {
        //Debug.Log("@#@#@#@#@#@#@#@ TRIGGERING REFRESH REQUEST.");

        if (loginData.Type == LoginType.Facebook && OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            callbacksHandler.loginSuccessCallback = OnTokenRefreshSuccess;
            callbacksHandler.loginFailCallback = OnTokenRefreshFail;

            implementation.Login(apiKey, LoginType.Facebook, OnTheRunFacebookManager.Instance.Token);
        }
    }

    void OnTokenRefreshSuccess(LoginData data)
    {
        //Debug.Log("@#@#@#@#@#@#@#@ OnTokenRefreshSuccess");

        loginData.RefreshToken(data.Token);

        SetTokenExpirationData(data.Expires);
        lastTokenCheckRealtimeSinceStartup = Time.realtimeSinceStartup;
    }

    void OnTokenRefreshFail()
    {
        //Debug.Log("@#@#@#@#@#@#@#@ OnTokenRefreshFail - tries: " + tokenRefreshRetiresCounter);

        tokenRefreshRetiresCounter++;
        if (tokenRefreshRetiresCounter <= numTokenRefreshRetriesOnFail)
            StartCoroutine(TokenRefreshRetry());
    }

    IEnumerator TokenRefreshRetry()
    {
        yield return new WaitForSeconds(tokenRefreshRetriesDelaySeconds);

        RequestTokenRefresh();
        lastTokenCheckRealtimeSinceStartup = Time.realtimeSinceStartup;
    }
}