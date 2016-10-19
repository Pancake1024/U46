using System;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class CurrenciesEventArgs : EventArgs 
	{
		private Dictionary<string, CurrencyCurrency> _currencies; 
			
		public Dictionary<string, CurrencyCurrency> Currencies
		{
			get { return this._currencies; }
		}
		
		//-----------------------------
		// Methods
		//-----------------------------
		public CurrenciesEventArgs( Dictionary<string, CurrencyCurrency> currencies ) 
		{
			this._currencies = currencies; //CurrenciesFactory.BuildCurrencies( json );
		}
	}
}
