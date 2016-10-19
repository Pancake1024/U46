using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class TangentsChecker
{
    public static List<T> GetSceneComponents<T>()
        where T : Component
    {
        List<T> componentsInScene = new List<T>();
        foreach (T obj in Resources.FindObjectsOfTypeAll<T>())
        {
            GameObject go = obj.gameObject;
            if (go == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
            if (!string.IsNullOrEmpty(assetPath))
                continue;

            componentsInScene.Add(obj);
        }
        return componentsInScene;
    }

    [MenuItem("SBS/CheckReadable")]
    static void CheckReadable()
    {
        var filters = GetSceneComponents<MeshFilter>();

        foreach (var filter in filters)
        {
            if (null == filter.sharedMesh)
                continue;

            List<Mesh> invalidMeshes = new List<Mesh>();
            bool isReadable = filter.sharedMesh.isReadable;
            if (isReadable)
            {
                Debug.Log(filter.name, filter);
            }
        }
    }

    [MenuItem("SBS/CheckTangents")]
    static void CheckTangents()
    {
        var filters = GetSceneComponents<MeshFilter>();

        foreach (var filter in filters)
        {
            if (null == filter.sharedMesh)
                continue;

            List<Mesh> invalidMeshes = new List<Mesh>();
            var tangents = filter.sharedMesh.tangents;
            if (tangents != null && tangents.Length > 0)
            {
                //Debug.Log(filter.name, filter);
                var rndr = filter.gameObject.GetComponent<MeshRenderer>();
                if (null == rndr)
                    continue;

                bool needsTangents = true;
                var materials = rndr.sharedMaterials;
                foreach (var mat in materials)
                {
                    if (null == mat || null == mat.shader)
                        continue;

                    string shaderName = mat.shader.name;
                    if (shaderName.IndexOf("Bump") > -1 || shaderName.IndexOf("Kajiya") > -1)
                        continue;

                    needsTangents = false;
                    break;
                }

                if (!needsTangents)
                    invalidMeshes.Add(filter.sharedMesh);
            }

            List<string> assetPaths = new List<string>();
            HashSet<UnityEngine.Object> mainAssets = new HashSet<UnityEngine.Object>();
            foreach (var mesh in invalidMeshes)
            {
                string assetPath = AssetDatabase.GetAssetPath(mesh);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetPath) || mainAssets.Contains(mainAsset))
                    continue;
                assetPaths.Add(assetPath);
                mainAssets.Add(mainAsset);
            }

            foreach (var path in assetPaths)
            {
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                bool allMeshesFound = true;
                foreach (var obj in objs)
                {
                    if (!(obj is Mesh))
                        continue;

                    if (!invalidMeshes.Contains((Mesh)obj))
                    {
                        allMeshesFound = false;
                        break;
                    }
                }
                if (allMeshesFound)
                {
                    Debug.LogError(path, AssetDatabase.LoadMainAssetAtPath(path));/*
                    var imp = (ModelImporter)AssetImporter.GetAtPath(path);
                    if (imp.tangentImportMode != ModelImporterTangentSpaceMode.None)
                    {
                        imp.tangentImportMode = ModelImporterTangentSpaceMode.None;
                        AssetDatabase.ImportAsset(path);
                    }*/
                }
            }
        }
    }
}
