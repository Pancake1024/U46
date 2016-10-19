using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("UI/UITextField")]
[ExecuteInEditMode]
public class UITextField : IntrusiveListNode<UITextField>
{
    protected static char[] lineEndingChars = { '\n', '\t', ' ', '.', ',', ';', ':', '-' };

    [Multiline]
    public string text;

    public Font font;
    public Material material = null;
    public FontStyle style = FontStyle.Normal;
    public int size = 8;
    public float scaling = 0.5f;
    public float lineSpacing = 1.0f;
    public Color color = Color.white;
    public TextAnchor alignment = TextAnchor.UpperLeft;
    public bool wordWrap = false;
    public bool dynamic = false;
    public bool scaleToFit = false;

    public float width = 1.0f;
    public float height = 1.0f;
    public Vector2 pivot = Vector2.zero;

    public bool resizeButton = false;

    public int sortingLayerID;
    public int sortingOrder;

	// useful when using textfields in animations that changes the color
	public enum ApplyTypes
	{
		APPLY_ON_CHANGE,
		ALWAYS_APPLY
	}
	public ApplyTypes ApplyParametersType = ApplyTypes.APPLY_ON_CHANGE;

    public Signal onResize = Signal.Create<UITextField>();

    protected GameObject textGO = null;
    protected Transform textTr = null;
    protected MeshRenderer meshRenderer = null;
    protected TextMesh textMesh = null;

    protected Dictionary<char, float> charSize;

    protected UIButton parentButton = null;
    protected UINineSlice[] buttonNineSlices = null;

    protected StringBuilder textBuilder = null;
    protected StringBuilder wordBuilder = null;

    protected bool awakeCalled = false;
    protected string prevText = string.Empty;

#if UNITY_EDITOR
    protected Font prevFont = null;
    protected Material prevMaterial = null;
    protected FontStyle prevStyle = FontStyle.Normal;
    protected int prevSize = 8;
    protected float prevLineSpacing = 1.0f;
    protected float prevScaling = 0.5f;
    protected Color prevColor = Color.white;
    protected TextAnchor prevAlignment = TextAnchor.UpperLeft;
    protected bool prevWordWrap = false;
    protected bool prevDynamic = false;

    protected int prevSortingLayerID;
    protected int prevSortingOrder;

    protected bool HasChanged()
    {
        return
            prevFont != font || prevMaterial != material || prevStyle != style || prevSize != size || prevLineSpacing != lineSpacing ||
            prevScaling != scaling || prevColor != color || prevAlignment != alignment ||
            prevWordWrap != wordWrap || prevDynamic != dynamic ||
            prevSortingLayerID != sortingLayerID || prevSortingOrder != sortingOrder;
    }
#endif

    protected void InitCharSize()
    {
        Vector3 prevScale = textTr.localScale;
        Quaternion prevRot = textTr.rotation;
        textTr.localScale = Vector3.one;
        textTr.rotation = Quaternion.identity;
        
        textMesh.text = "a";
        float aw = meshRenderer.bounds.size.x;

        textMesh.text = "a a";
        float cw = meshRenderer.bounds.size.x - 2 * aw;

        charSize = new Dictionary<char, float>();
        charSize.Add(' ', cw);
		charSize.Add('a', aw);

        textTr.localScale = prevScale;
        textTr.rotation = prevRot;
    }

    protected void CreateTextMesh()
    {
        if (null == font)
            return;

        if (null == textGO)
        {
            textGO = new GameObject(name + "_text");
            textGO.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;

            meshRenderer = textGO.AddComponent<MeshRenderer>();
            meshRenderer.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;

            textMesh = textGO.AddComponent<TextMesh>();
            textMesh.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave;

            textTr = textGO.transform;
        }

        textGO.layer = gameObject.layer;

        meshRenderer.material = null == material ? font.material : material;
        if (material != null)
            meshRenderer.material.mainTexture = font.material.mainTexture;

        meshRenderer.sortingLayerID = sortingLayerID;
        meshRenderer.sortingOrder = sortingOrder;

        switch (alignment)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                textMesh.anchor = alignment;
                textMesh.alignment = TextAlignment.Left;
                break;
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                textMesh.anchor = alignment;
                textMesh.alignment = TextAlignment.Center;
                break;
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                textMesh.anchor = alignment;
                textMesh.alignment = TextAlignment.Right;
                break;
        }
        textMesh.characterSize = size * scaling / Manager<UIManager>.Get().pixelsPerUnit;
        textMesh.lineSpacing = lineSpacing;
        textMesh.color = color;
        textMesh.font = font;
        textMesh.fontSize = size;
        textMesh.fontStyle = style;
        textMesh.richText = !wordWrap;//!dynamic

