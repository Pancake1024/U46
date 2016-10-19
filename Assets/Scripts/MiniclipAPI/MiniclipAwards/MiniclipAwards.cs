using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using com.miniclip;
using MiniJSON;


//Awards Shader: Unlit/Transparent

namespace com.miniclip.awards
{
	public class MiniclipAwards : AbstractUpdateableService, IAwardsAPI
	{	   
		public static float ZDepth = 0.0f;

		private const string AWARDS_INIT = "awards_Init";
		private const string AWARDS_SHOW_AWARD = "awards_ShowAward";
				
		private AwardsViewController _awardsViewController; 
		private AwardsAPI _api = new AwardsAPI();
		
		private Texture2D _awardTexture;
			
		//public event EventHandler<AwardNotificationEventArgs>	AwardNotificationReceived;
		public event EventHandler<AwardDataEventArgs>			AwardGiven;
		public event EventHandler<FailedEventArgs>				AwardFailed; //TODO: we should also return the awardId along with this.
		public event EventHandler<MessageEventArgs>				AwardError;
		public event EventHandler<MessageEventArgs>				DebugMessage;
		public event EventHandler								Initialized;
		
		//public event EventHandler<AwardsC>				DebugMessage;
		
		
		internal override AbstractUpdateable Updateable
		{
			get
			{
				return _awardsViewController;
			}
		}
	
		public MiniclipAwards()
		{
			_awardsViewController = new AwardsViewController();
		}
		
		//-------------------------------------
		// A AMF Service methods
		//-------------------------------------
			
		public void Init()
		{
			Debug.Log("-> MiniclipAwards::Init()");
			Init( AwardsConfigID.BOTTOM_STACKED );
		}
		
		public void Init(string awardConfigId)
		{
			Debug.Log("-> MiniclipAwards::Init(" + awardConfigId + ")");
			
			string jsonStr = "{ \"award_config_id\" : " + awardConfigId + " }";
			JSCaller.Call( AWARDS_INIT, jsonStr );
		}
			
		public void ShowAward(uint awardId)
		{
			Debug.Log("-> MiniclipAwards::ShowAward() - awardId: " + awardId);

			if (awardId < 1)  //awards ID must always be greater than 0
			{
				Debug.Log("awardId can't be smaller than one!");
				return;
			}
			
			string jsonStr = "{ \"award_id\" : " + awardId + " }";
			JSCaller.Call( AWARDS_SHOW_AWARD, jsonStr );  
		}
		
		public void GiveAward(uint awardId)
		{		
			_api.GiveAward(awardId);
		}
		
		
		public void HasAward(uint awardId)
		{	
			_api.HasAward(awardId);
		}
		
		//-------------------------------------
		// Awards Notification 'Stack'
		//-------------------------------------
		
		AwardData _awardData;
		AwardSlot _slot;
		AwardNotification _notification;
		public void Update()
		{
			_awardsViewController.Update();
		}
		
		//-------------------------------------
		// EventHandlers
		//-------------------------------------
		
