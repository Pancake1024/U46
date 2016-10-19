using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.UI
{
    public struct DPRect
    {
        public static float dpi = 160.0f;

        public int xMin;
        public int yMin;
        public int xMax;
        public int yMax;

        public int width
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

        public int height
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

        public DPRect(int x, int y, int width, int height)
        {
            xMin = x;
            yMin = y;
            xMax = x + width;
            yMax = y + height;
        }

        public static implicit operator Rect(DPRect r)
        {
            float m = Screen.dpi / dpi;
            return Rect.MinMaxRect(r.xMin * m, r.yMin * m, r.xMax * m, r.yMax * m);//ToDo: RoundToInt?
        }
    }
}
