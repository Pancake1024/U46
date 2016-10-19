using UnityEngine;
using System.Collections;
using SBS.Core;
using System;
using System.Globalization;

public class OnTheRunSessionsManager : Manager<OnTheRunSessionsManager>
{
    #region Singleton instance
    public static OnTheRunSessionsManager Instance
    {
        get
        {
            return Manager<OnTheRunSessionsManager>.Get();
        }
    }
    #endregion

    #region Public structs
    public struct Timer
    {
        public string id;
        public DateTime expireDate;
        public TimeSpan elapsedTime;
        public TimeSpan remainingTime;

        public void Print(string prefix)
        {
            prefix += "\n\tid: " + id;
            prefix += "\n\texpire date: " + expireDate;
            prefix += "\n\telapsed time: " + elapsedTime;
            prefix += "\n\tremaining time: " + remainingTime;
            Debug.Log(prefix);
        }
    }
    #endregion

    #region Consts
    private const string kFirstLaunchKey = "fl";
    private const string kFirstLaunchDateKey = "fld";
    private const string kFirstLaunchDateValidKey = "fldv";

    private const string kLastSessionBeginDateKey = "lsbd";
    private const string kLastSessionBeginDateValidKey = "lsbdv";

    private const string kLastSessionEndDateKey = "lsed";
    private const string kLastSessionEndDateValidKey = "lsedv";

    private const string kSessionCounterKey = "session_counter_key";
    #endregion

    #region Public members
    public Signal onSessionBegin = Signal.CreateVoid();
    public Signal onSessionEnd = Signal.CreateVoid();
    public Signal onTimerExpired = Signal.Create<Timer>();
    #endregion

    #region Private members
    private bool firstLaunch;
    private DateTime currentSessionBegin;
    private bool currentSessionBeginDateIsValid;

    private DateTime firstSessionBegin;
    private bool firstSessionDateIsValid;

    private DateTime lastSessionBegin;
    private bool lastSessionBeginDateIsValid;
    private DateTime lastSessionEnd;
    private bool lastSessionEndDateIsValid;

    private long secsPassedSinceLastSessionEnd;
    private bool firstUnpause;
    private bool sessionGuard;

    private long timersTimestamp;
    private Timer[] timers;

    private bool sessionHasBegun;

    private int sessionCounter;
    #endregion

    #region Public methods
    public bool IsFirstSession()
    {
        return firstLaunch;
    }

    public int GetSessionCounter()
    {
        return sessionCounter;
    }

    public DateTime GetCurrentSessionBegin()
    {
        return currentSessionBegin;
    }

    public bool IsCurrentSessionBeginValid()
    {
        return currentSessionBeginDateIsValid;
    }

    public bool GetDaysPassedSinceFirstSession(out int daysPassed)
    {
        DateTime now;
        bool isValid;
        DateTimeManager.Instance.GetDate(out now, out isValid);

        daysPassed = Mathf.FloorToInt((float)(now - firstSessionBegin).TotalDays);

        return isValid || firstSessionDateIsValid;
    }

    public bool GetSessionTimeSpan(out TimeSpan timeSpan)
    {
        DateTime now;
        bool isValid;
        DateTimeManager.Instance.GetDate(out now, out isValid);

        timeSpan = now - currentSessionBegin;

        return isValid || currentSessionBeginDateIsValid;
    }

    public long GetSecondsPassedSinceLastSessionEnd()
    {
        return secsPassedSinceLastSessionEnd;
    }

    public void SetTimer(string id, TimeSpan duration, bool resetTime)
    {
        DateTime expDate;
        bool isValid;
        DateTimeManager.Instance.GetDate(out expDate, out isValid);

        expDate += duration;
        int index = Array.FindIndex<Timer>(timers, (timer) => { return timer.id == id; });
        if (-1 == index)
        {
            index = timers.Length;
            Array.Resize<Timer>(ref timers, index + 1);

            timers[index] = new Timer()
            {
                id = id,
                expireDate = expDate,
                elapsedTime = TimeSpan.FromSeconds(.0),
                remainingTime = duration
            };
        }
        else
        {
            var timer = timers[index];

            timer.expireDate = expDate;
            if (resetTime)
            {
                timer.elapsedTime = TimeSpan.FromSeconds(.0);
                timer.remainingTime = duration;
            }
            else
                timer.remainingTime = duration - timer.elapsedTime;

            timers[index] = timer;
        }
    }

    public bool TryGetTimer(string id, out Timer timer)
    {
        int index = Array.FindIndex<Timer>(timers, (t) => { return t.id == id; });
        if (-1 == index)
        {
            timer = new Timer();
            return false;
        }
        timer = timers[index];
        return true;
    }

    public bool HasTimer(string id)
    {
        int index = Array.FindIndex<Timer>(timers, (t) => { return t.id == id; });
        return index > -1;
    }

