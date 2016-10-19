using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

namespace SBS.UI
{
    public class UICollection
    {
        #region Protected members
        protected Dictionary<string, UIElement> elements = new Dictionary<string, UIElement>();
        protected Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        #endregion

        #region Public methods
        public virtual void AddElement(string name, UIElement element)
        {
            elements.Add(name, element);
        }

        public virtual bool RemoveElement(string name)
        {
            UIElement elem = null;
            if (elements.TryGetValue(name, out elem))
            {
                elem.Destroy();
                elements.Remove(name);
                return true;
            }
            return false;
        }

        public virtual UIElement GetElement(string name)
        {
            UIElement elem = null;
            if (elements.TryGetValue(name, out elem))
                return elem;
            else
                return null;
        }

        public virtual void AddButton(string name, Button btn)
        {
            buttons.Add(name, btn);
        }

        public virtual bool RemoveButton(string name)
        {
            Button btn = null;
            if (buttons.TryGetValue(name, out btn))
            {
                UIManager_OLD.Instance.DestroyButton(btn);
                buttons.Remove(name);
                return true;
            }
            return false;
        }

        public virtual Button GetButton(string name)
        {
            Button btn = null;
            if (buttons.TryGetValue(name, out btn))
                return btn;
            else
                return null;
        }

        public virtual void SetTransform(SBSMatrix4x4 transform)
        {
#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
                item.Value.Transform = transform;

            foreach (KeyValuePair<string, Button> item in buttons)
                item.Value.Transform = transform;
#else
            foreach (UIElement element in elements.Values)
                element.Transform = transform;

            foreach (Button btn in buttons.Values)
                btn.Transform = transform;
#endif
        }

        public virtual void SetScissorRect(string name, Rect scissorRect)
        {
#if UNITY_FLASH
                foreach (KeyValuePair<string, UIElement> item in elements)
                {
                    UIElement elem = item.Value;
#else
            foreach (UIElement elem in elements.Values)
#endif
                //elem.ScissorRectIndex = elem.Batch.SetScissorRect(name, scrollArea);
                elem.SetScissorRect(name, scissorRect);
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
                    btnElem.SetScissorRect(name, scissorRect);
            }
        }

        public virtual void SetPosition(SBSVector3 pos)
        {
            Vector2 pos2D = new UIVector2(pos.x, pos.y).AsVector2Offset;
            SBSMatrix4x4 tr = SBSMatrix4x4.TRS(new SBSVector3(pos2D.x, pos2D.y), SBSQuaternion.identity, SBSVector3.one);
            this.SetTransform(tr);
        }

        public virtual void SetVisibility(bool visibility)
        {
#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
                item.Value.Visibility = visibility;

            foreach (KeyValuePair<string, Button> item in buttons)
                item.Value.Visibility = visibility;
#else
            foreach (UIElement element in elements.Values)
                element.Visibility = visibility;

            foreach (Button btn in buttons.Values)
                btn.Visibility = visibility;
#endif
        }

        public virtual void Destroy()
        {
#if UNITY_FLASH
            foreach (KeyValuePair<string, UIElement> item in elements)
                item.Value.Destroy();
#else
            foreach (UIElement element in elements.Values)
                element.Destroy();
#endif

            if (UIManager_OLD.Instance != null)
            {
#if UNITY_FLASH
                foreach (KeyValuePair<string, Button> item in buttons)
                     UIManager.Instance.DestroyButton(item.Value);
#else
                foreach (Button btn in buttons.Values)
                    UIManager_OLD.Instance.DestroyButton(btn);
#endif
            }
        }
        #endregion
    }
}
