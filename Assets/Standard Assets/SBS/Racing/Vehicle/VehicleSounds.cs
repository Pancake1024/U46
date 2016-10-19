using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/VehicleSounds")]
public class VehicleSounds : MonoBehaviour
{
    #region Constants
    public enum Sounds
    {
        FR_LEFT,
        FR_RIGHT,
        RR_LEFT,
        RR_RIGHT,
        ENGINE_1,
        ENGINE_2,
        NUM_SOUNDS
    }
    #endregion

    public class EngineSoundData
    {
        public enum PowerType
        {
            ON,
            OFF,
            BOTH
        }

        public float minRPM;
        public float maxRPM;
        public float sampleRPM;
        public float fullGainMinRPM;
        public float fullGainMaxRPM;
        public PowerType power;
        public AudioSource audioSource;
        public float tmpVolume;

        public EngineSoundData(float _minRPM, float _maxRPM, float _sampleRPM, float _fullGainMinRPM, float _fullGainMaxRPM, PowerType _power)
        {
            minRPM = _minRPM;
            maxRPM = _maxRPM;
            sampleRPM = _sampleRPM;
            fullGainMinRPM = _fullGainMinRPM;
            fullGainMaxRPM = _fullGainMaxRPM;
            power = _power;
            audioSource = null;
            tmpVolume = 0.0f;
        }
    }

    #region Public members
    public AudioClip engineClip;
    public AudioClip outOfTrackClip;
    public AudioClip[] slideClips;
    public AudioClip[] impactClips;
    public AudioClip[] ricochetClips;
    public AudioClip nitroStartClip;
    public AudioClip nitroLoopClip;
    #endregion

    #region Protected members
    protected List<AudioSource> sources;
    protected List<EngineSoundData> engineSoundDatas;
    protected AudioSource outOfTrackSource;
    protected AudioSource collisionSource;
    protected AudioSource ricochetSource;
    protected AudioSource nitroStartSource;
    protected AudioSource nitroLoopSource;
    protected VehiclePhysics vehiclePhysics = null;
    protected float lastCollisionTime = -1000.0f;
    #endregion

    //  void Start()
    public void Init()
    {
        vehiclePhysics = gameObject.GetComponent<VehiclePhysics>();

        sources = new List<AudioSource>();

        AudioSource source;
        for (int i = 0; i < (int)VehicleUtils.WheelPos.NUM_WHEELS; i++)
        {
            Transform wheelTransform = vehiclePhysics.GetWheelGraphics(i);
            source = wheelTransform.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = slideClips[0];
            source.loop = true;
            source.volume = 0.0f;
//          source.rolloffFactor = 0.2f;//1.0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 10.0f;
            source.maxDistance = 30.0f;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            sources.Add(source);
            SoundsManager.Instance.PlaySource(source);
        }

        engineSoundDatas = new List<EngineSoundData>();
        Engine engine = vehiclePhysics.Engine;
        //EngineSoundData engineSoundData = new EngineSoundData(1.0f, engine.RPMLimit + 1000.0f, 7500.0f, 1.0f, engine.RPMLimit + 1000.0f, EngineSoundData.PowerType.BOTH);
        EngineSoundData engineSoundData = new EngineSoundData(1.0f, engine.RPMLimit + 1000.0f, 6400.0f, 1.0f, engine.RPMLimit + 1000.0f, EngineSoundData.PowerType.BOTH);

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = engineClip;
        source.loop = true;
        source.volume = 0.0f;
//      source.rolloffFactor = 0.2f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 0.0f;
        source.maxDistance = 60.0f;
        source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        SoundsManager.Instance.PlaySource(source);

        //source.playOnAwake = false;
        engineSoundData.audioSource = source;
        engineSoundDatas.Add(engineSoundData);

        outOfTrackSource = gameObject.AddComponent<AudioSource>();
        outOfTrackSource.playOnAwake = false;
        outOfTrackSource.clip = outOfTrackClip;
        outOfTrackSource.loop = true;
        outOfTrackSource.volume = 40.0f;
//      outOfTrackSource.rolloffFactor = 0.2f;
        outOfTrackSource.rolloffMode = AudioRolloffMode.Linear;
        outOfTrackSource.minDistance = 6.0f;
        outOfTrackSource.maxDistance = 30.0f;
        outOfTrackSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        collisionSource = gameObject.AddComponent<AudioSource>();
        collisionSource.playOnAwake = false;
        collisionSource.clip = impactClips[0];
        collisionSource.loop = false;
        collisionSource.volume = 8.0f;
//      collisionSource.rolloffFactor = 0.5f;
        collisionSource.rolloffMode = AudioRolloffMode.Linear;
        collisionSource.minDistance = 5.0f;
        collisionSource.maxDistance = 25.0f;
        collisionSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        nitroStartSource = gameObject.AddComponent<AudioSource>();
        nitroStartSource.playOnAwake = false;
        nitroStartSource.clip = nitroStartClip;
        nitroStartSource.loop = false;
        nitroStartSource.volume = 50.0f;
        nitroStartSource.rolloffMode = AudioRolloffMode.Linear;
        nitroStartSource.minDistance = 5.0f;
        nitroStartSource.maxDistance = 25.0f;
        nitroStartSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        nitroLoopSource = gameObject.AddComponent<AudioSource>();
        nitroLoopSource.playOnAwake = false;
        nitroLoopSource.clip = nitroLoopClip;
        nitroLoopSource.loop = true;
        nitroLoopSource.volume = 50.0f;
        nitroLoopSource.rolloffMode = AudioRolloffMode.Linear;
        nitroLoopSource.minDistance = 5.0f;
        nitroLoopSource.maxDistance = 25.0f;
        nitroLoopSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        ricochetSource = gameObject.AddComponent<AudioSource>();
        ricochetSource.playOnAwake = false;
        ricochetSource.clip = ricochetClips[0];
        ricochetSource.loop = false;
        ricochetSource.volume = 50.0f;
        ricochetSource.rolloffMode = AudioRolloffMode.Linear;
        ricochetSource.minDistance = 5.0f;
        ricochetSource.maxDistance = 25.0f;
        ricochetSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
    }

