using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/OnTheRunConsumableBonusManager")]
public class OnTheRunConsumableBonusManager : Manager<OnTheRunConsumableBonusManager>
{
    public Sprite coinsSpriteRef;
    public Sprite gemsSpriteRef;
    public Sprite fuelSpriteRef;
    public Sprite fuelFreezeSpriteRef;
    public Sprite freeSpinsSpriteRef;

    protected List<ConsumableBonus> listOfConsumablesBonus;
    protected int listCount = 8;

    protected int globalCounter = 0;
    protected string globalCounterId = "cbm_contr";
    protected string consumableCounterId = "cbm_cnsmbl_cnt";
    protected string consumableBaseId = "cbm_cnsmbase_";

    protected int levelInit = 10;
    protected int levelInterval = 7;
    protected int coinsInit = 1;
    protected int gemsInit = 1;
    protected int fuelInit = 1;
    protected int spinsInit = 1;
    protected float coinsMultiplier = 1.5f;
    protected float gemsMultiplier = 1.5f;
    protected float fuelMultiplier = 1.5f;
    protected float freeSpinsMultiplier = 1.5f;

    protected int diamondsForLevelUpStart = 1;
    protected int diamondsForLevelUpIncrease = 1;
    protected int diamondsForLevelUpThreshold = 10;

    #region Singleton Instance
    static public OnTheRunConsumableBonusManager Instance
    {
        get
        {
            OnTheRunConsumableBonusManager instance = Manager<OnTheRunConsumableBonusManager>.Get();
            if (instance.listOfConsumablesBonus == null)
                instance.Initialize();
            return instance;
        }
    }
    #endregion

    #region Utils
    public enum ConsumableType
    {
        None = -1,
        GemsPack = 0,
        CoinsPack,
        FuelPack,
        FreeSpinsPack,
        FuelFreeze,
        Other,
        OnlyDiamonds,
        Car,
        Count
    }
    public class ConsumableBonus
    {
        public ConsumableType type;
        public int quantity;
        public int level;
        public int alternativeReward = -1;
    }

    void Initialize()
    {
        diamondsForLevelUpStart = OnTheRunDataLoader.Instance.GetLevelUpRewardData("first_time", 1);
        diamondsForLevelUpIncrease = OnTheRunDataLoader.Instance.GetLevelUpRewardData("increase", 1);
        diamondsForLevelUpThreshold = OnTheRunDataLoader.Instance.GetLevelUpRewardData("increase_every_levels", 10);
        levelInit = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("level_init", 0);
        levelInterval = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("level_interval", 7);
        coinsInit = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("coins_quantitiy_init", 1);
        gemsInit = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("gems_quantitiy_init", 1);
        fuelInit = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("fuel_quantitiy_init", 1);
        spinsInit = (int)OnTheRunDataLoader.Instance.GetConsumableBonusesData("spins_quantitiy_init", 1);
        coinsMultiplier = OnTheRunDataLoader.Instance.GetConsumableBonusesData("coins_quantitiy_mult", 1);
        gemsMultiplier = OnTheRunDataLoader.Instance.GetConsumableBonusesData("gems_quantitiy_mult", 1);
        fuelMultiplier = OnTheRunDataLoader.Instance.GetConsumableBonusesData("fuel_quantitiy_mult", 1);
        freeSpinsMultiplier = OnTheRunDataLoader.Instance.GetConsumableBonusesData("spins_quantitiy_mult", 1);

        /*Debug.Log("--> levelInit: " + levelInit);
        Debug.Log("--> levelInterval: " + levelInterval);
        Debug.Log("--> coinsInit: " + coinsInit);
        Debug.Log("--> gemsInit: " + gemsInit);
        Debug.Log("--> fuelInit: " + fuelInit);
        Debug.Log("--> spinsInit: " + spinsInit);
        Debug.Log("--> coinsMultiplier: " + coinsMultiplier);
        Debug.Log("--> gemsMultiplier: " + gemsMultiplier);
        Debug.Log("--> fuelMultiplier: " + fuelMultiplier);
        Debug.Log("--> freeSpinsMultiplier: " + freeSpinsMultiplier);*/

        globalCounter = PlayerPrefs.GetInt(globalCounterId, 1);

        listOfConsumablesBonus = new List<ConsumableBonus>();
        int consumableCounter = PlayerPrefs.GetInt(consumableCounterId, 0);

        /*if (consumableCounter == 0)
        {
            int typeIndex = 0;
            for (int i = 0; i < listCount; ++i)
            {
                ConsumableBonus tmp = new ConsumableBonus();
                tmp.type = (ConsumableType)typeIndex;
                tmp.multCounter = 1;
                listOfConsumablesBonus.Add(tmp);

                ++typeIndex;
                if (typeIndex >= (int)ConsumableType.Count)
                    typeIndex = 0;
            }

            OnTheRunUtils.Shuffle<ConsumableBonus>(listOfConsumablesBonus);

            SaveConsumables();
        }
        else
            LoadConsumables(consumableCounter);

        for (int i = 0; i < listOfConsumablesBonus.Count; ++i)
        {
            int[] data = GetNextConsumableBonusQuantityAndLevel(listOfConsumablesBonus[i]);
            listOfConsumablesBonus[i].quantity = data[0];
            listOfConsumablesBonus[i].level = data[1];
            ++globalCounter;
        }*/

        for (int i = 0; i < OnTheRunDataLoader.Instance.GetLevelBonusCount(); ++i)
        {
            listOfConsumablesBonus.Add(GetNextConsumableBonusQuantityAndLevel(i));
        }

        //check for already bought
        for (int i = 0; i < OnTheRunDataLoader.Instance.GetLevelBonusCount(); ++i)
        {
            if(listOfConsumablesBonus[i].type==ConsumableType.Other)
            {
                int currLevel = listOfConsumablesBonus[i].level;

                int tierLocked = PlayerPersistentData.Instance.IsParkingLotLockedByLevel(currLevel);
                int specialVehicleLocked = UnlockingManager.Instance.IsCarLocked(currLevel);
                int carLocked = PlayerPersistentData.Instance.IsCarLocked(currLevel);

                if (tierLocked == 0 || specialVehicleLocked == 0 || carLocked == 0)
                    OnSpecialVehicleBought(currLevel);
            }
        }
    }


