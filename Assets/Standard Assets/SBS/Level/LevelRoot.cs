using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.Level;

[AddComponentMenu("SBS/Level/LevelRoot")]
public class LevelRoot : MonoBehaviour
{
    #region Singleton instance
    protected static LevelRoot instance = null;

    public static LevelRoot Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Protected members
    protected Transform rootTransform = null;
    protected Layers layers;
    protected LevelCell rootCell;
    protected SBSBounds worldBounds;
    #endregion

    #region Public members
    public Vector3 worldBoundsMin;
    public Vector3 worldBoundsMax;
    public string[] categories;
    public string[] layersName;
    #endregion

    #region Public properties
    public Transform Root
    {
        get
        {
            return rootTransform;
        }
    }

    public Layers Layers
    {
        get
        {
            return layers;
        }
    }

    public LevelCell RootCell
    {
        get
        {
            return rootCell;
        }
    }
    #endregion

    #region Protected methods
    protected void RecursiveBuildQuadTree(LevelCell parentCell, LevelCell cell, int depth, int maxDepth, bool yUp/* = true*/)
    {
        cell.Parent = parentCell;

        if (maxDepth == depth)
            return;

        SBSVector3 halfSize = new SBSVector3(
            cell.Bounds.size.x * 0.5f,
            yUp ? cell.Bounds.size.y : cell.Bounds.size.y * 0.5f,
            yUp ? cell.Bounds.size.z * 0.5f : cell.Bounds.size.z
        );
        SBSVector3 leftTop = new SBSVector3(
            (cell.Bounds.min.x + cell.Bounds.center.x) * 0.5f,
            yUp ? cell.Bounds.center.y : (cell.Bounds.min.y + cell.Bounds.center.y) * 0.5f,
            yUp ? (cell.Bounds.min.z + cell.Bounds.center.z) * 0.5f : cell.Bounds.center.z
        );
        SBSVector3 rightTop = new SBSVector3(
            (cell.Bounds.max.x + cell.Bounds.center.x) * 0.5f,
            yUp ? cell.Bounds.center.y : (cell.Bounds.min.y + cell.Bounds.center.y) * 0.5f,
            yUp ? (cell.Bounds.min.z + cell.Bounds.center.z) * 0.5f : cell.Bounds.center.z
        );
        SBSVector3 rightBottom = new SBSVector3(
            (cell.Bounds.max.x + cell.Bounds.center.x) * 0.5f,
            yUp ? cell.Bounds.center.y : (cell.Bounds.max.y + cell.Bounds.center.y) * 0.5f,
            yUp ? (cell.Bounds.max.z + cell.Bounds.center.z) * 0.5f : cell.Bounds.center.z
        );
        SBSVector3 leftBottom = new SBSVector3(
            (cell.Bounds.min.x + cell.Bounds.center.x) * 0.5f,
            yUp ? cell.Bounds.center.y : (cell.Bounds.max.y + cell.Bounds.center.y) * 0.5f,
            yUp ? (cell.Bounds.max.z + cell.Bounds.center.z) * 0.5f : cell.Bounds.center.z
        );

        this.RecursiveBuildQuadTree(cell, new LevelCell(new SBSBounds(leftTop, halfSize)), depth + 1, maxDepth, yUp);
        this.RecursiveBuildQuadTree(cell, new LevelCell(new SBSBounds(rightTop, halfSize)), depth + 1, maxDepth, yUp);
        this.RecursiveBuildQuadTree(cell, new LevelCell(new SBSBounds(rightBottom, halfSize)), depth + 1, maxDepth, yUp);
        this.RecursiveBuildQuadTree(cell, new LevelCell(new SBSBounds(leftBottom, halfSize)), depth + 1, maxDepth, yUp);
    }

    protected LevelCell BuildQuadTree(SBSBounds bounds, int depth, bool yUp /*= true*/)
    {
        LevelCell root = new LevelCell(bounds);
        this.RecursiveBuildQuadTree(null, root, 0, depth - 1, yUp);
        return root;
    }
    #endregion

    #region Public methods
    public void Initialize()
    {
        // create layers
        layers = new Layers();
        foreach (string layerName in layersName)
            layers.Register(layerName);

        // build quad tree
        worldBounds = new SBSBounds();
        worldBounds.min = worldBoundsMin;
        worldBounds.max = worldBoundsMax;

        rootCell = this.BuildQuadTree(worldBounds, 5, true);

        // initialize level objects
        LevelObject[] objects = gameObject.GetComponentsInChildren<LevelObject>(true);
        foreach (LevelObject obj in objects)
            obj.Initialize();

        if (RacingManager.Instance != null)
            RacingManager.Instance.track.Build();

        foreach (LevelObject obj in objects)
            obj.gameObject.SendMessage("OnPostInit", SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Query functions
    public LevelObject[] Query(SBSBounds bbox, string category, int mask)
    {
        if (null == rootCell)
            return new LevelObject[0];
        List<LevelObject> results = new List<LevelObject>();
        rootCell.RecurseQuery(bbox, SBSMath.ClipStatus.Overlapping, category, mask, results);
        return results.ToArray();
    }

    public LevelObject[] Query(SBSBounds bbox, string category)
    {
        return this.Query(bbox, category, -1);
    }

    public LevelObject[] Query(SBSBounds bbox, int mask)
    {
        return this.Query(bbox, null, mask);
    }

    public LevelObject[] Query(SBSBounds bbox)
    {
        return this.Query(bbox, null, -1);
    }

    public LevelObject[] Query(string category)
    {
        if (null == rootCell)
            return new LevelObject[0];
        else
            return this.Query(rootCell.Bounds, category, -1);
    }

    public LevelObject[] Query(int mask)
    {
        if (null == rootCell)
            return new LevelObject[0];
        else
            return this.Query(rootCell.Bounds, null, mask);
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        rootTransform = gameObject.transform;
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
