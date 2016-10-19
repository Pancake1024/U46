using System;
using System.Collections.Generic;
using UnityEngine;

public class GuardRailBehaviour : MonoBehaviour
{
    public OnTheRunObjectsPool.ObjectType guardrailType;
    protected PlayerKinematics player = null;

    #region Unity callbacks
    void OnTriggerEnter(Collider other)
    {
        GameObject collisionWith = other.gameObject;
        if (collisionWith.tag.Equals("Player"))
        {
            collisionWith.GetComponent<PlayerKinematics>().SendMessage("OnEnterCentralMud", SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject collisionWith = other.gameObject;
        if (collisionWith.tag.Equals("Player"))
        {
            collisionWith.GetComponent<PlayerKinematics>().SendMessage("OnExitCentralMud", SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        //Delete objects too far
        float playerZ = player.PlayerRigidbody.position.z;
        if (playerZ - gameObject.transform.position.z > 30)
        {
            OnTheRunObjectsPool.Instance.NotifyDestroyingParent(gameObject, guardrailType);
        }
    }
    #endregion

}
