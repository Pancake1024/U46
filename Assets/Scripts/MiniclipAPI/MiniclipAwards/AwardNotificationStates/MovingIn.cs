using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class MovingIn : AwardNotificationState
	{
		public MovingIn(string id, AwardNotification award) : base(id, award)
		{
		//	Debug.Log("-> MovingIn - created!");
			_speed = AwardNotification.Speed;
		}
		
		public override void Enter()
		{
		//	Debug.Log("-> MovingIn::Enter()");
			_destinationX = _award.AllocatedSlot.Position.x;
			_destinationY = _award.AllocatedSlot.Position.y;
			_award.transform.position = new Vector3( _award.transform.position.x, _award.transform.position.y, (MiniclipAwards.ZDepth - 0.0001f) );
			
			CalculateDistance();
			CalculateDirection();
		}
		
		public override void Update()
		{
			CalculateDistance();
			CalculateDirection();

			if( _directionChanged )
			{
				_award.transform.position = new Vector3( _destinationX, _destinationY, (MiniclipAwards.ZDepth - 0.0001f) );
				_stateMachine.ChangeState( AwardStateId.DISPLAYING );
				return;
			}
		
			_award.transform.Translate( (_directionX * _speed * Time.deltaTime) , (_directionY * _speed * Time.deltaTime), 0f );						
		}
		
		public override void Exit()
		{
		//	Debug.Log("-> MovingIn::Exit()");
		
		}	
	}
}

