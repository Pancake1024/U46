using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadWorkBehaviour : MonoBehaviour
{
    #region Public Members
    public DamageLevel damageLevel;
    #endregion

    #region Protected Members
    protected PlayerKinematics player = null;
    protected OnTheRunGameplay gameplayManager;
    #endregion

    #region Public Properties
    public enum DamageLevel
    {
        Low,
        Medium,
        High
    }
    #endregion

    #region Unity callbacks
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject collisionWith = other.gameObject;
        if (collisionWith.tag.Equals("Player"))
        {
            PlayerKinematics playerKin = collisionWith.GetComponent<PlayerKinematics>();
            float speedReductionCoeff = 1.0f;
            switch (damageLevel)
            {
                case DamageLevel.Low:
                    speedReductionCoeff = 0.95f;
                    break;
                case DamageLevel.Medium:
                    speedReductionCoeff = 0.9f;
                    break;
                case DamageLevel.High:
                    speedReductionCoeff = 0.85f;
                    break;
            }

            if (playerKin.PlayerRigidbody.velocity.z > 5.0f)
                playerKin.PlayerRigidbody.velocity = new Vector3(playerKin.PlayerRigidbody.velocity.x, playerKin.PlayerRigidbody.velocity.y, playerKin.PlayerRigidbody.velocity.z * speedReductionCoeff);

            if (gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Tank && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Firetruck)
                LevelRoot.Instance.BroadcastMessage("HitBarrier");

            OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.WoodHit);
            OnTheRunObjectsPool.Instance.RequestEffect(other.gameObject.transform, Vector3.forward * 1.3f + Vector3.up * 1.5f, OnTheRunObjectsPool.ObjectType.WoodHitFx, true);
            OnTheRunObjectsPool.Instance.NotifyDestroyingParent(gameObject, OnTheRunObjectsPool.ObjectType.RoadWork);
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
            OnTheRunObjectsPool.Instance.NotifyDestroyingParent(gameObject, OnTheRunObjectsPool.ObjectType.RoadWork);
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
