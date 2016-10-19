using UnityEngine;
using System.Collections;

public class BigfootRotation : MonoBehaviour
{
    public GameObject playerRef;
    public int direction = 1;
    protected float initTime = -1.0f;
    protected float endTime = -1.0f;

    void FixedUpdate()
    {
        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, direction * playerRef.transform.rotation.y * Mathf.Rad2Deg * 1.1f);
    }
}
