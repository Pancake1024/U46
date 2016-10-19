using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("OnTheRun/OnTheRunDailyBonusManager")]
public class OnTheRunDailyBonusManager : Manager<OnTheRunDailyBonusManager>
{
    public static int MaxDailyBonusDays = 5; //10;
    #region Singleton instance
    public static OnTheRunDailyBonusManager Instance
    {
        get
        {
            return Manager<OnTheRunDailyBonusManager>.Get();
        }
    }
    #endregion
    
    public enum DailyBonus
    {
        Coin = 0,
        Diamond,
        Fuel,
        ExtraSpin,
        Boost,
        SpecialCar,
        Mistery
    }

    public class DailyBonusData
    {
        public DailyBonus bonusType;
        public int quantity;
        public List<DailyBonusData> misteryRewards;

        public DailyBonusData(DailyBonus _bonus, int _quantity, List<DailyBonusData> _misteryRewards=null)
        {
            bonusType = _bonus;
            quantity = _quantity;
            misteryRewards = _misteryRewards;
        }
    }

    protected DailyBonusData rewardItem;
    protected List<DailyBonusData> bonusList;
    protected DailyBonusData extraRewardBonus;
    protected int dailyBonusDaysCounter = -1;
    protected OnTheRunGameplay gameplayManager;
    protected List<OnTheRunGameplay.CarId> carAvailableForReward;
    protected OnTheRunBooster.Booster nextBoosterToWin;
    protected OnTheRunGameplay.CarId nextCarToWin;
    protected bool forceActiveMissionsUpdate = false;
    protected bool newsBadgdesActive = false;
    public string streakType = "none";
    public int streakDays = 0;
    
    public OnTheRunBooster.BoosterType CurrentRewardBooster
    {
        get { return nextBoosterToWin.type; }
    }

    public OnTheRunGameplay.CarId CurrentRewardCar
    {
        get { return nextCarToWin; }
    }

    public DailyBonusData RewardGained
    {
        get { return rewardItem; }
    }

    public int DaysRemaining
    {
        get { return OnTheRunDailyBonusManager.MaxDailyBonusDays - EncryptedPlayerPrefs.GetInt(kPrevDaysCounterKey, dailyBonusDaysCounter); }
    }

    public int DaysPassed
    {
        get { return OnTheRunDailyBonusManager.MaxDailyBonusDays - DaysRemaining; }
    }

    public bool ForceActiveMissionsUpdate
    {
        get { return forceActiveMissionsUpdate; }
        set { forceActiveMissionsUpdate = value; }
    }

    public bool NewsBadgdesActive
    {
        get { return newsBadgdesActive; }
        set { newsBadgdesActive = value; }
    }

    public bool ShowFirstTimeMissionPopup
    {
        get { return EncryptedPlayerPrefs.GetInt("ft_mpop", 0) == 1; }
        set { EncryptedPlayerPrefs.SetInt("ft_mpop", value ? 1 : 0); }
    }

