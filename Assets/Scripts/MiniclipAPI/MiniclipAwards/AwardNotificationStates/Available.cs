using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class Available : AwardNotificationState
	{
		public Available(string id, AwardNotification award) : base(id, award)
		{
		//	Debug.Log("-> Available - created!");
		}
		
		public override void Enter()
		{
		//	Debug.Log("-> Available::Enter()");
		}
		
		public override void Update()
		{

		}
		
		public override void Exit()
		{
		//	Debug.Log("-> Available::Exit()");
		}	
	}
}

