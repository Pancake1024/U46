#if USE_UNISWF
using System;
using System.Collections.Generic;
using pumpkin.display;
using pumpkin.geom;
using pumpkin.events;
using UnityEngine;
using SBS.Core;

namespace SBS.uniSWF
{
    public class ScrollingController
    {
        public const float maxSpeed = 2000.0f;

        [Flags]
        public enum Settings
        {
            SnapOnX = 0x1,
            LockOnX = 0x3,
            SnapOnY = 0x4,
            LockOnY = 0xC
        }

        #region Events
        #endregion

        #region Members
        protected bool mActive = true;
        protected bool mCircularOnX = false;
        protected bool mCircularOnY = false;
        internal ScrollingContainer mContainer = null;
        protected Rect mTotalArea;
        protected Rect mScrollArea;
        
        protected Rect mOffsetRect;
        protected Rect mOffsetRectExtra;
        protected Vector2 mOffset = Vector2.zero;
        protected Vector2 mOffsetVel = Vector2.zero;
        protected float mDamping = 0.8f;

        protected float mSpringK = 50.0f;
        protected float mSpringBounce = 0.0f;
        protected float mSpringRebound = 15.0f;
        protected Vector2 mLastSpringForce = Vector2.zero;

        protected Vector2 mSnapSpace = Vector2.zero;
        protected Tuple<int, int> mSnapIndices = new Tuple<int,int>(0, 0);
        protected Interval<int> mSnapXInterval = Interval<int>.CreateLowerBound(0);
        protected Interval<int> mSnapXInterval2 = Interval<int>.CreateLowerBound(0);
        protected int mIndexOffsetX = 0;
        protected bool mIsEasingOnX = false;
        protected Interval<int> mSnapYInterval = Interval<int>.CreateLowerBound(0);
        protected Interval<int> mSnapYInterval2 = Interval<int>.CreateLowerBound(0);
        protected int mIndexOffsetY = 0;
        protected bool mIsEasingOnY = false;
        protected Vector2 mSnapTargets = Vector2.zero;

        protected Settings mSettings;
        protected TimeSource mTimeSource;
        protected float mLastUpdateFrame;
        protected float mLastDt;

        protected bool mIsDragging = false;
        protected bool mIsMovingUnderMouse = false;
        #endregion

        #region Properties
        internal bool isDragging
        {
            get
            {
                return mIsDragging;
            }
            set
            {
                mIsDragging = value;

                if (mIsDragging)
                {
                    mContainer.disableInputs();

                    mIsEasingOnX = false;
                    mIsEasingOnY = false;
                }
                else
                {
                    mContainer.enableInputs();
                }
            }
        }

        internal bool isMovingUnderMouse
        {
            get
            {
                return mIsMovingUnderMouse;
            }
            set
            {
                mIsMovingUnderMouse = value;
            }
        }

        public bool active
        {
            get
            {
                return mActive;
            }
            set
            {
                mActive = value;

                if (!mActive)
                {
                    mIsMovingUnderMouse = false;
                    mIsDragging = false;

                    mContainer.enableInputs();
                }
            }
        }

        public bool isEasing
        {
            get
            {
                return mIsEasingOnX || mIsEasingOnY;
            }
        }

        public bool circularOnX
        {
            get
            {
                return mCircularOnX;
            }
            set
            {
                if (mCircularOnX == value)
                    return;

                if (value)
                {
                    mCircularOnX = true;
                    mCircularOnY = false;
                }
                else
                {
                    mCircularOnX = false;
                }
            }
        }

        public bool circularOnY
        {
            get
            {
                return mCircularOnY;
            }
            set
            {
                if (mCircularOnY == value)
                    return;

                if (value)
                {
                    mCircularOnX = false;
                    mCircularOnY = true;
                }
                else
                {
                    mCircularOnY = false;
                }
            }
        }
        
        public Rect scrollArea
        {
            get
            {
                return mScrollArea;
            }
            set
            {
                mScrollArea = value;

                this.updateOffsetRect();
                this.updateSnapData();
            }
        }

