#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System.Collections;
using Prime31;

public class GPGLeaderboardMetadata
{
	public string title;
	public double order;
	public string leaderboardId;
	public string iconUrl;
}
#endif
