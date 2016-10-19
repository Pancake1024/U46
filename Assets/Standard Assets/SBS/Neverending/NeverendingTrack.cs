using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Neverending/NeverendingTrack")]
public class NeverendingTrack : MonoBehaviour
{
    #region Singleton instance
    protected static NeverendingTrack instance = null;

    public static NeverendingTrack Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

#if UNITY_FLASH
    protected class IntNode
    {
        public int Value = 0;
        public IntNode Prev = null;
        public IntNode Next = null;

        public IntNode()
        { }

        public IntNode(int value)
        {
            Value = value;
        }
    }

    protected class IntList
    {
        public IntNode First = null;
        public IntNode Last = null;

        public void Remove(IntNode node)
        {
            if (node.Prev != null)
                node.Prev.Next = node.Next;
            if (node.Next != null)
                node.Next.Prev = node.Prev;

            if (First == node)
                First = node.Next;
            if (Last == node)
                Last = node.Prev;

            node.Prev = null;
            node.Next = null;
        }

        public void AddLast(IntNode node)
        {
            if (null == First)
            {
                First = node;
                Last = node;
            }
            else
            {
                node.Prev = Last;
                Last.Next = node;
                Last = node;
            }
        }

        public void AddLast(int value)
        {
            this.AddLast(new IntNode(value));
        }
    }

    private static IntList Shuffle(IntList c)
    {
        return c;//FLASH ToDo
    }
#else
    private static System.Random rng = new System.Random();
    private static LinkedList<int> Shuffle(LinkedList<int> c)
    {
        int[] a = new int[c.Count];
        c.CopyTo(a, 0);
        byte[] b = new byte[a.Length];
        rng.NextBytes(b);
        Array.Sort(b, a);
        return new LinkedList<int>(a);
    }
#endif

    #region Public classes
    public class BlockNode
    {
        public GameObject block = null;
        public BlockNode prev = null;
        public BlockNode next = null;
        public Transform transform = null;
        public NeverendingBlock component = null;
        public NeverendingObject[] objects = null;

        public BlockNode(GameObject _block)
        {
            block     = _block;
            transform = _block.transform;
            component = _block.GetComponent<NeverendingBlock>();
            objects   = _block.GetComponentsInChildren<NeverendingObject>();
        }
    }

    public class BlocksBranch
    {
        public BlockNode head = null;
        public BlockNode tail = null;
        public BlocksBranch prev = null;
        public BlocksBranch[] links = null;
        public TrackBranch trackBranch = null;
    }

    public class OpenBranch
    {
        public BlocksBranch branch = null;
        public NeverendingBlock lastBlock = null;
        public int lastBlockLinkIndex = 0;

        public OpenBranch(BlocksBranch _branch)
        {
            branch = _branch;
        }

        public OpenBranch(BlocksBranch _branch, NeverendingBlock _lastBlock, int linkIndex)
        {
            branch = _branch;
            lastBlock = _lastBlock;
            lastBlockLinkIndex = linkIndex;
        }
    }
    #endregion

    #region Public members
    public NeverendingBlock[] blocks;
    public int randomSetSize = 5;
    public NeverendingPlayer controller;
    public float minDistance = 25.0f;
    public float distForDeletion = 10.0f;
    public float relaxDeltaTimeMin = 35.0f;
    public float relaxDeltaTimeMax = 45.0f;
    public float relaxLengthRatio = 3.0f;
    public float noRelaxAfterMeters = 2000.0f;
    #endregion

    #region Protected members
    protected NeverendingTutorial ctrlTutorial;

    protected bool gameStarted = false;

    protected List<GameObject>[] blocksCache = null;
    protected List<BlocksBranch> branches = null;

    protected List<OpenBranch> openBranches = null;
#if UNITY_FLASH
    protected IntList blockIdsLRU = new IntList();
#else
    protected LinkedList<int> blockIdsLRU = new LinkedList<int>();
#endif
    protected float nextEnvChange = 0.0f;

    protected float nextRelaxTime = -1.0f;
    protected float relaxRailsLength = -1.0f;

    protected float startRunningTime = -1.0f;
    protected float voidRailsLength = -1.0f;
    #endregion

    #region Public properties
    public BlocksBranch FirstBranch
    {
        get
        {
            return branches[0];
        }
    }
    #endregion

    #region Protected methods
    protected int FindOpenBranchIndex(BlocksBranch branch)
    {
        int count = openBranches.Count;
        for (int i = 0; i < count; ++i)
        {
            if (openBranches[i].branch == branch)
                return i;
        }

        return -1;
    }

