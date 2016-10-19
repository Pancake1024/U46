//Finite State Machine
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace com.miniclip
{
	public class StateMachine
	{
		protected IState _currentState;
		protected string _prevStateId;
		
		protected Dictionary<string, IState> _states;
	
		
		public IState CurrentState 
		{
			get{ return _currentState; }
		}
	
		//-------------------------------------------
		// Methods
		//-------------------------------------------
		
		public StateMachine ()
		{
			_currentState = null;
			_states = new Dictionary<string, IState>();				
		}
				
		public virtual void Update()
		{
			//Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");	
			if(_currentState != null)
				_currentState.Update();
		}
		
		//public virtual void createStates() {}
			
		public void AddState( IState state )
		{
			Debug.Log("-> StateMachine::AddState() - state ID: " + state.Id);
			this._states.Add( state.Id, state );
		}
		
		public void SetStartState( string stateId )
		{
			if(_states.ContainsKey(stateId))
			{
				this._currentState = _states[stateId]; 
			}
		}
		
		//-------------------------------------------
		
		public bool ChangeState( string stateId )
		{			
			if(stateId == _currentState.Id)
				return true;
			
			return ChangeState( _states[stateId] );
		}
		
		
		public bool ChangeState( IState state )
		{
			if(state != null)
			{
				//if(_currentState != null)
				_currentState.Exit();
				
				_prevStateId = _currentState.Id;
				_currentState = state;
				_currentState.Enter();
				
				return true;
			}
			else
			{
				return false;
			}
		}
		
		//TODO: GetStateById
					
		public void RevertToPreviousState()
		{
			ChangeState( _prevStateId );
		}
			
	}
}