    public bool RemoveTimer(string id)
    {
        int index = Array.FindIndex<Timer>(timers, (t) => { return t.id == id; });
        if (-1 == index)
            return false;

        int newSize = timers.Length - 1;
        timers[index] = timers[newSize];
        Array.Resize<Timer>(ref timers, newSize);

        return true;
    }
    #endregion

    #region Private methods
    #region Timers
    private const string kTimersCountKey = "tc";
    private const string kTimerIdPrefixKey = "tid";
    private const string kTimerExpDatePrefixKey = "ted";
    private const string kTimerElapsedTimePrefixKey = "tet";
    private const string kTimerRemainingTimePrefixKey = "trt";

    private void LoadTimers()
    {
        int timersCount = EncryptedPlayerPrefs.GetInt(kTimersCountKey, 0);
        timers = new Timer[timersCount];
        timersTimestamp = Environment.TickCount;
        for (int i = 0; i < timersCount; ++i)
        {
            timers[i] = new Timer()
            {
                id = EncryptedPlayerPrefs.GetString(kTimerIdPrefixKey + i, string.Empty),
                expireDate = EncryptedPlayerPrefs.GetUtcDate(kTimerExpDatePrefixKey + i),
                elapsedTime = TimeSpan.FromMilliseconds(EncryptedPlayerPrefs.GetLong(kTimerElapsedTimePrefixKey + i, 0)),
                remainingTime = TimeSpan.FromMilliseconds(EncryptedPlayerPrefs.GetLong(kTimerRemainingTimePrefixKey + i, 0))
            };
        }
    }

    private void SaveTimers()
    {
        EncryptedPlayerPrefs.SetInt(kTimersCountKey, timers.Length);
        for (int i = 0, c = timers.Length; i < c; ++i)
        {
            var timer = timers[i];

            EncryptedPlayerPrefs.SetString(kTimerIdPrefixKey + i, timer.id);
            EncryptedPlayerPrefs.SetUtcDate(kTimerExpDatePrefixKey + i, timer.expireDate);
            EncryptedPlayerPrefs.SetLong(kTimerElapsedTimePrefixKey + i, (long)System.Math.Floor(timer.elapsedTime.TotalMilliseconds));
            EncryptedPlayerPrefs.SetLong(kTimerRemainingTimePrefixKey + i, (long)System.Math.Floor(timer.remainingTime.TotalMilliseconds));
        }
    }

    private void UpdateTimersOnResume()
    {
        timersTimestamp = Environment.TickCount;

        TimeSpan timeSpan = TimeSpan.FromSeconds(secsPassedSinceLastSessionEnd < 0L ? 0L : secsPassedSinceLastSessionEnd);

        for (int i = timers.Length - 1; i >= 0; --i)
        {
            var timer = timers[i];

            timer.elapsedTime += timeSpan;
            timer.remainingTime -= timeSpan;

            if (currentSessionBegin >= timer.expireDate)
            {
                Asserts.Assert(timer.remainingTime.TotalSeconds <= .0, "TotalSeconds: " + timer.remainingTime.TotalSeconds);
                timer.remainingTime = TimeSpan.FromSeconds(.0);
                
                int lastIndex = timers.Length - 1;
                timers[i] = timers[lastIndex];
                Array.Resize<Timer>(ref timers, lastIndex);
                onTimerExpired.Invoke(timer);
            }
            else
                timers[i] = timer;
        }
    }

    private void UpdateTimersOnFrame()
    {
        TimeSpan dt = TimeSpan.FromSeconds((Environment.TickCount - timersTimestamp) * 0.001);
        timersTimestamp = Environment.TickCount;

        for (int i = timers.Length - 1; i >= 0; --i)
        {
            var timer = timers[i];

            timer.elapsedTime += dt;
            timer.remainingTime -= dt;

            if (timer.remainingTime.TotalSeconds <= 0)
            {
                timer.remainingTime = TimeSpan.FromSeconds(.0);

                int lastIndex = timers.Length - 1;
                timers[i] = timers[lastIndex];
                Array.Resize<Timer>(ref timers, lastIndex);

                onTimerExpired.Invoke(timer);
            }
            else
                timers[i] = timer;
        }
    }
    #endregion

    private void PrintSessionBegin()
    {
        int days;
        this.GetDaysPassedSinceFirstSession(out days);

        Debug.Log("*** SESSION BEGIN ***");
        Debug.Log("First session: " + firstLaunch);
        Debug.Log("Session counter: " + sessionCounter);
        Debug.Log("Days passed since first session: " + days);
        Debug.Log("Last session begin: " + lastSessionBegin);
        Debug.Log("Last session end: " + lastSessionEnd);
        Debug.Log("Session begin: " + currentSessionBegin);
        Debug.Log("Seconds passed: " + secsPassedSinceLastSessionEnd);
        Debug.Log("Session begin is valid: " + currentSessionBeginDateIsValid);
    }

