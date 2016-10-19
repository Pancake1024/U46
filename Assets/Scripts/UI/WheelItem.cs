using UnityEngine;
using SBS.Core;
using System;

public class WheelItem
{
    public UIWheelItem.WheelItem item;
    public int quantity;
    public Level level;

    public enum Level
    {
        cheap = 0,
        medium,
        rich
    }

    public WheelItem(UIWheelItem.WheelItem _item, int _quantity, Level _level)
    {
        item = _item;
        quantity = _quantity;
        level = _level;
    }
}