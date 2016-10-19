#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System;
using System.Collections;

public class GPGQuest
{
	public string questId;
	public string name;
	public string questDescription;
	public string iconUrl;
	public string bannerUrl;
	public int state;
	public GPGQuestState stateEnum
	{
		get { return (GPGQuestState)state; }
	}
	public DateTime startTimestamp;
	public DateTime expirationTimestamp;
	public DateTime acceptedTimestamp;
	public GPGQuestMilestone currentMilestone;
}
#endif