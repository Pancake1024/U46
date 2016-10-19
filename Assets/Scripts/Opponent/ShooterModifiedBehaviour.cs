using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

public class ShooterModifiedBehaviour : MonoBehaviour
{
    #region Public members
    public float fireRate = 0.5f;
    public GameObject shooterMuzzle;
    #endregion

    #region Protected members
    protected OnTheRunGameplay gameplayManager;
    protected PlayerKinematics playerKin;
    protected OpponentKinematics opponentK;
    protected PoliceBehaviour PoliceBehaviour;
    protected float nextShotTime = -1.0f;
    protected float changeLaneTimer = -1.0f;
    protected float shooterMuzzleTimer = -1.0f;
    protected float shooterMuzzleScale = -1.0f;
    #endregion
    
    #region Unity callbacks
    void Awake()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        opponentK = gameObject.GetComponent<OpponentKinematics>();
        PoliceBehaviour = gameObject.GetComponent<PoliceBehaviour>();
    }

    void Start()
    {
        ActivateMuzzleFx(false);
    }
    
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (playerKin==null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (PoliceBehaviour.State == PoliceBehaviour.EnemyState.KeepDistance)
        {
            nextShotTime -= dt;

            if (nextShotTime < 0.0f)
            {
                nextShotTime = 0.5f;
                RaycastHit hit;
                Vector3 startPos = gameObject.transform.position + Vector3.back * 4.0f + Vector3.up * 1.0f;
                Physics.Raycast(startPos, Vector3.back, out hit);
                if (hit.collider && hit.collider.gameObject != gameObject)
                {
                    OpponentKinematics oppKin = hit.collider.gameObject.GetComponent<OpponentKinematics>();
                    if (oppKin != null)
                    {
                        OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
                        ActivateMuzzleFx(true);
                        oppKin.SendMessage("OnOpponentDestroyed");
                    }
                }
            }
        }
        else
        {
            ActivateMuzzleFx(false);
        }


        //Muzzle
        if (shooterMuzzleTimer >= 0.0f)
        {
            shooterMuzzleTimer -= dt;
            if (shooterMuzzleTimer < 0.0f)
            {
                ActivateMuzzleFx(false);
            }
            else
            {
                shooterMuzzleScale += dt * 15.0f;
                shooterMuzzle.transform.localScale = new Vector3(shooterMuzzleScale, shooterMuzzleScale, shooterMuzzleScale);
            }
        }
    }
    #endregion

    #region Functions
    protected void ActivateMuzzleFx(bool active)
    {
        if (shooterMuzzle != null)
        {
            shooterMuzzleScale = 2.0f;
            shooterMuzzle.SetActive(active);
            shooterMuzzleTimer = active ? 0.1f : -1.0f;
        }
    }
    #endregion
}
