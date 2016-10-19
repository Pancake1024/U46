using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;


public class OnTheRunSkyManager : MonoBehaviour 
{
    public GameObject controller = null;
    public Vector3 startOffset;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 ctrlPos = controller.transform.position;//controller.transform.Find("Root").position;
        //ctrlPos.y = ctrlPos.y - controller.groundDistance;
        this.transform.position = ctrlPos + startOffset;
       
    }

    void Update()
    {

       
    }
}
