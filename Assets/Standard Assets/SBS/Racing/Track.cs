using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/Track")]
public class Track : MonoBehaviour
{
    #region Public members
    public Transform[] startTransforms;
    #endregion

    #region Protected members
    protected Token[] startTokens;
    protected float[] startOffsets;
    protected TrackBranch[] rootBranches;
    protected float maxLength;

    protected Dictionary<TrackBranch, BranchNode> branchNodes = new Dictionary<TrackBranch, BranchNode>();
    #endregion

    #region Public properties
    public float Length
    {
        get
        {
            return maxLength;
        }
    }
    #endregion

    #region Public methods
    public float GetStartOffset(int index)
    {
        return startOffsets[index];
    }

    public float GetStartOffset()
    {
        return startOffsets[0];
    }

    public TrackBranch GetRootBranch(int index)
    {
        return rootBranches[index];
    }

    public TrackBranch GetRootBranch()
    {
        return rootBranches[0];
    }

    public BranchNode GetNodeFromBranch(TrackBranch branch)
    {
        return branchNodes[branch];
    }

#if UNITY_FLASH
    public Token EvaluateAt(TrackBranch firstBranch, float trackPos, float trasversal, int mask, SBSVector3 pos, SBSVector3 tang)
#else
    public Token EvaluateAt(TrackBranch firstBranch, float trackPos, float trasversal, int mask, out SBSVector3 pos, out SBSVector3 tang)
#endif
    {
        trackPos += startOffsets[0];
        trackPos = (trackPos < 0.0f ? trackPos + maxLength : (trackPos > maxLength ? trackPos - maxLength : trackPos));

        Stack<TrackBranch> branches = new Stack<TrackBranch>();
        List<TrackBranch> visitedBranches = new List<TrackBranch>();
        branches.Push(firstBranch);

        while (branches.Count > 0)
        {
            TrackBranch branch = branches.Pop();
            visitedBranches.Add(branch);

            if (trackPos >= branch.startLength && trackPos <= (branch.startLength + branch.Length * branch.LengthScale))
#if UNITY_FLASH
                return branch.EvaluateAt(trackPos, trasversal, pos, tang);
#else
                return branch.EvaluateAt(trackPos, trasversal, out pos, out tang);
#endif

            BranchNode node = branchNodes[branch];
            foreach (BranchNode prevNode in node.PrevLinks)
            {
                if ((prevNode.Branch.mask & mask) != 0 && !branches.Contains(prevNode.Branch) && !visitedBranches.Contains(prevNode.Branch))
                    branches.Push(prevNode.Branch);
            }
            foreach (BranchNode nextNode in node.NextLinks)
            {
                if ((nextNode.Branch.mask & mask) != 0 && !branches.Contains(nextNode.Branch) && !visitedBranches.Contains(nextNode.Branch))
                    branches.Push(nextNode.Branch);
            }
        }

        pos = SBSVector3.zero;
        tang = SBSVector3.zero;

        return null;
    }

    public void Build()
    {
        int count = startTransforms.Length;
        startTokens = new Token[count];
        startOffsets = new float[count];
        rootBranches = new TrackBranch[count];

        TokensManager.TokenHit[] hits = new TokensManager.TokenHit[count];
        for (int i = 0; i < count; ++i)
        {
            SBSVector3 p = startTransforms[i].position;

            hits[i] = new TokensManager.TokenHit();
            TokensManager.Instance.GetToken(p, out hits[i]);
            Asserts.Assert(hits[i].token != null);

            TrackBranch branch = hits[i].token.TrackBranch;
            startTokens[i] = hits[i].token;
            rootBranches[i] = branch;
        }
        // ToDo
        foreach (KeyValuePair<TrackBranch, BranchNode> item in branchNodes)
            item.Value.Initialize();
        Stack<TrackBranch> branches = new Stack<TrackBranch>();
        List<TrackBranch> visitedBranches = new List<TrackBranch>();
        branches.Push(rootBranches[0]);
        visitedBranches.Add(rootBranches[0]);
        rootBranches[0].startLength = 0.0f;
        while (branches.Count > 0)
        {
            TrackBranch branch = branches.Pop();

            BranchNode node = branchNodes[branch];

            float maxBranchLength = 0.0f;
            foreach (BranchNode nextNode in node.NextLinks)
                maxBranchLength = SBSMath.Max(maxBranchLength, nextNode.Branch.Length);

            foreach (BranchNode nextNode in node.NextLinks)
            {
                if (!branches.Contains(nextNode.Branch) && !visitedBranches.Contains(nextNode.Branch))
                {
                    nextNode.Branch.LengthScale = maxBranchLength / nextNode.Branch.Length;
                    nextNode.Branch.startLength = Mathf.Max(nextNode.Branch.startLength, branch.startLength + branch.Length);
                    visitedBranches.Add(branch);
                    branches.Push(nextNode.Branch);
                }
            }
        }/*
        foreach (KeyValuePair<TrackBranch, BranchNode> item in branchNodes)
            Debug.Log(item.Value.Branch.startLength + ", " + item.Value.Branch.Length);
        */
        for (int i = 0; i < count; ++i)
        {
            TrackBranch branch = hits[i].token.TrackBranch;
            startOffsets[i] = (branch.startLength + hits[i].longitudinal * hits[i].token.Length) * branch.LengthScale;
            Debug.Log("startOffsets[" + i + "] = " + startOffsets[i]);
        }

        maxLength = 0.0f;
        foreach (KeyValuePair<TrackBranch, BranchNode> item in branchNodes)
            maxLength = SBSMath.Max(maxLength, item.Key.startLength + item.Key.Length);
        Debug.Log("Track length: " + maxLength);
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        BranchNode[] nodesArray = gameObject.GetComponentsInChildren<BranchNode>();
        foreach (BranchNode node in nodesArray)
            branchNodes.Add(node.Branch, node);
    }
    #endregion
}