    protected GameObject GetFreeBlock(int id)
    {
        List<GameObject> cache = blocksCache[id];
        foreach (GameObject go in cache)
        {
            NeverendingBlock block = go.GetComponent<NeverendingBlock>();
            if (!block.used)
            {
                block.used = true;
#if UNITY_4_3
                go.SetActive(true);
#else
                go.SetActiveRecursively(true);
#endif
				return go;
            }
        }

        Debug.Log("New block #" + id);
        GameObject newGO = GameObject.Instantiate(blocks[id].gameObject) as GameObject;
        newGO.GetComponent<NeverendingBlock>().used = true;
        cache.Add(newGO);

        return newGO;
    }

    protected virtual List<TrackBranch> PlaceBlock(OpenBranch openBranch, TrackBranch activeBranch, GameObject prevBlock, GameObject newBlock, int nextLinkIndex)
    {
        NeverendingBlock newBlockRef = newBlock.GetComponent<NeverendingBlock>(),
                         prevBlockRef = (null == prevBlock ? null : prevBlock.GetComponent<NeverendingBlock>());
        float prevEndTrackPos = (null == prevBlockRef ? 0.0f : prevBlockRef.GetEndTrackPosition(nextLinkIndex));
        NeverendingBlockLink prevLink = (null == prevBlock ? null : prevBlockRef.links[nextLinkIndex + 1]);
        if (prevLink != null)
        {
            SBSMatrix4x4 matLinkWorld0 = prevLink.transform.localToWorldMatrix,
                         matLinkWorld1 = SBSMatrix4x4.TRS(newBlockRef.links[0].transform.localPosition, newBlockRef.links[0].transform.localRotation, SBSVector3.one);

            matLinkWorld1.InvertFast();
            matLinkWorld1.Prepend(matLinkWorld0);

            newBlock.transform.position = matLinkWorld1.position;
            newBlock.transform.rotation = Quaternion.identity;// matLinkWorld1.rotation;
        }
        else
        {
            newBlock.transform.position = Vector3.zero;
            newBlock.transform.rotation = Quaternion.identity;
        }

        bool placeRelaxBlock = false,
             useVoidLayer = false;
        if (relaxRailsLength > 0.0f || voidRailsLength > 0.0f)
        {
            placeRelaxBlock = true;
            useVoidLayer = (voidRailsLength > 0.0f);
        }

        if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
            newBlockRef.activeLayer = ctrlTutorial.GetTutorialBlock().blockLayer;
        else if (newBlockRef.Layers.Length > 0 && !placeRelaxBlock)
        {
            int count = newBlockRef.Layers.Length,
                startIndex = UnityEngine.Random.Range(0, count),
                diff = controller.GetDifficultyValue(),
                offset = 0;
            while ((!newBlockRef.IsDifficultyInRange((startIndex + offset) % count, diff) || newBlockRef.Layers[(startIndex + offset) % count].name.Contains("Relax")) && offset < count)
                ++offset;
            Asserts.Assert(offset < count, "Cound't find a groupId for difficulty " + diff);
            newBlockRef.activeLayer = (startIndex + offset) % count;
        }
        else
        {
            if (placeRelaxBlock && !useVoidLayer)
            {
                // search Relax layer
                int l = 0, len = newBlockRef.Layers.Length;
                for (; l < len; ++l)
                {
                    if (newBlockRef.Layers[l].name.Contains("Relax"))
                        break;
                }
                newBlockRef.activeLayer = (l < len ? l : -1);
            }
            else
                newBlockRef.activeLayer = -1;
        }

        newBlock.BroadcastMessage("OnMove", SendMessageOptions.DontRequireReceiver);

        List<TrackBranch> newBranches = newBlockRef.AddToBranch(activeBranch);

        float maxEndTrackPos = 0.0f;
        for (int i = newBlockRef.links.Length - 1; i >= 1; --i)
        {
            float endTrackPos = newBlockRef.GetEndTrackPosition(i - 1);
            if (endTrackPos > maxEndTrackPos)
                maxEndTrackPos = endTrackPos;
        }

        float blockLen = (maxEndTrackPos - prevEndTrackPos);

        if (placeRelaxBlock)
        {
            if (useVoidLayer)
            {
                voidRailsLength -= blockLen;
                if (voidRailsLength < 0.0f)
                    voidRailsLength = -1.0f;
            }
            else
            {
                relaxRailsLength -= blockLen;
                if (relaxRailsLength < 0.0f)
                    relaxRailsLength = -1.0f;
            }
        }

        return newBranches;
    }

