using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

namespace SBS.Math
{
    public class Curve
    {
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        public static float IntegrationDt = 0.125f;//0.1f;
        public static bool UseNewton = true;//false;
        public static int IntegrationSteps = 8;//10;
        public static float Error = 1.0e-2f;//5f;
		public static bool LinearSearch = true;
#else
        public static float IntegrationDt = 0.05f;
        public static bool UseNewton = false;
        public static int IntegrationSteps = 20;
        public static float Error = 1.0e-5f;
		public static bool LinearSearch = true;
#endif

        public enum Interpolation
        {
            Linear = 0,
            CatmullRom,
        }

#if UNITY_FLASH
        public class Evaluation
#else
        public struct Evaluation
#endif
        {
            public SBSVector3 f0;
            public SBSVector3 f1;
            public int index;
            public float time;

#if UNITY_FLASH
            public Evaluation()
            {
                f0 = new SBSVector3();
                f1 = new SBSVector3();
            }
#endif

            public Evaluation(SBSVector3 _f0, SBSVector3 _f1, int _index, float _time)
            {
                f0 = _f0;
                f1 = _f1;
                index = _index;
                time = _time;
            }
        }

        protected List<SBSVector3> points = new List<SBSVector3>();
        protected List<float> spaces = new List<float>();
        protected List<float> arcLenghts = new List<float>();
        protected float length = 0.0f;
        protected bool isClosed = false;
        protected Interpolation intType = Interpolation.Linear;

        private Curve()
        { }

        public Curve(Interpolation _intType, bool _isClosed)
        {
            intType = _intType;
            isClosed = _isClosed;
        }

        public Curve(Interpolation _intType, bool _isClosed, List<SBSVector3> _points)
        {
            intType = _intType;
            isClosed = _isClosed;

            this.BuildFromPointsList(_points);
        }

        public bool IsClosed
        {
            get
            {
                return isClosed;
            }
            set
            {
                isClosed = value;
            }
        }

        public float Length
        {
            get
            {
                return length;
            }
        }

        public int ArcCount
        {
            get
            {
                return Mathf.Max(0, points.Count - 1);
            }
        }

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public Interpolation InterpolationType
        {
            get
            {
                return intType;
            }
            set
            {
                intType = value;

                this.BuildFromPointsList(points);
            }
        }

        public SBSVector3 this[int i]
        {
            get
            {
                return points[i];
            }
            set
            {
                points[i] = value;

                int arcIndexLo = 0,
                    arcIndexHi = 0;
                switch (intType)
                {
                    case Interpolation.Linear:
                        arcIndexLo = i - 1;
                        arcIndexHi = i;
                        break;
                    case Interpolation.CatmullRom:
                        arcIndexLo = i - 2;
                        arcIndexHi = i + 1;
                        break;
                }

                int lastSpaceIndex = 0,
                    arcCount = arcLenghts.Count,
                    numIters = Mathf.Min(arcCount - 1, arcIndexHi - arcIndexLo);
                for (int j = 0; j < numIters; ++j)
                {
                    int arcIndex = arcIndexLo + j;
                    if (arcIndex < 0 || arcIndex > (arcCount - 1))
                        arcIndex = (arcIndex + arcCount) % arcCount;

                    length -= arcLenghts[arcIndex];

                    float l = this.ComputeArcLength(arcIndex, 0.0f, 1.0f);
                    arcLenghts[arcIndex] = l;

                    lastSpaceIndex = arcIndex + 1;
                    spaces[lastSpaceIndex] = spaces[arcIndex] + l;

                    length += l;
                }

                if (lastSpaceIndex >= 0 && spaces.Count > 1 && arcCount > 0)
                {
                    lastSpaceIndex = Mathf.Max(1, lastSpaceIndex);
                    int spaceIndexHi = spaces.Count - 1;
                    for (int j = lastSpaceIndex; j <= spaceIndexHi; ++j)
                        spaces[j] = spaces[j - 1] + arcLenghts[j - 1];
                }
            }
        }

