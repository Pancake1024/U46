using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Engine
    {
        //public string debugStr = "";

        #region Public const
        #endregion

        #region Private members
        private float rpmRedline;
        private float rpmLimit;
        private float idleThrottle;
        private float rpmStart;
        private float rpmStall;
        private float friction;
        private float mass;
        private SBSVector3 position;
        private AnimationCurve torqueCurve;

        private float throttlePos;
        private float clutchTorque;
        private bool rpmLimitExceeded;
        private RotationalAxle engineAxle;

        private float frictionTorque;
        private float combustionTorque;
        private bool stalled;
        private bool powerLimit;
        private float extraTorque;
        #endregion

        #region Ctors
        public Engine(float _inertia, float _mass, SBSVector3 _position, float _rpmRedline, float _rpmLimit, float _idleThrottle, float _rpmStart, float _rpmStall,
                 float _friction)
        {
            engineAxle = new RotationalAxle();
            Inertia = _inertia;
            mass = _mass;
            position = _position;
            rpmRedline = _rpmRedline;
            rpmLimit = _rpmLimit;
            idleThrottle = _idleThrottle;
            rpmStart = _rpmStart;
            rpmStall = _rpmStall;
            friction = _friction;

            throttlePos = 0.0f;
            clutchTorque = 0.0f;
            rpmLimitExceeded = false;
            frictionTorque = 0.0f;
            combustionTorque = 0.0f;
            stalled = false;
            powerLimit = false;
            extraTorque = 0.0f;
        }
        #endregion

        #region Public properties
        public float Inertia
        {
            get
            {
                return engineAxle.Inertia.m00;
            }
            set
            {
                engineAxle.Inertia = SBSMatrix4x4.TRS(SBSVector3.zero, SBSQuaternion.identity, new SBSVector3(value, value, value));
            }
        }

        public float RPMRedline
        {
            get
            {
                return rpmRedline;
            }
            set
            {
                rpmRedline = value;
            }
        }

        public float RPMLimit
        {
            get
            {
                return rpmLimit;
            }
            set
            {
                rpmLimit = value;
            }
        }

        public float RPMStart
        {
            get
            {
                return rpmStart;
            }
            set
            {
                rpmStart = value;
            }
        }

        public float RPMStall
        {
            get
            {
                return rpmStall;
            }
            set
            {
                rpmStall = value;
            }
        }

        public float Throttle
        {
            get
            {
                return throttlePos;
            }
            set
            {
                throttlePos = value;
            }
        }

        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }

        public SBSVector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public float AngularVelocity
        {
            get
            {
                return engineAxle.AngularVelocity.x;
            }
            set
            {
                engineAxle.AngularVelocity = new SBSVector3(value, 0.0f, 0.0f);
            }
        }

        public float IdleThrottle
        {
            get
            {
                return idleThrottle;
            }
        }

        public float RPM
        {
            get
            {
                return engineAxle.AngularVelocity.x * RotationalAxle.ANG_VEL_TO_RPM;
            }
        }

        public float ClutchTorque
        {
            set
            {
                clutchTorque = value;
            }
        }

        public float Torque
        {
            get
            {
                return combustionTorque + frictionTorque;
            }
        }

        public bool Stalled
        {
            get
            {
                return stalled;
            }
            set
            {
                stalled = value;
            }
        }

        public bool PowerLimit
        {
            get
            {
                return powerLimit;
            }
            set
            {
                powerLimit = value;
            }
        }

        public float ExtraTorque
        {
            get
            {
                return extraTorque;
            }
            set
            {
                extraTorque = value;
            }
        }
        #endregion

        #region Public methods
        public void Refresh(float _inertia, float _mass, SBSVector3 _position, float _rpmRedline, float _rpmLimit, float _idleThrottle, float _rpmStart, float _rpmStall,
                 float _friction)
        {
            Inertia = _inertia;
            mass = _mass;
            position = _position;
            rpmRedline = _rpmRedline;
            rpmLimit = _rpmLimit;
            idleThrottle = _idleThrottle;
            rpmStart = _rpmStart;
            rpmStall = _rpmStall;
            friction = _friction;
        }

        public void SetInitialConditions()
        {
            engineAxle.SetInitialTorque(SBSVector3.zero);
            StartEngine();
        }

        public void StartEngine()
        {
            engineAxle.AngularVelocity = new SBSVector3(rpmStart * RotationalAxle.RPM_TO_ANG_VEL, 0.0f, 0.0f);
        }

        public void SetRPM(float rpm)
        {
            engineAxle.AngularVelocity = new SBSVector3(rpm * RotationalAxle.RPM_TO_ANG_VEL, 0.0f, 0.0f);
        }

        public void Integrate1(float dt)
        {
            engineAxle.Integrate1(dt);
        }

        public void Integrate2(float dt)
        {
            engineAxle.Integrate2(dt);
        }

        public float GetTorqueFromCurve(float _throttle, float _rpm)
        {
            if (_rpm < 1.0f)
                return 0.0f;

            float torque = torqueCurve.Evaluate(_rpm);

            return torque * _throttle;
        }

        public float GetFrictionTorque(float _angVel, float _frictionCoeff, float _throttle)
        {
            //return _angVel * (-1300.0f * friction) * (1.0f - _frictionCoeff * _throttle);

            /*
            debugStr = "friction = " + friction + "\n"
                     + "_frictionCoeff = " + _frictionCoeff + "\n"
                     + "_angVel = " + _angVel + "\n"
                     + "_throttle = " + _throttle + "\n"
                     + "(1.0f - _frictionCoeff * _throttle) * 0.25f = " + ((1.0f - _frictionCoeff * _throttle) * 0.25f) + "\n"
                     + "return = " + _angVel * (-1300.0f * friction) * (1.0f - _frictionCoeff * _throttle) * 0.25f;

            */
            return _angVel * (-1300.0f * friction) * (1.0f - _frictionCoeff * _throttle) * 0.25f;
        }

        public void Update(float dt)
        {
            SBSVector3 totalTorque = Vector3.zero;//ALEX (NEEDS COPY)

            stalled = (RPM < rpmStall);

            if (throttlePos < idleThrottle)
                throttlePos = idleThrottle;

            //engine friction
            float currentAngVel = engineAxle.AngularVelocity.x;

            //engine drive torque
            float frictionCoeff = 1.0f;
            float tmpRpmLimit = rpmLimit;
            if (rpmLimitExceeded)
                tmpRpmLimit -= 100.0f;

            rpmLimitExceeded = (RPM >= tmpRpmLimit);

            combustionTorque = GetTorqueFromCurve(throttlePos, RPM) * (1.0f + extraTorque);

            if (!rpmLimitExceeded && !stalled && !powerLimit)
            {
                totalTorque.x += combustionTorque;
            }
            else
            {
                frictionCoeff = 0.0f;
                combustionTorque = 0.0f;
            }

            frictionTorque = GetFrictionTorque(currentAngVel, frictionCoeff, throttlePos);
            

            if (stalled)
                frictionTorque *= 100.0f;

            //    if (rpmLimitExceeded)
            //       frictionTorque *= 0.4f;

            totalTorque.x += frictionTorque;
            totalTorque.x -= clutchTorque;

            //DebugUtils.AddWatch(new Rect(0, 19 * 20, 600, 20), String.Format("clutchTorque: {0} RPM: {1} combustionTorque: {2} tmpRpmLimit: {3}", clutchTorque, tmpRpmLimit, RPM, combustionTorque));

            //Debug.Log("totalTorque: " + totalTorque + " rpmLimitExceeded: " + rpmLimitExceeded + " RPM: " + RPM + " tmpRpmLimit: " + tmpRpmLimit);

            engineAxle.Torque = totalTorque;
        }

        public void SetTorqueCurve(float _maxPowerRPM, Keyframe[] _curveKeys, VehicleUtils.CurveSmooth _curveSmooth)
        {
            torqueCurve = VehicleUtils.CreateCurve(_curveKeys, _curveSmooth);

            // engine friction
            //float maxPowerAngVel = _maxPowerRPM * RotationalAxle.RPM_TO_ANG_VEL;
            //float maxPower = torqueCurve.Evaluate(_maxPowerRPM) * maxPowerAngVel;
            //friction = maxPower / SBSMath.Pow(maxPowerAngVel, 3);

            // idle throttle search
            for (idleThrottle = 0.0f; idleThrottle < 1.0f; idleThrottle += 0.01f)
            {
                float torqueAtIdle = GetTorqueFromCurve(idleThrottle, rpmStart);
                float frictionAtIdle = GetFrictionTorque(rpmStart * RotationalAxle.RPM_TO_ANG_VEL, 1.0f, idleThrottle);

                if (torqueAtIdle > -frictionAtIdle)
                    break;
            }
        }
        #endregion

        
    }

    
}
