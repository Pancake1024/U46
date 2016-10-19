using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

[CustomEditor(typeof(ParallelTransportFrame))]
public class PTFEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ParallelTransportFrame ptf = target as ParallelTransportFrame;

        EditorGUILayout.LabelField("Curve Length", ptf.Curve.Length.ToString());

        ptf.Curve.InterpolationType = (Curve.Interpolation)EditorGUILayout.EnumPopup("Interpolation Type", ptf.Curve.InterpolationType);
        ptf.Curve.IsClosed = EditorGUILayout.Toggle("Is Closed", ptf.Curve.IsClosed);
        ptf.data = EditorGUILayout.CurveField("Data", ptf.data);
/*
#if UNITY_FLASH
        SerializedProperty dataKeys = serializedObject.FindProperty("dataKeys");
        SerializedArrayEditor.DrawGUI("DataKeys", dataKeys);
        EditorGUILayout.Separator();
        dataKeys.Dispose();
        serializedObject.ApplyModifiedProperties();
#endif
*/
        if (GUI.changed)
        {
            ptf.curveIntType = ptf.Curve.InterpolationType;
            ptf.curveIsClosed = ptf.Curve.IsClosed;

            EditorUtility.SetDirty(ptf);

            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Add Point"))
            Selection.activeGameObject = ptf.EditorInsertPointAt(ptf.points.Count);

        if (GUILayout.Button("Clear"))
        {
            for (int i = ptf.points.Count - 1; i >= 0; --i)
                ptf.EditorRemovePointAt(i);
        }

        if (GUILayout.Button("Update Object Transforms"))
        {
            ptf.BroadcastMessage("UpdateObjectTransform", SendMessageOptions.DontRequireReceiver);
        }
    }
}
