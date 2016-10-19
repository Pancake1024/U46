using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CoinsServiceBindings
{
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		protected static extern void csStart();

		[DllImport("__Internal")]
		protected static extern void csSetDialogText(string textType, string localizedText);

		[DllImport("__Internal")]
		protected static extern void csShow_noDialogAtEnd();

        [DllImport("__Internal")]
		protected static extern void csShow_withDialogAtEnd(int remainingVideos);
		
		[DllImport("__Internal")]
		protected static extern bool csAreVideoAdsAvailable();
		
		[DllImport("__Internal")]
		protected static extern int getNumberOfVideoAdsCoins();
#endif

    public enum SystemDialogTextType
    {
        TitleCongratulations,
        TitleSorry,
        MessageWatchMore,
        MessageCurrentlyNoMoreAvailable,
        MessageCurrentlyNotAvailable,
        ButtonLater,
        ButtonWatchNow,
        ButtonOk
    }

    public static void Start()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		csStart();
#endif
    }

    public static void SetDialogLocalizedText(SystemDialogTextType type, string localizedText)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		csSetDialogText(type.ToString(), localizedText);
#endif
	}
	
	public static void Show_noDialogAtEnd()
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		csShow_noDialogAtEnd();
#endif
	}
	
	public static void Show_withDialogAtEnd(int remainingVideos)
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		csShow_withDialogAtEnd(remainingVideos);
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