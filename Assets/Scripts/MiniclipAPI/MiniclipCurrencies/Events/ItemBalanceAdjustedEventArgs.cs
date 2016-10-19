using System;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class ItemBalanceAdjustedEventArgs : UserQuantitiesEventArgs
	{
		private int _itemId;
		private int _amount;
		
		public int ItemId
		{
			get{ return _itemId; }
		}
		
		public int Amount
		{
			get{ return _amount; }
		}
		
		public ItemBalanceAdjustedEventArgs(Dictionary<string,  CurrencyUserQuantity> userItemQuantities, int itemId, int amount) : base(userItemQuantities)
		{
			this._itemId = itemId;
			this._amount = amount;
		}
	}
}

