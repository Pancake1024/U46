using System;
using System.Collections.Generic;
using SBS.Math;

namespace SBS.Racing
{
    public enum VehicleType
    {
        RealRacing = 0,
        SimpleRacing,
        WaterRacing
    }

    public interface IVehiclePhysics
	{
        VehicleType Type { get; }
        float Mass { get; }
        SBSVector3 Velocity { get; }
        float CurrentSpeed { get; }
        float Gravity { get; }
        SBSMatrix4x4 Transform { get; }
        bool IsOnGround { get; }
        float AerodynamicDragCoeff { get; }
        float AerodynamicDownforceCoeff { get; }
        float LongitudinalFrictionCoeff { get; }
        float LateralFrictionCoeff { get; }
        float BestSteeringAngle { get; }
        float MaxSteeringAngle { get; }
	}
}
