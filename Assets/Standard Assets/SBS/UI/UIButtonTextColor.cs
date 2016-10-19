using UnityEngine;

[AddComponentMenu("UI/UIButtonTextColor")]
[RequireComponent(typeof(UIButton))]
public class UIButtonTextColor : MonoBehaviour
{
    public Color normal = Color.white;
    public Color onHover = Color.white;
    public Color onPressed = Color.white;
    public Color disabled = Color.white;

    protected UITextField[] textFields = null;

    public void RefreshTextFields()
    {
        textFields = gameObject.GetComponentsInChildren<UITextField>();

        this.enabled = (textFields.Length > 0);
    }

    void Awake()
    {
        UIButton button = gameObject.GetComponent<UIButton>();

        button.onPressedEvent.AddTarget(gameObject, "UIButtonTextColor_OnPressed");
        button.onReleaseEvent.AddTarget(gameObject, "UIButtonTextColor_OnRelease");
        button.onReleaseOutsideEvent.AddTarget(gameObject, "UIButtonTextColor_OnReleaseOutside");
        button.onMouseOverEvent.AddTarget(gameObject, "UIButtonTextColor_OnMouseOver");
        button.onMouseOutEvent.AddTarget(gameObject, "UIButtonTextColor_OnMouseOut");
        button.onEnableEvent.AddTarget(gameObject, "UIButtonTextColor_OnEnable");
        button.onDisableEvent.AddTarget(gameObject, "UIButtonTextColor_OnDisable");

        textFields = gameObject.GetComponentsInChildren<UITextField>();

        if (0 == textFields.Length)
            this.enabled = false;
        else
            this.UIButtonTextColor_OnEnable(button);
    }

    void UIButtonTextColor_OnPressed(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = onPressed;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnRelease(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = onHover;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnReleaseOutside(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = normal;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnMouseOver(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = onHover;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnMouseOut(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = normal;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnEnable(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = normal;
            tf.ApplyParameters();
        }
    }

    void UIButtonTextColor_OnDisable(UIButton button)
    {
        foreach (UITextField tf in textFields)
        {
            tf.color = disabled;
            tf.ApplyParameters();
        }
    }
}
