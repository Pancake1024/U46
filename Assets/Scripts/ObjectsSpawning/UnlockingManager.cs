using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockingManager
{
    protected int specialCarsNumber = 5;
    protected int[] startLocked = { 0, 0, 0, 1, 1 };
    protected int[] specialCarsCost = { 0, 50, 100, 150, 200 };
    protected int[] alternativeCosts;
    protected int[] specialCarsXPLevels;
    protected List<SpecialCarData> specialCarsDataList;

    protected OnTheRunGameplay.CarId lastSpecialVechileUnlocked = OnTheRunGameplay.CarId.None;
    protected int forceSpawnMaxTimes;
    protected int forceSpecialVehicleCounter;

    protected string forceSpecialVehicleCounterId = "fsv_c";
    
    #region Singleton instance
    protected static UnlockingManager instance = null;

    public static UnlockingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UnlockingManager();
                instance.Init();
            }

            return instance;
        }
    }
    #endregion

    public void Init()
    {
        startLocked = OnTheRunDataLoader.Instance.GetSpecialCarsLockData();
        specialCarsCost = OnTheRunDataLoader.Instance.GetSpecialCarsPriceData();
        alternativeCosts = OnTheRunDataLoader.Instance.GetAlternativeSpecialCarsPriceData();
        specialCarsXPLevels = OnTheRunDataLoader.Instance.GetSpecialCarsXPLevelsData();
        forceSpawnMaxTimes = (int)OnTheRunDataLoader.Instance.GetSpecialCarData("force_spawn_counter");
        forceSpecialVehicleCounter = EncryptedPlayerPrefs.GetInt(forceSpecialVehicleCounterId, -1);
    }

    #region Properties
    public class SpecialCarData
    {
        public OnTheRunGameplay.CarId type;
        public int startLocked;
        public bool locked;
        public bool canBeBought;
        public int cost;
        public int alternativeCost;
        public PriceData.CurrencyType currency;
        public PriceData.CurrencyType alternativeCurrency;
        public int level;
        public int unlockAtLevel;

        public int upgradeCost
        {
            get
            {
                return OnTheRunEconomy.Instance.GetSpecialVehicleUpgradePrice(type, level + 1);
            }
        }
    }

    public int SpecialCarsNumber
    {
        get { return specialCarsNumber; }
    }

    public OnTheRunGameplay.CarId LastSpecialVehicleUnlocked
    {
        get { return lastSpecialVechileUnlocked; }
        set { lastSpecialVechileUnlocked = value; }
    }

    public bool ForceSpawnSpecialVehicleCounter()
    {
        --forceSpecialVehicleCounter;
        EncryptedPlayerPrefs.SetInt(forceSpecialVehicleCounterId, forceSpecialVehicleCounter);
        return forceSpecialVehicleCounter >= 0;
    }
    #endregion

    #region Functions
    public void UnlockSpecialCar(OnTheRunGameplay.CarId type)
    {
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].type == type)
            {
                specialCarsDataList[i].canBeBought = true;
                break;
            }
        }
    }

    public void BuySpecialCar(OnTheRunGameplay.CarId type)
    {
        OnTheRunSingleRunMissions.Instance.AddMissionSpecialVehicleConstrain(type);
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].type == type)
            {
                specialCarsDataList[i].canBeBought = false;
                specialCarsDataList[i].locked = false;
                lastSpecialVechileUnlocked = type;
                forceSpecialVehicleCounter = forceSpawnMaxTimes;
                EncryptedPlayerPrefs.SetInt(forceSpecialVehicleCounterId, forceSpecialVehicleCounter);
                OnTheRunConsumableBonusManager.Instance.SendMessage("OnSpecialVehicleBought", specialCarsDataList[i].unlockAtLevel);
                break;
            }
        }
    }

    public void UpgradeSpecialCarLevel(OnTheRunGameplay.CarId type)
    {
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].type == type)
            {
                specialCarsDataList[i].level++;
                break;
            }
        }
    }

    public SpecialCarData GetSpecialCarData(OnTheRunGameplay.CarId type)
    {
        SpecialCarData retValue = null;
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].type == type)
            {
                retValue = specialCarsDataList[i];
                break;
            }
        }
        return retValue;
    }

    public TruckBehaviour.TrasformType GetUnlockedVehicleByXP( )
    {
        TruckBehaviour.TrasformType retValue = TruckBehaviour.TrasformType.None;

        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].locked && specialCarsDataList[i].unlockAtLevel <= PlayerPersistentData.Instance.Level)
            {
                retValue = FromCarIdToTrasformType(specialCarsDataList[i].type);
                break;
            }
        }

        return retValue;
    }

    public SpecialCarData GetSpecialCarData(TruckBehaviour.TrasformType type)
    {
        OnTheRunGameplay.CarId currType = FromTrasformTypeToCarId(type);
        return GetSpecialCarData(currType);
    }

    public bool IsCarLocked(OnTheRunGameplay.CarId type)
    {
        bool retValue = true;
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].type == type)
            {
                retValue = specialCarsDataList[i].locked;
                break;
            }
        }
        return retValue;
    }

    public bool IsCarLocked(TruckBehaviour.TrasformType type)
    {
        OnTheRunGameplay.CarId currType = FromTrasformTypeToCarId(type);
        return IsCarLocked(currType);
    }

    public int IsCarLocked(int level)
    {
        int retValue = -1;
        for (int i = 0; i < specialCarsDataList.Count; ++i)
        {
            if (specialCarsDataList[i].unlockAtLevel == level)
            {
                retValue = specialCarsDataList[i].locked ? 1 : 0;
                break;
            }
        }
        return retValue;
    }

    public OnTheRunGameplay.CarId FromTrasformTypeToCarId(TruckBehaviour.TrasformType type)
    {
        OnTheRunGameplay.CarId currType = OnTheRunGameplay.CarId.None;
        switch (type)
        {
            case TruckBehaviour.TrasformType.Bigfoot: currType = OnTheRunGameplay.CarId.Bigfoot; break;
            case TruckBehaviour.TrasformType.Tank: currType = OnTheRunGameplay.CarId.Tank; break;
            case TruckBehaviour.TrasformType.Firetruck: currType = OnTheRunGameplay.CarId.Firetruck; break;
            case TruckBehaviour.TrasformType.Ufo: currType = OnTheRunGameplay.CarId.Ufo; break;
            case TruckBehaviour.TrasformType.Plane: currType = OnTheRunGameplay.CarId.Plane; break;
        }
        return currType;
    }

    public TruckBehaviour.TrasformType FromCarIdToTrasformType(OnTheRunGameplay.CarId type)
    {
        TruckBehaviour.TrasformType currType = TruckBehaviour.TrasformType.None;
        switch (type)
        {
            case OnTheRunGameplay.CarId.Bigfoot: currType = TruckBehaviour.TrasformType.Bigfoot; break;
            case OnTheRunGameplay.CarId.Tank: currType = TruckBehaviour.TrasformType.Tank; break;
            case OnTheRunGameplay.CarId.Firetruck: currType = TruckBehaviour.TrasformType.Firetruck; break;
            case OnTheRunGameplay.CarId.Ufo: currType = TruckBehaviour.TrasformType.Ufo; break;
            case OnTheRunGameplay.CarId.Plane: currType = TruckBehaviour.TrasformType.Plane; break;
        }
        return currType;
    }
    #endregion

    #region Save/Load
    public void Load()
    {
        specialCarsDataList = new List<SpecialCarData>();
        for (int i = 0; i < specialCarsNumber; ++i)
        {
            SpecialCarData currData = new SpecialCarData();
            switch (i)
            {
                case 0: currData.type = OnTheRunGameplay.CarId.Bigfoot; break;
                case 1: currData.type = OnTheRunGameplay.CarId.Tank; break;
                case 2: currData.type = OnTheRunGameplay.CarId.Firetruck; break;
                case 3: currData.type = OnTheRunGameplay.CarId.Ufo; break;
                case 4: currData.type = OnTheRunGameplay.CarId.Plane; break;
            }
            currData.startLocked = startLocked[i];
            currData.cost = specialCarsCost[i];
            currData.alternativeCost = alternativeCosts[i];
            currData.unlockAtLevel = specialCarsXPLevels[i];
#if UNITY_WEBPLAYER
            currData.currency = PriceData.CurrencyType.FirstCurrency;
#else
            currData.currency = PriceData.CurrencyType.SecondCurrency;
            currData.alternativeCurrency = PriceData.CurrencyType.FirstCurrency;
#endif
            currData.locked = EncryptedPlayerPrefs.GetInt("special_" + i + "_locked", currData.startLocked) == 1;
            currData.canBeBought = EncryptedPlayerPrefs.GetInt("special_" + i + "_bebought", 0) == 1;
            currData.level = EncryptedPlayerPrefs.GetInt("special_" + i + "_level", 0);
            specialCarsDataList.Add(currData);
        }
    }

    public void Save()
    {
        for (int i = 0; i < specialCarsNumber; ++i)
        {
            SpecialCarData currData = specialCarsDataList[i];
            EncryptedPlayerPrefs.SetInt("special_" + i + "_locked", currData.locked ? 1 : 0);
            EncryptedPlayerPrefs.SetInt("special_" + i + "_bebought", currData.canBeBought ? 1 : 0);
            EncryptedPlayerPrefs.SetInt("special_" + i + "_level", currData.level);
        }
    }
    #endregion
}
