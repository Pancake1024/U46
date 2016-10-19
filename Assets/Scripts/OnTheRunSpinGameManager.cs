using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("OnTheRun/OnTheRunSpinGameManager")]
public class OnTheRunSpinGameManager : Manager<OnTheRunSpinGameManager>
{
    #region Singleton instance
    public static OnTheRunSpinGameManager Instance
    {
        get
        {
            return Manager<OnTheRunSpinGameManager>.Get();
        }
    }
    #endregion

    public enum SpinPrizes
    {
        Little,
        Medium,
        Precious
    }

    protected void SpinGameReward(SpinPrizes priceType)
    {
        int baseCoinsValue = 100,
            rnd;
        switch (priceType)
        {
            case SpinPrizes.Little:
                rnd = UnityEngine.Random.Range(0, 4);
                switch (rnd)
                {
                    case 1: PlayerPersistentData.Instance.ExtraSpin += 1; break;
                    case 2: PlayerPersistentData.Instance.Coins += baseCoinsValue; break;
                    case 3: PlayerPersistentData.Instance.Diamonds += 1; break;
                    case 4: OnTheRunFuelManager.Instance.Fuel += 1; break;
                }
                break;
            case SpinPrizes.Medium:
                rnd = UnityEngine.Random.Range(0, 4);
                switch (rnd)
                {
                    case 1: PlayerPersistentData.Instance.ExtraSpin += 2; break;
                    case 2: PlayerPersistentData.Instance.Coins += 5 * baseCoinsValue; break;
                    case 3: PlayerPersistentData.Instance.Diamonds += 2; break;
                    case 4: OnTheRunFuelManager.Instance.Fuel += 3; break;
                }
                break;
            case SpinPrizes.Precious:
                rnd = UnityEngine.Random.Range(0, 4);
                switch (rnd)
                {
                    case 1: PlayerPersistentData.Instance.Diamonds += 25; break;
                    case 2: OnTheRunFuelManager.Instance.Fuel += 10; break;
                    case 3: //a car
                        break;
                    case 4: //Fuel freeze ???
                        break;
                }
                break;
        }
    }
}