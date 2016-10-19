using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

public class PTFUtils : EditorWindow
{
    protected static string persistentData = "Assets/PTFPersistentData.asset";

    protected string baseDir = null;
    protected PTFUtilsData data = null;

    protected ParallelTransportFrame selectedPtf = null;
    protected int selectedMesh = 0;

    protected string newMeshName = "";
    protected string newGOName = "";
    protected Material newGOMaterial = null;

    protected List<PTFMeshData> presets = new List<PTFMeshData>();
    protected int selectedPreset = 0;

    protected Transform selectedTr = null;
    protected bool orient = false;
    protected float space = 0.0f;

    protected string feedbackString = "";

    [MenuItem("SBS/Parallel Transport Frame")]
    static void Init()
    {
        EditorWindow.GetWindow<PTFUtils>("Parallel Transport Frame", true);
    }

    protected void RefreshPresets()
    {
        presets.Clear();
        string[] files = Directory.GetFiles(baseDir + data.presetsDir, "*.ptf.asset");
        foreach (string file in files)
        {
            string fileName = file.Substring(file.IndexOf(data.presetsDir) + data.presetsDir.Length);
            PTFMeshData meshData = AssetDatabase.LoadAssetAtPath(data.presetsDir + fileName, typeof(PTFMeshData)) as PTFMeshData;
            if (meshData != null)
                presets.Add(meshData);
        }
    }

