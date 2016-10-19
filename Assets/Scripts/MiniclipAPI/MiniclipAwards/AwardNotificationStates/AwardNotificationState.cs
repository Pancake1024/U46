using System;
using UnityEngine;

namespace com.miniclip.awards
{
	public class AwardNotificationState : State
	{
		protected AwardNotification _award;
		
		protected float _speed = 0f;

		protected float _destinationX = 0f;
		protected float _destinationY = 0f;
		
		protected float _distX;
		protected float _distY;
		
		protected float _directionX = 0f;
		protected float _directionY = 0f;
		
		protected float _distance = 0f;
		
		protected float _lastDirectionX = 0f;
		protected float _lastDirectionY = 0f;
		
		protected bool _directionChanged = false;
		
	
	
		public AwardNotificationState(string id, AwardNotification award) : base(id, award.StateMachine)
		{
			this._award = award;
		}	
		
		
		public void CalculateDistance()
		{
			_distX = _destinationX - _award.transform.position.x;
			_distY = _destinationY - _award.transform.position.y; 
			
			float adjacent = Math.Abs( _distX );
			float opposite = Math.Abs( _distY );
			 
			_distance = (float) Math.Sqrt( (adjacent*adjacent) + (opposite*opposite) );			
		}
		
		
		public void CalculateDirection()
		{ // (calculate normalized vector)
			
			_directionX = _distX / _distance;
			_directionY = _distY / _distance;	
			
			
			_directionChanged = false;
			
			if(_directionX <= 0 && _lastDirectionX > 0)
			{
				_directionChanged = true;
			}
					
			if(_directionY <= 0 && _lastDirectionY > 0)
			{
				_directionChanged = true;
			}
			
		
			_lastDirectionX = 	_directionX;
			_lastDirectionY = 	_directionY;
		}
		
	}
}

