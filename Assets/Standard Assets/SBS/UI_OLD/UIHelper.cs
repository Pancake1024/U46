using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.UI;

[AddComponentMenu("SBS/UI/UIHelper")]
public class UIHelper : MonoBehaviour
{
    #region Singleton instance
    protected static UIHelper instance = null;

    public static UIHelper Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public enums
    public enum ButtonType
    {
        Overlays = 0,
        Compositions,
//      Texts
    }
    #endregion

    #region Public classes
    [Serializable]
    public class CompositionTemplate
    {
        public string name;
        public string batchName;
        public string overlay;
        public Rect coreRect;
        public bool stretchOverlays = true;
    }

    [Serializable]
    public class ButtonTemplate
    {
        public string name;
        public string batchName;
        public ButtonType type;
        public Button.Autoresize autoResize = Button.Autoresize.Fixed;
        public string font;
        public BitmapFontText.TextAlign textAlign = BitmapFontText.TextAlign.MiddleCenter;
        public RectOffset textPadding;
        public string normal;
        public string mouseOver;
        public string pressed;
        public string disabled;
    }
    #endregion

    #region Public members
    public float standardDpi = 326.0f;
    public float standardScreenMaxSize = 960.0f;
    public CompositionTemplate[] compTemplates;
    public ButtonTemplate[] btnTemplates;
    #endregion

    #region Public methods
    public Overlay CreateOverlay(string batchName, string name)
    {
        OverlaysBatch batch = UIManager_OLD.Instance.GetBatch(batchName);

        Rect imgRect = UIManager_OLD.Instance.GetOverlayRect(batchName, name);
        UIRect scrRect = imgRect;

        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            scrRect.width *= sx;
            scrRect.height *= sy;
        }

