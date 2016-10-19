#if USE_UNISWF
using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using pumpkin.display;
using pumpkin.ui;

namespace SBS.uniSWF
{
	public class ScrollersDirector
    {
        #region Members
        protected TimeSource mTimeSource = null;
        protected List<ScrollingController> mControllers = new List<ScrollingController>();

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        protected ScrollingController[] mControllerUnderFinger = new ScrollingController[4];
	    protected Vector2[] mControllerLastDelta = new Vector2[4];
        protected Vector2[] mControllerTotalDelta = new Vector2[4];
#else
        protected ScrollingController mControllerUnderMouse = null;
        protected Vector2 mControllerLastMousePos;
        protected Vector2 mControllerTotalDelta = Vector2.zero;
#endif
        #endregion

        #region Ctor
        public ScrollersDirector(TimeSource timeSource)
        {
            mTimeSource = timeSource;
        }
        #endregion

        #region Methods
        protected ScrollingController getControllerUnderPoint(Vector2 point)
        {
            foreach (ScrollingController controller in mControllers)
            {
                if (!controller.active)
                    continue;

                if (controller.scrollArea.Contains(point))
                    return controller;
            }

            return null;
        }

        public ScrollingController addController(Rect scrollArea, ScrollingController.Settings settings)
        {
            ScrollingController controller = new ScrollingController(scrollArea, settings, mTimeSource);
            mControllers.Add(controller);
            return controller;
        }

        public void removeController(ScrollingController controller)
        {
            mControllers.Remove(controller);
        }

        public bool isDragging()
        {
            foreach (ScrollingController ctrl in mControllers)
                if (ctrl.isDragging)
                    return true;

            return false;
        }

        public void updateScrollers()
        {
            foreach (ScrollingController controller in mControllers)
            {
                if (!controller.active)
                    continue;

                bool isMovingUnderMouse = false;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
                for (int i = 0; i < 4; ++i)
                    isMovingUnderMouse = isMovingUnderMouse || (controller == mControllerUnderFinger[i] && controller.isDragging);
#else
                isMovingUnderMouse = (controller == mControllerUnderMouse && controller.isDragging);
#endif

                controller.isMovingUnderMouse = isMovingUnderMouse;
            }

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch t = Input.GetTouch(i);
			    if (t.fingerId < 0 || t.fingerId >= 4)
				    continue;

                Vector2 pos = t.position;
			    pos.y = Screen.height - pos.y;

                if (mControllerUnderFinger[t.fingerId] != null && !mControllerUnderFinger[t.fingerId].active)
                    mControllerUnderFinger[t.fingerId] = null;

			    ScrollingController ctrl = null;
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        ctrl = this.getControllerUnderPoint(pos);
                        if (ctrl != null)
                        {
                            ctrl.offsetVelocity = Vector2.zero;
                            ctrl.isDragging = false;

                            if (mControllerUnderFinger[t.fingerId] != null)
                                mControllerUnderFinger[t.fingerId].isDragging = false;

                            mControllerUnderFinger[t.fingerId] = ctrl;
                            mControllerLastDelta[t.fingerId] = Vector2.zero;
                            mControllerTotalDelta[t.fingerId] = Vector2.zero;
                        }
                        break;
                    case TouchPhase.Stationary:
                        ctrl = mControllerUnderFinger[t.fingerId];
                        if (ctrl != null)
                        {
                            ctrl.offsetVelocity = Vector2.zero;
                            //ctrl.isDragging = false;

                            //mControllerLastDelta[t.fingerId] = Vector2.zero;
                            //mControllerTotalDelta[t.fingerId] = Vector2.zero;
                        }
                        break;
                    case TouchPhase.Moved:
                        ctrl = mControllerUnderFinger[t.fingerId];
                        if (ctrl != null)
                        {
                            if (!ctrl.isDragging && mControllerTotalDelta[t.fingerId].sqrMagnitude >= 10.0f)
                            {
                                ctrl.isDragging = true;
                            }

                            Vector2 delta = t.deltaPosition;
                            delta.y = -delta.y;

                            DisplayObjectContainer doc = (DisplayObjectContainer)ctrl;
                            if (doc.stage != null)
                            {
                                delta.x /= doc.stage.scaleX;
                                delta.y /= doc.stage.scaleY;
                            }

                            //if (ctrl.isDragging)
                                ctrl.offset = ctrl.offset + delta;

                            mControllerLastDelta[t.fingerId] = delta;
                            mControllerTotalDelta[t.fingerId] += delta;
                        }
                        break;
                    case TouchPhase.Ended:
                        ctrl = mControllerUnderFinger[t.fingerId];
                        if (ctrl != null)
                        {
                            Vector2 deltaVel = (mControllerLastDelta[t.fingerId] / mTimeSource.DeltaTime);
                            ctrl.offsetVelocity += deltaVel;
                            ctrl.isDragging = false;
                        }
                        mControllerUnderFinger[t.fingerId] = null;
                        mControllerLastDelta[t.fingerId] = Vector2.zero;
                        mControllerTotalDelta[t.fingerId] = Vector2.zero;
                        break;
                    case TouchPhase.Canceled:
                        ctrl = mControllerUnderFinger[t.fingerId];
                        if (ctrl != null)
                            ctrl.isDragging = false;
                        mControllerUnderFinger[t.fingerId] = null;
                        mControllerLastDelta[t.fingerId] = Vector2.zero;
                        mControllerTotalDelta[t.fingerId] = Vector2.zero;
                        break;
                }
            }
#else
            if (mControllerUnderMouse != null && !mControllerUnderMouse.active)
            {
                if (mControllerUnderMouse.isDragging)
                    mControllerUnderMouse.isDragging = false;

                mControllerUnderMouse = null;
            }

            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            if (null == mControllerUnderMouse)
            {
                mControllerUnderMouse = this.getControllerUnderPoint(mousePos);
                mControllerLastMousePos = mousePos;
            }
            else
            {
                bool mouseOver = mControllerUnderMouse.scrollArea.Contains(mousePos);
                if (!mControllerUnderMouse.isDragging)
                {
                    if (!Input.GetMouseButton(0))
                    {
                        mControllerLastMousePos = mousePos;
                        mControllerUnderMouse = null;
                    }

                    if (mouseOver && Input.GetMouseButton(0) && Vector2.Distance(mousePos, mControllerLastMousePos) > 10.0f)
                    {
                        mControllerUnderMouse.isDragging = true;
                        mControllerUnderMouse.offsetVelocity = Vector2.zero;
                        mControllerLastMousePos = mousePos;
                    }
                }
                else
                {
                    Vector2 delta = mousePos - mControllerLastMousePos;

                    DisplayObjectContainer doc = (DisplayObjectContainer)mControllerUnderMouse;
                    if (doc.stage != null)
                    {
                        delta.x /= doc.stage.scaleX;
                        delta.y /= doc.stage.scaleY;
                    }

                    mControllerTotalDelta += delta;
                    mControllerUnderMouse.offset = mControllerUnderMouse.offset + delta;
                    mControllerLastMousePos = mousePos;

                    if (Input.GetMouseButtonUp(0))
                    {
                        Vector2 deltaVel = (delta / mTimeSource.DeltaTime);

                        mControllerUnderMouse.offsetVelocity += deltaVel;

                        mControllerTotalDelta = Vector2.zero;
                        mControllerUnderMouse.isDragging = false;
                        mControllerUnderMouse = null;
                    }
                }
            }
#endif
        }
        #endregion
    }
}
#endif