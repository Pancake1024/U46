
using System;
using com.miniclip;
namespace com.miniclip
{
	public class Storage
	{
		public delegate void StorageChangedEventHandler(StorageEventArgs e);
		public event StorageChangedEventHandler Get;
		public event StorageChangedEventHandler Set;
		public event StorageChangedEventHandler Delete;
		public event StorageChangedEventHandler DataSet;
		public event StorageChangedEventHandler DataReceived;
		public event StorageChangedEventHandler DataDeleted;
		public event StorageChangedEventHandler Error;

		public event StorageChangedEventHandler GhostdataSet;
		public event StorageChangedEventHandler GhostdataReceived;
		public event StorageChangedEventHandler GhostdataListReceived;
		public event StorageChangedEventHandler GhostdataDeleted;

		private string _data = "";

		public Storage ()
		{
			Console.WriteLine("-> Storage created ");
		}

		public void ProcessData(string notice_id, string json)
		{
			JSCaller.consoleLog ("Storage ProcessData "+notice_id+" "+json);
			switch(notice_id)
			{
				case NoticeID.STORAGE_SET:	

					if(DataSet != null)
					{
						DataSet(new StorageEventArgs(json));
					}
					break;
				case NoticeID.STORAGE_GET:	
					if(DataReceived != null)
					{
						DataReceived(new StorageEventArgs(json));
					}
					break;
				case NoticeID.STORAGE_DELETE:	
					if(DataReceived != null)
					{
						DataDeleted(new StorageEventArgs(json));
					}
				break;

			case NoticeID.STORAGE_GHOSTDATA_SET:	
				if(DataReceived != null)
				{
					GhostdataSet(new StorageEventArgs(json));
				}
				break;
			case NoticeID.STORAGE_GHOSTDATA_GET:	
				if(DataReceived != null)
				{
					GhostdataReceived(new StorageEventArgs(json));
				}
				break;
			case NoticeID.STORAGE_GHOSTDATA_LIST:	
				if(DataReceived != null)
				{
					GhostdataListReceived(new StorageEventArgs(json));
				}
				break;
			case NoticeID.STORAGE_GHOSTDATA_DELETE:	
				if(DataReceived != null)
				{
					GhostdataDeleted(new StorageEventArgs(json));
				}
				break;
				case NoticeID.STORAGE_ERROR:
					if(Error != null)
					{
						Error(new StorageEventArgs(json));
					}
					break;
			}
		}

		public string data
		{
			get{return _data;}
		}

		public void get()
		{
			JSCaller.consoleLog ("Storage.get");
			this.Get( new StorageEventArgs(""));
		}

		public void get(string json)
		{
			JSCaller.consoleLog("Storage.get " + json);
			this.Get(new StorageEventArgs(json));
		}

		public void set(string json)
		{
			JSCaller.consoleLog("Storage.set "+ json);
			this.Set(new StorageEventArgs(json));
		}

		public void delete(string json)
		{
			JSCaller.consoleLog("Storage.delete "+ json);
			this.Delete(new StorageEventArgs(json));
		}

		// GHOST DATA
		public void getGhostData(string userID, int level)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("userID", userID.ToString());
			src.Add ("level", level.ToString());
			string jsonData = JSCaller.BuildJSON (src);		
			JSCaller.Call ("getGhostData", jsonData);	
		}

		public void setGhostData(string userID, int level, string jsonObject)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("userID", userID.ToString());
			src.Add ("level", level.ToString());
			src.Add ("jsonObject", jsonObject.ToString());
			string jsonData = JSCaller.BuildJSON (src);		
			JSCaller.Call ("setGhostData", jsonData);		
		}

		public void setGhostData(string userID, int level, string jsonObject, string score)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("userID", userID.ToString());
			src.Add ("level", level.ToString());
			src.Add ("jsonObject", jsonObject.ToString());
			src.Add ("score", score.ToString());
			string jsonData = JSCaller.BuildJSON (src);
			JSCaller.Call ("setGhostData", jsonData);		
		}

		public void deleteGhostData(string userID, int level)
		{
			string jsonData = "{\"userID\":\""+userID.ToString()+"\",\"level\":\""+level.ToString()+"\"}"; 
			JSCaller.Call ("deleteGhostData", jsonData);	
		}

		public void listGhostData(int level, int limit,  Boolean random, double low_score, double top_score)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("level", level.ToString ());
			src.Add ("limit", limit.ToString());
			src.Add ("random", (random?"1":"0"));
			src.Add ("low_score", low_score.ToString());
			src.Add ("top_score", top_score.ToString());

			string jsonData = JSCaller.BuildJSON (src);		

			JSCaller.Call ("listGhostData", jsonData);	
		}

		public void listGhostData(int level, int limit, Boolean random, int lek)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("level", level.ToString());
			src.Add ("limit", limit.ToString());
			src.Add ("random", (random?"1":"0"));
			src.Add ("lek", lek.ToString());
			string jsonData = JSCaller.BuildJSON (src);		
			JSCaller.Call ("listGhostData", jsonData);	
		}

		public void listGhostData(int level, int limit, Boolean random)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("level", level.ToString());
			src.Add ("limit", limit.ToString());
			src.Add ("random", (random?"1":"0"));
			string jsonData = JSCaller.BuildJSON (src);	
			JSCaller.Call ("listGhostData", jsonData);	
		}

		public void listGhostData(int level, string[] uids)
		{

			string uidsAsString = string.Join(",", uids);
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("level", level.ToString());
			src.Add ("uids", uidsAsString);
	
			string jsonData = JSCaller.BuildJSON (src);
		
			JSCaller.Call ("listGhostData", jsonData);	
		}

		public void getUser(string userID)
		{
			System.Collections.Generic.Dictionary<string, string> src = new System.Collections.Generic.Dictionary<string, string> ();
			src.Add ("uid0", userID);

			string jsonData = JSCaller.BuildJSON (src);
			
			JSCaller.Call ("getUser", jsonData);	
		}
	}
}

