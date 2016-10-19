#if UNITY_ANDROID

using System;

namespace GetJar.Android.SDK.Unity.Response {
	
	/// <summary>
	/// A response class indicating that a purchase operation succeeded.
	/// This is one of the possible Response subclasses that can be passed back to the callback 
	/// as a result of a call to GetJarPage.ShowPageAsync().
	/// </summary>
	public class PurchaseSucceededResponse : Response {

		public int amount;
		public string transactionId;
		public string productName;
		public string productId;

		public PurchaseSucceededResponse () {}
	}
}

#endif