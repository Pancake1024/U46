#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System;
using System.Collections;

public class GPGEvent
{
	public Int64 count;
	public string eventDescription;
	public string eventId;
	public string imageUrl;
	public string name;
	public bool visible;
}
#endif