using System;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Level/Checkline")]
[RequireComponent(typeof(LevelObject))]
public class Checkline : MonoBehaviour
{
    #region Public members
    public float width = 1.0f;
    public int layersMask = -1;
    public Vector3 perpendicularAxis = Vector3.forward;
    public Vector3 upAxis = Vector3.up;
    #endregion

    #region Public properties
    public SBSVector3 parallelAxis
    {
        get
        {
            return SBSVector3.Cross(upAxis, perpendicularAxis);
        }
    }
    #endregion

    #region Messages
    void OnInit()
    {
        gameObject.GetComponent<LevelObject>().Bounds = new SBSBounds(transform.position, new SBSVector3(width, width, width));
    }
    #endregion

    #region Unity Callbacks
    void OnDrawGizmos()
    {
        Vector3 n = transform.TransformDirection(this.parallelAxis),
                p0 = transform.position - n * width * 0.5f,
                p1 = transform.position + n * width * 0.5f;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(p0, p1);
    }
    #endregion
}
