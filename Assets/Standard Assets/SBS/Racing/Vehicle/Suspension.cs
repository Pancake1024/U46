using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class Suspension
    {
        #region Public const
        #endregion

        #region Private members
        private SBSVector3 hinge;
        private float springCoeff;
        private float bounce;
        private float rebound;
        private float maxVelocity;
        private float maxForce;
        private float travel;
        private float antirollCoeff;
        private float camber;
        private float caster;
        private float toe;

        private float displacement;
        private float prevDisplacement;
        private float overTravel;

        private float velocity;
        private float dampForce;
        private float springForce;
        private float antirollForce;

        private float restitution;
        private float penalty;
        #endregion

        #region Ctors
        public Suspension(float _springCoeff, float _bounce, float _rebound, float _travel, float _antirollCoeff,
            float _camber, float _caster, float _toe, float _restitution, float _penalty)
        {
            springCoeff = _springCoeff;
            bounce = _bounce;
            rebound = _rebound;
            travel = _travel;
            antirollCoeff = _antirollCoeff;
            camber = _camber;
            caster = _caster;
            toe = _toe;

            displacement = 0.0f;
            prevDisplacement = 0.0f;
            overTravel = 0.0f;
            velocity = 0.0f;
            dampForce = 0.0f;
            springForce = 0.0f;
            antirollForce = 0.0f;

            restitution = _restitution;
            penalty = _penalty;
        }
        #endregion

        #region Public properties
        public SBSVector3 Hinge
        {
            get
            {
                return hinge;
            }
            set
            {
                hinge = value;
            }
        }

        public float SpringCoeff
        {
            get
            {
                return springCoeff;
            }
            set
            {
                springCoeff = value;
            }
        }

        public float Bounce
        {
            get
            {
                return bounce;
            }
            set
            {
                bounce = value;
            }
        }

        public float Rebound
        {
            get
            {
                return rebound;
            }
            set
            {
                rebound = value;
            }
        }

        public float MaxVelocity
        {
            get
            {
                return maxVelocity;
            }
            set
            {
                maxVelocity = value;
            }
        }

        public float MaxForce
        {
            get
            {
                return maxForce;
            }
            set
            {
                maxForce = value;
            }
        }

        public float Travel
        {
            get
            {
                return travel;
            }
            set
            {
                travel = value;
            }
        }

        public float AntirollCoeff
        {
            get
            {
                return antirollCoeff;
            }
            set
            {
                antirollCoeff = value;
            }
        }

        public float Camber
        {
            get
            {
                return camber;
            }
            set
            {
                camber = value;
            }
        }

        public float Caster
        {
            get
            {
                return caster;
            }
            set
            {
                caster = value;
            }
        }

        public float Toe
        {
            get
            {
                return toe;
            }
            set
            {
                toe = value;
            }
        }

        public float Displacement
        {
            get
            {
                return displacement;
            }
            set
            {
                prevDisplacement = displacement;
                displacement = Mathf.Clamp(value, 0.0f, travel);
                overTravel = SBSMath.Max(value - travel, 0.0f);
            }
        }

        public float DisplacementPerc
        {
            get
            {
                return displacement / travel;
            }
            set
            {
                displacement = travel * Mathf.Clamp01(value);
            }
        }

        public float PrevDisplacement
        {
            get
            {
                return prevDisplacement;
            }
        }

        public float Velocity
        {
            get
            {
                return velocity;
            }
        }

        public float DampForce
        {
            get
            {
                return dampForce;
            }
        }

        public float SpringForce
        {
            get
            {
                return springForce;
            }
        }

        public float AntirollForce
        {
            get
            {
                return antirollForce;
            }
            set
            {
                antirollForce = value;
            }
        }

        public float Overtravel
        {
            get
            {
                return overTravel;
            }
        }

        public float Restitution
        {
            get
            {
                return restitution;
            }
            set
            {
                restitution = value;
            }
        }

        public float Penalty
        {
            get
            {
                return penalty;
            }
            set
            {
                penalty = value;
            }
        }
        #endregion

        #region Public methods
        public float GetForce(float dt, int idx, float vel)
        {
            velocity = (displacement - prevDisplacement) / dt;//displacement > 0.0f ? vel : 0.0f;

            float damping = bounce;
            if (velocity < 0.0f)
                damping = rebound;

            springForce = displacement * springCoeff;
            dampForce = velocity * damping;

            /*     if ( idx == 0 )
                    {
                        DebugUtils.AddWatch(new Rect(0, 15 * 20, 600, 20), String.Format("displacement: {0} springForce: {1}", displacement, springForce));
                        DebugUtils.AddWatch(new Rect(0, 16 * 20, 600, 20), String.Format("velocity: {0} velocity: {1}", velocity, velocity));
                    }
                    */
            return maxForce >= 0.0f ? SBSMath.Min(maxForce, springForce + dampForce) : springForce + dampForce;
        }
        #endregion
    }
}
