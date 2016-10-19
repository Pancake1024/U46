using UnityEngine;
using System.Collections;

public class PlaneBehaviour : MonoBehaviour
{
    public GameObject[] fireMuzzles;
    public float fireRate = -1.0f;
    public float shotDistance = 30.0f;
    public float rafficTime = -1.0f;
    public float rafficPauseTime = -1.0f;

    protected float tankMuzzleTimer = -1.0f;
    protected float tankMuzzleScale = -1.0f;
    protected float fireRateTimer = -1.0f;
    protected LayerMask ignoreLayers = 0;

    protected float startFlyingTime;
    protected float yMaxHeightFactor = 1.2f; //0.8f;
    protected float yMaxHeightSpeed = 2.0f; //4.0f

    protected bool onlyVerticalMovement = false;

    protected float rafficTimer = -1.0f;
    protected float pauseTimer = -1.0f;

    public bool OnlyVerticalMovement
    {
        set { onlyVerticalMovement = value; }
    }

    void Awake()
    {
        startFlyingTime = 0.0f;
    }

    void Start()
    {
        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));
        fireRateTimer = fireRate;
        ActivateMuzzlesFx(false);
        rafficTimer = rafficTime;
        pauseTimer = rafficPauseTime;
    }

    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (!onlyVerticalMovement)
        {
            if (rafficTimer >= 0.0f)
            {
                rafficTimer -= dt;

                if (fireRateTimer >= 0.0f)
                {
                    fireRateTimer -= dt;
                    if (fireRateTimer < 0.0f)
                    {
                        ShotRaycast();
                        fireRateTimer = fireRate;
                        ActivateMuzzlesFx(true);
                        OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.PlaneGun);
                    }
                }

                if (tankMuzzleTimer >= 0.0f)
                {
                    tankMuzzleTimer -= dt;
                    if (tankMuzzleTimer < 0.0f)
                    {
                        ActivateMuzzlesFx(false);
                    }
                    else
                    {
                        tankMuzzleScale += dt * 15.0f;
                        for (int i = 0; i < fireMuzzles.Length; ++i)
                            fireMuzzles[i].transform.localScale = new Vector3(tankMuzzleScale, tankMuzzleScale, tankMuzzleScale);
                    }
                }
            }
            else
            {
                pauseTimer -= dt;
                if (pauseTimer < 0.0f)
                {
                    rafficTimer = rafficTime;
                    pauseTimer = rafficPauseTime;
                }
            }
        }
    }

    void FixedUpdate( )
    {
        float dt = Time.fixedDeltaTime;

        startFlyingTime += dt;
        float yPosition = yMaxHeightFactor * Mathf.Sin(startFlyingTime * yMaxHeightSpeed);
        transform.FindChild("PlayerCar").position += new Vector3(0.0f, yPosition * dt, 0.0f);
    }

    #region Functions
    protected void ShotRaycast(  )
    {
        RaycastHit hit;
        Vector3 startPos = transform.position;
        startPos.y = 1.0f;
        Physics.Raycast(startPos, transform.forward, out hit, shotDistance, ~ignoreLayers);
        if (hit.collider)
        {
            OpponentKinematics oppKin = hit.collider.gameObject.GetComponent<OpponentKinematics>();
            if (oppKin != null)
            {
                OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyCarsWithPlane);
                LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.DestroyTraffic);
                oppKin.SendMessage("OnOpponentDestroyed");
            }
        }
        else
        {
            Vector3 explosionPosition = Vector3.forward * shotDistance;
            explosionPosition.y = -gameObject.transform.position.y;
            OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, explosionPosition, OnTheRunObjectsPool.ObjectType.SparksPlane, false);
        }
    }

    protected void ActivateMuzzlesFx(bool active)
    {
        for (int i = 0; i < fireMuzzles.Length; ++i)
        {
            fireMuzzles[i].SetActive(active);
        }
        tankMuzzleScale = 0.5f; // 2.0f;
        tankMuzzleTimer = active ? 0.1f : -1.0f;
    }
    #endregion
}
