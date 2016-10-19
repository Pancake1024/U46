using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

public class Pool
{
    public class PoolObject
    {
        GameObject gameObject;

        public bool isFree;
        public GameObject Object { get { return gameObject; } }


        public PoolObject(GameObject go)
        {
            gameObject = go;
            isFree = true;
        }
    }

    public List<PoolObject> pool;

    public Pool(int poolDim)
    {
        pool = new List<PoolObject>(poolDim);
    }

    public void AddObject(GameObject go)
    {
        pool.Add(new PoolObject(go));
    }

    public GameObject GetFreeObject()
    {
        int i = Random.Range(0, pool.Count),
            endIndex = 0;
        for (; i < int.MaxValue; ++i)
        {
            if (i >= pool.Count) i = 0;
            PoolObject currObj = pool[i];
            if (currObj.isFree)
            {
                currObj.isFree = false;
                return currObj.Object;
            }

            endIndex++;
            if (endIndex == pool.Count)
                break;
        }

/*
 * old without randomicity: always the firstobject if ratio is low
            foreach (PoolObject obj in pool)
                if (obj.isFree)
                {
                    obj.isFree = false;
                    return obj.Object;
                }
        */
        return null;
    }

    public void SetFreeObject(GameObject freedObject)
    {
        freedObject.transform.parent = null;
        string poolStatus = string.Empty;

        foreach (PoolObject obj in pool)
        {
            if (obj.Object == freedObject)
            {
                obj.isFree = true;
                return;
            }
        }

        Asserts.Assert(false, "Pool.SetFreeObject() - object with name \"" + freedObject.name + "\" not found in this pool. POOL STATUS: " + poolStatus);
    }

    public void SetUsedObject(GameObject freedObject)
    {
        freedObject.transform.parent = null;
        string poolStatus = string.Empty;

        foreach (PoolObject obj in pool)
        {
            if (obj.Object == freedObject)
            {
                obj.isFree = false;
                return;
            }
        }

        Asserts.Assert(false, "Pool.SetUsedObject() - object with name \"" + freedObject.name + "\" not found in this pool. POOL STATUS: " + poolStatus);
    }
}