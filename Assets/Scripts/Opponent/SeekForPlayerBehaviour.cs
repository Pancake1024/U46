using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

public class SeekForPlayerBehaviour : MonoBehaviour
{
    #region Public members
    public float minChangeLaneTimer;
    public float maxChangeLaneTimer;
    #endregion

    #region Protected members
    protected PlayerKinematics playerKin;
    protected OpponentKinematics opponentK;
    protected PoliceBehaviour PoliceBehaviour;
    protected float changeLaneTimer = -1.0f;
    protected float deactivateTimer = -1.0f;
    #endregion

    #region Public Porperties
    public float DeactivateTime
    {
        set { deactivateTimer = value; }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        opponentK = gameObject.GetComponent<OpponentKinematics>();
        PoliceBehaviour = gameObject.GetComponent<PoliceBehaviour>();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (playerKin==null)
            playerKin = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        if (deactivateTimer >= 0.0f)
        {
            deactivateTimer -= dt;
        }

        if (PoliceBehaviour.State == PoliceBehaviour.EnemyState.KeepDistance && deactivateTimer<0.0f)
        {
            bool samePlayerLane = (opponentK.CurrentLane == playerKin.CurrentLane);
            if (samePlayerLane)
            {
                changeLaneTimer = UnityEngine.Random.Range(minChangeLaneTimer, maxChangeLaneTimer);
            }
            else
            {
                changeLaneTimer -= dt;
                if (changeLaneTimer < 0.0f)
                {
                    opponentK.PreviousLane = opponentK.CurrentLane;
                    opponentK.CurrentLane = playerKin.CurrentLane;
                }
            }
        }

    }
    #endregion
}
