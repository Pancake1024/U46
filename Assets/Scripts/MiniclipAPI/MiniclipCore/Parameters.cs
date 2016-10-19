using System;
using System.Collections.Generic;
using MiniJSON;


namespace com.miniclip
{
	public class Parameters
	{
		private Dictionary<string, object> _dictionary;
		private string _geoCode = "";
		private string _sessionId = "";
		
		
		//-------------------------------
		// Accessors
		//-------------------------------
		
		public Dictionary<string, object> Dictionary
    	{
			get
       	 	{
            	return _dictionary;
        	}
		}
		
		public string GeoCode
    	{
			get
       	 	{
            	return _geoCode;
        	}
		}
		
		public string SessionId
    	{
			get
       	 	{
            	return _sessionId;
        	}
		}
		
				
		//-------------------------------
		// Methods
		//-------------------------------
		
		public Parameters(string json)
		{	
			_dictionary = Json.Deserialize(json) as Dictionary<string,object>;
			
			if( _dictionary.ContainsKey("mc_geoCode") )
			{
				_geoCode = _dictionary["mc_geoCode"] as string;
			}
			
			if( _dictionary.ContainsKey("mc_sessid") )
			{
				_sessionId = _dictionary["mc_sessid"] as string;
			}
		}
	}
}

