using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.UI
{
    public struct UIVector2
    {
        public float x;
        public float y;

        public UIVector2(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public Vector2 AsVector2Offset
        {
            get
            {
                return new Vector2(Mathf.RoundToInt(x * UIManager_OLD.screenWidth), Mathf.RoundToInt(y * UIManager_OLD.screenHeight));//ToDo: RoundToInt?
            }
        }

        public static implicit operator Vector2(UIVector2 r)
        {
            float ofx = UIManager_OLD.UsesCustomScreenRect ? UIManager_OLD.CustomScreenRect.xMin : 0.0f,
                  ofy = UIManager_OLD.UsesCustomScreenRect ? UIManager_OLD.CustomScreenRect.yMin : 0.0f;
            return new Vector2(Mathf.RoundToInt(ofx + r.x * UIManager_OLD.screenWidth), Mathf.RoundToInt(ofy + r.y * UIManager_OLD.screenHeight));//ToDo: RoundToInt?
        }

        public static implicit operator UIVector2(Vector2 r)
        {
            float ofx = UIManager_OLD.UsesCustomScreenRect ? -UIManager_OLD.CustomScreenRect.xMin : 0.0f,
                  ofy = UIManager_OLD.UsesCustomScreenRect ? -UIManager_OLD.CustomScreenRect.yMin : 0.0f;
            float oow = 1.0f / UIManager_OLD.screenWidth,
                  ooh = 1.0f / UIManager_OLD.screenHeight;
            return new UIVector2(Mathf.RoundToInt(r.x + ofx) * oow, Mathf.RoundToInt(r.y + ofy) * ooh);
        }
    }
}
