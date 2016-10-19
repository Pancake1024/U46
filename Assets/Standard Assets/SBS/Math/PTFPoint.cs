using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SBS.Math;

[AddComponentMenu("SBS/Math/PTFPoint")]
public class PTFPoint : MonoBehaviour
{
    public Vector3 position;
    public Quaternion rotation;

    void Awake()
    {
        position = transform.position;
        rotation = transform.rotation;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        ParallelTransportFrame ptf = transform.parent.gameObject.GetComponent<ParallelTransportFrame>();
        if (null == ptf)
            return;

        bool ptfSelected = Selection.activeTransform == ptf.transform || (Selection.activeTransform != null && Selection.activeTransform.parent == transform.parent);
        int index = ptf.EditorGetPointIndex(this);
        if (0 == index)
            ptf.EditorDrawCurve(ptfSelected);

        if (ptfSelected)
            Gizmos.DrawSphere(transform.position, HandleUtility.GetHandleSize(transform.position) * 0.1f);
    }
#endif
}
