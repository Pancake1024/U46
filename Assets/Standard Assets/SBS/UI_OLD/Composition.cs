using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.UI
{
    public class Composition : UIElement
    {
        #region Protected members
        protected Rect coreRect;
        protected Rect scrRect;
        protected Rect imgRect;
        protected bool stretchOverlays;
        protected List<Overlay> overlays = new List<Overlay>();
        protected OverlaysBatch batchBackup;
        #endregion

        #region Public properties
        public OverlaysBatch Batch
        {
            get
            {
                return overlays[0].Batch;//batchBackup;
            }
            set
            {
                batchBackup = value;
                this.CreateOverlays(batchBackup, overlays[0].Transform, overlays[0].Color);
            }
        }

        public SBSMatrix4x4 Transform
        {
            get
            {
                return overlays[0].Transform;
            }
            set
            {
                this.CreateOverlays(batchBackup, value, overlays[0].Color);
            }
        }

        public Rect CoreRect
        {
            get
            {
                return coreRect;
            }
            set
            {
                coreRect = value;
                this.CreateOverlays(batchBackup, overlays[0].Transform, overlays[0].Color);
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
                scrRect = value;
                this.CreateOverlays(batchBackup, overlays[0].Transform, overlays[0].Color);
            }
        }

        public Rect ImageRect
        {
            get
            {
                return imgRect;
            }
            set
            {
                imgRect = value;
                this.CreateOverlays(batchBackup, overlays[0].Transform, overlays[0].Color);
            }
        }

        public bool StretchOverlays
        {
            get
            {
                return stretchOverlays;
            }
            set
            {
                stretchOverlays = value;
                this.CreateOverlays(batchBackup, overlays[0].Transform, overlays[0].Color);
            }
        }

        public Color Color
        {
            get
            {
                return overlays[0].Color;
            }
            set
            {
                this.CreateOverlays(batchBackup, overlays[0].Transform, value);
            }
        }
        /*
        public bool Clipped
        {
            get
            {
                foreach (Overlay overlay in overlays)
                {
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
                return overlays[0].Visibility;
            }
            set
            {
                if (overlays[0].Visibility != value)
                {
                    foreach (Overlay overlay in overlays)
                        overlay.Visibility = value;
                }
            }
        }
        /*
        public int ScissorRectIndex
        {
            get
            {
                return overlays[0].ScissorRectIndex;
            }
            set
            {
                if (overlays[0].ScissorRectIndex != value)
                {
                    foreach (Overlay overlay in overlays)
                        overlay.ScissorRectIndex = value;
                }
            }
        }

        public string ScissorRectName
        {
            get
            {
                return overlays[0].ScissorRectName;
            }
            set
            {
                int index = overlays[0].Batch.GetScissorRectIndex(value);

                if (overlays[0].ScissorRectIndex != index)
                {
                    foreach (Overlay overlay in overlays)
                        overlay.ScissorRectIndex = index;
                }
            }
        }
        */
        public int Depth
        {
            get
            {
                return overlays[0].Depth;
            }
            set
            {
                foreach (Overlay overlay in overlays)
                    overlay.Depth = value;
            }
        }
        #endregion

        #region Ctors
        private Composition()
        {
        }

        public Composition(OverlaysBatch _batch, SBSMatrix4x4 _transform, Rect _coreRect, Rect _scrRect, Rect _imgRect, bool _stretchOverlays, Color _color)
        {
            coreRect = _coreRect;
            scrRect = _scrRect;
            imgRect = _imgRect;
            stretchOverlays = _stretchOverlays;
            batchBackup = _batch;

            this.CreateOverlays(_batch, _transform, _color);
        }
        #endregion

        #region Protected methods
        protected void CreateOverlays(OverlaysBatch _batch, SBSMatrix4x4 _transform, Color _color)
        {
            bool prevVisibility = overlays.Count > 0 ? overlays[0].Visibility : true;
            int prevDepth = overlays.Count > 0 ? overlays[0].Depth : -1;
            //int prevScissorRectIdx = overlays.Count > 0 ? overlays[0].ScissorRectIndex : -1;
            string prevScissorRectName = this.GetScissorRectName();

            Rect scissorRect = Rect.MinMaxRect(0, 0, UIManager_OLD.screenWidth, UIManager_OLD.screenHeight);
            bool setScissorRect = false;

            if (prevScissorRectName != null)
            {
                OverlaysLayer layer = _batch.Layer;
                setScissorRect = layer.GetScissorRect(prevScissorRectName, ref scissorRect);
                if (setScissorRect)
                	layer.AddScissorRectRef(_batch, prevScissorRectName, scissorRect); // add a ref to prevent deletion of scissor rect
                setScissorRect = false;
            }

            // clear old overlays
            foreach (Overlay overlay in overlays)
                overlay.Destroy();
            overlays.Clear();

            // top-left
            if (coreRect.yMin > 0.0f && coreRect.xMin > 0.0f)
            {
                Rect topLeft = new Rect(imgRect.xMin, imgRect.yMin, coreRect.xMin, coreRect.yMin);
                if (setScissorRect)
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMin, scrRect.yMin, topLeft.width, topLeft.height),
                        topLeft,
                        _color,
                        prevScissorRectName,
                        scissorRect));
                else
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMin, scrRect.yMin, topLeft.width, topLeft.height),
                        topLeft,
                        _color));
            }

            // top-right
            if (coreRect.yMin > 0.0f && coreRect.xMax < imgRect.width)
            {
                Rect topRight = new Rect(imgRect.xMin + coreRect.xMax, imgRect.yMin, imgRect.width - coreRect.xMax, coreRect.yMin);
                if (setScissorRect)
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMax - topRight.width, scrRect.yMin, topRight.width, topRight.height),
                        topRight,
                        _color,
                        prevScissorRectName,
                        scissorRect));
                else
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMax - topRight.width, scrRect.yMin, topRight.width, topRight.height),
                        topRight,
                        _color));
            }

            // bottom-right
            if (coreRect.yMax < imgRect.height && coreRect.xMax < imgRect.width)
            {
                Rect bottomRight = new Rect(imgRect.xMin + coreRect.xMax, imgRect.yMin + coreRect.yMax, imgRect.width - coreRect.xMax, imgRect.height - coreRect.yMax);
                if (setScissorRect)
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMax - bottomRight.width, scrRect.yMax - bottomRight.height, bottomRight.width, bottomRight.height),
                        bottomRight,
                        _color,
                        prevScissorRectName,
                        scissorRect));
                else
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMax - bottomRight.width, scrRect.yMax - bottomRight.height, bottomRight.width, bottomRight.height),
                        bottomRight,
                        _color));
            }

            // bottom-left
            if (coreRect.yMax < imgRect.height && coreRect.xMin > 0.0f)
            {
                Rect bottomLeft = new Rect(imgRect.xMin, imgRect.yMin + coreRect.yMax, coreRect.xMin, imgRect.height - coreRect.yMax);
                if (setScissorRect)
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMin, scrRect.yMax - bottomLeft.height, bottomLeft.width, bottomLeft.height),
                        bottomLeft,
                        _color,
                        prevScissorRectName,
                        scissorRect));
                else
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        new Rect(scrRect.xMin, scrRect.yMax - bottomLeft.height, bottomLeft.width, bottomLeft.height),
                        bottomLeft,
                        _color));
            }

            // top row
            if (coreRect.yMin > 0.0f)
            {
                Rect imgTop = new Rect(imgRect.xMin + coreRect.xMin, imgRect.yMin, coreRect.width, coreRect.yMin),
                     scrTop = new Rect(scrRect.xMin + coreRect.xMin, scrRect.yMin, scrRect.width - (imgRect.width - coreRect.width), coreRect.yMin);
                if (stretchOverlays)
                {
                    if (setScissorRect)
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrTop,
                            imgTop,
                            _color,
                            prevScissorRectName,
                            scissorRect));
                    else
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrTop,
                            imgTop,
                            _color));
                }
                else
                {
                    int c = Mathf.FloorToInt(scrTop.width / imgTop.width);
                    for (int i = 0; i < c; ++i)
                    {
                        if (setScissorRect)
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrTop.xMin + imgTop.width * i, scrTop.yMin, imgTop.width, imgTop.height),
                                imgTop,
                                _color,
                                prevScissorRectName,
                                scissorRect));
                        else
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrTop.xMin + imgTop.width * i, scrTop.yMin, imgTop.width, imgTop.height),
                                imgTop,
                                _color));
                    }
                }
            }

            // bottom row
            if (coreRect.yMax < imgRect.height)
            {
                Rect imgBottom = new Rect(imgRect.xMin + coreRect.xMin, imgRect.yMax - imgRect.height + coreRect.yMax, coreRect.width, imgRect.height - coreRect.yMax),
                     scrBottom = new Rect(scrRect.xMin + coreRect.xMin, scrRect.yMax - imgBottom.height, scrRect.width - (imgRect.width - coreRect.width), imgBottom.height);
                if (stretchOverlays)
                {
                    if (setScissorRect)
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrBottom,
                            imgBottom,
                            _color,
                            prevScissorRectName,
                            scissorRect));
                    else
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrBottom,
                            imgBottom,
                            _color));
                }
                else
                {
                    int c = Mathf.FloorToInt(scrBottom.width / imgBottom.width);
                    for (int i = 0; i < c; ++i)
                    {
                        if (setScissorRect)
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrBottom.xMin + imgBottom.width * i, scrBottom.yMin, imgBottom.width, imgBottom.height),
                                imgBottom,
                                _color,
                                prevScissorRectName,
                                scissorRect));
                        else
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrBottom.xMin + imgBottom.width * i, scrBottom.yMin, imgBottom.width, imgBottom.height),
                                imgBottom,
                                _color));
                    }
                }
            }

            // left column
            if (coreRect.xMin > 0.0f)
            {
                Rect imgLeft = new Rect(imgRect.xMin, imgRect.yMin + coreRect.yMin, coreRect.xMin, coreRect.height),
                     scrLeft = new Rect(scrRect.xMin, scrRect.yMin + coreRect.yMin, coreRect.xMin, scrRect.height - (imgRect.height - coreRect.height));
                if (stretchOverlays)
                {
                    if (setScissorRect)
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrLeft,
                            imgLeft,
                            _color,
                            prevScissorRectName,
                            scissorRect));
                    else
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrLeft,
                            imgLeft,
                            _color));
                }
                else
                {
                    int c = Mathf.FloorToInt(scrLeft.height / imgLeft.height);
                    for (int i = 0; i < c; ++i)
                    {
                        if (setScissorRect)
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrLeft.xMin, scrLeft.yMin + imgLeft.height * i, imgLeft.width, imgLeft.height),
                                imgLeft,
                                _color,
                                prevScissorRectName,
                                scissorRect));
                        else
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrLeft.xMin, scrLeft.yMin + imgLeft.height * i, imgLeft.width, imgLeft.height),
                                imgLeft,
                                _color));
                    }
                }
            }

            // right column
            if (coreRect.xMax < imgRect.width)
            {
                Rect imgRight = new Rect(imgRect.xMax - imgRect.width + coreRect.xMax, imgRect.yMin + coreRect.yMin, imgRect.width - coreRect.xMax, coreRect.height),
                     scrRight = new Rect(scrRect.xMax - imgRight.width, scrRect.yMin + coreRect.yMin, imgRect.width - coreRect.xMax, scrRect.height - (imgRect.height - coreRect.height));
                if (stretchOverlays)
                {
                    if (setScissorRect)
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrRight,
                            imgRight,
                            _color,
                            prevScissorRectName,
                            scissorRect));
                    else
                        overlays.Add(new Overlay(
                            _batch,
                            _transform,
                            scrRight,
                            imgRight,
                            _color));
                }
                else
                {
                    int c = Mathf.FloorToInt(scrRight.height / imgRight.height);
                    for (int i = 0; i < c; ++i)
                    {
                        if (setScissorRect)
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrRight.xMin, scrRight.yMin + imgRight.height * i, imgRight.width, imgRight.height),
                                imgRight,
                                _color,
                                prevScissorRectName,
                                scissorRect));
                        else
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(scrRight.xMin, scrRight.yMin + imgRight.height * i, imgRight.width, imgRight.height),
                                imgRight,
                                _color));
                    }
                }
            }

            // center image
            Rect scrCenterRect = new Rect(
                scrRect.xMin + coreRect.xMin,
                scrRect.yMin + coreRect.yMin,
                scrRect.width - imgRect.width + coreRect.width,
                scrRect.height - imgRect.height + coreRect.height);
            Rect coreImgRect = new Rect(
                imgRect.xMin + coreRect.xMin,
                imgRect.yMin + coreRect.yMin,
                coreRect.width,
                coreRect.height);
            if (stretchOverlays)
            {
                if (setScissorRect)
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        scrCenterRect,
                        coreImgRect,
                        _color,
                        prevScissorRectName,
                        scissorRect));
                else
                    overlays.Add(new Overlay(
                        _batch,
                        _transform,
                        scrCenterRect,
                        coreImgRect,
                        _color));
            }
            else
            {
                int nx = Mathf.CeilToInt(scrCenterRect.width / coreRect.width),
                    ny = Mathf.CeilToInt(scrCenterRect.height / coreRect.height);

                for (int i = 0; i < nx; ++i)
                {
                    for (int j = 0; j < ny; ++j)
                    {
                        if (setScissorRect)
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(
                                    scrCenterRect.xMin + (i * coreRect.width),
                                    scrCenterRect.yMin + (j * coreRect.height),
                                    coreRect.width,
                                    coreRect.height),
                                coreImgRect,
                                _color,
                                prevScissorRectName,
                                scissorRect));
                        else
                            overlays.Add(new Overlay(
                                _batch,
                                _transform,
                                new Rect(
                                    scrCenterRect.xMin + (i * coreRect.width),
                                    scrCenterRect.yMin + (j * coreRect.height),
                                    coreRect.width,
                                    coreRect.height),
                                coreImgRect,
                                _color));
                    }
                }
            }

            this.Visibility = prevVisibility;
            if (prevDepth >= 0)
                this.Depth = prevDepth;

            //this.ScissorRectIndex = prevScissorRectIdx;
            if (prevScissorRectName != null)
            {
                //Rect scissorRect = Rect.MinMaxRect(0, 0, UIManager.screenWidth, UIManager.screenHeight);
                if (/*overlays[0].Batch*/_batch.Layer.GetScissorRect(prevScissorRectName, ref scissorRect))
                    this.SetScissorRect(prevScissorRectName, scissorRect);
            }

            if (prevScissorRectName != null)
            {
                OverlaysLayer layer = _batch.Layer;
                layer.ReleaseScissorRectRef(_batch, prevScissorRectName); // remove previously added ref
            }
        }
        #endregion

        #region Public methods
        public void SetScissorRect(string name, Rect rect)
        {
            foreach (Overlay overlay in overlays)
                overlay.SetScissorRect(name, rect);
        }

        public void SetScissorRectWithTransform(string name, Rect scissorRect, SBSMatrix4x4 newTransform)
        {
            foreach (Overlay overlay in overlays)
                overlay.SetScissorRectWithTransform(name, scissorRect, newTransform);
        }

        public void RemoveScissorRect()
        {
            foreach (Overlay overlay in overlays)
                overlay.RemoveScissorRect();
        }

        public string GetScissorRectName()
        {
            if (overlays.Count > 0)
                return overlays[0].GetScissorRectName();
            else
                return null;
        }

        public void Destroy()
        {
            foreach (Overlay o in overlays)
                o.Destroy();
            overlays.Clear();
        }
        #endregion
    }
}
