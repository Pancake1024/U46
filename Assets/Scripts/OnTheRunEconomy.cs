using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.XML;
using System.Collections;
using System.IO;
using Ionic.Zip;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

public class OnTheRunEconomy : MonoBehaviour
{
    protected int avgCoinsForRun;
    protected int avgXPForRun;

    #region Singleton instance
    protected static OnTheRunEconomy instance = null;

    public static OnTheRunEconomy Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        DontDestroyOnLoad(gameObject);

        avgCoinsForRun = OnTheRunDataLoader.Instance.GetAvgCoinsForRun();
        avgXPForRun = OnTheRunDataLoader.Instance.GetAvgXPForRun();
    }
    #endregion

    #region Classes
    public class MissionEconomyData
    {
        public int cost;
        public int rewardXP;
    }
    #endregion

    #region Functions
    public MissionEconomyData GetMissionData(int index)
    {
        MissionEconomyData data = new MissionEconomyData();
        data.cost = avgCoinsForRun + index * OnTheRunDataLoader.Instance.GetMissionsCostData();
        data.rewardXP = avgXPForRun + index * OnTheRunDataLoader.Instance.GetMissionsXpData();
        return data;
    }

    public float GetBoostPrice(OnTheRunBooster.BoosterType type)
    {
        float retValue = 0.0f;

#if UNITY_WEBPLAYER
        if (type == OnTheRunBooster.BoosterType.MultiplePack || type == OnTheRunBooster.BoosterType.SurprisePack)
            retValue = OnTheRunDataLoader.Instance.GetBoostPrice(type);
        else
            retValue = avgCoinsForRun * OnTheRunDataLoader.Instance.GetBoostPrice(type);
#else
        if (type == OnTheRunBooster.BoosterType.DoubleCoins || type == OnTheRunBooster.BoosterType.DoubleExp || type == OnTheRunBooster.BoosterType.MultiplePack || type == OnTheRunBooster.BoosterType.SurprisePack)
            retValue = OnTheRunDataLoader.Instance.GetBoostPrice(type);
        else
            retValue = avgCoinsForRun * OnTheRunDataLoader.Instance.GetBoostPrice(type);
#endif
        return retValue;
    }

    public int GetSpecialVehicleUpgradePrice(OnTheRunGameplay.CarId carId, int level)
    {
        float basePow = 1.0f;
        switch (carId)
        {
            case OnTheRunGameplay.CarId.Bigfoot: basePow = OnTheRunDataLoader.Instance.GetSpecialCarsUpgradeData()[0]; break;
            case OnTheRunGameplay.CarId.Tank: basePow = OnTheRunDataLoader.Instance.GetSpecialCarsUpgradeData()[1]; break;
            case OnTheRunGameplay.CarId.Firetruck: basePow = OnTheRunDataLoader.Instance.GetSpecialCarsUpgradeData()[2]; break;
            case OnTheRunGameplay.CarId.Ufo: basePow = OnTheRunDataLoader.Instance.GetSpecialCarsUpgradeData()[3]; break;
            case OnTheRunGameplay.CarId.Plane: basePow = OnTheRunDataLoader.Instance.GetSpecialCarsUpgradeData()[4]; break;
        }
        float factor = Mathf.Pow(basePow, level - 1);
        float val = avgCoinsForRun * level * 2 * factor + avgCoinsForRun / 1.5f;
        return Mathf.CeilToInt((int)(Mathf.Ceil(val / 100.0f) * 100.0f));
    }

    public int GetSaveMeCost(UISaveMePopup.SaveMeButtonType type)
    {
#if UNITY_WEBPLAYER
        float cost = OnTheRunDataLoader.Instance.GetSaveMeData(type)[0];
        return (int)(cost);
#else
        float multiplier = OnTheRunDataLoader.Instance.GetSaveMeData(type)[0];
        return (int)(avgCoinsForRun * multiplier);
#endif
    }

    public int GetCarUpgradeCost(PlayerPersistentData.PlayerData carData, string type)
    {
        int tier = 1;
        string carIDString = carData.carId.ToString();
        if (carIDString.Contains("oriental"))
            tier = 2;
        else if (carIDString.Contains("american"))
            tier = 3;
        else if (carIDString.Contains("muscle"))
            tier = 4;
        
        int carLevel = 1;
        float multiplier = 0.0f;
        switch(type)
        {
            case "acceleration":
                multiplier = OnTheRunDataLoader.Instance.GetCarUpgradeMultiplierData(carData.carId.ToString()).acceleration_mult;
                carLevel = carData.acceleration + 1;
                break;
            case "maxSpeed":
                multiplier = OnTheRunDataLoader.Instance.GetCarUpgradeMultiplierData(carData.carId.ToString()).maxSpeed_mult;
                carLevel = carData.maxSpeed + 1;
                break;
            case "resistance":
                multiplier = OnTheRunDataLoader.Instance.GetCarUpgradeMultiplierData(carData.carId.ToString()).resistance_mult;
                carLevel = carData.resistance + 1;
                break;
            case "turboSpeed":
                multiplier = OnTheRunDataLoader.Instance.GetCarUpgradeMultiplierData(carData.carId.ToString()).turbo_speed_mult;
                carLevel = carData.turboSpeed + 1;
                break;
        }


        float val = avgCoinsForRun * tier * (Mathf.Pow(multiplier, (carLevel/1.5f)-1.0f));
        int finalPrice = Mathf.CeilToInt((int)(Mathf.Ceil(val / 100.0f) * 100.0f)) + avgCoinsForRun;

        float discount = OnTheRunDataLoader.Instance.GetCarsUpgradesDiscount(carData.carId.ToString());
        if (discount>0.0f)
        {
            finalPrice = Mathf.CeilToInt(finalPrice - ((finalPrice / 100.0f) * discount));
            finalPrice = Mathf.CeilToInt((int)(Mathf.Ceil(finalPrice / 50.0f) * 50.0f));
        }

        return finalPrice;
    }
    #endregion
}
