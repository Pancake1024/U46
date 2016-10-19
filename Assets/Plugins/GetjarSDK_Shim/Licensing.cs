#if UNITY_ANDROID

using System;
using UnityEngine;

namespace GetJar.Android.SDK.Unity {
	
	/// <summary>
	/// The possible valid values for license scope.
	/// </summary>
	public enum LicenseScope {
		/// Indicates a license to a specific user.
		USER,
		/// Indicates a license to a specific user on a specific platform.
		PLATFORM,
		/// Indicates a license to a specific user on a specific device.
		DEVICE
	}

	/// <summary>
	/// This class exposes various licensing related functionality such as license acquisition, querying for licenses, etc.
	/// </summary>
	public class Licensing : IDisposable {

		private GetJarContext _getJarContext = null;
		private AndroidJavaObject _rsdkLicensing = null;

		/// <summary>
		/// This constructs a new Licensing object using the current android application context.
		/// </summary>
		/// <param name='getJarContext'>The GetJarContext instance to be used by the new Licensing instance.</param>
		public Licensing(GetJarContext getJarContext) {
			if(getJarContext == null) { throw(new ArgumentException("'getJarContext' cannot be NULL")); }
			this._getJarContext = getJarContext;
			this._rsdkLicensing = new AndroidJavaObject("com.getjar.sdk.unity.Licensing", this._getJarContext.GetGetJarContextId()); 
		}

		/// <summary>Gets the existing license for the current user and the provided itemId.</summary>
		/// <param name='itemId'>The unique ID of the item to get the License for</param>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void IsUnmanagedProductLicensedAsync(String itemId, String gameObjectName, String callbackMethodName) {
			if(String.IsNullOrEmpty(itemId)){ throw(new ArgumentException("'itemId' cannot be null or empty")); }
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }

			// Start the async work
			this._rsdkLicensing.Call("isUnmanagedProductLicensed", itemId, gameObjectName, callbackMethodName);
		}

		/// <summary>Creates a new License or return an existing License for the provided combination of parameters.</summary>
		/// <param name='itemId'>The unique ID of the virtual item that the License is required for.</param>
		/// <param name='licenseScope'>The LicenseScope required for the License.</param>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void AcquireUnmanagedProductLicenseAsync(String itemId, LicenseScope licenseScope, String gameObjectName, String callbackMethodName) {
			if(String.IsNullOrEmpty(itemId)){ throw(new ArgumentException("'itemId' cannot be null or empty")); }
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }

			// Start the async work
			this._rsdkLicensing.Call("acquireUnmanagedProductLicense", itemId, licenseScope.ToString(), gameObjectName, callbackMethodName);
		}
		
		/// <summary>Gets the existing licenses for the current user and the provided itemIds.</summary>
		/// <param name='itemIds'>The unique IDs of the items to get the License objects for.</param>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void GetUnmanagedProductLicensesAsync(String[] itemIds, String gameObjectName, String callbackMethodName) {
			if((itemIds == null) || (itemIds.Length <= 0)) { throw(new ArgumentException("'itemIds' cannot be null or empty")); }
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }

			// Start the async work
			this._rsdkLicensing.Call("getUnmanagedProductLicenses", itemIds, gameObjectName, callbackMethodName);
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
		
		~Licensing() {
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if(!this._disposed) {
				if(disposing) {
					if(this._rsdkLicensing != null) {
						this._rsdkLicensing.Dispose();
					}
				}
			}
			this._disposed = true;
		}
		//------------------------------------------------------------------------------

	}
	
}

#endif