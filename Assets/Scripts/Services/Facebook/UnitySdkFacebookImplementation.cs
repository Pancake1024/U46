#if !UNITY_WP8

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class UnitySdkFacebookImplementation : MonoBehaviour, FacebookImplementation
{
    const int pictureResolution = 58;

    bool meApiCallIsCompleted;
    bool myPictureApiCallIsCompleted;
    bool loginHasBeenCalled;
    bool getFriendsCallCompleted;
    bool getInvitableFriendsCallCompleted;

    string country;
    string gender;
    string birthday;    // YYYY-MM-DD
    string name;

    public bool IsInitialized { get { return isInitialized; } }
    public bool IsLoggedIn { get { return FB.IsLoggedIn; } }
    public string UserId { get { return FB.UserId; } }
    public string AccessToken { get { return FB.AccessToken; } }
    public string Country { get { return country; } }
    public string Gender { get { return gender; } }
    public string Birthday { get { return birthday; } }
    public string Name { get { return name; } }
    public string MeAndFriendsIdsCommaSeparated
    {
        get
        {
            string res = FB.UserId;

            foreach (var kvp in friendsById)
                res += "," + kvp.Key;

            return res;
        }
    }

    public bool AreInvitablePicturesReady { get { return areInvitablePicturesReady; } }

    public Sprite UserPicture { get { return userPicture; } }
    public Dictionary<string, string> FriendsDictionary { get { return friendsById; } }

    Action facebookLoginSuccessCallback;
    Action facebookLoginFailCallback;
    Action loginErrorCallback;

    Dictionary<string, string> friendsById;
    Dictionary<string, Sprite> picturesById;
    Sprite userPicture;
    Queue<string> lruPictureIds;
    bool isInitialized;

    List<string> requestedPictureIds;
    bool lastRequestedPictureReceived;
    string lastRequestedPictureId;

    List<OnTheRunFacebookManager.InvitableFriend> invitableFriends;
    bool areInvitablePicturesReady;

    void Awake()
    {
        isInitialized = false;
        meApiCallIsCompleted = false;
        myPictureApiCallIsCompleted = false;
        getFriendsCallCompleted = false;
        getInvitableFriendsCallCompleted = false;
        areInvitablePicturesReady = true;
        country = string.Empty;
        gender = string.Empty;
        birthday = string.Empty;
        name = string.Empty;
        friendsById = new Dictionary<string, string>();
        picturesById = new Dictionary<string, Sprite>();
        lruPictureIds = new Queue<string>();

        requestedPictureIds = new List<string>();
        lastRequestedPictureReceived = false;
        lastRequestedPictureId = string.Empty;

        invitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();

        loginHasBeenCalled = false;
    }

    public void InitFb(string fbAppId, Action initWithSuccessfulLoginCallback, Action initWithoutLoginCallback)
    {
        if (isInitialized)
            return;

        facebookLoginSuccessCallback = initWithSuccessfulLoginCallback;
        facebookLoginFailCallback = initWithoutLoginCallback;
        loginErrorCallback = initWithoutLoginCallback;

        FB.Init(OnInitComplete, fbAppId);
    }

    void OnInitComplete()
    {
        isInitialized = true;

        if (loginHasBeenCalled)
            return;

        if (FB.IsLoggedIn)
        {
            /*
            meApiCallIsCompleted = false;
            myPictureApiCallIsCompleted = false;
            FB.API("/me", Facebook.HttpMethod.GET, ApiMeCallback);
            FB.API(FacebookUtil.GetPictureURL("me", pictureResolution, pictureResolution), Facebook.HttpMethod.GET, ApiMyPictureCallback);
            GetFriends();
            */

            StartCoroutine(LoginSuccessCoroutine());
        }
        else
        {
            LoginFail();
            /*
            facebookLoginFailCallback();

            facebookLoginSuccessCallback = null;
            facebookLoginFailCallback = null;
            loginErrorCallback = null;
            */
        }
    }
    
    void ApiMeCallback(FBResult result)
    {
        country = string.Empty;
        gender = string.Empty;
        birthday = string.Empty;
        name = string.Empty;

        if (string.IsNullOrEmpty(result.Error))
        {
            //Debug.Log("----------------------------------- FACEBOOK API ME: " + result.Text);
            //Debug.Log("----------------------------------- FACEBOOK My user id: " + FB.UserId);

            //FB.API("/me/permissions", Facebook.HttpMethod.GET, ApiTestCallback);
            //FB.API("/me/friends", Facebook.HttpMethod.GET, ApiTestCallback);
            //FB.API("/me/ids_for_business", Facebook.HttpMethod.GET, ApiTestCallback);
            //FB.API("/me/taggable_friends", Facebook.HttpMethod.GET, ApiTestCallback);

            //Debug.Log("----------------------------------- FACEBOOK /me/invitable_friends");
            //FB.API("/me/invitable_friends", Facebook.HttpMethod.GET, ApiTestCallback);
            //TestAppRequest();
            //StartCoroutine(GraphApiTestCoroutine());

            Dictionary<string, object> responseDict = Json.Deserialize(result.Text) as Dictionary<string, object>;
            if (responseDict.ContainsKey("locale"))
                country = (string)responseDict["locale"];

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
        //else
        //    Debug.Log("ApiMeCallback ERROR: " + result.Error);

        meApiCallIsCompleted = true;
    } 

    /*void ApiTestCallback(FBResult result)
    {
        if (string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("----------------------------------- FACEBOOK API result.Text: " + result.Text);

            // invitable_friends
            Dictionary<string, object> responseDict = Json.Deserialize(result.Text) as Dictionary<string, object>;
            List<object> dataArray = responseDict["data"] as List<object>;

            List<string> friendIds = new List<string>();
            foreach (Dictionary<string, object> friendDict in dataArray)
            {
                friendIds.Add((string)friendDict["id"]);
            }

            foreach (var f in friendIds)
                Debug.Log("- id: " + f);

            FB.AppRequest("Facebook AppRequest Test Message", friendIds.ToArray(), null, null, null, "", "Facebook AppRequest Test Title", appRequestCallback);
        }
        else
            Debug.Log("----------------------------------- FACEBOOK API result ERROR: " + result.Error);
    }*/

    /*
    IEnumerator GraphApiTestCoroutine()
    {
        string graphUrl = "http://graph.facebook.com/" + FB.UserId + "/picture?width=" + pictureResolution + "&height=" + pictureResolution;
        Debug.Log("----------------------------------- FACEBOOK GRAPH API request: " + graphUrl);
        WWW wwwRequest = new WWW(graphUrl);

        yield return wwwRequest;

        if (string.IsNullOrEmpty(wwwRequest.error))
            Debug.Log("----------------------------------- FACEBOOK GRAPH API result.Text: " + wwwRequest.text);
        else
            Debug.Log("----------------------------------- FACEBOOK GRAPH API result ERROR: " + wwwRequest.error);
    }*/

    void ApiMyPictureCallback(FBResult result)
    {
        //Debug.Log("----------------------------------- FACEBOOK ApiMyPictureCallback");

        Sprite picture = null;

        if (result.Error != null)
            FbDebug.Error(result.Error);
        else
            picture = Sprite.Create(result.Texture, new Rect(0, 0, result.Texture.width, result.Texture.height), new Vector2(0.5f, 0.5f));

        userPicture = picture;

        myPictureApiCallIsCompleted = true;
    }

    public void Login(string permissions, Action successCallback, Action failCallback, Action errorCallback)
    {
        facebookLoginSuccessCallback = successCallback;
        facebookLoginFailCallback = failCallback;
        loginErrorCallback = errorCallback;

        meApiCallIsCompleted = false;
        myPictureApiCallIsCompleted = false;
        getFriendsCallCompleted = false;
        getInvitableFriendsCallCompleted = false;
        loginHasBeenCalled = true;
        //FB.Login("email,publish_actions,user_friends", LoginCallback);
        FB.Login(permissions, LoginCallback);
    }

    void LoginCallback(FBResult result)
    {
        if (result.Error != null)
        {
            //Debug.Log("Facebook Login Error:\n" + result.Error);

            loginErrorCallback();

            facebookLoginSuccessCallback = null;
            facebookLoginFailCallback = null;
            loginErrorCallback = null;

            return;
        }

        if (FB.IsLoggedIn)
            StartCoroutine(LoginSuccessCoroutine());
        else
            LoginFail();
    }

    IEnumerator LoginSuccessCoroutine()
    {
        /*const bool SKIP_API_ME_CALL = false;
        if (SKIP_API_ME_CALL)
            meApiCallIsCompleted = true;
        else*/
        FB.API("/me", Facebook.HttpMethod.GET, ApiMeCallback);

        FB.API(FacebookUtil.GetPictureURL("me", pictureResolution, pictureResolution), Facebook.HttpMethod.GET, ApiMyPictureCallback);

        GetFriends();
        GetInvitableFriends();

        while (!meApiCallIsCompleted || !myPictureApiCallIsCompleted || !getFriendsCallCompleted)
            yield return new WaitForEndOfFrame();

        LoginSuccess();
    }

    void LoginSuccess()
    {
        facebookLoginSuccessCallback();

        facebookLoginSuccessCallback = null;
        facebookLoginFailCallback = null;
        loginErrorCallback = null;
    }

    void LoginFail()
    {
        facebookLoginFailCallback();

        facebookLoginSuccessCallback = null;
        facebookLoginFailCallback = null;
        loginErrorCallback = null;
    }

    public void GetFriends()
    {
        FB.API("/me/friends", Facebook.HttpMethod.GET, GetFriendsCallback);
    }

    void GetFriendsCallback(FBResult result)
    {
        if (!string.IsNullOrEmpty(result.Error))
            return;
        //Debug.Log("Error Response:\n" + result.Error);

        Debug.Log(result.Text);

        Dictionary<string, object> responseDict = Json.Deserialize(result.Text) as Dictionary<string, object>;
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

        //Debug.Log("##################################### REQUESTED FRIENDS PICTURES...");
        // OLD...DownloadPictures(friendIds, () => { /*Debug.Log("##################################### FRIENDS PICTURES READY! - NumPictures: " + picturesById.Count);*/ });
    }

    public void GetInvitableFriends()
    {
        FB.API("/me/invitable_friends", Facebook.HttpMethod.GET, GetInvitableFriendsCallback);
    }

    void GetInvitableFriendsCallback(FBResult result)
    {
        invitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();

        if (!string.IsNullOrEmpty(result.Error))
            return;

        //StartCoroutine(ProcessInvitableFriends_WithPictureDownload(result.Text));
        ProcessInvitableFriends(result.Text);
    }

    void ProcessInvitableFriends(string invitableFriendsJson)
    {
        const bool TEST_ALOT_OF_FRIENDS = false;
        

        Dictionary<string, object> responseDict = Json.Deserialize(invitableFriendsJson) as Dictionary<string, object>;
        List<object> dataArray = responseDict["data"] as List<object>;

        foreach (Dictionary<string, object> friendDict in dataArray)
        {
            string id = (string)friendDict["id"];
            string name = (string)friendDict["name"];

            Dictionary<string, object> pictureDict = friendDict["picture"] as Dictionary<string, object>;
            Dictionary<string, object> dataDictionary = pictureDict["data"] as Dictionary<string, object>;
            string pictureUrl = (string)dataDictionary["url"];

            if (TEST_ALOT_OF_FRIENDS)
            {
                int numAlotOfFriends = 100;
                for (int i = 0; i < numAlotOfFriends; i++)
                    invitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend(id, name + "_" + i, pictureUrl));
            }
            else
                invitableFriends.Add(new OnTheRunFacebookManager.InvitableFriend(id, name, pictureUrl));
        }

        //Debug.Log("### NUM. Invitable Friends: " + invitableFriends.Count);

        //if (!TEST_ALOT_OF_FRIENDS)
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

    IEnumerator ProcessInvitableFriends_WithPictureDownload(string invitableFriendsJson)
    {
        areInvitablePicturesReady = false;

        Dictionary<string, object> responseDict = Json.Deserialize(invitableFriendsJson) as Dictionary<string, object>;
        List<object> dataArray = responseDict["data"] as List<object>;

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

    public void DownloadPictures(List<string> userIds, Action picturesReadyCallback)
    {
        if (userIds == null || userIds.Count == 0)
        {
            picturesReadyCallback();
            return;
        }

        //Debug.Log("####################### Download Pictures - userIds.Count: " + userIds.Count);

        requestedPictureIds = new List<string>();
        foreach (string id in userIds)
            if (!picturesById.ContainsKey(id))
                requestedPictureIds.Add(id);

        //Debug.Log("####################### Download Pictures - requestedPictureIds.Count: " + requestedPictureIds.Count);

        if (requestedPictureIds.Count > 0)
            //StartCoroutine(DownloadPicturesCoroutine(picturesReadyCallback));
            StartCoroutine(DownloadPicturesWithGraphApiCoroutine(picturesReadyCallback));
        else
            picturesReadyCallback();
    }

    IEnumerator DownloadPicturesWithGraphApiCoroutine(Action picturesReadyCallback)
    {
        int lastRequestedPictureIndex = 0;

        while (lastRequestedPictureIndex < requestedPictureIds.Count)
        {
            //Debug.Log("######################## lastRequestedPictureIndex: " + lastRequestedPictureIndex + " - requestedPictureIds.Count:- " + requestedPictureIds.Count);

            lastRequestedPictureId = requestedPictureIds[lastRequestedPictureIndex];
            //FB.API(FacebookUtil.GetPictureURL(lastRequestedPictureId, pictureResolution, pictureResolution), Facebook.HttpMethod.GET, PictureDownloadCallback);
            string fbPictureRequestUrl = "http://graph.facebook.com/" + lastRequestedPictureId + "/picture?width=" + pictureResolution + "&height=" + pictureResolution;
            WWW wwwRequest = new WWW(fbPictureRequestUrl);

            yield return wwwRequest;

            //while (!lastRequestedPictureReceived)
            //    yield return new WaitForEndOfFrame();

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

    const int kFBPicturesCap = 100;

    void PictureDownloadCallback(WWW wwwRequest)
    {
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
            //Debug.Log("######################## PictureDownloadCallback - replacing picture for: " + lastRequestedPictureId);
            var oldSpr = picturesById[lastRequestedPictureId];
            if (oldSpr != null)
                Sprite.DestroyImmediate(oldSpr, true);

            picturesById[lastRequestedPictureId] = picture;// result.Texture;

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
            //Debug.Log("######################## PictureDownloadCallback - adding picture for: " + lastRequestedPictureId);
            if (kFBPicturesCap == picturesById.Count)
            {
                string oldest = lruPictureIds.Dequeue();

                var oldestSpr = picturesById[oldest];
                picturesById.Remove(oldest);

                Sprite.DestroyImmediate(oldestSpr, true);
            }

            picturesById.Add(lastRequestedPictureId, picture);  // result.Texture);
            lruPictureIds.Enqueue(lastRequestedPictureId);
        }

        lastRequestedPictureReceived = true;
    }

    public Sprite GetUsersPicture(string facebookId)
    {
        if (picturesById.ContainsKey(facebookId))
            return picturesById[facebookId];
        else
            return null;
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
        FB.Feed(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference, properties, (FBResult result) =>
        {
            //Debug.Log("################### FEED CALLBACK - Result:\n" + result.ToString() + "\nText: " + result.Text);
            callback(string.IsNullOrEmpty(result.Error));
        });
    }

    public void Logout()
    {
        if (!isInitialized)
            return;

        FB.Logout();

        meApiCallIsCompleted = false;
        myPictureApiCallIsCompleted = false;
        loginHasBeenCalled = false;
        getFriendsCallCompleted = false;
        getInvitableFriendsCallCompleted = false;
        areInvitablePicturesReady = true;
        country = string.Empty;
        gender = string.Empty;
        friendsById = new Dictionary<string, string>();
        picturesById = new Dictionary<string, Sprite>();

        requestedPictureIds = new List<string>();
        lastRequestedPictureReceived = false;
        lastRequestedPictureId = string.Empty;

        invitableFriends = new List<OnTheRunFacebookManager.InvitableFriend>();
    }

    public void ClearPictures()
    {
        if (picturesById != null)
            picturesById = new Dictionary<string, Sprite>();
    }

    public void Test_PrintNumPictures()
    {
        //Debug.Log("############################################ FACEBOOK PICTURES NUM: " + ((picturesById != null) ? picturesById.Count.ToString() : " -"));
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

    //const string INVITE_TEXT = "Click Here to play On The Run";
    //const string INVITE_TITLE = "You are invited";             

    public void InviteFriend(OnTheRunFacebookManager.InvitableFriend friend, Action<bool, string, string> callback)
    {
        //Invite(new List<string> { friend.Id }, callback);

        List<string> ids = new List<string> { friend.Id };

        FB.AppRequest(OnTheRunDataLoader.Instance.GetLocaleString("invite_friend_text"), ids.ToArray(), null, null, null, "", OnTheRunDataLoader.Instance.GetLocaleString("invite_friend_title"), fbResult => { inviteFriendsCallback(fbResult, friend.Name, callback); });
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

    /*void Invite(List<string> ids, Action<bool, string, string> callback)
    {
        FB.AppRequest(INVITE_TEXT, ids.ToArray(), null, null, null, "", INVITE_TITLE, fbResult => { inviteFriendsCallback(fbResult, callback); });
    }*/

    void inviteFriendsCallback(FBResult result, string invitedFriendName, Action<bool, string, string> callback)
    {
        bool success = false;
        string recipientId = string.Empty;

        if (result != null)
        {
            //Debug.Log("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX facebook invite friend callback: " + result.Text);

            var responseObject = Json.Deserialize(result.Text) as Dictionary<string, object>;

            if(responseObject.ContainsKey("request") && responseObject.ContainsKey("to"))
            {
                var recipientsList = responseObject["to"] as List<object>;
                recipientId = (string)recipientsList[0];
                success = true;
            }
        }

        callback(success, recipientId, invitedFriendName);
    } 

	public void LogEvent(string eventName, Dictionary<string, object> parameters)
	{
		FB.AppEvents.LogEvent (eventName, null, parameters);
	}
}

#endif