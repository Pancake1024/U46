using System;
using System.Collections;
using UnityEngine;

public static class AndroidUtils
{
    public static string GetVersionName()
    {
#if UNITY_EDITOR || !UNITY_ANDROID
        return "1.0";
#else
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.GameActivity"))
        {
            return jc.CallStatic<string>("getVersionName");
        }
#endif
    }

    public static bool HasFocus()
    {
#if UNITY_EDITOR || !UNITY_ANDROID
        return true;
#else
        bool unityHasFocus = true;
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.GameActivity"))
        {
            unityHasFocus = jc.CallStatic<bool>("unityHasFocus");
        }
        return unityHasFocus;
#endif
    }

    public static void Quit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.GameActivity"))
        {
            jc.CallStatic("unityQuit");
        }
#endif
    }
}
