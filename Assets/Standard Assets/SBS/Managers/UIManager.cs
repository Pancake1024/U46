using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;
using SBS.Pool;

[AddComponentMenu("UI/UIManager")]
[RequireComponent(typeof(TimeSource))]
[ExecuteInEditMode]
public class UIManager : Manager<UIManager>
{
    #region Singleton instance
    public static UIManager Instance
    {
        get
        {
            return Manager<UIManager>.Get();
        }
    }
    #endregion

    #region Public members
    public LayerMask uiLayers;
    //public int inputLayer = 0;

    public int baseScreenWidth = 960;
    public int baseScreenHeight = 640;
    public float pixelsPerUnit = 320.0f;

    [Range(1.0f, 100.0f)]
    public float maxDepth = 1.0f;

    public bool dontDestroyOnLoad = false;
    public bool disableUnityMouseEvents = true;
    public bool disableInputs = false;
    #endregion

    #region Protected members
    protected IntrusiveList<UIButton> buttons = new IntrusiveList<UIButton>();
    protected IntrusiveList<UINineSlice> nineSlices = new IntrusiveList<UINineSlice>();
    protected IntrusiveList<UITextField> textFields = new IntrusiveList<UITextField>();
    protected IntrusiveList<UILayoutAndScaling> layouts = new IntrusiveList<UILayoutAndScaling>();
    protected IntrusiveList<UIPage> pages = new IntrusiveList<UIPage>();
    protected IntrusiveList<UIPopup> popups = new IntrusiveList<UIPopup>();
    protected IntrusiveList<UIScroller> scrollers = new IntrusiveList<UIScroller>();

    protected Stack<UIPopup> popupsStack = new Stack<UIPopup>();
/*
    protected uint nextFlyerID = 0;
    protected Dictionary<string, uint> flyerIDs = new Dictionary<string,uint>();
    protected Dictionary<uint, Pool<UIFlyer>> flyerPools = new Dictionary<uint,Pool<UIFlyer>>();
*/
    protected PoolCollection<UIFlyer> flyers;

    protected Camera uiCamera = null;
    protected TimeSource uiTimeSource = null;
    protected UIPage prevPage = null;
    protected UIPage activePage = null;
    protected Stack<UIPage> pagesStack = new Stack<UIPage>();
#if UNITY_WP8
    protected Collider2D[] buttonsBuffer;
#else
    protected Collider2D[] buttonsBuffer = new Collider2D[16];
#endif
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
    protected UIButton[] buttonUnderFinger = new UIButton[4];
#else
    protected UIButton buttonUnderMouse = null;
#endif

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
	protected UIScroller[] scrollerUnderFinger = new UIScroller[4];
	protected Vector2[] scrollerLastDelta = new Vector2[4];
    protected Vector2[] scrollerTotalDelta = new Vector2[4];
#   if UNITY_ANDROID || UNITY_WP8
    protected Vector2[] scrollerPrevPos = new Vector2[4];
#   endif
#else
    protected UIScroller scrollerUnderMouse = null;
    protected Vector2 scrollerLastMousePos;
    protected Vector2 scrollerTotalDelta = Vector2.zero;
    protected bool scrollerClickedOn = false;
#endif

    protected float prevScreenWidth = 0.0f;
    protected float prevScreenHeight = 0.0f;
    protected bool shouldDoLayout = false;
    #endregion

    #region Buttons
    public bool IsButtonUnder(Vector2 pos)
    {
        return this.GetButton(pos) != null;
    }

    public void AddButton(UIButton button)
    {
        buttons.Add(button);
    }

    public void RemoveButton(UIButton button)
    {
        buttons.Remove(button);
    }

    protected UIButton GetButton(Vector2 pos)
    {/*
        Collider2D coll = Physics2D.OverlapPoint(this.ScreenToWorld(pos), uiLayers);
        if (null == coll)
            return null;
        UIButton btn = coll.gameObject.GetComponent<UIButton>();
        if (null == btn || btn.inputLayer != inputLayer)
            return null;
        return btn;*/
#if UNITY_WP8
        buttonsBuffer = Physics2D.OverlapPointAll(this.ScreenToWorld(pos), uiLayers);
        int count = buttonsBuffer.Length;
#else
        int count = Physics2D.OverlapPointNonAlloc(this.ScreenToWorld(pos), buttonsBuffer, uiLayers);
#endif
        UIButton btnFound = null;
        for (int i = 0; i < count; ++i)
        {
            UIButton btn = buttonsBuffer[i].gameObject.GetComponent<UIButton>();
            if (null == btn)
                continue;
            UIScroller scr = btn.gameObject.GetComponentInParents<UIScroller>();
            if (scr != null)
            {
                Vector2 loc = uiCamera.transform.worldToLocalMatrix * uiCamera.ScreenToWorldPoint(pos);
                if (!scr.ScrollArea.Contains(loc - (Vector2)(uiCamera.transform.worldToLocalMatrix * scr.transform.position)))
                    continue;
            }
            if (null == btnFound || btn.inputLayer > btnFound.inputLayer)
                btnFound = btn;
        }
        return btnFound;
    }

