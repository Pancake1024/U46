using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SBS.Core;

#if UNITY_ANDROID && !UNITY_KINDLE
public class AndroidSocialImplementation : MonoBehaviour, SocialImplementation
{
    const string kProgressAchievementValueSent = "ach_progr_sent_";

    string BestMetersLeaderboardId { get { return "StringLikeCgkIx5nbwKsJEAIQDQ"; } }
    string TotalMetersLeaderboardId { get { return "StringLikeCgkIx5nbwKsJEAIQDQ"; } }
    
    string SpecialVehicleId         { get { return "CgkI0sL297QCEAIQBg"; } }    // A new toy
    string MaxOutSpecialVehicleId   { get { return "CgkI0sL297QCEAIQDg"; } }    // Full power!
    string BuyCarId                 { get { return "CgkI0sL297QCEAIQAQ"; } }    // Fresh wheels
    string MaxOutCarId              { get { return "CgkI0sL297QCEAIQEA"; } }    // Beauty is on the inside
    string AllScenarioCarsId        { get { return "CgkI0sL297QCEAIQDw"; } }    // I need a bigger garage
    string NewScenarioUnlockedId    { get { return "CgkI0sL297QCEAIQBw"; } }    // Road tripping
    string SpinDiamondId            { get { return "CgkI0sL297QCEAIQCA"; } }    // Diamonds are forever
    string MissionEasyId            { get { return "CgkI0sL297QCEAIQCg"; } }    // What's on the list?
    string MissionHardId            { get { return "CgkI0sL297QCEAIQEg"; } }    // Master
    string LevelVeteranId           { get { return "CgkI0sL297QCEAIQEQ"; } }    // Veteran
    string FBLoginId                { get { return "CgkI0sL297QCEAIQAg"; } }    // Yes, that's me!
    string ComeBackEasyId           { get { return "CgkI0sL297QCEAIQCw"; } }    // I'll be back
    string ComeBackMediumId         { get { return "CgkI0sL297QCEAIQFA"; } }    // Finally mine!
    string CheckpointEasyId         { get { return "CgkI0sL297QCEAIQBA"; } }    // Checkpoint!
    string CheckpointHardId         { get { return "CgkI0sL297QCEAIQFg"; } }    // Stop the clock
    string DestroyVehicleEasyId     { get { return "CgkI0sL297QCEAIQAw"; } }    // Get out the way!
    string DestroyVehicleMediumId   { get { return "CgkI0sL297QCEAIQDQ"; } }    // No more rush hour!
    string UseBoostEasyId           { get { return "CgkI0sL297QCEAIQBQ"; } }    // A little help here!
    string UseBoostHardId           { get { return "CgkI0sL297QCEAIQFw"; } }    // Boostin' up
    string MetersMediumId           { get { return "CgkI0sL297QCEAIQDA"; } }    // Seasoned traveler
    string MetersHardId             { get { return "CgkI0sL297QCEAIQFQ"; } }    // Easy rider
    string CoinsMediumId            { get { return "CgkI0sL297QCEAIQCQ"; } }    // Big money
    string CoinsHardId              { get { return "CgkI0sL297QCEAIQEw"; } }    // Time to spend

    public bool IsLoggedIn { get { return isLoggedIn; } }

    bool isLoggedIn;

    Dictionary<string, int> progressSentByAchievementId;

    public void Init()
    {
        isLoggedIn = false;

        //Debug.Log("###----###----###----###----### Init");
        //PlayGameServices.enableDebugLog(false);

        InitializeGooglePlayServicesCallbacks();
        PlayGameServices.init(string.Empty, false);   // clientId is iOS only
        LoadProgressSent();
    }

    void LoadProgressSent()
    {
        string debugStr = string.Empty;

        progressSentByAchievementId = new Dictionary<string, int>();
        for (int i = 0; i < Enum.GetValues(typeof(OnTheRunAchievements.AchievementType)).Length; i++)
        {
            string achievementId = GetAchievementID((OnTheRunAchievements.AchievementType)i);
            int progress = EncryptedPlayerPrefs.GetInt(kProgressAchievementValueSent + achievementId, 0);

            progressSentByAchievementId.Add(achievementId, progress);

            debugStr += OnTheRunSocial.Instance.GetAchievementType(achievementId).ToString() + "  -  " + progress + "\n";
        }

        //Debug.Log("#################### LOADING ACHIEVEMENT PROGRESS \n" + debugStr);
    }

