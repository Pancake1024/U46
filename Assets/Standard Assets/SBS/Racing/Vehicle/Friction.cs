using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Friction
    {
        #region Public const
        #endregion

        #region Private members
        private float frictionCoeff;
        private float maxPressure;
        private float radius;
        private float area;
        private float threshold;

        private float position;
        private bool engaged;

        private float prevTorque;
        private float engineSpeed;
        private float driveSpeed;
        #endregion

        #region Ctors
        public Friction(float _friction, float _maxPressure, float _radius, float _area, float _threshold)
        {
            frictionCoeff = _friction;
            maxPressure = _maxPressure;
            radius = _radius;
            area = _area;
            threshold = _threshold;
            position = 0.0f;
            engaged = false;
        }
        #endregion

        #region Public properties
        public float FrictionCoeff
        {
            get
            {
                return frictionCoeff;
            }
            set
            {
                frictionCoeff = value;
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

        public float Position
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

        public bool Engaged
        {
            get
            {
                return engaged;
            }
        }
        #endregion

        #region Public methods
        public float GetTorque(float _engineSpeed, float _driveSpeed)
        {
            engineSpeed = _engineSpeed;
            driveSpeed = _driveSpeed;

            float normalForce = position * maxPressure * area;

            engaged = (SBSMath.Abs(engineSpeed - driveSpeed) < threshold * normalForce);
            if (engaged)
            {
                prevTorque = 0.0f;
                return prevTorque;
            }

            float force = frictionCoeff * normalForce;
            if (engineSpeed < driveSpeed)
                force = -force;

            prevTorque = force * radius;

            return prevTorque;
        }
        #endregion
    }
}
