using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIFBRequestLoginPopup")]
public class UIFBRequestLoginPopup : MonoBehaviour
{
    public GameObject FBIcon;
    public UITextField TitleTextfield;
    public UITextField DescriptionTextfield1;
    public UITextField DescriptionTextfield2;
    public UITextField DescriptionTextfield3;
    public UIButton loginButton;
    public UIButton backButton;
    OnTheRunInterfaceSounds interfaceSounds;

    protected int rewardValue = 5;
    protected PriceData.CurrencyType rewardCurrency = PriceData.CurrencyType.SecondCurrency;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        //backButton.gameObject.GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("later").ToUpper();// OnTheRunDataLoader.Instance.GetLocaleString("skip");
        loginButton.gameObject.GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("login");
        loginButton.gameObject.GetComponentInChildren<UITextField>().ApplyParameters();

        TitleTextfield.text = OnTheRunDataLoader.Instance.GetLocaleString("login_facebook_notlogged_title");
        DescriptionTextfield1.text = OnTheRunDataLoader.Instance.GetLocaleString("login_facebook_notlogged_compete");
        DescriptionTextfield2.text = OnTheRunDataLoader.Instance.GetLocaleString("login_facebook_notlogged_fuel");
        DescriptionTextfield3.text = OnTheRunDataLoader.Instance.GetLocaleString("login_facebook_notlogged_diamonds");
        
        if(FBIcon!=null)
        {
            float xPos = loginButton.transform.position.x - loginButton.GetComponent<BoxCollider2D>().size.x * 0.4f;
            Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
            FBIcon.transform.position = newPos;
        }
    }

    void Signal_OnSkipRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
    }
    
    void Signal_OnLoginWithRewardRelease(UIButton button)
    {
        //EncryptedPlayerPrefs.SetInt("fbp_rs", 1);
        PlayerPersistentData.Instance.IncrementCurrency(rewardCurrency, rewardValue);
        Manager<UIRoot>.Get().UpdateCurrenciesItem();
        Manager<UIManager>.Get().PopPopup();
        Manager<UIManager>.Get().ActivePage.SendMessage("Signal_OnFBLoginRelease", button);
    }

    void OnBackButtonAction()
    {
        UIButton skipButton = transform.FindChild("content/SkipButton").GetComponent<UIButton>();
        skipButton.onReleaseEvent.Invoke(skipButton);
    }
}