using System;
using UnityEngine;

namespace SBS.Math
{
	public class SBSMath
	{
		#region constants
#if UNITY_FLASH && !UNITY_EDITOR
		public static float PI = (float)System.Math.PI;
#else
		public static float PI = Mathf.PI;
#endif
    	public static float ToDegrees = 180.0f / PI;
		public static float ToRadians = PI / 180.0f;

		public static float Epsilon = 1.0e-5f;
		public static float SqrEpsilon = Epsilon * Epsilon;
		#endregion

        #region enums
        public enum ClipStatus
        {
            Inside = 0,
            Overlapping,
            Outside
        };
        #endregion

        #region float functions
		public static float Abs(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Abs(x);
#else
            return Mathf.Abs(x);
#endif
        }

        public static float Acos(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Acos(x);
#else
            return Mathf.Acos(x);
#endif
        }

        public static float Asin(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Asin(x);
#else
            return Mathf.Asin(x);
#endif
        }

        public static float Atan(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Atan(x);
#else
            return Mathf.Atan(x);
#endif
        }

        public static float Atan2(float y, float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Atan2(y, x);
#else
            return Mathf.Atan2(y, x);
#endif
        }

        public static float Ceil(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Ceiling(x);
#else
            return Mathf.Ceil(x);
#endif
        }

        public static float Cos(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Cos(x);
#else
            return Mathf.Cos(x);
#endif
        }

        public static float Exp(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Exp(x);
#else
            return Mathf.Exp(x);
#endif
        }

        public static float Floor(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Floor(x);
#else
            return Mathf.Floor(x);
#endif
        }

        public static float Frac(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return x - (float)System.Math.Floor(x);
#else
            return x - Mathf.Floor(x);
#endif
        }

		public static float Log(float x)
		{
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Log(x);
#else
            return Mathf.Log(x);
#endif
		}
		
        public static float Max(float a, float b)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return System.Math.Max(a, b);
#else
            return Mathf.Max(a, b);
#endif
        }
        
        public static float Min(float a, float b)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return System.Math.Min(a, b);
#else
            return Mathf.Min(a, b);
#endif
        }

        public static float Pow(float x, float p)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Pow(x, p);
#else
            return Mathf.Pow(x, p);
#endif
        }

        public static float Sin(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Sin(x);
#else
            return Mathf.Sin(x);
#endif
        }
		
		public static float Sign(float x)
		{/*
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Sign(x);
#else*/
            return Mathf.Sign(x);
//#endif
		}
		
        public static float Sqrt(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Sqrt(x);
#else
            return Mathf.Sqrt(x);
#endif
        }

        public static float Tan(float x)
        {
#if UNITY_FLASH && !UNITY_EDITOR
            return (float)System.Math.Tan(x);
#else
            return Mathf.Tan(x);
#endif
        }

        public static float RateLimit(float prevValue, float value, float posRateLimit, float negRateLimit)
        {
            if (value - prevValue > posRateLimit)
                return prevValue + posRateLimit;
            else if (value - prevValue < -negRateLimit)
                return prevValue - negRateLimit;
            else
                return value;
        }
		#endregion

        #region intersection functions
        public static ClipStatus GetClipStatus(SBSBounds a, SBSBounds b)
        {
            if (a.Contains(b))
                return ClipStatus.Inside;
            else if (a.Intersect(b))
                return ClipStatus.Overlapping;
            else
                return ClipStatus.Outside;
        }

        public static bool RectContains(Rect r1, Rect r2)
        {
            return (r1.xMin <= r2.xMin && r1.yMin <= r2.yMin) && (r1.xMax >= r2.xMax && r1.yMax >= r2.yMax);
        }

        public static bool RectIntersect(Rect r1, Rect r2)
        {
            return !((r1.xMax < r2.xMin || r1.yMax < r2.yMin) || (r1.xMin > r2.xMax || r1.yMin > r2.yMax));
        }

        public static ClipStatus GetClipStatus(Rect a, Rect b)
        {
            if (RectContains(a, b))
                return ClipStatus.Inside;
            else if (RectIntersect(a, b))
                return ClipStatus.Overlapping;
            else
                return ClipStatus.Outside;
        }
        #endregion
    }
}
