using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Differential
    {
        #region Public const
        #endregion

        #region Private members
        private float finalDrive;
        private float antiSlip;
        private float antiSlipTorque;
        private float antiSlipTorqueDecCoeff;
        private float torqueSplit;

        private float sideLeftSpeed;
        private float sideRightSpeed;
        private float sideLeftTorque;
        private float sideRightTorque;
        #endregion

        #region Ctors
        public Differential(float _finalDrive, float _antiSlip, float _antiSlipTorque, float _antiSlipTorqueDecCoeff, float _torqueSplit)
        {
            finalDrive = _finalDrive;
            antiSlip = _antiSlip;
            antiSlipTorque = _antiSlipTorque;
            antiSlipTorqueDecCoeff = _antiSlipTorqueDecCoeff;
            torqueSplit = _torqueSplit;
            sideLeftSpeed = 0.0f;
            sideRightSpeed = 0.0f;
            sideLeftTorque = 0.0f;
            sideRightTorque = 0.0f;
        }
        #endregion

        #region Public properties
        public float TorqueSplit
        {
            get
            {
                return torqueSplit;
            }
            set
            {
                torqueSplit = value;
            }
        }

        public float FinalDrive
        {
            get
            {
                return finalDrive;
            }
            set
            {
                finalDrive = value;
            }
        }

        public float AntiSlip
        {
            get
            {
                return antiSlip;
            }
            set
            {
                antiSlip = value;
            }
        }

        public float SideLeftTorque
        {
            get
            {
                return sideLeftTorque;
            }
        }

        public float SideRightTorque
        {
            get
            {
                return sideRightTorque;
            }
        }

        public float SideLeftSpeed
        {
            get
            {
                return sideLeftSpeed;
            }
        }

        public float SideRightSpeed
        {
            get
            {
                return sideRightSpeed;
            }
        }

        public float DriveshaftSpeed
        {
            get
            {
                return finalDrive * (sideLeftSpeed + sideRightSpeed) * 0.5f;
            }
        }
        #endregion

        #region Public methods
        public float UpdateDriveshaftSpeed(float _sideLeftSpeed, float _sideRightSpeed)
        {
            sideLeftSpeed = _sideLeftSpeed;
            sideRightSpeed = _sideRightSpeed;

            return DriveshaftSpeed;
        }

        public void UpdateWheelTorques(float driveshaftTorque)
        {
            float currAntiSlip = antiSlip;
            if (antiSlipTorque > 0.0f)
                currAntiSlip = antiSlipTorque * driveshaftTorque;

            if (currAntiSlip < 0.0f)
                currAntiSlip *= -antiSlipTorqueDecCoeff;

            currAntiSlip = SBSMath.Max(currAntiSlip, 0.0f);

            float diffDrag = Mathf.Clamp(currAntiSlip * (sideLeftSpeed - sideRightSpeed), -antiSlip, antiSlip);
            float diffTorque = driveshaftTorque * finalDrive;

            sideLeftTorque = diffTorque * (1.0f - torqueSplit) - diffDrag;
            sideRightTorque = diffTorque * torqueSplit + diffDrag;
        }
        #endregion
    }
}
