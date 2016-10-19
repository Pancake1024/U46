using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Globalization;

public class PlayerPersistentData
{
    public bool alternativeSlipstreamModeActive = true;

    #region Singleton instance
    protected static PlayerPersistentData instance = null;

    public static PlayerPersistentData Instance
    {
        get
        {
            if(instance == null)
                instance = new PlayerPersistentData();

            return instance;
        }
    }
    #endregion

	private OnTheRunInterfaceSounds interfaceSounds = null;

    #region Protected Members
    protected GameObject gameplayGameObject;
    protected DateTime lastValidDate = new DateTime(0);
    protected GameObject gameplayManagers;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunEnvironment environmentManager;
    protected Dictionary<OnTheRunGameplay.CarId, PlayerData> playerDataList;
    protected bool firstTimePlaying = true;
    protected bool firstTimeFuelFinished = true;
    protected int startPlayerLevel = 1;
    protected int playerLevel = 1;
    protected int playerExperiencePoints = 0;
    protected int nextLevelExperienceThreshold = 0;
    protected int[] prevBestMeters;
    protected int[] bestMeters;
    protected int[] bestScore;
    protected float coins = 0.0f; //9999.0f;
    protected int diamonds = 0;//9999;
    protected int extraSpin = 0;//9999;
    /*protected float maxFuelSlots = 6.0f;
    protected float fuel = 6.0f; //100.0f;
    protected float fuelTimer;
    protected float fuelUnitDeltaSeconds = 420.0f;
    protected int fuelUnitForTimePassed = 1;*/
    protected int[] parkingLotLocked = { 0, 1, 1, 1 };

    protected int[] garageCarSelection = { 0, 0, 0, 0, 0 };
    protected int garageCategorySelection = 0;
    protected int specialCarsSelection = 0;

    protected float[] parkingLotCost = { 0, 0, 50, 150 };
    #endregion
    
    #region Public Properties
    public class PlayerData
    {
        public OnTheRunGameplay.CarId carId;
        public int acceleration;
        public int maxSpeed;
        public int resistance;
        public int turboSpeed;
        public bool locked;
        public bool lockedByDaily;
        public bool canBeBought;
        public bool owned;
        public int cost;
        public int alternativeCost;
        public int rentCost;
        public int unlockAtLevel;
        public PriceData.CurrencyType currency;
        public PriceData.CurrencyType alternativeCurrency;

        public PlayerData(OnTheRunGameplay.CarId _carId, int _acceleration, int _maxSpeed, int _resistance, int _turboSpeed, bool _owned, bool _lockedByDaily, bool _canBeBought, int _unlockAtLevel, int _carCost, int _alternativeCost, PriceData.CurrencyType buyCurrency, PriceData.CurrencyType _alternativeCurrency)
        {
            acceleration = _acceleration;
            maxSpeed = _maxSpeed;
            resistance = _resistance;
            turboSpeed = _turboSpeed;
            owned = _owned;
            unlockAtLevel = _unlockAtLevel;
            locked = PlayerPersistentData.Instance.playerLevel < unlockAtLevel;
            lockedByDaily = _lockedByDaily;
            canBeBought = _canBeBought;
            rentCost = 400;
            cost = _carCost;
            alternativeCost = _alternativeCost;
            currency = buyCurrency;
            alternativeCurrency = _alternativeCurrency;
            carId = _carId;
        }

        public void UnlockCar()
        {
            Debug.Log("***** UnlockCar " + carId);
            canBeBought = true;
            lockedByDaily = false;
            PlayerPersistentData.Instance.Save();
            PlayerPersistentData.Instance.SaveCars();
        }

        public void BuyCar()
        {
            owned = true;
            lockedByDaily = false;
            PlayerPersistentData.Instance.Save();
            PlayerPersistentData.Instance.SaveCars();

            if (unlockAtLevel>0)
                OnTheRunConsumableBonusManager.Instance.SendMessage("OnSpecialVehicleBought", unlockAtLevel);
        }

        public void UpdateStatus()
        {
            locked = PlayerPersistentData.Instance.playerLevel < unlockAtLevel && !owned;
        }

		public int GetStatAmountByIndex( int iIndex )
		{
			switch (iIndex)
			{
				case 0: return acceleration;
				case 1: return maxSpeed;
				case 2: return resistance;
				case 3: return turboSpeed;

				default : return acceleration;
			}
		}
    }

