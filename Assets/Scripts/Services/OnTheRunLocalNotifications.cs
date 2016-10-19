using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SBS.Core;

public class OnTheRunLocalNotifications : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunLocalNotifications instance = null;

    public static OnTheRunLocalNotifications Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    static int numDelayedNotifications = 0;
    protected const int DAY_SECONDS = 86400;
    protected List<string> notificationStrings;
    protected string notificationFuelFreeze;
    protected string notificationFuelRefilled;
    protected string[] notificationDailyBonus;
    protected string[] notificationDailyMissions;

    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        GameObject.DontDestroyOnLoad(gameObject);

        Init();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    void Init()
    {
        notificationStrings = new List<string>();
        //notificationStrings.Add("Push notification test for OnTheRun mobile!");

        //GENERIC NOTIFICATIONS-----------------------------------
        string text = "",
               id = "";
        int idx = 0;
        do
        {
            ++idx;
            id = "notifications_generic_text" + idx;
            text = OnTheRunDataLoader.Instance.GetLocaleString(id);
            if ((text != id))
                notificationStrings.Add(text);
            //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- LocalNotifications loop");
        } while (text != id);

        notificationFuelFreeze = OnTheRunDataLoader.Instance.GetLocaleString("notifications_timedevents_text1");
        notificationFuelRefilled = OnTheRunDataLoader.Instance.GetLocaleString("notifications_timedevents_text2");
        
        notificationDailyBonus = new string[3];
        notificationDailyBonus[0] = OnTheRunDataLoader.Instance.GetLocaleString("notifications_daily_bonus1");
        notificationDailyBonus[1] = OnTheRunDataLoader.Instance.GetLocaleString("notifications_daily_bonus2");
        notificationDailyBonus[2] = OnTheRunDataLoader.Instance.GetLocaleString("notifications_daily_bonus3");

        /*notificationDailyMissions = new string[3];
        notificationDailyMissions[0] = OnTheRunDataLoader.Instance.GetLocaleString("notification_newdailymissions1");
        notificationDailyMissions[1] = OnTheRunDataLoader.Instance.GetLocaleString("notification_newdailymissions2");
        notificationDailyMissions[2] = OnTheRunDataLoader.Instance.GetLocaleString("notification_newdailymissions3");*/
    }

    void OnApplicationPause(bool paused)
    {
#if !UNITY_WEBPLAYER
        if (paused)
            ScheduleLocalNotifications();
        else
            SBS.Miniclip.LNBindings.CancelAllLocalNotifications();
#endif
    }

    void OnApplicationQuit()
    {
#if !UNITY_WEBPLAYER
        ScheduleLocalNotifications();
#endif
    }

    void ScheduleLocalNotifications()
    {
        if (notificationStrings == null)
            return;

        SBS.Miniclip.LNBindings.CancelAllLocalNotifications();
        numDelayedNotifications = 0;

        //cheat-----------------
        //TestLocalNotifications();
        //return;
        //----------------------

        //GENERIC NOTIFICATIONS-----------------------------------
        if (notificationStrings.Count > 0)
        {
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 2);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 3);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 5);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 7);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 15);
            ScheduleNotificationWithSafeTimeSpan(notificationStrings[UnityEngine.Random.Range(0, notificationStrings.Count)], DAY_SECONDS * 30);
        }

        if (OnTheRunFuelManager.Instance.IsFuelFreezeActive())
        {
            //FUEL FREEZE EXPIRATION NOTIFICATION--------------------------------
            int timeForFuelFreezeExpiration = (int)OnTheRunFuelManager.Instance.FuelFreezeTimer.TotalSeconds;
            if (timeForFuelFreezeExpiration < OnTheRunDataLoader.Instance.GetFuelFreezeNotificationTime() && timeForFuelFreezeExpiration > 0)
                ScheduleNotificationWithSafeTimeSpan(notificationFuelFreeze, timeForFuelFreezeExpiration);
        }
        else
        {
            //FUEL TANK REFILLED--------------------------------
            int timeForFuelRefilled = Mathf.CeilToInt(OnTheRunFuelManager.Instance.RemainingFuelTimer);
            if (timeForFuelRefilled>0)
                ScheduleNotificationWithSafeTimeSpan(notificationFuelRefilled, timeForFuelRefilled);
        }

        //DAILY BONUS NOTIFICATIONS-----------------------------------
        if (OnTheRunDailyBonusManager.Instance!=null)
            ScheduleNotificationWithSafeTimeSpan(notificationDailyBonus[UnityEngine.Random.Range(0, notificationDailyBonus.Length)], OnTheRunDailyBonusManager.Instance.GetDailyBonusNotificationTime());


        //DAILY MISSION NOTIFICATIONS-----------------------------------
        /*int days;
        OnTheRunSessionsManager.Instance.GetDaysPassedSinceFirstSession(out days);
        if (days % 2 == 0)
        {
            DateTime lastDayPlayed = OnTheRunSessionsManager.Instance.GetCurrentSessionBegin();
            DayOfWeek lastDayPlayedDayInWeek = lastDayPlayed.DayOfWeek;
            DateTime nextDay;

            for (int i = 0; i < 3; ++i)
            {
                DayOfWeek nextDayInWeek = lastDayPlayed.AddDays(2 + i * 2).DayOfWeek;
                nextDay = GetNextDateForDay(lastDayPlayed, nextDayInWeek);
                ScheduleNotificationWithSafeTimeSpan(notificationDailyMissions[UnityEngine.Random.Range(0, notificationDailyMissions.Length)], (int)(nextDay - lastDayPlayed).TotalSeconds);
            }
        }*/
    }

    DateTime GetNextDateForDay(DateTime startDate, DayOfWeek desiredDay)
    {
        DateTime nextDate = startDate;
        while (nextDate.DayOfWeek != desiredDay)
        {
            //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- GetNextDateForDay loop");
            nextDate = nextDate.AddDays(1D);
        }

        return nextDate;
    }

    void TestLocalNotifications()
    {
        return;

        // PIETRO: Commentate xche' davano fastidio! Rimettiamole quando implementate correttamente come richiesto dal gioco
        ScheduleNotificationWithSafeTimeSpan("Test SAFE 5 sec", 5);
        ScheduleNotificationWithSafeTimeSpan("Test SAFE 10 sec", 10);
        ScheduleNotificationWithSafeTimeSpan("Test SAFE 30 sec", 30);
    }

    void ScheduleNotificationWithSafeTimeSpan(string notificationString, int secondsDelay)
    {
        int prohibitedTimeSpanBegin = 22;
        int prohibitedTimeSpanEnd = 7;
        int minutesToWaitWhenPushIsDelayed = 30;

        DateTime notificationTime = DateTime.Now.AddSeconds(secondsDelay);
        if (notificationTime.TimeOfDay.Hours >= prohibitedTimeSpanBegin || notificationTime.TimeOfDay.Hours < prohibitedTimeSpanEnd)
        {
            int daysToAdd = 0;
            if (notificationTime.TimeOfDay.Hours >= prohibitedTimeSpanBegin && notificationTime.TimeOfDay.Hours < 24)
                daysToAdd = 1;

            DateTime safeNotificationTime = notificationTime.Date.AddDays(daysToAdd).AddHours(prohibitedTimeSpanEnd);
            notificationTime = safeNotificationTime.AddMinutes(minutesToWaitWhenPushIsDelayed * numDelayedNotifications);
            numDelayedNotifications++;
        }

        int notificationSecondsDelay = Mathf.CeilToInt((float)(notificationTime - DateTime.Now).TotalSeconds);
        SBS.Miniclip.LNBindings.ScheduleLocalNotification(notificationString, "View", notificationSecondsDelay, OnTheRunDataLoader.Instance.GetLocaleString("on_the_run"));
    }
}