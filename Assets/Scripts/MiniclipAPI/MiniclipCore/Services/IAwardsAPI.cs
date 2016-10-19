using System;

namespace com.miniclip
{
	public interface IAwardsAPI
	{		
		void Init();
		void Init( string awardConfigId );
		void ShowAward( uint awardId );
		void GiveAward( uint awardId );
		void HasAward( uint awardId );		
	}
}

