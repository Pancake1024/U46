using System;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using SBS.Core;
using SBS.Math;
using SBS.XML;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/VehiclePhysics")]
public class VehiclePhysics : MonoBehaviour, IVehiclePhysics 
{
	protected Rigidbody rigidBody;

	// vehicle components
	protected bool initialized = false;
	protected Engine engine;
	protected Friction friction;
	protected Gearbox gearbox;
	protected Differential[] differentials;
	protected Suspension[] suspensions;
	protected Wheel[] wheels;
	protected Brake[] brakes;
	protected Tyre[] tyres;
	protected Wing[] wings;

	protected SBSMatrix4x4 inertiaTensor;

	protected SBSVector3[] prevWheelPositions;
	protected SBSVector3[] wheelVelocities;

	protected SBSVector3[] tyreForces;
//	protected SBSVector3[] tmpVars;
	protected SBSVector3[] lastSuspensionForce;
#if UNITY_EDITOR
	protected SBSVector3[] lastSuspensionApply = null;
#endif
    protected bool[] asActive;
    public bool ASR_enabled = true;
    public float ASR_engageCoeff = 10.0f;
    public float ASR_disengageCoeff = 0.25f;

	protected bool autoClutch;
	protected float prevAutoClutch;
	protected float remainingShiftTime;
	protected int shiftGear;
	protected float shiftClutchStart;
	protected bool shifted;
	protected float prevSteer;
	protected bool autoShiftEnabled = true;
    protected bool autoShiftInNeuter = true;
    public int inputGearChange = 0;

	protected VehicleUtils.Traction traction;
	protected float minSteeringAngle;
	protected float maxSteeringAngle;
	protected float steeringSpeedRatio;
	protected float steeringMinSpeed;

	protected ContactData[] wheelContacts = null;
	protected RaycastHit[] wheelRaycastHits = null;
	protected float lastCollisionTime = -1000.0f;
    public bool gearDownOnCollision = true;

    protected VehicleInputs[] inputs = null;
    protected float throttleFromInput = 0.0f;
    protected float steeringFromInput = 0.0f;
    protected bool handbrakeFromInput = false;

	protected bool freezed = false;
    protected bool isAI = false;

//	protected XmlDocument vehicleXml;

	// graphics
	protected Transform chassisGraphics;
	protected float chassisHeightOffset;
	protected Transform[] wheelGraphics = new Transform[4];

    public FiniteStateMachine cameraFSM;

	protected float longitudinalFrictionCoeff;
	protected float lateralFrictionCoeff;
	protected float gravity;

	protected float frontTyreSlideBiasOffs = 0.0f;
	protected float rearTyreSlideBiasOffs = 0.0f;

	public float limitSpeed = -1.0f;

	protected float engineExtraPower = 0.0f;
	protected float engineNitroPower = 0.0f;

	protected float grip = 1.0f;
	protected bool damaged = false;
	
	protected float dynamicStart = -1.0f;

	protected int isOnGroundCounter = 0;
	protected List<Collider> groundColliders = new List<Collider>(8);

	protected float prevAngularDrag;
	protected float prevLinearDrag;

    protected float wingDragCoeff = 1.0f;

	protected bool isRestoring = false;

	public float onAirAngularDrag = 0.3f;
	public float onAirLinearDrag = 0.3f;
	public float velocityThreshold = 2.0f;
    /*
    public float restoreStrength = 15.0f;
    public float restoreDamping = 0.6f;
    */
	public TextAsset xml;
	public bool enablePhysics = true;

    public bool immediateGearDown = false;

    public LayerMask ignoreLayers = 0;

	protected SBSQuaternion cachedRBOri;
	protected SBSVector3 cachedRBPos;
    protected SBSVector3 cachedRBCom;
    protected SBSMatrix4x4 cachedRBMat;

    protected float engineTorqueCoeff = 3.5f;

    protected SBSVector3 prevSpeed;

    protected VehicleTrackData trackData;

    protected float lastRearingTime = -1.0f;
    protected bool rearing = false;
	
	protected Collider myCollider = null;
	protected SBSVector3[] localWheelsRestPosition;

    protected XMLNode torqueCurveNode;
    protected float xmlTorqueCoeff;
    protected float xmlRPMCoeff;

    protected Vector3 originalCenterOfMass;
    protected Vector3 originalInertiaTensor;

    protected float xmlCentralSplit;

	public void SetChassisGraphics(Transform go, float heightOffset)
	{
		chassisGraphics = go;
		chassisHeightOffset = heightOffset;
	}

    public Transform GetChassisGraphics()
    {
        return chassisGraphics;
    }

    public void SetWheelGraphics(int index, Transform go)
    {
        wheelGraphics[index] = go;
    }

    public Transform GetWheelGraphics(int index)
    {
        return wheelGraphics[index];
    }

    public SBSVector3 GetTyreForce(int index)
	{
        return tyreForces[index];
	}
    
	public SBSVector3 GetWheelPositionPrecomputed(int index)
	{
		return prevWheelPositions[index];
	}

    public void ResetInputs()
    { 
        throttleFromInput = 0.0f;
        steeringFromInput = 0.0f;
        handbrakeFromInput = false;
    }


    public void RefreshTyreParams(int tyreId, float[] _xmlLongitudinalParams, float[] _xmlLateralParams, float[] _xmlAlignParams)
    {
        tyres[tyreId].SetParams(_xmlLongitudinalParams, _xmlLateralParams, _xmlAlignParams);
    }

    public void RefreshSuspensionSpringCoeff(int tyreId, float _springCoeff )
    {
        suspensions[tyreId].SpringCoeff = _springCoeff * 0.1f;
    }

    public void RefreshDifferentialCentralSplit( float _centralSplit )
    {
        differentials[(int)VehicleUtils.DifferentialPos.CENTRAL].TorqueSplit = _centralSplit;
    }

    public void RefreshCenterOfMass(Vector3 _centerOfMass)
    {
        GetComponent<Rigidbody>().centerOfMass = _centerOfMass;

        Matrix4x4 linInertiaMatrix = Matrix4x4.identity;
        Vector3 inertiaVector = originalInertiaTensor; // rigidbody.inertiaTensor;

        Vector3 r = GetComponent<Rigidbody>().centerOfMass - originalCenterOfMass;
        float rDotR = Vector3.Dot(r, r);
        Vector3 rOff = new Vector3(rDotR - r.x * r.x, rDotR - r.y * r.y, rDotR - r.z * r.z);
        rOff = rOff * GetComponent<Rigidbody>().mass;
        inertiaVector = inertiaVector + rOff;

        linInertiaMatrix[0, 0] = inertiaVector.x;
        linInertiaMatrix[1, 1] = inertiaVector.y;
        linInertiaMatrix[2, 2] = inertiaVector.z;

        inertiaTensor = linInertiaMatrix;

        GetComponent<Rigidbody>().inertiaTensor = inertiaVector;
    }

	/*
	public void SetCamera(FiniteStateMachine _camera)
	{
		camera = _camera;
	}
    */
	protected void OnAwake()
	{
		chassisGraphics = null;/*
		wheelGraphics = new Transform[4];
		wheelGraphics[0] = null;
		wheelGraphics[1] = null;
		wheelGraphics[2] = null;
		wheelGraphics[3] = null;*/
//		camera = null;

		dynamicStart = -1.0f;

        inputs = gameObject.GetComponents<VehicleInputs>();
        trackData = gameObject.GetComponent<VehicleTrackData>();
		myCollider = GetComponent<Collider>();
        //Debug.Log("HO " + inputs.Length + " INPUTS");
        ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast"));
    }

    public void RefreshInputs()
    {
        inputs = gameObject.GetComponents<VehicleInputs>();
        //Debug.Log("HO " + inputs.Length + " INPUTS");
    }

    void Awake()
    {
        this.OnAwake();
    }

    void Start()
    {
        Init();
    }

    #region Public properties
    public VehicleType Type
    {
        get
        {
            return VehicleType.RealRacing;
        }
    }

    public float ThrottleFromInput
    {
        get
        {
            return throttleFromInput;
        }
    }

    public float SteeringFromInput
    {
        get
        {
            return steeringFromInput;
        }
    }

    public Tyre[] Tyres
    {
        get
        {
            return tyres;
        }
    }

    public Suspension[] Suspensions
    {
        get
        {
            return suspensions;
        }
    }

    public float Gravity
    {
        get
        {
            return gravity;
        }
        set
        {
            gravity = value;
        }
    }

    public SBSVector3 Position
    {
        get
        {
            return cachedRBPos;
        }
    }

    public SBSQuaternion Orientation
    {
        get
        {
            return cachedRBOri;
        }
    }

    public SBSMatrix4x4 Transform
    {
        get
        {
            return cachedRBMat;
        }
    }

    public float Mass
    {
        get
        {
            return rigidBody.mass;
        }
    }

    public float CurrentSpeed
    {
        get
        {
            return this.GetSpeed();
        }
    }

    public float Speed
    {
        get
        {
            return rigidBody.velocity.magnitude;
        }
    }

    public SBSVector3 Velocity
    {
        get
        {
            return rigidBody.velocity;
        }
    }

    public SBSVector3 PrevSpeed
    {
        get
        {
            return prevSpeed;
        }
    }

    public SBSVector3 DownVector
    {
        get
        {
            return cachedRBOri * SBSVector3.down;
	    }
    }

    public SBSVector3 ForwardVector
    {
        get
        {
            return cachedRBOri * SBSVector3.forward;
        }
    }

    public Engine Engine
    {
        get
        {
            return engine;
        }
    }

    public Gearbox GearBox
    {
        get
        {
            return gearbox;
        }
    }

	public Friction Friction
	{
		get
		{
			return friction;
		}
	}

    public bool AutoShiftEnabled
    {
        get
        {
            return autoShiftEnabled;
        }
        set
        {
            autoShiftEnabled = value;
        }
    }

    public bool AutoShiftInNeuter
    {
        get
        {
            return autoShiftInNeuter;
        }
        set
        {
            autoShiftInNeuter = value;
        }
    }

    public float FrontTyreSlideBiasOffs
    {
        get
        {
            return frontTyreSlideBiasOffs;
        }
        set
        {
            frontTyreSlideBiasOffs = value;
        }
    }

    public float RearTyreSlideBiasOffs
    {
        get
        {
            return rearTyreSlideBiasOffs;
        }
        set
        {
            rearTyreSlideBiasOffs = value;
        }
    }

    public float EngineTorqueCoeff
    {
        get
        {
            return engineTorqueCoeff;
        }
        set
        {
            engineTorqueCoeff = value;
        }
    }

    public float XmlTorqueCoeff
    {
        get
        {
            return xmlTorqueCoeff;
        }
        set
        {
            xmlTorqueCoeff = value;
        }
    }

    public float XmlCentralSplit
    {
        get
        {
            return xmlCentralSplit;
        }
    }

    public bool Initialized
    {
        get
        {
            return initialized;
        }
    }

	public bool Freezed
	{
		get
		{
			return freezed;
		}
	}

    public float EngineExtraPower
    {
        get
        {
            return engineExtraPower;
        }
        set
        {
            engineExtraPower = value;
        }
    }

    public float EngineNitroPower
    {
        get
        {
            return engineNitroPower;
        }
        set
        {
            engineNitroPower = value;
        }
    }

    public float Grip
    {
        get
        {
            return grip;
        }
        set
        {
            grip = value;
        }
    }

    public bool Damaged
    {
        get
        {
            return damaged;
        }
        set
        {
            damaged = value;
        }
    }

    public float DynamicStart
    {
        get
        {
            return dynamicStart;
        }
        set
        {
            dynamicStart = value;
        }
    }

	public bool IsOnGround
	{
		get
		{
			return isOnGroundCounter > 0 || this.wheelsOnGround();
		}
	}

    public float DefaultLinearDrag
    {
        get
        {
            return prevLinearDrag;
        }
    }

    public float WingDragCoeff
	{
		get
		{
            return wingDragCoeff;
		}
        set
        {
            wingDragCoeff = value;
        }
	}

	public float DefaultAngularDrag
	{
		get
		{
			return prevAngularDrag;
		}
        set
		{
			prevAngularDrag = value;
		}
	}

    public bool AntispinEnabled
    {
        get
        {
            return AntispinEnabled;
        }
        set
        {
            ASR_enabled = value;
        }
    }

    public bool AntispinActive
    {
        get
        {
            for (int i = 0; i < asActive.Length; ++i)
                if (asActive[i])
                    return ASR_enabled;

            return false;
        }
    }


    public VehicleUtils.Traction Traction
    {
        get
        {
            return traction;
        }
        set
        {
            traction = value;
        }
    }

    public Differential[] Differentials
    {
        get
        {
            return differentials;
        }
    }

    public float MinSteeringAngleXml
    {
        get
        {
            return minSteeringAngle;
        }
        set
        {
            minSteeringAngle = value;
        }
    }

    public float MaxSteeringAngleXml
    {
        get
        {
            return maxSteeringAngle;
        }
        set
        {
            maxSteeringAngle = value;
        }
    }

    public float SteeringSpeedRatioXml
    {
        get
        {
            return steeringSpeedRatio;
        }
        set
        {
            steeringSpeedRatio = value;
        }
    }

    public float SteeringMinSpeedXml
    {
        get
        {
            return steeringMinSpeed;
        }
        set
        {
            steeringMinSpeed = value;
        }
    }
	#endregion

    #region Ctors
    public VehiclePhysics()
    {
    }
    #endregion

