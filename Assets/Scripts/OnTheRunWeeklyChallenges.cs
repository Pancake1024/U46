using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/OnTheRunWeeklyChallenges")]
public class OnTheRunWeeklyChallenges : MonoBehaviour
{
    public class WeeklyChallenge
    {
        public int currentValue;
        public int goalValue;
        public int diamondsReward;
        protected string descriptionText;
        protected string completedText;
        protected bool rewardTaken = false;

        public WeeklyChallenge(string _descriptionText, string _completedText)
        {
            descriptionText = _descriptionText;
            completedText = _completedText;
        }

        public void Reset(int valuePerSession, int baseWeeklySessionPerWeek)
        {
            goalValue = baseWeeklySessionPerWeek * valuePerSession;
            //const bool CHEAT_TEST_GOAL = false;
            //if (CHEAT_TEST_GOAL)
            //    goalValue = 10;
            currentValue = 0;
            rewardTaken = false;
        }

        public string Description
        {
            get
            {
                if (Passed)
                    return completedText;
                else
                    return descriptionText;
            }
        }

        public bool RewardTaken
        {
            set
            {
                rewardTaken = value;
            }
            get
            {
                return rewardTaken;
            }
        }

        public bool Passed
        {
            get
            {
                return currentValue >= goalValue;
            }
        }
    }

    #region Consts
    protected const string kWeeklyChallengeTimerId = "wct";

    protected const string kMetersValueKey = "w_mt";
    protected const string kMetersGoalKey = "w_mg";
    protected const string kMetersRewardTakenKey = "w_mrt";

    protected const string kCoinsValueKey = "w_cn";
    protected const string kCoinsGoalKey = "w_cg";
    protected const string kCoinsRewardTakenKey = "w_crt";

    protected TimeSpan weeklyChallengeDuration;
    #endregion

    protected WeeklyChallenge weeklyMeters;
    protected WeeklyChallenge weeklyCoins;

    public int maxSessionPerDay = 12;
    public int baseWeeklySessionPerDay = 4;
    public int deltaWeeklySessionPerDay = 1;
    public int coinsPerSession = 5000;
    public int metersPerSession = 5000;

    #region Public Properties
    public TimeSpan WeeklyChallengeTimer
    {
        get
        {
            OnTheRunSessionsManager.Timer timer;
            if (OnTheRunSessionsManager.Instance.TryGetTimer(kWeeklyChallengeTimerId, out timer))
                return timer.remainingTime;
            else
                return TimeSpan.FromSeconds(-1.0);
        }
    }
    
    public WeeklyChallenge WeeklyMeters { get { return weeklyMeters; } }
    public WeeklyChallenge WeeklyCoins { get { return weeklyCoins; } }
    #endregion

    void Load()
    {
        SetupWeeklyChallenge();
    }

    void SetupWeeklyChallenge()
    {
#if UNITY_EDITOR
        weeklyChallengeDuration = TimeSpan.FromMinutes(2);
#else
        weeklyChallengeDuration = TimeSpan.FromDays(7);
#endif

        weeklyMeters = new WeeklyChallenge(OnTheRunDataLoader.Instance.GetLocaleString("meters"), OnTheRunDataLoader.Instance.GetLocaleString("challenge_completed"));
        weeklyCoins = new WeeklyChallenge(OnTheRunDataLoader.Instance.GetLocaleString("coins"), OnTheRunDataLoader.Instance.GetLocaleString("challenge_completed"));

        LoadPlayerPrefsData();

        weeklyMeters.diamondsReward = OnTheRunDataLoader.Instance.GetWeeklyChallengeReward();
        weeklyCoins.diamondsReward = OnTheRunDataLoader.Instance.GetWeeklyChallengeReward();

        OnTheRunSessionsManager.Instance.onTimerExpired.RemoveTarget(gameObject, "OnWeeklyChallengeTimerExpired");
        OnTheRunSessionsManager.Instance.onTimerExpired.AddTarget(gameObject, "OnWeeklyChallengeTimerExpired");

        if (!OnTheRunSessionsManager.Instance.HasTimer(kWeeklyChallengeTimerId))
        {
            OnTheRunSessionsManager.Instance.SetTimer(kWeeklyChallengeTimerId, weeklyChallengeDuration, true);
            ResetWeeklyChallenge();
        }
    }

    private void LoadPlayerPrefsData()
    {
        weeklyMeters.currentValue = EncryptedPlayerPrefs.GetInt(kMetersValueKey, 0);
        weeklyMeters.goalValue = EncryptedPlayerPrefs.GetInt(kMetersGoalKey, 0);
        weeklyMeters.RewardTaken = EncryptedPlayerPrefs.GetInt(kMetersRewardTakenKey, 0) == 1;

        weeklyCoins.currentValue = EncryptedPlayerPrefs.GetInt(kCoinsValueKey, 0);
        weeklyCoins.goalValue = EncryptedPlayerPrefs.GetInt(kCoinsGoalKey, 0);
        weeklyCoins.RewardTaken = EncryptedPlayerPrefs.GetInt(kCoinsRewardTakenKey, 0) == 1;
    }

    void Save()
    {
        EncryptedPlayerPrefs.SetInt(kMetersValueKey, weeklyMeters.currentValue);
        EncryptedPlayerPrefs.SetInt(kMetersGoalKey, weeklyMeters.goalValue);
        EncryptedPlayerPrefs.SetInt(kMetersRewardTakenKey, weeklyMeters.RewardTaken ? 1 : 0);

        EncryptedPlayerPrefs.SetInt(kCoinsValueKey, weeklyCoins.currentValue);
        EncryptedPlayerPrefs.SetInt(kCoinsGoalKey, weeklyCoins.goalValue);
        EncryptedPlayerPrefs.SetInt(kCoinsRewardTakenKey, weeklyCoins.RewardTaken ? 1 : 0);

        EncryptedPlayerPrefs.Save();
    }

    void OnWeeklyChallengeTimerExpired(OnTheRunSessionsManager.Timer timer)
    {
        if (timer.id.Equals(kWeeklyChallengeTimerId))
            ResetWeeklyChallenge();
    }

    void ResetWeeklyChallenge()
    {
        int baseWeeklySessionPerWeek = baseWeeklySessionPerDay * 7;
        int deltaWeeklyDays = EncryptedPlayerPrefs.GetInt("delta_day", deltaWeeklySessionPerDay - 1);

        weeklyMeters.Reset(metersPerSession, baseWeeklySessionPerWeek + deltaWeeklyDays);
        weeklyCoins.Reset(coinsPerSession, baseWeeklySessionPerWeek + deltaWeeklyDays);

        deltaWeeklyDays = Mathf.Clamp(deltaWeeklyDays + 1, 0, maxSessionPerDay);
        EncryptedPlayerPrefs.SetInt("delta_day", deltaWeeklyDays);

        OnTheRunSessionsManager.Instance.SetTimer(kWeeklyChallengeTimerId, weeklyChallengeDuration, true);

        if (Manager<UIManager>.Get().ActivePage.name.Equals("RewardPage"))
            Manager<UIManager>.Get().ActivePage.GetComponent<UIRewardPage>().UpdateWeeklyChallenges(0, 0);
    }

    public void UpdateWeeklyChallenges(int sessionMeters, int sessionCoins)
    {
        weeklyMeters.currentValue = Mathf.Clamp(weeklyMeters.currentValue + sessionMeters, 0, weeklyMeters.goalValue);
        weeklyCoins.currentValue = Mathf.Clamp(weeklyCoins.currentValue + sessionCoins, 0, weeklyCoins.goalValue);
    }
}
