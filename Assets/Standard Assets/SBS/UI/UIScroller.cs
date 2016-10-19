using System;
using UnityEngine;
using SBS.Math;
using SBS.Core;

[RequireComponent(typeof(BoxCollider2D))]
//[ExecuteInEditMode]
[AddComponentMenu("UI/UIScroller")]
public class UIScroller : IntrusiveListNode<UIScroller>
{
    //protected const float maxSpeed = 32.0f;
    protected const float fixedDt = 0.02f;

    #region Public Members
    public int inputLayer = 0;
    public float mSpringK = 50.0f;
    public float mSpringBounce = 0.0f;
    public float mSpringRebound = 15.0f;
    public float mDamping = 0.6f; //0.8f

    public DragActions xDragSets = DragActions.Snap;
    public DragActions yDragSets = DragActions.Snap;

    public int snapStartXInterval = 0;
    public int snapEndXInterval = 0;
    public int snapStartYInterval = 0;
    public int snapEndYInterval = 0;

    public float dampTime = 0.4f;
    public float maxSpeed = 32.0f;
    public float inputVelMult = 1.0f;
    public bool textfieldBoundsControl = false;

    public Signal onSnapDataChangedEvent = Signal.Create<UIScroller>();
    public Signal onClickEvent = Signal.Create<Vector2>();

    #endregion

    #region Protected Members
    protected Transform offsetNode = null;
    protected bool isDragging = false;
    protected Vector2 offset = Vector2.zero;
    protected Vector2 offsetVelocity = Vector2.zero;
    protected Rect mOffsetRect;
    protected Rect mOffsetRectExtra;
    [SerializeField]
    protected Rect mScrollArea = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
    protected Rect mTotalArea;

    public Vector2 mSnapSpace = Vector2.zero;
    protected SBSTuple<int, int> mSnapIndices = new SBSTuple<int, int>(0, 0);
    //protected Interval<int> mSnapXInterval;// = Interval<int>.CreateLowerBound(0);
    //protected Interval<int> mSnapXInterval2 = Interval<int>.CreateLowerBound(0);
    protected int mIndexOffsetX = 0;
    protected bool mIsEasingOnX = false;

    //protected Interval<int> mSnapYInterval2 = Interval<int>.CreateLowerBound(0);
    protected int mIndexOffsetY = 0;
    protected bool mIsEasingOnY = false;
    protected Vector2 mSnapTargets = Vector2.zero;

    protected Vector2 mLastSpringForce = Vector2.zero;

    protected float damping = 0.8f;
    protected Vector2 prevSpringForce = Vector2.zero;

    protected bool scrollX = true;
    protected bool scrollY = true;

    //protected Settings mSettings;

    protected int startInputLayer;
    protected UIButton[] scrButtonsList;
    protected int[] scrButtonsInputLayers;

    protected int elementCount = 0;

    protected BoxCollider2D box;

    protected bool isAddingElements = false;
    protected float mActiveSpringK;
    protected float mActiveSpringKVel;
    protected float mActiveSpringBounce;
    protected float mActiveSpringBounceVel;
    protected float mActiveSpringRebound;
    protected float mActiveSpringReboundVel;
    #endregion

    #region Public Enums
    //[Flags]
    //public enum Settings
    //{
    //    SnapOnX = 0x1,
    //    LockOnX = 0x3,
    //    SnapOnY = 0x4,
    //    LockOnY = 0xC
    //}

    public enum DragActions
    {
        Free,
        Locked,
        Snap
    }
    #endregion

    #region Properties
    public Vector2 snapSpace
    {
        get
        {
            return mSnapSpace;
        }
        set
        {
            mSnapSpace = value;

            this.UpdateSnapData();
        }
    }

    public bool IsEasing
    {
        get { return mIsEasingOnX || mIsEasingOnY; }
    }

    public int snapX
    {
        get
        {
            //int span = mSnapXInterval2.End - mSnapXInterval2.Start + 1;
            //if (span > 0 && mSnapXInterval2.HasStart)
            //    return (mSnapIndices.First - mSnapXInterval2.Start + mIndexOffsetX) % span;
            //else if (span > 0 && mSnapXInterval2.HasEnd)
            //    return -((mSnapXInterval2.End - mSnapIndices.First + mIndexOffsetX) % span);
            //else
            return mSnapIndices.First;
        }
        set
        {
            this.setSnapX(value);

            mIsEasingOnX = false;
            offset.x = mSnapTargets.x;
        }
    }