    protected void Init()
    {
        if (initialized)
            return;

        rigidBody = gameObject.GetComponent<Rigidbody>();
        
        prevAngularDrag = rigidBody.angularDrag;
		prevLinearDrag = rigidBody.drag;

        wingDragCoeff = 1.0f;

        traction = VehicleUtils.Traction.RWD;
        minSteeringAngle = 10.0f;
        maxSteeringAngle = 45.0f;
        steeringSpeedRatio = 0.20f;
        steeringMinSpeed = 0.0f;
        differentials = new Differential[3];
        suspensions = new Suspension[4];
        wheels = new Wheel[4];
        brakes = new Brake[4];
        tyres = new Tyre[4];
        wings = null;

        tyreForces = new SBSVector3[4];
//      tmpVars = new SBSVector3[4];

        autoClutch = true;
        prevAutoClutch = 1.0f;
        remainingShiftTime = 0.0f;
        shiftGear = 0;
        shifted = true;
        prevSteer = 0.0f;
        //autoShiftEnabled = true;
        //autoShiftInNeuter = true;

        engineExtraPower = 0.0f;
        engineNitroPower = 0.0f;
        grip = 1.0f;
        damaged = false;

        longitudinalFrictionCoeff = 0.24f;
        lateralFrictionCoeff = 0.65f;
        gravity = Physics.gravity.y;//15.0f;//9.81f;//12.0f;//9.81f;//9.81f;//15.0f;//9.81f;

        ParseVehicleXml();

        for (int i = 0; i < tyres.Length; i++)
        {
            if ( i < 2)
                tyres[i].SlideBias += frontTyreSlideBiasOffs;
            else
                tyres[i].SlideBias += rearTyreSlideBiasOffs;
        }

        
        //Debug.Log(String.Format("1 rigidbody.centerOfMass = {0}", rigidbody.centerOfMass));
        originalCenterOfMass = GetComponent<Rigidbody>().centerOfMass;
        originalInertiaTensor = GetComponent<Rigidbody>().inertiaTensor;
        //rigidBody.mass = 1000.0f; //1000.0f;//1100.0f;
        Matrix4x4 linInertiaMatrix = Matrix4x4.identity;
        Vector3 inertiaVector = GetComponent<Rigidbody>().inertiaTensor;

        //Debug.Log(String.Format("1.1 rigidbody.inertiaTensor = {0}", rigidbody.inertiaTensor));

        //rigidbody.centerOfMass = new Vector3(0.0f, 0.8f, -0.15f);
        //rigidbody.centerOfMass = new Vector3(0.0f, 1.25f, -0.15f);
/*        rigidbody.centerOfMass = new Vector3(0.0f, 1.25f, -0.15f);
        rigidbody.centerOfMass = new Vector3(0.0f, -0.75f, -0.15f);
        rigidbody.centerOfMass = new Vector3(0.0f, 0.75f, -0.20f);
        rigidbody.centerOfMass = new Vector3(0.0f, 0.75f, -0.25f);
        */
        //Debug.Log(String.Format("2 rigidbody.centerOfMass = {0}", rigidbody.centerOfMass));

        //Debug.Log(String.Format("1.2 rigidbody.inertiaTensor = {0}", rigidbody.inertiaTensor));
        //rigidbody.centerOfMass = new Vector3(0.0f, 0.0f, 1.0f);
        //rigidbody.centerOfMass = new Vector3(0.0f, 0.0f, 0.0f);
        /*
        Vector3 d = rigidbody.centerOfMass - originalCenterOfMass; // -rigidbody.centerOfMass;
        Vector3 dSquared = new Vector3(d.x * d.x, d.y * d.y, d.z * d.z);

        Debug.Log(String.Format("3 dSquared = {0}", dSquared));

        Debug.Log(String.Format("4 inertiaVector = {0}", inertiaVector));

        inertiaVector = inertiaVector + rigidbody.mass * dSquared;
        */
        Vector3 r = GetComponent<Rigidbody>().centerOfMass - originalCenterOfMass; // -rigidbody.centerOfMass;
        float rDotR = Vector3.Dot(r, r);
        Vector3 rOff = new Vector3(rDotR - r.x * r.x, rDotR - r.y * r.y, rDotR - r.z * r.z);
        rOff = rOff * GetComponent<Rigidbody>().mass;
        inertiaVector = inertiaVector + rOff;
        //Debug.Log(String.Format("5 inertiaVector = {0}", inertiaVector));

        linInertiaMatrix[0, 0] = inertiaVector.x;
        linInertiaMatrix[1, 1] = inertiaVector.y;
        linInertiaMatrix[2, 2] = inertiaVector.z;
		
		inertiaTensor = linInertiaMatrix;

        GetComponent<Rigidbody>().inertiaTensor = inertiaVector;
        
        SetInitialConditions(GetComponent<Rigidbody>().position, GetComponent<Rigidbody>().rotation);

        VehicleSounds sounds = gameObject.GetComponent<VehicleSounds>();
        if (sounds != null)
            sounds.Init();
		
		cachedRBOri = rigidBody.rotation;
		cachedRBOri.Normalize();
		cachedRBPos = rigidBody.position;
        cachedRBMat = SBSMatrix4x4.TRS(cachedRBPos, cachedRBOri, SBSVector3.one);
        cachedRBCom = cachedRBMat * rigidBody.centerOfMass;
		
		SBSMatrix4x4 invRBMat = cachedRBMat.inverseFast;
		localWheelsRestPosition = new SBSVector3[4];
		localWheelsRestPosition[0] = invRBMat * this.GetWheelPositionAtDisplacement(0, 1.0f);
		localWheelsRestPosition[1] = invRBMat * this.GetWheelPositionAtDisplacement(1, 1.0f);
		localWheelsRestPosition[2] = invRBMat * this.GetWheelPositionAtDisplacement(2, 1.0f);
		localWheelsRestPosition[3] = invRBMat * this.GetWheelPositionAtDisplacement(3, 1.0f);

        VehiclesManager.Instance.AddVehicle(gameObject);

        initialized = true;

        //Debug.Log(String.Format("downShiftPoint = {0} - {1} - {2} - {3} - {4} - {5}", DownshiftPoint(0), DownshiftPoint(1), DownshiftPoint(2), DownshiftPoint(3), DownshiftPoint(4), DownshiftPoint(5)));
    }

    void OnDestroy()
    {
        if (VehiclesManager.Instance != null)
            VehiclesManager.Instance.RemoveVehicle(gameObject);
    }

    /*
    private void SetLaunchedStart()
    {
        rigidbody.velocity = rigidbody.rotation * new Vector3(0.0f, 53.0f, 0.0f);
        Engine.SetRPM(8000.0f);
        GearBox.ChangeGear(3);
        SetWheelsSpeed(170.0f);
    }
    */

    protected void InititializeDynamicStart(float speedKmh)
    {
        float linVelocity = speedKmh * VehicleUtils.ToMetersPerSecs;
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().rotation * new Vector3(0.0f, 0.0f, linVelocity);
        for (int i = 0; i < gearbox.ForwardGears; ++i)
        {
            int gearId = i + 1;
            float wheelSpeed = linVelocity / tyres[0].Radius;
            float finalDrive = 1.0f;
            switch (traction)
            {
                case VehicleUtils.Traction.FWD:
                    finalDrive = differentials[(int)VehicleUtils.DifferentialPos.FRONT].FinalDrive;
                    break;
                case VehicleUtils.Traction.RWD:
                    finalDrive = differentials[(int)VehicleUtils.DifferentialPos.REAR].FinalDrive;
                    break;
                case VehicleUtils.Traction.AWD:
                    finalDrive = differentials[(int)VehicleUtils.DifferentialPos.CENTRAL].FinalDrive;
                    break;
            }

            float engineRPM = wheelSpeed * gearbox.GetGearRatio(gearId) * finalDrive / (2.0f * Mathf.PI) * 60.0f;

            if (engineRPM < engine.RPMLimit || i >= (gearbox.ForwardGears - 1))
            {
                Debug.Log("linVelocity: " + linVelocity + " finalDrive: " + finalDrive + " gear[" + gearId + "] = " + gearbox.GetGearRatio(gearId) + " ws: " + wheelSpeed + " engineRPM: " + engineRPM);
                Engine.SetRPM(engineRPM);
                GearBox.ChangeGear(gearId);
                SetWheelsSpeed(wheelSpeed);
                break;
            }
        }
    }

    protected void ParseVehicleXml()
    {
        XMLReader vehicleXml = new XMLReader();//new XmlDocument();
        XMLNode root = vehicleXml.read(xml.text).children[0] as XMLNode;//LoadXml(xml.text);
//		vehicleXml.printXML(root, 0);
		/*
        XmlNode root = vehicleXml.ChildNodes[1];*/
        Asserts.Assert("Vehicle" == root.tagName, "Invalid Vehicle xml");
        /*
        XmlAttribute tractionAttribute = root.Attributes["traction"];
        if (tractionAttribute != null)
        {
            string tractionType = root.Attributes["traction"].Value;
            switch (tractionType)
            {
                case "FWD": traction = VehicleUtils.Traction.FWD;
                    break;
                case "RWD": traction = VehicleUtils.Traction.RWD;
                    break;
                case "AWD": traction = VehicleUtils.Traction.AWD;
                    break;
            }
        }
        */
        foreach (XMLNode child in root.children)
        {
            switch (child.tagName)
            {
                case "Chassis":
                    ParseChassis(child);
                    break;
                case "Steering":
                    ParseSteering(child);
                    break;
                case "Engine":
                    ParseEngine(child);
                    break;
                case "Friction":
                    ParseFriction(child);
                    break;
                case "Gearbox":
                    ParseGearbox(child);
                    break;
                case "Differential":
                    ParseDifferential(child);
                    break;
                case "Suspensions":
                    ParseSuspensions(child);
                    break;
                case "Tyres":
                    ParseTyres(child);
                    break;
                case "Wheels":
                    ParseWheels(child);
                    break;
                case "Brakes":
                    ParseBrakes(child);
                    break;
                case "Wings":
                    ParseWings(child);
                    break;
            }
        }
    }

    private void ParseChassis(XMLNode root)
    {
        //Debug.Log("root[Traction]: " + root["Traction"]);
        if (root["Traction"] != null)
        {
            string xmlTraction = root["Traction"].innerText;//.InnerText;
            //Debug.Log("xmlTraction: " + xmlTraction);
            switch (xmlTraction)
            {
                case "FWD": traction = VehicleUtils.Traction.FWD;
                    break;
                case "RWD": traction = VehicleUtils.Traction.RWD;
                    break;
                case "AWD": traction = VehicleUtils.Traction.AWD;
                    break;
            }
        }
        GetComponent<Rigidbody>().mass = XMLUtils.ParseFloat(root["Mass"]);
        GetComponent<Rigidbody>().centerOfMass = XMLUtils.ParseVector3(root["CenterOfMass"]);
    }

    private void ParseSteering(XMLNode root)
    {
        minSteeringAngle = XMLUtils.ParseFloat(root["MinAngle"]);
        maxSteeringAngle = XMLUtils.ParseFloat(root["MaxAngle"]);
        steeringSpeedRatio = XMLUtils.ParseFloat(root["SpeedRatio"]);
        steeringMinSpeed = XMLUtils.ParseFloat(root["MinSpeed"]);
    }

	protected List<Keyframe> ParseEngineTorqueCurve(XMLNode root, float torqueCoeff, float rpmCoeff)
	{
		List<Keyframe> curve = new List<Keyframe>();
		foreach (XMLNode child in root.children)
		{
			if ("Sample" == child.tagName)
			{
				float[] sampleParams = XMLUtils.ParseFloatArray(child);
				curve.Add(new Keyframe(sampleParams[0] * rpmCoeff, sampleParams[1] * torqueCoeff));
			}
		}
		if (curve.Count < 2)
			Asserts.Assert(false, "Must be defined at leat 2 curve samples in: " + root.tagName);

        if (curve[0].time != 0.0f)
            curve.Insert(0, new Keyframe(0.0f, 0.0f));

        return curve;
    }

    private void ParseEngine(XMLNode root)
    {
        float xmlInertia = XMLUtils.ParseFloat(root["Inertia"]);
        float xmlMass = XMLUtils.ParseFloat(root["Mass"]);
        Vector3 xmlPosition = XMLUtils.ParseVector3(root["Position"]);
        float xmlRPMRedLine = XMLUtils.ParseFloat(root["RPMRedline"]);
        float xmlRPMLimit = XMLUtils.ParseFloat(root["RPMLimit"]);
        float xmlRPMStart = XMLUtils.ParseFloat(root["RPMStart"]);
        float xmlRPMStall = XMLUtils.ParseFloat(root["RPMStall"]);
        float xmlIdleThrottle = XMLUtils.ParseFloat(root["IdleThrottle"]);
        float xmlTorqueFriction = XMLUtils.ParseFloat(root["TorqueFriction"]);
        xmlTorqueCoeff = XMLUtils.ParseFloat(root["TorqueCoeff"]);
        xmlRPMCoeff = null == root["RPMCoeff"] ? 1.0f : XMLUtils.ParseFloat(root["RPMCoeff"]);
        engineTorqueCoeff = null == root["EngineTorqueCoeff"] ? 3.5f : XMLUtils.ParseFloat(root["EngineTorqueCoeff"]);
        
        engine = new Engine(xmlInertia, xmlMass, xmlPosition, xmlRPMRedLine * xmlRPMCoeff, xmlRPMLimit * xmlRPMCoeff, xmlIdleThrottle, xmlRPMStart, xmlRPMStall, xmlTorqueFriction);

        torqueCurveNode = (XMLNode)root["TorqueCurve"];
        if (torqueCurveNode != null)
        {
            List<Keyframe> torqueCurve = ParseEngineTorqueCurve(torqueCurveNode, xmlTorqueCoeff, xmlRPMCoeff);
            engine.SetTorqueCurve(engine.RPMRedline, torqueCurve.ToArray(), VehicleUtils.CurveSmooth.Auto);
        }
        else
        {
            Asserts.Assert(false, "Missing TorqueCurve node in: " + root.tagName);
        }
    }

    public void RefreshEngineTorqueCurve()
    {
        List<Keyframe> torqueCurve = ParseEngineTorqueCurve(torqueCurveNode, xmlTorqueCoeff, xmlRPMCoeff);
        engine.SetTorqueCurve(engine.RPMRedline, torqueCurve.ToArray(), VehicleUtils.CurveSmooth.Auto);
    }

    private void ParseFriction(XMLNode root)
    {
        float xmlFrictionCoeff = XMLUtils.ParseFloat(root["FrictionCoeff"]);
        float xmlMaxPressure = XMLUtils.ParseFloat(root["MaxPressure"]);
        float xmlRadius = XMLUtils.ParseFloat(root["Radius"]);
        float xmlArea = XMLUtils.ParseFloat(root["Area"]);

        friction = new Friction(xmlFrictionCoeff, xmlMaxPressure, xmlRadius, xmlArea, 0.001f);
    }

    private void ParseGearboxRatios(XMLNode root)
    {
        int count = 0;
        foreach (XMLNode child in root.children)
        {
            if ("Ratio" == child.tagName)
            {
                float ratio = XMLUtils.ParseFloat(child);
                if (count == 0)
                {
                    gearbox.SetGearRatio(-1, ratio);
                    gearbox.SetGearRatio(0, 0.0f);
                }
                else
                {
                    gearbox.SetGearRatio(count, ratio);
                }
                count++;
            }
        }
    }

    private void ParseGearbox(XMLNode root)
    {
        float xmlShiftDelay = XMLUtils.ParseFloat(root["ShiftDelay"]);

        gearbox = new Gearbox(xmlShiftDelay, 0);

        XMLNode ratiosNode = (XMLNode)root["Ratios"];
        if (ratiosNode != null)
        {
            ParseGearboxRatios(ratiosNode);
        }
        else
        {
            Asserts.Assert(false, "Missing Ratios node in: " + root.tagName);
        }
    }

