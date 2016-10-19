using System.Collections.Generic;
using UnityEngine;

public class UIPageIndicators : MonoBehaviour
{
    #region Public Members
    public GameObject indicatorPrefab;
    #endregion

    #region Protected Members
    protected GameObject offsetNode;

    protected const float xOffsetBetweenIndicators = 0.35f;

    protected List<SpriteRenderer> indicatorsRndrs = new List<SpriteRenderer>();

    private static readonly Color normalColor = new Color(1.0f, 1.0f, 1.0f, 0.5f),
                                  selectedColor = Color.white;
    private const float normalScale = 0.7f;

    protected int selectedId = 0;
    protected int currentPagesNum;

    #endregion

    #region Unity Callbacks
    void Awake()
    {
        offsetNode = new GameObject();
        offsetNode.transform.parent = this.transform;
        offsetNode.transform.localPosition = Vector3.zero;
    }

    void OnEnable()
    {
        if (null == indicatorsRndrs)
            this.Awake();
        else
            this.SetNormalState();
    }
    #endregion

    #region Protected Methods
    public void InitIndicators(int pagesNum)
    {
        selectedId = 0;
        currentPagesNum = pagesNum;

        DestroyIndicators();

        offsetNode.transform.localPosition = Vector3.left * (pagesNum - 1) * 0.5f * xOffsetBetweenIndicators;

        for (int i = 0; i < pagesNum; i++)
        {
            SpriteRenderer sr = (Instantiate(indicatorPrefab) as GameObject).GetComponent<SpriteRenderer>();
            sr.transform.parent = offsetNode.transform;
            sr.transform.localPosition = Vector3.right * i * xOffsetBetweenIndicators;
            sr.transform.localScale = Vector3.one * normalScale;
            sr.color = normalColor;

            indicatorsRndrs.Add(sr);
        }
    }

    void DestroyIndicators()
    {
        int indicatorsCount = indicatorsRndrs.Count;
        for (int i = 0; i < indicatorsCount; i++)
            DestroyImmediate(indicatorsRndrs[i].gameObject);

        indicatorsRndrs.Clear();
        offsetNode.transform.localPosition = Vector3.zero;
    }

    void SetNormalState()
    {
        for (int i = 0; i < indicatorsRndrs.Count; i++)
        {
            if (i == selectedId)
            {
                indicatorsRndrs[i].transform.localScale = Vector3.one;
                indicatorsRndrs[i].color = selectedColor;
            }
            else
            {
                indicatorsRndrs[i].transform.localScale = Vector3.one * normalScale;
                indicatorsRndrs[i].color = normalColor;
            }
        }
    }
    #endregion

    #region Public Methods
    public void SetSelected(int id)
    {
        indicatorsRndrs[selectedId].transform.localScale = Vector3.one * normalScale;
        indicatorsRndrs[selectedId].color = normalColor;

        indicatorsRndrs[id].transform.localScale = Vector3.one;
        indicatorsRndrs[id].color = selectedColor;

        selectedId = id;
    }

    #endregion
}
