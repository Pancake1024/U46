using UnityEngine;
using System.Collections;
using SBS.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class OnTheRunFacebookManager : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunFacebookManager instance = null;

    public static OnTheRunFacebookManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern string getFacebookAppId();
#endif

    public class InvitableFriend
    {
        string id;
        string name;
        string pictureUrl;
        Sprite picture;

        public string Id { get { return id; } }
        public string Name { get { return name; } }
        public string PictureUrl { get { return pictureUrl; } }
        public Sprite Picture { get { return picture; } set { picture = value; } }
        
        public InvitableFriend(string id, string name, string pictureUrl)
        {
            this.id = id;
            this.name = name;
            this.pictureUrl = pictureUrl;
        }

        public override string ToString()
        {
            string str = "Invitable friend:\n";
            str += "\t - id: " + id + "\n";
            str += "\t - name: " + name + "\n";
            str += "\t - pictureUrl: " + pictureUrl;
            str += "\t - picture: " + (picture != null ? "Available" : "Not available");

            return str;
        }
    }

    public string AppId
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			return getFacebookAppId();
#elif UNITY_WP8
            return "768589246509208"; // OTR_SBS
            //return "583129888466228";   // OnTheRun (Miniclip)
#else
            return FBSettings.AppId;  //pelle cheat FBSettings.AppId; //"768589246509208";
