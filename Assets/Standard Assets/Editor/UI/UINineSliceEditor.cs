using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UINineSlice))]
class UINineSliceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();

        UINineSlice ns = (UINineSlice)target;

        string[] sortingLayerNames = UIEditorUtils.GetSortingLayerNames();
        int[] sortingLayerIDs = UIEditorUtils.GetSortingLayerUniqueIDs();

        int selectedLayer = Array.IndexOf<int>(sortingLayerIDs, ns.sortingLayerID),
            newLayer = EditorGUILayout.Popup("Sorting Layer", selectedLayer, sortingLayerNames);

        if (newLayer != selectedLayer)
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.sortingLayerID = sortingLayerIDs[newLayer];
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("0"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.up * ns.height);
            ns.pivot = Vector2.up * ns.height;
        }
        if (GUILayout.Button("1"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width * 0.5f + Vector2.up * ns.height);
            ns.pivot = Vector2.right * ns.width * 0.5f + Vector2.up * ns.height;
        }
        if (GUILayout.Button("2"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width + Vector2.up * ns.height);
            ns.pivot = Vector2.right * ns.width + Vector2.up * ns.height;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("3"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.up * ns.height * 0.5f);
            ns.pivot = Vector2.up * ns.height * 0.5f;
        }
        if (GUILayout.Button("4"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width * 0.5f + Vector2.up * ns.height * 0.5f);
            ns.pivot = Vector2.right * ns.width * 0.5f + Vector2.up * ns.height * 0.5f;
        }
        if (GUILayout.Button("5"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width + Vector2.up * ns.height * 0.5f);
            ns.pivot = Vector2.right * ns.width + Vector2.up * ns.height * 0.5f;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("6"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot);
            ns.pivot = Vector2.zero;
        }
        if (GUILayout.Button("7"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width * 0.5f);
            ns.pivot = Vector2.right * ns.width * 0.5f;
        }
        if (GUILayout.Button("8"))
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            ns.transform.position = ns.transform.TransformPoint(-ns.pivot + Vector2.right * ns.width);
            ns.pivot = Vector2.right * ns.width;
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnSceneGUI()
    {
        UINineSlice ns = (UINineSlice)target;
        Vector3[] vertices = {
                                 new Vector3(-ns.pivot.x, -ns.pivot.y),
                                 new Vector3(-ns.pivot.x, -ns.pivot.y) + Vector3.right * ns.width,
                                 new Vector3(-ns.pivot.x, -ns.pivot.y) + Vector3.right * ns.width + Vector3.up * ns.height,
                                 new Vector3(-ns.pivot.x, -ns.pivot.y) + Vector3.up * ns.height
                             };
        Vector3[] wVertices = new Vector3[vertices.Length];
        bool[] edited = new bool[vertices.Length];
        int i = 0;
        for (; i < 4; ++i)
            wVertices[i] = ns.transform.TransformPoint(vertices[i]);

        Handles.DrawSolidRectangleWithOutline(wVertices, new Color(0.1f, 0.1f, 0.1f, 0.1f), Color.gray);
        for (i = 0; i < 4; ++i)
        {
            wVertices[i] = Handles.FreeMoveHandle(wVertices[i], ns.transform.rotation, HandleUtility.GetHandleSize(wVertices[i]) * 0.1f, Vector3.one, Handles.RectangleCap);
            Vector3 v = ns.transform.InverseTransformPoint(wVertices[i]);
            edited[i] = v != vertices[i];
            vertices[i] = v;
        }

        if (GUI.changed)
        {
            Undo.RecordObject(ns, "Edit 9-Slice");

            Vector2 newPivot = ns.pivot;
            float newWidth = ns.width, newHeight = ns.height;

            Vector3 x0 = vertices[1] - vertices[0],
                    x1 = vertices[2] - vertices[3],
                    px0 = Vector3.Project(x0, Vector3.right),
                    px1 = Vector3.Project(x1, Vector3.right);

            if (edited[0] || edited[1])
            {
                newWidth = Mathf.Max(0.0f, px0.x);
                newPivot.x += vertices[3].x - vertices[0].x;
            }
            else
            {
                newWidth = Mathf.Max(0.0f, px1.x);
                newPivot.x += vertices[0].x - vertices[3].x;
            }

            Vector3 y0 = vertices[3] - vertices[0],
                    y1 = vertices[2] - vertices[1],
                    py0 = Vector3.Project(y0, Vector3.up),
                    py1 = Vector3.Project(y1, Vector3.up);

            if (edited[0] || edited[3])
            {
                newHeight = Mathf.Max(0.0f, py0.y);
                newPivot.y += vertices[1].y - vertices[0].y;
            }
            else
            {
                newHeight = Mathf.Max(0.0f, py1.y);
                newPivot.y += vertices[0].y - vertices[1].y;
            }

            ns.pivot = newPivot;
            ns.width = newWidth;
            ns.height = newHeight;
        }
    }
}