    protected List<TrackBranch> PlaceBlock(OpenBranch openBranch, TrackBranch activeBranch, GameObject prevBlock, GameObject newBlock)
    {
        return this.PlaceBlock(openBranch, activeBranch, prevBlock, newBlock, 0);
    }

    protected void PushBlock(int openBranchIndex, int nextLinkIndex)
    {
        int id = this.GetRandomBlockId(openBranchIndex, nextLinkIndex);
        OpenBranch openBranch = openBranches[openBranchIndex];
        BlocksBranch branch = openBranch.branch;
        if (null == branch.head)
        {
            // first block
#if UNITY_FLASH
            BlockNode firstNode = new BlockNode(this.GetFreeBlock(id));
            branch.head = firstNode;
            branch.tail = firstNode;
#else
            BlockNode firstNode = branch.head = branch.tail = new BlockNode(this.GetFreeBlock(id));
#endif
            if (null == branch.prev)
                this.PlaceBlock(openBranch, branch.trackBranch, null, firstNode.block);
            else
            {
                BlocksBranch prev = branch.prev;
                int linkIndex = 0, linksCount = prev.links.Length;
                for (; linkIndex < linksCount; ++linkIndex)
                {
                    if (branch == prev.links[linkIndex])
                        break;
                }
                Asserts.Assert(linkIndex < linksCount);
                this.PlaceBlock(openBranch, branch.trackBranch, prev.tail.block, firstNode.block, linkIndex);
            }

            openBranches[openBranchIndex].lastBlock = branch.head.component;
            openBranches[openBranchIndex].lastBlockLinkIndex = 0;

            if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
                ctrlTutorial.SetTutorialBlockNode(firstNode);

            return;
        }

        // add in active branch
        BlockNode prevNode = branch.tail;

        BlockNode newNode = prevNode.next = new BlockNode(this.GetFreeBlock(id));
        newNode.prev = prevNode;

        branch.tail = newNode;
        List<TrackBranch> newTrackBranches = this.PlaceBlock(openBranch, branch.trackBranch, prevNode.block, newNode.block);

        openBranches[openBranchIndex].lastBlock = newNode.component;
        openBranches[openBranchIndex].lastBlockLinkIndex = 0;

        if (blocks[id].links.Length > 2)
        {
            // open N new branches
            int numLinks = blocks[id].links.Length - 1;
            Asserts.Assert(newTrackBranches != null && newTrackBranches.Count == numLinks);

            NeverendingBlock lastBlock = openBranches[openBranchIndex].lastBlock;
            openBranches.RemoveAt(openBranchIndex);

            branch.links = new BlocksBranch[numLinks];

            for (int i = 0; i < numLinks; ++i)
            {
                int j = 0;
                for (; j < numLinks; ++j)
                {
                    if (lastBlock.links[j + 1].firstBranchToken == newTrackBranches[i][0])
                    {
#if UNITY_FLASH
                        BlocksBranch newBranch = new BlocksBranch();
                        lastBlock.links[j + 1].branch = newBranch;
                        branch.links[j] = newBranch;
#else
                        lastBlock.links[j + 1].branch = branch.links[j] = new BlocksBranch();
#endif
                        Debug.Log("Link #" + j + " got branch #" + i);
                        break;
                    }
                }

                Asserts.Assert(j < numLinks);
                branch.links[j].prev = branch;
                branch.links[j].trackBranch = newTrackBranches[i];

                openBranches.Add(new OpenBranch(branch.links[j], lastBlock, j));
                branches.Add(branch.links[j]);
            }
        }

        if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
            ctrlTutorial.SetTutorialBlockNode(newNode);
    }

    protected bool PopBlock(BlocksBranch branch)
    {
        GameObject block = branch.head.block;
        block.GetComponent<NeverendingBlock>().used = false;
        block.BroadcastMessage("OnPopFromTrack", SendMessageOptions.DontRequireReceiver);

#if UNITY_4_3
        block.SetActive(false);
#else
		block.SetActiveRecursively(false);
#endif
        

        branch.head.block = null;
        if (branch.head == branch.tail)
        {
            //          Debug.Log("Removed branch #" + branches.IndexOf(branch));
            // branch cleared, remove from list
            if (branch.links != null)
            {
                foreach (BlocksBranch link in branch.links)
                    link.prev = null;
            }

            branches.Remove(branch);

            int openBranchIndex = this.FindOpenBranchIndex(branch);
            if (openBranchIndex >= 0)
                openBranches.RemoveAt(openBranchIndex);

            return true;
        }
        else
        {
            // remove node
            branch.head = branch.head.next;
            branch.head.prev = null;
            return false;
        }
    }

