using System;
using UnityEngine;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSBounds
#else
    public struct SBSBounds
#endif
	{
#if UNITY_FLASH
        public SBSVector3 min = new SBSVector3();
        public SBSVector3 max = new SBSVector3();
#else
        public SBSVector3 min;
        public SBSVector3 max;
#endif

#if UNITY_FLASH
        public SBSBounds()
        {
        }
#endif

        public SBSBounds(SBSVector3 center, SBSVector3 size)
        {
            float ex = size.x * 0.5f, ey = size.y * 0.5f, ez = size.z * 0.5f;

            min.x = center.x - ex;
            min.y = center.y - ey;
            min.z = center.z - ez;

            max.x = center.x + ex;
            max.y = center.y + ey;
            max.z = center.z + ez;
        }

        public static implicit operator SBSBounds(Bounds b)
        {
            return new SBSBounds(b.center, b.size);
        }

        public static implicit operator Bounds(SBSBounds b)
        {
            return new Bounds(b.center, b.size);
        }

        public SBSVector3 center
        {
            get
            {
                return (min + max) * 0.5f;
            }
        }

        public SBSVector3 extents
        {
            get
            {
                return (max - min) * 0.5f;
            }
        }

        public SBSVector3 size
        {
            get
            {
                return (max - min);
            }
        }

        public void Reset()
        {
            min.x = float.MaxValue;
            min.y = float.MaxValue;
            min.z = float.MaxValue;

            max.x = -float.MaxValue;
            max.y = -float.MaxValue;
            max.z = -float.MaxValue;
        }

        public void Encapsulate(SBSBounds bounds)
        {
            min.x = SBSMath.Min(min.x, bounds.min.x);
            min.y = SBSMath.Min(min.y, bounds.min.y);
            min.z = SBSMath.Min(min.z, bounds.min.z);

            max.x = SBSMath.Max(max.x, bounds.max.x);
            max.y = SBSMath.Max(max.y, bounds.max.y);
            max.z = SBSMath.Max(max.z, bounds.max.z);
        }

        public void Encapsulate(SBSVector3 p)
        {
            float px = p.x, py = p.y, pz = p.z;
            min.x = SBSMath.Min(min.x, px);
            min.y = SBSMath.Min(min.y, py);
            min.z = SBSMath.Min(min.z, pz);

            max.x = SBSMath.Max(max.x, px);
            max.y = SBSMath.Max(max.y, py);
            max.z = SBSMath.Max(max.z, pz);
        }

        public void Encapsulate(float px, float py, float pz)
        {
            min.x = SBSMath.Min(min.x, px);
            min.y = SBSMath.Min(min.y, py);
            min.z = SBSMath.Min(min.z, pz);

            max.x = SBSMath.Max(max.x, px);
            max.y = SBSMath.Max(max.y, py);
            max.z = SBSMath.Max(max.z, pz);
        }

        public SBSVector3[] GetVertices()
        {
            SBSVector3[] dest = new SBSVector3[8];
            dest[0] = new SBSVector3(min.x, min.y, min.z);
            dest[1] = new SBSVector3(max.x, min.y, min.z);
            dest[2] = new SBSVector3(max.x, max.y, min.z);
            dest[3] = new SBSVector3(min.x, max.y, min.z);
            dest[4] = new SBSVector3(min.x, min.y, max.z);
            dest[5] = new SBSVector3(max.x, min.y, max.z);
            dest[6] = new SBSVector3(max.x, min.y, max.z);
            dest[7] = new SBSVector3(min.x, min.y, max.z);
            return dest;
        }

        public void Transform(SBSMatrix4x4 matrix)
        {
            int i = 0;
            SBSVector3[] vertices = this.GetVertices();

            for (; i < 8; ++i)
#if UNITY_FLASH
                matrix.MultiplyPoint3x4(vertices[i], vertices[i]);
#else
                matrix.MultiplyPoint3x4(vertices[i], out vertices[i]);
#endif

            this.Reset();
            for (i = 0; i < 8; ++i)
                this.Encapsulate(vertices[i]);
        }

        public bool Contains(SBSBounds other)
        {
            return SBSVector3.LessAll(min, other.min) && SBSVector3.GreaterEqualAll(max, other.max);
        }

        public bool Intersect(SBSBounds other)
        {
            return !(SBSVector3.LessAny(max, other.min) || SBSVector3.GreaterAny(min, other.max));
        }
#if UNITY_FLASH
        public SBSBounds Clone()
        {
            return new SBSBounds(this.center, this.size);
        }
#endif
        public override string ToString()
        {
            return "min: " + min + ", max: " + max;
        }
    }
}