        protected SBSVector3 f0(int i, float t)
        {
            int count = points.Count;
            SBSVector3 p0, p1, p2, p3;
            switch (intType)
            {
                case Interpolation.Linear:
                    if (isClosed)
                    {
                        p0 = points[i];
                        p1 = points[(i + 1) % count];
                    }
                    else
                    {
                        p0 = points[i];
                        p1 = points[Mathf.Min(i + 1, count - 1)];
                    }
                    return p0 + (p1 - p0) * t;
                case Interpolation.CatmullRom:
                    if (isClosed)
                    {
                        if (0 == i)
                            p0 = points[count - 2];
                        else
                            p0 = points[i - 1];
                        p1 = points[i];
                        if ((count - 1) == i)
                            p2 = points[1];
                        else
                            p2 = points[i + 1];
                        if ((count - 2) == i)
                            p3 = points[1];
                        else
                            p3 = points[(i + 2) % count];
                    }
                    else
                    {
                        p0 = points[Mathf.Max(0, i - 1)];
                        p1 = points[i];
                        p2 = points[Mathf.Min(i + 1, count - 1)];
                        p3 = points[Mathf.Min(i + 2, count - 1)];
                    }
                    return 0.5f * ((-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t * t * t
                                + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t * t
                                + (-p0 + p2) * t
                                + (2.0f * p1));
                default:
                    return SBSVector3.zero;
            }
        }

        protected SBSVector3 f1(int i, float t)
        {
            int count = points.Count;
            SBSVector3 p0, p1, p2, p3;
            switch (intType)
            {
                case Interpolation.Linear:
                    if (isClosed)
                    {
                        p0 = points[i];
                        p1 = points[(i + 1) % count];
                    }
                    else
                    {
                        p0 = points[i];
                        p1 = points[Mathf.Min(i + 1, count - 1)];
                    }
                    return (p1 - p0);
                case Interpolation.CatmullRom:
					//Profiler.BeginSample("EvaluateInFront");
                    if (isClosed)
                    {
                        if (0 == i)
                            p0 = points[count - 2];
                        else
                            p0 = points[i - 1];
                        p1 = points[i];
                        if ((count - 1) == i)
                            p2 = points[1];
                        else
                            p2 = points[i + 1];
                        if ((count - 2) == i)
                            p3 = points[1];
                        else
                            p3 = points[(i + 2) % count];
                    }
                    else
                    {/*
                        p0 = points[Mathf.Max(0, i - 1)];
                        p1 = points[i];
                        p2 = points[Mathf.Min(i + 1, count - 1)];
                        p3 = points[Mathf.Min(i + 2, count - 1)];*/
                        if (0 == i)
                        {
                            if (count > 1)
                                p0 = points[0] + (points[0] - points[1]) * 0.9f;
                            else
                                p0 = points[0];
                        }
                        else
                            p0 = points[i - 1];
                        p1 = points[i++];
                        if (i < count)
                            p2 = points[i++];
                        else
                            p2 = p1 + (p1 - p0) * 0.9f;
                        if (i < count)
                            p3 = points[i];
                        else
                            p3 = p2 + (p2 - p1) * 0.9f;
                    }
					//Profiler.EndSample();
                    return 0.5f * ((3.0f * (-p0 + 3.0f * p1 - 3.0f * p2 + p3)) * t * t
                                + (2.0f * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3)) * t
                                + (-p0 + p2));
                default:
                    return SBSVector3.zero;
            }
        }

        protected float Speed(int i, float t)
        {
            return this.f1(i, t).magnitude;
        }

        protected float ComputeArcLength(int i, float tmin, float tmax)
        {
            float sign = 1.0f;
            if (tmin > tmax)
            {
                sign = -1.0f;
                float tmp = tmin;
                tmin = tmax;
                tmax = tmp;
            }

            switch (intType)
            {
                case Interpolation.Linear:
                    return sign * this.Speed(i, 0.0f) * (tmax - tmin);
                case Interpolation.CatmullRom:
                    float t = tmin,
                          s = 0.0f,
                          denom = 1.0f / 6.0f;

                    while (t < tmax)
                    {
                        float dt = SBSMath.Min(tmax - t, IntegrationDt),
                              v0 = this.Speed(i, t),
                              v1 = this.Speed(i, t + dt * 0.5f),
                              v2 = this.Speed(i, t + dt);

                        s += dt * (v0 + 4.0f * v1 + v2) * denom;
                        t += dt;
                    }
                    return sign * s;
                default:
                    return 0.0f;
            }
        }

