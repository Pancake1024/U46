using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AutoPrefabProcessing : UnityEditor.AssetModificationProcessor
{
    static string projectPath = string.Empty;
    static string scenePath = string.Empty;
    static string sceneName = string.Empty;
    static string autoPrefabPath = string.Empty;

    public static string[] OnWillSaveAssets(string[] paths)
    {
        projectPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf('/') + 1);

        foreach (string path in paths)
        {
            if (path.Contains(".unity"))
            {
                scenePath = Path.GetDirectoryName(path);
                sceneName = Path.GetFileNameWithoutExtension(path);

                autoPrefabPath = scenePath + "/" + sceneName + "_autoPrefabs";
            }
        }
            
        if (sceneName.Length == 0)
            return paths;

        string destPathFromSettings = AutoPrefabSettings.GetDestinationPath();
        if (destPathFromSettings != null)
            autoPrefabPath = destPathFromSettings;

        SavePrefabs();

        return paths;
    }

    static void SavePrefabs()
    {
        bool needsToCreateNewFolder = false;

        string absoluteAutoPrefabPath = projectPath + autoPrefabPath;
        if (!Directory.Exists(absoluteAutoPrefabPath))
            needsToCreateNewFolder = true;

        EditorUtility.DisplayProgressBar("Exporting AutoPrefabs", "Exporting...", 0.0f);

        List<GameObject> autoPrefabs = AutoPrefab.GetAutoPrefabGameObjects();
        for (int i = autoPrefabs.Count - 1; i >= 0; --i)
        {
            Object prefab = PrefabUtility.GetPrefabParent(autoPrefabs[i]);
            if (prefab != null)
                autoPrefabs.RemoveAt(i);
        }

        float progress = 0.0f, step = 1.0f / autoPrefabs.Count;
        foreach (GameObject go in autoPrefabs)
        {
            if (needsToCreateNewFolder)
            {
                Directory.CreateDirectory(absoluteAutoPrefabPath);
                needsToCreateNewFolder = false;
            }

            string info = "Exporting " + go.name + "...";
            Debug.Log("AutoPrefab: " + info);

            EditorUtility.DisplayProgressBar("Exporting AutoPrefabs", info, progress);
            PrefabUtility.CreatePrefab(autoPrefabPath + "/" + go.name + ".prefab", go, ReplacePrefabOptions.Default);
            progress += step;
            EditorUtility.DisplayProgressBar("Exporting AutoPrefabs", info, progress);
        }

        EditorUtility.ClearProgressBar();
    }
}
