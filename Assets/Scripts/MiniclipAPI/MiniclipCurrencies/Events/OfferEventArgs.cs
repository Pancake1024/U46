using System;

namespace com.miniclip.currencies
{
	public class OfferEventArgs : EventArgs
	{
		private int 	_currencyId;
		private string	_offerType;
		private bool	_available;
		
	
		public int CurrencyId
		{
			get{ return _currencyId; }
		}
		
		public string OfferType
		{
			get{ return _offerType; }
		}
		
		public bool Available
		{
			get{ return _available; }
		}
		
	
		public OfferEventArgs(int currencyId, string offerType, bool available)
		{
			this._currencyId = currencyId;
			this._offerType = offerType;
			this._available = available;
		}
	}
}