    protected void ClearUnlinkedBranches()
    {
        for (int i = branches.Count - 1; i >= 0; --i)
        {
            BlocksBranch branch = branches[i];

            if (null == branch.head || controller.ActiveBranch == branch || controller.ActiveBlock == branch.tail)
                continue;

            if (null == branch.prev)
            {
                Debug.Log("Clearing branch #" + i);
                while (!this.PopBlock(branch)) ;
            }
        }
    }

    protected virtual int GetRandomBlockId(int openBranchIndex, int nextLinkIndex)
    {
        if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
            return ctrlTutorial.GetTutorialBlock().blockId;

        int currDiff = controller.GetDifficultyValue();
        NeverendingBlock lastBlock = openBranches[openBranchIndex].lastBlock;
        NeverendingBlockLink currentLink = (null == lastBlock ? null : lastBlock.links[nextLinkIndex + 1]);

#if UNITY_FLASH
        IntNode[] bestIds = new IntNode[randomSetSize];
#else
        LinkedListNode<int>[] bestIds = new LinkedListNode<int>[randomSetSize];
#endif
        int i, bestIdCounter = 0;

#if UNITY_FLASH
        IntNode node = blockIdsLRU.First;
#else
        LinkedListNode<int> node = blockIdsLRU.First;
#endif
        while (node != null)
        {
            NeverendingBlock blockRef = blocks[node.Value];

            bool willUseRelaxLayer = (relaxRailsLength > 0.0f || voidRailsLength > 0.0f);
            bool blockOk = (willUseRelaxLayer || blockRef.IsDifficultyInRange(currDiff));

            if (blockOk && willUseRelaxLayer)
                blockOk = !blockRef.excludeFromRelax;

            if (blockOk)
            {
                if (blockOk && lastBlock != null)
                {
                    // black & white list
                    int listCount = currentLink.blackList.Count;
                    for (i = 0; i < listCount; ++i)
                    {
                        if (node.Value == currentLink.blackList[i].id)
                        {
                            blockOk = false;
                            break;
                        }
                    }

                    listCount = currentLink.whiteList.Count;
                    if (listCount > 0)
                    {
                        blockOk = false;
                        for (i = 0; i < listCount; ++i)
                        {
                            if (node.Value == currentLink.whiteList[i].id)
                            {
                                blockOk = true;
                                break;
                            }
                        }
                    }
                }
            }

#if UNITY_FLASH
            IntNode prevNode = node;
#else
            LinkedListNode<int> prevNode = node;
#endif
            node = node.Next;

            if (blockOk)
            {
                bestIds[bestIdCounter++] = prevNode;

                if (randomSetSize == bestIdCounter)
                    break;
            }
        }

#if UNITY_FLASH
        IntNode bestNode = null;
#else
        LinkedListNode<int> bestNode = null;
#endif
        int startIdx = UnityEngine.Random.Range(0, randomSetSize),
            count = 0;
        while (null == bestNode && count < randomSetSize)
        {
            int index = (startIdx + (count++)) % randomSetSize;
            bestNode = bestIds[index];
        }

        blockIdsLRU.Remove(bestNode);
        blockIdsLRU.AddLast(bestNode);

        return bestNode.Value;
    }
    #endregion

    #region Messages
    void OnStartGame()
    {
        
        gameStarted = true;
      
        branches = new List<BlocksBranch>();
        branches.Add(new BlocksBranch());
        branches[0].trackBranch = new TrackBranch();
       
        openBranches = new List<OpenBranch>();
        openBranches.Add(new OpenBranch(branches[0]));

        for (int i = 0; i < 2; ++i)
            this.PushBlock(0, 0);
       
        nextEnvChange = UnityEngine.Random.Range(29, 51) * 10.0f; //18,40

        startRunningTime = TimeManager.Instance.MasterSource.TotalTime;
        nextRelaxTime = startRunningTime + UnityEngine.Random.Range(relaxDeltaTimeMin, relaxDeltaTimeMax);
       
        controller.StartRunning(branches[0]);
    }

    void OnTutorialEnd()
    {
        nextEnvChange = controller.TrackPos + UnityEngine.Random.Range(29, 51) * 10.0f;
        startRunningTime = TimeManager.Instance.MasterSource.TotalTime;
        nextRelaxTime = startRunningTime + UnityEngine.Random.Range(relaxDeltaTimeMin, relaxDeltaTimeMax);
    }

