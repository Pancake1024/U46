﻿using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

public class NativeDispatcher : MonoBehaviour
{
    #region Singleton instance
    protected static NativeDispatcher instance = null;

    public static NativeDispatcher Instance
    {
        get
        {
            if (null == instance)
            {
                GameObject go = new GameObject("__dispatcher", typeof(NativeDispatcher));
                GameObject.DontDestroyOnLoad(go);
                Asserts.Assert(instance != null);
            }
            return instance;
        }
    }
    #endregion

    #region Protected members
    protected Dictionary<string, Action<string>> events = new Dictionary<string, Action<string>>();
    #endregion

    #region Unity callbacks
    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }
    #endregion

    #region Messages
    void dispatch(string message)
    {
        int sepIndex = message.IndexOf('|');
        string eventName = null,
               param = null;

        if (-1 == sepIndex)
            eventName = message;
        else
        {
            eventName = message.Substring(0, sepIndex);
            param = message.Substring(sepIndex + 1);
        }

        Action<string> callback;
        if (events.TryGetValue(eventName, out callback))
            callback.Invoke(param);
    }
    #endregion

    #region Public methods
    public void RegisterEvent(string name, Action<string> callback)
    {
        events[name] = callback;
    }

    public void RemoveEvent(string name)
    {
        events.Remove(name);
    }
    #endregion
}
