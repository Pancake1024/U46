using System;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip
{
	public class MiniclipUtils
	{
		public static int ExtractCode(string json)
		{
			int code = 0;
			
			Dictionary<string,object> fromJson = Json.Deserialize(json) as Dictionary<string,object>;
			
			if( fromJson.ContainsKey("code") )
			{
				code = Convert.ToInt32( fromJson["code"] );
			}
			
			return code;
		}
		
		public static string ExtractMessage(string json)
		{
			string message = "";
			
			Dictionary<string,object> fromJson = Json.Deserialize(json) as Dictionary<string,object>;
					
			if( fromJson.ContainsKey("message") )
			{
				message = fromJson["message"].ToString();
			}
			
			return message;
		}
		
		
		public static AwardData BuildAwardData(string json)
		{
			Dictionary<string, object> fromJson = Json.Deserialize (json) as Dictionary<string,object>;
			return BuildAwardData( fromJson );
		}
			
		
		public static AwardData BuildAwardData(Dictionary<string, object> fromJson)
		{
							
			uint id = 0;
			string title = "";
			string description = "";
			
			
			if(fromJson.ContainsKey("id")) 
			{
				id = Convert.ToUInt32(fromJson["id"]);
			}
		
			if(fromJson.ContainsKey("title"))
			{
				title = fromJson["title"].ToString();
			}
			
			if(fromJson.ContainsKey("description"))
			{
				description = fromJson["description"].ToString();
			}
						
			return new AwardData(id,title,description);	
		}
		
	}
}

