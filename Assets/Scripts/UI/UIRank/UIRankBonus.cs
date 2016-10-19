using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRankBonus")]
public class UIRankBonus : MonoBehaviour
{
    public SpriteRenderer itemSprite;
    public UITextField levelTextfield;
    public UITextField numTextfield;

    #region Unity callbacks
    public void Initialize(OnTheRunConsumableBonusManager.ConsumableBonus cons)
    {
        levelTextfield.text = cons.level.ToString();
        itemSprite.sprite = OnTheRunConsumableBonusManager.Instance.GetConsumableSprite(cons.type);
        numTextfield.text = cons.quantity.ToString();
    }
    #endregion

}
