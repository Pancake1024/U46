using System;
using System.Collections.Generic;


namespace com.miniclip.currencies
{
	public class ItemsPurchasedEventArgs : UserQuantitiesEventArgs
	{
		private int[] _itemIds;
		
		public int[] ItemIds
		{
			get{ return _itemIds; }
		}
		
		//------------------
			
		public ItemsPurchasedEventArgs(Dictionary<string,  CurrencyUserQuantity> userItemQuantities, int[] itemIds) : base(userItemQuantities)
		{
			this._itemIds	= itemIds;
		}
	}
}
