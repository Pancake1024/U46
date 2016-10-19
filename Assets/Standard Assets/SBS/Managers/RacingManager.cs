using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Core;

[AddComponentMenu("SBS/Managers/Racingmanager")]
public class RacingManager : MonoBehaviour
{
    #region Singleton instance
    protected static RacingManager instance = null;

    public static RacingManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public members
    public int laps;
    public List<Checkline> finishChecklines;
    public List<VehicleRaceData> vehicles;
    public Track track;
    #endregion

    #region Public methods
    public void SendMessageToVehicles(string methodName, object value, SendMessageOptions options)
    {
        foreach (VehicleRaceData vehRaceData in vehicles)
            vehRaceData.gameObject.SendMessage(methodName, value, options);
    }

    public void SendMessageToVehicles(string methodName, object value)
    {
        foreach (VehicleRaceData vehRaceData in vehicles)
            vehRaceData.gameObject.SendMessage(methodName, value);
    }

    public void SendMessageToVehicles(string methodName)
    {
        foreach (VehicleRaceData vehRaceData in vehicles)
            vehRaceData.gameObject.SendMessage(methodName);
    }

    public void CheckFinishLine(VehicleRaceData vehRaceData, ChecklinesManager.Message msg, ref int finishlineCounter)
    {
        if (finishChecklines.IndexOf(msg.checkline) > -1)
        {
            if (msg.front)
            {
                finishlineCounter = Mathf.Max(0, finishlineCounter + 1);
                if (finishlineCounter > vehRaceData.Lap)
                    vehRaceData.gameObject.SendMessage("OnNextLap", laps);
            }
            else
            {
                finishlineCounter = Mathf.Max(0, finishlineCounter - 1);
            }
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