    public int Level
    {
        set { playerLevel = value; }
        get { return playerLevel; }
    }

    public int Experience
    {
        set { playerExperiencePoints = value; }
        get { return playerExperiencePoints; }
    }

    public int NextExperienceLevelThreshold
    {
        get { return nextLevelExperienceThreshold; }
    }

    public float CurrentExperiencePerc
    {
        get { return (float)playerExperiencePoints / (float)nextLevelExperienceThreshold; }
    }

    public float Coins
    {
        get { return coins; }
        set { coins = value; }
    }

    public int Diamonds
    {
        get { return diamonds; }
        set { diamonds = value;
#if UNITY_WEBPLAYER
        Debug.LogError("Diamonds set in webplayer version!");
#endif
        }
    }

    public int ExtraSpin
    {
        get { return extraSpin; }
        set { extraSpin = value; }
    }

    public int SelectedGarageCategory
    {
        get { return garageCategorySelection; }
        set { garageCategorySelection = value; }
    }

    public int[] SelectedGarageCars
    {
        get { return garageCarSelection; }
        set { garageCarSelection = value; }
    }

    public int SelectedSpecialCar
    {
        get { return specialCarsSelection; }
        set { specialCarsSelection = value; }
    }

    public Dictionary<OnTheRunGameplay.CarId, PlayerData> CarList
    {
        get { return playerDataList; }
    }

    public bool FirstTimePlaying
    {
        get { return firstTimePlaying; }
        set { firstTimePlaying = value; }
    }

    public bool FirstTimeFuelFinished
    {
        get { return firstTimeFuelFinished; }
    }
    #endregion

    #region Ctor
    public PlayerPersistentData()
    {
        Asserts.Assert(null == instance);
        instance = this;
        
        playerDataList = new Dictionary<OnTheRunGameplay.CarId, PlayerData>();

        SetupManagers();
    }

    protected void SetupManagers( )
    {
        if (environmentManager == null)
        {
            gameplayGameObject = GameObject.FindGameObjectWithTag("GameplayManagers");
            if (gameplayGameObject != null)
            {
                gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
                environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
                gameplayManagers = GameObject.FindGameObjectWithTag("GameplayManagers");
            }
        }
    }

    public void AddCarData(int upgradeCost)
    {
        Debug.Log("upgradeCost: " + upgradeCost);
    }
    #endregion
    
