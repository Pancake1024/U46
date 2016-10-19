using System;
using System.Collections.Generic;
using System.Collections;
using MiniJSON;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrenciesFactory
	{		
		//Build Currencies
		public static Dictionary<string, CurrencyCurrency> BuildCurrencies(string json)
		{	
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			return BuildCurrencies( search );
		}
		
		public static Dictionary<string, CurrencyCurrency> BuildCurrencies( Dictionary<string,object> search)
		{		
			Dictionary<string, CurrencyCurrency> currencies = new Dictionary<string, CurrencyCurrency>();
			CurrencyCurrency currencyValue;
			
			//foreach (string kv in search.Keys) //old code - less efficient
			foreach (KeyValuePair<string,object> kv in search)
			{
				//Debug.Log("Currency ID: " + kv);

				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				currencyValue = new CurrencyCurrency(inner);
				currencies.Add(/*kv*/ kv.Key, currencyValue);	
			}
			
			return currencies;
		}
	
		
		//Build Items
		public static Dictionary<string, CurrencyItem> BuildItems(string json)
		{	
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			return BuildItems( search );
		}
		
		public static Dictionary<string, CurrencyItem> BuildItems( Dictionary<string,object> search)
		{		
			Dictionary<string, CurrencyItem> items = new Dictionary<string, CurrencyItem>();
			CurrencyItem itemValue;
			//foreach (string kv in search.Keys) 
			foreach (KeyValuePair<string,object> kv in search)
			{
				Debug.Log("--> CurrenciesFactory::BuildItem() -Item ID: " + kv);

				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				itemValue = new CurrencyItem(inner);
				items.Add(/*kv*/kv.Key, itemValue);	
			}
			
			return items;
		}
		
		
		//Build Item Ids
		public static int[] BuildItemsIds(string json)
		{
			//TODO:
			
			return new int[5];
		}
			
		//Build Bundles
		public static Dictionary<string, CurrencyBundle> BuildBundles(string json)
		{
			
			Dictionary<string, CurrencyBundle> bundles = new Dictionary<string, CurrencyBundle>();
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			
			
			CurrencyBundle bundleValue;
			//foreach (string kv in search.Keys) 
			foreach (KeyValuePair<string,object> kv in search)
			{
				//Debug.Log("Currency ID: " + kv);

				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				bundleValue = new CurrencyBundle(  inner );
				bundles.Add(/*kv*/kv.Key, bundleValue);
			}
			
			return bundles;
		}
		
		
		//Build Balances
		public static Dictionary<string, CurrencyBalance> BuildBalances(string json)
		{
			Dictionary<string, CurrencyBalance> balances = new Dictionary<string, CurrencyBalance>();
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			
			CurrencyBalance balanceValue;
			//foreach (string kv in search.Keys) 
			foreach (KeyValuePair<string,object> kv in search)
			{
				//Debug.Log("Currency ID: " + kv);

				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				balanceValue = new CurrencyBalance(  inner );
				balances.Add( /*kv*/kv.Key, balanceValue);
			}
			
			return balances;
		}
		
		//Build User Quantity
		public static CurrencyUserQuantity BuildUserQuantity(string json)
		{
			Dictionary<string,object> fromJson = Json.Deserialize (json) as Dictionary<string, object>;
			return new CurrencyUserQuantity(fromJson);
		}
		
		
		public static Dictionary<string, CurrencyUserQuantity> ExtractUserQuantities( Dictionary<string, object> dict )
		{
			if(dict.ContainsKey("quantities"))
			{
				Dictionary<string,object> quantitiesDict = dict["quantities"] as Dictionary<string,object>;
				return BuildUserQuantities( quantitiesDict );	
			}
			
			return null;
		}
		
		public static Dictionary<string, CurrencyUserQuantity> BuildUserQuantities(string json)
		{	
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			return BuildUserQuantities( search );
		}
		
		
		public static Dictionary<string, CurrencyUserQuantity> BuildUserQuantities( Dictionary<string,object> search )
		{
			Dictionary<string, CurrencyUserQuantity> userQuantities = new Dictionary<string, CurrencyUserQuantity>();
			CurrencyUserQuantity userQuantityValue;
			
			foreach (KeyValuePair<string,object> kv in search)
			{
				Dictionary<string,object> inner = /*search[kv]*/kv.Value as Dictionary<string,object>;
				userQuantityValue =  new CurrencyUserQuantity( inner );
				userQuantities.Add(/*kv*/ kv.Key, userQuantityValue);	
			}
			
			return userQuantities;
		}
		
		
		
		
	}
}

