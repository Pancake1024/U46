using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

[AddComponentMenu("SBS/Managers/TimeManager")]
public class TimeManager : MonoBehaviour
{
    #region Singleton instance
    protected static TimeManager instance = null;

    public static TimeManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Protected members
    public bool managefocus = true;
    #endregion

    #region Protected members
    protected bool appIsPaused;
    protected bool appIsFocused;
    protected TimeSource masterSource;
    protected List<TimeSource> sources;
    #endregion

    #region Public properties
    public TimeSource MasterSource
    {
        get
        {
            return masterSource;
        }
        set
        {
            if (masterSource != null && value != null)
            {
                if (masterSource.IsPaused && !value.IsPaused)
                    this.OnGamePaused();
                else if (!masterSource.IsPaused && value.IsPaused)
                    this.OnGameResumed();
            }

            masterSource = value;
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        appIsPaused = false;
        appIsFocused = true;

        masterSource = new TimeSource();

        sources = new List<TimeSource>();
        sources.Add(masterSource);
    }

    void OnApplicationPause(bool pause)
    {
        bool wasPaused = appIsPaused;
        appIsPaused = pause;

        if (!wasPaused && pause && masterSource != null)// && !masterSource.IsPaused)
            masterSource.Pause();
        if (wasPaused && !pause && masterSource != null)// && masterSource.IsPaused)
            masterSource.Resume();
    }

#if !UNITY_EDITOR
    void OnApplicationFocus(bool focus)
    {
        if (Application.runInBackground || Application.platform == RuntimePlatform.OSXWebPlayer || !managefocus || Application.platform == RuntimePlatform.Android)
            return;

        bool wasFocused = appIsFocused;
        appIsFocused = focus;

        if (wasFocused && !focus && masterSource != null)// && !masterSource.IsPaused)
            masterSource.Pause();
        if (!wasFocused && focus && masterSource != null)// && masterSource.IsPaused)
            masterSource.Resume();
    }
#endif


    void Update()
    {
        foreach (TimeSource source in sources)
            source.Update();
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion

    #region Public methods
    public void AddSource(TimeSource source)
    {
        sources.Add(source);
    }

    public void RemoveSource(TimeSource source)
    {
        sources.Remove(source);
    }

    public void OnGamePaused()
    {
        if (LevelRoot.Instance != null)
            LevelRoot.Instance.Root.BroadcastMessage("OnPause", SendMessageOptions.DontRequireReceiver);
    }

    public void OnGameResumed()
    {
        if (LevelRoot.Instance != null)
            LevelRoot.Instance.Root.BroadcastMessage("OnResume", SendMessageOptions.DontRequireReceiver);
    }
    #endregion
}