    protected void UpdateButtons()
    {
#if SBS_PROFILER
        Profiler.BeginSample("UpdateButtons");
#endif

        if (disableInputs)
            return;

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
        for (int i = 0, c = Input.touchCount; i < c; ++i)
        {
            Touch t = Input.GetTouch(i);
			if (t.fingerId < 0 || t.fingerId >= 4)
				continue;

            Vector2 pos = t.position;
			//pos.y = Screen.height - pos.y;

            if (buttonUnderFinger[t.fingerId] != null && (!buttonUnderFinger[t.fingerId].gameObject.activeInHierarchy || UIButton.StateType.Disabled == buttonUnderFinger[t.fingerId].State))// || buttonUnderMouse.inputLayer != inputLayer))
                buttonUnderFinger[t.fingerId] = null;
			
			//bool exitFromCicle = false;
            switch (t.phase)
            {
                case TouchPhase.Began:
                    UIButton b = this.GetButton(pos);
                    UIScroller scr = scrollerUnderFinger[t.fingerId];
                    if (b != null && (null == scr || !scr.IsDragging || scr.inputLayer <= b.inputLayer))
                    {
                        b.touchFingerId = t.fingerId;
                        b.mouseOrTouchPosition = pos;
                        b.State = UIButton.StateType.Pressed;
                        buttonUnderFinger[t.fingerId] = b;
                    }
					//exitFromCicle = true;
                    UIButton.enablePressSignals = UIButton.enableReleaseSignals = false;
                    break;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Ended:
                    if (buttonUnderFinger[t.fingerId] != null && buttonUnderFinger[t.fingerId].State != UIButton.StateType.Disabled)
                    {
                        buttonUnderFinger[t.fingerId].mouseOrTouchPosition = pos;
                        buttonUnderFinger[t.fingerId].State = buttonUnderFinger[t.fingerId].Contains(this.ScreenToWorld(pos)) ? UIButton.StateType.MouseOver : UIButton.StateType.Normal;
						if (buttonUnderFinger[t.fingerId] != null && buttonUnderFinger[t.fingerId].State != UIButton.StateType.Disabled)
                        	buttonUnderFinger[t.fingerId].State = UIButton.StateType.Normal;
                    }
                    buttonUnderFinger[t.fingerId] = null;
					//exitFromCicle = true;
                    UIButton.enablePressSignals = UIButton.enableReleaseSignals = false;
                    break;
                case TouchPhase.Canceled:
                    if (buttonUnderFinger[t.fingerId] != null && buttonUnderFinger[t.fingerId].State != UIButton.StateType.Disabled)
                    {
                        buttonUnderFinger[t.fingerId].mouseOrTouchPosition = pos;
                        buttonUnderFinger[t.fingerId].State = UIButton.StateType.Normal;
                    }
                    buttonUnderFinger[t.fingerId] = null;
					//exitFromCicle = true;
                    UIButton.enablePressSignals = UIButton.enableReleaseSignals = false;
                    break;
            }
/*
			if (exitFromCicle)
				break;*/
        }

        UIButton.enablePressSignals = UIButton.enableReleaseSignals = true;
#else

        //if (scrollerUnderMouse != null && buttonUnderMouse != null && buttonUnderMouse.inputLayer < scrollerUnderMouse.inputLayer)
        //    return;

        if (buttonUnderMouse != null && (!buttonUnderMouse.gameObject.activeInHierarchy || UIButton.StateType.Disabled == buttonUnderMouse.State))// || buttonUnderMouse.inputLayer != inputLayer))
            buttonUnderMouse = null;

        Vector2 mousePos = Input.mousePosition;

        if (null == buttonUnderMouse)
        {
            buttonUnderMouse = this.GetButton(mousePos);
            if (buttonUnderMouse != null)
                buttonUnderMouse.State = Input.GetMouseButton(0) ? UIButton.StateType.Normal : UIButton.StateType.MouseOver;
        }
        else
        {
            bool mouseOver = buttonUnderMouse.Contains(this.ScreenToWorld(mousePos));
            if (mouseOver)
            {
                UIButton otherBtnUnderMouse = this.GetButton(mousePos);
                if (otherBtnUnderMouse != buttonUnderMouse)
                {
                    buttonUnderMouse.mouseOrTouchPosition = mousePos;
                    buttonUnderMouse.State = UIButton.StateType.Normal;

                    if (otherBtnUnderMouse != null)
                    {
                        buttonUnderMouse = otherBtnUnderMouse;
                        buttonUnderMouse.mouseOrTouchPosition = mousePos;
                        buttonUnderMouse.State = Input.GetMouseButton(0) ? UIButton.StateType.Normal : UIButton.StateType.MouseOver;
                    }
                    else
                    {
                        buttonUnderMouse.State = UIButton.StateType.Normal;
                        buttonUnderMouse = null;
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    buttonUnderMouse.mouseOrTouchPosition = mousePos;
                    buttonUnderMouse.State = UIButton.StateType.Pressed;
                }
                else if (!Input.GetMouseButton(0))
                {
                    buttonUnderMouse.mouseOrTouchPosition = mousePos;
                    buttonUnderMouse.State = UIButton.StateType.MouseOver;
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (UIButton.StateType.MouseOver == buttonUnderMouse.State)
                    {
                        buttonUnderMouse.mouseOrTouchPosition = mousePos;
                        buttonUnderMouse.State = UIButton.StateType.Pressed;
                    }
                }
                else
                {
                    buttonUnderMouse.mouseOrTouchPosition = mousePos;
                    buttonUnderMouse.State = UIButton.StateType.Normal;
                    buttonUnderMouse = null;
                }
            }
        }
#endif

#if SBS_PROFILER
        Profiler.EndSample();
#endif
    }
    #endregion

    #region NineSlices
    public void AddNineSlice(UINineSlice button)
    {
        nineSlices.Add(button);
    }

    public void RemoveNineSlice(UINineSlice button)
    {
        nineSlices.Remove(button);
    }
    #endregion

    #region TextFields
    public void AddTextField(UITextField textField)
    {
        textFields.Add(textField);
    }

    public void RemoveTextField(UITextField textField)
    {
        textFields.Remove(textField);
    }
    #endregion

    #region Layouts

    public void AddLayout(UILayoutAndScaling layout)
    {
        layouts.Add(layout);
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (uiCamera != null)
            {
                float heightScale = baseScreenHeight / uiCamera.pixelHeight,
                      aspectScale = (baseScreenHeight * uiCamera.aspect) / baseScreenWidth;
                layout.UpdateLayoutAndScaling(heightScale, aspectScale);
            }
        }
    }

