using System;
using System.Collections.Generic;
using MiniJSON;

namespace com.miniclip.awards
{
	public class AwardNotificationEventArgs : EventArgs
	{
		private NotificationData _awardNotification;
		
		public NotificationData AwardNotification
		{
			get{ return _awardNotification; }
		}
		
		public AwardNotificationEventArgs( NotificationData awardNotification )
		{
			this._awardNotification = awardNotification;
		}
	}
}

