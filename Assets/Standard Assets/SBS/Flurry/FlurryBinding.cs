using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Flurry
{
    public class FlurryBinding
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void flurryLogEvent(
            string eventName,
            bool timed);

        [DllImport("__Internal")]
        private static extern void flurryEndTimedEvent(
            string eventName);

        [DllImport("__Internal")]
        private static extern void flurryLogEventWithParams(
            string eventName,
            bool timed,
            string[] keys,
            string[] values,
            int count);

        [DllImport("__Internal")]
        private static extern void flurryEndTimedEventWithNewParams(
            string eventName,
            string[] keys,
            string[] values,
            int count);
#endif

#if UNITY_WP8 && !UNITY_EDITOR
        public class LogEventArgs : EventArgs
        {
            string eventName;
            Dictionary<string, string> eventParams;
            bool timed;

            public string EventName { get { return eventName; } }
            public Dictionary<string, string> EventParams { get { return eventParams; } }
            public bool Timed { get { return timed; } }

            public LogEventArgs(string eventName, Dictionary<string, string> eventParams, bool timed)
            {
                SetParameters(eventName, eventParams, timed);
            }

            public LogEventArgs(string eventName)
            {
                SetParameters(eventName, null, false);
            }

            public LogEventArgs(string eventName, Dictionary<string, string> eventParams)
            {
                SetParameters(eventName, eventParams, false);
            }

            public LogEventArgs(string eventName, bool timed)
            {
                SetParameters(eventName, null, timed);
            }

            void SetParameters(string eventName, Dictionary<string, string> eventParams, bool timed)
            {
                this.eventName = eventName;
                this.eventParams = eventParams;
                this.timed = timed;
            }
        }

        public class EndTimedEventArgs : EventArgs
        {
            string eventName;
            Dictionary<string, string> newParams;

            public string EventName { get { return eventName; } }
            public Dictionary<string, string> NewParams { get { return newParams; } }

            public EndTimedEventArgs(string eventName, Dictionary<string, string> newParams = null)
            {
                this.eventName = eventName;
                this.newParams = newParams;
            }
        }

        public delegate void FlurryEventHandler(EventArgs args);
        public static event  FlurryEventHandler LogEvent_WP8 = delegate { };
        public static event  FlurryEventHandler EndTimedEvent_WP8 = delegate { };
#endif

        public static void logEvent(string eventName)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            flurryLogEvent(eventName, false);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                jc.CallStatic("logEvent", eventName);
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            LogEvent_WP8(new LogEventArgs(eventName));
#endif
        }

        public static void logEvent(string eventName, Dictionary<string, string> eventParams)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            int index = 0,
                count = eventParams.Count;
            string[] keys = new string[count];
            string[] values = new string[count];
            foreach (KeyValuePair<string, string> param in eventParams)
            {
                keys[index] = param.Key;
                values[index++] = param.Value;
            }
            flurryLogEventWithParams(eventName, false, keys, values, count);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                using (AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap"))
                {
                    foreach (KeyValuePair<string, string> param in eventParams)
                        map.Call<AndroidJavaObject>("put", param.Key, param.Value);

                    jc.CallStatic("logEvent", eventName, map);
                }
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            LogEvent_WP8(new LogEventArgs(eventName, eventParams));
#endif
        }

        public static void logEvent(string eventName, bool timed)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            flurryLogEvent(eventName, timed);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                jc.CallStatic("logEvent", eventName, timed);
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            LogEvent_WP8(new LogEventArgs(eventName, timed));
#endif
        }

        public static void logEvent(string eventName, Dictionary<string, string> eventParams, bool timed)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            int index = 0,
                count = eventParams.Count;
            string[] keys = new string[count];
            string[] values = new string[count];
            foreach (KeyValuePair<string, string> param in eventParams)
            {
                keys[index] = param.Key;
                values[index++] = param.Value;
            }
            flurryLogEventWithParams(eventName, timed, keys, values, count);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                using (AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap"))
                {
                    foreach (KeyValuePair<string, string> param in eventParams)
                        map.Call<AndroidJavaObject>("put", param.Key, param.Value);

                    jc.CallStatic("logEvent", eventName, map, timed);
                }
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            LogEvent_WP8(new LogEventArgs(eventName, eventParams, timed));
#endif
        }

        public static void endTimedEvent(string eventName)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            flurryEndTimedEvent(eventName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                jc.CallStatic("endTimedEvent", eventName);
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            EndTimedEvent_WP8(new EndTimedEventArgs(eventName));
#endif
        }

        public static void endTimedEvent(string eventName, Dictionary<string, string> newParams)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            int index = 0,
                count = newParams.Count;
            string[] keys = new string[count];
            string[] values = new string[count];
            foreach (KeyValuePair<string, string> param in newParams)
            {
                keys[index] = param.Key;
                values[index++] = param.Value;
            }
            flurryEndTimedEventWithNewParams(eventName, keys, values, count);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.flurry.android.FlurryAgent"))
            {
                using (AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap"))
                {
                    foreach (KeyValuePair<string, string> param in newParams)
                        map.Call<AndroidJavaObject>("put", param.Key, param.Value);

                    jc.CallStatic("endTimedEvent", eventName, map);
                }
            }
#elif UNITY_WP8 && !UNITY_EDITOR
            EndTimedEvent_WP8(new EndTimedEventArgs(eventName, newParams));
#endif
        }
    }
}
