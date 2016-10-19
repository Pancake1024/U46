#if UNITY_WP8
using UnityEngine;
using System.Collections;
using Prime31;
using Prime31.WinPhoneSocialNetworking;
using System;
using System.Collections.Generic;

public class P31Wp8FacebookImplementation : MonoBehaviour, FacebookImplementation
{
    public bool IsInitialized { get { return isInitialized; } }
    public bool IsLoggedIn { get { return FacebookAccess.isSessionValid(); } }    // isLoggedIn; } }
    public string UserId { get { return userId; } }
    public string AccessToken { get { return FacebookAccess.accessToken; } } //accessToken; } }
    public string Country { get { return locale; } }
    public string Gender { get { return gender; } }
    public string Birthday { get { return birthday; } }
    public string Name { get { return name; } }
    public string MeAndFriendsIdsCommaSeparated { get { return string.Empty; } }
    public Sprite UserPicture { get { return userPicture; } }
    public Dictionary<string, string> FriendsDictionary { get { return friendsById; } }

    public bool AreInvitablePicturesReady { get { return areInvitablePicturesReady; } }

    const string APP_SECRET = "decde6c8a86112d0881570f6752ed6af";   // OTR_SBS
    //const string APP_SECRET = "f990258a8950ca18976d4d26e1a3b853";   // OnTheRun (Miniclip)

    const string REDIRECT_URL = "https://m.facebook.com/dialog/return/ms";   //"http://prime31.com/facebook";

    const int PICTURE_RESOLUTION = 58;
    const string WP8_FACEBOOK_GAMEOBJECT_NAME = "WinPhoneFacebook";
    GameObject wp8FbGo;
    MonoBehaviourGUI wp8FbComponent;

    bool isInitialized = false;
    //bool isLoggedIn = false;
    string userId = string.Empty;
    //string accessToken = string.Empty;
    string locale = string.Empty;
    string gender = string.Empty;
    string birthday = string.Empty;
    string name = string.Empty;
    //string meAndFriendsIdsCommaSeparated  = string.Empty;
    Sprite userPicture = null;
    Dictionary<string, string> friendsById;
    Dictionary<string, Sprite> picturesById;
    Queue<string> lruPictureIds;

    Action facebookLoginSuccessCallback;
    Action facebookLoginFailCallback;
    Action loginErrorCallback;
    bool meApiCallIsCompleted;
    bool myPictureApiCallIsCompleted;
    bool getFriendsCallCompleted;
    bool getInvitableFriendsCallCompleted;

    List<string> requestedPictureIds;
    bool lastRequestedPictureReceived;
    string lastRequestedPictureId;

    List<OnTheRunFacebookManager.InvitableFriend> invitableFriends;
    bool areInvitablePicturesReady;

    void Awake()
    {
        ResetCachedData();
        SetWp8FbGameObjectAndComponent();
    }

    void ResetCachedData()
    {
        isInitialized = false;
        //isLoggedIn = false;
        userId = string.Empty;
        //accessToken = string.Empty;
        locale = string.Empty;
        gender = string.Empty;
        birthday = string.Empty;
        name = string.Empty;
        userPicture = null;
        friendsById = new Dictionary<string, string>();
        picturesById = new Dictionary<string, Sprite>();
        lruPictureIds = new Queue<string>();

        requestedPictureIds = new List<string>();
        lastRequestedPictureReceived = false;
        lastRequestedPictureId = string.Empty;

        meApiCallIsCompleted = false;
        myPictureApiCallIsCompleted = false;
        getFriendsCallCompleted = false;
        getInvitableFriendsCallCompleted = false;

        invitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();
        areInvitablePicturesReady = true;
    }

    void SetWp8FbGameObjectAndComponent()
    {
        //Debug.Log("### ### ### SetWp8FbGameObjectAndComponent");

        wp8FbGo = GameObject.Find(WP8_FACEBOOK_GAMEOBJECT_NAME);
        if (wp8FbGo == null)
            wp8FbGo = new GameObject(WP8_FACEBOOK_GAMEOBJECT_NAME);

        wp8FbComponent = wp8FbGo.GetComponent<MonoBehaviourGUI>();
        if (wp8FbComponent == null)
            wp8FbComponent = wp8FbGo.AddComponent<MonoBehaviourGUI>();

        //EnableHighDetailLogs();
    }

