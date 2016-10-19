using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyBundle
	{
		private int 		_bundleId	= 0;
		private string 		_name 		= "";
		private int 		_currencyId = 0;
		private decimal 	_amount 	= 0.00m;
		
		private Dictionary<string, CurrencyPrice>      _prices = null;
		private Dictionary<string, CurrencyPromotion>  _promotions = null;

		
		public int bundleId
		{
			get{ return _bundleId; }
		}
		
		public string name
		{
			get{ return _name; }
		}
		
		public int currencyId
		{
			get{ return _currencyId; }
		}
		
		public decimal amount
		{
			get{ return _amount; }
		}
		
		public Dictionary<string, CurrencyPrice> prices
		{
			get{ return _prices; }
		}
		
		public Dictionary<string, CurrencyPromotion> promotions
		{
			get{ return _promotions; }
		}
		
		
			
		public CurrencyBundle( IDictionary<string,object> fromJson )
		{
			if(fromJson.ContainsKey("bundleId"))
			{
				this._bundleId = Convert.ToInt32( fromJson["bundleId"] );
			}
			
			if(fromJson.ContainsKey("name") && fromJson["name"] != null)
			{
				this._name = fromJson["name"].ToString();
			}
			
			if(fromJson.ContainsKey("currencyId"))
			{
				this._currencyId = Convert.ToInt32( fromJson["currencyId"] );
			}
			
			if(fromJson.ContainsKey("amount"))
			{
				this._amount = Convert.ToDecimal( fromJson["amount"] );
			}
			
			
			//-- Create Prices --
			if(fromJson.ContainsKey("prices"))
			{
				IDictionary<string, object> pricesDict =  fromJson["prices"] as IDictionary<string,object>;
				
				this._prices = new Dictionary<string, CurrencyPrice>();
				
				//foreach (string priceKey in pricesDict.Keys) //old inefficient code
				foreach (KeyValuePair<string, object> kv in pricesDict)
				{
					//Debug.Log("Price ID: " + 	kv.Key);
					IDictionary<string,object> priceValue = /*pricesDict[priceKey]*/kv.Value as IDictionary<string,object>;
					CurrencyPrice p = new CurrencyPrice( priceValue );
					
					this._prices.Add(/*priceKey*/kv.Key, p);	
				}
			}
			
			
			//-- Create Promotions --
			if(fromJson.ContainsKey("promotions"))
			{
				//Debug.Log ("-> CurrencyItem::CurrencyItem - creating promotions...");
				
				IDictionary<string, object> promotionsDict =  fromJson["promotions"] as IDictionary<string,object>;
				
				//Debug.Log ("-> CurrencyItem::CurrencyItem - from['promotions'] - " +  fromJson["promotions"].ToString() );
		
				
				this._promotions = new Dictionary<string, CurrencyPromotion>();
				
				foreach (KeyValuePair<string, object> kv in promotionsDict)
				{
					IDictionary<string,object> promotionValue = kv.Value as IDictionary<string,object>;
					
					CurrencyPromotion promo = new CurrencyPromotion( promotionValue );
					
					this._promotions.Add(/*promotionKey*/kv.Key, promo);	
				}	
			}
		}
		
		public CurrencyPrice GetPriceById(int id)
		{
			if(_prices.ContainsKey( id.ToString() ))
			{
				return _prices[ id.ToString() ] as CurrencyPrice;
			}
			
			return null;
		}
		
		
	}
}
