using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("SBS/Neverending/NeverendingPlayer")]
public class NeverendingPlayer : MonoBehaviour
{
    #region Public members
    #endregion

    #region Protected members
    protected float tokenLongitudinal = 0.0f;
    protected float tokenTrasversal = 0.0f;
    protected int lastTokenIndex = 0;
    protected float trackPos = 0.0f;
    protected SBSVector3 tokenPos = new SBSVector3();
    protected SBSVector3 tokenTan = new SBSVector3();
    //protected SBSVector3 tokenNormal = new SBSVector3();

    protected NeverendingTrack.BlocksBranch activeBranch = null;
    protected NeverendingTrack.BlockNode prevBlock = null;
    protected NeverendingTrack.BlockNode activeBlock = null;
    protected SBSMatrix4x4 blockWorld = new SBSMatrix4x4();

    protected bool died = false;

    protected NeverendingTutorial tutorial;
    protected float startTrackPos = 0.0f;
    protected float sessionMeters = 0.0f;
    public Token lastToken;

    #endregion

    #region Public properties
    public float StartTrackPos
    {
        get
        {
            return startTrackPos;
        }
    }

    public float Meters
    {
        get
        {
            return SBSMath.Max(0.0f, trackPos - startTrackPos);
        }
    }

    public float SessionMeters
    {
        get
        {
            return sessionMeters;
        }
    }

    public float TrackPos
    {
        get
        {
            return trackPos;
        }
    }

    public SBSVector3 TokenTangent
    {
        get
        {
            return tokenTan;
        }
    }
	/*
    public SBSVector3 TokenNormal
    {
        get
        {
            return tokenNormal;
        }
    }
	*/
    public float TokenTrasversal
    {
        get
        {
            return tokenTrasversal;
        }
    }

    public float TokenLongitudinal
    {
        get
        {
            return tokenLongitudinal;
        }
    }
    


    public SBSMatrix4x4 BlockWorld
    {
        get
        {
            return blockWorld;
        }
    }

    public NeverendingTutorial Tutorial
    {
        get
        {
            return tutorial;
        }
    }

    public NeverendingTrack.BlocksBranch ActiveBranch
    {
        get
        {
            return activeBranch;
        }
    }

    public NeverendingTrack.BlockNode ActiveBlock
    {
        get
        {
            return activeBlock;
        }
    }

    public bool IsDied
    {
        get
        {
            return died;
        }
    }

    public int LastTokenIndex
    {
        get
        {
            return lastTokenIndex;
        }
    }
    #endregion

    #region Public methods
    public virtual void StartRunning(NeverendingTrack.BlocksBranch firstBranch)
    {
        activeBranch = firstBranch;
        prevBlock = null;
        activeBlock = activeBranch.head;
        blockWorld = activeBlock.transform.localToWorldMatrix;

        if (tutorial!=null)
            tutorial.BeforeStartRunning();

#if UNITY_4_3
        gameObject.SetActive(true);
#else
        gameObject.SetActiveRecursively(true);
#endif
		gameObject.SendMessage("OnStartRunning", SendMessageOptions.DontRequireReceiver);
        gameObject.SendMessage("OnBlockChanged", SendMessageOptions.DontRequireReceiver);

        SBSVector3 pos = transform.position;

        activeBranch.trackBranch[0].WorldToToken(pos, out tokenLongitudinal, out tokenTrasversal);
#if UNITY_FLASH
        activeBranch.trackBranch[0].TokenToWorld(tokenLongitudinal, tokenTrasversal, tokenPos, tokenTan);
#else
        activeBranch.trackBranch[0].TokenToWorld(tokenLongitudinal, tokenTrasversal, out tokenPos, out tokenTan);
        //tokenNormal = activeBranch.trackBranch[0].Normal;
#endif

        Camera.main.gameObject.SendMessage("OnStartRunning", SendMessageOptions.DontRequireReceiver);
        Camera.main.gameObject.SendMessage("OnBlockChanged", SendMessageOptions.DontRequireReceiver);

        died = false;
        sessionMeters = 0.0f;
    }

