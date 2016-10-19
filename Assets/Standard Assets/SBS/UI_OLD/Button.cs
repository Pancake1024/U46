using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

namespace SBS.UI
{
    public class Button
    {
        #region State enum
        public enum StateType
        {
            Normal = 0,
            MouseOver,
            Pressed,
            Disabled,
            StateCount
        };
        #endregion

        #region Autoresize enum
        public enum Autoresize
        {
            Fixed = 0,
            ExpandFromCenter,
            ExpandRight,
            ExpandLeft
        }
        #endregion

        #region Public events
        public delegate void Event();

        public Event onPressed;
        public Event onRelease;
        public Event onReleaseOutside;
        public Event onMouseOver;
        public Event onMouseOut;
        #endregion

        #region Protected members
        protected StateType state = StateType.Normal;
        protected UIElement[] elements = new UIElement[(int)StateType.StateCount];
        protected bool[] dirtyTransforms = new bool[(int)StateType.StateCount];
        //protected bool textDirtyTransform = false;
        protected BitmapFontText text = new BitmapFontText();
        protected RectOffset textPadding;
        protected bool visible = true;
        protected Rect baseRect;
        protected Autoresize autoResize;
        protected SBSMatrix4x4 transform = SBSMatrix4x4.identity;
        protected SBSMatrix4x4 invTransform = SBSMatrix4x4.identity;
        protected int inputLayer = 0;
        protected Color textColor = Color.white;
        protected Color textDisabledColor = Color.white;
        #endregion

        #region Ctors
        private Button()
        { }

        public Button(UIElement normal, UIElement mouseOver, UIElement pressed, UIElement disabled, RectOffset _textPadding)
        {
            Asserts.Assert(normal != null);

            elements[0] = normal;
            elements[1] = mouseOver;
            elements[2] = pressed;
            elements[3] = disabled;

            dirtyTransforms[0] = false;
            dirtyTransforms[1] = false;
            dirtyTransforms[2] = false;
            dirtyTransforms[3] = false;

            foreach (UIElement elem in elements)
            {
                if (null == elem)
                    continue;
                elem.Visibility = false;
            }
            elements[0].Visibility = true;

            textPadding = _textPadding;
            text.ScreenRect = textPadding.Remove(this.HitRect);
            text.Align = BitmapFontText.TextAlign.MiddleCenter;

            baseRect = elements[0].ScreenRect;
            autoResize = Autoresize.Fixed;
        }

        public Button(UIElement normal, UIElement mouseOver, UIElement pressed, UIElement disabled)
            : this(normal, mouseOver, pressed, disabled, new RectOffset(0, 0, 0, 0))
        { }
        #endregion

        #region Public properties
        public SBSMatrix4x4 Transform
        {
            get
            {
                return transform;
            }
            set
            {
                int i = 0;
                foreach (UIElement elem in elements)
                {
                    if (elem != null)
                    {
                        if (!elem.Visibility)
                            dirtyTransforms[i] = true;
                        else
                            elem.Transform = value;
                    }

                    ++i;
                }

                //if (text.Font != null)
                //{
                    //if (text.Visibility)
                        text.Transform = value;
                    //else
                    //    textDirtyTransform = true;
                //}

                transform = value;
                invTransform = value.inverseFast;
            }
        }

        public void SetScissorRectWithTransform(string name, Rect scissorRect, SBSMatrix4x4 newTransform)
        {
                foreach (UIElement elem in elements)
                {
                    if (elem != null)
                        elem.SetScissorRectWithTransform(name, scissorRect, newTransform);
                }

                text.SetScissorRectWithTransform(name, scissorRect, newTransform);

                transform = newTransform;
                invTransform = newTransform.inverseFast;
        }

        public List<UIElement> Elements
        {
            get
            {
                List<UIElement> allElements = new List<UIElement>();
                foreach (UIElement elem in elements)
                {
                    if (elem != null)
                        allElements.Add(elem);
                }

                if (text.Font != null)
                    allElements.Add(text);

                return allElements;
            }
        }

        public bool IsValid
        {
            get
            {
                return elements[0] != null;
            }
        }

        public Rect HitRect
        {
            get
            {
                return elements[0].ScreenRect;
            }
        }

        public BitmapFont Font
        {
            get
            {
                return text.Font;
            }
            set
            {
                text.Font = value;
            }
        }

