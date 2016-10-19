using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("SBS/Core/AutoPrefabSettings")]
public class AutoPrefabSettings : MonoBehaviour
{
    public string destinationPath = string.Empty;
    public bool exportInactiveObjects = true;

#if UNITY_EDITOR
    public static string GetDestinationPath()
    {
        AutoPrefabSettings[] allSettings = Resources.FindObjectsOfTypeAll<AutoPrefabSettings>();
        AutoPrefabSettings settings = null;
        foreach (AutoPrefabSettings item in allSettings)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(item.transform.root.gameObject)))
            {
                if (null == settings)
                    settings = item;
                else
                    Debug.LogError("Please use only one AutoPrefabSettings per scene.", item);
            }
        }

        if (settings != null && !string.IsNullOrEmpty(settings.destinationPath))
        {
            if (0 == settings.destinationPath.IndexOf("Assets"))
                return settings.destinationPath.Replace('\\', '/').TrimEnd('/');
            else
                Debug.LogError("AutoPrefabs destinaton path should always starts with 'Assets'.", settings);
        }

        return null;
    }

    public static bool ExportInactiveObjects()
    {
        AutoPrefabSettings[] allSettings = Resources.FindObjectsOfTypeAll<AutoPrefabSettings>();
        AutoPrefabSettings settings = null;
        foreach (AutoPrefabSettings item in allSettings)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(item.transform.root.gameObject)))
            {
                if (null == settings)
                    settings = item;
                else
                    Debug.LogError("Please use only one AutoPrefabSettings per scene.", item);
            }
        }

        if (null == settings)
            return true;
        else
            return settings.exportInactiveObjects;        
    }

#endif
}
