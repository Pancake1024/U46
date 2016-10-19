using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/BranchNode")]
public class BranchNode : MonoBehaviour
{
    #region Public members
    public int mask = -1;

    public Token startToken;
    public Token endToken;
    #endregion

    #region Protected members
    protected Track track = null;

    protected TrackBranch branch = new TrackBranch();
    protected bool noThrough = false;
    protected bool loop = false;

    protected BranchNode[] prevLinks;
    protected BranchNode[] nextLinks;
    #endregion

    #region Public properties
    public TrackBranch Branch
    {
        get
        {
            return branch;
        }
    }

    public bool NoThrough
    {
        get
        {
            return noThrough;
        }
    }

    public bool Loop
    {
        get
        {
            return loop;
        }
    }

    public BranchNode[] PrevLinks
    {
        get
        {
            return prevLinks;
        }
    }

    public BranchNode[] NextLinks
    {
        get
        {
            return nextLinks;
        }
    }
    #endregion

    #region Public methods
    public void Initialize()
    {
        Token firstToken = branch[0],
              lastToken = branch[branch.Count - 1];

        prevLinks = new BranchNode[firstToken.prevLinks.Count];
        nextLinks = new BranchNode[lastToken.nextLinks.Count];

        int i = 0;
        foreach (Token token in firstToken.prevLinks)
        {
            prevLinks[i] = track.GetNodeFromBranch(token.TrackBranch);
            //prevLinks[i].branch.startLength = SBSMath.Max(prevLinks[i].branch.startLength, branch.startLength - prevLinks[i].branch.Length);
            ++i;
        }

        i = 0;
        foreach (Token token in lastToken.nextLinks)
        {
            nextLinks[i] = track.GetNodeFromBranch(token.TrackBranch);
            if (track.GetRootBranch() == token.TrackBranch)
            {
                ++i;
                continue;
            }
            //nextLinks[i].branch.startLength = SBSMath.Max(nextLinks[i].branch.startLength, branch.startLength + branch.Length);
            ++i;
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        track = transform.parent.GetComponent<Track>();
        Asserts.Assert(track != null);

        branch.startLength = 0.0f;
        branch.mask = mask;

        Asserts.Assert(startToken != null && endToken != null);
        Token tok = startToken;
        while (tok != endToken)
        {
            branch.AddToken(tok);
            Asserts.Assert(1 == tok.nextLinks.Count);
            tok = tok.nextLinks[0];
        }
        branch.AddToken(tok);

        Token firstToken = branch[0],
              lastToken  = branch[branch.Count - 1];
        noThrough = (0 == lastToken.nextLinks.Count);

        bool loopAtStart = (firstToken.prevLinks.IndexOf(lastToken) >= 0),
             loopAtEnd   = (lastToken.nextLinks.IndexOf(firstToken) >= 0);

        Asserts.Assert((loopAtStart && loopAtEnd) || (!loopAtStart && !loopAtEnd) || noThrough); // TODO CHIEDERE AD ALEX
        loop = loopAtStart && loopAtEnd;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Token t = startToken;
        while (t != null)
        {
            t.drawGizmos = false;
            t.OnMove();
            t.DrawGizmos(-1.0f, Color.white);
            t.DrawGizmos(1.0f, Color.white);

            if (endToken == t)
                break;

            t = t.nextLinks[0];
        }
    }
#endif
    #endregion
}