        public virtual string Text
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
                this.UpdateSize();
                text.ScreenRect = textPadding.Remove(this.HitRect);
            }
        }

        public BitmapFontText.TextAlign TextAlign
        {
            get
            {
                return text.Align;
            }
            set
            {
                text.Align = value;
            }
        }

        public Color TextColor
        {
            get
            {
                return textColor;
            }
            set
            {
                if (state != StateType.Disabled)
                    text.Color = value;
                textColor = value;
            }
        }

        public Color TextDisabledColor
        {
            get
            {
                return textDisabledColor;
            }
            set
            {
                if (StateType.Disabled == state)
                    text.Color = value;
                textDisabledColor = value;
            }
        }

        public RectOffset TextPadding
        {
            get
            {
                return textPadding;
            }
            set
            {
                text.ScreenRect = value.Remove(this.HitRect);
                textPadding = value;
            }
        }

        public float MinWidth
        {
            get
            {
                return baseRect.width;
            }
            set
            {
                float w = SBSMath.Max(0.0f, value);

                switch (autoResize)
                {
                    case Autoresize.Fixed:
                        baseRect.width = w;
                        break;
                    case Autoresize.ExpandFromCenter:
                        float halfDiff =  Mathf.CeilToInt(0.5f * (w - baseRect.width));
                        baseRect.xMin -= halfDiff;
                        baseRect.xMax += halfDiff;
                        break;
                    case Autoresize.ExpandRight:
                        baseRect.xMax = baseRect.xMin + w;
                        break;
                    case Autoresize.ExpandLeft:
                        baseRect.xMin = baseRect.xMax - w;
                        break;
                }

                this.UpdateSize();
            }
        }

        public float MinHeight
        {
            get
            {
                return baseRect.height;
            }
            set
            {
                float h = SBSMath.Max(0.0f, value);

                switch (autoResize)
                {
                    case Autoresize.Fixed:
                        baseRect.height = h;
                        break;
                    case Autoresize.ExpandFromCenter:
                        float halfDiff =  Mathf.CeilToInt(0.5f * (h - baseRect.height));
                        baseRect.yMin -= halfDiff;
                        baseRect.yMax += halfDiff;
                        break;
                    case Autoresize.ExpandRight:
                        baseRect.yMax = baseRect.yMin + h;
                        break;
                    case Autoresize.ExpandLeft:
                        baseRect.yMin = baseRect.yMax - h;
                        break;
                }

                this.UpdateSize();
            }
        }

        public Autoresize AutoResize
        {
            get
            {
                return autoResize;
            }
            set
            {
                autoResize = value;
                this.UpdateSize();
            }
        }

        public bool MultiLine
        {
            get
            {
                return text.MultiLine;
            }
            set
            {
                text.MultiLine = value;
                this.UpdateSize();
            }
        }
        /*
        public bool Clipped
        {
            get
            {
                UIElement elem = elements[(int)state];
                if (null == elem)
                    return false;
                else
                    return elem.Clipped;
            }
        }
        */
        public bool Visibility
        {
            get
            {
                return visible;
            }
            set
            {
                if (value != visible)
                {
                    visible = value;

                    int intValue = (int)state;
                    if (elements[intValue] != null)
                    {
                        text.Visibility = visible;
                        elements[intValue].Visibility = visible;

                        if (visible && dirtyTransforms[intValue])
                        {
                            elements[intValue].Transform = transform;
                            dirtyTransforms[intValue] = false;
                        }
                        /*
                        if (visible && textDirtyTransform)
                        {
                            text.Transform = transform;
                            textDirtyTransform = false;
                        }*/
                    }
                }
            }
        }

        public bool Clipped
        {
            get
            {
                string scissorRectName = elements[0].GetScissorRectName();
                if (scissorRectName != null)
                {
                    Vector2 offset = Vector2.zero;
                    Rect scissorRect = Rect.MinMaxRect(0, 0, UIManager_OLD.screenWidth, UIManager_OLD.screenHeight);

                    elements[0].Batch.Layer.GetScissorRect(scissorRectName, ref scissorRect);
                    elements[0].Batch.Layer.GetScissorRectOffset(scissorRectName, ref offset);

                    Rect newRect = this.HitRect;
                    newRect.x += offset.x;
                    newRect.y += offset.y;

                    if (SBSMath.ClipStatus.Outside == SBSMath.GetClipStatus(scissorRect, newRect))
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }

        public Vector2 Position
        {
            get
            {
                Rect scrRect = elements[0].ScreenRect;
                return new Vector2(scrRect.xMin, scrRect.yMin);
            }
            set
            {
                Rect scrRect = elements[0].ScreenRect;
                Vector2 delta = value - new Vector2(scrRect.xMin, scrRect.yMin);

                foreach (UIElement elem in elements)
                {
                    if (elem != null)
                    {
                        scrRect = elem.ScreenRect;
                        elem.ScreenRect = new Rect(scrRect.xMin + delta.x, scrRect.yMin + delta.y, scrRect.width, scrRect.height);
                    }
                }

                scrRect = text.ScreenRect;
                text.ScreenRect = new Rect(scrRect.xMin + delta.x, scrRect.yMin + delta.y, scrRect.width, scrRect.height);
            }
        }

        public StateType State
        {
            get
            {
                return state;
            }
            set
            {
                if (state == value)
                    return;

                int intValue = (int)state;
                if (elements[intValue] != null)
                {
                    text.Visibility = false;
                    elements[intValue].Visibility = false;
                }

                switch (state)
                {
                    case StateType.Normal:
                        if (StateType.MouseOver == value && onMouseOver != null)
                            onMouseOver();
                        else if (StateType.Pressed == value && onPressed != null)
                            onPressed();
                        break;
                    case StateType.MouseOver:
                        if (StateType.Normal == value && onMouseOut != null)
                            onMouseOut();
                        else if (StateType.Pressed == value && onPressed != null)
                            onPressed();
                        break;
                    case StateType.Pressed:
                        if (StateType.Normal == value && onReleaseOutside != null)
                            onReleaseOutside();
                        else if (StateType.MouseOver == value && onRelease != null)
                            onRelease();
                        break;
                    case StateType.Disabled:
                        break;
                }

                intValue = (int)value;
                if (elements[intValue] != null && visible)
                {
                    text.Visibility = true;
                    elements[intValue].Visibility = true;

                    if (dirtyTransforms[intValue])
                    {
                        elements[intValue].Transform = transform;
                        dirtyTransforms[intValue] = false;
                    }
                    /*
                    if (textDirtyTransform)
                    {
                        text.Transform = transform;
                        textDirtyTransform = false;
                    }*/
                    if (StateType.Disabled == value)
                        text.Color = textDisabledColor;
                    else
                        text.Color = textColor;
                }

                if (visible)
                    state = value;
                else
                    state = StateType.Normal;
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

        public int Depth
        {
            get
            {
                int depth = 0;
                foreach (UIElement element in elements)
                {
                    if (element != null && element.Depth > depth)
                        depth = element.Depth;
                }
                if (text != null && text.Depth > depth)
                    depth = text.Depth;
                return depth;
            }
            set
            {
                foreach (UIElement element in elements)
                {
                    if (element != null)
                        element.Depth = value;
                }
                if (text != null)
                    text.Depth = value + 1;
            }
        }
        #endregion

        #region Protected methods
        protected void UpdateSize()
        {
            if (Autoresize.Fixed == autoResize)
                return;

            Rect scrRect = baseRect;
            if (text.MultiLine)
            {
                float wantedHeight = SBSMath.Max(baseRect.height, textPadding.vertical + text.TextSize.y);
                switch (autoResize)
                {
                    case Autoresize.ExpandFromCenter:
                        float halfDiff = Mathf.CeilToInt(0.5f * (wantedHeight - scrRect.height));
                        scrRect.yMin -= halfDiff;
                        scrRect.yMax += halfDiff;
                        break;
                    case Autoresize.ExpandRight:
                        scrRect.yMax = scrRect.yMin + wantedHeight;
                        break;
                    case Autoresize.ExpandLeft:
                        scrRect.yMin = scrRect.yMax - wantedHeight;
                        break;
                }
            }
            else
            {
                float wantedWidth = SBSMath.Max(baseRect.width, textPadding.horizontal + text.TextSize.x);
                switch (autoResize)
                {
                    case Autoresize.ExpandFromCenter:
                        float halfDiff =  Mathf.CeilToInt(0.5f * (wantedWidth - scrRect.width));
                        scrRect.xMin -= halfDiff;
                        scrRect.xMax += halfDiff;
                        break;
                    case Autoresize.ExpandRight:
                        scrRect.xMax = scrRect.xMin + wantedWidth;
                        break;
                    case Autoresize.ExpandLeft:
                        scrRect.xMin = scrRect.xMax - wantedWidth;
                        break;
                }
            }

            foreach (UIElement elem in elements)
            {
                if (null == elem)
                    continue;

                elem.ScreenRect = scrRect;
            }
        }
        #endregion

        #region Public methods
        public bool Contains(Vector2 position)
        {
            string scissorRectName = elements[0].GetScissorRectName();
            if (scissorRectName != null)
            {
                Vector2 offset = Vector2.zero;
                elements[0].Batch.Layer.GetScissorRectOffset(scissorRectName, ref offset);
                position.x -= offset.x;
                position.y -= offset.y;
            }
            SBSVector3 localPos = invTransform.MultiplyPoint3x4(new SBSVector3(position.x, position.y));
            return elements[0].ScreenRect.Contains(localPos);
            //return this.HitRect.Contains(position);
        }

        public virtual void Destroy()
        {
            foreach (UIElement elem in elements)
            {
                if (elem != null)
                    elem.Destroy();
            }

            elements[0] = null;
            elements[1] = null;
            elements[2] = null;
            elements[3] = null;

            if (text != null)
            {
                text.Destroy();
                text = null;
            }
        }
        #endregion
    }
}
