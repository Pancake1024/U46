#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System;
using System.Collections;

public class GPGQuestMilestone
{
	public string questMilestoneId;
	public string questId;
	public int state;
	public GPGQuestMilestoneState stateEnum
	{
		get { return (GPGQuestMilestoneState)state; }
	}
	public Int64 currentCount;
	public Int64 targetCount;
	public string rewardData;
}
#endif