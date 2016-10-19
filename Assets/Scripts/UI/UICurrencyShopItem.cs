using UnityEngine;
using SBS.Core;
using System;
using System.Globalization;

[AddComponentMenu("OnTheRun/UI/UICurrencyItem")]
public class UICurrencyShopItem : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UITextField tfQuantity = null;
    protected UITextField tfOldQuantity = null;
    protected UITextField tfPrice = null;
    protected GameObject bestValue = null;
    protected GameObject specialOffer = null;
    protected UIButton purchaseButton = null;
    protected GameObject freezeObjects = null;
    protected GameObject background = null;

    public GameObject fuelGroup = null;
    public GameObject fuelFreezeGroup = null;
    protected UITextField tfFuelQuantity = null;
    protected UITextField tfFuelFreezeText = null;
    protected UITextField tfFuelFreezeTime = null;

    public GameObject coinIcon;
    public GameObject diamondIcon;
    public GameObject fuelIcon;
    public GameObject redStripe;

    protected UICurrencyPopup.ShopPopupTypes itemType;

    void OnEnable()
    {
        //FindObjects();
        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void FindObjects()
    {
        purchaseButton = transform.FindChild("PurchaseButton").gameObject.GetComponent<UIButton>();
        tfPrice = purchaseButton.transform.FindChild("tfPrice").gameObject.GetComponent<UITextField>();
        bestValue = transform.FindChild("BestValue").gameObject;
        specialOffer = transform.FindChild("SpecialOffer").gameObject;
        tfQuantity = transform.FindChild("tfQuantity").gameObject.GetComponent<UITextField>();
        tfOldQuantity = transform.FindChild("tfOldQuantity").gameObject.GetComponent<UITextField>();
        redStripe = tfOldQuantity.transform.GetChild(0).gameObject;
        tfQuantity.gameObject.SetActive(false);
        tfQuantity.gameObject.SetActive(true);
        tfOldQuantity.gameObject.SetActive(false);

        if (fuelGroup != null)
        {
            tfFuelQuantity = transform.FindChild("FuelGroup/tfQuantity").gameObject.GetComponent<UITextField>();
            fuelGroup.SetActive(false);
        }
        if (fuelFreezeGroup != null)
        {
            tfFuelFreezeText = transform.FindChild("FuelFreezeGroup/tfText").gameObject.GetComponent<UITextField>();
            tfFuelFreezeTime = transform.FindChild("FuelFreezeGroup/tfTime").gameObject.GetComponent<UITextField>();
            fuelFreezeGroup.SetActive(false);
        }
    }

    public void SetQuantityText(string quantity, float discount, UICurrencyPopup.ShopPopupTypes shopType)
    {
        if (tfQuantity != null && quantity != "")
        {
            int textToShow = int.Parse(quantity, CultureInfo.InvariantCulture);
            if (itemType==UICurrencyPopup.ShopPopupTypes.Fuel)
            {
                tfFuelQuantity.gameObject.SetActive(true);
                tfFuelQuantity.text = Manager<UIRoot>.Get().FormatTextNumber(textToShow);
                tfFuelQuantity.ApplyParameters();
                
                tfQuantity.gameObject.SetActive(false);
            }
            else //if (itemType == UICurrencyPopup.ShopPopupTypes.Money)
            {
                    tfQuantity.gameObject.SetActive(true);
                    tfQuantity.text = Manager<UIRoot>.Get().FormatTextNumber(textToShow);
                    tfQuantity.ApplyParameters();
            }

            tfOldQuantity.gameObject.SetActive(discount > 0.0f);
            if (discount > 0.0f && OnTheRunDataLoader.Instance.GetShowShopPerc(shopType))
                tfOldQuantity.text = "+" + discount + "%";
            else
                tfOldQuantity.text = "";

            redStripe.SetActive(false);
            
            //old-------------
            //tfOldQuantity.text = Manager<UIRoot>.Get().FormatTextNumber((int)((int.Parse(quantity, CultureInfo.InvariantCulture) * 0.8f)));
            //redStripe.transform.localScale = new Vector3(tfOldQuantity.text.Length * 5.0f, redStripe.transform.localScale.y, redStripe.transform.localScale.z);

            //tfOldQuantity.text = "20% off";
            //redStripe.SetActive(false);
        }
        else
        {
            if (tfQuantity.gameObject.activeInHierarchy)
            {
                tfQuantity.text = "-";
                tfQuantity.ApplyParameters();
            }
        }
    }

    public void SetPriceText(string price)
    {
        if (tfPrice != null && tfPrice.gameObject.activeInHierarchy)
        {
            tfPrice.text = price; // +(currentType == UICurrencyPopup.ShopPopupTypes.Diamonds ? " €" : "");
            tfPrice.ApplyParameters();
        }
    }
    
    public void SetItemType(UICurrencyPopup.ShopPopupTypes type)
    {
        itemType = type;
        if (fuelGroup != null)
            fuelGroup.SetActive(type == UICurrencyPopup.ShopPopupTypes.Fuel);
        if (fuelFreezeGroup != null)
            fuelFreezeGroup.SetActive(type == UICurrencyPopup.ShopPopupTypes.FuelFreeze);
    }

    public void SetTypeText(string quantity, string price, UICurrencyPopup.ShopPopupTypes shopType, float discount = 0.0f)
    {
        SetQuantityText(quantity, discount, shopType);
        SetPriceText(price);
        SetFreezeState(shopType == UICurrencyPopup.ShopPopupTypes.Fuel || shopType == UICurrencyPopup.ShopPopupTypes.FuelFreeze);
    }

    public void ActivateBestTag(bool activate)
    {
        if (purchaseButton == null)
            this.FindObjects();

        bestValue.gameObject.SetActive(activate);
        //tfOldQuantity.gameObject.SetActive(activate);
    }

    public void ActivateSpecialOffer(bool activate)
    {
        specialOffer.gameObject.SetActive(activate);
        tfOldQuantity.gameObject.SetActive(activate);
        if(activate)
        {
            specialOffer.transform.FindChild("label").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popular_tag");
        }
    }
    
    public void EnableButton()
    {
        purchaseButton.State = UIButton.StateType.Normal;
    }

    public void DisableButton()
    {
        purchaseButton.State = UIButton.StateType.Disabled;
    }

    public void SetType(UICurrencyPopup.ShopPopupTypes curr, bool showDiamondIcon)
    {
        if (purchaseButton == null)
            FindObjects();

        purchaseButton.transform.FindChild("diamond").gameObject.SetActive(showDiamondIcon);
        coinIcon.SetActive(curr == UICurrencyPopup.ShopPopupTypes.Money);
        diamondIcon.SetActive(curr == UICurrencyPopup.ShopPopupTypes.Diamonds);
        fuelIcon.SetActive(curr == UICurrencyPopup.ShopPopupTypes.Fuel || curr == UICurrencyPopup.ShopPopupTypes.FuelFreeze);
    }

    public void SetFreezeState(bool active)
    {
        Transform tr = transform.FindChild("Background_ice");
        if (tr!=null)
        {
            fuelFreezeGroup.SetActive(active);
            freezeObjects = transform.FindChild("Background_ice").gameObject;
            freezeObjects.SetActive(active);
            tfQuantity.gameObject.SetActive(true);
            tfQuantity.gameObject.SetActive(!active);
            if (active)
            {
                tfQuantity.text = "";
            }
            if (active)
            {
                fuelIcon.SetActive(false);
                bool fuelSaverActive = OnTheRunFuelManager.Instance.IsFuelFreezeActive();
                purchaseButton.State = fuelSaverActive ? UIButton.StateType.Disabled : UIButton.StateType.Normal;
                tfFuelFreezeTime.gameObject.SetActive(fuelSaverActive);
                string notActiveString = OnTheRunDataLoader.Instance.GetLocaleString("currency_buy_freeze_prefix") + " " +OnTheRunDataLoader.Instance.GetFuelFreezeTime()+" "+ OnTheRunDataLoader.Instance.GetLocaleString("currency_buy_freeze_postfix");
                string activeString = OnTheRunDataLoader.Instance.GetLocaleString("currency_bought_freeze");
                tfFuelFreezeText.text = fuelSaverActive ? activeString : notActiveString;
            }
            else
                purchaseButton.gameObject.SetActive(true);
        }
    }

    public void Update( )
    {
        if (tfQuantity.text == "" && tfQuantity.gameObject.activeInHierarchy)
        {
            tfQuantity.text = "";
            tfQuantity.ApplyParameters();
        }

        if (itemType == UICurrencyPopup.ShopPopupTypes.FuelFreeze && tfFuelFreezeTime.gameObject.activeInHierarchy)
        {
            TimeSpan time = OnTheRunFuelManager.Instance.FuelFreezeTimer;
            tfFuelFreezeTime.text = time.ToString().Substring(0, 8);
        }
    }
}