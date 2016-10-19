using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
    public class FBBindings
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		protected static extern void facebookCheckLogin();
		
		[DllImport("__Internal")]
		protected static extern void facebookCheckLoginWithRedirect(string nextURL);

		[DllImport("__Internal")]
		protected static extern void facebookRequestGraphPath(string path);
		
		[DllImport("__Internal")]
		protected static extern void facebookLogout();

		[DllImport("__Internal")]
		protected static extern void facebookOpenDialog(
			string action,
			string[] keys,
            string[] values,
            int count);
#endif
		public static void CheckLogin()
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			facebookCheckLogin();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.FBBindings"))
            {
                jc.CallStatic("checkLogin");
            }
#endif
        }
		
		public static void CheckLogin(string nextURL)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			facebookCheckLoginWithRedirect(nextURL);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.FBBindings"))
            {
                jc.CallStatic("checkLoginWithRedirect", nextURL);
            }
#endif
        }

		public static void RequestGraphPath(string path)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			facebookRequestGraphPath(path);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.FBBindings"))
            {
                jc.CallStatic("requestGraphPath", path);
            }
#endif
        }
		
        public static void OpenDialog(string dialogAction, Dictionary<string, string> dialogParams)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            int index = 0,
                count = dialogParams.Count;
            string[] keys = new string[count];
            string[] values = new string[count];
            foreach (KeyValuePair<string, string> param in dialogParams)
            {
                keys[index] = param.Key;
                values[index++] = param.Value;
            }
            facebookOpenDialog(dialogAction, keys, values, count);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.FBBindings"))
            {
                using (AndroidJavaObject bundle = new AndroidJavaObject("android.os.Bundle"))
                {
                    foreach (KeyValuePair<string, string> param in dialogParams)
                        bundle.Call("putString", param.Key, param.Value);

                    jc.CallStatic("openDialog", dialogAction, bundle);
                }
            }
#endif
        }

		public static void Logout()
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			facebookLogout();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.FBBindings"))
            {
                jc.CallStatic("logout");
            }
#endif
        }
	}
}
