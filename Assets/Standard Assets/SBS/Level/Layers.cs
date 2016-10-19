using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Level
{
    public class Layers
    {
        #region Private members
        private int uniqueLayerBit = 1;
        private List<string> names = new List<string>(32);
        private int[] maps = new int[32];
        #endregion

        #region Public properties
        public int Count
        {
            get
            {
                return names.Count;
            }
        }

        public string this[int i]
        {
            get
            {
                return names[i];
            }
        }
        #endregion

        #region Public methods
        public Layers()
        {
            for (int i = 0; i < 32; ++i)
                maps[i] = -1;
        }

        public int Register(string name)
        {
            int bit = uniqueLayerBit;
            uniqueLayerBit <<= 1;
            names.Add(name);
            return bit;
        }

        public bool Exist(string name)
        {
            return names.Contains(name);
        }
        #endregion

        #region String <-> Mask
        public int StringToMask(string str)
        {
            string[] layers = str.Split(';');
            int mask = 0;
            foreach (string layer in layers)
                mask |= (1 << names.IndexOf(layer));
            return mask;
        }

        public string MaskToString(int mask)
        {
            string str = string.Empty;
            int bit = 1;
            foreach (string name in names)
            {
                if ((mask & bit) != 0)
                    str += name;
                bit <<= 1;
                if (mask < bit)
                    str += ";";
            }
            return str;
        }
        #endregion

        #region Mask <-> Unity Layer
        public int MaskToUnityLayer(int layerMask)
        {
            for (int i = 0; i < 32; ++i)
            {
                if (maps[i] == layerMask)
                    return i;
            }
            return -1;
        }

        public int UnityLayerToMask(string unityLayerName)
        {
            return this.UnityLayerToMask(LayerMask.NameToLayer(unityLayerName));
        }

        public int UnityLayerToMask(int unityLayerIndex)
        {
            return maps[unityLayerIndex];
        }
        #endregion

        #region Map functions
        public void MapUnityLayer(string unityLayerName, int layersMask)
        {
            this.MapUnityLayer(LayerMask.NameToLayer(unityLayerName), layersMask);
        }

        public void MapUnityLayer(int unityLayerIndex, int layersMask)
        {
            if (0 == layersMask) return;

            if (maps[unityLayerIndex] != layersMask)
            {
                maps[unityLayerIndex] = layersMask;

                if (Application.isEditor)
                {
#if UNITY_4_3
					LevelObject[] allObjs = GameObject.FindObjectsOfType(typeof(LevelObject)) as LevelObject[];
#else
                    LevelObject[] allObjs = GameObject.FindSceneObjectsOfType(typeof(LevelObject)) as LevelObject[];
#endif                    
					foreach (LevelObject obj in allObjs)
                    {
                        if (0 == (obj.LayersMask & layersMask))
                            continue;
                        obj.gameObject.layer = unityLayerIndex;
                    }
                }
                else
                {
                    LevelObject[] objs = LevelRoot.Instance.Query(layersMask);
                    foreach (LevelObject obj in objs)
                        obj.gameObject.layer = unityLayerIndex;
                }
            }
        }
        #endregion
    }
}
