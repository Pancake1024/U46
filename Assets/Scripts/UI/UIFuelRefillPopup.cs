using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIFuelRefillPopup")]
public class UIFuelRefillPopup : MonoBehaviour
{
    public UITextField title;
    public UITextField description1;
    public UITextField description2;
    public UITextField description3;
    public UITextField buttonText;

    void Awake()
    {
        title.text = OnTheRunDataLoader.Instance.GetLocaleString("not_enough_fuel");
        description1.text = OnTheRunDataLoader.Instance.GetLocaleString("watch_video");
        description2.text = OnTheRunDataLoader.Instance.GetLocaleString("get_free_fuel");
        description3.text = OnTheRunDataLoader.Instance.GetLocaleString("or");
        buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("buy_fuel");


    }

    void Signal_OnWatchVideoRelease(UIButton button)
    {
        OnTheRunCoinsService.Instance.WatchVideoForCallback((success) =>
        {
            if (success)
            {
                OnTheRunFuelManager.Instance.Fuel += 1;
                Manager<UIRoot>.Get().UpdateCurrenciesItem();
                OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.FreeFuel);
            }
            
            OnTheRunUITransitionManager.Instance.ClosePopup();
        });
    }
     
    void Signal_OnGoToShopRelease(UIButton button)
    {
        //directly to the shop
        this.StartCoroutine(this.AfterPopupClosed());
    }

    IEnumerator AfterPopupClosed()
    {
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        //OnTheRunUITransitionManager.Instance.ClosePopup();
        Manager<UIManager>.Get().PopPopup();

        Manager<UIRoot>.Get().OpenCurrencyPopup(UICurrencyPopup.ShopPopupTypes.Fuel);
    }

    void Signal_OnTapOutside(UIButton button)
    {
         OnTheRunUITransitionManager.Instance.ClosePopup();
    }

    void OnBackButtonAction()
    {
        OnTheRunUITransitionManager.Instance.ClosePopup();
        //UIButton collectButton = transform.FindChild("content/CollectButton").GetComponent<UIButton>();
        //collectButton.onReleaseEvent.Invoke(collectButton);
    }
}