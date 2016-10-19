using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections;

public class OnTheRunFireworks : Manager<OnTheRunFireworks>
{
    public GameObject fireworksPrefab;

    protected OnTheRunInterfaceSounds interfaceSounds;

    protected Vector3[] fireworksPlaceHolders;
    protected ParticleSystem[] fireworks;
    protected List<ParticleSystem> fireworksToPlay;
    protected float fireworksTimer;
    protected int fireworksPlaceHoldersCount = 0;

    #region Singleton instance
    public static OnTheRunFireworks Instance
    {
        get
        {
            return Manager<OnTheRunFireworks>.Get();
        }
    }
    #endregion

    void Start()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void Update()
    {
        UpdateFireworks();
    }

    void InitFireworks(Transform placeholders)
    {
        Transform fireworksPlaceHoldersTr = placeholders;// transform.FindChild("content/fireworks");
        fireworksPlaceHoldersCount = fireworksPlaceHoldersTr.childCount;
        fireworksPlaceHolders = new Vector3[fireworksPlaceHoldersCount];
        fireworks = new ParticleSystem[fireworksPlaceHoldersCount];
        for (int i = 0; i < fireworksPlaceHoldersCount; ++i)
        {
            Transform placeHolder = fireworksPlaceHoldersTr.GetChild(i);
            fireworksPlaceHolders[i] = placeHolder.position;
            GameObject fireworksParticleSystemGo = GameObject.Instantiate(fireworksPrefab, fireworksPlaceHolders[i], Quaternion.identity) as GameObject;
            fireworksParticleSystemGo.transform.parent = placeHolder; //transform;
            fireworks[i] = fireworksParticleSystemGo.GetComponent<ParticleSystem>();
        }
        fireworksTimer = -1.0f;
    }

    public void StartFireworksEffect(int fireworksCount, Transform placeholders)
    {
        ClearFireworks();

        interfaceSounds.PlayFireworkSound();

        InitFireworks(placeholders);

        fireworksToPlay = new List<ParticleSystem>();

        List<int> idxs = new List<int>();
        for (int i = 0; i < fireworksPlaceHoldersCount; ++i)
            idxs.Add(i);

        for (int i = 0; i < fireworksCount; ++i)
        {
            int id = UnityEngine.Random.Range(0, idxs.Count);
            int fireworksId = idxs[id];
            idxs.RemoveAt(id);
            fireworksToPlay.Add(fireworks[fireworksId]);
        }
        fireworksTimer = TimeManager.Instance.MasterSource.TotalTime + 0.5f; // +0.75f;
    }

    public void ClearFireworks()
    {
        fireworksTimer = -1.0f;
        for (int i = 0; i < fireworksPlaceHoldersCount; ++i)
        {
            fireworks[i].Stop();
            fireworks[i].Clear();
        }
    }

    void UpdateFireworks()
    {
        if (fireworksTimer > 0.0f)
        {
            if (TimeManager.Instance.MasterSource.TotalTime >= fireworksTimer)
            {
                fireworksToPlay[0].startSize = UnityEngine.Random.Range(1.75f, 2.75f);
                fireworksToPlay[0].Play();
                fireworksToPlay.RemoveAt(0);

                if (fireworksToPlay.Count > 0)
                {
                    fireworksToPlay[0].startSize = UnityEngine.Random.Range(1.5f, 2.5f);
                    fireworksToPlay[0].Play();
                    fireworksToPlay.RemoveAt(0);
                }

                if (fireworksToPlay.Count > 0)
                {
                    fireworksToPlay[0].startSize = UnityEngine.Random.Range(1.75f, 2.75f);
                    fireworksToPlay[0].Play();
                    fireworksToPlay.RemoveAt(0);
                }
                fireworksTimer = fireworksToPlay.Count == 0 ? -1.0f : TimeManager.Instance.MasterSource.TotalTime + UnityEngine.Random.Range(0.1f, 0.2f);
            }
        }
    }
}