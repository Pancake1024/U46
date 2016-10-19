//#define ALLOW_FOR_SYNTAX_HIGHLITING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS_MiniJSON;

#if UNITY_ANDROID && !UNITY_EDITOR || ALLOW_FOR_SYNTAX_HIGHLITING

public class JavaNativeGooglePlusImplementation : MonoBehaviour, GooglePlusImplementation
{
    const string bindingsClass = "com.miniclip.ontherun.GooglePlusBindings";

    const string loginRequestMethod = "OnGooglePlusLoginButtonPressed";
    const string userDataRequestMethod = "RequestUserData";
    const string accessTokenRequestMethod = "GetGooglePlusAccessToken";
    const string otherUsersPictureUrlsRequest = "RequestUsersPictures";

    const string jsonName = "name";
    const string jsonPictureUrl = "pictureUrl";
    const string jsonProfileUrl = "profileUrl";
    const string jsonEmail = "email";
    const string jsonPicureUrlsById = "picureUrlsById";
    const string jsonUserId = "userId";
    const string jsonAccessToken = "accessToken";
    const string jsonIsValid = "isValid";

    GooglePlusImplementationCallbacksContainer callbacksContainer;

    #region Google Plus Implementation Methods

    public void SetCallbacksContainer(GooglePlusImplementationCallbacksContainer callbacksContainer)
    {
        this.callbacksContainer = callbacksContainer;
    }

    public void Login()
    {
        Debug.Log("########### JavaNativeGooglePlusImplementation - Login");
        Call_StaticVoid_Method_OnBindingsClass(loginRequestMethod);
    }

    public void RequestUserData()
    {
        Debug.Log("########### JavaNativeGooglePlusImplementation - RequestUserData");
        Call_StaticVoid_Method_OnBindingsClass(userDataRequestMethod);
    }

    public void RequestAccessToken(string userEmail)
    {
        Debug.Log("########### JavaNativeGooglePlusImplementation - RequestAccessToken");
        Call_StaticVoid_Method_OnBindingsClass(accessTokenRequestMethod, userEmail);
    }

    public void RequestOtherUsersPicureUrl(List<string> idsList)
    {
        Debug.Log("########### JavaNativeGooglePlusImplementation - RequestOtherUsersPicureUrl");
        Call_StaticVoid_Method_OnBindingsClass(otherUsersPictureUrlsRequest, getCsvFromList(idsList));
    }

    #endregion

    #region Utility Methods

    void Call_StaticVoid_Method_OnBindingsClass(string method)
    {
        using (AndroidJavaClass jc = new AndroidJavaClass(bindingsClass))
        {
            Debug.Log("*************** JAVA CLASS (" + bindingsClass + ") IS NULL? " + (jc == null).ToString());
            jc.CallStatic(method);
        }
    }

    void Call_StaticVoid_Method_OnBindingsClass(string method, string parameter)
    {
        using (AndroidJavaClass jc = new AndroidJavaClass(bindingsClass))
        {
            Debug.Log("*************** JAVA CLASS (" + bindingsClass + ") IS NULL? " + (jc == null).ToString());
            jc.CallStatic(method, parameter);
        }
    }

    string getCsvFromList(List<string> list)
    {
        string csv = string.Empty;

        for (int i = 0; i < list.Count; i++)
        {
            csv += list[i];
            if (i < list.Count - 1)
                csv += ",";
        }

        return csv;
    }
    
    #endregion

    #region Java Callback Methods

    void OnGooglePlusLoginCompleted(string loginOutcome)
    {
        bool loginWasSuccessful = false;
        bool.TryParse(loginOutcome, out loginWasSuccessful);

        callbacksContainer.onLoginCompleted(loginWasSuccessful);
    }

    void OnGooglePlusUserDataAvailable(string userJson)
    {
        GooglePlusUser user = new GooglePlusUser();

        if (!string.IsNullOrEmpty(userJson))
        {
            Dictionary<string, object> userDictionary = Json.Deserialize(userJson) as Dictionary<string, object>;
            string userName   = GetDictionaryStringValue(userDictionary, jsonName);
            string photoUrl   = GetDictionaryStringValue(userDictionary, jsonPictureUrl);
            string profileUrl = GetDictionaryStringValue(userDictionary, jsonProfileUrl);
            string email      = GetDictionaryStringValue(userDictionary, jsonEmail);

            user = new GooglePlusUser(userName, photoUrl, profileUrl, email);
        }

        callbacksContainer.onUserDataRequestCompleted(user);
    }
         
    string GetDictionaryStringValue(Dictionary<string, object> dict, string key)
    {
        string value = string.Empty;

        if (dict != null && dict.ContainsKey(key))
            value = (string)dict[key];

        return value;
    }
     
    void OnGooglePlusAccessTokenAvailable(string accessTokenJson)
    {
        string accessToken = "";
        bool isValid = false;

        if (!string.IsNullOrEmpty(accessTokenJson))
        {
            Dictionary<string, object> tokenDictionary = Json.Deserialize(accessTokenJson) as Dictionary<string, object>;
            accessToken = GetDictionaryStringValue(tokenDictionary, jsonAccessToken);
            
            string isValidString = GetDictionaryStringValue(tokenDictionary, jsonIsValid);
            bool.TryParse(isValidString, out isValid);
        }

        callbacksContainer.onAccessTokenRequestCompleted(accessToken, isValid);
    }
     
    void OnGooglePlusUsersPicturesAvailable(string otherUsersPictureUrlsJson)
    {
        Debug.Log("OnGooglePlusUsersPicturesAvailable - json:\n" + otherUsersPictureUrlsJson);

        Dictionary<string, string> pictureUrlsByUserId = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(otherUsersPictureUrlsJson))
        {
            Dictionary<string, object> responseDict = Json.Deserialize(otherUsersPictureUrlsJson) as Dictionary<string, object>;
            List<object> dataList = new List<object>();

            if (responseDict.ContainsKey(jsonPicureUrlsById))
                dataList = responseDict[jsonPicureUrlsById] as List<object>;

            foreach(var element in dataList)
            {
                string userId = GetDictionaryStringValue(element as Dictionary<string, object>, jsonUserId);
                string pictureUrl = GetDictionaryStringValue(element as Dictionary<string, object>, jsonPictureUrl);

                pictureUrlsByUserId.Add(userId, pictureUrl);
            }
        }

        callbacksContainer.onOtherUsersPicureUrlRequestCompleted(pictureUrlsByUserId);
    }

    #endregion
}

#endif