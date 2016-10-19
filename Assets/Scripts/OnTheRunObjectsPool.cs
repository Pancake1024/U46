using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

public class OnTheRunObjectsPool : MonoBehaviour
{
    #region Singleton instance

    protected static OnTheRunObjectsPool instance = null;

    public static OnTheRunObjectsPool Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    [Serializable]
    public class PoolData
    {
        public ObjectType objType;
        public List<GameObject> obj_Prefab;
        public int quantity;
    }

    public enum ObjectType
    {
        None = -1,
        BonusTurbo = 0,
        BonusTime,
        BonusMoney,
        BonusMagnet,
        BonusShield,
        TrafficVehicle,
        Explosion,
        Smoke,
        Sparks,
        Checkpoint,
        RoadWork,
        TruckNormal,
        TruckTurbo,
        TruckTransform,
        BonusMoneyBig,
        BlackCar,
        BlackVan,
        BlackPickup,
        Helicopter,
        HelicopterExplosion,
        ElectricBomb,
        FrozenBlast,
        HellBurst,
        NoGravBeam,
        Mine,
        FrontCollision,
        SideCollision,
        PickupCoins,
        PickupMagnet,
        PickupShield,
        PickupBolt,
        AsphaltCrack,
        TankExplosion,
        Police,
        SparksPlane,
        RoadBlock,
        StartGuardRail,
        CentralGuardRail,
        EndGuardRail,
        WoodHitFx,
        CarExplosion,
        Count
    }

    public bool IsATruck(ObjectType checkType)
    {
        return checkType == ObjectType.TruckNormal || checkType == ObjectType.TruckTurbo || checkType == ObjectType.TruckTransform;
    }

    public bool IsABonus(ObjectType checkType)
    {
        return checkType == ObjectType.BonusTurbo || checkType == ObjectType.BonusTime || checkType == ObjectType.BonusMoney || checkType == ObjectType.BonusMoneyBig || checkType == ObjectType.BonusMagnet || checkType == ObjectType.BonusShield;
    }

    #region Public Members
    public List<PoolData> poolList;
    #endregion

