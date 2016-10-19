using UnityEngine;
using System.Collections;

public class TankBehaviour : MonoBehaviour
{
    #region Public Members
    public GameObject TankMuzzle;
    //public GameObject ShotRangeAreaPrefab;
    public ParticleSystem TankMuzzleSmoke;
    public float minShotDistance;
    public float maxShotDistance;
    #endregion

    #region Protected Members
    protected OnTheRunSpawnManager spawnManager;
    protected GameObject turret;
    //protected GameObject shotRangeArea;
    protected float turretSpeed = 25.0f;
    protected float turretReturnSpeed = 1.0f;//0.25f;
    protected GameObject cameraManager;
    protected float tankMuzzleTimer = -1.0f;
    protected float tankMuzzleScale = -1.0f;
    protected bool inputsEnable = true;
    protected bool inputLeft = false;
    protected bool inputRight = false;
    protected bool readyToFire = false;
    protected bool readyToMove = true;
    protected float goalRotation = 10.0f;
    protected float startTurretTime = 0.0f;
    protected int fireOnceFlag = 0;
    protected int oldFireOnceFlag = 0;
    protected float goalYRotation = 0.0f;
    #endregion

    #region Public properties
    #endregion

    #region Unity Callbacks
    void Start()
    {
        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        spawnManager = gameplayManager.GetComponent<OnTheRunSpawnManager>();

        //nextShot = fireRate;
        turret = gameObject.transform.FindChild("PlayerCar/tank_turret").gameObject;
        //shotRangeArea = Instantiate(ShotRangeAreaPrefab) as GameObject;
        cameraManager = GameObject.FindGameObjectWithTag("MainCamera");
        ActivateMuzzleFx(false);
        inputsEnable = true;
        readyToMove = true;
        turret.transform.rotation = Quaternion.identity;
        goalYRotation = 0.0f;
    }

    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (!gameObject.GetComponent<PlayerKinematics>().IsOnLane)
        {
            turret.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            goalYRotation = 0.0f;
            return;
        }

        //Turret movement---------------------------------------
        if (inputLeft)
        {
            inputLeft = false;
            if (goalYRotation != 360.0f - goalRotation)
            {
                fireOnceFlag = 1;
                startTurretTime = 0.0f;
            }
            goalYRotation = 360.0f - goalRotation;
        }
        else if (inputRight)
        {
            inputRight = false;
            if (goalYRotation != goalRotation)
            {
                fireOnceFlag = -1;
                startTurretTime = 0.0f;
            }
            goalYRotation = goalRotation;
        }

        startTurretTime += dt;
        if (goalYRotation == 0.0f)
        {
            turret.transform.rotation = Quaternion.Lerp(turret.transform.rotation, Quaternion.Euler(0.0f, goalYRotation, 0.0f), startTurretTime * turretReturnSpeed);
            bool almostReturned = false;
            almostReturned = Mathf.Abs(turret.transform.rotation.eulerAngles.y - goalYRotation) < 2.0f || Mathf.Abs(turret.transform.rotation.eulerAngles.y - goalYRotation) > 358.0f;
            if (!almostReturned)
                readyToMove = false;
            else
                readyToMove = true;
        }
        else
            turret.transform.rotation = Quaternion.Lerp(turret.transform.rotation, Quaternion.Euler(0.0f, goalYRotation, 0.0f), startTurretTime * turretSpeed);

        if (goalYRotation != 0.0f && Mathf.Abs(turret.transform.rotation.eulerAngles.y - goalYRotation) < 0.2f)
        {
            if (!readyToFire)
            {
                readyToFire = true;
            }
        }
        else
            readyToFire = false;

        bool shotToTheRight = goalYRotation<180.0f;
        if (readyToFire && fireOnceFlag!=oldFireOnceFlag) // && readyToFireTimer<0.0f)
        {
            goalYRotation = 0.0f;
            oldFireOnceFlag = fireOnceFlag;
            oldFireOnceFlag = 0;
            LevelRoot.Instance.BroadcastMessage("OnTankShot", shotToTheRight);
            OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.TankCannon);

            FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(0.3f, 0.4f, 0.0f);
            cameraManager.SendMessage("StartShakeCamera", sData);

            ActivateMuzzleFx(true);

            if((Random.value * 100000.0f) % 100.0f > 75.0f)
                OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.Boom);

            Vector3 lateralOffset = shotToTheRight ? Vector3.right : Vector3.left;
            lateralOffset *= Random.Range(2.0f, 8.0f);
            Vector3 explosionPosition = Vector3.forward * maxShotDistance + lateralOffset;
            bool someoneHitted = spawnManager.ShotNearestTrafficVehicle(shotToTheRight, minShotDistance, maxShotDistance);
            if (!someoneHitted)
            {
                OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, explosionPosition, OnTheRunObjectsPool.ObjectType.TankExplosion, false);
                OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position + explosionPosition, OnTheRunObjectsPool.ObjectType.TankExplosion);
                OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, explosionPosition + Vector3.up * 0.1f, OnTheRunObjectsPool.ObjectType.AsphaltCrack, false);
            }
            else
            {
                OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, explosionPosition, OnTheRunObjectsPool.ObjectType.TankExplosion, false); 
            }
        }

        //Shot area movement---------------------------------------
        /*if (shotRangeArea != null)
        {
            shotRangeArea.transform.position = new Vector3(0.0f, 0.1f, gameObject.transform.position.z + 20.0f);
        }*/

        //OLD BEHAVIOUR
        /*
        nextShot -= dt;

        if (nextShot < 0.0f)
        {
            nextShot = fireRate; 

            RaycastHit hit;
            Vector3 startPos = gameObject.transform.position + Vector3.up * 1.0f;
            Vector3 endPos = startPos + Vector3.forward * 200.0f;
            Physics.Raycast(startPos, endPos, out hit, 50.0f); //to improve: layer for traffic ??
            if (hit.collider)
            {
                OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.TankCannon);
                ActivateMuzzleFx(true);
            
                OpponentKinematics kin = hit.collider.gameObject.GetComponent<OpponentKinematics>();
                if (kin != null)
                    kin.SendMessage("OnPlayerEndJump", 0.0f);
            }
        }*/

        if (tankMuzzleTimer >= 0.0f)
        {
            tankMuzzleTimer -= dt;
            if (tankMuzzleTimer < 0.0f)
            {
                ActivateMuzzleFx(false);
            }
            else
            {
                tankMuzzleScale += dt * 15.0f;
                TankMuzzle.transform.localScale = new Vector3(tankMuzzleScale, tankMuzzleScale, tankMuzzleScale);
            }
        }
    }
    #endregion

    #region Functions
    protected void ActivateMuzzleFx(bool active)
    {
        if (TankMuzzle != null)
        {
            tankMuzzleScale = 2.0f;
            TankMuzzle.SetActive(active);
            tankMuzzleTimer = active ? 0.1f : -1.0f;
            if (active)
                TankMuzzleSmoke.Play();
            else
                TankMuzzleSmoke.Stop();
        }
    }    
    #endregion

    #region Messages
    void OnDestroyTankArea( )
    {
        /*if (shotRangeArea!=null)
            Destroy(shotRangeArea);*/
    }

    void OnLeftInputDown()
    {
        if (inputsEnable && readyToMove)
        {
            inputLeft = true;
            inputsEnable = false;
        }
    }

    void OnLeftInputUp()
    {
        inputsEnable = true;
    }

    void OnRightInputDown()
    {
        if (inputsEnable && readyToMove)
        {
            inputRight = true;
            inputsEnable = false;
        }
    }

    void OnRightInputUp()
    {
        inputsEnable = true;
    }
    #endregion
}
