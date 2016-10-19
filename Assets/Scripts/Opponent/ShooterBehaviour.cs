using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

public class ShooterBehaviour : MonoBehaviour
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

    #region Public Porperties
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
            bool samePlayerLane = (opponentK.CurrentLane == playerKin.CurrentLane);
            if (samePlayerLane)
            {
                nextShotTime -= dt;
                changeLaneTimer = UnityEngine.Random.Range(0.6f, 1.2f);
                if (nextShotTime < 0.0f)
                {
                    //SHOT!!!!!!
                    OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
                    nextShotTime = fireRate;
                    ActivateMuzzleFx(true);
                    playerKin.SendMessage("OnShooterDamage");
                }
            }
            else
            {
                nextShotTime = fireRate;
                changeLaneTimer -= dt;
                if (changeLaneTimer < 0.0f)
                    opponentK.CurrentLane = playerKin.CurrentLane;
            }
        }
        else
        {
            ActivateMuzzleFx(false);
            nextShotTime = fireRate;
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
                shooterMuzzleScale += dt * 45.0f;
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
            shooterMuzzleScale = 1.5f;
            shooterMuzzle.SetActive(active);
            shooterMuzzleTimer = active ? 0.1f : -1.0f;
        }
    }
    #endregion
}