        public Vector2 offset
        {
            get
            {
                return mOffset;
            }
            set
            {
                if (0 == (mSettings & Settings.LockOnX) || Settings.LockOnX == (mSettings & Settings.LockOnX))
                    mOffset.x = Mathf.Clamp(value.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
                else
                    mOffset.x = value.x;

                if (0 == (mSettings & Settings.LockOnY) || Settings.LockOnY == (mSettings & Settings.LockOnY))
                    mOffset.y = Mathf.Clamp(value.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);
                else
                    mOffset.y = value.y;

                mIsEasingOnX = mIsEasingOnY = false;

                this.updateSnapData();
            }
        }

        public Vector2 offsetVelocity
        {
            get
            {
                return mOffsetVel;
            }
            set
            {
                mOffsetVel.x = Mathf.Clamp(value.x, -maxSpeed, maxSpeed);
                mOffsetVel.y = Mathf.Clamp(value.y, -maxSpeed, maxSpeed);
            }
        }

        public Vector2 snapSpace
        {
            get
            {
                return mSnapSpace;
            }
            set
            {
                mSnapSpace = value;

                this.updateSnapData();
            }
        }

        public Interval<int> snapXInterval
        {
            get
            {
                return mSnapXInterval;
            }
            set
            {
                mSnapXInterval = value;
                mSnapXInterval2 = value;
                mIndexOffsetX = 0;
                mSnapIndices.First = mSnapXInterval2.Clamp(mSnapIndices.First);
                mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
            }
        }

        public Interval<int> snapYInterval
        {
            get
            {
                return mSnapYInterval;
            }
            set
            {
                mSnapYInterval = value;
                mSnapYInterval2 = value;
                mIndexOffsetY = 0;
                mSnapIndices.Second = mSnapYInterval2.Clamp(mSnapIndices.Second);
                mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
            }
        }

        public int snapX
        {
            get
            {
                int span = mSnapXInterval2.End - mSnapXInterval2.Start + 1;
                if (span > 0 && mSnapXInterval2.HasStart)
                    return (mSnapIndices.First - mSnapXInterval2.Start + mIndexOffsetX) % span;
                else if (span > 0 && mSnapXInterval2.HasEnd)
                    return -((mSnapXInterval2.End - mSnapIndices.First + mIndexOffsetX) % span);
                else
                    return mSnapIndices.First;
            }
            set
            {
                this.setSnapX(value);

                mIsEasingOnX = false;
                mOffset.x = mSnapTargets.x;

                if (mCircularOnX)
                {
                    mContainer.x = Mathf.RoundToInt(mOffset.x);
                    mContainer.updateBounds();

                    this.updateCircularBehaviour(true);

                    mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
                    mOffset.x = mSnapTargets.x;
                }
            }
        }

        public int snapY
        {
            get
            {
                int span = mSnapYInterval2.End - mSnapYInterval2.Start + 1;
                if (span > 0 && mSnapYInterval2.HasStart)
                    return (mSnapIndices.Second - mSnapYInterval2.Start + mIndexOffsetY) % span;
                else if (span > 0 && mSnapYInterval2.HasEnd)
                    return -((mSnapYInterval2.End - mSnapIndices.Second + mIndexOffsetY) % span);
                else
                    return mSnapIndices.Second;
            }
            set
            {
                this.setSnapY(value);

                mIsEasingOnY = false;
                mOffset.y = mSnapTargets.y;

                if (mCircularOnY)
                {
                    mContainer.y = Mathf.RoundToInt(mOffset.y);
                    mContainer.updateBounds();

                    this.updateCircularBehaviour(true);

                    mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
                    mOffset.y = mSnapTargets.y;
                }
            }
        }
        #endregion

        #region Ctor
        internal ScrollingController(Rect scrollArea, Settings settings, TimeSource timeSource)
        {
            mContainer = new ScrollingContainer(this);
            mScrollArea = scrollArea;
            mSettings = settings;
            mTimeSource = timeSource;
            mLastUpdateFrame = mTimeSource.TotalTime;
        }
        #endregion

        #region Methods
        internal virtual void setChildrenBounds(Rectangle bounds)
        {
            mTotalArea.xMin = bounds.x - mOffset.x;
            mTotalArea.yMin = bounds.y - mOffset.y;
            mTotalArea.xMax = mTotalArea.xMin + bounds.width;
            mTotalArea.yMax = mTotalArea.yMin + bounds.height;

            this.updateOffsetRect();
        }

        internal virtual void updateFrame(CEvent e)
        {
            float dt = mLastDt = mTimeSource.TotalTime - mLastUpdateFrame;
            mLastUpdateFrame = mTimeSource.TotalTime;

            if (!mActive)
                return;

            mOffset += mOffsetVel * dt;
            mOffsetVel *= Mathf.Pow(1.0f - mDamping, dt);
            if (mOffsetVel.sqrMagnitude < 1.0f)
                mOffsetVel = Vector2.zero;

            if (0 == (mSettings & Settings.LockOnX) || Settings.LockOnX == (mSettings & Settings.LockOnX))
                mOffset.x = Mathf.Clamp(mOffset.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
            if (0 == (mSettings & Settings.LockOnY) || Settings.LockOnY == (mSettings & Settings.LockOnY))
                mOffset.y = Mathf.Clamp(mOffset.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);

            this.updateSnapData();

            this.updateCircularBehaviour(false);

            mContainer.x = Mathf.RoundToInt(mOffset.x);
            mContainer.y = Mathf.RoundToInt(mOffset.y);
            
            if (dt < Mathf.Epsilon || mIsMovingUnderMouse)
                return;

            Vector2 springForce = Vector2.zero;
            if (Settings.SnapOnX == (mSettings & Settings.SnapOnX))
            {
                float dx = mSnapTargets.x - mOffset.x;
                if (Mathf.Abs(dx) <= 1.0f)
                {
                    springForce.x = 0.0f;
                    mOffset.x = mSnapTargets.x;
                    mIsEasingOnX = false;
                }
                else
                {
                    springForce.x += dx * mSpringK;
                    if ((dx * mOffsetVel.x) >= 0.0f)
                        springForce.x -= mSpringRebound * mOffsetVel.x;
                    else
                        springForce.x -= mSpringBounce * mOffsetVel.x;
                }
            }
            else if (mOffset.x < mOffsetRect.xMin)
            {
                springForce.x += (mOffsetRect.xMin - mOffset.x) * mSpringK;
                if (mOffsetVel.x >= 0.0f)
                    springForce.x -= mSpringRebound * mOffsetVel.x;
                else
                    springForce.x -= mSpringBounce * mOffsetVel.x;
            }
            else if (mOffset.x > mOffsetRect.xMax)
            {
                springForce.x -= (mOffset.x - mOffsetRect.xMax) * mSpringK;
                if (mOffsetVel.x <= 0.0f)
                    springForce.x -= mSpringRebound * mOffsetVel.x;
                else
                    springForce.x -= mSpringBounce * mOffsetVel.x;
            }
            else if (Mathf.Abs(mLastSpringForce.x) > 0.0f)
            {
                springForce.x = 0.0f;
                mOffset.x = Mathf.Clamp(mOffset.x, mOffsetRect.xMin, mOffsetRect.xMax);
            }

            if (Settings.SnapOnY == (mSettings & Settings.SnapOnY))
            {
                float dy = mSnapTargets.y - mOffset.y;
                if (Mathf.Abs(dy) <= 1.0f)
                {
                    springForce.y = 0.0f;
                    mOffset.y = mSnapTargets.y;
                    mIsEasingOnY = false;
                }
                else
                {
                    springForce.y += dy * mSpringK;
                    if ((dy * mOffsetVel.y) >= 0.0f)
                        springForce.y -= mSpringRebound * mOffsetVel.y;
                    else
                        springForce.y -= mSpringBounce * mOffsetVel.y;
                }
            }
            else if (mOffset.y < mOffsetRect.yMin)
            {
                springForce.y += (mOffsetRect.yMin - mOffset.y) * mSpringK;
                if (mOffsetVel.y >= 0.0f)
                    springForce.y -= mSpringRebound * mOffsetVel.y;
                else
                    springForce.y -= mSpringBounce * mOffsetVel.y;
            }
            else if (mOffset.y > mOffsetRect.yMax)
            {
                springForce.y -= (mOffset.y - mOffsetRect.yMax) * mSpringK;
                if (mOffsetVel.y <= 0.0f)
                    springForce.y -= mSpringRebound * mOffsetVel.y;
                else
                    springForce.y -= mSpringBounce * mOffsetVel.y;
            }
            else if (Mathf.Abs(mLastSpringForce.y) > 0.0f)
            {
                springForce.y = 0.0f;
                mOffset.y = Mathf.Clamp(mOffset.y, mOffsetRect.yMin, mOffsetRect.yMax);
            }

            mOffsetVel += springForce * dt;
            mLastSpringForce = springForce;
        }

        protected void setSnapX(int value)
        {
            int span = mSnapXInterval2.End - mSnapXInterval2.Start + 1,
                index = span > 0 ? (value - mIndexOffsetX + span) % span : value;

            if (mSnapXInterval2.HasStart)
                index = mSnapXInterval2.Start + index;
            else if (mSnapXInterval2.HasEnd)
                index = mSnapXInterval2.End + index;

            mSnapIndices.First = mSnapXInterval2.Clamp(index);
            mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
        }

        protected void setSnapY(int value)
        {
            int span = mSnapYInterval2.End - mSnapYInterval2.Start + 1,
                index = span > 0 ? (value - mIndexOffsetY + span) % span : value;

            if (mSnapYInterval2.HasStart)
                index = mSnapXInterval2.Start + index;
            else if (mSnapYInterval2.HasEnd)
                index = mSnapYInterval2.End + index;

            mSnapIndices.Second = mSnapYInterval2.Clamp(index);
            mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
        }

        protected void updateOffsetRect()
        {
            bool scrollX = Settings.LockOnX != (mSettings & Settings.LockOnX),
                 scrollY = Settings.LockOnY != (mSettings & Settings.LockOnY);

            mOffsetRect.xMin = scrollX ? (mScrollArea.xMax - mTotalArea.xMax) : 0.0f;
            mOffsetRect.yMin = scrollY ? (mScrollArea.yMax - mTotalArea.yMax) : 0.0f;
            mOffsetRect.xMax = scrollX ? (mScrollArea.xMin - mTotalArea.xMin) : 0.0f;
            mOffsetRect.yMax = scrollY ? (mScrollArea.yMin - mTotalArea.yMin) : 0.0f;

            if (mOffsetRect.xMax < mOffsetRect.xMin)
                mOffsetRect.xMin = mOffsetRect.xMax;

            if (mOffsetRect.yMax < mOffsetRect.yMin)
                mOffsetRect.yMin = mOffsetRect.yMax;

            float minWidth = Mathf.Min(mTotalArea.width, mScrollArea.width),
                  minHeight = Mathf.Min(mTotalArea.height, mScrollArea.height);

            mOffsetRectExtra.xMin = scrollX ? mOffsetRect.xMin - minWidth : 0.0f;
            mOffsetRectExtra.yMin = scrollY ? mOffsetRect.yMin - minHeight : 0.0f;
            mOffsetRectExtra.xMax = scrollX ? mOffsetRect.xMax + minWidth : 0.0f;
            mOffsetRectExtra.yMax = scrollY ? mOffsetRect.yMax + minHeight : 0.0f;

            if (0 == (mSettings & Settings.LockOnX) || Settings.LockOnX == (mSettings & Settings.LockOnX))
                mOffset.x = Mathf.Clamp(mOffset.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
            if (0 == (mSettings & Settings.LockOnY) || Settings.LockOnY == (mSettings & Settings.LockOnY))
                mOffset.y = Mathf.Clamp(mOffset.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);
        }

        protected void updateSnapData()
        {
            if (!mIsEasingOnX && Settings.SnapOnX == (mSettings & Settings.SnapOnX) && mSnapSpace.x > 0.0f)
            {
                float snapOffset = mOffset.x / mSnapSpace.x;

                int snapIdx0 = Mathf.FloorToInt(snapOffset),
                    snapIdx1 = snapIdx0 + 1;
                snapOffset -= (float)snapIdx0;

                if (snapOffset <= 0.5f)
                    mSnapIndices.First = mSnapXInterval2.Clamp(snapIdx0);
                else
                    mSnapIndices.First = mSnapXInterval2.Clamp(snapIdx1);

                mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
            }

            if (!mIsEasingOnY && Settings.SnapOnY == (mSettings & Settings.SnapOnY) && mSnapSpace.y > 0.0f)
            {
                float snapOffset = mOffset.y / mSnapSpace.y;

                int snapIdx0 = Mathf.FloorToInt(snapOffset),
                    snapIdx1 = snapIdx0 + 1;
                snapOffset -= (float)snapIdx0;

                if (snapOffset <= 0.5f)
                    mSnapIndices.Second = mSnapYInterval.Clamp(snapIdx0);
                else
                    mSnapIndices.Second = mSnapYInterval.Clamp(snapIdx1);

                mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
            }
        }

        public void setSpringParameters(float damping, float springK, float springBounce, float springRebound)
        {
            mDamping = damping;
            mSpringK = springK;
            mSpringBounce = springBounce;
            mSpringRebound = springRebound;
        }

        public void easeToSnapX(int value)
        {
            if (Settings.SnapOnX == (mSettings & Settings.SnapOnX))
            {
                this.setSnapX(value);

                mIsEasingOnX = true;
            }
        }

        public void easeToSnapY(int value)
        {
            if (Settings.SnapOnY == (mSettings & Settings.SnapOnY))
            {
                this.setSnapY(value);

                mIsEasingOnY = true;
            }
        }

        protected void updateCircularBehaviour(bool forceUpdate)
        {
            if ((mCircularOnX || mCircularOnY) && (forceUpdate || mOffsetVel.sqrMagnitude > 1.0f || mIsDragging))
            {
                float deltaS = mCircularOnX ?
                    mSnapSpace.x * ((mSnapXInterval.End - mSnapXInterval.Start) + 1) :
                    mSnapSpace.y * ((mSnapYInterval.End - mSnapYInterval.Start) + 1);

                float minS = float.MaxValue, maxS = -float.MaxValue;
                int c = mContainer.numChildren;

                if (0 == c)
                    return;

                DisplayObject[] orderedObjects = new DisplayObject[c];
                for (int i = 0; i < c; ++i)
                {
                    DisplayObject obj = mContainer.getChildAt(i);
                    orderedObjects[i] = obj;

                    Rectangle objBounds = obj.getBounds(null);
                    Vector2 objMin = obj.localToGlobal(new Vector2(objBounds.left, objBounds.top)),
                            objMax = obj.localToGlobal(new Vector2(objBounds.right, objBounds.bottom));

                    if (mCircularOnX)
                    {
                        float center = Mathf.Floor((objMin.x + objMax.x) * 0.5f / mSnapSpace.x) * mSnapSpace.x;
                        objMin.x = center - mSnapSpace.x * 0.5f;
                        objMax.x = center + mSnapSpace.x * 0.5f;
                    }
                    else
                    {
                        float center = Mathf.Floor((objMin.y + objMax.y) * 0.5f / mSnapSpace.y) * mSnapSpace.y;
                        objMin.y = center - mSnapSpace.y * 0.5f;
                        objMax.y = center + mSnapSpace.y * 0.5f;
                    }

                    obj.userData = new Rectangle(objMin.x, objMin.y, objMax.x - objMin.x, objMax.y - objMin.y);

                    if (mCircularOnX)
                    {
                        if (objMin.x < minS)
                            minS = objMin.x;

                        if (objMax.x > maxS)
                            maxS = objMax.x;
                    }
                    else
                    {
                        if (objMin.y < minS)
                            minS = objMin.y;

                        if (objMax.y > maxS)
                            maxS = objMax.y;
                    }
                }

                if ((mCircularOnX && (maxS - minS) < mScrollArea.width) || (maxS - minS) < mScrollArea.height)
                    return;

                if (mCircularOnX)
                    Array.Sort<DisplayObject>(orderedObjects, (a, b) => { return ((Rectangle)a.userData).x.CompareTo(((Rectangle)b.userData).x); });
                else
                    Array.Sort<DisplayObject>(orderedObjects, (a, b) => { return ((Rectangle)a.userData).y.CompareTo(((Rectangle)b.userData).y); });

                int movement = 0, span = mCircularOnX ? mSnapXInterval2.End - mSnapXInterval2.Start + 1 : mSnapYInterval2.End - mSnapYInterval2.Start + 1;
                do
                {
                    movement = 0;

                    DisplayObject leftMostObj = orderedObjects[0],
                                  rightMostObj = orderedObjects[c - 1];

                    Rectangle objRect = (Rectangle)rightMostObj.userData;
                    float leftOffset = mCircularOnX ?
                        mScrollArea.xMax - (objRect.x + objRect.width) :
                        mScrollArea.yMax - (objRect.y + objRect.height);

                    objRect = (Rectangle)leftMostObj.userData;
                    float rightOffset = mCircularOnX ?
                        objRect.x - mScrollArea.xMin :
                        objRect.y - mScrollArea.yMin;

                    if (leftOffset > 0.0f && rightOffset > 0.0f)
                    {
                        if (leftOffset > rightOffset)
                            movement = -1;
                        else
                            movement = 1;
                    }
                    else if (leftOffset > 0.0f)
                        movement = -1;
                    else if (rightOffset > 0.0f)
                        movement = 1;

                    if (movement != 0)
                    {
                        if (1 == movement)
                        { // move rightmost @ the beginning
                            if (mCircularOnX)
                                rightMostObj.x -= deltaS;
                            else
                                rightMostObj.y -= deltaS;

                            Rectangle rect = (Rectangle)rightMostObj.userData;
                            if (mCircularOnX)
                                rect.x -= deltaS;
                            else
                                rect.y -= deltaS;

                            for (int i = c - 1; i >= 1; --i)
                                orderedObjects[i] = orderedObjects[i - 1];

                            orderedObjects[0] = rightMostObj;
                        }
                        else
                        { // move leftmost @ the end
                            if (mCircularOnX)
                                leftMostObj.x += deltaS;
                            else
                                leftMostObj.y += deltaS;

                            Rectangle rect = (Rectangle)leftMostObj.userData;
                            if (mCircularOnX)
                                rect.x += deltaS;
                            else
                                rect.y += deltaS;

                            for (int i = 0; i < (c - 1); ++i)
                                orderedObjects[i] = orderedObjects[i + 1];

                            orderedObjects[c - 1] = leftMostObj;
                        }

                        float deltaMov = -movement * mSnapSpace.x;

                        if (mCircularOnX)
                        {
                            if (mSnapXInterval2.HasStart)
                                mSnapXInterval2.Start += movement;
                            if (mSnapXInterval2.HasEnd)
                                mSnapXInterval2.End += movement;

                            mIndexOffsetX += movement;
                        }
                        else
                        {
                            if (mSnapYInterval2.HasStart)
                                mSnapYInterval2.Start += movement;
                            if (mSnapYInterval2.HasEnd)
                                mSnapYInterval2.End += movement;

                            mIndexOffsetY += movement;
                        }

                        minS += deltaMov;
                        maxS += deltaMov;
                    }
                } while (((mCircularOnX && (minS > mScrollArea.xMin || maxS < mScrollArea.xMax)) || (minS > mScrollArea.yMin || maxS < mScrollArea.yMax)) && movement != 0);

                if (mCircularOnX)
                {
                    while (mIndexOffsetX < 0)
                        mIndexOffsetX += span;

                    mIndexOffsetX %= span;
                }
                else
                {
                    while (mIndexOffsetY < 0)
                        mIndexOffsetY += span;

                    mIndexOffsetY %= span;
                }
            }
        }
        #endregion

        #region Children methods
        public void addChild(DisplayObject displayObject)
        {
            mContainer.addChild(displayObject);
            mContainer.updateBounds();
        }

        public void addChildAt(DisplayObject displayObject, int index)
        {
            mContainer.addChildAt(displayObject, index);
            mContainer.updateBounds();
        }

        public bool contains(DisplayObject obj)
        {
            return mContainer.contains(obj);
        }

        public DisplayObject getChildAt(int id)
        {
            return mContainer.getChildAt(id);
        }

        public DisplayObject getChildByName(string name)
        {
            return mContainer.getChildByName(name);
        }

        public T getChildAt<T>(int id)
            where T : DisplayObject
        {
            return mContainer.getChildAt<T>(id);
        }

        public T getChildByName<T>(string name)
            where T : DisplayObject
        {
            return mContainer.getChildByName<T>(name);
        }

        public int getChildIndex(DisplayObject child)
        {
            return mContainer.getChildIndex(child);
        }

        public void removeAllChildren()
        {
            mContainer.removeAllChildren();
            mContainer.updateBounds();
        }

        public void removeChild(DisplayObject displayObject)
        {
            mContainer.removeChild(displayObject);
            mContainer.updateBounds();
        }

        public void removeChildAt(int id)
        {
            mContainer.removeChildAt(id);
            mContainer.updateBounds();
        }

        public void setChildIndex(DisplayObject child, int index)
        {
            mContainer.setChildIndex(child, index);
        }
        #endregion

        #region Conversion to DisplayObjectContainer
        public static explicit operator DisplayObjectContainer(ScrollingController controller)
        {
            return controller.mContainer;
        }
        #endregion
    }
}
#endif