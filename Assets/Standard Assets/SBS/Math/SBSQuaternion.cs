using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSQuaternion
#else
    public struct SBSQuaternion
#endif
	{
		public static SBSQuaternion identity = Quaternion.identity;
		
		public float x;
		public float y;
		public float z;
		public float w;
		
		public SBSQuaternion inverse
		{
			get
			{
				return new SBSQuaternion(-x, -y, -z, w);
			}
		}
		
		public SBSQuaternion normalized
		{
			get {
#if UNITY_FLASH
				SBSQuaternion r = this.Clone();
				r.Normalize();
				return r;
#else
                float oom = x * x + y * y + z * z + w * w;
                if (oom < SBSMath.SqrEpsilon)
                    return new SBSQuaternion(0.0f, 0.0f, 0.0f, 1.0f);

                oom = 1.0f / SBSMath.Sqrt(oom);
                return new SBSQuaternion(x * oom, y * oom, z * oom, w * oom);
#endif
			}
		}
		
		public SBSAngleAxis angleAxis
		{
			get {
				SBSQuaternion _q = this;

				if (w > 1.0f)
					_q = this.normalized;

				float a = 2.0f * SBSMath.Acos(_q.w),
					  s = SBSMath.Sqrt(1.0f - _q.w * _q.w),
					  x, y, z;
				if (s < SBSMath.Epsilon) {
					x = _q.x;
					y = _q.y;
					z = _q.z;
				} else {
					float oos = 1.0f / s;

					x = _q.x * oos;
					y = _q.y * oos;
					z = _q.z * oos;
				}

				return new SBSAngleAxis(a * SBSMath.ToDegrees, x, y, z);
			}
			set {
				this.SetAngleAxis(value.angle, value.axis);
			}
		}
		
#if UNITY_FLASH
        public SBSQuaternion()
		{
		}
#endif

        public SBSQuaternion(float _x, float _y, float _z, float _w)
		{
			x = _x;
			y = _y;
			z = _z;
			w = _w;
		}

        public static SBSQuaternion Integrate(SBSQuaternion rotation, SBSVector3 angularVelocity, float dt)
        {
            SBSQuaternion spin = new SBSQuaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0.0f) * rotation;
            spin.ScaleBy(0.5f * dt);
            SBSQuaternion output = rotation + spin;
            output.Normalize();
            return output;
        }

		public static SBSQuaternion AngleAxis(float angle, SBSVector3 axis)
		{
			angle *= (SBSMath.ToRadians * 0.5f);
			SBSVector3 a = axis.normalized;
			float s = SBSMath.Sin(angle),
				  c = SBSMath.Cos(angle);
			return new SBSQuaternion(a.x * s, a.y * s, a.z * s, c);
		}

        public static SBSQuaternion LookRotation(SBSVector3 forward, SBSVector3 up)
        {
#if UNITY_FLASH
            forward = forward.normalized;
#else
            forward.Normalize();
#endif
            SBSVector3 right = SBSVector3.Cross(up.normalized, forward).normalized;
            up = SBSVector3.Cross(forward, right);

            float w    = SBSMath.Sqrt(1.0f + right.x + up.y + forward.z) * 0.5f,
                  oo4w = 1.0f / (4.0f * w),
                  x    = (forward.y - up.z) * oo4w,
                  y    = (right.z - forward.x) * oo4w,
                  z    = (up.x - right.y) * oo4w;

            return new SBSQuaternion(x, y, z, w);
        }

        public static SBSQuaternion LookRotation(SBSVector3 forward)
        {
            return LookRotation(forward, SBSVector3.up);
        }

        public static SBSQuaternion Slerp(SBSQuaternion q0, SBSQuaternion q1, float t)
		{
			float w1 = q0.w, x1 = q0.x, y1 = q0.y, z1 = q0.z,
				  w2 = q1.w, x2 = q1.x, y2 = q1.y, z2 = q1.z,
				  dot = w1 * w2 + x1 * x2 + y1 * y2 + z1 * z2;

			if (dot < 0.0f) {
				dot = -dot;
				w2 = -w2;
				x2 = -x2;
				y2 = -y2;
				z2 = -z2;
			}
			
			if (dot < 0.95f) {
				float angle = SBSMath.Acos(dot),
					  s     = 1.0f / SBSMath.Sin(angle),
					  s1    = SBSMath.Sin(angle * (1.0f - t)) * s,
					  s2    = SBSMath.Sin(angle * t) * s;

				w1 = w1 * s1 + w2 * s2;
				x1 = x1 * s1 + x2 * s2;
				y1 = y1 * s1 + y2 * s2;
				z1 = z1 * s1 + z2 * s2;
			} else {
				w1 += t * (w2 - w1);
				x1 += t * (x2 - x1);
				y1 += t * (y2 - y1);
				z1 += t * (z2 - z1);

				float oom = x1 * x1 + y1 * y1 + z1 * z1 + w1 * w1;
				if (oom < SBSMath.SqrEpsilon)
					return new SBSQuaternion(0.0f, 0.0f, 0.0f, 1.0f);

				oom = 1.0f / SBSMath.Sqrt(oom);

				x1 *= oom;
		        y1 *= oom;
		        z1 *= oom;
		        w1 *= oom;
			}

			return new SBSQuaternion(x1, y1, z1, w1);
		}
