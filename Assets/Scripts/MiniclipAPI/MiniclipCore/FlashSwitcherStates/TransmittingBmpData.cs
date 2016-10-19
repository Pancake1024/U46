using System;
using UnityEngine;

namespace com.miniclip
{
	public class TransmittingBmpData : FlashSwitcherState
	{
		/*
		 * TODO: We may detect the client's OS and pass it here as a URL var, whereupon
		 * if we are on Windows we can send all the bmp data in one frame (splitting it 
		 * into separate External Calls of 20000 chars per chunk).
		 */
		
		private const int MAX_CHUNK_SIZE = 16000; //A higher chunk size causes problems on Mac OS

		private int _strPos 			= 0; //string position in Bmp Data String (base64)
		private string _bitmapString	= "";
		
		
		public TransmittingBmpData(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> TransmittingBmpData - created!");
		}
		
		public override void Enter()
		{
			Debug.Log("-> TransmittingBmpData::Enter()");
			_strPos = 0;
			_bitmapString = _flashSwitcher.bitmapString;
		}
		
		public override void Update()
		{
			if( TransmitScreenGrabChunk() )
			{
				//_state = (int) State.RenderScreenGrab;
				_stateMachine.ChangeState( StateId.RENDERING_SCREEN_GRAB );
			}
		}
		
		public override void Exit()
		{
			Debug.Log("-> TransmittingBmpData::Exit()");
		}
		
		//----
		
		private bool TransmitScreenGrabChunk()
		{
			string bitmapChunk;
		
			if(_strPos < _bitmapString.Length)
			{
				int bytesLeft = _bitmapString.Length - _strPos;
				if (bytesLeft > MAX_CHUNK_SIZE)
					bytesLeft = MAX_CHUNK_SIZE;
				
				bitmapChunk = _bitmapString.Substring(_strPos, bytesLeft);
					
				//this.DebugMessage("Transmitting ScreenGrab Data 'Chunk' at pos: " + _strPos + "\n"); 
					
				JSCaller.Call( FlashSwitcher.RECEIVE_SCREEN_GRAB_DATA, bitmapChunk );
				
			 	_strPos+= MAX_CHUNK_SIZE;
				return false;
			}
			else
			{
				return true;
			}
		}
		
	}
}

