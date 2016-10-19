using System;
using System.Collections.Generic;
using MiniJSON;


namespace com.miniclip
{
	public class EmbedVariables
	{
		private Dictionary<string, object> _dictionary;
		
		private string	_sessionId			= "";
		private string	_geoCode			= "";
		private string	_language			= "";
		private bool	_isWebmasterGame	= false;
		
		//-------------------------------
		// Accessors
		//-------------------------------
		
		public Dictionary<string, object> Dictionary
    	{
			get{ return _dictionary; }
		}
		
		//-------------------------------
		
		public string SessionId			{ get{ return _sessionId;  		} }
		public string GeoCode			{ get{ return _geoCode;    		} }
		public string Language			{ get{ return _language;   		} }
		public bool   IsWebmasterGame	{ get{ return _isWebmasterGame;	} }
		
		//public string GameName		{ get{ return _dictionary["mc_gamename"] as string;			} }
		//public string GameId		{ get{ return _dictionary["mc_hsname"] as string; 			} } //(highscore name)
		//public string IconBig		{ get{ return _dictionary["mc_iconBig"] as string; 			} }
		//public string Icon			{ get{ return _dictionary["mc_icon"] as string;				} }
		//public string NegativeScore { get{ return _dictionary["mc_negativescore"] as string; 	} }
		//public string PlayersSite	{ get{ return _dictionary["mc_players_site"] as string; 	} }
		//public string ScoreIsTime	{ get{ return _dictionary["mc_scoreistime"] as string; 		} }
		//public string LowScore		{ get{ return _dictionary["mc_lowscore"] as string; 		} }
		//public string Width			{ get{ return _dictionary["mc_width"] as string;			} }
		//public string Height		{ get{ return _dictionary["mc_height"] as string; 			} }	
		//---
		//public string UserId		{ get{ return _dictionary["mc_uid"] as string; 				} }
		
		//public string Shockwave		{ get{ return _dictionary["mc_shockwave"] as string;  		} }
		//public string GameUrl		{ get{ return _dictionary["mc_gameUrl"] as string; 			} }
		//public string UserAgent		{ get{ return _dictionary["mc_ua"] as string; 				} }
		//public string Geo			{ get{ return _dictionary["mc_geo"] as string;  			} }
		
		
		//public string SContent	{ get{ return _sContent; 		} } //not used???

	
		//public string Filename		{ get{ return _dictionary["fn"] as string;					} }
		//public string QuantSegs		{ get{ return _dictionary["quantSegs"] as string;			} }
		//public string LocalConnectionId	{ get{ return _dictionary["mc_lcid"] as string; 		} }
		//public string ExtraId		{ get{ return _dictionary["mc_extra"] as string; 			} }
		//public string FacebookAppId { get{ return _dictionary["facebook_app_id"] as string;		} }
		
		//-------------------------------
		// Methods
		//-------------------------------
		
		public EmbedVariables(string json)
		{	
			//public string SessionId			{ get{ return _dictionary["mc_sessid"] as string;  			} }
			//public string GeoCode			{ get{ return _dictionary["mc_geoCode"] as string;  		} }
			//public string Language			{ get{ return _dictionary["mc_lang"] as string;				} }
			//public string IsWebmasterGame	{ get{ return _dictionary["mc_webmaster"] as string;		} }
		
			_dictionary = Json.Deserialize(json) as Dictionary<string,object>;
			
			//Session Id
			if( _dictionary.ContainsKey("mc_sessid") )
			{
				_sessionId = _dictionary["mc_sessid"] as string;
			}
			
			//Geographic Code (e.g. "US" )
			if( _dictionary.ContainsKey("mc_geoCode") )
			{
				_geoCode = _dictionary["mc_geoCode"] as string;
			}
			
			//Language
			if( _dictionary.ContainsKey("mc_lang") )
			{
				_language = _dictionary["mc_lang"] as string;
			}
			
			//Webmaster
			if( _dictionary.ContainsKey("mc_webmaster") )
			{
				_isWebmasterGame = (_dictionary["mc_webmaster"] as string) == "1" ? true : false;
			}	
		}
	}
}

