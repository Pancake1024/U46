using UnityEngine;
using System.Collections;
using System;

namespace com.miniclip
{
	public class FlashSwitcher : AbstractUpdateable, IStateable
	{
		internal override event EventHandler<UpdateEventArgs> updatePoolRequested;
		
		//External Function Name Constants
		public const string CLEAR_SCREEN_GRAB_DATA		= "clearScreenGrabData";
		public const string RENDER_SCREEN_GRAB			= "renderScreenGrab";
		public const string RECEIVE_SCREEN_GRAB_DATA	= "receiveScreenGrabData";
		
		enum ClientOS { Windows, MacOSX };
		enum State { Ready, TakeScreenGrab, ClearBmpData, TransmitBmpData, RenderScreenGrab, CallJSFunction };
		
		
		//public event EventHandler<MessageEventArgs> DebugMessage;	
		
		//private const int MAX_CHUNK_SIZE = 16000;//20000;
	
		//private byte[] _bitmapBytes;
		//private Texture2D _texture;
		
		//private int 		_clientOS; //TODO: Once we have a _clientOS flashvar
		//private int 		_state;	
		private CallArgs	_callArgs;
		
		public string bitmapString = "";
		
		
		private StateMachine _stateMachine;
		public StateMachine StateMachine
		{
			get{ return _stateMachine; }
		}
		
		//-------------------------------------------
		// Methods
		//-------------------------------------------
		
		public FlashSwitcher()
		{
			
			//this._clientOS  = (int) ClientOS.Windows; //TODO: Once we have a _clientOS flashvar
			//this._state 	= (int) State.Ready;
			_stateMachine = new StateMachine();
			
			_stateMachine.AddState(new Ready( StateId.READY, this ));
			_stateMachine.AddState(new TakingScreenGrab( StateId.TAKING_SCREEN_GRAB, this ));
			_stateMachine.AddState(new CallingRequestedFunction( StateId.CALLING_REQUESTED_FUNCTION, this ));
			_stateMachine.AddState(new ClearingBmpData( StateId.CLEARING_BMP_DATA, this ));
			_stateMachine.AddState(new TransmittingBmpData( StateId.TRANSMITTING_BMP_DATA, this ));
			_stateMachine.AddState(new RenderingScreenGrab( StateId.RENDERING_SCREEN_GRAB, this ));

			_stateMachine.SetStartState(StateId.READY);
			
			//_currentState = _states[StateId.READY];
		}
		
		public void DisplayServices()
		{
			//Just Display Services (i.e. Take and send a screengrab)
			//Without calling an API function
			_callArgs = null;
			
			//Start updating this FlashSwitcher at the end of every frame
			ChangeUpdatePool( UpdatePoolId.END_OF_FRAME );
			
			_stateMachine.ChangeState( StateId.TAKING_SCREEN_GRAB );
		}
			
		
		public void DisplayServicesAndCall( string functionName )
		{
			DisplayServicesAndCall( functionName, null );
		}
			
		public void DisplayServicesAndCall( string functionName, string data )
		{
			_callArgs = new CallArgs( functionName, data );
			
			//Start updating this FlashSwitcher at the end of every frame
			ChangeUpdatePool( UpdatePoolId.END_OF_FRAME );
			
			_stateMachine.ChangeState( StateId.TAKING_SCREEN_GRAB );
			
			//this._state = (int) State.TakeScreenGrab; //Start the Screen Grab & Send Process...
		}
		
		internal void ChangeUpdatePool(int updatePoolId)
		{
			this.updatePoolRequested( this, new UpdateEventArgs(updatePoolId) );
		}
		
		internal override void Update ()
		{
			this._stateMachine.Update();
		}
		
		//--------------------------------------------------------------------
		
		internal void CallRequestedFunction()
		{
			if(_callArgs != null)
			{
				JSCaller.Call( _callArgs );
				_callArgs = null;
			}
		}	
	}
}
