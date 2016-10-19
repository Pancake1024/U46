using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
	public class WebViewBindings
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void openWebView(
			string url,
			float x, float y, float width, float height,
			bool withBackButton,
			bool pauseUnity);

		[DllImport("__Internal")]
		private static extern void closeWebView();
#endif
		public static void OpenURL(string url, Rect rect)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			openWebView(url, rect.x, rect.y, rect.width, rect.height, true, true);
#endif
		}

		public static void OpenURL(string url, Rect rect, bool withBackButton)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			openWebView(url, rect.x, rect.y, rect.width, rect.height, withBackButton, withBackButton);
#endif
		}

		public static void OpenURL(string url, Rect rect, bool withBackButton, bool pauseUnity)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			openWebView(url, rect.x, rect.y, rect.width, rect.height, withBackButton, pauseUnity);
#endif
		}
		
		public static void CloseOpenedURL()
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			closeWebView();
#endif
		}
	}
}
