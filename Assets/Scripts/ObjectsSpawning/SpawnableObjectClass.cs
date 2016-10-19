using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;

[System.Serializable]
public class SpawnableObjectClass
{
    #region Public Members
    public float startMinSpawnDistance;
    public float startMaxSpawnDistance;
    public float endMinSpawnDistance;
    public float endMaxSpawnDistance;
    public int priority;
    public OnTheRunObjectsPool.ObjectType type;
    public bool alwaysSpawn = false;
    public bool avoidSpecialEvents = false;
    #endregion

    #region Protected Members
    protected float newObjectDistance = 0.0f;
    #endregion

    public float NextSpawnObjectDistance
    {
        get { return newObjectDistance; }
        set { newObjectDistance = value;  }
    }

    /*public enum ObjectType
    {
        CAR = 0,
        BONUS,
        CHECKPOINT
    }*/
    
    public SpawnableObjectClass()
    {}

    public SpawnableObjectClass(OnTheRunObjectsPool.ObjectType _type, int _priority, float _startMinSpawnDistance, float _startMaxSpawnDistance, float _endMinSpawnDistance, float _endMaxSpawnDistance)
    {
        type = _type;
        newObjectDistance = -1.0f;
        priority = _priority;
        startMinSpawnDistance = _startMinSpawnDistance;
        startMaxSpawnDistance = _startMaxSpawnDistance;
        endMinSpawnDistance = _endMinSpawnDistance;
        endMaxSpawnDistance = _endMaxSpawnDistance;
    }

    public void Reset()
    {
        newObjectDistance = -1.0f;
    }
}