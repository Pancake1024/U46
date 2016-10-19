using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip.awards
{
	public class AwardsViewController : AbstractUpdateable
	{
		internal override event EventHandler<UpdateEventArgs> updatePoolRequested;
		
		private Queue<AwardNotification>  _awardNotificationsWaiting;
		private Queue<NotificationData>   _notificationDataWaiting;
		
		//private float _awardSeparation 	= 0.14f;
		private int	_maxAwardsDisplayed 	= 3;
		private int	_numAwardNotifications 	= 4; 
		
		private AwardNotification[] _awardNotifications;
		
		
		private AwardSlot[]	_awardSlots; //Award Notifications occupy an empty slot while displaying
		
		private bool _objectsActivated = false;
		
		
		//---------------------------------------
		// Accessors
		//---------------------------------------
		
		internal int NotificationDataWaitingCount
		{
			get{ return _notificationDataWaiting.Count; }
		}
		
		internal int AwardNotificationsWaitingCount
		{
			get{ return _awardNotificationsWaiting.Count; }
		}
		
		
		//---------------------------------------
		// Methods
		//---------------------------------------
		
		public AwardsViewController()
		{
			Init();
		}
		
		private void Init()
		{
			//Create queues...
			_awardNotificationsWaiting	= new Queue<AwardNotification>();
			_notificationDataWaiting	= new Queue<NotificationData>();
		
			//CreateAwardNotifications();
		}
						
		public void ConfigureAwardNotifications(String json)
		{
			Debug.Log("-> AwardsViewController::ConfigureAwardNotifications()");
			
			//float start_x = 0f;
			//float start_y = 0f;
						
			Dictionary<string,object> fromJson = Json.Deserialize(json) as Dictionary<string,object>;
						
			if(fromJson.ContainsKey("speed") )
			{
				AwardNotification.Speed = Convert.ToSingle( fromJson["speed"] );
			}
			
			if(fromJson.ContainsKey("wait") )
			{
				AwardNotification.WaitTime = Convert.ToSingle( fromJson["wait"] );
			}
			
			if(fromJson.ContainsKey("start") )
			{
				AwardNotification.StartScreenPos = SetStartPosition( fromJson["start"] as Dictionary<string,object> );
			}
			
			
			//Debug.Log("-> AwardsViewController::ConfigureAwardNotifications() - start_x: " + start_x
			//	+ ", start_y: " + start_y );
			
			//AwardNotification.StartPos = new Vector3(start_x, start_y, 0f);
			
			
			
			if( fromJson.ContainsKey("slots") )
			{
				Debug.Log("-> AwardsViewController::ConfigureAwardNotifications() - found 'slots'!");				
				Dictionary<string,object> slotsDict = fromJson["slots"] as Dictionary<string,object>;
				
				_maxAwardsDisplayed = slotsDict.Count;
				_numAwardNotifications = _maxAwardsDisplayed + 1;
				
				_awardSlots 		= new AwardSlot[ _maxAwardsDisplayed ];
				_awardNotifications = new AwardNotification[ _numAwardNotifications ];
		
				BuildAwardSlots(slotsDict);
				
				for(int i=0; i < _numAwardNotifications; i++)
				{
					_awardNotifications[i] =  ConstructAwardNotification(i);//MonoBehaviour.Instantiate(Resources.Load("Prefabs/AwardNotification")) as GameObject;
				
					if(_awardNotifications[i] == null)
					{
						Debug.LogError("-> AwardsViewController::CreateAwardNotifications() - _awardNotifications[" + i + "] is NULL :(" );
					}
					else
					{
						
					}
				}
				
			}
		
		}
		
		
		private Vector3 SetStartPosition( Dictionary<string,object> startDict )
		{
			float x = 0f;
			float y = 0f;
			float z = 0f;
			
//		  	float offsetPixelX = 0;//-132;
//			float offsetPixelY = 0;//30;
			
		
			if(startDict.ContainsKey("x"))
			{
				x = Convert.ToSingle( startDict["x"] );
			}
			
			if(startDict.ContainsKey("y"))
			{
				y = Convert.ToSingle( startDict["y"] );
			}
			
			if(startDict.ContainsKey("z"))
			{
				z = Convert.ToSingle( startDict["z"] );
			}
			
			
			/*
			if(startDict.ContainsKey("offset_pixel_x"))
			{
				offsetPixelX = Convert.ToSingle( startDict["offset_pixel_x"] );
			}
			
			if(startDict.ContainsKey("offset_pixel_y"))
			{
				offsetPixelY = Convert.ToSingle( startDict["offset_pixel_y"] );
			}
	
			
			Debug.Log("---> AwardsViewController::SetStartPosition() - Screen.Width: " +  Screen.width + ", Screen.Height: " + Screen.height);
		
			x = x + (offsetPixelX / Screen.width);
			y = y + (offsetPixelY / Screen.height);
			
			Debug.Log("+++> AwardsViewController::SetStartPosition() - (offsetPixelX / Screen.width): " + (offsetPixelX / Screen.width) + ", (offsetPixelY/ Screen.height): " + (offsetPixelY/ Screen.height) );
			Debug.Log(">>>> AwardsViewController::SetStartPosition() - x: " +  x + ", y: " + y );
			*/
			return new Vector3(x,y,z);
		}
		
		
	//	private 
			
		
		
		private void BuildAwardSlots( Dictionary<string,object> search)
		{		
			//Dictionary<string, AwardSlot> slots = new Dictionary<string, AwardSlot>();
			
			AwardSlot awardSlot;
			
			float relativePixelX = 0f;
			float relativePixelY = 0f;
			
			float relativeRatioX = 0f;
			float relativeRatioY = 0f;
			
			float posX = 0f;
			float posY = 0f;
			
			
			//foreach (string kv in search.Keys) 
			foreach (KeyValuePair<string,object> kv in search)
			{
				Debug.Log("---> AwardsViewController::BuildAwardSlots() - Slot Value: " + kv);
				
				

				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				
				if(inner.ContainsKey("rel_pixel_x") )
				{
					relativePixelX = Convert.ToSingle( inner["rel_pixel_x"] );
				}
				
				if(inner.ContainsKey("rel_pixel_y") )
				{
					relativePixelY = Convert.ToSingle( inner["rel_pixel_y"] );
				}
				
				relativeRatioX = (relativePixelX / Screen.width);
				relativeRatioY = (relativePixelY / Screen.height);
				
				posX = AwardNotification.StartScreenPos.x + relativeRatioX;
				posY = AwardNotification.StartScreenPos.y + relativeRatioY;
				
				awardSlot = new AwardSlot( new Vector3(posX, posY, 0f) ); ///CurrencyItem(inner); 
				
				Debug.Log("---> AwardsViewController::BuildAwardSlots() - slot[" + kv.Key + "] .x: " + awardSlot.Position.x + " ,y: " + awardSlot.Position.y + " ,z:  " + awardSlot.Position.z);
					
			
				_awardSlots[ Convert.ToUInt32((kv.Key as string) ) ] =  awardSlot;
				
				//TODO: !!!!!!!!!
//				slots.Add(/*kv*/kv.Key, itemValue);	
			}
			

		}
		
		//private void BuildSlots
		
		
		private AwardNotification ConstructAwardNotification(int id)
		{
			
			AwardNotification awardNotification;
			GameObject goAwardNotification = MonoBehaviour.Instantiate(Resources.Load("Prefabs/AwardGUITexture")) as GameObject;
			//goAwardNotification.name = "AwardNotification" + id.ToString();
			
			awardNotification = goAwardNotification.AddComponent<AwardNotification>();
			awardNotification.name = "AwardNotification" + id.ToString();
			
			
			
			awardNotification.StartPos = AwardNotification.CalcNotificationOffScreenPosition(264, 60); //width/height of default notification
			
			awardNotification.transform.position = new Vector3( awardNotification.StartPos.x,
																awardNotification.StartPos.y,
																awardNotification.StartPos.z );
																						
			Debug.Log(">>>>>>>> AwardsViewController::ConstructAwardNotificatio() - awardNotification.StartPos.x: " + awardNotification.StartPos.x + ",  awardNotification.StartPos.y: " +  awardNotification.StartPos.y );	
			Debug.Log(">>>>>>>> AwardsViewController::ConstructAwardNotificatio() - AwardNotification.StartScreenPos.x: " + AwardNotification.StartScreenPos.x + ",  AwardNotification.StartScreenPos.y: " + AwardNotification.StartScreenPos.y );	
			
			//			awardNotification.transform.position = new Vector3( AwardNotification.StartPos.x, AwardNotification.StartPos.y, AwardNotification.StartPos.z); 
			
			awardNotification.SlotRequested += OnSlotRequested;
			//awardNotification
			
			
			Debug.Log(" ******************> AwardsViewController:: ConstructAwardNotification( " + id + " )");
					
			return awardNotification;
			
			
			//return null;
		}
		
		private void SetObjectActivation( bool activation )
		{
			Debug.Log(" ******************> AwardsViewController::SetObjectActivation( " + activation + " )");
			
			for(int i=0; i < _awardNotifications.Length; i++)
			{
				_awardNotifications[i].gameObject.SetActive( activation );
			}
			
			_objectsActivated = activation;
		}
		
		public void AddAwardData(NotificationData notificationData)
		{
			if(!_objectsActivated)
			{
				SetObjectActivation(true);
			}
			
			this.ChangeUpdatePool( UpdatePoolId.UPDATE );
			
			//trigger and event to make sure that this ViewController is in the UpdatePool
				
			_notificationDataWaiting.Enqueue( notificationData );
		}
		
		//-------------------------------------
		// Awards Notification 'Stack'
		//-------------------------------------
		
		NotificationData _notificationData;
		AwardSlot _slot;
		AwardNotification _awardNotification;
		internal override void Update()
		{
			Debug.Log("-> AwardsViewController::Update() - UPDATE");
			
			if(_notificationDataWaiting.Count > 0)
			{
				_awardNotification = LookForAvailableNotification();
				
				if(_awardNotification != null)
				{
					Debug.Log("-> AwardsViewController::Update() - Dequeue _notificationData !!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					_notificationData = _notificationDataWaiting.Dequeue();
					
					//assigning AwardData to the notification will trigger it to start 
					//loading the icon (the icon Id is part of the Data)
					_awardNotification.NotificationData = _notificationData;
				}
				
				_notificationData = null;
				_awardNotification = null;
			}
			
			
			//---------------------------------------------------
			
			if(_awardNotificationsWaiting.Count > 0)
			{
				_slot = LookForAvailableSlot();
				
				if(_slot != null)
				{
					_awardNotification = _awardNotificationsWaiting.Dequeue();
					_slot.UsedBy = _awardNotification;
					
					//allocating a slot to a notification will have the affect 
					//of causing that notification to move towards the slot
					_awardNotification.AllocatedSlot = _slot;	
				}
			}
			
			//---------------------------------------------------
			
			if(_notificationDataWaiting.Count < 1 && _awardNotificationsWaiting.Count < 1)
			{			
				if(HaveAllNotificationsFinished())
				{
					SetObjectActivation(false);
					this.ChangeUpdatePool( UpdatePoolId.NONE );
					
					//Debug.Log("-> AwardsViewController::Update() - Notifications finished, STOP UPDATING !!!!");
				}
			}
		}
		
		
		private bool HaveAllNotificationsFinished()
		{
			for(int i=0; i < _awardNotifications.Length; i++)
			{
				//get the first award notification that is available			
				if(! _awardNotifications[i].IsAvailable)
				{
					return false;//_awardNotifications[i];
				}
			}
			
			return true;
		}
		
			
		private AwardNotification LookForAvailableNotification()
		{
			for(int i=0; i < _awardNotifications.Length; i++)
			{
				//get the first award notification that is available			
				if(_awardNotifications[i].IsAvailable )
				{
					return _awardNotifications[i];
				}
			}	
			
			return null;
		}
		
		private AwardSlot LookForAvailableSlot()
		{
			for(int i=0; i < _maxAwardsDisplayed; i++)
			{
				//get the first slot from the bottom that is available
				if(_awardSlots[i].IsAvailable)
				{
					return _awardSlots[i];
				}
			}
				
			return null;
		}
		
		
		internal void ChangeUpdatePool(int updatePoolId)
		{
			this.updatePoolRequested( this, new UpdateEventArgs(updatePoolId) );
		}
	
		//--------------------------------------
		// Events Handler
		//--------------------------------------
		
		internal void OnSlotRequested(object sender, EventArgs args)
		{
			Debug.Log("AwardsService::OnSlotRequested(..)");
			
			AwardNotification awardNotification = (sender as AwardNotification);
			//awardNotification.AllocatedSlot = _awardSlots[0];
			
			_awardNotificationsWaiting.Enqueue(awardNotification);
			
			Debug.Log("AwardsService::OnAwardSlotRequested(..) Notification Count: " + _awardNotificationsWaiting.Count);
		}
	}
}

