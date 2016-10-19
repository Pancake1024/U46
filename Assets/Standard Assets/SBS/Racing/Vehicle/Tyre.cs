using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Tyre
    {
        #region Public const
        #endregion

        #region Public members
        public float[] longitudinalParams;
        public float[] lateralParams;
        public float[] alignParams;
        #endregion

        #region Private members
        private float radius;
        private float tread;
        private float rollResistance;
        private float rollResistanceCoeff;

        private AnimationCurve sigmaMaxCurve;
        private AnimationCurve alphaMaxCurve;

        private float feedbackEffect;
        private float slideBias;

        private float slide;
        private float slip;
        private float slideRatio;
        private float slipRatio;
        #endregion

        #region Ctors
        public Tyre()
        {
            slide = 0.0f;
            slip = 0.0f;

            longitudinalParams = new float[11];
            lateralParams = new float[15];
            alignParams = new float[18];

            slideBias = 1.0f;
        }
        #endregion

        #region Public properties
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
            }
        }

        public float Tread
        {
            get
            {
                return tread;
            }
            set
            {
                tread = value;
            }
        }

        public float Slide
        {
            get
            {
                return slide;
            }
            set
            {
                slide = value;
            }
        }

        public float Slip
        {
            get
            {
                return slip;
            }
            set
            {
                slip = value;
            }
        }

        public float SlideRatio
        {
            get
            {
                return slideRatio;
            }
        }

        public float SlipRatio
        {
            get
            {
                return slipRatio;
            }
        }

        public float FeedbackEffect
        {
            get
            {
                return feedbackEffect;
            }
            set
            {
                feedbackEffect = value;
            }
        }

        public float SlideBias
        {
            get
            {
                return slideBias;
            }
            set
            {
                slideBias = value;
            }
        }
        #endregion

        #region Private methods
        private float CalculateSigmaMax(float normalForce)
        {
            float result = 0.0f;
            float maxForce = 0.0f;
            float xMin = -2.0f;
            float xMax = 2.0f;
            float xStep = (xMax - xMin) / 400.0f;

            for (float x = xMin; x < xMax; x += xStep)
            {
                float force = PacejkaLongitudinal(x, normalForce, 1.0f);
                if (force > maxForce)
                {
                    result = x;
                    maxForce = force;
                }
            }

            return result;
        }

        private float CalculateAlphaMax(float normalForce)
        {
            float result = 0.0f;
            float maxForce = 0.0f;
            float xMin = -20.0f;
            float xMax = 20.0f;
            float xStep = (xMax - xMin) / 400.0f;

            for (float x = xMin; x < xMax; x += xStep)
            {
                float force = PacejkaLateral(x, normalForce, 0.0f, 1.0f);
                if (force > maxForce)
                {
                    result = x;
                    maxForce = force;
                }
            }

            return result;
        }
        #endregion

        #region Public methods
        public void SetRollResistance(float _rollResistance, float _rollResistanceCoeff)
        {
            rollResistance = _rollResistance;
            rollResistanceCoeff = _rollResistanceCoeff;
        }

        public void SetParams(float[] _longitudinalParams, float[] _lateralParams, float[] _alignParams)
        {
            longitudinalParams = _longitudinalParams;
            lateralParams = _lateralParams;
            alignParams = _alignParams;
        }

        public float PacejkaLongitudinal(float sigma, float normalForceKN, float frictionCoeff)
        {
            float[] p = longitudinalParams;

            float D = (p[1] * normalForceKN + p[2]) * normalForceKN * frictionCoeff;
            float B = (p[3] * normalForceKN + p[4]) * SBSMath.Exp(-p[5] * normalForceKN) / (p[0] * (p[1] * normalForceKN + p[2]));
            float E = (p[6] * normalForceKN + p[7]) * normalForceKN + p[8];
            float S = p[9] * normalForceKN + p[10] + 100.0f * sigma;

            float longitudinalForce = D * SBSMath.Sin(p[0] * SBSMath.Atan(S * B + E * (SBSMath.Atan(S * B) - S * B)));

            return longitudinalForce;
        }

        public float PacejkaLateral(float alpha, float normalForceKN, float gamma, float frictionCoeff)
        {
            float[] p = lateralParams;

            float D = (p[1] * normalForceKN + p[2]) * normalForceKN * frictionCoeff;
            float B = p[3] * SBSMath.Sin(2.0f * SBSMath.Atan(normalForceKN / p[4])) * (1.0f - p[5] * SBSMath.Abs(gamma)) / (p[0] * (p[1] * normalForceKN + p[2]) * normalForceKN);
            float E = p[6] * normalForceKN + p[7];
            float S = alpha + p[8] * gamma + p[9] * normalForceKN + p[10];
            float Sv = ((p[11] * normalForceKN + p[12]) * gamma + p[13]) * normalForceKN + p[14];

            float lateralForce = D * SBSMath.Sin(p[0] * SBSMath.Atan(S * B + E * (SBSMath.Atan(S * B) - S * B))) + Sv;

            return lateralForce;
        }

        public float PacejkaAlign(float sigma, float alpha, float normalForceKN, float gamma, float frictionCoeff)
        {
            float[] p = alignParams;

            float D = (p[1] * normalForceKN + p[2]) * normalForceKN * frictionCoeff;
            float B = (p[3] * normalForceKN + p[4]) * normalForceKN * (1.0f - p[6] * SBSMath.Abs(gamma)) * SBSMath.Exp(-p[5] * normalForceKN) / (p[0] * D);
            float E = ((p[7] * normalForceKN + p[8]) * normalForceKN + p[9]) * (1.0f - p[10] * SBSMath.Abs(gamma));
            float S = alpha + p[11] * gamma + p[12] * normalForceKN + p[13];
            float Sv = ((p[14] * normalForceKN + p[15]) * gamma + p[16]) * normalForceKN + p[17];

            float alignForce = D * SBSMath.Sin(p[0] * SBSMath.Atan(S * B + E * (SBSMath.Atan(S * B) - S * B))) + Sv;

            return alignForce;
        }

        public float GetMaximumLongitudinalForce(float normalForce)
        {
            float[] p = longitudinalParams;
            float normalForceKN = normalForce * VehicleUtils.ToKiloNewtons;
            float D = (p[1] * normalForceKN + p[2]) * normalForceKN;

            return D;
        }

        public float GetMaximumLateralForce(float normalForce, float camber)
        {
            float[] p = lateralParams;
            float normalForceKN = normalForce * VehicleUtils.ToKiloNewtons;
            float gamma = camber * SBSMath.ToDegrees;

            float D = (p[1] * normalForceKN + p[2]) * normalForceKN;
            float Sv = ((p[11] * normalForceKN + p[12]) * gamma + p[13]) * normalForceKN + p[14];

            return D + Sv;
        }

        public float GetMaximumAlignForce(float normalForce, float camber)
        {
            float[] p = alignParams;
            float normalForceKN = normalForce * VehicleUtils.ToKiloNewtons;
            float gamma = camber * SBSMath.ToDegrees;

            float D = (p[1] * normalForceKN + p[2]) * normalForceKN;
            float Sv = ((p[14] * normalForceKN + p[15]) * gamma + p[16]) * normalForceKN + p[17];

            return D + Sv;
        }

        public SBSVector3 GetForces(float normalForce, float frictionCoeff, SBSVector3 hubVelocity, float patchSpeed, float camber, int idx)
        {
            float sigmaMax = GetSigmaMax(normalForce);
            float alphaMax = GetAlphaMax(normalForce);

            float normalForceKN = SBSMath.Min(normalForce * VehicleUtils.ToKiloNewtons, 30.0f);

            if (normalForceKN < SBSMath.Epsilon)
                return Vector3.zero;//ALEX (NEEDS COPY)

            float vel = hubVelocity.z;
            float posVel = SBSMath.Max(SBSMath.Abs(vel), 0.1f); //MathUtils.Epsilon);// 0.1f);

            float sigma = (patchSpeed - vel) / posVel;//10.0f; //(patchSpeed - vel) / posVel;
            float alpha = -SBSMath.Atan2(hubVelocity.x, posVel) * SBSMath.ToDegrees;//LEFTHAND
            float gamma = camber * SBSMath.ToDegrees;

            //precomb
            slideRatio = sigma / sigmaMax;
            slipRatio = alpha / alphaMax;
            float scaleFactor = SBSMath.Max(SBSMath.Sqrt(slideRatio * slideRatio + slipRatio * slipRatio), SBSMath.Epsilon);

            //float longitidinalForce = (sigma / scaleFactor) * PacejkaLongitudinal(scaleFactor * sigmaMax, normalForceKN, frictionCoeff);
            //        float longitidinalForce = (sigma / scaleFactor) * PacejkaLongitudinal(scaleFactor * sigmaMax, normalForceKN, frictionCoeff);
            //        float lateralForce = (alpha / scaleFactor) * PacejkaLateral(scaleFactor * alphaMax, normalForceKN, gamma, frictionCoeff);
            float longitidinalForce = (slideRatio / scaleFactor) * PacejkaLongitudinal(scaleFactor * sigmaMax, normalForceKN, frictionCoeff);
            float lateralForce = (slipRatio / scaleFactor) * PacejkaLateral(scaleFactor * alphaMax, normalForceKN, gamma, frictionCoeff);

            float alignForce = PacejkaAlign(sigma, alpha, normalForceKN, gamma, frictionCoeff);

            slide = sigma;
            slip = alpha;

            /*
            if (idx == 0)
            {
                DebugUtils.AddWatch(new Rect(0, (7 ) * 20, 600, 20), String.Format("slide: {0} slip: {1} sigmaMax: {2} alphaMax: {3} nf: {4}", slide, slip, sigmaMax, alphaMax, normalForceKN));
                DebugUtils.AddWatch(new Rect(0, (8) * 20, 600, 20), String.Format("slideRatio: {0} patchSpeed - vel: {1} ", slideRatio, (patchSpeed - vel)));
            }
            */

            //DebugUtils.AddWatch(new Rect(0, (20 + idx) * 20, 600, 20), String.Format("slide: {0} slip: {1} sigmaMax: {2} alphaMax: {3} nf: {4}", slide, slip, sigmaMax, alphaMax, normalForceKN));
            //DebugUtils.AddWatch(new Rect(0, (16 + (idx - 1) * 2) * 20, 600, 20), String.Format("realSigma: {0} slide: {1} slip: {2} slideRatio: {3}  slipRatio: {4}", realSigma, slide, slip, slideRatio, slipRatio));
            //DebugUtils.AddWatch(new Rect(0, (16 + (idx - 1) * 2 + 1) * 20, 600, 20), String.Format("sigmaMax: {0} alphaMax: {1} hubVelocity: {2} patchSpeed: {3}", sigmaMax, alphaMax, hubVelocity, patchSpeed));        
            //return new Vector3(longitidinalForce, lateralForce, alignForce);
            return new SBSVector3(lateralForce, alignForce, longitidinalForce);
        }

        public void InitSigmaMaxCurve(int samples)
        {
            float loadStep = 0.5f;

            List<Keyframe> curveKeys = new List<Keyframe>();
            for (int i = 0; i < samples; i++)
            {
                float normalForceKN = (float)(i + 1) * loadStep;
                float sigmaMax = CalculateSigmaMax(normalForceKN);

                curveKeys.Add(new Keyframe(normalForceKN, sigmaMax));
            }

            sigmaMaxCurve = VehicleUtils.CreateCurve(curveKeys.ToArray(), VehicleUtils.CurveSmooth.Linear);
        }

        public void InitAlphaMaxCurve(int samples)
        {
            float loadStep = 0.5f;

            List<Keyframe> curveKeys = new List<Keyframe>();
            for (int i = 0; i < samples; i++)
            {
                float normalForceKN = (float)(i + 1) * loadStep;
                float alphaMax = CalculateAlphaMax(normalForceKN);

                curveKeys.Add(new Keyframe(normalForceKN, alphaMax));
            }

            alphaMaxCurve = VehicleUtils.CreateCurve(curveKeys.ToArray(), VehicleUtils.CurveSmooth.Linear);
        }

        public float GetSigmaMax(float normalForce)
        {
            float normalForceKN = normalForce * VehicleUtils.ToKiloNewtons;
            return sigmaMaxCurve.Evaluate(normalForceKN);
        }

        public float GetAlphaMax(float normalForce)
        {
            float normalForceKN = normalForce * VehicleUtils.ToKiloNewtons;
            return alphaMaxCurve.Evaluate(normalForceKN);
        }

        public float GetBestSteeringAngle(float normalForce)
        {
            return GetAlphaMax(normalForce);
        }

        public float GetRollResistance(float speed, float rollResistanceFactor)
        {
            return rollResistanceFactor * ((speed >= 0.0f ? rollResistance : -rollResistance) + rollResistanceCoeff * speed * speed);
        }
        #endregion
    }
}
