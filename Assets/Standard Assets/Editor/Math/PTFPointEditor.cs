using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PTFPoint))]
class PTFPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PTFPoint point = target as PTFPoint;
        ParallelTransportFrame ptf = point.transform.parent.gameObject.GetComponent<ParallelTransportFrame>();

        if (null == ptf)
        {
            GUILayout.Label("PTFPoint doesn't have a ParallelTransportFrame parent!", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Curve Length", ptf.Curve.Length.ToString());
            int index = ptf.EditorGetPointIndex(point);
            if (index < 0)
            {
                GUILayout.Label("PFRPoint isn't contained in ParallelTransportFrame parent!", EditorStyles.boldLabel);
            }
            else
            {
                if (GUILayout.Button("Delete"))
                {
                    ptf.EditorRemovePointAt(index);
                    index = Mathf.Clamp(index, 0, ptf.points.Count - 1);
                    Selection.activeGameObject = ptf.points[index].gameObject;
                }

                if (GUILayout.Button("Insert Before"))
                {
                    Selection.activeGameObject = ptf.EditorInsertPointAt(index);
                }

                if (GUILayout.Button("Insert After"))
                {
                    Selection.activeGameObject = ptf.EditorInsertPointAt(index + 1);
                }
            }
        }
    }
}
