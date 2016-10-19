using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace com.miniclip
{
	public class JSCaller
	{
		public static string BuildJSON(Dictionary<string, string> source)
		{
			Dictionary<string, object> deserialized = new Dictionary<string, object> ();

			foreach(KeyValuePair<string, string> entry in source)
			{
				// do something with entry.Value or entry.Key

				Dictionary<string, object> current;

				try
				{
					current = MiniJSON.Json.Deserialize(entry.Value) as Dictionary<string, object>;
				} 
				catch(Exception ex)
				{
					Console.WriteLine(ex.ToString());
					current = null;
				}	

				if(current != null)
				{
					deserialized.Add(entry.Key, current);
				}
				else
				{
					deserialized.Add(entry.Key, entry.Value);
				}
			} 
			return MiniJSON.Json.Serialize(deserialized);
		}
		
		public static void Call( CallArgs callArgs)
		{
			Call( callArgs.functionName, callArgs.data );
		}
	
		public static void Call( string functionName )
		{
			Call( functionName, null );
		}
	
		public static void Call( string functionName, string data )
		{
			//string message = "JSCaller::Call() - functionName: " + functionName;
			//if(data != null)
			//{
			//	message += ", data: " + ((functionName == "receiveScreenGrabData" ) ? data.Substring(0,30) : data);
			//}
			//message += "\n";
			//this.DebugMessage(this, new MessageEventArgs( message ) );
			//------
			
			object[] obj;
		
			if( data == null )
			{
				obj = new object[] { functionName };
			}
			else
			{
				obj = new object[] { functionName, data };
			}

			object[] p = new object[1];
			p[0]="??calling: "+functionName;
		//	p[1]="with: "+data;
			callJs("console.log",p);
			callJs("getGameInstance().invokeService", obj );
				
		}

		public static void Call(string apiName, string functionName, string data )
		{
			try
			{
				Dictionary<string,object> serialized = MiniJSON.Json.Deserialize(data) as Dictionary<string,object>;
				serialized.Add("method",functionName);
				data = MiniJSON.Json.Serialize(serialized);
				Call(apiName, data);
			}
			catch(Exception ex)
			{
				Call(functionName, data);
				JSCaller.callJs ("console.log", new[]{ex.ToString()});
			}

			//'{"method":"chellengeFriend","msg":"alamakota", "platform":"facebook"}'
			//	("socialAPI",)
		}

		public static void callJs(string functionName, object[] paramsArray )
		{	
			Application.ExternalCall(functionName, paramsArray);
		}

		public static void consoleLog(string message )
		{	
			JSCaller.callJs ("console.log", new[]{message});
		}
	}
}

