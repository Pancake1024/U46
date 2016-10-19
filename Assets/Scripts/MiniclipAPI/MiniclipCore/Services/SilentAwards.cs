using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using com.miniclip;
using MiniJSON;


//Awards Shader: Unlit/Transparent

namespace com.miniclip.awards
{
	public class SilentAwards : AbstractService, IAwardsAPI
	{	     		
		private AwardsAPI _api = new AwardsAPI();
		
		//Events	
		public event EventHandler<AwardDataEventArgs> 	AwardGiven;
		public event EventHandler<FailedEventArgs>		AwardFailed; //TODO: we should also return the awardId along with this.
		public event EventHandler<MessageEventArgs>		AwardError;
		
		public SilentAwards()
		{
		}
					
		//-------------------------------------
		// A AMF Service methods
		//-------------------------------------
		
		public void Init()
		{
			Debug.Log("-> AwardsService::Init() - not implemented in this basic service!");
		}
		
		public void Init(string awardConfigId)
		{
			Debug.Log("-> AwardsService::Init(string awardConfigId) - not implemented in this basic service!");
		}
		
		public void ShowAward(uint awardId)
		{
			Debug.Log("-> AwardsService::ShowAward() - not implemented in this basic service!");
		}
		
		public void GiveAward(uint awardId)
		{		
			_api.GiveAward(awardId);
		}
		
		
		public void HasAward(uint awardId)
		{	
			_api.HasAward(awardId);
		}
		
		
		//-------------------------------------
		// EventHandlers
		//-------------------------------------
		
		internal override void ProcessData( string noticeID, string json )
		{					
			switch(noticeID)
			{
				case NoticeID.AWARD_GIVEN:	
					
					if(AwardGiven != null)
					{
						this.AwardGiven( this, new AwardDataEventArgs( MiniclipUtils.BuildAwardData(json) ) );
					}
				
					break;
				
				case NoticeID.AWARD_FAILED:
				
					if(AwardFailed != null)
					{		
						this.AwardFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
					
					break;
				
				case NoticeID.AWARD_SERVICE_ERROR:
					
					if(AwardError != null)	
					{
						this.AwardError(this, new MessageEventArgs( MiniclipUtils.ExtractMessage(json) ) );
					}
	
					break;
			}
		}
		
		
	}
}