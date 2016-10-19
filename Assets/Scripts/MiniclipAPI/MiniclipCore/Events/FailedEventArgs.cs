using System;
using com.miniclip;

namespace com.miniclip
{
	public class FailedEventArgs : MessageEventArgs
	{
		private int _code;
		
		public int Code
	   	{
	    	get { return this._code; }
	    }
	
	   	public FailedEventArgs(int code, string message) : base(message)
	    {
	   		this._code = code;
	    }
	}
}

