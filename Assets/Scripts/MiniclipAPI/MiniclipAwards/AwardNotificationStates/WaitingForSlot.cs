using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class WaitingForSlot : AwardNotificationState
	{
		public WaitingForSlot(string id, AwardNotification award) : base(id, award)
		{
		//	Debug.Log("-> WaitingForSlot - created!");
		}
		
		public override void Enter()
		{
		//	Debug.Log("-> WaitingForSlot::Enter()");
			
			_award.RequestAwardSlot();
		}
		
		public override void Update()
		{
			//_award.Request
			
			//_stateMachine.ChangeState( AwardStateId.WAITING_FOR_SLOT );
		}
		
	
		
		public override void Exit()
		{
		//	Debug.Log("-> WaitingForSlot::Exit()");
		}	
	}
}

