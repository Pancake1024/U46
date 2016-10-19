using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SBS.Core;

namespace SBS.Pool
{
    public struct PoolCollection<T> : IEnumerable<T>
        where T : IntrusiveListNode<T>, IPoolCollectionObject<T>, IPoolObject<T>
    {
        private uint nextOriginalID;
        private Dictionary<string, uint> idsByName;
        //private Dictionary<uint, Pool<T>> poolsByID; // should be a plain array with a freeIDs list/array
        private Pool<T>[] poolsByID;
        private List<uint> freeIDs;
        //private Dictionary<ulong, T> objectsByKey;

        public bool IsNull
        {
            get
            {
                return null == idsByName;
            }
        }

        private ulong GetObjectKey(uint originalID, uint instanceID)
        {
            return originalID << 32 | instanceID;
        }

        private ulong GetObjectKey(T instance)
        {
            return (instance.OriginalID << 32) | instance.InstanceID;
        }

        private uint GetNextID()
        {
            uint nextID;
            int c = freeIDs.Count;
            if (c > 0)
            {
                --c;
                nextID = freeIDs[c];
                freeIDs.RemoveAt(c);
            }
            else
            {
                nextID = nextOriginalID++;
                if (nextID >= poolsByID.Length)
                    Array.Resize<Pool<T>>(ref poolsByID, poolsByID.Length * 2); // ToDo grow size
            }
            return nextID;
        }

        public void Initialize()
        {
            idsByName = new Dictionary<string, uint>();
            //poolsByID = new Dictionary<uint, Pool<T>>();
            poolsByID = new Pool<T>[16];
            freeIDs = new List<uint>();
            //objectsByKey = new Dictionary<ulong, T>();
        }

        public uint RegisterPrefab(T original, int poolSize)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetPrefabType(original) != PrefabType.Prefab)
            {
                Debug.LogError("PoolCollection: object \"" + original.name + "\" is not a Prefab!", original);
                return uint.MaxValue;
            }

            if (idsByName.ContainsKey(original.name))
            {
                Debug.LogError("PoolCollection: object \"" + original.name + "\" already exist!", original);
                return uint.MaxValue;
            }
#endif
            original.OriginalID = this.GetNextID();//nextOriginalID++;

            idsByName.Add(original.name, original.OriginalID);
            //poolsByID.Add(original.OriginalID, new Pool<T>(original, true, poolSize));
            poolsByID[original.OriginalID] = new Pool<T>(original, true, poolSize);

            return original.OriginalID;
        }

        public uint RegisterPrefab(T original)
        {
            return this.RegisterPrefab(original, 0);
        }

        public uint RegisterInstance(T original, int poolSize)
        {
#if UNITY_EDITOR
            if (PrefabType.Prefab == PrefabUtility.GetPrefabType(original))
            {
                Debug.LogError("PoolCollection: object \"" + original.name + "\" is not an instance!", original);
                return uint.MaxValue;
            }

            if (idsByName.ContainsKey(original.name))
            {
                Debug.LogError("PoolCollection: object \"" + original.name + "\" already exist!", original);
                return uint.MaxValue;
            }
#endif
            original.OriginalID = this.GetNextID();//nextOriginalID++;

            idsByName.Add(original.name, original.OriginalID);
            //poolsByID.Add(original.OriginalID, new Pool<T>(original, false, poolSize));
            poolsByID[original.OriginalID] = new Pool<T>(original, false, poolSize);

            return original.OriginalID;
        }

        public uint RegisterInstance(T original)
        {
            return this.RegisterInstance(original, 0);
        }

        public void Unregister(uint originalID)
        {
            if (originalID >= poolsByID.Length)
                return;
            Pool<T> pool = poolsByID[originalID];
            if (!pool.IsNull)//poolsByID.TryGetValue(originalID, out pool))
            {
                // log alive objects?

                //poolsByID.Remove(originalID);
                poolsByID[originalID].Destroy();
                freeIDs.Add(originalID);

                string originalName = pool.Original.name;
                idsByName.Remove(pool.Original.name);

                // unload pool.Original if prefab & resource, destroy if not prefab?
            }
        }

        public uint GetOriginalID(string originalName)
        {
            uint id;
            if (idsByName.TryGetValue(originalName, out id))
                return id;
            return uint.MaxValue;
        }

        public uint GetOriginalID(T original)
        {
            return original.OriginalID;
        }

        public IntrusiveList<T> GetActiveList(uint originalID)
        {
            if (originalID >= poolsByID.Length)
                return null;
            Pool<T> pool = poolsByID[originalID];
            if (!pool.IsNull)//poolsByID.TryGetValue(originalID, out pool))
                return pool.UsedList;
            return null;
        }

        public IntrusiveList<T> GetActiveList(T original)
        {
            return this.GetActiveList(this.GetOriginalID(original));
        }

        public IntrusiveList<T> GetActiveList(string originalName)
        {
            return this.GetActiveList(this.GetOriginalID(originalName));
        }

        public IntrusiveList<T> GetInactiveList(uint originalID)
        {
            if (originalID >= poolsByID.Length)
                return null;
            Pool<T> pool = poolsByID[originalID];
            if (!pool.IsNull)//poolsByID.TryGetValue(originalID, out pool))
                return pool.FreeList;
            return null;
        }

        public IntrusiveList<T> GetInactiveList(T original)
        {
            return this.GetInactiveList(this.GetOriginalID(original));
        }

        public IntrusiveList<T> GetInactiveList(string originalName)
        {
            return this.GetInactiveList(this.GetOriginalID(originalName));
        }
