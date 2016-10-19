using UnityEngine;
using System.Collections;

public class RotationAround : MonoBehaviour
{
    public RotationType type = RotationType.Loop;
    public Vector3 Pivot;
    //it could be a Vector3 but its more user friendly 
    public bool RotateX = true;
    public bool RotateY = false;
    public bool RotateZ = false;
    public float speed = 45.0f;

    public float[] ABAngles = { };
    protected float[] originalABAngles;

    protected int initAngle = 0;
    protected int rotationDir = 1;

    [System.Serializable]
    public enum RotationType
    {
        Loop,
        ABClassic
    }

    void Awake()
    {
        originalABAngles = new float[ABAngles.Length];
        ABAngles.CopyTo(originalABAngles, 0);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime; //TimeManager.Instance.MasterSource.DeltaTime
        switch (type)
        {
            case RotationType.Loop:
                RotationStep(dt);
                break;

            case RotationType.ABClassic:
                if (ABAngles.Length > 0)
                {
                    RotationStep(dt);
                    float currentAngle = 0.0f;
                    if (RotateX)
                        currentAngle = transform.localRotation.eulerAngles.x;
                    else if (RotateY)
                        currentAngle = transform.localRotation.eulerAngles.y;
                    else if (RotateZ)
                        currentAngle = transform.localRotation.eulerAngles.z;

                    if (currentAngle < 180.0f && currentAngle > ABAngles[0])
                            rotationDir *= -1;
                    else if (currentAngle > 180.0f && currentAngle < ABAngles[1])
                        rotationDir *= -1;
                }
                break;
        }
    }

    void RotationStep(float deltaTime)
    {
        transform.position += (transform.rotation * Pivot);
        if (RotateX)
            transform.rotation *= Quaternion.AngleAxis(speed * rotationDir * deltaTime, Vector3.right);
        if (RotateY)
            transform.rotation *= Quaternion.AngleAxis(speed * rotationDir * deltaTime, Vector3.up);
        if (RotateZ)
            transform.rotation *= Quaternion.AngleAxis(speed * rotationDir * deltaTime, Vector3.forward);
        transform.position -= (transform.rotation * Pivot);
    }
}
