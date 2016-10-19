using UnityEngine;
using System.Collections;

public class UIScrollerController : MonoBehaviour 
{
    protected UIScroller scr;
    protected bool isMovingUnderMouse = false;
    void Awake()
    {
        scr = gameObject.GetComponent<UIScroller>();
    }

#if MOBILE
    int lastTouchId;
#endif

    void Update()
    {
        //Debug.Log(scr.Offset);
#if MOBILE
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch t = Input.GetTouch(i);
			if (t.fingerId < 0 || t.fingerId >= 4)
				continue;
			
            switch (t.phase)
            {
                case TouchPhase.Began:
                case TouchPhase.Stationary:  
                case TouchPhase.Moved:
                    if (-1 == lastTouchId)
                    {
                        lastTouchId = t.fingerId;
                        isMovingUnderMouse = true;
                    }
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    if (t.fingerId == lastTouchId)
                    {
                        isMovingUnderMouse = true;
                        lastTouchId = -1;
                    }
                    break;
            }
        }
#else
        if (Input.GetMouseButton(0))
            isMovingUnderMouse = true;

        if (Input.GetMouseButtonUp(0))
            isMovingUnderMouse = false;
#endif
        float dt = UIManager.Instance.UITimeSource.DeltaTime;
        if (!isMovingUnderMouse && (scr.OffsetVelocity.y * scr.OffsetVelocity.y)< dt)
            scr.Offset = new Vector2(0.0f, scr.Offset.y + dt * 0.5f);

        if (!isMovingUnderMouse && (scr.OffsetVelocity.y * scr.OffsetVelocity.y) < dt && scr.Offset.y >= scr.OffsetRect.max.y)
        scr.Offset = new Vector2(0.0f, scr.OffsetRect.min.y);

    }
}