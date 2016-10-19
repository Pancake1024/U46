using System;
using System.Globalization;
using UnityEngine;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSVector3
#else
    public struct SBSVector3
#endif
    {
        public static SBSVector3 zero = new SBSVector3(0.0f, 0.0f, 0.0f);
        public static SBSVector3 one = new SBSVector3(1.0f, 1.0f, 1.0f);
        public static SBSVector3 right = new SBSVector3(1.0f, 0.0f, 0.0f);
        public static SBSVector3 left = new SBSVector3(-1.0f, 0.0f, 0.0f);
        public static SBSVector3 up = new SBSVector3(0.0f, 1.0f, 0.0f);
        public static SBSVector3 down = new SBSVector3(0.0f, -1.0f, 0.0f);
        public static SBSVector3 forward = new SBSVector3(0.0f, 0.0f, 1.0f);
        public static SBSVector3 back = new SBSVector3(0.0f, 0.0f, -1.0f);

		public float x;
		public float y;
		public float z;
		
		public float magnitude
		{
			get {
				return SBSMath.Sqrt(x * x + y * y + z * z);
			}
		}
		
		public float sqrMagnitude
		{
			get {
				return x * x + y * y + z * z;
			}
		}
		
		public SBSVector3 normalized
		{
			get {
				SBSVector3 r = new SBSVector3(x, y, z);
				r.Normalize();
				return r;
			}
		}

#if UNITY_FLASH
		public SBSVector3()
        {
        }
#endif

        public SBSVector3(float _x, float _y)
        {
			x = _x;
			y = _y;
			z = 0.0f;
        }

        public SBSVector3(float _x, float _y, float _z)
        {
			x = _x;
			y = _y;
			z = _z;
        }

        public static implicit operator SBSVector3(Vector3 v)
        {
            return new SBSVector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(SBSVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

		public static SBSVector3 operator +(SBSVector3 v0, SBSVector3 v1)
        {
			return new SBSVector3(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
        }

        public static SBSVector3 operator -(SBSVector3 v0, SBSVector3 v1)
        {
			return new SBSVector3(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
        }

        public static SBSVector3 operator -(SBSVector3 v)
        {
            return new SBSVector3(-v.x, -v.y, -v.z);
        }

        public static SBSVector3 operator *(SBSVector3 v, float s)
        {
            return new SBSVector3(v.x * s, v.y * s, v.z * s);
        }

        public static SBSVector3 operator *(float s, SBSVector3 v)
        {
            return new SBSVector3(v.x * s, v.y * s, v.z * s);
        }
        
        public static SBSVector3 operator /(SBSVector3 v, float s)
        {
			float oos = 1.0f / s;
            return new SBSVector3(v.x * oos, v.y * oos, v.z * oos);
        }

		public static float Dot(SBSVector3 v0, SBSVector3 v1)
        {
            return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
        }

        public static SBSVector3 Cross(SBSVector3 v0, SBSVector3 v1)
        {
            return new SBSVector3(
				v0.y * v1.z - v1.y * v0.z,
				v0.z * v1.x - v1.z * v0.x,
				v0.x * v1.y - v1.x * v0.y);
        }

        public static float Angle(SBSVector3 v0, SBSVector3 v1)
        {
			return SBSMath.Acos(SBSVector3.Dot(v0.normalized, v1.normalized)) * SBSMath.ToDegrees;
        }

        public static float Distance(SBSVector3 v0, SBSVector3 v1)
        {
			float dx = v1.x - v0.x;
			float dy = v1.y - v0.y;
			float dz = v1.z - v0.z;
			float dd = dx * dx + dy * dy + dz * dz;
            return SBSMath.Sqrt(dd);
        }
		
		public static float SqrDistance(SBSVector3 v0, SBSVector3 v1)
		{
			float dx = v1.x - v0.x;
			float dy = v1.y - v0.y;
			float dz = v1.z - v0.z;
			return dx * dx + dy * dy + dz * dz;
		}

        public static SBSVector3 Parse(string str)
        {
            char[] delimeters = { ',' };
            string[] parts = str.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);

            float x = parts.Length > 0 ? float.Parse(parts[0], CultureInfo.InvariantCulture) : 0.0f;
            float y = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0.0f;
            float z = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 0.0f;

            return new SBSVector3(x, y, z);
        }

        public static SBSVector3 Rotate(SBSVector3 from, SBSVector3 to, float t)
        {
            SBSVector3 a = SBSVector3.Cross(from, to);
            if (a.sqrMagnitude > 1.0e-3f)
                return SBSQuaternion.AngleAxis(SBSVector3.Angle(from, to) * t, a) * from;
            else
                return to;
        }

        public static SBSVector3 Lerp(SBSVector3 from, SBSVector3 to, float t)
        {
            t = Mathf.Clamp01(t);
            float s = 1.0f - t;
            return new SBSVector3(from.x * s + to.x * t, from.y * s + to.y * t, from.z * s + to.z * t);
        }

        public static SBSVector3 RandomDirection(SBSVector3 direction, float angle)
        {
            SBSVector3 d = direction.normalized;
            SBSVector3 t = SBSVector3.Cross(d + (SBSVector3)UnityEngine.Random.onUnitSphere, d).normalized;
            return SBSQuaternion.AngleAxis(UnityEngine.Random.value * angle, t) * d;
        }

        public static bool LessEqualAll(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x <= v2.x && v1.y <= v2.y && v1.z <= v2.z;
        }

        public static bool LessAll(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x < v2.x && v1.y < v2.y && v1.z < v2.z;
        }

        public static bool LessEqualAny(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x <= v2.x || v1.y <= v2.y || v1.z <= v2.z;
        }

        public static bool LessAny(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x < v2.x || v1.y < v2.y || v1.z < v2.z;
        }

        public static bool GreaterEqualAll(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x >= v2.x && v1.y >= v2.y && v1.z >= v2.z;
        }

        public static bool GreaterAll(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x > v2.x && v1.y > v2.y && v1.z > v2.z;
        }

        public static bool GreaterEqualAny(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x >= v2.x || v1.y >= v2.y || v1.z >= v2.z;
        }

        public static bool GreaterAny(SBSVector3 v1, SBSVector3 v2)
        {
            return v1.x > v2.x || v1.y > v2.y || v1.z > v2.z;
        }

        public void Set(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public void Negate()
        {
            x = -x;
            y = -y;
            z = -z;
        }
		
		public float Normalize()
		{
			float m = x * x + y * y + z * z;

			if (m < SBSMath.SqrEpsilon)
				return 0.0f;
			
			m = SBSMath.Sqrt(m);
			float oom = 1.0f / m;

			x *= oom;
			y *= oom;
			z *= oom;

			return m;
		}
		
		public void ScaleBy(float s)
        {
            x *= s;
            y *= s;
            z *= s;
        }

        public void IncrementBy(SBSVector3 v0)
        {
            x += v0.x;
            y += v0.y;
            z += v0.z;
        }

        public void DecrementBy(SBSVector3 v0)
        {
            x -= v0.x;
            y -= v0.y;
            z -= v0.z;
        }
#if UNITY_FLASH
        public SBSVector3 Clone()
        {
            return new SBSVector3(x, y, z);
        }
#endif		
		override public string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ")";
		}
	}
}
