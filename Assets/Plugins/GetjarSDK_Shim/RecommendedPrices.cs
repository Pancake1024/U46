#if UNITY_ANDROID

using System;
using UnityEngine;
using LitJson;
using System.Collections.Generic;

namespace GetJar.Android.SDK.Unity {
	
	/// <summary>
	/// This is a helper class that parses the recommended prices JSON response 
	/// and facilitates looking up recommended prices keyed off Pricing objects.
	/// </summary>
	public class RecommendedPrices {
		
		private Dictionary<Pricing, int> prices = new Dictionary<Pricing, int>();

		/// <summary>
		/// Initializes a new instance of the <see cref="GetJar.Android.SDK.Unity.RecommendedPrices"/> class.
		/// </summary>
		/// <param name='recommendedPricesJson'>
		/// The JSON text resulting from a call to GetJar.Android.SDK.Unity.Localization.GetRecommendedPricesAsync().
		/// </param>
		public RecommendedPrices(String recommendedPricesJson) {
	    	if(String.IsNullOrEmpty(recommendedPricesJson)) { return; }
			
			// Example JSON:
			//		[{"recommendedPrice":1,"maxDiscount":1,"maxMarkup":1,"basePrice":17},{"recommendedPrice":60,"maxDiscount":1,"maxMarkup":1,"basePrice":43}]

			JsonData data = JsonMapper.ToObject(recommendedPricesJson);
			for(int i = 0; i < data.Count; i++) {
				JsonData recPrice = data[i];
				
				JsonData basePriceObj = recPrice["basePrice"];
				JsonData maxDiscountObj = null;
				JsonData maxMarkupObj = null;

				try { maxDiscountObj = recPrice["maxDiscount"]; } catch(Exception) {}  // No-op OK
				try { maxMarkupObj = recPrice["maxMarkup"]; } catch(Exception) {}  // No-op OK
				
				// Create the correct Pricing instance
				Pricing pricing = null;
				int basePrice = int.Parse(basePriceObj.ToJson());
				if((maxDiscountObj != null) && (maxMarkupObj != null)) {
					pricing = new Pricing(basePrice, float.Parse(maxDiscountObj.ToJson()), float.Parse(maxMarkupObj.ToJson()));
				} else {
					pricing = new Pricing(basePrice);
				}

				// Add this recommended pricing record to our result list
				if(!prices.ContainsKey(pricing)) {
					prices.Add(pricing, (int)recPrice["recommendedPrice"]);
				}
			}
			
			foreach(Pricing pricing in prices.Keys) {
				int recPrice = prices[pricing];
				Debug.Log("GETJAR: RecommendedPrices: Added " + 
					pricing.GetBasePrice() + " " + 
					pricing.GetMaxDiscountPercent() + " " + 
					pricing.GetMaxMarkupPercent() + " " + 
					recPrice);
			}
		}

		/// <summary>
	    /// Takes a Pricing object that was included in the request to get recommended prices and returns the price for that particular Pricing object. 
		/// </summary>
		/// <returns>
		/// An integer denoting a recommended price or NULL if the Pricing object is not found in the original request to get the recommended prices.
		/// </returns>
		/// <param name='theLookup'>The Pricing object to get the recommended price for.</param>
	    public Nullable<int> GetRecommendedPrice(Pricing theLookup) {
	    	if(theLookup == null) { throw(new ArgumentException("theLookup cannot be null")); }
	    	return(prices[theLookup]);
	    }

		/// <summary>Returns the number of entries in the recommended prices list.</summary>
	    public int Count() {
	    	return(prices.Count);
	    }

	}

}

#endif