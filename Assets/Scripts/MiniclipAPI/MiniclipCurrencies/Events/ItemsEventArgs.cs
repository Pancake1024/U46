using System;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip.currencies
{
	public class ItemsEventArgs : EventArgs
	{
		private Dictionary<string, CurrencyItem> _items;
		
		public Dictionary<string, CurrencyItem> Items
		{
			get{ return _items; }
		}
		
		//------------------
			
		public ItemsEventArgs(Dictionary<string, CurrencyItem> items)
		{
			this._items = items;	
		}
	}
}