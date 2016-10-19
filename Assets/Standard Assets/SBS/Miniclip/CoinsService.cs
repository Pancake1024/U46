using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SBS.Miniclip
{
	public class CoinsService
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		protected static extern void csStart();

        [DllImport("__Internal")]
		protected static extern void csShow(bool showDialogOnVideoEnd);
		
		[DllImport("__Internal")]
		protected static extern bool csAreVideoAdsAvailable();
		
		[DllImport("__Internal")]
		protected static extern int getNumberOfVideoAdsCoins();
#endif

        public static void Start()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            csStart();
#endif
        }
		
		public static void Show(bool showDialogOnVideoEnd)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			csShow(showDialogOnVideoEnd);
#endif
		}

		public static bool AreVideoAdsAvailable()
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			return csAreVideoAdsAvailable();
#endif
			return false;
		}

        public static int GetFreeCoinsReward()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			return getNumberOfVideoAdsCoins();
#endif
            return 50;
        }
    }
}