    void SaveProgressSent()
    {
        string debugStr = string.Empty;

        for (int i = 0; i < Enum.GetValues(typeof(OnTheRunAchievements.AchievementType)).Length; i++)
        {
            string achievementId = GetAchievementID((OnTheRunAchievements.AchievementType)i);
            int progress = progressSentByAchievementId[achievementId];

            EncryptedPlayerPrefs.SetInt(kProgressAchievementValueSent + achievementId, progress);

            debugStr += OnTheRunSocial.Instance.GetAchievementType(achievementId).ToString() + "\t" + progress + "\n";
        }

        //Debug.Log("#################### SAVING ACHIEVEMENT PROGRESS \n" + debugStr);
        EncryptedPlayerPrefs.Save();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveProgressSent();
    }

    public void LogIn()
    {
        //Debug.Log("###----###----###----###----### LogIn");
        PlayGameServices.authenticate();
    }

    public void OnApplicationResumed()
    {
        isLoggedIn = PlayGameServices.isSignedIn();
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
            PlayGameServices.submitScore(TotalMetersLeaderboardId, meters);
    }

    public void ReportLeaderboardBestMeters(int meters)
    {
        if (isLoggedIn)
            PlayGameServices.submitScore(BestMetersLeaderboardId, meters);
    }
    
    public void ReportAchievementDone(string achievementId, float perc)
    {
        //Debug.Log("###----###----###----###----### ReportAchievementDone - loggedin: " + isLoggedIn + " - achievementId: " + achievementId + " - perc: " + perc);
#if UNITY_EDITOR
        if (true)
#else
        if (isLoggedIn)
#endif
        {
            /*if(perc==100.0f)
                PlayGameServices.unlockAchievement(achievementId, true);
            else
                PlayGameServices.unlockAchievement(achievementId, false);*/

            int progressDiff = Mathf.FloorToInt(perc) - progressSentByAchievementId[achievementId];
            Asserts.Assert(progressDiff >= 0, "Progress diff cannot be negative.");
            if (progressDiff <= 0)
                return;

#if UNITY_EDITOR
            //Debug.Log("###----###----###----###----### REPORT ACHIEVEMENT - id: " + OnTheRunSocial.Instance.GetAchievementType(achievementId) + " - progress: " + perc + " - progressDiff: " + progressDiff);
            incrementAchievementSucceededEvent(achievementId, false);
#else
            PlayGameServices.incrementAchievement(achievementId, progressDiff); //(int)perc);//(int)increment);
#endif
        }
    }

    public void ShowLeaderboards()
    {
        PlayGameServices.showLeaderboards();
    }

    public void ShowAchievements()
    {
        PlayGameServices.showAchievements();
    }

    #region GooglePlay Services Callbacks

    public void InitializeGooglePlayServicesCallbacks()
    {
        GPGManager.authenticationSucceededEvent += authenticationSucceededEvent;
        GPGManager.authenticationFailedEvent += authenticationFailedEvent;
        GPGManager.licenseCheckFailedEvent += licenseCheckFailedEvent;
        GPGManager.userSignedOutEvent += userSignedOutEvent;

        GPGManager.submitScoreFailedEvent += submitScoreFailedEvent;
        GPGManager.submitScoreSucceededEvent += submitScoreSucceededEvent;

        GPGManager.unlockAchievementFailedEvent += unlockAchievementFailedEvent;
        GPGManager.unlockAchievementSucceededEvent += unlockAchievementSucceededEvent;

        GPGManager.incrementAchievementFailedEvent += incrementAchievementFailedEvent;
        GPGManager.incrementAchievementSucceededEvent += incrementAchievementSucceededEvent;

        GPGManager.revealAchievementFailedEvent += revealAchievementFailedEvent;
        GPGManager.revealAchievementSucceededEvent += revealAchievementSucceededEvent;
    }

