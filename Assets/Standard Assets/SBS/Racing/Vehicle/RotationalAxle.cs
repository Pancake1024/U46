using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

namespace SBS.Racing
{
    public class RotationalAxle
    {
        #region Public const
        public const float RPM_TO_ANG_VEL = 2.0f * Mathf.PI / 60.0f;
        public const float ANG_VEL_TO_RPM = 1.0f / RPM_TO_ANG_VEL;
        #endregion

        #region Private members
        private SBSQuaternion orientation;
        private SBSVector3 angMomentum;
        private SBSVector3 torque;

        //  private SBSVector3 prevTorque;
        private SBSMatrix4x4 orientationMat;
        private SBSMatrix4x4 worldInverseInertiaTensor;
        private SBSVector3 angVel;

        private SBSMatrix4x4 inverseInertiaTensor;
        private SBSMatrix4x4 inertiaTensor;

        //  private bool prevTorqueDone;
        private int step;
        #endregion

        #region Private methods
        private void recalcSecondary()
        {/*
        prevTorque     = torque;
        prevTorqueDone = true;*/
            orientationMat.SetTRS(SBSVector3.zero, orientation, SBSVector3.one);
            worldInverseInertiaTensor = orientationMat.transposed * inverseInertiaTensor * orientationMat;
            angVel = worldInverseInertiaTensor.MultiplyVector(angMomentum);// * angMomentum;
        }

        private SBSQuaternion GetSpinFromMomentum(SBSVector3 _angMomentum)
        {
            //Debug.Log("*** _angMomentum: " + _angMomentum);
            SBSVector3 angVel = worldInverseInertiaTensor.MultiplyVector(_angMomentum);// * _angMomentum;//worldInverseInertiaTensor.m.MultiplyVector(_angMomentum.v);
            //Debug.Log("*** angVel: " + angVel);
            SBSQuaternion qav = new SBSQuaternion(angVel.x, angVel.y, angVel.z, 0.0f);
            SBSQuaternion tmp = qav * orientation;
            //tmp.ScaleBy(0.5f);
            tmp.x *= 0.5f;
            tmp.y *= 0.5f;
            tmp.z *= 0.5f;
            tmp.w *= 0.5f;
            return tmp;//new Quaternion(tmp.x * 0.5f, tmp.y * 0.5f, tmp.z * 0.5f, tmp.w * 0.5f);
            //return new Quaternion((qav.x + orientation.x) * 0.5f, (qav.y + orientation.y) * 0.5f, (qav.z + orientation.z) * 0.5f, (qav.w + orientation.w) * 0.5f);
        }

        private SBSVector3 TransformTorqueToLocal(SBSVector3 _torque)
        {
            return _torque;//new Vector3(_torque.x, _torque.y, _torque.z);
        }
        #endregion

        #region Public properties
        public SBSMatrix4x4 Inertia
        {
            get
            {
                return inertiaTensor;
            }
            set
            {
                inertiaTensor = value;
                inverseInertiaTensor = inertiaTensor.inverseFast;
            }
        }

        public SBSQuaternion Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
            }
        }

        public SBSVector3 AngularVelocity
        {
            get
            {
                return angVel;
            }
            set
            {
                angMomentum = inertiaTensor.MultiplyVector(value);//inverseInertiaTensor.inverse.MultiplyVector(value);
                recalcSecondary();
            }
        }

        public SBSVector3 Torque
        {
            get
            {
                return torque;
            }
            set
            {
                torque = TransformTorqueToLocal(value);
                step++;
            }
        }
        #endregion

        #region Public methods
        public RotationalAxle()
        {
            //      prevTorqueDone = false;
            step = 0;
            orientation = Quaternion.identity;// ALEX (NEEDS COPY)
            orientationMat = Matrix4x4.identity;// ALEX (NEEDS COPY)
            angVel = Vector3.zero;// ALEX (NEEDS COPY)
            angMomentum = Vector3.zero;// ALEX (NEEDS COPY)
            worldInverseInertiaTensor = Matrix4x4.identity;
        }

        public void Integrate1(float dt)
        {
            step++;
        }

        public void Integrate2(float dt)
        {/*
        orientation = MathUtils.Add(orientation, MathUtils.Scale(GetSpinFromMomentum(angMomentum + torque * dt * 0.5f), dt));
        orientation = MathUtils.Normalize(orientation);*/

            /*		orientation.IncrementBy(GetSpinFromMomentum(angMomentum + torque * dt * 0.5f) * dt);
                    orientation.Normalize();

                    angMomentum = angMomentum + torque * dt;
            */
            SBSVector3 halfTorqueDt = torque * (dt * 0.5f);
            angMomentum.x += halfTorqueDt.x;
            angMomentum.y += halfTorqueDt.y;
            angMomentum.z += halfTorqueDt.z;
            SBSQuaternion spin = GetSpinFromMomentum(angMomentum);
            orientation.x += (spin.x * dt);
            orientation.y += (spin.y * dt);
            orientation.z += (spin.z * dt);
            orientation.w += (spin.w * dt);
            orientation.Normalize();
            angMomentum.x += halfTorqueDt.x;
            angMomentum.y += halfTorqueDt.y;
            angMomentum.z += halfTorqueDt.z;

            recalcSecondary();

            step = 0;
        }

        public void SetInitialTorque(SBSVector3 _torque)
        {/*
        prevTorque = TransformTorqueToLocal ( _torque );
        prevTorqueDone = true;*/
        }
        #endregion
    }
}
