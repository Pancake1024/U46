using SBS.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OnTheRunRankManager : Manager<OnTheRunRankManager>
{
    #region Singleton instance
    public static OnTheRunRankManager Instance
    {
        get
        {
            return Manager<OnTheRunRankManager>.Get();
        }
    }
    #endregion

    public Sprite[] tierSprites;
    public Sprite[] carSprites;
    public Sprite[] specialVehiclesSprites;

    private OnTheRunGameplay gameplayManager;

    private int[] currentRankThreshold = { 10, 50, 100 };
    private int deltaEndRun = 10;
    private int deltaCompleteMission = 10;
    private List<Unlockable> unlockablesList;
    private List<int> lockedTiers;
    protected float[] lockedTiersLevel;
    protected List<int> lockedCarsLevel;
    private List<OnTheRunGameplay.CarId> lockedCars;
    private List<OnTheRunGameplay.CarId> lockedSpecialVehicles;

    #region Enum/Struct/Classes    
    public enum UnlockableType
    {
        Tier = 0,
        Car,
        SpecialVehicle
    }

    public struct Unlockable
    {
        public UnlockableType category;
        public int rankLevel;
        public OnTheRunGameplay.CarId carId;
        public int tierIndex;
        public Sprite sprite;
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        //Initialize unlockable items
        unlockablesList = new List<Unlockable>();
        
        Load();
    }
    #endregion

    #region Getters/Setters
    public float CurrentRank
    {
        get 
        {
            float retValue = PlayerPersistentData.Instance.Level + ((float)PlayerPersistentData.Instance.Experience / (float)PlayerPersistentData.Instance.NextExperienceLevelThreshold);
            return (float)Math.Round(retValue, 2);
        }
    }
    
    public List<Unlockable> UnlockableItems
    {
        get 
        {
            UpdateUnlockableItems();
            return unlockablesList;
        }
    }
    #endregion

    #region Utils Functions
    private void CheckForLockedTiers()
    {
        lockedTiers = PlayerPersistentData.Instance.GetParkingLotLocked();
        lockedTiersLevel = OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold();
    }

    private void CheckForLockedSpecialVehicles()
    {
        if (gameplayManager==null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        lockedSpecialVehicles = new List<OnTheRunGameplay.CarId>();
        for (int i = 0; i < (int)TruckBehaviour.TrasformType.Count; ++i)
        {
            OnTheRunGameplay.CarId currCarId = OnTheRunGameplay.CarId.None;
            switch ((TruckBehaviour.TrasformType)i)
            {
                case TruckBehaviour.TrasformType.Bigfoot: currCarId = OnTheRunGameplay.CarId.Bigfoot; break;
                case TruckBehaviour.TrasformType.Tank: currCarId = OnTheRunGameplay.CarId.Tank; break;
                case TruckBehaviour.TrasformType.Firetruck: currCarId = OnTheRunGameplay.CarId.Firetruck; break;
                case TruckBehaviour.TrasformType.Ufo: currCarId = OnTheRunGameplay.CarId.Ufo; break;
                case TruckBehaviour.TrasformType.Plane: currCarId = OnTheRunGameplay.CarId.Plane; break;
            }

            UnlockingManager.SpecialCarData data = UnlockingManager.Instance.GetSpecialCarData(currCarId);
            if (data.locked)
                lockedSpecialVehicles.Add(currCarId);
        }
    }

    private void CheckForLockedCars()
    {
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        lockedCars = new List<OnTheRunGameplay.CarId>();
        lockedCarsLevel = new List<int>();
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            GameObject[] currentCarsList = gameplayManager.GetCarsFromIndex(i);
            OnTheRunGameplay.CarId currentCarId = currentCarsList[2].GetComponent<PlayerKinematics>().carId;
            PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
            if (!playerData.owned)
            {
                lockedCars.Add(currentCarId);
                lockedCarsLevel.Add(playerData.unlockAtLevel);
            }
        }
    }
    #endregion

    #region Update Functions
    private void UpdateUnlockableItems()
    {
        CheckForLockedTiers();
        CheckForLockedCars();
        CheckForLockedSpecialVehicles();

        int listLenght = lockedCars.Count + lockedSpecialVehicles.Count + lockedTiers.Count;

        Unlockable currentUnlockable;
        for (int i = 0; i < lockedTiers.Count; ++i)
        {
            currentUnlockable = new Unlockable();
            currentUnlockable.category = UnlockableType.Tier;
            currentUnlockable.tierIndex = lockedTiers[i];
            currentUnlockable.rankLevel = (int)lockedTiersLevel[lockedTiers[i]];
            int spriteIndex = lockedTiers[i];
            currentUnlockable.sprite = tierSprites[spriteIndex];
            unlockablesList.Add(currentUnlockable);
        }

        for (int i = 0; i < lockedCars.Count; ++i)
        {
            currentUnlockable = new Unlockable();
            currentUnlockable.category = UnlockableType.Car;
            currentUnlockable.carId = lockedCars[i];
            currentUnlockable.rankLevel = lockedCarsLevel[i];
            currentUnlockable.sprite = carSprites[i];
            unlockablesList.Add(currentUnlockable);
        }

        for (int i = 0; i < lockedSpecialVehicles.Count; ++i)
        {
            currentUnlockable = new Unlockable();
            currentUnlockable.category = UnlockableType.SpecialVehicle;
            UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(lockedSpecialVehicles[i]);
            currentUnlockable.carId = currSpecialCarData.type;
            currentUnlockable.rankLevel = currSpecialCarData.unlockAtLevel;
            int spriteIndex = (int)UnlockingManager.Instance.FromCarIdToTrasformType(currentUnlockable.carId);
            currentUnlockable.sprite = specialVehiclesSprites[spriteIndex];
            unlockablesList.Add(currentUnlockable);
        }
        
        //order list
        unlockablesList.Sort(
            delegate(Unlockable data1, Unlockable data2)
            {
                return data1.rankLevel.CompareTo(data2.rankLevel);
            });
    }
    #endregion

    #region Load/Save
    public void Load()
    {
    }

    public void Save()
    {
    }
    #endregion
}
