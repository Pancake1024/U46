using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
    public class UtilsBindings
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		protected static extern bool _isMusicPlaying();

		[DllImport("__Internal")]
		protected static extern void _consoleLog(string text);
#endif
        public static bool IsMusicPlaying()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			return _isMusicPlaying();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return false;
#else
            return false;
#endif
        }

		public static void ConsoleLog(string text)
		{
			Debug.Log(text);
			#if UNITY_IPHONE && !UNITY_EDITOR
			_consoleLog(text);
			#elif UNITY_ANDROID && !UNITY_EDITOR
			#endif
		}
    }
}
