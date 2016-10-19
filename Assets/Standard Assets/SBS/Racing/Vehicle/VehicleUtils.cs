using System;
using UnityEngine;

public class VehicleUtils
{
    #region Constants
    public static float ToKiloNewtons = 0.001f;
    public static float ToRPM = 30.0f / (float)Math.PI;
    public static float ToKmh = 3.6f;
    public static float ToMph = 3.6f * 0.621371f;
    public static float ToMetersPerSecs = 1.0f / ToKmh;

    public enum CurveSmooth
    {
        Free,
        Linear,
        Auto
    }

    public enum Traction
    {
        FWD,
        RWD,
        AWD
    }

    public enum WheelPos
    {
        FR_LEFT,
        FR_RIGHT,
        RR_LEFT,
        RR_RIGHT,
        NUM_WHEELS
    }

    public enum DifferentialPos
    {
        FRONT,
        REAR,
        CENTRAL,
        NUM_DIFFERENTIALS
    }
    #endregion

    #region Functions
    public static AnimationCurve CreateCurve(Keyframe[] _curveKeys, CurveSmooth curveSmooth)
    {
//      Keyframe[] curveKeys = (Keyframe[])_curveKeys.Clone();
		Keyframe[] curveKeys = new Keyframe[_curveKeys.Length];
		for (int i = _curveKeys.Length - 1; i >= 0; --i)
			curveKeys[i] = _curveKeys[i];
        if (curveSmooth == CurveSmooth.Linear || curveSmooth == CurveSmooth.Auto)
        {
            int rightBound = curveKeys.Length - 1;
            for (int i = 0; i < rightBound; i++)
            {
                Keyframe s = curveKeys[i];
                Keyframe e = curveKeys[i + 1];

                float dy = e.value - s.value;
                float dx = e.time - s.time;
                float tangent = dy / dx;
                s.outTangent = tangent;
                e.inTangent = tangent;

                if (i == 0)
                    s.inTangent = s.outTangent;

                if (i == rightBound)
                    e.outTangent = e.inTangent;

                curveKeys[i] = s;
                curveKeys[i + 1] = e;
            }
        }

        AnimationCurve curve = new AnimationCurve(curveKeys);

        if (curveSmooth == CurveSmooth.Auto)
        {
            for (int i = 0; i < curve.length; i++)
                curve.SmoothTangents(i, 0.0f);
        }

        return curve;
    }
    #endregion
};
