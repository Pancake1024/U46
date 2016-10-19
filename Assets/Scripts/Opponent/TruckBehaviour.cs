using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

public class TruckBehaviour : MonoBehaviour
{
    static public int tutorialTruckCount = 0;
    static public TrasformType randomTruckFromInAppPage = TrasformType.None;

    #region Public Members
    public TruckType truckType;
    public GameObject bigFootRef;
    public GameObject tankRef;
    public GameObject firetruckRef;
    public GameObject ufoRef;
    public GameObject planeRef;

    public Material fakeUfoShadowMat;
    public Material fakePlaneShadowMat;

    public SpecialVehicleMaterials[] lockedMaterialData;
    #endregion

    #region Protected Members
    protected OnTheRunSpawnManager spawnManager;
    protected OpponentKinematics truckKin;
    protected GameObject playerRef;
    protected GameObject fakeCarInstance;
    protected TrasformType fakeCarType = TrasformType.Bigfoot;
    protected float distanceCheck = 7.0f;
    protected bool jumpStarted = false;
    protected Material BackupShadowMat;
    protected LayerMask ignoreLayers = 0;
    #endregion

    #region Public properties
    [System.Serializable]
    public struct SpecialVehicleMaterials
    {
        public TrasformType type;
        public Material[] defaultMaterial;
        public Material[] lockedMaterial;
    }

    public enum TruckType
    {
        None = -1,
        Normal = 0,
        Turbo,
        Transform
    }

    public enum TrasformType
    {
        None = -1,
        Bigfoot = 0,
        Tank,
        Firetruck,
        Ufo,
        Plane,
        Count
    }

    public TruckType TypeOfTruck
    {
        get { return truckType; }
    }

    public TrasformType CarOverTruckType
    {
        get { return fakeCarType; }
        set { fakeCarType = value; }
    }

    public Vector3 FakeCarPosition
    {
        get 
        {
            if (fakeCarInstance != null)
                return fakeCarInstance.transform.position;
            else
                return Vector3.zero;
        }
    }

    public Transform FakeCarTransform
    {
        get
        {
            return fakeCarInstance.transform;
        }
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        truckKin = gameObject.GetComponent<OpponentKinematics>();
        //InitializeTruck();
    }