    public void DisableGooglePlayServicesCallbacks()
    {
        GPGManager.authenticationSucceededEvent -= authenticationSucceededEvent;
        GPGManager.authenticationFailedEvent -= authenticationFailedEvent;
        GPGManager.licenseCheckFailedEvent -= licenseCheckFailedEvent;
        GPGManager.userSignedOutEvent -= userSignedOutEvent;

        GPGManager.submitScoreFailedEvent -= submitScoreFailedEvent;
        GPGManager.submitScoreSucceededEvent -= submitScoreSucceededEvent;

        GPGManager.unlockAchievementFailedEvent -= unlockAchievementFailedEvent;
        GPGManager.unlockAchievementSucceededEvent -= unlockAchievementSucceededEvent;

        GPGManager.incrementAchievementFailedEvent -= incrementAchievementFailedEvent;
        GPGManager.incrementAchievementSucceededEvent -= incrementAchievementSucceededEvent;

        GPGManager.revealAchievementFailedEvent -= revealAchievementFailedEvent;
        GPGManager.revealAchievementSucceededEvent -= revealAchievementSucceededEvent;
    }

    void authenticationSucceededEvent(string param)
    {
        //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### authenticationSucceededEvent - " + param);

        isLoggedIn = true;

        StartCoroutine(WaitForAchievementMetadataAtLogin());
    }
    
    IEnumerator WaitForAchievementMetadataAtLogin()
    {
        const int NUM_FRAMES_TIMEOUT = 120;

        for (int frame = 0; frame < NUM_FRAMES_TIMEOUT; frame++)
        {
            //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### WaitForAchievementMetadataAtLogin - framet: " + frame);
            var achievementsMeta = PlayGameServices.getAllAchievementMetadata();

            if (achievementsMeta.Count > 0)
            {
                //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### WaitForAchievementMetadataAtLogin - achievementsMeta.Count: " + achievementsMeta.Count);
                for (int i = 0; i < achievementsMeta.Count; i++)
                {
                    string achId = achievementsMeta[i].achievementId;
                    if (progressSentByAchievementId.ContainsKey(achId))
                    {
                        //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### WaitForAchievementMetadataAtLogin - Seting ach: " + achId + " - progress BEFORE: " + progressSentByAchievementId[achId]);
                        progressSentByAchievementId[achId] = Math.Max(0, achievementsMeta[i].completedSteps);
                        //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### WaitForAchievementMetadataAtLogin - Seting ach: " + achId + " - progress AFTER: " + progressSentByAchievementId[achId]);
                    }
                }

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.ResendProgressNotAcknowledged();
    }

    void authenticationFailedEvent(string param)
    {
        //Debug.Log("###-----###-----###-----###-----###-----###-----###-----### authenticationFailedEvent - " + param);
        isLoggedIn = false;
    }

    void licenseCheckFailedEvent() { }
    
    void userSignedOutEvent()
    {
        isLoggedIn = false;
    }

    void submitScoreFailedEvent(string leaderboardId, string error) { }
    void submitScoreSucceededEvent(string leaderboardId, Dictionary<string, object> scoreReport) { }
    void unlockAchievementFailedEvent(string achievementId, string error) { }
    void unlockAchievementSucceededEvent(string achievementId, bool newlyLocked) { }
    void incrementAchievementFailedEvent(string achievementId, string error)
    {
        //Debug.Log("###----###----###----### incrementAchievementFailedEvent: " + achievementId + " - error: " + error);
    }
    void incrementAchievementSucceededEvent(string achievementId, bool newlyUnlocked)
    {
        //Debug.Log("###----###----###----### incrementAchievementSucceededEvent: " + achievementId + " - newlyUnlocked: " + newlyUnlocked);

        OnTheRunAchievements.AchievementType achievementType = OnTheRunSocial.Instance.GetAchievementType(achievementId);

        if (OnTheRunAchievements.Instance != null)
            OnTheRunAchievements.Instance.OnAchievementProgressSent(achievementType);

        //Debug.Log("###----###----###----### \t\t\t " + achievementType + ": " + Mathf.FloorToInt(OnTheRunAchievements.Instance.GetAchievement(achievementType).Perc));
        progressSentByAchievementId[achievementId] = Mathf.FloorToInt(OnTheRunAchievements.Instance.GetAchievement(achievementType).Perc);

        SaveProgressSent();
    }
    void revealAchievementFailedEvent(string achievementId, string error) { }
    void revealAchievementSucceededEvent(string achievementId) { }

    #endregion
}
#endif