using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

public class ChangeCarBehaviour : MonoBehaviour
{
    #region Protected members
    protected GameObject truckGameObject;
    protected bool triggerActive = true;
    #endregion

    #region Public properties
    public bool TriggerActive
    {
        set { triggerActive = value; }
    }
    #endregion
    
    #region Unity callbacks
    void Awake()
    {
        truckGameObject = gameObject.transform.parent.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerActive && other.gameObject.tag.Equals("Player"))
        {
            OnTheRunGameplay.CarId carId = other.gameObject.GetComponent<PlayerKinematics>().carId;
            bool checkForFlyingPlayer = carId == OnTheRunGameplay.CarId.Ufo || carId == OnTheRunGameplay.CarId.Plane;

            if (!checkForFlyingPlayer && (other.gameObject.GetComponent<PlayerKinematics>().IsLifting || other.gameObject.GetComponent<PlayerKinematics>().IsJumping))
            {
                triggerActive = false;
                other.gameObject.GetComponent<PlayerKinematics>().OnTruckEnter(truckGameObject);
                LevelRoot.Instance.Root.BroadcastMessage("OnChangeCarEvent", truckGameObject.GetComponent<TruckBehaviour>().FakeCarPosition);
                truckGameObject.SendMessage("RemoveFakeCar");
            }
        }
    }
    #endregion

    #region Messages
    #endregion
}
