using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Racing/KeyboardInputs")]
public class KeyboardInputs : VehicleInputs
{
    override public float Throttle
    {
        get
        {
            return Input.GetAxis("Vertical");
        }
    }

    override public float Steering
    {
        get
        {
            float h = Input.GetAxis("Horizontal");
            if (h >= 0.0f)
                return SBSMath.Pow(h, 0.55f);
            else
                return -SBSMath.Pow(-h, 0.55f);
        }
    }

    public override bool Handbrake
    {
        get
        {
            return Input.GetKey(KeyCode.Space);
        }
    }

    void SetAIEnabled(bool flag)
    {
        this.enabled = !flag;
    }

    void DisableInputs()
    {
        this.enabled = false;  
    }
}
