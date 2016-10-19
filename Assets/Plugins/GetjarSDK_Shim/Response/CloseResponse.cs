#if UNITY_ANDROID

using System;

namespace GetJar.Android.SDK.Unity.Response {

	/// <summary>
	/// A response class indicating that the user closed the SDK UI.
	/// This is one of the possible Response subclasses that can be passed back to the callback 
	/// as a result of a call to GetJarPage.ShowPageAsync().
	/// </summary>
	public class CloseResponse : Response {
		public CloseResponse () {}
	}
	
}

#endif