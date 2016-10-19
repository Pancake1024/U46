using UnityEngine;
using System.Collections;

public class FiretruckBehaviour : MonoBehaviour
{
    #region Public Members
    public GameObject waterCannon;
    public float cannonSpeed = 45.0f;
    public float maxRotation = 10.0f;
    public float waterStreamDistance = 10.0f;
    public ParticleSystem waterStream;
    #endregion

    #region Protected Members
    protected bool inputLeft = false;
    protected int waterStreamDirection = 0;
    protected bool inputRight = false;
    //protected GameObject waterStreamStart;
    protected LayerMask ignoreLayers = 0;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));
        waterCannon.transform.rotation = Quaternion.identity;
        //waterStreamStart = waterCannon.gameObject.transform.parent.transform.FindChild("waterStart").gameObject;
    }

    void OnEnable()
    {
        OnTheRunSounds.Instance.StartFiretruckSounds();
    }

    void OnDisable()
    {
        OnTheRunSounds.Instance.StopFiretruckSounds();
    }
    
    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        if (!gameObject.GetComponent<PlayerKinematics>().IsOnLane)
        {
            waterCannon.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            return;
        }

        RaycastHit hit;
        Vector3 startPos = gameObject.transform.position;//waterStreamStart.transform.position;
        startPos.z += 4.0f;
        startPos.y = 1.0f;
        Vector3 endPos = startPos + waterCannon.transform.forward * waterStreamDistance;
        //Physics.Raycast(startPos, waterCannon.transform.forward, out hit, waterStreamDistance, ~ignoreLayers);
        Physics.CapsuleCast(startPos, endPos, 1.5f, waterCannon.transform.forward, out hit, waterStreamDistance, ~ignoreLayers);
        if (hit.collider)
        {
            OpponentKinematics oppKin = hit.collider.gameObject.GetComponent<OpponentKinematics>();
            if (oppKin != null)
            {
                OnTheRunEffectsSounds.Instance.PlayFxSoundInPosition(gameObject.transform.position, OnTheRunObjectsPool.ObjectType.Explosion);
                oppKin.SendMessage("OnFiretruckHit");
            }
        }

        //Water cannon movement---------------------------------------
        waterStreamDirection = 0;
        if (inputLeft)
        {
            waterStreamDirection = -1;
        }
        else if (inputRight)
        {
            waterStreamDirection = 1;
        }

        float cannonAngle = waterCannon.transform.rotation.y * Mathf.Rad2Deg;
        bool tooRight = inputRight && cannonAngle > 0.0f && cannonAngle > maxRotation;
        bool tooLeft = inputLeft && cannonAngle < 0.0f && cannonAngle < -maxRotation;
        if (tooRight || tooLeft)
            waterStreamDirection = 0;
        waterCannon.transform.Rotate(0.0f, waterStreamDirection * cannonSpeed * dt, 0.0f);
    }
    #endregion

    #region Functions
    #endregion

    #region Messages
    void OnLeftInputDown()
    {
        inputLeft = true;
    }

    void OnLeftInputUp()
    {
        inputLeft = false;
    }

    void OnRightInputDown()
    {
        inputRight = true;
    }

    void OnRightInputUp()
    {
        inputRight = false;
    }
    #endregion
}
