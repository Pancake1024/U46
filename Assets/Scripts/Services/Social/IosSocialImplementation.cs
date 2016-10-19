using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.SocialPlatforms.GameCenter;

public class IosSocialImplementation : MonoBehaviour, SocialImplementation
{
	/*
	[DllImport("__Internal")]
	private static extern string getBestMeters_0_LeaderboardId();
	[DllImport("__Internal")]
    private static extern string getTotalScoreLeaderboardId();
    */
    
    [DllImport("__Internal")]
    private static extern string getBuySpecialVehicleId();
    [DllImport("__Internal")]
    private static extern string getMaxOutSpecialVehicleId();
    [DllImport("__Internal")]
    private static extern string getBuyCarId();
    [DllImport("__Internal")]
    private static extern string getMaxOutCarId();
    [DllImport("__Internal")]
    private static extern string getAllScenarioCarsId();
    [DllImport("__Internal")]
    private static extern string getNewScenarioUnlockedId();
    [DllImport("__Internal")]
    private static extern string getSpinDiamondId();
    [DllImport("__Internal")]
    private static extern string getMissionEasyId();
    [DllImport("__Internal")]
    private static extern string getMissionHardId();
    [DllImport("__Internal")]
    private static extern string getLevelVeteranId();
    [DllImport("__Internal")]
    private static extern string getFBLoginId();
    [DllImport("__Internal")]
    private static extern string getComeBackEasyId();
    [DllImport("__Internal")]
    private static extern string getComeBackMediumId();
    [DllImport("__Internal")]
    private static extern string getCheckpointEasyId();
    [DllImport("__Internal")]
    private static extern string getCheckpointHardId();
    [DllImport("__Internal")]
    private static extern string getDestroyVehicleEasyId();
    [DllImport("__Internal")]
    private static extern string getDestroyVehicleMediumId();
    [DllImport("__Internal")]
    private static extern string getUseBoostEasyId();
    [DllImport("__Internal")]
    private static extern string getUseBoostHardId();
    [DllImport("__Internal")]
    private static extern string getMetersMediumId();
    [DllImport("__Internal")]
    private static extern string getMetersHardId();
    [DllImport("__Internal")]
    private static extern string getCoinsMediumId();
    [DllImport("__Internal")]
    private static extern string getCoinsHardId();

	string BestMeters_0_LeaderboardId { get { return ""; } } //getBestMeters_0_LeaderboardId(); } }
	string TotalMetersLeaderboardId { get { return ""; } } //return getTotalScoreLeaderboardId(); } }
	
    string SpecialVehicleId { get { return getBuySpecialVehicleId(); } }
    string MaxOutSpecialVehicleId { get { return getMaxOutSpecialVehicleId(); } }
    string BuyCarId { get { return getBuyCarId(); } }
    string MaxOutCarId { get { return getMaxOutCarId(); } }
    string AllScenarioCarsId { get { return getAllScenarioCarsId(); } }
    string NewScenarioUnlockedId { get { return getNewScenarioUnlockedId(); } }
    string SpinDiamondId { get { return getSpinDiamondId(); } }
    string MissionEasyId { get { return getMissionEasyId(); } }
    string MissionHardId { get { return getMissionHardId(); } }
    string LevelVeteranId { get { return getLevelVeteranId(); } }
    string FBLoginId { get { return getFBLoginId(); } }
    string ComeBackEasyId { get { return getComeBackEasyId(); } }
    string ComeBackMediumId { get { return getComeBackMediumId(); } }
    string CheckpointEasyId { get { return getCheckpointEasyId(); } }
    string CheckpointHardId { get { return getCheckpointHardId(); } }
    string DestroyVehicleEasyId { get { return getDestroyVehicleEasyId(); } }
    string DestroyVehicleMediumId { get { return getDestroyVehicleMediumId(); } }
    string UseBoostEasyId { get { return getUseBoostEasyId(); } }
    string UseBoostHardId { get { return getUseBoostHardId(); } }
    string MetersMediumId { get { return getMetersMediumId(); } }
    string MetersHardId { get { return getMetersHardId(); } }
    string CoinsMediumId { get { return getCoinsMediumId(); } }
    string CoinsHardId { get { return getCoinsHardId(); } }

    public bool IsLoggedIn { get { return isLoggedIn; }  }

    bool isLoggedIn;

