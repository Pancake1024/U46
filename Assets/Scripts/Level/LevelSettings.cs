using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("OnTheRun/Level/LevelSettings")]
public class LevelSettings : MonoBehaviour
{
    #region Protected members
    #endregion

    #region Public members
    public float playerSpeed = 30.0f;
    public float playerMaxEnergy = 100.0f;

    protected float maxPlayerSpeed = 50.0f;
    #endregion

    #region Public properties
    public float MaxPlayerSpeed
    {
        get { return maxPlayerSpeed; }
    }
    #endregion

    #region Public methods
    #endregion

    #region Protected methods
    #endregion

    #region Messages
    #endregion

    #region Unity Callbacks
    void Awake()
    { }

    void Start()
    { }

    void Update()
    { }
    #endregion
}
