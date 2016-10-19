using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

[AddComponentMenu("UI/UINineSlice")]
[ExecuteInEditMode]
public class UINineSlice : IntrusiveListNode<UINineSlice>
{
    public Material material = null;
    public Sprite sprite = null;
    public Color color = Color.white;
    public Rect coreRect;
    public float width = 1.0f;
    public float height = 1.0f;
    public Vector2 pivot = Vector2.zero;

    public int sortingLayerID;
    public int sortingOrder;

    protected float prevWidth;
    protected float prevHeight;
    protected Vector2 prevPivot;

#if UNITY_EDITOR
    protected Material prevMaterial = null;
    protected Sprite prevSprite = null;
    protected Color prevColor = Color.white;
    protected Rect prevCoreRect;

    protected int prevSortingLayerID;
    protected int prevSortingOrder;
#endif

    protected Transform slicesParent;
    protected float pixelsToUnit;
    protected float unitsToPixel;
    protected Sprite[] slices = new Sprite[9];
    protected SpriteRenderer[] sliceRenderers = new SpriteRenderer[9];
    protected Transform[] sliceTransforms = new Transform[9];
    protected Rect spriteTexRect;

    protected Animation anim;

    protected Rect GetSliceRect(int index)
    {
        Asserts.Assert(index >= 0 && index < 9);
        Rect tr = sprite.textureRect, r = new Rect();
        switch (index)
        {
            case 0: // up-left
                r = new Rect(0, 0, coreRect.xMin, coreRect.yMin);
                break;
            case 1: // up-center
                r = new Rect(coreRect.xMin, 0, coreRect.width, coreRect.yMin);
                break;
            case 2: // up-right
                r = new Rect(coreRect.xMax, 0, tr.width - coreRect.xMax, coreRect.yMin);
                break;
            case 3: // middle-left
                r = new Rect(0, coreRect.yMin, coreRect.xMin, coreRect.height);
                break;
            case 4: // middle-center
                r = coreRect;
                break;
            case 5: // middle-right
                r = new Rect(coreRect.xMax, coreRect.yMin, tr.width - coreRect.xMax, coreRect.height);
                break;
            case 6: // down-left
                r = new Rect(0, coreRect.yMax, coreRect.xMin, tr.height - coreRect.yMax);
                break;
            case 7: // down-center
                r = new Rect(coreRect.xMin, coreRect.yMax, coreRect.width, tr.height - coreRect.yMax);
                break;
            case 8: // down-right
                r = new Rect(coreRect.xMax, coreRect.yMax, tr.width - coreRect.xMax, tr.height - coreRect.yMax);
                break;
            default:
                break;
        }
        float dx = tr.xMin + sprite.textureRectOffset.x,
              dy = tr.yMin + sprite.textureRectOffset.y;
        r.xMin += dx;
        r.xMax += dx;
        r.yMin += dy;
        r.yMax += dy;
        return r;
    }

#if UNITY_EDITOR
    protected bool HasChanged()
    {
        return prevMaterial != material || prevSprite != sprite || prevColor != color || prevCoreRect != coreRect || prevSortingLayerID != sortingLayerID || prevSortingOrder != sortingOrder;
    }
#endif

    protected bool SizeHasNotChanged()
    {
        return prevWidth == width && prevHeight == height && prevPivot == pivot;
    }

