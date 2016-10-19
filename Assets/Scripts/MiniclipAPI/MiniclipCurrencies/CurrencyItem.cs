using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyItem
	{
		private int		_id 			= 0;	// Unique ID
		private string	_name			= "";	// Internal (short) name
		private string 	_description 	= "";	// Displayable description
		private string	_item_code 		= "";	// ) Internal grouping fields
		
		private string	_category 		= "";	// ) Internal grouping fields
		private string	_type 	 		= "";	// Item type (consumable/non-consumable)
		private int		_display_order 	= 0;	// Relative sort order (inside the game client)
		private uint 	_quantity 		= 0;
		private uint 	_max_allowed 	= 0;	// Max qty the user can own (0 = unlimited)
		
		private Dictionary<string, CurrencyItem>		_contains = null;	//child items contained in this item
		private Dictionary<string, CurrencyPrice>		_prices   = null;	//prices for this item
		private Dictionary<string, CurrencyPromotion>	_promotions = null; //promotions for this item
		
		//-----------------------------------
		// Accessors
		//-----------------------------------
		
		public int id
		{
			get{ return _id; }
		}
		
		public string name
		{
			get{ return _name; }
		}
		
		public string description
		{
			get{ return _description; }
		}
		
		public string itemCode
		{
			get{ return _item_code; }
		}
		
		public string category
		{
			get{ return _category; }
		}
		
		public string type
		{
			get{ return _type; }
		}
		
		public int displayOrder
		{
			get{ return _display_order; }
		}
		
		public uint quantity
		{
			get{ return _quantity; }
		}
		
		public uint maxAllowed
		{
			get{ return _max_allowed; }
		}
		
		//--------------------
		
		public Dictionary<string, CurrencyItem> contains
		{
			get{ return _contains; }
		}
		
		public Dictionary<string, CurrencyPrice> prices
		{
			get{ return _prices; }
		}
		
		public Dictionary<string, CurrencyPromotion> promotions
		{
			get{ return _promotions; }
		}
		
		//--------------------
		
		public CurrencyItem( IDictionary<string,object> fromJson )
		{
			if(fromJson.ContainsKey("id")) 
			{
				this._id = Convert.ToInt32(fromJson["id"]);
			}
			else
			{
				Debug.Log("**> Currencyitem - no 'id' ! :(");
			}
				
			
			if(fromJson.ContainsKey("name") && fromJson["name"] != null) 
			{
				this._name = fromJson["name"].ToString();
			}
			else
			{
				Debug.Log("**> Currencyitem - no 'name' ! :(");
			}
			
			if(fromJson.ContainsKey("description") && fromJson["description"] != null) 
			{
				this._description = fromJson["description"].ToString();
			}
			else
			{
				Debug.Log("**> Currencyitem - no 'description' ! :(");
			}
		
			if(fromJson.ContainsKey("item_code") && fromJson["item_code"] != null) 
			{
				this._item_code = fromJson["item_code"].ToString();
			}
			
			if(fromJson.ContainsKey("category") && fromJson["category"] != null) 
			{
				this._category = fromJson["category"].ToString();
			}
			
			
			if(fromJson.ContainsKey("type") && fromJson["type"] != null) 
			{
				this._type = fromJson["type"].ToString();
			}
			
			
			if(fromJson.ContainsKey("display_order")) 
			{
				this._display_order = Convert.ToInt32(fromJson["display_order"]);
			}
			
			if(fromJson.ContainsKey("quantity")) 
			{
				this._quantity = Convert.ToUInt32(fromJson["quantity"]);
			}
			
			if(fromJson.ContainsKey("max_allowed")) 
			{
				this._max_allowed = Convert.ToUInt32(fromJson["max_allowed"]);
			}
			

			//-- Contains Child Items --
			
			if(fromJson.ContainsKey("contains"))
			{
				IDictionary<string, object> itemsDict = fromJson["contains"] as IDictionary<string,object>;
				
				if( itemsDict != null && itemsDict.Count > 0)
				{
					this._contains = new Dictionary<string, CurrencyItem>();
					
					//foreach (string itemKey in itemsDict.Keys)
					foreach (KeyValuePair<string, object> kv in itemsDict) 
					{
						//Debug.Log("item ID: " + itemKey);
	
						IDictionary<string,object> childItemValue = /*itemsDict[itemKey]*/ kv.Value as IDictionary<string,object>;
						CurrencyItem itm = new CurrencyItem( childItemValue );
						this._contains.Add(/*itemKey*/kv.Key, itm);	
					}
				}
			}
			
			//-- Create Prices --

			if(fromJson.ContainsKey("prices"))
			{
				//Debug.Log ("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
				
				IDictionary<string, object> pricesDict =  fromJson["prices"] as IDictionary<string,object>;
				
				//Debug.Log ("-> from['prices'] - " +  fromJson["prices"].ToString() );
				
				this._prices = new Dictionary<string, CurrencyPrice>();
				
				//foreach (string priceKey in pricesDict.Keys) 
				foreach (KeyValuePair<string, object> kv in pricesDict)
				{
					//Debug.Log("Price ID: " + priceKey);

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
			
			
		}//end of method
			
	}
}
