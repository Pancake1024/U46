using UnityEngine;
using System.Collections;

public class PlayerExternalData
{
    public string carName;
    public int minAcceleration;
    public int maxAcceleration;
    public int minMaxSpeed;
    public int maxMaxSpeed;
    public int minResistance;
    public int maxResistance;
    public int minTurboSpeed;
    public int maxTurboSpeed;
    public int buyCost;
    public PriceData.CurrencyType upgradeCurrency;
    public PriceData.CurrencyType buyCurrency;
    public bool locked;
    public int lockedByDaily;
    public int alternativeCost;
    public int unlockAtLevel;
    public PriceData.CurrencyType alternativeBuyCurrency;
}