    private void PrintSessionEnd()
    {
        TimeSpan timeSpan;
        this.GetSessionTimeSpan(out timeSpan);

        Debug.Log("*** SESSION END ***");
        Debug.Log("Session duration: " + timeSpan);
        Debug.Log("Last session begin: " + lastSessionBegin);
        Debug.Log("Last session end: " + lastSessionEnd);
    }

    /*private void OnSessionBegin()
    {
        Asserts.Assert(!sessionGuard);
        sessionGuard = true;

        firstLaunch = 1 == EncryptedPlayerPrefs.GetInt(kFirstLaunchKey, 1);

        if (!firstLaunch)
        {
            if (!firstUnpause)
            {
                lastSessionBegin = currentSessionBegin;
                lastSessionBeginDateIsValid = currentSessionBeginDateIsValid;
            }

            Asserts.Assert(lastSessionEnd.Ticks > 0);
            currentSessionBegin = lastSessionEnd;
        }
        else
        {
            Asserts.Assert(0 == lastSessionEnd.Ticks);
        }

        DateTimeManager.Instance.GetDeltaSecondsAsync(currentSessionBegin, currentSessionBeginDateIsValid, secsPassedSinceLastSessionEnd, (currentDate, currentDateIsValid, deltaSeconds) =>
        {
            currentSessionBegin = currentDate;
            currentSessionBeginDateIsValid = currentDateIsValid;
            secsPassedSinceLastSessionEnd = (long)(currentSessionBegin - lastSessionEnd).TotalSeconds;

            if (!currentSessionBeginDateIsValid && currentDateIsValid)
            {
                currentSessionBeginDateIsValid = true;

                if (firstLaunch)
                {
                    firstSessionDateIsValid = true;
                    EncryptedPlayerPrefs.SetInt(kFirstLaunchDateValidKey, 1);
                }

                EncryptedPlayerPrefs.SetInt(kLastSessionBeginDateValidKey, 1);

                EncryptedPlayerPrefs.Save();
            }
            
            if (firstLaunch)
            {
                secsPassedSinceLastSessionEnd = 0;

                Asserts.Assert(0 == firstSessionBegin.Ticks);
                firstSessionBegin = currentSessionBegin;
                firstSessionDateIsValid = currentSessionBeginDateIsValid;

                EncryptedPlayerPrefs.SetUtcDate(kFirstLaunchDateKey, currentSessionBegin);
                EncryptedPlayerPrefs.SetInt(kFirstLaunchDateValidKey, currentSessionBeginDateIsValid ? 1 : 0);
            }

            EncryptedPlayerPrefs.SetUtcDate(kLastSessionBeginDateKey, currentSessionBegin);
            EncryptedPlayerPrefs.SetInt(kLastSessionBeginDateValidKey, currentSessionBeginDateIsValid ? 1 : 0);

            firstUnpause = false;

            EncryptedPlayerPrefs.Save();

            this.UpdateTimersOnResume();

            this.PrintSessionBegin();

            sessionHasBegun = true;
            onSessionBegin.Invoke();
        });
    }*/

    private void OnSessionBegin()
    {
        Asserts.Assert(!sessionGuard);
        sessionGuard = true;

        firstLaunch = 1 == EncryptedPlayerPrefs.GetInt(kFirstLaunchKey, 1);

        sessionCounter = EncryptedPlayerPrefs.GetInt(kSessionCounterKey, -1);
        sessionCounter++;
        EncryptedPlayerPrefs.SetInt(kSessionCounterKey, sessionCounter);
        EncryptedPlayerPrefs.Save();

        if (!firstLaunch)
        {
            if (!firstUnpause)
            {
                lastSessionBegin = currentSessionBegin;
                lastSessionBeginDateIsValid = currentSessionBeginDateIsValid;
            }

            Asserts.Assert(lastSessionEnd.Ticks > 0);
            currentSessionBegin = lastSessionEnd;
        }
        else
        {
            Asserts.Assert(0 == lastSessionEnd.Ticks);
        }

        if (firstUnpause)
        {
            DateTimeManager.Instance.GetDeltaSecondsAsync(currentSessionBegin, currentSessionBeginDateIsValid, secsPassedSinceLastSessionEnd, (currentDate, currentDateIsValid, deltaSeconds) =>
            {
                currentSessionBegin = currentDate;
                currentSessionBeginDateIsValid = currentDateIsValid;
                secsPassedSinceLastSessionEnd = (long)(currentSessionBegin - lastSessionEnd).TotalSeconds;

                SetSessionIsValidFlagsOnBegin(currentDateIsValid);
                StartSessionOnBegin();
            });
        }
        else
        {
            DateTimeManager.Instance.GetDeltaSecondsWithValidation(ref currentSessionBegin, ref currentSessionBeginDateIsValid, out secsPassedSinceLastSessionEnd, 30.0f, (success) =>
            {
                SetSessionIsValidFlagsOnBegin(success);
            });
            StartSessionOnBegin();

        }
    }