    public int snapY
    {
        get
        {
            //int span = mSnapYInterval2.End - mSnapYInterval2.Start + 1;
            //if (span > 0 && mSnapYInterval2.HasStart)
            //    return (mSnapIndices.Second - mSnapYInterval2.Start + mIndexOffsetY) % span;
            //else if (span > 0 && mSnapYInterval2.HasEnd)
            //    return -((mSnapYInterval2.End - mSnapIndices.Second + mIndexOffsetY) % span);
            //else
            return mSnapIndices.Second;
        }
        set
        {
            this.setSnapY(value);

            mIsEasingOnY = false;
            offset.y = mSnapTargets.y;
        }
    }

    public int InputLayer
    {
        get { return inputLayer; }
        set { inputLayer = value; }
    }

    public int StartInputLayer
    {
        get { return startInputLayer; }
    }

    public bool IsDragging
    {
        get
        {
            return isDragging;
        }
        set
        {
            if (value == isDragging)
                return;

            isDragging = value;
            if (isDragging)
            {
                mActiveSpringK = 0.0f;
                mActiveSpringKVel = 0.0f;

                mActiveSpringBounce = 0.0f;
                mActiveSpringBounceVel = 0.0f;

                mActiveSpringRebound = 0.0f;
                mActiveSpringReboundVel = 0.0f;

                foreach (UIButton bt in scrButtonsList)
                {
                    bt.inputLayer = -1;
                    bt.State = bt.State == UIButton.StateType.Disabled ? UIButton.StateType.Disabled : UIButton.StateType.Normal;
                }
            }
            else
            {
                for (int i = 0; i < scrButtonsList.Length; i++ )
                    scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];
            }
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

    public Vector2 Offset
    {
        get
        {
            return offset;
        }
        set
        {
            offset = value;

            offset.x = Mathf.Clamp(value.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
            offset.y = Mathf.Clamp(value.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);

            mIsEasingOnX = mIsEasingOnY = false;

            this.UpdateSnapData();
        }
    }

    public Rect OffsetRect
    {
        get { return mOffsetRect; }
    }

    public Vector2 OffsetVelocity
    {
        get
        {
            return offsetVelocity;
        }
        set
        {
            offsetVelocity.x = Mathf.Clamp(value.x, -maxSpeed, maxSpeed);
            offsetVelocity.y = Mathf.Clamp(value.y, -maxSpeed, maxSpeed);
        }
    }

    public Rect ScrollArea
    {
        get
        {
            return mScrollArea;
        }
        set
        {
            mScrollArea = value;

            if (box == null)
                box = gameObject.GetComponent<BoxCollider2D>();

            box.offset = new Vector2(mScrollArea.x + mScrollArea.width * 0.5f, mScrollArea.y + mScrollArea.height * 0.5f);
            box.size = new Vector2(mScrollArea.width, mScrollArea.height);

            this.UpdateOffsetRect();
            this.UpdateSnapData();
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

    public Rect TotalArea
    {
        get
        {
            return mTotalArea;
        }
        set
        {
            mTotalArea = value;

            this.UpdateOffsetRect();
            this.UpdateSnapData();
        }
    }

    public int NumberOfElements
    {
        get { return elementCount; }
    }

    public void BeginAddElements()
    {
        Asserts.Assert(!isAddingElements);
        isAddingElements = true;
        if (null == offsetNode)
        {
            offsetNode = new GameObject("offsetNode").transform;
            offsetNode.parent = transform;
            offsetNode.localPosition = Vector3.zero;
            offsetNode.localRotation = Quaternion.identity;
            offsetNode.localScale = Vector3.one;

            elementCount = 0;

            mTotalArea = new Rect(float.MaxValue, float.MaxValue, -float.MaxValue, -float.MaxValue);

            scrButtonsList = new UIButton[0];
        }
        else
        {
            offsetNode.localPosition = Vector3.zero;

            elementCount = offsetNode.childCount;

            scrButtonsList = gameObject.GetComponentsInChildren<UIButton>(true);
            scrButtonsInputLayers = new int[scrButtonsList.Length];
            for (int i = 0; i < scrButtonsList.Length; i++)
            {
                //foreach (UIButton btn in scrButtonsList)
                scrButtonsInputLayers[i] = scrButtonsList[i].inputLayer;
                scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];
            }
            this.SetTotalArea();
        }
    }

    public void AddElement(Transform element, Vector3 elementLocalScale)
    {
        if (0 == elementCount)
            mTotalArea = new Rect(float.MaxValue, float.MaxValue, -float.MaxValue, -float.MaxValue);

        Asserts.Assert(isAddingElements && offsetNode != null);
        ++elementCount;

        element.parent = offsetNode;
        element.localScale = elementLocalScale;

        UINineSlice[] nSlices = element.gameObject.GetComponentsInChildren<UINineSlice>(true);
        foreach (var nSlice in nSlices)
            this.ExtendsAreaBounds(nSlice.GetBounds());

        if (textfieldBoundsControl)
        {
            UITextField[] texts = element.gameObject.GetComponentsInChildren<UITextField>(true);
            foreach (var text in texts)
                this.ExtendsAreaBounds(text.GetRendererBounds());
        }

        Renderer[] renderers = element.gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
            this.ExtendsAreaBounds(r.bounds);

        UIButton[] newButtons = element.gameObject.GetComponentsInChildren<UIButton>(true);
        int[] newButtonsInputLayers = new int[newButtons.Length];
        
        if (newButtons.Length > 0)
        {
            for(int i = 0; i< newButtons.Length; i++)
            //foreach (UIButton btn in newButtons)
            {
                newButtonsInputLayers[i] = newButtons[i].inputLayer;
                newButtons[i].inputLayer = startInputLayer + newButtonsInputLayers[i];
            }

            Array.Resize<UIButton>(ref scrButtonsList, scrButtonsList.Length + newButtons.Length);
            Array.Copy(newButtons, 0, scrButtonsList, scrButtonsList.Length - newButtons.Length, newButtons.Length);
            Array.Resize<int>(ref scrButtonsInputLayers, scrButtonsInputLayers.Length + newButtonsInputLayers.Length);
            Array.Copy(newButtonsInputLayers, 0, scrButtonsInputLayers, scrButtonsInputLayers.Length - newButtonsInputLayers.Length, newButtonsInputLayers.Length);
        }
    }

    public void AddElement(Transform element)
    {
        element.parent = offsetNode;
        this.AddElement(element, element.localScale);
    }

    public void EndAddElements()
    {
        if (mTotalArea.width < 0 || mTotalArea.height < 0)
            mTotalArea = mScrollArea;

        this.UpdateOffsetRect();
        this.UpdateSnapData();

        isAddingElements = false;
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        startInputLayer = inputLayer;
        damping = mDamping;

        mActiveSpringK = mSpringK;
        mActiveSpringKVel = 0.0f;
        mActiveSpringBounce = mSpringBounce;
        mActiveSpringBounceVel = 0.0f;
        mActiveSpringRebound = mSpringRebound;
        mActiveSpringReboundVel = 0.0f;

        if (null == offsetNode)
        {
            offsetNode = new GameObject("offsetNode").transform;
            offsetNode.parent = transform;//.parent;
            offsetNode.position = transform.position;
            offsetNode.rotation = transform.rotation;
            offsetNode.localScale = Vector3.one;
        }

        scrButtonsList = gameObject.GetComponentsInChildren<UIButton>(true);
        //foreach (UIButton btn in scrButtonsList)
        for (int i = 0; i < scrButtonsList.Length; i++)
            scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];

        foreach (Transform tr in transform)
        {
            if (tr == offsetNode)
                continue;

            //UIScroller scChild = tr.gameObject.GetComponent<UIScroller>();
            //if (scChild != null)
            //    scChild.InputLayer = inputLayer + 1;

            tr.parent = offsetNode;
        }

        offsetNode.localPosition = Vector3.zero;

        this.SetTotalArea();

        elementCount = offsetNode.transform.childCount;

        box = gameObject.GetComponent<BoxCollider2D>();
        box.offset = new Vector2(mScrollArea.x + mScrollArea.width * 0.5f, mScrollArea.y + mScrollArea.height * 0.5f);
        box.size = new Vector2(mScrollArea.width, mScrollArea.height);
    }
    #endregion

    #region Signals
    void OnEnable()
    {
        offsetNode.gameObject.SetActive(true);

        if (offsetNode.childCount != elementCount)
        {
            elementCount = offsetNode.childCount;

            scrButtonsList = gameObject.GetComponentsInChildren<UIButton>(true);

            //foreach (UIButton btn in scrButtonsList)
            for (int i = 0; i < scrButtonsList.Length; i++)
                scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];

            offsetNode.localPosition = Vector3.zero;

            this.SetTotalArea();
        }

        Manager<UIManager>.Get().AddScroller(this);
    }

    void OnDisable()
    {
        if (offsetNode != null)
        {
            if (offsetNode.childCount != elementCount)
            {
                elementCount = offsetNode.childCount;

                scrButtonsList = gameObject.GetComponentsInChildren<UIButton>(true);
                scrButtonsInputLayers = new int[scrButtonsList.Length];
                //foreach (UIButton btn in scrButtonsList)
                for (int i = 0; i < scrButtonsList.Length; i++)
                {
                    scrButtonsInputLayers[i] = scrButtonsList[i].inputLayer;
                    scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];
                }

                offsetNode.localPosition = Vector3.zero;

                this.SetTotalArea();
            }

            offsetNode.gameObject.SetActive(false);
        }

        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemoveScroller(this);
    }
    #endregion

