using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyPromotion
	{
		private int 	_currency_id			= 0;
		private DateTime  _starts_at;
		private DateTime  _expires_at;
		private decimal _standard_amount		= 0.0M;
		private decimal _standard_price			= 0.0M;
		private decimal _standard_free_amount	= 0.0M;
		private decimal	_standard_display_price = 0.0M;
		
		
		/**
		 * Unique ID of the currency in which this price is presented
		 */
		public int currencyId
		{
			get{ return _currency_id; }
		}
		
		
		/**
		 * Promotion start date "yyyy-mm-dd hh:mm:ss";
		 */
		public DateTime startsAt
		{
			get{ return _starts_at; }
		}
		
		/**
		 * Promotion expire date "yyyy-mm-dd hh:mm:ss";
		 */
		public DateTime expiresAt
		{
			get{ return _expires_at; }
		}
		
		/**
		 * Numerical value of this standard amount(non-promotional amount)
		 */
		public decimal standardAmount
		{
			get{ return _standard_amount; }
		}
		
		/**
		 * Numerical value of this standard price (non-promotional price)
		 */
		public decimal standardPrice
		{
			get{ return _standard_price; }
		}
		
		/**
		 * Numerical value of this standard free amount 
		 */
		public decimal standardFreeAmount
		{
			get{ return _standard_free_amount; }
		}
		
		/**
		 * Printable value of this standard price (including
		 * currency symbol, formatting etc
		 */
		public decimal standardDisplayPrice
		{
			get{ return _standard_display_price; }
		}
		
	
	
		public CurrencyPromotion( IDictionary<string,object> fromJson )
		{
			//Debug.Log("------------> CurrencyPromotion : " + fromJson.ToString()); 
		
			if(fromJson.ContainsKey("currencyId")) 
			{
				this._currency_id = Convert.ToInt32(fromJson["currencyId"]);
			}
			
			if(fromJson.ContainsKey("startsAt")) 
			{
				this._starts_at = Convert.ToDateTime(fromJson["startsAt"]);
			}
			
			if(fromJson.ContainsKey("expiresAt")) 
			{
				this._expires_at = Convert.ToDateTime(fromJson["expiresAt"]);
			}
			
			if(fromJson.ContainsKey("standardAmount")) 
			{
				this._standard_amount = Convert.ToDecimal(fromJson["standardAmount"]);
			}
			
			if(fromJson.ContainsKey("standardPrice")) 
			{
				this._standard_price = Convert.ToDecimal(fromJson["standardPrice"]);
			}
			
			if(fromJson.ContainsKey("standardFreeAmount")) 
			{
				this._standard_free_amount = Convert.ToDecimal(fromJson["standardFreeAmount"]);
			}
		
			if(fromJson.ContainsKey("standardDisplayPrice")) 
			{
				this._standard_display_price = Convert.ToDecimal(fromJson["standardDisplayPrice"]);
			}
		}
	
	}	
}
