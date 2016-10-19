using System;
using System.Collections.Generic;
using UnityEngine;


namespace com.miniclip
{
	//equivalent AS3 class: com.miniclip.net.authentication.UserDetails
	
	public class UserDetails
	{
		private uint 	_id;
		private string 	_email;
		private string 	_nickname;
		private string 	_location;
		private bool 	_avatar;
		private float	_worldRank;
		private float	_starRank;
		private uint	_challenges;
		private uint	_friends;
		private string 	_playerPageURL;
		private string 	_playerAvatarURL;
		private string 	_sessionid;
	
		private int 	_userLevel;	
		
		//------------------------
		// Properties
		//------------------------	
		
		/**
		 * The user's ID.
		 */
		public uint Id
		{
    		get { return _id; }
		}
		
		/**
		 * The user's email address.
		 */
		public string Email
		{
			get { return _email; }
		}
		
		/**
		 * The user's Miniclip nickname.
		 */
		public string Nickname
		{
			get { return _nickname; }
		}
		
		/**
		 * The user's location (Country).
		 */
		public string Location
		{
			get { return _location; }
		}
		
		/**
		 * Has the user created an an avatar (YoMe)?
		 */
		public bool Avatar
		{
			get { return _avatar; }
		}
		
		/**
		 * The user's 'World Rank' (range: 0-100)
		 */
		public float WorldRank
		{
			get { return _worldRank; }
		}
		
		/**
		 * The user's 'Star Rank' (range: 1-20) 
		 */
		public float StarRank
		{
			get { return _starRank; }
		}
		
		/**
		 * The total number of challenges that the user has won.
		 */
		public uint Challenges
		{
			get { return _challenges; }
		}
		
		/**
		 * The total number of challenges that the user has won.
		 */
		public uint Friends
		{
			get { return _friends; }
		}
		
		/**
		 * The user's public 'player page'
		 */
		public string PlayerPageURL
		{
			get { return _playerPageURL; }
		}
		
		/**
		 * The URL to the user's Avatar (YoMe)
		 */
		public string PlayerAvatarURL
		{
			get { return _playerAvatarURL; }
		}
		
		
		/**
		 * The user's current session ID.
		 */
		public string SessionId
		{
    		get { return _sessionid; }
		}
		
		/**
		 * The player's user security level
		 */
		public int UserLevel
		{
			get { return _userLevel; }
		}
			
		//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		
		//------------------------
		// Methods
		//------------------------	
		
		public UserDetails(Dictionary<string,object> dict)
		{
			// id
			Debug.Log("-> id - Type: " + (dict["id"]).GetType() );	
			_id = Convert.ToUInt32( dict["id"] );
			
			// sessionid
			if(dict.ContainsKey("sessionid"))
			{
				_sessionid 		= (string)	dict["sessionid"];
			}
			else
			{
				_sessionid 		= (string)	dict["sid"];
			}
			
			_email 				= (string)	dict["email"];
			_nickname 			= (string)	dict["nickname"];
			_location			= (string)	dict["location"];
			
			_avatar				= false; 
			
			_worldRank 			= Convert.ToSingle( dict["worldRank"] );
			_starRank 			= Convert.ToSingle(	dict["starRank"] );
			_challenges			= Convert.ToUInt32(	dict["challenges"] );
			_friends 	    	= Convert.ToUInt32(	dict["friends"] );
			
			_playerPageURL 		= (string)  dict["playerPageURL"];
			_playerAvatarURL 	= (string)	dict["playerAvatarURL"];
			
			_userLevel			= Convert.ToInt32( dict["userLevel"] );
			
			if(dict.ContainsKey("avatar"))
			{
				_avatar = Convert.ToBoolean( dict["avatar"] );
			}
			
			if(dict.ContainsKey("avatarCode"))
			{
				uint avatarCode = Convert.ToUInt32( dict["avatarCode"] ); //assuming avatarCode cannot be negative?
				_avatar = (avatarCode > 0);
			}
		}
		
		
		/**
		 * @return A standard Object containing UserDetail's properties
		 */
		public Dictionary<string,object> ToDictionary()
		{
			//dictionary capacity: 13 key/value pairs
			Dictionary<string,object> dict = new Dictionary<string, object>(13); 
			
			dict["id"]				= _id;
			dict["sessionid"]		= _sessionid;
			dict["email"]			= _email;
			dict["nickname"] 		= _nickname;
			dict["location"]		= _location;
			dict["avatar"] 			= _avatar;
			dict["worldRank"] 		= _worldRank;
			dict["starRank"]    	= _starRank;
			dict["challenges"]  	= _challenges;
			dict["friends"]     	= _friends;
			dict["playerPageURL"] 	= _playerPageURL;
			dict["playerAvatarURL"] = _playerAvatarURL;
			dict["userLevel"]		= _userLevel;
			
			return dict;
		}
			

	}
}
