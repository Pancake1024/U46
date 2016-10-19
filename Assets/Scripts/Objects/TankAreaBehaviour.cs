using System;
using System.Collections.Generic;
using UnityEngine;

public class TankAreaBehaviour : MonoBehaviour
{
    protected List<GameObject> trafficListRight;
    protected List<GameObject> trafficListLeft;

    #region Unity callbacks
    void Start()
    {
        trafficListRight = new List<GameObject>();
        trafficListLeft = new List<GameObject>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Traffic"))
        {
            other.gameObject.SendMessage("OnTankShotAreaEnter");
            if (other.gameObject.GetComponent<OpponentKinematics>().CurrentLane < 2)
                trafficListLeft.Add(other.gameObject);
            else
                trafficListRight.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Traffic"))
        {
            other.gameObject.SendMessage("OnTankShotAreaExit");

            if (trafficListLeft.Contains(other.gameObject))
                trafficListLeft.Remove(other.gameObject);
            else
                trafficListRight.Remove(other.gameObject);
        }
    }
    #endregion

    #region Functions
    public bool SomeoneInside(bool rightSide)
    {
        if (rightSide)
            return trafficListRight.Count > 0;
        else
            return trafficListLeft.Count > 0;
    }
    #endregion
}
