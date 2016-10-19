using UnityEngine;
using SBS.Core;
using System;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UITierItem")]
public class UITierItem : MonoBehaviour
{
    #region Public Members
    public UITextField tfTitle;
    public UITextField tfDescription;
    public UITextField tfButtonLabel;
	public UIButton BuyButton;

    public UIButton btPlay;
    public UIButton btClickArea;
    public SpriteRenderer tierIcon;
    public SpriteRenderer tierSnapshot;

    public GameObject LockedArea;

	public GameObject LockAnimationBump;
	public GameObject LockAnimationUnlock;

    public UITextField tfLockedTitle;
    public UITextField tfLockedDescription;
    public UITextField tfLockedBuyFor;
    public UITextField tfLockedLevel;
    public UITextField tfTierPrice;

    public Sprite[] tiersIcons;
    public Sprite[] tiersSnap;

    [HideInInspector]
    public int tierId;
    #endregion

    #region Protected Members
    #endregion

    //public float ccc = 0.2f;
    //public float sss = 0.4f;
    void PlacePrizeIcons()
    {
        GameObject coinIco = BuyButton.transform.FindChild("coin").gameObject;
        GameObject diamondIco = BuyButton.transform.FindChild("diamond").gameObject;
        UITextField tf = BuyButton.transform.FindChild("tfTextfield").gameObject.GetComponent<UITextField>();
        float dist = tf.text.Length * 0.2f * 0.5f + 0.4f; // ccc * 0.5f + sss;
        coinIco.transform.localPosition = new Vector3(tf.transform.localPosition.x + dist, coinIco.transform.localPosition.y, coinIco.transform.localPosition.z);
        diamondIco.transform.localPosition = new Vector3(tf.transform.localPosition.x + dist, diamondIco.transform.localPosition.y, diamondIco.transform.localPosition.z);
    }


    #region Public Methods
    public void Initialize(int _tierId)
    {
        tierId = _tierId;
        var dataLoader = OnTheRunDataLoader.Instance;
        var persistentData = PlayerPersistentData.Instance;

        tierIcon.sprite = tiersIcons[tierId];
        tierSnapshot.sprite = tiersSnap[tierId];
        tfTitle.text = dataLoader.GetLocaleString(GetTitleCodeFromId(tierId));
        tfDescription.text = OnTheRunDataLoader.Instance.GetLocaleString("description_tier_" + (_tierId + 1));
        tfButtonLabel.text = dataLoader.GetLocaleString("btPlay");
        tfButtonLabel.ApplyParameters();
        tfLockedTitle.text = dataLoader.GetLocaleString("unlock_tier_prefix");
        tfLockedDescription.text = dataLoader.GetLocaleString("unlock_tier_postfix");

#if !UNITY_WEBPLAYER
        tfLockedBuyFor.text = dataLoader.GetLocaleString("buy_for") + " " + dataLoader.GetTiersBuyFor()[tierId].ToString() + " " + dataLoader.GetLocaleString("diamonds") + " " + dataLoader.GetLocaleString("or") + " " + dataLoader.GetLocaleString("unlock_specials_prefix") + " " + dataLoader.GetTiersPlayerLevelThreshold()[tierId].ToString() + " " + dataLoader.GetLocaleString("unlock_tier_postfix");
        BuyButton.transform.FindChild("coin").gameObject.SetActive(false);
#else
        tfLockedBuyFor.text = dataLoader.GetLocaleString("buy_for") + " " + Manager<UIRoot>.Get().FormatTextNumber((int)dataLoader.GetTiersBuyFor()[tierId]) + " " + dataLoader.GetLocaleString("coins") + " " + dataLoader.GetLocaleString("or") + " " + dataLoader.GetLocaleString("unlock_specials_prefix") + " " + dataLoader.GetTiersPlayerLevelThreshold()[tierId].ToString() + " " + dataLoader.GetLocaleString("unlock_tier_postfix");
#endif
        
        tfLockedLevel.text = dataLoader.GetTiersPlayerLevelThreshold()[tierId].ToString();
#if !UNITY_WEBPLAYER
        tfTierPrice.text = dataLoader.GetTiersBuyFor()[tierId].ToString();
#else
        tfTierPrice.text = Manager<UIRoot>.Get().FormatTextNumber((int)dataLoader.GetTiersBuyFor()[tierId]);
#endif
        PlacePrizeIcons();

        LockedArea.SetActive(persistentData.IsParkingLotLocked(tierId));
        btPlay.gameObject.SetActive(!persistentData.IsParkingLotLocked(tierId));
        btClickArea.gameObject.SetActive(!persistentData.IsParkingLotLocked(tierId));

        if (dataLoader.GetTiersCurrencies()[tierId] == PriceData.CurrencyType.FirstCurrency)
			LockedArea.transform.FindChild("btBuyTier/btBuyTierbtn/diamond").gameObject.SetActive(false);

		LockAnimationBump.SetActive( LockedArea.activeSelf );
    }

	void Start()
	{
		LockAnimationUnlock.SetActive( false );
	}

    #endregion

    #region Protected Members
    public string GetTitleCodeFromId(int id)
    {
        switch (id)
        {
            default:
            case 0:
                return "europeanCarsTitle";
            case 1:
                return "orientalCarsTitle";
            case 2:
                return "americanCarsTitle";
            case 3:
                return "muscleCarsTitle";
        }
    }
    #endregion


}