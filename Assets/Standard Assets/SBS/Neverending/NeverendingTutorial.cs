using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

[AddComponentMenu("SBS/Neverending/NeverendingTutorial")]
[RequireComponent(typeof(NeverendingPlayer))]
public class NeverendingTutorial : MonoBehaviour
{
    [Serializable]
    public class TutorialBlock
    {
        public int blockId;
        public int blockLayer;
        protected NeverendingTrack.BlockNode blockNode = null;

        public NeverendingTrack.BlockNode BlockNode
        {
            get
            {
                return blockNode;
            }
            set
            {
                blockNode = value;
            }
        }
    }

    [Serializable]
    public class TutorialSequence
    {
        public TutorialBlock[] blocks;
    }

    public TutorialSequence[] sequences;

    protected bool isActive = true;
    protected NeverendingPlayer controller;
    protected NeverendingTrack.BlockNode prevBlock;

    protected int trackSequenceIndex;
    protected int trackBlockIndex;

    protected int ctrlSequenceIndex;
    protected int ctrlBlockIndex;

    protected float seqStartTrackPos = -1.0f;
    protected float seqEndTrackPos = -1.0f;

    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            bool prevIsActive = isActive;
            isActive = value;
            PlayerPrefs.SetInt("tutorialActive", isActive ? 1 : 0);
            if (!prevIsActive && isActive)
            {
                trackSequenceIndex = ctrlSequenceIndex = 0;
                trackBlockIndex = ctrlBlockIndex = 0;
            }

            if (prevIsActive != isActive)
                gameObject.SendMessage("OnTutorialActive", isActive);
        }
    }

    public bool AllSequencesPlaced
    {
        get
        {
            return trackSequenceIndex < 0;
        }
    }

    public TutorialBlock GetTutorialBlock()
    {
        return sequences[trackSequenceIndex].blocks[trackBlockIndex];
    }

    public void SetTutorialBlockNode(NeverendingTrack.BlockNode node)
    {
        sequences[trackSequenceIndex].blocks[trackBlockIndex].BlockNode = node;
        if (0 == trackBlockIndex)
            seqStartTrackPos = node.component.GetMaxEndTrackPosition() - 6.0f;
        if ((sequences[trackSequenceIndex].blocks.Length - 1) == trackBlockIndex)
            seqEndTrackPos = node.component.GetMaxEndTrackPosition() - 6.0f;
        this.NextBlock();
    }

    protected void UpdateCourse()
    {
        Debug.Log("ctrlSequenceIndex: " + ctrlSequenceIndex + ", ctrlBlockIndex: " + ctrlBlockIndex);
        Asserts.Assert(prevBlock == sequences[ctrlSequenceIndex].blocks[ctrlBlockIndex].BlockNode);

        sequences[ctrlSequenceIndex].blocks[ctrlBlockIndex].BlockNode = null;
        ++ctrlBlockIndex;
        TutorialSequence seq = sequences[ctrlSequenceIndex];
        if (seq.blocks.Length == ctrlBlockIndex)
        {
            ++ctrlSequenceIndex;
            gameObject.SendMessage("OnTutorialSequenceDone");
            if (sequences.Length == ctrlSequenceIndex)
            {
                gameObject.SendMessage("OnTutorialEnd");
                NeverendingTrack.Instance.gameObject.SendMessage("OnTutorialEnd");
            }
            ctrlBlockIndex = 0;
        }
    }

    protected void NextBlock()
    {
        if (0 == sequences.Length || trackSequenceIndex < 0)
            return;

        ++trackBlockIndex;
        TutorialSequence seq = sequences[trackSequenceIndex];
        if (seq.blocks.Length == trackBlockIndex)
        {
            ++trackSequenceIndex;
            if (sequences.Length == trackSequenceIndex)
            {
                NeverendingTrack.Instance.gameObject.SendMessage("OnAllSequencesPlaced");
                trackSequenceIndex = -1;
            }
            trackBlockIndex = 0;
        }
    }

    void OnTutorialSequenceDone()
    {
        Debug.Log("OnTutorialSequenceDone");
    }

    void OnSequenceStart(int seqIndex)
    {
        Debug.Log("OnSequenceStart(" + seqIndex + ")");
    }

    void OnSequenceEnd(int seqIndex)
    {
        Debug.Log("OnSequenceEnd(" + seqIndex + ")");
    }

    void OnTutorialEnd()
    {
        Debug.Log("OnTutorialEnd");

        this.IsActive = false;
    }

    public virtual void BeforeStartRunning()
    { }

    void OnStartRunning()
    {
        prevBlock = controller.ActiveBlock;
    }

    void OnBlockChanged()
    {
        if (!isActive)
            return;

        if (prevBlock != controller.ActiveBlock)
        {
            if (!controller.IsDied)
                this.UpdateCourse();
            prevBlock = controller.ActiveBlock;
        }
    }

    void OnRailChanged()
    {
        if (!isActive)
            return;

        if (prevBlock != controller.ActiveBlock)
        {
            if (!controller.IsDied)
                this.UpdateCourse();
            prevBlock = controller.ActiveBlock;
        }
    }

    void OnGameover()
    {
        if (!isActive)
            return;

        trackSequenceIndex = ctrlSequenceIndex;
        trackBlockIndex = 0;
        ctrlBlockIndex = 0;
    }

    void OnDieFinal()
    {
        if (!isActive)
            return;

        UIManager_OLD.Instance.StartFadeOut(0.5f, onFadeOutCompleted);
    }

    void onFadeOutCompleted()
    {
        LevelRoot.Instance.Root.BroadcastMessage("OnGameover", SendMessageOptions.DontRequireReceiver);
        LevelRoot.Instance.Root.BroadcastMessage("OnStartGame", SendMessageOptions.DontRequireReceiver);

        UIManager_OLD.Instance.StartFadeIn(0.5f, null);
    }

    void Awake()
    {
        isActive = (1 == PlayerPrefs.GetInt("tutorialActive", 1));

        if (0 == sequences.Length)
            isActive = false;

        controller = gameObject.GetComponent<NeverendingPlayer>();

        trackSequenceIndex = ctrlSequenceIndex = 0;
        trackBlockIndex = ctrlBlockIndex = 0;
    }

    void Update()
    {
        if (!isActive)
            return;

        if (seqStartTrackPos > 0.0f && controller.TrackPos >= seqStartTrackPos)
        {
            gameObject.SendMessage("OnSequenceStart", ctrlSequenceIndex);
            seqStartTrackPos = -1.0f;
        }

        if (seqEndTrackPos > 0.0f && controller.TrackPos >= seqEndTrackPos)
        {
            gameObject.SendMessage("OnSequenceEnd", ctrlSequenceIndex);
            seqEndTrackPos = -1.0f;
        }
    }
}
