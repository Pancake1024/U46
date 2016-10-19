//using UnityEngine;
//using SBS.Core;

//[RequireComponent(typeof(UINineSlice))]
//[AddComponentMenu("UI/UIScrollBar")]
//public class UIScrollBar : MonoBehaviour
//{
//    public UIScroller scroller;

//    public int inputLayer;
//    public float minWidth;
//    public float minHeight;

//    protected UINineSlice barNineSlice;

//    float maxWidth;
//    float maxHeight;
    
//    Vector2 scrOffset;
//    Rect scrScrollArea;

//    void Awake()
//    {
//        barNineSlice = gameObject.GetComponent<UINineSlice>();
//    }

//    void Start()
//    {
//        inputLayer = scroller.inputLayer + 2;

//        maxWidth = scroller.ScrollArea.width;
//        maxHeight = scroller.ScrollArea.height;

//        scrOffset = scroller.Offset;
//        scrScrollArea = scroller.ScrollArea;

//        SetBarPosition();
//    }

//    void Update()
//    {
//        if (scrScrollArea != scroller.ScrollArea)
//            this.Start();
        
//        if (scrOffset != scroller.Offset)
//        {
//            SetBarPosition();
//            scrOffset = scroller.Offset;
//        }
//    }

//    void SetBarPosition()
//    {
//        if (scroller.xDragSets != UIScroller.DragActions.Locked)
//        {
//            barNineSlice.width = Mathf.Clamp(scrScrollArea.width * scrScrollArea.width / scroller.TotalArea.width, minWidth, maxWidth);

//            Vector2 scrollerNodePos = scroller.GetScrollerX();
//            transform.position = (Vector3)scrollerNodePos;
//        }

//        if (scroller.yDragSets != UIScroller.DragActions.Locked)
//        {
//            barNineSlice.height = Mathf.Clamp(scrScrollArea.height * scrScrollArea.height / scroller.TotalArea.height, minHeight, maxHeight);

//            Vector2 scrollerNodePos = scroller.GetScrollerY();
//            transform.position = (Vector3)scrollerNodePos;
//        }
//    }
//}

using UnityEngine;
using SBS.Core;

[RequireComponent(typeof(UINineSlice))]
[AddComponentMenu("UI/UIScrollBar")]
public class UIScrollBar : MonoBehaviour
{
    public UIScroller scroller;

    public int inputLayer;
    public float minWidth;
    public float minHeight;
    public bool draggable;
    public UINineSlice bg;

    protected Camera uiCamera;
    protected UINineSlice barNineSlice;
    protected UIButton barButton;
    protected BoxCollider2D collider;
    protected bool isDragging = false;
    protected Vector3 lastInputPosition;
    protected Vector3 inputOffset;

    float maxWidth;
    float maxHeight;

    Vector2 scrOffset;
    Rect scrScrollArea;

    void Awake()
    {
        barNineSlice = gameObject.GetComponent<UINineSlice>();
    }

    void Start()
    {
        uiCamera = Manager<UIManager>.Get().UICamera;

        inputLayer = scroller.inputLayer + 2;

        maxWidth = scroller.ScrollArea.width;
        maxHeight = scroller.ScrollArea.height;

        scrOffset = scroller.Offset;
        scrScrollArea = scroller.ScrollArea;

        if (draggable)
        {
            barButton = gameObject.AddComponent<UIButton>();
            barButton.onReleaseEvent.AddTarget(gameObject, "Signal_OnScrollbarRelease");
            barButton.onReleaseOutsideEvent.AddTarget(gameObject, "Signal_OnScrollbarRelease");
            barButton.onPressedEvent.AddTarget(gameObject, "Signal_OnScrollbarPressed");
            barButton.inputLayer = inputLayer;
            collider = gameObject.GetComponent<BoxCollider2D>();
        }

        SetBar();

        if (bg != null)
        {
            bg.sortingOrder = barNineSlice.sortingOrder;
            barNineSlice.sortingOrder += 1;
            barNineSlice.ApplyParameters();
            
            if (scroller.xDragSets != UIScroller.DragActions.Locked)
            {
                bg.width = Mathf.Abs(scrScrollArea.xMin - scrScrollArea.xMax);
                bg.pivot = new Vector2(bg.width * 0.5f, bg.height * 0.5f);
                bg.ApplyParameters();
                bg.transform.position = new Vector3(scroller.transform.TransformPoint(scrScrollArea.center).x, barNineSlice.transform.position.y);
            }

            if (scroller.yDragSets != UIScroller.DragActions.Locked)
            {
                bg.height = Mathf.Abs(scrScrollArea.yMin - scrScrollArea.yMax);
                bg.pivot = new Vector2(bg.width * 0.5f, bg.height * 0.5f);
                bg.ApplyParameters();
                bg.transform.position = new Vector3(barNineSlice.transform.position.x, scroller.transform.TransformPoint(scrScrollArea.center).y);
            }
        }
    }

    void Update()
    {
        if (scrScrollArea != scroller.ScrollArea)
            this.Start();

        if (scrOffset != scroller.Offset)
        {
            SetBar();
            scrOffset = scroller.Offset;
        }

        UpdateDragging();
    }