    void OnGameover()
    {
        controller.EndRunning();

        int i = blocksCache.Length - 1;
        for (; i >= 0; --i)
        {
            foreach (GameObject go in blocksCache[i])
            {
                go.GetComponent<NeverendingBlock>().used = false;
                go.BroadcastMessage("OnPopFromTrack", SendMessageOptions.DontRequireReceiver);

#if UNITY_4_3
                go.SetActive(false);
#else
				go.SetActiveRecursively(false);
#endif
            }
        }

        nextRelaxTime = -1.0f;
        relaxRailsLength = -1.0f;
        voidRailsLength = -1.0f;

        gameStarted = false;
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        ctrlTutorial = (null == controller ? null : controller.gameObject.GetComponent<NeverendingTutorial>());
    }

    void Start()
    {
        int i = 0,
            cacheInitialSize = 0,
            countInQueue = 0;

        blocksCache = new List<GameObject>[blocks.Length];

        foreach (NeverendingBlock block in blocks)
        {
            block.id = i;

            switch (block.frequency)
            {
                case NeverendingBlock.Frequency.Lowest:
                    countInQueue = 1;
                    break;
                case NeverendingBlock.Frequency.Low:
                    countInQueue = 3;
                    break;
                case NeverendingBlock.Frequency.Medium:
                    countInQueue = 5;
                    break;
                case NeverendingBlock.Frequency.High:
                    countInQueue = 7;
                    break;
                case NeverendingBlock.Frequency.Highest:
                    countInQueue = 9;
                    break;
            }

            switch (block.cacheSize)
            {
                case NeverendingBlock.CacheSize.Small:
                    cacheInitialSize = 1;//2;
                    break;
                case NeverendingBlock.CacheSize.Medium:
                    cacheInitialSize = 2;//3;
                    break;
                case NeverendingBlock.CacheSize.Big:
                    cacheInitialSize = 3;//4;
                    break;
            }

            for (int k = 0; k < countInQueue; ++k)
                blockIdsLRU.AddLast(i);

#if UNITY_FLASH
            List<GameObject> list = new List<GameObject>();
            blocksCache[i++] = list;
#else
            List<GameObject> list = blocksCache[i++] = new List<GameObject>();
#endif

            for (int j = 0; j < cacheInitialSize; ++j)
            {
                GameObject instance = GameObject.Instantiate(block.gameObject) as GameObject;
                NeverendingBlock blockInstance = instance.GetComponent<NeverendingBlock>();

                blockInstance.used = false;
                blockInstance.id = block.id;

                blockInstance.toDelete = false;

                list.Add(instance);
            }

            foreach (GameObject go in list)
            {
#if UNITY_4_3
                go.SetActive(false);
#else
				go.SetActiveRecursively(false);
#endif        
			}
        }
      
        blockIdsLRU = Shuffle(blockIdsLRU);
    }

    void Update()
    {
        if (!gameStarted)
            return;

        NeverendingBlock headBlock = this.FirstBranch.head.component;
        if (!headBlock.toDelete && controller.TrackPos > headBlock.GetMaxEndTrackPosition() + distForDeletion)
            headBlock.toDelete = true;

        NeverendingBlock oldestBlock = this.FirstBranch.head.component;
        if (oldestBlock.toDelete)
            this.PopBlock(this.FirstBranch);

        this.ClearUnlinkedBranches();

        for (int i = openBranches.Count - 1; i >= 0; --i)
        {
            OpenBranch openBranch = openBranches[i];
            NeverendingBlock lastBlock = openBranch.lastBlock;
            if (lastBlock != null && controller.TrackPos > (lastBlock.GetMaxEndTrackPosition() - minDistance))
            {           
                this.PushBlock(i, openBranch.lastBlockLinkIndex);
            }
        }

        if (!ctrlTutorial.IsActive && controller.SessionMeters < noRelaxAfterMeters && nextRelaxTime > 0.0f && TimeManager.Instance.MasterSource.TotalTime >= nextRelaxTime && voidRailsLength < 0.0f)
        {
            relaxRailsLength = 100.0f;// controller.CurrPlainVelocity* relaxLengthRatio; ToDo
            nextRelaxTime = TimeManager.Instance.MasterSource.TotalTime + UnityEngine.Random.Range(relaxDeltaTimeMin, relaxDeltaTimeMax);
        }
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
