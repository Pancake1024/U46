using UnityEngine;
using SBS.Core;
using System;

[AddComponentMenu("OnTheRun/UI/UIBuyPopup")]
public class UIBuyPopup : MonoBehaviour
{
    [NonSerialized]
    public TruckBehaviour.TrasformType specialCarIndex;
    [NonSerialized]
    public GameObject truckRef;

    public GameObject[] objectToDisable;

	public UITextField MaxedOutText;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UITextField nameText;
    protected UITextField shortDescrText;
    protected UITextField buyButtonText;
    protected UITextField upgradeButtonText;
    protected UnlockingManager.SpecialCarData currSpecialCarData;
    protected PriceData.CurrencyType buyCarCurrency = PriceData.CurrencyType.SecondCurrency;
    protected PriceData.CurrencyType upgradeCarCurrency = PriceData.CurrencyType.FirstCurrency;

    void Awake()
    {
        nameText = transform.FindChild("nameTextField").GetComponent<UITextField>();
        shortDescrText = transform.FindChild("descrTextField").GetComponent<UITextField>();
        buyButtonText = transform.FindChild("BuyPanel/BuyButton/PriceTextField").GetComponent<UITextField>();
        upgradeButtonText = transform.FindChild("UpgradePanel/InfoItem/UpgradeButton/tfTextfield").GetComponent<UITextField>();
    }

    void Start( )
    {
		if( MaxedOutText != null )
			MaxedOutText.text = OnTheRunDataLoader.Instance.GetLocaleString("maxed");

        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        
        UpdatePopupStatus();
    }

    /*
    void Update()
    {
        if (nameText.color.a < 1.0f)
        {
            foreach (GameObject obj in objectToDisable)
            {
                if (obj.GetComponent<BoxCollider2D>().enabled)
                    obj.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
        {
            foreach (GameObject obj in objectToDisable)
            {
                if (!obj.GetComponent<BoxCollider2D>().enabled)
                    obj.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }
    */
    
    public void FillData(bool carChanged = false)
    {
        if (carChanged)
            Manager<UIManager>.Get().ActivePage.GetComponent<UITrucksPage>().ClearSteppers();
       
        UpdatePopupStatus();
    }

    void UpdatePopupStatus()
    {
        OnTheRunDataLoader dataLoader = OnTheRunDataLoader.Instance;
        currSpecialCarData = UnlockingManager.Instance.GetSpecialCarData(specialCarIndex);
        buyCarCurrency = currSpecialCarData.currency;
        transform.FindChild("BuyPanel/BuyButton/Currency/coin").gameObject.SetActive(buyCarCurrency == PriceData.CurrencyType.FirstCurrency);
        transform.FindChild("BuyPanel/BuyButton/Currency/diamond").gameObject.SetActive(buyCarCurrency == PriceData.CurrencyType.SecondCurrency);
        transform.FindChild("nameTextField").GetComponent<UITextField>().text = OnTheRunGameplay.GetCarLocalizedStr(currSpecialCarData.type);
        
        TruckBehaviour.TrasformType truckType = TruckBehaviour.TrasformType.Bigfoot;
        if (currSpecialCarData.type == OnTheRunGameplay.CarId.Firetruck)
            truckType = TruckBehaviour.TrasformType.Firetruck;
        else if (currSpecialCarData.type == OnTheRunGameplay.CarId.Plane)
            truckType = TruckBehaviour.TrasformType.Plane;
        else if (currSpecialCarData.type == OnTheRunGameplay.CarId.Tank)
            truckType = TruckBehaviour.TrasformType.Tank;
        else if (currSpecialCarData.type == OnTheRunGameplay.CarId.Ufo)
            truckType = TruckBehaviour.TrasformType.Ufo;
        transform.FindChild("descrTextField").GetComponent<UITextField>().text = OnTheRunGameplay.GetCarLocalizedDescrStr(truckType);
        string currencyStr = dataLoader.GetLocaleString("diamonds");
#if UNITY_WEBPLAYER
        currencyStr = OnTheRunDataLoader.Instance.GetLocaleString("coins");
#endif       
        transform.FindChild("BuyPanel/buyTextField").GetComponent<UITextField>().text = dataLoader.GetLocaleString("buy_for");
        transform.FindChild("UpgradePanel/tfTextLabel").GetComponent<UITextField>().text = dataLoader.GetLocaleString("duration");

        if (currSpecialCarData == null)
            return;

        Manager<UIManager>.Get().ActivePage.GetComponent<UITrucksPage>().SetupLockedPanel(currSpecialCarData);

        //Debug.Log("UpdatePopupStatus currSpecialCarData.locked " + currSpecialCarData.locked + " currSpecialCarData.canBeBought " + currSpecialCarData.canBeBought + " currSpecialCarData.unlockAtLevel " + currSpecialCarData.unlockAtLevel + " PlayerPersistentData.Instance.Level " + PlayerPersistentData.Instance.Level);

        if (currSpecialCarData.locked)
        {
            transform.FindChild("BuyPanel").gameObject.SetActive(false);
            transform.FindChild("UpgradePanel").gameObject.SetActive(false);
            buyButtonText.text = Manager<UIRoot>.Get().FormatTextNumber(currSpecialCarData.cost);
            transform.FindChild("descrTextField2").gameObject.SetActive(true);

            string descrStr = "";
#if !UNITY_WEBPLAYER
            if (currSpecialCarData.unlockAtLevel <= PlayerPersistentData.Instance.Level)
                descrStr = dataLoader.GetLocaleString("buy_for") + " " + currSpecialCarData.alternativeCost.ToString() + " " + dataLoader.GetLocaleString("coins") + " " + dataLoader.GetLocaleString("or") + " " + dataLoader.GetLocaleString("buy_for") + " " + currSpecialCarData.cost.ToString() + " " + currencyStr;
            else
                descrStr = dataLoader.GetLocaleString("buy_for") + " " + currSpecialCarData.cost.ToString() + " " + currencyStr + " " + OnTheRunDataLoader.Instance.GetLocaleString("or") + " " + OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_prefix") + " " + currSpecialCarData.unlockAtLevel + " " + OnTheRunDataLoader.Instance.GetLocaleString("buy_with_coins_postfix");
                //descrStr = OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_prefix") + " " + currSpecialCarData.unlockAtLevel + " " + OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_postfix") + " " + currSpecialCarData.cost.ToString() + " " + currencyStr;
#else
            if (currSpecialCarData.canBeBought)
                descrStr = dataLoader.GetLocaleString("buy_for") + " " + Manager<UIRoot>.Get().FormatTextNumber((int)currSpecialCarData.alternativeCost) + " " + dataLoader.GetLocaleString("coins");
            else if (currSpecialCarData.unlockAtLevel <= PlayerPersistentData.Instance.Level)
                descrStr = dataLoader.GetLocaleString("buy_for") + " " + currSpecialCarData.cost.ToString() + " " + currencyStr;
            else
                descrStr = OnTheRunDataLoader.Instance.GetLocaleString("unlock_specials_prefix") + " " + currSpecialCarData.unlockAtLevel + " " + OnTheRunDataLoader.Instance.GetLocaleString("buy_with_coins_postfix");
#endif

            transform.FindChild("descrTextField2").GetComponent<UITextField>().text = descrStr;
        }
        else
        {
            transform.FindChild("BuyPanel").gameObject.SetActive(false);
            transform.FindChild("UpgradePanel").gameObject.SetActive(true);
            upgradeButtonText.text = Manager<UIRoot>.Get().FormatTextNumber(currSpecialCarData.upgradeCost);
            upgradeButtonText.ApplyParameters();
            FillDurationBar(currSpecialCarData.level, 0, 10);
            transform.FindChild("descrTextField2").gameObject.SetActive(false);
        }
    }