        if (null == charSize)
            this.InitCharSize();

        if (wordWrap && text.Length > 0)
            this.FitToWidth(width);
        else
            textMesh.text = text;

        if (scaleToFit)
            textMesh.characterSize = this.GetCharacterSize();

        this.PlaceText();

        if (resizeButton)
            this.ResizeButton();

        prevText = text;

#if UNITY_EDITOR
        prevFont = font;
        prevStyle = style;
        prevSize = size;
        prevScaling = scaling;
        prevColor = color;
        prevAlignment = alignment;
        prevWordWrap = wordWrap;
        prevDynamic = dynamic;

        prevSortingLayerID = sortingLayerID;
        prevSortingOrder = sortingOrder;
#endif
    }

    protected float GetCharacterSize()
    {
        float bs = size * scaling / Manager<UIManager>.Get().pixelsPerUnit;

        if (!scaleToFit)
            return bs;

        Quaternion backupRot = textTr.rotation;
        textTr.rotation = Quaternion.identity;

        //Vector2 textSize = this.GetTextSize();
        //float s = Mathf.Min(width / textSize.x, height / textSize.y);
        Rect textBounds = this.GetTextBounds();
        float s = Mathf.Min(width / textBounds.width, height / textBounds.height);

        textTr.rotation = backupRot;

        return bs * Mathf.Min(1.0f, s);
    }

    protected void FitToWidth(float wantedWidth)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            this.InitCharSize();