    private void ParseDifferential(XMLNode root)
    {
        float xmlFinalDrive = XMLUtils.ParseFloat(root["FinalDrive"]);
        float xmlAntiSlip = XMLUtils.ParseFloat(root["AntiSlip"]);
        xmlCentralSplit = XMLUtils.ParseFloat(root["CentralSplit"]);

        float AntiSlipTorque = 0.0f;
        float AntiSlipTorqueDecCoeff = 0.0f;
        float TorqueSplit = 0.5f;

        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                differentials[(int)VehicleUtils.DifferentialPos.FRONT] = new Differential(xmlFinalDrive, xmlAntiSlip, AntiSlipTorque, AntiSlipTorqueDecCoeff, TorqueSplit);
                break;
            case VehicleUtils.Traction.RWD:
                differentials[(int)VehicleUtils.DifferentialPos.REAR] = new Differential(xmlFinalDrive, xmlAntiSlip, AntiSlipTorque, AntiSlipTorqueDecCoeff, TorqueSplit);
                break;
            case VehicleUtils.Traction.AWD:
                differentials[(int)VehicleUtils.DifferentialPos.FRONT] = new Differential(1.0f, xmlAntiSlip, AntiSlipTorque, AntiSlipTorqueDecCoeff, TorqueSplit);
                differentials[(int)VehicleUtils.DifferentialPos.REAR] = new Differential(1.0f, xmlAntiSlip, AntiSlipTorque, AntiSlipTorqueDecCoeff, TorqueSplit);
                differentials[(int)VehicleUtils.DifferentialPos.CENTRAL] = new Differential(xmlFinalDrive, xmlAntiSlip, AntiSlipTorque, AntiSlipTorqueDecCoeff, xmlCentralSplit);
                break;
        }
    }

    private void ParseSuspensions(XMLNode root)
    {
        foreach (XMLNode child in root.children)
        {
            if ("Suspension" == child.tagName)
            {
                string axleAttribute = child.GetAttributeAsString("axle");
                string sideAttribute = child.GetAttributeAsString("side");
                if (axleAttribute == null)
                {
                    Asserts.Assert(false, "Missing axle attribute in: " + root.tagName);
                }

                int startIdx = -1;
                int endIdx = -1;
                if (axleAttribute == "front")
                {
                    startIdx = (int)VehicleUtils.WheelPos.FR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.FR_RIGHT;
                }
                else if (axleAttribute == "rear")
                {
                    startIdx = (int)VehicleUtils.WheelPos.RR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.RR_RIGHT;
                }

                if (sideAttribute != null)
                {
                    if (sideAttribute == "left")
                        endIdx = startIdx;
                    else
                        startIdx = endIdx;
                }

                if (startIdx >= 0 && endIdx >= 0)
                {
                    if (startIdx != endIdx)
                    {
                        float xmlSpringCoeff = XMLUtils.ParseFloat(child["SpringCoeff"]);
                        float xmlBounce = XMLUtils.ParseFloat(child["Bounce"]);
                        float xmlRebound = XMLUtils.ParseFloat(child["Rebound"]);
//                      float xmlMaxVelocity = XMLUtils.ParseFloat(child["MaxVelocity"]);
                        float xmlMaxForce = (null == child["MaxForce"] ? -1.0f : XMLUtils.ParseFloat(child["MaxForce"]));
                        float xmlTravel = XMLUtils.ParseFloat(child["Travel"]);
                        float xmlAntirollCoeff = XMLUtils.ParseFloat(child["AntirollCoeff"]);
                        float xmlCamber = XMLUtils.ParseFloat(child["Camber"]);
                        float xmlCaster = XMLUtils.ParseFloat(child["Caster"]);
                        float xmlToe = XMLUtils.ParseFloat(child["Toe"]);
						float xmlRestitution = (null == child["Restitution"] ? -1.0f : XMLUtils.ParseFloat(child["Restitution"]));
						float xmlPenalty = (null == child["Penalty"] ? 0.0f : XMLUtils.ParseFloat(child["Penalty"]));

						for (int i = startIdx; i <= endIdx; i++)
						{
							suspensions[i] = new Suspension(xmlSpringCoeff * 0.1f, xmlBounce, xmlRebound, xmlTravel, xmlAntirollCoeff, xmlCamber, xmlCaster, xmlToe, xmlRestitution, xmlPenalty);
//                          suspensions[i].MaxVelocity = xmlMaxVelocity;
                            suspensions[i].MaxForce = xmlMaxForce;
                            //Debug.Log(String.Format("Suspension[{0}] = {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", i, xmlSpringCoeff, xmlBounce, xmlRebound, xmlTravel, xmlAntirollCoeff, xmlCamber, xmlCaster, xmlToe));
						}
					}
					else
					{
						Vector3 xmlHinge = XMLUtils.ParseVector3(child["Hinge"]);

                        suspensions[startIdx].Hinge = xmlHinge;
                    }
                }
            }
        }
    }

    private void ParseTyres(XMLNode root)
    {
        foreach (XMLNode child in root.children)
        {
            if ("Tyre" == child.tagName)
            {
                string axleAttribute = child.GetAttributeAsString("axle");
                if (axleAttribute == null)
                {
                    Asserts.Assert(false, "Missing axle attribute in: " + root.tagName);
                }

                int startIdx = -1;
                int endIdx = -1;
                if (axleAttribute == "front")
                {
                    startIdx = (int)VehicleUtils.WheelPos.FR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.FR_RIGHT;
                }
                else if (axleAttribute == "rear")
                {
                    startIdx = (int)VehicleUtils.WheelPos.RR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.RR_RIGHT;
                }

                if (startIdx >= 0 && endIdx >= 0)
                {
                    if (startIdx != endIdx)
                    {
                        float xmlRadius = XMLUtils.ParseFloat(child["Radius"]);
                        float xmlTread = XMLUtils.ParseFloat(child["Tread"]);
                        float xmlSlideBias = XMLUtils.ParseFloat(child["SlideBias"]);
                        float xmlRollResistance = XMLUtils.ParseFloat(child["RollResistance"]);
                        float xmlRollResistanceCoeff = XMLUtils.ParseFloat(child["RollResistanceCoeff"]);

                        float[] xmlLongitudinalParams = XMLUtils.ParseFloatArray(child["LongitudinalParams"]);
                        float[] xmlLateralParams = XMLUtils.ParseFloatArray(child["LateralParams"]);
                        float[] xmlAlignParams = XMLUtils.ParseFloatArray(child["AlignParams"]);

                        for (int i = startIdx; i <= endIdx; i++)
                        {
                            tyres[i] = new Tyre();
                            Tyre tyre = tyres[i];

                            tyre.Radius = xmlRadius;
                            tyre.Tread = xmlTread;
                            tyre.SlideBias = xmlSlideBias;
                            tyre.SetRollResistance(xmlRollResistance, xmlRollResistanceCoeff);
                            tyre.SetParams(xmlLongitudinalParams, xmlLateralParams, xmlAlignParams);

                            tyre.InitSigmaMaxCurve(20);
                            tyre.InitAlphaMaxCurve(20);
                        }
                    }
                }
            }
        }
    }

    private void ParseWheels(XMLNode root)
    {
        foreach (XMLNode child in root.children)
        {
            if ("Wheel" == child.tagName)
            {
                string axleAttribute = child.GetAttributeAsString("axle");
                string sideAttribute = child.GetAttributeAsString("side");
                if (axleAttribute == null)
                {
                    Asserts.Assert(false, "Missing axle attribute in: " + root.tagName);
                }
                if (sideAttribute == null)
                {
                    Asserts.Assert(false, "Missing side attribute in: " + root.tagName);
                }

                int startIdx = -1;
                int endIdx = -1;
                if (axleAttribute == "front")
                {
                    startIdx = (int)VehicleUtils.WheelPos.FR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.FR_RIGHT;
                }
                else if (axleAttribute == "rear")
                {
                    startIdx = (int)VehicleUtils.WheelPos.RR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.RR_RIGHT;
                }

                if (sideAttribute != null)
                {
                    if (sideAttribute == "left")
                        endIdx = startIdx;
                    else
                        startIdx = endIdx;
                }

                if (startIdx >= 0 && endIdx >= 0)
                {
                    Vector3 xmlPosition = XMLUtils.ParseVector3(child["Position"]);
                    float xmlRollHeight = XMLUtils.ParseFloat(child["RollHeight"]);
                    float xmlMass = XMLUtils.ParseFloat(child["Mass"]);
                    float xmlInertia = XMLUtils.ParseFloat(child["Inertia"]);

                    wheels[startIdx] = new Wheel(xmlRollHeight, xmlMass, xmlPosition, xmlInertia);
                }
            }
        }
    }

    private void ParseBrakes(XMLNode root)
    {
        foreach (XMLNode child in root.children)
        {
            if ("Brake" == child.tagName)
            {
                string axleAttribute = child.GetAttributeAsString("axle");
                if (axleAttribute == null)
                {
                    Asserts.Assert(false, "Missing axle attribute in: " + root.tagName);
                }

                int startIdx = -1;
                int endIdx = -1;
                if (axleAttribute == "front")
                {
                    startIdx = (int)VehicleUtils.WheelPos.FR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.FR_RIGHT;
                }
                else if (axleAttribute == "rear")
                {
                    startIdx = (int)VehicleUtils.WheelPos.RR_LEFT;
                    endIdx = (int)VehicleUtils.WheelPos.RR_RIGHT;
                }

                if (startIdx >= 0 && endIdx >= 0)
                {
                    if (startIdx != endIdx)
                    {
                        float xmlFriction = XMLUtils.ParseFloat(child["Friction"]);
                        float xmlMaxPressure = XMLUtils.ParseFloat(child["MaxPressure"]);
                        float xmlRadius = XMLUtils.ParseFloat(child["Radius"]);
                        float xmlArea = XMLUtils.ParseFloat(child["Area"]);
                        float xmlBias = XMLUtils.ParseFloat(child["Bias"]);
                        float xmlThreshold = 2e-4f;
                        float xmlHandbrake = XMLUtils.ParseFloat(child["Handbrake"]);

                        for (int i = startIdx; i <= endIdx; i++)
                            brakes[i] = new Brake(xmlFriction, xmlMaxPressure * xmlBias, xmlRadius, xmlArea, xmlBias, xmlThreshold, xmlHandbrake);
                    }
                }
            }
        }
    }

    private void ParseWings(XMLNode root)
    {
        List<Wing> wingList = new List<Wing>();
        float xmlAirDensity = 1.2f;
        foreach (XMLNode child in root.children)
        {
            if ("Drag" == child.tagName)
            {
                float xmlDragArea = XMLUtils.ParseFloat(child["DragArea"]);
                float xmlDragCoeff = XMLUtils.ParseFloat(child["DragCoeff"]);
                Vector3 xmlPosition = XMLUtils.ParseVector3(child["Position"]);

                Wing wing = new Wing(xmlAirDensity, xmlDragArea, xmlDragCoeff, 0.0f, 0.0f, 0.0f, xmlPosition);
                wingList.Add(wing);
            }
            else if ("Wing" == child.tagName)
            {
                float xmlDragArea = XMLUtils.ParseFloat(child["DragArea"]);
                float xmlDragCoeff = XMLUtils.ParseFloat(child["DragCoeff"]);
                float xmlLiftArea = XMLUtils.ParseFloat(child["LiftArea"]);
                float xmlLiftCoeff = XMLUtils.ParseFloat(child["LiftCoeff"]);
                float xmlLiftEfficiency = XMLUtils.ParseFloat(child["LiftEfficiency"]);
                Vector3 xmlPosition = XMLUtils.ParseVector3(child["Position"]);

                Wing wing = new Wing(xmlAirDensity, xmlDragArea, xmlDragCoeff, xmlLiftArea, xmlLiftCoeff, xmlLiftEfficiency, xmlPosition);
                wingList.Add(wing);
            }
        }
        wings = wingList.ToArray();
    }

    private float CalcDriveshaftSpeed ( )
    {
        float result = 0.0f;
        float frLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_LEFT].AngularVelocity;
        float frRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].AngularVelocity;
        float rrLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_LEFT].AngularVelocity;
        float rrRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_RIGHT].AngularVelocity;

        //Debug.Log("frLeftWheelSpeed: " + frLeftWheelSpeed + " rigidbody.velocity: " + rigidbody.velocity + " Engine.RPM: " + engine.RPM + " GearBox: " + GearBox.CurrentGear); 

        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                result = differentials[(int)VehicleUtils.DifferentialPos.FRONT].UpdateDriveshaftSpeed(frLeftWheelSpeed, frRightWheelSpeed);
                break;
            case VehicleUtils.Traction.RWD:
                result = differentials[(int)VehicleUtils.DifferentialPos.REAR].UpdateDriveshaftSpeed(rrLeftWheelSpeed, rrRightWheelSpeed);
                break;
            case VehicleUtils.Traction.AWD:
                float frontDiffSpeed = differentials[(int)VehicleUtils.DifferentialPos.FRONT].UpdateDriveshaftSpeed(frLeftWheelSpeed, frRightWheelSpeed);
                float rearDiffSpeed = differentials[(int)VehicleUtils.DifferentialPos.REAR].UpdateDriveshaftSpeed(rrLeftWheelSpeed, rrRightWheelSpeed);
                result = differentials[(int)VehicleUtils.DifferentialPos.CENTRAL].UpdateDriveshaftSpeed(frontDiffSpeed, rearDiffSpeed);
                break;
        }

        return result;
    }

    public void SetWheelsSpeed(float speed)
    {
        if (wheels == null)
            return;

        for (int i = 0; i < 4; ++i)
        {
            if (wheels[i] != null)
            {
                wheels[i].AngularVelocity = speed;
                Debug.Log("wheel vel[" + i + "]: " + wheels[i].AngularVelocity);
            }
        }
    }

    //engine drag force from clutch
    private void ApplyFrictionTorque(float engineDrag, float frictionSpeed)
    {
        if (gearbox.CurrentGear == 0)
        {
            engine.ClutchTorque = 0.0f;
        }
        else
        {
            if (friction.Engaged)
            {
                //engine speed = transmission speed
                engine.ClutchTorque = 0.0f;
                engine.AngularVelocity = frictionSpeed;
            }
            else
            {
                //pass engine clutch torque
                engine.ClutchTorque = engineDrag;
            }
        }
    }

    private void UpdateDriveTorque(float[] wheelDriveTorques, float frictionTorque)
    {
        float driveshaftTorque = gearbox.GetTorque(frictionTorque);

        for (int i = 0; i < (int)VehicleUtils.WheelPos.NUM_WHEELS; i++)
            wheelDriveTorques[i] = 0.0f;

        Differential frontDifferential = differentials[(int)VehicleUtils.DifferentialPos.FRONT];
        Differential rearDifferential = differentials[(int)VehicleUtils.DifferentialPos.REAR];

        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                frontDifferential.UpdateWheelTorques(driveshaftTorque);
                wheelDriveTorques[(int)VehicleUtils.WheelPos.FR_LEFT] = frontDifferential.SideLeftTorque;
                wheelDriveTorques[(int)VehicleUtils.WheelPos.FR_RIGHT] = frontDifferential.SideRightTorque;
                break;
            case VehicleUtils.Traction.RWD:
                rearDifferential.UpdateWheelTorques(driveshaftTorque);
                wheelDriveTorques[(int)VehicleUtils.WheelPos.RR_LEFT] = rearDifferential.SideLeftTorque;
                wheelDriveTorques[(int)VehicleUtils.WheelPos.RR_RIGHT] = rearDifferential.SideRightTorque;
                break;
            case VehicleUtils.Traction.AWD:
                Differential centralDifferential = differentials[(int)VehicleUtils.DifferentialPos.CENTRAL];
                centralDifferential.UpdateWheelTorques(driveshaftTorque);
                frontDifferential.UpdateWheelTorques(centralDifferential.SideLeftTorque);
                rearDifferential.UpdateWheelTorques(centralDifferential.SideRightTorque);

                // Debug.Log("driveshaftTorque: " + driveshaftTorque + " centralDifferential.SideLeftTorque: " + centralDifferential.SideLeftTorque + " centralDifferential.SideRightTorque: " + centralDifferential.SideRightTorque);
               
                wheelDriveTorques[(int)VehicleUtils.WheelPos.FR_LEFT] = frontDifferential.SideLeftTorque;
                wheelDriveTorques[(int)VehicleUtils.WheelPos.FR_RIGHT] = frontDifferential.SideRightTorque;
                wheelDriveTorques[(int)VehicleUtils.WheelPos.RR_LEFT] = rearDifferential.SideLeftTorque;
                wheelDriveTorques[(int)VehicleUtils.WheelPos.RR_RIGHT] = rearDifferential.SideRightTorque;
               
                break;
        }

        //DebugUtils.AddWatch(new Rect(0, 1 * 20, 200, 20), "driveshaftTorque: {0}", driveshaftTorque);
    }

#if UNITY_FLASH
    private void ApplyEngineTorque(SBSVector3 totalForce, SBSVector3 totalTorque)
#else
    private void ApplyEngineTorque(ref SBSVector3 totalForce, ref SBSVector3 totalTorque)
#endif
    {
        //Vector3 engineTorque = new Vector3(-engine.Torque, 0.0f, 0.0f);
        //Vector3 engineTorque = new Vector3(0.0f, engine.Torque, 0.0f);
        //Vector3 engineTorque = new Vector3(engine.Torque * 3.5f, 0.0f, 0.0f);
        //Vector3 engineTorque = new Vector3(engine.Torque * 3.5f, 0.0f, 0.0f);
//      SBSVector3 engineTorque = rigidBody.transform.TransformDirection(new Vector3(engine.Torque * 3.5f, 0.0f, 0.0f));
        SBSVector3 engineTorque = cachedRBMat.MultiplyVector(engine.Torque * engineTorqueCoeff, 0.0f, 0.0f);//rigidBody.transform.TransformDirection(new Vector3(engine.Torque * 3.5f, 0.0f, 0.0f));
        SBSVector3 enginePos = cachedRBMat * engine.Position;//VehicleToWorld(engine.Position);// - CenterOfMassPos); // PIE_271111

#if UNITY_FLASH
        GetForceAndTorqueAtOffset(Vector3.zero, engineTorque, enginePos, totalForce, totalTorque);
#else
        GetForceAndTorqueAtOffset(Vector3.zero, engineTorque, enginePos, ref totalForce, ref totalTorque);
#endif
    }

    public void SetWheelsParam(float param)
    {
        for (var i = 2; i < 4; ++i)
        {
        //    tyres[i].lateralParams[2] = param;
        }
    }

