using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIFuelGiftPopup")]
public class UIFuelGiftPopup : MonoBehaviour
{
    public UITextField tfTitle;
    public UITextField tfDescription;
    public GameObject icon;

    OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        tfTitle.text = OnTheRunDataLoader.Instance.GetLocaleString("fuel_gift_title");
        tfDescription.text = OnTheRunDataLoader.Instance.GetLocaleString("fuel_gift_description");
    }

    void StartFireworks( )
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("fireworks"));
    }

    void Signal_OnOkSingleButtonRelease(UIButton button)
    {
        PlayerPersistentData.Instance.SaveFirstTimeFuelFinished();
        OnTheRunFuelManager.Instance.Fuel += OnTheRunDataLoader.Instance.GetFirstFuelGift();
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
    }

    void OnBackButtonAction()
    {
        if (Manager<UIManager>.Get().disableInputs)
            return;

        UIButton okButton = null;
        if (transform.FindChild("content/ResumeButton") != null)
            okButton = transform.FindChild("content/ResumeButton").GetComponent<UIButton>();

        if (okButton != null && okButton.State != UIButton.StateType.Disabled)
            okButton.onReleaseEvent.Invoke(okButton);
    }
}