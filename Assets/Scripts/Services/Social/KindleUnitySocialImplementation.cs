using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.SocialPlatforms.GameCenter;

#if UNITY_ANDROID && UNITY_KINDLE

public class KindleUnitySocialImplementation : MonoBehaviour, SocialImplementation
{
    string BestMetersLeaderboardId { get { return "leaderboard_meters"; } }                 
    string TotalMetersLeaderboardId { get { return "leaderboard_total_meters"; } }

    string SpecialVehicleId         { get { return "achiev_buy_special_vehicle"; } }        // A new toy
    string MaxOutSpecialVehicleId   { get { return "achiev_max_out_special_vehicle"; } }    // Full power!
    string BuyCarId                 { get { return "achiev_buy_car"; } }                    // Fresh wheels
    string MaxOutCarId              { get { return "achiev_max_out_car"; } }                // Beauty is on the inside
    string AllScenarioCarsId        { get { return "achiev_all_cars_stage"; } }             // I need a bigger garage
    string NewScenarioUnlockedId    { get { return "achiev_new_stage_unlocked"; } }         // Road tripping
    string SpinDiamondId            { get { return "achiev_win_diamonds_spin"; } }          // Diamonds are forever
    string MissionEasyId            { get { return "achiev_complete_missions_medium"; } }   // What's on the list?
    string MissionHardId            { get { return "achiev_complete_missions_hard"; } }     // Master
    string LevelVeteranId           { get { return "achiev_level_veteran"; } }              // Veteran
    string FBLoginId                { get { return "achiev_fb_login"; } }                   // Yes, that's me!
    string ComeBackEasyId           { get { return "achiev_come_back"; } }                  // I'll be back
    string ComeBackMediumId         { get { return "achiev_final_daily_bonus_prize"; } }    // Finally mine!
    string CheckpointEasyId         { get { return "achiev_cross_checkpoints_easy"; } }     // Checkpoint!
    string CheckpointHardId         { get { return "achiev_cross_checkpoints_hard"; } }     // Stop the clock
    string DestroyVehicleEasyId     { get { return "achiev_destroy_vehicles_easy"; } }      // Get out the way!
    string DestroyVehicleMediumId   { get { return "achiev_destroy_vehicles_medium"; } }    // No more rush hour!
    string UseBoostEasyId           { get { return "achiev_use_boosts_easy"; } }            // A little help here!
    string UseBoostHardId           { get { return "achiev_use_boosts_hard"; } }            // Boostin' up
    string MetersMediumId           { get { return "achiev_meters_medium"; } }              // Seasoned traveler
    string MetersHardId             { get { return "achiev_meters_hard"; } }                // Easy rider
    string CoinsMediumId            { get { return "achiev_earn_coins_medium"; } }          // Big money
    string CoinsHardId              { get { return "achiev_earn_coins_hard"; } }            // Time to spend

    const bool LOGS_ARE_ENABLED = false;

    bool shouldDeferInit = false;

    public bool IsLoggedIn { get { return Social.Active.localUser.authenticated; } }

    public void Init()
    {
        Log("Init()");
        // Add the prefab to the preloader scene !!!
        /*GameObject gameCircleManagerGo = new GameObject();
        DontDestroyOnLoad(gameCircleManagerGo);
        gameCircleManagerGo.AddComponent<GameCircleManager>();*/

        Social.Active = GameCircleSocial.Instance;
        shouldDeferInit = true;
    }

    public void LogIn()
    {
        float delay = 0.0f;
        if (shouldDeferInit)
        {
            delay = 1.0f;
            shouldDeferInit = false;
        }

        StartCoroutine(DeferredInit(delay));
    }

    IEnumerator DeferredInit(float delay)
    {
        yield return new WaitForSeconds(delay);

        Log("LogIn() - delay: " + delay);
        Social.localUser.Authenticate(OnAuthenticationCallback);
    }

    void OnAuthenticationCallback(bool success)
    {
        Log("OnAuthenticationCallback(" + success + ")");

        if (success)
        {
            if (OnTheRunAchievements.Instance != null)
                OnTheRunAchievements.Instance.ResendProgressNotAcknowledged();
        }
        else
        {
            Log("OnAuthenticationCallback(" + success + ") - calling AGSClient.Shutdown()");
            AGSClient.Shutdown();
            Log("OnAuthenticationCallback(" + success + ") - Creating new JavaWrapper Object");
            AGSClient.CreateNewJavaWrapperObject();
        }
    }