    void Update()
    {
        if (truckKin.IsExploded) return;

        if (playerRef == null) playerRef = GameObject.FindGameObjectWithTag("Player");

        PlayerKinematics kin = playerRef.GetComponent<PlayerKinematics>();

        bool checkForFlyingPlayer = kin.carId == OnTheRunGameplay.CarId.Ufo || kin.carId == OnTheRunGameplay.CarId.Plane;

        if (kin.PlayerIsDead || kin.transform.position.y > 10.0f || checkForFlyingPlayer)
            return;

        bool sameLane = kin.PrevLane == kin.CurrentLane && kin.CurrentLane == gameObject.GetComponent<OpponentKinematics>().CurrentLane;
        if (sameLane)
        {
            float distanceZ = gameObject.transform.position.z - playerRef.transform.position.z;
            bool canCheckForPlayer = distanceZ > 0 && distanceZ < distanceCheck && !jumpStarted;
            if (canCheckForPlayer)
            {
                //Player is near: check the lane & turbo mode active
                bool notInTurbo = true; // !kin.TurboOn;
                bool isOnLane = kin.IsOnLane;
                if (notInTurbo && isOnLane)
                {
                    LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.Jump);

                    switch (truckType)
                    {
                        //case TruckType.Normal:
                        //    LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.OverYellowTruck);
                        //    break;
                        case TruckType.Turbo:
                            OnTheRunSounds.Instance.PlaySpeech(OnTheRunSounds.Speech.TruckTurbo);
                            LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.OverRedTruck);
                            break;
                        //case TruckType.Transform:
                        //    LevelRoot.Instance.BroadcastMessage("OnGenericMissionEvent", OnTheRunSingleRunMissions.MissionType.OverBlueTruck);
                        //    break;
                    }
                    jumpStarted = true;
                    gameObject.GetComponent<Collider>().enabled = false;
                    kin.OnTruckEnter(gameObject);
                }
            }
            else if (distanceZ < 0.0f)
            {
                jumpStarted = false;
                gameObject.GetComponent<Collider>().enabled = true;
            }
        }
    }
    #endregion

    #region Messages
    void OnReset()
    {
        InitializeTruck();
    }

    void InitializeTruck()
    {
        jumpStarted = false;

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            GetComponent<Rigidbody>().mass = 100000;
        else
            GetComponent<Rigidbody>().mass = 100;

        if (truckType == TruckType.Transform)
        {
            gameObject.transform.FindChild("changeCarTrigger").GetComponent<ChangeCarBehaviour>().TriggerActive = true;

            if (true) //fakeCarInstance == null)
            {
                //Choose type of vehicle over the truck
                //int rndVal = (int)((Random.value * 10000.0f) % (int)TrasformType.Count);
                //fakeCarType = (TrasformType)rndVal;
                if (spawnManager == null)
                    spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();

                fakeCarType = spawnManager.GetNextSpecialVehicles();

                if (spawnManager.firstSpecialVehicleInSession)
                {
                    spawnManager.firstSpecialVehicleInSession = false;
                    if (UnlockingManager.Instance.ForceSpawnSpecialVehicleCounter())
                    {
                        switch(UnlockingManager.Instance.LastSpecialVehicleUnlocked)
                        {
                            case OnTheRunGameplay.CarId.Bigfoot: fakeCarType = TrasformType.Bigfoot; break;
                            case OnTheRunGameplay.CarId.Tank: fakeCarType = TrasformType.Tank; break;
                            case OnTheRunGameplay.CarId.Firetruck: fakeCarType = TrasformType.Firetruck; break;
                            case OnTheRunGameplay.CarId.Ufo: fakeCarType = TrasformType.Ufo; break;
                            case OnTheRunGameplay.CarId.Plane: fakeCarType = TrasformType.Plane; break;
                        }
                    }
                }

                if (OnTheRunTutorialManager.Instance.TutorialActive)
                {
                    switch (tutorialTruckCount)
                    {
                        case 0: fakeCarType = TrasformType.Bigfoot; break;
                        case 1: fakeCarType = TrasformType.Bigfoot; break; //TrasformType.Tank
                    }
                    ++tutorialTruckCount;
                    if (tutorialTruckCount > 1)
                        tutorialTruckCount = 0;
                }

                //Place the player over the truck
                GameObject fakeCarRef = null;
                switch (fakeCarType)
                {
                    case TrasformType.Bigfoot: fakeCarRef = bigFootRef; break;
                    case TrasformType.Tank: fakeCarRef = tankRef; break;
                    case TrasformType.Firetruck: fakeCarRef = firetruckRef; break;
                    case TrasformType.Ufo: fakeCarRef = ufoRef; break;
                    case TrasformType.Plane: fakeCarRef = planeRef; break;
                }
                if (fakeCarInstance!=null)
                    Destroy(fakeCarInstance);
                fakeCarInstance = GameObject.Instantiate(fakeCarRef) as GameObject;
                fakeCarInstance.transform.parent = gameObject.transform;
                fakeCarInstance.transform.localPosition = gameObject.transform.FindChild("carPlaceholder").transform.localPosition;
                fakeCarInstance.transform.localRotation = gameObject.transform.FindChild("carPlaceholder").transform.localRotation;

                if (fakeCarType == TrasformType.Ufo)
                {
                    fakeCarInstance.transform.localPosition += Vector3.up * 0.5f + Vector3.forward * -0.4f;
                }
                else if (fakeCarType == TrasformType.Firetruck)
                {
                    fakeCarInstance.transform.localPosition += Vector3.up * 0.2f + Vector3.forward * -0.6f;
                }
                else if (fakeCarType == TrasformType.Plane)
                {
                    fakeCarInstance.transform.localPosition += Vector3.up * 0.4f + Vector3.forward * -0.7f;
                }
            }
        }

        if(!OnTheRunTutorialManager.Instance.TutorialActive)
            StartCoroutine(CheckForTooNearCars());
    }

    IEnumerator CheckForTooNearCars()
    {
        yield return new WaitForSeconds(0.5f);

        //Force opponent to change lane if too near
        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));
        RaycastHit hit;
        Vector3 startPos = gameObject.transform.position + Vector3.up * 1.0f;
        Vector3 endPos = Vector3.back;// startPos + Vector3.back * 15.0f;
        Physics.Raycast(startPos, endPos, out hit, 90.0f, ~ignoreLayers);
        if (hit.collider)
        {
            OpponentKinematics oppKin = hit.collider.gameObject.GetComponent<OpponentKinematics>();
            if (oppKin!=null && !oppKin.IsTruck)
                oppKin.SendMessage("ChangeLaneForRoadWorks");
        }
    }

    void RemoveFakeCar()
    {
        if (jumpStarted)
        {
            Destroy(fakeCarInstance);
        }
    }

    void SetTruckActive(bool active)
    {
        gameObject.GetComponent<Renderer>().enabled = active;
        gameObject.transform.FindChild("lights_stop").gameObject.SetActive(active);
        gameObject.transform.FindChild("veh_shadow").gameObject.SetActive(active);
        gameObject.transform.FindChild("truck_arrows").gameObject.SetActive(active);
    }

    void InitializeTruckForGarage(int index)
    {
        jumpStarted = false;
        gameObject.transform.FindChild("changeCarTrigger").GetComponent<ChangeCarBehaviour>().TriggerActive = true;

       /* if (spawnManager == null)
            spawnManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunSpawnManager>();
        fakeCarType = spawnManager.GetNextSpecialVehicles();*/

        fakeCarType = (TrasformType)index;

        UnlockingManager.SpecialCarData currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(fakeCarType);

        GameObject fakeCarRef = null;
        switch (fakeCarType)
        {
            case TrasformType.Bigfoot: fakeCarRef = bigFootRef; break;
            case TrasformType.Tank: fakeCarRef = tankRef; break;
            case TrasformType.Firetruck: fakeCarRef = firetruckRef; break;
            case TrasformType.Ufo: fakeCarRef = ufoRef; break;
            case TrasformType.Plane: fakeCarRef = planeRef; break;
        }
        if (fakeCarInstance != null)
            Destroy(fakeCarInstance);
        fakeCarInstance = GameObject.Instantiate(fakeCarRef) as GameObject;

        GameObject shadow = gameObject.transform.FindChild("veh_shadow").gameObject;
        if (currSpecialCarData.locked)
        {
            fakeCarInstance.transform.parent = gameObject.transform;
            fakeCarInstance.transform.localScale = Vector3.one * 1.5f;
            SetTruckActive(false);
            shadow.SetActive(true);

            RotationAround rotationComponent = null;

            switch (fakeCarType)
            {
                case TrasformType.Tank:
                    fakeCarInstance.transform.localPosition = Vector3.up * 0.0f;
                    fakeCarInstance.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

                    GameObject turret = fakeCarInstance.transform.FindChild("tank_turret").gameObject;
                    rotationComponent = turret.AddComponent<RotationAround>();
                    rotationComponent.type = RotationAround.RotationType.ABClassic;
                    rotationComponent.RotateX = false;
                    rotationComponent.RotateY = true;
                    rotationComponent.RotateX = false;
                    rotationComponent.ABAngles = new float[2]{25.0f, 335.0f};
                    rotationComponent.speed = 25.0f;

                    break;
                case TrasformType.Ufo:
                    fakeCarInstance.transform.localPosition = Vector3.up * 2.5f;
                    fakeCarInstance.transform.localRotation = Quaternion.identity;
                    fakeCarInstance.transform.localScale = Vector3.one * 1.3f;
                    BackupShadowMat = shadow.GetComponent<Renderer>().material;
                    shadow.GetComponent<Renderer>().material = fakeUfoShadowMat;
                    shadow.transform.localScale = new Vector3(2.5f, 1.0f, 1.5f);
                    shadow.transform.localPosition = Vector3.zero;
                    fakeCarInstance.AddComponent<TextureShifter>();
                    fakeCarInstance.GetComponent<TextureShifter>().smooth = true;
                    fakeCarInstance.GetComponent<TextureShifter>().dt = 1.0f;
                    fakeCarInstance.GetComponent<TextureShifter>().shift = new Vector2(0.35f, 0.0f);
                    fakeCarInstance.GetComponent<TextureShifter>().materialIndex = 1;
                    break;
                case TrasformType.Plane:
                    fakeCarInstance.transform.localPosition = Vector3.up * 2.0f;
                    fakeCarInstance.transform.localRotation = Quaternion.Euler(0.0f ,180.0f, 0.0f);
                    BackupShadowMat = shadow.GetComponent<Renderer>().material;
                    shadow.GetComponent<Renderer>().material = fakePlaneShadowMat;
                    shadow.transform.localScale = new Vector3(3.0f, 1.0f, -2.0f);

                    GameObject propeller = fakeCarInstance.transform.FindChild("baron_propellers").gameObject;
                    propeller.AddComponent<RotationAround>();
                    propeller.GetComponent<RotationAround>().speed = 800.0f;
                    propeller.GetComponent<RotationAround>().RotateX = false;
                    propeller.GetComponent<RotationAround>().RotateY = false;
                    propeller.GetComponent<RotationAround>().RotateZ = true;
                    break;
                case TrasformType.Firetruck:
                    fakeCarInstance.transform.localPosition = Vector3.up * 0.0f;
                    fakeCarInstance.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                    shadow.transform.localPosition = new Vector3(0.0f, 0.0f, -0.32f);
                    shadow.transform.localScale = new Vector3(1.5f, 1.0f, 2.3f);

                    GameObject cannon = fakeCarInstance.transform.FindChild("truck_cannon").gameObject;
                    rotationComponent = cannon.AddComponent<RotationAround>();
                    rotationComponent.type = RotationAround.RotationType.ABClassic;
                    rotationComponent.RotateX = false;
                    rotationComponent.RotateY = true;
                    rotationComponent.RotateX = false;
                    rotationComponent.ABAngles = new float[2]{15.0f, 345.0f};
                    rotationComponent.speed = 35.0f;
                    break;
            }

            fakeCarInstance.gameObject.AddComponent<LockedBehaviour>();
            if ((int)fakeCarType < lockedMaterialData.Length)
            {
                SpecialVehicleMaterials currentMaterials = lockedMaterialData[(int)fakeCarType];
                fakeCarInstance.gameObject.GetComponent<LockedBehaviour>().OnObjectLocked(currentMaterials.lockedMaterial, fakeCarInstance.GetComponentsInChildren<MeshRenderer>());
            }
            //fakeCarInstance.gameObject.renderer.material.mainTexture = Color.red;
        }
        else
        {
            //Place the player over the truck
            SetTruckActive(true);
            if (BackupShadowMat != null)
            {
                shadow.GetComponent<Renderer>().material = BackupShadowMat;
                shadow.transform.localScale = Vector3.one;
            }
            fakeCarInstance.transform.parent = gameObject.transform;
            fakeCarInstance.transform.localScale = Vector3.one;
            fakeCarInstance.transform.localPosition = gameObject.transform.FindChild("carPlaceholder").transform.localPosition;
            fakeCarInstance.transform.localRotation = gameObject.transform.FindChild("carPlaceholder").transform.localRotation;

            switch (fakeCarType)
            {
                case TrasformType.Bigfoot:
                    fakeCarInstance.transform.localPosition = new Vector3(0.0f, 1.47f, -2.2f);
                    break;
                case TrasformType.Tank:
                    fakeCarInstance.transform.localPosition = new Vector3(0.0f, 1.64f, -2.2f);
                    break;
                case TrasformType.Ufo:
                    fakeCarInstance.transform.localPosition = new Vector3(0.0f, 2.8f, -2.36f);
                    break;
                case TrasformType.Firetruck:
                    fakeCarInstance.transform.localPosition = new Vector3(0.0f, 1.72f, -2.17f);
                    break;
                case TrasformType.Plane:
                    fakeCarInstance.transform.localPosition = new Vector3(0.0f, 1.8f, -2.9f);
                    break;
            }

            fakeCarInstance.gameObject.AddComponent<LockedBehaviour>();
            if ((int)fakeCarType < lockedMaterialData.Length)
            {
                SpecialVehicleMaterials currentMaterials = lockedMaterialData[(int)fakeCarType];
                fakeCarInstance.gameObject.GetComponent<LockedBehaviour>().OnObjectLocked(currentMaterials.defaultMaterial, fakeCarInstance.GetComponentsInChildren<MeshRenderer>());
            }
        }
    }
    #endregion
}