    void EnableHighDetailLogs()
    {
        // optionally enable debugging of requests
        //Facebook.instance.debugRequests = true;
    }

    #region FacebookImplementation Interface Methods
    public void InitFb(string fbAppId, Action initWithSuccessfulLoginCallback, Action initWithoutLoginCallback)
    {
        //Debug.Log("### ### ### InitFb - fbAppId: " + fbAppId);

        facebookLoginSuccessCallback = initWithSuccessfulLoginCallback;
        facebookLoginFailCallback = initWithoutLoginCallback;
        loginErrorCallback = initWithoutLoginCallback;

        FacebookAccess.applicationId = fbAppId;
        //SetWp8FbGameObjectAndComponent();

        //Debug.Log("### ### ### InitFb - about to call prepareForMetroUse");
        Facebook.instance.prepareForMetroUse(wp8FbGo, wp8FbComponent);

        //Debug.Log("### ### ### InitFb - Is Session Valid? " + FacebookAccess.isSessionValid());

        isInitialized = true;

        if (FacebookAccess.isSessionValid())
        {
            //Debug.Log("### ### ### InitFb - about to start LoginSuccessCoroutine");
            StartCoroutine(LoginSuccessCoroutine());
        }
        else
        {
            //Debug.Log("### ### ### InitFb - about to call LoginFail -> initWithoutLoginCallback");
            LoginFail();
        }
    }

    public void Login(string permissions, Action successCallback, Action failCallback, Action errorCallback)
    {
        //Debug.Log("### ### ### Login - permissions: " + permissions);

        PushBlockButtonPopup();

        facebookLoginSuccessCallback = successCallback;
        facebookLoginFailCallback = failCallback;
        loginErrorCallback = errorCallback;

        meApiCallIsCompleted = false;
        myPictureApiCallIsCompleted = false;
        getFriendsCallCompleted = false;
        getInvitableFriendsCallCompleted = false;

        //string[] permissionsArray = permissions.Split(',');
        var permissionsArray = new string[] { "user_friends" };

        FacebookAccess.login(REDIRECT_URL, APP_SECRET, permissionsArray, (token, error) =>
        {
            //Debug.Log("### ### ### Login - callback - token: " + token + " - error: " + error);

            if (error != null)
            {
                Debug.Log("Error logging in: " + error);
                LoginError();
            }
            else
            {
                //isLoggedIn = FacebookAccess.isSessionValid();
                //accessToken = token;

                //Debug.Log("### ### ### Login - callback - FacebookAccess.isSessionValid(): " + FacebookAccess.isSessionValid());
                if (FacebookAccess.isSessionValid())//isLoggedIn)
                    StartCoroutine(LoginSuccessCoroutine());
                else
                    LoginFail();
            }
        });
    }

    public void Logout()
    {
        //Debug.Log("### ### ### Logout");

        //if (isLoggedIn)
            FacebookAccess.logout();

        ResetCachedData();
    }

    public void GetFriends()
    {
        Facebook.instance.get("me/friends", GetFriendsCallback);
    }

    public void Feed(string toId = "",
                     string link = "",
                     string linkName = "",
                     string linkCaption = "",
                     string linkDescription = "",
                     string picture = "",
                     string mediaSource = "",
                     string actionName = "",
                     string actionLink = "",
                     string reference = "",
                     Dictionary<string, string[]> properties = null,
                     Action<bool> callback = null)
    {
        var parameters = new Dictionary<string, object>
			{
				{ "link", link },
				{ "name", linkName },
				{ "caption", linkCaption },
				{ "description", linkDescription },
				{ "picture", picture }
			};
        //Facebook.instance.post("me/feed", parameters, (a, b) => { callback(); });

        PushBlockButtonPopup();

        FacebookAccess.showDialog("stream.publish", parameters, dialogResultUrl => { PushBlockButtonPopup(); callback(true); });
    }


    /*public void Feed(string toId = "",
                     string link = "",
                     string linkName = "",
                     string linkCaption = "",
                     string linkDescription = "",
                     string picture = "",
                     string mediaSource = "",
                     string actionName = "",
                     string actionLink = "",
                     string reference = "",
                     Dictionary<string, string[]> properties = null,
                     Action<bool> callback = null)
    {
        Facebook.instance.postMessageWithLinkAndLinkToImage(linkDescription, link, linkName, picture, linkCaption, (str, obj) =>
        {
            Debug.Log("#-#-#----#-#-# Prime31 Facebook - string: " + str);
            callback(true);
        });
    }*/

