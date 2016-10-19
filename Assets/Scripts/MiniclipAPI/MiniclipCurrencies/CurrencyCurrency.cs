using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyCurrency
	{
		private int 	_id 		= 0;		// EG 2 
		private string	_name 		= "";		// EG "Miniclip Credits"
		private string	_code 		= "";		// EG "MC"
		private int		_typeId 	= 0;		// 2 = Hard, 4 = Soft, 6 = Real 
		private bool	_js_item 	= false;	// Require JS to purchase item with this currency
		private bool	_js_other 	= false;	// Require JS to purchase other currency with this one
		private bool 	_js_this 	= false;	// Require JS to purchase this currency with another
		
		
		
		public int id
		{
			get{ return _id; }
		}
		
		public string name
		{
			get{ return _name; }
		}
		
		public string code
		{
			get{ return _code; }
		}
		
		public int typeId
		{
			get{ return _typeId; }
		}
		
		public bool js_item
		{
			get{ return _js_item; }
		}
		
		public bool js_other
		{
			get{ return _js_other; }
		}
		
		public bool js_this
		{
			get{ return _js_this; }
		}
		
		
		
		public CurrencyCurrency(IDictionary<string,object> fromJson)
		{
			if (fromJson.ContainsKey("id")) {
				_id = Convert.ToInt32(fromJson["id"]);
			}

			if (fromJson.ContainsKey("name") && fromJson["name"] != null) 
			{
				_name = fromJson["name"].ToString();
			}
			
			if (fromJson.ContainsKey("code") && fromJson["code"] != null) 
			{
				_code = fromJson["code"].ToString();
			}
			
			if (fromJson.ContainsKey("typeId")) {
				_typeId = Convert.ToInt32(fromJson["typeId"]);
			}

			if (fromJson.ContainsKey("js_item")) {
				_js_item = Convert.ToBoolean(fromJson["js_item"]);
			}

			if (fromJson.ContainsKey("js_other")) {
				_js_other = Convert.ToBoolean(fromJson["js_other"]);
			}

			if (fromJson.ContainsKey("js_item")) {
				_js_this = Convert.ToBoolean(fromJson["js_this"]);
			}

			//Debug.Log ("[Constructed currency] (" + _id.ToString () + ") :: " + _name.ToString () + " :: " + _code.ToString() + " :: Type: " + _typeId.ToString() + ", "
			//           + _js_item.ToString() + " / " + _js_other.ToString() + " / " + _js_this.ToString());
		}
	}
}
