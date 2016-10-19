using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;




public class InterpolatedTimeEffects : Manager<InterpolatedTimeEffects>
{

    #region Public Members
    public static InterpolatedTimeEffects Instance
    {
        get
        {
            return Manager<InterpolatedTimeEffects>.Get();
        }
    }

    #endregion

    #region Protected members

    protected List<InterpEffect> interpEffects = new List<InterpEffect>();
    protected TimeSource myTimeSource;
    protected float baseFixedDeltaTime = -1.0f;

    #endregion


    #region Protected classes
    protected class InterpEffect
    {

        public float duration;
        public float timeFactorStart;
        public float timeFactorEnd;
        private float startTime;
        private TimeSource timesource;
        private float prevFixedDeltaTime;



        public InterpEffect(float _duration, float _timeFactorStart, float _timeFactorEnd, TimeSource _timesource)
        {
            duration = _duration;
            timeFactorStart = _timeFactorStart;
            timeFactorEnd = _timeFactorEnd;
            timesource = _timesource;
            startTime = timesource.TotalTime;//TimeManager.Instance.MasterSource.TotalTime;

            prevFixedDeltaTime = Time.fixedDeltaTime;


        }

        // simple linear tweening - no easing
        //t: current time, b: beginning value, c: change in value, d: duration
        //lEffectProgressPause, lStartValuePause, lEffectSpanPause, pPauseEffect.duration )
        float InterpLinear(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }


        public bool Update()
        {
            float currentTime = timesource.TotalTime;
            float elapsedTime = currentTime - startTime;

            float timeMult = this.InterpLinear(elapsedTime, timeFactorStart, timeFactorEnd - timeFactorStart, duration);
            timeMult = Mathf.Max(0.0f, timeMult);
            TimeManager.Instance.MasterSource.TimeMultiplier = timeMult;
            Time.fixedDeltaTime = prevFixedDeltaTime * timeMult;

            //Debug.Log("timeMult" + timeMult + " elapsed:" + elapsedTime);
            if (elapsedTime >= duration)
            {
                TimeManager.Instance.MasterSource.TimeMultiplier = timeFactorEnd;
                //Time.fixedDeltaTime = prevFixedDeltaTime;
                return true;
            }
            else
                return false;
        }

    }
    #endregion


    public bool IsRallenty()
    {
        return TimeManager.Instance.MasterSource.TimeMultiplier < 1.0f;
    }

    #region Public properties

    public TimeSource TimeSource
    {
        get
        {
            return myTimeSource;
        }
    }

    #endregion

    public void AddInterpEffect(float duration, float startTimeFactor, float endTimeFactor)
    {
        //Debug.Log("ADD" + endTimeFactor);
        if (baseFixedDeltaTime < 0.0f)
            baseFixedDeltaTime = Time.fixedDeltaTime;
        interpEffects.Add(new InterpEffect(duration, startTimeFactor, endTimeFactor, myTimeSource));
    }

    public void RemoveAllInterpEffects()
    {
        TimeManager.Instance.MasterSource.TimeMultiplier = 1.0f;
        if (baseFixedDeltaTime>=0.0f)
            Time.fixedDeltaTime = baseFixedDeltaTime;
        interpEffects.Clear();
    }

    public void ReturnToNormal(float duration)
    {
        interpEffects.Clear();
        interpEffects.Add(new InterpEffect(duration, TimeManager.Instance.MasterSource.TimeMultiplier, 1.0f, myTimeSource));
    }


    #region Unity Callbacks

    new void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        myTimeSource = new TimeSource(); //TimeManager.Instance.MasterSource; // new TimeSource(); //gameObject.AddComponent<TimeSource>();//new TimeSource();
        //TimeManager.Instance.onGamePaused.AddTarget(gameObject, "OnPause");
        //TimeManager.Instance.onGameResumed.AddTarget(gameObject, "OnResume");
        TimeManager.Instance.AddSource(myTimeSource);
    }


    void OnPause()
    {
        //myTimeSource.Pause();
        //Debug.Log("PAUSEEEEEEEEEEEEEEEEEEEEEEEEEE");
    }


    void OnResume()
    {
        //myTimeSource.Resume();
    }

    void Update()
    {
        if (interpEffects.Count > 0 && !TimeManager.Instance.MasterSource.IsPaused)
        {
            bool finished = interpEffects[0].Update();
            if (finished)
                interpEffects.RemoveAt(0);
        }
    }


    #endregion


}
