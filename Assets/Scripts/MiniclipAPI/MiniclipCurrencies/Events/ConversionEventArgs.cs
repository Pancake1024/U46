using System;
using System.Collections.Generic;

namespace com.miniclip.currencies
{
	public class ConversionEventArgs : BalancesEventArgs
	{
		private int	_sourceId = 0;
		private int _destinationId = 0;
		private int _sourceAmount = 0;
		
		
		public int SourceId
		{
			get{ return _sourceId; }
		}
		
		public int DestinationId
		{
			get{ return _destinationId; }
		}
				
		public int SourceAmount
		{
			get{ return _sourceAmount; }
		}
		
	
		public ConversionEventArgs( Dictionary<string, CurrencyBalance> balances, int sourceId, int destinationId, int sourceAmount ) : base(balances)
		{
			this._sourceId 		= sourceId;
			this._destinationId = destinationId; 
			this._sourceAmount	= sourceAmount;
		}
	}
}

