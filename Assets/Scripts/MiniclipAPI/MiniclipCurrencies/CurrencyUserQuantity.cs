using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyUserQuantity
	{
		private int 			_item_id		= 0;		// ID of item to which this relates
		private uint 			_item_balance	= 0;		// Count (balance) of item 
		private CurrencyItem 	_item			= null;		// Reference to the item object describing the item
		
		
		/**
		 * The unique id of the item whose quantity balance this
		 * object relates to
		 */ 
		public int itemId
		{
			get{  return _item_id; }
		}
		
		/**
		 * The count (balance) of this item which are owned by 
		 * the currently logged-in player
		 */
		public uint quantity
		{
			get{ return _item_balance; }
		}
		
		/**
		 * A <code>CurrencyItem</code> object which contains a more
		 * complete description of the item to which this quantity
		 * object relates
		 */
		public CurrencyItem item
		{
			get{  return _item;	}
		}
		
		public CurrencyUserQuantity( IDictionary<string,object> fromJson )
		{
			if (fromJson.ContainsKey("itemId")) 
			{
				_item_id = Convert.ToInt32(fromJson["itemId"]);
			}

			if (fromJson.ContainsKey("quantity")) 
			{
				_item_balance = Convert.ToUInt32(fromJson["quantity"]); 
			}
			
			if (fromJson.ContainsKey("item")) 
			{
				if( fromJson["item"] is IDictionary<string, object> )
				{
					IDictionary<string, object> itemValue = fromJson["item"] as IDictionary<string,object>;
					this._item = new CurrencyItem( itemValue );
				}	
			}
		}
		
	}
}
