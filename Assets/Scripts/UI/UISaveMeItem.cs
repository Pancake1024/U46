using UnityEngine;
using SBS.Core;
using System;

public class UISaveMeItem : MonoBehaviour
{
    public static bool IsFirstTimeInSession = false;

    public GameObject parent;
    public GameObject opponentPopup;
    public SpriteRenderer fbIconRenderer;
    public UITextField opponentNameText;
    public UITextField opponentScoreText;
    public UITextField metersMoreText;
    public UITextField beatText;
    public UIButton saveMeButton;
    public UIButton skipButton;
    public UIButton skipButtonWeb;
    public UITextField tfSecs;
    public UITextField tfPrice;
    public UITextField tfKeepRunning;
    public GameObject coinsIcon;
    public GameObject diamondIcon;
    public UIButton backButton;
    public GameObject videoNode;
    public GameObject coinsIconVideo;
    public GameObject diamondIconVideo;
    public UITextField tfPriceVideo;

    public GameObject turbo;
    public GameObject crono;
    public GameObject hand;

    public UITextField freeSaveMeVideoText;

    protected UIRoot uiRoot;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected PriceData.CurrencyType currentCurrency = PriceData.CurrencyType.FirstCurrency;
    int price;
    int seconds;
    UISaveMePopup.SaveMeButtonType buttonType;
    protected int saveMeCounter;

    protected Transform tfSkipWeb;

    void Awake()
    {
        //saveMeButton = gameObject.transform.FindChild("SaveMeButton").GetComponent<UIButton>();
        //tfSecs = gameObject.transform.FindChild("tfSecs").GetComponent<UITextField>();
        //coinsIcon = gameObject.transform.FindChild("SaveMeButton/coin").gameObject;
        //diamondIcon = gameObject.transform.FindChild("SaveMeButton/diamond").gameObject;
        
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        tfSkipWeb = transform.FindChild("tfSkipWeb");
        tfSkipWeb.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_skip_saveme");
        tfSkipWeb.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(true);
        skipButtonWeb.gameObject.SetActive(false);

        tfPrice.onResize.AddTarget(gameObject, "OnResizeDone");

#if UNITY_WEBPLAYER
        skipButton.gameObject.SetActive(false);
        skipButtonWeb.gameObject.SetActive(false); //true);
        tfSkipWeb.gameObject.SetActive(true);
#endif
    }

    //public float ccc = 0.38f;
    void OnResizeDone(UITextField tf)
    {
        float xPos = saveMeButton.GetComponent<BoxCollider2D>().size.x * 0.45f; // ccc;
        coinsIcon.transform.position = new Vector3(xPos, coinsIcon.transform.position.y, coinsIcon.transform.position.z);
        diamondIcon.transform.position = new Vector3(xPos, diamondIcon.transform.position.y, diamondIcon.transform.position.z);
    }

    void OnEnable()
    {
        uiRoot = Manager<UIRoot>.Get();
        tfKeepRunning.text = OnTheRunDataLoader.Instance.GetLocaleString("keep_running");
        saveMeCounter = gameplayManager.SaveMeCounter + 1;

        bool isFreeSaveMeVideoAvailable = false;
#if !UNITY_WEBPLAYER
        if (OnTheRunCoinsService.Instance != null)
        {
            isFreeSaveMeVideoAvailable = IsFirstTimeInSession && OnTheRunCoinsService.Instance.IsFreeSaveMeVideoAvailable();
            if (isFreeSaveMeVideoAvailable)
            {
                freeSaveMeVideoText.text = OnTheRunDataLoader.Instance.GetLocaleString("free_video");
            }
        }
#endif
        videoNode.SetActive(isFreeSaveMeVideoAvailable);
        saveMeButton.gameObject.SetActive(!isFreeSaveMeVideoAvailable);
    }

