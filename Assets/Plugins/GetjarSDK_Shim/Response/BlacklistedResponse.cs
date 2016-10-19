#if UNITY_ANDROID

using System;

namespace GetJar.Android.SDK.Unity.Response {
	
	/// <summary>
	/// A response class indicating that the current device, user, or application has been blacklisted by GetJar.
	/// Check the BlacklistedResponse.blacklistType field to find out which. This is one of the possible Response 
	/// subclasses that can be passed back to the callback as a result of a call to GetJarPage.ShowPageAsync().
	/// </summary>
	public class BlacklistedResponse : Response {
		
		public string blacklistType;

		public BlacklistedResponse () {}

	}
}

#endif