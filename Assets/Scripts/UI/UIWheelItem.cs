using UnityEngine;
using SBS.Core;
using System;

[AddComponentMenu("OnTheRun/UI/UIWheelItem")]
public class UIWheelItem : MonoBehaviour
{
    public WheelItem item;

    public enum WheelItem
    {
        Car = 0,
        Coin,
        Diamond,
        Fuel,
        ExtraSpin,
        FuelFreeze
    }

    public static string GetWheelItemString(WheelItem item)
    {
        string res = "";
        switch (item)
        {
            case WheelItem.Car:
                res = OnTheRunDataLoader.Instance.GetLocaleString("car");
                break;
            case WheelItem.Coin:
                res = OnTheRunDataLoader.Instance.GetLocaleString("coin");
                break;
            case WheelItem.Diamond:
                res = OnTheRunDataLoader.Instance.GetLocaleString("diamond");
                break;
            case WheelItem.Fuel:
                res = OnTheRunDataLoader.Instance.GetLocaleString("fuel");
                break;
            case WheelItem.ExtraSpin:
                res = OnTheRunDataLoader.Instance.GetLocaleString("extra_spin");
                break;
            case WheelItem.FuelFreeze:
                res = OnTheRunDataLoader.Instance.GetLocaleString("fuel_freeze");
                break;
        }
        return res;
    }
}