#if UNITY_FLASH
    private void ApplyWingsForce(SBSVector3 totalForce, SBSVector3 totalTorque)
#else
    private void ApplyWingsForce(ref SBSVector3 totalForce, ref SBSVector3 totalTorque)
#endif
    {
        float prevDragCoeff;
        for (int i = wings.Length - 1; i >= 0; --i)
        {
            SBSVector3 windVelocity = rigidBody.transform.InverseTransformDirection(-rigidBody.velocity);
//          SBSVector3 windForce = wings[i].GetForce(windVelocity, i);
            SBSVector3 wingPos = cachedRBMat * wings[i].Position;//VehicleToWorld(wings[i].Position);// - CenterOfMassPos); //PIE_271111

            prevDragCoeff = wings[i].DragCoeff;
            wings[i].DragCoeff = prevDragCoeff * wingDragCoeff;
            SBSVector3 windForce = cachedRBOri * wings[i].GetForce(windVelocity, i);
            wings[i].DragCoeff = prevDragCoeff;
#if UNITY_FLASH
            GetForceAtOffset(windForce, wingPos, totalForce, totalTorque);
#else
            GetForceAtOffset(windForce, wingPos, ref totalForce, ref totalTorque);
#endif

            //rigidbody.AddForceAtPosition(windForce, wingPos);
        }

        //Vector3 rotationalDrag = -rigidBody.angularVelocity * 1000.0f;
        //totalTorque = totalTorque + rotationalDrag;
    }


    private void ComputeSuspensionDisplacement(int idx, float dt)
    {
        //TODO
        ContactData wheelContact = wheelContacts[idx];
        float TwoSqrt = 1.414214f;
        float px = wheelContact.Position.x;
        float pz = wheelContact.Position.z;// y;
        float bumpPhase = 2.0f * Mathf.PI * (px + pz) / wheelContact.BumpWaveLength;
        float bumpShift = 2.0f * SBSMath.Sin(bumpPhase * TwoSqrt);
        float bumpAmplitude = 0.25f * wheelContact.BumpAmplitude;
        float bumpOffset = bumpAmplitude * (SBSMath.Sin(bumpPhase + bumpShift) + SBSMath.Sin(bumpPhase * TwoSqrt) - 2.0f);

        float wheelHeight = wheelContact.WheelHeight - tyres[idx].Radius - bumpOffset;

        suspensions[idx].Displacement = suspensions[idx].Travel - wheelHeight;
    }

#if UNITY_FLASH
	private SBSVector3 ApplySuspensionForce(int idx, float dt, SBSVector3 totalForce, SBSVector3 totalTorque)
#else
    private SBSVector3 ApplySuspensionForce(int idx, float dt, ref SBSVector3 totalForce, ref SBSVector3 totalTorque)
#endif
    {
        SBSVector3 applyPos = prevWheelPositions[idx];// GetWheelPositionTMP(idx);// -(rigidbody.rotation * CenterOfMassPos); // PIE_271111
        //float suspVel = SBSVector3.Dot(rigidBody.GetPointVelocity(applyPos), DownVector);

        float force = suspensions[idx].GetForce(dt, idx, 0.0f);//suspVel);

		int antirollIdx = idx;
		if (idx == 0 || idx == 2)
			antirollIdx++;
		else
			antirollIdx--;

		float antirollForce = suspensions[idx].AntirollCoeff * (suspensions[idx].Displacement - suspensions[antirollIdx].Displacement);
        SBSVector3 suspensionForce = cachedRBOri * (SBSVector3.up * (antirollForce + force));
#if UNITY_FLASH
		GetForceAtOffset(suspensionForce, applyPos, totalForce, totalTorque);
#else
        GetForceAtOffset(suspensionForce, applyPos, ref totalForce, ref totalTorque);
#endif

#if UNITY_EDITOR
		lastSuspensionApply[idx] = applyPos; // GetWheelPosition(idx);
#endif
		// TODO Overtravel
		float overTravel = suspensions[idx].Overtravel;
		if (overTravel > 0.0f)// && suspensions[idx].Velocity > 0.0f)
		{
			float invM = 1.0f / rigidBody.mass;

            SBSMatrix4x4 invR = cachedRBMat.inverse;
            SBSMatrix4x4 invI = new SBSMatrix4x4(
                rigidBody.inertiaTensor.x, 0.0f, 0.0f, 0.0f,
                0.0f, rigidBody.inertiaTensor.y, 0.0f, 0.0f,
                0.0f, 0.0f, rigidBody.inertiaTensor.z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);

            invI.Prepend(invR);
            invI.Append(cachedRBMat);
            invI.Invert();

            SBSVector3 r = invR.MultiplyVector(applyPos - cachedRBCom);
            SBSVector3 v = invR.MultiplyVector(rigidBody.velocity);
			SBSVector3 w = invR.MultiplyVector(rigidBody.angularVelocity);

			float e = suspensions[idx].Restitution;
			float penalty = suspensions[idx].Penalty;

			SBSVector3 rxn = SBSVector3.Cross(r, SBSVector3.down);
			float denom = invM + SBSVector3.Dot(rxn, invI.MultiplyVector(rxn));
			float J = (SBSVector3.Dot(v, SBSVector3.down) + SBSVector3.Dot(rxn, w)) / (denom * dt);
			J = SBSMath.Max(0.0f, (1.0f + e) * J);
			J += SBSMath.Min(0.00125f, overTravel) * penalty * rigidBody.mass;//TODO: MAX OVERTRAVEL

            SBSVector3 otForce = J * (cachedRBOri * SBSVector3.up);
#if UNITY_FLASH
			GetForceAtOffset(otForce, applyPos, totalForce, totalTorque);
#else
            GetForceAtOffset(otForce, applyPos, ref totalForce, ref totalTorque);
#endif
            suspensionForce += otForce;
		}

        return suspensionForce;
    }

    private SBSVector3 CalcTyreFrictionForce(int idx, float dt, SBSVector3 suspensionForce, bool frictionLimiting, float wheelSpeed, SBSVector3 groundVel, SBSQuaternion wheelOrientation)
    {
//      Vector3 wheelNormal = wheelOrientation * Vector3.up;
        float normalForce = suspensionForce.magnitude;

//      Vector3 B = MathUtils.Vector3Forward;
        ContactData contact = wheelContacts[idx];
//      Vector3 A = contact.Normal;                 //surface normal
        SBSVector3 A = (cachedRBOri * wheelOrientation).inverse * contact.Normal;
        SBSVector3 AProj = SBSVector3.Cross(SBSVector3.forward, SBSVector3.Cross(A, SBSVector3.forward));
        AProj.Normalize();
        float camberAngle = -SBSMath.Acos(SBSVector3.Dot(AProj, SBSVector3.up));//LEFTHAND

        Tyre tyre = tyres[idx];
        float frictionCoeff = tyre.Tread * contact.FrictionTread + (1.0f - tyre.Tread) * contact.FrictionNoTread;
        //Debug.Log("normalForce: " + normalForce + " frictionCoeff: " + frictionCoeff + " groundVel: " + groundVel + " wheelSpeed: " + wheelSpeed + " camberAngle: " + camberAngle);
        SBSVector3 frictionForce = tyre.GetForces(normalForce, frictionCoeff, groundVel, wheelSpeed, camberAngle, idx);
        //Debug.Log ( "frictionForce: " + frictionForce );
        float limit = 0;
        if (frictionLimiting)
        {
            limit = ((wheelSpeed - groundVel.z) * dt * rigidBody.mass * 0.25f) / (dt * dt);
            if ((frictionForce.z < 0.0f && limit > 0.0f) || (frictionForce.z > 0.0f && limit < 0.0f))
                limit = 0.0f;
            if (frictionForce.z > 0.0f && frictionForce.z > limit)
                frictionForce.z = limit;
            else if ( frictionForce.z < 0.0f && frictionForce.z < limit )
                frictionForce.z = limit;

        }

        //DebugUtils.AddWatch(new Rect(0, (11 + idx) * 20, 600, 20), String.Format("limit: {0} frictionForce: {1} groundVel: {2}, wheelSpeed: {3}, camberAngle: {4}", limit, frictionForce, groundVel, wheelSpeed, camberAngle));

        return frictionForce;
    }

#if UNITY_FLASH
    public void ApplyWheelForces(float dt, float wheelDriveTorque, int idx, SBSVector3 suspensionForce, SBSVector3 totalForce, SBSVector3 totalTorque)
#else
    public void ApplyWheelForces(float dt, float wheelDriveTorque, int idx, SBSVector3 suspensionForce, ref SBSVector3 totalForce, ref SBSVector3 totalTorque)
#endif
    {
        //Debug.Log("wheelDriveTorque: " + wheelDriveTorque);
        // tyre fricion force
        bool frictionLimiting = false;
        SBSVector3 groundVel = GetWheelVelocity(idx);
        SBSQuaternion wheelOrientation = GetWheelSteerSuspOrientation(idx);

        groundVel = (cachedRBOri * wheelOrientation).inverse * groundVel;

        SBSVector3 tyreForce = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        SBSVector3 tyreTorque = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;

        int excecutions = 1;
        float wheelSpeed = 0;
        //Vector3 frictionForce = new Vector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        SBSVector3 frictionForce = new SBSVector3(0.0f, 0.0f, 0.0f);
        float executionsDt = dt / (float)excecutions;
        for (int i = 0; i < excecutions; i++)
        {
            Wheel wheel = wheels[idx];
            Brake brake = brakes[idx];
            Tyre tyre = tyres[idx];
            wheel.Integrate1(executionsDt);

            wheelSpeed = wheel.AngularVelocity * tyre.Radius;
            frictionForce = CalcTyreFrictionForce(idx, executionsDt, suspensionForce, frictionLimiting, wheelSpeed, groundVel, wheelOrientation);
            //DebugUtils.AddWatch(new Rect(0, idx * 40, 200, 40), "wheelSpeed[{0}]: {1}", idx, lastSuspensionForce[0].ToString());
            //DebugUtils.AddWatch(new Rect(0, 0, 200, 40), "Susp: " + lastSuspensionForce[0].ToString()); 
            //frictionForce.z = 0.0f;
            // reaction torque
            //tyreForce = new Vector3(frictionForce.y, -frictionForce.x, 0.0f);
            //tyreForce = new Vector3(frictionForce.x, 0.0f, frictionForce.z);
            tyreForce.x = frictionForce.x;
            tyreForce.z = frictionForce.z;

            tyreForces[idx] = frictionForce;

            float reactionTorque = tyreForce.z * tyre.Radius;

            // torques
            wheel.DriveTorque = wheelDriveTorque;
            float wheelBrakeTorque = -brake.GetTorque(wheel.AngularVelocity, idx);
            wheel.BrakeTorque = wheelBrakeTorque;

            // limit the torque to drive and braking
            float torque = wheelDriveTorque + wheelBrakeTorque;
            if ((torque > 0.0f && reactionTorque > torque) || (torque < 0.0f && reactionTorque < torque))
                reactionTorque = torque;

            //tyreTorque = new Vector3(0.0f, -reactionTorque, -frictionForce.z);
            //tyreTorque = new Vector3(0.0f, 0.0f, 0.0f);
            //tyreTorque = new Vector3(reactionTorque, -frictionForce.z, 0.0f);//-frictionForce.z);
//            tyreTorque = new Vector3(reactionTorque, frictionForce.y, 0.0f);
            //tyreTorque = new Vector3(reactionTorque, frictionForce.y, 0.0f);
            tyreTorque.x = reactionTorque;
            tyreTorque.y = frictionForce.y;
            tyre.FeedbackEffect = frictionForce.y;


            float tyreRollResistanceTorque = 0.0f;
            if (!brake.Locked)
            {
                float tyreFrictionTorque = tyreForce.z * tyre.Radius;

                if (frictionLimiting)
                {
                    float limit = -torque + ((wheelSpeed - groundVel.x) * executionsDt * wheel.Inertia) / (executionsDt * executionsDt * tyre.Radius);
                    if ((tyreFrictionTorque < 0.0f && limit > 0.0f) || (tyreFrictionTorque > 0.0f && limit < 0.0f))
                        limit = 0.0f;

                    if (tyreFrictionTorque > 0.0f && tyreFrictionTorque > limit)
                        tyreFrictionTorque = limit;
                    else if (tyreFrictionTorque < 0.0f && tyreFrictionTorque < limit)
                        tyreFrictionTorque = limit;
                }

                //Debug.Log(" wheel.AngularVelocity: " + wheel.AngularVelocity + "wheelContacts[idx].RollingResistance: " + wheelContacts[idx].RollingResistance + " tyre.Radius: " + tyre.Radius + " tyreFrictionTorque: " + tyreFrictionTorque);
                tyreRollResistanceTorque = -tyre.GetRollResistance(wheel.AngularVelocity, this.wheelOnGround(idx) ? wheelContacts[idx].RollingResistance : 1.0f) * tyre.Radius - tyreFrictionTorque;
                //tyreRollResistanceTorque *= 0.8f;
                //tyreRollResistanceTorque *= 0.5f;
                // PIETRO
                //tyreRollResistanceTorque *= 0.250f;
                tyreRollResistanceTorque *= 0.250f;
                //tyreRollResistanceTorque = 0.0f;
            }
            wheel.RollResistanceTorque = tyreRollResistanceTorque;

            //tyreForce = tyreForce + groundVel * wheelContacts[idx].RollingDrag;
            tyreForce.x -= groundVel.x * wheelContacts[idx].RollingDrag;
            tyreForce.z -= groundVel.z * wheelContacts[idx].RollingDrag;

            if (brake.Locked)
            {
                wheel.AngularVelocity = 0.0f;
                wheel.Reset(); // TODO
            }
            else
                wheel.Update(executionsDt, idx);

            wheel.Integrate2(executionsDt);
        }

        //apply forces to body
        //Vector3 worldTyreForce = wheelOrientation * tyreForce;
        SBSVector3 worldTyreForce = cachedRBOri * (wheelOrientation * tyreForce);

//      tmpVars[idx] = worldTyreForce * 0.1f;

        //tyreTorque = new Vector3(1.0f, 0.0f, 0.0f) * (wheelSpeed - 30.0f) * 1000.0f;
        //Vector3 worldTyreTorque = wheelOrientation * tyreTorque;
        SBSVector3 worldTyreTorque = cachedRBOri * (wheelOrientation * tyreTorque);

        SBSVector3 wheelPosition = prevWheelPositions[idx];//GetWheelPosition(idx);// -CenterOfMassPos; //PIE_271111
//      SBSVector3 cmForce = Vector3.zero;
//      SBSVector3 cmTorque = Vector3.zero;
//        Debug.Log(worldTyreForce);
#if UNITY_FLASH
        GetForceAndTorqueAtOffset(SBSVector3.zero, worldTyreTorque, wheelPosition, totalForce, totalTorque);
        GetForceAtOffset(worldTyreForce, wheelPosition, totalForce, totalTorque);
#else
        GetForceAndTorqueAtOffset(SBSVector3.zero, worldTyreTorque, wheelPosition, ref totalForce, ref totalTorque);
        GetForceAtOffset(worldTyreForce, wheelPosition, ref totalForce, ref totalTorque);
#endif

        //DebugUtils.AddWatch(new Rect(0, (idx) * 20, 600, 20), String.Format("wheelSpeed: {0} cmForce: {1} cmTorque: {2} tyreForce: {3}", wheelSpeed, cmForce, cmTorque, tyreForce));
//        DebugUtils.AddWatch(new Rect(0, (idx) * 20, 600, 20), String.Format("frictionForce: {0} wheelSpeed: {1} cmForce: {2} cmTorque: {3} tyreForce: {4}", frictionForce, wheelSpeed, cmForce, cmTorque, tyreForce));

        //DebugUtils.AddWatch(new Rect(0, (idx) * 20, 600, 20), String.Format("wheelSide{0} frictionForce: {1}", wheelSide, frictionForce));
      //DebugUtils.AddWatch(new Rect(0, (15 + idx) * 20, 600, 20), String.Format("tyreForce: {0} cmForce: {1} cmTorque: {2}", tyreForce, cmForce, cmTorque)); 
        //DebugUtils.AddWatch(new Rect(0, (15 + idx) * 20, 600, 20), String.Format("tyreTorque: {0} cmForce: {1} cmTorque: {2}", tyreTorque, cmForce, cmTorque)); 

//      totalForce = totalForce + cmForce;
//      totalTorque = totalTorque + cmTorque;
    }

    public void resetWheels()
    {
        if (null == wheels)
            return;

        for ( int i = 0; i < wheels.Length; i++ )
        {
            Wheel wheel = wheels[i];

            wheel.AngularVelocity = 0.0f;
            wheel.Reset();
        }
    }

    public void StartRearing(float time)
    {
        if (rearing)
            return;

        if( !(lastRearingTime == -1.0f || (time - lastRearingTime > 2.0f)))
            return;

        lastRearingTime = time;
        rearing = true;
    }

    public void StopRearing()
    {
        rearing = false;
    }

    public void WheelAntispin(int idx, float normalForce, float dt)
    {
        float slipRatio = tyres[idx].GetSigmaMax(normalForce),
              verse = gearbox.CurrentGear >= 0 ? 1.0f : -1.0f,
              throttle = engine.Throttle,
              throttleThreshold = 0.1f;

        if (throttle > throttleThreshold)
        {
            /*
            float maxSpinDelta = 0.0f,
                  angVel = wheels[idx].AngularVelocity;
            for (int i = 0; i < wheels.Length; ++i)
            {
                float spinDelta = angVel - wheels[i].AngularVelocity;
                if (spinDelta < 0.0f)
                    spinDelta = -spinDelta;

                if (spinDelta > maxSpinDelta)
                    maxSpinDelta = spinDelta;
            }

            if (maxSpinDelta > 1.0f)
            {
                float error = tyres[idx].Slide * verse - slipRatio,
                      engage = 0.0f,
                      disengage = -slipRatio * 0.5f;

                if (error > engage && !asActive[idx])
                    asActive[idx] = true;

                if (error < disengage && asActive[idx])
                    asActive[idx] = false;

                if (asActive[idx])
                {
                    float oldThrottle = throttle;
                    throttle = throttle - error * 10.0f * Mathf.Clamp01(friction.Position);
                    engine.Throttle = Mathf.Clamp01(throttle);

                    //Debug.Log ("decreasing throttle: " + oldThrottle + " -> " + throttle);
                }
            }
            else
                asActive[idx] = false;
            */

            float error = tyres[idx].Slide * verse - slipRatio,
                    /*engage = slipRatio * 10.0f,
                    disengage = -slipRatio * 0.25f;*/
                    /*
                    engage = slipRatio * 1000.0f,
                    disengage = -slipRatio * 50.0f;
                    */
                    engage = slipRatio * ASR_engageCoeff,
                    disengage = -slipRatio * ASR_disengageCoeff;

            //if (idx == 0)
            //    Debug.Log("error: " + error + " ||| " + slipRatio + " ||| " + tyres[idx].Slide * verse);

            if (error > engage && !asActive[idx])
                asActive[idx] = true;

            if (error < disengage && asActive[idx])
                asActive[idx] = false;

            if (asActive[idx] && true)
            {
                //float oldThrottle = throttle;
                throttle = throttle - error * 10.0f * Mathf.Clamp01(friction.Position);
                engine.Throttle = Mathf.Clamp01(throttle);

                //Debug.Log ("decreasing throttle: " + oldThrottle + " -> " + throttle);
            }
        }
        else
            asActive[idx] = false;
    }

    public void UpdateAntispin(float dt)
    {
        if (!ASR_enabled)
            return;

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Y))
            return;