#endif
        }
    }

    string StoreName
    {
        get
        {
#if UNITY_EDITOR
            return "Editor";
#elif UNITY_IPHONE
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

    string BitLyLink
    {
        get
        {
#if UNITY_EDITOR
            return "EditorBitLy";
#elif UNITY_IPHONE
            return "http://bit.ly/Ih1X6y";
#elif UNITY_ANDROID && !UNITY_KINDLE
            return "http://bit.ly/19BVMRv";
#elif UNITY_ANDROID && UNITY_KINDLE
            return "http://amzn.to/1e5nAVT";
#else
            return "BitLy Link";
#endif
        }
    }

    //const string FbFeedPictureLink = "http://static.miniclipcdn.com/iphone/assets/hotrod/hotrod_icon.png";
    //const string FbFeedLink = "https://www.facebook.com/pages/On-The-Run/251185165078080";

    public bool IsInitialized { get { return implementation.IsInitialized; } }
    public bool IsLoggedIn { get { return implementation.IsLoggedIn; } }
    public string UserId { get { return implementation.UserId; } }
    public string Token { get { return implementation.AccessToken; } }
    public string Country { get { return implementation.Country; } }
    public string Gender { get { return implementation.Gender; } }
    public string Birthday { get { return implementation.Birthday; } }
    public string UserName { get { return implementation.Name; } }
    public string MeAndFriendsIdxsCommaSeparated { get { return implementation.MeAndFriendsIdsCommaSeparated; } }
    public Sprite UserPicture { get { return implementation.UserPicture; } }
    public Dictionary<string, string> FriendsDictionary { get { return implementation.FriendsDictionary; } }

    bool isInitInProgress;
    public bool IsInitInProgress { get { return isInitInProgress; } }

    public bool AreInvitablePicturesReady { get { return implementation.AreInvitablePicturesReady; } }

    FacebookImplementation implementation;

    void Awake()
    {
        Asserts.Assert(instance == null);
        instance = this;
        DontDestroyOnLoad(gameObject);

        SetImplementation();

        isInitInProgress = false;
    }

    void OnDestroy()
    {
        Asserts.Assert(instance != null);
        instance = null;
    }

    void SetImplementation()
    {
#if UNITY_WEBPLAYER || (UNITY_WP8 && UNITY_EDITOR)
        implementation = gameObject.AddComponent<FakeFacebookImplementation>();
#elif UNITY_WP8 && !UNITY_EDITOR
        implementation = gameObject.AddComponent<P31Wp8FacebookImplementation>();
#else
        implementation = gameObject.AddComponent<UnitySdkFacebookImplementation>();
#endif
    }

    public void InitFacebook()
    {
        isInitInProgress = true;

        StartCoroutine(InitializationTimeoutCoroutine());

        implementation.InitFb(
            AppId,
            () =>
            {
                //Debug.Log("########################### Facebook init and login is successful");

                OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, null);

                InitializationFinished();
                /*
                isInitInProgress = false;
                
                if (Manager<UIManager>.Get().ActivePageName == "StartPage")
                {
                    UIStartPage startPageComponent = Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>();
                    startPageComponent.UpdateFacebookButtonText();
                    startPageComponent.HideLoadingPopup();
                }*/
            },
            () =>
            {
                //Debug.Log("########################### Facebook init but no login...");

                //McSocialApiManager.Instance.LoginAsGuest();

                InitializationFinished();
                /*
                isInitInProgress = false;

                if (Manager<UIManager>.Get().ActivePageName == "StartPage")
                {
                    UIStartPage startPageComponent = Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>();
                    startPageComponent.UpdateFacebookButtonText();
                    startPageComponent.HideLoadingPopup();
                }*/
            }
        );
    }

    IEnumerator InitializationTimeoutCoroutine()
    {
        const float TIMEOUT_SECONDS = 4.0f; // 10.0f

        float timeBefore = TimeManager.Instance.MasterSource.TotalTime;

        while (TimeManager.Instance.MasterSource.TotalTime - timeBefore < TIMEOUT_SECONDS)
        {
            if (implementation.IsInitialized)
                yield break;

            yield return new WaitForEndOfFrame();
        }

        if (!implementation.IsInitialized)
            InitializationFinished();
    }

    void InitializationFinished()
    {
        isInitInProgress = false;
        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- FacebookManager InitializationFinished -> active page: " + Manager<UIManager>.Get().ActivePageName);

        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
        {
            UIStartPage startPageComponent = Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>();
            startPageComponent.UpdateFacebookButtonText();
            startPageComponent.HideLoadingPopup();
        }
    }

    public void Login(Action successCallback, Action failCallback, Action errorCallback)
    {
        if (!implementation.IsInitialized)
        {
            errorCallback();
            return;
        }

        StartCoroutine(LoginTimeoutCoroutine(errorCallback));

        implementation.Login("user_friends"/*",user_birthday"*/, successCallback, failCallback, errorCallback);
    }

    IEnumerator LoginTimeoutCoroutine(Action errorCallback)
    {
        const float TIMEOUT_SECONDS = 4.0f;

        float timeBefore = TimeManager.Instance.MasterSource.TotalTime;

        while (TimeManager.Instance.MasterSource.TotalTime - timeBefore < TIMEOUT_SECONDS)
        {
            if (implementation.IsLoggedIn)
                yield break;

            yield return new WaitForEndOfFrame();
        }

        if (!implementation.IsLoggedIn)
            errorCallback();
    }

    public void Logout()
    {
        if (!implementation.IsInitialized)
            return;

        implementation.Logout();
    }

    public void GetFacebookFriends()
    {
        if (!implementation.IsLoggedIn)
            return;

        implementation.GetFriends();
    }

    public void DownloadPictures(List<string> userIds, Action picturesReadyCallback)
    {
        //Debug.Log("################ Facebook Manager - DownloadPictures - implementation.IsInitialized: " + implementation.IsInitialized);
        if (!implementation.IsInitialized)
        {
            picturesReadyCallback();
            return;
        }

        implementation.DownloadPictures(userIds, picturesReadyCallback);
    }

    public Sprite GetUsersPicture(string facebookId)
    {
        if (!implementation.IsInitialized)
            return null;

        return implementation.GetUsersPicture(facebookId);
    }


    public void ClearPictures()
    {
        implementation.ClearPictures();
    }

    public void Test_PrintNumPictures()
    {
        implementation.Test_PrintNumPictures();
    }

    public void Feed(string feedMessage, Action<bool> callback)
    {
        if (!implementation.IsInitialized)
        {
            callback(false);
            return;
        }

        //Dictionary<string, string[]> propertiesDict = new Dictionary<string, string[]>();
        //propertiesDict.Add("On " + StoreName, new string[] { BitLyLink, BitLyLink });

        string linkName = OnTheRunDataLoader.Instance.GetLocaleString("on_the_run");
        string linkCaption = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_caption");
        string pageLink = OnTheRunDataLoader.Instance.GetFacebookPageLink();

        string pictureLink = "http://static.miniclipcdn.com/images/smartphone/icons/ontherunicon.png";

        implementation.Feed(string.Empty,	// toId
		                    pageLink,		// link	//old: FbFeedLink,
		                    linkName,		// linkName
		                    linkCaption,	// linkCaption
		                    feedMessage,	// linkDescription
		                    pictureLink,	// picture
		                    string.Empty,	// mediaSource
		                    linkName,		// actionName
		                    pageLink,		// actionLink // old: FbFeedLink,
		                    string.Empty,	// reference
                            null,   		// properties // old: propertiesDict,
                            callback);
    }


    public void FeedToUserId(string userId, string feedMessage, Action<bool> callback)
    {
        if (!implementation.IsInitialized)
        {
            callback(false);
            return;
        }

        //Dictionary<string, string[]> propertiesDict = new Dictionary<string, string[]>();
        //propertiesDict.Add("On " + StoreName, new string[] { BitLyLink, BitLyLink });

        string linkName = OnTheRunDataLoader.Instance.GetLocaleString("on_the_run");
        string linkCaption = OnTheRunDataLoader.Instance.GetLocaleString("facebook_feed_caption");
        string pageLink = OnTheRunDataLoader.Instance.GetFacebookPageLink();

        string pictureLink = "http://static.miniclipcdn.com/images/smartphone/icons/ontherunicon.png";

        implementation.Feed(userId,
                            pageLink,		//FbFeedLink,
                            linkName,
                            linkCaption,
                            feedMessage,
                            pictureLink,
                            string.Empty,	// Media Source
		                    linkName,	    // Action Name
                            pageLink,		// FbFeedLink,
                            string.Empty,	// Reference
                            null,   		// propertiesDict,
                            callback);
    }

    public bool UserIsFriend(string userId)
    {
        if (!implementation.IsInitialized || !implementation.IsLoggedIn)
            return false;

        return implementation.UserIsFriend(userId);
    }

    public void RequestInvitableFriends(Action<List<InvitableFriend>> callback)
    {
        if (!implementation.IsInitialized || !implementation.IsLoggedIn)
            callback(null);

        implementation.RequestInvitableFriends(callback);
    }

    public void InviteFriend(InvitableFriend friend, Action<bool, string, string> callback)
    {
        if (!implementation.IsInitialized || !implementation.IsLoggedIn)
            callback(false, string.Empty, string.Empty);

        implementation.InviteFriend(friend, callback);
    }

    /*
	int test_event_counter = 0;
	public void Test_LogEvent(string eventName)
	{
		// Log only in internal application - it's just a test
		if (AppId == "768589246509208") // OTR_SBS
		{
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add ("test_param_counter", test_event_counter++);
			implementation.LogEvent(eventName, parameters);
		}
	}*/
}