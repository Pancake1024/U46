using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UILoadingPopup")]
public class UILoadingPopup : MonoBehaviour
{
    //protected float inAppFeedbackTimer = -1.0f;
    protected GameObject purchaseAnimObj;
    protected float zRotation = 0.0f;
    OnTheRunInterfaceSounds interfaceSounds;
    protected Vector3 okButtonPosition;

    GameObject okButton;

    void Awake()
    {
        purchaseAnimObj = transform.FindChild("content/icon_purchase").gameObject;
        SetText(OnTheRunDataLoader.Instance.GetLocaleString(""));//"loading"));
    }

    void OnEnable()
    {
        //OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("bg_black_gradient"));
    }

    void Update()
    {
        if (purchaseAnimObj.activeInHierarchy)
        {
            zRotation += TimeManager.Instance.MasterSource.DeltaTime * 150;
            purchaseAnimObj.transform.eulerAngles = new Vector3(0.0f, 0.0f, -zRotation);
        }
    }

    public void SetText(string text)
    {
        transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = "";// text;
    }
}