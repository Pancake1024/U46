using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SBS/Racing/Cameras/CameraSky")]
//[ExecuteInEditMode]
public class CameraSky : MonoBehaviour
{
    public Transform sky;
    public Vector3 offset;
    public bool staticHeight = true;

    protected float height;

    void Awake()
    {
        if (staticHeight)
            height = sky.position.y;
    }

    void LateUpdate()
    {
        sky.transform.position = transform.position + offset;
        if (staticHeight)
            sky.transform.position = new Vector3(sky.transform.position.x, height, sky.transform.position.z);
    }
}
