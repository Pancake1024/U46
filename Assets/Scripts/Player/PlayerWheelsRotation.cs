using UnityEngine;
using System.Collections;

public class PlayerWheelsRotation : MonoBehaviour
{
    #region Public Members
    public PlayerKinematics playerRef;
    public float wheelRadius;
    #endregion

    protected OnTheRunGameplay gameplayManager;

    //void Awake()
	void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
    }

	void FixedUpdate ()
    {
        if (playerRef == null)
            return;

        float dt = Time.fixedDeltaTime,
              currSpeed = playerRef.PlayerRigidbody.velocity.magnitude;

        transform.Rotate(Vector3.right, (currSpeed * dt / wheelRadius) * Mathf.Rad2Deg, Space.Self);
	}
}
