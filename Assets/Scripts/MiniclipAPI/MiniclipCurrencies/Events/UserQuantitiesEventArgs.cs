using System;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class UserQuantitiesEventArgs : EventArgs
	{
		protected Dictionary<string, CurrencyUserQuantity> _userItemQuantities;
		
		//------------
		
		public Dictionary<string, CurrencyUserQuantity> UserItemQuantities
		{
			get{ return _userItemQuantities; }
		}
		
		 //Dictionary<string, CurrencyUserQuantity>
		public UserQuantitiesEventArgs(Dictionary<string,  CurrencyUserQuantity> userItemQuantities)
		{
			this._userItemQuantities 	= userItemQuantities;
		}
	}
}

