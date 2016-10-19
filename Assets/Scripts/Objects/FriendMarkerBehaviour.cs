using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("OnTheRun/FriendMarkerBehaviour")]
public class FriendMarkerBehaviour : MonoBehaviour
{
    #region Protected Members
    protected PlayerKinematics player = null;
    #endregion

    #region Unity callbacks
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
    }
    
    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        //Delete objects too far
        float playerZ = player.PlayerRigidbody.position.z;
        if (playerZ - gameObject.transform.position.z > 30)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Messages
    void OnChangePlayerCar()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
    }
    #endregion
}
