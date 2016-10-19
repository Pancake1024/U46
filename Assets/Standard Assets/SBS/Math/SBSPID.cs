using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Math
{
#if !UNITY_FLASH
    public class SBSPID<T, C> where C : IPIDController<T>, new()
	{
        static protected IPIDController<T> c = new C();

        static public T GetForce(T current, T target, float dt, float P, float I, float D, ref T prevError, ref T errorIntegral)
        {
            return c.GetForce(current, target, dt, P, I, D, ref prevError, ref errorIntegral);
        }
    }

    public interface IPIDController<T>
    {
        T GetForce(T current, T target, float dt, float P, float I, float D, ref T prevError, ref T errorIntegral);
    }

    public class Vector3PID : IPIDController<Vector3>
    {
        public Vector3 GetForce(Vector3 current, Vector3 target, float dt, float P, float I, float D, ref Vector3 prevError, ref Vector3 errorIntegral)
        {
            if (dt < SBSMath.Epsilon)
                return Vector3.zero;

            float oodt = 1.0f / dt;

            Vector3 e     = target - current,
                    de    = e - prevError,
                    deriv = de * oodt;

            prevError = e;
            errorIntegral += deriv;

            return P * e + I * errorIntegral + D * deriv;
        }
    }

    public class SBSVector3PID : IPIDController<SBSVector3>
    {
        public SBSVector3 GetForce(SBSVector3 current, SBSVector3 target, float dt, float P, float I, float D, ref SBSVector3 prevError, ref SBSVector3 errorIntegral)
        {
            if (dt < SBSMath.Epsilon)
                return SBSVector3.zero;

            float oodt = 1.0f / dt;

            SBSVector3 e     = target - current,
                       de    = e - prevError,
                       deriv = de * oodt;

            prevError = e;
            errorIntegral += deriv;

            return P * e + I * errorIntegral + D * deriv;
        }
    }
#else
    public class SBSPid { }
#endif
}
