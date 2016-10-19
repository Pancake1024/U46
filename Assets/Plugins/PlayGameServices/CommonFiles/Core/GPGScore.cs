#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System.Collections;
using Prime31;

public class GPGScore
{
	public string leaderboardId;
	public double rank;
	public double writeTimestamp;
	public string avatarUrl;
	public string avatarUrlHiRes; // Android only
	public string formattedRank;
	public long value;
	public string playerId;
	public string formattedScore;
	public string displayName;
}
#endif
