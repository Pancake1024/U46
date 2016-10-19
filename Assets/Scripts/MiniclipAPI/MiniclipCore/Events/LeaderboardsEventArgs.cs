using System;

namespace com.miniclip
{
	public class LeaderboardsEventArgs : EventArgs
	{
		private string _data;
		
		public string data
		{
			get { return this._data; }
		}
		
		public LeaderboardsEventArgs(string data)
		{
			this._data = data;
		}
	}
}