    int[] GetNextConsumableBonusQuantityAndLevel(ConsumableBonus bonus)
    {
        int currQuantity = 0;
        int currLevel = 0;
        switch (bonus.type)
        {
            case ConsumableType.CoinsPack:
                currQuantity = coinsInit + (int)(globalCounter * coinsMultiplier);
                currLevel = levelInit + globalCounter * levelInterval;
                break;
            case ConsumableType.GemsPack:
                currQuantity = gemsInit + (int)(globalCounter * gemsMultiplier);
                currLevel = levelInit + globalCounter * levelInterval;
                break;
            case ConsumableType.FuelPack:
                currQuantity = fuelInit + (int)(globalCounter * fuelMultiplier);
                currLevel = levelInit + globalCounter * levelInterval;
                break;
            case ConsumableType.FreeSpinsPack:
                currQuantity = spinsInit + (int)(globalCounter * freeSpinsMultiplier);
                currLevel = levelInit + globalCounter * levelInterval;
                break;
        }
        return new int[]{currQuantity, currLevel};
    }

    ConsumableBonus GetNextConsumableBonusQuantityAndLevel(int index)
    {
        ConsumableBonus tmp = OnTheRunDataLoader.Instance.GetLevelBonus(index);
        return tmp;
    }

    ConsumableBonus GetCurrentConsumableBonus(int playerLevel)
    {
        ConsumableBonus retVal = null;
        for (int i = 0; i < listOfConsumablesBonus.Count; ++i)
        {
            if (listOfConsumablesBonus[i].level == playerLevel)
            {
                retVal = listOfConsumablesBonus[i];
                break;
            }

        }
        return retVal;
    }

    void OnSpecialVehicleBought(int unlock_level)
    {
        for (int i = 0; i < listOfConsumablesBonus.Count; ++i)
        {
            if (listOfConsumablesBonus[i].level == unlock_level)
            {
                listOfConsumablesBonus[i] = OnTheRunDataLoader.Instance.GetLevelBonusAlternative(unlock_level);
                break;
            }

        }
    }
    #endregion

    #region Public functions
    public int GetDiamondsForLevelUp(int currLevel)
    {
        int coeff = (currLevel / diamondsForLevelUpThreshold);
        return diamondsForLevelUpStart + coeff * diamondsForLevelUpIncrease;
    }

