using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("SBS/UI/UIAtlas")]
public class UIAtlas : MonoBehaviour
{
    #region Public classes
    [System.Serializable]
    public class CutData
    {
        public string name;
        public Rect rect;
    }
    #endregion

    #region Public members
    public Texture2D atlas;
    public int capacity;
    public CutData[] cuts;
    #endregion
}
