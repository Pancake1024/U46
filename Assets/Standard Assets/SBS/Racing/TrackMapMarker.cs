using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/TrackMapMark")]
public class TrackMapMarker : MonoBehaviour
{
    #region Public members
    public TrackMap map;
    #endregion

    #region Protected members
    protected int prevX = -1;
    protected int prevY = -1;
    #endregion

    #region Messages
    void InitMapPosition()
    {
        this.LateUpdate();
    }
    #endregion

    #region Unity callbacks
    void LateUpdate()
    {
        if (null == map)
            return;

        Vector3 s = map.bounds.size;
        if (s.x < Mathf.Epsilon || s.z < Mathf.Epsilon)
            return;

        float aspect      = ((float)map.imageHeight / (float)map.imageWidth),
              orthoHeight = Mathf.Max(map.bounds.size.x * aspect, map.bounds.size.z);

        Vector3 v = transform.position - map.bounds.center;
        v.x *= aspect;
        v.x /= orthoHeight;
        v.z /= orthoHeight;

        int newX = Mathf.RoundToInt(Mathf.Clamp01(v.x + 0.5f) * (float)map.imageWidth),
            newY = Mathf.RoundToInt((1.0f - Mathf.Clamp01(v.z + 0.5f)) * (float)map.imageHeight);

        if (prevX != newX || prevY != newY)
        {
            prevX = newX;
            prevY = newY;

            gameObject.SendMessage("OnMapPosChanged", new SBS.Core.Tuple<int, int>(newX, newY));
        }
    }
    #endregion
}
