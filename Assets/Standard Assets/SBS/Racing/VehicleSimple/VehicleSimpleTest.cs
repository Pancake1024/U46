using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VehiclePhysicsSimple))]
[AddComponentMenu("SBS/Racing/SimplePhysics/VehicleSimpleTest")]
public class VehicleSimpleTest : MonoBehaviour
{
    public Vector3 centerOfMass;

    void Start()
    {
        Vector3 originalCenterOfMass = GetComponent<Rigidbody>().centerOfMass;

        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        Vector3 r = GetComponent<Rigidbody>().centerOfMass - originalCenterOfMass;
        float rDotR = Vector3.Dot(r, r);
        Vector3 rOff = new Vector3(rDotR - r.x * r.x, rDotR - r.y * r.y, rDotR - r.z * r.z);
        rOff = rOff * GetComponent<Rigidbody>().mass;

        GetComponent<Rigidbody>().inertiaTensor += rOff;

        VehiclesManager.Instance.AddVehicle(gameObject);
        VehiclesManager.Instance.SetWheels(gameObject, VehiclesManager.WheelsPosition.Front, new VehiclesManager.WheelsDesc(0, 0, 2));
        VehiclesManager.Instance.SetWheels(gameObject, VehiclesManager.WheelsPosition.Rear, new VehiclesManager.WheelsDesc(0, 2, 2));

        gameObject.SendMessage("ResetPhysics");
        gameObject.SendMessage("ResetWheels");
    }

    void OnEnable()
    {
        Vector3 originalCenterOfMass = GetComponent<Rigidbody>().centerOfMass;

        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        Vector3 r = GetComponent<Rigidbody>().centerOfMass - originalCenterOfMass;
        float rDotR = Vector3.Dot(r, r);
        Vector3 rOff = new Vector3(rDotR - r.x * r.x, rDotR - r.y * r.y, rDotR - r.z * r.z);
        rOff = rOff * GetComponent<Rigidbody>().mass;

        GetComponent<Rigidbody>().inertiaTensor += rOff;
    }
}
