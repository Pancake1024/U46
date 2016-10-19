using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UI/UILayoutAndScaling")]
public class UILayoutAndScaling : IntrusiveListNode<UILayoutAndScaling>
{
    [Flags]
    public enum LayoutType
    {
        None = 0,

        Outer = 1 << 0,
        Inner = 1 << 1
    }

    public enum ScalingType
    {
        None = 0,

        KeepPixelSize,
        KeepScreenSize,

        Scale,

        KeepScreenSizeMin
    }

    public LayoutType xLayout;
    public LayoutType yLayout;
    public ScalingType scalingType;

    protected Vector3 initialPivot;
    protected Vector3 initialScale;
    //protected Bounds initialBounds;
    protected bool doReset = false;

    void Start()//Awake()
    {
        //initialPivot = transform.position;
        //initialScale = transform.localScale;
/*
        initialBounds.min = Vector3.one *  float.MaxValue;
        initialBounds.max = Vector3.one * -float.MaxValue;

        Renderer[] renderers = gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (Renderer rndr in renderers)
        {
            initialBounds.Encapsulate(rndr.bounds);
        }*/
    }

    void OnEnable()
    {
        if (doReset)
            this.ResetLayoutAndScaling();

        initialPivot = transform.localPosition;//position;
        initialScale = transform.localScale;

        Manager<UIManager>.Get().AddLayout(this);
    }

    void OnDisable()
    {
        //this.ResetLayoutAndScaling();

        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemoveLayout(this);
    }

    public void ResetLayoutAndScaling()
    {
        if (!doReset)
            return;

        transform.localPosition = initialPivot;
        transform.localScale = initialScale;

        doReset = false;
    }

    public void UpdateLayoutAndScaling(float heightScale, float aspectScale)
    {
        if (doReset)
            this.ResetLayoutAndScaling();

        doReset = true;

        switch (scalingType)
        {
            case ScalingType.None:
                // do nothing
                break;
            case ScalingType.KeepPixelSize:
                initialScale = transform.localScale;
                transform.localScale = new Vector3(heightScale, heightScale, 1.0f);
                break;
            case ScalingType.KeepScreenSize:
                initialScale = transform.localScale;
                float s = aspectScale;// <= 1.0f ? aspectScale : (1.0f / aspectScale);
                transform.localScale = new Vector3(s, s, 1.0f);
                break;
            case ScalingType.Scale:
                initialScale = transform.localScale;
                transform.localScale = new Vector3(aspectScale, 1.0f, 1.0f);
                break;
            case ScalingType.KeepScreenSizeMin:
                initialScale = transform.localScale;
                float s2 = aspectScale <= 1.0f ? aspectScale : 1.0f;
                transform.localScale = new Vector3(s2, s2, 1.0f);
                break;
        }

        initialPivot = transform.localPosition;

        if (LayoutType.None == xLayout && LayoutType.None == yLayout)
        {
            return;
        }

        Camera cam = Manager<UIManager>.Get().UICamera;
        Vector3 camPos = cam.worldToCameraMatrix * transform.position;

        if (xLayout != LayoutType.None)
        {
            if (LayoutType.Inner == xLayout)
                camPos.x *= aspectScale;
            else
            {
                float orthoSize = cam.orthographicSize * cam.aspect;
                if (camPos.x >= 0.0f)
                    camPos.x =  orthoSize - (orthoSize / aspectScale - camPos.x);
                else
                    camPos.x = -orthoSize + (orthoSize / aspectScale + camPos.x);
            }
        }

        if (yLayout != LayoutType.None)
        {
            if (LayoutType.Inner == yLayout)
                camPos.y /= heightScale;
            else
            {
                float orthoSize = cam.orthographicSize / heightScale;
                if (camPos.y >= 0.0f)
                    camPos.y =  orthoSize - (orthoSize * heightScale - camPos.y);
                else
                    camPos.y = -orthoSize + (orthoSize * heightScale + camPos.y);
            }
        }

        transform.position = Manager<UIManager>.Get().UICamera.cameraToWorldMatrix * camPos;
    }
}
