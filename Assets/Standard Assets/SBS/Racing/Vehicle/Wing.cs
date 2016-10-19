using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Wing
    {
        #region Public const
        #endregion

        #region Private members
        private float airDensity;
        private float dragArea;
        private float dragCoeff;
        private float liftArea;
        private float liftCoeff;
        private float liftEfficiency;
        private SBSVector3 position;
        #endregion

        #region Ctors
        public Wing(float _airDensity, float _dragArea, float _dragCoeff, float _liftArea, float _liftCoeff, float _liftEfficency, SBSVector3 _position)
        {
            airDensity = _airDensity;
            dragArea = _dragArea;
            dragCoeff = _dragCoeff;
            liftArea = _liftArea;
            liftCoeff = _liftCoeff;
            liftEfficiency = _liftEfficency;
            position = _position;
        }
        #endregion

        #region Public properties
        public float AirDensity
        {
            get
            {
                return airDensity;
            }
            set
            {
                airDensity = value;
            }
        }

        public float DragArea
        {
            get
            {
                return dragArea;
            }
            set
            {
                dragArea = value;
            }
        }

        public float DragCoeff
        {
            get
            {
                return dragCoeff;
            }
            set
            {
                dragCoeff = value;
            }
        }

        public float LiftArea
        {
            get
            {
                return liftArea;
            }
            set
            {
                liftArea = value;
            }
        }

        public float LiftCoeff
        {
            get
            {
                return liftCoeff;
            }
            set
            {
                liftCoeff = value;
            }
        }

        public float LiftEfficiency
        {
            get
            {
                return liftEfficiency;
            }
            set
            {
                liftEfficiency = value;
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
        #endregion

        #region Public methods
        public SBSVector3 GetForce(SBSVector3 windVec, int idx)
        {
            //      Vector3 drag = windVec * windVec.magnitude * 0.5f * airDensity * dragCoeff * dragArea;
            SBSVector3 force = windVec * windVec.magnitude * 0.5f * airDensity * dragCoeff * dragArea;

            float windSpeed = -windVec.z;
            if (windSpeed < 0.0f)
                windSpeed = -windSpeed * 0.2f;

            float k = 0.5f * airDensity * windSpeed * windSpeed;
            float liftForce = k * liftCoeff * liftArea;
            float dragForce = -liftCoeff * liftForce * (1.0f - liftEfficiency);
            //      lift = new Vector3(0.0f, liftForce, -dragForce);
            force.y += liftForce;
            force.z -= dragForce;

            //DebugUtils.AddWatch(new Rect(0, (idx + 15) * 20, 600, 20), "windVec: {0} drag: {1} lift: {2}", windVec, drag, lift);

            return force;// drag + lift;
        }

        public float GetAerodynamicDownforceCoeff()
        {
            return 0.5f * airDensity * liftCoeff * liftArea;
        }

        public float GetAerodynamicDragCoeff()
        {
            return 0.5f * airDensity * (dragCoeff * dragArea + liftCoeff * liftCoeff * liftArea * (1.0f - liftEfficiency));
        }
        #endregion
    }
}
