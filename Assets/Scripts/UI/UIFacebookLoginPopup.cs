using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIFacebookLoginPopup")]
public class UIFacebookLoginPopup : MonoBehaviour
{
    public GameObject FBIcon;
    public UITextField TitleTextfield;
    public UITextField DescriptionTextfield1;
    public UITextField DescriptionTextfield2;
    public UITextField RewardTextfield;
    public UITextField GetTextfield;
    public UIButton loginButton;
    public GameObject CoinsIcon;
    public GameObject DiamondIcon;
    OnTheRunInterfaceSounds interfaceSounds;

    protected int rewardValue;
    protected PriceData.CurrencyType rewardCurrency;

    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        transform.FindChild("content/SkipButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("later").ToUpper();// OnTheRunDataLoader.Instance.GetLocaleString("skip");
        transform.FindChild("content/LoginButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("login");
        transform.FindChild("content/LoginButton").GetComponentInChildren<UITextField>().ApplyParameters();

        if (DescriptionTextfield1 != null)
        {
            DescriptionTextfield1.text = "";
            DescriptionTextfield2.text = "";
            TitleTextfield.text = OnTheRunDataLoader.Instance.GetLocaleString("facebook_login_popup_title");
            GetTextfield.text = OnTheRunDataLoader.Instance.GetLocaleString("facebook_login_popup_get");
        }

        if(FBIcon!=null)
        {
            float xPos = loginButton.transform.position.x - loginButton.GetComponent<BoxCollider2D>().size.x * 0.4f;
            Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
            FBIcon.transform.position = newPos;
        }

        if (RewardTextfield != null)
        {

            DescriptionTextfield1.text = OnTheRunDataLoader.Instance.GetLocaleString("facebook_login_popup_description_1");
            DescriptionTextfield2.text = OnTheRunDataLoader.Instance.GetLocaleString("facebook_login_popup_description_2");
            TitleTextfield.GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("facebook_login_popup_title");
        }
    }

    void Initialize(int counter)
    {
        counter -= OnTheRunDataLoader.Instance.GetFacebookRewardAppearAfter();

        if (counter > OnTheRunDataLoader.Instance.GetFacebookReward().Length - 1)
            counter = OnTheRunDataLoader.Instance.GetFacebookReward().Length - 1;

        rewardValue = (int)OnTheRunDataLoader.Instance.GetFacebookReward()[counter];
        rewardCurrency = OnTheRunDataLoader.Instance.GetFacebookRewardCurrency()[counter];
        RewardTextfield.text = rewardValue.ToString();
        CoinsIcon.SetActive(rewardCurrency == PriceData.CurrencyType.FirstCurrency);
        DiamondIcon.SetActive(rewardCurrency == PriceData.CurrencyType.SecondCurrency);
    }

    void Signal_OnSkipRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
    }

    void Signal_OnLoginRelease(UIButton button)
    {
        Manager<UIManager>.Get().PopPopup();
        Manager<UIManager>.Get().ActivePage.SendMessage("Signal_OnFBLoginRelease", button);
    }

    void Signal_OnLoginWithRewardRelease(UIButton button)
    {
        EncryptedPlayerPrefs.SetInt("fbp_rs", 1);
        PlayerPersistentData.Instance.IncrementCurrency(rewardCurrency, rewardValue);
        Manager<UIManager>.Get().PopPopup();
        //Manager<UIManager>.Get().ActivePage.SendMessage("Signal_OnFBLoginRelease", button);
        
        ShowLoadingPopup();

        OnTheRunFacebookManager.Instance.Login(
            () =>
            {
                OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, null);
                HideLoadingPopup();
            },
            () => { HideLoadingPopup(); },
            () => { HideLoadingPopup(); });
    }

    public void ShowLoadingPopup()
    {
        UIManager uiManager = Manager<UIManager>.Get();
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
            uiManager.PushPopup("LoadingPopup");
            if (uiManager.FrontPopup != null)
                uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
        }
    }

    public void HideLoadingPopup()
    {
        UIManager uiManager = Manager<UIManager>.Get();
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");
    }

    void OnBackButtonAction()
    {
        UIButton skipButton = transform.FindChild("content/SkipButton").GetComponent<UIButton>();
        skipButton.onReleaseEvent.Invoke(skipButton);
    }
}