    void Awake()
    {
        if (null == sprite)
            return;

#if UNITY_EDITOR
        if (sprite.packed && sprite.packingMode != SpritePackingMode.Rectangle)
            Debug.LogError("UINineSlice sprites should always be packed FullRect!", gameObject);
#endif

        spriteTexRect = sprite.textureRect;

        coreRect.xMin = Mathf.Clamp(coreRect.xMin, 0, spriteTexRect.width);
        coreRect.xMax = Mathf.Clamp(coreRect.xMax, 0, spriteTexRect.width);
        coreRect.yMin = Mathf.Clamp(coreRect.yMin, 0, spriteTexRect.height);
        coreRect.yMax = Mathf.Clamp(coreRect.yMax, 0, spriteTexRect.height);

        pixelsToUnit = sprite.bounds.size.y / spriteTexRect.height;
        unitsToPixel = 1.0f / pixelsToUnit;

        slicesParent = new GameObject(name + "_slices").transform;
        slicesParent.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
        slicesParent.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
        for (int i = 0; i < 9; ++i)
        {
            Rect sliceRect = this.GetSliceRect(i);
            slices[i] = Sprite.Create(sprite.texture, sliceRect, Vector2.zero, unitsToPixel, 0, SpriteMeshType.FullRect);
            slices[i].hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;

            sliceRenderers[i] = new GameObject("slice" + i, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sliceRenderers[i].gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            sliceRenderers[i].gameObject.layer = gameObject.layer;

            sliceTransforms[i] = sliceRenderers[i].transform;
            sliceTransforms[i].hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            sliceTransforms[i].parent = slicesParent;
            sliceTransforms[i].localRotation = Quaternion.identity;

            sliceRenderers[i].hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.DontSave;
            sliceRenderers[i].sprite = slices[i];
            sliceRenderers[i].color = color;
            sliceRenderers[i].sharedMaterial = material;

            sliceRenderers[i].sortingLayerID = sortingLayerID;
            sliceRenderers[i].sortingOrder = sortingOrder;
        }

#if UNITY_EDITOR
        prevMaterial = material;
        prevSprite = sprite;
        prevColor = color;
        prevCoreRect = coreRect;

        prevSortingLayerID = sortingLayerID;
        prevSortingOrder = sortingOrder;
#endif

        if (!this.enabled)
        {
            foreach (SpriteRenderer spriteRndr in sliceRenderers)
                spriteRndr.gameObject.SetActive(false);
        }

        Transform t = transform;
        anim = null;
        while (null == anim)
        {
            anim = t.gameObject.GetComponent<Animation>();
            t = t.parent;
            if (null == t)
                break;
        }
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;

        if (!Application.isPlaying && (null == slicesParent || this.HasChanged()))
            this.Awake();
#endif

        Manager<UIManager>.Get().AddNineSlice(this);

        if (sliceRenderers[0] != null)
        {
            foreach (SpriteRenderer spriteRndr in sliceRenderers)
                spriteRndr.gameObject.SetActive(true);

            this.UpdateSlices();
        }
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;
#endif

        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemoveNineSlice(this);

        if (sliceRenderers[0] != null)
        {
            foreach (SpriteRenderer spriteRndr in sliceRenderers)
                spriteRndr.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying && slicesParent != null)
            this.OnDestroy();
#endif
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        Gizmos.DrawCube(transform.TransformPoint(-(Vector3)pivot + new Vector3(width * 0.5f, height * 0.5f)), new Vector3(width, height));
    }

    void OnDrawGizmosSelected()
    { }
#endif
    void OnDestroy()
    {
        //Debug.Log("NineSlice.OnDestroy");
#if UNITY_EDITOR
        if (null == sliceRenderers[0])
            return;
#endif

        for (int i = 0; i < 9; ++i)
        {
            GameObject.DestroyImmediate(sliceRenderers[i].gameObject);
            Sprite.DestroyImmediate(slices[i]);

            sliceRenderers[i] = null;
            sliceTransforms[i] = null;
            slices[i] = null;
        }
        GameObject.DestroyImmediate(slicesParent.gameObject);
        slicesParent = null;
    }

    public Bounds GetBounds()
    {
        Vector3[] vertices = {
            transform.TransformPoint(-pivot),
            transform.TransformPoint(-pivot + Vector2.right * width),
            transform.TransformPoint(-pivot + Vector2.right * width + Vector2.up * height),
            transform.TransformPoint(-pivot + Vector2.up * height)
        };

        Bounds bounds = new Bounds();
        bounds.SetMinMax(Vector3.one * float.MaxValue, Vector3.one * -float.MaxValue);

        foreach (Vector3 wv in vertices)
            bounds.Encapsulate(wv);

        return bounds;
    }

    public void ApplyParameters()
    {
        if (null == sliceRenderers[0])
            return;

        // ToDo: check also if sprite changed?

        for (int i = 0; i < 9; ++i)
        {
            sliceRenderers[i].color = color;
            sliceRenderers[i].sharedMaterial = material;
            sliceRenderers[i].sortingLayerID = sortingLayerID;
            sliceRenderers[i].sortingOrder = sortingOrder;
        }
    }

    public void LateUpdateSlices()
    {
//#if !UNITY_EDITOR
        if (transform.hasChanged && slicesParent != null)
        {
//#endif
            slicesParent.position = transform.position;
            slicesParent.rotation = transform.rotation;
            slicesParent.localScale = transform.lossyScale;
//#if !UNITY_EDITOR
            transform.hasChanged = false;
        }
//#endif
        if (anim != null && anim.isPlaying)
            this.ApplyParameters();
    }

    public void UpdateSlices()
    {
#if SBS_PROFILER
        Profiler.BeginSample("UpdateSlices");
#endif
#if UNITY_EDITOR
        if (!Application.isPlaying && (null == slicesParent || this.HasChanged()))
        {
            this.OnDestroy();
            this.Awake();
        }
#endif

        if (null == sprite)
        {
#if SBS_PROFILER
            Profiler.EndSample();
#endif
            return;
        }
/*
        if (transform.hasChanged)
        {
            slicesParent.position = transform.position;
            slicesParent.rotation = transform.rotation;
            slicesParent.localScale = transform.lossyScale;

            transform.hasChanged = false;
        }
*/
#if UNITY_EDITOR
        if (Application.isPlaying && this.SizeHasNotChanged())
#else
        if (this.SizeHasNotChanged())
#endif
        {
#if SBS_PROFILER
            Profiler.EndSample();
#endif
            return;
        }

        prevWidth = width;
        prevHeight = height;
        prevPivot = pivot;

        float pixelsWidth = width * unitsToPixel,
              pixelsHeight = height * unitsToPixel;

        float extraWidth  = coreRect.xMin + (spriteTexRect.width - coreRect.xMax),
              extraHeight = coreRect.yMin + (spriteTexRect.height - coreRect.yMax);

#if UNITY_EDITOR
        if (coreRect.width < Mathf.Epsilon || coreRect.height < Mathf.Epsilon)
        {
#   if SBS_PROFILER
            Profiler.EndSample();
#   endif
            return;
        }
#endif

        float sx = ( pixelsWidth -  extraWidth) /  coreRect.width,
              sy = (pixelsHeight - extraHeight) / coreRect.height;

        float xMin = coreRect.xMin, xMax = coreRect.xMin + coreRect.width  * sx,
              yMin = coreRect.yMin, yMax = coreRect.yMin + coreRect.height * sy;

#if UNITY_EDITOR
        if (null == sliceRenderers[0])
        {
#   if SBS_PROFILER
            Profiler.EndSample();
#   endif
            return;
        }
#endif

        sliceTransforms[0].localPosition = -pivot;
        sliceTransforms[0].localScale = Vector3.one;

        sliceTransforms[1].localPosition = -pivot + Vector2.right * xMin * pixelsToUnit;
        sliceTransforms[1].localScale = new Vector3(sx, 1.0f, 1.0f);

        sliceTransforms[2].localPosition = -pivot + Vector2.right * xMax * pixelsToUnit;
        sliceTransforms[2].localScale = Vector3.one;

        sliceTransforms[3].localPosition = -pivot + Vector2.up * yMin * pixelsToUnit;
        sliceTransforms[3].localScale = new Vector3(1.0f, sy, 1.0f);

        sliceTransforms[4].localPosition = -pivot + (Vector2.right * xMin + Vector2.up * yMin) * pixelsToUnit;
        sliceTransforms[4].localScale = new Vector3(sx, sy, 1.0f);

        sliceTransforms[5].localPosition = -pivot + (Vector2.right * xMax + Vector2.up * yMin) * pixelsToUnit;
        sliceTransforms[5].localScale = new Vector3(1.0f, sy, 1.0f);

        sliceTransforms[6].localPosition = -pivot + Vector2.up * yMax * pixelsToUnit;
        sliceTransforms[6].localScale = Vector3.one;

        sliceTransforms[7].localPosition = -pivot + (Vector2.right * xMin + Vector2.up * yMax) * pixelsToUnit;
        sliceTransforms[7].localScale = new Vector3(sx, 1.0f, 1.0f);

        sliceTransforms[8].localPosition = -pivot + (Vector2.right * xMax + Vector2.up * yMax) * pixelsToUnit;
        sliceTransforms[8].localScale = Vector3.one;

#if SBS_PROFILER
        Profiler.EndSample();
#endif
    }
}
