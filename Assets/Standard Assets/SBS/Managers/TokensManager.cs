using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Managers/TokensManager")]
public class TokensManager : MonoBehaviour
{
    #region Singleton instance
    protected static TokensManager instance = null;

    public static TokensManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region TokenHit class
    public class TokenHit
    {
        public Token token = null;
        public SBSVector3 position = new SBSVector3();
        public SBSVector3 tangent = new SBSVector3();
        public float longitudinal = 0.0f;
        public float trasversal = 0.0f;
    };
    #endregion

    #region Public members
    public float longitudinalExt = SBSMath.Epsilon;
    public float trasversalExt = SBSMath.Epsilon;
    #endregion

    #region Protected members
    protected Token[] tokens = null;
    #endregion

    #region Public properties
    public Token[] Tokens
    {
        get
        {
            return tokens;
        }
    }
    #endregion

    #region GetTokens methods
    public bool GetToken(Vector3 worldPos, out TokenHit hitInfo)
    {
        return this.GetToken(worldPos, null, out hitInfo);
    }

    public bool GetToken(Vector3 worldPos, Token startToken, out TokenHit hitInfo)
    {
        return this.GetToken(worldPos, startToken, null, out hitInfo);
    }

    public bool GetToken(Vector3 worldPos, Token startToken, TrackBranch trackBranch, out TokenHit hitInfo)
    {
        List<TokenHit> hits = this.GetTokens(worldPos, startToken, trackBranch, true, -1);

        if (hits.Count > 0)
        {
            hitInfo = hits[0];

            return true;
        }
        else
        {
            hitInfo = null;

            return false;
        }
    }

    public List<TokenHit> GetTokens(Vector3 worldPos)
    {
        return this.GetTokens(worldPos, null);
    }

    public List<TokenHit> GetTokens(Vector3 worldPos, Token startToken)
    {
        return this.GetTokens(worldPos, startToken, null, false, -1);
    }

    public List<TokenHit> GetTokens(Vector3 worldPos, Token startToken, TrackBranch trackBranch, bool exitAtFirstHit, int branchMask)
    {
        if (null == startToken)
        {
            if (null == trackBranch)
            {
                LevelObject[] objs = LevelRoot.Instance.Query(new Bounds(worldPos, Vector3.zero), "tokens");
                if (objs.Length > 0)
                    startToken = objs[0].gameObject.GetComponent<Token>();
                else
                {
                    objs = LevelRoot.Instance.Query("tokens");
                    if (objs.Length > 0)
                        startToken = objs[0].gameObject.GetComponent<Token>();
                    else
                        return new List<TokenHit>();
                }
            }
            else
            {
                startToken = trackBranch[0];
            }
        }

        List<TokenHit> tokenHits = new List<TokenHit>();
        List<int> visitedTokens = new List<int>();

#if UNITY_FLASH
        List<Token> queue = new List<Token>();
        queue.Add(startToken);
#else
        Queue<Token> queue = new Queue<Token>();
        queue.Enqueue(startToken);
#endif

        visitedTokens.Add(startToken.UniqueId);

        while (queue.Count > 0)
        {
            TokenHit tokenHit = new TokenHit();

#if UNITY_FLASH
            tokenHit.token = queue[0];
            queue.RemoveAt(0);
#else
            tokenHit.token = queue.Dequeue();
#endif
            tokenHit.token.WorldToToken(worldPos, out tokenHit.longitudinal, out tokenHit.trasversal);

            if (tokenHit.longitudinal >= 0.0f - longitudinalExt &&
                tokenHit.longitudinal <= 1.0f + longitudinalExt &&
                tokenHit.trasversal >= -1.0f - trasversalExt &&
                tokenHit.trasversal <= 1.0f + trasversalExt &&
                (tokenHit.token.TrackBranch == trackBranch || null == trackBranch) &&
                (null == tokenHit.token.TrackBranch || (tokenHit.token.TrackBranch.mask & branchMask) != 0))
            {
#if UNITY_FLASH
                tokenHit.token.TokenToWorld(tokenHit.longitudinal, tokenHit.trasversal, tokenHit.position, tokenHit.tangent);
#else
                tokenHit.token.TokenToWorld(tokenHit.longitudinal, tokenHit.trasversal, out tokenHit.position, out tokenHit.tangent);
#endif
                tokenHits.Add(tokenHit);

                if (exitAtFirstHit)
                    break;
            }

            foreach (Token link in tokenHit.token.nextLinks)
            {
#if UNITY_FLASH
                int linkIndex = AS3Utils.BinarySearchList(visitedTokens, link.UniqueId);
#else
                int linkIndex = visitedTokens.BinarySearch(link.UniqueId);
#endif

                if (linkIndex < 0)
                {
#if UNITY_FLASH
                    queue.Add(link);
#else
                    queue.Enqueue(link);
#endif

                    visitedTokens.Insert(~linkIndex, link.UniqueId);
                }
            }

            foreach (Token link in tokenHit.token.prevLinks)
            {
#if UNITY_FLASH
                int linkIndex = AS3Utils.BinarySearchList(visitedTokens, link.UniqueId);
#else
                int linkIndex = visitedTokens.BinarySearch(link.UniqueId);
#endif

                if (linkIndex < 0)
                {
#if UNITY_FLASH
                    queue.Add(link);
#else
                    queue.Enqueue(link);
#endif

                    visitedTokens.Insert(~linkIndex, link.UniqueId);
                }
            }
        }

        return tokenHits;//.ToArray(); ToDo Flash
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        tokens = GameObject.FindObjectsOfType(typeof(Token)) as Token[];
#if UNITY_EDITOR
        foreach (Token tok in tokens)
        {
            LevelObject levelObj = tok.gameObject.GetComponent<LevelObject>();
            Asserts.Assert(levelObj != null && "tokens" == levelObj.category);
        }
#endif
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
