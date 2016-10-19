using System;

namespace com.miniclip
{
	public class EmbedVariablesEventArgs : EventArgs
	{
		private EmbedVariables _embedVariables;
		
		public EmbedVariables EmbedVariables
	   	{
	    	get { return _embedVariables; }
	    }
		
		public EmbedVariablesEventArgs( EmbedVariables embedVariables )
		{
			this._embedVariables = embedVariables;
		}
	}
}

