using UnityEngine;
using System.Collections;
using SBS.Core;

[AddComponentMenu("UI/UIPage")]
public class UIPage : IntrusiveListNode<UIPage>
{
    public Signal onEnter = Signal.Create<UIPage>();
    public Signal onExit = Signal.Create<UIPage>();
    public bool showOnAwake = false;

    protected bool awakeGuard = false;

    void Awake()
    {
        awakeGuard = true;

        Manager<UIManager>.Get().AddPage(this);

        gameObject.SetActive(false);

        if (showOnAwake)
            Manager<UIManager>.Get().GoToPage(this);

        awakeGuard = false;
    }

    void OnEnable()
    {
        if (awakeGuard)
            return;

        onEnter.Invoke(this);
    }

    void OnDisable()
    {
        if (awakeGuard)
            return;

        //onExit.Invoke(this);
    }

    void OnDestroy()
    {
        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemovePage(this);
    }
}