    #region Rent Car
    /*
    protected List<RentCarData> rentedCar;

    public class RentCarData
    {
        public OnTheRunGameplay.CarId carId;
        public DateTime expirationDate;

        public RentCarData(OnTheRunGameplay.CarId _carId, DateTime _expirationDate)
        {
            carId = _carId;
            expirationDate = _expirationDate;
        }
    }

    public bool TryRentCar(OnTheRunGameplay.CarId carId, int cost, PriceData.CurrencyType currency)
    {
        if (currency == PriceData.CurrencyType.FirstCurrency)
        {
            if (coins >= cost)
            {
                coins -= cost;
                Manager<UIRoot>.Get().UpdateCurrenciesItem();
                return true;
            }
        }
        else
        {
            if (diamonds >= cost)
            {
                diamonds -= cost;
                Manager<UIRoot>.Get().UpdateCurrenciesItem();
                return true;
            }
        }

        return false;
    }

    public bool IsCarRented(OnTheRunGameplay.CarId carId)
    {
        bool retValue = false;
        for (int i = 0; i < rentedCar.Count; ++i)
        {
            if (rentedCar[i].carId == carId)
            {
                retValue = true;
                break;
            }
        }

        return retValue;
    }

    public void SetupRentCar(DateTime now)
    {
        rentedCar = new List<RentCarData>();
        int rentedCarNumber = EncryptedPlayerPrefs.GetInt("rentCarNumber", 0);
        for(int i=0; i<rentedCarNumber; ++i)
        {
            OnTheRunGameplay.CarId currCarId = (OnTheRunGameplay.CarId)EncryptedPlayerPrefs.GetInt("rent_carId_" + i, 0);
            DateTime expirationDate = DateTime.Parse(EncryptedPlayerPrefs.GetString("rent_carDate_" + i, string.Empty));
            if(now.CompareTo(expirationDate)<0)
                rentedCar.Add(new RentCarData(currCarId, expirationDate));
        }
    }

    public void ResetRentCar(OnTheRunGameplay.CarId carId)
    {
        for (int i = rentedCar.Count - 1; i >= 0; --i)
        {
            if (rentedCar[i].carId == carId)
            {
                rentedCar.RemoveAt(i);
                Manager<UIManager>.Get().ActivePage.SendMessage("OnRefreshRentedCars");
            }
        }
    }

    public void SaveRentCar( )
    {
        EncryptedPlayerPrefs.SetInt("rentCarNumber", rentedCar.Count);
        for (int i = 0; i < rentedCar.Count; ++i)
        {
            EncryptedPlayerPrefs.SetInt("rent_carId_" + i, (int)rentedCar[i].carId);
            EncryptedPlayerPrefs.SetString("rent_carDate_" + i, rentedCar[i].expirationDate.ToString());
        }
    }

    public void StartRentCar(OnTheRunGameplay.CarId carId)
    {
        DateTime now = iOSUtils.GetNetworkDate();
        if (iOSUtils.IsNetworkDateValid)
        {
            DateTime expirationDate = now.AddHours(24.0f);
            rentedCar.Add(new RentCarData(carId, expirationDate));
        }
    }

    public void RefreshRentCar( )
    {
        DateTime now = iOSUtils.GetNetworkDate();
        if (iOSUtils.IsNetworkDateValid)
        {
            for (int i = rentedCar.Count - 1; i >= 0; --i)
            {
                if (now.CompareTo(rentedCar[i].expirationDate) > 0)
                {
                    rentedCar.RemoveAt(i);
                    Manager<UIManager>.Get().ActivePage.SendMessage("OnRefreshRentedCars", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    public TimeSpan RentCarTimer(OnTheRunGameplay.CarId carId)
    {
        TimeSpan retValue = new TimeSpan();
        DateTime now = iOSUtils.GetNetworkDate();
        if (iOSUtils.IsNetworkDateValid)
        {
            for (int i = 0; i < rentedCar.Count; ++i)
            {
                if (rentedCar[i].carId == carId)
                {
                    retValue = rentedCar[i].expirationDate.Subtract(now);
                    break;
                }
            }
        }

        return retValue;
    }
    */
    #endregion
    
    #region Experience Functions
    protected int firstXPThreshold = 5000;
    protected int baseXPThreshold = 10000;
    protected int deltaXPThreshold = 2000;
    //protected int maxXPThreshold = 40000;

    public int GetExperienceThresholdByLevel(int currLevel)
    {
        if (currLevel == startPlayerLevel)
            return firstXPThreshold;
        else
            return baseXPThreshold + ((currLevel - 1) - startPlayerLevel) * deltaXPThreshold;
    }

    void SetNextLevelExperienceThreshold( )
    {
        nextLevelExperienceThreshold = GetExperienceThresholdByLevel(playerLevel);
        /*if (playerLevel == startPlayerLevel)
        {
            nextLevelExperienceThreshold = firstXPThreshold;
            //Debug.Log("PLAYER XP LEVEL START: now level -> " + playerLevel + "; next threshold -> " + nextLevelExperienceThreshold);
        }
        else
        {
            nextLevelExperienceThreshold = baseXPThreshold + (playerLevel - startPlayerLevel - 1) * deltaXPThreshold;
            nextLevelExperienceThreshold = Mathf.Clamp(nextLevelExperienceThreshold, 0, maxXPThreshold);
            //Debug.Log("PLAYER XP LEVEL INCREASED: now level -> " + playerLevel + "; next threshold -> " + nextLevelExperienceThreshold);
        }*/
    }

    public bool WillRankUpAfterUpdate(int pointToAdd)
    {
        return (playerExperiencePoints + pointToAdd * gameplayManager.ExperienceMultiplier) >= nextLevelExperienceThreshold;
    }

