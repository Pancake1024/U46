using System;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Core
{
	public class iOSUtils
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern string getVersionString();

        [DllImport("__Internal")]
		private static extern string ntpGetNetworkDate();
		[DllImport("__Internal")]
		private static extern double ntpGetTimeOffsetInSeconds();
		[DllImport("__Internal")]
		private static extern int ntpGetUsefulAssociationsCount();
#endif

        public static string _YenHack(string input)
        {
#if UNITY_WP8
            return input;
#else
            byte[] _yenBytes = { 0xEF, 0xBF, 0xA5 };
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);

            int i = 0, c = bytes.Length, j = 0;
            for (; i < c; ++i)
            {
                if (bytes[i] == _yenBytes[j])
                {
                    ++j;

                    if (j == _yenBytes.Length)
                    {
                        bytes[i - 2] = 0x20;
                        bytes[i - 1] = 0xC2;
                        bytes[i - 0] = 0xA5;

                        j = 0;
                    }
                }
                else
                {
                    j = 0;
                }
            }

            return System.Text.Encoding.UTF8.GetString(bytes);
#endif
        }

        public static string GetAppVersion()
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			return getVersionString();
#else
			return "1.0";
#endif
		}

        public static DateTime GetNetworkDate()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return DateTime.Parse(ntpGetNetworkDate());
#elif UNITY_ANDROID && !UNITY_EDITOR
            string netDate = null;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ntp.NTPBindings"))
            {
                netDate = jc.CallStatic<string>("ntpGetNetworkDate");
                Debug.Log("netDate: " + netDate);
            }
            if (null == netDate)
                return DateTime.Now;
            else
                return DateTime.ParseExact(netDate, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);//DateTime.Parse(netDate);
#else
            return DateTime.Now;
#endif
        }

        public static double GetNetworkTimeOffset()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return ntpGetTimeOffsetInSeconds();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ntp.NTPBindings"))
            {
                return jc.CallStatic<double>("ntpGetTimeOffsetInSeconds");
            }
#else
            return 0.0;
#endif
        }

        public static bool IsNetworkDateValid
        {
            get
            {
#if UNITY_IPHONE && !UNITY_EDITOR
                return ntpGetUsefulAssociationsCount() >= 2;//8;
#elif UNITY_ANDROID && !UNITY_EDITOR
                using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ntp.NTPBindings"))
                {
                    int c = jc.CallStatic<int>("ntpGetResponcesCount");
                    return c > 0;
                }
#else
                return true;
#endif
            }
        }
    }
}

