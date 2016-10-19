using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SBS.Core;

[AddComponentMenu("SBS/Core/AutoPrefab")]
public class AutoPrefab : MonoBehaviour
{
#if UNITY_EDITOR
    public static List<GameObject> GetAutoPrefabGameObjects()
    {
        List<GameObject> autoPrefabsInScene = new List<GameObject>();
        HashSet<string> autoPrefabNames = new HashSet<string>();
        bool exportInactiveObjects = AutoPrefabSettings.ExportInactiveObjects();

        foreach (Object obj in Resources.FindObjectsOfTypeAll(typeof(AutoPrefab)))
        {
            GameObject go = (obj as AutoPrefab).gameObject;
            if (go == null || (!go.activeSelf && !exportInactiveObjects))
                continue;

            if (GameObjectIsPrefab(go))
                continue;

            if (GameObjectHasAutoprefabInParent(go))
            {
                Debug.LogWarning("GameObject \"" + go.name + "\" has an AutoPrefab in a parent.", go);
                continue;
            }

            if (autoPrefabNames.Contains(go.name))
            {
                Debug.LogError("GameObject \"" + go.name + "\" is already an AutoPrefab GameObject.", go);
                continue;
            }
           
            autoPrefabsInScene.Add(go);
            autoPrefabNames.Add(go.name);
        }

        return autoPrefabsInScene;
    }

    static bool GameObjectIsPrefab(GameObject go)
    {
        string assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
        return (!string.IsNullOrEmpty(assetPath));
    }

    static bool GameObjectHasAutoprefabInParent(GameObject go)
    {
        Transform parent = go.transform.parent;
        if (parent == null)
            return false;

        bool parentHasAutoprefab = parent.GetComponent<AutoPrefab>() != null;

        return parentHasAutoprefab || GameObjectHasAutoprefabInParent(parent.gameObject);
    }

    [MenuItem("AutoPrefabs/Print list")]
    public static void PrintAutoPrefabsList()
    {
        List<GameObject> objects = GetAutoPrefabGameObjects();
        string str = "AutoPrefabs count: ";
        str += objects.Count;
        str += "\nDestination path: ";
        str += AutoPrefabSettings.GetDestinationPath();
        str += "\n";

        foreach (GameObject obj in objects)
            str += obj.name + "\n";

        Debug.Log(str);
    }

#endif
}