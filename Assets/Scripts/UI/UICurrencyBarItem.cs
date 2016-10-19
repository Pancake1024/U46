using UnityEngine;
using SBS.Core;
using System.Collections;
using System;

[AddComponentMenu("OnTheRun/UI/UICurrencyBarItem")]
public class UICurrencyBarItem : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected float TEMPMAXFUEL = 20.0f;

    public GameObject CoinItemButton;
    public GameObject DiamondItem;
    public GameObject FuelItem;
    public GameObject LinksBarItem;

    public GameObject saveFuelTimer;
    public GameObject normalBar;
    public GameObject freezeBar;

    public GameObject barElementPrefab;
    public GameObject[] fuelsIcons;
    public GameObject[] fuelsFreezeIcons;

    protected int fuelBarElementsNum = 3;
    protected bool fuelSaverActive = false;
    protected bool prevSaverActive = false;

    protected UIManager uiManager;

    GameObject fuelBar;
    UITextField tfFuelTime;
    UITextField tfFuelValue;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        LinksBarItem.SetActive(false);
        uiManager = Manager<UIManager>.Get();

#if UNITY_WEBPLAYER
        LinksBarItem.SetActive(true);
        //LinksBarBgItem.SetActive(true);
        Transform coinItem = CoinItemButton.transform.parent.transform;
        coinItem.localPosition = new Vector3(-0.8f, coinItem.localPosition.y, coinItem.localPosition.z);
        coinItem.FindChild("Background_normal").GetComponent<UINineSlice>().width = 1.45f;
        CoinItemButton.transform.FindChild("Normal/coin").transform.parent = coinItem;
