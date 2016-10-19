using System;
using UnityEngine;

namespace com.miniclip
{
	public class ClearingBmpData : FlashSwitcherState 
	{
		public ClearingBmpData(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> ClearBmpDataState - created!");	
		}
		
		public override void Enter()
		{
			Debug.Log("-> ClearBmpDataState::Enter()");
			JSCaller.Call( FlashSwitcher.CLEAR_SCREEN_GRAB_DATA );	
		}
		
		public override void Update()
		{
			_stateMachine.ChangeState( StateId.TRANSMITTING_BMP_DATA );
			//_stateMachine.SetNextState( StateId.TRANSMIT_BMP_DATA );
		}
		
		public override void Exit()
		{
			Debug.Log("-> ClearBmpDataState::Exit()");
		}	
	}
}