    void UpdateDragging()
    {
        if (isDragging)
        {
            Vector3 inputPosition = uiCamera.ScreenToWorldPoint(Input.mousePosition);
            if (lastInputPosition != inputPosition)
            {
                lastInputPosition = inputPosition;

                //X
                if (scroller.xDragSets != UIScroller.DragActions.Locked)
                {
                    float newX = lastInputPosition.x - inputOffset.x;
                    transform.position = new Vector3(transform.position.x, newX);

                    float scrOffsetX = scroller.GetOffsetXFromPosition(transform.position - barNineSlice.width * 0.5f * Vector3.right);
                    scroller.Offset = new Vector2(scroller.Offset.x, scrOffsetX);

                    Vector2 scrollerNodePos = scroller.GetScrollerX();
                    transform.position = new Vector3(scrollerNodePos.x + barNineSlice.width * 0.5f, scrollerNodePos.y);
                }

                //Y
                if (scroller.yDragSets != UIScroller.DragActions.Locked)
                {
                    float newY = lastInputPosition.y - inputOffset.y;
                    transform.position = new Vector3(transform.position.x, newY);

                    float scrOffsetY = scroller.GetOffsetYFromPosition(transform.position - barNineSlice.height * 0.5f * Vector3.up);
                    scroller.Offset = new Vector2(scroller.Offset.x, scrOffsetY);

                    Vector2 scrollerNodePos = scroller.GetScrollerY();
                    transform.position = new Vector3(scrollerNodePos.x, scrollerNodePos.y + barNineSlice.height * 0.5f);
                }
            }
        }
    }

    void SetBar()
    {
        //X
        if (scroller.xDragSets != UIScroller.DragActions.Locked)
        {
            if (scroller.ScrollArea.width < scroller.TotalArea.width)
            {
                float bgX = scroller.transform.TransformPoint(scrScrollArea.center).x;
                float sx = scroller.transform.lossyScale.x;
                barNineSlice.width = Mathf.Clamp(scrScrollArea.width * scrScrollArea.width / scroller.TotalArea.width, minWidth, maxWidth);
                Vector2 p = barNineSlice.pivot;
                p.x = barNineSlice.width * 0.5f;
                barNineSlice.pivot = p;

                Vector2 scrollerNodePos = scroller.GetScrollerX();
                if (scroller.ScrollArea.width < scroller.TotalArea.width)
                    transform.position = new Vector3(scrollerNodePos.x + barNineSlice.width * 0.5f * sx, scrollerNodePos.y);
                else
                    transform.position = new Vector3(bgX, scrollerNodePos.y);

                if (bg != null && bg.transform.localPosition.y != barNineSlice.transform.localPosition.y)
                {
                    bg.height = barNineSlice.height;
                    bg.transform.localPosition = new Vector3(bgX, barNineSlice.transform.localPosition.y, 0.0f);
                }
            }
        }

        //Y
        if (scroller.yDragSets != UIScroller.DragActions.Locked)
        {
            float bgY = scroller.transform.TransformPoint(scrScrollArea.center).y;
            float sy = scroller.transform.lossyScale.y;
            barNineSlice.height = Mathf.Clamp(scrScrollArea.height * scrScrollArea.height / scroller.TotalArea.height, minHeight, maxHeight);
            Vector2 p = barNineSlice.pivot;
            p.y = barNineSlice.height * 0.5f;
            barNineSlice.pivot = p;
                
            Vector2 scrollerNodePos = scroller.GetScrollerY();
            if (scroller.ScrollArea.height < scroller.TotalArea.height)
                transform.position = new Vector3(scrollerNodePos.x, scrollerNodePos.y + barNineSlice.height * 0.5f * sy);
            else
                transform.position = new Vector3(scrollerNodePos.x, bgY);

            if (bg != null && bg.transform.position.x != barNineSlice.transform.position.x)
            {
                bg.width = barNineSlice.width;
                bg.transform.position = new Vector3(barNineSlice.transform.position.x, bgY);
                bg.ApplyParameters();
            }
            
        }

        if (draggable)
        {
            Vector2 p = barNineSlice.pivot;
            collider.offset = new Vector2(p.x - barNineSlice.width * 0.5f, p.y - barNineSlice.height * 0.5f);
            collider.size = new Vector2(barNineSlice.width, barNineSlice.height);
        }
    }

    //void SetBarBG()
    //{
    //    //X
    //    if (scroller.xDragSets != UIScroller.DragActions.Locked)
    //    {
    //        barNineSlice.width = Mathf.Clamp(scrScrollArea.width * scrScrollArea.width / scroller.TotalArea.width, minWidth, maxWidth);
    //        Vector2 p = barNineSlice.pivot;
    //        p.x = barNineSlice.width * 0.5f;
    //        barNineSlice.pivot = p;

    //        Vector2 scrollerNodePos = scroller.GetScrollerX();
    //        transform.position = new Vector3(scrollerNodePos.x + barNineSlice.width * 0.5f, scrollerNodePos.y);
    //    }

    //    //Y
    //    if (scroller.yDragSets != UIScroller.DragActions.Locked)
    //    {
    //        barNineSlice.height = Mathf.Clamp(scrScrollArea.height, minHeight, maxHeight);
    //        Vector2 p = bg.pivot;
    //        p.y = bg.height * 0.5f;
    //        bg.pivot = p;

    //        Vector2 scrollerNodePos = scroller.GetScrollerY();
    //        bg.transform.position = new Vector3(scrollerNodePos.x, 0.0f, 0.0f);
    //    }

    //    if (draggable)
    //    {
    //        Vector2 p = barNineSlice.pivot;
    //        collider.center = new Vector2(p.x - barNineSlice.width * 0.5f, p.y - barNineSlice.height * 0.5f);
    //        collider.size = new Vector2(barNineSlice.width, barNineSlice.height);
    //    }
    //}

    void Signal_OnScrollbarPressed(UIButton button)
    {
        if (!draggable)
            return;

        isDragging = true;

        lastInputPosition = uiCamera.ScreenToWorldPoint(Input.mousePosition);
        inputOffset = lastInputPosition - barNineSlice.transform.position;
    }

    void Signal_OnScrollbarRelease(UIButton button)
    {
        if (!draggable)
            return;

        isDragging = false;
    }
}