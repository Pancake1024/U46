using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
	public class MCUtilsBindings
	{
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        protected static extern void MCUtilsGetMessagesCount(ref int unread, ref int count);
        [DllImport("__Internal")]
        protected static extern void MCUtilsShowBoard();
        [DllImport("__Internal")]
        protected static extern bool MCUtilsShowUrgentBoard();
        [DllImport("__Internal")]
        protected static extern void MCUtilsDismissBoard();
		[DllImport("__Internal")]
		protected static extern void MCUtilsSetInGameFlag(bool flag);
		[DllImport("__Internal")]
		protected static extern bool _isMassiveTestBuild();
		[DllImport("__Internal")]
       	protected static extern void MCUtilsShowCorporateInfo();
		[DllImport("__Internal")]
        protected static extern void MCUtilsShowMoreGames();
		[DllImport("__Internal")]
        protected static extern string MCUtilsGetUniqueDeviceId();
#endif
        public static bool isMassiveTestBuild()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return _isMassiveTestBuild();
#else
			return true;
#endif
        }

        public static void GetMessagesCount(ref int unread, ref int count)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            MCUtilsGetMessagesCount(ref unread, ref count);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                unread = jc.CallStatic<int>("getUnreadMessages");
                count = jc.CallStatic<int>("getTotalMessages");
            }
#endif
        }

        public static void ShowBoard()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            MCUtilsShowBoard();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                jc.CallStatic("showBoard");
            }
#endif
        }

        public static bool ShowUrgentBoard()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return MCUtilsShowUrgentBoard();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                jc.CallStatic("showUrgentBoard");
            }
            return true;
#else
            return false;
#endif
        }

        public static void DismissBoard()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            MCUtilsDismissBoard();
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                jc.CallStatic("dismissBoard");
            }
#endif
        }
		
		public static void SetInGameFlag(bool flag)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			MCUtilsSetInGameFlag(flag);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                jc.CallStatic("setInGameFlag", flag ? 1 : 0);
            }
#endif
        }

        public static void ShowCorporateInfo()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            MCUtilsShowCorporateInfo();
#endif
        }

        public static void ShowMoreGames()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            MCUtilsShowMoreGames();
#endif
        }

        public static string GetUniqueDeviceId()
        {
            string deviceId = string.Empty;
#if UNITY_IPHONE && !UNITY_EDITOR
            deviceId = MCUtilsGetUniqueDeviceId();
#elif UNITY_ANDROID && !UNITY_EDITOR
            /*
            using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.MCUtilsBindings"))
            {
                jc.CallStatic("", );
            }
            */
#endif
            return deviceId;
        }
    }
}
