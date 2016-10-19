using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SBS/Neverending/NeverendingBlockLink")]
public class NeverendingBlockLink : MonoBehaviour
{
    #region Public members
    public List<NeverendingBlock> blackList;
    public List<NeverendingBlock> whiteList;
    public Token firstBranchToken = null;

    [NonSerialized]
    public NeverendingTrack.BlocksBranch branch = null;
    #endregion

    #region Messages
    void OnPopFromTrack()
    {
        branch = null;
    }
    #endregion
}