#endif

        for (int i = wheels.Length - 1; i >= 0; --i)
            WheelAntispin(i, lastSuspensionForce[i].magnitude, dt);
    }

    public void ApplyForces(float dt)
    {
        UpdateAntispin(dt);

        // driveshaft Speed
        float driveshaftSpeed = CalcDriveshaftSpeed();

        // friction speed
        float frictionSpeed = gearbox.GetClutchSpeed(driveshaftSpeed);

        // 
        float engineAxleSpeed = engine.AngularVelocity;

        float engineDrag = friction.GetTorque(engineAxleSpeed, frictionSpeed);

        engine.ExtraTorque = engineExtraPower + engineNitroPower;

        //DebugUtils.AddWatch(new Rect(0, 0 * 20, 600, 20), String.Format("engineDrag: {0} engineAxleSpeed: {1} frictionSpeed: {2}", engineDrag, engineAxleSpeed, frictionSpeed));
        //DebugUtils.AddWatch(new Rect(0, 0 * 20, 600, 20), String.Format("Engine.RPM: {0} CurrentGear: {1}", engine.RPM, gearbox.CurrentGear));
        //DebugUtils.AddWatch(new Rect(0, 1 * 20, 600, 20), String.Format("Speed: {0}", GetSpeed() * VehicleUtils.ToKmh));
		
        ApplyFrictionTorque(engineDrag, frictionSpeed);
        engine.Update ( dt );

        //Debug.Log("Engine.RPM: " + engine.RPM + " gearbox.CurrentGear: " + gearbox.CurrentGear);

        // get the drive torque for each wheel
        float[] wheelDriveTorques = new float[4];
        float engineTorque = engineDrag;
        if ( friction.Engaged)
            engineTorque = engine.Torque;

        UpdateDriveTorque(wheelDriveTorques, engineTorque);

        //for ( int i = 0; i < wheelDriveTorques.Length; i++)
        //    DebugUtils.AddWatch(new Rect(0, (i + 4) * 20, 200, 20), "Displacement[{0}]: {1}", i, suspensions[i].Displacement);

        SBSVector3 totalForce = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        SBSVector3 totalTorque = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;

//      Vector3 totalForceFake = Vector3.zero;
//      Vector3 totalTorqueFake = Vector3.zero;

		Boolean vehOnGround = wheelsOnGround();
		Boolean resetWheels = false;

        if ( vehOnGround )
#if UNITY_FLASH
            ApplyEngineTorque(totalForce, totalTorque);
#else
            ApplyEngineTorque(ref totalForce, ref totalTorque);
#endif

#if UNITY_FLASH
        ApplyWingsForce(totalForce, totalTorque);
#else
        ApplyWingsForce(ref totalForce, ref totalTorque);
#endif

        for ( int i = wheels.Length - 1; i >= 0; --i )
        {
            ComputeSuspensionDisplacement( i, dt );
        }
				
        // suspension forces
        for ( int i = wheels.Length - 1; i >= 0; --i )
        {
#if UNITY_FLASH
            lastSuspensionForce[i] = ApplySuspensionForce(i, dt, totalForce, totalTorque);
#else
            lastSuspensionForce[i] = ApplySuspensionForce(i, dt, ref totalForce, ref totalTorque);
#endif
        }

		if (isOnGroundCounter > 0 || vehOnGround)
		{
			if (SBSMath.Abs(rigidBody.angularDrag - prevAngularDrag) > 1.0e-6f)
				rigidBody.angularDrag = prevAngularDrag;

			if (SBSMath.Abs(rigidBody.drag - prevLinearDrag) > 1.0e-6f)
                rigidBody.drag = prevLinearDrag;
        }
		else
		{
			if (SBSMath.Abs(rigidBody.angularDrag - onAirAngularDrag) > 1.0e-6f)
				rigidBody.angularDrag = onAirAngularDrag;
			if (SBSMath.Abs(rigidBody.drag - onAirLinearDrag) > 1.0e-6f)
				rigidBody.drag = onAirLinearDrag;
        }

		if (vehOnGround)
		{
			float input = throttleFromInput;
			if (freezed)
			{
				/*
				if (!rigidbody.isKinematic)
				{
					rigidbody.velocity = rigidbody.velocity * 0.8f;
					rigidbody.angularVelocity = rigidbody.angularVelocity * 0.8f;
				}
				*/
				input = 0.0f;
			}

			if (input == 0.0f || gearbox.CurrentGear == 0)
			{
				float vel = SBSVector3.Dot(rigidBody.velocity, ForwardVector);
				if (SBSMath.Abs(vel) < velocityThreshold)
				{
					if (!rigidBody.isKinematic)
					{
						rigidBody.velocity = rigidBody.velocity * 0.8f;
						rigidBody.angularVelocity = rigidBody.angularVelocity * 0.8f;
					}
					resetWheels = true;
				}
			}
		}
		else
		{
			//totalForce.z -= 200000.0f * dt;
			//     totalTorque = totalTorque - Orientation * new Vector3(500000.0f * dt, 0.0f, 0.0f);
		}

		if (!resetWheels)
			for (int i = wheels.Length - 1; i >= 0; --i)
#if UNITY_FLASH
				ApplyWheelForces(dt, wheelDriveTorques[i], i, lastSuspensionForce[i], totalForce, totalTorque);
#else
                ApplyWheelForces(dt, wheelDriveTorques[i], i, lastSuspensionForce[i], ref totalForce, ref totalTorque);
#endif
		else
			this.resetWheels();
		
        if (rearing)
        {
            SBSVector3 contactsUp = new SBSVector3();
            for (int i = 0; i < wheelContacts.Length; i++)
                contactsUp = contactsUp + wheelContacts[i].Normal;
            contactsUp.ScaleBy(1.0f / wheelContacts.Length);

            SBSVector3 vehUp = cachedRBOri * SBSVector3.up;
            float vehUpAngle = SBSVector3.Angle(contactsUp, vehUp);

            //Debug.Log("vehUpAngle: " + vehUpAngle);

            float rearingStrength = 8000.0f;
            float rearingMinAngle = 8.0f;
            float rearingMaxAngle = 10.0f;
            SBSVector3 rearingApplyPos = new SBSVector3(0.0f, 0.0f, 2.0f);

            vehUpAngle = Mathf.Clamp(vehUpAngle, 0.0f, rearingMaxAngle);

            SBSVector3 rearingForce = VehicleToWorld(new SBSVector3(0, vehUpAngle < rearingMinAngle ? rearingStrength : rearingStrength * ((rearingMaxAngle - vehUpAngle) / (rearingMaxAngle - rearingMinAngle)), 0.0f));
            GetForceAtOffset(rearingForce, cachedRBMat * rearingApplyPos, ref totalForce, ref totalTorque);

            if (vehUpAngle >= rearingMinAngle)
                rearing = false;
        }

        rigidBody.AddForce(totalForce, ForceMode.Force);
		rigidBody.AddTorque(totalTorque, ForceMode.Force);
        //Debug.Log("totalForce: " + totalForce);
        //Debug.Log("totalTorque: " + totalTorque);
		//Debug.Log("lastSuspensionForce[0]: " + lastSuspensionForce[0]);
    }

	public Boolean wheelsOnGround()
	{
		int len = suspensions.Length;

        for (int i = 0; i < len; ++i)
            if (suspensions[i].Displacement > 0.0f)
                return true;

        return false;
    }

    public Boolean wheelOnGround(int index)
    {
        return (suspensions[index].Displacement > 0.0f);
    }

    private void InitWheelVelocities()
    {
        wheelVelocities = new SBSVector3[4];
        wheelVelocities[0] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        wheelVelocities[1] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        wheelVelocities[2] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        wheelVelocities[3] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
    }

    public void SetInitialConditions(Vector3 initialPos, Quaternion initialOrientation)
    {
        GetComponent<Rigidbody>().position = initialPos;
        GetComponent<Rigidbody>().rotation = initialOrientation;

        cachedRBOri = rigidBody.rotation;
		cachedRBOri.Normalize();
        cachedRBPos = rigidBody.position;
        cachedRBMat = SBSMatrix4x4.TRS(cachedRBPos, cachedRBOri, SBSVector3.one);
        cachedRBCom = cachedRBMat * GetComponent<Rigidbody>().centerOfMass;

        engine.SetInitialConditions();

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].SetInitialConditions();
        }

        InitWheelVelocities();

        wheelContacts = new ContactData[4];
		wheelRaycastHits = new RaycastHit[4];

		lastSuspensionForce = new SBSVector3[4];
        lastSuspensionForce[0] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionForce[1] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionForce[2] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionForce[3] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
#if UNITY_EDITOR
        lastSuspensionApply = new SBSVector3[4];
        lastSuspensionApply[0] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionApply[1] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionApply[2] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
        lastSuspensionApply[3] = new SBSVector3(0.0f, 0.0f, 0.0f);//Vector3.zero;
#endif
        prevWheelPositions = new SBSVector3[wheels.Length];
        for (int i = 0; i < prevWheelPositions.Length; i++)
            prevWheelPositions[i] = GetWheelPosition(i);

        asActive = new bool[wheels.Length];
        for (int i = 0; i < asActive.Length; i++)
            asActive[i] = false;
    }

    public void UpdatePhysics(float dt)
	{
        UpdateCarWheelCollisions();

		remainingShiftTime -= dt;
        if (remainingShiftTime < 0.0f)
            remainingShiftTime = 0.0f;
		UpdateVehicle(dt);
	}

    public void UpdateVehicle(float dt)
    {
		SBSVector3[] newWheelPositions = new SBSVector3[4];
		float oodt = 1.0f / dt;
        for (int i = prevWheelPositions.Length - 1; i >= 0; --i) {
			newWheelPositions[i] = this.GetWheelPosition(i);

			wheelVelocities[i] = (newWheelPositions[i] - prevWheelPositions[i]);

			wheelVelocities[i].x *= oodt;
			wheelVelocities[i].y *= oodt;
			wheelVelocities[i].z *= oodt;
		}
		prevWheelPositions = newWheelPositions;

        engine.Integrate1(dt);
		
		if (enablePhysics)
			ApplyForces(dt);

        engine.Integrate2(dt);
		/*
        for (int i = prevWheelPositions.Length - 1; i >= 0; --i)
            prevWheelPositions[i] = GetWheelPosition(i);*/
    }

    public void SetWheelContactProperties(int idx, float wheelHeight, SBSVector3 position, SBSVector3 normal, float bumpWaveLength, float bumpAmplitude, float frictionNoTread, float frictionTread, float rollingResistance, float rollingDrag, ContactData.SurfaceType surfaceType)
	{
//      wheelContacts[idx] = new ContactData(wheelHeight, position, normal, bumpWaveLength, bumpAmplitude, frictionNoTread, frictionTread, rollingResistance, rollingDrag, surfaceType);
        if (null == wheelContacts[idx])
            wheelContacts[idx] = new ContactData(wheelHeight, position, normal, bumpWaveLength, bumpAmplitude, frictionNoTread, frictionTread, rollingResistance, rollingDrag, surfaceType);
        else
            wheelContacts[idx].Set(wheelHeight, position, normal, bumpWaveLength, bumpAmplitude, frictionNoTread, frictionTread, rollingResistance, rollingDrag, surfaceType);
    }

    public ContactData GetWheelContactProperties(int idx)
    {
        return wheelContacts[idx];
    }

	public RaycastHit GetWheelRaycastHit(int idx)
	{
		return wheelRaycastHits[idx];
	}

#if UNITY_FLASH
    public void GetForceAtOffset(SBSVector3 force, SBSVector3 offset, SBSVector3 outForce, SBSVector3 outTorque)
#else
    public void GetForceAtOffset(SBSVector3 force, SBSVector3 offset, ref SBSVector3 outForce, ref SBSVector3 outTorque)
