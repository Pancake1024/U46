using System;
using System.Collections.Generic;


namespace com.miniclip.currencies
{
	public class BundlesEventArgs : EventArgs
	{
		private Dictionary<string, CurrencyBundle> _bundles;
		
		public Dictionary<string, CurrencyBundle> Bundles
		{
			get{ return _bundles; }
		}
		
		public BundlesEventArgs( Dictionary<string, CurrencyBundle> bundles )
		{
			this._bundles = bundles;
		}
	}
}

