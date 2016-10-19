using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UfoBehaviour : MonoBehaviour
{
    protected float startFlyingTime;
    protected float zMaxAngle = 6.0f;
    protected float xMaxAngle = 8.0f;
    protected float zRotationSpeed = 3.0f;
    protected float xRotationSpeed = 2.0f;
    protected float yMaxHeightFactor = 0.8f;
    protected float yMaxHeightSpeed = 4.0f;

    #region Unity Callbacks
    void Awake()
    {
        startFlyingTime = 0.0f;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        startFlyingTime += dt;
        float zRotation = zMaxAngle * Mathf.Sin(startFlyingTime * zRotationSpeed);
        float xRotation = xMaxAngle * Mathf.Deg2Rad * Mathf.Sin(startFlyingTime * xRotationSpeed);
        transform.rotation = Quaternion.Euler(xRotation, 0.0f, zRotation);

        float yPosition = yMaxHeightFactor * Mathf.Sin(startFlyingTime * yMaxHeightSpeed);
        transform.position += new Vector3(0.0f, yPosition * dt, 0.0f);
    }
    #endregion

    #region Messages
    #endregion
}