    #region Protected Methods
    protected void SetTotalArea()
    {
        mTotalArea = new Rect(float.MaxValue, float.MaxValue, -float.MaxValue, -float.MaxValue);

        foreach (Transform tr in offsetNode)
        {
            UINineSlice[] nSlices = tr.GetComponentsInChildren<UINineSlice>(true);
            foreach (var nSlice in nSlices)
                this.ExtendsAreaBounds(nSlice.GetBounds());

            if (textfieldBoundsControl)
            {
                UITextField[] texts = tr.GetComponentsInChildren<UITextField>(true);
                foreach (var text in texts)
                    this.ExtendsAreaBounds(text.GetRendererBounds());
            }

            Renderer[] renderers = tr.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
                this.ExtendsAreaBounds(r.bounds);
        }

        if (mTotalArea.width < 0 || mTotalArea.height < 0)
            mTotalArea = mScrollArea;

        this.UpdateOffsetRect();
    }

    protected void ExtendsAreaBounds(Bounds bounds)
    {/*
        mTotalArea.xMin = Mathf.Min(bounds.min.x - transform.position.x, mTotalArea.xMin);
        mTotalArea.yMin = Mathf.Min(bounds.min.y - transform.position.y, mTotalArea.yMin);
        mTotalArea.xMax = Mathf.Max(bounds.max.x - transform.position.x, mTotalArea.xMax);
        mTotalArea.yMax = Mathf.Max(bounds.max.y - transform.position.y, mTotalArea.yMax);*/
        Vector3[] v = new Vector3[] {
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)
        };