    protected void Initialize()
    {
        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DailyBonusManager Initialize Init");

        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        OnTheRunDataLoader dataLoader = OnTheRunDataLoader.Instance;
        bonusList = new List<DailyBonusData>();
        for (int i = 0; i < dataLoader.GetActiveDailyBonusesNumber(); i++)
        {
            //if (i == dataLoader.GetActiveDailyBonusesNumber()-1)
            //    extraRewardBonus = dataLoader.GetBonusDataById(i);
            //else
                bonusList.Add(dataLoader.GetBonusDataById(i));
        }
        
        string loadNextBoost = EncryptedPlayerPrefs.GetString("nextBoosterToWin", String.Empty);
        if (loadNextBoost == String.Empty)
        {
            int minIndex = 0;
            int maxIndex = ((int)OnTheRunBooster.BoosterType.Count) - 2;
            int randomIndex = UnityEngine.Random.Range(minIndex, maxIndex);
            nextBoosterToWin = OnTheRunBooster.Instance.GetBoosterById((OnTheRunBooster.BoosterType)randomIndex);
            EncryptedPlayerPrefs.SetString("nextBoosterToWin", nextBoosterToWin.type.ToString());
        }
        else
            nextBoosterToWin = OnTheRunBooster.Instance.GetBoosterById((OnTheRunBooster.BoosterType)System.Enum.Parse(typeof(OnTheRunBooster.BoosterType), loadNextBoost));

        string loadNextCar = EncryptedPlayerPrefs.GetString("nextCarToWin", String.Empty);
        if (loadNextCar == String.Empty)
        {
            SetupAvailableDailyBonusCars();
            if (carAvailableForReward.Count > 0)
                nextCarToWin = carAvailableForReward[0];
            else
                nextCarToWin = OnTheRunGameplay.CarId.None;

            EncryptedPlayerPrefs.SetString("nextCarToWin", nextCarToWin.ToString());
        }
        else
        {
            SetupAvailableDailyBonusCars();
            nextCarToWin = (OnTheRunGameplay.CarId)System.Enum.Parse(typeof(OnTheRunGameplay.CarId), loadNextCar);
            if (carAvailableForReward.IndexOf(nextCarToWin) < 0)
            {
                if (carAvailableForReward.Count > 0)
                    nextCarToWin = carAvailableForReward[0];
                else
                    nextCarToWin = OnTheRunGameplay.CarId.None;
            }
        }


        //to do: mettere da parte
        //bonusList.Add(new DailyBonusData(DailyBonus.Diamond, 15));

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DailyBonusManager Initialize End");

    }

    protected const string kDaysCounterKey = "dck";
    protected const string kPrevDaysCounterKey = "pdck";
    protected const string kLastDayKey = "ldk";
    protected const string kLastDayValidKey = "ldvk";

    protected const string kShowDailyBonusKey = "sdbk";
    protected const string kDaysInARowKey = "dark";
    protected const string kShowSequenceKey = "ssk";

    protected const string kFirstTimeEnteredKey = "ftek";

    protected void SetupDailyBonus(bool forceDailyBonus = false)
    {
        streakType = "none";
        streakDays = 0;

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DailyBonusManager SetupDailyBonus Init");

		DateTime sessionStart = OnTheRunSessionsManager.Instance.GetCurrentSessionBegin();

        if (bonusList == null)
            this.Initialize();

        if (!Manager<UIManager>.Get().IsPopupInStack("DailyBonusPopup") && (EncryptedPlayerPrefs.GetInt(kFirstTimeEnteredKey, 0) == 1 || forceDailyBonus))//!PlayerPersistentData.Instance.FirstTimePlaying)
        {
            dailyBonusDaysCounter = EncryptedPlayerPrefs.GetInt(kDaysCounterKey, -1);
            if (dailyBonusDaysCounter > OnTheRunDailyBonusManager.MaxDailyBonusDays-1) //9
				dailyBonusDaysCounter = -1;

            DateTime lastDayPlayed = EncryptedPlayerPrefs.GetUtcDate(kLastDayKey);

            bool initialTimeException = false;
            if (lastDayPlayed == new DateTime(0, DateTimeKind.Utc))
            {
                initialTimeException = true;
                //   lastDayPlayed = sessionStart;
            }

            bool lastDayPlayedValid = 1 == EncryptedPlayerPrefs.GetInt(kLastDayValidKey, 0);
#if UNITY_EDITOR
            int daysPassed = (int)Math.Floor((sessionStart - lastDayPlayed).TotalMinutes / 10.0f);
#else
            int daysPassed = (int)(sessionStart - lastDayPlayed).TotalDays;
#endif

            //cheat daily bonus
            //daysPassed = (int)Math.Floor((sessionStart - lastDayPlayed).TotalMinutes / 5);  //(sessionStart - lastDayPlayed).TotalSeconds > 0 ? 2 : 0;// 
            
			SBS.Miniclip.UtilsBindings.ConsoleLog("*** 1 - DAILY BONUS, last day played: " + lastDayPlayed + ", days passed: " + daysPassed + ", daysCounter: " + dailyBonusDaysCounter);

            if (daysPassed <= 0 || (!lastDayPlayedValid && !OnTheRunSessionsManager.Instance.IsCurrentSessionBeginValid() && dailyBonusDaysCounter >= 0))
                return;

            if (daysPassed >= 1 && !initialTimeException)
            {
                forceActiveMissionsUpdate = true;
                newsBadgdesActive = true;
                OnTheRunNotificationManager.Instance.ResetGiftAndInviteDone(daysPassed);
            }

            if (daysPassed == 1 || dailyBonusDaysCounter < 0)
            {
                if (dailyBonusDaysCounter < 0) 
                    dailyBonusDaysCounter = 0;
                ++dailyBonusDaysCounter;

                EncryptedPlayerPrefs.SetInt(kShowDailyBonusKey, 1);
                EncryptedPlayerPrefs.SetInt(kDaysInARowKey, dailyBonusDaysCounter);
            }
            else if (daysPassed > 1)
            {

				if (dailyBonusDaysCounter < 0) 
					dailyBonusDaysCounter = 0;
				++dailyBonusDaysCounter;

				EncryptedPlayerPrefs.SetInt(kPrevDaysCounterKey, dailyBonusDaysCounter); //Mathf.Max(EncryptedPlayerPrefs.GetInt(kPrevDaysCounterKey, 1), dailyBonusDaysCounter));
                dailyBonusDaysCounter = 1;
				EncryptedPlayerPrefs.SetInt(kDaysInARowKey, dailyBonusDaysCounter);
                EncryptedPlayerPrefs.SetInt(kShowSequenceKey, 1);
            }
			//Debug.Log("*** 2 - DAILY BONUS, last day played: " + lastDayPlayed + ", days passed: " + daysPassed + ", daysCounter: " + dailyBonusDaysCounter);
            EncryptedPlayerPrefs.SetInt(kDaysCounterKey, dailyBonusDaysCounter);
            EncryptedPlayerPrefs.SetUtcDate(kLastDayKey, sessionStart);
            EncryptedPlayerPrefs.SetInt(kLastDayValidKey, OnTheRunSessionsManager.Instance.IsCurrentSessionBeginValid() ? 1 : 0);
        }
        EncryptedPlayerPrefs.SetInt(kFirstTimeEnteredKey, 1);
        EncryptedPlayerPrefs.Save();

        if (forceDailyBonus)
        {
            OnTheRunDailyBonusManager.Instance.DailyPopupHasToShow = true;
            Manager<UIRoot>.Get().ShowDailyBonusPopup();
        }

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- DailyBonusManager SetupDailyBonus End");
    } 

