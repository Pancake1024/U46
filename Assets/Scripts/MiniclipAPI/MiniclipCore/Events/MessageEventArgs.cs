using System;

namespace com.miniclip
{
	public class MessageEventArgs : EventArgs
	{
	    private string _message;
		
		public string Message
	   	{
	    	get { return this._message; }
	    }
		
	   	public MessageEventArgs(string message)
	    {
	   		this._message = message;
	    }
	}
}
