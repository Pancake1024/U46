using UnityEngine;
using System.Collections;
using SBS.Core;

#if UNITY_ANDROID && UNITY_KINDLE
public class KindleSocialImplementation : MonoBehaviour, SocialImplementation
{
    enum GameCircleInitializationStatus
    {
        Uninitialized,
        InitializationRequested,
        Ready,
        Unavailable,
    }

    string BestMetersLeaderboardId { get { return "leaderboard_meters"; } }
    string TotalMetersLeaderboardId { get { return "leaderboard_total_meters"; } }

    string SpecialVehicleId       { get { return "achiev_buy_special_vehicle"; } }        // A new toy
    string MaxOutSpecialVehicleId { get { return "achiev_max_out_special_vehicle"; } }    // Full power!
    string BuyCarId               { get { return "achiev_buy_car"; } }                    // Fresh wheels
    string MaxOutCarId            { get { return "achiev_max_out_car"; } }                // Beauty is on the inside
    string AllScenarioCarsId      { get { return "achiev_all_cars_stage"; } }             // I need a bigger garage
    string NewScenarioUnlockedId  { get { return "achiev_new_stage_unlocked"; } }         // Road tripping
    string SpinDiamondId          { get { return "achiev_win_diamonds_spin"; } }          // Diamonds are forever
    string MissionEasyId          { get { return "achiev_complete_missions_medium"; } }   // What's on the list?
    string MissionHardId          { get { return "achiev_complete_missions_hard"; } }     // Master
    string LevelVeteranId         { get { return "achiev_level_veteran"; } }              // Veteran
    string FBLoginId              { get { return "achiev_fb_login"; } }                   // Yes, that's me!
    string ComeBackEasyId         { get { return "achiev_come_back"; } }                  // I'll be back
    string ComeBackMediumId       { get { return "achiev_final_daily_bonus_prize"; } }    // Finally mine!
    string CheckpointEasyId       { get { return "achiev_cross_checkpoints_easy"; } }     // Checkpoint!
    string CheckpointHardId       { get { return "achiev_cross_checkpoints_hard"; } }     // Stop the clock
    string DestroyVehicleEasyId   { get { return "achiev_destroy_vehicles_easy"; } }      // Get out the way!
    string DestroyVehicleMediumId { get { return "achiev_destroy_vehicles_medium"; } }    // No more rush hour!
    string UseBoostEasyId         { get { return "achiev_use_boosts_easy"; } }            // A little help here!
    string UseBoostHardId         { get { return "achiev_use_boosts_hard"; } }            // Boostin' up
    string MetersMediumId         { get { return "achiev_meters_medium"; } }              // Seasoned traveler
    string MetersHardId           { get { return "achiev_meters_hard"; } }                // Easy rider
    string CoinsMediumId          { get { return "achiev_earn_coins_medium"; } }          // Big money
    string CoinsHardId            { get { return "achiev_earn_coins_hard"; } }            // Time to spend

    const bool LOGS_ARE_ENABLED = false;

    public bool IsLoggedIn { get { return IsLoggedIn; } }

    bool isLoggedIn;
    bool usesLeaderboards = false;
    bool usesAchievements = true;
    bool usesWhispersync = false;
    bool enablePopups = true;
    GameCirclePopupLocation toastLocation = GameCirclePopupLocation.BOTTOM_CENTER;
    GameCircleInitializationStatus initializationStatus = GameCircleInitializationStatus.Uninitialized;

    public void Init()
    {
        Log("Init");
        gameObject.AddComponent<GameCircleManager>();
        //InitializeGameCircle();
    }

    public void LogIn()
    {
        Log("LogIn");
        InitializeGameCircle();
    }

    public void InitializeGameCircle()
    {
        Log("InitializeGameCircle");
        initializationStatus = GameCircleInitializationStatus.InitializationRequested;
        SubscribeToGameCircleInitializationEvents();
        Log("InitializeGameCircle - Calling AGSClient.Init - usesLeaderboards: " + usesLeaderboards + " - usesAchievements: " + usesAchievements + " - usesWhispersync: " + usesWhispersync);
        AGSClient.Init(usesLeaderboards, usesAchievements, usesWhispersync);
    }

    public void OnApplicationResumed()
    {
        Log("OnApplicationResumed - initializationStatus: " + initializationStatus);
        isLoggedIn = (initializationStatus == GameCircleInitializationStatus.Ready);

        if (!isLoggedIn)
        {
            Log("OnApplicationResumed - forcing login");
            LogIn();
        }
    }

