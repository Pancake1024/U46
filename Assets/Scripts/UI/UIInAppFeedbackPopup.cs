using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIInAppFeedbackPopup")]
public class UIInAppFeedbackPopup : MonoBehaviour
{
    //protected float inAppFeedbackTimer = -1.0f;
    protected GameObject purchaseAnimObj;
    protected float zRotation = 0.0f;
    OnTheRunInterfaceSounds interfaceSounds;
    protected Vector3 okButtonPosition;

    protected bool hideOkButton = true;
    GameObject okButton;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        purchaseAnimObj = transform.FindChild("icon_purchase").gameObject;
        okButton = transform.FindChild("OkButton").gameObject;
        transform.FindChild("OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");

        HideOkButton();
        okButton.SetActive(false);
    }

    void OnEnable()
    {
        //inAppFeedbackTimer = TimeManager.Instance.MasterSource.TotalTime;
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        //HideOkButton();
        //okButton.SetActive(false);
    }

    void Update()
    {
        if (purchaseAnimObj.activeInHierarchy)
        {
            zRotation += TimeManager.Instance.MasterSource.DeltaTime * 150;
            purchaseAnimObj.transform.eulerAngles = new Vector3(0.0f, 0.0f, zRotation);
        }

        if (okButton.transform.position.y == 0.0f || hideOkButton)
            okButton.SetActive(false);
        else
            okButton.SetActive(true);
        /*
        if (inAppFeedbackTimer > 0.0f && TimeManager.Instance.MasterSource.TotalTime - inAppFeedbackTimer > 2.0f)
        {
            inAppFeedbackTimer = -1.0f;
            purchaseAnimObj.SetActive(false);
            transform.FindChild("OkButton").gameObject.SetActive(true);
            transform.FindChild("OkButton").GetComponent<UIButton>().onReleaseEvent.AddTarget(gameObject, "Signal_OkButtonRelease");
            transform.FindChild("popupTextField").GetComponent<UITextField>().text = "PURCHASE COMPLETED";
        }
        */
    }

    void Signal_OkButtonRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        //button.onReleaseEvent.RemoveTarget(gameObject);
        Manager<UIManager>.Get().PopPopup();
    }

    void OnBackButtonAction()
    {
        if (okButton.activeInHierarchy)
        {
            UIButton okButtonComponent = okButton.GetComponent<UIButton>();
            okButtonComponent.onReleaseEvent.Invoke(okButtonComponent);
        }
    }

    public void SetText(string text)
    {
        transform.FindChild("popupTextField").GetComponent<UITextField>().text = text;
    }

    public void ShowOkButton()
    {
        hideOkButton = false;
        //okButton.SetActive(true);
    }

    public void HideOkButton()
    {
        hideOkButton = true;
        //okButton.SetActive(false);
    }

    public void ShowPurchaseIcon()
    {
        purchaseAnimObj.SetActive(true);
    }

    public void HidePurchaseIcon()
    {
        purchaseAnimObj.SetActive(false);
    }
}