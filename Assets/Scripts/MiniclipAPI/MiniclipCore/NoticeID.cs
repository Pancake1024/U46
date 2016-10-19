namespace com.miniclip
{
	public class NoticeID
	{
		public const string REQUEST_SCREENGRAB  = "request_screengrab";
		
		//Core
		public const string EMBED_VARIABLES		= "parameters";
		public const string HIGHSCORES_CLOSE 	= "highscores_close";
		public const string HIGHSCORES			= "highscores";
		public const string HIGHSCORES_FAILED	= "highscores_failed";
		public const string HIGHSCORES_ERROR	= "highscores_error";
		
		//Core - Authentication
		public const string USER_DETAILS 		= "auth_user_details";
		public const string LOGIN 				= "auth_login";
		public const string LOGIN_CANCELLED 	= "auth_cancelled";
		public const string SIGNUP 				= "auth_signup";
		
		//Currencies
		public const string CURRENCIES_READY					  = "currencies_ready";
		public const string CURRENCIES_BALANCE					  = "currencies_balance";
		public const string CURRENCIES_BALANCES					  = "currencies_balances";
		public const string CURRENCIES_PURCHASED				  = "currencies_purchased";
		public const string CURRENCIES_BUNDLE_PURCHASE_FAILED	  = "currencies_bundle_purchase_failed";
		public const string CURRENCIES_BUNDLE_PURCHASED			  = "currencies_bundle_purchased";
		public const string CURRENCIES_PURCHASE_FAILED		 	  = "currencies_purchase_failed";
		public const string CURRENCIES_ITEM_QTY_DECREMENTED		  = "currencies_itemqnt_decremented";
		public const string CURRENCIES_PURCHASE_CANCELLED_BY_USER = "currencies_purchase_cancelled";
		public const string CURRENCIES_PURCHASE_ACCEPTED_BY_USER  = "currencies_purchase_accepted";
		
		public const string CURRENCIES_CONVERT_CANCELLED_BY_USER  = "currencies_convert_cancelled";
		public const string CURRENCIES_CONVERT_ACCEPTED_BY_USER   = "currencies_convert_accepted";
		public const string CURRENCIES_CURRENCY_CONVERTED		  = "currencies_currency_converted";
		public const string CURRENCIES_CONVERSION_FAILED		  = "currencies_conversion_failed";
		
		public const string CURRENCIES_GET_ITEM_BY_ID 			  = "currencies_item_byid";
		public const string CURRENCIES_GET_ITEMS_BY_GAME_ID		  = "currencies_items_gameid";
		public const string CURRENCIES_GET_BUNDLES				  = "currencies_bundles";
		public const string CURRENCIES_ITEM_INFO				  = "currencie_item_info";
		public const string CURRENCIES_GET_AVAILABLE_CURRENCIES	  = "currencies_available_currencies";
		public const string CURRENCIES_GET_CURRENCY_BY_ID		  = "currencies_currency_byid";
		public const string CURRENCIES_ADJUST_CURRENCY_BALANCE	  = "currencies_adjust_Currency_Balance";
		public const string CURRENCIES_DECREMENT_ITEM_BALANCE	  = "currencies_decrement_Item_Balance";
		public const string CURRENCIES_USER_ITEM_QUANTITY		  = "currencies_user_item_quantity";
		public const string CURRENCIES_USER_ITEMS_BY_GAME_ID	  = "currencies_user_items_by_gameid";
		public const string CURRENCIES_ERROR					  = "currencies_error"; 	// Timeout or Misc Error
		public const string CURRENCIES_FAILED					  = "currencies_failed";	// Timeout or Misc Error
		public const string CURRENCIES_GIVE_ITEM 		 		  = "currencies_give_item";
		public const string CURRENCIES_TOPUP_CLOSED				  = "currencies_topup_closed";
		public const string CURRENCIES_OFFER_AVAILABILITY 		  = "currencies_offer_availability";		
		public const string CURRENCIES_OFFER_COMPLETE 			  = "currencies_offer_complete";	
		
		//Awards
		public const string AWARD_INITIALIZED					  = "award_initialized";
		public const string AWARD_GIVEN							  = "award_given";  	
		public const string AWARD_FAILED						  = "award_failed";
		public const string AWARD_SERVICE_ERROR					  = "award_service_error";

		public const string STORAGE_GET							  = "storage_get";
		public const string STORAGE_SET							  = "storage_set";
		public const string STORAGE_ERROR						  = "storage_error";
		public const string STORAGE_DELETE						  = "storage_delete";

		public const string STORAGE_GHOSTDATA_SET				  = "setGhostData";
		public const string STORAGE_GHOSTDATA_GET			      = "getGhostData";
		public const string STORAGE_GHOSTDATA_LIST				  = "listGhostData";
		public const string STORAGE_GHOSTDATA_DELETE			  = "deleteGhostData";
		public const string SOCIAL_API							  = "socialAPI";
		public const string LEADERBOARDS_API				      = "leaderboardsAPI";

	}
}
