using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.UI;
using SBS.Math;

[AddComponentMenu("SBS/Managers/UIManager_OLD")]
public class UIManager_OLD : MonoBehaviour
{
    protected static int _screenWidth = -1;
    protected static int _screenHeight = -1;

    protected static bool useCustomScreenRect = false;
    protected static Rect customScreenRect;

    public static bool UsesCustomScreenRect
    {
        get
        {
            return useCustomScreenRect;
        }
    }

    public static Rect CustomScreenRect
    {
        get
        {
            return customScreenRect;
        }
    }

    public static void SetCustomScreenRect(int x, int y, int width, int height)
    {
        useCustomScreenRect = true;
        customScreenRect = new Rect(x, y, width, height);
    }

    public static void SetNativeScreenRect()
    {
        useCustomScreenRect = false;
    }

    public static float screenWidth
    {
        get
        {
#if !UNITY_IPHONE
            if (useCustomScreenRect)
                return customScreenRect.width;
            else
                return Screen.width;
#else
            if (_screenWidth < 0)
                _screenWidth = Screen.width;
            if (useCustomScreenRect)
                return customScreenRect.width;
            else
                return _screenWidth;
#endif
        }
    }

    public static float screenHeight
    {
        get
        {
#if !UNITY_IPHONE
            if (useCustomScreenRect)
                return customScreenRect.height;
            else
                return Screen.height;
#else
            if (_screenHeight < 0)
                _screenHeight = Screen.height;
            if (useCustomScreenRect)
                return customScreenRect.height;
            else
                return _screenHeight;
#endif
        }
    }

    #region Public enums
    public enum DeviceSim
    {
        None = 0,
        iPhone,
        iPhoneRetina,
        iPad,
        iPadRetina
    }

    public enum MultipleResolutionsBehavior
    {
        None = 0,
        MultipleBatches,
        ScaleToFit
    }
    #endregion

    #region Singleton instance
    protected static UIManager_OLD instance = null;

    public static UIManager_OLD Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public members
    public BitmapFont[] _fonts;
    public UIAtlas[] _batches;
    public UIController controller = null;
	//public bool supportsMultipleResolutions = false;
    public MultipleResolutionsBehavior multipleResBehavior = MultipleResolutionsBehavior.None;
    public DeviceSim deviceSim = DeviceSim.None;
    public ScreenOrientation deviceSimOrientation = ScreenOrientation.Unknown;
    public bool dontDestroyOnLoad = true;
	public bool iphoneOnly = true;
	
	public string[] resourceFonts;
	public string[] resourceBatches;
	#endregion

    #region Protected members
	protected List<UIAtlas> loadedBatches = new List<UIAtlas>();

	protected OverlaysLayer layer;
    protected Dictionary<string, OverlaysBatch> batches;
    protected Dictionary<string, BitmapFont> fonts;
    protected Dictionary<string, PopupsHolder> holders;
    protected List<Button> buttons;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
    protected Button[] buttonUnderFinger = new Button[4];
#else
    protected Button buttonUnderMouse = null;
#endif
    protected List<AnimatedOverlay> animatedOverlays;
    protected int inputLayer = 0;
    protected List<Scroller> scrollers;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
    protected Scroller[] scrollerUnderFinger = new Scroller[4];
	protected Vector2[] scrollerLastDelta = new Vector2[4];
    protected Vector2[] scrollerTotalDelta = new Vector2[4];
#else
    protected Scroller scrollerUnderMouse = null;
    protected Vector2 scrollerLastMousePos;
    protected Vector2 scrollerTotalDelta = Vector2.zero;
#endif

    protected Overlay fadeOverlay = null;
    protected float fadeStartTime;
    protected float fadeEndTime = -1.0f;
    protected float fadeStartValue;
    protected float fadeEndValue;
    protected Action fadeCompleted = null;

    protected TimeSource uiTimeSource = null; // TimeSource that never pauses
    #endregion

    #region Public properties
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

    public TimeSource TimeSource
    {
        get
        {
            return uiTimeSource;
        }
    }
    #endregion  
    
