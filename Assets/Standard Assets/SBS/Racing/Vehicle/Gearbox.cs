using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Racing
{
    public class Gearbox
    {
        #region Public const
        #endregion

        #region Private members
        private Dictionary<int, float> gearRatios;
        private int forwardGears;
        private int reverseGears;
        private float shiftDelay;

        private int currentGear;
        /*
        private float driveShaftRPM;
        private float engineShaftRPM;*/
        #endregion

        #region Ctors
        public Gearbox(float _shiftDelay, int _currentGear)
        {
            shiftDelay = _shiftDelay;
            currentGear = _currentGear;
/*          driveShaftRPM = 0.0f;
            engineShaftRPM = 0.0f;*/
            gearRatios = new Dictionary<int, float>();
        }

        public void Refresh(float _shiftDelay)
        {
            shiftDelay = _shiftDelay;
        }
        #endregion

        #region Public properties
        public int CurrentGear
        {
            get
            {
                return currentGear;
            }
        }

        public float ForwardGears
        {
            get
            {
                return forwardGears;
            }
        }

        public float ReverseGears
        {
            get
            {
                return reverseGears;
            }
        }

        public float ShiftDelay
        {
            get
            {
                return shiftDelay;
            }
            set
            {
                shiftDelay = value;
            }
        }
        #endregion

        #region Private methods
        private void InitGears()
        {
            forwardGears = 0;
            int tmpGear = 1;
            while (gearRatios.ContainsKey(tmpGear))
            {
                forwardGears++;
                tmpGear++;
            }

            reverseGears = 0;
            tmpGear = -1;
            while (gearRatios.ContainsKey(tmpGear))
            {
                reverseGears++;
                tmpGear--;
            }
        }
        #endregion

        #region Public methods
        public float GetTorque(float clutchTorque)
        {
            return clutchTorque * gearRatios[currentGear];
        }

        public void ChangeGear(int gear)
        {
            if (gear <= forwardGears && gear >= -reverseGears)
                currentGear = gear;
        }

        public void SetGearRatio(int gear, float ratio)
        {
            gearRatios[gear] = ratio;

            InitGears();
        }

        public float GetGearRatio(int gear)
        {
            float result = 1.0f;
            if (gear <= forwardGears && gear >= -reverseGears)
                result = gearRatios[gear];

            return result;
        }

        public float UpdateClutchSpeed(float driveShaftSpeed)
        {
            //driveShaftRPM = driveShaftSpeed * VehicleUtils.ToRPM;
            //engineShaftRPM = driveShaftSpeed * gearRatios[currentGear] * VehicleUtils.ToRPM;
            return GetClutchSpeed(driveShaftSpeed);
        }

        public float GetClutchSpeed(float driveShaftSpeed)
        {
            return driveShaftSpeed * gearRatios[currentGear];
        }
        #endregion
    }
}