    void OnEnable()
    {
        baseDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));

        data = AssetDatabase.LoadAssetAtPath(persistentData, typeof(PTFUtilsData)) as PTFUtilsData;
        if (null == data)
        {
            data = ScriptableObject.CreateInstance<PTFUtilsData>();
            AssetDatabase.CreateAsset(data, persistentData);
            AssetDatabase.SaveAssets();
        }

        this.RefreshPresets();
    }

    void OnDisable()
    {
        AssetDatabase.SaveAssets();
    }

    protected List<KeyValuePair<MeshFilter, PTFMeshData>> GetExtrudeMeshes(ParallelTransportFrame ptf)
    {
        List<KeyValuePair<MeshFilter, PTFMeshData>> list = new List<KeyValuePair<MeshFilter, PTFMeshData>>();

        MeshFilter[] meshFilters = ptf.gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in meshFilters)
        {
            string meshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
            if (0 == meshPath.Length)
                continue;
            string extrPath = Path.GetDirectoryName(meshPath) + "/" + Path.GetFileNameWithoutExtension(meshPath) + ".ptf.asset";

            PTFMeshData extr = AssetDatabase.LoadAssetAtPath(extrPath, typeof(PTFMeshData)) as PTFMeshData;
            if (extr != null)
                list.Add(new KeyValuePair<MeshFilter, PTFMeshData>(filter, extr));
        }

        return list;
    }

    protected void ExtrudeMesh(KeyValuePair<MeshFilter, PTFMeshData> item)
    {
        ParallelTransportFrame ptf = item.Key.transform.parent.GetComponent<ParallelTransportFrame>();
        if (null == ptf)
            return;

        float relSpace = item.Value.spaceEnd - item.Value.spaceBegin;
        ptf.ResetAtSpace(item.Value.spaceStep, item.Value.spaceBegin);

        if (null == item.Value.mesh)
        {
            int numPoints = item.Value.figurePoints.Length;
            SBSVector3[] points = new SBSVector3[numPoints];
            for (int i = 0; i < numPoints; ++i)
                points[i] = item.Value.figurePoints[i];

            Vector2[][] uvs = new Vector2[2][];
            uvs[0] = item.Value.uv0;
            uvs[1] = item.Value.uv1;

            ptf.Extrude(item.Key.sharedMesh, item.Key.transform.worldToLocalMatrix, points, uvs, item.Value.closeFigure, item.Value.spaceStep, relSpace, item.Value.maxError);
        }
        else
        {
            int numPoints = item.Value.anchorPoints.Length;
            SBSVector3[] points = new SBSVector3[numPoints];
            for (int i = 0; i < numPoints; ++i)
                points[i] = item.Value.anchorPoints[i];

            ptf.DuplicateMerge(item.Key.sharedMesh, item.Key.transform.worldToLocalMatrix, item.Value.mesh, points, item.Value.orientMesh, item.Value.skipLast, item.Value.spaceStep, relSpace, item.Value.maxError);
        }

        AssetDatabase.SaveAssets();
    }

    void OnGUI()
    {
        GUILayout.Label("Options", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Presets Directory");
        GUILayout.Label(data.presetsDir);
        if (GUILayout.Button("Change..."))
        {
            string newDir = EditorUtility.SaveFolderPanel("Mesh Presets Directory", baseDir + data.presetsDir, "");
            if (newDir.Length > 0)
            {
                int index = newDir.IndexOf("Assets");
                if (index >= 0)
                {
                    data.presetsDir = newDir.Substring(index) + "/";
                    AssetDatabase.SaveAssets();
                    this.RefreshPresets();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Meshes Directory");
        GUILayout.Label(data.generatedMeshesDir);
        if (GUILayout.Button("Change..."))
        {
            string newDir = EditorUtility.SaveFolderPanel("Generated Meshes Directory", baseDir + data.generatedMeshesDir, "");
            if (newDir.Length > 0)
            {
                int index = newDir.IndexOf("Assets");
                if (index >= 0)
                {
                    data.generatedMeshesDir = newDir.Substring(index) + "/";
                    AssetDatabase.SaveAssets();
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Extrusions", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        selectedPtf = EditorGUILayout.ObjectField("PTF", selectedPtf, typeof(ParallelTransportFrame), true) as ParallelTransportFrame;
        GUILayout.EndHorizontal();

        if (selectedPtf != null)
        {
            List<KeyValuePair<MeshFilter, PTFMeshData>> list = this.GetExtrudeMeshes(selectedPtf);

            if (list.Count > 0) {
                string[] meshNames = new string[list.Count];

                int count = 0;
                foreach (KeyValuePair<MeshFilter, PTFMeshData> item in list)
                    meshNames[count++] = item.Key.name;

                EditorGUILayout.BeginHorizontal();
                selectedMesh = EditorGUILayout.Popup(selectedMesh, meshNames);
                if (GUILayout.Button("Update"))
                    this.ExtrudeMesh(list[selectedMesh]);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Update All"))
                {
                    foreach (KeyValuePair<MeshFilter, PTFMeshData> item in list)
                        this.ExtrudeMesh(item);
                }
            }
        }

        GUILayout.Label("New extrusion", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        newMeshName = EditorGUILayout.TextField("Mesh Name", newMeshName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        newGOName = EditorGUILayout.TextField("GO Name", newGOName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        newGOMaterial = EditorGUILayout.ObjectField("Material", newGOMaterial, typeof(Material), true) as Material;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        string[] presetNames = new string[presets.Count + 1];
        presetNames[0] = "None";
        for (int i = 0; i < presets.Count; ++i)
            presetNames[i + 1] = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(presets[i].GetInstanceID()));
        selectedPreset = EditorGUILayout.Popup("Preset", selectedPreset, presetNames);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create New"))
        {
            Mesh mesh = new Mesh();
            string meshPath = data.generatedMeshesDir + newMeshName + ".asset";
            if (null == AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)))
            {
                AssetDatabase.CreateAsset(mesh, meshPath);

                PTFMeshData extr = null;
                if (0 == selectedPreset)
                {
                    extr = ScriptableObject.CreateInstance<PTFMeshData>();
                    AssetDatabase.CreateAsset(extr, data.generatedMeshesDir + newMeshName + ".ptf.asset");
                }
                else
                {
                    string newExtrName = data.generatedMeshesDir + newMeshName + ".ptf.asset";
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(presets[selectedPreset - 1]), newExtrName);
                    AssetDatabase.Refresh();
                    extr = AssetDatabase.LoadAssetAtPath(newExtrName, typeof(PTFMeshData)) as PTFMeshData;
                }

                AssetDatabase.SaveAssets();

                GameObject go = new GameObject(newGOName, typeof(MeshFilter), typeof(MeshRenderer));
                if (selectedPtf != null)
                {
                    go.transform.parent = selectedPtf.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                }

                MeshFilter meshFilter = go.GetComponent<MeshFilter>();

                meshFilter.sharedMesh = mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = newGOMaterial;

                if (selectedPreset > 0)
                    this.ExtrudeMesh(new KeyValuePair<MeshFilter, PTFMeshData>(meshFilter, extr));

                feedbackString = "";
            }
            else
            {
                feedbackString = "Mesh \"" + newMeshName + "\" already exist.";
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Label(feedbackString);

        GUILayout.Label("Selected transform", EditorStyles.boldLabel);
        orient = EditorGUILayout.Toggle("Orient", orient);
        if (null == Selection.activeTransform ||
            Selection.activeTransform.gameObject.GetComponentsInChildren<ParallelTransportFrame>().Length > 0 ||
            Selection.activeTransform.gameObject.GetComponentsInChildren<PTFPoint>().Length > 0)
        {
            GUILayout.Label("None");
        }
        else
        {
            if (selectedPtf != null) {
                Transform tr = Selection.activeTransform = EditorGUILayout.ObjectField(Selection.activeTransform, typeof(Transform), true) as Transform;

                float prevSpace = 0.0f;
                if (selectedTr != tr)
                {
                    prevSpace = selectedPtf.Curve.GetClosestSpace(tr.position);
                    selectedTr = tr;
                }
                else
                {
                    prevSpace = space;
                }

                space = EditorGUILayout.Slider("Space", prevSpace, 0.0f, selectedPtf.Curve.Length);

                if (SBSMath.Abs(space - prevSpace) > 1.0e-2)
                {
                    Curve.Evaluation eval = selectedPtf.Curve.EvaluateAtSpace(space);
                    tr.position = eval.f0;
                    if (orient)
                        tr.LookAt(eval.f0 + eval.f1, SBSVector3.up);
                    EditorUtility.SetDirty(tr);
                }
            }
        }
    }
}
