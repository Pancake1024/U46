using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

[CustomEditor(typeof(TexShiftManager))]
public class TexShiftManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedProperty descs = serializedObject.FindProperty("texShiftDescs");
        SerializedArrayEditor.DrawGUI("Materials", descs);
        descs.Dispose();

        serializedObject.ApplyModifiedProperties();
    }
}
