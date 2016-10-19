using System;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSPlane
#else
    public struct SBSPlane
#endif
	{
		public SBSVector3 normal;
		public float distance;

#if UNITY_FLASH
		public SBSPlane()
		{
		}
#endif

        public SBSPlane(SBSVector3 _normal, float _distance)
		{
			normal = _normal;
			distance = _distance;
		}
		
		public SBSPlane(SBSVector3 _normal, SBSVector3 point)
		{
			normal = _normal;
			distance = -SBSVector3.Dot(_normal, point);
		}
		
		public SBSPlane(SBSVector3 p0, SBSVector3 p1, SBSVector3 p2)
		{
			normal = SBSVector3.Cross(p1 - p0, p2 - p0);
			distance = -SBSVector3.Dot(normal, p0);
		}
		
		public void SetNormalAndPoint(SBSVector3 _normal, SBSVector3 point)
		{
			normal = _normal;
			distance = -SBSVector3.Dot(_normal, point);
		}
		
		public void Set3Points(SBSVector3 p0, SBSVector3 p1, SBSVector3 p2)
		{
			normal = SBSVector3.Cross(p1 - p0, p2 - p0);
			distance = -SBSVector3.Dot(normal, p0);
		}
		
		public float GetDistanceToPoint(SBSVector3 p)
		{
			return SBSVector3.Dot(normal, p) + distance;
		}
		
		public SBSVector3 ReflectPoint(SBSVector3 p)
		{
			return p - (2.0f * (SBSVector3.Dot(normal, p) + distance)) * normal;
		}

		public void ReflectPoint(SBSVector3 p, SBSVector3 o)
		{
			float _2d = 2.0f * (SBSVector3.Dot(normal, p) + distance);
			o.x = p.x - _2d * normal.x;
			o.y = p.y - _2d * normal.y;
			o.z = p.z - _2d * normal.z;
		}

		public SBSVector3 ReflectVector(SBSVector3 v)
		{
			return v - (2.0f * SBSVector3.Dot(normal, v)) * normal;
		}
		
		public void ReflectVector(SBSVector3 v, SBSVector3 o)
		{
			float _2d = 2.0f * SBSVector3.Dot(normal, v);
			o.x = v.x - _2d * normal.x;
			o.y = v.y - _2d * normal.y;
			o.z = v.z - _2d * normal.z;
		}
		
		public SBSVector3 ProjectPoint(SBSVector3 p)
		{
			return p - (SBSVector3.Dot(normal, p) + distance) * normal;
		}
		
		public void ProjectPoint(SBSVector3 p, SBSVector3 o)
		{
			float d = SBSVector3.Dot(normal, p) + distance;
			o.x = p.x - d * normal.x;
			o.y = p.y - d * normal.y;
			o.z = p.z - d * normal.z;
		}
		
		public SBSVector3 ProjectVector(SBSVector3 v)
		{
			return v - SBSVector3.Dot(normal, v) * normal;
		}
		
		public void ProjectVector(SBSVector3 v, SBSVector3 o)
		{
			float d = SBSVector3.Dot(normal, v);
			o.x = v.x - d * normal.x;
			o.y = v.y - d * normal.y;
			o.z = v.z - d * normal.z;
		}

		public void Normalize()
		{
			float m = normal.Normalize();
			if (m < SBSMath.Epsilon)
				return;
			distance /= m;
		}

        override public string ToString()
        {
            return "( " + normal.x + ", " + normal.y + ", " + normal.z + ", " + distance + ")";
        }
    }
}

