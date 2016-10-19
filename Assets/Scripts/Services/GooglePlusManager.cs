using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using SBS.Core;
//using SBS.Core;

public class GooglePlusManager : MonoBehaviour
{
    #region Singleton instance
    protected static GooglePlusManager instance = null;

    public static GooglePlusManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    const int kFBPicturesCap = 100;

    GooglePlusImplementation implementation;
    GooglePlusManagerCallbacksContainer managerCallbacks = new GooglePlusManagerCallbacksContainer();

    bool isLoggedIn = false;
    GooglePlusUser user = new GooglePlusUser();
    Sprite userPicture = null;
    string accessToken = "n/a";
    Dictionary<string, Sprite> otherUsersPicturesByUserId = new Dictionary<string, Sprite>();
    Queue<string> lruPictureIds = new Queue<string>();

    public bool IsLoggedIn { get { return isLoggedIn; } }
    public GooglePlusUser User { get { return user; } }
    public Sprite UserPicture { get { return userPicture; } }
    //public string AccessToken { get { return accessToken; } }
    public Dictionary<string, Sprite> OtherUsersPicturesByUserId { get { return otherUsersPicturesByUserId; } }

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);

        SetImplementation();
        RegisterCallbacks();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    #region Initializations

    void SetImplementation()
    {
#if UNITY_EDITOR
        implementation = gameObject.AddComponent<EditorGooglePlusImplementation>();
#elif UNITY_ANDROID
        implementation = gameObject.AddComponent<JavaNativeGooglePlusImplementation>();
		#else
		implementation = gameObject.AddComponent<VoidGooglePlusImplementation>();
#endif
    }

    void RegisterCallbacks()
    {
        GooglePlusImplementationCallbacksContainer callbacksContainer = new GooglePlusImplementationCallbacksContainer();

        callbacksContainer.onLoginCompleted = OnLoginCompleted;
        callbacksContainer.onUserDataRequestCompleted = OnUserDataRequestCompleted;
        callbacksContainer.onAccessTokenRequestCompleted = OnAccessTokenRequestCompleted;
        callbacksContainer.onOtherUsersPicureUrlRequestCompleted = OnOtherUsersPicureUrlRequestCompleted;

        implementation.SetCallbacksContainer(callbacksContainer);
    }

    #endregion

    #region Public Methods

    public void Login(Action<bool> callback)
    {
        managerCallbacks.loginCallback = callback;

        implementation.Login();
    }

    public void RequestOtherUsersPicure(List<string> idsList, Action<bool> callback)
    {
        if (!isLoggedIn)
        {
            callback(false);
            return;
        }

        managerCallbacks.otherUsersPicureUrlRequestCallback = callback;

        implementation.RequestOtherUsersPicureUrl(idsList);
    }

    #endregion

    #region Callback Methods

    void OnLoginCompleted(bool loggedInSuccessfully)
    {
        /*isLoggedIn = loggedInSuccessfully;
        TriggerCallback(managerCallbacks.loginCallback, isLoggedIn);*/

        this.RequestUserData((userDataRequestWasSuccessful) =>
        {
            this.RequestAccessToken((accesstokenRequestWasSuccessful) =>
            {
                this.RequestUserPicture((pictureRequestWasSuccessful) =>
                {
                    bool allPassed = loggedInSuccessfully && userDataRequestWasSuccessful && accesstokenRequestWasSuccessful;
#if UNITY_EDITOR
                    //allPassed = true;

                    accessToken = "ya29.dgD88j2A_XupiBwAAADEq1YOqUCB8VdCD93W_s8JUcxLQ99NpcDvBhu4r5C3_A";
                    allPassed = true;
#endif
                    if (allPassed)
                    {
                        if (pictureRequestWasSuccessful)
                            OnTheRunMcSocialApiData.Instance.OnGooglePlusPictureAvailable();
//#if !UNITY_EDITOR
                        McSocialApiManager.Instance.LoginWithGoogle(accessToken);
//#endif
                    }
                    //else
                    //    McSocialApiManager.Instance.LoginAsGuest();
                    Debug.Log("########################################################################### allPassed: " + allPassed);
                    FinalizeLogin(allPassed);
                });
            });
        });
    }

    void FinalizeLogin(bool loggedInSuccessfully)
    {
        isLoggedIn = loggedInSuccessfully;
        TriggerCallback(managerCallbacks.loginCallback, isLoggedIn);
    }

    void OnUserDataRequestCompleted(GooglePlusUser user)
    {
        this.user = user;

        TriggerCallback(managerCallbacks.userDataRequestCallback, true);
    }

    void OnAccessTokenRequestCompleted(string accessToken, bool accessTokenIsValid)
    {
        this.accessToken = accessToken;

        TriggerCallback(managerCallbacks.accessTokenRequestCallback, accessTokenIsValid);
    }

    void OnOtherUsersPicureUrlRequestCompleted(Dictionary<string, string> otherUsersPicturesUrlByUserId)
    {
        StartCoroutine(DownloadOtherUsersPictures(otherUsersPicturesUrlByUserId));
    }

    #endregion

    #region Private Methods

    /*public*/
    void RequestUserData(Action<bool> callback)
    {
        managerCallbacks.userDataRequestCallback = callback;

        implementation.RequestUserData();
    }

    /*public*/
    void RequestUserPicture(Action<bool> callback)
    {
        if (user.PhotoUrl.Equals("n/a"))
            callback(false);
        else
        {
            managerCallbacks.userPictureRequestCallback = callback;
            StartCoroutine(DownloadUserPicture());
        }
    }

    /*public*/
    void RequestAccessToken(Action<bool> callback)
    {
        managerCallbacks.accessTokenRequestCallback = callback;

        implementation.RequestAccessToken(user.Email);
    }

    IEnumerator DownloadUserPicture()
    {
        bool isSuccessful = false;

        WWW wwwRequest = new WWW(user.PhotoUrl);
        yield return wwwRequest;

        if (string.IsNullOrEmpty(wwwRequest.error))
        {
            Texture2D tex = wwwRequest.textureNonReadable;
            if (tex != null)
            {
                userPicture = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                isSuccessful = true;
            }
        }

        TriggerCallback(managerCallbacks.userPictureRequestCallback, isSuccessful);
    }

    IEnumerator DownloadOtherUsersPictures(Dictionary<string, string> pictureUrlsByUserId)
    {
        bool isSuccessful = false;

        foreach (var kvp in pictureUrlsByUserId)
        {
            string userId = kvp.Key;
            string pictureUrl = kvp.Value;

            Debug.Log("Requesting picture url: userId: " + userId + " - url: " + pictureUrl);

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

            if (otherUsersPicturesByUserId.ContainsKey(userId))
            {
                var oldSpr = otherUsersPicturesByUserId[userId];
                if (oldSpr != null)
                    Sprite.DestroyImmediate(oldSpr, true);

                otherUsersPicturesByUserId[userId] = picture;

                string[] array = lruPictureIds.ToArray();
                int index = Array.IndexOf<string>(array, userId),
                    lastIndex = array.Length - 1;
                if (index >= 0 && lastIndex > 0)
                {
                    int i = index;
                    for (; i < lastIndex; ++i)
                        array[i] = array[i + 1];
                    array[i] = userId;
                }
                lruPictureIds = new Queue<string>(array);
            }
            else
            {
                if (kFBPicturesCap == otherUsersPicturesByUserId.Count)
                {
                    string oldest = lruPictureIds.Dequeue();

                    var oldestSpr = otherUsersPicturesByUserId[oldest];
                    otherUsersPicturesByUserId.Remove(oldest);

                    Sprite.DestroyImmediate(oldestSpr, true);
                }

                otherUsersPicturesByUserId.Add(userId, picture);
                lruPictureIds.Enqueue(userId);
            }

            isSuccessful = true;
        }

        TriggerCallback(managerCallbacks.otherUsersPicureUrlRequestCallback,isSuccessful);
    }

    void TriggerCallback(Action<bool> callback, bool outcome)
    {
        if (callback != null)
            callback(outcome);
        callback = null;
    }

    #endregion
}