    void FillDurationBar(int value, int minValue, int maxValue, bool forceEmpty = false)
    {
        Transform parent = transform.FindChild("UpgradePanel/InfoItem");
        GameObject infoBar = parent.FindChild("Bar").gameObject;

        Transform upgradeButton = parent.FindChild("UpgradeButton");
        upgradeButton.GetComponent<UIButton>().State = UIButton.StateType.Normal;
        upgradeButton.FindChild("coin").gameObject.SetActive(upgradeCarCurrency == PriceData.CurrencyType.FirstCurrency);
        upgradeButton.FindChild("diamond").gameObject.SetActive(upgradeCarCurrency == PriceData.CurrencyType.SecondCurrency);

        for (int i = 0; i < minValue; i++)
        {
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));
            currElement.FindChild("empty").gameObject.SetActive(false);
            currElement.FindChild("full").gameObject.SetActive(true);
        }

        for (int i = minValue; i < maxValue; i++)
        {
            bool isEmpty = true;
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));
            if ((i + 1) <= value)
                isEmpty = false;

            currElement.FindChild("empty").gameObject.SetActive(isEmpty || forceEmpty);
            currElement.FindChild("full").gameObject.SetActive(!isEmpty && !forceEmpty);
        }

        for (int i = maxValue; i < infoBar.transform.childCount; i++)
        {
            Transform currElement = infoBar.transform.FindChild("barElement" + (i + 1));
            currElement.FindChild("empty").gameObject.SetActive(false);
            currElement.FindChild("full").gameObject.SetActive(false);
        }

        if (infoBar.transform.childCount <= value || value >= maxValue)
        {
            upgradeButtonText.text = "";
            upgradeButtonText.ApplyParameters();
            upgradeButton.gameObject.SetActive(false);

			if( MaxedOutText != null )
			MaxedOutText.gameObject.SetActive( true );
        }
        else if (!upgradeButton.gameObject.activeInHierarchy)
        {
			if( MaxedOutText != null )
				MaxedOutText.gameObject.SetActive( false );

            upgradeButton.gameObject.SetActive(true);
            //parent.FindChild("UpgradeButton").GetComponent<UIButton>().State = UIButton.StateType.Disabled;
        }
    }

    void Signal_OnBuyReleased(UIButton button)
    {
        Manager<UIManager>.Get().ActivePage.GetComponent<UITrucksPage>().OnBuyPressed(specialCarIndex, truckRef);
        UpdatePopupStatus();
    }

    void Signal_OnBuyAlternativeReleased(UIButton button)
    {
        Manager<UIManager>.Get().ActivePage.GetComponent<UITrucksPage>().OnBuyAlternativePressed(specialCarIndex, truckRef);
        UpdatePopupStatus();
    }

    void Signal_OnUpdateReleased(UIButton button)
    {
        Manager<UIManager>.Get().ActivePage.GetComponent<UITrucksPage>().OnMoreDurationPressed(specialCarIndex);
        UpdatePopupStatus();
    }

    void Signal_OnInfoReleased(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonPopup");
        UIManager uiManager = Manager<UIManager>.Get();
        uiManager.FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunGameplay.GetCarLocalizedLongDescrStr(currSpecialCarData.type);
    }
}