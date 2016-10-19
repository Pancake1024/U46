using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Managers/VehiclesManager")]
public class VehiclesManager : MonoBehaviour
{
    #region Singleton instance
    protected static VehiclesManager instance = null;

    public static VehiclesManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public enums/classes/structs
    public enum WheelsPosition
    {
        Front = 0,
        Rear,
        Custom0,
        Custom1,
        Count
    }

    [Serializable]
    public class WheelsDesc
    {
        public int index;
        public int offset;
        public int count;

        public WheelsDesc(int _index /*= 0*/, int _offset/* = 0*/, int _count/* = 4*/)
        {
            index = _index;
            offset = _offset;
            count = _count;
        }
    }

    public class VehicleSimpleDesc
    {
        public Rigidbody rigidbody;
        public VehiclePhysicsSimple physics;
        public WheelRay[] wheels;
        public WheelsDesc[] wheelsDesc;
    }
    #endregion

    #region Protected members
    protected Dictionary<GameObject, VehicleSimpleDesc> vehSimpleList = new Dictionary<GameObject,VehicleSimpleDesc>();
    protected Dictionary<GameObject, IVehiclePhysics> interfaces = new Dictionary<GameObject, IVehiclePhysics>();
    #endregion

    #region Public methods
    public void AddVehicle(GameObject vehicle)
    {
        VehiclePhysicsSimple vehSimple = vehicle.GetComponent<VehiclePhysicsSimple>();
        VehiclePhysics veh = vehicle.GetComponent<VehiclePhysics>();

        if (vehSimple != null)
        {
            VehicleSimpleDesc desc = new VehicleSimpleDesc();
            desc.rigidbody = vehicle.GetComponent<Rigidbody>();
            desc.physics = vehSimple;
            desc.wheels = vehicle.GetComponents<WheelRay>();
            desc.wheelsDesc = new WheelsDesc[(int)WheelsPosition.Count];
            vehSimpleList.Add(vehicle, desc);
        }

        interfaces.Add(vehicle, (null == vehSimple ? veh as IVehiclePhysics : vehSimple as IVehiclePhysics));
    }

    public bool HasVehicle(GameObject vehicle)
    {
        return interfaces.ContainsKey(vehicle);
    }

    public IVehiclePhysics GetVehicleInterface(GameObject vehicle)
    {
        return interfaces[vehicle];
    }

    public void RemoveVehicle(GameObject vehicle)
    {
        if (vehSimpleList.ContainsKey(vehicle))
            vehSimpleList.Remove(vehicle);

        interfaces.Remove(vehicle);
    }

    public void SetWheels(GameObject vehicle, WheelsPosition position, WheelsDesc wheelsDesc)
    {
        VehicleSimpleDesc desc = vehSimpleList[vehicle];
        desc.wheelsDesc[(int)position] = wheelsDesc;

        switch (position)
        {
            case WheelsPosition.Front:
                Vector3 frontPt = Vector3.zero;
                for (int i = 0; i < wheelsDesc.count; ++i)
                    frontPt += desc.wheels[wheelsDesc.index].positions[wheelsDesc.offset + i];
                frontPt /= (float)wheelsDesc.count;
                desc.physics.frontPoint = frontPt;
                break;
            case WheelsPosition.Rear:
                Vector3 rearPt = Vector3.zero;
                for (int i = 0; i < wheelsDesc.count; ++i)
                    rearPt += desc.wheels[wheelsDesc.index].positions[wheelsDesc.offset + i];
                rearPt /= (float)wheelsDesc.count;
                desc.physics.rearPoint = rearPt;
                break;
            default:
                break;
        }
    }