    public void RemoveLayout(UILayoutAndScaling layout)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            //layout.ResetLayoutAndScaling();
        }

        layouts.Remove(layout);
    }
    #endregion

    #region Pages
    public UIPage ActivePage
    {
        get
        {
            return activePage;
        }
    }

    public UIPage PreviousPage
    {
        get
        {
            return prevPage;
        }
    }

    public string ActivePageName
    {
        get
        {
            return activePage != null ? activePage.name : string.Empty;
        }
    }

    public string PreviousPageName
    {
        get
        {
            return prevPage != null ? prevPage.name : string.Empty;
        }
    }
    
    public void AddPage(UIPage page)
    {
        pages.Add(page);
    }

    public void RemovePage(UIPage page)
    {
        pages.Remove(page);
    }

    public UIPage GetPage(string name)
    {
        foreach (UIPage page in pages)
        {
            if (page.name.Equals(name))
                return page;
        }
        return null;
    }

#if UNITY_EDITOR
    protected bool gotoPageGuard = false;
#endif
    public void GoToPage(string name)
    {
#if UNITY_EDITOR
        if (gotoPageGuard)
        {
            Debug.LogError("GoToPage called inside another GoToPage");
            return;
        }

        gotoPageGuard = true;
#endif
        prevPage = activePage;

        if (activePage != null)
        {
            activePage.onExit.Invoke(activePage);
            activePage.gameObject.SetActive(false);
            activePage = null;
        }
        foreach (UIPage page in pages)
        {
            if (page.name.Equals(name))
            {
                activePage = page;
                page.gameObject.SetActive(true);
                break;
            }
        }
#if UNITY_EDITOR
        gotoPageGuard = false;
#endif
    }

    public void GoToPage(UIPage page)
    {
#if UNITY_EDITOR
        if (gotoPageGuard)
        {
            Debug.LogError("GoToPage called inside another GoToPage");
            return;
        }

        gotoPageGuard = true;
#endif
        if (activePage != null)
        {
            activePage.onExit.Invoke(activePage);
            activePage.gameObject.SetActive(false);
            activePage = null;
        }
        foreach (UIPage otherPage in pages)
        {
            if (otherPage == page)
            {
                activePage = otherPage;
                otherPage.gameObject.SetActive(true);
                break;
            }
        }
#if UNITY_EDITOR
        gotoPageGuard = false;
#endif
    }
    #endregion

    #region Popups
    public int PopupsInStack
    {
        get
        {
            return popupsStack.Count;
        }
    }

    public UIPopup FrontPopup
    {
        get
        {
            if (popupsStack.Count > 0)
                return popupsStack.Peek();
            else
                return null;
        }
    }

    public void AddPopup(UIPopup popup)
    {
        popups.Add(popup);
    }

    public void RemovePopup(UIPopup popup)
    {
        this.RemovePopupFromStack(popup);

        popups.Remove(popup);
    }

    public bool IsPopupInStack(UIPopup popup)
    {
        return popupsStack.Contains(popup);
    }

    public bool IsPopupInStack(string name)
    {
        foreach (UIPopup popup in popups)
        {
            if (popup.name.Equals(name))
                return this.IsPopupInStack(popup);
        }
        return false;
    }

    public void PushPopup(UIPopup popup, bool deactivatePrevious = true)
    {
        if (popupsStack.Count > 0 && deactivatePrevious)
            popupsStack.Peek().gameObject.SetActive(false);

        popupsStack.Push(popup);

       if (popup.pausesGame)
            TimeManager.Instance.MasterSource.Pause();

        popup.gameObject.SetActive(true);
    }

    public void PushPopup(string name, bool deactivatePrevious=true)
    {
        foreach (UIPopup popup in popups)
        {
            if (popup.name.Equals(name))
            {
                this.PushPopup(popup, deactivatePrevious);
                break;
            }
        }
    }

    public void EnqueuePopup(UIPopup popup)
    {
        if (0 == popupsStack.Count)
        {
            this.PushPopup(popup);
            return;
        }

        UIPopup[] stack = popupsStack.ToArray();

        popupsStack.Clear();
        popupsStack.Push(popup);

        for (int i = stack.Length - 1; i >= 0; --i)
            popupsStack.Push(stack[i]);
    }

    public void EnqueuePopup(string name)
    {
        foreach (UIPopup popup in popups)
        {
            if (popup.name.Equals(name))
            {
                this.EnqueuePopup(popup);
                break;
            }
        }
    }

    public bool PopPopup()
    {
        if (popupsStack.Count > 0)
        {
            UIPopup popup = popupsStack.Peek();

            popup.gameObject.SetActive(false);
            popupsStack.Pop();

           if (popup.pausesGame)
                TimeManager.Instance.MasterSource.Resume();
            popup.onRemoveFromStack.Invoke(popup);

            if (popupsStack.Count > 0)
                popupsStack.Peek().gameObject.SetActive(true);

            return true;
        }
        return false;
    }

    public bool RemovePopupFromStack(UIPopup popup)
    {
        UIPopup[] stack = popupsStack.ToArray();
        int index = Array.IndexOf<UIPopup>(stack, popup);
        if (index >= 0)
        {
            popupsStack.Clear();
            for (int i = stack.Length - 1; i >= 0; --i)
            {
                if (i == index)
                    continue;

                popupsStack.Push(stack[i]);
            }

            popup.gameObject.SetActive(false);
            if (popup.pausesGame)
                TimeManager.Instance.MasterSource.Resume();
            popup.onRemoveFromStack.Invoke(popup);

            if (0 == index && popupsStack.Count > 0)
                popupsStack.Peek().gameObject.SetActive(true);

            return true;
        }
        return false;
    }

    public bool RemovePopupFromStack(string name)
    {
        foreach (UIPopup popup in popups)
        {
            if (popup.name.Equals(name))
                return this.RemovePopupFromStack(popup);
        }
        return false;
    }

    public bool BringPopupToFront(UIPopup popup)
    {
        UIPopup[] stack = popupsStack.ToArray();
        int index = Array.IndexOf<UIPopup>(stack, popup);
        if (index >= 0)
        {
            if (0 == index)
                return true;

            popupsStack.Peek().gameObject.SetActive(false);

            popupsStack.Clear();
            for (int i = stack.Length - 1; i >= 0; --i)
            {
                if (i == index)
                    continue;

                popupsStack.Push(stack[i]);
            }

            popupsStack.Push(popup);
            popup.gameObject.SetActive(true);

            return true;
        }
        return false;
    }

    public bool BringPopupToFront(string name)
    {
        foreach (UIPopup popup in popups)
        {
            if (popup.name.Equals(name))
                return this.BringPopupToFront(popup);
        }
        return false;
    }

    public void ClearPopups()
    {
        while (this.PopPopup()) ;
    }
    #endregion

    #region Flyers
    public void RegisterFlyerPrefab(UIFlyer flyerPrefab, int poolSize)
    {/*
#if UNITY_EDITOR
        if (PrefabUtility.GetPrefabType(flyerPrefab) != PrefabType.Prefab)
        {
            Debug.LogError("Flyer \"" + flyerPrefab.name + "\" is not a Prefab!", flyerPrefab);
            return;
        }

        if (flyerIDs.ContainsKey(flyerPrefab.name))
        {
            Debug.LogError("Flyer \"" + flyerPrefab.name + "\" already exist!", flyerPrefab);
            return;
        }
#endif
        flyerIDs.Add(flyerPrefab.name, nextFlyerID);
        flyerPools.Add(nextFlyerID, new Pool<UIFlyer>(flyerPrefab, true, poolSize));

        ++nextFlyerID;*/
        flyers.RegisterPrefab(flyerPrefab, poolSize);
    }

    public void RegisterFlyerPrefab(UIFlyer flyerPrefab)
    {
        //this.RegisterFlyerPrefab(flyerPrefab, 0);
        flyers.RegisterPrefab(flyerPrefab);
    }

    public void RegisterFlyer(UIFlyer flyer, int poolSize)
    {/*
#if UNITY_EDITOR
        if (PrefabType.Prefab == PrefabUtility.GetPrefabType(flyer))
        {
            Debug.LogError("Flyer \"" + flyer.name + "\" is not an instance!", flyer);
            return;
        }

        if (flyerIDs.ContainsKey(flyer.name))
        {
            Debug.LogError("Flyer \"" + flyer.name + "\" already exist!", flyer);
            return;
        }
#endif
        flyerIDs.Add(flyer.name, nextFlyerID);
        flyerPools.Add(nextFlyerID, new Pool<UIFlyer>(flyer, false, poolSize));

        ++nextFlyerID;*/
        flyers.RegisterInstance(flyer, poolSize);
    }

    public void RegisterFlyer(UIFlyer flyer)
    {
        //this.RegisterFlyer(flyer, 0);
        flyers.RegisterInstance(flyer);
    }

    public uint GetFlyerID(string flyerName)
    {/*
        uint hash;
        if (flyerIDs.TryGetValue(flyerName, out hash))
            return hash;
        return uint.MaxValue;*/
        return flyers.GetOriginalID(flyerName);
    }

    public uint GetFlyerID(UIFlyer flyer)
    {
        //return this.GetFlyerID(flyer.name);
        return flyers.GetOriginalID(flyer);
    }

    public IntrusiveList<UIFlyer> GetActiveFlyers(uint flyerID)
    {/*
#if UNITY_EDITOR
        if (!flyerPools.ContainsKey(flyerID))
        {
            Debug.LogError("Flyer ID" + flyerID + " doesn't exist!");
            return null;
        }
#endif
        return flyerPools[flyerID].UsedList;*/
        return flyers.GetActiveList(flyerID);
    }

    public IntrusiveList<UIFlyer> GetActiveFlyers(string flyerName)
    {
        //return this.GetActiveFlyers(this.GetFlyerID(flyerName));
        return flyers.GetActiveList(flyerName);
    }

    public IntrusiveList<UIFlyer> GetActiveFlyers(UIFlyer flyer)
    {
        //return this.GetActiveFlyers(this.GetFlyerID(flyer));
        return flyers.GetActiveList(flyer);
    }

    public UIFlyer PlayFlyer(uint flyerID, Vector3 position, Quaternion rotation)
    {/*
#if UNITY_EDITOR
        if (!flyerPools.ContainsKey(flyerID))
        {
            Debug.LogError("Flyer ID" + flyerID + " doesn't exist!");
            return null;
        }
#endif
        UIFlyer flyer = flyerPools[flyerID].Get();
        flyer.transform.position = transform.TransformPoint(position);
        flyer.transform.rotation = transform.rotation * rotation;
        flyer.Play();

        return flyer;*/
        UIFlyer flyer = flyers.Instantiate(flyerID);
        flyer.transform.position = transform.TransformPoint(position);
        flyer.transform.rotation = transform.rotation * rotation;
        flyer.Play();
        return flyer;
    }

    public UIFlyer PlayFlyer(string flyerName, Vector3 position, Quaternion rotation)
    {
        return this.PlayFlyer(this.GetFlyerID(flyerName), position, rotation);
    }

    public UIFlyer PlayFlyer(UIFlyer flyer, Vector3 position, Quaternion rotation)
    {
        return this.PlayFlyer(this.GetFlyerID(flyer), position, rotation);
    }

    protected void UpdateFlyers()
    {
#if SBS_PROFILER
        Profiler.BeginSample("UpdateFlyers");
#endif
        /*foreach (KeyValuePair<uint, Pool<UIFlyer>> item in flyerPools)
        {
            UIFlyer node = item.Value.UsedList.Head;
            while (node != null)
            {
                node.UpdateFlyer();

                node = node.next;
            }
        }*/
        foreach (UIFlyer flyer in flyers)
            flyer.UpdateFlyer();
#if SBS_PROFILER
        Profiler.EndSample();
#endif
    }
    #endregion

    #region Scrollers
    public void AddScroller(UIScroller scroller)
    {
        scrollers.Add(scroller);
    }

    public void RemoveScroller(UIScroller scroller)
    {
        scrollers.Remove(scroller);
    }

    protected void UpdateScrollers()
    {
        foreach (UIScroller scroller in scrollers)
        {
            bool isMovingUnderMouse = false;
#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
            for (int i = 0; i < 4; ++i)
                isMovingUnderMouse = isMovingUnderMouse || (scroller == scrollerUnderFinger[i] && scroller.IsDragging);
#else
            isMovingUnderMouse = (scroller == scrollerUnderMouse && scroller.IsDragging);
#endif
            scroller.UpdateScroller(uiTimeSource.DeltaTime, isMovingUnderMouse);
            //layer.SetScissorRectOffset(scroller.Name, scroller.Offset);
        }

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8) && !UNITY_EDITOR
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch t = Input.GetTouch(i);
			if (t.fingerId < 0 || t.fingerId >= 4)
				continue;
			
            Vector2 pos = uiCamera.transform.worldToLocalMatrix * uiCamera.ScreenToWorldPoint(t.position);

            if (scrollerUnderFinger[t.fingerId] != null && !scrollerUnderFinger[t.fingerId].gameObject.activeInHierarchy)
                scrollerUnderFinger[t.fingerId] = null;
			
			UIScroller scr = null;
            switch (t.phase)
            {
                case TouchPhase.Began:
                    scr = this.GetScroller(pos, t.position);
                    if (scr != null)
                    {
                        scr.OffsetVelocity = Vector2.zero;
                        scrollerUnderFinger[t.fingerId] = scr;
						scrollerLastDelta[t.fingerId] = Vector2.zero;
                        scrollerTotalDelta[t.fingerId] = Vector2.zero;
#   if UNITY_ANDROID || UNITY_WP8
                        scrollerPrevPos[t.fingerId] = t.position;
#   endif
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
#   if UNITY_ANDROID || UNITY_WP8
                        Vector2 delta = t.position - scrollerPrevPos[t.fingerId];
                        scrollerPrevPos[t.fingerId] = t.position;

                        delta = uiCamera.transform.worldToLocalMatrix.MultiplyVector(uiCamera.ScreenToWorldPoint(delta) - uiCamera.ViewportToWorldPoint(Vector3.zero));
#   else
                        Vector2 delta = uiCamera.transform.worldToLocalMatrix.MultiplyVector(uiCamera.ScreenToWorldPoint(t.deltaPosition) - uiCamera.ViewportToWorldPoint(Vector3.zero));
#   endif
                        //delta.y = -delta.y;

                        scrollerLastDelta[t.fingerId] = delta;
                        scrollerTotalDelta[t.fingerId] += delta;

                        bool thresholdOk = scrollerTotalDelta[t.fingerId].magnitude > (10.0f / pixelsPerUnit);

                        if (!scr.IsDragging)
                        {
                            scr.IsDragging = thresholdOk;
                        }
                        else
                        {
                            scr.Offset = scr.Offset + delta;
                            if (buttonUnderFinger[t.fingerId] != null && thresholdOk)
                            {
                        	    buttonUnderFinger[t.fingerId].State = UIButton.StateType.Normal;
                                buttonUnderFinger[t.fingerId] = null;
                            }
                        }
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    scr = scrollerUnderFinger[t.fingerId];
                    if (scr != null)
                    {
                        Vector2 delta = scrollerLastDelta[t.fingerId];
                        scr.OffsetVelocity += (delta / uiTimeSource.DeltaTime) * scr.inputVelMult;//1.0f;//0.25f;
                        if (!scr.IsDragging)
                            scr.onClickEvent.Invoke(t.position);
                        scr.IsDragging = false;
                    }
                    scrollerUnderFinger[t.fingerId] = null;
                    break;
            }
        }
