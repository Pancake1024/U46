#if UNITY_ANDROID

using System;

namespace GetJar.Android.SDK.Unity {

	/// <summary>
	/// This is <b>THE</b> GetJarContext, used by developers for interacting with the GetJar SDK.
	/// Use GetJarManager to create and retrieve instances. The GetJarManager 
	/// documentation also contains simple example code of integrating with the GetJar SDK.
	/// </summary>
	public class GetJarContext {

		private String _getJarContextId = null;

		internal GetJarContext(String getJarContextId) {
			this._getJarContextId = getJarContextId;
		}

		/// <summary>Returns the unique ID for this GetJarContext instance.</summary>
		public String GetGetJarContextId() {
			return(this._getJarContextId);
		}

	}
	
}

#endif