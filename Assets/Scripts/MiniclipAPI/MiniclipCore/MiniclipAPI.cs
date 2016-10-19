using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using com.miniclip;
using com.miniclip.awards;

using MiniJSON;

//----------------------------------------------------
// Important: attach this script to an object in your 
// scene hierarchy called: MiniclipAPI
//----------------------------------------------------

//Note: Namespaces cannot be applied to MonoBehaviour's in Unity 3.x (ok for Unity 4)
//namespace com.miniclip
//{
	public class MiniclipAPI : MonoBehaviour
	{
		//External Function Name Constants
		public const string SHOW_LOGIN_BOX 			= "showLoginBox";
		public const string SHOW_HIGHSCORES 		= "showHighscores";
		public const string SAVE_HIGHSCORE			= "saveHighscore";
		public const string GET_USER_DETAILS		= "getUserDetails";


		public const string GET_EMBED_VARIABLES		= "getEmbedVariables";
		public const string GET_TOP_HIGHSCORES		= "getTopHighscores";
		public const string WRITE_HIGHSCORE			= "writeHighscore";
	
		private ICurrenciesAPI _currencies;
		private IAwardsAPI 	 _awards; 
		private Storage _storage;
		private MCSocial _social;
		private MCLeaderboards _leaderboards;
		private List<AbstractUpdateable>	_updateables;

		List<AbstractUpdateable> _updatePool;
		List<AbstractUpdateable> _lateUpdatePool;
		List<AbstractUpdateable> _endOfFramePool;

		private FlashSwitcher _flashSwitcher;
	
		//private CoreNoticeHandler _coreNoticeHandler;

		//If true then object in NOT destroyed when new scene is loaded
		public bool dontDestroyOnLoad = true;
	
		//private CoreNoticeHandler _coreNoticeHandler;
	
		//----------------------------------------------
		// Events
		//----------------------------------------------
		 
		public event EventHandler<EmbedVariablesEventArgs>	EmbedVariablesReceived;
	
		public event EventHandler<UserDetailsEventArgs>	UserDetailsReceived;
		public event EventHandler<UserDetailsEventArgs> LogInCompleted;
	
		public event EventHandler<UserDetailsEventArgs> SignUpCompleted;
		
		public event EventHandler						LogInCancelled;
		public event EventHandler						HighscoresClosed;
	
		public event EventHandler<HighscoresEventArgs> 	HighscoresReceived;

		public event EventHandler<FailedEventArgs> 		HighscoresFailed;

		public event EventHandler<MessageEventArgs> 	HighscoresError;
	
		public event EventHandler<UndefinedEventArgs>	UndefinedEvent;

		//public event EventHandler<StorageEventArgs>	StorageEvent;
		
						
		
		//public event EventHandler<MessageEventArgs> 	DebugMessage;	

	
		//----------------------------------------------
		// Monobehavior Methods
		//----------------------------------------------
	
		public IAwardsAPI Awards
		{
			get { return _awards; }
		}

		public ICurrenciesAPI Currencies
		{
			get { return _currencies; }
		}

		public Storage storage
		{
			get { return _storage; }
		}

		public MCSocial social
		{
			get { return _social; }
		}

		public MCLeaderboards leaderboards
		{
			get { return _leaderboards; }
		}

		//-- MonoBehaviour Methods --
		void Awake()
		{
			Init();
		}
		
		void Start() 
		{
			Debug.Log("-> MiniclipAPI::Start()");
		}
	
		//----------------------------------------------
		// Update Methods
		//----------------------------------------------
	
		private int _poolCount;
		private int _i;
		
		void Update()
		{		
			_poolCount = _updatePool.Count;

			//Comment out to: Temporarily disabled	
			//End of Frame Updates happen here... (currently only awards use this)
			for(_i=0; _i < _poolCount; _i++)
			{
				_updatePool[_i].Update();
			}
		}
	
	
		void LateUpdate()
		{
			StartCoroutine( EndOfFrame() );	
		}

		IEnumerator EndOfFrame()
		{
			yield return new WaitForEndOfFrame();
		
			_poolCount = _endOfFramePool.Count;
			
			//End of Frame Updates happen here...
			for(_i=0; _i < _poolCount; _i++)
			{
				_endOfFramePool[_i].Update();
			}
			//e.g) The FlashSwitcher call Texture2d.readPixels() which will throw an error if it is executed
			//before the very end of the frame.
		}
	
	
		//----------------------------------------------
		// Methods
		//----------------------------------------------
	
		private void Init()
		{
			//Debug.Log("-> MiniclipAPI::Awake()");
	
			if(dontDestroyOnLoad)
			{
            	DontDestroyOnLoad( this.gameObject );
			}
		
			createUpdatePools();
			
			//flashSwitcher is an Updateable, however, since it's a core component,
			//it has it's own reference, rather than being added to the updateables list
			_flashSwitcher  = new FlashSwitcher(); 
			_flashSwitcher.updatePoolRequested += OnUpdatePoolRequested;
		
			//Initialize a basic Awards API (with no GUI)
			//This can be replaced by a more sophisticated Awards Service by calling AddService(...)
			_awards = new SilentAwards();
			_storage = new Storage ();

			_storage.Get += OnStorageGetRequest;
			_storage.Set += OnStorageSetRequest;
			_storage.Delete += OnDeleteRequest;

			_social = new MCSocial();
			_leaderboards = new MCLeaderboards ();
		}
		

		private void OnDeleteRequest(StorageEventArgs e)
		{
			JSCaller.Call ("storage_delete", e.data);	
		}
		private void OnStorageGetRequest(StorageEventArgs e)
		{
			JSCaller.Call ("storage_get", e.data);	
		}
		private void OnStorageSetRequest(StorageEventArgs e)
		{
			JSCaller.Call ("storage_set", e.data);	
		}
	
		private void createUpdatePools()
		{
			//references to updateables (such as the AwardsViewController) are stored her permanently
			_updateables 	= new List<AbstractUpdateable>();  
		
			_updatePool  	= new List<AbstractUpdateable>();
			_lateUpdatePool = new List<AbstractUpdateable>();
			_endOfFramePool	= new List<AbstractUpdateable>();
		}


		//--public API Methods --
		public void AddService( AbstractService service )
		{
			Debug.Log("MiniclipAPI::AddService(...)");
		
			AbstractUpdateable updateable;
		
		
			if(service is IAwardsAPI)
			{
				Debug.Log("MiniclipAPI::AddService(...) - Adding Awards");
				_awards = null;
				
				//TODO: remove temporary if statement - awards should not require flashSwitcher
				if(service is MiniclipAwards)
				{
					service.SetFlashSwitcher(_flashSwitcher);
				}
			
				_awards = service as IAwardsAPI;
			}
			else if(service is ICurrenciesAPI)
			{
				service.SetFlashSwitcher(_flashSwitcher);
				this._currencies = service as ICurrenciesAPI;		
			}
			
			Debug.Log("MiniclipAPI::AddService(...) - _updateables.Count: " + _updateables.Count );
		
		
			// If the Awards service also contains an Updateable component 
			// (a visual ViewController that manages the AwardNotifications)
			// Add this to our updateables list...
			if(service is AbstractUpdateableService)
			{
				updateable = (service as AbstractUpdateableService).Updateable;
				_updateables.Add(updateable);
				updateable.updatePoolRequested += OnUpdatePoolRequested;
			}
		}
		
		public void ShowLoginBox()
		{
			_flashSwitcher.DisplayServicesAndCall( SHOW_LOGIN_BOX );
		}
	
		//---------------------------------------
		public void ShowHighscores()
		{
			//_flashSwitcher.DisplayServicesAndCall( SHOW_HIGHSCORES );
			SaveHighscore(0, 0, "");
		}
	
	
		public void ShowHighscores(uint level)
		{
			SaveHighscore(0, level, "");
	
		}
	
	
		public void ShowHighscores(uint level, string levelName)
		{
			SaveHighscore(0, level, levelName);

		}
	
		//---------------------------------------
		
		public void SaveHighscore(int score)
		{
			SaveHighscore(score, 0, "");
	
		}
	
	
		public void SaveHighscore(int score, uint level)
		{
			SaveHighscore(score, level, "");
		}
	
	
		public void SaveHighscore(int score, uint level, string levelName)
		{
			string scoreJson = "{\"score\": " + score.ToString() + ", \"level\": " + level.ToString()  + ", \"levelName\": \"" + levelName + "\" }"; 
			_flashSwitcher.DisplayServicesAndCall (SAVE_HIGHSCORE, scoreJson);
		}
	
		//---------------------------------------
	
		public void GetTopHighscores()
		{
			GetTopHighscores(1, 10, 0);
		}
	
		public void GetTopHighscores(uint period)
		{
			GetTopHighscores(period, 10, 0);
		}
	
	
		public void GetTopHighscores(uint period, uint count)
		{
			GetTopHighscores(period, count, 0);
		}
	
	
		public void GetTopHighscores(uint period, uint count, uint level)
		{
			string topScoresJson = "{\"period\": " + period.ToString() + ", \"count\": " + count.ToString() + ", \"level\": " + level.ToString() + " }"; 
			JSCaller.Call( GET_TOP_HIGHSCORES, topScoresJson );
		}
	
	
		//---------------------------------------
		/*
	
		public void WriteHighscore(int score)
		{
			WriteHighscore(score, 0, "");
		}
	
	
		public void WriteHighscore(int score, uint level)
		{
			WriteHighscore(score, level, "");
		}
	
	
		public void WriteHighscore(int score, uint level, string levelName)
		{
			string scoreJson = "{\"score\": " + score.ToString() + ", \"level\": " + level.ToString()  + ", \"levelName\": \"" + levelName + "\" }"; 
			JSCaller.Call( WRITE_HIGHSCORE, scoreJson );
		}
		
		*/
		//---------------------------------------
		
		public void GetUserDetails()
		{
			JSCaller.Call( GET_USER_DETAILS );
		}
	
		public void GetEmbedVariables()
		{
			JSCaller.Call( GET_EMBED_VARIABLES );
		}
		
		//-----------------------
	
		//Build Highscores
		private Dictionary<string, Highscore> BuildHighscores(string json)
		{
			Dictionary<string, Highscore> highscores = new Dictionary<string, Highscore>();
			Dictionary<string,object> search = Json.Deserialize (json) as Dictionary<string,object>;
			
			
			Highscore highscoreValue;
			//foreach (string kv in search.Keys) 
			foreach (KeyValuePair<string,object> kv in search)
			{
				//Debug.Log("position: " + kv);
				IDictionary<string,object> inner = /*search[kv]*/kv.Value as IDictionary<string,object>;
				highscoreValue = new Highscore(  inner );
				highscores.Add(/*kv*/kv.Key, highscoreValue);
			}
			
			return highscores;
		}
	
		
		private UserDetails BuildUserDetails(string data)
		{
			UserDetails ud = null;
			
			if(data == null || data.Length < 1)
			{
				Debug.Log("-> MiniclipAPI::BuildUserDetails() - no User Details - NOT logged in!");
				return null;
			}
			
			Dictionary<string,object> dict = Json.Deserialize(data) as Dictionary<string,object>;
			
			if(dict == null || dict.Count < 1)
			{
				Debug.Log("-> MiniclipAPI::BuildUserDetails() - No JSON Dictionary! :(");
			}
			else
			{
				ud = new UserDetails( dict );
			}
			
			return ud;
		}
	
		//----------------------------------------------
		// EventHandlers
		//----------------------------------------------
	
		public void OnUpdatePoolRequested(object sender, UpdateEventArgs args)
		{
			AbstractUpdateable updateable = sender as AbstractUpdateable;
		
			if(updateable.currentUpdatePool == args.RequestedUpdatePool)
				return;
			
			// Remove from current pool
			bool removedSuccessfully = true;
			switch(updateable.currentUpdatePool)
			{
				case UpdatePoolId.UPDATE:
					removedSuccessfully = _updatePool.Remove( updateable );
					break;
				case UpdatePoolId.LATE_UPDATE:
					removedSuccessfully = _lateUpdatePool.Remove( updateable );
					break;
				case UpdatePoolId.END_OF_FRAME:
					removedSuccessfully = _endOfFramePool.Remove( updateable );
					break;
			}
		
		
			if(removedSuccessfully)
			{
				updateable.currentUpdatePool = UpdatePoolId.NONE;
			}
			else
			{
				Debug.LogError("-> MiniclipAPI::OnUpdatePoolRequested - removal from current Pool failed!");
				return;
			}
		
			//--------------------------------------------------------------
			//Add to new requested pool
			switch(args.RequestedUpdatePool)
			{
				case UpdatePoolId.UPDATE:
					_updatePool.Add( updateable );
					break;
				case UpdatePoolId.LATE_UPDATE:
					_lateUpdatePool.Add( updateable );
					break;
				case UpdatePoolId.END_OF_FRAME:
					_endOfFramePool.Add( updateable );
					break;
			}
		
			updateable.currentUpdatePool = args.RequestedUpdatePool;
		
			Debug.Log("-> MiniclipAPI::OnUpdatePoolRequested - [" + updateable.ToString() 
			+ "].currentUpdatePool = " + updateable.currentUpdatePool ); 
		}
		
	
		//----------------------------------------------
		// Callback (called from Javascript)
		//----------------------------------------------
	
		public void OnJSNotification(string notification)
		{
		JSCaller.Call ("flashtracer", "OnJSNotification: "+notification);
		//Debug.Log("-> MiniclipAPI::OnJSNotification()");
			//DispatchDebugMessage( "MiniclipAPI::OnJSNotification - " + notification + "\n" );
					
			string noticeID = "";
			string json = "";
			int noticeLength = -1;
	
			noticeLength = notification.IndexOf("\n");
				
			//Debug.Log("-> MiniclipAPI::OnJSNotification()");
		//	DispatchDebugMessage( "MiniclipAPI::OnJSNotification - " + notification + "\n" );
			JSCaller.consoleLog ("************************ OnJSNotification************************* ");
			JSCaller.consoleLog (noticeID);
			JSCaller.consoleLog (json);


			if(noticeLength < 0)
			{
			 	//Debug.Log("-> MiniclipAPI::OnJSNotification() - invalid message - no NoticeID!");
				return;
			}
		
			noticeID = notification.Substring(0,noticeLength);
			json = notification.Substring( (noticeLength + 1) );
			JSCaller.consoleLog (noticeID);
			JSCaller.consoleLog (json);
			//---------------------------------------------------------------------------
						
			// Let the Currencies object deal with any Currencies NoticeIDs
			if(noticeID.IndexOf("currencies") > -1)
			{
				//Application.ExternalEval("console.log('ProcessCurrenciesData before " + Time.realtimeSinceStartup + "')");
				(_currencies as AbstractService).ProcessData(noticeID, json );
				//Application.ExternalEval("console.log('ProcessCurrenciesData after " + Time.realtimeSinceStartup + "')");
				return;
			}
			else if(noticeID.IndexOf("award") > -1)
			{
				(_awards as AbstractService).ProcessData(noticeID, json);
				return;
			}
		
			//---------------------------------------


			//this._coreNoticeHandler.Handle(noticeID, json);
					
			// Deal with core API NoticeIDs
			switch(noticeID)
			{
			case NoticeID.STORAGE_SET:		
			case NoticeID.STORAGE_GET:	
			case NoticeID.STORAGE_DELETE:		
			case NoticeID.STORAGE_ERROR:
			case NoticeID.STORAGE_GHOSTDATA_SET:
			case NoticeID.STORAGE_GHOSTDATA_GET:	
			case NoticeID.STORAGE_GHOSTDATA_LIST:	
			case NoticeID.STORAGE_GHOSTDATA_DELETE:	
				storage.ProcessData(noticeID, json);
					break;
			case NoticeID.LEADERBOARDS_API:	
				leaderboards.ProcessData(json); 
				break;
			case NoticeID.SOCIAL_API:	

				social.ProcessData(json); 
			break;

			case NoticeID.REQUEST_SCREENGRAB:		
					//*** This request should be made by the services_wrapper NOT the game. ***
			
					//Take a screenshot and then send it to the services_wrapper,
					//without calling an API function...
					_flashSwitcher.DisplayServices();
					break;
			
				case NoticeID.USER_DETAILS:
			
					if(UserDetailsReceived != null)
					{
						UserDetailsReceived(this, new UserDetailsEventArgs( BuildUserDetails(json) ) );
					}
			
					break;
			
				case NoticeID.LOGIN:
								
					if(LogInCompleted != null)
					{
						LogInCompleted( this, new UserDetailsEventArgs( BuildUserDetails(json) ) );
					}

					break;
			
				case NoticeID.LOGIN_CANCELLED:
			
					if(LogInCancelled != null)
					{
						LogInCancelled( this, EventArgs.Empty );
					}
			
					break;
			
				case NoticeID.SIGNUP:
						
					if(SignUpCompleted != null)
					{
						SignUpCompleted( this, new UserDetailsEventArgs( BuildUserDetails(json) ) ); 
					}

					break;
			
				case NoticeID.HIGHSCORES_CLOSE:
					
					if(HighscoresClosed != null)
					{
						HighscoresClosed(this, EventArgs.Empty);
					}
			
					break;
			
				case NoticeID.HIGHSCORES:
					
					if(HighscoresReceived != null)
					{
						HighscoresReceived(this, new HighscoresEventArgs( BuildHighscores(json) ) );
					}
					
					break;
			
				case NoticeID.HIGHSCORES_FAILED:
			
					if(HighscoresFailed != null)
					{
						HighscoresFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
			
					break;
			
				case NoticeID.HIGHSCORES_ERROR:
			
					if(HighscoresError != null)
					{
						HighscoresError(this, new MessageEventArgs( MiniclipUtils.ExtractMessage(json) ) );
					}
			
					break;
			
				case NoticeID.EMBED_VARIABLES:
						
					if(EmbedVariablesReceived != null)
					{
						EmbedVariables embedVars = new EmbedVariables( json );
						EmbedVariablesReceived( this, new EmbedVariablesEventArgs( embedVars ) );
					}	

					break;
			
				default:
						
					if(UndefinedEvent != null)
					{
						UndefinedEvent(this, new UndefinedEventArgs(noticeID, json) );
					}
			
					break;
			}
			
		}	
	}
//}
