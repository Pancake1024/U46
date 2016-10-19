using UnityEngine;
using System.Collections;
using SBS.Core;

public class PlaneShadowScaling : MonoBehaviour
{
    public float shadowScaleWhenPlaneOnGround = 1.5f;
    public float shadowScaleWhenPlaneAtReferenceHeight = 1.0f;
    public float referenceHeight = 4.0f;

    Transform planeTransform;
    float a;
    float b;

    void Awake()
    {
        planeTransform = transform.parent.Find("PlayerCar");
        Asserts.Assert(planeTransform != null, "Could not find GameObject \"PlayerCar\" in parent.");

        //   ( y = a * x + b )
        // scale = a * distance + b
        a = (shadowScaleWhenPlaneAtReferenceHeight - shadowScaleWhenPlaneOnGround) / referenceHeight;
        b = shadowScaleWhenPlaneOnGround;
    }

    void Update()
    {
        float distanceFromShadowPlane = planeTransform.position.y - transform.position.y;
        float scale = a * distanceFromShadowPlane + b;

        transform.localScale = Vector3.one * scale;
    }
}