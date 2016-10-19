using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;
#if !UNITY_WP8
using CodeTitans.JSon;
#endif

// This was used in the early prototypes, not used any more. For FB feeds we use the FB unity sdk.
public class OnTheRunFeeds : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunFeeds instance = null;

    public static OnTheRunFeeds Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

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

    //string fbFeedName;
    //string fbFeedCaption;
    string fbFeedPictureLink;
    string fbFeedLink;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        GameObject.DontDestroyOnLoad(gameObject);

        Init();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    void Init()
    {
        //fbFeedName = OnTheRunDataLoader.Instance.GetLocaleString("on_the_run");
        //fbFeedCaption = OnTheRunDataLoader.Instance.GetLocaleString("feed_caption");
        fbFeedPictureLink = "http://static.miniclipcdn.com/iphone/assets/hotrod/hotrod_icon.png";
        fbFeedLink = "https://www.facebook.com/hotrodracers";
    }

    public void ShareFacebookFeed(string feedMessage)
    {
        Dictionary<string, string> feedParams = new Dictionary<string, string>();
        feedParams.Add("name", OnTheRunDataLoader.Instance.GetLocaleString("on_the_run")); //fbFeedName);
        feedParams.Add("caption", OnTheRunDataLoader.Instance.GetLocaleString("feed_caption")); //fbFeedCaption);
        feedParams.Add("picture", fbFeedPictureLink);
        feedParams.Add("description", feedMessage);
        feedParams.Add("link", fbFeedLink);
        feedParams.Add("properties", GetFacebookPropertiesJSON());

        SBS.Miniclip.FBBindings.OpenDialog("feed", feedParams);
    }

    string GetFacebookPropertiesJSON()
    {
        string propertiesString = string.Empty;

#if !UNITY_WP8
        JSonWriter jsonWriter = new JSonWriter();
        Dictionary<string, object> properties = new Dictionary<string, object>();
        properties.Add("On " + StoreName, new Dictionary<string, string>() { { "text", BitLyLink }, { "href", BitLyLink } });
        jsonWriter.Write(properties);
        propertiesString = jsonWriter.ToString();
        jsonWriter.Dispose();
#endif

        return propertiesString;
    }

    public void ShareTwitterFeed(string feedMessage)
    {
#if UNITY_ANDROID
        SBS.Miniclip.TWBindings.SendTweet(feedMessage, BitLyLink);
#else
        SBS.Miniclip.TWBindings.SendTweet(feedMessage + BitLyLink);
#endif
    }
}