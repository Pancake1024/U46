using System;
using UnityEngine;
using SBS.Core;
using System.Collections;

public class PriceData
{
    #region Protected Members
    protected PriceDataId priceDataId;
    protected float price;
    protected CurrencyType currencyType;
    #endregion

    #region Public Properties
    public enum PriceDataId
    {
        BoosterTurbo,
        BoosterShield,
        BoosterSpecialCar,
        BoosterDoubleCoins,
        BoosterDoubleExp,
        BoosterMoreCheckpointTime,
        BoosterSurprisePack,
        BoosterMultiplePack
    }

    public enum CurrencyType
    {
        FirstCurrency,
        SecondCurrency
    }

    public float Price
    {
        get { return price; }
    }

    public CurrencyType Currency
    {
        get { return currencyType; }
    }
    #endregion

    #region Ctor
    public PriceData(PriceDataId _priceDataId, CurrencyType _currencyType)
    {
        priceDataId = _priceDataId;
        currencyType = _currencyType; 
        price = ComputePrice();
    }
    #endregion

    #region Price Computaion
    public float ComputePrice( )
    {
        switch (priceDataId)
        {
            case PriceDataId.BoosterTurbo: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.Turbo); break;
            case PriceDataId.BoosterShield: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.Shield); break;
            case PriceDataId.BoosterSpecialCar: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.SpecialCar); break;
            case PriceDataId.BoosterDoubleCoins: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.DoubleCoins); break;
            case PriceDataId.BoosterDoubleExp: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.DoubleExp); break;
            case PriceDataId.BoosterMoreCheckpointTime: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.MoreCheckpointTime); break;
            case PriceDataId.BoosterSurprisePack: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.SurprisePack); break;
            case PriceDataId.BoosterMultiplePack: price = OnTheRunEconomy.Instance.GetBoostPrice(OnTheRunBooster.BoosterType.MultiplePack); break;
        }
        return price;
    }
    #endregion
}