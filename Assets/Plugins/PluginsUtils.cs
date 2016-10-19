using UnityEngine;
using System.Collections;
#if UNITY_IPHONE
using System.Runtime.InteropServices;
#endif

public static class PluginsUtils
{
#if UNITY_IPHONE && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern bool _deviceIsPad();
#endif

    public static bool DeviceIsTablet()
    {
#if UNITY_EDITOR
		return false;
#elif (UNITY_WEBPLAYER || UNITY_WEBGL)
		return false;
#elif UNITY_IPHONE
        return _deviceIsPad();
#elif UNITY_ANDROID
		return false;
        //return AndroidUtils.GetVersionName();
#else
        reutrn false;
#endif
    }
}