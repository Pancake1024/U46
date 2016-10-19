using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.miniclip.currencies
{
	public class CurrencyPrice
	{
		private int 		_currencyId = 0;
		private decimal 	_amount		= 0m;
		private string 		_display	= "";
		
		
		public int currencyId
		{
			get{ return _currencyId; }
		}
	
		public decimal amount
		{
			get{ return _amount; }
		}
		
		public string display
		{
			get{ return _display; }
		}
		
		
		
		public CurrencyPrice(IDictionary<string,object> fromJson)
		{
			if(fromJson == null)
			{
				Debug.Log("---> CurrencyPrice : fromJson is null! :(" );
				return;
			}
				
			
			Debug.Log("------------> CurrencyPrice : " + fromJson.ToString() );
			
			
			if (fromJson.ContainsKey("currencyId")) 
			{
				_currencyId = Convert.ToInt32(fromJson["currencyId"]);
			}

			if (fromJson.ContainsKey("display") && fromJson["display"] != null) 
			{
				_display = fromJson["display"].ToString();
			}
			
			if (fromJson.ContainsKey("amount")) {
				_amount = Convert.ToDecimal(fromJson["amount"]);
			}

			//Debug.Log ("[Constructed price] (" + _currencyId.ToString () + ") = " + _amount.ToString () + " :: " + _display);
		}
	}
}
