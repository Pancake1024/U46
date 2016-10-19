using UnityEngine;
using System;
using SBS.Core;

public class UIToggleButton : MonoBehaviour
{
    protected UIButton[] buttons;
    protected UIButton lastActive;
    public int currentActiveIdx;

    public UIButton firstActive;
    public Signal onChange = Signal.Create<UIButton>();

    void Awake()
    {
        buttons = gameObject.GetComponentsInChildren<UIButton>();
        lastActive = null;

        if (firstActive != null)
        {
            int index = Array.IndexOf<UIButton>(buttons, firstActive);
            if (index >= 0)
            {
                lastActive = buttons[index];
                currentActiveIdx = index;
            }
        }

        foreach (UIButton btn in buttons)
        {
            btn.onReleaseEvent.AddTarget(gameObject, "UIToggleButton_OnRelease");
        }
    }

    public bool IsActiveButton(UIButton button)
    {
        return button == lastActive;
    }

    void UIToggleButton_OnRelease(UIButton btn)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            return;

#if UNITY_EDITOR
        if (Array.IndexOf<UIButton>(buttons, btn) < 0)
            Debug.LogError("Wrong button");
#endif

        lastActive = btn;
        foreach (UIButton otherBtn in buttons)
        {
            if (otherBtn == btn)
                otherBtn.State = UIButton.StateType.Disabled;
            else
                otherBtn.State = UIButton.StateType.Normal;
        }
        //Debug.Log("Selected " + btn.name);
        onChange.Invoke(btn);
        currentActiveIdx = Array.IndexOf<UIButton>(buttons, btn);
    }

    void OnEnable()
    {
        foreach (UIButton btn in buttons)
        {
            if (btn == lastActive)
                btn.State = UIButton.StateType.Disabled;
            else
                btn.State = UIButton.StateType.Normal;
        }
    }

    public void SetActiveButton(UIButton button)
    {
        if(buttons == null || buttons.Length < 0)
            buttons = gameObject.GetComponentsInChildren<UIButton>();
#if UNITY_EDITOR
            if (Array.IndexOf<UIButton>(buttons, button) < 0)
                Debug.LogError("Wrong button");
#endif
        lastActive = button;

        foreach (UIButton btn in buttons)
        {
            if (btn == lastActive)
                btn.State = UIButton.StateType.Disabled;
            else
                btn.State = UIButton.StateType.Normal;
        }
        onChange.Invoke(lastActive);
    }
}
