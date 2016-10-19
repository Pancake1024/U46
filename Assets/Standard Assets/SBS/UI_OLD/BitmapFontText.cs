using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.UI
{
    public class BitmapFontText : UIElement
    {
        #region Align enum
        public enum TextAlign
        {
            UpLeft = 0,
            UpCenter,
            UpRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            DownLeft,
            DownCenter,
            DownRight
        }
        #endregion

        #region Protected members
        protected BitmapFont font;
        protected Color color;
        protected string text;
        protected Vector2 position;
        protected bool forcedPosition = false;
        protected Vector2 textSize;
        protected Rect scrRect;
        protected TextAlign align;
        protected bool multiline = false;
        protected List<KeyValuePair<int, float>> lineEndIndices = null;
        protected bool visible;
        protected List<Overlay> overlays;
        protected SBSMatrix4x4 transform = SBSMatrix4x4.identity;
        //protected int scissorRectIndex = -1;
        protected string scissorRectName = null;
        protected Rect scissorRect;
        protected int depth = -1;
		protected bool useKerning = true;
        #endregion

        #region Ctors
        public BitmapFontText()
        {
            font = null;
            color = Color.white;
            text = string.Empty;
            position = Vector2.zero;
            textSize = Vector2.zero;
            scrRect = new Rect(0, 0, 0, 0);
            align = TextAlign.UpLeft;
            visible = true;
            overlays = new List<Overlay>();
        }
        #endregion

        #region Public properties
        public OverlaysBatch Batch
        {
            get
            {
                return font.GetBatch(0);
            }
        }

        public BitmapFont Font
        {
            get
            {
                return font;
            }
            set
            {
                foreach (Overlay overlay in overlays)
                    overlay.Destroy();

                overlays.Clear();

                font = value;
                depth = font.GetBatch(0).BaseDepth;

                this.Text = text;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;

                foreach (Overlay overlay in overlays)
                    overlay.Color = color;
            }
        }

        public virtual string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (!forcedPosition)
                {
                    if (font != null)
                    {
                        if (multiline)
                            textSize = font.GetMultilineTextSize(value, scrRect.width, out lineEndIndices);
                        else
                            textSize = font.GetTextSize(value);
                    }

                    this.RefreshPositionFromAlign(scrRect);
                }
                else if (multiline)
                    textSize = font.GetMultilineTextSize(value, scrRect.width, out lineEndIndices);

                this.RenderText(position, value);

                text = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                forcedPosition = true;
                this.RenderText(value, text);
                position = value;
            }
        }

        public Vector2 TextSize
        {
            get
            {
                return textSize;
            }
        }

        public Rect ScreenRect
        {
            get
            {
                return scrRect;
            }
            set
            {
                this.RefreshPositionFromAlign(value);
                this.RenderText(position, text);
                scrRect = value;
            }
        }

        public TextAlign Align
        {
            get
            {
                return align;
            }
            set
            {
                align = value;
                this.RefreshPositionFromAlign(scrRect);
                this.RenderText(position, text);
            }
        }

        public bool MultiLine
        {
            get
            {
                return multiline;
            }
            set
            {
                if (value != multiline)
                {
                    multiline = value;
                    this.Text = text;
                }
            }
        }
		
		public bool UseKerning
		{
			get
			{
				return useKerning;
			}
			set
			{
				if (value != useKerning)
				{
					useKerning = !useKerning;
					this.RenderText(position, text);
				}
			}
		}
		
		/*
        public bool Clipped
        {
            get
            {
                foreach (Overlay overlay in overlays)
                {
                    if (!overlay.Visibility)
                        continue;
                    if (!overlay.Clipped)
                        return false;
                }
                return true;
            }
        }
        */
        public bool Visibility
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;

                    if (value)
                    {
                        this.RenderText(position, text);
                    }
                    else
                    {
                        foreach (Overlay overlay in overlays)
                            overlay.Visibility = false;
                    }
                }
            }
        }

        public SBSMatrix4x4 Transform
        {
            get
            {
                return transform;
            }
            set
            {
                foreach (Overlay overlay in overlays)
                    overlay.Transform = value;
                transform = value;
            }
        }
        /*
        public int ScissorRectIndex
        {
            get
            {
                return scissorRectIndex;
            }
            set
            {
                foreach (Overlay overlay in overlays)
                    overlay.ScissorRectIndex = value;
                scissorRectIndex = value;
            }
        }

        public string ScissorRectName
        {
            get
            {
                return font.GetBatch(0).GetScissorRectName(scissorRectIndex);
            }
            set
            {
                scissorRectIndex = font.GetBatch(0).GetScissorRectIndex(value);
                foreach (Overlay overlay in overlays)
                    overlay.ScissorRectIndex = scissorRectIndex;
            }
        }
        */
        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                foreach (Overlay overlay in overlays)
                    overlay.Depth = value;
                depth = value;
            }
        }
        #endregion

        #region Protected methods
        protected void RefreshPositionFromAlign(Rect rect)
        {
            forcedPosition = false;

            switch (align)
            {
                case TextAlign.UpLeft:
                    position.x = rect.xMin;
                    position.y = rect.yMin;
                    break;
                case TextAlign.UpCenter:
                    position.x = Mathf.RoundToInt((rect.xMin + rect.xMax - textSize.x) * 0.5f);
                    position.y = rect.yMin;
                    break;
                case TextAlign.UpRight:
                    position.x = rect.xMax - textSize.x;
                    position.y = rect.yMin;
                    break;
                case TextAlign.MiddleLeft:
                    position.x = rect.xMin;
                    position.y = Mathf.RoundToInt((rect.yMin + rect.yMax - textSize.y) * 0.5f);
                    break;
                case TextAlign.MiddleCenter:
                    position.x =  Mathf.RoundToInt((rect.xMin + rect.xMax - textSize.x) * 0.5f);
                    position.y =  Mathf.RoundToInt((rect.yMin + rect.yMax - textSize.y) * 0.5f);
                    break;
                case TextAlign.MiddleRight:
                    position.x = rect.xMax - textSize.x;
                    position.y =  Mathf.RoundToInt((rect.yMin + rect.yMax - textSize.y) * 0.5f);
                    break;
                case TextAlign.DownLeft:
                    position.x = rect.xMin;
                    position.y = rect.yMax - textSize.y;
                    break;
                case TextAlign.DownCenter:
                    position.x =  Mathf.RoundToInt((rect.xMin + rect.xMax - textSize.x) * 0.5f);
                    position.y = rect.yMax - textSize.y;
                    break;
                case TextAlign.DownRight:
                    position.x = rect.xMax - textSize.x;
                    position.y = rect.yMax - textSize.y;
                    break;
            }
        }

        protected void RenderText(Vector2 position, string text)
        {
            if (null == font)
                return;

            float sx = 1.0f, sy = 1.0f;
            if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                UIManager_OLD.Instance.GetDeviceSimScaling(out sx, out sy);

            Vector2 prevTextSize = textSize;
            textSize.x = textSize.y = 0.0f;
            Vector2 p = new Vector2(position.x, position.y);

            int textLen = text.Length,
                textLast = textLen - 1,
                counter = 0,
                overlayCounter = 0,
                lineCounter = 0;

            if (multiline)
            {
                switch (align)
                {
                    case TextAlign.UpLeft:
                    case TextAlign.MiddleLeft:
                    case TextAlign.DownLeft:
                        break;
                    case TextAlign.UpCenter:
                    case TextAlign.MiddleCenter:
                    case TextAlign.DownCenter:
                        p.x = position.x +  Mathf.RoundToInt((prevTextSize.x - lineEndIndices[lineCounter].Value) * 0.5f);
                        break;
                    case TextAlign.UpRight:
                    case TextAlign.MiddleRight:
                    case TextAlign.DownRight:
                        p.x = position.x + prevTextSize.x - lineEndIndices[lineCounter].Value;
                        break;
                }
            }

            for (; counter < textLen; ++counter)
            {
                if (multiline && counter == lineEndIndices[lineCounter].Key)
                {
                    ++lineCounter;
                    if (UIManager_OLD.MultipleResolutionsBehavior.ScaleToFit == UIManager_OLD.Instance.multipleResBehavior)
                        p.y += font.lineHeight * sy;
                    else
                        p.y += font.lineHeight;

                    switch (align)
                    {
                        case TextAlign.UpLeft:
                        case TextAlign.MiddleLeft:
                        case TextAlign.DownLeft:
                            p.x = position.x;
                            break;
                        case TextAlign.UpCenter:
                        case TextAlign.MiddleCenter:
                        case TextAlign.DownCenter:
                            p.x = position.x +  Mathf.RoundToInt((prevTextSize.x - lineEndIndices[lineCounter].Value) * 0.5f);
                            break;
                        case TextAlign.UpRight:
                        case TextAlign.MiddleRight:
                        case TextAlign.DownRight:
                            p.x = position.x + prevTextSize.x - lineEndIndices[lineCounter].Value;
                            break;
                    }
                }

                if (overlayCounter >= overlays.Count)
                {
                    Overlay overlay;
                    if (counter < textLast && useKerning)
                        overlay = font.DrawCharacterWithKerning(text[counter], text[counter + 1], color, ref p, ref textSize, null);
                    else
                        overlay = font.DrawCharacter(text[counter], color, ref p, ref textSize, null);

                    if (overlay != null)
                    {
                        overlay.Transform = transform;
                        overlay.Visibility = visible;
                        //overlay.ScissorRectIndex = scissorRectIndex;
                        if (scissorRectName != null)
                            overlay.SetScissorRect(scissorRectName, scissorRect);
                        overlay.Depth = depth;
                        overlays.Add(overlay);

                        ++overlayCounter;
                    }
                }
                else
                {
                    Overlay overlay = overlays[overlayCounter];
                    if (counter < textLast && useKerning)
                        overlay = font.DrawCharacterWithKerning(text[counter], text[counter + 1], color, ref p, ref textSize, overlay);
                    else
                        overlay = font.DrawCharacter(text[counter], color, ref p, ref textSize, overlay);
                    if (overlay != null)
                    {
                        overlay.Visibility = visible;

                        ++overlayCounter;
                    }
                }
            }

            if (multiline)
                textSize = prevTextSize;

            for (int i = counter; i < overlays.Count; ++i)
                overlays[i].Visibility = false;
        }
        #endregion

        #region Public methods
        public Rect GetCharacterRect(int index)
        {
            return overlays[index].ScreenRect;
        }

        public void SetScissorRect(string name, Rect rect)
        {
            foreach (Overlay overlay in overlays)
                overlay.SetScissorRect(name, rect);

            scissorRectName = name;
            scissorRect = rect;
        }

        public void SetScissorRectWithTransform(string name, Rect _scissorRect, SBSMatrix4x4 newTransform)
        {
            foreach (Overlay overlay in overlays)
                overlay.SetScissorRectWithTransform(name, _scissorRect, newTransform);

			transform = newTransform;
            scissorRectName = name;
            scissorRect = _scissorRect;
        }

        public void RemoveScissorRect()
        {
            foreach (Overlay overlay in overlays)
                overlay.RemoveScissorRect();

            scissorRectName = null;
        }

        public string GetScissorRectName()
        {
            return scissorRectName;
        }

        public virtual void Destroy()
        {
            foreach (Overlay overlay in overlays)
                overlay.Destroy();

            overlays.Clear();
        }
        #endregion
    }
}
