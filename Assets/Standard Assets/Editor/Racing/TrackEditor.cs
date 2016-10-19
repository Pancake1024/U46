using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBS.Math;

[CustomEditor(typeof(Track))]
public class TrackEditor : Editor
{/*
    protected bool SkipNextToken(List<BranchNode> branchNodes, Token next)
    {
        bool skipToken = false;
        foreach (BranchNode node in branchNodes)
        {
            if (next == node.startToken)
            {
                skipToken = true;
                break;
            }
        }

        return skipToken;
    }

    protected bool SkipPrevToken(List<BranchNode> branchNodes, Token prev)
    {
        bool skipToken = false;
        foreach (BranchNode node in branchNodes)
        {
            if (prev == node.endToken)
            {
                skipToken = true;
                break;
            }
        }

        return skipToken;
    }
    */
    protected BranchNode CreateNewBranch(List<BranchNode> branchNodes, int[] oldMasks, int count, Transform parent, Token startToken)
    {
        GameObject branch = new GameObject("TrackBranch", typeof(BranchNode));
        branch.transform.parent = parent;

        BranchNode branchNode = branch.GetComponent<BranchNode>();
        if (count < oldMasks.Length)
            branchNode.mask = oldMasks[count];
        branchNode.startToken = startToken;

        branchNodes.Add(branchNode);

        return branchNode;
    }

    protected bool BranchAlreadyExist(Token startToken, List<BranchNode> openBranches, List<BranchNode> closedBranches)
    {
        foreach (BranchNode node in openBranches)
        {
            if (node.startToken == startToken)
                return true;
        }

        foreach (BranchNode node in closedBranches)
        {
            if (node.startToken == startToken)
                return true;
        }

        return false;
    }

    protected void CreateBranches()
    {
        Track track = target as Track;
        GameObject go = track.gameObject;
        BranchNode[] oldBranches = go.GetComponentsInChildren<BranchNode>();

        int i = oldBranches.Length - 1;
        int[] oldMasks = new int[i + 1];
        for (; i >= 0; --i)
        {
            oldMasks[i] = oldBranches[i].mask;
            GameObject.DestroyImmediate(oldBranches[i].gameObject);
        }

        Token[] tokens = FindObjectsOfType(typeof(Token)) as Token[];

        for (i = track.startTransforms.Length - 1; i >= 0; --i)
        {
            SBSVector3 pos = track.startTransforms[i].position;
            Token startTok = null;
            foreach (Token tok in tokens)
            {
                SBSBounds tokBounds = tok.ComputeBounds();
                if (tokBounds.min.x > pos.x || tokBounds.min.z > pos.z || tokBounds.max.x < pos.x || tokBounds.max.z < pos.z)
                    continue;

                float l, t;
                tok.WorldToToken(pos, out l, out t);
                if (l >= 0.0f && l <= 1.0f && t >= -1.0f && t <= 1.0f)
                {
                    startTok = tok;
                    break;
                }
            }

            if (null == startTok)
                continue;

            List<BranchNode> openBranches = new List<BranchNode>(),
                             closedBranches = new List<BranchNode>();
            this.CreateNewBranch(openBranches, oldMasks, closedBranches.Count, go.transform, startTok);
            while (openBranches.Count > 0)
            {
                for (int j = openBranches.Count - 1; j >= 0; --j)
                {
                    BranchNode openBranch = openBranches[j];
                    Token n = null, t = openBranch.startToken;
                    while (1 == t.nextLinks.Count && (null == n || 1 == n.prevLinks.Count) && !this.BranchAlreadyExist(t.nextLinks[0], openBranches, closedBranches))
                    {
                        t = t.nextLinks[0];
                        n = t.nextLinks.Count > 0 ? t.nextLinks[0] : null;
                    }
                    openBranch.endToken = t;

                    for (int k = 0; k < t.nextLinks.Count; ++k)
                    {
                        if (!this.BranchAlreadyExist(t.nextLinks[k], openBranches, closedBranches))
                            this.CreateNewBranch(openBranches, oldMasks, closedBranches.Count, go.transform, t.nextLinks[k]);
                    }

                    openBranches.RemoveAt(j);
                    closedBranches.Add(openBranch);
                }
            }
            /*Stack<Token> startTokens = new Stack<Token>();
            Stack<bool> directions = new Stack<bool>();
            startTokens.Push(startTok);
            directions.Push(true);

            List<BranchNode> branchNodes = new List<BranchNode>();
            while (startTokens.Count > 0)
            {
                Token t = startTokens.Pop();
                bool d = directions.Pop();

                GameObject branch = new GameObject("TrackBranch", typeof(BranchNode));
                branch.transform.parent = go.transform;

                BranchNode branchNode = branch.GetComponent<BranchNode>();
                if (branchNodes.Count < oldMasks.Length)
                    branchNode.mask = oldMasks[branchNodes.Count];
                if (d)
                {
                    branchNode.startToken = t;
                    while (1 == t.nextLinks.Count && t.nextLinks[0] != branchNode.startToken)
                    {
                        Token nextLink = t.nextLinks[0];
                        if (nextLink.prevLinks.Count > 1)
                        {
                            for (int j = 0; j < nextLink.prevLinks.Count; ++j)
                            {
                                if (t == nextLink.prevLinks[j] || this.SkipPrevToken(branchNodes, nextLink.prevLinks[j]))
                                    continue;

                                startTokens.Push(nextLink.prevLinks[j]);
                                directions.Push(false);
                            }
                            break;
                        }
                        else
                            t = nextLink;
                    }
                    branchNode.endToken = t;

                    branchNodes.Add(branchNode);

                    foreach (Token next in branchNode.endToken.nextLinks)
                    {
                        if (this.SkipNextToken(branchNodes, next))
                            continue;

                        startTokens.Push(next);
                        directions.Push(true);
                    }
                }
                else
                {
                    branchNode.endToken = t;
                    while (1 == t.prevLinks.Count && t.prevLinks[0] != branchNode.endToken)
                    {
                        Token prevLink = t.prevLinks[0];
                        if (prevLink.nextLinks.Count > 1)
                        {
                            for (int j = 0; j < prevLink.nextLinks.Count; ++j)
                            {
                                if (t == prevLink.nextLinks[j] || this.SkipNextToken(branchNodes, prevLink.nextLinks[j]))
                                    continue;

                                startTokens.Push(prevLink.nextLinks[j]);
                                directions.Push(true);
                            }
                            break;
                        }
                        else
                            t = prevLink;
                    }
                    branchNode.startToken = t;

                    branchNodes.Add(branchNode);

                    foreach (Token prev in branchNode.startToken.prevLinks)
                    {
                        if (this.SkipPrevToken(branchNodes, prev))
                            continue;

                        startTokens.Push(prev);
                        directions.Push(false);
                    }
                }
            }*/

            break;
        }
    }

    public override void OnInspectorGUI()
    {
        this.DrawDefaultInspector();

        if (GUILayout.Button("Create Branches"))
            this.CreateBranches();
    }
}
