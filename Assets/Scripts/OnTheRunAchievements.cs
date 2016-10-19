using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("OnTheRun/OnTheRunAchievements")]
public class OnTheRunAchievements : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunAchievements instance = null;
    public static OnTheRunAchievements Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    public class Achievement
    {
        public string id;
        public AchievementType type;
        public float levelToReach;

        protected bool done;
        protected float counter;
        protected bool sent;

        public float Counter { get { return counter; } set { counter = value; } }
        public bool Done { get { return done; } set { done = value; EncryptedPlayerPrefs.SetInt(id + "_d", done ? 1 : 0); } }
        public bool Sent { get { return sent; } set { sent = value; } }

        public float Perc
        {
            get
            {
                if (done)
                    return 100.0f;

                Asserts.Assert(levelToReach > 0.0f, "Invalid level to reach");

                return (counter / levelToReach) * 100.0f;
            }
        }

        public Achievement(int _num, string _id, AchievementType _type, float _levelToReach = 0)
        {
            id = _id;
            if (id == "")
                id = _type.ToString() + "_" + _num.ToString();
            type = _type;
            levelToReach = _levelToReach;
            done = false;
            counter = 0.0f;
            sent = false;
        }

        public void Update(bool isRunning, float increaseQuantity = 1.0f)
        {
            if (!done)
            {
                counter += increaseQuantity;
                if (counter >= levelToReach)
                    done = true;

                sent = false;

                //Debug.Log("###----###----###----###----###------------------------ updating achievement - id: " + id + " - perc: " + Perc);

                save();

                if (isRunning && !done)
                    return;

                this.sendProgress();
            }
            else if (!sent)
                this.sendProgress();
        }

        public void load()
        {
            done = EncryptedPlayerPrefs.GetInt(id + "_d", 0) == 1 ? true : false;
            counter = EncryptedPlayerPrefs.GetFloat(id + "_c", 0.0f);
            sent = EncryptedPlayerPrefs.GetInt(id + "_s", 0) == 1 ? true : false;
            //Debug.Log("LOAD achiev " + id + ": " + done + " " + counter);
        }

        public void save()
        {
            EncryptedPlayerPrefs.SetInt(id + "_d", done ? 1 : 0);
            EncryptedPlayerPrefs.SetFloat(id + "_c", counter);
            EncryptedPlayerPrefs.SetInt(id + "_s", sent ? 1 : 0);
            //Debug.Log("SAVE achiev " + id + ": " + done + " " + counter);
        }

        public void sendProgress()
        {
            //Debug.Log("###----###----###----###----###------------------------ sending achievement - id: " + id + " - levelToReach: " + levelToReach + " - counter: " + counter + " - perc: " + Perc);
            if (Perc > 0.0f)
                OnTheRunSocial.Instance.ReportAchievementDone(id, Perc);
        }
    }

    public enum AchievementType
    {
        BUY_SPECIAL_VEHICLE = 0,
        MAX_OUT_SPECIAL_VEHICLE,
        BUY_CAR,
        MAX_OUT_CAR,
        ALL_SCENARIO_CARS,
        NEW_SCENARIO_UNLOCKED,
        SPIN_DIAMOND,
        MISSION_MEDIUM,
        MISSION_HARD,
        LEVEL_VETERAN,
        FB_LOGIN,
        COME_BACK_MEDIUM,
        COME_BACK_HARD,
        CHECKPOINT_EASY,
        CHECKPOINT_HARD,
        DESTROY_VEHICLE_EASY,
        DESTROY_VEHICLE_MEDIUM,
        USE_BOOST_EASY,
        USE_BOOST_HARD,
        METERS_MEDIUM,
        METERS_HARD,
        COINS_MEDIUM,
        COINS_HARD,
        COUNT
    }

    protected bool isRunning = false;
    protected Dictionary<AchievementType, Achievement> achievements = null;

    protected int mission_easy_counter = 10;
    protected int mission_hard_counter = 100;
    protected int level_veteran_counter = 10;
    protected int checkpoint_easy_counter = 30;
    protected int checkpoint_hard_counter = 500;
    protected int destroy_vehicle_easy_counter = 20;
    protected int destroy_vehicle_medium_counter = 200;
    protected int use_boost_easy_counter = 15;
    protected int use_boost_hard_counter = 100;
    protected float meters_medium = 40000.0f;
    protected float meters_hard = 1000000.0f;
    protected int coins_medium = 5000;
    protected int coins_hard = 50000;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        DontDestroyOnLoad(gameObject);
