using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Math
{
#if !UNITY_FLASH
    public class SBSLerp<T, L> where L : ILerp<T>, new()
    {
        static protected ILerp<T> l = new L();

        static public T Lerp(T a, T b, float t)
        {
            return l.Lerp(a, b, t);
        }
    }

    public interface ILerp<T>
    {
        T Lerp(T a, T b, float t);
    }

    public class Vector2Lerp : ILerp<Vector2>
    {
        public Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Vector2.Lerp(a, b, t);
        }
    }

    public class Vector3Lerp : ILerp<Vector3>
    {
        public Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }
    }

    public class QuaternionSlerp : ILerp<Quaternion>
    {
        public Quaternion Lerp(Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Slerp(a, b, t);
        }
    }

    public class ColorLerp : ILerp<Color>
    {
        public Color Lerp(Color a, Color b, float t)
        {
            return Color.Lerp(a, b, t);
        }
    }

    public class SBSVector3Lerp : ILerp<SBSVector3>
    {
        public SBSVector3 Lerp(SBSVector3 a, SBSVector3 b, float t)
        {
            return SBSVector3.Lerp(a, b, t);
        }
    }

    public class SBSQuaternionSlerp : ILerp<SBSQuaternion>
    {
        public SBSQuaternion Lerp(SBSQuaternion a, SBSQuaternion b, float t)
        {
            return SBSQuaternion.Slerp(a, b, t);
        }
    }
#else
    public class SBSLerp { }
#endif
}
