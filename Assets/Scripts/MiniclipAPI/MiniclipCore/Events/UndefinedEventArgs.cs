using System;

namespace com.miniclip
{
	public class UndefinedEventArgs : EventArgs
	{
		private string _noticeID;
		private string _json;
		
		public string NoticeID
	   	{
	    	get { return this._noticeID; }
	    }
		
		public string Json
	   	{
	    	get { return this._json; }
	    }
		
	   	public UndefinedEventArgs(string noticeID, string json)
	    {
			this._noticeID = noticeID;
	   		this._json = json;
	    }
	}
}
