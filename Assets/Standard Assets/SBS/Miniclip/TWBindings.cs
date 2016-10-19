using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using SBS.Core;

namespace SBS.Miniclip
{
    public class TWBindings
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        protected static extern bool canSendTweet();

        [DllImport("__Internal")]
        protected static extern bool sendTweet(string text);

        [DllImport("__Internal")]
        protected static extern bool sendTweetWithURL(string text, string url);

        [DllImport("__Internal")]
        protected static extern bool sendTweetWithURLAndImage(string text, string url, string image);
#endif

        public static bool CanSendTweet
        {
            get
            {
#if UNITY_IPHONE && !UNITY_EDITOR
			    return canSendTweet();
#else
                return true;
#endif
            }
        }

        public static void SendTweet(string text)
        {
            Asserts.Assert(CanSendTweet);
#if UNITY_IPHONE && !UNITY_EDITOR
			sendTweet(text);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.TWBindings"))
            {
                jc.CallStatic("sendTweet", text);
            }
#endif
        }

        public static void SendTweet(string text, string url)
        {
            Asserts.Assert(CanSendTweet);
#if UNITY_IPHONE && !UNITY_EDITOR
			sendTweetWithURL(text, url);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.TWBindings"))
            {
                jc.CallStatic("sendTweetWithUrl", text, url);
            }
#endif
        }

        public static void SendTweet(string text, string url, string image)
        {
            Asserts.Assert(CanSendTweet);
#if UNITY_IPHONE && !UNITY_EDITOR
			sendTweetWithURLAndImage(text, url, image);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.TWBindings"))
            {
                jc.CallStatic("sendTweetWithUrlAndImage", text, url, image);
            }
#endif
        }
    }
}
