using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UISkipMissionPopup")]
public class UISkipMissionPopup : MonoBehaviour
{
    OnTheRunInterfaceSounds interfaceSounds;
    public UITextField title_step1;
    public UITextField title_step2;
    public UITextField description_step1;
    public UITextField description_step2;
    public UITextField buttonText;
    public GameObject Step1Item;
    public GameObject Step2Item;
    protected int cost;
    protected OnTheRunSingleRunMissions.DriverMission tierMission;

    void Awake()
    {
        title_step1.text = OnTheRunDataLoader.Instance.GetLocaleString("skip_popup_title_step1");
        title_step2.text = OnTheRunDataLoader.Instance.GetLocaleString("skip_popup_title_step2");
        description_step1.text = OnTheRunDataLoader.Instance.GetLocaleString("skip_popup_text_step1");
        description_step2.text = OnTheRunDataLoader.Instance.GetLocaleString("skip_popup_text_step2");

        transform.FindChild("content/step1/OkButton/tfTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("skip_popup_upgrade");
        transform.FindChild("content/step2/CancelButton/tfTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("cancel");
    }

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        //OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));

        //gameObject.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    public void SetupPopup(int step, int buy_cost = -1)
    {
        tierMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        Step1Item.SetActive(step == 1);
        Step2Item.SetActive(step == 2);
        if (step == 2)
        {
            cost = buy_cost;
            buttonText.text = buy_cost.ToString();
            buttonText.ApplyParameters();
        }
    }

    void Signal_OnSkipButtonRelease(UIButton button)
    {
        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        

        bool canAfford = PlayerPersistentData.Instance.CanAfford(PriceData.CurrencyType.SecondCurrency, cost);
        if (canAfford)
        {
            OnTheRunOmniataManager.Instance.TrackSkipMissionPopup(tierMission.id, tierMission.checkCounter, "buy");
            OnTheRunSingleRunMissions.Instance.BuyCurrentTierMission();
            PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.SecondCurrency, cost);
            UIRewardPage.skipMissionCounter = 0;
            OnTheRunUITransitionManager.Instance.ClosePopup();
        }
        else
            Manager<UIRoot>.Get().ShowWarningPopup(UICurrencyPopup.ShopPopupTypes.Diamonds);
    }

    void Signal_OnOkRelease(UIButton button)
    {
        if (interfaceSounds==null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        iTween.Stop();

        Manager<UIManager>.Get().ActivePage.GetComponent<UIRewardPage>().StopSwitchPanel();

        OnTheRunUITransitionManager.Instance.ClosePopup(Advance);
    }

    public void Advance()
    {
        UIRoot rootManager = Manager<UIRoot>.Get();
        rootManager.SendMessage("OnMissionPageAdvance");
    }

    void Signal_OnCancelRelease(UIButton button)
    {
        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();

        if (Step2Item.activeInHierarchy)
            OnTheRunOmniataManager.Instance.TrackSkipMissionPopup(tierMission.id, tierMission.checkCounter, "skip");
    }

    void OnBackButtonAction()
    {
        UIButton okButton = null;
        if (transform.FindChild("content/step1/OkButton") != null)
            okButton = transform.FindChild("content/step1/OkButton").GetComponent<UIButton>();
        if (okButton != null && okButton.State != UIButton.StateType.Disabled)
            okButton.onReleaseEvent.Invoke(okButton);
    }
}