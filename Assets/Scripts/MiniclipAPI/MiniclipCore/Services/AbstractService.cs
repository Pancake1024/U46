using System;

namespace com.miniclip
{
	public abstract class AbstractService
	{		
		//the flashSwitcher is used whenever a function requires us to switch visibility 
		// to the services_wrapper (which is a Flash instance)
		protected FlashSwitcher _flashSwitcher; 
		
		
		//internal AbstractNoticeHandler noticeHandler;
		internal abstract void ProcessData( string noticeID, string json );
		
		internal void SetFlashSwitcher(FlashSwitcher flashSwitcher)
		{
			this._flashSwitcher = flashSwitcher;
		}
		
	}
}

