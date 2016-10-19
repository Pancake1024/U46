using UnityEngine;
using System.Collections;

public static class OmniataIds
{
	public const string Evt_Purchase = "om_revenue";
	public const string Evt_Hacker_Purchase = "mc_wrong_purchase";	//"om_hacker_revenue";
    public const string Evt_User = "om_user";
    public const string Evt_Info = "mc_info";
    public const string Evt_SessionEnd = "mc_session_finished";
    public const string Evt_VirtualPurchase = "mc_virtual_purchase";
    public const string Evt_Run = "mc_run";
    public const string Evt_Tutorial = "mc_tutorial";
    public const string Evt_LevelUp = "mc_level";
    public const string Evt_VideoAds = "mc_watch_video_ads";
    public const string Evt_CarUpgrade = "mc_car_upgrade";

	public const string Evt_FacebookShare = "mc_fb_share";
	public const string Evt_FacebookInvite = "mc_fb_invite";
	public const string Evt_FuelSent = "mc_fuel_sent";
	public const string Evt_FuelReceived = "mc_fuel_received";

    //public const string Evt_MissionCompleted = "mc_mission";
    public const string Evt_EndRun_Mission = "mc_endrun_mission";
    public const string Evt_EndRun_Mission_Skip = "mc_endrun_mission_skip";

    public const string Evt_DailyBonus = "mc_dailybonus";
    public const string Evt_SpinGame = "mc_spingame";
    public const string Evt_SaveMe = "mc_save_me";

    public const string Param_iOS_Idfa = "om_ios_idfa";
    public const string Param_TotalPlayTime = "total_play_time";

    public const string Param_MissionCompleted = "completed";
    public const string Param_MissionLast = "total_runs";
    public const string Param_MissionHelperPopup = "helper_popup";
    public const string Param_MissionSkipBtnClicked = "btn_clicked";

    public const string Param_Coins = "coins";
    public const string Param_Gems = "gems";
    public const string Param_PromotionId = "promotion_id";
    public const string Param_ProductId = "product_id";
    public const string Param_Type = "type";
    public const string Param_UpgradeType = "upgrade_id";
    public const string Param_UpgradeLevel = "upgrade_level";
    public const string Param_CurrencyCode = "currency_code";
    public const string Param_Tier = "tier";
    public const string Param_CardId = "car_id";

    public const string Param_Dob = "dob";
    public const string Param_FacebookId = "facebook_id";
    public const string Param_FacebookFriendsNumber = "friends";
    public const string Param_Gender = "gender";
    public const string Param_LoginType = "login_type";

	public const string Param_Jailbroken = "user_state";
    public const string Param_GameVersion = "game_version";
    public const string Param_PlayerLevel = "player_level";
    public const string Param_UserIsHacker = "user_var";	//"user_is_hacker";
    public const string Param_SpinPrize = "spin_prize_id";

    public const string Param_SessionLength = "session_length";
    public const string Param_MissionId = "mission_id";
    public const string Param_MissionThreshold = "mission_value";
    public const string Param_MissionPlayerReach = "mission_result";
    public const string Param_MissionType = "mission_type";
    public const string Param_Day = "day";
    public const string Param_DailyCompletedCount = "daily_completed_count";
    public const string Param_LastMenu = "last_menu";
    public const string Param_VideoAdsPlacement = "placement";
    public const string Param_Streak = "streak";
    public const string Param_StreakDays = "streak_days";

    public const string Param_SaveMeCounter = "counter";
    public const string Param_SaveMeClicked = "clicked";
    public const string Param_SaveMeType = "type";
    public const string Param_SaveMeTimePassed = "time_passed";
    public const string Param_SaveMeTimeRemaining = "time_remaining";

    public const string Param_SoftCurrency = "soft_currency";
    public const string Param_Total = "total";

    public const string Param_CoinsWon = "coins_won";
    public const string Param_Distance = "distance";
    public const string Param_Vehicle = "vehicle_used";
    public const string Param_RunTime = "run_time";

    public const string Param_SlipstreamCount = "slipstream_count";
    public const string Param_MaxCombo = "max_combo";
    public const string Param_HitsCount = "hits";

    public const string Param_TutorialStep = "step";

	public const string Param_FacebookShareType = "share_type";

    public const string Currency_Coins = "coins";
    public const string Currency_Diamonds = "gems";


    public const string Product_ExtraSpin = "extra_spin";
    
    //public const string Product_Fuel = "fuel";
    public static string[] Product_Fuel = { "fuel_5", "fuel_4", "fuel_3", "fuel_2", "fuel_1", "fuel_0" };

    public const string Product_FuelFreeze = "fuel_freeze";
    //public const string Product_ParkingLot = "parking_lot";
	public static string[] Product_ParkingLot = { "parking_lot_europe", "parking_lot_oriental", "parking_lot_american", "parking_lot_muscle" };

    public const string Product_Type_Standard = "standard";
    public const string Product_Type_Upgrade = "upgrade";
    public const string Product_Type_Unlock = "unlock";

    public const string Product_Special = "special";
    public const string Product_SpecialDuration = "duration";
    public const string Product_SaveMe = "save_me";
    public const string Product_Booster = "booster";
    public const string Product_RestoreBonusStreak = "restore_bonus_streak";
    public const string Product_Mission = "mission";

    public const string TutorialStep_AvoidTraffic = "1_avoid_traffic";
    public const string TutorialStep_Slipstream = "2_slipstream";
    public const string TutorialStep_Bolts = "3_bolts";
    public const string TutorialStep_Trucks = "4_trucks";
	
	public const string FacebookShare_Car = "new_car";
	public const string FacebookShare_ParkingLot = "new_parking_lot";
	public const string FacebookShare_SpecialVehicle = "new_special_vehicle";
	public const string FacebookShare_LevelUp = "level_up";
	public const string FacebookShare_DistanceReached = "distance_reached";
	public const string FacebookShare_SpinPrize = "wheel_prize";
}