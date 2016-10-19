using System;
using UnityEngine;

namespace com.miniclip
{
	public class WaitState : State
	{
		protected float _waitTime;
		private float _timeRemaining;
		private string _nextStateId;
		
		public WaitState(string id, StateMachine stateMachine, float t, string nextStateId) : base(id, stateMachine)
		{
			this._waitTime = t;
			this._nextStateId = nextStateId;
		}
		
		public override void Enter()
		{
			_timeRemaining = _waitTime;
		}
		
		public override void Update()
		{
			_timeRemaining -= Time.deltaTime;
			
			if( _timeRemaining < 0f)
			{
				_stateMachine.ChangeState( _nextStateId );
			}
		}
	}
}

