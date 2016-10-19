using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Racing;

public class NeverendingTrackModified : MonoBehaviour
{
    #region Singleton instance
    protected static NeverendingTrackModified instance = null;

    public static NeverendingTrackModified Instance
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

    #region Public members
    public enum AmbientName
    {
        BEACH = 0,
        EU_CITY,
        EU_TUNNEL,
        EU_UPTOWN,
        AS_OLDCITY,
        AS_DOWNTOWN,
        AS_TUNNEL,
        AS_BRIDGE,
        US_CANYON,
        US_DESERT,
        US_SMALLTOWN,
        US_TUNNEL,
        NY_BRIDGE,
        NY_TUNNEL,
        NY_CITY,
        NY_HARBOUR
    }

    [System.Serializable]
    public class AmbientData
    {
        public AmbientName ambientId;
        public NeverendingBlock firstAmbientBlock;
        public NeverendingBlock lastAmbientBlock;
        public NeverendingBlock[] blocks;
        public Texture2D[] lightmaps;

        protected int lightmapsOffset = -1;

        public int LightmapsOffset
        {
            get
            {
                return lightmapsOffset;
            }
            set
            {
                lightmapsOffset = value;
            }
        }
    }

    protected class MinMaxIndexes
    {
        public int minIndex;
        public int maxIndex;
    }

    protected struct BlockData
    {
        public AmbientName ambient;
        public int lightmapsOffset;

        public BlockData(AmbientName _ambient, int lmOffset)
        {
            ambient = _ambient;
            lightmapsOffset = lmOffset;
        }
    }
    #endregion

    #region Public members
    public List<AmbientData> ambientsBlockList;
    public AmbientName ambientsBlockListIndex = AmbientName.BEACH;

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
    //protected NeverendingTutorial ctrlTutorial;
    protected OnTheRunEnvironment environmentManager;

    protected List<NeverendingBlock> blocks;
    protected List<BlockData> blocksData;
    //protected NeverendingBlock[] blocks;

    protected bool gameStarted = false;
    protected GameObject firstblock = null;
    protected GameObject lastblock = null;

    protected List<GameObject>[] blocksCache = null;
    protected List<GameObject> firstAmbientBlocksCache = null;
    protected List<BlockData> firstAmbientBlocksData = null;
    protected List<GameObject> lastAmbientBlocksCache = null;
    protected List<BlockData> lastAmbientBlocksData = null;
    protected List<NeverendingTrack.BlocksBranch> branches = null;
    protected bool ambientJustChanged = false;
    protected bool spawnFirstAmbientBlock = false;
    protected List<MinMaxIndexes> ambientsBlockListIndexes = null;

    protected List<NeverendingTrack.OpenBranch> openBranches = null;
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
    public NeverendingTrack.BlocksBranch FirstBranch
    {
        get
        {
            return branches[0];
        }
    }
    #endregion

    #region Protected methods
    protected GameObject CreateInstanceForCache(NeverendingBlock block, BlockData data)
    {
        GameObject instance = GameObject.Instantiate(block.gameObject) as GameObject;
        NeverendingBlock blockInstance = instance.GetComponent<NeverendingBlock>();

        blockInstance.used = false;
        blockInstance.id = block.id;

        blockInstance.toDelete = false;

        int lmOffset = data.lightmapsOffset;
        Renderer[] rndrs = blockInstance.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rndr in rndrs)
        {
            if (rndr.lightmapIndex >= 254)
                continue;

            rndr.lightmapIndex += lmOffset;
        }

        InitializeBlock(instance); // TEMP

#if UNITY_4_3
        instance.SetActive(false);
#else
        instance.SetActiveRecursively(false);
#endif