    public void DownloadPictures(List<string> userIds, Action picturesReadyCallback)
    {
        //Debug.Log("### ### ### DownloadPictures");

        if (userIds == null || userIds.Count == 0)
        {
            //Debug.Log("### ### ### DownloadPictures - no userIds were requested - calling picturesReadyCallback");

            picturesReadyCallback();
            return;
        }

        //Debug.Log("### ### ### DownloadPictures - userIds.Count: " + userIds.Count);

        requestedPictureIds = new List<string>();
        foreach (string id in userIds)
            if (!picturesById.ContainsKey(id))
                requestedPictureIds.Add(id);

        if (requestedPictureIds.Count > 0)
            StartCoroutine(DownloadPicturesCoroutine(picturesReadyCallback));
        else
            picturesReadyCallback();
    }

    public Sprite GetUsersPicture(string facebookId)
    {
        if (picturesById.ContainsKey(facebookId))
            return picturesById[facebookId];
        else
            return null;
    }

    public void ClearPictures()
    {
        if (picturesById != null)
            picturesById = new Dictionary<string, Sprite>();
    }

    public void Test_PrintNumPictures()
    {
        return;
    }

    public bool UserIsFriend(string userId)
    {
        return friendsById.ContainsKey(userId);
    }

    public void RequestInvitableFriends(Action<List<OnTheRunFacebookManager.InvitableFriend>> invitableFriendsReadyCallback)
    {
        StartCoroutine(InvitableFriendsRequestCoroutine(invitableFriendsReadyCallback));
    }

    IEnumerator InvitableFriendsRequestCoroutine(Action<List<OnTheRunFacebookManager.InvitableFriend>> invitableFriendsReadyCallback)
    {
        while (!getInvitableFriendsCallCompleted)
            yield return new WaitForEndOfFrame();

        invitableFriendsReadyCallback(invitableFriends);
    }

    const string INVITE_TEXT = "Click Here to play On The Run"; //LOCALIZATION
    const string INVITE_TITLE = "You are invited";              //LOCALIZATION

    const string TAIL_TO_REMOVE = "#_=_";
    const string HEADER_REQUEST = "request=";
    const string HEADER_TO_FIRST = "to[0]=";
    const string HEADER_TO_ANY = "to[";

    public void InviteFriend(OnTheRunFacebookManager.InvitableFriend friend, Action<bool, string, string> callback)
    {
        //Invite(new List<string> { friend.Id }, callback);

        List<string> ids = new List<string> { friend.Id };

        var parameters = new Dictionary<string, object>
			{
                { "to", friend.Id },
                { "app_id", OnTheRunFacebookManager.Instance.AppId },
                { "message", INVITE_TEXT },
                { "title", INVITE_TITLE}
			};

        PushBlockButtonPopup();

        FacebookAccess.showDialog("apprequests", parameters, dialogResultUrl =>
        {
            PopBlockButtonPopup();

            string debugStr = "#-#-#----#-#-# invite callback\n" + dialogResultUrl;
            
            // Example of dialogRequestUrl:
            // ?request=373391242819117&to[0]=1569435489943255#_=_

            string requestId = string.Empty;
            string recipientId = string.Empty;
            bool success = false;

            dialogResultUrl = dialogResultUrl.TrimStart('?');
            if (dialogResultUrl.EndsWith(TAIL_TO_REMOVE))
                dialogResultUrl = dialogResultUrl.Substring(0, dialogResultUrl.LastIndexOf(TAIL_TO_REMOVE));

            var resultsStrings = dialogResultUrl.Split('&');
            foreach (string str in resultsStrings)
            {
                if (str.StartsWith(HEADER_REQUEST))
                    requestId = str.Substring(HEADER_REQUEST.Length);

                if (str.StartsWith(HEADER_TO_FIRST))
                {
                    recipientId = str.Substring(HEADER_TO_FIRST.Length);

                    if (recipientId.IndexOf(HEADER_TO_ANY) > 0)
                        recipientId = recipientId.Substring(0, recipientId.IndexOf(HEADER_TO_ANY));
                }
            }

            if (!string.IsNullOrEmpty(requestId) && !string.IsNullOrEmpty(recipientId))
                success = true;

            debugStr += "\nrequestId: " + requestId + "\nrecipientId: " + recipientId + "\nsuccess: " + success;
            Debug.Log(debugStr);

            callback(success, recipientId, friend.Name);
        });
    }