    public void SetItemValues(int _price, UISaveMePopup.SaveMeButtonType _type, int _seconds, PriceData.CurrencyType currency)
    {
        buttonType = _type;
        seconds = _seconds;
        price = _price;

        tfSecs.text = "+" + seconds.ToString() + "''";
        string priceStr = uiRoot.FormatTextNumber(price);
        //while (priceStr.Length < 3)
        //    priceStr = " " + priceStr;
        tfPrice.text = priceStr;
        coinsIcon.SetActive(currency == PriceData.CurrencyType.FirstCurrency);
        diamondIcon.SetActive(currency == PriceData.CurrencyType.SecondCurrency);

        tfPriceVideo.text = tfPrice.text; // uiRoot.FormatTextNumber(price);
        coinsIconVideo.SetActive(currency == PriceData.CurrencyType.FirstCurrency);
        diamondIconVideo.SetActive(currency == PriceData.CurrencyType.SecondCurrency);

        currentCurrency = currency;
    }
    public void SetOpponentValues(bool active)
    {
        if(!active)
            SetOpponentValues(active, null, string.Empty, string.Empty, string.Empty);
    }

    public string GetSaveMeParam()
    {
        if (videoNode.activeInHierarchy)
            return "video";
        else if (currentCurrency == PriceData.CurrencyType.FirstCurrency)
            return "coins";
        else
            return "diamonds";
    }

    public void SetOpponentValues(bool active, Sprite opponentIcon, string opponentName, string opponentScore, string metersToGo)
    {
        opponentPopup.SetActive(active);

        if (active)
        {
            fbIconRenderer.sprite = opponentIcon;
            opponentNameText.text = opponentName;
            opponentScoreText.text = opponentScore + " " + OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
            metersMoreText.text = metersToGo + " " + OnTheRunDataLoader.Instance.GetLocaleString("more_meters");

            //beatText.text = OnTheRunDataLoader.Instance.GetLocaleString("beat_score");
            bool isYourFriend = OnTheRunIngameHiScoreCheck.Instance.IsNextOpponentFacebookFriend;
            bool isYou = OnTheRunIngameHiScoreCheck.Instance.IsNextOpponentMe;
            if (isYourFriend)
                beatText.text = OnTheRunDataLoader.Instance.GetLocaleString("beat_score");
            else if (isYou)
                beatText.text = OnTheRunDataLoader.Instance.GetLocaleString("beat_your_score");
            else
                beatText.text = OnTheRunDataLoader.Instance.GetLocaleString("beat_non_friend_score");
        }
    }

