#if UNITY_ANDROID

using System;
using System.Text;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace GetJar.Android.SDK.Unity {
	
	/// <summary>
	/// This class exposes various localization related functionality such as regional pricing, etc.
	/// </summary>
	public class Localization : IDisposable {

		private GetJarContext _getJarContext = null;
		private AndroidJavaObject _rsdkLocalization = null;

		/// <summary>
		/// This constructs a new Localization object using the current android application context.
		/// </summary>
		/// <param name='getJarContext'>The GetJarContext instance to be used by the new Localization instance.</param>
		public Localization(GetJarContext getJarContext) {
			if(getJarContext == null) { throw(new ArgumentException("'getJarContext' cannot be NULL")); }
			this._getJarContext = getJarContext;
			this._rsdkLocalization = new AndroidJavaObject("com.getjar.sdk.unity.Localization", this._getJarContext.GetGetJarContextId()); 
		}

		/// <summary>
		/// This helper method takes a collection of Pricing objects and the specified callback recieves a JSON 
		/// serialized RecommendedPrices object that contains pricing recommendations. You can query the 
		/// RecommendedPrices using the Pricing object to get the recommended price for that particular criteria.
		/// </summary>
		/// <param name='pricingList'>
		/// A Collection of Pricing objects. These Pricing objects contain the pricing criteria that you need the recommendation for.
		/// </param>
		/// <param name='gameObjectName'>The GameObject the result callback will be made to.</param>
		/// <param name='callbackMethodName'>The name of the method, registered on the given GameObject, that the result callback will be made to.</param>
		public void GetRecommendedPricesAsync(List<Pricing> pricingList, String gameObjectName, String callbackMethodName) {
			if((pricingList == null) || (pricingList.Count <= 0)) { throw(new ArgumentException("'pricingList' cannot be null or empty")); }
			if(String.IsNullOrEmpty(gameObjectName)){ throw(new ArgumentException("'gameObjectName' cannot be null or empty")); }
			if(String.IsNullOrEmpty(callbackMethodName)){ throw(new ArgumentException("'callbackMethodName' cannot be null or empty")); }
			
			// Start the async work
			//	public void getRecommendedPrices(String jsonPricingArray, final String gameObjectName, final String methodName) {
			string jsonPricingArray = this.PricingListToJson(pricingList);
			this._rsdkLocalization.Call("getRecommendedPrices", jsonPricingArray, gameObjectName, callbackMethodName);
		}

		/// <summary>
		/// Returns a JSON serialized representation of the given List of Pricing objects.
		/// </summary>
		private String PricingListToJson(List<Pricing> pricingList) {
			StringBuilder json = new StringBuilder();
	        JsonWriter writer = new JsonWriter(json);
			
			writer.WriteArrayStart();
			foreach(Pricing pricing in pricingList) {

				writer.WriteObjectStart();
				writer.WritePropertyName ("basePrice");
		        writer.Write(pricing.GetBasePrice());
				if(pricing.GetMaxDiscountPercent() != null) {
			        writer.WritePropertyName ("maxDiscount");
			        writer.Write(pricing.GetMaxDiscountPercent().Value);
				}
				if(pricing.GetMaxMarkupPercent() != null) {
			        writer.WritePropertyName ("maxMarkup");
			        writer.Write(pricing.GetMaxMarkupPercent().Value);
				}
		        writer.WriteObjectEnd();
				
			}
			writer.WriteArrayEnd();
			
			return(json.ToString());
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
		
		~Localization() {
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing) {
			if(!this._disposed) {
				if(disposing) {
					if(this._rsdkLocalization != null) {
						this._rsdkLocalization.Dispose();
					}
				}
			}
			this._disposed = true;
		}
		//------------------------------------------------------------------------------
		
	}

}

#endif