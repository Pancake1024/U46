using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("SBS/Racing/VehicleAISensor")]
public class VehicleAISensor : MonoBehaviour
{
    #region Public members
    public VehicleTrackData trackData = null;
    public float latTreshold = 3.0f;
    public float longTreshold = 3.0f;
    #endregion

    #region Protected members
    protected List<VehicleNearData> neighbours = new List<VehicleNearData>();
    protected GameObject[] nearest = new GameObject[(int)Region.Count];
    #endregion

    #region Region enum
    public enum Region
    {
        Unknown = -1,
        FrontLeft = 0,
        FrontCenter,
        FrontRight,
        Left,
        Right,
        RearLeft,
        RearCenter,
        RearRight,
        Count
    }
    #endregion

    #region VehicleNearData class
    public class VehicleNearData
    {
        public Region region;
        public GameObject go;
        public VehicleTrackData trackData;
        public float sqDist;

        public VehicleNearData(Region _region, GameObject _go, VehicleTrackData _trackData)
        {
            region = _region;
            go = _go;
            trackData = _trackData;
            sqDist = 0.0f;
        }
    }
    #endregion

    #region Protected methods
    protected Region FindRegion(VehicleTrackData otherTrackData)
    {
        if (null == trackData || null == trackData.Token || null == otherTrackData)
            return Region.Unknown;

        float deltaTrasv = (otherTrackData.Trasversal - trackData.Trasversal) * trackData.Token.width * 0.5f,
              deltaLong = otherTrackData.TrackPosition - trackData.TrackPosition;

        if (SBSMath.Abs(deltaLong) < longTreshold)
        {
            if (deltaTrasv >= 0.0f)
                return Region.Right;
            else
                return Region.Left;
        }
        else if (deltaLong >= 0.0f)
        {
            if (SBSMath.Abs(deltaTrasv) <= latTreshold)
                return Region.FrontCenter;
            else if (deltaTrasv >= 0.0f)
                return Region.FrontRight;
            else
                return Region.FrontLeft;
        }
        else
        {
            if (SBSMath.Abs(deltaTrasv) <= latTreshold)
                return Region.RearCenter;
            else if (deltaTrasv >= 0.0f)
                return Region.RearRight;
            else
                return Region.RearLeft;
        }
    }
    #endregion

    #region Public methods
    public GameObject GetNearestNeighbour(Region region)
    {
        return nearest[(int)region];
    }

    public GameObject[] GetNeighbours()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (VehicleNearData nearData in neighbours)
        {
            if (null == nearData.go)
                continue;

            list.Add(nearData.go);
        }
        return list.ToArray();
    }

    public void RemoveObject(GameObject other)
    {
        //same as OnTriggerExit, used to manually remove gameobjects from neighbours list
        if (null == trackData || other.gameObject == trackData.gameObject)
            return;
      
        for (int i = neighbours.Count - 1; i >= 0; --i)
        {
            VehicleNearData nearData = neighbours[i];
            if (nearData.go == other.gameObject)
            {
                neighbours.RemoveAt(i);
               
                if (nearData.region >= 0 && other.gameObject == nearest[(int)nearData.region])
                {
                    nearest[(int)nearData.region] = null;                   
                }
                break;
            }
        }
    }

    public void Reset()
    {
        neighbours = new List<VehicleNearData>();
    }
    #endregion

    #region Unity callbacks
    void OnTriggerEnter(Collider other)
    {
        if (null == trackData || other.gameObject == trackData.gameObject)
            return;
        
        VehicleTrackData otherTrackData = other.gameObject.GetComponent<VehicleTrackData>();

        if (otherTrackData == null)
            return;  //Ken: added to avoid useless checks

        if (other.isTrigger)
            return; //Redz: added to avoid useless checks

        VehicleNearData nearData = new VehicleNearData(
                this.FindRegion(otherTrackData),
                other.gameObject,
                otherTrackData);

        nearData.sqDist = SBSVector3.SqrDistance(transform.position, other.transform.position);

        neighbours.Add(nearData);
    }
 
    void OnTriggerExit(Collider other)
    {
        if (null == trackData || other.gameObject == trackData.gameObject)
            return;
        VehicleTrackData otherTrackData = other.gameObject.GetComponent<VehicleTrackData>();

        if (otherTrackData == null)
            return;  //Ken: added to avoid useless checks

        
        for (int i = neighbours.Count - 1; i >= 0; --i)
        {
            VehicleNearData nearData = neighbours[i];
            if (nearData.go == other.gameObject)
            {
                neighbours.RemoveAt(i);

                if (nearData.region >= 0 && other.gameObject == nearest[(int)nearData.region])
                {
                    nearest[(int)nearData.region] = null;
                    // ToDo get new nearest
                }

                break;
            }
        }
    }

    void FixedUpdate()
    {
        SBSVector3 pos = transform.position;
        float[] minSqDists = new float[(int)Region.Count];

        for (int i = (int)Region.Count - 1; i >= 0; --i)
        {
            minSqDists[i] = float.MaxValue;
            nearest[i] = null;
        }

        for (int i = neighbours.Count - 1; i >= 0; --i)
        {
            VehicleNearData nearData = neighbours[i];
            GameObject go = nearData.go;
            VehicleTrackData td = nearData.trackData;

			bool bActive = false;
#if UNITY_4_3
            bActive = !go.activeSelf;
#else
			bActive = !go.active;
#endif
			if (null == go || bActive)
            {
                neighbours.RemoveAt(i);
                continue;
            }

            nearData.sqDist = SBSVector3.SqrDistance(pos, go.transform.position);
            int regionIndex = (int)(nearData.region = this.FindRegion(td));

            if (regionIndex < 0)
                continue;

            if (nearData.sqDist < minSqDists[regionIndex])
            {
                nearest[regionIndex] = nearData.go;
                minSqDists[regionIndex] = nearData.sqDist;
            }
        }
    }

    void OnDrawGizmos()
    {
        GameObject go = this.GetNearestNeighbour(Region.FrontCenter);
        if (go != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, go.transform.position);
        }
    }
    #endregion
}