#endif
    }

    void Update()
    {
        RefreshTimeData();
    }

    void UpdateFuelText()
    {

        if (tfFuelTime.gameObject.activeInHierarchy)
        {
            string millis = StringUtils.MillisecondsToTime(OnTheRunFuelManager.Instance.RemainingFuelTimerNextSlot, false); //display time for only next slot to fill up
            tfFuelTime.text = millis;
            //tfFuelTime.text = StringUtils.MillisecondsToTime(OnTheRunFuelManager.Instance.RemainingFuelTimer, false); //display time for all slot to fill up
        }
    }

    void SetFuelVisibility(bool visib)
    {
        if (OnTheRunFuelManager.Instance.IsFuelFreezeActive())
        {
            fuelsFreezeIcons[0].SetActive(visib);
            fuelsFreezeIcons[1].SetActive(!visib);
        }
        else
        {
            fuelsIcons[0].SetActive(visib);
            fuelsIcons[1].SetActive(!visib);
        }
    }

    #region Messages
    void OnShowBar()
    {
        fuelSaverActive = OnTheRunFuelManager.Instance.IsFuelFreezeActive();

        fuelBar = transform.FindChild("FuelItem/FuelNormal/FuelBar").gameObject;
        tfFuelTime = transform.FindChild("FuelItem/FuelNormal/tfFuelTime").GetComponent<UITextField>();
        tfFuelValue = transform.FindChild("FuelItem/FuelNormal/tfFuelValue").GetComponent<UITextField>();

        transform.FindChild("FuelItem/FuelFreeze/tfFuelFreezeText").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("freeze");

        prevSaverActive = fuelSaverActive;
        normalBar.SetActive(!fuelSaverActive);
        freezeBar.SetActive(fuelSaverActive);


        CreateFuelBar();
        SetFuelValue();
        UpdateTotalCoinsValue();
        RefreshTimeData();

        SetFuelVisibility(true);


#if UNITY_WEBPLAYER
        FuelItem.SetActive(false);
        DiamondItem.SetActive(false);
        CoinItemButton.SetActive(false);
#endif

    }
    #endregion

    #region Signals
    void Signal_OnCoinsReleased(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition || uiManager.IsPopupInStack("NoMoreSpinsPopup"))
            return;

        //Debug.Log("*** Signal_OnCoinsReleased " + OnTheRunUITransitionManager.Instance.isInTransition + " " + uiManager.IsPopupInStack("NoMoreSpinsPopup"));
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        StartCoroutine(GoToNextPage("CurrencyPopup", UICurrencyPopup.ShopPopupTypes.Money));
    }

    void Signal_OnDiamondsReleased(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition)
            return;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        StartCoroutine(GoToNextPage("CurrencyPopup", UICurrencyPopup.ShopPopupTypes.Diamonds));
    }

    void Signal_OnFuelReleased(UIButton button)
    {
        if (OnTheRunUITransitionManager.Instance.isInTransition)
            return;

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        StartCoroutine(GoToNextPage("CurrencyPopup", UICurrencyPopup.ShopPopupTypes.Fuel));
    }
    #endregion

    public void SetFuelTime()
    {

    }

    public void UpdateTotalCoinsValue()
    {
        transform.FindChild("CoinsItem/ValueTextField").GetComponent<UITextField>().text = Manager<UIRoot>.Get().FormatTextNumber((int)PlayerPersistentData.Instance.Coins);
    }

    protected float totalBarWidth = 65.0f;
    protected float intraElementsSpace = 1.0f;
    protected float elementWidth = 10.0f;
    protected float elementScaleX = 1.0f;
    
    void CreateFuelBar()
    {
        fuelBarElementsNum = OnTheRunDataLoader.Instance.GetFuelVisibleInBar();
        //fuelBarElementsNum = 3;
        if (fuelBar.transform.childCount != fuelBarElementsNum)
        {
            int childNumber = fuelBar.transform.childCount;
            for (int i = 0; i < childNumber; i++)
            {
                DestroyImmediate(fuelBar.transform.FindChild("barElement" + (i + 1)).gameObject);
            }

            elementScaleX = totalBarWidth / ((elementWidth + intraElementsSpace) * fuelBarElementsNum);
             
            for (int i = 0; fuelBarElementsNum > i; i++)
            {
                GameObject barElement = Instantiate(barElementPrefab) as GameObject;
                barElement.name = "barElement" + (fuelBarElementsNum - i);
                barElement.transform.parent = fuelBar.transform;
                barElement.transform.localPosition = new Vector3(i * -((elementWidth + intraElementsSpace) * elementScaleX) * 0.01f, 0.0f, 0.0f);
                barElement.transform.localScale = new Vector3(elementScaleX, 1.0f, 1.0f);
            }
        }
    }

    public void SetFuelValue()
    {
        //float fuelPerc = (OnTheRunFuelManager.Instance.Fuel / 100.0f) * TEMPMAXFUEL;

        //for (int i = 0; i < fuelBar.transform.childCount; ++i)
        //{
        //    bool isEmpty = true;
        //    Transform currElement = fuelBar.transform.FindChild("barElement" + (i + 1));
        //    if ((i + 1) <= fuelPerc)
        //    {
        //        isEmpty = false;
        //    }

        //    currElement.FindChild("empty").gameObject.SetActive(isEmpty);
        //    currElement.FindChild("full").gameObject.SetActive(!isEmpty);
        //}

        //if (fuelPerc > fuelBar.transform.childCount)
        //    tfFuelValue.text = (fuelPerc - fuelBar.transform.childCount).ToString();

        //tfFuelValue.gameObject.SetActive(fuelPerc > fuelBar.transform.childCount);
        //tfFuelTime.gameObject.SetActive(fuelPerc <= fuelBar.transform.childCount);

        if (fuelBar == null)
            return;

        for (int i = 0; i < fuelBar.transform.childCount; i++)
        {
            bool isEmpty = true;
            Transform currElement = fuelBar.transform.FindChild("barElement" + (i + 1));
            if ((i + 1) <= OnTheRunFuelManager.Instance.Fuel)
                isEmpty = false;

            currElement.FindChild("empty").gameObject.SetActive(isEmpty);
            currElement.FindChild("full").gameObject.SetActive(!isEmpty);
        }

        tfFuelValue.text = "+" + Manager<UIRoot>.Get().FormatTextNumber((int)(OnTheRunFuelManager.Instance.Fuel - fuelBar.transform.childCount));
        if (tfFuelTime != null && TimeManager.Instance != null)
            tfFuelTime.text = StringUtils.MillisecondsToTime(TimeManager.Instance.MasterSource.TotalTime, false);

        tfFuelValue.gameObject.SetActive(OnTheRunFuelManager.Instance.Fuel > fuelBar.transform.childCount);
        if (tfFuelTime != null && TimeManager.Instance != null)
            tfFuelTime.gameObject.SetActive(OnTheRunFuelManager.Instance.Fuel < fuelBar.transform.childCount);

        UpdateFuelText();
    }

    #region UI Animations
    public IEnumerator GoToNextPage(string nextPage, UICurrencyPopup.ShopPopupTypes popupType)
    {
        uiManager.ActivePage.BroadcastMessage("ResetAnimationsCounter", SendMessageOptions.DontRequireReceiver);
        string currentPage = uiManager.ActivePageName;

        if (uiManager.IsPopupInStack("CurrencyPopup"))
        {
            if (uiManager.FrontPopup.Equals("CurrencyPopup"))
            {
                OnTheRunUITransitionManager.Instance.ClosePopup();
                //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
                while (UIEnterExitAnimations.activeAnimationsCounter > 0)
                {
                    yield return null;
                }
                uiManager.PushPopup(nextPage);
                uiManager.FrontPopup.SendMessage("Initialize", popupType);
            }
            else
            {
                //uiManager.PopPopup();
                //uiManager.PushPopup(nextPage);
                if (nextPage.Equals("CurrencyPopup"))
                {
                    uiManager.PopPopup();
                    uiManager.PushPopup(nextPage);
                }
                else
                    uiManager.PushPopup(nextPage, true);

                uiManager.FrontPopup.transform.position = Vector3.zero;
                uiManager.FrontPopup.SendMessage("Initialize", popupType);
            }
        }
        else
        {
            bool inappRemoveAdsFeedbackActive = OnTheRunDataLoader.Instance.ShowInappRemoveAdsFeedbackPopup();
            if (inappRemoveAdsFeedbackActive)
            {
                UIMsgPopup msgPopup = uiManager.FrontPopup.GetComponent<UIMsgPopup>();
                while (msgPopup != null && msgPopup.gameObject.activeSelf)
                {
                    yield return null;
                }
            }

            bool wheelPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("WheelPopup");
            bool optionsPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("OptionsPopup");
            bool facebookFriendsPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("FBFriendsPopup");
            if (wheelPopupActive)
                currentPage = "WheelPopup";
            else if (optionsPopupActive)
                currentPage = "OptionsPopup";
            else if (facebookFriendsPopupActive)
                currentPage = "FBFriendsPopup";

            Manager<UIRoot>.Get().lastPageBeforePopup = currentPage;
            OnTheRunUITransitionManager.Instance.OnPageExiting(currentPage, nextPage);

            //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
            while (UIEnterExitAnimations.activeAnimationsCounter > 0)
            {
                yield return null;
            }

            uiManager.PushPopup(nextPage);
            uiManager.FrontPopup.SendMessage("Initialize", popupType);
            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
        }
    }


    void RefreshTimeData()
    {
        fuelSaverActive = OnTheRunFuelManager.Instance.IsFuelFreezeActive();
        if (fuelSaverActive != prevSaverActive)
        {
            normalBar.SetActive(!fuelSaverActive);
            freezeBar.SetActive(fuelSaverActive);
        }
        prevSaverActive = fuelSaverActive;

        if (saveFuelTimer.activeInHierarchy)
        {
            TimeSpan time = OnTheRunFuelManager.Instance.FuelFreezeTimer;
            saveFuelTimer.GetComponent<UITextField>().text = time.ToString().Substring(0, 8);
        }

        UpdateFuelText();
    }
    #endregion
}