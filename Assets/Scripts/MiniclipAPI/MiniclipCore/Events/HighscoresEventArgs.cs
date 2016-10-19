using System;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip
{
	public class HighscoresEventArgs : EventArgs
	{
		private Dictionary<string, Highscore> _highscores;
	
		public Dictionary<string, Highscore> Highscores
		{
			get{ return _highscores; }
		}
		

		public HighscoresEventArgs( Dictionary<string, Highscore> highscores )
		{
			this._highscores = highscores;
		}
	}
}

