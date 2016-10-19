using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class CurrencyBalance
	{
		private int 				_currencyId = 0;
		private decimal 			_balance	= 0m;
		private CurrencyCurrency 	_currency  = null;
		
		
		public int currencyId
		{
			get{ return _currencyId; }
		}
	
		public decimal balance
		{
			get{ return _balance; }
		}
		
		public CurrencyCurrency currency
		{
			get{ return _currency; }
		}
		
					
		public CurrencyBalance( IDictionary<string,object> fromJson )
		{
			if (fromJson.ContainsKey("currencyId")) 
			{
				this._currencyId = Convert.ToInt32(fromJson["currencyId"]);
			}
			
			if (fromJson.ContainsKey("balance")) 
			{
				this._balance = Convert.ToDecimal(fromJson["balance"]);
			}
			
			if (fromJson.ContainsKey("currency")) 
			{
				IDictionary<string, object> currencyValue = fromJson["currency"] as IDictionary<string,object>;
				this._currency = new CurrencyCurrency( currencyValue );
			}
		}	
		
	}
}