    /*public void InviteAllFriends(Action<bool> callback)
    {
        if (invitableFriends == null || invitableFriends.Count == 0)
        {
            callback(false);
            return;
        }

        List<string> ids = new List<string>();
        foreach (var f in invitableFriends)
            ids.Add(f.Id);

        Invite(ids, callback);
    }*/
    #endregion
    
    #region Login Methods
    IEnumerator LoginSuccessCoroutine()
    {
        //Debug.Log("### ### ### LoginSuccessCoroutine - about to call me method");

        Facebook.instance.get("me", ApiMeCallback);

        //Facebook.instance.get(FacebookUtil.GetPictureURL("me", PICTURE_RESOLUTION, PICTURE_RESOLUTION), ApiMyPictureCallback);
        //Facebook.instance.fetchProfileImageForUserId(userId, ApiMyPictureCallback);

        GetFriends();
        GetInvitableFriends();

        while (!meApiCallIsCompleted || !myPictureApiCallIsCompleted || !getFriendsCallCompleted)
            yield return new WaitForEndOfFrame();

        //Debug.Log("### ### ### LoginSuccessCoroutine - both meApiCallIsCompleted and myPictureApiCallIsCompleted are true - about to call LoginSuccess()");
        LoginSuccess();
    }

    void ApiMeCallback(string error, object result)
    {
        //Debug.Log("### ### ### ApiMeCallback - error: " + error + " - result is null: " + (result == null).ToString());

        userId = string.Empty;
        locale = string.Empty;
        gender = string.Empty;
        birthday = string.Empty;
        name = string.Empty;

        if (string.IsNullOrEmpty(error) && result != null)
        {
            Debug.Log("----------------------------------- FACEBOOK API ME: " + Json.encode(result));

            Dictionary<string, object> responseDict = result as Dictionary<string, object>;

            if (responseDict.ContainsKey("id"))
                userId = (string)responseDict["id"];

            if (responseDict.ContainsKey("locale"))
                locale = (string)responseDict["locale"];

            if (responseDict.ContainsKey("gender"))
                gender = (string)responseDict["gender"];

            if (responseDict.ContainsKey("birthday"))
            {
                // Birthday:"09\/20\/1987" -> "DD/MM/YYYY"
                birthday = (string)responseDict["birthday"];

                string[] splitBirthday = birthday.Split('/');
                if (splitBirthday.Length >= 3)
                {
                    string birthdayYear = splitBirthday[2];
                    string birthdayMonth = splitBirthday[1];
                    string birthdayDay = splitBirthday[0];
                    birthday = birthdayYear + "-" + birthdayMonth + "-" + birthdayDay;
                }
            }

            if (responseDict.ContainsKey("name"))
                name = (string)responseDict["name"];
        }
        else
            Debug.Log("ApiMeCallback ERROR: " + error);

        meApiCallIsCompleted = true;

        //Debug.Log("### ### ### ApiMeCallback - about to request user's picture");
        Facebook.instance.fetchProfileImageForUserId(userId, ApiMyPictureCallback);
    }

    /*void ApiMyPictureCallback(string error, object result)
    {
        Sprite picture = null;

        if (result is Texture2D)
        {
            // TODO...
            Texture2D texture = result as Texture2D;
            if (!string.IsNullOrEmpty(error))
                Debug.Log("ApiMyPictureCallback ERROR: " + error);
            else
                picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
            Debug.Log("ApiMyPictureCallback no Texture2D");

        userPicture = picture;

        myPictureApiCallIsCompleted = true;
    }*/
    
    void ApiMyPictureCallback(Texture2D texture)
    {
        //Debug.Log("### ### ### ApiMyPictureCallback - texture is null? " + (texture == null).ToString());

        Sprite picture = null;

        if (texture != null)
            picture = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        else
            Debug.Log("ApiMyPictureCallback texture is null");

        userPicture = picture;

        myPictureApiCallIsCompleted = true;
    }

    void LoginSuccess()
    {
        PopBlockButtonPopup();

        facebookLoginSuccessCallback();

        facebookLoginSuccessCallback = null;
        facebookLoginFailCallback = null;
        loginErrorCallback = null;
    }

