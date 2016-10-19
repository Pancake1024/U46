using UnityEngine;
using SBS.Math;

namespace SBS.UI
{
    public class Overlay : UIElement
    {
        #region Protected members
        protected OverlaysBatch batch;
        protected int index0 = -1;
        //protected int index1;
        protected SBSMatrix4x4 transform;
        protected Rect scrRect;
        protected Rect imgRect;
        protected Color color;
        protected bool visible;
        //protected int scissorRectIndex;
        protected int depth;
        protected string scissorRectName;
        protected Rect scissorRect;
        #endregion

        #region Public properties
        public OverlaysBatch Batch
        {
            get
            {
                return batch;
            }
            set
            {
            	if (value == batch)
            		return;

                if (index0 > -1)
                {
                    batch.RemoveOverlay(index0);
                    index0 = -1;
                }

                batch = value;
                index0 = batch.AddOverlay(transform, scrRect, imgRect, color);

                batch.SetVisibility(index0, visible);
                batch.SetDepth(index0, depth);
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
                /*if (-1 == scissorRectIndex)
                    batch.FillVertices(index0, value, scrRect);
                else
                    batch.FillClippedVerticesAndUVs(index0, ref index1, value, scrRect, imgRect, scissorRectIndex);*/
				if (index0 > -1)
				{
	                if (null == scissorRectName)
	                    batch.FillVertices(index0, value, scrRect, batch.Layer.Camera.pixelRect);
	                else
	                    batch.FillVertices(index0, value, scrRect, scissorRect);
				}
                transform = value;
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
                /*if (-1 == scissorRectIndex)
                    batch.FillVertices(index0, transform, value);
                else
                    batch.FillClippedVerticesAndUVs(index0, ref index1, transform, value, imgRect, scissorRectIndex);*/
				if (index0 > -1)
				{
	                if (null == scissorRectName)
	                    batch.FillVertices(index0, transform, value, batch.Layer.Camera.pixelRect);
	                else
	                    batch.FillVertices(index0, transform, value, scissorRect);
				}
                scrRect = value;
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
                //if (-1 == scissorRectIndex)
				if (index0 > -1)
                    batch.FillUVs(index0, value);
                /*else
                    batch.FillClippedVerticesAndUVs(index0, ref index1, transform, scrRect, value, scissorRectIndex);*/
                imgRect = value;
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
				if (index0 > -1)
                	batch.FillColors(index0, value);
                /*if (index1 >= 0)
                    batch.FillColors(index1, value);*/
                color = value;
            }
        }
        /*
        public bool Clipped
        {
            get
            {
                bool clipped = batch.IsQuadClipped(index0);
                if (index1 >= 0)
                    clipped &= batch.IsQuadClipped(index1);
                return clipped;
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
					if (index0 > -1)
                    	batch.SetVisibility(index0, value);
                    /*if (index1 >= 0)
                        batch.SetVisibility(index1, value);*/
                    visible = value;
                }
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
                if (value != scissorRectIndex)
                {
                    scissorRectIndex = value;

                    batch.FillClippedVerticesAndUVs(index0, ref index1, transform, scrRect, imgRect, scissorRectIndex);
                }
            }
        }

        public string ScissorRectName
        {
            get
            {
                return batch.GetScissorRectName(scissorRectIndex);
            }
            set
            {
                int index = batch.GetScissorRectIndex(value);

                if (index != scissorRectIndex)
                {
                    scissorRectIndex = index;

                    batch.FillClippedVerticesAndUVs(index0, ref index1, transform, scrRect, imgRect, scissorRectIndex);
                }
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
                depth = Mathf.Max(0, value);
                /*if (index1 >= 0)
                    batch.SetDepth(index0, index1, depth);
                else*/
				if (index0 > -1)
                    batch.SetDepth(index0, depth);
            }
        }
        #endregion

        #region Ctors
        private Overlay()
        { }

        public Overlay(OverlaysBatch _batch, SBSMatrix4x4 _transform, Rect _scrRect, Rect _imgRect, Color _color)
        {
            batch = _batch;
            index0 = _batch.AddOverlay(_transform, _scrRect, _imgRect, _color);
            //index1 = -1;

            transform = _transform;
            scrRect = _scrRect;
            imgRect = _imgRect;
            color = _color;

            visible = true;

            //scissorRectIndex = -1;
            depth = _batch.BaseDepth;
            scissorRectName = null;
        }
        /*
        public Overlay(OverlaysBatch _batch, SBSMatrix4x4 _transform, Rect _scrRect, Rect _imgRect, Color _color, int _scissorRectIndex)
            : this(_batch, _transform, _scrRect, _imgRect, _color)
        {
            scissorRectIndex = _scissorRectIndex;
        }*/
        public Overlay(OverlaysBatch _batch, SBSMatrix4x4 _transform, Rect _scrRect, Rect _imgRect, Color _color, string _scissorRectName, Rect _scissorRect)
        {
            batch = _batch.Layer.AddScissorRectRef(_batch, _scissorRectName, _scissorRect);
            index0 = batch.AddOverlay(_transform, _scrRect, _imgRect, _color);
            //index1 = -1;

            transform = _transform;
            scrRect = _scrRect;
            imgRect = _imgRect;
            color = _color;

            visible = true;

            depth = batch.BaseDepth;
            scissorRectName = _scissorRectName;
            scissorRect = _scissorRect;
        }
        #endregion

        #region Public static methods
        public static SBSMatrix4x4 RotateAroundPivot(float angle, Vector2 pivot)
        {
#if UNITY_FLASH
            SBSMatrix4x4 T = SBSMatrix4x4.identity.Clone(),
                         invT = SBSMatrix4x4.identity.Clone(),
#else
            SBSMatrix4x4 T = SBSMatrix4x4.identity,
                         invT = SBSMatrix4x4.identity,
#endif
                         R = SBSMatrix4x4.TRS(SBSVector3.zero, SBSQuaternion.AngleAxis(angle, SBSVector3.forward), SBSVector3.one);
            /*
            T.SetColumn(3, new Vector4(pivot.x, pivot.y, 0.0f, 1.0f));
            invT.SetColumn(3, new Vector4(-pivot.x, -pivot.y, 0.0f, 1.0f));
            */
            T.position = new SBSVector3(pivot.x, pivot.y);
            invT.position = new SBSVector3(-pivot.x, -pivot.y);

            return T * R * invT;
        }

        public static SBSMatrix4x4 ScaleAroundPivot(Vector2 scale, Vector2 pivot)
        {
#if UNITY_FLASH
            SBSMatrix4x4 T = SBSMatrix4x4.identity.Clone(),
                         invT = SBSMatrix4x4.identity.Clone(),
#else
            SBSMatrix4x4 T = SBSMatrix4x4.identity,
                         invT = SBSMatrix4x4.identity,
#endif
                         S = SBSMatrix4x4.TRS(SBSVector3.zero, SBSQuaternion.identity, new SBSVector3(scale.x, scale.y, 1.0f));
           
            T.position = new SBSVector3(pivot.x, pivot.y);
            invT.position = new SBSVector3(-pivot.x, -pivot.y);

            return T * S * invT;
        }
        #endregion

        #region Public methods
        public void SetScissorRect(string name, Rect rect)
        {
            scissorRectName = name;
            scissorRect = rect;
            this.Batch = batch.Layer.AddScissorRectRef(batch, scissorRectName, rect);
            batch.FillVertices(index0, transform, scrRect, scissorRect);
        }

        public void SetScissorRectWithTransform(string name, Rect _scissorRect, SBSMatrix4x4 newTransform)
        {
        	transform = newTransform;
            scissorRectName = name;
            scissorRect = _scissorRect;
            this.Batch = batch.Layer.AddScissorRectRef(batch, scissorRectName, _scissorRect);
            batch.FillVertices(index0, transform, scrRect, scissorRect);
        }

        public void RemoveScissorRect()
        {
            this.Batch = batch.Layer.ReleaseScissorRectRef(batch, scissorRectName);
            scissorRectName = null;
            batch.FillVertices(index0, transform, scrRect, batch.Layer.Camera.pixelRect);
        }

        public string GetScissorRectName()
        {
            return scissorRectName;
        }

        public void Destroy()
        {
            if (scissorRectName != null)
                this.RemoveScissorRect();
            /*
            if (index1 >= 0)
                batch.RemoveOverlay(index0, index1);
            else*/
            if (index0 > -1)
            {
                batch.RemoveOverlay(index0);
                index0 = -1;
            }
        }
        #endregion
    }
}