    public void EndRunning()
    {
        gameObject.SendMessage("OnEndRunning", SendMessageOptions.DontRequireReceiver);
        Camera.main.gameObject.SendMessage("OnEndRunning", SendMessageOptions.DontRequireReceiver);

        activeBranch = null;
        prevBlock = null;
        activeBlock = null;
        tokenLongitudinal = 0.0f;
        tokenTrasversal = 0.0f;
        lastTokenIndex = 0;
        trackPos = 0.0f;
        startTrackPos = 0.0f;
        sessionMeters = 0.0f;

        //gameObject.SendMessage("OnEndRunning");
#if UNITY_4_3
        gameObject.SetActive(true);
#else
        gameObject.SetActiveRecursively(false);
#endif
    }

    public virtual int GetDifficultyValue()
    {
        return Mathf.Min(100, Mathf.RoundToInt(sessionMeters * 0.05f));
    }
    #endregion

    #region Protected methods
    protected virtual bool SelectBranch(NeverendingBlock block, int linkIndex)
    {
        return true;
    }
    #endregion

    #region Messages
    void OnInit()
    { }

    void OnTutorialEnd()
    {
        startTrackPos = trackPos;
    }

    void OnDie()
    {
        died = true;
    }

    void OnDieFinal()
    { }

    void OnGameover()
    { }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        tutorial = gameObject.GetComponent<NeverendingTutorial>();
    }

    void LateUpdate()
    {
        if (null == activeBranch)
            return;

        SBSVector3 pos = transform.position;
        lastToken = null;

        lastToken = activeBranch.trackBranch[lastTokenIndex];
        lastToken.WorldToToken(pos, out tokenLongitudinal, out tokenTrasversal);
        while (tokenLongitudinal > 1.0f)
        {
            ++lastTokenIndex;
            if (lastTokenIndex < activeBranch.trackBranch.Count)
            {
                lastToken = activeBranch.trackBranch[lastTokenIndex];

                if (activeBlock.next != null && lastToken == activeBlock.next.component.firstToken)
                {
                    activeBlock = activeBlock.next;
                    blockWorld = activeBlock.transform.localToWorldMatrix;

                    gameObject.SendMessage("OnBlockChanged", SendMessageOptions.DontRequireReceiver);
                }

                lastToken.WorldToToken(pos, out tokenLongitudinal, out tokenTrasversal);
            }
            else
            {
                bool branchChanged = false;
                NeverendingBlock blockRef = activeBlock.component;
                int i = 1;
                for (; i < blockRef.links.Length; ++i)
                {
                    if (this.SelectBranch(blockRef, i))
                    {
                        activeBranch = blockRef.links[i].branch;
                        Debug.Log("Choosed link #" + (i - 1) + ", Token " + activeBranch.trackBranch[0].name);
                        lastTokenIndex = 0;
                        branchChanged = true;
                        break;
                    }
                }

                Asserts.Assert(0 == lastTokenIndex);
                lastToken = activeBranch.trackBranch[lastTokenIndex];
                lastToken.WorldToToken(pos, out tokenLongitudinal, out tokenTrasversal);

                if (branchChanged)
                    gameObject.SendMessage("OnBranchChanged", SendMessageOptions.DontRequireReceiver);
            }
        }

        if (lastToken != null)
        {
#if UNITY_FLASH
            lastToken.TokenToWorld(tokenLongitudinal, tokenTrasversal, tokenPos, tokenTan);
#else
            lastToken.TokenToWorld(tokenLongitudinal, tokenTrasversal, out tokenPos, out tokenTan);
#endif
            trackPos = activeBranch.trackBranch.GetTrackPosition(lastToken, tokenLongitudinal);
            //tokenNormal = lastToken.Normal;
            //Debug.Log(tokenLongitudinal + ", " + tokenTrasversal);
        }
        if (tutorial != null)
        {
            if (!died && !tutorial.IsActive)
                sessionMeters = trackPos - startTrackPos;
        }
        else
        {
            if (!died)
                sessionMeters = trackPos - startTrackPos;
        }
    }
    #endregion
}