    public SBSVector3 GetWheelLocalPosition(GameObject vehicle, int wheelIndex)
    {
        IVehiclePhysics vehPhys = null;
        if (interfaces.TryGetValue(vehicle, out vehPhys))
        {
            switch (vehPhys.Type)
            {
                case VehicleType.WaterRacing:
                case VehicleType.SimpleRacing:
                    VehicleSimpleDesc desc = vehSimpleList[vehicle];
                    int i = 0, j = 0, c = desc.wheels[0].positions.Length;
                    while (wheelIndex >= (i + c))
                    {
                        i += c;
                        ++j;
                        c = desc.wheels[j].positions.Length;
                    }
                    return desc.wheels[j].positions[wheelIndex - i];
                case VehicleType.RealRacing:
                    VehiclePhysics comp = vehPhys as VehiclePhysics;
                    return comp.Transform.inverseFast * comp.GetWheelPosition(wheelIndex);
                default:
                    return SBSVector3.zero;
            }
        }
        else
            return SBSVector3.zero;
    }

    public SBSVector3 GetWheelWorldPosition(GameObject vehicle, int wheelIndex)
    {
        IVehiclePhysics vehPhys = null;
        if (interfaces.TryGetValue(vehicle, out vehPhys))
        {
            switch (vehPhys.Type)
            {
                case VehicleType.WaterRacing:
                case VehicleType.SimpleRacing:
                    VehicleSimpleDesc desc = vehSimpleList[vehicle];
                    int i = 0, j = 0, c = desc.wheels[0].positions.Length;
                    while (wheelIndex >= (i + c))
                    {
                        i += c;
                        ++j;
                        c = desc.wheels[j].positions.Length;
                    }
                    return desc.wheels[j].GetWorldPosition(wheelIndex - i);
                case VehicleType.RealRacing:
                    VehiclePhysics comp = vehPhys as VehiclePhysics;
                    return comp.GetWheelPositionPrecomputed(wheelIndex);
                default:
                    return SBSVector3.zero;
            }
        }
        else
            return SBSVector3.zero;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        foreach (VehicleSimpleDesc desc in vehSimpleList.Values)
        {
            if (null == desc.physics || !desc.physics.enabled)
                continue;

            float currSpeed = desc.physics.CurrentSpeed;

            SBSVector3 force  = SBSVector3.zero,
                       torque = SBSVector3.zero;

            foreach (WheelRay wheelRay in desc.wheels)
                wheelRay.AccumForces(dt, ref force, ref torque);

            SBSVector3 netForce = SBSVector3.zero;
            int i = 0;
            bool wheelsOnGround = false;
            for (WheelsPosition p = 0; p < WheelsPosition.Count; ++p)
            {
                WheelsDesc wheelsDesc = desc.wheelsDesc[(int)p];
                if (null == wheelsDesc)
                    continue;

                for (; i < wheelsDesc.count; ++i)
                {
                    WheelRay wheelRay = null;
                    if (wheelsDesc.index >= 0)
                    {
                        wheelRay = desc.wheels[wheelsDesc.index];
                        netForce += wheelRay.GetLastForce(wheelsDesc.offset + i);
                        wheelsOnGround |= wheelRay.GetWheelOnGround(i);
                    }

                    Transform node = wheelRay.nodes[wheelsDesc.offset + i];
                    if (null == node)
                        continue;

                    if (wheelRay != null)
                        node.position = wheelRay.GetWorldPosition(wheelsDesc.offset + i);

                    node.Rotate(Vector3.right, (currSpeed * dt / wheelRay.radius) * Mathf.Rad2Deg, Space.Self);
                }

                switch (p)
                {
                    case WheelsPosition.Front:
                        desc.physics.frontWeight = SBSMath.Max(0.0f, SBSVector3.Dot(netForce, SBSVector3.up));
                        desc.physics.frontWheelsOnGround = wheelsOnGround;
                        break;
                    case WheelsPosition.Rear:
                        desc.physics.rearWeight = SBSMath.Max(0.0f, SBSVector3.Dot(netForce, SBSVector3.up));
                        desc.physics.rearWheelsOnGround = wheelsOnGround;
                        break;
                    default:
                        break;
                }

                netForce.Set(0.0f, 0.0f, 0.0f);
                i = 0;
                wheelsOnGround = false;
            }

            if (desc.rigidbody.isKinematic)
                continue;

            desc.physics.AccumForces(dt, ref force, ref torque);

            desc.rigidbody.AddForce(force, ForceMode.Force);
            desc.rigidbody.AddTorque(torque, ForceMode.Force);
        }
    }
    #endregion
}
