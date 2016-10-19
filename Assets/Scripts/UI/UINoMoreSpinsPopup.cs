using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UINoMoreSpinsPopup")]
public class UINoMoreSpinsPopup : MonoBehaviour
{
    OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void Signal_OnCancelRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
    }

    void Signal_OnBuyExtraSpinRelease(UIButton button)
    {
        int extraSpinGained = OnTheRunDataLoader.Instance.GetExtraSpinData()[0];
        int extraSpinCost = OnTheRunDataLoader.Instance.GetExtraSpinData()[1];
        UIRoot uiRoot = Manager<UIRoot>.Get();

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, extraSpinCost);
        if (canAfford)
        {
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, extraSpinCost);
            PlayerPersistentData.Instance.ExtraSpin += extraSpinGained;
            //Manager<UIManager>.Get().BroadcastMessage("UpdateInterface");
            uiRoot.UpdateCurrenciesItem();

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_ExtraSpin, PriceData.CurrencyType.SecondCurrency, extraSpinCost.ToString(), OmniataIds.Product_Type_Standard);

            OnTheRunUITransitionManager.Instance.ClosePopup();
        }
        else
        {
            OnTheRunUITransitionManager.Instance.ClosePopupAndOpen("NoMoreDiamonds", OnTheRunDataLoader.Instance.GetLocaleString("not_enough_diamonds"));
        }
    }

    void OnBackButtonAction()
    {
        UIButton skipButton = transform.FindChild("content/SkipButton").GetComponent<UIButton>();
        skipButton.onReleaseEvent.Invoke(skipButton);
    }
}