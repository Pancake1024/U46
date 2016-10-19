using UnityEngine;
using System;
using System.Collections;
using SBS.Core;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("UI/UIButton")]
public class UIButton : IntrusiveListNode<UIButton>
{
    public static bool enablePressSignals = true;
    public static bool enableReleaseSignals = true;

    public enum StateType
    {
        Normal = 0,
        MouseOver,
        Pressed,
        Disabled
    };

    protected StateType state = StateType.Disabled;
    protected int inStateChange = 0;

    public GameObject normal;
    public GameObject onHover;
    public GameObject onPressed;
    public GameObject disabled;

    public RectOffset resizeOffset;
    public Vector2 minSize;

    public int inputLayer = 0;

    public Signal onPressedEvent = Signal.Create<UIButton>();
    public Signal onReleaseEvent = Signal.Create<UIButton>();
    public Signal onReleaseOutsideEvent = Signal.Create<UIButton>();
    public Signal onMouseOverEvent = Signal.Create<UIButton>();
    public Signal onMouseOutEvent = Signal.Create<UIButton>();
    public Signal onEnableEvent = Signal.Create<UIButton>();
    public Signal onDisableEvent = Signal.Create<UIButton>();

    [NonSerialized] public Vector2 mouseOrTouchPosition;
    [NonSerialized] public int touchFingerId = -1;
    [NonSerialized] public bool shouldDisablePressSignal = true;
    [NonSerialized] public bool shouldDisableReleaseSignal = true;

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

            int prevInStateChange = ++inStateChange;

            switch (state)
            {
                case StateType.Normal:
                    if (StateType.MouseOver == value)
                        onMouseOverEvent.Invoke(this);
                    else if (StateType.Pressed == value)
                    {
                        if (enablePressSignals || !shouldDisablePressSignal)
                            onPressedEvent.Invoke(this);
                    }
                    else if (StateType.Disabled == value)
                        onDisableEvent.Invoke(this);
                    break;
                case StateType.MouseOver:
                    if (StateType.Normal == value)
                        onMouseOutEvent.Invoke(this);
                    else if (StateType.Pressed == value)
                    {
                        if (enablePressSignals || !shouldDisablePressSignal)
                            onPressedEvent.Invoke(this);
                    }
                    else if (StateType.Disabled == value)
                        onDisableEvent.Invoke(this);
                    break;
                case StateType.Pressed:
                    if (StateType.Normal == value)
                    {
                        if (enableReleaseSignals || !shouldDisableReleaseSignal)
                            onReleaseOutsideEvent.Invoke(this);
                    }
                    else if (StateType.MouseOver == value)
                    {
                        if (enableReleaseSignals || !shouldDisableReleaseSignal)
                            onReleaseEvent.Invoke(this);
                    }
                    else if (StateType.Disabled == value)
                        onDisableEvent.Invoke(this);
                    break;
                case StateType.Disabled:
                    onEnableEvent.Invoke(this);
                    break;
            }

            if (inStateChange > prevInStateChange)
            {
                if (1 == --inStateChange)
                    inStateChange = 0;
                return;
            }
            state = value;

            switch (state)
            {
                case StateType.Normal:
                    if (normal != null)
                        normal.SetActive(true);
                    if (onHover != normal)
                        onHover.SetActive(false);
                    if (onPressed != onHover && onPressed != normal)
                        onPressed.SetActive(false);
                    if (disabled != null)
                        disabled.SetActive(false);
                    break;
                case StateType.MouseOver:
                    if (normal != null)
                        normal.SetActive(false);
                    if (onHover != null)
                        onHover.SetActive(true);
                    if (onPressed != onHover && onPressed != normal)
                        onPressed.SetActive(false);
                    if (disabled != null)
                        disabled.SetActive(false);
                    break;
                case StateType.Pressed:
                    if (normal != null)
                        normal.SetActive(false);
                    if (onHover != onPressed)
                        onHover.SetActive(false);
                    if (onPressed != null)
                        onPressed.SetActive(true);
                    if (disabled != null)
                        disabled.SetActive(false);
                    break;
                case StateType.Disabled:
                    if (normal != null)
                        normal.SetActive(false);
                    if (onHover != null)
                        onHover.SetActive(false);
                    if (onPressed != onHover)
                        onPressed.SetActive(false);
                    if (disabled != null)
                        disabled.SetActive(true);
                    break;
            }

            GetComponent<Collider2D>().enabled = state != StateType.Disabled;
        }
    }

    public bool Contains(Vector3 pos)
    {
        return GetComponent<Collider2D>().OverlapPoint(pos);
    }

    void Awake()
    {
        this.State = StateType.Normal;
    }

    void OnEnable()
    {
        if (state != StateType.Disabled)
            this.State = StateType.Normal;

        Manager<UIManager>.Get().AddButton(this);
    }

    void OnDisable()
    {
        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemoveButton(this);
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        BoxCollider2D box = gameObject.GetComponent<BoxCollider2D>();
        Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        Gizmos.DrawCube(transform.TransformPoint(box.offset), box.size);
    }

    void OnDrawGizmosSelected()
    { }
#endif
}
