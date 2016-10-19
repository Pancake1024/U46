using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SerializedArrayEditor
{
    static public void DrawGUI(string title, SerializedProperty prop)
    {
        if (!prop.isArray)
            return;

        int count = prop.arraySize;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        if (GUILayout.Button("Push Front"))
        {
            if (0 == count)
                count = prop.arraySize = 1;
            else
            {
                prop.InsertArrayElementAtIndex(0);
                ++count;
            }
        }
        if (GUILayout.Button("Push Back"))
        {
            count++;
            prop.arraySize++;
        }
        if (GUILayout.Button("Clear"))
        {
            count = 0;
            prop.ClearArray();
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            SerializedProperty item = prop.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(item, true);
            item.Dispose();
            if (GUILayout.Button("Ins", GUILayout.Width(32)))
                prop.InsertArrayElementAtIndex(i);
            if (GUILayout.Button("Del", GUILayout.Width(32)))
            {
                prop.DeleteArrayElementAtIndex(i--);
                --count;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
