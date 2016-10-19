using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

namespace SBS.UI
{
	public class Scroller : UICollection
    {
        #region Public members
        public bool isDragging = false;
        #endregion

        #region Protected members
        protected string name;
        protected Rect scrollArea;
        protected Rect totalArea;
        protected Rect offsetRect;
        protected Rect offsetRectExtra;
        //protected Vector2 prevOffset = Vector2.zero;
        protected Vector2 offset = Vector2.zero;
        protected Vector2 offsetVel = Vector2.zero;
        protected bool scrollX = true;
        protected bool scrollY = true;
        protected bool visible = true;
        protected int inputLayer = 0;

        protected float damping = 0.8f;

        protected bool active = true;

        protected Vector2 prevSpringForce = Vector2.zero;

        protected SBSMatrix4x4 matrix = SBSMatrix4x4.identity;
        #endregion

        #region Ctor
        public Scroller(string _name, Rect _scrollArea)
            : base()
        {
            name = _name;
            scrollArea = _scrollArea;
            totalArea = new Rect(float.MaxValue, float.MaxValue, -float.MaxValue * 2.0f, -float.MaxValue * 2.0f);
        }
        #endregion

        #region Public properties
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value == name)
                    return;

                Vector3 matrixPos = matrix.position;
                Rect finalRect = new Rect(scrollArea.xMin + matrixPos.x, scrollArea.yMin + matrixPos.y, scrollArea.width, scrollArea.height);

#if UNITY_FLASH
                foreach (KeyValuePair<string, UIElement> item in elements)
                {
                    UIElement elem = item.Value;
#else
                foreach (UIElement elem in elements.Values)
#endif
                    //elem.Batch.RemoveScissorRect(name);
                    elem.RemoveScissorRect();
#if UNITY_FLASH
                }
#endif

#if UNITY_FLASH
                foreach (KeyValuePair<string, Button> item in buttons)
                {
                    Button btn = item.Value;
#else
                foreach (Button btn in buttons.Values)
                {
#endif
                    List<UIElement> btnElements = btn.Elements;
                    foreach (UIElement btnElem in btnElements)
                        //btnElem.Batch.RemoveScissorRect(name);
                        btnElem.RemoveScissorRect();
                }

                name = value;

#if UNITY_FLASH
                foreach (KeyValuePair<string, UIElement> item in elements)
                {
                    UIElement elem = item.Value;
#else
                foreach (UIElement elem in elements.Values)
#endif
                    //elem.ScissorRectIndex = elem.Batch.SetScissorRect(name, scrollArea);
                    elem.SetScissorRect(name, finalRect);//scrollArea);
#if UNITY_FLASH
                }
#endif

#if UNITY_FLASH
                foreach (KeyValuePair<string, Button> item in buttons)
                {
                    Button btn = item.Value;
#else
                foreach (Button btn in buttons.Values)
                {
#endif
                    List<UIElement> btnElements = btn.Elements;
                    foreach (UIElement btnElem in btnElements)
                        //btnElem.ScissorRectIndex = btnElem.Batch.SetScissorRect(name, scrollArea);
                        btnElem.SetScissorRect(name, finalRect);//scrollArea);
                }
            }
        }

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                if (active != value)
                {
                    active = value;
                    UIManager_OLD.Instance.NotifyScrollerActive(this, active);
                }
            }
        }

        public Rect ScrollArea
        {
            get
            {
                return scrollArea;
            }
            set
            {
                Vector3 matrixPos = matrix.position;
                Rect finalRect = new Rect(value.xMin + matrixPos.x, value.yMin + matrixPos.y, value.width, value.height);

#if UNITY_FLASH
                foreach (KeyValuePair<string, UIElement> item in elements)
                {
                    UIElement elem = item.Value;
#else
                foreach (UIElement elem in elements.Values)
#endif
                    //elem.ScissorRectIndex = elem.Batch.SetScissorRect(name, value);
                    elem.SetScissorRect(name, finalRect);//value);
#if UNITY_FLASH
                }
#endif

#if UNITY_FLASH
                foreach (KeyValuePair<string, Button> item in buttons)
                {
                    Button btn = item.Value;
#else
                foreach (Button btn in buttons.Values)
                {
#endif
                    List<UIElement> btnElements = btn.Elements;
                    foreach (UIElement btnElem in btnElements)
                        //btnElem.ScissorRectIndex = btnElem.Batch.SetScissorRect(name, value);
                        btnElem.SetScissorRect(name, finalRect);//value);
                }

                scrollArea = value;

                offsetRect.xMin = scrollX ? (scrollArea.xMax - totalArea.xMax) : 0.0f;
                offsetRect.yMin = scrollY ? (scrollArea.yMax - totalArea.yMax) : 0.0f;
                offsetRect.xMax = scrollX ? (scrollArea.xMin - totalArea.xMin) : 0.0f;
                offsetRect.yMax = scrollY ? (scrollArea.yMin - totalArea.yMin) : 0.0f;

                if (offsetRect.xMax < offsetRect.xMin)
                    offsetRect.xMin = offsetRect.xMax;

                if (offsetRect.yMax < offsetRect.yMin)
                    offsetRect.yMin = offsetRect.yMax;

                float minWidth = SBSMath.Min(totalArea.width, scrollArea.width),
                      minHeight = SBSMath.Min(totalArea.height, scrollArea.height);
                offsetRectExtra.xMin = scrollX ? offsetRect.xMin - minWidth : 0.0f;
                offsetRectExtra.yMin = scrollY ? offsetRect.yMin - minHeight : 0.0f;
                offsetRectExtra.xMax = scrollX ? offsetRect.xMax + minWidth : 0.0f;
                offsetRectExtra.yMax = scrollY ? offsetRect.yMax + minHeight : 0.0f;

                offset.x = Mathf.Clamp(offset.x, offsetRectExtra.xMin, offsetRectExtra.xMax);
                offset.y = Mathf.Clamp(offset.y, offsetRectExtra.yMin, offsetRectExtra.yMax);
            }
        }

        public float DepthOffset
        {
            get
            {
                return UIManager_OLD.Instance.GetOverlaysLayer().GetScissorRectDepthOffset(name);
            }
            set
            {
                UIManager_OLD.Instance.GetOverlaysLayer().SetScissorRectDepthOffset(name, value);
            }
        }

        public Rect TotalArea
        {
            get
            {
                return totalArea;
            }
        }

        public Vector2 Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset.x = Mathf.Clamp(value.x, offsetRectExtra.xMin, offsetRectExtra.xMax);
                offset.y = Mathf.Clamp(value.y, offsetRectExtra.yMin, offsetRectExtra.yMax);
            }
        }

        public Rect OffsetRect
        {
            get
            {
                return offsetRect;
            }
        }

        public Rect OffsetRectExtra
        {
            get
            {
                return offsetRectExtra;
            }
        }

        public Vector2 OffsetVelocity
        {
            get
            {
                return offsetVel;
            }
            set
            {
                offsetVel.x = Mathf.Clamp(value.x, -1000.0f, 1000.0f);
                offsetVel.y = Mathf.Clamp(value.y, -1000.0f, 1000.0f);
            }
        }

        public bool ScrollX
        {
            get
            {
                return scrollX;
            }
            set
            {
                scrollX = value;
            }
        }

        public bool ScrollY
        {
            get
            {
                return scrollY;
            }
            set
            {
                scrollY = value;
            }
        }

        public float Damping
        {
            get
            {
                return damping;
            }
            set
            {
                damping = Mathf.Clamp01(value);
            }
        }

        public bool IsVisible
        {
            get
            {
                return visible;
            }
        }

        public int InputLayer
        {
            get
            {
                return inputLayer;
            }
            set
            {
                inputLayer = value;
            }
        }
        #endregion

        #region Public overridden methods
        public override void AddElement(string elemName, UIElement element)
        {
            base.AddElement(elemName, element);

            Vector3 matrixPos = matrix.position;
            Rect finalRect = new Rect(scrollArea.xMin + matrixPos.x, scrollArea.yMin + matrixPos.y, scrollArea.width, scrollArea.height);
            //element.ScissorRectIndex = element.Batch.SetScissorRect(name, scrollArea);
            element.SetScissorRect(name, finalRect);//scrollArea);

            if (element.Visibility)
            {
                totalArea.xMin = SBSMath.Min(offsetRect.xMin, element.ScreenRect.xMin);
                totalArea.yMin = SBSMath.Min(offsetRect.yMin, element.ScreenRect.yMin);
                totalArea.xMax = SBSMath.Max(offsetRect.xMax, element.ScreenRect.xMax);
                totalArea.yMax = SBSMath.Max(offsetRect.yMax, element.ScreenRect.yMax);

                offsetRect.xMin = scrollX ? (scrollArea.xMax - totalArea.xMax) : 0.0f;
                offsetRect.yMin = scrollY ? (scrollArea.yMax - totalArea.yMax) : 0.0f;
                offsetRect.xMax = scrollX ? (scrollArea.xMin - totalArea.xMin) : 0.0f;
                offsetRect.yMax = scrollY ? (scrollArea.yMin - totalArea.yMin) : 0.0f;

                if (offsetRect.xMax < offsetRect.xMin)
                    offsetRect.xMin = offsetRect.xMax;

                if (offsetRect.yMax < offsetRect.yMin)
                    offsetRect.yMin = offsetRect.yMax;

                float minWidth = SBSMath.Min(totalArea.width, scrollArea.width),
                      minHeight = SBSMath.Min(totalArea.height, scrollArea.height);
                offsetRectExtra.xMin = scrollX ? offsetRect.xMin - minWidth : 0.0f;
                offsetRectExtra.yMin = scrollY ? offsetRect.yMin - minHeight : 0.0f;
                offsetRectExtra.xMax = scrollX ? offsetRect.xMax + minWidth : 0.0f;
                offsetRectExtra.yMax = scrollY ? offsetRect.yMax + minHeight : 0.0f;

                offset.x = Mathf.RoundToInt(Mathf.Clamp(offset.x, offsetRectExtra.xMin, offsetRectExtra.xMax));
                offset.y = Mathf.RoundToInt(Mathf.Clamp(offset.y, offsetRectExtra.yMin, offsetRectExtra.yMax));
            }
        }

        public override void AddButton(string btnName, Button btn)
        {
            base.AddButton(btnName, btn);

            Vector3 matrixPos = matrix.position;
            Rect finalRect = new Rect(scrollArea.xMin + matrixPos.x, scrollArea.yMin + matrixPos.y, scrollArea.width, scrollArea.height);

            List<UIElement> btnElements = btn.Elements;
            foreach (UIElement btnElem in btnElements)
                //btnElem.ScissorRectIndex = btnElem.Batch.SetScissorRect(name, scrollArea);
                btnElem.SetScissorRect(name, finalRect);//scrollArea);

            if (btn.Visibility)
            {
                totalArea.xMin = SBSMath.Min(offsetRect.xMin, btn.HitRect.xMin);
                totalArea.yMin = SBSMath.Min(offsetRect.yMin, btn.HitRect.yMin);
                totalArea.xMax = SBSMath.Max(offsetRect.xMax, btn.HitRect.xMax);
                totalArea.yMax = SBSMath.Max(offsetRect.yMax, btn.HitRect.yMax);

                offsetRect.xMin = scrollX ? (scrollArea.xMax - totalArea.xMax) : 0.0f;
                offsetRect.yMin = scrollY ? (scrollArea.yMax - totalArea.yMax) : 0.0f;
                offsetRect.xMax = scrollX ? (scrollArea.xMin - totalArea.xMin) : 0.0f;
                offsetRect.yMax = scrollY ? (scrollArea.yMin - totalArea.yMin) : 0.0f;

                if (offsetRect.xMax < offsetRect.xMin)
                    offsetRect.xMin = offsetRect.xMax;

                if (offsetRect.yMax < offsetRect.yMin)
                    offsetRect.yMin = offsetRect.yMax;

                float minWidth = SBSMath.Min(totalArea.width, scrollArea.width),
                      minHeight = SBSMath.Min(totalArea.height, scrollArea.height);
                offsetRectExtra.xMin = scrollX ? offsetRect.xMin - minWidth : 0.0f;
                offsetRectExtra.yMin = scrollY ? offsetRect.yMin - minHeight : 0.0f;
                offsetRectExtra.xMax = scrollX ? offsetRect.xMax + minWidth : 0.0f;
                offsetRectExtra.yMax = scrollY ? offsetRect.yMax + minHeight : 0.0f;

                offset.x = Mathf.RoundToInt(Mathf.Clamp(offset.x, offsetRectExtra.xMin, offsetRectExtra.xMax));
                offset.y = Mathf.RoundToInt(Mathf.Clamp(offset.y, offsetRectExtra.yMin, offsetRectExtra.yMax));
            }
        }

        public override void SetVisibility(bool visibility)
        {
            base.SetVisibility(visibility);
            visible = visibility;
        }

        public override void SetTransform(SBSMatrix4x4 transform)
        {
            matrix = transform;
            SBSVector3 matrixPos = matrix.position;
            Rect finalRect = new Rect(scrollArea.xMin + matrixPos.x, scrollArea.yMin + matrixPos.y, scrollArea.width, scrollArea.height);
/*
#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
            {
                UIElement elem = item.Value;
#else
            foreach (UIElement elem in elements.Values)
            {
#endif
                //elem.ScissorRectIndex = elem.Batch.SetScissorRect(name, value);
                if (!elem.Visibility)
                    continue;
                elem.SetScissorRect(name, finalRect);
            }

#if UNITY_FLASH
            foreach (KeyValuePair<string, Button> item in buttons)
            {
                Button btn = item.Value;
#else
            foreach (Button btn in buttons.Values)
            {
#endif
                if (!btn.Visibility)
                    continue;
                List<UIElement> btnElements = btn.Elements;
                foreach (UIElement btnElem in btnElements)
                    //btnElem.ScissorRectIndex = btnElem.Batch.SetScissorRect(name, value);
                    btnElem.SetScissorRect(name, finalRect);
            }
*/
#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
            {
                    UIElement element = item.Value;
#else
            foreach (UIElement element in elements.Values)
            {
#endif
                Rect newRect = element.ScreenRect;
                newRect.x += offset.x;
                newRect.y += offset.y;
                if (!element.Visibility || (/*element.Clipped &&*/ SBSMath.ClipStatus.Outside == SBSMath.GetClipStatus(scrollArea, newRect)))
                    continue;
                //element.SetScissorRect(name, finalRect);
                element.SetScissorRectWithTransform(name, finalRect, transform);
                //element.Transform = transform;
            }

#if UNITY_FLASH
            foreach (KeyValuePair<string, Button> item in buttons)
            {
                Button btn = item.Value;
#else
            foreach (Button btn in buttons.Values)
            {
#endif
                Rect newRect = btn.HitRect;
                newRect.x += offset.x;
                newRect.y += offset.y;
                if (!btn.Visibility || (/*btn.Clipped &&*/ SBSMath.ClipStatus.Outside == SBSMath.GetClipStatus(scrollArea, newRect)))
                    continue;
                //List<UIElement> btnElements = btn.Elements;
                //foreach (UIElement btnElem in btnElements)
                //    btnElem.SetScissorRect(name, finalRect);
                //btn.Transform = transform;
                btn.SetScissorRectWithTransform(name, finalRect, transform);
            }
        }
        #endregion

        #region Public methods
        public void Update(float dt, bool isMovingUnderMouse /*= false*/)//, bool forceTransformUpdates = false)
        {
            totalArea = new Rect(float.MaxValue, float.MaxValue, -float.MaxValue * 2.0f, -float.MaxValue * 2.0f);

#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
            {
                UIElement elem = item.Value;
#else
            foreach (UIElement elem in elements.Values)
            {
#endif
                if (!elem.Visibility)
                    continue;

                Rect scrRect = elem.ScreenRect;

                totalArea.xMin = SBSMath.Min(totalArea.xMin, scrRect.xMin);
                totalArea.yMin = SBSMath.Min(totalArea.yMin, scrRect.yMin);
                totalArea.xMax = SBSMath.Max(totalArea.xMax, scrRect.xMax);
                totalArea.yMax = SBSMath.Max(totalArea.yMax, scrRect.yMax);
            }

#if UNITY_FLASH
            foreach (KeyValuePair<string, Button> item in buttons)
            {
                Button btn = item.Value;
#else
            foreach (Button btn in buttons.Values)
            {
#endif
                if (!btn.Visibility)
                    continue;

                Rect hitRect = btn.HitRect;

                totalArea.xMin = SBSMath.Min(totalArea.xMin, hitRect.xMin);
                totalArea.yMin = SBSMath.Min(totalArea.yMin, hitRect.yMin);
                totalArea.xMax = SBSMath.Max(totalArea.xMax, hitRect.xMax);
                totalArea.yMax = SBSMath.Max(totalArea.yMax, hitRect.yMax);
            }

            offsetRect.xMin = scrollX ? (scrollArea.xMax - totalArea.xMax) : 0.0f;
            offsetRect.yMin = scrollY ? (scrollArea.yMax - totalArea.yMax) : 0.0f;
            offsetRect.xMax = scrollX ? (scrollArea.xMin - totalArea.xMin) : 0.0f;
            offsetRect.yMax = scrollY ? (scrollArea.yMin - totalArea.yMin) : 0.0f;

            if (offsetRect.xMax < offsetRect.xMin)
                offsetRect.xMin = offsetRect.xMax;

            if (offsetRect.yMax < offsetRect.yMin)
                offsetRect.yMin = offsetRect.yMax;

            float minWidth = SBSMath.Min(totalArea.width, scrollArea.width),
                  minHeight = SBSMath.Min(totalArea.height, scrollArea.height);
            offsetRectExtra.xMin = scrollX ? offsetRect.xMin - minWidth : 0.0f;
            offsetRectExtra.yMin = scrollY ? offsetRect.yMin - minHeight : 0.0f;
            offsetRectExtra.xMax = scrollX ? offsetRect.xMax + minWidth : 0.0f;
            offsetRectExtra.yMax = scrollY ? offsetRect.yMax + minHeight : 0.0f;

            offset += offsetVel * dt;

            offsetVel *= SBSMath.Pow(1.0f - damping, dt);
            if (offsetVel.sqrMagnitude < 1.0f)
                offsetVel = Vector2.zero;

            offset.x = Mathf.RoundToInt(Mathf.Clamp(offset.x, offsetRectExtra.xMin, offsetRectExtra.xMax));
            offset.y = Mathf.RoundToInt(Mathf.Clamp(offset.y, offsetRectExtra.yMin, offsetRectExtra.yMax));

            if (dt < SBSMath.Epsilon || isMovingUnderMouse)
                return;
            float springK       = 50.0f,
                  springBounce  = 0.0f,
                  springRebound = 15.0f;
            Vector2 springForce = Vector2.zero;
            if (offset.x < offsetRect.xMin)
            {
                springForce.x += (offsetRect.xMin - offset.x) * springK;
                if (offsetVel.x >= 0.0f)
                    springForce.x -= springRebound * offsetVel.x;
                else
                    springForce.x -= springBounce * offsetVel.x;
            }
            else if (offset.x > offsetRect.xMax)
            {
                springForce.x -= (offset.x - offsetRect.xMax) * springK;
                if (offsetVel.x <= 0.0f)
                    springForce.x -= springRebound * offsetVel.x;
                else
                    springForce.x -= springBounce * offsetVel.x;
            }
            else if (SBSMath.Abs(prevSpringForce.x) > 0.0f)
            {
                springForce.x = 0.0f;
				//if (offsetVel.x * prevSpringForce.x < 0.0f)
                //	offsetVel.x = 0.0f;
                offset.x = Mathf.Clamp(offset.x, offsetRect.xMin, offsetRect.xMax);
            }

            if (offset.y < offsetRect.yMin)
            {
                springForce.y += (offsetRect.yMin - offset.y) * springK;
                if (offsetVel.y >= 0.0f)
                    springForce.y -= springRebound * offsetVel.y;
                else
                    springForce.y -= springBounce * offsetVel.y;
            }
            else if (offset.y > offsetRect.yMax)
            {
                springForce.y -= (offset.y - offsetRect.yMax) * springK;
                if (offsetVel.y <= 0.0f)
                    springForce.y -= springRebound * offsetVel.y;
                else
                    springForce.y -= springBounce * offsetVel.y;
            }
            else if (SBSMath.Abs(prevSpringForce.y) > 0.0f)
            {
                springForce.y = 0.0f;
				//if (offsetVel.y * prevSpringForce.y < 0.0f)
                //	offsetVel.y = 0.0f;
                offset.y = Mathf.Clamp(offset.y, offsetRect.yMin, offsetRect.yMax);
            }

            offsetVel += springForce * dt;
            prevSpringForce = springForce;

            /*
            if (Vector2.Distance(prevOffset, offset) > 1.0f || forceTransformUpdates)
			{
	            SBSMatrix4x4 scroll = SBSMatrix4x4.TRS(new SBSVector3(offset.x, offset.y), SBSQuaternion.identity, SBSVector3.one);
	            this.SetTransform(scroll);
				
				prevOffset = offset;
			}*/
        }
        #endregion
    }
}
