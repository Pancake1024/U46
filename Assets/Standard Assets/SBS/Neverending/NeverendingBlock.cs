using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Neverending/NeverendingBlock")]
public class NeverendingBlock : MonoBehaviour
{
    #region Public enums
    public enum Frequency
    {
        Lowest = 0,
        Low,
        Medium,
        High,
        Highest
    }

    public enum CacheSize
    {
        Small = 0,
        Medium,
        Big
    }
    #endregion

    #region Layer class
    [Serializable]
    public class Layer
    {
        public string name = "Layer";
        public float perc = 0.0f;
        public int diffMin = 1;
        public int diffMax = 100;
    }
    #endregion

    #region Public members
    public NeverendingBlockLink[] links;
    public Token firstToken;
    public Frequency frequency;
    public CacheSize cacheSize;
    public bool excludeFromRelax = false;
    #endregion

    #region NonSerialized public members
    [NonSerialized]
    public bool used = false;
    [NonSerialized]
    public int id = -1;
    [NonSerialized]
    public bool toDelete = false;
#if UNITY_EDITOR
    [NonSerialized]
    public int EDITOR_visibleLayer = 0;
#endif
    [NonSerialized]
    public int activeLayer = -1;
    #endregion

    #region Protected members
    protected float[] endTrackPositions;
    #endregion

    #region Protected SerializeField members
    [SerializeField]
    protected Layer[] layers;
    #endregion

    #region Public properties
    public Layer[] Layers
    {
        get
        {
            return layers;
        }
    }
    #endregion

    #region Protected methods
    protected void FillBranch(Token startToken, TrackBranch branch, ref List<TrackBranch> newBranches)
    {
        Token token = startToken;
        while (token != null && token.nextLinks.Count > 0)
        {
            branch.AddToken(token);

            int nextLinksCount = token.nextLinks.Count;
            if (1 == nextLinksCount)
                token = token.nextLinks[0];
            else
            {
                for (int i = 0; i < nextLinksCount; ++i)
                {
                    TrackBranch newBranch = new TrackBranch();
                    newBranch.startLength = branch.startLength + branch.Length;
                    if (null == newBranches)
                        newBranches = new List<TrackBranch>();
                    newBranches.Add(newBranch);
                    this.FillBranch(token.nextLinks[i], newBranch, ref newBranches);
                }
                token = null;
            }
        }

        if (token != null)
            branch.AddToken(token);
    }
    #endregion

    #region Public methods
    virtual public List<TrackBranch> AddToBranch(TrackBranch startBranch)
    {
        List<TrackBranch> newBranches = null;
        this.FillBranch(firstToken, startBranch, ref newBranches);
        if (null == newBranches)
            endTrackPositions[0] = startBranch.startLength + startBranch.Length;
        else
        {
            for (int l = 0; l < endTrackPositions.Length; ++l)
                endTrackPositions[l] = startBranch.startLength + startBranch.Length + newBranches[l].Length;
        }

        return newBranches;
    }

    public float GetEndTrackPosition(int nextLinkIndex)
    {
        return endTrackPositions[nextLinkIndex];
    }

    public float GetMaxEndTrackPosition()
    {
        float endTrackPos = 0.0f;
        foreach (float trackPos in endTrackPositions)
            endTrackPos = SBSMath.Max(endTrackPos, trackPos);
        return endTrackPos;
    }

    public int GetDifficultyMin()
    {
        if (0 == layers.Length)
            return 0;

        int r = 100;
        foreach (Layer layer in layers)
            r = Mathf.Min(r, layer.diffMin);
        return r;
    }

    public int GetDifficultyMax()
    {
        if (0 == layers.Length)
            return 100;

        int r = 0;
        foreach (Layer layer in layers)
            r = Mathf.Max(r, layer.diffMax);
        return r;
    }

    public bool IsDifficultyInRange(int index, int difficulty)
    {
        return layers[index].diffMin <= difficulty && difficulty <= layers[index].diffMax;
    }

    public bool IsDifficultyInRange(int difficulty)
    {
        for (int i = 0; i < layers.Length; ++i)
        {
            if (layers[i].name.Contains("Relax"))
                continue;

            if (this.IsDifficultyInRange(i, difficulty))
                return true;
        }

        return false;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        endTrackPositions = new float[links.Length - 1];

#if UNITY_FLASH && !UNITY_EDITOR
        AS3Stack<Token> tokens = new AS3Stack<Token>();
        tokens.Push(firstToken);
        while (tokens.Count > 0)
        {
            Token tok = tokens.Pop();

            List<Token> _nextLinks = tok.nextLinks,
                        _prevLinks = tok.prevLinks;

            if (_prevLinks.Count > 0)
            {
                tok.prevLinks = new List<Token>();
                foreach (Token prv in _prevLinks)
                    tok.prevLinks.Add(tok.transform.parent.Find(prv.name).GetComponent<Token>());
            }

            if (_nextLinks.Count > 0)
            {
                tok.nextLinks = new List<Token>();
                foreach (Token nxt in _nextLinks)
                    tok.nextLinks.Add(tok.transform.parent.Find(nxt.name).GetComponent<Token>());
            }

            foreach (Token nextTok in tok.nextLinks)
                tokens.Push(nextTok);
        }
#endif
    }

#if !UNITY_FLASH
    void Start()
    {
        // ToDo: Static batching of static elements
    }
#endif
    #endregion

    #region Messages
    void OnMove()
    {
        toDelete = false;
    }
    #endregion
}
