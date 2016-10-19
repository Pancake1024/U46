using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.UI;

[AddComponentMenu("SBS/UI/UICollectionNode")]
public class UICollectionNode : MonoBehaviour
{
    #region Protected members
    protected UICollection collection = new UICollection();
    #endregion

    #region Public properties
    public UICollection Collection
    {
        get
        {
            return collection;
        }
    }
    #endregion

    #region Unity callbacks
    void LateUpdate()
    {
        Vector3 p = transform.position;
        SBSMatrix4x4 world = SBSMatrix4x4.TRS(new SBSVector3(p.x * UIManager_OLD.screenWidth, p.y * UIManager_OLD.screenHeight, p.z), transform.rotation, SBSVector3.one);
        collection.SetTransform(world);
    }

    void OnEnable()
    {
        collection.SetVisibility(true);
    }

    void OnDisable()
    {
        collection.SetVisibility(false);
    }

    void OnDestroy()
    {
        collection.Destroy();
    }
    #endregion
}
