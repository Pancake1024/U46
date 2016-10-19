using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using System.Collections;

public class OnTheRunBooster
{
    #region Singleton instance
    protected static OnTheRunBooster instance = null;

    public static OnTheRunBooster Instance
    {
        get
        {
            if (instance == null)
                instance = new OnTheRunBooster();

            return instance;
        }
    }
    #endregion
    
    #region Protected Members
    protected List<Booster> boosters;
    protected ParticleSystem buyEffect;
    protected OnTheRunGameplay gameplayManager;
    #endregion

    #region Public Properties
    public class Booster
    {
        public BoosterType type;
        public string name;
        public string displayedName;
        public string description;
        public bool equipped;
        public int quantity;
        public PriceData priceData;
        private OnTheRunBooster boosterManager;
        public UIShopItem shopItemComp;
        public bool used;

        public Booster(OnTheRunBooster _boosterManager, BoosterType _type, int _quantity)
        {
            boosterManager = _boosterManager;
            type = _type;
            name = type.ToString();
            //equipped = false;
            equipped = EncryptedPlayerPrefs.GetInt("boost_" + type, 0) == 1;
            used = false;
            quantity = _quantity;
#if UNITY_WEBPLAYER
            priceData = new PriceData(GetPriceDataIdFromBoosterType(_type), PriceData.CurrencyType.FirstCurrency);
#else
            if (type == BoosterType.DoubleCoins || type == BoosterType.DoubleExp)
                priceData = new PriceData(GetPriceDataIdFromBoosterType(_type), PriceData.CurrencyType.SecondCurrency);
            else
                priceData = new PriceData(GetPriceDataIdFromBoosterType(_type), PriceData.CurrencyType.FirstCurrency);
#endif

            switch (type)
            {
                case BoosterType.Turbo:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_turbo_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_turbo_descr");
                    break;
                case BoosterType.Shield:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_shield_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_shield_descr");
                    break;
                case BoosterType.SpecialCar:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_specialcar_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_specialcar_descr");
                    break;
                case BoosterType.DoubleCoins:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_doublecoins_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_doublecoins_descr");
                    break;
                case BoosterType.DoubleExp:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_doubleexp_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_doubleexp_descr");
                    break;
                case BoosterType.MoreCheckpointTime:
                    displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_moreckp_name");
                    description = OnTheRunDataLoader.Instance.GetLocaleString("booster_moreckp_descr");
                    break;
                case BoosterType.SurprisePack:
                    //displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_surprisepack_name");
                    //description = OnTheRunDataLoader.Instance.GetLocaleString("booster_surprisepack_descr");
                    break;
                case BoosterType.MultiplePack:
                    //displayedName = OnTheRunDataLoader.Instance.GetLocaleString("booster_multiplepack_name");
                    //description = OnTheRunDataLoader.Instance.GetLocaleString("booster_multiplepack_descr");
                    break;
            }
        }

        public bool TryEquipping()
        {
            if (quantity > 0 || equipped)
            {
                boosterManager.EquippingBooster(type, !equipped);
                return true;
            }
            
            return false;
        }

        public bool TryBuying(int howMany, bool avoidPaying = false, bool skipFlyier = false)
        {
            bool retValue = boosterManager.OnBuyBooster(type, howMany, avoidPaying, this, skipFlyier);
            //if (shopItemComp!=null)
            //    shopItemComp.transform.FindChild("Badge/BadgeTextfield").GetComponent<UITextField>().text = quantity.ToString();
            return retValue;
        }

        protected PriceData.PriceDataId GetPriceDataIdFromBoosterType(BoosterType type)
        {
            PriceData.PriceDataId retValue = PriceData.PriceDataId.BoosterTurbo;
            switch (type)
            {
                case BoosterType.Turbo:
                    retValue = PriceData.PriceDataId.BoosterTurbo;
                    break;
                case BoosterType.Shield:
                    retValue = PriceData.PriceDataId.BoosterShield;
                    break;
                case BoosterType.SpecialCar:
                    retValue = PriceData.PriceDataId.BoosterSpecialCar;
                    break;
                case BoosterType.DoubleCoins:
                    retValue = PriceData.PriceDataId.BoosterDoubleCoins;
                    break;
                case BoosterType.DoubleExp:
                    retValue = PriceData.PriceDataId.BoosterDoubleExp;
                    break;
                case BoosterType.MoreCheckpointTime:
                    retValue = PriceData.PriceDataId.BoosterMoreCheckpointTime;
                    break;
                case BoosterType.SurprisePack:
                    retValue = PriceData.PriceDataId.BoosterSurprisePack;
                    break;
                case BoosterType.MultiplePack:
                    retValue = PriceData.PriceDataId.BoosterMultiplePack;
                    break;
            }

            return retValue;
        }
    }

