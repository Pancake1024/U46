using System;

namespace com.miniclip
{
	public interface IAwardsService
	{
		/*
		event EventHandler<AwardDataEventArgs> 	AwardGiven;
		event EventHandler<MessageEventArgs>	AwardFailed;
		event EventHandler<MessageEventArgs>	AwardError;
		*/
		
		void ShowAward(uint awardId);
		void GiveAward(uint awardId);
		void HasAward(uint awardId);
	}
}

