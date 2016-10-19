using System;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTriggerBehaviour : MonoBehaviour
{
    #region Unity callbacks
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Debug.Log("*** ASSET COLLISION ***");
            GameObject.FindGameObjectWithTag("Player").SendMessage("PlayDebris");
        }
    }
    #endregion
}