        protected void BuildFromPointsList(List<SBSVector3> _points)
        {
            if (_points != points)
                points.Clear();

            spaces.Clear();
            arcLenghts.Clear();
            length = 0.0f;

            if (_points != points)
            {
                foreach (SBSVector3 p in _points)
                    points.Add(p);
            }

            int arcCount = this.ArcCount;
            spaces.Add(length);
            for (int i = 0; i < arcCount; ++i)
            {
                float l = this.ComputeArcLength(i, 0.0f, 1.0f);
                length += l;

                arcLenghts.Add(l);
                spaces.Add(length);
            }
        }

        protected float ComputeTimeFromSpace(int i, float s)
        {
            switch (intType)
            {
                case Interpolation.Linear:
                    return (s - spaces[i]) / arcLenghts[i];
                case Interpolation.CatmullRom:
                    if (UseNewton)
                    {
                        s -= spaces[i];
                        float t = s / arcLenghts[i],
                              tmin = 0.0f,
                              tmax = 1.0f;

                        for (int j = 0; j < IntegrationSteps; ++j)
                        {
                            float F = this.ComputeArcLength(i, 0.0f, t) - s;

                            if (SBSMath.Abs(F) < Error)
                                return t;

                            float DF = this.Speed(i, t),
                                  tnew = t - F / DF;
                            if (F > 0.0f)
                            {
                                tmax = t;
                                if (tnew <= tmin)
                                    t = 0.5f * (tmin + tmax);
                                else
                                    t = tnew;
                            }
                            else
                            {
                                tmin = t;
                                if (tnew >= tmax)
                                    t = 0.5f * (tmin + tmax);
                                else
                                    t = tnew;
                            }
                        }

                        return t;
                    }
                    else
                    {
                        float t = 0.0f,
                              h = (s - spaces[i]) / (float)IntegrationSteps,
                              d = 1.0f / 6.0f;

                        for (int j = 0; j < IntegrationSteps; ++j)
                        {
                            float k1 = h / this.Speed(i, t),
                                  k2 = h / this.Speed(i, t + k1 * 0.5f),
                                  k3 = h / this.Speed(i, t + k2 * 0.5f),
                                  k4 = h / this.Speed(i, t + k3);

                            t += (k1 + 2.0f * (k2 + k3) + k4) * d;
                        }

                        return t;
                    }
                default:
                    return 0.0f;
            }
        }

        public float GetArcLenght(int i)
        {
            return arcLenghts[i];
        }

        public float GetSpaceAt(int i)
        {
            return spaces[i];
        }

        public Evaluation EvaluateAtSpace(float s)
        {
			int index = 0;
			if (LinearSearch)
			{
				int cnt = spaces.Count;
				while (index < cnt && spaces[index] < s)
					++index;
			}
			else
			{
#if UNITY_FLASH
	            index = AS3Utils.BinarySearchList(spaces, s);
#else
	            index = spaces.BinarySearch(s);
#endif
	            index = (index < 0 ? ~index : index);
			}
//          int i = Mathf.Max(0, index - 1);
            int i = Mathf.Clamp(index - 1, 0, arcLenghts.Count - 1);
            float t = this.ComputeTimeFromSpace(i, s);
//          t = Mathf.Clamp01(t);
            return new Evaluation(this.f0(i, t), this.f1(i, t), i, t);
        }