/*
        public T GetObject(uint originalID, uint instanceID)
        {
            T instance;
            if (objectsByKey.TryGetValue(this.GetObjectKey(originalID, instanceID), out instance))
                return instance;
            return null;
        }
*/
        public T Instantiate(uint originalID)
        {
            if (originalID >= poolsByID.Length)
                return null;
            Pool<T> pool = poolsByID[originalID];
            if (!pool.IsNull)//poolsByID.TryGetValue(originalID, out pool))
            {
                T instance = pool.Get();

                instance.OriginalID = originalID;
                instance.PoolCollection = this;

                //objectsByKey.Add(this.GetObjectKey(instance), instance);

                return instance;
            }
            return null;
        }

        public T Instantiate(T original)
        {
            return this.Instantiate(this.GetOriginalID(original));
        }

        public T Instantiate(string originalName)
        {
            return this.Instantiate(this.GetOriginalID(originalName));
        }

        public bool Free(T instance)
        {
            //Pool<T> pool;
            if (!instance.Pool.IsNull)//poolsByID.TryGetValue(instance.OriginalID, out pool))
            {
                //pool.Free(instance);
                instance.Pool.Free(instance);

                //objectsByKey.Remove(this.GetObjectKey(instance));

                return true;
            }
            return false;
        }

        public void OnDestroyed(T instance)
        {/*
            if (objectsByKey != null)
                objectsByKey.Remove(this.GetObjectKey(instance));*/
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator() { id = 0, pools = poolsByID, node = null };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            internal uint id;
            //internal Dictionary<uint, Pool<T>> pools;
            internal Pool<T>[] pools;
            internal T node;

            public T Current
            {
                get
                {
                    return node;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return node;
                }
            }

            public bool MoveNext()
            {
                // TODO ALEX
                if (null == node)
                {
                    Pool<T> pool = pools[id];
                    if (!pool.IsNull)//pools.TryGetValue(id, out pool))
                        node = pool.UsedList.Head;

                    if (null == node)
                    {
                        ++id;
                        if (id >= pools.Length)
                            return false;
                        else
                            return this.MoveNext();
                    }

                    return (node != null);
                }
                else
                {
                    node = node.next;
                    if (null == node)
                    {
                        ++id;
                        return this.MoveNext();
                    }
                    return (node != null);
                }
            }

            public void Reset()
            {
                id = 0;
                pools = null;
                node = null;
            }

            public void Dispose()
            {
                pools = null;
                node = null;
            }
        }
    }
}
