using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("SBS/Managers/TexShiftManager")]
public class TexShiftManager : MonoBehaviour
{
    #region Singleton instance
    protected static TexShiftManager instance = null;

    public static TexShiftManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region TexShiftDesc class
    [Serializable]
    public class TexShiftDesc
    {
        public Material material = null;
        public float dt = 1.0f;
        public Vector2 shift = new Vector2(1.0f, 0.0f);
        public bool smooth = true;
    }
    #endregion

    #region Public members
    public List<TexShiftDesc> texShiftDescs = new List<TexShiftDesc>();
    #endregion

    #region Protected members
    protected Dictionary<Material, TexShiftDesc> texShiftDict = new Dictionary<Material, TexShiftDesc>();
    #endregion

    #region Public methods
    public void AddMaterial(Material mat, float dt, Vector2 shift, bool smooth)
    {
        TexShiftDesc newDesc = null;
        bool addDesc = false;

        if (!texShiftDict.TryGetValue(mat, out newDesc))
        {
            newDesc = new TexShiftDesc();
            addDesc = true;
        }

        newDesc.material = mat;
        newDesc.dt = dt;
        newDesc.shift = shift;
        newDesc.smooth = smooth;

        if (addDesc)
        {
            texShiftDescs.Add(newDesc);
            texShiftDict.Add(mat, newDesc);
        }
    }

    public void RemoveMaterial(Material mat)
    {
        TexShiftDesc matDesc = null;
        if (texShiftDict.TryGetValue(mat, out matDesc))
        {
            texShiftDict.Remove(mat);
            texShiftDescs.Remove(matDesc);

            mat.mainTextureOffset = Vector2.zero;
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        foreach (TexShiftDesc desc in texShiftDescs)
            texShiftDict.Add(desc.material, desc);
    }

    void Update()
    {
        float time = TimeManager.Instance.MasterSource.TotalTime;

        foreach (TexShiftDesc desc in texShiftDescs)
        {
            Vector2 offset = Vector2.zero,
                    scale = desc.material.mainTextureScale;
            if (desc.smooth)
            {
                offset = desc.shift * (time / desc.dt);
            }
            else
            {
                int step = Mathf.FloorToInt(time / desc.dt);
                offset = desc.shift * step;
            }
            desc.material.mainTextureOffset = new Vector2(
            scale.x > 0.0f ? SBSMath.Frac(offset.x) : 0.0f,
            scale.y > 0.0f ? SBSMath.Frac(offset.y) : 0.0f);
        }
    }

    void OnDestroy()
    {
        foreach (TexShiftDesc desc in texShiftDescs)
            desc.material.mainTextureOffset = Vector2.zero;

        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}
