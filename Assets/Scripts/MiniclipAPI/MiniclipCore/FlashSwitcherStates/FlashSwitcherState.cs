using System;
using UnityEngine;

namespace com.miniclip
{
	public class FlashSwitcherState : State
	{
		protected FlashSwitcher _flashSwitcher;
		
		public FlashSwitcherState(string id, FlashSwitcher flashSwitcher) : base(id, flashSwitcher.StateMachine)
		{
			this._flashSwitcher = flashSwitcher;
		}	
	}
}

