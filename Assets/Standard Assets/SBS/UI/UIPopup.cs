using UnityEngine;
using System.Collections;
using SBS.Core;

[AddComponentMenu("UI/UIPopup")]
public class UIPopup : IntrusiveListNode<UIPopup>
{
    public Signal onShow = Signal.Create<UIPopup>();
    public Signal onHide = Signal.Create<UIPopup>();
    public Signal onRemoveFromStack = Signal.Create<UIPopup>();
    public bool showOnAwake = false;
    public bool pausesGame = true;

    protected bool awakeGuard = false;

    void Awake()
    {
        awakeGuard = true;

        Manager<UIManager>.Get().AddPopup(this);

        gameObject.SetActive(false);

        if (showOnAwake)
            Manager<UIManager>.Get().PushPopup(this);

        awakeGuard = false;
    }

    void OnEnable()
    {
        if (awakeGuard)
            return;

        onShow.Invoke(this);
    }

    void OnDisable()
    {
        if (awakeGuard)
            return;

        onHide.Invoke(this);
    }

    void OnDestroy()
    {
        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemovePopup(this);
    }
}
