using System;
using UnityEngine;

namespace com.miniclip
{
	public class Ready : FlashSwitcherState
	{
		public Ready(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher)
		{
			Debug.Log("-> ReadyState - created!");
		}
	
		public override void Enter()
		{
			Debug.Log("-> ReadyState::Enter()"); 
			
			//The flashswitcher needs to be removed from the update pool.
			//i.e. Update() will no won't be called on it unless it is needed again.
			_flashSwitcher.ChangeUpdatePool( UpdatePoolId.NONE );
		}
		
		public override void Exit()
		{
			Debug.Log("-> ReadyState::Exit()"); 
		}	
	}
}

