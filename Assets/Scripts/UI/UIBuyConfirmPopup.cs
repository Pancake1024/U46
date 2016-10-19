using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIBuyConfirmPopup")]
public class UIBuyConfirmPopup : MonoBehaviour
{
    public UITextField tfOk;
    public UITextField tfNo;
    public UITextField tfCost;
    public UITextField tfText;
    public GameObject diamondIcon;
    public GameObject coinIcon;

    OnTheRunInterfaceSounds interfaceSounds;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        //transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");
        if (null != tfOk)
            tfOk.text = OnTheRunDataLoader.Instance.GetLocaleString("ok");

#if UNITY_WEBPLAYER
        if (null != tfOk)
        {
            if (gameObject.name.Equals("HelpPopup") || gameObject.name.Equals("HighscoresPopup"))
                tfOk.text = OnTheRunDataLoader.Instance.GetLocaleString("web_back");
        }
#endif
        gameObject.BroadcastMessage("ResetEnterExitAnimation", UIEnterExitAnimations.AnimationType.FadeInPopup);
    }

    void Initialize(PlayerPersistentData.PlayerData playerData)
    {
        int cost = playerData.cost;
        string popupText = OnTheRunDataLoader.Instance.GetLocaleString("confirm_buy_car");
        string okText = OnTheRunDataLoader.Instance.GetLocaleString("yes");
        string cancelText = OnTheRunDataLoader.Instance.GetLocaleString("no");
        tfOk.text = okText;
        tfNo.text = cancelText;
        tfText.text = popupText;
        tfCost.text = Manager<UIRoot>.Get().FormatTextNumber(cost);
        coinIcon.SetActive(playerData.currency == PriceData.CurrencyType.FirstCurrency);
        diamondIcon.SetActive(playerData.currency == PriceData.CurrencyType.SecondCurrency);

        OnResizeDone(tfCost);
    }

    //public float ccc = 0.2f;
    //public float starSpace = 0.4f;
    void OnResizeDone(UITextField tf)
    {
        float size = tfCost.text.Length * 0.2f + 0.4f; //* ccc + starSpace;
        tfCost.transform.localPosition = new Vector3(size * 0.5f - 0.4f, tfCost.transform.localPosition.y, tfCost.transform.localPosition.z);
        diamondIcon.transform.localPosition = new Vector3(tfCost.transform.localPosition.x + 0.4f, diamondIcon.transform.localPosition.y, diamondIcon.transform.localPosition.z);
        coinIcon.transform.localPosition = new Vector3(tfCost.transform.localPosition.x + 0.4f, coinIcon.transform.localPosition.y, coinIcon.transform.localPosition.z);
    }
    /*
    void Update()
    {
        OnResizeDone(tfCost);
    }
    */
    void Signal_OnCancelRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
#if UNITY_WEBPLAYER
        if (TimeManager.Instance.MasterSource.IsPaused)
            Manager<UIManager>.Get().PopPopup();
        else
            OnTheRunUITransitionManager.Instance.ClosePopup();
#else
        OnTheRunUITransitionManager.Instance.ClosePopup();
#endif
    }

    void Signal_OnOkRelease(UIButton button)
    {
        Manager<UIManager>.Get().ActivePage.SendMessage("OnCloseConfirmPopup");
    }


    void OnBackButtonAction()
    {
        UIButton cancelButton = null;
        if (transform.FindChild("content/CancelButton") != null)
            cancelButton = transform.FindChild("content/CancelButton").GetComponent<UIButton>();

        UIButton okButton = null;
        if (transform.FindChild("content/OkButton") != null)
            okButton = transform.FindChild("content/OkButton").GetComponent<UIButton>();

        if (cancelButton != null && cancelButton.State != UIButton.StateType.Disabled)
            cancelButton.onReleaseEvent.Invoke(cancelButton);
        else if (okButton != null && okButton.State != UIButton.StateType.Disabled)
            okButton.onReleaseEvent.Invoke(okButton);
        else
            Signal_OnCancelRelease(null);
    }
}