using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIDailyBonusSequencePopup")]
public class UIDailyBonusSequencePopup : MonoBehaviour
{
    public UITextField titleText;
    public UITextField descriptionText;
    public UITextField saveText;
    public UITextField daysLeftPrefixText;
    public UITextField daysLeftPostfixText;
    public UITextField daysLeftText;
    public UIButton skipButton;
    public UIButton buyButton;
    public SpriteRenderer icon;
     
    protected int buyStreakBonusCost;
    protected PriceData.CurrencyType currency = PriceData.CurrencyType.SecondCurrency;
    protected UIDailyBonusPopup dailyBonusPopup;

    void Awake()
    {
        buyStreakBonusCost = OnTheRunDataLoader.Instance.GetRecoverStreakCost();
    }

    void Signal_OnEnter(UIPopup popup)
    {
        if (dailyBonusPopup == null)
            dailyBonusPopup = Manager<UIRoot>.Get().gameObject.transform.FindChild("DailyBonusPopup").GetComponent<UIDailyBonusPopup>();

        titleText.text = OnTheRunDataLoader.Instance.GetLocaleString("streak_bonus_title");
        descriptionText.text = OnTheRunDataLoader.Instance.GetLocaleString("streak_bonus_description");
        saveText.text = OnTheRunDataLoader.Instance.GetLocaleString("streak_bonus_buy_text1");
        daysLeftPrefixText.text = OnTheRunDataLoader.Instance.GetLocaleString("streak_bonus_buy_text2");
        daysLeftText.text = OnTheRunDailyBonusManager.Instance.DaysRemaining.ToString();
        daysLeftPostfixText.text = OnTheRunDataLoader.Instance.GetLocaleString("streak_bonus_buy_text3");
        buyButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = buyStreakBonusCost.ToString();
        buyButton.transform.FindChild("coin").gameObject.SetActive(currency==PriceData.CurrencyType.FirstCurrency);
        buyButton.transform.FindChild("diamond").gameObject.SetActive(currency==PriceData.CurrencyType.SecondCurrency);
        skipButton.transform.FindChild("TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("skip");
        icon.sprite = dailyBonusPopup.GetCarSprite();
        buyButton.State = UIButton.StateType.Normal;
        OnTheRunDailyBonusManager.Instance.streakDays = OnTheRunDailyBonusManager.Instance.DaysPassed;
    }

    void Signal_OnExit(UIPopup popup)
    {
    }

    void Signal_OnOkButtonRelease(UIButton button)
    {
        OnTheRunDailyBonusManager.Instance.streakType = "lost";
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //OnTheRunUITransitionManager.Instance.ClosePopup();
        Manager<UIManager>.Get().PopPopup();
        OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusPopup");
    }

    void Signal_OnBuyButtonRelease(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        bool canAfford = PlayerPersistentData.Instance.CanAfford(currency, buyStreakBonusCost);
        if (canAfford)
        {
            OnTheRunDailyBonusManager.Instance.streakType = "bought";

            PlayerPersistentData.Instance.IncrementCurrency(currency, -buyStreakBonusCost);

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_RestoreBonusStreak, currency, buyStreakBonusCost.ToString(), OmniataIds.Product_Type_Standard);

            buyButton.State = UIButton.StateType.Disabled;
            OnTheRunDailyBonusManager.Instance.RestoreDailyBonusStreak();
            Manager<UIManager>.Get().PopPopup();
            OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusPopup");
        }
        else
        {
            Manager<UIManager>.Get().PushPopup("CurrencyPopup");
            Manager<UIManager>.Get().FrontPopup.SendMessage("Initialize", UICurrencyPopup.ShopPopupTypes.Diamonds);
            OnTheRunUITransitionManager.Instance.OnPageChanged("CurrencyPopup", "DailyBonusSequencePopup");
        }
    }

    void OnBackButtonAction()
    {
        UIButton okButton = transform.FindChild("content/content/OkButton").GetComponent<UIButton>();
        okButton.onReleaseEvent.Invoke(okButton);
    }
}