    private void SetSessionIsValidFlagsOnBegin(bool currentDateIsValid)
    {
        if (!currentSessionBeginDateIsValid && currentDateIsValid)
        {
            currentSessionBeginDateIsValid = true;

            if (firstLaunch)
            {
                firstSessionDateIsValid = true;
                EncryptedPlayerPrefs.SetInt(kFirstLaunchDateValidKey, 1);
            }

            EncryptedPlayerPrefs.SetInt(kLastSessionBeginDateValidKey, 1);

            EncryptedPlayerPrefs.Save();
        }
    }

    private void StartSessionOnBegin()
    {
        if (firstLaunch)
        {
            secsPassedSinceLastSessionEnd = 0;

            Asserts.Assert(0 == firstSessionBegin.Ticks);
            firstSessionBegin = currentSessionBegin;
            firstSessionDateIsValid = currentSessionBeginDateIsValid;

            EncryptedPlayerPrefs.SetUtcDate(kFirstLaunchDateKey, currentSessionBegin);
            EncryptedPlayerPrefs.SetInt(kFirstLaunchDateValidKey, currentSessionBeginDateIsValid ? 1 : 0);
        }

        EncryptedPlayerPrefs.SetUtcDate(kLastSessionBeginDateKey, currentSessionBegin);
        EncryptedPlayerPrefs.SetInt(kLastSessionBeginDateValidKey, currentSessionBeginDateIsValid ? 1 : 0);

        firstUnpause = false;

        EncryptedPlayerPrefs.Save();

        this.UpdateTimersOnResume();

        this.PrintSessionBegin();

        sessionHasBegun = true;
        onSessionBegin.Invoke();
    }

    private void OnSessionEnd()
    {
        if (firstLaunch)
            EncryptedPlayerPrefs.SetInt(kFirstLaunchKey, 0);

        lastSessionBegin = currentSessionBegin;
        lastSessionBeginDateIsValid = currentSessionBeginDateIsValid;

        DateTime prevSessionEnd = lastSessionEnd;
        bool prevSessionEndWasValid = lastSessionEndDateIsValid;
        DateTimeManager.Instance.GetDate(out lastSessionEnd, out lastSessionEndDateIsValid);
        if (lastSessionEnd.Ticks < prevSessionEnd.Ticks) // don't rewind time
        {
            lastSessionEnd = prevSessionEnd;
            lastSessionEndDateIsValid = prevSessionEndWasValid;
        }

        EncryptedPlayerPrefs.SetUtcDate(kLastSessionEndDateKey, lastSessionEnd);
        EncryptedPlayerPrefs.SetInt(kLastSessionEndDateValidKey, lastSessionEndDateIsValid ? 1 : 0);

        this.PrintSessionEnd();
        onSessionEnd.Invoke();

        this.SaveTimers();

        EncryptedPlayerPrefs.Save();

        Asserts.Assert(sessionGuard);
        sessionGuard = false;
    }
    #endregion

    #region Unity callbacks
    new void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
        firstUnpause = true;
        sessionGuard = false;

        sessionHasBegun = false;

        firstSessionBegin = EncryptedPlayerPrefs.GetUtcDate(kFirstLaunchDateKey);
        firstSessionDateIsValid = 1 == EncryptedPlayerPrefs.GetInt(kFirstLaunchDateValidKey, 0);

        lastSessionBegin = EncryptedPlayerPrefs.GetUtcDate(kLastSessionBeginDateKey);
        lastSessionBeginDateIsValid = 1 == EncryptedPlayerPrefs.GetInt(kLastSessionBeginDateValidKey, 0);

        lastSessionEnd = EncryptedPlayerPrefs.GetUtcDate(kLastSessionEndDateKey);
        lastSessionEndDateIsValid = 1 == EncryptedPlayerPrefs.GetInt(kLastSessionEndDateValidKey, 0);

        this.LoadTimers();
    }

    void Start()
    {
        if (!sessionGuard)
            this.OnSessionBegin();
    }

    void Update()
    {
		//SBS.Miniclip.UtilsBindings.ConsoleLog("SessionManager Update");
        if (sessionHasBegun)
            this.UpdateTimersOnFrame();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            this.OnSessionEnd();
        else
            this.OnSessionBegin();
    }

    void OnApplicationQuit()
    {
        if (sessionGuard)
            this.OnSessionEnd();
    }
    #endregion
}
