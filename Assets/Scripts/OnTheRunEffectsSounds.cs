using UnityEngine;
using System.Collections;
using SBS.Core;

public class OnTheRunEffectsSounds : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunEffectsSounds instance = null;

    public static OnTheRunEffectsSounds Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public Classes
    public class FxSoundSource
    {
        public GameObject go;
        public AudioSource s;
        public FxSoundSource()
        {
            go = new GameObject();
            s = go.AddComponent<AudioSource>();
            s.loop = false;
            s.volume = 10.0f;
            s.minDistance = 25.0f;
            s.maxDistance = 150.0f;
            s.playOnAwake = false;
        }
    }
    #endregion
    
    #region Public Members
    public AudioClip[] Explosions;
    #endregion

    #region Protected Members
    protected int fxSourcesNum = 8;
    protected FxSoundSource[] fxSources;
    #endregion

    #region Functions
    void InitFxSources()
    { 
        fxSources = new FxSoundSource[fxSourcesNum];
        for (int i = 0; i < fxSourcesNum; i++)
        {
            fxSources[i] = new FxSoundSource();
            fxSources[i].go.name = "FxSoundSource_" + i.ToString();
        }
    }

    public FxSoundSource FreeAudioSource(FxSoundSource[] sourcesGO)
    {
        for (int i = 0; i < sourcesGO.Length; i++)
        {
            if (!sourcesGO[i].s.isPlaying)
            {
                return sourcesGO[i];
            }
        }
        return null;
    }

    #endregion

    #region Messages
    public void PlayFxSoundInPosition(Vector3 pos, OnTheRunObjectsPool.ObjectType type)
    {
        FxSoundSource fx = FreeAudioSource(fxSources);
        if (fx != null)
        {
            switch (type)
            {
                case OnTheRunObjectsPool.ObjectType.Explosion:
                case OnTheRunObjectsPool.ObjectType.TankExplosion:
                    fx.s.clip = Explosions[Random.Range(0, Explosions.Length)];
                    break;
            }
            //fx.go.transform.position = pos;

            OnTheRunSounds.Instance.CheckForMusicDownVolume(fx.s);

            OnTheRunSoundsManager.Instance.PlaySource(fx.s);
            //Debug.Log("@@@@@@ - Playing " + fx.go.name + " SOURCE in position: " + fx.go.transform.position + "e pos vale: "+ pos + " the clip: " + fx.s.clip);
        }
    }

    void StopAllSources()
    {
        foreach (FxSoundSource fx in fxSources)
        {
            fx.s.Stop();
        }
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        InitFxSources();
    }
    #endregion
}
