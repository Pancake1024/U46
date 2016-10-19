using SBS.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OnTheRunFuelManager : Manager<OnTheRunFuelManager>
{
    #region Singleton instance
    public static OnTheRunFuelManager Instance
    {
        get
        {
            return Manager<OnTheRunFuelManager>.Get();
        }
    }
    #endregion

    #region Consts
    protected const string kFuelFreezeTimerId = "fs";
    protected const string kFuelFillTimerId = "ff";
    protected const string kFuelKey = "f";

#if UNITY_EDITOR
    protected TimeSpan fuelFillTime;// = TimeSpan.FromSeconds(30);
    protected TimeSpan fuelFreezeTime;// = TimeSpan.FromMinutes(2);
#else
    protected TimeSpan fuelFillTime = TimeSpan.FromMinutes(7);
    protected TimeSpan fuelFreezeTime = TimeSpan.FromDays(1);
#endif

    protected int maxFuel = 6; // Now from XML
    #endregion

    #region Protected members
    protected int fuel;
    #endregion

    #region Public properties
    public int Fuel
    {
        get
        {
            return fuel;
        }
        set
        {
#if !UNITY_WEBPLAYER
            fuel = value;
#endif
            if (fuel < maxFuel)
                OnTheRunSessionsManager.Instance.SetTimer(kFuelFillTimerId, fuelFillTime, false);
            else
                OnTheRunSessionsManager.Instance.RemoveTimer(kFuelFillTimerId);
        }
    }

    public float RemainingFuelTimer
    {
        get
        {
            if (fuel >= maxFuel)
                return -1.0f;

            TimeSpan offset = TimeSpan.FromSeconds(.0);

            int c = maxFuel - fuel;
            OnTheRunSessionsManager.Timer timer;
            if (OnTheRunSessionsManager.Instance.TryGetTimer(kFuelFillTimerId, out timer))
            {
                offset = timer.remainingTime;
                --c;
            }

            for (int i = 0; i < c; ++i)
                offset += fuelFillTime;

            return (float)offset.TotalSeconds;
        }
    }

    public float RemainingFuelTimerNextSlot
    {
        get 
        {
            int c = maxFuel - fuel;
            OnTheRunSessionsManager.Timer timer;
            OnTheRunSessionsManager.Instance.TryGetTimer(kFuelFillTimerId, out timer);
            return (float)timer.remainingTime.TotalSeconds;
        }
    }

    public TimeSpan FuelFreezeTimer
    {
        get
        {
            OnTheRunSessionsManager.Timer timer;
            if (OnTheRunSessionsManager.Instance.TryGetTimer(kFuelFreezeTimerId, out timer))
                return timer.remainingTime;
            else
                return TimeSpan.FromSeconds(-1.0);
        }
    }
    #endregion

    #region Public methods
    public bool IsFuelFreezeActive()
    {
        return OnTheRunSessionsManager.Instance.HasTimer(kFuelFreezeTimerId);
    }

    public void StartFuelFreeze()
    {
        OnTheRunSessionsManager.Instance.SetTimer(kFuelFreezeTimerId, fuelFreezeTime, true);
        OnTheRunSessionsManager.Instance.RemoveTimer(kFuelFillTimerId);

        Manager<UIRoot>.Get().ShowCurrenciesItem(true);
    }
    #endregion

    #region Unity callbacks
    new void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        maxFuel = OnTheRunDataLoader.Instance.GetFuelVisibleInBar();
        fuel = EncryptedPlayerPrefs.GetInt(kFuelKey, maxFuel);
        
#if UNITY_WEBPLAYER
        fuel = maxFuel;
#endif

        fuelFillTime = TimeSpan.FromMinutes(OnTheRunDataLoader.Instance.GetFuelSingleRefillTime());
        fuelFreezeTime = TimeSpan.FromHours(OnTheRunDataLoader.Instance.GetFuelFreezeTime());

        //Debug.Log("OnTheRunFuelManager - Start - fuelFillTime: " + fuelFillTime.ToString() + " - fuelFreezeTime: " + fuelFreezeTime.ToString());

		OnTheRunSessionsManager.Instance.onSessionEnd.AddTarget(gameObject, "FuelManagerSessionEnd");
		OnTheRunSessionsManager.Instance.onTimerExpired.AddTarget(gameObject, "OnFuelTimerExpired");
    }

    new void Start()
    {
        base.Start();
    }
    #endregion

    #region Signals
    void OnFuelTimerExpired(OnTheRunSessionsManager.Timer timer)
    {
        switch (timer.id)
        {
            case kFuelFreezeTimerId:
                OnTheRunSessionsManager.Instance.SetTimer(kFuelFillTimerId, fuelFillTime, true);

                //Manager<UIManager>.Get().ActivePage.SendMessage("OnRefreshFuelSaver", SendMessageOptions.DontRequireReceiver);
                break;
            case kFuelFillTimerId:
                ++fuel;

                DateTime now;
                bool isValid;
                DateTimeManager.Instance.GetDate(out now, out isValid);

                TimeSpan extraTime = now - timer.expireDate;
                double extraFuelTime = extraTime.TotalSeconds / fuelFillTime.TotalSeconds;
                //double extraFuelTime = 0;
                //if (fuelFillTime.TotalSeconds > 0)
                //    extraFuelTime = extraTime.TotalSeconds / fuelFillTime.TotalSeconds;
                if (extraFuelTime > 0.0)
                {
                    int extraFuel = (int)System.Math.Floor(extraFuelTime);
                    fuel = Mathf.Min(fuel + extraFuel, maxFuel); 
                    //Debug.Log("new fuel:" + fuel + ", extraFuelTime: " + extraFuelTime + ", extraTime: " + extraTime + ", extraFuel: " + extraFuel + ", now: " + now.ToString() + ", timer.expireDate: " + timer.expireDate.ToString());
                }

                if (fuel < maxFuel)
                {
                    extraFuelTime = extraFuelTime > 0.0 ? (extraFuelTime - System.Math.Floor(extraFuelTime)) : 0.0;
                    Debug.Log("frac: " + extraFuelTime + ", seconds for next fuel: " + fuelFillTime.TotalSeconds * (1.0 - extraFuelTime));
                    OnTheRunSessionsManager.Instance.SetTimer(kFuelFillTimerId, TimeSpan.FromSeconds(fuelFillTime.TotalSeconds * (1.0 - extraFuelTime)), true);
                }

                if (Manager<UIRoot>.Get() != null)
                    Manager<UIRoot>.Get().UpdateCurrenciesItem();
                break;
        }
    }

    void FuelManagerSessionEnd()
    {
        EncryptedPlayerPrefs.SetInt(kFuelKey, fuel);
    }
    #endregion
}
