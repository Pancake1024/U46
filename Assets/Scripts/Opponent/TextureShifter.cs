using UnityEngine;
using System.Collections;
using SBS.Math;

public class TextureShifter : MonoBehaviour
{
    #region Public Members
    public bool smooth = true;
    public float dt = 1.0f;
    public Vector2 shift = new Vector2(1.0f, 0.0f);
    public int materialIndex = 0;
    #endregion

    #region Protected Members
    protected Material materialToShift;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        materialToShift = gameObject.GetComponent<Renderer>().materials[materialIndex];
    }

    void Update()
    {
        float time = TimeManager.Instance.MasterSource.TotalTime;

        Vector2 offset = Vector2.zero,
                    scale = materialToShift.mainTextureScale;
        if (smooth)
        {
            offset = shift * (time / dt);
        }
        else
        {
            int step = Mathf.FloorToInt(time / dt);
            offset = shift * step;
        }
        materialToShift.mainTextureOffset = new Vector2(
        scale.x > 0.0f ? SBSMath.Frac(offset.x) : 0.0f,
        scale.y > 0.0f ? SBSMath.Frac(offset.y) : 0.0f);
    }
    #endregion
}