#endif
    {
//      outForce.IncrementBy(force);// = outForce + force;
		outForce.x += force.x;
		outForce.y += force.y;
		outForce.z += force.z;

        SBSVector3 torque = SBSVector3.Cross(force, cachedRBCom - offset);
//      outTorque.IncrementBy(torque);// = outTorque + torque;
		outTorque.x += torque.x;
		outTorque.y += torque.y;
		outTorque.z += torque.z;
    }

#if UNITY_FLASH
    void GetForceAndTorqueAtOffset(SBSVector3 force, SBSVector3 torque, SBSVector3 point, SBSVector3 outForce, SBSVector3 outTorque)
#else
    void GetForceAndTorqueAtOffset(SBSVector3 force, SBSVector3 torque, SBSVector3 point, ref SBSVector3 outForce, ref SBSVector3 outTorque)
#endif
    {
#if UNITY_FLASH
        GetForceAtOffset(force, point, outForce, outTorque);
#else
        GetForceAtOffset(force, point, ref outForce, ref outTorque);
#endif

        float tao = torque.magnitude;
        if (tao < SBSMath.Epsilon)
        {
            return;
        }
        /*
        Vector3 n = -torque / tao; //PIETRO_241111
        //Vector3 r = transform.TransformPoint(rigidBody.centerOfMass) - point;
        Vector3 r = cachedRBPos - point;
        r = r - Vector3.Dot(r, n) * n;*/

#if UNITY_FLASH
        SBSVector3 n = torque.Clone();
#else
        SBSVector3 n = torque;
#endif
        n.Negate();
        n.ScaleBy(1.0f / tao);
        SBSVector3 r = cachedRBCom - point;
        r = r - SBSVector3.Dot(r, n) * n;

        float rr = r.sqrMagnitude;
        if (rr < SBSMath.Epsilon)
        {
            //rigidBody.AddTorque(torque, ForceMode.Force); // ToDo: outTorque = outTorque + torque;
            outTorque.IncrementBy(torque);// = outTorque + torque;
            return;
        }

        float Ia = SBSVector3.Dot(inertiaTensor * n, n),
              Ip = Ia + rr * rigidBody.mass,
              Ipa = Ia / Ip,
              localTao = tao * Ipa;
        /*
        Vector3 f = (Vector3.Cross(n, r) * tao * (1.0f - Ipa)) / rr,
                t = n * localTao;
        */
        SBSVector3 f = SBSVector3.Cross(n, r);
        f.ScaleBy(tao * (1.0f - Ipa) / rr);
//      n.ScaleBy(localTao);

//      outForce.IncrementBy(f);// = outForce + f;
//      outTorque.IncrementBy(n);//t);// = outTorque + t;
        //rigidbody.AddForce(f, ForceMode.Force);
        //rigidbody.AddTorque(t, ForceMode.Force);
		outForce.x += f.x;
		outForce.y += f.y;
		outForce.z += f.z;
		outTorque.x += (n.x * localTao);
		outTorque.y += (n.y * localTao);
		outTorque.z += (n.z * localTao);
    }

    public float GetTyreSlide ( int idx )
    {
//      Vector3 groundVel = GetWheelVelocity(idx);
        SBSQuaternion wheelOrientation = GetWheelSteerSuspOrientation(idx);

        SBSVector3 groundVel = (cachedRBOri * wheelOrientation).inverse * GetWheelVelocity(idx);

        //tmpVars[idx] = groundVel;

        float wheelSpeed = wheels[idx].AngularVelocity * tyres[idx].Radius;
        groundVel.x *= 2.0f;
        //groundVel.x *= 2.0f;
        groundVel.y = 0.0f;
        groundVel.z -= wheelSpeed;

        //tmpVars[idx] = groundVel;

        float slide = ( groundVel.magnitude - 3.0f ) * 0.2f;



        Tyre tyre = tyres[idx];
        //float backWheelCoeff = idx > 1 ? 2.0f : 0.75f;
        float maxRatio = SBSMath.Max(SBSMath.Abs(tyre.SlideRatio), SBSMath.Abs(tyre.SlipRatio)) * tyre.SlideBias;

        float squealCoeff = SBSMath.Max ( 0.0f, maxRatio - 1.0f );


        slide = Mathf.Clamp01(slide * squealCoeff);

        //tmpVars[idx] = slide * MathUtils.Vector3Up * 30.0f;
        //tmpVars[idx] = tyre.SlideRatio * MathUtils.Vector3Up * 30.0f;
        //slide = tyre.SlipRatio;

        return slide;
    }

    void Update()
    {
        // prepare frame
    }

    void LateUpdate()
    {
        // exit frame
    }

    public SBSVector3 GetWheelPositionAtDisplacementTMP(int idx, float displacementPerc)
    {
        Wheel wheel = wheels[idx];
        Suspension suspension = suspensions[idx];
        SBSVector3 relWheelExtended = wheel.RelaxPosition - suspension.Hinge;

        float travel = suspension.Travel;
        float displacement = displacementPerc * travel;

        //Vector3 up = MathUtils.Vector3Forward;//Vector3.up;
        //Vector3 up = MathUtils.Vector3Up;//Vector3.up;
        //GetWheelPositionAtDisplacement
        /*
        Vector3 up = rigidBody.transform.TransformDirection(new Vector3(0.0f, 1.0f, 0.0f));
        //Debug.Log("up: " + up);
        Vector3 rotationalAxis = Vector3.Cross(up, relWheelExtended.normalized);*/
        SBSVector3 up = cachedRBOri * SBSVector3.up;
        SBSVector3 rotationalAxis = SBSVector3.Cross(up, relWheelExtended);

        float hingeRad = relWheelExtended.magnitude;
        float displacementAngle = displacement / hingeRad;

        SBSQuaternion HingeRotate = SBSQuaternion.AngleAxis(-displacementAngle * SBSMath.ToDegrees, rotationalAxis);
        SBSVector3 localWheelPos = HingeRotate * relWheelExtended;
        localWheelPos.IncrementBy(suspension.Hinge);
        //DebugUtils.AddWatch(new Rect(0, (12 + idx) * 20, 600, 20), String.Format("dispPerc[{0}]: {1} : {2}: {3} : {4} : {5} : {6}", idx, displacementPerc, localWheelPos, suspension.Travel, displacement, displacementAngle, up));
        //DebugUtils.AddWatch(new Rect(0, (12 + idx) * 20, 600, 20), String.Format("displacement: {0} wheel.RelaxPosition: {1} localWheelPos: {2}", displacementPerc, wheel.RelaxPosition, localWheelPos)); 
        return cachedRBMat * localWheelPos;//VehicleToWorld(localWheelPos);// + suspension.Hinge);
    }

    public SBSVector3 GetWheelPositionTMP(int idx)
    {
        return GetWheelPositionAtDisplacementTMP(idx, suspensions[(int)idx].DisplacementPerc);
    }

    public SBSVector3 GetWheelPositionAtDisplacement(int idx, float displacementPerc)
    {
        Wheel wheel = wheels[idx];
        Suspension suspension = suspensions[idx];
        SBSVector3 relWheelExtended = wheel.RelaxPosition - suspension.Hinge;

        float travel = suspension.Travel;
        float displacement = displacementPerc * travel;

        //Vector3 up = MathUtils.Vector3Forward;//Vector3.up;
        //Vector3 up = MathUtils.Vector3Up;//Vector3.up;
        //GetWheelPositionAtDisplacement
        /*
        Vector3 up = rigidBody.transform.TransformDirection(new Vector3(0.0f, 1.0f, 0.0f));
        //Debug.Log("up: " + up);
        Vector3 rotationalAxis = Vector3.Cross(up, relWheelExtended.normalized);*/
        SBSVector3 up = cachedRBOri * SBSVector3.up;
        SBSVector3 rotationalAxis = SBSVector3.Cross(up, relWheelExtended);

        float hingeRad = relWheelExtended.magnitude;
        float displacementAngle = displacement / hingeRad;

        SBSQuaternion HingeRotate = SBSQuaternion.AngleAxis(-displacementAngle * SBSMath.ToDegrees, rotationalAxis);
        SBSVector3 localWheelPos = HingeRotate * relWheelExtended;
        localWheelPos.IncrementBy(suspension.Hinge);
//		return localWheelPos + hinge;//Vector3.zero;
//      DebugUtils.AddWatch(new Rect(0, (12 + idx) * 20, 600, 20), String.Format("dispPerc[{0}]: {1} : {2}: {3} : {4} : {5} : {6}", idx, displacementPerc, localWheelPos, suspension.Travel, displacement, displacementAngle, up));
        return cachedRBMat * localWheelPos;//VehicleToWorld(localWheelPos);// + suspension.Hinge);
    }

    public SBSVector3 GetWheelPosition(int idx)
    {
        return GetWheelPositionAtDisplacement(idx, suspensions[(int)idx].DisplacementPerc);
    }

    public SBSVector3 GetWheelVelocity(int idx)
    {
        return rigidBody.GetPointVelocity(this.GetWheelPositionPrecomputed(idx));// wheelVelocities[idx];
    }

    public SBSQuaternion GetWheelOrientation(int idx)
    {
        return GetWheelOrientation(idx, SBSQuaternion.identity);//Quaternion.identity);
    }

    public SBSQuaternion GetWheelOrientation(int idx, SBSQuaternion parentOrientation)
    {
        SBSQuaternion wheelRotation = wheels[idx].Orientation;//.inverse;
        SBSQuaternion sideRotation = new SBSQuaternion(0.0f, 0.0f, 0.0f, 1.0f);
        if (idx == (int)VehicleUtils.WheelPos.FR_RIGHT || idx == (int)VehicleUtils.WheelPos.RR_RIGHT)
        {
            sideRotation.SetAngleAxis(180.0f, SBSVector3.up);
        }

        return cachedRBOri * GetWheelSteerSuspOrientation(idx) * parentOrientation * wheelRotation * sideRotation;
    }

    public SBSQuaternion GetWheelSteerSuspOrientation(int idx)
    {
        float steerRad = -wheels[idx].SteerAngle;// *MathUtils.ToRadians;
        SBSQuaternion steer = SBSQuaternion.AngleAxis(steerRad, SBSVector3.up);

        Suspension suspension = suspensions[idx];

        float camberRad = -suspension.Camber;// *MathUtils.ToRadians;
        if (idx == (int)VehicleUtils.WheelPos.FR_RIGHT || idx == (int)VehicleUtils.WheelPos.RR_RIGHT)
            camberRad = -camberRad;
        //Quaternion camber = Quaternion.AngleAxis(camberRad, new Vector3(1.0f, 0.0f, 0.0f));
        SBSQuaternion camber = SBSQuaternion.AngleAxis(camberRad, SBSVector3.back);

        float toeRad = suspension.Toe; // *MathUtils.ToRadians;
        if (idx == (int)VehicleUtils.WheelPos.FR_LEFT || idx == (int)VehicleUtils.WheelPos.RR_LEFT)
            toeRad = -toeRad;
        SBSQuaternion toe = SBSQuaternion.AngleAxis(toeRad, SBSVector3.up);
        //Debug.Log("steerRad: " + steerRad + " camber: " + camber + " toe: " + toe + " steer: " + steer);

        //DebugUtils.AddWatch(new Rect(0, (15 + idx) * 20, 600, 20), String.Format("steerRad: {0} steer: {1}", steerRad, steer)); 
        return camber * toe * steer;
    }

    public SBSVector3 VehicleToWorld(SBSVector3 vehicleLocal)
    {
//      Matrix4x4 m = MathUtils.MatrixFromQuaternion(cachedRBOri, cachedRBPos);
        return cachedRBMat * vehicleLocal;//.MultiplyPoint3x4(vehicleLocal);
    }
	
	public SBSVector3 WorldToVehicle(SBSVector3 vehicleWorld)
	{
		return cachedRBMat.inverseFast * vehicleWorld;
	}
	
	/*
    public SBSVector3 VehicleDirectionToWorld(SBSVector3 vehicleLocal)
    {
        Matrix4x4 m = MathUtils.MatrixFromQuaternion(cachedRBOri, cachedRBPos);
        return m.MultiplyVector(vehicleLocal);
    }

    public SBSVector3 WorldToVehicle(SBSVector3 worldPos)
    {
        Matrix4x4 m = MathUtils.MatrixFromQuaternion(cachedRBOri, cachedRBPos).inverse;
        return m.MultiplyPoint3x4(worldPos);
    }
    */
    /*
    public float GetMaxSteeringAngle()
    {
        //return Mathf.Max((maxSteeringAngle - Speed * VehicleUtils.ToKmh * steeringSpeedRatio), minSteeringAngle);
        //if (VehicleAI.enabled)
        //    return maxSteeringAngle;
        //if (VehicleAI.enabled)
        //    return Mathf.Max((maxSteeringAngle - ((Speed * VehicleUtils.ToKmh) >= steeringMinSpeed ? (Speed * VehicleUtils.ToKmh - steeringMinSpeed) : 0.0f) * steeringSpeedRatio), minSteeringAngle * 1.5f);
        return Mathf.Max((maxSteeringAngle - ((Speed * VehicleUtils.ToKmh) >= steeringMinSpeed ? (Speed * VehicleUtils.ToKmh - steeringMinSpeed) : 0.0f) * steeringSpeedRatio), minSteeringAngle * (isAI ? 1.5f : 1.0f));
        //return maxSteeringAngle;
    }
    */
    public float MaxSteeringAngle
    {
        get
        {
            return Mathf.Max((maxSteeringAngle - ((Speed * VehicleUtils.ToKmh) >= steeringMinSpeed ? (Speed * VehicleUtils.ToKmh - steeringMinSpeed) : 0.0f) * steeringSpeedRatio), minSteeringAngle * (isAI ? 1.5f : 1.0f));
        }
    }

    /*
    public float GetMaxSteeringAngle()
    {
        //return Mathf.Max((maxSteeringAngle - Speed * VehicleUtils.ToKmh * steeringSpeedRatio), minSteeringAngle);
        return Mathf.Max((maxSteeringAngle - ((Speed * VehicleUtils.ToKmh) >= steeringMinSpeed ? (Speed * VehicleUtils.ToKmh - steeringMinSpeed) : 0.0f) * steeringSpeedRatio), minSteeringAngle);
        //return maxSteeringAngle;
    }
    */
    /*
    public float GetBestSteeringAngle()
    {
        return tyres[(int)VehicleUtils.WheelPos.FR_LEFT].GetBestSteeringAngle(gravity * rigidBody.mass * 0.25f);
    }
    */
    public float BestSteeringAngle
    {
        get
        {
            return tyres[(int)VehicleUtils.WheelPos.FR_LEFT].GetBestSteeringAngle(gravity * rigidBody.mass * 0.25f);
        }
    }

    public float GetTyreMaximumLongitudinalForce(int idx)
    {
        return tyres[idx].GetMaximumLongitudinalForce(gravity * rigidBody.mass * 0.25f);
    }

    public float GetTyreMaximumLateralForce(int idx)
    {
        return tyres[idx].GetMaximumLateralForce(gravity * rigidBody.mass * 0.25f, 0.0f);
    }

    public void GetFrictionCoeffs(out float longCoeff, out float latCoeff)
    {
        float frictionLong = 0.0f, frictionLat = 0.0f;

        for(int i = tyres.Length - 1; i >= 0; --i)
        {
            frictionLong += GetTyreMaximumLongitudinalForce(i);
            frictionLat += GetTyreMaximumLateralForce(i);
        }

        float normalForce =  gravity * rigidBody.mass;
        longCoeff = longitudinalFrictionCoeff * frictionLong / normalForce;
        latCoeff = lateralFrictionCoeff * frictionLat / normalForce;
    }

    public float LongitudinalFrictionCoeff
    {
        get
        {
            float frictionLong = 0.0f;

            for (int i = tyres.Length - 1; i >= 0; --i)
                frictionLong += GetTyreMaximumLongitudinalForce(i);

            float normalForce = gravity * rigidBody.mass;

            return longitudinalFrictionCoeff * frictionLong / normalForce;
        }
    }

    public float LateralFrictionCoeff
    {
        get
        {
            float frictionLat = 0.0f;

            for (int i = tyres.Length - 1; i >= 0; --i)
                frictionLat += GetTyreMaximumLateralForce(i);

            float normalForce = gravity * rigidBody.mass;

            return lateralFrictionCoeff * frictionLat / normalForce;
        }
    }

    public float AerodynamicDownforceCoeff
    {
        get
        {
            float result = 0.0f;
            for (int i = wings.Length - 1; i >= 0; --i)
                result += wings[i].GetAerodynamicDownforceCoeff();
            return result;
        }
    }

    public float AerodynamicDragCoeff
    {
        get
        {
            float result = 0.0f;
            for (int i = wings.Length - 1; i >= 0; --i)
                result += wings[i].GetAerodynamicDragCoeff();
            return result;
        }
    }

    public void SetSteering(float value)
    {
        //float steerAngle = value * maxSteeringAngle; //outside wheel steering angle in radians
        float maxAngle = this.MaxSteeringAngle;// GetMaxSteeringAngle();
        /*
        if (!vehicleAI.enabled)
        {
            //value = Mathf.Clamp01(value);
            if (value != 0.0f)
            {
                if (value > 0.0f)
                    value = SBSMath.Pow(value, 0.55f);
                else
                    value = -SBSMath.Pow(-value, 0.55f);
            }
        }*/

        //Debug.Log("st: " + maxAngle + " v: " + Speed * VehicleUtils.ToKmh);
        float steerAngle = value * maxAngle; //outside wheel steering angle in radians
       
//      float steerOffs = 0.0f;
        /*
        if (!vehicleAI.enabled)
        {
            Vector3 rightVec = rigidbody.rotation * MathUtils.Vector3Right;//Vector3.Cross(ForwardVector, -DownVector);
            if (Math.Abs(rightVec.z) > 0.01f)
                steerOffs = rightVec.z * 2.5f;
            steerAngle += steerOffs;
        }
        */
        //DebugUtils.AddWatch(new Rect(0, 180, 600, 20), "steerAngle: {0} maxSteeringAngle: {1} maxAngle: {2}", steerAngle, maxSteeringAngle, maxAngle);
        //DebugUtils.AddWatch(new Rect(0, 180, 600, 20), "off: {0} steerAngle: {1} maxSteeringAngle: {2} maxAngle: {3}", steerOffs, steerAngle, maxSteeringAngle, maxAngle);
        float width = wheels[(int)VehicleUtils.WheelPos.FR_LEFT].RelaxPosition.x - wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].RelaxPosition.x;
        float length = wheels[(int)VehicleUtils.WheelPos.FR_LEFT].RelaxPosition.z - wheels[(int)VehicleUtils.WheelPos.RR_LEFT].RelaxPosition.z;

        float alpha = SBSMath.Abs(steerAngle * SBSMath.ToRadians);
        float beta = SBSMath.Atan2(1.0f, ((1.0f / (SBSMath.Tan(Mathf.Max(Mathf.Epsilon, alpha)))) - width / length)); //inside wheel steering angle in radians

        float leftWheelAngle = 0.0f;
        float rightWheelAngle = 0.0f;

        if (value > 0.0f)
        {
            leftWheelAngle = alpha;
            rightWheelAngle = beta;
        }
        else
        {
            rightWheelAngle = -alpha;
            leftWheelAngle = -beta;
        }

        leftWheelAngle *= SBSMath.ToDegrees;
        rightWheelAngle *= SBSMath.ToDegrees;

        //Debug.Log("steerAngle: " + steerAngle + " width: " + width + " length: " + length + " alpha: " + alpha + " beta: " + beta);

        wheels[(int)VehicleUtils.WheelPos.FR_LEFT].SteerAngle = leftWheelAngle;
        wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].SteerAngle = rightWheelAngle;

        //DebugUtils.AddWatch(new Rect(0, 180, 600, 20), "wheels[(int)VehicleUtils.WheelPos.FR_LEFT].SteerAngle: " + wheels[(int)VehicleUtils.WheelPos.FR_LEFT].SteerAngle);
        //DebugUtils.AddWatch(new Rect(0, 200, 600, 20), "wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].SteerAngle: " + wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].SteerAngle);

        //Debug.Log("wheels[(int)VehicleUtils.WheelPos.FR_LEFT].SteerAngle: " + wheels[(int)VehicleUtils.WheelPos.FR_LEFT].SteerAngle + " wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].SteerAngle: " + wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].SteerAngle);
    }

    public float GetSpeed()
    {
        float frLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_LEFT].AngularVelocity;
        float frRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].AngularVelocity;
        float rrLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_LEFT].AngularVelocity;
        float rrRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_RIGHT].AngularVelocity;

        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                return (frLeftWheelSpeed + frRightWheelSpeed) * 0.5f * tyres[(int)VehicleUtils.WheelPos.FR_LEFT].Radius;               
            case VehicleUtils.Traction.RWD:
                return (rrLeftWheelSpeed + rrRightWheelSpeed) * 0.5f * tyres[(int)VehicleUtils.WheelPos.RR_LEFT].Radius;   
            case VehicleUtils.Traction.AWD:
                return ((frLeftWheelSpeed + frRightWheelSpeed) * 0.5f * tyres[(int)VehicleUtils.WheelPos.FR_LEFT].Radius +
                    (rrLeftWheelSpeed + rrRightWheelSpeed) * 0.5f * tyres[(int)VehicleUtils.WheelPos.RR_LEFT].Radius) * 0.5f;
        }

        return 0.0f;
    }

	public float GetDriveshaftRPM()
	{
		float driveshaftSpeed = 0.0f;
        /*float frLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_LEFT].AngularVelocity;
        float frRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.FR_RIGHT].AngularVelocity;
        float rrLeftWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_LEFT].AngularVelocity;
        float rrRightWheelSpeed = wheels[(int)VehicleUtils.WheelPos.RR_RIGHT].AngularVelocity;
        */
        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                driveshaftSpeed = differentials[(int)VehicleUtils.DifferentialPos.FRONT].DriveshaftSpeed;
                break;
            case VehicleUtils.Traction.RWD:
                driveshaftSpeed = differentials[(int)VehicleUtils.DifferentialPos.REAR].DriveshaftSpeed;
                break;
            case VehicleUtils.Traction.AWD:
                driveshaftSpeed = differentials[(int)VehicleUtils.DifferentialPos.CENTRAL].DriveshaftSpeed;
                break;
        }

        return gearbox.GetClutchSpeed(driveshaftSpeed) * VehicleUtils.ToRPM;
	}

	public void HandleInputs(float dt)
	{
        foreach (VehicleInputs input in inputs)
        {
            if (input.enabled)
            {
                input.UpdateInputs();

                throttleFromInput = input.Throttle;
                steeringFromInput = input.Steering;
                handbrakeFromInput = input.Handbrake;

                //Debug.Log("throttleFromInput " + throttleFromInput + " steeringFromInput " + steeringFromInput + " handbrakeFromInput " + handbrakeFromInput + " input " + input);
            }
        }

        //float throttleInput = (freezed ? -0.35f : Input.GetAxis("Vertical") * ( gearbox.CurrentGear >= 0 ? 1.0f: -1.0f));
        float throttleInput = (freezed ? -0.35f : throttleFromInput * (gearbox.CurrentGear >= 0 ? 1.0f : -1.0f));// Input.GetAxis("Vertical") * (gearbox.CurrentGear >= 0 ? 1.0f : -1.0f));
        /*
        if (vehicleAI.enabled)// && Input.GetKey(KeyCode.A))
        {
            throttleInput = vehicleAI.Throttle * ( gearbox.CurrentGear >= 0 ? 1.0f: -1.0f);
        }
        */
        float vehicleSpeed = rigidBody.velocity.magnitude;
        if (limitSpeed != -1.0f)
        {
            if (vehicleSpeed * VehicleUtils.ToMph > limitSpeed)
            {
                engine.PowerLimit = true;
                //throttleInput = -Mathf.Min((vehicleSpeed * VehicleUtils.ToMph - limitSpeed) / 30.0f, 1.0f);
                throttleInput = -SBSMath.Min((vehicleSpeed * VehicleUtils.ToMph - limitSpeed) / 50.0f, 0.8f);
            }
            else
                engine.PowerLimit = false;
        }
        else
        {
            engine.PowerLimit = false;
        }

        if (rigidBody.isKinematic)
            throttleInput = 0.0f;
        float throttle = SBSMath.Max(0.0f, throttleInput);

        //set the throttle
        if(autoClutch)
            throttle *= ShiftAutoClutchThrottle();
        engine.Throttle = throttle;

        //Debug.Log(" throttle: " + throttle);

        //set the brakes
        float brakeInput = -throttleInput;
        for (int i = 0; i < 4; i++)
        {
            float brakeCoeff = brakeInput;
            brakes[i].BrakeCoeff = SBSMath.Max(0.0f, brakeCoeff);
            brakes[i].HandbrakeCoeff = handbrakeFromInput ? 4.0f : 0.0f;
        }

        float steerValue = (freezed ? 0.0f : -steeringFromInput);//-Input.GetAxis("Horizontal"));
/*      if (vehicleAI.enabled)// && !Input.GetKey(KeyCode.S))
        {
            steerValue = -vehicleAI.Steering;
        }*/
        if (!IsOnGround)
            steerValue = 0.0f;

        SetSteering(steerValue);
	    prevSteer = steerValue;

        //gearbox.CurrentGear = 6;

        int gearChange = 0;
        if (autoShiftEnabled)
        {
            if (gearbox.CurrentGear != 0 || (gearbox.CurrentGear == 0 && autoShiftInNeuter))
                gearChange = AutoShift(throttleInput);
        }
        else
            gearChange = inputGearChange;
        
	    int currentGear = gearbox.CurrentGear;
	    int newGear = currentGear + gearChange;

        //DebugUtils.AddWatch(new Rect(0, 60, 600, 20), "gearChange: {0} newGear: {1} ForGears: {2} -RevGears: {3} friction.Pos: {4}", gearChange, newGear, gearbox.ForwardGears, -gearbox.ReverseGears, friction.Position);

	    if (currentGear != newGear && shifted)
	    {
		    if (newGear <= gearbox.ForwardGears && newGear >= -gearbox.ReverseGears)
		    {
			    ShiftGears(newGear, (newGear == 0)); //immediately shift into neutral
                //Debug.Log( "newGear == 0 " + (newGear == 0) );
		    }
	    }

	    if (remainingShiftTime <= gearbox.ShiftDelay *0.5f && !shifted)
    	{
	    	shifted = true;
		    gearbox.ChangeGear(shiftGear);
            if (!autoShiftEnabled)
                inputGearChange = 0;
	    }

	    if (autoClutch)
	    {
            float newAutoClutch = AutoClutch(prevAutoClutch, dt);
		    prevAutoClutch = newAutoClutch;
		    friction.Position = newAutoClutch;
            //DebugUtils.AddWatch(new Rect(0, 5 * 20, 400, 20), String.Format("autoClutch: {0} newAutoClutch: {1}", autoClutch, newAutoClutch));
	    }
	
	    if (autoClutch)
	    {
		    if (engine.Stalled)
			    engine.StartEngine();
	    }
    }

    public void ShiftGears(int newGear)
    {
        ShiftGears(newGear, false);
    }

    public void ShiftGears(int newGear, bool immediate)
	{
        if (immediate)
			remainingShiftTime = 0.001f;
		else
			remainingShiftTime = gearbox.ShiftDelay;
		
		shiftGear = newGear;
		shifted = false;
		shiftClutchStart = friction.Position;
	}


    public bool IsDriveWheel(int idx)
    {
        switch (traction)
        {
            case VehicleUtils.Traction.FWD:
                return (idx == (int)VehicleUtils.WheelPos.FR_LEFT || idx == (int)VehicleUtils.WheelPos.FR_RIGHT);
            case VehicleUtils.Traction.RWD:
                return (idx == (int)VehicleUtils.WheelPos.RR_LEFT || idx == (int)VehicleUtils.WheelPos.RR_RIGHT);
            case VehicleUtils.Traction.AWD:
                return true;
        }
        return true;
    }

    private float AutoClutch ( float prevClutch, float dt )
    {
        float threshold = 1000.0f;
        float margin = 100.0f;
        float gearEffect = 1.0f;

        bool protectBrakeLockup = false;

        float rpm = engine.RPM;
        //float driveshaftRPM = GetDriveshaftRPM();
        float driveshaftRPM  = rpm;
        bool braking = false;

        if (protectBrakeLockup)
        {
            for (int i = wheels.Length - 1; i >= 0; --i)
            {
                if (IsDriveWheel(i))
                {
                    if (brakes[i].WillLock(wheels[i].AngularVelocity))
                    {
                        braking = true;
                    }
                }
            }
        }

        if (driveshaftRPM < rpm)
            rpm = driveshaftRPM;

        float rpmLimit = engine.RPMLimit;
        float rpmStall = engine.RPMStall + margin * (rpmLimit / 2000.0f);
        int currentGear = gearbox.CurrentGear;

        float gearCoeff = 1.0f;
        if (currentGear <= 1)
            gearCoeff = 2.0f;

        float tmpThreshold = threshold * (rpmLimit / 7000.0f) * ((1.0f - gearEffect) + gearCoeff * gearCoeff) + rpmStall;
        if (friction.Engaged)
            tmpThreshold *= 0.5f;

        float clutch = Mathf.Clamp01 ( (rpm - rpmStall) / (tmpThreshold - rpmStall) );
        float shiftClutch = ShiftAutoClutch();

        if (currentGear == 0)
            clutch = 0.0f;

        float newAutoClutch = clutch * shiftClutch;

        // limit  autoclutch
        float minEngageTime = 0.05f; 
        float engageRateLimit = 1.0f / minEngageTime;
        float rate = (prevClutch - newAutoClutch) / dt;

        if (rate > engageRateLimit)
            newAutoClutch = prevClutch - engageRateLimit * dt;

        if (braking)
            return 0;
        else
            return newAutoClutch;
    }

    public float ShiftAutoClutch()
    {
        float shiftClutch = 1.0f;
        if (remainingShiftTime > gearbox.ShiftDelay * 0.5f)
            shiftClutch = 0.0f;
        else if (remainingShiftTime > 0.0f && remainingShiftTime <= gearbox.ShiftDelay * 0.5f)
            shiftClutch = 1.0f - remainingShiftTime / (gearbox.ShiftDelay * 0.5f);

        return shiftClutch;
    }

    public float ShiftAutoClutchThrottle()
    {
        return ShiftAutoClutch() < 1.0f ? 0.0f : 1.0f;
    }

    //-1 shift down, 1 shift up, 0 no shift
    public int AutoShift(float throttleInput)
    {
	    int currentGear = gearbox.CurrentGear;

        //float rpm = GetEngineRPM();
        float rpm = GetDriveshaftRPM();

	    if (shifted)
	    {
            if (currentGear > 0)// && friction.Position >= 1.0f)
            {                
                if (friction.Position >= 1.0f)
					if (rpm > engine.RPMRedline && currentGear < gearbox.ForwardGears)
                    	return 1;
                /*
                if (immediateGearDown)
                {
                    Debug.Log("friction.Position " + friction.Position);
                }
                */
                if (currentGear > 0 && rpm < DownshiftPoint(currentGear) && (throttleInput <= 0.0f || ( TimeManager.Instance.MasterSource.TotalTime <= lastCollisionTime + 1.0f && gearDownOnCollision ) || (currentGear > 1 && immediateGearDown && (rpm + 1000.0f < DownshiftPoint(currentGear))))) // friction.Position >= 1.0f)))
                {    
                    return -1;
                }
            }
            else if (currentGear == 1)
            {
                if (rpm < DownshiftPoint(currentGear))
                {
                    if (throttleInput > 0.0f)
                    {
                        if (friction.Position >= 1.0f)
                            return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else if (currentGear == 0)
            {
                if (throttleInput > 0.5f)
                    return 1;
                else if (throttleInput < -0.5f)
                {
                    //if (vehicleAI.enabled)
                    //    return 0;
                    return -1;
                }
            }
            else if (currentGear < 0)
            {
                if ( throttleInput < 0.0f && rpm < DownshiftPoint(currentGear) )
                    return 1;
            }
        }

	    return 0;
    }

    public float DownshiftPoint(int gear)
    {
	    //float shiftDownPoint = 1200.0f;
        float shiftDownPoint = 4000.0f;
	    if (gear > 2)
	    {
		    double currentGearRatio = gearbox.GetGearRatio(gear);
		    double lowerGearRatio = gearbox.GetGearRatio(gear - 1);
		    float peakEngineSpeed = engine.RPMRedline;
            shiftDownPoint = (float)(peakEngineSpeed / lowerGearRatio * currentGearRatio);
	    }

        return shiftDownPoint;
    }

    private void UpdateCarWheelCollisions()
    {
	    SBSVector3 dir = DownVector;
		for (int n = wheels.Length - 1; n >= 0; --n)
		{
			//Vector3 wp = GetWheelPosition(n);
			//GetWheelPositionAtDisplacement

			float raylen = 10.0f;
            SBSVector3 raystart = /*GetWheelPositionAtDisplacement(n, 1.0f)*/(cachedRBMat * localWheelsRestPosition[n]) - dir * tyres[n].Radius;

            RaycastHit[] hitInfo;
			hitInfo = Physics.RaycastAll(raystart, dir, raylen, ~ignoreLayers);//~(1 << LayerMask.NameToLayer("Ignore Raycast")));
			int l = hitInfo.Length;
			if (l > 0)
			{
				int minIdx = -1;
				float minDistance = float.MaxValue;
				int i = 0;
				//foreach (RaycastHit item in hitInfo)
				while (i < l)
				{
					RaycastHit item = hitInfo[i];
					if (/*item.collider.name.IndexOf("veh_") != 0 && -1 == item.collider.name.IndexOf("_cam")*/item.collider != myCollider && SBSVector3.Angle(item.normal, SBSVector3.up) < 75.0f && item.distance < minDistance)
					{
						minDistance = hitInfo[i].distance;
						minIdx = i;
					}

					++i;
				}
				
				if (-1 == minIdx)
				{
					SetWheelContactProperties(n, 100.0f, (raystart + dir * 100.0f), (-dir), 1.0f, 0.0f, 1.0f, 0.9f, 1.0f, 0.0f, ContactData.SurfaceType.None);
//					wheelRaycastHits[n] = new RaycastHit();
				}
				else
				{
					RaycastHit firstHit = hitInfo[minIdx];
					float dist = firstHit.distance;
					float depth = dist - tyres[n].Radius;// - moveback;
					SBSVector3 hitPos = firstHit.point;
					SBSVector3 hitNorm = firstHit.normal;

                    ContactData.SurfaceType surfType = ContactData.SurfaceType.Asphalt;
					//if (firstHit.collider.name.StartsWith("l_t") || firstHit.collider.name.StartsWith("ext_boxroad") || firstHit.collider.name.StartsWith("l_r"))
					/*surfType = ContactData.SurfaceType.Asphalt;
					if (firstHit.collider.name.StartsWith("Grass"))
						surfType = ContactData.SurfaceType.Grass;
					if (firstHit.collider.name.StartsWith("Safe"))
						surfType = ContactData.SurfaceType.None;*/
                    /*if (trackData != null) {
                        List<TokensManager.TokenHit> tokenHits = TokensManager.Instance.GetTokens(hitPos, trackData.Token, null, false, -1);
                        foreach (TokensManager.TokenHit tokenHit in tokenHits)
                        {
                            TrackBranch tokenBranch = tokenHit.token.TrackBranch;
                            if (null == tokenBranch)
                                continue;

                            bool stopLoop = false;
                            switch (tokenBranch.mask)
                            {
                                case 1:
                                case 3:
                                    surfType = ContactData.SurfaceType.Asphalt;
                                    stopLoop = true;
                                    break;
                                case 2:
                                case 4:
                                    surfType = ContactData.SurfaceType.Grass;
                                    break;
                            }

                            if (stopLoop)
                                break;
                        }
                    }
                    if (Input.GetKey(KeyCode.Space))
                        Debug.Log(surfType);*/

					//(int idx, float wheelHeight, Vector3 position, Vector3 normal, float bumpWaveLength, float bumpAmplitude, float frictionNoTread, float frictionTread, float rollingResistance, float rollingDrag, ContactData.SurfaceType surfaceType)
					//PIETRO
					if (surfType == ContactData.SurfaceType.Asphalt)
						SetWheelContactProperties(n, depth, hitPos, hitNorm, damaged ? 30.0f : 1.0f, damaged ? 0.055f : 0.0f, 1.0f * grip, 0.9f * grip, 1.0f, 1.0f, surfType);
					else
						SetWheelContactProperties(n, depth, hitPos, hitNorm, damaged ? 30.0f : 25.0f, damaged ? 0.065f : 0.040f, 0.9f * grip, 0.9f * grip, 8.0f, 8.0f, surfType);

//					wheelRaycastHits[n] = firstHit;
				}
			}
			else
			{
				SetWheelContactProperties(n, 100.0f, (raystart + dir * 100.0f), (-dir), 1.0f, 0.0f, 1.0f, 0.9f, 1.0f, 0.0f, ContactData.SurfaceType.None);
//				wheelRaycastHits[n] = new RaycastHit();
			}
		}
    }

    void Restore(float dt)
    {
		if (!enablePhysics)
			return;
        /*
        SBSVector3 downVec = this.DownVector;

        float threshold = -0.5f;
        if (!this.IsOnGround)
            threshold = -0.95f;

		if (!isRestoring && SBSVector3.Dot(downVec, SBSVector3.up) > threshold)// && this.Speed < 10.0f)
			isRestoring = true;

		if (isRestoring)//Vector3.Dot(downVec, MathUtils.Vector3Up) > 0.0f && this.Speed < 10.0f)
		{
            if (SBSVector3.Dot(downVec, SBSVector3.up) <= threshold)
			{
				isRestoring = false;
				return;
			}

			//Vector3 upVec = -downVec;
            float angle = SBSVector3.Angle(downVec, SBSVector3.down) * SBSMath.ToRadians;
            SBSVector3 axis = SBSVector3.Cross(downVec, SBSVector3.down);

            if (SBSVector3.Dot(axis, axis) < SBSMath.Epsilon)
                axis = new SBSVector3(0.0f, 0.0f, 1.0f);
            else
                axis.Normalize();

            rigidBody.AddTorque(angle * restoreStrength * axis, ForceMode.Acceleration);
            rigidbody.AddTorque(-SBSVector3.Dot(rigidbody.angularVelocity, axis) * SBSMath.Pow(1.0f - restoreDamping, dt) * axis, ForceMode.VelocityChange);
		}*/
        /*
        float maxAngle = 30.0f;
        if (!this.IsOnGround)
            maxAngle = 5.0f;

        SBSVector3 upVec = -this.DownVector;
        float angle = SBSVector3.Angle(upVec, SBSVector3.up);
        SBSVector3 axis = SBSVector3.Cross(upVec, SBSVector3.up);

        if (angle > maxAngle && axis.Normalize() > 0.0f)
        {
            rigidBody.AddTorque((angle - maxAngle) * restoreStrength * axis, ForceMode.Force);
            rigidbody.AddTorque(-SBSVector3.Dot(rigidbody.angularVelocity, axis) * SBSMath.Pow(1.0f - restoreDamping, dt) * axis, ForceMode.VelocityChange);
        }*/
    }

    void FixedUpdate()
    {
        prevSpeed = rigidBody.velocity;

		cachedRBOri = rigidBody.rotation;
		cachedRBOri.Normalize();
		cachedRBPos = rigidBody.position;
        cachedRBMat = SBSMatrix4x4.TRS(cachedRBPos, cachedRBOri, SBSVector3.one);
        cachedRBCom = cachedRBMat * rigidBody.centerOfMass;

		if (dynamicStart != -1.0f)
        {
            InititializeDynamicStart(dynamicStart);
            dynamicStart = -1.0f;
        }

        float dt = Time.fixedDeltaTime;
        HandleInputs(dt);
        UpdatePhysics(dt);

//      this.Restore(dt);
		if (chassisGraphics != null)
		{
			chassisGraphics.rotation = cachedRBOri;
			/*
			Vector3 offset = transform.TransformDirection(new Vector3(0.0f, chassisHeightOffset, 0.0f));
            chassisGraphics.position = (Vector3)cachedRBPos + offset;*/
			chassisGraphics.position = cachedRBMat.MultiplyPoint3x4(0.0f, chassisHeightOffset, 0.0f);
		}

        for (int i = 0; i < 4; ++i)
        {
            if (null == wheelGraphics[i])
                continue;

//          float radius = tyres[i].Radius;
//          ContactData contactData = this.GetWheelContactProperties(i);
			
//			SBSQuaternion q = this.GetWheelOrientation(i);
            wheelGraphics[i].rotation = this.GetWheelOrientation(i);//new Quaternion(q.x, q.y, q.z, q.w);

//			SBSVector3 p = prevWheelPositions[i];//this.GetWheelPosition(i);
            wheelGraphics[i].position = prevWheelPositions[i];//new Vector3(p.x, p.y, p.z);
        }

        if (cameraFSM != null)
        {
            cameraFSM.ForceUpdate();
        }
	}

	protected virtual bool ContactIsOnGround(ContactPoint contact)
	{
		return SBSMath.Abs(Vector3.Dot(contact.normal, Vector3.up)) > 0.75f;
	}

	public void ResetGroundCounter()
	{
		isOnGroundCounter = 0;
		groundColliders = new List<Collider>(8);
	}

    #region Alex
    protected Bounds localBBox;

    public Bounds LocalBBox
    {
        get
        {
            return localBBox;
        }
    }

    public void ForceUpdateGraphics()
    {
        if (!initialized)
            this.Init();

        cachedRBOri = rigidBody.rotation;
        cachedRBOri.Normalize();
        cachedRBPos = rigidBody.position;
        cachedRBMat = SBSMatrix4x4.TRS(cachedRBPos, cachedRBOri, SBSVector3.one);
        cachedRBCom = cachedRBMat * rigidBody.centerOfMass;

        if (chassisGraphics != null)
        {
            ChassisGraphics gfx = chassisGraphics.GetComponent<ChassisGraphics>();
            if (gfx != null)
                gfx.Init();

            chassisGraphics.rotation = cachedRBOri;
            chassisGraphics.position = cachedRBMat.MultiplyPoint3x4(0.0f, chassisHeightOffset, 0.0f);
        }

        for (int i = 0; i < 4; ++i)
        {
            if (null == wheelGraphics[i])
                continue;

            wheelGraphics[i].rotation = this.GetWheelOrientation(i);
            wheelGraphics[i].position = prevWheelPositions[i];
        }
    }

    public void UpdateLocalBoundingBox()
    {
        localBBox.SetMinMax(Vector3.zero, Vector3.zero);

        MeshFilter[] filters = null;
        if (chassisGraphics != null)
        {
            filters = chassisGraphics.gameObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
            {
                Bounds b = filter.sharedMesh.bounds;
                Matrix4x4 m = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                Vector3 s = m.MultiplyVector(b.size);
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                s.z = Mathf.Abs(s.z);
                localBBox.Encapsulate(new Bounds(m.MultiplyPoint(b.center), s));
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            if (null == wheelGraphics[i])
                continue;

            filters = wheelGraphics[i].gameObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
            {
                Bounds b = filter.sharedMesh.bounds;
                Matrix4x4 m = transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                Vector3 s = m.MultiplyVector(b.size);
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                s.z = Mathf.Abs(s.z);
                localBBox.Encapsulate(new Bounds(m.MultiplyPoint(b.center), s));
            }
        }

        Debug.Log("### localBBox: " + localBBox);
    }
    #endregion

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("rigidbody.name: " + collision.relativeVelocity);
        /*
        DeformableMesh defMesh = null == chassisGraphics ? null : chassisGraphics.gameObject.GetComponent<DeformableMesh>();
        if (defMesh != null)
            defMesh.Deform(collision);
        */
        int len = collision.contacts.Length;
		for (int i = 0; i < len; ++i)
		{
			//Debug.Log("contact Normal[" + i + "]: " + collision.contacts[i].normal + " dot: " + Vector3.Dot(collision.contacts[i].normal, MathUtils.Vector3Up));
			//if (Vector3.Dot(collision.contacts[i].normal, MathUtils.Vector3Up) > 0.75f)
			int index = groundColliders.IndexOf(collision.collider);
			if (index < 0)
			{
				if (this.ContactIsOnGround(collision.contacts[i]))
				{
					isOnGroundCounter++;
					groundColliders.Add(collision.collider);
					break;
					//return;
				}
			}
		}
    }

    public void OnCollision(Collision collision)
    {
        lastCollisionTime = TimeManager.Instance.MasterSource.TotalTime;
    }

    void Freeze(bool resetVelocities)
    {
//      rigidbody.isKinematic = true;
        if (resetVelocities)
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
        freezed = true;
//      vehicleAI.enabled = true;
    }

    void Unfreeze()
    {
//      rigidbody.isKinematic = false;
        freezed = false;
/*      if (gameObject.GetComponent<VehicleRaceData>().NotifyChanges)
            vehicleAI.enabled = false;*/
    }

    void SetAIEnabled(bool flag)
    {
        isAI = flag;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (null == lastSuspensionApply || null == wheelContacts || !enablePhysics || !enabled)
            return;

        for ( int i = 0; i < lastSuspensionApply.Length; i++ )
        {
            Gizmos.DrawRay(lastSuspensionApply[i], lastSuspensionForce[i] * VehicleUtils.ToKiloNewtons * 0.5f);
            //if ( i > 1)
                //Gizmos.DrawIcon(lastSuspensionApply[i], "Models/Textures/not_found");
    //            Gizmos.DrawIcon(GetWheelPositionAtDisplacement(i, 1.0f), "Models/Textures/not_found");
                //Gizmos.DrawIcon(GetWheelPositionAtDisplacement(i, 0.0f), "Models/Textures/not_found");
        }

        for ( int i = 0; i < wheelContacts.Length; i++)
        {
            if (wheelContacts[i] != null)
                Gizmos.DrawRay(wheelContacts[i].Position, tyreForces[i] * 0.001f);
            //GetTyreSlide
     //       Gizmos.DrawRay(wheelContacts[i].Position, tmpVars[i] * 0.01f);//GetTyreSlide(i) * MathUtils.Vector3Up);
            //if (i > 1)
     //           Gizmos.DrawIcon(wheelContacts[i].Position, "Models/Textures/not_found");
        }

        /*
        SBSVector3 contactUp = new SBSVector3();
        for (int i = 0; i < wheelContacts.Length; i++)
        {
            contactUp = contactUp + wheelContacts[i].Normal;
        }
        contactUp.ScaleBy(1.0f / wheelContacts.Length);
        Gizmos.DrawRay(wheelContacts[0].Position, contactUp * 10.0f);
        */

  //      Gizmos.DrawIcon(transform.TransformPoint(rigidbody.centerOfMass), "Models/Textures/not_found");
        //DebugUtils.AddWatch(new Rect(0, 0, 200, 40), "Susp: {0}", lastSuspensionForce[0].ToString());
        //DebugUtils.AddWatch(new Rect(0, 0, 200, 40), "Susp: " + lastSuspensionForce[0].ToString()); 
    }
#endif
};