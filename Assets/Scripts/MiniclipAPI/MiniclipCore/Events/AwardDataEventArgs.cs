using System;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip
{
	public class AwardDataEventArgs : EventArgs
	{
		private AwardData _awardData;
		
		public AwardData AwardData
		{
			get{ return _awardData; }
		}
		
		public AwardDataEventArgs( AwardData awardData )
		{
			this._awardData = awardData;
		}
	}
}