    public Sprite GetConsumableSprite(ConsumableType type)
    {
        Sprite retValue = null;
        switch (type)
        {
            case ConsumableType.CoinsPack:
                retValue = coinsSpriteRef;
                break;
            case ConsumableType.GemsPack:
                retValue = gemsSpriteRef;
                break;
            case ConsumableType.FuelPack:
                retValue = fuelSpriteRef;
                break;
            case ConsumableType.FuelFreeze:
                retValue = fuelFreezeSpriteRef;
                break;
            case ConsumableType.FreeSpinsPack:
                retValue = freeSpinsSpriteRef;
                break;
            case ConsumableType.Car:
                retValue = freeSpinsSpriteRef;
                break;
            case ConsumableType.Other:
                List<OnTheRunRankManager.Unlockable> unlockableList = OnTheRunRankManager.Instance.UnlockableItems;
                for (int i = unlockableList.Count-1; i >= 0; --i)
                {
                    OnTheRunRankManager.Unlockable currUnlockable = unlockableList[i];
                    if (currUnlockable.rankLevel == PlayerPersistentData.Instance.Level)
                    {
                        retValue = currUnlockable.sprite;
                    }
                }
                break;
            case ConsumableType.None:
                break;
        }

        return retValue;
    }

    public List<ConsumableBonus> GetConsumableBonusList()
    {
        return listOfConsumablesBonus;
    }

    public ConsumableBonus IsConsumableBounsAvailable()
    {
        ConsumableBonus tmp = GetCurrentConsumableBonus(PlayerPersistentData.Instance.Level);
        return tmp;
    }

    public void TakeConsumableBonusReward()
    {
        ConsumableBonus currReward = GetCurrentConsumableBonus(PlayerPersistentData.Instance.Level);
        if (currReward != null)
        {
            Debug.Log("TakeConsumableBonusReward " + currReward.type);
            switch (currReward.type)
            {
                case ConsumableType.CoinsPack:
                    PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.FirstCurrency, currReward.quantity);
                    break;
                case ConsumableType.GemsPack:
                    PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, currReward.quantity);
                    break;
                case ConsumableType.FuelPack:
                    OnTheRunFuelManager.Instance.Fuel += currReward.quantity;
                    break;
                case ConsumableType.FreeSpinsPack:
                    PlayerPersistentData.Instance.ExtraSpin += currReward.quantity;
                    break;
                case ConsumableType.FuelFreeze:
                    OnTheRunFuelManager.Instance.StartFuelFreeze();
                    break;
                case ConsumableType.Other: 
                    List<OnTheRunRankManager.Unlockable> unlockableList = OnTheRunRankManager.Instance.UnlockableItems;
                    for (int i = unlockableList.Count - 1; i >= 0; --i)
                    {
                        OnTheRunRankManager.Unlockable currUnlockable = unlockableList[i];
                        if (currUnlockable.rankLevel == PlayerPersistentData.Instance.Level)
                        {
                            if (currUnlockable.category == OnTheRunRankManager.UnlockableType.Car)
                            {
                                PlayerPersistentData.Instance.GetPlayerData(PlayerPersistentData.Instance.GetCarLockedId(PlayerPersistentData.Instance.Level)).UnlockCar();
                                break;
                            }
                        }
                    }
                    break;
            }

            ++globalCounter;
            PlayerPrefs.SetInt(globalCounterId, globalCounter);

            /*int[] data = GetNextConsumableBonusQuantityAndLevel(currReward);
            currReward.quantity = data[0];
            currReward.level = data[1];*/
            //currReward = GetNextConsumableBonusQuantityAndLevel();
            Manager<UIRoot>.Get().UpdateCurrenciesItem();
            //Manager<UIManager>.Get().GetComponentInChildren<UIRewardBar>().SendMessage("RefreshSpinWheelButton", SendMessageOptions.DontRequireReceiver);
        }
    }
    #endregion

    #region Save/Load
    void LoadConsumables(int consumablesCounter)
    {
        for (int i = 0; i < consumablesCounter; ++i)
        {
            ConsumableBonus tmp = new ConsumableBonus();
            tmp.type = (ConsumableType)PlayerPrefs.GetInt(consumableBaseId + i, (int)ConsumableType.None);
            listOfConsumablesBonus.Add(tmp);
        }
    }

    void SaveConsumables()
    {
        PlayerPrefs.SetInt(consumableCounterId, listOfConsumablesBonus.Count);
        for (int i = 0; i < listOfConsumablesBonus.Count; ++i)
        {
            PlayerPrefs.SetInt(consumableBaseId + i, (int)listOfConsumablesBonus[i].type);
        }
    }
    #endregion
}