    public void UpdateExperiencePoints(int pointToAdd, bool canCheckForLevelPassedPopup=true)
    {
        playerExperiencePoints += pointToAdd;
        
        if (playerExperiencePoints >= nextLevelExperienceThreshold)
        {
            //Debug.Log("***** UpdateExperiencePoints " + playerExperiencePoints + " " + nextLevelExperienceThreshold);
            //LEVEL PASSED
            ++playerLevel;
            
            //playerExperiencePoints = 0;
            playerExperiencePoints = playerExperiencePoints - nextLevelExperienceThreshold;
            SetNextLevelExperienceThreshold();

            if (canCheckForLevelPassedPopup)
            {
                Manager<UIRoot>.Get().PauseExperienceBarAnimation(true);
                OnTheRunUITransitionManager.Instance.OpenPopup("RankFeedbackPopup");
                string newLevelReachedText = OnTheRunDataLoader.Instance.GetLocaleString("level") + " " + playerLevel;// +" " + OnTheRunDataLoader.Instance.GetLocaleString("reached");
                UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;
                frontPopup.GetComponent<UIRankFeedbackPopup>().SetText(newLevelReachedText, OnTheRunDataLoader.Instance.GetLocaleString("you_win"), OnTheRunConsumableBonusManager.Instance.GetDiamondsForLevelUp(playerLevel).ToString());
                frontPopup.SendMessage("StartFireworks");
                if (playerLevel==10)
                    OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.LEVEL_VETERAN);

                if (OnTheRunOmniataManager.Instance != null)
                    OnTheRunOmniataManager.Instance.TrackLevelUp();
            }
        }

        List<McSocialApiUtils.DataToStore> dataTostore = new List<McSocialApiUtils.DataToStore>();
        
        if (playerLevel > McSocialApiManager.Instance.lastStoredLevel)
        {
            dataTostore.Add(new McSocialApiUtils.DataToStore(OnTheRunMcSocialApiData.Instance.levelPropertyName, playerLevel, true));
            McSocialApiManager.Instance.lastStoredLevel = playerLevel;
        }

        int currExperience = PlayerPersistentData.Instance.Experience;
        if (currExperience > McSocialApiManager.Instance.lastStoredExperience)
        {
            dataTostore.Add(new McSocialApiUtils.DataToStore(OnTheRunMcSocialApiData.Instance.experiencePropertyName, currExperience, true));
            McSocialApiManager.Instance.lastStoredExperience = currExperience;
        }

