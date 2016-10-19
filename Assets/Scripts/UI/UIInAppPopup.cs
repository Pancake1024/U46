using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIInAppPopup")]
public class UIInAppPopup : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;
    public GameObject ShopElementPrefab;
    public int numItems = 8;

    bool elementsHeveBeenAdded = false;

    float spaceLeftBelow = 0.14f;
    float aspectRatio = Screen.width / Screen.height;

    void OnEnable()
    {
        if (!elementsHeveBeenAdded)
            AddShopElements();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void AddShopElements()
    {
        GameObject firstElement = GameObject.Instantiate(ShopElementPrefab) as GameObject;
        UINineSlice shopElementBg = firstElement.transform.FindChild("Background_normal").GetComponent<UINineSlice>();

        float elementWidth = shopElementBg.width;
        float elementHeight = shopElementBg.height;

        //Debug.Log("elementWidth: " + elementWidth + ", elementHeight: " + elementHeight);

        float baseScreenWidth = Manager<UIManager>.Get().baseScreenWidth;
        float baseScreenHeight = Manager<UIManager>.Get().baseScreenHeight;
        float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
        int maxColumns = Mathf.FloorToInt((baseScreenWidth / pixelsPerUnit) / elementWidth);
        maxColumns = 4;
        //Debug.Log("maxColumns: " + maxColumns);
        
        int numColumns = numItems < maxColumns ? numItems : maxColumns;
        int numRows = Mathf.CeilToInt(numItems / maxColumns);

        //Debug.Log("numRows: " + numRows + ", " + "numColumns: " + numColumns);

        float horizontalResidue = baseScreenWidth / pixelsPerUnit - numColumns * elementWidth - aspectRatio;
        float horizontalSpacing = horizontalResidue / (numColumns + 1);

        float verticalResidue = baseScreenHeight / pixelsPerUnit - numRows * elementHeight;
        float verticalSpacing = verticalResidue / (numRows + 1);

        verticalSpacing = 0.4f;
        //Debug.Log("horizontalSpacing: " + horizontalSpacing + ", " + "verticalSpacing: " + verticalSpacing);

        Vector3 offset = new Vector3(-1.0f * (elementWidth + horizontalSpacing) * (numColumns - 1) * 0.5f, (elementHeight + verticalSpacing) * (numRows - 1) * 0.5f + spaceLeftBelow, 0.0f);
        float horizontalIncrement = elementWidth + horizontalSpacing;
        float verticalIncrement = elementHeight + verticalSpacing;

        int elementIndex = 0;
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                GameObject currentElement = null;
                if (row == 0 && col == 0)
                    currentElement = firstElement;
                else
                    currentElement = GameObject.Instantiate(ShopElementPrefab) as GameObject;

                currentElement.transform.position = gameObject.transform.position + offset + new Vector3(col * horizontalIncrement, -1.0f * row * verticalIncrement, 0.0f);
                currentElement.transform.parent = gameObject.transform;

                currentElement.transform.FindChild("Background_active").gameObject.SetActive(false);
				//currentElement.transform.FindChild("Price/PriceFrame_normal").gameObject.SetActive(true);
				//currentElement.transform.FindChild("Price/PriceFrame_active").gameObject.SetActive(false);

                FillShopElementData(currentElement, elementIndex);
                ++elementIndex;
            }
        }

        elementsHeveBeenAdded = true;
    }

    void FillShopElementData(GameObject shopElement, int elementIndex)
    {
        UIShopItem shopItemComp = shopElement.GetComponent<UIShopItem>();

        OnTheRunBooster.Booster currBooster = OnTheRunBooster.Instance.GetBoosterById((OnTheRunBooster.BoosterType)elementIndex);
        shopItemComp.boosterRef = currBooster;
        currBooster.shopItemComp = shopItemComp;

        shopElement.transform.FindChild("Badge/BadgeTextfield").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber(currBooster.quantity);
        shopElement.transform.FindChild("Price/PriceTextField").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber((int)currBooster.priceData.Price);
        shopElement.transform.FindChild("Price/Currency/coin").gameObject.SetActive(currBooster.priceData.Currency==PriceData.CurrencyType.FirstCurrency);
        shopElement.transform.FindChild("Price/Currency/diamond").gameObject.SetActive(currBooster.priceData.Currency == PriceData.CurrencyType.SecondCurrency);

        shopElement.transform.FindChild("Background_normal").gameObject.SetActive(!currBooster.equipped);
        shopElement.transform.FindChild("Background_active").gameObject.SetActive(currBooster.equipped);
        if (currBooster.equipped && currBooster.type==OnTheRunBooster.BoosterType.SpecialCar)
            shopItemComp.transform.FindChild("bg_selection_prize").gameObject.SetActive(false);
        
        if (currBooster.type == OnTheRunBooster.BoosterType.MultiplePack || currBooster.type == OnTheRunBooster.BoosterType.SurprisePack)
        {
            shopElement.transform.FindChild("Badge").gameObject.SetActive(false);
        }
    }

    void Signal_OnPlayPressed(UIButton button)
    {
        Manager<UIManager>.Get().PopPopup();

        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        OnTheRunInterfaceSounds interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        Manager<UIManager>.Get().GoToPage("IngamePage");
        Manager<UIRoot>.Get().ShowCurrenciesItem(false);
        OnTheRunBooster.Instance.SaveBoosters();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Ready);

        UIFlyer ready = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero, Quaternion.identity);

        UITextField tf = ready.GetComponentInChildren<UITextField>();
        tf.text = OnTheRunDataLoader.Instance.GetLocaleString("item_ready");
        tf.ApplyParameters();

        GameObject uiRoot = GameObject.Find("UI");
        ready.onEnd.AddTarget(uiRoot, "OnReadyEnded");

        OnTheRunBooster.Instance.ActivateEquippedBoosters();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
    }
}