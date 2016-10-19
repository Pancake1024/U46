using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.UI
{
    public struct UIRect
    {
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public float x
        {
            get
            {
                return xMin;
            }
            set
            {
                float dx = value - xMin;
                xMin += dx;
                xMax += dx;
            }
        }

        public float y
        {
            get
            {
                return yMin;
            }
            set
            {
                float dy = value - yMin;
                yMin += dy;
                yMax += dy;
            }
        }

        public float width
        {
            get
            {
                return xMax - xMin;
            }
            set
            {
                xMax = xMin + value;
            }
        }

        public float height
        {
            get
            {
                return yMax - yMin;
            }
            set
            {
                yMax = yMin + value;
            }
        }

        public UIRect(float x, float y, float width, float height)
        {
            xMin = x;
            yMin = y;
            xMax = x + width;
            yMax = y + height;
        }

        public Rect AsRectOffset
        {
            get
            {
                return Rect.MinMaxRect(
                    Mathf.RoundToInt(xMin * UIManager_OLD.screenWidth),
                    Mathf.RoundToInt(yMin * UIManager_OLD.screenHeight),
                    Mathf.RoundToInt(xMax * UIManager_OLD.screenWidth),
                    Mathf.RoundToInt(yMax * UIManager_OLD.screenHeight));//ToDo: RoundToInt?
            }
        }
        
        public static implicit operator Rect(UIRect r)
        {
            float ofx = UIManager_OLD.UsesCustomScreenRect ? UIManager_OLD.CustomScreenRect.xMin : 0.0f,
                  ofy = UIManager_OLD.UsesCustomScreenRect ? UIManager_OLD.CustomScreenRect.yMin : 0.0f;
            return Rect.MinMaxRect(
                Mathf.RoundToInt(ofx + r.xMin * UIManager_OLD.screenWidth),
                Mathf.RoundToInt(ofy + r.yMin * UIManager_OLD.screenHeight),
                Mathf.RoundToInt(ofx + r.xMax * UIManager_OLD.screenWidth),
                Mathf.RoundToInt(ofy + r.yMax * UIManager_OLD.screenHeight));//ToDo: RoundToInt?
        }

        public static implicit operator UIRect(Rect r)
        {
            float ofx = UIManager_OLD.UsesCustomScreenRect ? -UIManager_OLD.CustomScreenRect.xMin : 0.0f,
                  ofy = UIManager_OLD.UsesCustomScreenRect ? -UIManager_OLD.CustomScreenRect.yMin : 0.0f;
            float oow = 1.0f / UIManager_OLD.screenWidth,
                  ooh = 1.0f / UIManager_OLD.screenHeight;
            return new UIRect(Mathf.RoundToInt(r.x + ofx) * oow, Mathf.RoundToInt(r.y + ofy) * ooh, Mathf.RoundToInt(r.width) * oow, Mathf.RoundToInt(r.height) * ooh);
        }
    }
}
