using UnityEngine;
using SBS.Math;
using SBS.Level;

[AddComponentMenu("SBS/Level/LevelObject")]
public class LevelObject : MonoBehaviour
{
	#region Public members
    public string category = null;
    public int layersMask = 0;
	#endregion

	#region Protected members
    protected Transform _transform;
	protected SBSMatrix4x4 _localToWorld;
	protected SBSMatrix4x4 _worldToLocal;
    protected SBSBounds _bounds;
    protected LevelCell cell = null;
    #endregion

    #region Public properties
    public Transform Transform {
        get {
            return _transform;
        }
    }

	public SBSMatrix4x4 LocalToWorld {
		get {
			return _localToWorld;
		}
	}

	public SBSMatrix4x4 WorldToLocal {
		get {
			return _worldToLocal;
		}
	}

    public SBSBounds Bounds {
        get {
            return _bounds;
        }
        set {
            _bounds = value;

            LevelCell newCell = cell.FindObjectContainmentCell(this);

            if (newCell != cell) {
                cell.DetachObject(this);
                cell = newCell;
                cell.AttachObject(this);
            }
        }
    }

    public string Category {
        get {
            return category;
        }
        set {
            if (cell != null)
                cell.NotifyObjectCategory(this, category, value);

            category = value;
        }
    }

    public int LayersMask {
        get {
            return layersMask;
        }
        set {
            if (cell != null)
                cell.NotifyObjectLayersMask(this, layersMask, value);

            layersMask = value;
        }
    }

    virtual public bool IsMovable {
        get {
            return false;
        }
    }

    public LevelCell Cell {
        get {
            return cell;
        }
    }
    #endregion

    #region Public methods
    public virtual void Initialize()
    {
        _transform    = transform;
		_localToWorld = _transform.localToWorldMatrix;
		_worldToLocal = _transform.worldToLocalMatrix;

        if (GetComponent<Renderer>() != null)
            _bounds = GetComponent<Renderer>().bounds;
        else if (GetComponent<Collider>() != null)
            _bounds = GetComponent<Collider>().bounds;
        else
            _bounds = new SBSBounds(_localToWorld.position, SBSVector3.zero);

        cell = LevelRoot.Instance.RootCell.FindObjectContainmentCell(this);
        cell.AttachObject(this);

        gameObject.SendMessage("OnInit", SendMessageOptions.DontRequireReceiver);
    }
    #endregion

    #region Unity callbacks
    #endregion
};
