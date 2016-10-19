using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabTools
{
    [MenuItem("Tools/Revert Selected Prefabs")]
    static public void RevertSelPrefabs()
    {
        GameObject[] sel = Selection.gameObjects;
        if (sel.Length > 0)
        {
            foreach (GameObject go in sel)
            {
                PrefabType prefabType = PrefabUtility.GetPrefabType(go);
                if (PrefabType.PrefabInstance == prefabType || PrefabType.DisconnectedPrefabInstance == prefabType)
                    PrefabUtility.ResetToPrefabState(go);
            }
        }
    }

    [MenuItem("Tools/Apply Selected Prefabs")]
    static public void ApplySelPrefabs()
    {
        GameObject[] sel = Selection.gameObjects;
        if (sel.Length > 0)
        {
            foreach (GameObject go in sel)
            {
                PrefabType prefabType = PrefabUtility.GetPrefabType(go);
                if (PrefabType.PrefabInstance == prefabType || PrefabType.DisconnectedPrefabInstance == prefabType)
                {
                    PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go));
                }
            }

            AssetDatabase.Refresh();
        }
    }

#if UNITY_FLASH
    [MenuItem("Tools/Apply Selected Prefabs (Update PTF data)")]
    static public void ApplySelPrefabsPTF()
    {
        GameObject[] sel = Selection.gameObjects;
        if (sel.Length > 0)
        {
            foreach (GameObject go in sel)
            {
                PrefabType prefabType = PrefabUtility.GetPrefabType(go);
                if (PrefabType.PrefabInstance == prefabType || PrefabType.DisconnectedPrefabInstance == prefabType)
                {
                    ParallelTransportFrame[] ptfs = go.GetComponentsInChildren<ParallelTransportFrame>(true);
                    foreach (ParallelTransportFrame ptf in ptfs)
                    {
                        //ptf.dataKeys = ptf.data.keys;
                        ptf.dataKeys = new float[22];
                        for (int i = 0; i < 11; ++i)
                        {
                            float t = (i * 0.1f);
                            ptf.dataKeys[i * 2 + 0] = t;
                            ptf.dataKeys[i * 2 + 1] = ptf.data.Evaluate(t);
                        }
                    }

                    PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go));
                }
            }

            AssetDatabase.Refresh();
        }
    }
#endif
}
