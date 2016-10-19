using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Math
{
#if !UNITY_FLASH
    public class SBSMix<T, M> where M : IMix<T>, new()
    {
        static protected IMix<T> m = new M();

        static public T Mix(T[] values, int[] indices, float[] ratios)
        {
            return m.Mix(values, indices, ratios);
        }
    }

    public interface IMix<T>
    {
        T Mix(T[] values, int[] indices, float[] ratios);
    }

    public class Vector2Mix : IMix<Vector2>
    {
        public Vector2 Mix(Vector2[] values, int[] indices, float[] ratios)
        {
            Vector2 v = values[indices[0]] * ratios[0];
            int i = 1, c = indices.Length;
            for (; i < c; ++i)
                v += (values[indices[i]] * ratios[i]);
            return v;
        }
    }

    public class Vector3Mix : IMix<Vector3>
    {
        public Vector3 Mix(Vector3[] values, int[] indices, float[] ratios)
        {
            Vector3 v = values[indices[0]] * ratios[0];
            int i = 1, c = indices.Length;
            for (; i < c; ++i)
                v += (values[indices[i]] * ratios[i]);
            return v;
        }
    }

    public class SBSVector3Mix : IMix<SBSVector3>
    {
        public SBSVector3 Mix(SBSVector3[] values, int[] indices, float[] ratios)
        {
            SBSVector3 v = values[indices[0]] * ratios[0];
            int i = 1, c = indices.Length;
            for (; i < c; ++i)
                v += (values[indices[i]] * ratios[i]);
            return v;
        }
    }
#else
    public class SBSMix { }
#endif
}
