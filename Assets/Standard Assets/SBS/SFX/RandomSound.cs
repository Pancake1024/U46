using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/SFX/RandomSound")]
public class RandomSound : MonoBehaviour
{
    #region Public members
    public AudioClip[] soundClips;
    public bool playOnAwake = true;
    public float volume = 15.0f;
    public float minDistance = 5.0f;
    public float maxDistance = 25.0f;
    #endregion

    #region Protected members
    protected AudioSource soundSource;
    #endregion

    void Awake()
    {
        soundSource = gameObject.AddComponent<AudioSource>();
        soundSource.playOnAwake = false;
        soundSource.clip = soundClips[0];
        soundSource.loop = false;
        soundSource.volume = volume;
        soundSource.rolloffMode = AudioRolloffMode.Linear;
        soundSource.minDistance = minDistance;
        soundSource.maxDistance = maxDistance;
        soundSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

        if (playOnAwake)
        {
            soundSource.clip = soundClips[UnityEngine.Random.Range(0, soundClips.Length)];
            SoundsManager.Instance.PlaySource(soundSource);
        }
    }
}
