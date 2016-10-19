using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Wheel
    {
        #region Public const
        #endregion

        #region Private members
        private SBSVector3 relaxPosition;
        private float rollHeight;
        private float mass;
        private RotationalAxle wheelAxle;

        private float driveTorque;
        private float brakeTorque;
        private float rollResistanceTorque;
        private float inertiaCache;
        private float steerAngle;

        private float angVel;

        #endregion

        #region Ctors
        public Wheel(float _rollHeight, float _mass, SBSVector3 _position, float _inertia)
        {
            rollHeight = _rollHeight;
            mass = _mass;
            relaxPosition = _position;

            driveTorque = 0.0f;
            brakeTorque = 0.0f;
            rollResistanceTorque = 0.0f;
            steerAngle = 0.0f;

            wheelAxle = new RotationalAxle();
            inertiaCache = _inertia;
            Inertia = _inertia;
        }
        #endregion

        #region Public properties
        public SBSVector3 RelaxPosition
        {
            get
            {
                return relaxPosition;
            }
            set
            {
                relaxPosition = value;
            }
        }

        public float RPM
        {
            get
            {
                return AngularVelocity * RotationalAxle.ANG_VEL_TO_RPM;
            }
        }

        public float AngVelInfo
        {
            get
            {
                return angVel;
            }
        }

        public float AngularVelocity
        {
            get
            {
                return wheelAxle.AngularVelocity.x;
            }
            set
            {
                wheelAxle.AngularVelocity = new SBSVector3(value, 0.0f, 0.0f);
            }
        }

        public float RollHeight
        {
            get
            {
                return rollHeight;
            }
            set
            {
                rollHeight = value;
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

        public float Inertia
        {
            get
            {
                return inertiaCache;
            }
            set
            {
                inertiaCache = value;
                wheelAxle.Inertia = SBSMatrix4x4.TRS(SBSVector3.zero, SBSQuaternion.identity, new SBSVector3(value, value, value));
            }
        }

        public float Torque
        {
            get
            {
                return wheelAxle.Torque.x;
            }
        }

        public float DriveTorque
        {
            get
            {
                return driveTorque;
            }
            set
            {
                driveTorque = value;
            }
        }

        public float BrakeTorque
        {
            get
            {
                return brakeTorque;
            }
            set
            {
                brakeTorque = value;
            }
        }

        public SBSQuaternion Orientation
        {
            get
            {
                return wheelAxle.Orientation;
            }
        }

        public float RollResistanceTorque
        {
            get
            {
                return rollResistanceTorque;
            }
            set
            {
                rollResistanceTorque = value;
            }
        }

        public float SteerAngle
        {
            get
            {
                return steerAngle;
            }
            set
            {
                steerAngle = value;
            }
        }

        #endregion

        #region Public methods
        public void SetInitialConditions()
        {
            wheelAxle.SetInitialTorque(Vector3.zero);//ALEX (NEEDS COPY)
        }

        public void Integrate1(float dt)
        {
            wheelAxle.Integrate1(dt);
        }

        public void Integrate2(float dt)
        {
            wheelAxle.Integrate2(dt);
        }

        public void Update(float dt, int idx)
        {
            float torque = driveTorque + brakeTorque + rollResistanceTorque;
            //Debug.Log("driveTorque: " + driveTorque + " brakeTorque: " + brakeTorque + " rollResistanceTorque: " + rollResistanceTorque);
            wheelAxle.Torque = new SBSVector3(torque, 0.0f, 0.0f);
            //wheelAxle.Torque = new Vector3(0.0f, -torque, 0.0f); // PIE

            //        DebugUtils.AddWatch(new Rect(0, (11 + idx) * 20, 600, 20), String.Format("driveTorque: {0} brakeTorque: {1} rollResistanceTorque: {2}", driveTorque, brakeTorque, rollResistanceTorque));

            angVel = AngularVelocity;
        }

        public void Reset()
        {
            wheelAxle.Torque = Vector3.zero;//ALEX (NEEDS COPY)
        }
        #endregion
    }
}
