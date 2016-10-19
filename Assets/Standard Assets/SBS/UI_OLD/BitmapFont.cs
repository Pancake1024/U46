using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.UI;

[AddComponentMenu("SBS/UI/BitmapFont")]
public class BitmapFont : MonoBehaviour
{
    protected static char[] lineEndingChars = { '\n', '\t', ' ', '.', ',', ';', ':', '-' };

    #region Public classes
    [Serializable]
    public class CharacterDesc {
        public int code;
        public Rect rect;
        public Vector2 offset;
        public int batchIndex;
        public float xAdvance;
    };

    [Serializable]
    public class CharsKerning
    {
        public int firstChar;
        public int secondChar;
        public float amount;
    }
    #endregion

    #region Public memebers
    public string fontName;
    public string fontFace;
    public float fontSize;
    public float lineHeight;
    public CharacterDesc[] chars;
    public CharsKerning[] kernings;
    public Texture2D[] pages;
    public int capacityPerPage;
    public RectOffset padding;
    public float extraSpacing;
    #endregion

    #region Protected members
    protected Dictionary<int, CharacterDesc> charsDict;
    protected Dictionary<int, List<CharsKerning>> kernHash;
    protected OverlaysBatch[] batches;
    #endregion

    #region Public methods
    public void Initialize()
    {
        charsDict = new Dictionary<int, CharacterDesc>();
        foreach (CharacterDesc desc in chars)
            charsDict.Add(desc.code, desc);

        kernHash = new Dictionary<int, List<CharsKerning>>();
        foreach (CharsKerning kern in kernings)
        {
            int hash = kern.firstChar + kern.secondChar;
            List<CharsKerning> kernList;

            if (kernHash.TryGetValue(hash, out kernList))
                kernList.Add(kern);
            else
            {
                kernList = new List<CharsKerning>();
                kernList.Add(kern);

                kernHash.Add(hash, kernList);
            }
        }

        batches = new OverlaysBatch[pages.Length];
        int i = 0;
        foreach (Texture2D tex in pages)
            batches[i++] = UIManager_OLD.Instance.RegisterBatch(fontName + "_" + i, capacityPerPage, tex);
    }

    public OverlaysBatch GetBatch(int index)
    {
        return batches[index];
    }

    public bool HasCharacter(char c)
    {
        return charsDict.ContainsKey(c);
    }

    public Vector2 GetMultilineTextSize(string text, float maxWidth, out List<KeyValuePair<int, float>> lineEndIndices)
    {
        Vector2 size = new Vector2(0.0f, lineHeight);
        float sx = 1.0f, sy = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
        {
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            size.y = lineHeight * sy;
        }

        int last = text.Length - 1;
        CharacterDesc charDesc;
        List<CharsKerning> kernList;
        List<float> sizes = new List<float>();
        float maxLineSize = 0.0f;
        char c0, c1;

        lineEndIndices = new List<KeyValuePair<int, float>>();

        for (int i = 0; i <= last; ++i)
        {
            c0 = text[i];

            if (charsDict.TryGetValue(c0, out charDesc))
            {
                float kerning = 0.0f;
                if (i < last)
                {
                    c1 = text[i + 1];
                    if (kernHash.TryGetValue((int)c0 + (int)c1, out kernList))
                    {
                        foreach (CharsKerning kern in kernList)
                        {
                            if (c0 == kern.firstChar && c1 == kern.secondChar)
                            {
                                kerning = kern.amount;
                                break;
                            }
                        }
                    }
                }

                if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                    size.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning) * sx;
                else
                    size.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning);
            }

            sizes.Add(size.x);