    public void Init()
    {
		//LogIn();
    }

    public void LogIn()
    {
		//Debug.Log ("#################### IosSocialImplementation - LogIn()");
        GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        Social.localUser.Authenticate(OnAuthenticationCallback);
    }

    void OnAuthenticationCallback(bool success)
	{
		//Debug.Log ("#################### IosSocialImplementation - OnAuthenticationCallback(" + success + ")");
        if (success)
        {
            isLoggedIn = true;
            if (OnTheRunAchievements.Instance != null)
                OnTheRunAchievements.Instance.ResendProgressNotAcknowledged();
        }
    }

    public void OnApplicationResumed()
	{
		//Debug.Log ("#################### IosSocialImplementation - OnApplicationResumed()");
		if (!isLoggedIn)
			LogIn();
    }

    public string GetAchievementID(OnTheRunAchievements.AchievementType type)
    {
        string retValue = "";

        switch (type)
        {
            case OnTheRunAchievements.AchievementType.BUY_SPECIAL_VEHICLE:      retValue = SpecialVehicleId;        break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_SPECIAL_VEHICLE:  retValue = MaxOutSpecialVehicleId;  break;
            case OnTheRunAchievements.AchievementType.BUY_CAR:                  retValue = BuyCarId;                break;
            case OnTheRunAchievements.AchievementType.MAX_OUT_CAR:              retValue = MaxOutCarId;             break;
            case OnTheRunAchievements.AchievementType.ALL_SCENARIO_CARS:        retValue = AllScenarioCarsId;       break;
            case OnTheRunAchievements.AchievementType.NEW_SCENARIO_UNLOCKED:    retValue = NewScenarioUnlockedId;   break;
            case OnTheRunAchievements.AchievementType.SPIN_DIAMOND:             retValue = SpinDiamondId;           break;
            case OnTheRunAchievements.AchievementType.MISSION_MEDIUM:           retValue = MissionEasyId;           break;
            case OnTheRunAchievements.AchievementType.MISSION_HARD:             retValue = MissionHardId;           break;
            case OnTheRunAchievements.AchievementType.LEVEL_VETERAN:            retValue = LevelVeteranId;          break;
            case OnTheRunAchievements.AchievementType.FB_LOGIN:                 retValue = FBLoginId;               break;
            case OnTheRunAchievements.AchievementType.COME_BACK_MEDIUM:         retValue = ComeBackEasyId;          break;
            case OnTheRunAchievements.AchievementType.COME_BACK_HARD:           retValue = ComeBackMediumId;        break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_EASY:          retValue = CheckpointEasyId;        break;
            case OnTheRunAchievements.AchievementType.CHECKPOINT_HARD:          retValue = CheckpointHardId;        break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_EASY:     retValue = DestroyVehicleEasyId;    break;
            case OnTheRunAchievements.AchievementType.DESTROY_VEHICLE_MEDIUM:   retValue = DestroyVehicleMediumId;  break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_EASY:           retValue = UseBoostEasyId;          break;
            case OnTheRunAchievements.AchievementType.USE_BOOST_HARD:           retValue = UseBoostHardId;          break;
            case OnTheRunAchievements.AchievementType.METERS_MEDIUM:            retValue = MetersMediumId;          break;
            case OnTheRunAchievements.AchievementType.METERS_HARD:              retValue = MetersHardId;            break;
            case OnTheRunAchievements.AchievementType.COINS_MEDIUM:             retValue = CoinsMediumId;           break;
            case OnTheRunAchievements.AchievementType.COINS_HARD:               retValue = CoinsHardId;             break;
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
		Social.ReportScore(meters, BestMeters_0_LeaderboardId, success => {
            Debug.Log(success ? "Best Meters reported successfully" : "Failed to report Best Meters");
        });
    }
    
    public void ReportAchievementDone(string achievementId, float perc)
    {
        Social.ReportProgress(achievementId, perc, success =>
        {
            Debug.Log(success ? "Achievement reported successfully" : "Failed to report achievement");
            if (success && OnTheRunAchievements.Instance != null)
                OnTheRunAchievements.Instance.OnAchievementProgressSent(OnTheRunSocial.Instance.GetAchievementType(achievementId));
        });
    }

    public void ShowLeaderboards()
    {
        Social.ShowLeaderboardUI();
    }

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
        //Social.Active.ShowAchievementsUI();
    }
}