#if UNITY_FLASH
		public static void Slerp(SBSQuaternion q0, SBSQuaternion q1, float t, SBSQuaternion o)
#else
        public static void Slerp(SBSQuaternion q0, SBSQuaternion q1, float t, out SBSQuaternion o)
#endif
		{
			float w1 = q0.w, x1 = q0.x, y1 = q0.y, z1 = q0.z,
				  w2 = q1.w, x2 = q1.x, y2 = q1.y, z2 = q1.z,
				  dot = w1 * w2 + x1 * x2 + y1 * y2 + z1 * z2;

			if (dot < 0.0f) {
				dot = -dot;
				w2 = -w2;
				x2 = -x2;
				y2 = -y2;
				z2 = -z2;
			}
			
			if (dot < 0.95f) {
				float angle = SBSMath.Acos(dot),
					  s     = 1.0f / SBSMath.Sin(angle),
					  s1    = SBSMath.Sin(angle * (1.0f - t)) * s,
					  s2    = SBSMath.Sin(angle * t) * s;

				o.w = w1 * s1 + w2 * s2;
				o.x = x1 * s1 + x2 * s2;
				o.y = y1 * s1 + y2 * s2;
				o.z = z1 * s1 + z2 * s2;
			} else {
				o.w = w1 + t * (w2 - w1);
				o.x = x1 + t * (x2 - x1);
				o.y = y1 + t * (y2 - y1);
				o.z = z1 + t * (z2 - z1);

				o.Normalize();
			}
		}
				
		public static implicit operator SBSQuaternion(Quaternion q)
        {
            return new SBSQuaternion(q.x, q.y, q.z, q.w);
        }
		
		public static implicit operator Quaternion(SBSQuaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

		public static SBSQuaternion operator +(SBSQuaternion q0, SBSQuaternion q1)
		{
			return new SBSQuaternion(q0.x + q1.x, q0.y + q1.y, q0.z + q1.z, q0.w + q1.w);
		}
		
		public static SBSQuaternion operator -(SBSQuaternion q0, SBSQuaternion q1)
		{
			return new SBSQuaternion(q0.x - q1.x, q0.y - q1.y, q0.z - q1.z, q0.w - q1.w);
		}
		
		public static SBSQuaternion operator -(SBSQuaternion q)
		{
			return new SBSQuaternion(-q.x, -q.y, -q.z, -q.w);
		}
		
        public static SBSQuaternion operator *(SBSQuaternion q, float s)
        {
            return new SBSQuaternion(q.x * s, q.y * s, q.z * s, q.w * s);
        }

        public static SBSQuaternion operator *(float s, SBSQuaternion q)
        {
            return new SBSQuaternion(q.x * s, q.y * s, q.z * s, q.w * s);
        }
        
        public static SBSQuaternion operator /(SBSQuaternion q, float s)
        {
			float oos = 1.0f / s;
            return new SBSQuaternion(q.x * oos, q.y * oos, q.z * oos, q.w * oos);
        }
		
		public static SBSQuaternion operator *(SBSQuaternion q0, SBSQuaternion q1)
		{
			return new SBSQuaternion(
				q0.w * q1.x + q0.x * q1.w + q0.y * q1.z - q0.z * q1.y,
				q0.w * q1.y - q0.x * q1.z + q0.y * q1.w + q0.z * q1.x,
				q0.w * q1.z + q0.x * q1.y - q0.y * q1.x + q0.z * q1.w,
				q0.w * q1.w - q0.x * q1.x - q0.y * q1.y - q0.z * q1.z);
		}
		
        public static SBSVector3 operator *(SBSQuaternion q, SBSVector3 v)
        {
			float w1 = -q.x * v.x - q.y * v.y - q.z * v.z;
			float x1 =  q.w * v.x + q.y * v.z - q.z * v.y;
			float y1 =  q.w * v.y - q.x * v.z + q.z * v.x;
			float z1 =  q.w * v.z + q.x * v.y - q.y * v.x;

			return new SBSVector3(
				-w1 * q.x + x1 * q.w - y1 * q.z + z1 * q.y,
				-w1 * q.y + x1 * q.z + y1 * q.w - z1 * q.x,
				-w1 * q.z - x1 * q.y + y1 * q.x + z1 * q.w);
        }
		
#if UNITY_FLASH
		public void Rotate(SBSVector3 v, SBSVector3 o)
#else
        public void Rotate(SBSVector3 v, ref SBSVector3 o)
#endif
		{
			float w1 = -x * v.x - y * v.y - z * v.z;
			float x1 =  w * v.x + y * v.z - z * v.y;
			float y1 =  w * v.y - x * v.z + z * v.x;
			float z1 =  w * v.z + x * v.y - y * v.x;

			o.x = -w1 * x + x1 * w - y1 * z + z1 * y;
			o.y = -w1 * y + x1 * z + y1 * w - z1 * x;
			o.z = -w1 * z - x1 * y + y1 * x + z1 * w;
		}
		
		public void Set(float _x, float _y, float _z, float _w)
		{
			x = _x;
			y = _y;
			z = _z;
			w = _w;
		}
		
		public void SetAngleAxis(float angle, SBSVector3 axis)
		{
			angle *= (SBSMath.ToRadians * 0.5f);
			SBSVector3 a = axis.normalized;
			float s = SBSMath.Sin(angle),
				  c = SBSMath.Cos(angle);
			x = a.x * s;
			y = a.y * s;
			z = a.z * s;
			w = c;
		}
		
		public void Normalize()
		{
			float oom = x * x + y * y + z * z + w * w;
			if (oom < SBSMath.SqrEpsilon) {
				x = 0.0f;
				y = 0.0f;
				z = 0.0f;
				w = 1.0f;
				return;
			}
	        oom = 1.0f / SBSMath.Sqrt(oom);
	        x *= oom;
	        y *= oom;
	        z *= oom;
	        w *= oom;
		}
		
		public void Invert()
		{
			x = -x;
			y = -y;
			z = -z;
		}

		public void Negate()
        {
            x = -x;
            y = -y;
            z = -z;
			w = -w;
        }

		public void ScaleBy(float s)
        {
            x *= s;
            y *= s;
            z *= s;
			w *= s;
        }

        public void IncrementBy(SBSQuaternion q0)
        {
            x += q0.x;
            y += q0.y;
            z += q0.z;
            w += q0.w;
        }

        public void DecrementBy(SBSQuaternion q0)
        {
            x -= q0.x;
            y -= q0.y;
            z -= q0.z;
            w -= q0.w;
        }
#if UNITY_FLASH
        public SBSQuaternion Clone()
        {
            return new SBSQuaternion(x, y, z, w);
        }
#endif
		override public string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ", " + w + ")";
		}
	}
}

