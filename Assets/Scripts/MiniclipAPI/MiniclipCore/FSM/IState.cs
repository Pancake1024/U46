using System;

namespace com.miniclip
{
	public interface IState
	{
		string Id 
		{
			get;
		}
				
		void Enter();
		void Update();
		void Exit();
	}
}

