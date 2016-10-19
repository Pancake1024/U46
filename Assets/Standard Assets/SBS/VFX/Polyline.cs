using System;
using UnityEngine;
using SBS.Core;
using SBS.Math;

namespace SBS.VFX
{
	public class Polyline
	{
        #region Point struct
#if UNITY_FLASH
        public class Point
#else
        public struct Point
#endif
        {
            public SBSVector3 point;
            public SBSVector3 tangent;
            public SBSVector3 normal;
            public float width;
            public float u0, u1;
            public float v;
            public bool isFirst;
        }
        #endregion

        #region Protected members
        protected TimeSource timeSource = null;
        protected Point[] points = null;
        protected Mesh mesh = null;

        protected Vector3[] vertexBuffer;
        protected Vector3[] normalBuffer;
        protected Vector2[] uvBuffer;

        protected int maxIndex;
        protected int backIndex, count;
        protected float lastPointTimeStamp = 0.0f;
        protected SBSVector3 lastPointPosition = Vector3.zero;
        #endregion

        #region Public members
        public float minDistBetweenPoints = 0.0f;
        public float minTimeBetweenPoints = 0.0f;
        #endregion

        #region Public properties
        public Point this[int index]
        {
            get
            {
                Asserts.Assert(index >= 0 && index < count);
                return points[(backIndex - count + index + 1 + points.Length) % points.Length];
            }
            set
            {
                Asserts.Assert(index >= 0 && index < count);

                int curIndex = (backIndex - count + index + 1 + points.Length) % points.Length,
                    prevIndex = (0 == curIndex ? maxIndex : curIndex - 1),
                    nextIndex = (maxIndex == curIndex ? 0 : curIndex + 1);

                points[curIndex] = value;

                if (index > 0 && !value.isFirst)
                {
                    Asserts.Assert(prevIndex < maxIndex);
                    this.FillQuad(prevIndex, points[prevIndex], value);
                }

                if (index < count - 1)
                {
                    Asserts.Assert(curIndex < maxIndex);
                    this.FillQuad(curIndex, value, points[nextIndex]);
                }
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public Point Front
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }

        public Point Back
        {
            get
            {
                return this[count - 1];
            }
            set
            {
                this[count - 1] = value;
            }
        }

        public float Length
        {
            get
            {
                float l = 0.0f;
                for (int i = 0; i < count - 1; i++)
                {
                    l += (this[i + 1].point - this[i].point).magnitude;
                }
                return l;
            }
        }

        public Mesh Mesh
        {
            get
            {
                return mesh;
            }
        }
        #endregion

        #region Ctors
        private Polyline()
        {
        }

        public Polyline(string meshName, int numPoints, Point startPoint, TimeSource _timeSource)
        {
            Asserts.Assert(numPoints >= 2);

            timeSource = (null == _timeSource ? TimeManager.Instance.MasterSource : _timeSource);

            points = new Point[numPoints];
            maxIndex = points.Length - 1;

            mesh = new Mesh();
            mesh.name = meshName;

            int numQuads = points.Length - 1;
            int[] indexBuffer = new int[numQuads * 6];
            for (int i = 0; i < numQuads; ++i)
            {
                indexBuffer[(i * 6) + 0] = (i * 4) + 0;
                indexBuffer[(i * 6) + 1] = (i * 4) + 1;
                indexBuffer[(i * 6) + 2] = (i * 4) + 2;

                indexBuffer[(i * 6) + 3] = (i * 4) + 2;
                indexBuffer[(i * 6) + 4] = (i * 4) + 3;
                indexBuffer[(i * 6) + 5] = (i * 4) + 0;
            }

            this.Reset(startPoint);

            mesh.triangles = indexBuffer;
        }
        #endregion

        #region Protected methods
        protected void FillQuad(int quadIndex, Point curPt, Point nextPt)
        {
            SBSVector3 curBiNormal = SBSVector3.Cross(curPt.tangent, curPt.normal).normalized,
                       nextBiNormal = SBSVector3.Cross(nextPt.tangent, nextPt.normal).normalized;

            vertexBuffer[(quadIndex * 4) + 0] = curPt.point + curBiNormal * curPt.width * 0.5f;
            vertexBuffer[(quadIndex * 4) + 1] = nextPt.point + nextBiNormal * nextPt.width * 0.5f;
            vertexBuffer[(quadIndex * 4) + 2] = nextPt.point - nextBiNormal * nextPt.width * 0.5f;
            vertexBuffer[(quadIndex * 4) + 3] = curPt.point - curBiNormal * curPt.width * 0.5f;

            normalBuffer[(quadIndex * 4) + 0] = curPt.normal;
            normalBuffer[(quadIndex * 4) + 1] = nextPt.normal;
            normalBuffer[(quadIndex * 4) + 2] = nextPt.normal;
            normalBuffer[(quadIndex * 4) + 3] = curPt.normal;

            uvBuffer[(quadIndex * 4) + 0] = new Vector2(curPt.u0, curPt.v);
            uvBuffer[(quadIndex * 4) + 1] = new Vector2(nextPt.u0, nextPt.v);
            uvBuffer[(quadIndex * 4) + 2] = new Vector2(nextPt.u1, nextPt.v);
            uvBuffer[(quadIndex * 4) + 3] = new Vector2(curPt.u1, curPt.v);
        }

        protected void PushBack(Point newPoint)
        {
            int prevPointIndex = backIndex;

            if (maxIndex == prevPointIndex)
            {
                points[0] = points[prevPointIndex];
                prevPointIndex = 0;
                backIndex = 0;
            }

            backIndex = (maxIndex == backIndex ? 0 : backIndex + 1);
            points[backIndex] = newPoint;

            if (!newPoint.isFirst)
                this.FillQuad(prevPointIndex, points[prevPointIndex], newPoint);

            count = Mathf.Min(points.Length, count + 1);
        }
        #endregion

        #region Public methods
        public void Reset(Point startPoint)
        {
            int i;
            for (i = 0; i < points.Length; ++i)
                points[i] = startPoint;

            int numQuads = points.Length - 1,
                numVertices = numQuads * 4;

            SBSVector3 biNormal = SBSVector3.Cross(startPoint.tangent, startPoint.normal).normalized;

            vertexBuffer = new Vector3[numVertices];
            normalBuffer = new Vector3[numVertices];
            uvBuffer = new Vector2[numVertices];

            for (i = 0; i < numQuads; ++i)
            {
                vertexBuffer[(i * 4) + 0] = startPoint.point + biNormal * startPoint.width * 0.5f;
                vertexBuffer[(i * 4) + 1] = startPoint.point + biNormal * startPoint.width * 0.5f;
                vertexBuffer[(i * 4) + 2] = startPoint.point - biNormal * startPoint.width * 0.5f;
                vertexBuffer[(i * 4) + 3] = startPoint.point - biNormal * startPoint.width * 0.5f;

                normalBuffer[(i * 4) + 0] = startPoint.normal;
                normalBuffer[(i * 4) + 1] = startPoint.normal;
                normalBuffer[(i * 4) + 2] = startPoint.normal;
                normalBuffer[(i * 4) + 3] = startPoint.normal;

                uvBuffer[(i * 4) + 0] = new Vector2(startPoint.u0, startPoint.v);
                uvBuffer[(i * 4) + 1] = new Vector2(startPoint.u0, startPoint.v);
                uvBuffer[(i * 4) + 2] = new Vector2(startPoint.u1, startPoint.v);
                uvBuffer[(i * 4) + 3] = new Vector2(startPoint.u1, startPoint.v);
            }

            mesh.vertices = vertexBuffer;
            mesh.normals = normalBuffer;
            mesh.uv = uvBuffer;

            backIndex = 1;
            count = 1;
        }

        public bool AddPoint(Point newPoint)
        {
            float now = timeSource.TotalTime,
                  diff = now - lastPointTimeStamp;

            if (!newPoint.isFirst)
            {
                if (minDistBetweenPoints > 0.0f && SBSVector3.Distance(lastPointPosition, newPoint.point) < minDistBetweenPoints)
                {
                    return false;
                }

                if (minTimeBetweenPoints > 0.0f && diff < minTimeBetweenPoints)
                {
                    return false;
                }
            }

            this.PushBack(newPoint);

            lastPointTimeStamp = now - SBSMath.Min(minTimeBetweenPoints, diff - minTimeBetweenPoints);
            lastPointPosition = newPoint.point;
            return true;
        }

        public void Flush()
        {
            mesh.vertices = vertexBuffer;
            mesh.normals = normalBuffer;
            mesh.uv = uvBuffer;

            mesh.RecalculateBounds();
        }
        #endregion
    }
}
