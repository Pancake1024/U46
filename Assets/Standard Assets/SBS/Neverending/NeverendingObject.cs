using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("SBS/Neverending/NeverendingObject")]
public class NeverendingObject : MonoBehaviour
{
    #region Public members
    public int layersMask = 0;
    #endregion

    #region Messages
    void OnMove()
    {
        gameObject.SendMessage("ObjectOnMoveBefore", SendMessageOptions.DontRequireReceiver);
        //if (renderer != null)
        {
            int blockObjsLayerId = transform.parent.GetComponent<NeverendingBlock>().activeLayer,
                blockObjsLayerMask = 0;

            if (blockObjsLayerId >= 0)
                blockObjsLayerMask = (1 << blockObjsLayerId);    
            
            bool flag = ((layersMask & blockObjsLayerMask) != 0);
            if (GetComponent<Renderer>() != null)
                GetComponent<Renderer>().enabled = flag;
            if (GetComponent<Collider>() != null)
                GetComponent<Collider>().enabled = flag;

            //check children also
            foreach (Renderer childRenderer in gameObject.GetComponentsInChildren<Renderer>() )
            {
                childRenderer.enabled = flag;              
            }
            foreach (Collider childCollider in gameObject.GetComponentsInChildren<Collider>())
            {
                childCollider.enabled = flag;             
            }
        }

        gameObject.SendMessage("ObjectOnMoveAfter", SendMessageOptions.DontRequireReceiver);
    }
    #endregion
}
