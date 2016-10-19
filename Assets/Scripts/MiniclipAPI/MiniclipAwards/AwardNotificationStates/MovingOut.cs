using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class MovingOut : AwardNotificationState
	{		
	//	private float _yPos;
		
		public MovingOut(string id, AwardNotification award) : base(id, award)
		{
		//	Debug.Log("-> MovingOut - created!");
			_speed = AwardNotification.Speed;
		}
		
		public override void Enter()
		{
		//	Debug.Log("-> MovingOut::Enter()");
			_award.VacantedSlot();
			_award.transform.position = new Vector3( _award.transform.position.x, _award.transform.position.y, (MiniclipAwards.ZDepth - 0.0002f) );
			
			_destinationX = _award.StartPos.x;
			_destinationY = _award.StartPos.y;
			
			CalculateDistance();
			CalculateDirection();
		}
		
		public override void Update()
		{
			CalculateDistance();
			CalculateDirection();

			if( _directionChanged )
			{
				_award.transform.position = new Vector3( _destinationX, _destinationY, (MiniclipAwards.ZDepth - 0.0002f) );
				_stateMachine.ChangeState( AwardStateId.AVAILABLE );
				return;
			}
		
			_award.transform.Translate( (_directionX * _speed * Time.deltaTime) , (_directionY * _speed * Time.deltaTime), 0f);
		}
		
		public override void Exit()
		{
		//	Debug.Log("-> MovingOut::Exit()");
			_award.transform.position = new Vector3(  _award.StartPos.x, _award.StartPos.y, (MiniclipAwards.ZDepth - 0.0001f) );
		}	
	}
}

