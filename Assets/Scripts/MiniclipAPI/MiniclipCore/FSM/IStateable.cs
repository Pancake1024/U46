using System;

namespace com.miniclip
{
	public interface IStateable
	{
		//A IStateable object has multiple states which are managed by a Finite State Machine
		StateMachine StateMachine
		{
			get;
		}
	}
}

