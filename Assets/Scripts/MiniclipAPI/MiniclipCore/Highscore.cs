using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.miniclip
{
	public class Highscore
	{
		private string	_location 			= "";
		private string	_playerAvatarURL  	= "";
		private string	_playerPageURL		= "";
		private uint	_position 			= 0;
		private float	_rank 				= 0.0f;
		private bool	_registered 		= true;
		private int 	_score 				= 0; 
		private uint	_userId 			= 0;
		private string	_username			= "";
        

        //TR4
        private Texture2D avatar            = null;

        public Texture2D Avatar
		{
            get { return avatar; }
            set { avatar = value; }
		}



        public int Score
        {
            get { return _score; }
        }
				
		public string Username
		{
			get{ return _username; }
		}
		
		public uint UserId
		{
			get{ return _userId; }
		}
		
		public string Location
		{
			get{ return _location; }
		}
		
		public string PlayerAvatarURL
		{
			get{ return _playerAvatarURL; }
		}
		
		public string PlayerPageURL
		{
			get{ return _playerPageURL; }
		}
		
		public uint Position
		{
			get{ return _position; }
		}
		
		public float Rank
		{
			get{ return _rank; }
            //TR4
            set { _rank = value; }
		}
		
		public bool Registered
		{
			get{ return _registered; }
		}
		

	
		
		public Highscore( IDictionary<string,object> fromJson )
		{
			if(fromJson.ContainsKey("location")) 
			{
				this._location = fromJson["location"].ToString();
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'location' ! :(");
			}
			
			
			if(fromJson.ContainsKey("player_avatar_url")) 
			{
				this._playerAvatarURL = fromJson["player_avatar_url"].ToString();
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'player_avatar_url' ! :(");
			}
			
			
			if(fromJson.ContainsKey("player_page_url")) 
			{
				this._playerPageURL = fromJson["player_page_url"].ToString();
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'player_page_url' ! :(");
			}
			
			
			if(fromJson.ContainsKey("position")) 
			{
				this._position = Convert.ToUInt32(fromJson["position"]);
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'position' ! :(");
			}
			
			
			if(fromJson.ContainsKey("rank")) 
			{
				this._rank = Convert.ToSingle(fromJson["rank"]);
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'rank' ! :(");
			}
			
			
			if(fromJson.ContainsKey("registered")) 
			{
				this._registered = Convert.ToBoolean(fromJson["registered"]);
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'registered' ! :(");
			}
			
			
			if(fromJson.ContainsKey("score")) 
			{
				this._score = Convert.ToInt32(fromJson["score"]);
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'score' ! :(");
			}
			
			
			if(fromJson.ContainsKey("user_id")) 
			{
				this._userId = Convert.ToUInt32(fromJson["user_id"]);
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'user_id' ! :(");
			}
			
			
			if(fromJson.ContainsKey("username")) 
			{
				this._username = fromJson["username"].ToString();
			}
			else
			{
				Debug.Log("-> HighscoreEntry - no 'username' ! :(");
			}
			
		}
				
	}
}