        return instance;
    }

    protected int FindOpenBranchIndex(NeverendingTrack.BlocksBranch branch)
    {
        int count = openBranches.Count;
        for (int i = 0; i < count; ++i)
        {
            if (openBranches[i].branch == branch)
                return i;
        }

        return -1;
    }

    protected GameObject GetFreeBlock(int id, bool updateFirstAmbientBlock=true)
    {
        bool specialFirstLastBlock = false;

        GameObject newGO = null;
        if (ambientJustChanged)
        {
            specialFirstLastBlock = true;
            ambientJustChanged = false;
            newGO = lastblock;
            spawnFirstAmbientBlock = true;

            firstblock = GetAmbientFirstBlock(id);
            if (firstblock!=null)
                firstblock.SetActive(false);

            //Debug.Log("*** GetFreeBlock " + lastblock);
            if (lastblock == null)
            {
                spawnFirstAmbientBlock = false;
                if (firstblock!=null)
                    newGO = firstblock;
                else
                    specialFirstLastBlock = false;
            }

            if (newGO != null)
            {
#if UNITY_4_3
                newGO.SetActive(true);
#else
            newGO.SetActiveRecursively(true);
#endif
                newGO.GetComponent<NeverendingBlock>().used = true;
                lastblock = null;
            }

        }
        else if (spawnFirstAmbientBlock && firstblock!=null)
        {
            specialFirstLastBlock = true;
            spawnFirstAmbientBlock = false;
            newGO = firstblock;
#if UNITY_4_3
            newGO.SetActive(true);
#else
            newGO.SetActiveRecursively(true);
#endif
            newGO.GetComponent<NeverendingBlock>().used = true;
            firstblock = null;
        }


        if (!specialFirstLastBlock)
        {
            if (updateFirstAmbientBlock)
                spawnFirstAmbientBlock = false;
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
            /*newGO = GameObject.Instantiate(blocks[id].gameObject) as GameObject;

            int lmOffset = blocksData[id].lightmapsOffset;
            Renderer[] rndrs = newGO.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rndr in rndrs)
            {
                if (rndr.lightmapIndex >= 254)
                    continue;

                rndr.lightmapIndex += lmOffset;
            }*/
            newGO = this.CreateInstanceForCache(blocks[id], blocksData[id]);
            newGO.GetComponent<NeverendingBlock>().used = true;
            newGO.transform.position = Vector3.zero;
#if UNITY_4_3
            newGO.SetActive(true);
#else
            newGO.SetActiveRecursively(true);
#endif

            cache.Add(newGO);
        }

        return newGO;
    }

    protected GameObject GetAmbientLastBlock()
    {
        GameObject newGO = null;
        if (lastAmbientBlocksCache[(int)ambientsBlockListIndex] == null)
        {
            NeverendingBlock lastAmbientBlock = ambientsBlockList[(int)ambientsBlockListIndex].lastAmbientBlock;
            if (lastAmbientBlock != null)
            {
                newGO = this.CreateInstanceForCache(lastAmbientBlock, lastAmbientBlocksData[(int)ambientsBlockListIndex]);//GameObject.Instantiate(lastAmbientBlock.gameObject) as GameObject;
                lastAmbientBlocksCache[(int)ambientsBlockListIndex] = newGO;
                newGO.GetComponent<NeverendingBlock>().used = true;
            }
        }
        else
        {
            GameObject tempGO = lastAmbientBlocksCache[(int)ambientsBlockListIndex];
            if (!tempGO.GetComponent<NeverendingBlock>().used)
            {
                newGO = tempGO;
                newGO.GetComponent<NeverendingBlock>().used = true;
            }
            else
                Debug.Log("AAAAAAAAAAAAA LAST BLOCK USED!!!!!!! " + ambientsBlockListIndex);
        }

        return newGO;
    }

    protected GameObject GetAmbientFirstBlock(int id, bool startGame=false)
    {
        GameObject newGO = null;
        if (firstAmbientBlocksCache[(int)ambientsBlockListIndex] == null)
        {
            NeverendingBlock firstAmbientBlock = ambientsBlockList[(int)ambientsBlockListIndex].firstAmbientBlock;
            if (firstAmbientBlock != null)
            {
                newGO = this.CreateInstanceForCache(firstAmbientBlock, firstAmbientBlocksData[(int)ambientsBlockListIndex]);//GameObject.Instantiate(firstAmbientBlock.gameObject) as GameObject;
                newGO.GetComponent<NeverendingBlock>().used = true;
                firstAmbientBlocksCache[(int)ambientsBlockListIndex] = newGO;
            }
        }
        else
        {
            GameObject tempGO = firstAmbientBlocksCache[(int)ambientsBlockListIndex];
            if (!tempGO.GetComponent<NeverendingBlock>().used)
            {
                newGO = tempGO;
                newGO.GetComponent<NeverendingBlock>().used = true;
            }
            //else
            //    Debug.Log("AAAAAAAAAAAAA FIRST BLOCK USED!!!!!!!");
        }

        if (newGO == null && startGame)
            newGO = this.GetFreeBlock(id, false);
        else if (newGO != null)
            newGO.SetActive(true);

        return newGO;
    }

    protected virtual List<TrackBranch> PlaceBlock(NeverendingTrack.OpenBranch openBranch, TrackBranch activeBranch, GameObject prevBlock, GameObject newBlock, int nextLinkIndex)
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

        // if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
        //    newBlockRef.activeLayer = ctrlTutorial.GetTutorialBlock().blockLayer;
        //else 
        if (newBlockRef.Layers.Length > 0 && !placeRelaxBlock)
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

    protected List<TrackBranch> PlaceBlock(NeverendingTrack.OpenBranch openBranch, TrackBranch activeBranch, GameObject prevBlock, GameObject newBlock)
    {
        return this.PlaceBlock(openBranch, activeBranch, prevBlock, newBlock, 0);
    }

    protected void PushBlock(int openBranchIndex, int nextLinkIndex)
    {
        int id = this.GetRandomBlockId(openBranchIndex, nextLinkIndex);
        NeverendingTrack.OpenBranch openBranch = openBranches[openBranchIndex];
        NeverendingTrack.BlocksBranch branch = openBranch.branch;
        if (null == branch.head)
        {
            // first block
#if UNITY_FLASH
            BlockNode firstNode = new BlockNode(GetAmbientFirstBlock(id, true));//this.GetFreeBlock(id));
            branch.head = firstNode;
            branch.tail = firstNode;
#else
            NeverendingTrack.BlockNode firstNode = branch.head = branch.tail = new NeverendingTrack.BlockNode(GetAmbientFirstBlock(id, true));//this.GetFreeBlock(id));
#endif
            if (null == branch.prev)
                this.PlaceBlock(openBranch, branch.trackBranch, null, firstNode.block);
            else
            {
                NeverendingTrack.BlocksBranch prev = branch.prev;
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

            //if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
            //    ctrlTutorial.SetTutorialBlockNode(firstNode);

            return;
        }

        // add in active branch
        NeverendingTrack.BlockNode prevNode = branch.tail;

        NeverendingTrack.BlockNode newNode = prevNode.next = new NeverendingTrack.BlockNode(this.GetFreeBlock(id));
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

            branch.links = new NeverendingTrack.BlocksBranch[numLinks];

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
                        lastBlock.links[j + 1].branch = branch.links[j] = new NeverendingTrack.BlocksBranch();
#endif
                        Debug.Log("Link #" + j + " got branch #" + i);
                        break;
                    }
                }

                Asserts.Assert(j < numLinks);
                branch.links[j].prev = branch;
                branch.links[j].trackBranch = newTrackBranches[i];

                openBranches.Add(new NeverendingTrack.OpenBranch(branch.links[j], lastBlock, j));
                branches.Add(branch.links[j]);
            }
        }

        //if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
        //    ctrlTutorial.SetTutorialBlockNode(newNode);
    }

    protected bool PopBlock(NeverendingTrack.BlocksBranch branch)
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
                foreach (NeverendingTrack.BlocksBranch link in branch.links)
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
            NeverendingTrack.BlocksBranch branch = branches[i];

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
        //if (ctrlTutorial.IsActive && !ctrlTutorial.AllSequencesPlaced)
        //    return ctrlTutorial.GetTutorialBlock().blockId;

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

            if (node.Value < ambientsBlockListIndexes[(int)ambientsBlockListIndex].minIndex || node.Value > ambientsBlockListIndexes[(int)ambientsBlockListIndex].maxIndex)
                blockOk = false;

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
        ambientsBlockListIndex = gameObject.GetComponent<AmbientController>().CurrentAmbient;

        SetupAmbientIndex();
        
        gameStarted = true;

        branches = new List<NeverendingTrack.BlocksBranch>();
        branches.Add(new NeverendingTrack.BlocksBranch());
        branches[0].trackBranch = new TrackBranch();

        openBranches = new List<NeverendingTrack.OpenBranch>();
        openBranches.Add(new NeverendingTrack.OpenBranch(branches[0]));

        for (int i = 0; i < 2; ++i)
            this.PushBlock(0, 0);

        nextEnvChange = UnityEngine.Random.Range(29, 51) * 10.0f; //18,40

        startRunningTime = TimeManager.Instance.MasterSource.TotalTime;
        nextRelaxTime = startRunningTime + UnityEngine.Random.Range(relaxDeltaTimeMin, relaxDeltaTimeMax);

        controller.StartRunning(branches[0]);

        //Initialize asphalts
        /*
        for (int j = 0; j < blocksCache.Length; ++j)
        {
            foreach (GameObject go in blocksCache[j])
            {
                InitializeBlock(go);
            }
        }

        for (int j = 0; j < firstAmbientBlocksCache.Count; ++j)
        {
            foreach (GameObject go in firstAmbientBlocksCache)
            {
                if(go!=null)
                    InitializeBlock(go);
            }
        }

        for (int j = 0; j < lastAmbientBlocksCache.Count; ++j)
        {
            foreach (GameObject go in lastAmbientBlocksCache)
            {
                if (go != null)
                    InitializeBlock(go);
            }
        }*/
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
        
        for (int j = 0; j < firstAmbientBlocksCache.Count; ++j)
        {
            foreach (GameObject go in firstAmbientBlocksCache)
            {
                if (go != null)
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
        }

        for (int j = 0; j < lastAmbientBlocksCache.Count; ++j)
        {
            foreach (GameObject go in lastAmbientBlocksCache)
            {
                if (go != null)
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
        }

        nextRelaxTime = -1.0f;
        relaxRailsLength = -1.0f;
        voidRailsLength = -1.0f;

        ambientsBlockListIndex = AmbientName.BEACH;
        ambientJustChanged = false;
        gameStarted = false;
    }

    void ChangeAmbient(AmbientName nextAmbientIdx)
    {
        lastblock = GetAmbientLastBlock();
        //Debug.Log("*** PICK LAST BLOCK " + lastblock);
        ambientJustChanged = true;
        ambientsBlockListIndex = nextAmbientIdx;
        SetupAmbientIndex();
    }

    void SetupAmbientIndex()
    {
        if (ambientsBlockListIndex > AmbientName.US_TUNNEL)
            ambientsBlockListIndex -= 12;
        if (ambientsBlockListIndex > AmbientName.AS_BRIDGE)
            ambientsBlockListIndex -= 8;
        else if (ambientsBlockListIndex > AmbientName.EU_UPTOWN)
            ambientsBlockListIndex -= 4;
    }

    public void DestroyAllFromScene()
    {
        int i = blocksCache.Length - 1;
        for (; i >= 0; --i)
        {
            foreach (GameObject go in blocksCache[i])
                Destroy(go);
            blocksCache[i].Clear();
            blocksCache[i] = null;
        }

        for (int j = 0; j < firstAmbientBlocksCache.Count; ++j)
        {
            var go = firstAmbientBlocksCache[j];
            firstAmbientBlocksCache[j] = null;
            if (go != null)
                Destroy(go);
        }
        firstAmbientBlocksCache.Clear();

        for (int j = 0; j < lastAmbientBlocksCache.Count; ++j)
        {
            var go = lastAmbientBlocksCache[j];
            lastAmbientBlocksCache[j] = null;
            if (go != null)
                Destroy(go);
        }
        lastAmbientBlocksCache.Clear();
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        if (instance == null)
        {
            Asserts.Assert(null == instance);
            instance = this;
        }

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<NeverendingPlayer>();

        environmentManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GetComponent<OnTheRunEnvironment>();

        //ctrlTutorial = (null == controller ? null : controller.gameObject.GetComponent<NeverendingTutorial>());
        
        blocks = new List<NeverendingBlock>();
        blocksData = new List<BlockData>();
        ambientsBlockListIndexes = new List<MinMaxIndexes>();
        lastAmbientBlocksCache = new List<GameObject>();
        lastAmbientBlocksData = new List<BlockData>();
        firstAmbientBlocksCache = new List<GameObject>();
        firstAmbientBlocksData = new List<BlockData>();

        List<LightmapData> lightmaps = new List<LightmapData>();
        int lmBaseIndex = 0;

        int lastAmbientLastIndex = 0;
        for (int i = 0; i < ambientsBlockList.Count; ++i)
        {
            ambientsBlockListIndexes.Add(new MinMaxIndexes());
            lastAmbientBlocksCache.Add(null);
            firstAmbientBlocksCache.Add(null);

            AmbientData data = ambientsBlockList[i];
            data.LightmapsOffset = lmBaseIndex;

            lastAmbientBlocksData.Add(new BlockData(data.ambientId, lmBaseIndex));
            firstAmbientBlocksData.Add(new BlockData(data.ambientId, lmBaseIndex));

            foreach (Texture2D lm in data.lightmaps)
            {
                LightmapData lmData = new LightmapData();
                lmData.lightmapFar = lm;
                lightmaps.Add(lmData);
            }

            foreach (NeverendingBlock block in data.blocks)
            {
                blocks.Add(block);
                blocksData.Add(new BlockData(data.ambientId, lmBaseIndex));
            }

            lmBaseIndex += data.lightmaps.Length;

            ambientsBlockListIndexes[i].minIndex = lastAmbientLastIndex;
            ambientsBlockListIndexes[i].maxIndex = lastAmbientLastIndex + data.blocks.Length - 1;
            lastAmbientLastIndex = ambientsBlockListIndexes[i].maxIndex + 1;
        }

        LightmapSettings.lightmaps = lightmaps.ToArray();

        initCache();
    }

    void InitializeBlock(GameObject block)
    {
        /*for (int i = 0; i < block.transform.childCount; ++i)
        {
            GameObject currChild = block.transform.GetChild(i).gameObject;
            if (currChild.name.Contains("_road_"))
            {
                currChild.renderer.material = environmentManager.CurrentAsphalt;
            }
        }*/
    }

    void initCache()
    {
        int i = 0,
            cacheInitialSize = 0,
            countInQueue = 0;

        int cacheSize = 0;
        for (int j = 0; j < ambientsBlockList.Count; ++j)
        {
            cacheSize += ambientsBlockList[j].blocks.Length;
        }

        blocksCache = new List<GameObject>[cacheSize];

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

            //int lmOffset = blocksData[block.id].lightmapsOffset;
            //Debug.Log(block.id + ": " + blocksData[block.id].ambient + ", " + blocksData[block.id].lightmapsOffset);
            for (int j = 0; j < cacheInitialSize; ++j)
            {/*
                GameObject instance = GameObject.Instantiate(block.gameObject) as GameObject;
                NeverendingBlock blockInstance = instance.GetComponent<NeverendingBlock>();

                blockInstance.used = false;
                blockInstance.id = block.id;

                blockInstance.toDelete = false;

                Renderer[] rndrs = blockInstance.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer rndr in rndrs)
                {
                    if (rndr.lightmapIndex >= 254)
                        continue;

                    rndr.lightmapIndex += lmOffset;
                }

                list.Add(instance);*/
                list.Add(this.CreateInstanceForCache(block, blocksData[block.id]));
            }
            /*
                        foreach (GameObject go in list)
                            go.SetActiveRecursively(false);*/
        }

        blockIdsLRU = Shuffle(blockIdsLRU);

        //Creating last blocks cache
        for (int j = 0; j < ambientsBlockList.Count; ++j)
        {
            NeverendingBlock lastAmbientBlock = ambientsBlockList[j].lastAmbientBlock;
            /*GameObject newGO = GameObject.Instantiate(lastAmbientBlock.gameObject) as GameObject;
            lastAmbientBlocksCache[j] = newGO;

            int lmOffset = ambientsBlockList[j].LightmapsOffset;
            Renderer[] rndrs = newGO.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rndr in rndrs)
            {
                if (rndr.lightmapIndex >= 254)
                    continue;

                rndr.lightmapIndex += lmOffset;
            }*/

            GameObject newGO = null;

            if (lastAmbientBlock != null)
            {
                newGO = this.CreateInstanceForCache(lastAmbientBlock, lastAmbientBlocksData[j]);
                //newGO.SetActiveRecursively(false);
                //newGO.GetComponent<NeverendingBlock>().used = false;
                lastAmbientBlocksCache[j] = newGO;
            }

            NeverendingBlock firstAmbientBlock = ambientsBlockList[j].firstAmbientBlock;
            if (firstAmbientBlock != null)
            {/*
                newGO = GameObject.Instantiate(firstAmbientBlock.gameObject) as GameObject;
                firstAmbientBlocksCache[j] = newGO;*/
                newGO = this.CreateInstanceForCache(firstAmbientBlock, firstAmbientBlocksData[j]);
                //newGO.SetActiveRecursively(false);
                //newGO.GetComponent<NeverendingBlock>().used = false;
                firstAmbientBlocksCache[j] = newGO;
            }
        }
    }

    void Start()
    {
        //initCache();
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
            NeverendingTrack.OpenBranch openBranch = openBranches[i];
            NeverendingBlock lastBlock = openBranch.lastBlock;
            if (lastBlock != null && controller.TrackPos > (lastBlock.GetMaxEndTrackPosition() - minDistance))
            {
                this.PushBlock(i, openBranch.lastBlockLinkIndex);
            }
        }

        //!ctrlTutorial.IsActive && 
        if (controller.SessionMeters < noRelaxAfterMeters && nextRelaxTime > 0.0f && TimeManager.Instance.MasterSource.TotalTime >= nextRelaxTime && voidRailsLength < 0.0f)
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