            if (size.x > maxWidth || '\n' == c0)
            {
                int lastLineEndIndex = lineEndIndices.Count > 0 ? lineEndIndices[lineEndIndices.Count - 1].Key : 0;
#if UNITY_FLASH
                int newLineEndIndex = AS3Utils.LastIndexOfAny(text, lineEndingChars, i, i - lastLineEndIndex);
#else
                int newLineEndIndex = text.LastIndexOfAny(lineEndingChars, i, i - lastLineEndIndex);
#endif
#if UNITY_EDITOR
                int count = 0;
                while (newLineEndIndex >= 0 && sizes[newLineEndIndex] > maxWidth && count++ < 100)
#else
                while (newLineEndIndex >= 0 && sizes[newLineEndIndex] > maxWidth)
#endif
                {
                    Debug.Log("newLineEndIndex: " + newLineEndIndex + ", lastLineEndIndex: " + lastLineEndIndex);
#if UNITY_FLASH
                    newLineEndIndex = AS3Utils.LastIndexOfAny(text, lineEndingChars, newLineEndIndex - 1, newLineEndIndex - 1 - lastLineEndIndex);
#else
                    newLineEndIndex = text.LastIndexOfAny(lineEndingChars, newLineEndIndex - 1, newLineEndIndex - 1 - lastLineEndIndex);
#endif
                }
                if (-1 == newLineEndIndex)
                {
                    maxLineSize = SBSMath.Max(maxLineSize, size.x);
                }
                else
                {
                    float sizeOffset = sizes[newLineEndIndex];

                    bool retOnSpace = (' ' == text[newLineEndIndex]);
                    ++newLineEndIndex;
                    if (' ' == text[newLineEndIndex])
                    {
                    	++newLineEndIndex;
                    	retOnSpace = true;
                    }

                    lineEndIndices.Add(new KeyValuePair<int, float>(newLineEndIndex, (retOnSpace ? sizes[Mathf.Max(0, newLineEndIndex - 1)] : sizeOffset)));

                    ++newLineEndIndex;
                    for (; newLineEndIndex < i; ++newLineEndIndex)
                        sizes[newLineEndIndex] -= sizeOffset;

                    size.x -= sizeOffset;
                    if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                        size.y += lineHeight * sy;
                    else
                        size.y += lineHeight;
                }
            }
        }

        lineEndIndices.Add(new KeyValuePair<int, float>(last + 1, size.x));
        size.x = SBSMath.Max(maxLineSize, maxWidth);

        return size;
    }

    public Vector2 GetTextSize(string text)
    {
        Vector2 size = new Vector2(0.0f, lineHeight);
        float sx = 1.0f, sy = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
        {
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);
            size.y = lineHeight * sy;
        }

        int last = text.Length - 1;
        CharacterDesc charDesc;
        List<CharsKerning> kernList;
        char c0, c1;

        for (int i = 0; i <= last; ++i)
        {
            c0 = text[i];
            if (charsDict.TryGetValue(c0, out charDesc))
            {
//              size.y = SBSMath.Max(size.y, charDesc.rect.height);

                float kerning = 0.0f;
                if (i < last)
                {
                    c1 = text[i + 1];
                    if (kernHash.TryGetValue((int)c0 + (int)c1, out kernList))
                    {
                        foreach (CharsKerning kern in kernList)
                        {
                            if (c0 == kern.firstChar && c1 == kern.secondChar)
                            {
                                kerning = kern.amount;
                                break;
                            }
                        }
                    }
                }

                if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                    size.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning) * sx;
                else
                    size.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning);
            }
        }

        return size;
    }

    public Overlay DrawCharacter(char c, Color color, ref Vector2 position, ref Vector2 textSize, Overlay overlay)
    {
        float sx = 1.0f, sy = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);

        CharacterDesc charDesc;
        if (charsDict.TryGetValue(c, out charDesc))
        {
            OverlaysBatch charBatch = batches[charDesc.batchIndex];
            Asserts.Assert(null == overlay || overlay.Batch.Image == charBatch.Image);

            Rect charRect = new Rect(position.x, position.y, 0.0f, 0.0f);
            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            {
                charRect.xMin += charDesc.offset.x * sx;
                charRect.yMin += charDesc.offset.y * sy;
                charRect.width = (charDesc.rect.xMax - charDesc.rect.xMin) * sx;
                charRect.height = (charDesc.rect.yMax - charDesc.rect.yMin) * sy;
            }
            else
            {
                charRect.xMin += charDesc.offset.x;
                charRect.yMin += charDesc.offset.y;
                charRect.width = charDesc.rect.xMax - charDesc.rect.xMin;
                charRect.height = charDesc.rect.yMax - charDesc.rect.yMin;
            }

            if (null == overlay)
                overlay = new Overlay(batches[charDesc.batchIndex], SBSMatrix4x4.identity, charRect, charDesc.rect, color);
            else
            {
                overlay.ScreenRect = charRect;
                overlay.ImageRect = charDesc.rect;
                overlay.Color = color;
            }

            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                position.x = charRect.x + (charDesc.xAdvance + extraSpacing) * sx;
            else
                position.x = charRect.x + charDesc.xAdvance + extraSpacing;

            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            {
                textSize.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing) * sx;
                textSize.y = lineHeight * sy;
            }
            else
            {
                textSize.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing);
                textSize.y = lineHeight;// SBSMath.Max(textSize.y, charRect.height);
            }

            return overlay;
        }
        else
        {
            return null;
        }
    }

    public Overlay DrawCharacterWithKerning(char c0, char c1, Color color, ref Vector2 position, ref Vector2 textSize, Overlay overlay)
    {
        float sx = 1.0f, sy = 1.0f;
        if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);

        CharacterDesc charDesc;
        if (charsDict.TryGetValue(c0, out charDesc))
        {
            OverlaysBatch charBatch = batches[charDesc.batchIndex];
            Asserts.Assert(null == overlay || overlay.Batch.Image == charBatch.Image);

            Rect charRect = new Rect(position.x, position.y, 0.0f, 0.0f);
            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            {
                charRect.xMin += charDesc.offset.x * sx;
                charRect.yMin += charDesc.offset.y * sy;
                charRect.width = (charDesc.rect.xMax - charDesc.rect.xMin) * sx;
                charRect.height = (charDesc.rect.yMax - charDesc.rect.yMin) * sy;
            }
            else
            {
                charRect.xMin += charDesc.offset.x;
                charRect.yMin += charDesc.offset.y;
                charRect.width = charDesc.rect.xMax - charDesc.rect.xMin;
                charRect.height = charDesc.rect.yMax - charDesc.rect.yMin;
            }

            if (null == overlay)
                overlay = new Overlay(batches[charDesc.batchIndex], SBSMatrix4x4.identity, charRect, charDesc.rect, color);
            else
            {
                overlay.ScreenRect = charRect;
                overlay.ImageRect = charDesc.rect;
                overlay.Color = color;
            }

            float kerning = 0.0f;
            List<CharsKerning> kernList;
            if (kernHash.TryGetValue((int)c0 + (int)c1, out kernList))
            {
                foreach (CharsKerning kern in kernList)
                {
                    if (c0 == kern.firstChar && c1 == kern.secondChar)
                    {
                        kerning = kern.amount;
                        break;
                    }
                }
            }

            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                position.x = charRect.x + (charDesc.xAdvance + extraSpacing + kerning) * sx;
            else
                position.x = charRect.x + charDesc.xAdvance + extraSpacing + kerning;

            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
            {
                textSize.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning) * sx;
                textSize.y = lineHeight * sy;
            }
            else
            {
                textSize.x += (charDesc.offset.x + charDesc.xAdvance + extraSpacing + kerning);
                textSize.y = lineHeight;// SBSMath.Max(textSize.y, charRect.height);
            }

            return overlay;
        }
        else
        {
            return null;
        }
    }
    #endregion
}
