using UnityEngine;
using System.Collections;
using SBS.Core;
using System;
using System.Collections.Generic;

public class OnTheRunPlayTimeCounter : Manager<OnTheRunPlayTimeCounter>
{
    #region Singleton instance
    public static OnTheRunPlayTimeCounter Instance
    {
        get
        {
            return Manager<OnTheRunPlayTimeCounter>.Get();
        }
    }
    #endregion

    const bool LOGS_ARE_ENABLED = false;
    const string kPlayTimeKey = "total_play_time";

    bool worldSessionGuard;
    //long lastRecordedWorldTicks;
    float lastRecordedWorldRealTimeSinceStartup;
    float currentSessionWorldTime;
    float currentSessionTime;

    bool sessionGuard;
    float lastRecordedPlayTime;
    //long lastRecordedTicks;
    float lastRecordedRealtimeSinceStartup;

    bool saveOnRead;

    public void SetSaveOnRead(bool saveOnReadIsEnabled)
    {
        saveOnRead = saveOnReadIsEnabled;
    }

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        sessionGuard = false;
        worldSessionGuard = false;
        lastRecordedPlayTime = 0.0f;
        //lastRecordedTicks = Environment.TickCount;
        lastRecordedRealtimeSinceStartup = Time.realtimeSinceStartup;
        //lastRecordedWorldTicks = Environment.TickCount;
        lastRecordedWorldRealTimeSinceStartup = Time.realtimeSinceStartup;
        saveOnRead = false;
    }

    void Start()
    {
        currentSessionTime = 0.0f;

        if (!sessionGuard)
            this.OnSessionBegin();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (sessionGuard)
                this.OnSessionEnd();
        }
        else
        {
            if(!sessionGuard)
                this.OnSessionBegin();
        }
    }

    void OnApplicationQuit()
    {
        if (sessionGuard)
            OnSessionEnd();
    }

    void OnSessionBegin()
    {
        Asserts.Assert(!sessionGuard);
        sessionGuard = true;

        lastRecordedPlayTime = PlayerPrefs.GetFloat(kPlayTimeKey, 0.0f);
        currentSessionTime = 0.0f;
        //lastRecordedTicks = Environment.TickCount;
        lastRecordedRealtimeSinceStartup = Time.realtimeSinceStartup;

        Log("OnSessionBegin - playTime: " + GetPlayTime() + " - sessionTime: " + GetSessionTime());
    }

    void OnSessionEnd()
    {
        UpdatePlayTimeData();
        SavePlayTime();

        Log("OnSessionEnd - playTime: " + GetPlayTime() + " - sessionTime: " + GetSessionTime());

        Asserts.Assert(sessionGuard);
        sessionGuard = false;
    }

    public float GetPlayTime()
    {
        UpdatePlayTimeData();
        if (saveOnRead)
            SavePlayTime();

        Log("GetPlayTime - playTime: " + lastRecordedPlayTime);

        return lastRecordedPlayTime;
    }

    public float GetSessionTime()
    {
        UpdatePlayTimeData();
        return currentSessionTime;
    }

    public void OverridePlayTime(float time)
    {
        lastRecordedPlayTime = time;
        //lastRecordedTicks = Environment.TickCount;
        lastRecordedRealtimeSinceStartup = Time.realtimeSinceStartup;
    }

    void UpdatePlayTimeData()
    {
        /*long currentTicks = Environment.TickCount;
        TimeSpan dt = TimeSpan.FromSeconds((currentTicks - lastRecordedTicks) * 0.001);

        currentSessionTime += (float)dt.TotalSeconds;
        lastRecordedPlayTime += (float)dt.TotalSeconds;
        lastRecordedTicks = currentTicks;
        */
        float dt = Time.realtimeSinceStartup - lastRecordedRealtimeSinceStartup;
        currentSessionTime += dt;
        lastRecordedPlayTime += dt;
        lastRecordedRealtimeSinceStartup = Time.realtimeSinceStartup;
    }

    void SavePlayTime()
    {
        PlayerPrefs.SetFloat(kPlayTimeKey, lastRecordedPlayTime);
        PlayerPrefs.Save();
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### TIME COUNTER - " + logStr);
    }
}