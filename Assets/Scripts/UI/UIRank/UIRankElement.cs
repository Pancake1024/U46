using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRankElement")]
public class UIRankElement : MonoBehaviour
{
    public UITextField diamondTextfield;
    public UITextField levelTextfield;
    public GameObject diamondIcon;

    #region Unity callbacks
    public void Initialize(string text, bool visible)
	{
        diamondTextfield.text = OnTheRunConsumableBonusManager.Instance.GetDiamondsForLevelUp(int.Parse(text)).ToString();
        levelTextfield.text = text;
        levelTextfield.transform.parent.gameObject.SetActive(!visible);
        diamondIcon.SetActive(!visible); 
    }
    #endregion
     
}
