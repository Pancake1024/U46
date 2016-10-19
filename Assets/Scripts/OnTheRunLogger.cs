using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/OnTheRunLogger")]
public class OnTheRunLogger : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunLogger instance = null;

    public static OnTheRunLogger Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Protected methods
    #endregion

    #region Messages
    void OnStartGame()
    {
    }

    void OnGameover()
    {
    }
    #endregion

    #region Public methods
    public void LogBuy(String category, String id, int owned, int nuggets, int rank)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("category", category);
        values.Add("id", id);
        values.Add("owned", owned.ToString());
        values.Add("nggts", nuggets.ToString());
        values.Add("rank", rank.ToString());
        SBS.Flurry.FlurryBinding.logEvent("itemBuy", values);
    }

    public void LogInAppPurchase(String id, int nuggets, int rank)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("id", id);
        values.Add("nggts", nuggets.ToString());
        values.Add("rank", rank.ToString());
        SBS.Flurry.FlurryBinding.logEvent("inAppPurchase", values);
    }

    public void LogPage(String screenId)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("id", screenId);
        SBS.Flurry.FlurryBinding.logEvent("page", values);
    }

    public void LogPageTimed(String screenId)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("id", screenId);
        SBS.Flurry.FlurryBinding.logEvent("pageTimed", values, true);
    }

    public void EndPageTimed()
    {
        SBS.Flurry.FlurryBinding.endTimedEvent("pageTimed");
    }

    public void LogClick(String id)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("id", id);
        SBS.Flurry.FlurryBinding.logEvent("click", values);
    }

    public void LogMissionDone(String id, int totalMissions)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("id", id);
        values.Add("totalMissions", totalMissions.ToString());
        SBS.Flurry.FlurryBinding.logEvent("missionDone", values);
    }

    public void LogNewRank(int rank, int bestRank)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("rank", rank.ToString());
        values.Add("bestRank", bestRank.ToString());
        SBS.Flurry.FlurryBinding.logEvent("newRank", values);
    }

    public void LogRockeggGift()
    {
        SBS.Flurry.FlurryBinding.logEvent("rockeggGift");
    }

    public void LogGemTaken(String gemType)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("type", gemType);
        SBS.Flurry.FlurryBinding.logEvent("gemTaken", values);
    }

    public void LogEnvironmentChange(String envType)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("type", envType);
        SBS.Flurry.FlurryBinding.logEvent("envChange", values);
    }

    public void LogDie(String obstacleType)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("type", obstacleType);
        SBS.Flurry.FlurryBinding.logEvent("died", values);
    }

    public void LogPlay(String charaId, int rank, int totalPlays, int totalMissionsDone, int totalMeters, int nuggetsEarned, int bestMeters)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("characterId", charaId);
        values.Add("rank", rank.ToString());
        values.Add("totalPlays", totalPlays.ToString());
        values.Add("totalMissions", totalMissionsDone.ToString());
        values.Add("totalMeters", totalMeters.ToString());
        values.Add("totalNuggetsEarned", nuggetsEarned.ToString());
        values.Add("bestMeters", bestMeters.ToString());
        SBS.Flurry.FlurryBinding.logEvent("play", values, true);
    }

    public void EndPlay()
    {
        SBS.Flurry.FlurryBinding.endTimedEvent("play");
    }

    public void LogPlayData(int meters, int nuggets, int nuggetsEarnedPlay, int nuggetsEarned, int rank, float velocity, int bestMeters, int numJumps, int numCrouches, int numTrackChanged)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("meters", meters.ToString());
        values.Add("nuggets", nuggets.ToString());
        values.Add("nuggetsEarnedPlay", nuggetsEarnedPlay.ToString());
        values.Add("nuggetsEarned", nuggetsEarned.ToString());
        values.Add("rank", rank.ToString());
        values.Add("velocity", velocity.ToString("0.00"));
        values.Add("bestMeters", bestMeters.ToString());
        values.Add("numJumps", numJumps.ToString());
        values.Add("numCrouches", numCrouches.ToString());
        values.Add("numTrackChanged", numTrackChanged.ToString());
        SBS.Flurry.FlurryBinding.logEvent("playData", values);
    }

    public void LogPlayDataMeters(int totalMeters, int totalMetersOnLeft, int totalMetersOnCenter, int totalMetersOnRight, int totalMetersTiltRight, int totalMetersTiltLeft)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("totalMeters", totalMeters.ToString());
        values.Add("totalMetersOnLeft", totalMetersOnLeft.ToString());
        values.Add("totalMetersOnCenter", totalMetersOnCenter.ToString());
        values.Add("totalMetersOnRight", totalMetersOnRight.ToString());
        values.Add("totalMetersTiltRight", totalMetersTiltRight.ToString());
        values.Add("totalMetersTiltLeft", totalMetersTiltLeft.ToString());
        SBS.Flurry.FlurryBinding.logEvent("playDataMeters", values);
    }

    public void LogPlayDataObstacles(int obstaclesUpperDone, int obstaclesLowerDone, int obstaclesRightDone, int obstaclesLeftDone)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("upper", obstaclesUpperDone.ToString());
        values.Add("lower", obstaclesLowerDone.ToString());
        values.Add("right", obstaclesRightDone.ToString());
        values.Add("left", obstaclesLeftDone.ToString());
        SBS.Flurry.FlurryBinding.logEvent("playDataObstacles", values);
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
