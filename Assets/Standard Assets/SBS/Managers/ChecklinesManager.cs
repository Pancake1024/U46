using System;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Managers/ChecklinesManager")]
public class ChecklinesManager : MonoBehaviour
{
    #region Public classes
    public class Message
    {
        public Checkline checkline;
        public bool front;

        public Message(Checkline _checkline, bool _front)
        {
            checkline = _checkline;
            front = _front;
        }
    }
    #endregion

    #region Protected members
    protected Checkline[] checklines = null;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        checklines = gameObject.GetComponentsInChildren<Checkline>();       
    }

    void Update()
    {
        foreach (Checkline checkline in checklines)
        {
            if (null == checkline || !checkline.enabled)
                continue;

            LevelObject chkLineObj = checkline.GetComponent<LevelObject>();
            SBSVector3 linePos = chkLineObj.LocalToWorld.position;
            LevelObject[] gameObjects = LevelRoot.Instance.Query(chkLineObj.Bounds, checkline.layersMask);

            foreach (LevelObject levelObj in gameObjects)
            {
                if (!levelObj.IsMovable)
                    continue;

                SBSVector3 levelObjPos = levelObj.LocalToWorld.position;
                SBSPlane linePlane = new SBSPlane(chkLineObj.LocalToWorld.MultiplyVector(checkline.perpendicularAxis), linePos);

                float d0 = linePlane.GetDistanceToPoint(((MovableLevelObject)levelObj).PrevLocalToWorld.position);
                float d1 = linePlane.GetDistanceToPoint(levelObjPos);

                bool front = d0 < 0.0f && d1 >= 0.0f,
                     back = d0 >= 0.0f && d1 < 0.0f;
                if (front || back)
                {
                    SBSVector3 n = chkLineObj.LocalToWorld.MultiplyVector(checkline.parallelAxis);

                    SBSPlane p0 = new SBSPlane( n, linePos - n * (checkline.width * 0.5f)),
                             p1 = new SBSPlane(-n, linePos + n * (checkline.width * 0.5f));

                    if (p0.GetDistanceToPoint(levelObjPos) >= 0.0f && p1.GetDistanceToPoint(levelObjPos) >= 0.0f)
                    {
                        Debug.Log(levelObj.name + " has passed checkline " + checkline.name);
                        levelObj.SendMessage("OnCheckline", new Message(checkline, front), SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }
    #endregion
};