        public float GetClosestSpace(SBSVector3 point)
        {
            /*
            int count = points.Count - 1;
            float minDist = float.MaxValue;
            int bestIndex = -1;
            float bestTime = 0.0f;
            for (int i = 0; i < count; ++i)
            {
                float t = 0.0f;
                while (t < 1.0)
                {
                    float d = SBSVector3.SqrDistance(this.f0(i, t), point);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestIndex = i;
                        bestTime = t;
                    }

                    t += SBSMath.Min(1.0f - t, 0.15f);
                }
            }
            return spaces[bestIndex] + arcLenghts[bestIndex] * bestTime;*/
            int count = points.Count - 1,
                nearestSeg = -1,
                i = 0;
            float nearestRatio = 0.0f,
                  nearestSqDist = float.MaxValue;
            for (; i < count; ++i)
            {
                SBSVector3 d = points[i + 1] - points[i],
                           v = point - points[i];
                float ratio = SBSVector3.Dot(d, v) / SBSVector3.Dot(d, d),
                      dist = SBSVector3.SqrDistance(points[i] + d * Mathf.Clamp01(ratio), point);
                if (dist < nearestSqDist)
                {
                    nearestSeg = i;
                    nearestRatio = ratio;
                    nearestSqDist = dist;
                }
            }

            nearestSqDist = float.MaxValue;
            float t = nearestRatio,
                  bestTime = 0.0f;
            for (i = 0; i < 10; ++i, t += 0.04f)
            {
                float dist = SBSVector3.SqrDistance(this.f0(nearestSeg, Mathf.Clamp01(t)), point);
                if (dist < nearestSqDist)
                {
                    nearestSqDist = dist;
                    bestTime = t;
                }
            }

            return spaces[nearestSeg] + arcLenghts[nearestSeg] * Mathf.Clamp01(bestTime);
        }

        public void Clear()
        {
            points.Clear();
            spaces.Clear();
            arcLenghts.Clear();
            length = 0.0f;
        }

        public void Add(SBSVector3 v)
        {
            points.Add(v);

            int count = points.Count;
            if (count > 1)
            {
                float l;
                switch (intType)
                {
                    case Interpolation.CatmullRom:
                        int arcCount   = arcLenghts.Count,
                            arcIndexLo = Mathf.Max(0, arcCount - 2),//0,
                            numIters   = arcCount - arcIndexLo;
                        for (int i = 0; i < numIters; ++i)
                        {
                            int arcIndex = arcIndexLo + i;
                            length -= arcLenghts[arcIndex];

                            l = this.ComputeArcLength(arcIndex, 0.0f, 1.0f);
                            arcLenghts[arcIndex] = l;

                            spaces[arcIndex + 1] = spaces[arcIndex] + l;
                            length += l;
                        }
                        break;
                    case Interpolation.Linear:
                        break;
                }

                l = this.ComputeArcLength(count - 1, 0.0f, 1.0f);
                arcLenghts.Add(l);

                length += l;
                spaces.Add(spaces[count - 2] + l);
            }
            else
            {
                spaces.Add(0.0f);
            }
        }

        public void Insert(int index, SBSVector3 v)
        {
            // ToDo
            points.Insert(index, v);
            this.BuildFromPointsList(points);
        }

        public void RemoveAt(int index)
        {
            Asserts.Assert(index >= 0 && index < points.Count);

            points.RemoveAt(index);
            spaces.RemoveAt(index);
            if (index == 0 && spaces.Count > 0)
                spaces[0] = 0.0f;

            if (0 == arcLenghts.Count)
                return;

            int arcToDelete = Mathf.Min(index, arcLenghts.Count - 1);
            length -= arcLenghts[arcToDelete];
            arcLenghts.RemoveAt(arcToDelete);

            int arcIndexLo = 0,
                arcIndexHi = 0;
            switch (intType)
            {
                case Interpolation.CatmullRom:
                    arcIndexLo = index - 2;
                    arcIndexHi = index;
                    break;
                case Interpolation.Linear:
                    arcIndexLo = index - 1;
                    arcIndexHi = index - 1;
                    break;
            }

            int lastSpaceIndex = 0,
                arcCount = arcLenghts.Count,
                numIters = Mathf.Min(arcCount, arcIndexHi - arcIndexLo + 1);
            for (int i = 0; i < numIters; ++i)
            {
                int arcIndex = arcIndexLo + i;
                if (arcIndex < 0 || arcIndex >= arcCount)
                    arcIndex = (arcIndex + arcCount) % arcCount;

                length -= arcLenghts[arcIndex];

                float l = this.ComputeArcLength(arcIndex, 0.0f, 1.0f);
                arcLenghts[arcIndex] = l;

                lastSpaceIndex = arcIndex + 1;
                spaces[lastSpaceIndex] = spaces[arcIndex] + l;

                length += l;
            }

            if (lastSpaceIndex > 0)
            {
                int spaceIndexHi = spaces.Count - 1;
                for (int i = lastSpaceIndex; i <= spaceIndexHi; ++i)
                    spaces[i] = spaces[i - 1] + arcLenghts[i - 1];
            }
        }
    }
}
