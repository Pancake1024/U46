#if UNITY_ANDROID

using System;
using UnityEngine;
using System.Text;

namespace GetJar.Android.SDK.Unity {
	
	/// <summary>
	/// This class is used to get the recommended price for the given criteria. You can set the criteria in terms of
	/// base price for the product, the maximum discount percent and the maximum markup percent.
	/// </summary>
	public class Pricing {
		
	    private int basePrice;							// can be greater than or equal to zero
	    private Nullable<float> maxDiscount = null;	// a value between 0 and 1 only, inclusively
    	private Nullable<float> maxMarkup = null;		// a value between 0 and 2 only, inclusively
		
		private int hashCode;

		/// <summary>
		/// Initializes a new instance of the <see cref="GetJar.Android.SDK.Unity.Pricing"/> class for the provided value. 
		/// This uses the server determined max discount percent and max markup percent.
		/// </summary>
		/// <param name='basePrice'>the base price for the product</param>
    	public Pricing(int theBasePrice) {
    		if(theBasePrice < 0){ throw(new ArgumentException("'theBasePrice' must be greater than or equal to 0")); }
    	  	this.basePrice = theBasePrice;
			this.UpdateHashCode();
    	}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GetJar.Android.SDK.Unity.Pricing"/> class for the provided values.
		/// If the OPTIONAL parameters are not provided then server determined values will be used.
		/// </summary>
		/// <param name='theBasePrice'>the base price for the product</param>
		/// <param name='theMaxDiscountPercent'>[OPTIONAL] this max discount percent should be between 0 and 1, inclusive</param>
		/// <param name='theMaxMarkupPercent'>[OPTIONAL] this max markup percent should be between 0 and 2, inclusive</param>
		public Pricing(int theBasePrice, Nullable<float> theMaxDiscountPercent, Nullable<float> theMaxMarkupPercent) : this(theBasePrice) {
	    	if((theMaxDiscountPercent != null) && ((theMaxDiscountPercent < 0) || (theMaxDiscountPercent > 1))) {
				throw(new ArgumentException("'theMaxDiscountPercent' must be between 0 and 1"));
			}
	    	if((theMaxMarkupPercent != null) && ((theMaxMarkupPercent < 0) || (theMaxMarkupPercent > 2))){
				throw(new ArgumentException("'theMaxMarkupPercent' must be between 0 and 2"));
			}
			this.maxDiscount = theMaxDiscountPercent;
			this.maxMarkup = theMaxMarkupPercent;
			this.UpdateHashCode();
		}
		
		/// <summary>Gets the base price.</summary>
		public int GetBasePrice() { return(this.basePrice); }
		
		/// <summary>Gets the max discount percent.</summary>
		public Nullable<float> GetMaxDiscountPercent() { return(this.maxDiscount); }
		
		/// <summary>Gets the max markup percent.</summary>
		public Nullable<float> GetMaxMarkupPercent() { return(this.maxMarkup); }

		// Deal with hash code, equality, identity, etc
		private void UpdateHashCode() {
			
			StringBuilder sb = new StringBuilder();
			sb.Append(this.basePrice);
			if((maxDiscount != null) && (maxDiscount.HasValue)) {
				sb.Append(this.maxDiscount);
			}
			if((maxMarkup != null) && (maxMarkup.HasValue)) {
				sb.Append(this.maxMarkup);
			}
			this.hashCode = sb.ToString().GetHashCode();
		}
		
	    public override int GetHashCode() {
	        return(this.hashCode);
	    }
		
	    public override bool Equals(object obj) {
			if((obj == null) || (!(obj is Pricing))) { return(false); }
			return(this.hashCode == obj.GetHashCode());
	    }

	}

}

#endif