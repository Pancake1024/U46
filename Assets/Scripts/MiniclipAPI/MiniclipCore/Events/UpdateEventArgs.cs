using System;

namespace com.miniclip
{
	public class UpdateEventArgs : EventArgs
	{		
		private int _requestedUpdatePool;
		
		public int RequestedUpdatePool
	   	{
	    	get { return this._requestedUpdatePool; }
	    }
		
		public UpdateEventArgs(int requestedUpdatePool)
		{
			this._requestedUpdatePool = requestedUpdatePool;
		}
	}
}