        Vector3 minBox = Vector3.one *  float.MaxValue,
                maxBox = Vector3.one * -float.MaxValue;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 vertex = transform.InverseTransformPoint(v[i]);
            minBox.x = Mathf.Min(minBox.x, vertex.x);
            minBox.y = Mathf.Min(minBox.y, vertex.y);
            maxBox.x = Mathf.Max(maxBox.x, vertex.x);
            maxBox.y = Mathf.Max(maxBox.y, vertex.y);
        }

        mTotalArea.xMin = Mathf.Min(mTotalArea.xMin, minBox.x);
        mTotalArea.yMin = Mathf.Min(mTotalArea.yMin, minBox.y);
        mTotalArea.xMax = Mathf.Max(mTotalArea.xMax, maxBox.x);
        mTotalArea.yMax = Mathf.Max(mTotalArea.yMax, maxBox.y);
    }

    protected void setSnapX(int value)
    {
        //int span = mSnapXInterval2.End - mSnapXInterval2.Start + 1,
        //    index = span > 0 ? (value - mIndexOffsetX + span) % span : value;

        //if (mSnapXInterval2.HasStart)
        //    index = mSnapXInterval2.Start + index;
        //else if (mSnapXInterval2.HasEnd)
        //    index = mSnapXInterval2.End + index;

        mSnapIndices.First = Mathf.Clamp(value, snapStartXInterval, snapEndXInterval);  //  mSnapXInterval.Clamp(value);
        mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
    }

    protected void setSnapY(int value)
    {
        //int span = mSnapYInterval2.End - mSnapYInterval2.Start + 1,
        //    index = span > 0 ? (value - mIndexOffsetY + span) % span : value;

        //if (mSnapYInterval2.HasStart)
        //    index = mSnapXInterval2.Start + index;
        //else if (mSnapYInterval2.HasEnd)
        //    index = mSnapYInterval2.End + index;

        mSnapIndices.Second = Mathf.Clamp(value, snapStartYInterval, snapEndYInterval); // mSnapYInterval.Clamp(value);
        mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
    }

    protected void UpdateOffsetRect()
    {
        //scrollX = Settings.LockOnX != (mSettings & Settings.LockOnX);
        //scrollY = Settings.LockOnY != (mSettings & Settings.LockOnY);

        scrollX = xDragSets != DragActions.Locked;
        scrollY = yDragSets != DragActions.Locked;

        mOffsetRect.xMin = scrollX ? (mScrollArea.xMax - mTotalArea.xMax) : 0.0f;
        mOffsetRect.yMin = scrollY ? (mScrollArea.yMax - mTotalArea.yMax) : 0.0f;
        mOffsetRect.xMax = scrollX ? (mScrollArea.xMin - mTotalArea.xMin) : 0.0f;
        mOffsetRect.yMax = scrollY ? (mScrollArea.yMin - mTotalArea.yMin) : 0.0f;

        if (mOffsetRect.xMax < mOffsetRect.xMin)
            mOffsetRect.xMin = mOffsetRect.xMax;

        if (mOffsetRect.yMin > mOffsetRect.yMax)
            mOffsetRect.yMax = mOffsetRect.yMin;

        float minWidth = Mathf.Min(mTotalArea.width, mScrollArea.width),
              minHeight = Mathf.Min(mTotalArea.height, mScrollArea.height);

        mOffsetRectExtra.xMin = scrollX ? mOffsetRect.xMin - minWidth : 0.0f;
        mOffsetRectExtra.yMin = scrollY ? mOffsetRect.yMin - minHeight : 0.0f;
        mOffsetRectExtra.xMax = scrollX ? mOffsetRect.xMax + minWidth : 0.0f;
        mOffsetRectExtra.yMax = scrollY ? mOffsetRect.yMax + minHeight : 0.0f;

        if (xDragSets != DragActions.Locked)
            offset.x = Mathf.Clamp(offset.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
        if (yDragSets != DragActions.Locked)
            offset.y = Mathf.Clamp(offset.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);

        //if (0 == (mSettings & Settings.LockOnX) || Settings.LockOnX == (mSettings & Settings.LockOnX))
        //    offset.x = Mathf.Clamp(offset.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
        //if (0 == (mSettings & Settings.LockOnY) || Settings.LockOnY == (mSettings & Settings.LockOnY))
        //    offset.y = Mathf.Clamp(offset.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);
    }

    protected void UpdateSnapData()
    {
        SBSTuple<int, int> prevSnapIndices = mSnapIndices;

        if (!mIsEasingOnX && xDragSets == DragActions.Snap && mSnapSpace.x > 0.0f) // Settings.SnapOnX == (mSettings & Settings.SnapOnX)&& 
        {
            float snapOffset = offset.x / mSnapSpace.x;

            int snapIdx0 = Mathf.FloorToInt(snapOffset),
                snapIdx1 = snapIdx0 + 1;
            snapOffset -= (float)snapIdx0;

            if (snapOffset <= 0.5f)
                mSnapIndices.First = Mathf.Clamp(snapIdx0, snapStartXInterval, snapEndXInterval); // mSnapXInterval.Clamp(snapIdx0);
            else
                mSnapIndices.First = Mathf.Clamp(snapIdx1, snapStartXInterval, snapEndXInterval); // mSnapXInterval.Clamp(snapIdx1);

            mSnapTargets.x = mSnapIndices.First * mSnapSpace.x;
        }

        if (!mIsEasingOnY && yDragSets == DragActions.Snap && mSnapSpace.y > 0.0f) //Settings.SnapOnY == (mSettings & Settings.SnapOnY) 
        {
            float snapOffset = offset.y / mSnapSpace.y;

            int snapIdx0 = Mathf.FloorToInt(snapOffset),
                snapIdx1 = snapIdx0 + 1;
            snapOffset -= (float)snapIdx0;

            if (snapOffset <= 0.5f)
                mSnapIndices.Second = Mathf.Clamp(snapIdx0, snapStartYInterval, snapEndYInterval); //mSnapYInterval.Clamp(snapIdx0);
            else
                mSnapIndices.Second = Mathf.Clamp(snapIdx1, snapStartYInterval, snapEndYInterval); // mSnapYInterval.Clamp(snapIdx1);

            mSnapTargets.y = mSnapIndices.Second * mSnapSpace.y;
        }

        if (prevSnapIndices.First != mSnapIndices.First)
        {
            prevSnapIndices.First = mSnapIndices.First;
            onSnapDataChangedEvent.Invoke(this);
        }

        if (prevSnapIndices.Second != mSnapIndices.Second)
        {
            prevSnapIndices.Second = mSnapIndices.Second;
            onSnapDataChangedEvent.Invoke(this);
        }
    }
    #endregion

    #region Public methods
    //public Vector2 GetScrollerX()
    //{
    //    //Local Coords
    //    return transform.TransformPoint(new Vector2(Mathf.Lerp(mScrollArea.xMax, mScrollArea.xMin, Mathf.InverseLerp(mOffsetRect.xMin, mOffsetRect.xMax, offset.x)), mScrollArea.yMin));
    //}

    //public Vector2 GetScrollerY()
    //{
    //    return transform.TransformPoint(new Vector2(mScrollArea.xMax, Mathf.Lerp(mScrollArea.yMin, mScrollArea.yMax, Mathf.InverseLerp(mOffsetRect.yMin, mOffsetRect.yMax, offset.y))));
    //}

    public Vector2 GetScrollerX()
    {
        float ratio = (mTotalArea.width - mScrollArea.width) / mTotalArea.width;
        return transform.TransformPoint(new Vector2(Mathf.Lerp(mScrollArea.xMin + (mScrollArea.xMax - mScrollArea.xMin) * ratio, mScrollArea.xMin, Mathf.InverseLerp(mOffsetRect.xMin, mOffsetRect.xMax, offset.x)), mScrollArea.yMin));
    }

    public Vector2 GetScrollerY()
    {
        float ratio = (mTotalArea.height - mScrollArea.height) / mTotalArea.height;
        return transform.TransformPoint(new Vector2(mScrollArea.xMax, Mathf.Lerp(mScrollArea.yMin + (mScrollArea.yMax - mScrollArea.yMin) * ratio, mScrollArea.yMin, Mathf.InverseLerp(mOffsetRect.yMin, mOffsetRect.yMax, offset.y))));
    }

    public float GetOffsetXFromPosition(Vector3 worldPos)
    {
        float ratio = (mTotalArea.width - mScrollArea.width) / mTotalArea.width;
        Vector3 locPos = transform.InverseTransformPoint(worldPos);
        return Mathf.Lerp(mOffsetRect.xMin, mOffsetRect.xMax, Mathf.InverseLerp(mScrollArea.xMin + (mScrollArea.xMax - mScrollArea.xMin) * ratio, mScrollArea.xMin, locPos.x));
    }

    public float GetOffsetYFromPosition(Vector3 worldPos)
    {
        float ratio = (mTotalArea.height - mScrollArea.height) / mTotalArea.height;
        Vector3 locPos = transform.InverseTransformPoint(worldPos);
        return Mathf.Lerp(mOffsetRect.yMin, mOffsetRect.yMax, Mathf.InverseLerp(mScrollArea.yMin + (mScrollArea.yMax - mScrollArea.yMin) * ratio, mScrollArea.yMin, locPos.y));
    }

    public void easeToSnapX(int value)
    {
        //if (Settings.SnapOnX == (mSettings & Settings.SnapOnX))
        if (xDragSets == DragActions.Snap)
        {
            this.setSnapX(value);

            mIsEasingOnX = true;
        }
    }

    public void easeToSnapY(int value)
    {
        //if (Settings.SnapOnY == (mSettings & Settings.SnapOnY))
        if (yDragSets == DragActions.Snap)
        {
            this.setSnapY(value);

            mIsEasingOnY = true;
        }
    }

    public void UpdateScroller(float dt, bool isMovingUnderMouse /*= false*/)//, bool forceTransformUpdates = false)
    {/*
        Debug.DrawLine(transform.TransformPoint(new Vector3(mTotalArea.min.x, mTotalArea.min.y, 0.0f)),
                       transform.TransformPoint(new Vector3(mTotalArea.max.x, mTotalArea.min.y, 0.0f)));
        Debug.DrawLine(transform.TransformPoint(new Vector3(mTotalArea.max.x, mTotalArea.min.y, 0.0f)),
                       transform.TransformPoint(new Vector3(mTotalArea.max.x, mTotalArea.max.y, 0.0f)));
        Debug.DrawLine(transform.TransformPoint(new Vector3(mTotalArea.max.x, mTotalArea.max.y, 0.0f)),
                       transform.TransformPoint(new Vector3(mTotalArea.min.x, mTotalArea.max.y, 0.0f)));
        Debug.DrawLine(transform.TransformPoint(new Vector3(mTotalArea.min.x, mTotalArea.max.y, 0.0f)),
                       transform.TransformPoint(new Vector3(mTotalArea.min.x, mTotalArea.min.y, 0.0f)));
*/
        if (null == transform || null == offsetNode) // on exit
            return;

        dt = Mathf.Min(dt, fixedDt * 2.0f);

        if (transform.childCount > 1)
        {
            offsetNode.position = transform.position;

            UIButton[] newButtons;
            foreach (Transform tr in transform)
            {
                if (tr == offsetNode)
                    continue;

                UINineSlice[] nSlices = tr.GetComponentsInChildren<UINineSlice>(true);
                foreach (var nSlice in nSlices)
                    this.ExtendsAreaBounds(nSlice.GetBounds());

                if (textfieldBoundsControl)
                {
                    UITextField[] texts = tr.GetComponentsInChildren<UITextField>(true);
                    foreach (var text in texts)
                        this.ExtendsAreaBounds(text.GetRendererBounds());
                }

                Renderer[] renderers = tr.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                    this.ExtendsAreaBounds(r.bounds);

                tr.parent = offsetNode;

                newButtons = tr.gameObject.GetComponentsInChildren<UIButton>(true);
                int[] newButtonsInputLayers = new int[newButtons.Length];
                for (int i = 0; i < newButtons.Length; i++)
                    newButtonsInputLayers[i] = newButtons[i].inputLayer;

                if (newButtons.Length > 0)
                {
                    Array.Resize<UIButton>(ref scrButtonsList, scrButtonsList.Length + newButtons.Length);
                    Array.Copy(newButtons, 0, scrButtonsList, scrButtonsList.Length - newButtons.Length, newButtons.Length);
                    Array.Resize<int>(ref scrButtonsInputLayers, scrButtonsInputLayers.Length + newButtonsInputLayers.Length);
                    Array.Copy(newButtonsInputLayers, 0, scrButtonsInputLayers, scrButtonsInputLayers.Length - newButtonsInputLayers.Length, newButtonsInputLayers.Length);
                }
            }

            elementCount = offsetNode.childCount;

            this.UpdateOffsetRect();
        }

        if (offsetNode.childCount != elementCount)
        {
            elementCount = offsetNode.childCount;

            scrButtonsList = gameObject.GetComponentsInChildren<UIButton>(true);
            scrButtonsInputLayers = new int[scrButtonsList.Length];

            for (int i = 0; i < scrButtonsList.Length; i++)
            {
                scrButtonsInputLayers[i] = scrButtonsList[i].inputLayer;
                scrButtonsList[i].inputLayer = startInputLayer + scrButtonsInputLayers[i];
            }

            offsetNode.localPosition = Vector3.zero;

            this.SetTotalArea();
        }

        //scrollX = Settings.LockOnX != (mSettings & Settings.LockOnX);
        //scrollY = Settings.LockOnY != (mSettings & Settings.LockOnY);
        scrollX = xDragSets != DragActions.Locked;
        scrollY = yDragSets != DragActions.Locked;

        mOffsetRect.xMin = scrollX ? (mScrollArea.xMax - mTotalArea.xMax) : 0.0f;
        mOffsetRect.yMin = scrollY ? (mScrollArea.yMax - mTotalArea.yMax) : 0.0f;
        mOffsetRect.xMax = scrollX ? (mScrollArea.xMin - mTotalArea.xMin) : 0.0f;
        mOffsetRect.yMax = scrollY ? (mScrollArea.yMin - mTotalArea.yMin) : 0.0f;

        if (mOffsetRect.xMax < mOffsetRect.xMin)
            mOffsetRect.xMin = mOffsetRect.xMax;

        if (mOffsetRect.yMin > mOffsetRect.yMax)
            mOffsetRect.yMax = mOffsetRect.yMin;

        float minWidth = Mathf.Min(mTotalArea.width, mScrollArea.width),
              minHeight = Mathf.Min(mTotalArea.height, mScrollArea.height);
        mOffsetRectExtra.xMin = scrollX ? mOffsetRect.xMin - minWidth : 0.0f;
        mOffsetRectExtra.yMin = scrollY ? mOffsetRect.yMin - minHeight : 0.0f;
        mOffsetRectExtra.xMax = scrollX ? mOffsetRect.xMax + minWidth : 0.0f;
        mOffsetRectExtra.yMax = scrollY ? mOffsetRect.yMax + minHeight : 0.0f;

        offset += offsetVelocity * dt;
        offsetVelocity *= Mathf.Pow(1.0f - damping, dt);
        if (offsetVelocity.sqrMagnitude <= 0.16f)//1.0e-2f)
            offsetVelocity = Vector2.zero;

        offset.x = Mathf.Clamp(offset.x, mOffsetRectExtra.xMin, mOffsetRectExtra.xMax);
        offset.y = Mathf.Clamp(offset.y, mOffsetRectExtra.yMin, mOffsetRectExtra.yMax);

        //Vector2 pixelOffset = transform.TransformPoint(offset) * UIManager.Instance.pixelsPerUnit;
        //pixelOffset.x = (Mathf.Floor(pixelOffset.x) + 0.5f) / UIManager.Instance.pixelsPerUnit;
        //pixelOffset.y = (Mathf.Floor(pixelOffset.y) + 0.5f) / UIManager.Instance.pixelsPerUnit;

        offsetNode.position = transform.TransformPoint(offset);// pixelOffset;

        this.UpdateSnapData();

        if (0 == elementCount || dt < SBSMath.Epsilon || isMovingUnderMouse)
            return;

        while (dt > 0.0f)
        {
            float stepDt = Mathf.Min(fixedDt, dt);

            mActiveSpringK = Mathf.SmoothDamp(mActiveSpringK, mSpringK, ref mActiveSpringKVel, dampTime, mSpringK / dampTime, stepDt);
            mActiveSpringBounce = Mathf.SmoothDamp(mActiveSpringBounce, mSpringBounce, ref mActiveSpringBounceVel, dampTime, mSpringBounce / dampTime, stepDt);
            mActiveSpringRebound = Mathf.SmoothDamp(mActiveSpringRebound, mSpringRebound, ref mActiveSpringReboundVel, dampTime, mSpringRebound / dampTime, stepDt);

            Vector2 springForce = Vector2.zero;

            //if (Settings.SnapOnX == (mSettings & Settings.SnapOnX))
            if (xDragSets == DragActions.Snap)
            {
                float dx = mSnapTargets.x - offset.x;
                if (Mathf.Abs(dx) <= 1.0e-2f)
                {
                    springForce.x = 0.0f;
                    offset.x = mSnapTargets.x;
                    mIsEasingOnX = false;
                }
                else
                {
                    springForce.x += dx * mActiveSpringK;
                    if ((dx * offsetVelocity.x) >= 0.0f)
                        springForce.x -= mActiveSpringRebound * offsetVelocity.x;
                    else
                        springForce.x -= mActiveSpringBounce * offsetVelocity.x;
                }
            }
            else if (offset.x < mOffsetRect.xMin)
            {
                springForce.x += (mOffsetRect.xMin - offset.x) * mSpringK;
                if (offsetVelocity.x >= 0.0f)
                    springForce.x -= mActiveSpringRebound * offsetVelocity.x;
                else
                    springForce.x -= mActiveSpringBounce * offsetVelocity.x;
            }
            else if (offset.x > mOffsetRect.xMax)
            {
                springForce.x -= (offset.x - mOffsetRect.xMax) * mSpringK;
                if (offsetVelocity.x <= 0.0f)
                    springForce.x -= mActiveSpringRebound * offsetVelocity.x;
                else
                    springForce.x -= mActiveSpringBounce * offsetVelocity.x;
            }
            else if (Mathf.Abs(prevSpringForce.x) > 0.0f)
            {
                springForce.x = 0.0f;
                offset.x = Mathf.Clamp(offset.x, mOffsetRect.xMin, mOffsetRect.xMax);
            }

            //if (Settings.SnapOnY == (mSettings & Settings.SnapOnY))
            if (yDragSets == DragActions.Snap)
            {
                float dy = mSnapTargets.y - offset.y;
                if (Mathf.Abs(dy) <= 1.0e-2f)
                {
                    springForce.y = 0.0f;
                    offset.y = mSnapTargets.y;
                    mIsEasingOnY = false;
                }
                else
                {
                    springForce.y += dy * mActiveSpringK;
                    if ((dy * offsetVelocity.y) >= 0.0f)
                        springForce.y -= mActiveSpringRebound * offsetVelocity.y;
                    else
                        springForce.y -= mActiveSpringBounce * offsetVelocity.y;
                }
            }
            else if (offset.y < mOffsetRect.yMin)
            {
                springForce.y += (mOffsetRect.yMin - offset.y) * mSpringK;
                if (offsetVelocity.y >= 0.0f)
                    springForce.y -= mActiveSpringRebound * offsetVelocity.y;
                else
                    springForce.y -= mActiveSpringBounce * offsetVelocity.y;
            }
            else if (offset.y > mOffsetRect.yMax)
            {
                springForce.y -= (offset.y - mOffsetRect.yMax) * mSpringK;
                if (offsetVelocity.y <= 0.0f)
                    springForce.y -= mActiveSpringRebound * offsetVelocity.y;
                else
                    springForce.y -= mActiveSpringBounce * offsetVelocity.y;
            }
            else if (Mathf.Abs(prevSpringForce.y) > 0.0f)
            {
                springForce.y = 0.0f;
                offset.y = Mathf.Clamp(offset.y, mOffsetRect.yMin, mOffsetRect.yMax);
            }

            offsetVelocity += springForce * stepDt;
            offsetVelocity.x = Mathf.Clamp(offsetVelocity.x, -maxSpeed, maxSpeed);
            offsetVelocity.y = Mathf.Clamp(offsetVelocity.y, -maxSpeed, maxSpeed);

            prevSpringForce = springForce;
            dt -= fixedDt;
        }
    }
    #endregion
}
