using System;
using UnityEngine;

namespace com.miniclip
{
	public class RenderingScreenGrab : FlashSwitcherState
	{
		public RenderingScreenGrab(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> RenderingScreenGrab - created!");;	
		}
		
		public override void Enter()
		{
			Debug.Log("-> RenderingScreenGrab::Enter()");	
			
			//--Send the Screen Grab Data --
			//..to services_wrapper.swf, whereupon it is rendered as the background.
			JSCaller.Call( FlashSwitcher.RENDER_SCREEN_GRAB );
		}
		
		public override void Update()
		{
			_stateMachine.ChangeState( StateId.READY );
		}
		
		public override void Exit()
		{
			Debug.Log("-> RenderingScreenGrab::Exit()");
		}	
	}
}

