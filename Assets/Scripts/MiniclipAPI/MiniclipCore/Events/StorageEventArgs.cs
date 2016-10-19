using System;

namespace com.miniclip
{
	public class StorageEventArgs : EventArgs
	{
		private string _data;
		
		public string data
		{
			get { return this._data; }
		}
		
		public StorageEventArgs(string data)
		{
			this._data = data;
		}
	}
}