#endif

        if (null == textBuilder)
            textBuilder = new StringBuilder();
        else
            textBuilder.Length = 0;

        int lineStartIndex = 0, lineEndIndex = text.IndexOf('\n'), textLen = text.Length;
        if (-1 == lineEndIndex)
            lineEndIndex = textLen;
        while (lineStartIndex < lineEndIndex)
        {
            this.WrapLine(wantedWidth, text, lineStartIndex, lineEndIndex - 1);

            lineStartIndex = lineEndIndex + 1;
            if (lineStartIndex < textLen)
            {
                lineEndIndex = text.IndexOf('\n', lineStartIndex);
                if (-1 == lineEndIndex)
                    lineEndIndex = text.Length;
            }
        }
        int lastIndex = textBuilder.Length - 1;
        if (lastIndex >= 0)
            textMesh.text = textBuilder.ToString(0, '\n' == textBuilder[lastIndex] ? lastIndex : lastIndex + 1); // Allocation of string...BAD
        else
            textMesh.text = textBuilder.ToString();
    }

    protected void WrapLine(float w, string s, int lineStartIndex, int lineEndIndex)
    {
        int lineLen = (lineEndIndex - lineStartIndex) + 1;
        // need to check if smaller than maximum character length, really...
        if (lineLen <= 0)
            return;

        if (null == wordBuilder)
            wordBuilder = new StringBuilder();
        else
            wordBuilder.Length = 0;

        float charWidth = 0.0f, wordWidth = 0.0f, currentWidth = 0.0f;
        for (int i = lineStartIndex; i <= lineEndIndex; ++i)
        {
            char c = text[i];

            if (!charSize.TryGetValue(c, out charWidth) || charWidth < Mathf.Epsilon)
            {
                Vector3 prevScale = textTr.localScale;
                Quaternion prevRot = textTr.rotation;
                textTr.localScale = Vector3.one;
                textTr.rotation = Quaternion.identity;

                textMesh.text = "" + c;
                charWidth = meshRenderer.bounds.size.x;
                charSize[c] = charWidth;

                textTr.localScale = prevScale;
                textTr.rotation = prevRot;
            }

            if (i == lineEndIndex || Array.IndexOf<char>(lineEndingChars, c) > -1)
            {
                wordBuilder.Append(c);
                if (c != ' ')
                    wordWidth += charWidth;

                if (currentWidth + wordWidth <= w)
                {
                    currentWidth += wordWidth;
                    if (' ' == c)
                    {
                        if ((currentWidth + charWidth) <= w)
                            currentWidth += charWidth;
                        else
                            wordBuilder.Remove(wordBuilder.Length - 1, 1);
                    }

                    for (int j = 0, k = wordBuilder.Length; j < k; ++j)
                        textBuilder.Append(wordBuilder[j]);
                }
                else
                {
                    if (' ' == c)
                        wordWidth += charWidth;

                    currentWidth = wordWidth;

                    if (textBuilder.Length > 0 && textBuilder[textBuilder.Length - 1] != '\n')
                        textBuilder.Append('\n');

                    int j = (' ' == wordBuilder[0] ? 1 : 0);
                    for (int k = wordBuilder.Length; j < k; ++j)
                        textBuilder.Append(wordBuilder[j]);
                }

                wordBuilder.Length = 0;
                wordWidth = 0;
            }
            else
            {
                wordBuilder.Append(c);
                wordWidth += charWidth;
            }
        }

        textBuilder.Append('\n');
    }

    protected void ResizeButton()
    {
        UIButton btn = transform.parent.gameObject.GetComponent<UIButton>();
        if (null == btn)
            return;

        if (btn != parentButton)
        {
            parentButton = btn;
            buttonNineSlices = btn.gameObject.GetComponentsInChildren<UINineSlice>(true);
        }

		Quaternion prevBtnRot = btn.transform.rotation,
		prevTxtRot = textTr.rotation;
		btn.transform.rotation = Quaternion.identity;
		textTr.rotation = Quaternion.identity;

        UINineSlice[] nSlices = buttonNineSlices;// btn.gameObject.GetComponentsInChildren<NineSlice>(true); // Prevent allocation at every frame
        Rect margins = new Rect();
        foreach (UINineSlice nSlice in nSlices)
        {
            if (null == nSlice.sprite)
                continue;

            margins.xMin = Mathf.Max(margins.xMin, nSlice.coreRect.xMin);
            margins.xMax = Mathf.Max(margins.xMax, nSlice.sprite.textureRect.width - nSlice.coreRect.xMax);
            margins.yMin = Mathf.Max(margins.yMin, nSlice.coreRect.yMin);
            margins.yMax = Mathf.Max(margins.yMax, nSlice.sprite.textureRect.height - nSlice.coreRect.yMax);
        }
        /*float ptu = 1.0f / Manager<UIManager>.Get().pixelsPerUnit;
        margins.xMin *= ptu;
        margins.xMax *= ptu;
        margins.yMin *= ptu;
        margins.yMax *= ptu;*/
/*
        Vector3 prevScale = Vector3.one;
        UILayoutAndScaling lsParent = null;
        Transform par = transform.parent;
        while (par != null && null == lsParent)
        {
            lsParent = par.gameObject.GetComponent<UILayoutAndScaling>();
            par = par.parent;
        }
        if (lsParent != null)
        {
            prevScale = lsParent.transform.localScale;
            lsParent.transform.localScale = Vector3.one;
        }
*/
        Rect rect = this.GetTextBounds();
        if (rect.width < SBSMath.Epsilon && rect.height < SBSMath.Epsilon)
        {
            btn.transform.rotation = prevBtnRot;
            textTr.rotation = prevTxtRot;
            return;
        }

        float utp = Manager<UIManager>.Get().pixelsPerUnit;
        rect.xMin *= utp;
        rect.xMax *= utp;
        rect.yMin *= utp;
        rect.yMax *= utp;

        rect.xMin -= (margins.xMin + parentButton.resizeOffset.left);// * ptu);
        rect.xMax += (margins.xMax + parentButton.resizeOffset.right);// * ptu);
        rect.yMin -= (margins.yMin + parentButton.resizeOffset.bottom);// * ptu);
        rect.yMax += (margins.yMax + parentButton.resizeOffset.top);// * ptu);

        float delta = parentButton.minSize.x - rect.width;
        if (delta > 0.0f)
        {
            delta *= 0.5f;
            rect.xMin -= delta;
            rect.xMax += delta;
        }

        delta = parentButton.minSize.y - rect.height;
        if (delta > 0.0f)
        {
            delta *= 0.5f;
            rect.yMin -= delta;
            rect.yMax += delta;
        }

        float ptu = 1.0f / utp;
        rect.xMin *= ptu;
        rect.xMax *= ptu;
        rect.yMin *= ptu;
        rect.yMax *= ptu;

        BoxCollider2D btnBox = btn.gameObject.GetComponent<BoxCollider2D>();
        //btnBox.center = rect.center - (Vector2)(btn.transform.parent.position + btn.transform.parent.rotation * btn.transform.localPosition);
        btnBox.offset = btn.transform.InverseTransformPoint(rect.center);
        btnBox.size = new Vector2(rect.width, rect.height);

        foreach (UINineSlice nSlice in nSlices)
        {
            //nSlice.pivot = -(new Vector2(rect.xMin, rect.yMin) - (Vector2)(nSlice.transform.parent.position + nSlice.transform.parent.rotation * nSlice.transform.localPosition));
            //nSlice.pivot = -nSlice.transform.InverseTransformPoint(new Vector2(rect.xMin, rect.yMin));
            nSlice.pivot = -(Vector2)nSlice.transform.InverseTransformPoint(rect.center) + new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            nSlice.width = rect.width;
            nSlice.height = rect.height;
        }
/*
        if (lsParent != null)
            lsParent.transform.localScale = prevScale;
*/
        btn.transform.rotation = prevBtnRot;
		textTr.rotation = prevTxtRot;
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            onResize.Invoke(this);
        }
    }

    protected void PlaceText()
    {
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height);
                break;
            case TextAnchor.UpperCenter:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height + Vector2.right * width * 0.5f);
                break;
            case TextAnchor.UpperRight:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height + Vector2.right * width);
                break;
            case TextAnchor.MiddleLeft:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height * 0.5f);
                break;
            case TextAnchor.MiddleCenter:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height * 0.5f + Vector2.right * width * 0.5f);
                break;
            case TextAnchor.MiddleRight:
                textTr.position = transform.TransformPoint(-pivot + Vector2.up * height * 0.5f + Vector2.right * width);
                break;
            case TextAnchor.LowerLeft:
                textTr.position = transform.TransformPoint(-pivot);
                break;
            case TextAnchor.LowerCenter:
                textTr.position = transform.TransformPoint(-pivot + Vector2.right * width * 0.5f);
                break;
            case TextAnchor.LowerRight:
                textTr.position = transform.TransformPoint(-pivot + Vector2.right * width);
                break;
        }
        textTr.rotation = transform.rotation;
        textTr.localScale = transform.lossyScale;
    }

    void Awake()
    {
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;
#endif

        this.CreateTextMesh();

        if (textGO != null)
            textGO.SetActive(this.enabled);

        awakeCalled = true;
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;

        if (!Application.isPlaying && (null == textGO || null == charSize || this.HasChanged()))
        {
            if (charSize != null && (prevFont != font || prevStyle != style || prevSize != size))
                charSize.Clear();

            this.CreateTextMesh();
        }
#endif

        if (textGO != null)
        {
            textGO.SetActive(true);

            if (scaleToFit)
            {
                textMesh.characterSize = size * scaling / Manager<UIManager>.Get().pixelsPerUnit;
                textMesh.characterSize = this.GetCharacterSize();
            }
        }
        else
            this.CreateTextMesh();

        Manager<UIManager>.Get().AddTextField(this);
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;
#endif

        if (Manager<UIManager>.Get() != null)
            Manager<UIManager>.Get().RemoveTextField(this);

        if (textGO != null)
            textGO.SetActive(false);
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
        //Debug.Log("TextField.OnDestroy");
#if UNITY_EDITOR
        if (null == textGO)
            return;
#endif

        GameObject.DestroyImmediate(textGO);
        textGO = null;
        textTr = null;
    }

    public Rect GetTextBounds()
    {
        Vector2 c = meshRenderer.bounds.center,//textTr.InverseTransformPoint(meshRenderer.bounds.center),
                sz = meshRenderer.bounds.size,
                sc = textTr.lossyScale;
        sc.x = Mathf.Abs(sc.x) > 1.0e-3f ? 1.0f / sc.x : 0.0f;
        sc.y = Mathf.Abs(sc.y) > 1.0e-3f ? 1.0f / sc.y : 0.0f;
        sz.Scale(sc);
        //c += (Vector2)textTr.position;
        return new Rect(c.x - sz.x * 0.5f, c.y - sz.y * 0.5f, sz.x, sz.y);
        //Vector2 sz = meshRenderer.bounds.size;
        //return new Rect(meshRenderer.bounds.min.x, meshRenderer.bounds.min.y, sz.x, sz.y);
    }

    public Bounds GetRendererBounds()
    {
        if (null == meshRenderer)
            return new Bounds(transform.position, Vector3.zero);
        return meshRenderer.bounds;
    }

    public Vector2 GetTextSize()
    {
        return meshRenderer.bounds.size;
    }

    public void ApplyParameters(bool fontChanged)
    {
        if (!awakeCalled)
            return;
#if UNITY_EDITOR
        if (null == Manager<UIManager>.Get())
            return;
#endif
        if (fontChanged)
            charSize = null;

        this.CreateTextMesh();

        if (textGO != null)
            textGO.SetActive(this.enabled);
    }

    public void ApplyParameters()
    {
        this.ApplyParameters(false);
    }

    public void UpdateText()
    {
		if( ApplyParametersType == ApplyTypes.ALWAYS_APPLY && awakeCalled)
		{
            if (scaleToFit)
                textMesh.characterSize = size * scaling / Manager<UIManager>.Get().pixelsPerUnit;

            textMesh.characterSize = this.GetCharacterSize();
			textMesh.color = color;
			textMesh.fontSize = size;
            textMesh.lineSpacing = lineSpacing;
		}

#if UNITY_EDITOR
        Profiler.BeginSample("UpdateText");

        if (!Application.isPlaying && (null == textGO || this.HasChanged()))
            this.CreateTextMesh();

        if (Application.isPlaying && null == textGO)
        {
            this.CreateTextMesh();

            if (textGO != null)
                textGO.SetActive(this.enabled);
        }

        if (Application.isPlaying)
        {
            meshRenderer.sortingLayerID = sortingLayerID;
            meshRenderer.sortingOrder = sortingOrder;
        }

        if (null == textGO)
        {
            Profiler.EndSample();
            return;
        }
#else
        if (null == textGO)
        {
            this.CreateTextMesh();

            if (textGO != null)
                textGO.SetActive(this.enabled);
        }

        if (null == textGO || !dynamic)
            return;
#endif
#if UNITY_EDITOR || !UNITY_METRO
        if (!text.Equals(prevText, StringComparison.InvariantCulture))
#else
        if (!text.Equals(prevText, StringComparison.Ordinal))
#endif
        {
            if (scaleToFit)
                textMesh.characterSize = size * scaling / Manager<UIManager>.Get().pixelsPerUnit;

            if (wordWrap && text.Length > 0)
                this.FitToWidth(width);
            else
                textMesh.text = text;

            if (scaleToFit)
                textMesh.characterSize = this.GetCharacterSize();

            prevText = text;
        }

        this.PlaceText();

        if (resizeButton)
            this.ResizeButton();

#if UNITY_EDITOR
        Profiler.EndSample();
#endif
    }
}