		internal override void ProcessData( string noticeID, string json )
		{
			//string msg = "";
			//Dictionary<string, object> fromJson;
			//string bitmapString;
			NotificationData notificationData;
			
			switch(noticeID)
			{
				case NoticeID.AWARD_INITIALIZED:
					
					//TODO: Configure Awards !!!!!
					_awardsViewController.ConfigureAwardNotifications( json );
				
				
					if(Initialized != null)
					{
						this.Initialized( this, EventArgs.Empty );
					}
				
					break;
				
				case NoticeID.AWARD_GIVEN:
				
					this.DebugMessage( this, new MessageEventArgs("-> MiniclipAwards::ProcessData() - NoticeID.AWARD_GIVEN") );
				
					notificationData = BuildNotificationData( json );
				
					_awardsViewController.AddAwardData( notificationData ); 
				
					if(AwardGiven != null)
					{
						this.AwardGiven( this, new AwardDataEventArgs( notificationData ) );
					}
				
					break;
				
				case NoticeID.AWARD_FAILED:
					
					//this.DebugMessage( this, new MessageEventArgs("-> MiniclipAwards::ProcessData() - AwardFailed! - json: " + json) );
				
					if(AwardFailed != null)
					{		
						this.AwardFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
				
				case NoticeID.AWARD_SERVICE_ERROR:
					
					if(AwardError != null)	
					{
						this.AwardError(this, new MessageEventArgs( MiniclipUtils.ExtractMessage(json) ) );
					}
			
					break;
			}
		}
		
		//-----------------------------------------
		// Helper Methods
		//-----------------------------------------
		
		private byte[] ExtractBitmapData(Dictionary<string,object> fromJson)
		{
			byte[] bitmapData = null;
			
			if(fromJson.ContainsKey("bitmapString"))
			{
				string bitmapString  = fromJson["bitmapString"].ToString();
				bitmapData = Convert.FromBase64String( bitmapString );
			}
			
			return bitmapData;
		}
		
		//temp test method
		public void ConfigureAwardNotifications(string json)
		{
			_awardsViewController.ConfigureAwardNotifications(json);
		}
		
		
		/*
		public static AwardData BuildAwardData(string json)
		{
			Dictionary<string, object> fromJson = Json.Deserialize (json) as Dictionary<string,object>;
			return BuildAwardData( fromJson );
		}
		*/	
		
		private NotificationData BuildNotificationData(string json)
		{
			Debug.Log("~~~~~~> MiniclipAwards::BuildNotificationData() - json: " + json);
			
			Dictionary<string,object> fromJson = Json.Deserialize(json) as Dictionary<string,object>;
							
			
			//json = "{}"
			
			uint id = 0;
			string title = "";
			string description = "";
			string bitmapString = "";
			byte[] bitmapData;
			
			if(fromJson == null)
			{
				Debug.LogError("~~~~~~> MiniclipAwards::BuildNotificationData() - fromJson is null!");
			}
	
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
			
			if(fromJson.ContainsKey("bitmapString"))
			{
				bitmapString = fromJson["bitmapString"].ToString();
			}
			
						
			bitmapData = Convert.FromBase64String( bitmapString );
			
						
			return new NotificationData(id,title,description,bitmapData);	
		}
		
			
	
		/*
		private Texture2D BuildAwardTexture(string bitmapString)
		{
			Texture2D awardTexture;
			
			byte[] base64Bytes = Convert.FromBase64String( bitmapString );
		
			awardTexture = new Texture2D(  AWARD_NOTIFICATION_WIDTH, AWARD_NOTIFICATION_HEIGHT, TextureFormat.ARGB32, false);
			awardTexture.LoadImage(base64Bytes);
			
			return awardTexture;
		}
		*/
		
		/*
		public static byte[] createAwardNotificationByteArray()
		{
			string bitmapString = "iVBORw0KGgoAAAANSUhEUgAAAQgAAAA8CAYAAACaaiTwAAAgH0lEQVR42u1dB1gVx/fFivTyBGyIYuwxamJvWNCoIBbsldi7Yu8do9hRI0Y";
			bitmapString += "ldmPXWGLXoCYqllhi7F0xoqLGbuT83xkyz33wGs1fkv/e7zvfezs7Ozuzu3P23jszd62srKw8tMiphdeECRMqnT9/fkJsbOzup0+fHiOgiiqq/GdF9nP2efZ";
			bitmapString += "9cgC54G9OIDfEk8PNmzdnqpdLFVVUIRcoSMLKKzo6OkK9LKqooooUcoIgie3btzdXL4cqqqiSUMgNVg8ePFj/MU72+vlTHFsfjuX9A/SwbnQb3Pz1sHo3VFH";
			bitmapString += "lHybkBqs3b97cSesTndm1CpPraDAzUINvWmmwrGNWrO6XC+uGFMCiDh6YFqDBigEBgkRUUUWVf4aQG6zSWmtYP6YNZjTSYHQ9Z4Q0ccbmfm64vqQAXt8eIfK";
			bitmapString += "8uD4U+8Z6YnZTLXm0KamShCqq/IMkTQli9zfDMb2hBn7FbVC3hA2GBjhhTS83XJzthRe/VcPbPxaIX26v6JZVaBg0OVRRRZX/BwQxq7E32pW3h6tdelQpZI0e";
			bitmapString += "vg5Y2MUde4Z64PJ8b9xZ/ykOTS2Co4tqYfXQzzC2gTNC/TSqT0IVVf7rBMFOPr2BBgU8MiKdlRWal3NA1LIGuLBvFs7v6I29Uwti1+QqeHr/Ah5eP4BbUSMws";
			bitmapString += "42nMEfO7Fyl3hlVVPkvE8TBpZOFyWCTKR2stASxpK8n3jz8Cb/t6IdfFvviZmQpvLg7SZDDiOYZ0b2+HWYPKIfZzTXCoblhbBtcPLxdvUOqqPJf1yBIEMu1BPE";
			bitmapString += "29jgOLA5EpaIZsGWOF9496IanN1fj59XVcGRdDRyMKI2f5hbBnileWNbNA1P8NTi7S9UmVFHlP0cQkUsmi+HLeiVtYZs5HfrXdsSxGblxaXVBPDlTGXGx/RD3";
			bitmapString += "Yg3i3l1FXNzb+F9ua9Pf3qqDmMhy2DnSE9Pqa7BqUAC2Tu2J+1fOqndMFVX+zQTBYUrOaaD2MKuFK+YEuYqhzV8nZse17/Ljwd7SeHnRFy9uBCLmXAfcPdYa9";
			bitmapString += "453wLX9tXBmXSFELcut1SKyY/f0Aoic74+NIdW15kZFhDXVYGo9DSK6++DYhnCVLFRR5d9IEBHdfITvYWSAE4YFOGJpVw12DnZH1LhsODQxJ3ZOzIGf5nkiark";
			bitmapString += "Xbh6siseXuuLlH9MR93q/Vou4pNUmXunKinv/BHFvf0Pcqx149WAuLu5ugRV984jyqVlwlGTlwABBGDRpEmLrlJ5C+zAG+jnoK/m3OUVfvXqlvU5x6tNrwXVS2";
			bitmapString += "/4PIgh2yqlas2JGS1d0qWGPGa1dsLCTK8LaumDd0Bw4Mr8g7u4vi1dXauL9g3qIe9wZcU/HI+55hCCBFw92IObqJsRc26XFTu3/dYi5/C1iLnyNmN8HIPrXtji";
			bitmapString += "6rhw2DrfFvjAv7JmZH6v6e4q5FtQu6LOQoHkzu5kGYS20aBmPpV3csapHNoGlndyxuJOb0ExINiv6/3tmctrZ2WHnzp0qA5gRR0dHnDx5Um37P4kgaFr0r+uI3";
			bitmapString += "l86oG9tB8xrr8HhUC/cXvsp/thdCrFHK+LFhRp4d88PcY/aIu7JUPwZPQnzxnyJr9vYYNvYzAaxamAmjGqUHgfCXPDk96p4fbMTXl4fhcdRHbB3nCf2jNXHkh4";
			bitmapString += "emNY6K6a2dsOucV64vqSgwLnZ+fDrTC88/yUPXh/PK/DTtOyi3snRJC5dugR3d3fs2rUr0b5ixYph/vz5KkGoBKESBIWLsdjROCGqaVlbHBjujt9Cc+ByuDduri";
			bitmapString += "qM6G0l8ehQefx5pire3KyNuIeNsG5ORXSsqUHLCvbo62drlCCCa6bD2iGZ8OBkBTw7UxdPoiobxb09pbB3hIPAwRCNLv3xkXKI3uKOp5G58GifJ47Pd8ONAxW0RN";
			bitmapString += "USi7tlE1O9kyPjxo1D6dKl9dLGjx+PWrVq6aX9+eefqU4QT58+NWhuvHnzBrGxsckq/8aNG6mSP7l1ePbsmUkTylz9Hj9+bHEnsaSO5uqTWtc9NcpJSts/KkFwWj";
			bitmapString += "XNi5CGzqhTPAu+75kVh0Z64PBoD+wbng0HR2XDr5NzCi3iyfHKeHnJF73rxxODhDGCWBacEYPqpsPPCzy0psg+xL1/ZRLvn/2iNWGWxePF7x/2PYvE+/thQms4/o";
			bitmapString += "0LwppZYdWwPNr0r3FwQVlMScEsziJFimDGjBni/+XLl5EuXTrdDRoyZAgyZMgAGxsb1K9fHy9evNAd5+HhgW3btumVlS1bNqxfv94sQfj6+orzEGPHjhX7Hj16BC";
			bitmapString += "8vL5FmbW0NjUZrck2ZIvbx3MOGDdMra8KECWjYsKH4T23Hzc0NhQoVEucwp/0Yy2+qDrLNhw8fRvfu3ZE7d26961CzZk1dm0i8lpxPyogRI8T5MmXKhAoVKoj/xjq";
			bitmapString += "JJXU0Vx9Ly+vWrRt69Oihy7tixQoULVpUj/B4vr/++itZ186StvO43r17/28I4vCKUOEDGO7vhBpFrNGpqh1WdM+KiM4arOyRVYxi0FFJkri5pigeH9ZqEaer6pGDh";
			bitmapString += "JIYflmYW2BTiDvGN0mP8z8U1Jokw83jVFkg6jOBuEeDPqTf+BzvzhcSBLF9pD06V7ZCyzJWWtLZijsnemBGQw2OrQtP1jXYtGkTbG1t8ccff6BJkya6jvjdd9/hyy+";
			bitmapString += "/1DmNgoOD0bVrV91xzs7OySKIzz//HHv37sX79++xZMkS8UBduHBBPGBbtmzB69evRd4dO3aIfXwbrVy5EgUKFNAri+WsXr1a1IFa0J078Yt7o6KixHFXrlwxWAdT+";
			bitmapString += "U3VgUKTrFy5chg1ahTWrFmjuw4lSpRI1CaacObOJ8nj008/xfnz53X7Tb1FzdXRXH2SUh6v+xdffKHL27p1a+TKlUvcL8q6detQpUoVi+pl6NpZ0vasWbP+bwji1pn";
			bitmapString += "Dwik4vr4zKhe0hu+nWYTvYXILF8xu54qFneJJYmOwG34c5I7zC/LjwZ7SiD1SER1qOOmIoWNNF2z/TnuRYjsnwqWD/ghplh4XthXGi4sNBLmYwuWIzDoo06N3ZkFsZF";
			bitmapString += "ZBEGHNrVC9oBWK57LClZOrcOfkCEEQkUtCk30tmjdvjk6dOul1wkqVKul19nv37okbnlKCSOiDsLe3Z4APvTQ+ZHw7sbyff/5ZqMkksSNHjoj9586dg4ODg/hfo0YN";
			bitmapString += "QRRKocbBjmFILM2fsA7yIedbUCmGrgPbyQ5iyfmowZGMk2OHG6qjufokpTySGu85zQbZWYcPH45p06bpNExuW1IvQ9cuJW1PU4Kg1z88qCSmBbrCt6g1av1NDsPrO2";
			bitmapString += "FcY2c9kqBGsb6PG45N88LdzcXFfIjRbTwEOYzv4o0HF1ri5Z0gHPy+KnZHVMT8QfkFhgQ4Y6CfPYb6a1X2lV54droRbmu1EFNYHeyAlb3tBJTphyc54toaD0EQ05tYo";
			bitmapString += "by3FfJorHD51GbcOTU2xQRx9epV8SAoOzffFL/99luim3fr1q1UJQges3btWp3/o2TJkqJsagisk3yo2rdvj4EDB4r/NEtIaBSqtTxGCRKdNF0Sirn8pupgKUEor4O5";
			bitmapString += "81G1Pnv2bJI6iak6Jue+mCqPdY2MjMTBgwfRrl070eFJehSaij/++KNF5Ri6dslp+0chiA3j2ohOFVjKFjW0BNGluoMYvRjo56gjiUnNXTCzjSvmt9dgSdes2DYkG2";
			bitmapString += "59XwT3tpTApE45EbW9jtASrv7cAJPa50BICwd808VOYEmwDVYMyILFfazRq3o67Jhog3d36+L01Lw4FZLdKEIbOaN/NUeBvUPcdelr2mfFnlHxBLFjggsalbRC/dJ2";
			bitmapString += "iHsThd93dRQ+lNM7UjYngm/o/fv367Zz5sxpkCBu3ryZJgTBtxJt9J9++klvn3yo+Ab09vYW/6UKLet54MABoeImhCExld9cHZJDEKbOR81ImliWdhJzdUzqfTFXXs";
			bitmapString += "eOHREaGipMT6kJZc+eXTiZnZycdH6ppF675LT9oxAEnXmTa2vQr6YjqmhNiyZlbBFUxV5HEgP+Jokxgc6Y2MwZ01u7YG6QK5ZpNYmrCz/BrbWf4n1Me6E1rAktITSF";
			bitmapString += "TtUcjDoqJ7VIj7D2GXDpxyK4/XNT4QQ1hmHac9craiPA/zJ9bitXhAa64MlBL0T/mBML+ufAgZWNxTyMPWHlEVpXgyf3b6UqQdBhtGHDBt12dHS0nonh6emZSHVOCU";
			bitmapString += "E0atQIY8aMSbRP+VBRq+GbLG/evLq0qlWrYuZMywObm8pvrg7JIQhz9cuRIwc2b95scScxV8ekEoS58mgKNW7cWGgGz58/F2nU3uifKFOmTIquXVLb/lEIYnlwAELr";
			bitmapString += "u6J6EWuhPQSWttWaC3b46m+S6FUrniQYJGZUQydMaBJvclCbuDAzN/7YWQnn9vhhTHM3bT4ntKv8wVG5cURigtg0MhMG1Ykf6rx3tAyuHWmFmV3yYqyWgBKCGkzRbBl";
			bitmapString += "RwC2j+O2jJSymj9bWo6uPPb4N1ggt4q+7NREX2wMPzrVFeDsPoRGlVBISxIIFC1C7dm28e/dObFO959tECr3kQUFB4k3C0Y9evXolMlOSQhD9+/dHnTp1hEecQ170vC";
			bitmapString += "tVVMqAAQPg4+ODoUOHftAGtSTm6uqq1yn48BIGtUcT+c3VITkEYa5+/fr1E9eSPp67d++KN7VyJCmhmKtjUgnCXHnXr1+Hi4uLcFhL2bhxo3BO8n5YWo6ha2dJ2z+qk";
			bitmapString += "5LrIKg9dPVxEI7JGkWywL+kjY4kqEl01pJEj5oOCK7jiMH1nDCiQbw2EdLUGfuHe+B0aE5809ENfes4oVVF/ZEMYxOmOFlKkgQ1ibiHAdoO3htxT4fF/2q3T6zMj+l";
			bitmapString += "NHdFTe/7iuTOL1aQlvTKjWw0Hgc5aLaVFOTvsC8uOG5s8cWXrp1jcNRtmN/dGzI2Ur+/gQ6AkCDlywRtGVq9bt65urJpCW/STTz4R+3nsvHnzzGoQzGeMIB48eICyZ";
			bitmapString += "cuK8mibchgzoQZBLzfLOH36tF4ZJDOq8hyO5fF8sxkjCFP5zdXB0EPO+pjrkKbqRy+/ckiSb2GWaYwgzNXRkvokpTwKNbZZs2bptvnS4Hk4AmZpOYaunSVt/6gEwQl";
			bitmapString += "R9D0EaEkhfuTCBrWK2ehIokV5O7TVagQdq9mjm68D+tR2FNrEEK02MVL7Fl/Xxw0nJ2TH4dHZDA51tqtihxkdM2P5QMOTpmZ9lQGztOYGfRLHl3ni3Mb8woG5Z6oDwlrZY";
			bitmapString += "lFXHxGZipO2LEFYM29cOZq2sSf4MCiJIaEYG05MrrC8ly9fJuvYJ0+eCI0mpflTUofk1o9vUE5s+hjXKS3LS045SW17mhEEFzhxnQPJQRIEUVMLvxI2aFDKFk21b+nWFe3";
			bitmapString += "Q3ife5KA20fdvopjV1hWRIz1wZGw2rfrvmIggeHwpb2sdKhXMjMblM2NAww/oWy+zlogyokmZjNpjMqJZuYzoVs0aUxu4iCnTHGExtIDLEJ49uAVVVFEljQlCok5xG9T/w";
			bitmapString += "haNy8SbHNQmOlS1R9ca8URB5yXnROwd6oFprVwSEUTlQln0CMJSDKnrJKJRqZGxVVHlH0wQxJefxZscjUrHaxOtKn4gCmoUnGW5KdgNq3tm1SOHgM9tk0UOxLTGriLMviqq";
			bitmapString += "qPIPJwhpclCbYKeXREGNok0lewyo64jl3eOHH7n6UxJECx8nzAjOh/mTKiYNE8pgQYdsmOqvwfyvSohYDzQ1UjpsqYoqKkGkgCCqmCCIhERR72+ioOnRTEsWfes4YGFHjc7M";
			bitmapString += "GNfKDb8vLoRbq4vi4UFfxMX2TRJiL3XA5q+LY2oTF8xo4ioiZNNZSa1CJQpVVPlfEEQh8wRBUvjKx144LDmy0aW6vRhy7P63PyK8vStGBrrg8NS8uLa4gA6Pj7ZC3LNZSca";
			bitmapString += "ikWXQr44jhtZzxKTGWrLQmh6zmnqr4fRVUSWtCeLioe0i+pIhguDXs9pqzQfOfeBn9sLauor1FxKnlxbH1a3lDIL7D4zJgUvz8upwZVFRvHm4FXFvjiUJMbd+wLKxxfDt";
			bitmapString += "oLzCMdq7lgOmaImC5odKEqqokgYEwWXQ0xt6x8eCbKLBvLYaMb16QF0HTGrmjPlaLeC7zpoUY+tAd/w+w1OH+wdGJ6th34Z0wdIpPjjzU5DA193yI7Sxi5jzoJobqqiSigT";
			bitmapString += "BiVFc0h3W3BVz2rhidltXoR0kFauH5sbj34Pw8m5vg+C+DWPyYu9Qd5yemF3g/JxiiHt9QA9/PduIV7dD8PLGSLy8GYI3MSvx7skW7b5DujzXzkVg/pB8OLKytMDSsQXRqoI";
			bitmapString += "dpge6YtvUnuqdT6IYi1zFmX0MhiNjEyRHOKOTSLiozZRwgRPPy5mnqd2m5NTnY4mxunHWpaXXIyntM0sQnFPARUzTm7lifGNnkxgW4CT8DA1L2aJakSwolCMT8rhlFL/ctyj";
			bitmapString += "YE2fXlzUJ5iEBHR6VTYeXt4eKqE/E8wtt8egXPzw5FYSnZ7vh2flgPD3XK377dDe8jZmuy9u6qisalbbTw+C6ToLsUnuuBDuKElypV7BgQXTu3FkX5OTfJpzNx2hIXAMhp/Fy";
			bitmapString += "YRDbJMPnsa1MTwlBMNoRy/j++++TRBA8JqkEYUmbklOf5EjlypV1zwunv6fkWhUvXlzAkuuRlPaZJQiGlGeYeXbwQf7x4MKq6loCqFjAGp6aDHB3TC++nmUKzCePtwSb+7lj1";
			bitmapString += "xAPgYcnfEQU7D/PVsPjYy0Rc6IXTi2shCMziwucWVod51fXxuPTffBgvy/e3PhS5B8alAPr51fDjZNNBE7v9kUXX3sxTTy1PxDMue686AwDxrn0cn0FQaKwVK5du2ZRvrdv3y";
			bitmapString += "Y7tDmn496/f99kHi6jZtBd1j9//vxigZmfn58gPqYlhyCMndfcA8uITgmPS0gQDMYiozCltE2m6sNFVCT8lE5plrFDJAYPHpwmZMrp2jExMWlHEByxYGh4OvuICvmtE3X+wrm";
			bitmapString += "sUKmwFXJn/ZCWw8UKxXLHQ6ZxeJNlTO6UA2+fjTeKvn7OWNRRg4193QTOrvpMRML+Y18tPD7bH1sG5dTtU4Lpj37tjeht5fDqagOEBudBSGcHLBvnpEP76nZi+JNmU1oQBFf";
			bitmapString += "iSQkICNA9APKB4s0heHO4eIYPJ1fhcSkzF3MxL1XFnj17JrqpBBf7MHKVLFcuwGHYMpknIaRs3bpVj7jYuRkf0ZCww8t8rJ9SJk6caJQg+vbtqzuvJDtT55UdlpDHtWzZUux";
			bitmapString += "jEBVG5JL7Gf2Kb/+EBMEQfzIPF8YZE0vaZKo+JBVlp+a9UwbUMXZvDcnkyZNFGYwGxV8Zo8NUWYULFzZaN2V+yvLly4VGIfNnyZIFbdu2Ndm+ZBMERyxaVbAXyJRBnxycbK";
			bitmapString += "3gav+BJJiWIb0V8mePR4m8H4ijmGdmUcb2OcXxOMrXKHppTYBZbVyxrFtWgaiIIri/swyenh+AH0OKYX9YCdw83BrRp4Lx7FYYHl4Jx+HvmmPD8GL4YVRBPD7eCdHbS2Lhm";
			bitmapString += "PwGh13pZGW70pIguDCLMQyY5u/vn4i9y5cvr7tRixYt0q32HD16tIg5yG2GIlOaMEqNhMFM5TZXj5IgPvvsMx3Sp08v9nH5udAEjx3TaTiLFy/Gt99+qzvekNbCEGbKTscVhM";
			bitmapString += "Z8EJIg+HDKY3744QeLzsuAuQywym3+J/jAypiTBImWb9iKFSuK6EtKgiAYYEVpMjDWhSGxpE3G6kPhqkrGcCApKElaBt0xdG+NEQTja3I/40kaqrehsvhsGaubUitgbEp5D";
			bitmapString += "IPlTpo0SVxDxpow1b4UEQT9CpUKWps0IyRBEHbW8dskD6lF0BfBciwBF3WFt9cIHJxbEPf3BOL81hZYO6IA7pzsheUDPhH7Di0LwuZJvuL/voU9sHmyn8h3e11lzBuSF/PH";
			bitmapString += "VsOfl4YJXDnYAz6FsqQpQRCZM2fW/ef6fmWYdnkjmc5vafzyyy/CFpUPC4XOI25nzJgxUUeUy4Vv376tO0dERIReXbgEWu6TwWr4IHB77ty5unxUr5k2e/Zsg+q4koRkfRj";
			bitmapString += "0RLncWEkQjKHA/yEhIXodztx5Dam8jDfJNEkIUrj82ZCJwXgLsp7Lli0zamJY0iZLVHB2fAaiZT4ZW9LQvTUUkUveX4IBhBgjgv/79Olj8jlhWcbqpkwnEcvyZSBbCoPhp";
			bitmapString += "omJQYKo/ZkNynhnNkkQ1Bbkf2oRJAemUZNgmqtdelFOo7K2+LqXNyK+LpUIoQNKoG3tnOju64BpLV0Edk/Jh3vb/XHg25qIvT4Jq8eU0+27f3UfNk1rods+vHaEyHd9eWkEN";
			bitmapString += "3YTJlFCpCVBsLPT4dWhQwed+kjwDW/s5sgw50pHFVVCpsnYlQkJgkLtIGHn4xtbag/KcOmyLlTTJWQ+Y2r5mTNnRDAb2fEleBw7pLJesrNQa1KKJec1dE0Y9Ypp06dPt9hJS";
			bitmapString += "U++kmiT2yZjHYgaiNQKlWAouaR0PMZrYD4ZyXrOnDk6Z6k5krIknSMUyvpRe+vSpYvOj5MmBME3LwOwWKpBECSGUvniySLeHEkvypk70B2Pjn6BJ6erG8S8EfkRVNleFyFqy";
			bitmapString += "1gv3NnaCLu+qYmXDzdj/8o+WDzSFz8uGiC+DP7y2W3tdi2Rtn/lAK0JUwtXIj7HVzWdtaRmnQgfw8Sg0Ikob5IMUGqKII4fP65Lk0FRZOxKQwQhO4kkCJo1DI7KNGVofYpMZ4";
			bitmapString += "cyBHNy6tQp8TaT7QkPD9erF9/Ccp+yPEvOa+iasLMwzViYOUMEIQnT0q+ZGWuTofoo/Rck3n379unU/6QShPQDMCAty1CaEfILbSkhCArDGErNVILbaUIQc1poUFrbsRiZKS";
			bitmapString += "kEkRAOWdKJcvzK2GJmP3ej4P4mZe10Ixr7JufB9Y3NEbW+OcIHV0fUrgVaYvgLce+fI+6vh1rcRty7y4h7ewbT+1fH1ogWuLTcT9Q3cnUzRK5tIxDSt5yY/clRmeOb0t5JyU";
			bitmapString += "jD8uYEBgYavTnVq1fXMxUuXryoc1YaMzEMEQSjVXGb0YUSilT1lR9wkSMEYWFhifLzuwsJOxq/+SHbIyMrKU0Mhk6T2gJVZ0vPK0cR6IuRwuvItHr16ukdJ7Wi5BCEpW0yV";
			bitmapString += "B+GdZP+ECnJIYgTJ07ozsfoWBK8Zkyj9mmqLEN1S5hfqVFy1IXb8pz0uRgrI0UEQQcjYSlBUGugH0K5P306K1055uBfwkZ8wo/YPz4XTk4vjKcXJmBCUF6RFnP3NMJHNsDaOV";
			bitmapString += "0Rc3s3wkfUxvOHO9Cr4Se4F/kVto4vha4NnBATmV+H1rUc8GUxmzQd5qQ3mkwtHY0SjD9o7MYzVBzT6GhjDELpuefbzVKCoONMnotqvRLULGjDyv206+n0o9pJx5uhtzQ7E/N";
			bitmapString += "Su2nQoIHIy69BMY3RsGUUZiVBcKiRDzu327SJX3ZvyXlle/PlyyfiKdJpdujQId1xdK7xy1EkjYROyqQShCVtMlQfmjrKIUkZOzSpBME4oMxDR3JC34p8BkyVZahuhgiCzyG/";
			bitmapString += "tcFrLElaXjtjZaQKQbCTmyMIjmzQMUnzQjnMKUcyiAOb6iL23mCDGB5cWQSc4XwL4tSknDgTmhOnFvvj9z2d0LWWK85HrcHEnlUwIqgEdmjNCuab2O1znN3eDlHza2FGhzwGia";
			bitmapString += "d9JXvxFbDUnm5NgmAcQAneaD6I9HbzYVcyPfcnvPFTp07V+R2UbxIlQfA4JUHIc0mCUJ5fCRnqjuqrUp0lOHRmiCCWLl0qOpHSVud/mi7Kr0vJeslhTtrp8rzS72LuvIyvyZEI";
			bitmapString += "uU8+sBwJkd5+6SfgV6mUbVcShEwzRhCWtslYfaRzleAwNNvE8ykJwtC9VQqHe5kn4Ydy6BuR9We7jZVlrG7K/HwelEOiRKtWrUS8S1NlpApB2FmnM0gO1pmsUK6A/vAnhzflSI";
			bitmapString += "aSIHyKWSMywtUoapfOIkYyZIyIqPHZBPYNc0fkzFI4saE5Zg8qrRdohttHVjXC7gmFcGiUh1g4NrhDScwcXlWgrX8+ce6Q+i7Y+g+eas3vG1AtTGvhRJ2EE2hMTbZJSozK5J6X";
			bitmapString += "b3Dld0uVk8I4EmTpB3RTq02G6sPRi9S6FikRY9cqYR7peE1uGUkmCHfHDAYJgg5J5UQpSQwJJ1BZYl7QGSo7PpdtMzSdxO7B7ljfLwdOrPTH1T1BOpzdEIgN/XOI/Vw4FuRri";
			bitmapString += "zk9HHWoVSILGpe2xbQAjbpYSxVVLBSLRzG+yBvfeQtmz5TIzKC/gRqDHK1QahEkCHcnfYLoVc8OkWvr4ui2QD0c3BSI4b1KoXLBLDqC4Hc1DM2aXNsrK1b1+ABuy331StokI";
			bitmapString += "h3Wf2qgK9aPVUPSqaJKqhEEv4HBJd58+8rOlss1g0lfBM0NzqykyaHUHqST8uuvHPD9xELYMLNEIoztlh9+JT6YF2MaOSVpuTg/2EPn5O5wTwH+5zmH+zlhdvO8qvagiiqpS";
			bitmapString += "RDC8dTdR7x9qxXOkujNnNctoyAMmh6ONumM+igI5jFnXnyeJ7Oeb4Hf9+TXuCzB+CbOQlMY1sxeB5bZu4ajILkzu9RgMaqokuoEQS1iVhNvTGnoit6+jmKoUJocxlAkZyZB";
			bitmapString += "Htmd48mDv1LVb+ljI3wESgRWiDcLahS10SMIfrbP0Of1DIErTJV1YD3H+Dur5KCKKmlJEBSq5vT+M5aC+BpVUw0mNnDBgFqO+KqSvUWkQVQtaq33hleC+5Tf6GxTMd5JaQkm";
			bitmapString += "N3XRmhDx0a4kpjfUIDyoBC4e3q7eaVVUSUuCkCK/WEXnJaNFh7croSONmY01GK19Y/eq7oiW5ewEaVg6MYrmy+QmLsIhOriOkwiN31pLEB18HMxiQqCLOP+60W1EvSTkF7ZU";
			bitmapString += "UUWVZBLEs2fPjqa0EEkakUsmY3n/AMwI9EZoHY2YkMRpzZYirKk3Irr5iOMYtp7Rn8xhuL+TOJbf41RFFVVST8gNVnfv3v0uLQqnScII2JFLQvXe6kahJRf5tifZrBgYID6j";
			bitmapString += "ZwlIDqqmoIoqqSvkBqtx48ZVVi+FKqqoklDIDVZa8YqOjo5QL4cqqqgihZxAbiBB5OSfmJiYdeplUUUVVcgFf5MDucHKQ5LE1q1bW8TGxu5WL5Eqqvz/E/Z9coCCHDz+D3c8N";
			bitmapString += "OaVr8zAAAAAAElFTkSuQmCC";
		
			return Convert.FromBase64String( bitmapString );
		}
		*/
		
		
	}
}