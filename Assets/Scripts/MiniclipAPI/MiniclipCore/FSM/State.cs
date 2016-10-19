using System;
using UnityEngine;

namespace com.miniclip
{
	public class State : IState
	{		
		protected string _id = "";
		protected StateMachine _stateMachine;
		
		public string Id 
		{
			get { return _id; }
		}
		
		public State(string id, StateMachine stateMachine)
		{
			this._id = id;
			this._stateMachine = stateMachine;
		}
		
		public virtual void Enter() {}
		public virtual void Update() {}
		public virtual void Exit() {}
	}
}