#if !UNITY_WEBPLAYER
        Initialize();
#endif
    }

    void Initialize()
    {
        achievements = new Dictionary<AchievementType, Achievement>();
        achievements.Add(AchievementType.BUY_SPECIAL_VEHICLE,     new Achievement( 1, OnTheRunSocial.Instance.GetAchievementID(AchievementType.BUY_SPECIAL_VEHICLE),     AchievementType.BUY_SPECIAL_VEHICLE,     1                             ));
        achievements.Add(AchievementType.MAX_OUT_SPECIAL_VEHICLE, new Achievement( 2, OnTheRunSocial.Instance.GetAchievementID(AchievementType.MAX_OUT_SPECIAL_VEHICLE), AchievementType.MAX_OUT_SPECIAL_VEHICLE, 1                             ));
        achievements.Add(AchievementType.BUY_CAR,                 new Achievement( 3, OnTheRunSocial.Instance.GetAchievementID(AchievementType.BUY_CAR),                 AchievementType.BUY_CAR,                 1                             ));
        achievements.Add(AchievementType.MAX_OUT_CAR,             new Achievement( 4, OnTheRunSocial.Instance.GetAchievementID(AchievementType.MAX_OUT_CAR),             AchievementType.MAX_OUT_CAR,             1                             ));
        achievements.Add(AchievementType.ALL_SCENARIO_CARS,       new Achievement( 5, OnTheRunSocial.Instance.GetAchievementID(AchievementType.ALL_SCENARIO_CARS),       AchievementType.ALL_SCENARIO_CARS,       1                             ));
        achievements.Add(AchievementType.NEW_SCENARIO_UNLOCKED,   new Achievement( 6, OnTheRunSocial.Instance.GetAchievementID(AchievementType.NEW_SCENARIO_UNLOCKED),   AchievementType.NEW_SCENARIO_UNLOCKED,   1                             ));
        achievements.Add(AchievementType.SPIN_DIAMOND,            new Achievement( 7, OnTheRunSocial.Instance.GetAchievementID(AchievementType.SPIN_DIAMOND),            AchievementType.SPIN_DIAMOND,            1                             ));
        achievements.Add(AchievementType.MISSION_MEDIUM,          new Achievement( 8, OnTheRunSocial.Instance.GetAchievementID(AchievementType.MISSION_MEDIUM),          AchievementType.MISSION_MEDIUM,          mission_easy_counter          ));
        achievements.Add(AchievementType.MISSION_HARD,            new Achievement( 9, OnTheRunSocial.Instance.GetAchievementID(AchievementType.MISSION_HARD),            AchievementType.MISSION_HARD,            mission_hard_counter          ));
        achievements.Add(AchievementType.LEVEL_VETERAN,           new Achievement(10, OnTheRunSocial.Instance.GetAchievementID(AchievementType.LEVEL_VETERAN),           AchievementType.LEVEL_VETERAN,           level_veteran_counter         ));
        achievements.Add(AchievementType.FB_LOGIN,                new Achievement(11, OnTheRunSocial.Instance.GetAchievementID(AchievementType.FB_LOGIN),                AchievementType.FB_LOGIN,                1                             ));
        achievements.Add(AchievementType.COME_BACK_MEDIUM,        new Achievement(12, OnTheRunSocial.Instance.GetAchievementID(AchievementType.COME_BACK_MEDIUM),        AchievementType.COME_BACK_MEDIUM,        1                             ));
        achievements.Add(AchievementType.COME_BACK_HARD,          new Achievement(13, OnTheRunSocial.Instance.GetAchievementID(AchievementType.COME_BACK_HARD),          AchievementType.COME_BACK_HARD,          1                             ));
        achievements.Add(AchievementType.CHECKPOINT_EASY,         new Achievement(14, OnTheRunSocial.Instance.GetAchievementID(AchievementType.CHECKPOINT_EASY),         AchievementType.CHECKPOINT_EASY,         checkpoint_easy_counter       ));
        achievements.Add(AchievementType.CHECKPOINT_HARD,         new Achievement(15, OnTheRunSocial.Instance.GetAchievementID(AchievementType.CHECKPOINT_HARD),         AchievementType.CHECKPOINT_HARD,         checkpoint_hard_counter       ));
        achievements.Add(AchievementType.DESTROY_VEHICLE_EASY,    new Achievement(16, OnTheRunSocial.Instance.GetAchievementID(AchievementType.DESTROY_VEHICLE_EASY),    AchievementType.DESTROY_VEHICLE_EASY,    destroy_vehicle_easy_counter  ));
        achievements.Add(AchievementType.DESTROY_VEHICLE_MEDIUM,  new Achievement(17, OnTheRunSocial.Instance.GetAchievementID(AchievementType.DESTROY_VEHICLE_MEDIUM),  AchievementType.DESTROY_VEHICLE_MEDIUM,  destroy_vehicle_medium_counter));
        achievements.Add(AchievementType.USE_BOOST_EASY,          new Achievement(18, OnTheRunSocial.Instance.GetAchievementID(AchievementType.USE_BOOST_EASY),          AchievementType.USE_BOOST_EASY,          use_boost_easy_counter        ));
        achievements.Add(AchievementType.USE_BOOST_HARD,          new Achievement(19, OnTheRunSocial.Instance.GetAchievementID(AchievementType.USE_BOOST_HARD),          AchievementType.USE_BOOST_HARD,          use_boost_hard_counter        ));
        achievements.Add(AchievementType.METERS_MEDIUM,           new Achievement(20, OnTheRunSocial.Instance.GetAchievementID(AchievementType.METERS_MEDIUM),           AchievementType.METERS_MEDIUM,           meters_medium                 ));
        achievements.Add(AchievementType.METERS_HARD,             new Achievement(21, OnTheRunSocial.Instance.GetAchievementID(AchievementType.METERS_HARD),             AchievementType.METERS_HARD,             meters_hard                   ));
        achievements.Add(AchievementType.COINS_MEDIUM,            new Achievement(22, OnTheRunSocial.Instance.GetAchievementID(AchievementType.COINS_MEDIUM),            AchievementType.COINS_MEDIUM,            coins_medium                  ));
        achievements.Add(AchievementType.COINS_HARD,              new Achievement(23, OnTheRunSocial.Instance.GetAchievementID(AchievementType.COINS_HARD),              AchievementType.COINS_HARD,              coins_hard                    ));

        LoadAchievements();
    }

    public Achievement GetAchievement(AchievementType _type)
    {
        Achievement retValue = null;
        if (achievements.ContainsKey(_type))
            retValue = achievements[_type];

        return retValue;
    }

    /*public void Save()
    {
        Debug.Log("Save Achievements---------- " + achievements.Count);

#if !UNITY_WEBPLAYER
        if (achievements != null)
        {            
            foreach (var kvp in achievements)
                kvp.Value.save();
        }
#endif
    }*/

    public void CheckForAchievementsToSend()
    {
#if !UNITY_WEBPLAYER
        foreach (var kvp in achievements)
        {
            var achievement = kvp.Value;
            if (achievement.Done && !achievement.Sent)
                achievement.sendProgress();
        }
#endif
    }

    public void LoadAchievements( )
    {
#if !UNITY_WEBPLAYER
        foreach (var kvp in achievements)
            kvp.Value.load();
#endif
    }

    public void Reset()
    {
#if !UNITY_WEBPLAYER
        Initialize();
#endif
    }

    public void ResetAchievement(AchievementType _type)
    {
#if !UNITY_WEBPLAYER
        if (achievements.ContainsKey(_type))
            achievements[_type].Counter = 0.0f;
#endif
    }

    public void AchievementEvent(AchievementType _type, float _updateValue = 1.0f)
    {
#if !UNITY_WEBPLAYER
        if (achievements.ContainsKey(_type))
            achievements[_type].Update(isRunning, _updateValue);
#endif
    }
    
    public void OnRunStarted()
    {
        isRunning = true;
    }

    public void OnRunFinished()
    {
        isRunning = false;

        ResendProgressNotAcknowledged();
    }

    public void ResendProgressNotAcknowledged()
    {
#if !UNITY_WEBPLAYER
        //Debug.Log("###----###----### ResendProgressNotAcknowledged");

        if (achievements != null)
        {
            foreach (var kvp in achievements)
            {
                var achievement = kvp.Value;
                if (!achievement.Sent)
                    achievement.sendProgress();
            }
        }
#endif
    }

    public void OnAchievementProgressSent(AchievementType achievementType)
    {
        //Debug.Log("###----###----###----### OnAchievementProgressSent - " + achievementType);

        if (achievements != null && achievements.ContainsKey(achievementType))
        {
            achievements[achievementType].Sent = true;
            achievements[achievementType].save();
        }
    }
}