using UnityEngine;
using System;
using com.miniclip;
using MiniJSON;

namespace com.miniclip
{
	public class AwardsAPI
	{
		public const string AWARDS_HAS_AWARD			= "awards_HasAward";
		public const string AWARDS_GIVE_AWARD			= "awards_GiveAward";
		
		public void GiveAward(uint awardId)
		{		
			if (awardId < 1)  //awards ID must always be greater than 0
			{
				Debug.Log("awardId can't be smaller than one!");
				return;
			}
			
			string jsonStr = "{ \"award_id\" : " + awardId + " }";
			JSCaller.Call( AWARDS_GIVE_AWARD, jsonStr );  
		}
		
		
		public void HasAward(uint awardId)
		{			
			if (awardId < 1)  //awards ID must always be greater than 0
			{
				Debug.Log("awardId can't be smaller than one!");
				return;
			}
			
			string jsonStr = "{ \"award_id\" : " + awardId + " }";
			JSCaller.Call( AWARDS_HAS_AWARD, jsonStr );  
		}
	}
}