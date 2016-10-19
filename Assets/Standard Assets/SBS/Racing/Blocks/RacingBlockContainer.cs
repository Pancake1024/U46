using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;

[AddComponentMenu("SBS/Racing/Blocks/RacingBlockContainer")]
public class RacingBlockContainer : MonoBehaviour
{
    #region Public members
//  public int selectedBlock;
    public RacingBlock selectedBlockPrefab;
    public RacingBlockContainer prev;
    public RacingBlockContainer next;
    public RacingBlock block;

    public bool resetTokenVerticalOrientation = false;
    #endregion

    #region Public methods
    public void SetPrevious(RacingBlockContainer newPrev)
    {
        if (newPrev == this)
            return;

        if (prev != null)
        {
            if (block != null && prev.block != null)
            {
                block.firstToken.prevLinks.Remove(prev.block.lastToken);
                prev.block.lastToken.nextLinks.Remove(block.firstToken);
            }

            prev.next = null;
        }

        prev = newPrev;
        if (prev != null)
        {
            if (block != null && prev.block != null)
            {
                block.firstToken.prevLinks.Add(prev.block.lastToken);
                prev.block.lastToken.nextLinks.Add(block.firstToken);
            }

            RacingBlockContainer oldNext = prev.next;
            if (oldNext != null && oldNext != this)
            {
                if (oldNext.block != null && prev.block != null)
                {
                    oldNext.block.firstToken.prevLinks.Remove(prev.block.lastToken);
                    prev.block.lastToken.nextLinks.Remove(oldNext.block.firstToken);
                }

                oldNext.prev = null;
            }

            prev.next = this;
        }
    }

    public void SetNext(RacingBlockContainer newNext)
    {
        if (newNext == this)
            return;

        if (next != null)
        {
            if (block != null && next.block != null)
            {
                block.lastToken.nextLinks.Remove(next.block.firstToken);
                next.block.firstToken.prevLinks.Remove(block.lastToken);
            }

            next.prev = null;
        }

        next = newNext;
        if (next != null)
        {
            if (block != null && next.block != null)
            {
                block.lastToken.nextLinks.Add(next.block.firstToken);
                next.block.firstToken.prevLinks.Add(block.lastToken);
            }

            RacingBlockContainer oldPrev = next.prev;
            if (oldPrev != null && oldPrev != this)
            {
                if (oldPrev.block != null && next.block != null)
                {
                    oldPrev.block.lastToken.nextLinks.Remove(next.block.firstToken);
                    next.block.firstToken.prevLinks.Remove(oldPrev.block.lastToken);
                }

                oldPrev.next = null;
            }

            next.prev = this;
        }
    }

    public void MoveNextBlocks()
    {
        if (null == block)
            return;

        RacingBlockContainer node = next;
#if UNITY_FLASH
        SBSVector3 p = new SBSVector3(), t = new SBSVector3();
        block.lastToken.TokenToWorld(1.0f, 0.0f, p, t);
#else
        SBSVector3 p, t;
        block.lastToken.TokenToWorld(1.0f, 0.0f, out p, out t);
#endif

        if (resetTokenVerticalOrientation)
            t = new Vector3(t.x, 0.0f, t.z).normalized;

        SBSMatrix4x4 mat = SBSMatrix4x4.LookAt(p, p + t, SBSVector3.up);
        while (node != null)
        {
            node.transform.position = mat.position;
            node.transform.rotation = mat.rotation;
            node.gameObject.BroadcastMessage("OnMove");

            if (node.block != null)
            {
#if UNITY_FLASH
                node.block.lastToken.TokenToWorld(1.0f, 0.0f, p, t);
#else
                node.block.lastToken.TokenToWorld(1.0f, 0.0f, out p, out t);
#endif
                if (resetTokenVerticalOrientation)
                    t = new Vector3(t.x, 0.0f, t.z).normalized;

                mat = SBSMatrix4x4.LookAt(p, p + t, SBSVector3.up);
            }

            node = node.next;
        }
    }

    public GameObject InsertBefore()
    {
        SBSMatrix4x4 mat;
        if (block != null)
        {
#if UNITY_FLASH
            SBSVector3 p = new SBSVector3(), t = new SBSVector3();
            block.lastToken.TokenToWorld(0.0f, 0.0f, p, t);
#else
            SBSVector3 p, t;
            block.lastToken.TokenToWorld(0.0f, 0.0f, out p, out t);
#endif
            if (resetTokenVerticalOrientation)
                t = new Vector3(t.x, 0.0f, t.z).normalized;

            mat = SBSMatrix4x4.LookAt(p, p + t, SBSVector3.up);
        }
        else
        {
            mat = transform.localToWorldMatrix;
        }

        RacingBlockContainer newContainer = new GameObject("BlockNode", typeof(RacingBlockContainer)).GetComponent<RacingBlockContainer>();
        newContainer.next = this;
        if (prev != null)
        {
            prev.next = newContainer;
            if (block != null && prev.block != null)
            {
                prev.block.lastToken.nextLinks.Remove(block.firstToken);
                block.firstToken.prevLinks.Remove(prev.block.lastToken);
            }
        }
        newContainer.prev = prev;
        prev = newContainer;

        newContainer.transform.position = mat.position;
        newContainer.transform.rotation = mat.rotation;
        newContainer.transform.parent = transform.parent;

        return newContainer.gameObject;
    }

    public GameObject InsertAfter()
    {
        SBSMatrix4x4 mat;
        if (block != null)
        {
#if UNITY_FLASH
            SBSVector3 p = new SBSVector3(), t = new SBSVector3();
            block.lastToken.TokenToWorld(1.0f, 0.0f, p, t);
#else
            SBSVector3 p, t;
            block.lastToken.TokenToWorld(1.0f, 0.0f, out p, out t);
#endif

            if (resetTokenVerticalOrientation)
                t = new Vector3(t.x, 0.0f, t.z).normalized;

            mat = SBSMatrix4x4.LookAt(p, p + t, SBSVector3.up);
        }
        else
        {
            mat = transform.localToWorldMatrix;
        }

        RacingBlockContainer newContainer = new GameObject("BlockNode", typeof(RacingBlockContainer)).GetComponent<RacingBlockContainer>();
        newContainer.prev = this;
        if (next != null)
        {
            next.prev = newContainer;
            if (block != null && next.block != null)
            {
                next.block.firstToken.prevLinks.Remove(block.lastToken);
                block.lastToken.nextLinks.Remove(next.block.firstToken);
            }
        }
        newContainer.next = next;
        next = newContainer;

        newContainer.transform.position = mat.position;
        newContainer.transform.rotation = mat.rotation;
        newContainer.transform.parent = transform.parent;

        return newContainer.gameObject;
    }

    public void Remove()
    {
        if (prev != null)
        {
            prev.next = next;
            if (prev.block != null)
                prev.block.lastToken.nextLinks.Remove(block.firstToken);
        }

        if (next != null)
        {
            next.prev = prev;
            if (next.block != null)
                next.block.firstToken.prevLinks.Remove(block.lastToken);
        }
    }
    #endregion
}
