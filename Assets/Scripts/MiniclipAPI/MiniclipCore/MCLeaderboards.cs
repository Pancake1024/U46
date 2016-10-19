using System;
using com.miniclip;
using System.Collections.Generic;
namespace com.miniclip
{
	public class MCLeaderboards
	{
		public delegate void LeaderboardsEvents(LeaderboardsEventArgs e);
		public event LeaderboardsEvents RegisterUser;
		public event LeaderboardsEvents ListActiveEvents;
		public event LeaderboardsEvents GetEvent;
		public event LeaderboardsEvents ShowLeaderboard;
		public event LeaderboardsEvents SubmitScore;
		
		public MCLeaderboards ()
		{

		}
	
		public void ProcessData(string json)
		{
			//JSCaller.consoleLog ("MCLeaderboards::"+ json);
			JSCaller.Call ("flashtracer","MCLeaderboards::ProcessData "+json);
			Dictionary<string, object> deserialized = MiniJSON.Json.Deserialize(json) as Dictionary<string,object>;
			string notice_id  = "default";
			
			if (deserialized.ContainsKey ("method"))
			{
				object x = null;
				if (deserialized.TryGetValue("method", out x))
				{
					if(x != null)
					{
						notice_id = x.ToString();
					}
				}
				
				object payload = null;
				if (deserialized.TryGetValue("data", out payload))
				{
					if (payload != null)
					{
						try
						{
							json =  MiniJSON.Json.Serialize(payload);
						}
						catch(Exception ex)
						{
							JSCaller.consoleLog (ex.ToString());
							json = @"{""error"":""response data corrupted""}";
						}
					}
					else
					{
						json = @"{""error"":""response data not found""}";
					}
				}
			}
			
			//	data = MiniJSON.Json.Serialize(serialized);
			//	object payload = MiniJSON.Json.Deserialize (json);
			
			//{"method":"chellengeFriend","msg":"alamakota", "platform":"facebook"}

			JSCaller.Call ("flashtracer","MCLeaderboards: notice_id"+notice_id);
			switch(notice_id)
			{
				case "registerUser":	
					RegisterUser(new LeaderboardsEventArgs(json));
					break;
				case "listActiveEvents":	
					ListActiveEvents(new LeaderboardsEventArgs(json));
					break;
				case "getEvent":	
					GetEvent(new LeaderboardsEventArgs(json));
					break;
				case "showLeaderboard":	
					ShowLeaderboard(new LeaderboardsEventArgs(json));
					break;
				case "submitScore":	
					SubmitScore(new LeaderboardsEventArgs(json));
					break;
				default:
					JSCaller.consoleLog("##: "+ json);	
					break;
			}
		}

		
		public void registerUser(String event_id)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("event_id", event_id);
			string jsonData = JSCaller.BuildJSON (src);
			JSCaller.Call ("leaderboardsAPI","registerUser", jsonData);	
		}
	
		public void listActiveEvents()
		{
			JSCaller.Call ("leaderboardsAPI","listActiveEvents", "{}");	
		}

		public void getEvent(String event_id)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("event_id", event_id);
			string jsonData = JSCaller.BuildJSON (src);
			JSCaller.Call ("leaderboardsAPI","getEvent", jsonData);	
		}

		public void showLeaderboard(String level)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("level", level);
			string jsonData = JSCaller.BuildJSON (src);
			JSCaller.Call ("leaderboardsAPI","showLeaderboard", jsonData);	
		}

		public void submitScore(String score, String level)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("score", score);
			src.Add ("level", level);
			string jsonData = JSCaller.BuildJSON(src);
			JSCaller.Call ("leaderboardsAPI","submitScore", jsonData);	
		}
	}
}