    public string GetAchievementID(OnTheRunAchievements.AchievementType type)
    {
        string retValue = "";

        switch (type)
        {
            case OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE:
                retValue = SpecialVehicleId;
                break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_SPECIAL_VEHICLE:
                retValue = MaxOutSpecialVehicleId;
                break;
            case OnTheRunAchievements.AchievementType.BUY_CAR:
                retValue = BuyCarId;
                break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_CAR:
                retValue = MaxOutCarId;
                break;
            case OnTheRunAchievements.AchievementType.ALL_SCENARIO_CARS:
                retValue = AllScenarioCarsId;
                break;
            case OnTheRunAchievements.AchievementType.NEW_SCENARIO_UNLOCKED:
                retValue = NewScenarioUnlockedId;
                break;
            case OnTheRunAchievements.AchievementType.SPIN_DIAMOND:
                retValue = SpinDiamondId;
                break;
            case OnTheRunAchievements.AchievementType.MISSION_MEDIUM:
                retValue = MissionEasyId;
                break;
            case OnTheRunAchievements.AchievementType.MISSION_HARD:
                retValue = MissionHardId;
                break;
            case OnTheRunAchievements.AchievementType.LEVEL_VETERAN:
                retValue = LevelVeteranId;
                break;
            case OnTheRunAchievements.AchievementType.FB_LOGIN:
                retValue = FBLoginId;
                break;
            case OnTheRunAchievements.AchievementType.COME_BACK_MEDIUM:
                retValue = ComeBackEasyId;
                break;
            case OnTheRunAchievements.AchievementType.COME_BACK_HARD:
                retValue = ComeBackMediumId;
                break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_EASY:
                retValue = CheckpointEasyId;
                break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_HARD:
                retValue = CheckpointHardId;
                break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY:
                retValue = DestroyVehicleEasyId;
                break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM:
                retValue = DestroyVehicleMediumId;
                break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_EASY:
                retValue = UseBoostEasyId;
                break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_HARD:
                retValue = UseBoostHardId;
                break;
            case OnTheRunAchievements.AchievementType.METERS_MEDIUM:
                retValue = MetersMediumId;
                break;
            case OnTheRunAchievements.AchievementType.METERS_HARD:
                retValue = MetersHardId;
                break;
            case OnTheRunAchievements.AchievementType.COINS_MEDIUM:
                retValue = CoinsMediumId;
                break;
            case OnTheRunAchievements.AchievementType.COINS_HARD:
                retValue = CoinsHardId;
                break;
        }

        return retValue;
    }

    public void ReportLeaderboardTotalMeters(int meters)
    {
        if (isLoggedIn)
            SubmitScoreToLeaderboard(TotalMetersLeaderboardId, meters);
    }

    public void ReportLeaderboardBestMeters(int meters)
    {
        if (isLoggedIn)
            SubmitScoreToLeaderboard(BestMetersLeaderboardId, meters);
    }

    public void ReportAchievementDone(string achievementId, float perc)
    {
        Log("ReportAchievementDone - achievementId: " + achievementId + " - perc: " + perc);
        if (isLoggedIn)
            SubmitAchievement(achievementId, perc);
    }

    public void ShowLeaderboards()
    {
        Log("ShowLeaderboards - initializationStatus: " + initializationStatus);
        if (initializationStatus != GameCircleInitializationStatus.Ready || !usesLeaderboards)
            return;

        AGSLeaderboardsClient.ShowLeaderboardsOverlay();
    }

    public void ShowAchievements()
    {
        Log("ShowAchievements - initializationStatus: " + initializationStatus);
        if (initializationStatus != GameCircleInitializationStatus.Ready || !usesAchievements)
            return;

        AGSAchievementsClient.ShowAchievementsOverlay();
    }

#region GameCircle Submission Methods

    public void SubmitScoreToLeaderboard(string leaderboardId, long scoreValue)
    {
        if (initializationStatus != GameCircleInitializationStatus.Ready || !usesLeaderboards)
            return;

        SubscribeToScoreSubmissionEvents();
        AGSLeaderboardsClient.SubmitScore(leaderboardId, scoreValue);
    }