    #region Protected methods
    protected void UpdateFade()
    {
        if (fadeEndTime > 0.0f)
        {
            if (fadeEndTime > fadeStartTime)
            {
                float now = Time.realtimeSinceStartup;
                float t = Mathf.Clamp01((now - fadeStartTime) / (fadeEndTime - fadeStartTime)),
                      s = 1.0f - t;
                fadeOverlay.Color = new Color(0.0f, 0.0f, 0.0f, fadeStartValue * s + fadeEndValue * t);

                if (now > fadeEndTime)
                {
                    fadeOverlay.Color = new Color(0.0f, 0.0f, 0.0f, fadeEndValue);
                    fadeEndTime = -1.0f;

					if (fadeEndValue < 1.0e-2)
						fadeOverlay.Visibility = false;
                    if (fadeCompleted != null)
                        fadeCompleted.Invoke();
                }
            }
            else
            {
                fadeOverlay.Color = new Color(0.0f, 0.0f, 0.0f, fadeEndValue);
                fadeEndTime = -1.0f;

				if (fadeEndValue < 1.0e-2)
					fadeOverlay.Visibility = false;
                if (fadeCompleted != null)
                    fadeCompleted.Invoke();
            }
        }
    }

    protected Button GetButton(Vector2 position)
    {
        foreach (Button btn in buttons)
        {
            if (Button.StateType.Disabled == btn.State || !btn.Visibility || btn.Clipped || btn.InputLayer != inputLayer)
                continue;
            if (btn.Contains(position))
            {
                string scissorRectName = btn.Elements[0].GetScissorRectName();
                if (null == scissorRectName)
                    return btn;
                else
                {
                    Rect scissorRect = Rect.MinMaxRect(0, 0, screenWidth, screenHeight);
                    layer.GetScissorRect(scissorRectName, ref scissorRect);
                    if (scissorRect.Contains(position))
                        return btn;
                }
            }
        }

        return null;
    }

    protected Scroller GetScroller(Vector2 position)
    {
        foreach (Scroller scroller in scrollers)
        {
            if (!scroller.ScrollX && !scroller.ScrollY)
                continue;

            if (scroller.ScrollArea.Contains(position) && scroller.InputLayer == inputLayer)
                return scroller;
        }

        return null;
    }

    protected void UpdateButtons()
    {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch t = Input.GetTouch(i);
			if (t.fingerId < 0 || t.fingerId >= 4)
				continue;

            Vector2 pos = t.position;
			pos.y = Screen.height - pos.y;

            if (buttonUnderFinger[t.fingerId] != null && (!buttonUnderFinger[t.fingerId].Visibility || buttonUnderFinger[t.fingerId].Clipped || Button.StateType.Disabled == buttonUnderFinger[t.fingerId].State))
                buttonUnderFinger[t.fingerId] = null;

            switch (t.phase)
            {
                case TouchPhase.Began:
                    Button b = this.GetButton(pos);
                    if (b != null)
                    {
                        b.State = Button.StateType.Pressed;
                        buttonUnderFinger[t.fingerId] = b;
                    }
                    break;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Ended:
                    if (buttonUnderFinger[t.fingerId] != null && buttonUnderFinger[t.fingerId].State != Button.StateType.Disabled)
                    {
                        buttonUnderFinger[t.fingerId].State = buttonUnderFinger[t.fingerId].Contains(pos) ? Button.StateType.MouseOver : Button.StateType.Normal;
						if (buttonUnderFinger[t.fingerId] != null)
                        	buttonUnderFinger[t.fingerId].State = Button.StateType.Normal;
                    }
                    buttonUnderFinger[t.fingerId] = null;
                    break;
                case TouchPhase.Canceled:
                    if (buttonUnderFinger[t.fingerId] != null && buttonUnderFinger[t.fingerId].State != Button.StateType.Disabled)
                        buttonUnderFinger[t.fingerId].State = Button.StateType.Normal;
                    buttonUnderFinger[t.fingerId] = null;
                    break;
            }
        }
#else
        if (buttonUnderMouse != null && (!buttonUnderMouse.Visibility || buttonUnderMouse.Clipped || Button.StateType.Disabled == buttonUnderMouse.State || buttonUnderMouse.InputLayer != inputLayer))
            buttonUnderMouse = null;

