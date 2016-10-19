using System;
using UnityEngine;

namespace SBS.Math
{
#if UNITY_FLASH
	public class SBSMatrix4x4
#else
    public struct SBSMatrix4x4
#endif
	{
		public static SBSMatrix4x4 identity = Matrix4x4.identity;

        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;

        public SBSMatrix4x4 transposed
		{
			get {
				return new SBSMatrix4x4(
					m00, m10, m20, m30,
					m01, m11, m21, m31,
					m02, m12, m22, m32,
					m03, m13, m23, m33);
			}
		}
		
		public SBSMatrix4x4 inverse
		{
			get {
				SBSMatrix4x4 r = new SBSMatrix4x4();
				float ood = this.determinant;
				if (SBSMath.Abs(ood) < SBSMath.Epsilon)
					return r;
				ood = 1.0f / ood;

				r.m00 = ood * (m11 * (m22 * m33 - m32 * m23) + m21 * (m32 * m13 - m12 * m33) + m31 * (m12 * m23 - m22 * m13));
				r.m10 = ood * (m12 * (m20 * m33 - m30 * m23) + m22 * (m30 * m13 - m10 * m33) + m32 * (m10 * m23 - m20 * m13));
				r.m20 = ood * (m13 * (m20 * m31 - m30 * m21) + m23 * (m30 * m11 - m10 * m31) + m33 * (m10 * m21 - m20 * m11));
				r.m30 = ood * (m10 * (m31 * m22 - m21 * m32) + m20 * (m11 * m32 - m31 * m12) + m30 * (m21 * m12 - m11 * m22));
				r.m01 = ood * (m21 * (m02 * m33 - m32 * m03) + m31 * (m22 * m03 - m02 * m23) + m01 * (m32 * m23 - m22 * m33));
				r.m11 = ood * (m22 * (m00 * m33 - m30 * m03) + m32 * (m20 * m03 - m00 * m23) + m02 * (m30 * m23 - m20 * m33));
				r.m21 = ood * (m23 * (m00 * m31 - m30 * m01) + m33 * (m20 * m01 - m00 * m21) + m03 * (m30 * m21 - m20 * m31));
				r.m31 = ood * (m20 * (m31 * m02 - m01 * m32) + m30 * (m01 * m22 - m21 * m02) + m00 * (m21 * m32 - m31 * m22));
				r.m02 = ood * (m31 * (m02 * m13 - m12 * m03) + m01 * (m12 * m33 - m32 * m13) + m11 * (m32 * m03 - m02 * m33));
				r.m12 = ood * (m32 * (m00 * m13 - m10 * m03) + m02 * (m10 * m33 - m30 * m13) + m12 * (m30 * m03 - m00 * m33));
				r.m22 = ood * (m33 * (m00 * m11 - m10 * m01) + m03 * (m10 * m31 - m30 * m11) + m13 * (m30 * m01 - m00 * m31));
				r.m32 = ood * (m30 * (m11 * m02 - m01 * m12) + m00 * (m31 * m12 - m11 * m32) + m10 * (m01 * m32 - m31 * m02));
				r.m03 = ood * (m01 * (m22 * m13 - m12 * m23) + m11 * (m02 * m23 - m22 * m03) + m21 * (m12 * m03 - m02 * m13));
				r.m13 = ood * (m02 * (m20 * m13 - m10 * m23) + m12 * (m00 * m23 - m20 * m03) + m22 * (m10 * m03 - m00 * m13));
				r.m23 = ood * (m03 * (m20 * m11 - m10 * m21) + m13 * (m00 * m21 - m20 * m01) + m23 * (m10 * m01 - m00 * m11));
				r.m33 = ood * (m00 * (m11 * m22 - m21 * m12) + m10 * (m21 * m02 - m01 * m22) + m20 * (m01 * m12 - m11 * m02));

				return r;
			}
		}
		
		public SBSMatrix4x4 inverseFast
		{
			get {
				SBSMatrix4x4 r = new SBSMatrix4x4();
				float ood = this.determinant;
				if (SBSMath.Abs(ood) < SBSMath.Epsilon)
					return r;
				ood = 1.0f / ood;

				r.m00 = ood * (m11 * m22 - m12 * m21);
				r.m01 = ood * (m21 * m02 - m22 * m01);
				r.m02 = ood * (m01 * m12 - m02 * m11);
				r.m03 = 0.0f;
				r.m10 = ood * (m12 * m20 - m10 * m22);
				r.m11 = ood * (m22 * m00 - m20 * m02);
				r.m12 = ood * (m02 * m10 - m00 * m12);
				r.m13 = 0.0f;
				r.m20 = ood * (m10 * m21 - m11 * m20);
				r.m21 = ood * (m20 * m01 - m21 * m00);
				r.m22 = ood * (m00 * m11 - m01 * m10);
				r.m23 = 0.0f;
				r.m30 = ood * (m10 * (m22 * m31 - m21 * m32) + m11 * (m20 * m32 - m22 * m30) + m12 * (m21 * m30 - m20 * m31));
				r.m31 = ood * (m20 * (m02 * m31 - m01 * m32) + m21 * (m00 * m32 - m02 * m30) + m22 * (m01 * m30 - m00 * m31));
				r.m32 = ood * (m30 * (m02 * m11 - m01 * m12) + m31 * (m00 * m12 - m02 * m10) + m32 * (m01 * m10 - m00 * m11));
				r.m33 = 1.0f;

				return r;
			}
		}

		public float determinant
		{
			get {
				return ((m00 * m11 - m10 * m01) * (m22 * m33 - m32 * m23) - (m00 * m21 - m20 * m01) * (m12 * m33 - m32 * m13) +
						(m00 * m31 - m30 * m01) * (m12 * m23 - m22 * m13) + (m10 * m21 - m20 * m11) * (m02 * m33 - m32 * m03) -
						(m10 * m31 - m30 * m11) * (m02 * m23 - m22 * m03) + (m20 * m31 - m30 * m21) * (m02 * m13 - m12 * m03));
			}
		}
		
		public SBSVector3 xAxis {
			get {
				return new SBSVector3(m00, m01, m02);
			}
			set {
				m00 = value.x;
				m01 = value.y;
				m02 = value.z;
				m03 = 0.0f;
			}
		}

		public SBSVector3 yAxis {
			get {
				return new SBSVector3(m10, m11, m12);
			}
			set {
				m10 = value.x;
				m11 = value.y;
				m12 = value.z;
				m13 = 0.0f;
			}
		}

		public SBSVector3 zAxis {
			get {
				return new SBSVector3(m20, m21, m22);
			}
			set {
				m20 = value.x;
				m21 = value.y;
				m22 = value.z;
				m23 = 0.0f;
			}
		}
		
		public SBSVector3 position {
			get {
				return new SBSVector3(m30, m31, m32);
			}
			set {
				m30 = value.x;
				m31 = value.y;
				m32 = value.z;
				m33 = 1.0f;
			}
		}
		
		public SBSVector3 scale {
			get {
				return new SBSVector3(this.xAxis.magnitude, this.yAxis.magnitude, this.zAxis.magnitude);
			}
			set {
				SBSVector3 _x = this.xAxis,
						   _y = this.yAxis,
						   _z = this.zAxis;

				float _xx = SBSVector3.Dot(_x, _x),
					  _yy = SBSVector3.Dot(_y, _y),
					  _zz = SBSVector3.Dot(_z, _z);
				
				if (_xx > SBSMath.SqrEpsilon)
					_x.ScaleBy(value.x / SBSMath.Sqrt(_xx));
				if (_yy > SBSMath.SqrEpsilon)
					_y.ScaleBy(value.y / SBSMath.Sqrt(_yy));
				if (_zz > SBSMath.SqrEpsilon)
					_z.ScaleBy(value.z / SBSMath.Sqrt(_zz));

				this.xAxis = _x;
				this.yAxis = _y;
				this.zAxis = _z;
			}
		}
		
		public SBSQuaternion rotation {
			get {
		        float diagonal, scale, oos, x, y, z, w;

		        diagonal = m00 + m11 + m22 + m33;
		        if (diagonal > SBSMath.Epsilon) {
		            scale = SBSMath.Sqrt(diagonal) * 2.0f;
		            oos   = 1.0f / scale;

		            x = (m12 - m21) * oos;
		            y = (m20 - m02) * oos;
		            z = (m01 - m10) * oos;
		            w = 0.25f * scale;
		        } else {
		            if (m00 > m11 && m00 > m22) {
		                scale = SBSMath.Sqrt(1.0f + m00 - m11 - m22) * 2.0f;
		                oos   = 1.0f / scale;
		
		                x = -0.25f * scale;
		                y = -(m10 + m01) * oos;
		                z = -(m02 + m20) * oos;
		                w =  (m12 - m21) * oos;    
		            } else if (m11 > m22) {
		                scale = SBSMath.Sqrt(1.0f + m11 - m00 - m22) * 2.0f;
		                oos   = 1.0f / scale;
		
		                x = -(m10 + m01) * oos;
		                y = -0.25f * scale;
		                z = -(m21 + m12) * oos;
		                w =  (m20 - m02) * oos;
		            } else {
		                scale = SBSMath.Sqrt(1.0f + m22 - m00 - m11) * 2.0f;
		                oos   = 1.0f / scale;
		
		                x = -(m02 + m20) * oos;
		                y = -(m21 + m12) * oos;
		                z = -0.25f * scale;
		                w =  (m01 - m10) * oos;
		            }
		        }
				
				SBSQuaternion q = new SBSQuaternion(x, y, z, w);
				q.Normalize();
				return q;
			}
			set {
				SBSVector3 _t = this.position,
						   _s = this.scale;

				this.SetTRS(_t, value, _s);
			}
		}
		
		public SBSAngleAxis angleAxis {
			get {
				return this.rotation.angleAxis;
			}
			set {
				SBSVector3 _t = this.position,
						   _s = this.scale;
				SBSQuaternion _q = SBSQuaternion.AngleAxis(value.angle, value.axis);

				this.SetTRS(_t, _q, _s);
			}
		}

#if UNITY_FLASH
		public SBSMatrix4x4()
		{
        }
#endif

		public SBSMatrix4x4(
			float _00, float _01, float _02, float _03,
			float _10, float _11, float _12, float _13,
			float _20, float _21, float _22, float _23,
			float _30, float _31, float _32, float _33)
		{
			m00 = _00;
			m01 = _01;
			m02 = _02;
			m03 = _03;
			m10 = _10;
			m11 = _11;
			m12 = _12;
			m13 = _13;
			m20 = _20;
			m21 = _21;
			m22 = _22;
			m23 = _23;
			m30 = _30;
			m31 = _31;
			m32 = _32;
			m33 = _33;
		}

        public static implicit operator SBSMatrix4x4(Matrix4x4 _m)
        {
			SBSMatrix4x4 r = new SBSMatrix4x4();
			r.m00 = _m.m00;
			r.m01 = _m.m10;
			r.m02 = _m.m20;
			r.m03 = _m.m30;
			r.m10 = _m.m01;
			r.m11 = _m.m11;
			r.m12 = _m.m21;
			r.m13 = _m.m31;
			r.m20 = _m.m02;
			r.m21 = _m.m12;
			r.m22 = _m.m22;
			r.m23 = _m.m32;
			r.m30 = _m.m03;
			r.m31 = _m.m13;
			r.m32 = _m.m23;
			r.m33 = _m.m33;
			return r;
        }
		
		public static implicit operator Matrix4x4(SBSMatrix4x4 _m)
		{
			Matrix4x4 m = new Matrix4x4();
			m.m00 = _m.m00;
			m.m10 = _m.m01;
			m.m20 = _m.m02;
			m.m30 = _m.m03;
			m.m01 = _m.m10;
			m.m11 = _m.m11;
			m.m21 = _m.m12;
			m.m31 = _m.m13;
			m.m02 = _m.m20;
			m.m12 = _m.m21;
			m.m22 = _m.m22;
			m.m32 = _m.m23;
			m.m03 = _m.m30;
			m.m13 = _m.m31;
			m.m23 = _m.m32;
			m.m33 = _m.m33;
			return m;
		}
		
		public static SBSMatrix4x4 operator *(SBSMatrix4x4 m1, SBSMatrix4x4 m0)
		{
            SBSMatrix4x4 r = new SBSMatrix4x4();
	        r.m00 = m0.m00 * m1.m00 + m0.m01 * m1.m10 + m0.m02 * m1.m20 + m0.m03 * m1.m30;
	        r.m01 = m0.m00 * m1.m01 + m0.m01 * m1.m11 + m0.m02 * m1.m21 + m0.m03 * m1.m31;
	        r.m02 = m0.m00 * m1.m02 + m0.m01 * m1.m12 + m0.m02 * m1.m22 + m0.m03 * m1.m32;
	        r.m03 = m0.m00 * m1.m03 + m0.m01 * m1.m13 + m0.m02 * m1.m23 + m0.m03 * m1.m33;
	        r.m10 = m0.m10 * m1.m00 + m0.m11 * m1.m10 + m0.m12 * m1.m20 + m0.m13 * m1.m30;
	        r.m11 = m0.m10 * m1.m01 + m0.m11 * m1.m11 + m0.m12 * m1.m21 + m0.m13 * m1.m31;
	        r.m12 = m0.m10 * m1.m02 + m0.m11 * m1.m12 + m0.m12 * m1.m22 + m0.m13 * m1.m32;
	        r.m13 = m0.m10 * m1.m03 + m0.m11 * m1.m13 + m0.m12 * m1.m23 + m0.m13 * m1.m33;
	        r.m20 = m0.m20 * m1.m00 + m0.m21 * m1.m10 + m0.m22 * m1.m20 + m0.m23 * m1.m30;
	        r.m21 = m0.m20 * m1.m01 + m0.m21 * m1.m11 + m0.m22 * m1.m21 + m0.m23 * m1.m31;
	        r.m22 = m0.m20 * m1.m02 + m0.m21 * m1.m12 + m0.m22 * m1.m22 + m0.m23 * m1.m32;
	        r.m23 = m0.m20 * m1.m03 + m0.m21 * m1.m13 + m0.m22 * m1.m23 + m0.m23 * m1.m33;
	        r.m30 = m0.m30 * m1.m00 + m0.m31 * m1.m10 + m0.m32 * m1.m20 + m0.m33 * m1.m30;
	        r.m31 = m0.m30 * m1.m01 + m0.m31 * m1.m11 + m0.m32 * m1.m21 + m0.m33 * m1.m31;
	        r.m32 = m0.m30 * m1.m02 + m0.m31 * m1.m12 + m0.m32 * m1.m22 + m0.m33 * m1.m32;
	        r.m33 = m0.m30 * m1.m03 + m0.m31 * m1.m13 + m0.m32 * m1.m23 + m0.m33 * m1.m33;
			return r;
		}
		
		public static SBSVector3 operator *(SBSMatrix4x4 m, SBSVector3 v)
		{
			float vx = v.x, vy = v.y, vz = v.z;
			return new SBSVector3(
				vx * m.m00 + vy * m.m10 + vz * m.m20 + m.m30,
				vx * m.m01 + vy * m.m11 + vz * m.m21 + m.m31,
				vx * m.m02 + vy * m.m12 + vz * m.m22 + m.m32);
		}

        public static SBSMatrix4x4 LookAt(SBSVector3 eye, SBSVector3 target, SBSVector3 up)
        {
            SBSVector3 zAxis = target - eye;
            zAxis.Normalize();
            SBSVector3 xAxis = SBSVector3.Cross(up, zAxis);
            xAxis.Normalize();
            SBSVector3 yAxis = SBSVector3.Cross(zAxis, xAxis);
            return new SBSMatrix4x4(
                xAxis.x, xAxis.y, xAxis.z, 0.0f,
                yAxis.x, yAxis.y, yAxis.z, 0.0f,
                zAxis.x, zAxis.y, zAxis.z, 0.0f,
                eye.x, eye.y, eye.z, 1.0f);
        }

		public static SBSMatrix4x4 TRS(SBSVector3 t, SBSQuaternion q, SBSVector3 s)
		{
			SBSMatrix4x4 r = new SBSMatrix4x4();

			float xy2 = 2.0f * q.x * q.y, xz2 = 2.0f * q.x * q.z, xw2 = 2.0f * q.x * q.w,
				  yz2 = 2.0f * q.y * q.z, yw2 = 2.0f * q.y * q.w, zw2 = 2.0f * q.z * q.w,
				  xx = q.x * q.x, yy = q.y * q.y, zz = q.z * q.z, ww = q.w * q.w,
				  sx = s.x, sy = s.y, sz = s.z;
			
			r.m00 = (xx - yy - zz + ww) * sx;
			r.m01 = (xy2 + zw2) * sx;
			r.m02 = (xz2 - yw2) * sx;
			r.m03 = 0.0f;
			r.m10 = (xy2 - zw2) * sy;
			r.m11 = (-xx + yy - zz + ww) * sy;
			r.m12 = (yz2 + xw2) * sy;
			r.m13 = 0.0f;
			r.m20 = (xz2 + yw2) * sz;
			r.m21 = (yz2 - xw2) * sz;
			r.m22 = (-xx - yy + zz + ww) * sz;
			r.m23 = 0.0f;
			r.m30 = t.x;
			r.m31 = t.y;
			r.m32 = t.z;
			r.m33 = 1.0f;

			return r;
		}

        public static SBSMatrix4x4 TRS(SBSVector3 t, SBSAngleAxis r, SBSVector3 s)
        {
            return TRS(t, SBSQuaternion.AngleAxis(r.angle, r.axis), s);
        }
        
        public void SetTRS(SBSVector3 t, SBSQuaternion q, SBSVector3 s)
		{
			float xy2 = 2.0f * q.x * q.y, xz2 = 2.0f * q.x * q.z, xw2 = 2.0f * q.x * q.w,
				  yz2 = 2.0f * q.y * q.z, yw2 = 2.0f * q.y * q.w, zw2 = 2.0f * q.z * q.w,
				  xx = q.x * q.x, yy = q.y * q.y, zz = q.z * q.z, ww = q.w * q.w,
				  sx = s.x, sy = s.y, sz = s.z;
			
			m00 = (xx - yy - zz + ww) * sx;
			m01 = (xy2 + zw2) * sx;
			m02 = (xz2 - yw2) * sx;
			m03 = 0.0f;
			m10 = (xy2 - zw2) * sy;
			m11 = (-xx + yy - zz + ww) * sy;
			m12 = (yz2 + xw2) * sy;
			m13 = 0.0f;
			m20 = (xz2 + yw2) * sz;
			m21 = (yz2 - xw2) * sz;
			m22 = (-xx - yy + zz + ww) * sz;
			m23 = 0.0f;
			m30 = t.x;
			m31 = t.y;
			m32 = t.z;
			m33 = 1.0f;
		}

        public void SetTRS(SBSVector3 t, SBSAngleAxis r, SBSVector3 s)
        {
            this.SetTRS(t, SBSQuaternion.AngleAxis(r.angle, r.axis), s);
        }

		public SBSVector3 MultiplyPoint3x4(SBSVector3 p)
		{
			float px = p.x, py = p.y, pz = p.z;
			return new SBSVector3(
				px * m00 + py * m10 + pz * m20 + m30,
				px * m01 + py * m11 + pz * m21 + m31,
				px * m02 + py * m12 + pz * m22 + m32);
		}
		
#if UNITY_FLASH
		public void MultiplyPoint3x4(SBSVector3 p, SBSVector3 o)
#else
        public void MultiplyPoint3x4(SBSVector3 p, out SBSVector3 o)
#endif
		{
			float px = p.x, py = p.y, pz = p.z;
			o.x = px * m00 + py * m10 + pz * m20 + m30;
			o.y = px * m01 + py * m11 + pz * m21 + m31;
			o.z = px * m02 + py * m12 + pz * m22 + m32;
		}
		
		public SBSVector3 MultiplyPoint3x4(float px, float py, float pz)
		{
			return new SBSVector3(
				px * m00 + py * m10 + pz * m20 + m30,
				px * m01 + py * m11 + pz * m21 + m31,
				px * m02 + py * m12 + pz * m22 + m32);
		}

		public SBSVector3 MultiplyPoint(SBSVector3 p)
		{
			float px = p.x, py = p.y, pz = p.z,
				  oow = 1.0f / (px * m03 + py * m13 + pz * m23 + m33);
			return new SBSVector3(
				(px * m00 + py * m10 + pz * m20 + m30) * oow,
				(px * m01 + py * m11 + pz * m21 + m31) * oow,
				(px * m02 + py * m12 + pz * m22 + m32) * oow);
		}

#if UNITY_FLASH
		public void MultiplyPoint(SBSVector3 p, SBSVector3 o)
#else
        public void MultiplyPoint(SBSVector3 p, out SBSVector3 o)
#endif
		{
			float px = p.x, py = p.y, pz = p.z,
				  oow = 1.0f / (px * m03 + py * m13 + pz * m23 + m33);
			o.x = (px * m00 + py * m10 + pz * m20 + m30) * oow;
			o.y = (px * m01 + py * m11 + pz * m21 + m31) * oow;
			o.z = (px * m02 + py * m12 + pz * m22 + m32) * oow;
		}

		public SBSVector3 MultiplyPoint(float px, float py, float pz)
		{
			float oow = 1.0f / (px * m03 + py * m13 + pz * m23 + m33);
			return new SBSVector3(
				(px * m00 + py * m10 + pz * m20 + m30) * oow,
				(px * m01 + py * m11 + pz * m21 + m31) * oow,
				(px * m02 + py * m12 + pz * m22 + m32) * oow);
		}

		public SBSVector3 MultiplyVector(SBSVector3 v)
		{
			float vx = v.x, vy = v.y, vz = v.z;
			return new SBSVector3(
				vx * m00 + vy * m10 + vz * m20,
				vx * m01 + vy * m11 + vz * m21,
				vx * m02 + vy * m12 + vz * m22);
		}
#if UNITY_FLASH
		public void MultiplyVector(SBSVector3 v, SBSVector3 o)
#else
        public void MultiplyVector(SBSVector3 v, out SBSVector3 o)
#endif
		{
			float vx = v.x, vy = v.y, vz = v.z;
			o.x = vx * m00 + vy * m10 + vz * m20;
			o.y = vx * m01 + vy * m11 + vz * m21;
			o.z = vx * m02 + vy * m12 + vz * m22;
		}

		public SBSVector3 MultiplyVector(float vx, float vy, float vz)
		{
			return new SBSVector3(
				vx * m00 + vy * m10 + vz * m20,
				vx * m01 + vy * m11 + vz * m21,
				vx * m02 + vy * m12 + vz * m22);
		}

        public void Append(SBSMatrix4x4 m0)
        {
            float _m00 = m00, _m01 = m01, _m02 = m02, _m03 = m03,
                  _m10 = m10, _m11 = m11, _m12 = m12, _m13 = m13,
                  _m20 = m20, _m21 = m21, _m22 = m22, _m23 = m23,
                  _m30 = m30, _m31 = m31, _m32 = m32, _m33 = m33;
            m00 = m0.m00 * _m00 + m0.m01 * _m10 + m0.m02 * _m20 + m0.m03 * _m30;
            m01 = m0.m00 * _m01 + m0.m01 * _m11 + m0.m02 * _m21 + m0.m03 * _m31;
            m02 = m0.m00 * _m02 + m0.m01 * _m12 + m0.m02 * _m22 + m0.m03 * _m32;
            m03 = m0.m00 * _m03 + m0.m01 * _m13 + m0.m02 * _m23 + m0.m03 * _m33;
            m10 = m0.m10 * _m00 + m0.m11 * _m10 + m0.m12 * _m20 + m0.m13 * _m30;
            m11 = m0.m10 * _m01 + m0.m11 * _m11 + m0.m12 * _m21 + m0.m13 * _m31;
            m12 = m0.m10 * _m02 + m0.m11 * _m12 + m0.m12 * _m22 + m0.m13 * _m32;
            m13 = m0.m10 * _m03 + m0.m11 * _m13 + m0.m12 * _m23 + m0.m13 * _m33;
            m20 = m0.m20 * _m00 + m0.m21 * _m10 + m0.m22 * _m20 + m0.m23 * _m30;
            m21 = m0.m20 * _m01 + m0.m21 * _m11 + m0.m22 * _m21 + m0.m23 * _m31;
            m22 = m0.m20 * _m02 + m0.m21 * _m12 + m0.m22 * _m22 + m0.m23 * _m32;
            m23 = m0.m20 * _m03 + m0.m21 * _m13 + m0.m22 * _m23 + m0.m23 * _m33;
            m30 = m0.m30 * _m00 + m0.m31 * _m10 + m0.m32 * _m20 + m0.m33 * _m30;
            m31 = m0.m30 * _m01 + m0.m31 * _m11 + m0.m32 * _m21 + m0.m33 * _m31;
            m32 = m0.m30 * _m02 + m0.m31 * _m12 + m0.m32 * _m22 + m0.m33 * _m32;
            m33 = m0.m30 * _m03 + m0.m31 * _m13 + m0.m32 * _m23 + m0.m33 * _m33;
        }

        public void Prepend(SBSMatrix4x4 m1)
        {
            float _m00 = m00, _m01 = m01, _m02 = m02, _m03 = m03,
                  _m10 = m10, _m11 = m11, _m12 = m12, _m13 = m13,
                  _m20 = m20, _m21 = m21, _m22 = m22, _m23 = m23,
                  _m30 = m30, _m31 = m31, _m32 = m32, _m33 = m33;
            m00 = _m00 * m1.m00 + _m01 * m1.m10 + _m02 * m1.m20 + _m03 * m1.m30;
            m01 = _m00 * m1.m01 + _m01 * m1.m11 + _m02 * m1.m21 + _m03 * m1.m31;
            m02 = _m00 * m1.m02 + _m01 * m1.m12 + _m02 * m1.m22 + _m03 * m1.m32;
            m03 = _m00 * m1.m03 + _m01 * m1.m13 + _m02 * m1.m23 + _m03 * m1.m33;
            m10 = _m10 * m1.m00 + _m11 * m1.m10 + _m12 * m1.m20 + _m13 * m1.m30;
            m11 = _m10 * m1.m01 + _m11 * m1.m11 + _m12 * m1.m21 + _m13 * m1.m31;
            m12 = _m10 * m1.m02 + _m11 * m1.m12 + _m12 * m1.m22 + _m13 * m1.m32;
            m13 = _m10 * m1.m03 + _m11 * m1.m13 + _m12 * m1.m23 + _m13 * m1.m33;
            m20 = _m20 * m1.m00 + _m21 * m1.m10 + _m22 * m1.m20 + _m23 * m1.m30;
            m21 = _m20 * m1.m01 + _m21 * m1.m11 + _m22 * m1.m21 + _m23 * m1.m31;
            m22 = _m20 * m1.m02 + _m21 * m1.m12 + _m22 * m1.m22 + _m23 * m1.m32;
            m23 = _m20 * m1.m03 + _m21 * m1.m13 + _m22 * m1.m23 + _m23 * m1.m33;
            m30 = _m30 * m1.m00 + _m31 * m1.m10 + _m32 * m1.m20 + _m33 * m1.m30;
            m31 = _m30 * m1.m01 + _m31 * m1.m11 + _m32 * m1.m21 + _m33 * m1.m31;
            m32 = _m30 * m1.m02 + _m31 * m1.m12 + _m32 * m1.m22 + _m33 * m1.m32;
            m33 = _m30 * m1.m03 + _m31 * m1.m13 + _m32 * m1.m23 + _m33 * m1.m33;
        }

		public void Transpose()
		{
			float f;
			f     = m01;
			m01 = m10;
			m10 = f;
			f     = m02;
			m02 = m20;
			m20 = f;
			f     = m03;
			m03 = m30;
			m30 = f;
			f     = m12;
			m12 = m21;
			m21 = f;
			f     = m13;
			m13 = m31;
			m31 = f;
			f     = m23;
			m23 = m32;
			m32 = f;
		}

		public bool Invert()
		{
			float ood = this.determinant;
			if (SBSMath.Abs(ood) < SBSMath.Epsilon)
				return false;
			ood = 1.0f / ood;
			
			float _m00 = ood * (m11 * (m22 * m33 - m32 * m23) + m21 * (m32 * m13 - m12 * m33) + m31 * (m12 * m23 - m22 * m13));
			float _m10 = ood * (m12 * (m20 * m33 - m30 * m23) + m22 * (m30 * m13 - m10 * m33) + m32 * (m10 * m23 - m20 * m13));
			float _m20 = ood * (m13 * (m20 * m31 - m30 * m21) + m23 * (m30 * m11 - m10 * m31) + m33 * (m10 * m21 - m20 * m11));
			float _m30 = ood * (m10 * (m31 * m22 - m21 * m32) + m20 * (m11 * m32 - m31 * m12) + m30 * (m21 * m12 - m11 * m22));
			float _m01 = ood * (m21 * (m02 * m33 - m32 * m03) + m31 * (m22 * m03 - m02 * m23) + m01 * (m32 * m23 - m22 * m33));
			float _m11 = ood * (m22 * (m00 * m33 - m30 * m03) + m32 * (m20 * m03 - m00 * m23) + m02 * (m30 * m23 - m20 * m33));
			float _m21 = ood * (m23 * (m00 * m31 - m30 * m01) + m33 * (m20 * m01 - m00 * m21) + m03 * (m30 * m21 - m20 * m31));
			float _m31 = ood * (m20 * (m31 * m02 - m01 * m32) + m30 * (m01 * m22 - m21 * m02) + m00 * (m21 * m32 - m31 * m22));
			float _m02 = ood * (m31 * (m02 * m13 - m12 * m03) + m01 * (m12 * m33 - m32 * m13) + m11 * (m32 * m03 - m02 * m33));
			float _m12 = ood * (m32 * (m00 * m13 - m10 * m03) + m02 * (m10 * m33 - m30 * m13) + m12 * (m30 * m03 - m00 * m33));
			float _m22 = ood * (m33 * (m00 * m11 - m10 * m01) + m03 * (m10 * m31 - m30 * m11) + m13 * (m30 * m01 - m00 * m31));
			float _m32 = ood * (m30 * (m11 * m02 - m01 * m12) + m00 * (m31 * m12 - m11 * m32) + m10 * (m01 * m32 - m31 * m02));
			float _m03 = ood * (m01 * (m22 * m13 - m12 * m23) + m11 * (m02 * m23 - m22 * m03) + m21 * (m12 * m03 - m02 * m13));
			float _m13 = ood * (m02 * (m20 * m13 - m10 * m23) + m12 * (m00 * m23 - m20 * m03) + m22 * (m10 * m03 - m00 * m13));
			float _m23 = ood * (m03 * (m20 * m11 - m10 * m21) + m13 * (m00 * m21 - m20 * m01) + m23 * (m10 * m01 - m00 * m11));
			float _m33 = ood * (m00 * (m11 * m22 - m21 * m12) + m10 * (m21 * m02 - m01 * m22) + m20 * (m01 * m12 - m11 * m02));

            m00 = _m00; m01 = _m01; m02 = _m02; m03 = _m03;
            m10 = _m10; m11 = _m11; m12 = _m12; m13 = _m13;
            m20 = _m20; m21 = _m21; m22 = _m22; m23 = _m23;
            m30 = _m30; m31 = _m31; m32 = _m32; m33 = _m33;

			return true;
		}
		
		public bool InvertFast()
		{
			float ood = this.determinant;
			if (SBSMath.Abs(ood) < SBSMath.Epsilon)
				return false;
			ood = 1.0f / ood;
			
			float _m00 = ood * (m11 * m22 - m12 * m21);
			float _m01 = ood * (m21 * m02 - m22 * m01);
			float _m02 = ood * (m01 * m12 - m02 * m11);
			float _m03 = 0.0f;
			float _m10 = ood * (m12 * m20 - m10 * m22);
			float _m11 = ood * (m22 * m00 - m20 * m02);
			float _m12 = ood * (m02 * m10 - m00 * m12);
			float _m13 = 0.0f;
			float _m20 = ood * (m10 * m21 - m11 * m20);
			float _m21 = ood * (m20 * m01 - m21 * m00);
			float _m22 = ood * (m00 * m11 - m01 * m10);
			float _m23 = 0.0f;
			float _m30 = ood * (m10 * (m22 * m31 - m21 * m32) + m11 * (m20 * m32 - m22 * m30) + m12 * (m21 * m30 - m20 * m31));
			float _m31 = ood * (m20 * (m02 * m31 - m01 * m32) + m21 * (m00 * m32 - m02 * m30) + m22 * (m01 * m30 - m00 * m31));
			float _m32 = ood * (m30 * (m02 * m11 - m01 * m12) + m31 * (m00 * m12 - m02 * m10) + m32 * (m01 * m10 - m00 * m11));
			float _m33 = 1.0f;

            m00 = _m00; m01 = _m01; m02 = _m02; m03 = _m03;
            m10 = _m10; m11 = _m11; m12 = _m12; m13 = _m13;
            m20 = _m20; m21 = _m21; m22 = _m22; m23 = _m23;
            m30 = _m30; m31 = _m31; m32 = _m32; m33 = _m33;
            
            return true;
		}
#if UNITY_FLASH
		public SBSMatrix4x4 Clone()
        {
            return new SBSMatrix4x4(
                m00, m01, m02, m03,
                m10, m11, m12, m13,
                m20, m21, m22, m23,
                m30, m31, m32, m33);
        }
#endif
	}
}