    public enum BoosterType
    {
        Turbo = 0,
        Shield,
        SpecialCar,
        DoubleCoins,
        DoubleExp,
        MoreCheckpointTime,
        SurprisePack,
        MultiplePack,
        Count
    }
    #endregion
    
    #region Ctor
    public OnTheRunBooster()
    {
        GameObject uiRoot = GameObject.Find("UI");
        buyEffect = uiRoot.transform.FindChild("FxUIBuy").GetComponent<ParticleSystem>();
        buyEffect.Stop();
        boosters = new List<Booster>();
        //DebugResetBoosters();
        Load();
    }
    #endregion

    #region Functions
    public Booster GetBoosterById(BoosterType type)
    {
        for (int i = 0; i < boosters.Count; ++i)
        {
            Booster currData = boosters[i];
            if (currData.type == type)
                return currData;
        }

        return null;
    }

    public bool IsBoosterEquipped(BoosterType type)
    {
        for (int i = 0; i < boosters.Count; ++i)
        {
            Booster currData = boosters[i];
            if (currData.type == type)
                return currData.equipped;
        }

        Asserts.Assert(false, "IsBoosterEquipped Exception: no booster " + type + "found.");
        return false;
    }

    public bool OnBuyBooster(BoosterType type, int quantityBought, bool avoidPaying, Booster boosterRef, bool skipFlyier = false)
    {
        bool retValue = false;
        for (int i = 0; i < boosters.Count; ++i)
        {
            Booster currData = boosters[i];
            if (currData.type == type)
            {
                float cost = quantityBought * currData.priceData.Price;
                bool canBuy = currData.quantity < 1 && !currData.equipped;
                bool canAfford = PlayerPersistentData.Instance.CanAfford(currData.priceData.Currency, cost);
                if ((canAfford || avoidPaying) && canBuy)
                {
                    if (currData.type != BoosterType.MultiplePack && currData.type != BoosterType.SurprisePack)
                    {
                        EncryptedPlayerPrefs.SetInt("boost_" + type, 1);
                        currData.quantity += quantityBought;
                        currData.quantity = Mathf.Clamp(currData.quantity, 0, 1);
                        if (!avoidPaying)
                            PlayerPersistentData.Instance.BuyItem(currData.priceData.Currency, cost);
                    }
                    else
                    {
                        EquippingBooster(type, true);
						if (!avoidPaying)
                        	PlayerPersistentData.Instance.BuyItem(currData.priceData.Currency, cost);
                    }

                    if (!avoidPaying && OnTheRunOmniataManager.Instance != null)
                        OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_Booster + "_" + type.ToString(), currData.priceData.Currency, cost.ToString(), OmniataIds.Product_Type_Standard);

                    retValue = true;
                    Manager<UIRoot>.Get().BroadcastMessage("PlayBoosterPurchasedAnim", boosterRef, SendMessageOptions.DontRequireReceiver);
                    Manager<UIRoot>.Get().UpdateCurrenciesItem();
                    //if (!skipFlyier)
                    //    Manager<UIRoot>.Get().LaunchFlyer("EnterExitFlyer", OnTheRunDataLoader.Instance.GetLocaleString("item_purchased"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                }
                else if (!canBuy)
                {
                    OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
                    Manager<UIManager>.Get().FrontPopup.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("you_already_have") + " " + currData.name.ToUpper() + " " + OnTheRunDataLoader.Instance.GetLocaleString("boost");
                }
                else
                {
                    UICurrencyPopup.ShopPopupTypes currentCurrencyButton = currData.priceData.Currency == PriceData.CurrencyType.FirstCurrency ? UICurrencyPopup.ShopPopupTypes.Money : UICurrencyPopup.ShopPopupTypes.Diamonds;
                    Manager<UIRoot>.Get().ShowWarningPopup(currentCurrencyButton);
                }
            }
        }
        return retValue;
    }
    
