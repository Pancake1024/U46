using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

namespace SBS.Pool
{
    public struct Pool<T>
        where T : IntrusiveListNode<T>, IPoolObject<T>
	{
        private static int minimumGrowth = 8;
        private static float growFactor = 2.0f;

        public static int MinimumGrowth
        {
            get
            {
                return minimumGrowth;
            }
            set
            {
                minimumGrowth = Mathf.Max(growFactor > 1.0f ? 0 : 1, value);
            }
        }

        public static float GrowFactor
        {
            get
            {
                return growFactor;
            }
            set
            {
                growFactor = Mathf.Max(1.0f, value);
            }
        }

        private T original;
        private uint nextInstanceID;
        private IntrusiveList<T> freeList;
        private IntrusiveList<T> usedList;

        public bool IsNull
        {
            get
            {
                return null == original;
            }
        }

        public T Original
        {
            get
            {
                return original;
            }
        }

        public int Capacity
        {
            get
            {
                return freeList.Count + usedList.Count;
            }
            set
            {
                int oldCapacity = this.Capacity;
                if (value > oldCapacity)
                {
                    for (int i = oldCapacity; i < value; ++i)
                        this.NewInstance();
                }
                else if (value < oldCapacity)
                {
                    int delCount = Mathf.Min(freeList.Count, oldCapacity - value);
                    for (int i = 0; i < delCount; ++i)
                        UnityEngine.Object.Destroy(freeList.Tail.gameObject);
                }
            }
        }

        public IntrusiveList<T> FreeList
        {
            get
            {
                return freeList;
            }
        }

        public IntrusiveList<T> UsedList
        {
            get
            {
                return usedList;
            }
        }

        public Pool(T source, bool isAPrefab)
        {
            original = source;
            nextInstanceID = 0;

            freeList = new IntrusiveList<T>();
            usedList = new IntrusiveList<T>();

            if (!isAPrefab)
            {
                freeList.Add(original);

                original.Pool = this;
                original.InstanceID = nextInstanceID++;
                original.IsUsed = false;
                original.OnInstantiation();
                original.gameObject.SetActiveRecursively(false);
            }
        }

        public Pool(T source, bool isAPrefab, int startCapacity)
            : this(source, isAPrefab)
        {
            for (int i = this.Capacity; i < startCapacity; ++i)
                this.NewInstance();
        }

        private void NewInstance()
        {
            T instance = (T)UnityEngine.Object.Instantiate(original);
            freeList.Add(instance);

            instance.Pool = this;
            instance.InstanceID = nextInstanceID++;
            instance.IsUsed = false;
            instance.OnInstantiation();
            instance.gameObject.SetActiveRecursively(false);
        }

        private void Grow()
        {
            int oldCapacity = freeList.Count + usedList.Count,
                newCapacity = Mathf.RoundToInt(oldCapacity * growFactor) + minimumGrowth;
            for (int i = oldCapacity; i < newCapacity; ++i)
                this.NewInstance();
        }

        public T Get()
        {
            if (0 == freeList.Count)
                this.Grow();

            T instance = freeList.Tail;

            freeList.Remove(instance);
            usedList.Add(instance);

            instance.gameObject.SetActiveRecursively(true);
            instance.OnRequested();
            instance.IsUsed = true;

            return instance;
        }

        public void Free(T instance)
        {
            instance.IsUsed = false;
            instance.OnFreed();
            instance.gameObject.SetActiveRecursively(false);

            usedList.Remove(instance);
            freeList.Add(instance);
        }

        public void OnDestroyed(T instance)
        {
            if (instance.IsUsed)
                usedList.Remove(instance);
            else
                freeList.Remove(instance);
        }

        public void Destroy()
        {
            if (null == original)
                return;

            original = null;

            this.Capacity = 0;

            freeList.Clear();
            usedList.Clear();

            freeList = usedList = null;
        }
    }
}
