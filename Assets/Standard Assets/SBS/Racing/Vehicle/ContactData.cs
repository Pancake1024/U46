using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class ContactData
    {
        #region Public const
        public enum SurfaceType
        {
            None,
            Asphalt,
            Grass,
            NumSurfaces
        }
        #endregion

        #region Private members
        private float wheelHeight;
        private SBSVector3 position;
        private SBSVector3 normal;
        private float bumpWaveLength;
        private float bumpAmplitude;
        private float frictionNoTread;
        private float frictionTread;
        private float rollingResistance;
        private float rollingDrag;
        private SurfaceType surfaceType;
        #endregion

        #region Ctors
        public ContactData()
        {
        }
        public ContactData(float _wheelHeight, SBSVector3 _position, SBSVector3 _normal, float _bumpWaveLength, float _bumpAmplitude, float _frictionNoTread, float _frictionTread, float _rollingResistance, float _rollingDrag, SurfaceType _surfaceType)
        {
            wheelHeight = _wheelHeight;
            position = _position;
            normal = _normal;
            bumpWaveLength = _bumpWaveLength;
            bumpAmplitude = _bumpAmplitude;
            frictionNoTread = _frictionNoTread;
            frictionTread = _frictionTread;
            rollingResistance = _rollingResistance;
            rollingDrag = _rollingDrag;
            surfaceType = _surfaceType;
        }
        #endregion

        #region Public methods
        public void Set(float _wheelHeight, SBSVector3 _position, SBSVector3 _normal, float _bumpWaveLength, float _bumpAmplitude, float _frictionNoTread, float _frictionTread, float _rollingResistance, float _rollingDrag, SurfaceType _surfaceType)
        {
            wheelHeight = _wheelHeight;
            position = _position;
            normal = _normal;
            bumpWaveLength = _bumpWaveLength;
            bumpAmplitude = _bumpAmplitude;
            frictionNoTread = _frictionNoTread;
            frictionTread = _frictionTread;
            rollingResistance = _rollingResistance;
            rollingDrag = _rollingDrag;
            surfaceType = _surfaceType;
        }
        #endregion

        #region Public properties
        public float WheelHeight
        {
            get
            {
                return wheelHeight;
            }
            set
            {
                wheelHeight = value;
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

        public SBSVector3 Normal
        {
            get
            {
                return normal;
            }
            set
            {
                normal = value;
            }
        }

        public float BumpWaveLength
        {
            get
            {
                return bumpWaveLength;
            }
            set
            {
                bumpWaveLength = value;
            }
        }

        public float BumpAmplitude
        {
            get
            {
                return bumpAmplitude;
            }
            set
            {
                bumpAmplitude = value;
            }
        }

        public float FrictionNoTread
        {
            get
            {
                return frictionNoTread;
            }
            set
            {
                frictionNoTread = value;
            }
        }

        public float FrictionTread
        {
            get
            {
                return frictionTread;
            }
            set
            {
                frictionTread = value;
            }
        }

        public float RollingResistance
        {
            get
            {
                return rollingResistance;
            }
            set
            {
                rollingResistance = value;
            }
        }

        public float RollingDrag
        {
            get
            {
                return rollingDrag;
            }
            set
            {
                rollingDrag = value;
            }
        }

        public SurfaceType TypeOfSurface
        {
            get
            {
                return surfaceType;
            }
            set
            {
                surfaceType = value;
            }
        }
        #endregion

        #region Public methods
        #endregion
    }
}