    public void SubmitAchievement(string achievementId, float progress)
    {
        Log("SubmitAchievement - initializationStatus: " + initializationStatus + " - achievementId: " + achievementId + " - progress: " + progress);
        if (initializationStatus != GameCircleInitializationStatus.Ready || !usesAchievements)
            return;

        SubscribeToSubmitAchievementEvents();
        Log("SubmitAchievement - UpdateAchievementProgress");
        AGSAchievementsClient.UpdateAchievementProgress(achievementId, progress);
    }

#endregion

#region GameCircle Initialization Methods

    void SubscribeToGameCircleInitializationEvents()
    {
        Log("SubscribeToGameCircleInitializationEvents");
        AGSClient.ServiceReadyEvent += GameCircleReadyHandler;
        AGSClient.ServiceNotReadyEvent += GameCircleNotReadyHandler;
    }

    void UnsubscribeFromGameCircleInitializationEvents()
    {
        Log("UnsubscribeFromGameCircleInitializationEvents");
        AGSClient.ServiceReadyEvent -= GameCircleReadyHandler;
        AGSClient.ServiceNotReadyEvent -= GameCircleNotReadyHandler;
    }

    void GameCircleReadyHandler()
    {
        Log("GameCircleReadyHandler");
        initializationStatus = GameCircleInitializationStatus.Ready;                
        UnsubscribeFromGameCircleInitializationEvents();

        AGSClient.SetPopUpEnabled(enablePopups);
        AGSClient.SetPopUpLocation(toastLocation);

        isLoggedIn = true;

        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.ResendProgressNotAcknowledged();
    }

    void GameCircleNotReadyHandler(string error)
    {
        Log("GameCircleNotReadyHandler - error: " + error);
        initializationStatus = GameCircleInitializationStatus.Unavailable;
        UnsubscribeFromGameCircleInitializationEvents();

        isLoggedIn = false;
    }

#endregion

#region GameCircle Achievements Events

    void SubscribeToSubmitAchievementEvents()
    {
        Log("SubscribeToSubmitAchievementEvents");
        AGSAchievementsClient.UpdateAchievementFailedEvent += UpdateAchievementsFailed;
        AGSAchievementsClient.UpdateAchievementSucceededEvent += UpdateAchievementsSucceeded;
    }

    void UnsubscribeFromSubmitAchievementEvents()
    {
        Log("UnsubscribeFromSubmitAchievementEvents");
        AGSAchievementsClient.UpdateAchievementFailedEvent -= UpdateAchievementsFailed;
        AGSAchievementsClient.UpdateAchievementSucceededEvent -= UpdateAchievementsSucceeded;
    }

    void UpdateAchievementsSucceeded(string achievementId)
    {
        Log("UpdateAchievementsSucceeded - achievementId: " + achievementId);
        UnsubscribeFromSubmitAchievementEvents();
        
        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.OnAchievementProgressSent(OnTheRunSocial.Instance.GetAchievementType(achievementId));
    }

    void UpdateAchievementsFailed(string achievementId, string error)
    {
        Log("UpdateAchievementsFailed - achievementId: " + achievementId + " - error: " + error);
        UnsubscribeFromSubmitAchievementEvents();
    }

#endregion

#region GameCircle Leaderboards Events

    void SubscribeToScoreSubmissionEvents()
    {
        Log("SubscribeToScoreSubmissionEvents");
        AGSLeaderboardsClient.SubmitScoreFailedEvent += SubmitScoreFailed;
        AGSLeaderboardsClient.SubmitScoreSucceededEvent += SubmitScoreSucceeded;
    }

    void UnsubscribeFromScoreSubmissionEvents()
    {
        Log("UnsubscribeFromScoreSubmissionEvents");
        AGSLeaderboardsClient.SubmitScoreFailedEvent -= SubmitScoreFailed;
        AGSLeaderboardsClient.SubmitScoreSucceededEvent -= SubmitScoreSucceeded;
    }

    void SubmitScoreSucceeded(string leaderboardId)
    {
        Log("SubmitScoreSucceeded - leaderboardId: " + leaderboardId);
        UnsubscribeFromScoreSubmissionEvents();
    }

    void SubmitScoreFailed(string leaderboardId, string error)
    {
        Log("SubmitScoreFailed - leaderboardId: " + leaderboardId + " - error: " + error);
        UnsubscribeFromScoreSubmissionEvents();
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### KindleSocialImplementation - " + logStr);
    }
#endregion
}
#endif