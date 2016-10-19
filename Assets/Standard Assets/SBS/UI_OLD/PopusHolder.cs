using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.UI
{
    public class PopupsHolder
    {
        #region Protected members
        protected Dictionary<string, Popup> popups;
        protected Stack<string> stack;
        protected Stack<Dictionary<string, Popup.ItemDesc>> items;
        protected Popup frontPopup;
        #endregion

        #region Ctors
        public PopupsHolder()
        {
            popups = new Dictionary<string, Popup>();
            stack = new Stack<string>();
            items = new Stack<Dictionary<string, Popup.ItemDesc>>();
            frontPopup = null;
        }
        #endregion

        #region Public properties
        public Popup FrontPopup
        {
            get
            {
                return frontPopup;
            }
        }
        #endregion

        #region Public methods
        public Popup CreatePopup(string name)
        {
            Popup popup = new Popup(name);
            popups.Add(name, popup);
            return popup;
        }

        public void DestroyPopup(string name)
        {
            Popup popup;
            if (popups.TryGetValue(name, out popup))
            {
                if (popup == frontPopup)
                    this.Pop();

                popup.Destroy();
                popups.Remove(name);
            }
        }

        public void Push(string popupName, Dictionary<string, Popup.ItemDesc> itemsDesc)
        {
            Popup prevFrontPopup = frontPopup;
            if (popups.TryGetValue(popupName, out frontPopup))
            {
                if (prevFrontPopup != null)
                    prevFrontPopup.Hide();

                frontPopup.Show(itemsDesc);

                stack.Push(popupName);
                items.Push(itemsDesc);
            }
        }

        public void Push(string popupName)
        {
            this.Push(popupName, null);
        }

        public void Remove(string popupName)
        {
            if (frontPopup != null && frontPopup.Name == popupName)
            {
                this.Pop();
                return;
            }

#if UNITY_FLASH
            string[] popupsList = new string[stack.Count];
            int k = 0;
            foreach (string s in stack)
                popupsList[k++] = s;
            Dictionary<string, Popup.ItemDesc>[] itemsList = new Dictionary<string, Popup.ItemDesc>[items.Count];
            k = 0;
            foreach (Dictionary<string, Popup.ItemDesc> dict in items)
                itemsList[k++] = dict;
#else
            string[] popupsList = stack.ToArray();
            Dictionary<string, Popup.ItemDesc>[] itemsList = items.ToArray();
#endif
            stack.Clear();
            items.Clear();

            for (int i = popupsList.Length - 1; i >= 0; --i)
            {
                string name = popupsList[i];
                if (name != popupName)
                {
                    stack.Push(name);
                    items.Push(itemsList[i]);
                }
            }
        }

        public void Pop()
        {
            if (null == frontPopup)
                return;

            frontPopup.Hide();
            stack.Pop();
            items.Pop();

            while (stack.Count > 0 && !popups.TryGetValue(stack.Peek(), out frontPopup))
            {
                stack.Pop();
                items.Pop();
            }

            if (0 == stack.Count)
                frontPopup = null;
            else
                frontPopup.Show(items.Peek());
        }

        public void Clear()
        {
            while (frontPopup != null)
                this.Pop();
        }

        public bool IsInStack(string popupName)
        {
            return stack.Contains(popupName);
        }

        public void Update()
        {
            if (null == frontPopup)
                return;

            frontPopup.Update();
        }

        public void GUIUpdate()
        {
            if (null == frontPopup)
                return;

            frontPopup.GUIUpdate();
        }

        public void Destroy()
        {
/*          foreach (Popup popup in popups.Values)
                popup.Destroy();*/
            foreach (KeyValuePair<string, Popup> pair in popups)
                pair.Value.Destroy();
        }
        #endregion
    }
}
