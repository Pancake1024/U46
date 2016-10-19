#if UNITY_ANDROID

using System;
using UnityEngine;

namespace GetJar.Android.SDK.Unity {

	/// <summary>
	/// This class provides mechanisms to ensure that the user is properly authorized and authenticated for using GetJar.
	/// </summary>
	public class UserAuth : IDisposable {
			
		private GetJarContext _getJarContext = null;
		private AndroidJavaObject _rsdkUserAuth = null;

		/// <summary>
		/// This constructs a new UserAuth object using the current android application context.
		/// </summary>
		/// <param name='getJarContext'>The GetJarContext instance to be used by the new UserAuth instance.</param>
		public UserAuth(GetJarContext getJarContext) {
			if(getJarContext == null) { throw(new ArgumentException("'getJarContext' cannot be NULL")); }
			this._getJarContext = getJarContext;
			this._rsdkUserAuth = new AndroidJavaObject("com.getjar.sdk.unity.UserAuth", this._getJarContext.GetGetJarContextId()); 
		}

		/// <summary>
		/// This method ensures that a valid user is available for use with the GetJar SDK. It will open the GetJar UI to allow the user 
		/// to pick between multiple accounts if more than one gmail account is available on the device. The provide "userAuthCallback" 
		/// parameter specifies the callback to be made with the results when work finishes.
		/// <p>
		/// This is a Non-blocking method. 
		/// </summary>
		/// <param name='theTitle'>The title that is displayed when the user is prompted to pick between multiple accounts.</param>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void EnsureUserAsync(String theTitle, String gameObjectName, String callbackMethodName) {
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }

			// Start the async work
			this._rsdkUserAuth.Call("ensureUser", theTitle, gameObjectName, callbackMethodName);
		}
		
		//------------------------------------------------------------------------------
        // Implement IDisposable pattern
		private Boolean _disposed = false;

		public void Dispose() {
			this.Close();
		}

		public void Close() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		~UserAuth() {
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if(!this._disposed) {
				if(disposing) {
					if(this._rsdkUserAuth != null) {
						this._rsdkUserAuth.Dispose();
					}
				}
			}
			this._disposed = true;
		}
		//------------------------------------------------------------------------------

	}

}

#endif