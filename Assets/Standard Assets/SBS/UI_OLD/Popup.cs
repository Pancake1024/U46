using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.UI
{
    public class Popup
    {
        #region Protected classes
        protected class GUIButton
        {
            public string name;
            public Rect position;
            public string text;
            public GUIStyle style;
            public bool visible;

            public GUIButton(string _name, Rect _position, string _text, GUIStyle _style)
            {
                name = _name;
                position = _position;
                text = _text;
                style = _style;
                visible = true;
            }
        }

        protected class GUIText
        {
            public string uniqueId;
            public Rect position;
            public string text;
            public GUIStyle style;
            public bool visible;

            public GUIText(string _uniqueId, Rect _position, string _text, GUIStyle _style)
            {
                uniqueId = _uniqueId;
                position = _position;
                text = _text;
                style = _style;
                visible = true;
            }
        }
        #endregion

        #region Public structs
        public struct ItemDesc
        {
            public bool visible;
            public string text;
            // ToDo: expand

            public ItemDesc(bool _visible, string _text)
            {
                visible = _visible;
                text = _text;
            }
        }
        #endregion

        #region Protected members
        protected string name;
        protected List<GUIButton> guiButtons;
        protected List<GUIText> guiTexts;
        protected List<KeyValuePair<string, Overlay>> overlays;
        protected List<KeyValuePair<string, BitmapFontText>> texts;
        protected List<KeyValuePair<string, Button>> buttons;
        protected string guiButtonClicked;
        protected bool visible;
        #endregion

        #region Ctors
        private Popup()
        {
        }

        public Popup(string _name)
        {
            name = _name;
            guiButtons = new List<GUIButton>();
            guiTexts = new List<GUIText>();
            overlays = new List<KeyValuePair<string, Overlay>>();
            texts = new List<KeyValuePair<string, BitmapFontText>>();
            buttons = new List<KeyValuePair<string, Button>>();
            guiButtonClicked = string.Empty;
            visible = false;
        }
        #endregion

        #region Public properties
        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool IsVisible
        {
            get
            {
                return visible;
            }
        }

        public string GUIButtonClicked
        {
            get
            {
                return guiButtonClicked;
            }
        }
        #endregion

        #region Public methods
        public void AddGUIButton(string name, Rect position, string text)
        {
            guiButtons.Add(new GUIButton(name, position, text, null));
        }

        public void AddGUIButton(string name, Rect position, string text, GUIStyle style)
        {
            guiButtons.Add(new GUIButton(name, position, text, style));
        }

        public void AddGUIText(string name, Rect position, string text)
        {
            guiTexts.Add(new GUIText(name, position, text, null));
        }

        public void AddGUIText(string name, Rect position, string text, GUIStyle style)
        {
            guiTexts.Add(new GUIText(name, position, text, style));
        }

        public void AddOverlay(string uniqueId, Overlay overlay)
        {
            overlay.Visibility = visible;
            overlays.Add(new KeyValuePair<string, Overlay>(uniqueId, overlay));
        }

        public void AddOverlay(Overlay overlay)
        {
            overlay.Visibility = visible;
            overlays.Add(new KeyValuePair<string, Overlay>(string.Empty, overlay));
        }

        public void AddText(string uniqueId, BitmapFontText text)
        {
            text.Visibility = visible;
            texts.Add(new KeyValuePair<string, BitmapFontText>(uniqueId, text));
        }

        public void AddText(BitmapFontText text)
        {
            text.Visibility = visible;
            texts.Add(new KeyValuePair<string, BitmapFontText>(string.Empty, text));
        }

        public void AddButton(string uniqueId, Button button)
        {
            button.Visibility = visible;
            buttons.Add(new KeyValuePair<string, Button>(uniqueId, button));
        }

        public void AddButton(Button button)
        {
            button.Visibility = visible;
            buttons.Add(new KeyValuePair<string, Button>(string.Empty, button));
        }

        public void Destroy()
        {
            foreach (KeyValuePair<string, Overlay> item in overlays)
                item.Value.Destroy();

            foreach (KeyValuePair<string, BitmapFontText> item in texts)
                item.Value.Destroy();

            foreach (KeyValuePair<string, Button> item in buttons)
                UIManager_OLD.Instance.DestroyButton(item.Value);
        }

        public void Show(Dictionary<string, ItemDesc> itemsDesc)
        {
            if (!visible)
            {
                foreach (KeyValuePair<string, Overlay> item in overlays)
                {
                    if (string.Empty == item.Key)
                    {
                        item.Value.Visibility = true;
                    }
                    else
                    {
                        ItemDesc itemDesc;
                        if (itemsDesc != null && itemsDesc.TryGetValue(item.Key, out itemDesc))
                            item.Value.Visibility = itemDesc.visible;
                        else
                            item.Value.Visibility = true;
                    }
                }

                foreach (KeyValuePair<string, BitmapFontText> item in texts)
                {
                    if (string.Empty == item.Key)
                    {
                        item.Value.Visibility = true;
                    }
                    else
                    {
                        ItemDesc itemDesc;
                        if (itemsDesc != null && itemsDesc.TryGetValue(item.Key, out itemDesc))
                        {
                            item.Value.Visibility = itemDesc.visible;
                            item.Value.Text = itemDesc.text;
                        }
                        else
                        {
                            item.Value.Visibility = true;
                        }
                    }
                }

                foreach (KeyValuePair<string, Button> item in buttons)
                {
                    if (string.Empty == item.Key)
                    {
                        item.Value.Visibility = true;
                    }
                    else
                    {
                        ItemDesc itemDesc;
                        if (itemsDesc != null && itemsDesc.TryGetValue(item.Key, out itemDesc))
                        {
                            item.Value.Visibility = itemDesc.visible;
                            item.Value.Text = itemDesc.text;
                        }
                        else
                        {
                            item.Value.Visibility = true;
                        }
                    }
                }

                foreach (GUIText txt in guiTexts)
                {
                    if (string.Empty == txt.uniqueId)
                        continue;

                    ItemDesc itemDesc;
                    if (itemsDesc != null && itemsDesc.TryGetValue(txt.uniqueId, out itemDesc))
                    {
                        txt.visible = itemDesc.visible;
                        txt.text = itemDesc.text;
                    }
                    else
                    {
                        txt.visible = true;
                    }
                }

                foreach (GUIButton btn in guiButtons)
                {
                    if (string.Empty == btn.name)
                        continue;

                    ItemDesc itemDesc;
                    if (itemsDesc != null && itemsDesc.TryGetValue(btn.name, out itemDesc))
                    {
                        btn.visible = itemDesc.visible;
                        btn.text = itemDesc.text;
                    }
                    else
                    {
                        btn.visible = true;
                    }
                }

                visible = true;
            }
        }

        public void ChangeItem(string name, ItemDesc desc)
        {
            if (visible)
            {
                foreach (KeyValuePair<string, Overlay> item in overlays)
                {
                    if (name == item.Key)
                    {
                        item.Value.Visibility = desc.visible;
                        return;
                    }
                }

                foreach (KeyValuePair<string, BitmapFontText> item in texts)
                {
                    if (name == item.Key)
                    {
                        item.Value.Visibility = desc.visible;
                        item.Value.Text = desc.text;
                        return;
                    }
                }

                foreach (KeyValuePair<string, Button> item in buttons)
                {
                    if (name == item.Key)
                    {
                        item.Value.Visibility = desc.visible;
                        item.Value.Text = desc.text;
                        return;
                    }
                }

                foreach (GUIText txt in guiTexts)
                {
                    if (string.Empty == txt.uniqueId)
                        continue;

                    if (name == txt.uniqueId)
                    {
                        txt.visible = desc.visible;
                        txt.text = desc.text;
                        return;
                    }
                }

                foreach (GUIButton btn in guiButtons)
                {
                    if (string.Empty == btn.name)
                        continue;

                    if (name == btn.name)
                    {
                        btn.visible = desc.visible;
                        btn.text = desc.text;
                        return;
                    }
                }
            }
        }

        public void Hide()
        {
            if (visible)
            {
                foreach (KeyValuePair<string, Overlay> item in overlays)
                    item.Value.Visibility = false;

                foreach (KeyValuePair<string, BitmapFontText> item in texts)
                    item.Value.Visibility = false;

                foreach (KeyValuePair<string, Button> item in buttons)
                    item.Value.Visibility = false;

                visible = false;

                guiButtonClicked = string.Empty;
            }
        }

        public void Update()
        {
            guiButtonClicked = string.Empty;
        }

        public void GUIUpdate()
        {
            foreach (GUIButton btn in guiButtons)
            {
                if (!btn.visible)
                    continue;

                if (btn.style != null)
                {
                    if (GUI.Button(btn.position, btn.text, btn.style))
                    {
                        SoundsManager.Instance.PlayClick();
                        guiButtonClicked = btn.name;
                    }
                }
                else
                {
                    if (GUI.Button(btn.position, btn.text))
                    {
                        SoundsManager.Instance.PlayClick();
                        guiButtonClicked = btn.name;
                    }
                }
            }

            foreach (GUIText txt in guiTexts)
            {
                if (!txt.visible)
                    continue;

                if (txt.style != null)
                    GUI.Label(txt.position, txt.text, txt.style);
                else
                    GUI.Label(txt.position, txt.text);
            }
        }
        #endregion
    }
}
