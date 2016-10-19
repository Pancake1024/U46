using UnityEngine;
using System.Collections;
using System;

namespace com.miniclip.awards
{
	public class AwardNotification : MonoBehaviour, IStateable 
	{
		public static float   Speed = 0.5f;
		public static Vector3 StartScreenPos;
		public static float	  WaitTime = 1.5f;
		
		public Vector3 StartPos = new Vector3();
		
		private NotificationData	_notificationData;
		private StateMachine		_stateMachine;
		private AwardSlot			_allocatedSlot = null;
			
		GUITexture _guiTexture;
		Texture2D  _texture2D;
		
		internal event EventHandler<EventArgs> SlotRequested;
		
		public AwardSlot AllocatedSlot
		{
			get{ return _allocatedSlot; }
			set
			{
				_allocatedSlot = value;
				
				if(_stateMachine.CurrentState.Id == AwardStateId.WAITING_FOR_SLOT)
				{
					_stateMachine.ChangeState( AwardStateId.MOVING_IN );
				}
			}
		}
		
		
		public uint AwardId
		{
			get
			{ 
				if(_notificationData == null)
					return 0;
				
				return _notificationData.Id;
			}
		}
		
		
		public string Name
		{
			get
			{ 
				if(_notificationData == null)
					return "";
				
				return _notificationData.Title;
			}
		}
		
		
		public string Description
		{
			get
			{ 
				if(_notificationData == null)
					return "";
				
				return _notificationData.Description;
			}
		}
		
		//------------------
		
		public GUITexture GUITexture
		{
			get{ return _guiTexture; }
			set
			{
				_guiTexture = value;
			}
		}
	
		public StateMachine StateMachine
		{
			get{ return _stateMachine; }
		}
		
		//-----------------------
	
		public NotificationData NotificationData
		{
			get{ return _notificationData; }
			set
			{
				_notificationData = value;				
				createGUITexture(_notificationData);
				
				//Each time we receive new Award Notification Data we want to wait for a new
				// 'AwardSlot' to become available
				_stateMachine.ChangeState( AwardStateId.WAITING_FOR_SLOT );
			}		
		}
		
		public bool IsAvailable
		{
			get
			{
				/*
				if(_stateMachine.CurrentState == null)
				{
					return false;
				}
				*/
				return (_stateMachine.CurrentState.Id == AwardStateId.AVAILABLE ); 
			}
		}
		
		
		//--------------------------------------
		// MonoBehavior Methods
		//--------------------------------------
			
		void Awake()
		{
			Debug.Log("-> AwardNotification::Awake()");
			InitStateMachine();
		}
		
		// Use this for initialization
		void Start() 
		{
			Debug.Log("-> AwardNotification::Start()");
			//ReferenceComponents();
			_guiTexture = this.gameObject.GetComponent<GUITexture>();
		}
		
		// Update is called once per frame
		void Update() 
		{
			_stateMachine.Update();
		}    
		
		
		//--------------------------------------
		// Methods
		//--------------------------------------
		
		private void InitStateMachine()
		{
			Debug.Log("-> AwardNotification::Awake()");
			//Init State Machine
			_stateMachine = new StateMachine();
			_stateMachine.AddState(new Available(AwardStateId.AVAILABLE, this));
			_stateMachine.AddState(new WaitingForSlot(AwardStateId.WAITING_FOR_SLOT, this));
			_stateMachine.AddState(new MovingIn(AwardStateId.MOVING_IN, this));
			_stateMachine.AddState(new MovingOut(AwardStateId.MOVING_OUT, this));
			_stateMachine.AddState(new Displaying(AwardStateId.DISPLAYING, this));
			_stateMachine.SetStartState( AwardStateId.AVAILABLE );
		}
				
		/*
		private void ReferenceComponents()
		{
			Debug.Log("-> AwardNotification::ReferenceComponents()");
			
			//get references to the sub-components of the AwardNotification panel
			Transform trans = this.transform.FindChild("AwardIcon");
			
			if(trans != null)
			{
				Debug.Log("-> Icon Transform found :)");
				_iconMaterial = trans.renderer.material;
			}
			
			trans = null;
			
			//-------------------
			
			trans = this.transform.FindChild("txtAwardEarned");
			
			if(trans != null)
			{
				Debug.Log("-> Award Earned found :)");
				_awardEarned = trans.GetComponent<TextMesh>();
				
				if(_awardEarned != null)
				{
					_awardEarned.text = "Award! Award! Award!";
				}
			}
			
			trans = null;
			
			//-------------------
			
			trans = this.transform.FindChild("txtAwardDescription");
			
			if(trans != null)
			{
				Debug.Log("-> Award Description found :)");
				_awardDescription = trans.GetComponent<TextMesh>();
				
				if(_awardDescription != null)
				{
					_awardDescription.text = "Blah! Blah! Blah!";
				}
			}
			
			trans = null;
		}
		*/	
		
		internal void RequestAwardSlot()
		{
			this.SlotRequested(this, null);
		}
		
		internal void VacantedSlot()
		{
			_allocatedSlot.UsedBy = null;
			_allocatedSlot = null;
		}
		
		private void createGUITexture( NotificationData notificationData )
		{
			_texture2D = new Texture2D( notificationData.Width, notificationData.Height, TextureFormat.ARGB32, false);
			_texture2D.LoadImage( notificationData.BitmapData ); 
			
			_guiTexture.texture = _texture2D;
			_guiTexture.pixelInset = new Rect(-(notificationData.Width / 2),
											  -(notificationData.Height / 2),
												notificationData.Width,
												notificationData.Height);
			this.StartPos = CalcNotificationOffScreenPosition( notificationData.Width, notificationData.Height ); 	
			this.transform.position = new Vector3( this.StartPos.x,
												   this.StartPos.y,
												   this.StartPos.z );	
		}	
		
		public static Vector3 CalcNotificationOffScreenPosition(int width, int height)
		{
			float x;
			float y;
			
			float offsetPixelX = 0;
			float offsetPixelY = 0;
					
			if(StartScreenPos.x <= 0)
			{
				offsetPixelX = -((width/2) + 10);
			}
			else if(StartScreenPos.x >= 1)
			{
				offsetPixelX = (width/2) + 10;
			}
			
			if(StartScreenPos.y <= 0)
			{
				offsetPixelY = -((height/2) + 10);	
			}
			else if(StartScreenPos.y >= 1)
			{
				offsetPixelY = (height/2) + 10;	
			}
				
			x = StartScreenPos.x + (offsetPixelX / Screen.width);
			y = StartScreenPos.y + (offsetPixelY / Screen.height);
			
			return new Vector3(x,y,0);
		}
	}
}
