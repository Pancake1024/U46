using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRankUnlockable")]
public class UIRankUnlockable : MonoBehaviour
{
    public GameObject verticalBar;
    public UITextField levelTextfield;
    public GameObject star;
    public SpriteRenderer icon;
    public GameObject fireworksGo;
    public GameObject scrollerGo;

    #region Unity callbacks
    public void Initialize(string text, Sprite iconSprite)
	{
        levelTextfield.text = text;
        if (iconSprite!=null)
            icon.sprite = iconSprite;
    }

    void Update()
    {
        if(fireworksGo!=null)
        {
            float x = Mathf.Abs(scrollerGo.transform.InverseTransformPoint(transform.position).x);
            if(x < 3.4f && !fireworksGo.activeSelf)
                fireworksGo.SetActive(true);
            else if(x >= 3.4f && fireworksGo.activeSelf)
                fireworksGo.SetActive(false);
        }
    }
    #endregion

}
