using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class Displaying : AwardNotificationState
	{
		private float _elapsedTime = 0f;
		private float _waitTime;
	
		public Displaying(string id, AwardNotification award) : base(id, award)
		{
			//Debug.Log("-> Displaying - created!");
			_waitTime = AwardNotification.WaitTime;
		}
		
		public override void Enter()
		{
			//Debug.Log("-> Displaying::Enter()");
			_elapsedTime = 0f;
			_award.transform.position = new Vector3( _award.transform.position.x, _award.transform.position.y, MiniclipAwards.ZDepth );
		}
		
		public override void Update()
		{
			//Debug.Log("-> Showing::Update() - elapsed Time: " + _elapsedTime);
			 _elapsedTime += Time.deltaTime;
			
			if(_elapsedTime > _waitTime)
			{
				_stateMachine.ChangeState( AwardStateId.MOVING_OUT );
			}
		}
		
		public override void Exit()
		{
			//Debug.Log("-> Displaying::Exit()");
			_elapsedTime = 0f;
		}	
	}
}
