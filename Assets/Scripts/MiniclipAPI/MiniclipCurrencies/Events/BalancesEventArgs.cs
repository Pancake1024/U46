using System;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class BalancesEventArgs : EventArgs
	{
		private Dictionary<string, CurrencyBalance> _balances;
		
		public Dictionary<string, CurrencyBalance> Balances
		{
			get{ return _balances; }
		}
		
		public BalancesEventArgs( Dictionary<string, CurrencyBalance> balances )
		{
			this._balances = balances;
		}
	}
}

