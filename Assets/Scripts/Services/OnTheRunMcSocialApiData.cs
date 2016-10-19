using UnityEngine;
using System.Collections;
using SBS.Core;
using System.Collections.Generic;

public class OnTheRunMcSocialApiData : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunMcSocialApiData instance = null;

    public static OnTheRunMcSocialApiData Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    public List<string> leaderboardIds;
    public string levelPropertyName;
    public string experiencePropertyName;
    public Sprite defaultUserPicture;
    public Sprite blueUserPicture;
    public Sprite greenUserPicture;
    public Sprite pinkUserPicture;

    Sprite socialNetworkUserPicture;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    public void OnFacebookPictureAvailable()
    {
        //Texture2D picture = OnTheRunFacebookManager.Instance.UserPicture;
        //socialNetworkUserPicture = Sprite.Create(picture, new Rect(0, 0, picture.width, picture.height), new Vector2(0.5f, 0.5f));
        socialNetworkUserPicture = OnTheRunFacebookManager.Instance.UserPicture;
    }

    public void OnGooglePlusPictureAvailable()
    {
        if (GooglePlusManager.Instance.UserPicture != null)
            socialNetworkUserPicture = GooglePlusManager.Instance.UserPicture;
    }

    public Sprite GetPicture()
    {
        bool isLoggedOnSocialNetwork = OnTheRunFacebookManager.Instance.IsLoggedIn || (GooglePlusManager.Instance != null && GooglePlusManager.Instance.IsLoggedIn);
        if (isLoggedOnSocialNetwork && socialNetworkUserPicture != null)
            return socialNetworkUserPicture;
        else
            return defaultUserPicture;
    }

    public string GetLeaderboardId(int locationIndex)
    {
        Asserts.Assert(leaderboardIds.Count >= locationIndex);

        return leaderboardIds[locationIndex];
    }

    public string TrimStringAtMaxChars(string str, int maxChars)
    {
        const string truncTail = "..."; //LOCALIZATION ???
        const int lengthMargin = 1;

        if (str.Length <= maxChars + lengthMargin)
            return str;

        string returnstr = str.Remove(maxChars);
        returnstr += truncTail;
        return returnstr;
    }
}