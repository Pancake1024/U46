using System;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Managers/CheckpointsManager")]
public class CheckpointsManager : MonoBehaviour
{
    #region Protected members
    protected Checkpoint[] checkpoints = null;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        checkpoints = gameObject.GetComponentsInChildren<Checkpoint>();
    }

    void Update()
    {
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (null == checkpoint || !checkpoint.enabled)
                continue;

            LevelObject chkPtLvlObj = checkpoint.GetComponent<LevelObject>();
            LevelObject[] gameObjects = LevelRoot.Instance.Query(chkPtLvlObj.Bounds, checkpoint.layersMask);

            foreach (LevelObject levelObj in gameObjects)
            {
                if (!levelObj.IsMovable)
                    continue;

                SBSVector3 prevLocalPt = chkPtLvlObj.WorldToLocal.MultiplyPoint3x4(((MovableLevelObject)levelObj).PrevLocalToWorld.position);
                SBSVector3 localPt = chkPtLvlObj.WorldToLocal.MultiplyPoint3x4(levelObj.LocalToWorld.position);
                float prevDist = 0.0f, dist = 0.0f;

                switch (checkpoint.type)
                {
                    case Checkpoint.Type.Cylindrical:
                        prevLocalPt -= (SBSVector3)checkpoint.upVector * SBSVector3.Dot(checkpoint.upVector, prevLocalPt);
                        localPt -= (SBSVector3)checkpoint.upVector * SBSVector3.Dot(checkpoint.upVector, localPt);
                        prevDist = prevLocalPt.magnitude;
                        dist = localPt.magnitude;
                        break;
                    case Checkpoint.Type.Spherical:
                        prevDist = prevLocalPt.magnitude;
                        dist = localPt.magnitude;
                        break;
                }

                if (prevDist > checkpoint.radius && dist <= checkpoint.radius)
                {
                    Debug.Log(levelObj.name + " has entered checkpoint " + checkpoint.name);
                    levelObj.SendMessage("OnCheckpointEnter", checkpoint, SendMessageOptions.DontRequireReceiver);
                }

                if (prevDist <= checkpoint.radius && dist > checkpoint.radius)
                {
                    Debug.Log(levelObj.name + " has exited checkpoint " + checkpoint.name);
                    levelObj.SendMessage("OnCheckpointExit", checkpoint, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
    #endregion
}
