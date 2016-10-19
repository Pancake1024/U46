using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Brake
    {
        #region Public const
        #endregion

        #region Private members
        private float friction;
        private float maxPressure;
        private float radius;
        private float area;
        private float bias;
        private float threshold;
        private float handbrake;

        private float brakeCoeff;
        private float handbrakeCoeff;
        private bool locked;
        #endregion

        #region Ctors
        public Brake(float _friction, float _maxPressure, float _radius, float _area, float _bias, float _threshold, float _handbrake)
        {
            friction = _friction;
            maxPressure = _maxPressure;
            radius = _radius;
            area = _area;
            bias = _bias;
            threshold = _threshold;
            handbrake = _handbrake;
            brakeCoeff = 0.0f;
            handbrakeCoeff = 0.0f;
            locked = false;
        }
        #endregion

        #region Public properties
        public float BrakeCoeff
        {
            get
            {
                return brakeCoeff;
            }
            set
            {
                brakeCoeff = value;
            }
        }

        public float Friction
        {
            get
            {
                return friction;
            }
            set
            {
                friction = value;
            }
        }

        public float MaxPressure
        {
            get
            {
                return maxPressure;
            }
            set
            {
                maxPressure = value;
            }
        }

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

        public float Area
        {
            get
            {
                return area;
            }
            set
            {
                area = value;
            }
        }

        public float Bias
        {
            get
            {
                return bias;
            }
            set
            {
                bias = value;
            }
        }

        public bool Locked
        {
            get
            {
                return locked;
            }
        }

        public float Handbrake
        {
            get
            {
                return handbrake;
            }
            set
            {
                handbrake = value;
            }
        }

        public float HandbrakeCoeff
        {
            get
            {
                return handbrakeCoeff;
            }
            set
            {
                handbrakeCoeff = value;
            }
        }
        #endregion

        #region Public methods
        public float GetTorque(float rotSpeed, int idx)
        {
            float brake = SBSMath.Max(brakeCoeff, handbrakeCoeff * handbrake);

            float pressure = brake * bias * maxPressure;
            float normalForce = pressure * area;
            float torque = friction * normalForce * radius;
            float velocity = radius * rotSpeed;
            if (velocity < 0.0f)
                torque = -torque;

            locked = (SBSMath.Abs(velocity) < threshold * normalForce);
            if (locked)
                torque = 0.0f;

            //DebugUtils.AddWatch(new Rect(0, (11 + idx) * 20, 600, 20), String.Format("brake: {0} bias: {1} pressure: {2} maxPressure: {3} torque: {4}", brake, bias, pressure, maxPressure, torque));

            return torque;
        }

        // TODO
        public bool WillLock(float rotSpeed)
        {
            float brake = SBSMath.Max(brakeCoeff, handbrakeCoeff);

            float pressure = brake * bias * maxPressure;
            float normalForce = pressure * area;
            float torque = friction * normalForce * radius;
            float velocity = radius * rotSpeed;
            if (velocity < 0.0f)
                torque = -torque;

            return (SBSMath.Abs(velocity) < threshold * normalForce);
        }
        #endregion
    }
}