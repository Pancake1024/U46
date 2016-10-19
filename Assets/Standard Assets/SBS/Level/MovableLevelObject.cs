using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.Level;

[AddComponentMenu("SBS/Level/MovableLevelObject")]
public class MovableLevelObject : LevelObject
{
    #region Protected members
    protected SBSMatrix4x4 _prevLocalToWorld;
    protected SBSMatrix4x4 _prevWorldToLocal;
    #endregion

    #region Overrided properties
    public override bool IsMovable
    {
        get
        {
            return true;
        }
    }
    #endregion

    #region Public properties
    public SBSMatrix4x4 PrevLocalToWorld
    {
        get
        {
            return _prevLocalToWorld;
        }
    }

    public SBSMatrix4x4 PrevWorldToLocal
    {
        get
        {
            return _prevWorldToLocal;
        }
    }
    #endregion

    #region Public methods
    public override void Initialize()
    {
        base.Initialize();

        _prevLocalToWorld = _transform.localToWorldMatrix;
        _prevWorldToLocal = _transform.worldToLocalMatrix;
    }
    #endregion

    #region Unity callbacks
    void Update()
    {
        if (GetComponent<Renderer>() != null)
            _bounds = GetComponent<Renderer>().bounds;
        else if (GetComponent<Collider>() != null)
            _bounds = GetComponent<Collider>().bounds;
        else
            _bounds = new SBSBounds(_transform.position, SBSVector3.zero);

        if (null == cell)
        {
            cell = LevelRoot.Instance.RootCell;
            if (null == cell)
                return;
            cell.AttachObject(this);
        }

        LevelCell newCell = cell.FindObjectContainmentCell(this);

        if (newCell != cell)
        {
            if (cell != null)
                cell.DetachObject(this);

            cell = newCell;
            cell.AttachObject(this);
        }

        _prevLocalToWorld = _localToWorld;
        _prevWorldToLocal = _worldToLocal;

		if( _transform == null )
			return;

        _localToWorld = _transform.localToWorldMatrix;
        _worldToLocal = _transform.worldToLocalMatrix;
    }
    #endregion
}
