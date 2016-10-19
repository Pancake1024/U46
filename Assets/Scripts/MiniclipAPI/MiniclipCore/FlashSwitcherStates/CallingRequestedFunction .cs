using System;
using UnityEngine;

namespace com.miniclip
{
	public class CallingRequestedFunction  : FlashSwitcherState
	{
		public CallingRequestedFunction  (string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> CallingRequestedFunction - created!");
		}
		
		public override void Enter()
		{
			Debug.Log("-> CallingRequestedFunction::Enter()");	
			
			//--Send the Screen Grab Data --
			//..to services_wrapper.swf, whereupon it is rendered as the background.
			_flashSwitcher.CallRequestedFunction();
		}
		
		public override void Update()
		{
			_stateMachine.ChangeState( StateId.CLEARING_BMP_DATA );
		}
		
		public override void Exit()
		{
			Debug.Log("-> CallingRequestedFunction::Exit()");
		}	
	}
}

