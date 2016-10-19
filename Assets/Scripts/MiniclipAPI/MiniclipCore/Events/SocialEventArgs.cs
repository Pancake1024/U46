using System;

namespace com.miniclip
{
	public class SocialEventArgs : EventArgs
	{
		private string _data;
		private string _methodCalled = "";
		
		public string data
		{
			get { return this._data; }
		}

		public string methodCalled
		{
			get{ return this._methodCalled; }
		}

		public SocialEventArgs(string data)
		{
			this._data = data;
		}
	}
}
