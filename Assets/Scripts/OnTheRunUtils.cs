using System;
using UnityEngine;
using System.Collections.Generic;

public static class OnTheRunUtils
{
    #region Constants
    public static float ToOnTheRunMeters = 1.0f / 1.5f;
    public static float ToRealMeters = 1.5f;
    #endregion

    #region Functions
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
    #endregion
};