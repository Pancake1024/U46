using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using SBS.Core;

namespace SBS.Miniclip
{
    public class LNBindings
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        protected static extern void cancelAllLocalNotifications();

        [DllImport("__Internal")]
        protected static extern void scheduleLocalNotification(string text, string action, int delay);
#endif

        public static void CancelAllLocalNotifications()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			cancelAllLocalNotifications();
#elif UNITY_ANDROID && !UNITY_EDITOR            
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.LNBindings"))
        {
            jc.CallStatic("cancelAllLocalNotifications");
        }
#endif
        }

        public static void ScheduleLocalNotification(string text, string action, int delay, string title /*= ""*/)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			scheduleLocalNotification(text, action, delay);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.LNBindings"))
        {
            jc.CallStatic("scheduleLocalNotification", title, text, delay);
        }
#endif
        }
    }
}
