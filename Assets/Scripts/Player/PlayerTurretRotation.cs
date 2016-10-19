using UnityEngine;
using System.Collections;

public class PlayerTurretRotation : MonoBehaviour
{
    #region Public Members
    public GameObject playerRef;
    #endregion
        
	void FixedUpdate ()
    {
        transform.localRotation = Quaternion.Euler(0.0f, -playerRef.transform.rotation.y * Mathf.Rad2Deg, 0.0f);
	}
}
