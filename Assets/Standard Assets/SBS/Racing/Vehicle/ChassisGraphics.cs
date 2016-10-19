using System;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/ChassisGraphics")]
public class ChassisGraphics : MonoBehaviour
{
    #region Public members
    public float heightOffset;
    public VehiclePhysics vehiclePhysics = null;
    public Transform shadow = null;
    public bool shadowIsProjector = false;
    #endregion

    #region Protected members
    protected bool initialized = false;
    #endregion

    #region Public methods
    public void Init()
    {
        if (initialized)
            return;

        if (vehiclePhysics != null)
            vehiclePhysics.SetChassisGraphics(transform, heightOffset);

        if (GetComponent<Renderer>().material != null && GetComponent<Renderer>().material.mainTexture != null)
            GetComponent<Renderer>().material.mainTexture.mipMapBias = -1.0f;

        initialized = true;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
    }

    void Start()
    {
        this.Init();
    }

    void Update()
    {
        if (!vehiclePhysics.Initialized)
            return;

        if (shadow != null)
        {
            if (shadowIsProjector)
                shadow.LookAt((SBSVector3)shadow.position - SBSVector3.up, transform.TransformDirection(SBSVector3.forward));
            else
            {
//              VehicleTrackData trackData = vehiclePhysics.gameObject.GetComponent<VehicleTrackData>();
//              shadow.position = new Vector3(transform.position.x, trackData.TokenPosition.y + 0.05f, transform.position.z);
                SBSVector3 pos = SBSVector3.zero, norm = SBSVector3.zero;
                for (int i = 0; i < 4; ++i)
                {
                    ContactData cd = vehiclePhysics.GetWheelContactProperties(i);
                    if (null == cd)
                        continue;
                    pos += cd.Position;
                    norm += cd.Normal;
                }
                pos *= 0.25f;
                norm *= 0.25f;
                pos += norm * 0.05f;
                pos.x = transform.position.x;
                pos.z = transform.position.z;
                /*
                Vector3 frw = transform.TransformDirection(Vector3.forward);
                frw.y = 0.0f;
                shadow.LookAt(shadow.position + frw, Vector3.up);*/
                shadow.position = pos;
                SBSVector3 worldForward = transform.TransformDirection(Vector3.forward);
                worldForward -= norm * SBSVector3.Dot(worldForward, norm);
                shadow.LookAt(pos + worldForward, norm);
            }
        }
    }
    #endregion
}