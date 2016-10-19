using System;

namespace com.miniclip
{
	public class ParametersEventArgs : EventArgs
	{
		private Parameters _parameters;
		
		public Parameters Parameters
	   	{
	    	get { return _parameters; }
	    }
		
		public ParametersEventArgs( Parameters parameters )
		{
			this._parameters = parameters;
		}
	}
}

