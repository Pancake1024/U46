#if (UNITY_IPHONE && ENABLE_GOOGLEPLAY_ON_IOS) || UNITY_ANDROID

using UnityEngine;
using System.Collections;

public class GPGTurnBasedInvitation
{
	public GPGTurnBasedParticipant invitingParticipant;
	public GPGTurnBasedMatch match;


	public override string ToString()
	{
		return Prime31.JsonFormatter.prettyPrint( Prime31.Json.encode( this ) );
	}
}
#endif