#else
        if (scrollerUnderMouse != null && !scrollerUnderMouse.gameObject.activeInHierarchy)
            scrollerUnderMouse = null;

        Vector2 mousePos = uiCamera.transform.worldToLocalMatrix * uiCamera.ScreenToWorldPoint(Input.mousePosition);
        //mousePos.y = Screen.height - mousePos.y;

        if (null == scrollerUnderMouse)
        {
            scrollerUnderMouse = this.GetScroller(mousePos, Input.mousePosition);
        }
        else
        {
            bool mouseOver = scrollerUnderMouse.ScrollArea.Contains(mousePos - (Vector2)(uiCamera.transform.worldToLocalMatrix * scrollerUnderMouse.transform.position));
            if (!scrollerUnderMouse.IsDragging)
            {
                UIButton btnUnderTouch = this.GetButton(Input.mousePosition);
                if (btnUnderTouch != null &&
                    btnUnderTouch.gameObject.GetComponentInParents<UIScroller>() != scrollerUnderMouse &&
                    btnUnderTouch.inputLayer > scrollerUnderMouse.InputLayer)
                {
                    scrollerUnderMouse = null;
                    return;
                }

                if (mouseOver && Input.GetMouseButtonDown(0))
                {
                    scrollerLastMousePos = mousePos;
                    scrollerClickedOn = true;
                }

                if (mouseOver && scrollerClickedOn && Input.GetMouseButton(0) && Vector2.Distance(mousePos, scrollerLastMousePos) > (10.0f / pixelsPerUnit))
                {
                    scrollerClickedOn = false;
                    scrollerUnderMouse.IsDragging = true;
                    scrollerUnderMouse.OffsetVelocity = Vector2.zero;
                    scrollerLastMousePos = mousePos;
                }

                //if (mouseOver && Input.GetMouseButtonUp(0) && !scrollerUnderMouse.IsDragging)
                //    scrollerUnderMouse.onClickEvent.Invoke((Vector2)Input.mousePosition);
            }
            else
            {
                Vector2 delta = mousePos - scrollerLastMousePos;
                scrollerTotalDelta += delta;
                scrollerUnderMouse.Offset = scrollerUnderMouse.Offset + delta;
                scrollerLastMousePos = mousePos;

                if (Input.GetMouseButtonUp(0))
                {
                    //scrollerUnderMouse.ScrollerReleasedEvent(); //TEMP
                    scrollerUnderMouse.OffsetVelocity += (delta / uiTimeSource.DeltaTime) * scrollerUnderMouse.inputVelMult;//1.0f; //0.25f;
                    scrollerTotalDelta = Vector2.zero;
                    scrollerUnderMouse.IsDragging = false;
                    scrollerUnderMouse = null;
                }
            }
        }