    public int EquippingBooster(BoosterType type, bool equipped)
    {
        bool found = true;
        int quantityRemaining = -1;

        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        if (type == BoosterType.SurprisePack)
        {
			GameObject.Find("InAppPage").GetComponent<UIInAppPage>().SpinForSurprisePack();
        }
        else if (type == BoosterType.MultiplePack)
        {
            OnTheRunFireworks.Instance.StartFireworksEffect(25, Manager<UIManager>.Get().ActivePage.gameObject.transform.FindChild("fireworks"));
            for (int i = 0; i < boosters.Count; ++i)
            {
                Booster currData = boosters[i];
                if (currData.type != BoosterType.SurprisePack && currData.type != BoosterType.MultiplePack)
                {
                    //if(currData.quantity < 1 && !currData.equipped) //NEW DENIS
                    if (!currData.equipped)
                    {
                        currData.TryBuying(1, true);
                        currData.TryEquipping();
                    }
                }
                else if (currData.type == BoosterType.MultiplePack || currData.type == BoosterType.SurprisePack)
                {
                    //currData.quantity -= 1;
                    currData.used = true;
                }
            }
        }
        else
        {
            found = false;
            for (int i = 0; i < boosters.Count; ++i)
            {
                Booster currData = boosters[i];
                if (currData.type == type)
                {
                    found = true;
                    currData.equipped = equipped;
                    currData.quantity += equipped ? -1 : 1;
                    currData.quantity = Mathf.Clamp(currData.quantity, 0, 1);
                    quantityRemaining = currData.quantity;
                }
            }
        }

        if (!found)
            Asserts.Assert(false, "OnEquipBooster Exception: no booster " + type + "found.");

        return quantityRemaining;
    }

    public void ActivateEquippedBoosters( )
    {
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        for (int i = 0; i < boosters.Count; ++i)
        {
            Booster currData = boosters[i];
            if (currData.equipped)
            {
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.USE_BOOST_EASY);
                OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.USE_BOOST_HARD);
                gameplayManager.UpdateRunParameter(OnTheRunGameplay.RunParameters.Booster, (int)currData.type);
                LevelRoot.Instance.Root.BroadcastMessage("OnEquipBooster", currData.type);
                currData.equipped = false;
            }
            if (currData.used)
                currData.used = false;

            /*if (currData.type == BoosterType.SurprisePack || currData.type == BoosterType.MultiplePack)
            {
                if (!currData.shopItemComp.transform.FindChild("BuyBoosterButton").gameObject.activeSelf)
                    gameplayManager.UpdateRunParameter(OnTheRunGameplay.RunParameters.Booster, (int)currData.type);
            }*/

            EncryptedPlayerPrefs.SetInt("boost_" + currData.type, 0);
        }
    }
    #endregion

    #region Load/Save
    void DebugResetBoosters()
    {
        for (int i = 0; i < (int)BoosterType.Count; ++i)
        {
            EncryptedPlayerPrefs.DeleteKey((BoosterType)i + "_quantity");
            EncryptedPlayerPrefs.DeleteKey((BoosterType)i + "_equipped");
        }
    }

    void Load()
    {
        boosters.Clear();
        for (int i = 0; i < (int)BoosterType.Count; ++i)
        {
            Booster currBooster = new Booster(this, (BoosterType)i, 0);
            boosters.Add(currBooster);
            //currBooster.quantity = EncryptedPlayerPrefs.GetInt(currBooster.name + "_quantity", 0);
            //currBooster.equipped = EncryptedPlayerPrefs.GetInt(currBooster.name + "_equipped", 0) == 1;
        }
    }

    public void SaveBoosters()
    {
        /*for (int i = 0; i < (int)BoosterType.Count; ++i)
        {
            //Booster currBooster = boosters[i];
            //EncryptedPlayerPrefs.SetInt(currBooster.name + "_quantity", currBooster.quantity);
            //EncryptedPlayerPrefs.SetInt(currBooster.name + "_equipped", currBooster.equipped ? 1 : 0);
        }*/
    }
    #endregion
}