        Vector2 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        if (null == buttonUnderMouse)
        {
            buttonUnderMouse = this.GetButton(mousePos);
            if (buttonUnderMouse != null)
                buttonUnderMouse.State = Input.GetMouseButton(0) ? Button.StateType.Normal : Button.StateType.MouseOver;
        }
        else
        {
            bool mouseOver = buttonUnderMouse.Contains(mousePos);
            if (mouseOver)
            {
                if (Input.GetMouseButtonDown(0))
                    buttonUnderMouse.State = Button.StateType.Pressed;
                else if (!Input.GetMouseButton(0))
                    buttonUnderMouse.State = Button.StateType.MouseOver;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (Button.StateType.MouseOver == buttonUnderMouse.State)
                        buttonUnderMouse.State = Button.StateType.Pressed;
                }
                else
                {
                    buttonUnderMouse.State = Button.StateType.Normal;
                    buttonUnderMouse = null;
                }
            }
        }
#endif
    }

    protected void UpdateScrollers()
    {
        foreach (Scroller scroller in scrollers)
        {
            if (!scroller.Active)
                continue;

            bool isMovingUnderMouse = false;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
            for (int i = 0; i < 4; ++i)
                isMovingUnderMouse = isMovingUnderMouse || (scroller == scrollerUnderFinger[i] && scroller.isDragging);
#else
            isMovingUnderMouse = (scroller == scrollerUnderMouse && scroller.isDragging);
#endif

            scroller.Update(uiTimeSource.DeltaTime, isMovingUnderMouse);

            layer.SetScissorRectOffset(scroller.Name, scroller.Offset);
        }

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch t = Input.GetTouch(i);
			if (t.fingerId < 0 || t.fingerId >= 4)
				continue;
			
            Vector2 pos = t.position;
			pos.y = Screen.height - pos.y;

            if (scrollerUnderFinger[t.fingerId] != null && !scrollerUnderFinger[t.fingerId].IsVisible)
                scrollerUnderFinger[t.fingerId] = null;
			
			Scroller scr = null;
            switch (t.phase)
            {
                case TouchPhase.Began:
                    scr = this.GetScroller(pos);
                    if (scr != null)
                    {
                        scr.isDragging = true;
                        scr.OffsetVelocity = Vector2.zero;
                        scrollerUnderFinger[t.fingerId] = scr;
						scrollerLastDelta[t.fingerId] = Vector2.zero;
                        scrollerTotalDelta[t.fingerId] = Vector2.zero;
                    }
                    break;
                case TouchPhase.Stationary:
                    scr = scrollerUnderFinger[t.fingerId];
                    if (scr != null)
                        scr.OffsetVelocity = Vector2.zero;
                    break;
                case TouchPhase.Moved:
                    scr = scrollerUnderFinger[t.fingerId];
                    if (scr != null)
                    {
                        Vector2 delta = t.deltaPosition;
                        delta.y = -delta.y;
                        scr.Offset = scr.Offset + delta;
						scrollerLastDelta[t.fingerId] = delta;
                        scrollerTotalDelta[t.fingerId] += delta;

                        if (buttonUnderFinger[t.fingerId] != null && scrollerTotalDelta[t.fingerId].magnitude > 10.0f)
                        {
                        	buttonUnderFinger[t.fingerId].State = Button.StateType.Normal;
                            buttonUnderFinger[t.fingerId] = null;
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    scr = scrollerUnderFinger[t.fingerId];
                    if (scr != null)
                    {
                        Vector2 delta = scrollerLastDelta[t.fingerId];
                        scr.OffsetVelocity += (delta / uiTimeSource.DeltaTime) * 1.0f;//0.25f;
                    }
                    scrollerUnderFinger[t.fingerId] = null;
                    break;
                case TouchPhase.Canceled:
                    scrollerUnderFinger[t.fingerId] = null;
                    break;
            }
        }
#else
        if (scrollerUnderMouse != null && !scrollerUnderMouse.IsVisible)
            scrollerUnderMouse = null;

        Vector2 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        if (null == scrollerUnderMouse)
        {
            scrollerUnderMouse = this.GetScroller(mousePos);
        }
        else
        {
            bool mouseOver = scrollerUnderMouse.ScrollArea.Contains(mousePos);
            if (!scrollerUnderMouse.isDragging)
            {
                if (mouseOver && Input.GetMouseButtonDown(0))
                {
                    scrollerUnderMouse.isDragging = true;
                    scrollerUnderMouse.OffsetVelocity = Vector2.zero;
                    scrollerLastMousePos = mousePos;
                }
            }
            else
            {
                Vector2 delta = mousePos - scrollerLastMousePos;
                scrollerTotalDelta += delta;
                scrollerUnderMouse.Offset = scrollerUnderMouse.Offset + delta;
                scrollerLastMousePos = mousePos;

                if (buttonUnderMouse != null && scrollerTotalDelta.magnitude >= 10.0f)
                {
                    buttonUnderMouse.State = Button.StateType.Normal;
                    buttonUnderMouse = null;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    scrollerUnderMouse.OffsetVelocity += (delta / uiTimeSource.DeltaTime) * 1.0f;//0.25f;
                    scrollerTotalDelta = Vector2.zero;
                    scrollerUnderMouse.isDragging = false;
                    scrollerUnderMouse = null;
                }
            }
        }
#endif
    }

    protected void UpdateAnimatedOverlays()
    {
        //foreach (AnimatedOverlay animatedOverlay in animatedOverlays)
        for (int i = animatedOverlays.Count - 1; i >= 0; --i)
        {
            animatedOverlays[i].Update();//(uiTimeSource.DeltaTime);
        }
    }

    protected string GetResourceName(string baseName)
    {
        if (MultipleResolutionsBehavior.None == multipleResBehavior)
			return baseName;

        if (MultipleResolutionsBehavior.MultipleBatches == multipleResBehavior)
        {
#if UNITY_IPHONE
            int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
            switch (screenBiggerSize)
            {
                case 2048:
                    if (iphoneOnly)
                        return baseName + "@x2";
                    else
                        return baseName + "@ipadx2";
                case 1024:
                    if (iphoneOnly)
                        return baseName;
                    else
                        return baseName + "@ipad";
                case 960:
                    return baseName + "@x2";
                case 480:
                    return baseName;
                default:
                    Asserts.Assert(false);
                    return baseName;
            }
#elif UNITY_ANDROID
            int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
            if (screenBiggerSize >= 800)
                return baseName + "@x2";
            else
                return baseName;
#else
            Asserts.Assert(false, "MultipleResolutionsBehavior.MultipleBatches == multipleResBehavior");
#endif
        }
        else // MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior
        {
#if UNITY_ANDROID
            int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
            if (screenBiggerSize >= 800)
                return baseName + "@x2";
            else
                return baseName;
#else
            switch (deviceSim)
            {
                case DeviceSim.iPhone:
                    return baseName;
                case DeviceSim.iPhoneRetina:
                    return baseName + "@x2";
                case DeviceSim.iPad:
                    return baseName + "@ipad";
                case DeviceSim.iPadRetina:
                    return baseName + "@ipadx2";
                case DeviceSim.None:
                default:
                    return baseName;
            }
#endif
        }

        return null;
    }
    #endregion

    #region Public methods
    public OverlaysLayer GetOverlaysLayer()
    {
        return layer;
    }

    public float GetDeviceSimDpi()
    {
        switch (deviceSim)
        {
            case DeviceSim.iPhone:
                return 163.0f;
            case DeviceSim.iPhoneRetina:
                return 326.0f;
            case DeviceSim.iPad:
                return 132.0f;
            case DeviceSim.iPadRetina:
                return 264.0f;
            default:
                return Screen.dpi;
        }
    }

    public float GetDeviceSimScreenMaxSize()
    {
        switch (deviceSim)
        {
            case DeviceSim.iPhone:
                return 480.0f;
            case DeviceSim.iPhoneRetina:
                return 960.0f;
            case DeviceSim.iPad:
                return 1024.0f;
            case DeviceSim.iPadRetina:
                return 2048.0f;
            default:
                return SBSMath.Max(UIManager_OLD.screenWidth, UIManager_OLD.screenHeight);
        }
    }

    public void GetDeviceSimScaling(out float sx, out float sy)
    {
        sx = sy = 1.0f;

        if (DeviceSim.None == deviceSim)
            return;

        float simWidth = 1.0f,
              simHeight = 1.0f;
        switch (deviceSimOrientation)
        {
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                switch (deviceSim)
                {
                    case DeviceSim.iPhone:
                        simWidth = 320.0f;
                        simHeight = 480.0f;
                        break;
                    case DeviceSim.iPhoneRetina:
                        simWidth = 640.0f;
                        simHeight = 960.0f;
                        break;
                    case DeviceSim.iPad:
                        simWidth = 768.0f;
                        simHeight = 1024.0f;
                        break;
                    case DeviceSim.iPadRetina:
                        simWidth = 1536.0f;
                        simHeight = 2048.0f;
                        break;
                }
                break;
            case ScreenOrientation.LandscapeLeft:
            case ScreenOrientation.LandscapeRight:
            default:
                switch (deviceSim)
                {
                    case DeviceSim.iPhone:
                        simWidth = 480.0f;
                        simHeight = 320.0f;
                        break;
                    case DeviceSim.iPhoneRetina:
                        simWidth = 960.0f;
                        simHeight = 640.0f;
                        break;
                    case DeviceSim.iPad:
                        simWidth = 1024.0f;
                        simHeight = 768.0f;
                        break;
                    case DeviceSim.iPadRetina:
                        simWidth = 2048.0f;
                        simHeight = 1536.0f;
                        break;
                }
                break;
        }

        sx = UIManager_OLD.screenWidth / simWidth;
        sy = UIManager_OLD.screenHeight / simHeight;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, string texture)
    {
        OverlaysBatch batch = layer.AddBatch(capacity, Resources.Load(texture, typeof(Texture)) as Texture, Matrix4x4.identity);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, string texture, int insertIndex)
    {
        OverlaysBatch batch = layer.InsertBatch(capacity, Resources.Load(texture, typeof(Texture)) as Texture, Matrix4x4.identity, insertIndex);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, string texture, int insertIndex, SBSMatrix4x4 transform)
    {
        OverlaysBatch batch = layer.InsertBatch(capacity, Resources.Load(texture, typeof(Texture)) as Texture, transform, insertIndex);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, Texture texture)
    {
        OverlaysBatch batch = layer.AddBatch(capacity, texture, Matrix4x4.identity);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, Texture texture, int insertIndex)
    {
        OverlaysBatch batch = layer.InsertBatch(capacity, texture, Matrix4x4.identity, insertIndex);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public OverlaysBatch RegisterBatch(string name, int capacity, Texture texture, int insertIndex, SBSMatrix4x4 transform)
    {
        OverlaysBatch batch = layer.InsertBatch(capacity, texture, transform, insertIndex);
        batches.Add(name, batch);
        if (MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior)
        {
            float sx, sy;
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            if (SBSMath.Abs(1.0f - sx) > SBSMath.Epsilon ||
                SBSMath.Abs(1.0f - sy) > SBSMath.Epsilon)
                batch.Image.filterMode = FilterMode.Bilinear;
        }
        return batch;
    }

    public bool HasBatch(string name)
    {
        return batches.ContainsKey(this.GetResourceName(name));
    }

    public OverlaysBatch GetBatch(string name)
    {
        OverlaysBatch ret = null;
        if (batches.TryGetValue(this.GetResourceName(name), out ret))
            return ret;
        else
            return null;
    }

    public Rect GetOverlayRect(string batchName, string overlayName)
    {
        batchName = this.GetResourceName(batchName);
        foreach (UIAtlas data in _batches)
        {
            if (data.name/*atlas.name*/ == batchName)
            {
                foreach (UIAtlas.CutData data2 in data.cuts)
                {
                    if (data2.name == overlayName)
                        return data2.rect;
                }

                break;
            }
        }
		foreach (UIAtlas data in loadedBatches)
		{
            if (data.name == batchName)
            {
                foreach (UIAtlas.CutData data2 in data.cuts)
                {
                    if (data2.name == overlayName)
                        return data2.rect;
                }

                break;
            }
		}
        Asserts.Assert(false);
        return new Rect();
    }

    public bool HasOverlayRect(string batchName, string overlayName)
    {
        batchName = this.GetResourceName(batchName);
        foreach (UIAtlas data in _batches)
        {
            if (data.atlas.name == batchName)
            {
                foreach (UIAtlas.CutData data2 in data.cuts)
                {
                    if (data2.name == overlayName)
                        return true;
                }

                break;
            }
        }
		foreach (UIAtlas data in loadedBatches)
		{
            if (data.name == batchName)
            {
                foreach (UIAtlas.CutData data2 in data.cuts)
                {
                    if (data2.name == overlayName)
                        return true;
                }

                break;
            }
		}
        return false;
    }

    public void RegisterFont(string name, BitmapFont font)
    {
        fonts.Add(name, font);
    }

    public bool HasFont(string name)
    {
        return fonts.ContainsKey(this.GetResourceName(name));
    }

    public BitmapFont GetFont(string name)
    {
        return fonts[this.GetResourceName(name)];
    }

    public PopupsHolder RegisterPopupsHolder(string name)
    {
        PopupsHolder holder = new PopupsHolder();
        holders.Add(name, holder);
        return holder;
    }

    public bool HasPopupHolder(string name)
    {
        return holders.ContainsKey(name);
    }

    public PopupsHolder GetPopupHolder(string name)
    {
        return holders[name];
    }

    public Button AddButton(UIElement normal, UIElement mouseOver, UIElement pressed, UIElement disabled)
    {
        Button btn = new Button(normal, mouseOver, pressed, disabled);
        buttons.Add(btn);
        return btn;
    }

    public Button AddButton(Button btn)
    {
        buttons.Add(btn);
        return btn;
    }

    public void DestroyButton(Button button)
    {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        for (int i = 0; i < 4; ++i)
        {
            if (button == buttonUnderFinger[i])
                buttonUnderFinger[i] = null;
        }
#else
        if (button == buttonUnderMouse)
            buttonUnderMouse = null;
#endif
        button.Destroy();
        buttons.Remove(button);
    }

    public Scroller AddScroller(string name, Rect scrollArea)
    {
        Scroller scroller = new Scroller(name, scrollArea);
        scrollers.Add(scroller);
        return scroller;
    }

    public void DestroyScroller(Scroller scroller)
    {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        for (int i = 0; i < 4; ++i)
        {
            if (scroller == scrollerUnderFinger[i])
                scrollerUnderFinger[i] = null;
        }
#else
        if (scroller == scrollerUnderMouse)
            scrollerUnderMouse = null;
#endif
        scroller.Destroy();
        scrollers.Remove(scroller);
    }

    public void NotifyScrollerActive(Scroller scroller, bool active)
    {
        if (!active)
        {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
            for (int i = 0; i < 4; ++i)
            {
                if (scroller == scrollerUnderFinger[i])
                    scrollerUnderFinger[i] = null;
            }
#else
            if (scroller == scrollerUnderMouse)
                scrollerUnderMouse = null;
#endif
        }

        layer.SetScissorRectActive(scroller.Name, active);
    }

    public UIAnimation CreateUIAnimation(string name, string batchName, string baseOverlayName, int startFrame, int endFrame)
    {
        List<Rect> frames = new List<Rect>();
        OverlaysBatch batch = GetBatch(batchName);

        UIAnimation animation = new UIAnimation(name, batch, frames);
        if (startFrame >= 0 && endFrame >= 0)
        {
            for (int i = startFrame; i <= endFrame; ++i)
                frames.Add(GetOverlayRect(batchName, baseOverlayName + i));
        }
        else
            frames.Add(GetOverlayRect(batchName, baseOverlayName));

        return animation;
    }

    public AnimatedOverlay AddAnimatedOverlay(SBSMatrix4x4 _transform, Rect _scrRect, Color _color, UIAnimation _animation)
    {
        AnimatedOverlay animatedOverlay = new AnimatedOverlay(_transform, _scrRect, _color, _animation);
        animatedOverlays.Add(animatedOverlay);
        return animatedOverlay;
    }

    public void DestroyAnimatedOverlay(AnimatedOverlay animatedOverlay)
    {
        animatedOverlay.Destroy();
        animatedOverlays.Remove(animatedOverlay);
    }

    public void OnNewLevelLoaded()
    {
        if (dontDestroyOnLoad)
        {
            layer = GameObject.FindObjectOfType(typeof(OverlaysLayer)) as OverlaysLayer;//GameCamera.Instance.GetComponent<OverlaysLayer>();
            layer.Initialize(dontDestroyOnLoad);
        }
    }

    public void StartFadeOut(float duration, Action callback)
    {
        if (null == fadeOverlay)
            return;
		if (!fadeOverlay.Visibility)
			fadeOverlay.Visibility = true;
        fadeStartTime = Time.realtimeSinceStartup;
        fadeStartValue = fadeOverlay.Color.a;
        fadeEndValue = 1.0f;
        fadeEndTime = fadeStartTime + duration * (fadeEndValue - fadeStartValue);
        fadeCompleted = callback;
    }

    public void StartFadeIn(float duration, Action callback)
    {
        if (null == fadeOverlay)
            return;
		if (!fadeOverlay.Visibility)
			fadeOverlay.Visibility = true;
        fadeStartTime = Time.realtimeSinceStartup;
        fadeStartValue = fadeOverlay.Color.a;
        fadeEndValue = 0.0f;
        fadeEndTime = fadeStartTime + duration * (fadeStartValue - fadeEndValue);
        fadeCompleted = callback;
    }

    public void DisableAllButtons()
    {
        foreach (Button btn in buttons)
            btn.State = Button.StateType.Disabled;

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        buttonUnderFinger[0] = null;
        buttonUnderFinger[1] = null;
        buttonUnderFinger[2] = null;
        buttonUnderFinger[3] = null;
#else
        buttonUnderMouse = null;
#endif
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

#if UNITY_IPHONE
        if (iPhoneGeneration.iPodTouch1Gen == iPhone.generation ||
            iPhoneGeneration.iPodTouch2Gen == iPhone.generation ||
            iPhoneGeneration.iPodTouch3Gen == iPhone.generation ||
            iPhoneGeneration.iPhone3G == iPhone.generation ||
            iPhoneGeneration.iPhone3GS == iPhone.generation)
            multipleResBehavior = MultipleResolutionsBehavior.MultipleBatches;
#endif

        batches = new Dictionary<string, OverlaysBatch>();
        fonts = new Dictionary<string, BitmapFont>();
        holders = new Dictionary<string, PopupsHolder>();
        buttons = new List<Button>();
        scrollers = new List<Scroller>();
        animatedOverlays = new List<AnimatedOverlay>();

        controller = null;

        layer = GameObject.FindObjectOfType(typeof(OverlaysLayer)) as OverlaysLayer;//GameCamera.Instance.GetComponent<OverlaysLayer>();
        layer.Initialize(dontDestroyOnLoad);

        string wantedSuffix = null;
        if (MultipleResolutionsBehavior.None != multipleResBehavior)
        {
            if (MultipleResolutionsBehavior.MultipleBatches == multipleResBehavior)
            {
#if UNITY_IPHONE
	            switch (iPhone.generation)
	            {
				    case iPhoneGeneration.Unknown:
					    int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
			            switch (screenBiggerSize)
			            {
			                case 2048:
							    if (!iphoneOnly)
								    wantedSuffix = "ipadx2";
							    else
								    wantedSuffix = "x2";
							    break;
			                case 1024:
							    if (!iphoneOnly)
								    wantedSuffix = "ipad";
							    break;
			                case 960:
							    wantedSuffix = "x2";
							    break;
			                case 480:
							    wantedSuffix = null;
							    break;
			                default:
			                    Asserts.Assert(false);
							    wantedSuffix = "x2";
							    break;
					    }
					    break;
	                case iPhoneGeneration.iPad1Gen:
	                case iPhoneGeneration.iPad2Gen:
					    if (!iphoneOnly)
		                    wantedSuffix = "ipad";
	                    break;
                    case iPhoneGeneration.iPad3Gen:
					    if (!iphoneOnly)
	                        wantedSuffix = "ipadx2";
					    else
						    wantedSuffix = "x2";
                        break;
	                case iPhoneGeneration.iPhone4:
	                case iPhoneGeneration.iPhone4S:
	                case iPhoneGeneration.iPodTouch4Gen:
	                    wantedSuffix = "x2";
	                    break;
	            }
#elif UNITY_ANDROID
                deviceSim = DeviceSim.iPhone;
                int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
                if (screenBiggerSize >= 800)
                {
                    deviceSim = DeviceSim.iPhoneRetina;
                    wantedSuffix = "x2";
                }
#else
                Asserts.Assert(false, "MultipleResolutionsBehavior.MultipleBatches == multipleResBehavior");
#endif
            }
            else // MultipleResolutionsBehavior.ScaleToFit == multipleResBehavior
            {
#if UNITY_ANDROID
                deviceSim = DeviceSim.iPhone;
                int screenBiggerSize = Mathf.Max(Screen.width, Screen.height);
                if (screenBiggerSize >= 800)
                {
                    deviceSim = DeviceSim.iPhoneRetina;
                    wantedSuffix = "x2";
                }
#else
                switch (deviceSim)
                {
                    case DeviceSim.iPhone:
                        break;
                    case DeviceSim.iPhoneRetina:
                        wantedSuffix = "x2";
                        break;
                    case DeviceSim.iPad:
                        wantedSuffix = "ipad";
                        break;
                    case DeviceSim.iPadRetina:
                        wantedSuffix = "ipadx2";
                        break;
                    case DeviceSim.None:
                    default:
                        break;
                }
#endif
            }
        }

        foreach (BitmapFont font in _fonts)
        {
            int index = font.fontName.LastIndexOf("@");
            string suffix = index > 0 ? font.fontName.Substring(index + 1) : null;
            if (suffix != wantedSuffix)
                continue;

            font.Initialize();
            fonts.Add(font.fontName, font);
        }

		foreach (string fontPath in resourceFonts)
		{
            int index = fontPath.LastIndexOf("@");
            string suffix = index > 0 ? fontPath.Substring(index + 1) : null;
            if (suffix != wantedSuffix)
                continue;

			BitmapFont font = Resources.Load(fontPath, typeof(BitmapFont)) as BitmapFont;
			if (null == font)
				continue;

			font.Initialize();
			fonts.Add(font.fontName, font);
		}

		foreach (UIAtlas data in _batches)
        {
            string batchName = data.name;//atlas.name;

            int index = batchName.LastIndexOf("@");
            string suffix = index > 0 ? batchName.Substring(index + 1) : null;
            if (suffix != wantedSuffix)
                continue;

            this.RegisterBatch(batchName, data.capacity, data.atlas);
        }

		foreach (string batchPath in resourceBatches)
		{
            int index = batchPath.LastIndexOf("@");
            string suffix = index > 0 ? batchPath.Substring(index + 1) : null;
            if (suffix != wantedSuffix)
                continue;

			UIAtlas data = Resources.Load(batchPath, typeof(UIAtlas)) as UIAtlas;
			if (null == data)
				continue;

			loadedBatches.Add(data);
            this.RegisterBatch(data.name, data.capacity, data.atlas);
		}

		Texture2D fade = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        fade.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black }, 0);
        fade.Apply();
        fadeOverlay = new Overlay(this.RegisterBatch("Fade", 1, fade), SBSMatrix4x4.identity, new UIRect(0.0f, 0.0f, 1.0f, 1.0f), Rect.MinMaxRect(0, 0, 2, 2), new Color(0.0f, 0.0f, 0.0f, 0.0f));
		fadeOverlay.Visibility = false;
	}

    void Start()
    {
        uiTimeSource = new TimeSource();
        TimeManager.Instance.AddSource(uiTimeSource);
    }

    void LateUpdate()
    {
        this.UpdateFade();
        /*
        foreach (PopupsHolder holder in holders.Values)
            holder.Update();*/
        foreach (KeyValuePair<string, PopupsHolder> pair in holders)
            pair.Value.Update();

        this.UpdateScrollers();
        this.UpdateButtons();
        this.UpdateAnimatedOverlays();

        if (controller != null)
            controller.Update();
    }
#if !(UNITY_IPHONE || UNITY_ANDROID || UNITY_FLASH)
    void OnGUI()
    {/*
        foreach (PopupsHolder holder in holders.Values)
            holder.GUIUpdate();*/
        foreach (KeyValuePair<string, PopupsHolder> pair in holders)
            pair.Value.GUIUpdate();

        if (controller != null)
            controller.GUIUpdate();
    }
#endif
    void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.RemoveSource(uiTimeSource);
        uiTimeSource = null;

        fonts.Clear();
        /*
        foreach (PopupsHolder holder in holders.Values)
            holder.Destroy();

        foreach (OverlaysBatch batch in batches.Values)
            layer.DestroyBatch(batch);
        */
        foreach (KeyValuePair<string, PopupsHolder> pair in holders)
            pair.Value.Destroy();

        foreach (KeyValuePair<string, OverlaysBatch> pair in batches)
            layer.RemoveBatch(pair.Value);

        batches.Clear();

        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