#endif
    }

    protected UIScroller GetScroller(Vector2 position, Vector2 screenPos)
    {
        UIScroller scrFound = null;
        UIButton btnUnderTouch = this.GetButton(screenPos);
        UIScroller btnUnderTouchParentScroller = null == btnUnderTouch ? null : btnUnderTouch.gameObject.GetComponentInParents<UIScroller>();

        foreach (UIScroller scroller in scrollers)
        {
            if (!scroller.ScrollX && !scroller.ScrollY)
                continue;

            if (scroller.ScrollArea.Contains(position - (Vector2)(uiCamera.transform.worldToLocalMatrix * scroller.transform.position)) &&
                (null == btnUnderTouch || btnUnderTouchParentScroller == scroller || (scroller.InputLayer > btnUnderTouch.inputLayer)) &&
                (scrFound == null || (scrFound.InputLayer < scroller.InputLayer)))
                scrFound = scroller;
        }

        return scrFound;
    }
    #endregion

    #region Camera
    public Camera UICamera
    {
        get
        {
            return uiCamera;
        }
    }

    protected void CreateCamera()
    {
        GameObject camGO = GameObject.Find("uiCamera_" + name);
        if (camGO != null)
            uiCamera = camGO.GetComponent<Camera>();
        if (null == uiCamera)
        {
            GameObject go = new GameObject("uiCamera_" + name);
            go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;

           
            uiCamera = /*gameObject*/go.AddComponent<Camera>();

            uiCamera.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            uiCamera.useOcclusionCulling = false;
            uiCamera.hdr = false;
            uiCamera.orthographic = true;
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.depth = float.MaxValue;
            uiCamera.nearClipPlane = 0.0f;
            //uiCamera.farClipPlane = 1.0f;

            if (disableUnityMouseEvents)
            {
                foreach (Camera cam in Camera.allCameras)
                    cam.eventMask = 0;
            }
            else
                uiCamera.eventMask = 0;
        }

        uiCamera.cullingMask = uiLayers;
        uiCamera.orthographicSize = baseScreenHeight * 0.5f / pixelsPerUnit;
        uiCamera.farClipPlane = maxDepth;
    }

    protected void DestroyCamera()
    {
        if (uiCamera != null)
        {
            Camera.DestroyImmediate(uiCamera.gameObject);
            uiCamera = null;
        }
    }

    protected Vector3 ScreenToWorld(Vector3 screenPos)
    {
        return uiCamera.ScreenToWorldPoint(screenPos);/*
        Vector3 clipPos = new Vector3((screenPos.x * 2.0f / Screen.width ) - 1.0f,
                                      (screenPos.y * 2.0f / Screen.height) - 1.0f,
                                      uiCamera.nearClipPlane);
        clipPos.y *= uiCamera.orthographicSize;
        clipPos.x *= (uiCamera.orthographicSize * uiCamera.aspect);
        return uiCamera.cameraToWorldMatrix * clipPos;*/
    }

    protected void SetupCamera()
    {
#if UNITY_EDITOR
        uiCamera.cullingMask = uiLayers;
        uiCamera.orthographicSize = baseScreenHeight * 0.5f / pixelsPerUnit;
        uiCamera.farClipPlane = maxDepth;

        if (Application.isPlaying)
            uiCamera.ResetAspect();
        else
            uiCamera.aspect = baseScreenWidth / (float)baseScreenHeight;
#else
        uiCamera.farClipPlane = maxDepth;

        uiCamera.ResetAspect();
#endif
    }
    #endregion

    #region TimeSource
    public TimeSource UITimeSource
    {
        get
        {
            return uiTimeSource;
        }
    }
    #endregion

    #region Unity callbacks
    void OnEnable()
    {
        this.CreateCamera();
    }

    void OnDisable()
    {
        this.DestroyCamera();
    }

    new void Awake()
    {
        base.Awake();
        
        flyers.Initialize();
    }

    void Start()
    {
#if UNITY_EDITOR
        gotoPageGuard = false;
#endif

        uiTimeSource = new TimeSource();
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            TimeManager.Instance.AddSource(uiTimeSource);
        }
        if (dontDestroyOnLoad)
            GameObject.DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        this.SetupCamera();

        this.UpdateScrollers();

        UINineSlice nsNode = nineSlices.Head;
        while (nsNode != null)
        {
            nsNode.UpdateSlices();
#if UNITY_EDITOR
            if (!Application.isPlaying)
                nsNode.LateUpdateSlices();
#endif
            nsNode = nsNode.next;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UITextField tfNode = textFields.Head;
            while (tfNode != null)
            {
                tfNode.UpdateText();
                tfNode = tfNode.next;
            }
        }

        if (Application.isPlaying)