    public void PlayCollisionSound(float relVelocity, int clipIndex)
    {
        collisionSource.clip = impactClips[Mathf.Clamp(clipIndex, 0, impactClips.Length - 1)];
        collisionSource.volume = relVelocity * 5.0f;
        //collisionSource.volume *= SoundsManager.Instance.VolumeScaling;
        SoundsManager.Instance.PlaySource(collisionSource);
    }

    public void OnCollision(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1.0f && TimeManager.Instance.MasterSource.TotalTime > lastCollisionTime + 1.0f)
        {
            if(collision.collider.gameObject.name.StartsWith("ramp"))
                return;

            if (Mathf.Abs(Vector3.Dot(collision.relativeVelocity, new Vector3(0.0f, 1.0f, 0.0f))) < 0.60f)
                PlayCollisionSound(collision.relativeVelocity.magnitude, UnityEngine.Random.Range(0, impactClips.Length));
        }

        lastCollisionTime = TimeManager.Instance.MasterSource.TotalTime;
    }

    public void OnExplosiveObjectCollision(float velocity)
    {
        //Debug.Log("OnExplosiveObjectCollision: " + velocity);
        //if (velocity > 1.0f)
        //    PlayCollisionSound(velocity, UnityEngine.Random.Range(0, impactClips.Length));
    }

    public void Shooted()
    {
        ricochetSource.clip = ricochetClips[UnityEngine.Random.Range(0, ricochetClips.Length)];
        SoundsManager.Instance.PlaySource(ricochetSource);
    }

    public void ResetSounds()
    {
        outOfTrackSource.Stop();
        nitroLoopSource.Stop();
        foreach (AudioSource source in sources)
            source.Stop();
        for (int i = 0; i < engineSoundDatas.Count; i++)
        {
            EngineSoundData engineSoundData = engineSoundDatas[i];
            engineSoundData.audioSource.volume = 0.0f;
        }
    }

    void Update()
    {
        if (null == vehiclePhysics || !vehiclePhysics.Initialized)
            return;

        float engineRPM = vehiclePhysics.Engine.RPM;
        float engineThrottle = vehiclePhysics.Engine.Throttle;
        float totalVolume = 0.0f;
        bool isOnGround = vehiclePhysics.IsOnGround;
        for (int i = 0; i < engineSoundDatas.Count; i++)
        {
            EngineSoundData engineSoundData = engineSoundDatas[i];
            float volume = 1.0f;

            if (engineRPM < engineSoundData.minRPM)
                volume = 0.0f;
            else if (engineRPM < engineSoundData.fullGainMinRPM && engineSoundData.fullGainMinRPM > engineSoundData.minRPM)
                volume *= (engineRPM - engineSoundData.minRPM) / (engineSoundData.fullGainMinRPM - engineSoundData.minRPM);

            if (engineRPM > engineSoundData.maxRPM)
                volume = 0.0f;
            else if (engineRPM > engineSoundData.fullGainMaxRPM && engineSoundData.fullGainMaxRPM < engineSoundData.maxRPM)
                volume *= 1.0f - (engineRPM - engineSoundData.fullGainMaxRPM) / (engineSoundData.maxRPM - engineSoundData.fullGainMaxRPM);

            if (engineSoundData.power == EngineSoundData.PowerType.BOTH)
                volume *= (1.0f + vehiclePhysics.EngineNitroPower * 1.5f) * engineThrottle * 0.55f + 0.36f;
            else if (engineSoundData.power == EngineSoundData.PowerType.ON)
                volume *= engineThrottle;
            else if (engineSoundData.power == EngineSoundData.PowerType.OFF)
                volume *= (1.0f - engineThrottle);

            totalVolume += volume;

            float pitch = engineRPM / engineSoundData.sampleRPM;

            engineSoundData.audioSource.pitch = pitch + 0.25f;
            //engineSoundData.audioSource.volume = volume;
            engineSoundData.tmpVolume = volume;
        }

        for (int i = 0; i < engineSoundDatas.Count; i++)
        {
            EngineSoundData engineSoundData = engineSoundDatas[i];
            if (totalVolume == 0.0f)
                engineSoundData.audioSource.volume = 0.0f;
            else if (engineSoundDatas.Count == 1 && engineSoundData.power == EngineSoundData.PowerType.BOTH)
                engineSoundData.audioSource.volume = engineSoundData.tmpVolume * 0.6f;// * 0.2f;
            else
                engineSoundData.audioSource.volume = engineSoundData.tmpVolume / totalVolume * 0.6f;// *0.2f;
            //engineSoundData.audioSource.volume *= SoundsManager.Instance.VolumeScaling;
        }

        bool outOfTrack = false;
        for (int i = 0; i < (int)VehicleUtils.WheelPos.NUM_WHEELS; i++)
        {
            float slide = vehiclePhysics.GetTyreSlide(i);
            float maxVolume = 0.7f;//2.5f;//0.85f;
            float pitchDelta = 0.025f;

            AudioSource audioSource = sources[i];

            ContactData contactData = vehiclePhysics.GetWheelContactProperties(i);
            ContactData.SurfaceType surfType = (null == contactData ? ContactData.SurfaceType.None : contactData.TypeOfSurface);
            outOfTrack |= (ContactData.SurfaceType.Grass == surfType);

            AudioClip clip = slideClips[Mathf.Clamp((int)surfType, 0, slideClips.Length - 1)];
            if (clip != audioSource.clip)
            {
                audioSource.clip = clip;
                SoundsManager.Instance.PlaySource(audioSource);
            }

            Vector3 wheelVel = vehiclePhysics.GetWheelVelocity(i);
            audioSource.volume = isOnGround ? slide * maxVolume : 0.0f;
            //audioSource.volume *= SoundsManager.Instance.VolumeScaling;
            float pitch = 1.0f - Mathf.Clamp01((wheelVel.magnitude - 5.0f) * 0.1f);
            pitch *= pitchDelta;
            pitch = pitch + (1.0f - pitchDelta) - 0.28f;
            pitch = Mathf.Clamp(pitch, 0.1f, 3.0f);
            audioSource.pitch = pitch;
        }

        float speed = vehiclePhysics.Speed;

        if (outOfTrack && speed >= 1.0f && !outOfTrackSource.isPlaying)
            SoundsManager.Instance.PlaySource(outOfTrackSource);

        if ((!outOfTrack || speed < 1.0f) && outOfTrackSource.isPlaying)
            outOfTrackSource.Stop();

        if (vehiclePhysics.EngineNitroPower > 0.0f && speed >= 1.0f && !nitroLoopSource.isPlaying)
        {
            SoundsManager.Instance.PlaySource(nitroStartSource);
            SoundsManager.Instance.PlaySource(nitroLoopSource);
            //nitroStartSource.Play();
            //nitroLoopSource.Play();
        }

        if ((vehiclePhysics.EngineNitroPower <= 0.0f || speed < 1.0f) && nitroLoopSource.isPlaying)
            nitroLoopSource.Stop();
    }
}
