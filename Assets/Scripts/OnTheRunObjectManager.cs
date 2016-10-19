using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;
using SBS.Core;


public class OnTheRunObjectManager : MonoBehaviour
{


    public AnimatedObstacle[] animatedObstacles;

    [Serializable]
    public class AnimatedObstacle
    {
        public OnTheRunObject.ObjectType type = OnTheRunObject.ObjectType.Void;
        public GameObject prefab = null;
        public int cacheSize = 3;

        protected List<GameObject> cache = null;
#if UNITY_FLASH
        protected AS3Stack<int> freeIndices = null;
#else
        protected Stack<int> freeIndices = null;
#endif

        public void InitCache()
        {
            cache = new List<GameObject>();
#if UNITY_FLASH
            freeIndices = new AS3Stack<int>();
#else
            freeIndices = new Stack<int>();
#endif
            for (int i = 0; i < cacheSize; ++i)
            {
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
                go.SetActiveRecursively(false);

                cache.Add(go);
                freeIndices.Push(i);
            }
        }

        public GameObject GetInstance()
        {
            Asserts.Assert(freeIndices.Count > 0);
            int index = freeIndices.Pop();
            GameObject go = cache[index];
            go.SetActiveRecursively(true);
            return go;
        }

        public void FreeInstance(GameObject go)
        {
            go.SetActiveRecursively(false);
            int index = cache.IndexOf(go);
            Asserts.Assert(index >= 0);
            freeIndices.Push(index);
        }
    }


    void Start()
    {
        foreach (AnimatedObstacle animatedObs in animatedObstacles)
            animatedObs.InitCache();
    }

    void FixedUpdate()
    {


    }


    public void FreeAnimatedObstacle(OnTheRunObject.ObjectType type, GameObject instance)
    {
        foreach (AnimatedObstacle animatedObs in animatedObstacles)
        {
            if (animatedObs.type == type)
            {
                animatedObs.FreeInstance(instance);
                return;
            }
        }
        Asserts.Assert(false, "No animated obstacle of type " + type);
    }



    public void PlaceAnimatedObstacle(OnTheRunObject obj)
    {
        foreach (AnimatedObstacle animatedObs in animatedObstacles)
        {
            if (animatedObs.type == obj.type)
            {
                obj.animatedInstance = animatedObs.GetInstance();
                obj.animatedInstance.transform.position = obj.transform.position;
                obj.animatedInstance.transform.rotation = obj.transform.rotation;
                obj.animationStarted = true;
                break;
            }
        }
    }


    void Update()
    {


    }
}
