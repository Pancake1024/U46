using System;
using com.miniclip;
using System.Collections.Generic;
namespace com.miniclip
{
	public class MCSocial
	{

		public delegate void SocialChangedEventHandler(SocialEventArgs e);
		public event SocialChangedEventHandler SaveFriend;
		public event SocialChangedEventHandler ListUsersFriended;
		public event SocialChangedEventHandler ListUsersFriending;
		public event SocialChangedEventHandler GetFriendship;
		public event SocialChangedEventHandler DeleteFriend;
		public event SocialChangedEventHandler SubmitAward;
		public event SocialChangedEventHandler ChellengeFriend;
		public event SocialChangedEventHandler SubmitScore;
		public event SocialChangedEventHandler InviteFriend;
		public event SocialChangedEventHandler PostMessage;
		public event SocialChangedEventHandler GetUser;
		
		public MCSocial()
		{
			Console.WriteLine("-> Storage created ");
		}

		public void ProcessData(string json)
		{
			JSCaller.consoleLog ("MCSocial::"+ json);
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
					if(payload != null)
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
						json = @"{""error"":""response datanot found""}";
					}
				}
			}
		
		//	data = MiniJSON.Json.Serialize(serialized);
		//	object payload = MiniJSON.Json.Deserialize (json);
		
			//{"method":"chellengeFriend","msg":"alamakota", "platform":"facebook"}
			JSCaller.consoleLog ("################"+ notice_id);	
			switch(notice_id)
			{
				case "saveFriend":	
					SaveFriend(new SocialEventArgs(json));
					break;
				case "listUsersFriended":	
					ListUsersFriended(new SocialEventArgs(json)); 
					break;
				case "listUsersFriending":	
					ListUsersFriending(new SocialEventArgs(json)); 
					break;
				case "getFriendship":	
					GetFriendship(new SocialEventArgs(json));
					break;
				case "deleteFriend":	
					DeleteFriend(new SocialEventArgs(json));
					break;
				case "submitAward":	
					SubmitAward(new SocialEventArgs(json));
					break;
				case "chellengeFriend":	
					ChellengeFriend(new SocialEventArgs(json));
					break;
				case "submitScore":	
					SubmitScore(new SocialEventArgs(json));
					break;
				case "inviteFriend":
					InviteFriend(new SocialEventArgs(json));
					break;
				case "postMessage":
					PostMessage(new SocialEventArgs(json));
					break;
				case "getUser":
					GetUser(new SocialEventArgs(json));
					break;
			default:
				JSCaller.consoleLog("##: "+ json);	
					break;
			}
		}
	
		// GHOST DATA
		public void saveFriend(string uid0)
		{
			string jsonData = "{\"uid0\":\""+uid0.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI","saveFriend", jsonData);	
		}

		public void listUsersFriended(string uid0)
		{
			string jsonData = "{\"uid0\":\""+uid0.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI","listUsersFriended", jsonData);	
		}

		public void listUsersFriending(string uid0)
		{
			string jsonData = "{\"uid0\":\""+uid0.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI","listUsersFriending", jsonData);	
		}

		public void getFriendship(string uid0, string uid1)
		{
			string jsonData = "{\"uid0\":\""+uid0.ToString()+"\",\"uid1\":\""+uid1.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI","getFriendship", jsonData);	
		}

		public void deleteFriend(string userID)
		{
			string jsonData = "{\"uid0\":\""+userID.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI", "deleteFriend", jsonData);	
		}

		public void submitAward(string[] awards, string platform )
		{
			string awardsAsString = string.Join (",", awards);
			string jsonData = "{\"awards\":\""+awardsAsString+"\",\"platform\":\""+platform.ToString()+"\"}"; 
			JSCaller.Call ("socialAPI","submitAward", jsonData);	
		}

		public void chellengeFriend(string platform)
		{
			string jsonData = "{\"platform\":\""+platform.ToString()+"\"}"; 
			JSCaller.Call("socialAPI","chellengeFriend", jsonData);	
		}

		public void submitScore(string score, string level, string platform )
		{
		//	string jsonData = "{\"score\":\""+score+"\",\"platform\":\""+platform.ToString()+"\"}"; 
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("score", score);
			src.Add ("level", level);
			src.Add ("platform", platform);

			string jsonData = JSCaller.BuildJSON (src);
			JSCaller.Call ("socialAPI","submitScore", jsonData);	
		}

		public void submitScore(string score, string platform )
		{
			submitScore (score, "1", platform);
		}

		public void inviteFriend(string platform )
		{
			string jsonData = "{\"platform\":\""+platform.ToString()+"\"}"; 
			JSCaller.Call("socialAPI","inviteFriend", jsonData);	
		}

		public void postMessage(string msg, string platform )
		{
			//string jsonData = "{\"msg\":\""+msg+"\",\"platform\":\""+platform.ToString()+"\"}"; 
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("msg", msg);
			src.Add ("platform", platform);
			string jsonData = JSCaller.BuildJSON (src);

			JSCaller.Call("socialAPI", "postMessage", jsonData);	
		}

		public void getUser(string uid0)
		{
			//string jsonData = "{\"msg\":\""+msg+"\",\"platform\":\""+platform.ToString()+"\"}"; 
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("uid0", uid0);
			string jsonData = JSCaller.BuildJSON (src);	
			JSCaller.Call("socialAPI", "getUser", jsonData);	
		}
	}
}