    #region Protected Members
    Dictionary<ObjectType, Pool> poolsByType;
    Dictionary<ObjectType, int> instancesCountersByType;
    protected List<GameObject> additionalObjects;
    protected List<ObjectType> additionalObjectsType;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        /*poolsByType = new Dictionary<ObjectType, Pool>();
        instancesCountersByType = new Dictionary<ObjectType, int>();

        for (int i = 0; i < poolList.Count; ++i)
            InitializePool(poolList[i].quantity, poolList[i].objType);*/
        
    }
    #endregion

    #region Functions
    void FillPools( )
    {
        poolsByType = new Dictionary<ObjectType, Pool>();
        instancesCountersByType = new Dictionary<ObjectType, int>();

        for (int i = 0; i < poolList.Count; ++i)
        {
            InitializePool(poolList[i].quantity, poolList[i].objType);
        }
    }

    public void DestroyPoolElements()
    {
        for (int i = 0; i < additionalObjects.Count; ++i)
        {
            Destroy(additionalObjects[i]);
            additionalObjects[i] = null;
        }
        additionalObjects.Clear();

        foreach (KeyValuePair<ObjectType, Pool> entry in poolsByType)
        {
            // do something with entry.Value or entry.Key
            List<Pool.PoolObject> list = entry.Value.pool;
            foreach (Pool.PoolObject obj in list)
                Destroy(obj.Object);
            list.Clear();
        }
    }

    void ResetPool()
    {
        for (int i = 0; i < additionalObjects.Count; ++i)
        {
            Destroy(additionalObjects[i]);
            additionalObjects[i] = null;
        }
        additionalObjects.Clear();

        for (int i = poolList.Count-1; i >= 0; --i)
        {
            if (additionalObjectsType.Contains(poolList[i].objType))
            {
                poolList.RemoveAt(i);
            }
        }
        
        //poolList.Clear();
    }

    void InitializePool(int poolDim, ObjectType type)
    {
        Pool pool = CreatePool(poolDim, type);
        instancesCountersByType.Add(type, 0);

        if (pool != null)
        {
            pool = InstantiateObjects(type, pool, poolDim);
        }

        poolsByType.Add(type, pool);
    }

    Pool CreatePool(int poolDim, ObjectType type)
    {
        Pool pool;

        if (poolDim <= 0 || !IsObjectPrefabAvailable(type))
            pool = null;
        else
            pool = new Pool(poolDim);

        return pool;
    }
    

    Pool InstantiateObjects(ObjectType type, Pool pool, int poolDim)
    {
        int prefabsNumer = ObjectTypePrefabNumber(type);
        for (int i = 0; i < prefabsNumer;++i)
        {
            for (int j = 0; j < poolDim; j++)
            {
                GameObject newObject = (GameObject)GameObject.Instantiate(ObjectTypeToPrefab(type, i), Vector3.zero, Quaternion.identity);
                newObject.name = type.ToString() + "_" + instancesCountersByType[type] + "_" + i;
                if (type == ObjectType.Explosion || type == ObjectType.Smoke || type == ObjectType.Sparks || type == ObjectType.HelicopterExplosion || type == ObjectType.TankExplosion || type == ObjectType.CarExplosion)
                {
                    EffectBehaviour effectBehaviourComponent = newObject.AddComponent<EffectBehaviour>();
                    effectBehaviourComponent.effectType = type;
                }
                DontDestroyOnLoad(newObject);
                newObject.SetActiveRecursively(false);
                instancesCountersByType[type] = instancesCountersByType[type] + 1;
                pool.AddObject(newObject);

                if (additionalObjectsType.Contains(type))
                {
                    additionalObjects.Add(newObject);
                }
            }
        }

        return pool;
    }

    int ObjectTypePrefabNumber(ObjectType type)
    {
        int counter = 0;
        for (int i = 0; i < poolList.Count; ++i)
        {
            if (poolList[i].objType == type)
            {
                counter = poolList[i].obj_Prefab.Count;
                break;
            }
        }

        return counter;
    }

    bool IsObjectPrefabAvailable(ObjectType type)
    {
        return 0 != ObjectTypePrefabNumber(type);
    }

    GameObject ObjectTypeToPrefab(ObjectType type, int index)
    {
        GameObject retValue = null;
        for (int i = 0; i < poolList.Count; ++i)
        {
            if (poolList[i].objType == type)
            {
                retValue = poolList[i].obj_Prefab[index];
                break;
            }
        }

        return retValue; 
    }

    GameObject GetUnusedObject(ObjectType type)
    {
        Pool pool;
        if (poolsByType.TryGetValue(type, out pool))
        {
            GameObject freeObjectFromPool = GetUnusedObjectFromPool(pool);
            if (freeObjectFromPool == null)
            {
                Debug.Log("********************** POOL LIMIT REACHED FOR: " + type +". INCREASING POOL SIZE...");
                int prefabIndex = 0;
                if (type == ObjectType.TrafficVehicle)
                    prefabIndex = UnityEngine.Random.Range(0, ObjectTypePrefabNumber(type));
                freeObjectFromPool = (GameObject)GameObject.Instantiate(ObjectTypeToPrefab(type, prefabIndex), Vector3.zero, Quaternion.identity);
                freeObjectFromPool.name = type.ToString() + "_" + instancesCountersByType[type] + "_" + prefabIndex;
                if (type == ObjectType.Explosion || type == ObjectType.Smoke || type == ObjectType.Sparks || type == ObjectType.HelicopterExplosion || type == ObjectType.TankExplosion || type == ObjectType.CarExplosion)
                {
                    EffectBehaviour effectBehaviourComponent = freeObjectFromPool.AddComponent<EffectBehaviour>();
                    effectBehaviourComponent.effectType = type;
                }
                DontDestroyOnLoad(freeObjectFromPool);
                freeObjectFromPool.SetActiveRecursively(false);
                //currObj.isFree = false;
                instancesCountersByType[type] = instancesCountersByType[type] + 1;
                pool.AddObject(freeObjectFromPool);
                pool.SetUsedObject(freeObjectFromPool);
            }
            return freeObjectFromPool;
        }
        else
            return null;
    }

    GameObject GetUnusedObjectFromPool(Pool pool)
    {
        if (pool == null)
            return null;
        else
            return pool.GetFreeObject();
    }
    #endregion


    #region Exposed Functions
    public GameObject RequestObject(ObjectType type)
    {
        GameObject requestObj = GetUnusedObject(type);
        requestObj.SetActiveRecursively(true);
        return requestObj;
    }

    public void RequestEffect(Transform spawnTransform, Vector3 offset, ObjectType type, bool parentToTransform)
    {
        GameObject expl = GetUnusedObject(type);
        if (expl == null)
            return;

        if (type == ObjectType.Explosion)
            offset += Vector3.up * 0.5f;
        else if (type == ObjectType.Smoke)
            offset += Vector3.up * 1.0f;
        else if (type == ObjectType.TankExplosion)
            offset += Vector3.up * 0.1f;

        expl.SetActiveRecursively(true);
        expl.SendMessage("Spawn", new EffectBehaviour.EffectSpawnParameters(spawnTransform, offset, parentToTransform));
    }

    public void RequestEffectInPoint(Vector3 point, Vector3 offset, ObjectType type)
    {
        GameObject expl = GetUnusedObject(type);
        if (expl == null)
            return;

        expl.SetActiveRecursively(true);
        expl.SendMessage("SpawnInPoint", point);
    }

    public void EffectFinshed(GameObject effectGO)
    {
        ObjectType effectType = effectGO.GetComponent<EffectBehaviour>().effectType;
        NotifyDestroyingParent(effectGO, effectType);
    }
    
    public void NotifyDestroyingExplosion(GameObject parent=null)
    {
        if (parent == null)
        {
            for (int i = (int)ObjectType.Explosion; i <= (int)ObjectType.Sparks; i++)
            {
                Pool pool = poolsByType[(ObjectType)i];

                foreach (Pool.PoolObject po in pool.pool)
                    if (!po.isFree)
                    {
                        pool.SetFreeObject(po.Object);
                        po.Object.SetActiveRecursively(false);
                    }
            }
        }
        else
        {
            List<GameObject> toRemove = new List<GameObject>();
            List<OnTheRunObjectsPool.ObjectType> toRemoveType = new List<OnTheRunObjectsPool.ObjectType>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject currObject = parent.transform.GetChild(i).gameObject;
                OnTheRunObjectsPool.ObjectType type = ObjectType.Count;
                if (currObject.name.Contains("Explosion"))
                    type = ObjectType.Explosion;
                else if (currObject.name.Contains("Smoke"))
                    type = ObjectType.Smoke;
                if (currObject.name.Contains("CarExplosion"))
                    type = ObjectType.CarExplosion;

                if (type != ObjectType.Count)
                {
                    toRemoveType.Add(type);
                    toRemove.Add(currObject);
                }
            }

            //remove
            for (int i = 0; i < toRemove.Count; i++)
            {
                Pool pool;
                if (poolsByType.TryGetValue(toRemoveType[i], out pool))
                {
                    pool.SetFreeObject(toRemove[i]);
                    toRemove[i].SetActiveRecursively(false);
                }
            }
        }

    }

    public void NotifyDestroyingParent(GameObject currObject, ObjectType type )
    {
        if (type == ObjectType.TrafficVehicle || type == ObjectType.Police || type == ObjectType.Helicopter || IsATruck(type)) //|| type == ObjectType.BlackVan || type == ObjectType.BlackPickup 
        {
            if (currObject.GetComponent<OpponentKinematics>()!=null)
                currObject.GetComponent<OpponentKinematics>().SendMessage("OnResetVehicle", SendMessageOptions.DontRequireReceiver);
        }

        if (IsATruck(type))
            gameObject.SendMessage("TruckRemoved");


        Pool pool;
        if (poolsByType.TryGetValue(type, out pool))
        {
            pool.SetFreeObject(currObject);
            currObject.SetActiveRecursively(false);
        }
        else
            Debug.LogError("ERROR - NO OBJECTS FOUND IN POOL");

    }


    public void AddNewItems(List<PoolData> additionalPoolList)
    {
        if (additionalObjects!=null)
            ResetPool();

        additionalObjects = new List<GameObject>();
        additionalObjectsType = new List<ObjectType>();

        for (int i = 0; i < additionalPoolList.Count; ++i)
        {
            if (!additionalObjectsType.Contains(additionalPoolList[i].objType))
                additionalObjectsType.Add(additionalPoolList[i].objType);
            poolList.Add(additionalPoolList[i]);
        }

        FillPools();
        
        /*for (int i = 0; i < additionalPoolList.Count; ++i)
            InitializePool(additionalPoolList[i].quantity, additionalPoolList[i].objType);*/
    }
    #endregion
}
