using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;
using SBS.Core;

[AddComponentMenu("OnTheRun/OnTheRunObject")]
public class OnTheRunObject : MonoBehaviour
{
    OnTheRunObjectManager objectManager;
    
    public GameObject animatedInstance = null;
    public bool animationStarted = false;

    public enum ObjectType
    {
        Trash = 0,
        ToDo,      
        Void
    }

    public ObjectType type;

    void Start()
    {
        objectManager = GameObject.Find("Track").GetComponent<OnTheRunObjectManager>();
        type = ObjectType.Trash; //TODO!
    }

    void FixedUpdate()
    { }

    void ObstacleHit()
    {
        //Debug.Log("OBSTACLE HI");
        if (GetComponent<Renderer>() != null && GetComponent<Renderer>().enabled)
            GetComponent<Renderer>().enabled = false;

        objectManager.PlaceAnimatedObstacle(this);
    }

    void OnPopFromTrack()
    {
        //Debug.Log("POP");     
        animationStarted = false;
        if (animatedInstance != null)
        {
            objectManager.FreeAnimatedObstacle(type, animatedInstance);
            animatedInstance = null;
        }
    }
    
    void Update()
    { }
}