#endif
        {
            if (uiCamera.pixelWidth != prevScreenWidth || uiCamera.pixelHeight != prevScreenHeight)
            {
                UILayoutAndScaling lsNode = layouts.Head;
                while (lsNode != null)
                {
                    lsNode.ResetLayoutAndScaling();
                    //lsNode.UpdateLayoutAndScaling(heightScale, aspectScale);
                    lsNode = lsNode.next;
                }

                prevScreenWidth = uiCamera.pixelWidth;
                prevScreenHeight = uiCamera.pixelHeight;
                shouldDoLayout = true;
            }
        }
    }

    void LateUpdate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (shouldDoLayout)
            {
                float heightScale = baseScreenHeight / uiCamera.pixelHeight,
                      aspectScale = (baseScreenHeight * uiCamera.aspect) / baseScreenWidth;
                UILayoutAndScaling lsNode = layouts.Head;
                while (lsNode != null)
                {
                    lsNode.UpdateLayoutAndScaling(heightScale, aspectScale);
                    lsNode = lsNode.next;
                }

                shouldDoLayout = false;
            }

            this.UpdateButtons();

            UITextField tfNode = textFields.Head;
            while (tfNode != null)
            {
                tfNode.UpdateText();
                tfNode = tfNode.next;
            }

            this.UpdateFlyers();

            UINineSlice nsNode = nineSlices.Head;
            while (nsNode != null)
            {
                nsNode.LateUpdateSlices();
                nsNode = nsNode.next;
            }
        }
    }

    void OnLevelWasLoaded(int level)
    {
        if (disableUnityMouseEvents)
        {
            foreach (Camera cam in Camera.allCameras)
                cam.eventMask = 0;
        }
        else
            uiCamera.eventMask = 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (uiCamera != null)
        {
            Gizmos.DrawWireCube(
                transform.TransformPoint(Vector3.forward * (uiCamera.nearClipPlane + uiCamera.farClipPlane) * 0.5f),
                new Vector3(2.0f * uiCamera.orthographicSize * uiCamera.aspect, 2.0f * uiCamera.orthographicSize, uiCamera.farClipPlane - uiCamera.nearClipPlane));

            //Gizmos.DrawSphere(this.ScreenToWorld(Input.mousePosition), 0.1f);
        }
    }
#endif
    #endregion
}
