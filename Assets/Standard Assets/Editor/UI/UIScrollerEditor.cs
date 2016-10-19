using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Core;

[CustomEditor(typeof(UIScroller))]
class UIScrollerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIScroller sc = (UIScroller)target;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scroll Area", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        float newScrollAreaX = EditorGUILayout.FloatField("X", sc.ScrollArea.x);
        float newScrollAreaY = EditorGUILayout.FloatField("Y", sc.ScrollArea.y);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        float newScrollAreaWidth = EditorGUILayout.FloatField("Width", sc.ScrollArea.width);
        float newScrollAreaHeight = EditorGUILayout.FloatField("Height", sc.ScrollArea.height);
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
            sc.ScrollArea = new Rect(newScrollAreaX, newScrollAreaY, newScrollAreaWidth, newScrollAreaHeight);
    }

    void OnSceneGUI()
    {
        UIScroller sc = (UIScroller)target;
        Vector3[] vertices = {
                                 new Vector3(sc.ScrollArea.x, sc.ScrollArea.y),
                                 new Vector3(sc.ScrollArea.x, sc.ScrollArea.y) + Vector3.right * sc.ScrollArea.width,
                                 new Vector3(sc.ScrollArea.x, sc.ScrollArea.y) + Vector3.right * sc.ScrollArea.width + Vector3.up * sc.ScrollArea.height,
                                 new Vector3(sc.ScrollArea.x, sc.ScrollArea.y) + Vector3.up * sc.ScrollArea.height
                             };

        Vector3[] wVertices = new Vector3[vertices.Length];
        bool[] edited = new bool[vertices.Length];
        int i = 0;
        for (; i < 4; ++i)
            wVertices[i] = sc.transform.TransformPoint(vertices[i]);

        Handles.DrawSolidRectangleWithOutline(wVertices, new Color(0.1f, 0.1f, 0.1f, 0.1f), Color.gray);
        for (i = 0; i < 4; ++i)
        {
            wVertices[i] = Handles.FreeMoveHandle(wVertices[i], sc.transform.rotation, HandleUtility.GetHandleSize(wVertices[i]) * 0.1f, Vector3.one, Handles.RectangleCap);
            Vector3 v = sc.transform.InverseTransformPoint(wVertices[i]);
            edited[i] = v != vertices[i];
            vertices[i] = v;
        }

        if (GUI.changed)
        {
            Undo.RecordObject(sc, "Edit 9-Slice");

            Vector2 newPivot = new Vector2(sc.ScrollArea.x, sc.ScrollArea.y);
            float newWidth = sc.ScrollArea.width, newHeight = sc.ScrollArea.height;

            Vector3 x0 = vertices[1] - vertices[0],
                    x1 = vertices[2] - vertices[3],
                    px0 = Vector3.Project(x0, Vector3.right),
                    px1 = Vector3.Project(x1, Vector3.right);

            if (edited[0] || edited[1])
            {
                newWidth = Mathf.Max(0.0f, px0.x);
                newPivot.x -= vertices[3].x - vertices[0].x;
            }
            else
            {
                newWidth = Mathf.Max(0.0f, px1.x);
                newPivot.x -= vertices[0].x - vertices[3].x;
            }

            Vector3 y0 = vertices[3] - vertices[0],
                    y1 = vertices[2] - vertices[1],
                    py0 = Vector3.Project(y0, Vector3.up),
                    py1 = Vector3.Project(y1, Vector3.up);

            if (edited[0] || edited[3])
            {
                newHeight = Mathf.Max(0.0f, py0.y);
                newPivot.y -= vertices[1].y - vertices[0].y;
            }
            else
            {
                newHeight = Mathf.Max(0.0f, py1.y);
                newPivot.y -= vertices[0].y - vertices[1].y;
            }

            sc.ScrollArea = new Rect(newPivot.x, newPivot.y, newWidth, newHeight);
        }
    }
}