    //void Start()
    void Awake()
    {
        this.SetupDailyBonus();

        OnTheRunSessionsManager.Instance.onSessionBegin.AddTarget(gameObject, "OnDailyBonusSessionBegin");
    }

    void OnDestroy()
    {
        if (OnTheRunSessionsManager.Instance != null)
            OnTheRunSessionsManager.Instance.onSessionBegin.RemoveTarget(gameObject);

        base.OnDestroy();
    }

    void OnDailyBonusSessionBegin()
    {
        this.SetupDailyBonus();
    }

    public bool DailyPopupHasToShow
    {
        get { return 1 == EncryptedPlayerPrefs.GetInt(kShowDailyBonusKey, 0); }
        set { EncryptedPlayerPrefs.SetInt(kShowDailyBonusKey, value ? 1 : 0); }
    }

    public bool DailyBonusSequencePopupHasToShow
    {
        get { return 1 == EncryptedPlayerPrefs.GetInt(kShowSequenceKey, 0); }
        set { EncryptedPlayerPrefs.SetInt(kShowSequenceKey, value ? 1 : 0); }
    }

    public int DaysInARow
    {
        get { return EncryptedPlayerPrefs.GetInt(kDaysInARowKey, 0); }
    }

    public void TestDailyBonus(int _dailyBonusDaysCounter)
    {
        newsBadgdesActive = true;
        OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate = true;
        dailyBonusDaysCounter = _dailyBonusDaysCounter;
        EncryptedPlayerPrefs.SetInt(kDaysCounterKey, dailyBonusDaysCounter);
        EncryptedPlayerPrefs.SetInt(kDaysInARowKey, dailyBonusDaysCounter);
        OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusPopup");
    }