    void Signal_OnBuySaveMe(UIButton button)
    {
        //Debug.Log("BUY SAVE ME ");
        if (parent != null)
            parent.SendMessage("DestroyAnimatedTexts");

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        const bool FORCE_VIDEO_FOR_SAVEME = false;
        if (FORCE_VIDEO_FOR_SAVEME)
            OnTheRunCoinsService.Instance.WatchVideoForCallback((success) => 
            {
                if (success)
                {
                    DoSaveMe(true);
                    OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.SaveMe);
                }

                if (OnTheRunOmniataManager.Instance != null)
                    OnTheRunOmniataManager.Instance.TrackWatchVideoAds(OnTheRunCoinsService.WatchVideoPlacement.SaveMeAdsPlacement);
            });
        else
            DoSaveMe(false);
    }


    void Signal_OnWatchVideoForSaveMe(UIButton button)
    {
        if (OnTheRunCoinsService.Instance.IsVideoAdAvailable())
            TimeManager.Instance.MasterSource.Pause();

        OnTheRunCoinsService.Instance.WatchVideoForCallback((success) =>
        {
            if (TimeManager.Instance.MasterSource.IsPaused)
                TimeManager.Instance.MasterSource.Resume();

            if (success)
            {
                OnTheRunDaysCounterForAdvertising.Instance.OnVideoAdShown(OnTheRunCoinsService.VideoType.SaveMe);
                DoSaveMe(true);
            }
            else
            {
                if (parent != null)
                    parent.SendMessage("ResetSaveMeTimer");
            }
        });
    }

    void DoSaveMe(bool avoidPaying)
    {
        IsFirstTimeInSession = false;

        bool canSaveMe = PlayerPersistentData.Instance.CanAfford(currentCurrency, price);
        if (avoidPaying)
            canSaveMe = true;

        if(canSaveMe)
        {
            gameplayManager.MainCamera.SendMessage("OnFinalCameraEvent");
            if (OnTheRunOmniataManager.Instance != null)
            {
                double timePassed = Math.Round(transform.parent.GetComponent<UISaveMePopup>().TimePassed, 1);
                double timeRemaining = Math.Round(transform.parent.GetComponent<UISaveMePopup>().TimeRemaining, 1);

                string currency = currentCurrency == PriceData.CurrencyType.FirstCurrency ? "coins" : "diamonds";
                if (avoidPaying)
                    currency = "video";
                OnTheRunOmniataManager.Instance.TrackSaveMe(true, currency, saveMeCounter, timePassed, timeRemaining);
            }

            if (!avoidPaying)
            {
                PlayerPersistentData.Instance.BuyItem(currentCurrency, price);
                
                if (OnTheRunOmniataManager.Instance != null)
                    OnTheRunOmniataManager.Instance.TrackVirtualPurchase(OmniataIds.Product_SaveMe + "_" + buttonType, currentCurrency, price.ToString(), OmniataIds.Product_Type_Standard);
            }
            gameplayManager.restartFromSaveMe = true;
            Manager<UIRoot>.Get().ShowHudPage(true);
#if UNITY_WEBPLAYER
            Manager<UIManager>.Get().ActivePage.SendMessage("DectivateTapToContinue");
#endif

            switch (buttonType)
            {
                case UISaveMePopup.SaveMeButtonType.HelpingHand:
                    gameplayManager.SendMessage("OnSaveMeHelpingHandPressed", seconds);
                    break;
                case UISaveMePopup.SaveMeButtonType.ThirdBolt:
                    gameplayManager.SendMessage("OnSaveMeBoltPressed", seconds);
                    break;
                case UISaveMePopup.SaveMeButtonType.Time:
                    gameplayManager.SendMessage("OnSaveMeTimePressed", seconds);
                    break;
            }

            if (!gameplayManager.FirstTimeSaveMeShown)
                transform.parent.GetComponent<UISaveMePopup>().counter++;
            gameplayManager.FirstTimeSaveMeShown = false;

            Manager<UIManager>.Get().PopPopup();

            if (Manager<UIManager>.Get().IsPopupInStack("SaveMePopup"))
            {
                Manager<UIManager>.Get().RemovePopupFromStack("SaveMePopup");
                Manager<UIRoot>.Get().ShowHudPage(true);
            }
        }
        else
        {
#if UNITY_WEBPLAYER
            Manager<UIManager>.Get().PushPopup("SingleButtonPopup");
            Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content").transform.localPosition = Vector3.zero;
            Manager<UIManager>.Get().FrontPopup.gameObject.transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_not_enough_coins");
#else
            if (currentCurrency == PriceData.CurrencyType.FirstCurrency)
            {
                Manager<UIManager>.Get().PushPopup("CurrencyPopup");
                Manager<UIManager>.Get().FrontPopup.SendMessage("Initialize", UICurrencyPopup.ShopPopupTypes.Money);
                OnTheRunUITransitionManager.Instance.OnPageChanged("CurrencyPopup", "SaveMePopup");
            }
            else
            {
                Manager<UIManager>.Get().PushPopup("CurrencyPopup");
                Manager<UIManager>.Get().FrontPopup.SendMessage("Initialize", UICurrencyPopup.ShopPopupTypes.Diamonds);
                OnTheRunUITransitionManager.Instance.OnPageChanged("CurrencyPopup", "SaveMePopup");
            }
#endif
            //Manager<UIManager>.Get().FrontPopup.pausesGame = true;
        }
    }
    /*
    void Update()
    {
        OnResizeDone(tfPrice);
    }*/
}