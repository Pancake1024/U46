#if UNITY_ANDROID

using System;
using UnityEngine;

namespace GetJar.Android.SDK.Unity {
	
	/// <summary>
	/// This class supports configuring and displaying the GetJar Rewards UI.
    /// This includes configuring the products that you are offering for sale.
	/// </summary>
	public class GetJarPage : IDisposable {
		
		private GetJarContext _getJarContext = null;
		private AndroidJavaObject _rsdkGetJarPage = null;
		
		/// <summary>
		/// This constructs a new GetJarPage object using the current android application context.
		/// </summary>
		/// <param name='getJarContext'>The GetJarContext instance to be used by the new GetJarPage instance.</param>
		public GetJarPage(GetJarContext getJarContext) {
			if(getJarContext == null) { throw(new ArgumentException("'getJarContext' cannot be NULL")); }
			this._getJarContext = getJarContext;
			this._rsdkGetJarPage = new AndroidJavaObject("com.getjar.sdk.unity.GetJarPage", this._getJarContext.GetGetJarContextId()); 
		}

		/// <summary>
		/// This is intended to be used if your product requires a License to be associated with it. This will generate a License 
		/// associated with the current user and the product when the user completes the transaction. This method configures this 
		/// GetJarPage instance to make a Product available for purchase with the given product details. Product IDs must be between 
		/// 1 and 36 characters in length and contain only characters A-Z, a-z, 0-9, '_', or '.'.
		/// </summary>
		/// <param name='theProductId'>
		/// Your product ID for the product your are offering. This needs to be UNIQUE for the app.
		/// Product IDs must be between 1 and 36 characters in length and contain only characters A-Z, a-z, 0-9, '_', or '.'.
		/// </param>
		/// <param name='theProductName'>Your product name for the product your are offering. This should not be NULL or empty.</param>
		/// <param name='theProductDescription'>Your description of the product you are offering.</param>
		/// <param name='theAmount'>The amount (In GetJar gold coins) that your product costs. This cannot be less than zero.</param>
		/// <param name='licenseScope'>The LicenseScope for the product. This scope indicates the various levels at which the user can access this virtual good.</param>
		public void SetLicensableProduct(String theProductId, String theProductName, String theProductDescription, long theAmount, LicenseScope licenseScope) {
	        if(String.IsNullOrEmpty(theProductId)) { throw(new ArgumentException("'theProductId' cannot be NULL or empty")); }
	        if(String.IsNullOrEmpty(theProductName)) { throw(new ArgumentException("'theProductName' cannot be NULL or empty")); }
    	    if(theAmount < 0) { throw(new ArgumentException("'theAmount' cannot be less than zero")); }

			// Start the async work
			this._rsdkGetJarPage.Call("setLicensableProduct", theProductId, theProductName, theProductDescription, theAmount, licenseScope.ToString());
		}
		
		/// <summary>
		/// This method configures this GetJarPage instance to make a Product available for purchase with the given product details.
		/// Product IDs must be between 1 and 36 characters in length and contain only characters A-Z, a-z, 0-9, '_', or '.'.
		/// </summary>
		/// <param name='theProductId'>
		/// Your product ID for the product your are offering. This needs to be UNIQUE for the app.
		/// Product IDs must be between 1 and 36 characters in length and contain only characters A-Z, a-z, 0-9, '_', or '.'.
		/// </param>
		/// <param name='theProductName'>Your product name for the product your are offering. This should not be NULL or empty.</param>
		/// <param name='theProductDescription'>Your description of the product you are offering.</param>
		/// <param name='theAmount'>The amount (In GetJar gold coins) that your product costs. This cannot be less than zero.</param>
		public void SetConsumableProduct(String theProductId, String theProductName, String theProductDescription, long theAmount) {
	        if(String.IsNullOrEmpty(theProductId)) { throw(new ArgumentException("'theProductId' cannot be NULL or empty")); }
	        if(String.IsNullOrEmpty(theProductName)) { throw(new ArgumentException("'theProductName' cannot be NULL or empty")); }
    	    if(theAmount < 0) { throw(new ArgumentException("'theAmount' cannot be less than zero")); }

			// Start the async work
			this._rsdkGetJarPage.Call("setConsumableProduct", theProductId, theProductName, theProductDescription, theAmount);
		}
		
		/// <summary>
		/// Sets the public key provided by Google Play to ensure integrity of the transaction. 
		/// This key can be found in your developer profile on Google Play.
		/// </summary>
		/// <param name='publicKey'>The Base64-encoded RSA public key from the Licensing & In-app Billing section on your Google Play developer console.</param>
		public void SetGoogleInAppBillingKey(String publicKey) {
	        if(String.IsNullOrEmpty(publicKey)) { throw(new ArgumentException("'publicKey' cannot be NULL or empty")); }
			this._rsdkGetJarPage.Call("setGoogleInAppBillingKey", publicKey);
		}
		
		/// <summary>
		/// Opens the GetJar Rewards UI (the GetJar WebView Activity). The results of the UI interaction such as a
		/// successful purchase, the UI being closed, etc. will be sent to the call-back that you provide here.
		/// </summary>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void ShowPageAsync(String gameObjectName, String callbackMethodName) {
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }

			// Start the async work
			this._rsdkGetJarPage.Call("showPage", gameObjectName, callbackMethodName);
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
		
		~GetJarPage() {
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if(!this._disposed) {
				if(disposing) {
					if(this._rsdkGetJarPage != null) {
						this._rsdkGetJarPage.Dispose();
					}
				}
			}
			this._disposed = true;
		}
		//------------------------------------------------------------------------------

	}

}

#endif