    public void RestoreDailyBonusStreak()
    {
        dailyBonusDaysCounter = EncryptedPlayerPrefs.GetInt(kPrevDaysCounterKey, 1);
		//Debug.Log("??dailyBonusDaysCounter: " + dailyBonusDaysCounter);
        EncryptedPlayerPrefs.SetInt(kDaysCounterKey, dailyBonusDaysCounter);
		EncryptedPlayerPrefs.SetInt(kDaysInARowKey, dailyBonusDaysCounter);
    }

    public DailyBonusData GetDailyBonusReward(int popupDayCounter)
    {
        DailyBonusData retValue = bonusList[popupDayCounter - 1];
        return retValue;
    }

    public void AssignDailyBonusReward(int daysInaRow, DailyBonusData misteryReward=null)
    {
        if (daysInaRow < 0)
        {
            rewardItem = bonusList[bonusList.Count-1];
            //rewardItem = extraRewardBonus;
        }
        else
            rewardItem = bonusList[daysInaRow];

        if (misteryReward != null)
            rewardItem = misteryReward;
        
        switch (rewardItem.bonusType)
        {
            case DailyBonus.Coin:
                PlayerPersistentData.Instance.Coins += rewardItem.quantity;
                break;
            case DailyBonus.Diamond:
                PlayerPersistentData.Instance.Diamonds += rewardItem.quantity;
                break;
            case DailyBonus.Fuel:
                OnTheRunFuelManager.Instance.Fuel += rewardItem.quantity;
                break;
            case DailyBonus.ExtraSpin:
                PlayerPersistentData.Instance.ExtraSpin += rewardItem.quantity;
                Manager<UIRoot>.Get().CanSpinWheelButtonBounce = true;
                break;
            case DailyBonus.Boost:
                if (!nextBoosterToWin.equipped)
                {
                    nextBoosterToWin.TryBuying(1, true, true);
                    nextBoosterToWin.TryEquipping();
                }
                EncryptedPlayerPrefs.SetString("nextBoosterToWin", string.Empty);
                break;
            case DailyBonus.SpecialCar:
                if (nextCarToWin!=OnTheRunGameplay.CarId.None)
                {
                    if (OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(nextCarToWin.ToString().ToLowerInvariant(), PriceData.CurrencyType.FirstCurrency, "0", OmniataIds.Product_Type_Unlock);

                    UIGaragePage.lastWonCarId = nextCarToWin;
                    PlayerPersistentData.Instance.GetPlayerData(nextCarToWin).UnlockCar();
                    EncryptedPlayerPrefs.SetString("nextCarToWin", string.Empty);
                }
                else
                {
                    AssignDailyBonusReward(-1);
                }
                Manager<UIManager>.Get().ActivePage.SendMessage("OnRefreshRentedCars", SendMessageOptions.DontRequireReceiver);
                break;
        }

        Manager<UIRoot>.Get().UpdateCurrenciesItem();
        //Manager<UIRoot>.Get().RefreshSpinWheelButton();
    }

    public void SetupAvailableDailyBonusCars()
    {
        carAvailableForReward = new List<OnTheRunGameplay.CarId>();
        for (int i = 0; i < (int)OnTheRunEnvironment.Environments.Count; ++i)
        {
            GameObject[] currentCarsList = gameplayManager.GetCarsFromIndex(i);

            if (!PlayerPersistentData.Instance.IsParkingLotLocked(i))
            {
                OnTheRunGameplay.CarId currentCarId = currentCarsList[2].GetComponent<PlayerKinematics>().carId;
                PlayerPersistentData.PlayerData playerData = PlayerPersistentData.Instance.GetPlayerData(currentCarId);
                if (playerData.lockedByDaily)
                {
                    carAvailableForReward.Add(currentCarId);
                }
            }
        }

        //foreach (OnTheRunGameplay.CarId carId in carAvailableForReward)
        //    Debug.Log("--<  " + carId);
    }

    public int GetDailyBonusNotificationTime( )
    {
        DateTime lastDayPlayed = OnTheRunSessionsManager.Instance.GetCurrentSessionBegin();
        DateTime nextDay = lastDayPlayed.AddDays(1.0);

        return (int)nextDay.Subtract(lastDayPlayed).TotalSeconds;
    }
}