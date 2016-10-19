using System;
using UnityEngine;
using SBS.Math;
using SBS.VFX;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/WheelGraphics")]
public class WheelGraphics : MonoBehaviour
{
    #region Public members
    public VehiclePhysics vehiclePhysics = null;
    public VehicleUtils.WheelPos index = VehicleUtils.WheelPos.FR_LEFT;

    public float polylineWidth = 0.5f;
    public int polylinePoints = 25;
    public float polylineTextureMult = 1.0f;
    public float polylineHeightOffset = 0.01f;
    public Material polylineMaterial;

    public GameObject[] particleSystems;
    public GameObject fireParticleSystems;
    #endregion

    #region Protected members
    protected bool polylineActive;
    protected Polyline polyline;
    protected GameObject polylineGo;

    protected GameObject[] particles = new GameObject[(int)ContactData.SurfaceType.NumSurfaces];
    protected GameObject fireParticle;
    #endregion

    private bool _aiEnabled = false;

    #region Public methods
    public void ResetParticles()
    {
        for (int i = 0; i < (int)ContactData.SurfaceType.NumSurfaces; ++i)
        {
            if (null == particles[i])
                continue;
            ParticleSystem emitter = particles[i].GetComponent<ParticleSystem>();
            if (emitter.isPlaying)
                emitter.Stop();
        }

        if (null != fireParticle)
        {
            ParticleSystem fireEmitter = fireParticle.GetComponent<ParticleSystem>();
            if (fireEmitter.isPlaying)
                fireEmitter.Stop();
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        if (vehiclePhysics != null)
            vehiclePhysics.SetWheelGraphics((int)index, transform);
    }

    void Start()
    {
        polylineActive = false;

        Polyline.Point startPt = new Polyline.Point();

        startPt.point = SBSVector3.zero;
        startPt.tangent = SBSVector3.forward;
        startPt.normal = SBSVector3.up;
        startPt.width = polylineWidth;
        startPt.u0 = 0.0f;
        startPt.u1 = 1.0f;
        startPt.v = 0.0f;

        polyline = new Polyline(name + "_pl", polylinePoints, startPt, null);
        polyline.minDistBetweenPoints = 1.0f;
        polyline.minTimeBetweenPoints = 0.0f;
        polylineGo = new GameObject(polyline.Mesh.name);
        polylineGo.transform.parent = LevelRoot.Instance.Root;
        polylineGo.AddComponent<MeshFilter>().sharedMesh = polyline.Mesh;
        Renderer rnd = polylineGo.AddComponent<MeshRenderer>();
        rnd.sharedMaterial = polylineMaterial;

        for (int i = 0; i < (int)ContactData.SurfaceType.NumSurfaces; ++i)
        {
            int j = Mathf.Clamp(i, 0, particleSystems.Length - 1);
            if (null == particleSystems[j])
            {
                particles[i] = null;
                continue;
            }
            Transform tr = (particles[i] = GameObject.Instantiate(particleSystems[j]) as GameObject).transform;
            tr.parent = transform;
        }

        if (null == fireParticleSystems)
        {
            fireParticle = null;
        }
        else
        {
            Transform fireTr = (fireParticle = GameObject.Instantiate(fireParticleSystems) as GameObject).transform;
            fireTr.position = transform.position;
            fireTr.parent = transform;
        }
        
       
    }

    void OnDestroy()
    {
        if (polylineGo != null)
            DestroyImmediate(polylineGo);
    }

    void Update()
    {
        if (null == vehiclePhysics || !vehiclePhysics.Initialized)
            return;

        float slide = vehiclePhysics.GetTyreSlide((int)index),
              threshold = 0.85f;

        ContactData contactData = vehiclePhysics.GetWheelContactProperties((int)index);
        if (null == contactData)
            return;
        float speed = vehiclePhysics.Speed;

        Polyline.Point newPt = new Polyline.Point();

        newPt.normal = contactData.Normal;
        newPt.width = polylineWidth;
        newPt.u0 = 0.0f;
        newPt.u1 = 1.0f;
        newPt.v = polyline.Length * polylineTextureMult;

        SBSVector3 wheelPos = transform.position,
                   wheelTan = vehiclePhysics.GetComponent<Rigidbody>().GetPointVelocity(wheelPos);

        newPt.point = contactData.Position + SBSVector3.up * polylineHeightOffset;
        newPt.tangent = wheelTan;
        newPt.width = polylineWidth;
        newPt.isFirst = false;

        if (polylineActive && (slide < threshold || ContactData.SurfaceType.None == contactData.TypeOfSurface || !vehiclePhysics.wheelOnGround((int)index)))
            polylineActive = false;

        if (!polylineActive && slide >= threshold && contactData.TypeOfSurface != ContactData.SurfaceType.None)
        {
            polylineActive = true;
            newPt.isFirst = true;

            polyline.AddPoint(newPt);
        }

        if (polylineActive)
        {
            if (!polyline.AddPoint(newPt))
                if (!polyline.Back.isFirst)
                    polyline.Back = newPt;

            polyline.Flush();
        }

        foreach (GameObject particle in particles)
        {
            if (null == particle)
                continue;

            particle.transform.position = contactData.Position;
            particle.transform.LookAt(contactData.Position + contactData.Normal - vehiclePhysics.ForwardVector, SBSVector3.up);
        }

        for (int i = 0; i < (int)ContactData.SurfaceType.NumSurfaces; ++i)
        {
            if (null == particles[i])
                continue;


            bool active = ((int)contactData.TypeOfSurface == i && polylineActive);
            
            if (i == (int)ContactData.SurfaceType.Grass && (int)contactData.TypeOfSurface == (int)ContactData.SurfaceType.Grass && speed >= 2.0f)
                active = true;

            ParticleSystem emitter = particles[i].GetComponent<ParticleSystem>();
            if (active)
            {
                if (!emitter.isPlaying)
                    emitter.Play();
            }
            else
            {
                if (emitter.isPlaying)
                    emitter.Stop();
            }
        }

        
        if (null != fireParticleSystems)
        {
            fireParticle.transform.position = contactData.Position + SBSVector3.up * 0.1f;
            fireParticle.transform.LookAt(contactData.Position + contactData.Normal  - vehiclePhysics.ForwardVector * 5.0f, SBSVector3.up); //+ SBSVector3.up * 0.5f + contactData.Normal 


            bool fireActive = ((!_aiEnabled) && vehiclePhysics.wheelOnGround((int)index) && vehiclePhysics.EngineNitroPower > 0.0f ); //&& (vehiclePhysics.gameObject.GetComponent<Turbo>().turboLevel > 2));
            ParticleSystem fireEmitter = fireParticle.GetComponent<ParticleSystem>();
            if (fireActive)
            {
                if (!fireEmitter.isPlaying)
                    fireEmitter.Play();
            }
            else
            {
                if (fireEmitter.isPlaying)
                    fireEmitter.Stop();
            }
        }
    }
    #endregion

    #region Messages
    void IsPlayerAiEnabled(bool flag)
    {
        _aiEnabled = flag;
    }
    #endregion
}