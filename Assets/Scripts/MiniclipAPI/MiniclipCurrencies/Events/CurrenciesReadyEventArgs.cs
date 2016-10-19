using System;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace com.miniclip.currencies
{
	public class CurrenciesReadyEventArgs : EventArgs
	{
		private Dictionary<string, CurrencyCurrency> _currencies;
		private Dictionary<string, CurrencyItem> _items;
		
		
		public Dictionary<string, CurrencyCurrency> Currencies
		{
			get{ return _currencies; }
		}
		
		public Dictionary<string, CurrencyItem> Items
		{
			get{ return _items; }
		}
				
		public CurrenciesReadyEventArgs( Dictionary<string, CurrencyCurrency> currencies, Dictionary<string, CurrencyItem> items)
		{
			this._currencies = currencies;
			this._items = items;	
		}
	}
}

