using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
    public class AlertViewBindings
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void openAlertBox(
            string title,
            string message,
            string button,
            string[] otherButtons,
            int otherButtonsCount,
            bool pauseUnity);
#endif
        public static void AlertBox(string title, string message)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			openAlertBox(title, message, "OK", null, 0, true);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            {
                jc.CallStatic("open", title, message, "OK", null, 0, true);
            }
#endif
        }

        public static void AlertBox(string title, string message, string button)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			openAlertBox(title, message, button, null, 0, true);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            {
                jc.CallStatic("open", title, message, button, null, 0, true);
            }
#endif
        }

        public static void AlertBox(string title, string message, string button, string[] otherButtons)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			openAlertBox(title, message, button, otherButtons, null == otherButtons ? 0 : otherButtons.Length, true);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            {
                jc.CallStatic("open", title, message, button, otherButtons, otherButtons.Length, true);
            }
#endif
        }

        public static void AlertBox(string title, string message, string button, string[] otherButtons, bool pauseUnity)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			openAlertBox(title, message, button, otherButtons, null == otherButtons ? 0 : otherButtons.Length, pauseUnity);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.AlertDialogBindings"))
            {
                jc.CallStatic("open", title, message, button, otherButtons, otherButtons.Length, pauseUnity ? 1 : 0);
            }
#endif
        }
    }
}
