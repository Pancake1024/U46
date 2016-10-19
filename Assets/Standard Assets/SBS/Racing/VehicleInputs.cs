using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SBS/Racing/VehicleInputs")]
public class VehicleInputs : MonoBehaviour
{
    virtual public float Throttle { get; set; }
    virtual public float Steering { get; set; }
    virtual public bool Handbrake { get; set; }

    virtual public void UpdateInputs() { }
}