    void LoginFail()
    {
        PopBlockButtonPopup();

        facebookLoginFailCallback();

        facebookLoginSuccessCallback = null;
        facebookLoginFailCallback = null;
        loginErrorCallback = null;
    }

    void LoginError()
    {
        PopBlockButtonPopup();

        loginErrorCallback();

        facebookLoginSuccessCallback = null;
        facebookLoginFailCallback = null;
        loginErrorCallback = null;
    }
    #endregion

    #region GetFriends Methods
    void GetFriendsCallback(string error, object result)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.Log("GetFriendsCallback ERROR: " + error);
            return;
        }

        Dictionary<string, object> responseDict = result as Dictionary<string, object>;
        if (!responseDict.ContainsKey("data"))
        {
            //Debug.Log("The response dictionary does not contain the key: \"data\"");
            return;
        }

        List<object> dataArray = responseDict["data"] as List<object>;
        List<string> friendIds = new List<string>();
        foreach (Dictionary<string, object> friendDict in dataArray)
        {
            if (!friendDict.ContainsKey("name") || !friendDict.ContainsKey("id"))
            {
                //Debug.Log("Invalid friend data");
                continue;
            }

            string name = (string)friendDict["name"];
            string id = (string)friendDict["id"];

            if (friendsById.ContainsKey(id))
                friendsById[id] = name;
            else
                friendsById.Add(id, name);

            friendIds.Add(id);
        }

        getFriendsCallCompleted = true;
    }

    public void GetInvitableFriends()
    {
        Facebook.instance.get("/me/invitable_friends", GetInvitableFriendsCallback);
    }

    void GetInvitableFriendsCallback(string error, object result)
    {
        invitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();

        if (!string.IsNullOrEmpty(error))
        {
            Debug.Log("GetInvitableFriendsCallback ERROR: " + error);
            return;
        }

        //StartCoroutine(ProcessInvitableFriends_WithPictureDownload(result));
        ProcessInvitableFriends(result);
    }

    void ProcessInvitableFriends(object result)
    {
        Dictionary<string, object> responseDict = result as Dictionary<string, object>;

        List<object> dataArray = new List<object>();
        if (responseDict.ContainsKey("data"))
            dataArray = responseDict["data"] as List<object>;

        foreach (Dictionary<string, object> friendDict in dataArray)
        {
            string id = (string)friendDict["id"];
            string name = (string)friendDict["name"];

            Dictionary<string, object> pictureDict = friendDict["picture"] as Dictionary<string, object>;
            Dictionary<string, object> dataDictionary = pictureDict["data"] as Dictionary<string, object>;
            string pictureUrl = (string)dataDictionary["url"];

            invitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend(id, name, pictureUrl));
        }

        StartCoroutine(DownloadInvitableFriendsPictures());

        getInvitableFriendsCallCompleted = true;
    }

    IEnumerator DownloadInvitableFriendsPictures()
    {
        if (invitableFriends == null || invitableFriends.Count <= 0)
            yield break;

        areInvitablePicturesReady = false;

        foreach (var friend in invitableFriends)
        {
            WWW wwwRequest = new WWW(friend.PictureUrl);
            yield return wwwRequest;

            Sprite picture = null;
            if (wwwRequest.error != null)
                Debug.Log(wwwRequest.error);
            else
            {
                Texture2D tex = wwwRequest.textureNonReadable;
                if (tex != null)
                    picture = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            friend.Picture = picture;

            wwwRequest.Dispose();
        }

        areInvitablePicturesReady = true;
    }

    IEnumerator ProcessInvitableFriends_WithPictureDownload(object result)
    {
        areInvitablePicturesReady = false;

        Dictionary<string, object> responseDict = result as Dictionary<string, object>;

        List<object> dataArray = new List<object>();
        if (responseDict.ContainsKey("data"))
            dataArray = responseDict["data"] as List<object>;

        foreach (Dictionary<string, object> friendDict in dataArray)
        {
            string id = (string)friendDict["id"];
            string name = (string)friendDict["name"];

            Dictionary<string, object> pictureDict = friendDict["picture"] as Dictionary<string, object>;
            Dictionary<string, object> dataDictionary = pictureDict["data"] as Dictionary<string, object>;
            string pictureUrl = (string)dataDictionary["url"];

            WWW wwwRequest = new WWW(pictureUrl);

            yield return wwwRequest;

            Sprite picture = null;

            if (wwwRequest.error != null)
                Debug.Log(wwwRequest.error);
            else
            {
                Texture2D tex = wwwRequest.textureNonReadable;
                if (tex != null)
                    picture = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            wwwRequest.Dispose();

            //invitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend(id, name, picture));
        }

        /*foreach (var f in invitableFriends)
            Debug.Log(f.ToString());*/

        getInvitableFriendsCallCompleted = true;
        areInvitablePicturesReady = true;
    }
    #endregion

    #region Download Friends Pictures Methods

    const int kFBPicturesCap = 100;

    IEnumerator DownloadPicturesCoroutine(Action picturesReadyCallback)
    {
        //Debug.Log("### ### ### DownloadPicturesCoroutine");

        int lastRequestedPictureIndex = 0;

        while (lastRequestedPictureIndex < requestedPictureIds.Count)
        {
            lastRequestedPictureId = requestedPictureIds[lastRequestedPictureIndex];

            //Debug.Log("### ### ### DownloadPicturesCoroutine - requesting pictures for lastRequestedPictureId: " + lastRequestedPictureId);
            string fbPictureRequestUrl = "http://graph.facebook.com/" + lastRequestedPictureId + "/picture?width=" + PICTURE_RESOLUTION + "&height=" + PICTURE_RESOLUTION;
            WWW wwwRequest = new WWW(fbPictureRequestUrl);

            yield return wwwRequest;

            PictureDownloadCallback(wwwRequest);

            wwwRequest.Dispose();

            lastRequestedPictureReceived = false;
            lastRequestedPictureIndex++;
        }

        requestedPictureIds.Clear();
        lastRequestedPictureReceived = false;
        lastRequestedPictureId = string.Empty;

        picturesReadyCallback();
    }

    void PictureDownloadCallback(WWW wwwRequest)
    {
        //Debug.Log("### ### ### PictureDownloadCallback - ok? " + (wwwRequest.error == null).ToString());
        Sprite picture = null;

        if (wwwRequest.error != null)
            Debug.Log(wwwRequest.error);
        else
        {
            Texture2D tex = wwwRequest.textureNonReadable;
            if (tex != null)
                picture = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        if (picturesById.ContainsKey(lastRequestedPictureId))
        {
            var oldSpr = picturesById[lastRequestedPictureId];
            if (oldSpr != null)
                Sprite.DestroyImmediate(oldSpr, true);

            picturesById[lastRequestedPictureId] = picture;

            string[] array = lruPictureIds.ToArray();
            int index = Array.IndexOf<string>(array, lastRequestedPictureId),
                lastIndex = array.Length - 1;
            if (index >= 0 && lastIndex > 0)
            {
                int i = index;
                for (; i < lastIndex; ++i)
                    array[i] = array[i + 1];
                array[i] = lastRequestedPictureId;
            }
            lruPictureIds = new Queue<string>(array);
        }
        else
        {
            if (kFBPicturesCap == picturesById.Count)
            {
                string oldest = lruPictureIds.Dequeue();

                var oldestSpr = picturesById[oldest];
                picturesById.Remove(oldest);

                Sprite.DestroyImmediate(oldestSpr, true);
            }

            picturesById.Add(lastRequestedPictureId, picture);
            lruPictureIds.Enqueue(lastRequestedPictureId);
        }

        lastRequestedPictureReceived = true;
    }

    #endregion

    void PushBlockButtonPopup()
    {
        if (Manager<UIManager>.Get().IsPopupInStack("Wp8FacebookBlockPopup"))
            Manager<UIManager>.Get().BringPopupToFront("Wp8FacebookBlockPopup");
        else
            Manager<UIManager>.Get().PushPopup("Wp8FacebookBlockPopup");

        Debug.Log("@@@----@@@@----@@@@----@@@@ PushBlockButtonPopup");
    }

    void PopBlockButtonPopup()
    {
        if (Manager<UIManager>.Get().IsPopupInStack("Wp8FacebookBlockPopup"))
        {
            Manager<UIManager>.Get().RemovePopupFromStack("Wp8FacebookBlockPopup");
            Debug.Log("@@@----@@@@----@@@@----@@@@ PopBlockButtonPopup");
        }
    }
}
#endif