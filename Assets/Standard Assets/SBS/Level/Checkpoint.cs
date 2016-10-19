using System;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Level/Checkpoint")]
[RequireComponent(typeof(LevelObject))]
public class Checkpoint : MonoBehaviour
{
    #region Type enum
    public enum Type
    {
        Spherical = 0,
        Cylindrical
    };
    #endregion

    #region Public members
    public Type type;
    public float radius;
    public Vector3 upVector = Vector3.up;
    public int layersMask = -1;
    #endregion

    #region Messages
    public void OnInit()
    {
        gameObject.GetComponent<LevelObject>().Bounds = new SBSBounds(transform.position, new SBSVector3(radius, radius, radius));
    }
    #endregion

    #region Unity callbacks
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endregion
}