        return new Overlay(batch, SBSMatrix4x4.identity, scrRect, imgRect, Color.white);
    }

    public Overlay CreateOverlay(string batchName, string name, float x, float y)
    {
        OverlaysBatch batch = UIManager_OLD.Instance.GetBatch(batchName);

        Rect imgRect = UIManager_OLD.Instance.GetOverlayRect(batchName, name);
        Rect scrRect = imgRect;
        Vector2 pos = new UIVector2(x, y);
        scrRect.x = pos.x;
        scrRect.y = pos.y;

        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            scrRect.width *= sx;
            scrRect.height *= sy;
        }

        return new Overlay(batch, SBSMatrix4x4.identity, scrRect, imgRect, Color.white);
    }

    public Overlay CreateOverlay(string batchName, string name, Rect screenRect)
    {
        OverlaysBatch batch = UIManager_OLD.Instance.GetBatch(batchName);

        Rect imgRect = UIManager_OLD.Instance.GetOverlayRect(batchName, name);

        return new Overlay(batch, SBSMatrix4x4.identity, screenRect, imgRect, Color.white);
    }

    public Composition CreateComposition(string name, Rect screenRect)
    {
        CompositionTemplate templ = this.FindCompositionTemplate(name);
        if (null == templ)
            return null;

        OverlaysBatch batch = UIManager_OLD.Instance.GetBatch(templ.batchName);

        Rect imgRect = UIManager_OLD.Instance.GetOverlayRect(templ.batchName, templ.overlay);

        Rect coreRect = templ.coreRect;

        float scale = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            scale = UIManager_OLD.Instance.GetDeviceSimScreenMaxSize() / standardScreenMaxSize;//UIManager.Instance.GetDeviceSimDpi() / standardDpi;
        else
            scale = SBSMath.Max(UIManager_OLD.screenWidth, UIManager_OLD.screenHeight) / standardScreenMaxSize;//Screen.dpi / standardDpi;

        coreRect.xMin = Mathf.RoundToInt(coreRect.xMin * scale);
        coreRect.xMax = Mathf.RoundToInt(coreRect.xMax * scale);
        coreRect.yMin = Mathf.RoundToInt(coreRect.yMin * scale);
        coreRect.yMax = Mathf.RoundToInt(coreRect.yMax * scale);

        return new Composition(batch, SBSMatrix4x4.identity, coreRect, screenRect, imgRect, templ.stretchOverlays, Color.white);
    }

    public BitmapFontText CreateText(string font, string text)
    {
        BitmapFontText txt = new BitmapFontText();
        txt.Font = UIManager_OLD.Instance.GetFont(font);
        txt.Text = text;
        return txt;
    }

    public BitmapFontText CreateText(string font, BitmapFontText.TextAlign align, string text)
    {
        BitmapFontText txt = new BitmapFontText();
        txt.Font = UIManager_OLD.Instance.GetFont(font);
        txt.Align = align;
        txt.Text = text;
        return txt;
    }

    public BitmapFontText CreateText(string font, BitmapFontText.TextAlign align, Rect screenRect, string text)
    {
        BitmapFontText txt = new BitmapFontText();
        txt.Font = UIManager_OLD.Instance.GetFont(font);
        txt.Align = align;
        txt.ScreenRect = screenRect;
        txt.Text = text;
        return txt;
    }

    public BitmapFontText CreateText(string font, BitmapFontText.TextAlign align, Rect screenRect, string text, bool multiline)
    {
        BitmapFontText txt = new BitmapFontText();
        txt.Font = UIManager_OLD.Instance.GetFont(font);
        txt.Align = align;
        txt.ScreenRect = screenRect;
        txt.MultiLine = multiline;
        txt.Text = text;
        return txt;
    }

    public Button CreateButton(string name, Rect screenRect, string text)
    {
        ButtonTemplate templ = this.FindButtonTemplate(name);
        if (null == templ)
            return null;

        UIElement normal = null,
                  mouseOver = null,
                  pressed = null,
                  disabled = null;

        switch (templ.type)
        {
            case ButtonType.Overlays:
                normal    = templ.normal.Length > 0    ? this.CreateOverlay(templ.batchName, templ.normal, screenRect)    : null;
                mouseOver = templ.mouseOver.Length > 0 ? this.CreateOverlay(templ.batchName, templ.mouseOver, screenRect) : null;
                pressed   = templ.pressed.Length > 0   ? this.CreateOverlay(templ.batchName, templ.pressed, screenRect)   : null;
                disabled  = templ.disabled.Length > 0  ? this.CreateOverlay(templ.batchName, templ.disabled, screenRect)  : null;
                break;
            case ButtonType.Compositions:
                normal    = templ.normal.Length > 0    ? this.CreateComposition(templ.normal, screenRect)    : null;
                mouseOver = templ.mouseOver.Length > 0 ? this.CreateComposition(templ.mouseOver, screenRect) : null;
                pressed   = templ.pressed.Length > 0   ? this.CreateComposition(templ.pressed, screenRect)   : null;
                disabled  = templ.disabled.Length > 0  ? this.CreateComposition(templ.disabled, screenRect)  : null;
                break;
/*          case ButtonType.Texts:
                break;*/
        }

        RectOffset textPadding = new RectOffset(templ.textPadding.left, templ.textPadding.right, templ.textPadding.top, templ.textPadding.bottom);

        float scale = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            scale = UIManager_OLD.Instance.GetDeviceSimScreenMaxSize() / standardScreenMaxSize;//UIManager.Instance.GetDeviceSimDpi() / standardDpi;
        else
            scale = SBSMath.Max(UIManager_OLD.screenWidth, UIManager_OLD.screenHeight) / standardScreenMaxSize;//Screen.dpi / standardDpi;

        textPadding.left   = Mathf.RoundToInt(textPadding.left * scale);
        textPadding.right  = Mathf.RoundToInt(textPadding.right * scale);
        textPadding.top    = Mathf.RoundToInt(textPadding.top * scale);
        textPadding.bottom = Mathf.RoundToInt(textPadding.bottom * scale);

        Button btn = new Button(normal, mouseOver, pressed, disabled, textPadding);

        if (templ.font.Length > 0)
            btn.Font = UIManager_OLD.Instance.GetFont(templ.font);
        btn.TextAlign = templ.textAlign;
        btn.AutoResize = templ.autoResize;
        if (text.Length > 0)
            btn.Text = text;

        return UIManager_OLD.Instance.AddButton(btn);
    }

    public Button CreateButton(string name, float x, float y, string text)
    {
        ButtonTemplate templ = this.FindButtonTemplate(name);
        if (null == templ || ButtonType.Compositions == templ.type)
            return null;

        Rect scrRect = UIManager_OLD.Instance.GetOverlayRect(templ.batchName, templ.normal);
        Vector2 pos = new UIVector2(x, y);
        scrRect.x = pos.x;
        scrRect.y = pos.y;

        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            scrRect.width *= sx;
            scrRect.height *= sy;
        }

        return this.CreateButton(name, scrRect, text);
    }
    #endregion

    #region Protected methods
    protected CompositionTemplate FindCompositionTemplate(string name)
    {
        foreach (CompositionTemplate templ in compTemplates)
        {
            if (templ.name == name)
                return templ;
        }
        return null;
    }

    protected ButtonTemplate FindButtonTemplate(string name)
    {
        foreach (ButtonTemplate templ in btnTemplates)
        {
            if (templ.name == name)
                return templ;
        }
        return null;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