    public void OnApplicationResumed()
    {
        Log("OnApplicationResumed() - IsLoggedIn: " + IsLoggedIn);
        if (!IsLoggedIn)
            LogIn();
    }

    public string GetAchievementID(OnTheRunAchievements.AchievementType type)
    {
        string retValue = "";

        switch (type)
        {
            case OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE: retValue = SpecialVehicleId; break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_SPECIAL_VEHICLE: retValue = MaxOutSpecialVehicleId; break;
            case OnTheRunAchievements.AchievementType.BUY_CAR: retValue = BuyCarId; break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_CAR: retValue = MaxOutCarId; break;
            case OnTheRunAchievements.AchievementType.ALL_SCENARIO_CARS: retValue = AllScenarioCarsId; break;
            case OnTheRunAchievements.AchievementType.NEW_SCENARIO_UNLOCKED: retValue = NewScenarioUnlockedId; break;
            case OnTheRunAchievements.AchievementType.SPIN_DIAMOND: retValue = SpinDiamondId; break;
            case OnTheRunAchievements.AchievementType.MISSION_MEDIUM: retValue = MissionEasyId; break;
            case OnTheRunAchievements.AchievementType.MISSION_HARD: retValue = MissionHardId; break;
            case OnTheRunAchievements.AchievementType.LEVEL_VETERAN: retValue = LevelVeteranId; break;
            case OnTheRunAchievements.AchievementType.FB_LOGIN: retValue = FBLoginId; break;
            case OnTheRunAchievements.AchievementType.COME_BACK_MEDIUM: retValue = ComeBackEasyId; break;
            case OnTheRunAchievements.AchievementType.COME_BACK_HARD: retValue = ComeBackMediumId; break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_EASY: retValue = CheckpointEasyId; break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_HARD: retValue = CheckpointHardId; break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY: retValue = DestroyVehicleEasyId; break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM: retValue = DestroyVehicleMediumId; break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_EASY: retValue = UseBoostEasyId; break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_HARD: retValue = UseBoostHardId; break;
            case OnTheRunAchievements.AchievementType.METERS_MEDIUM: retValue = MetersMediumId; break;
            case OnTheRunAchievements.AchievementType.METERS_HARD: retValue = MetersHardId; break;
            case OnTheRunAchievements.AchievementType.COINS_MEDIUM: retValue = CoinsMediumId; break;
            case OnTheRunAchievements.AchievementType.COINS_HARD: retValue = CoinsHardId; break;
        }

        return retValue;
    }

    public void ReportLeaderboardTotalMeters(int meters)
    {
        /*
        Social.ReportScore(meters, TotalMetersLeaderboardId, success =>{
            Debug.Log(success ? "Total Meters reported successfully" : "Failed to report Total Meters");
        });
        */
    }

    public void ReportLeaderboardBestMeters(int meters)
    {
        /*
        Social.ReportScore(meters, BestMetersLeaderboardId, success =>
        {
            Debug.Log(success ? "Best Meters reported successfully" : "Failed to report Best Meters");
        });
        */
    }

    public void ReportAchievementDone(string achievementId, float perc)
    {
        Log("ReportAchievementDone - achievementId: " + achievementId + " - perc: " + perc);

        Social.ReportProgress(achievementId, perc, success =>
        {
            Log(success ? "Achievement reported successfully" : "Failed to report achievement");
            if (success && OnTheRunAchievements.Instance != null)
                OnTheRunAchievements.Instance.OnAchievementProgressSent(OnTheRunSocial.Instance.GetAchievementType(achievementId));
        });
    }

    public void ShowLeaderboards()
    {
        Log("ShowLeaderboards");
        Social.ShowLeaderboardUI();
    }

    public void ShowAchievements()
    {
        Log("ShowAchievements");
        Social.ShowAchievementsUI();
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("#################### KindleUnitySocialImplementation - " + logStr);
    }
}

#endif