        if (dataTostore.Count > 0)
            McSocialApiManager.Instance.StoreData(dataTostore);
    }
    /*
    public void UpdateFuelTime(float deltaTime)
    {
        if (fuel < maxFuelSlots)
        {
            fuelTimer -= deltaTime;
            if (fuelTimer < 0.0f)
            {
                fuel += fuelUnitForTimePassed;
                fuelTimer = fuelUnitDeltaSeconds;
                Manager<UIRoot>.Get().UpdateCurrenciesItem();
            }
        }
    }*/
    #endregion

    #region Functions
    public List<int> GetParkingLotLocked( )
    {
        List<int> retValue = new List<int>();
        for (int i = 0; i < parkingLotLocked.Length; i++)
        {
            if (IsParkingLotLocked(i))
                retValue.Add(i);
        }

        return retValue;
    }

    public int GetParkingLotCost(int index)
    {
        return (int)parkingLotCost[index];
    }

    public bool IsParkingLotLocked(int index)
    {
        return parkingLotLocked[index] == 1;
    }

    public int IsParkingLotLockedByLevel(int level)
    {
        int retValue = -1;
        float[] playerLevelToUnlockTiers = OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold();
        for (int i = 0; i < playerLevelToUnlockTiers.Length; i++)
        {
            if (playerLevelToUnlockTiers[i] == level)
            {
                retValue = parkingLotLocked[i];
                break;
            }
        }

        return retValue;
    }

    public bool IsParkingLotLockedEnv(OnTheRunEnvironment.Environments env)
    {
        return parkingLotLocked[gameplayManager.EnvironmentIdx(env)] == 1;
    }

    public void UnlockParkingLot(int index)
    {
        float[] playerLevelToUnlockTiers = OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold();
        OnTheRunConsumableBonusManager.Instance.SendMessage("OnSpecialVehicleBought", playerLevelToUnlockTiers[index]);
        OnTheRunSingleRunMissions.Instance.AddMissionTierConstrain((OnTheRunEnvironment.Environments)index);
        OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.NEW_SCENARIO_UNLOCKED);
        parkingLotLocked[index] = 0;
        EncryptedPlayerPrefs.SetInt("csc_advise", 0);
    }

    public PlayerData GetPlayerData(OnTheRunGameplay.CarId carId)
    {
        return playerDataList[carId];
    }

    public void IncrementCurrency(PriceData.CurrencyType currency, float value)
    {
        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency: coins += value; break;
            case PriceData.CurrencyType.SecondCurrency: diamonds += (int)value; break;
        }

		this.Save();
    }

    public bool CanAfford(PriceData.CurrencyType currency, float cost)
    {
        bool retValue = false;
        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency: retValue = coins >= cost; break;
            case PriceData.CurrencyType.SecondCurrency: retValue = diamonds >= cost; break;
        }

        return retValue;
    }

    public void BuyItem(PriceData.CurrencyType currency, float cost, bool showPopup = false, string titleText = "", string descriptionText = "", UIBuyFeedbackPopup.ItemBought itemBougth = UIBuyFeedbackPopup.ItemBought.Other, string itemName = "")
    {
		if( interfaceSounds == null )
			interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

		interfaceSounds.Buy();

        switch (currency)
        {
            case PriceData.CurrencyType.FirstCurrency: coins -= cost; break;
            case PriceData.CurrencyType.SecondCurrency: diamonds -= (int)cost; break;
        }

        //--------------------------------
        if (showPopup)
        {
            OnTheRunUITransitionManager.Instance.OpenBuyPopup(titleText, descriptionText);
            UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;
            if (frontPopup != null && frontPopup.name.Equals("SingleButtonFeedbackPopup") && frontPopup.GetComponent<UIBuyFeedbackPopup>() != null)
            {
                frontPopup.GetComponent<UIBuyFeedbackPopup>().SetBoughtItem(itemBougth);
                frontPopup.GetComponent<UIBuyFeedbackPopup>().SetFeedItemName(itemName);
            }
        }
    }

    public int GetBestMeters()
    {
        SetupManagers();
        return bestMeters[(int)environmentManager.currentEnvironment];
    }

    public int GetPrevBestMeters()
    {
        SetupManagers();
        return prevBestMeters[(int)environmentManager.currentEnvironment];
    }
    
    public int GetBestMeters(OnTheRunEnvironment.Environments env)
    {
        return bestMeters[(int)env];
    }

    public int GetBestScore()
    {
        SetupManagers();
        return bestScore[(int)environmentManager.currentEnvironment];
    }

    public int GetBestScore(OnTheRunEnvironment.Environments env)
    {
        return bestScore[(int)env];
    }

    public void SetBestMeters(int newBestMeters)
    {
        SetupManagers();
        if (bestMeters[(int)environmentManager.currentEnvironment] < newBestMeters)
        {
            prevBestMeters[(int)environmentManager.currentEnvironment] = bestMeters[(int)environmentManager.currentEnvironment];
            bestMeters[(int)environmentManager.currentEnvironment] = newBestMeters;
        }
    }

    public void SetBestScore(int newBestScore)
    {
        SetupManagers();
        if (bestScore[(int)environmentManager.currentEnvironment] < newBestScore)
            bestScore[(int)environmentManager.currentEnvironment] = newBestScore;
    }

    public void Reset()
    {
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            prevBestMeters[i] = 0;
            bestMeters[i] = 0;
            bestScore[i] = 0;
        }
    }

    public int IsCarLocked(int unlock_level)
    {
        int retValue = -1;
        for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber + 3; ++i)
        {
            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            PlayerData currData = playerDataList[(OnTheRunGameplay.CarId)i];
            if(currData.unlockAtLevel==unlock_level)
            {
                retValue = currData.canBeBought ? 0 : 1;
                break;
            }
        }
        return retValue;
    }

    public OnTheRunGameplay.CarId GetCarLockedId(int unlock_level)
    {
        OnTheRunGameplay.CarId retValue = OnTheRunGameplay.CarId.None;
        for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber + 3; ++i)
        {
            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            PlayerData currData = playerDataList[(OnTheRunGameplay.CarId)i];
            if (currData.unlockAtLevel == unlock_level)
            {
                retValue = currData.carId;
                break;
            }
        }
        return retValue;
    }
    #endregion

    #region Load/Save
    public void LoadSpecialCarSelection()
    {
        specialCarsSelection = EncryptedPlayerPrefs.GetInt("scsel", specialCarsSelection);
    }

    public void SaveSpecialCarSelection(int selectedSpecialCar)
    {
        specialCarsSelection = selectedSpecialCar;
        EncryptedPlayerPrefs.SetInt("scsel", selectedSpecialCar);
    }

    public void SaveFirstTimePlayed( )
    {
        firstTimePlaying = false;
        EncryptedPlayerPrefs.SetInt("ft_played", 0);
    }

    public void SaveFirstTimeFuelFinished()
    {
        firstTimeFuelFinished = false;
        EncryptedPlayerPrefs.SetInt("ft_fuel_finished", 0);
    }

    public void LoadGarageSelection()
    {
        garageCategorySelection = EncryptedPlayerPrefs.GetInt("selcat", garageCategorySelection);
        for (int i = 0; i < 5; i++)
            garageCarSelection[i] = EncryptedPlayerPrefs.GetInt("selcar" + i, garageCarSelection[i]);
    }

    public void SaveGarageSelection(int selectedCategory, int[] selectedCars)
    {
        garageCategorySelection = selectedCategory;
        EncryptedPlayerPrefs.SetInt("selcat", selectedCategory);

        for (int i = 0; i < 5; i++)
        {
            garageCarSelection[i] = selectedCars[i];
            EncryptedPlayerPrefs.SetInt("selcar" + i, selectedCars[i]);
        }
    }

    public void Load()
    {
        parkingLotCost = OnTheRunDataLoader.Instance.GetTiersPrices();

        UnlockingManager.Instance.Load();

        if (OnTheRunRankManager.Instance != null)
            OnTheRunRankManager.Instance.Load();
        
        SetupManagers();
        gameplayManagers.SendMessage("Load");

        alternativeSlipstreamModeActive = EncryptedPlayerPrefs.GetInt("alt_slipstream", 0) == 1;
        playerLevel = EncryptedPlayerPrefs.GetInt("pl", playerLevel);
        playerLevel = Mathf.Clamp(playerLevel, startPlayerLevel, int.MaxValue);
        playerExperiencePoints = EncryptedPlayerPrefs.GetInt("pep", playerExperiencePoints);

        firstTimePlaying = EncryptedPlayerPrefs.GetInt("ft_played", 1) == 1;

        firstTimeFuelFinished = EncryptedPlayerPrefs.GetInt("ft_fuel_finished", 1) == 1;

        for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber+3; ++i)
        {
            PlayerExternalData tmpData = OnTheRunDataLoader.Instance.GetPlayerData((OnTheRunGameplay.CarId)i);

            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            int loadAcceleration = EncryptedPlayerPrefs.GetInt(currCarName + "_acc", tmpData.minAcceleration);
            int loadMaxSpeed = EncryptedPlayerPrefs.GetInt(currCarName + "_mspeed", tmpData.minMaxSpeed);
            int loadResistance = EncryptedPlayerPrefs.GetInt(currCarName + "_res", tmpData.minResistance);
            int loadTurboSpeed = EncryptedPlayerPrefs.GetInt(currCarName + "_tspeed", tmpData.minTurboSpeed);
            int carCost = tmpData.buyCost;
            int carAlternativeCost = tmpData.buyCost;
            PriceData.CurrencyType buyCurrency = tmpData.buyCurrency;

            bool lockedByDaily = EncryptedPlayerPrefs.GetInt(currCarName + "_lbdaily", tmpData.lockedByDaily) == 1;
            bool canBeBought = EncryptedPlayerPrefs.GetInt(currCarName + "_cbbought", 0) == 1;

            int alternativeCost = tmpData.alternativeCost;
            PriceData.CurrencyType alternativeBuyCurrency = tmpData.alternativeBuyCurrency;

            bool loadOwnedStatus = EncryptedPlayerPrefs.GetInt(currCarName + "_owned", tmpData.locked ? 0:1) == 1;
            int unlockCarAtLevel = tmpData.unlockAtLevel;

            if (!playerDataList.ContainsKey((OnTheRunGameplay.CarId)i))
            {
                playerDataList.Add((OnTheRunGameplay.CarId)i, new PlayerData((OnTheRunGameplay.CarId)i, loadAcceleration, loadMaxSpeed, loadResistance, loadTurboSpeed, loadOwnedStatus, lockedByDaily, canBeBought, unlockCarAtLevel, carCost, alternativeCost, buyCurrency, alternativeBuyCurrency));
            }
        }

        for (int i = 0; i < parkingLotLocked.Length; i++)
        {
            bool isLocked = OnTheRunDataLoader.Instance.GetTiersPrices()[i] > 0 || OnTheRunDataLoader.Instance.GetTiersPlayerLevelThreshold()[i] > 0;
            parkingLotLocked[i] = EncryptedPlayerPrefs.GetInt("lot_" + i + "_locked", isLocked ? 1 : 0);
        }

        coins = EncryptedPlayerPrefs.GetFloat("coins", OnTheRunDataLoader.Instance.GetInitialCoins());
        diamonds = EncryptedPlayerPrefs.GetInt("diamonds", OnTheRunDataLoader.Instance.GetInitialDiamonds());
        extraSpin = EncryptedPlayerPrefs.GetInt("extraSpin", extraSpin);

        bestMeters = new int[(int)OnTheRunEnvironment.Environments.Count];
        prevBestMeters = new int[(int)OnTheRunEnvironment.Environments.Count];
        bestScore = new int[(int)OnTheRunEnvironment.Environments.Count];
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            prevBestMeters[i] = EncryptedPlayerPrefs.GetInt("pbm_" + (i), 0);
            bestMeters[i] = EncryptedPlayerPrefs.GetInt("bm_" + (i), 0);
            bestScore[i] = EncryptedPlayerPrefs.GetInt("bs_" + (i), 0);
        }

        //Debug.Log("LOAD BEST SCORE: " + bestScore);

        long tempDate = long.Parse(EncryptedPlayerPrefs.GetString("lastValidDateString", new DateTime(0).Ticks.ToString()), CultureInfo.InvariantCulture);
        lastValidDate = new DateTime(tempDate);
        //Debug.Log("----------> LOAD DATE: " + lastValidDate);

        //initialize time related stuff---------------
        bool dateValid = false;
        long secondsPassed = 0;
        DateTimeManager.Instance.GetDeltaSeconds(ref lastValidDate, ref dateValid, out secondsPassed);

        //Debug.Log("----------> CURRENT DATE: " + lastValidDate);
        //Debug.Log("----------> secondsPassed: " + secondsPassed + " " + dateValid);

        /*
        DateTime now = iOSUtils.GetNetworkDate();
        if (!iOSUtils.IsNetworkDateValid)
        {
            string strLastPlayTime = EncryptedPlayerPrefs.GetString("lastPlayTime", string.Empty);

            DateTime lastPlayTime = new DateTime(0);
            if (strLastPlayTime.Length > 0)
                lastPlayTime = DateTime.Parse(strLastPlayTime);
            now = lastPlayTime;
        }

        SetupRentCar(now); //lastValidDate
        */

        SetNextLevelExperienceThreshold();
        Save();
    }
    
    public void SaveBeforeStart()
    {
        if (UnlockingManager.Instance == null)
            return;

        UnlockingManager.Instance.Save();

        SetupManagers();
        gameplayManagers.SendMessage("Save");

        EncryptedPlayerPrefs.SetInt("alt_slipstream", alternativeSlipstreamModeActive ? 1 : 0);
        /*for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber + 3; ++i)
        {
            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            PlayerData currData = playerDataList[(OnTheRunGameplay.CarId)i];
            EncryptedPlayerPrefs.SetInt(currCarName + "_acc", currData.acceleration);
            EncryptedPlayerPrefs.SetInt(currCarName + "_mspeed", currData.maxSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_res", currData.resistance);
            EncryptedPlayerPrefs.SetInt(currCarName + "_tspeed", currData.turboSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_owned", currData.owned ? 1 : 0);
            EncryptedPlayerPrefs.SetInt(currCarName + "_lbdaily", currData.lockedByDaily ? 1 : 0);
        }

        for (int i = 0; i < parkingLotLocked.Length; i++)
            EncryptedPlayerPrefs.SetInt("lot_" + i + "_locked", parkingLotLocked[i]);*/

        EncryptedPlayerPrefs.SetFloat("coins", coins);
        EncryptedPlayerPrefs.SetInt("diamonds", diamonds);
        EncryptedPlayerPrefs.SetInt("extraSpin", extraSpin);
        
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            EncryptedPlayerPrefs.SetInt("pbm_" + (i), prevBestMeters[i]);
            EncryptedPlayerPrefs.SetInt("bm_" + (i), bestMeters[i]);
            EncryptedPlayerPrefs.SetInt("bs_" + (i), bestScore[i]);
        }

        EncryptedPlayerPrefs.SetInt("pl", playerLevel);
        EncryptedPlayerPrefs.SetInt("pep", playerExperiencePoints);

        bool dateValid = false;
        long secondsPassed = 0;
        DateTimeManager.Instance.GetDeltaSeconds(ref lastValidDate, ref dateValid, out secondsPassed);
        EncryptedPlayerPrefs.SetString("lastValidDateString", lastValidDate.Ticks.ToString());

        EncryptedPlayerPrefs.Save();
    }

    public void Save(bool saveLastValidDate = true)
    {
        //Debug.Log("SAVING------------------");
        //float initSaveTime = Time.realtimeSinceStartup;

		if( UnlockingManager.Instance == null )
			return;

        UnlockingManager.Instance.Save();

        if (OnTheRunRankManager.Instance != null)
            OnTheRunRankManager.Instance.Save();

        SetupManagers();

        gameplayManagers.SendMessage("Save");
        
        EncryptedPlayerPrefs.SetInt("alt_slipstream", alternativeSlipstreamModeActive ? 1 : 0);
        /*for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber + 3; ++i)
        {
            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            PlayerData currData = playerDataList[(OnTheRunGameplay.CarId)i];
            EncryptedPlayerPrefs.SetInt(currCarName + "_acc", currData.acceleration);
            EncryptedPlayerPrefs.SetInt(currCarName + "_mspeed", currData.maxSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_res", currData.resistance);
            EncryptedPlayerPrefs.SetInt(currCarName + "_tspeed", currData.turboSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_owned", currData.owned ? 1:0);
            EncryptedPlayerPrefs.SetInt(currCarName + "_lbdaily", currData.lockedByDaily ? 1 : 0);
        }

        Debug.Log("step 3: " + (Time.realtimeSinceStartup - initSaveTime));

        for (int i = 0; i < parkingLotLocked.Length; i++)
            EncryptedPlayerPrefs.SetInt("lot_" + i + "_locked", parkingLotLocked[i]);*/

        //Debug.Log("SAVE BEST SCORE: " + bestScore);
        EncryptedPlayerPrefs.SetFloat("coins", coins);
        EncryptedPlayerPrefs.SetInt("diamonds", diamonds);
        EncryptedPlayerPrefs.SetInt("extraSpin", extraSpin);
        
        //EncryptedPlayerPrefs.SetFloat("fuel", fuel);

        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            EncryptedPlayerPrefs.SetInt("pbm_" + (i), prevBestMeters[i]);
            EncryptedPlayerPrefs.SetInt("bm_" + (i), bestMeters[i]);
            EncryptedPlayerPrefs.SetInt("bs_" + (i), bestScore[i]);
        }

        EncryptedPlayerPrefs.SetInt("pl", playerLevel);
        EncryptedPlayerPrefs.SetInt("pep", playerExperiencePoints);

        //EncryptedPlayerPrefs.SetFloat("remainingFuelTime", fuelTimer);

        if (saveLastValidDate)
        {
            bool dateValid = false;
            long secondsPassed = 0;
            DateTimeManager.Instance.GetDeltaSeconds(ref lastValidDate, ref dateValid, out secondsPassed);
            //Debug.Log("***SAVE DATE: " + lastValidDate);
            EncryptedPlayerPrefs.SetString("lastValidDateString", lastValidDate.Ticks.ToString());
        }

        //SaveRentCar();
        //SaveFuelSaver();
        
        EncryptedPlayerPrefs.Save();

    }

    public void SaveCars()
    {
        for (int i = (int)OnTheRunGameplay.CarId.Car_1_europe; i < OnTheRunGameplay.availableCarsNumber + 3; ++i)
        {
            string currCarName = ((OnTheRunGameplay.CarId)i).ToString();
            PlayerData currData = playerDataList[(OnTheRunGameplay.CarId)i];
            EncryptedPlayerPrefs.SetInt(currCarName + "_acc", currData.acceleration);
            EncryptedPlayerPrefs.SetInt(currCarName + "_mspeed", currData.maxSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_res", currData.resistance);
            EncryptedPlayerPrefs.SetInt(currCarName + "_tspeed", currData.turboSpeed);
            EncryptedPlayerPrefs.SetInt(currCarName + "_owned", currData.owned ? 1:0);
            EncryptedPlayerPrefs.SetInt(currCarName + "_lbdaily", currData.lockedByDaily ? 1 : 0);
            EncryptedPlayerPrefs.SetInt(currCarName + "_cbbought", currData.canBeBought ? 1 : 0);
        }

        for (int i = 0; i < parkingLotLocked.Length; i++)
            EncryptedPlayerPrefs.SetInt("lot_" + i + "_locked", parkingLotLocked[i]);
    }
    #endregion
}
