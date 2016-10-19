using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UISaveMePopup")]
public class UISaveMePopup : MonoBehaviour
{
    public GameObject saveMeItemPrefab;
    public GameObject bestObj;
    public GameObject metersObj;

    public int numItems = 3;
    protected float duration;
    protected float[] costs;

    protected bool elementsHaveBeenAdded = false;
    protected float timer = -1.0f;
    protected Transform timeBar;
    protected List<SaveMeButtonStruct> buttons;
    protected int buttonsNumber = 3;

    protected int[] coinsValues = { 300, 500, 700 };
    protected int[] baseDiamondsValues = { 1, 1, 2 };
    protected int[] secondsValues = { 10, 5, 5 };
    public int counter = 0;

    protected GameObject bestMeters;
    protected GameObject bestMetersLabel;
    protected GameObject yourMeters;
    protected GameObject yourMetersLabel;

    protected UIRoot uiRoot;
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UISharedData uiSharedData;
    protected Vector3 originalPosition;

    protected UIButton skipButton;
    protected float widthScreen;

    protected bool finalCameraActive;

    public enum SaveMeButtonType
    {
        HelpingHand = 0,
        ThirdBolt,
        Time
    }

    public struct SaveMeButtonStruct
    {
        public GameObject button;
        public SaveMeButtonType type;
    }

    public float TimeRemaining
    {
        get { return (timer / duration) * duration; }
    }

    public float TimePassed
    {
        get { return duration - (timer / duration) * duration; }
    }

    public int CurrentSaveMeCost
    {
        get
        {
            int index = counter;
            if (index >= costs.Length)
                return (int)costs[costs.Length - 1];
            else
                return (int)costs[index];
        }
    }

    void OnEnable()
    {
        uiRoot = Manager<UIRoot>.Get();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        uiSharedData = uiRoot.gameObject.GetComponent<UISharedData>();

        if (originalPosition == null)
            originalPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);

        if (!elementsHaveBeenAdded)
            AddPopupElements();
    }

    void Awake()
    {
        finalCameraActive = (int)OnTheRunDataLoader.Instance.GetFinalCameraData()[0] == 1;
#if UNITY_WEBPLAYER
        transform.localPosition = Vector3.zero;
#endif
    }

    protected UISaveMeItem currentItem;
    void AddPopupElements()
    {
        buttons = new List<SaveMeButtonStruct>();
        for (int i = 0; i < buttonsNumber; ++i)
        {
            GameObject currButton = GameObject.Instantiate(saveMeItemPrefab) as GameObject;
            currButton.transform.parent = this.transform;
            currButton.GetComponent<UISaveMeItem>().parent = this.gameObject;
            Vector3 firstPosition = uiRoot.transform.position + new Vector3(0.0f, 0.0f, 0.0f);
            currButton.transform.position = firstPosition;
            timeBar = currButton.transform.FindChild("Time/Bar/bar_orange_bg");
            timeBar.localScale = Vector3.one;
            elementsHaveBeenAdded = true;
            coinsValues[i] = OnTheRunEconomy.Instance.GetSaveMeCost((SaveMeButtonType)i);//(int)OnTheRunDataLoader.Instance.GetSaveMeData((SaveMeButtonType)i)[0];
            baseDiamondsValues[i] = (int)OnTheRunDataLoader.Instance.GetSaveMeData((SaveMeButtonType)i)[1];
            secondsValues[i] = (int)OnTheRunDataLoader.Instance.GetSaveMeData((SaveMeButtonType)i)[2];
            UISaveMeItem currButtonSaveMeComp = currButton.GetComponent<UISaveMeItem>();
            switch ((SaveMeButtonType)i)
            {
                case SaveMeButtonType.HelpingHand:
                    currButtonSaveMeComp.SetItemValues(coinsValues[i], (SaveMeButtonType)i, secondsValues[i], PriceData.CurrencyType.FirstCurrency);
                    currButtonSaveMeComp.crono.SetActive(false);
                    currButtonSaveMeComp.turbo.SetActive(false);
                    currButtonSaveMeComp.tfSecs.gameObject.SetActive(false);
                    break;
                case SaveMeButtonType.ThirdBolt:
                    currButtonSaveMeComp.SetItemValues(coinsValues[i], (SaveMeButtonType)i, secondsValues[i], PriceData.CurrencyType.FirstCurrency);
                    currButtonSaveMeComp.crono.SetActive(false);
                    currButtonSaveMeComp.hand.SetActive(false);
                    currButtonSaveMeComp.tfSecs.gameObject.SetActive(false);
                    break;
                case SaveMeButtonType.Time:
                    currButtonSaveMeComp.SetItemValues(coinsValues[i], (SaveMeButtonType)i, secondsValues[i], PriceData.CurrencyType.FirstCurrency);
                    currButtonSaveMeComp.hand.SetActive(false);
                    currButtonSaveMeComp.turbo.SetActive(false);
				    currButtonSaveMeComp.tfSecs.gameObject.SetActive(true);
                    break;
            }

#if UNITY_WEBPLAYER
            currButtonSaveMeComp.skipButtonWeb.onReleaseEvent.AddTarget(gameObject, "Signal_OnGoToOffgame");
#else
            currButtonSaveMeComp.skipButton.onReleaseEvent.AddTarget(gameObject, "Signal_OnGoToOffgame");
            currButtonSaveMeComp.skipButton.gameObject.SetActive(false);
            currButtonSaveMeComp.backButton.onReleaseEvent.AddTarget(gameObject, "Signal_OnGoToOffgame");
#endif
            

            currButton.SetActive(false);
            SaveMeButtonStruct data = new SaveMeButtonStruct();
            data.button = currButton;
            data.type = (SaveMeButtonType)i;
            buttons.Add(data);
        }
    }

    void Signal_OnGoToOffgame(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        SkipPopup();
        //button.onReleaseEvent.RemoveTarget(gameObject);
    }

    void Signal_OnHide(UIPopup button)
    {
        DestroyAnimatedTexts();
    }

    void Signal_OnShow(UIPopup popup)
    {
        if (finalCameraActive)
        {
            Camera cam = Manager<UIManager>.Get().UICamera;
            float pixelsPerUnit = Manager<UIManager>.Get().pixelsPerUnit;
            float heightScreen = Manager<UIManager>.Get().baseScreenHeight * 0.5f / pixelsPerUnit;
            widthScreen = heightScreen * cam.aspect;

            GameObject playerRef = GameObject.FindGameObjectWithTag("Player");
            Vector3 playerPos = new Vector3(playerRef.transform.position.x, 0.0f, 0.0f);
            float deltaOffset = widthScreen * 0.25f;
            if (playerPos.x < 0.0f)
                gameObject.transform.localPosition = new Vector3(deltaOffset, originalPosition.y, originalPosition.z);
            else
                gameObject.transform.localPosition = new Vector3(-deltaOffset, originalPosition.y, originalPosition.z);
        }

        duration = OnTheRunDataLoader.Instance.GetSaveMeDuration();
        costs = OnTheRunDataLoader.Instance.GetSaveMeCost();

        uiRoot.ShowHudPage(false);
        timer = duration;
        //Transform bar = transform.FindChild("Time");

        for (int i = 0; i < buttonsNumber; ++i)
        {
            buttons[i].button.SetActive(false);
        }

        int selectedItemIndex = 2;
        float playerDistanceFromNextCheckpoint = uiSharedData.CheckpointDistance * OnTheRunUtils.ToOnTheRunMeters;
        if (playerDistanceFromNextCheckpoint < gameplayManager.helpingHandDistance)
            selectedItemIndex = 0;
        else if (gameplayManager.TurboCounter == 2)
            selectedItemIndex = 1;
        
        SaveMeButtonStruct selectedButton = buttons[selectedItemIndex];
        selectedButton.button.SetActive(true);

        timeBar = selectedButton.button.transform.FindChild("Time/Bar/bar_orange_bg"); //saveMeButtonGO.transform.FindChild("Time/Bar/bar_orange_bg");
        timeBar.localScale = Vector3.one;

        currentItem = selectedButton.button.GetComponent<UISaveMeItem>();
        UISaveMeItem currButtonSaveMeComp = selectedButton.button.GetComponent<UISaveMeItem>();
#if UNITY_WEBPLAYER
        /*
        if (gameplayManager.FirstTimeSaveMeShown)
        {
            Debug.LogError("gameplayManager.FirstTimeSaveMeShown"); //todo dani
            counter = 1;
            currButtonSaveMeComp.SetItemValues(coinsValues[selectedItemIndex], selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.FirstCurrency);
        }
        else
        {
            Debug.LogError("gameplayManager.FirstTimeSaveMeShown"); //todo dani
            currButtonSaveMeComp.SetItemValues(coinsValues[selectedItemIndex] + counter * baseDiamondsValues[selectedItemIndex], selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.FirstCurrency);
        }*/
        /*
        if (gameplayManager.FirstTimeSaveMeShown)
        {
            counter = 0;
            currButtonSaveMeComp.SetItemValues(coinsValues[selectedItemIndex], selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.FirstCurrency);
        }
        else
        {*/
            //currButtonSaveMeComp.SetItemValues(baseDiamondsValues[selectedItemIndex] + counter, selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.SecondCurrency);
        if (gameplayManager.FirstTimeSaveMeShown)
            counter = 0;

            currButtonSaveMeComp.SetItemValues(CurrentSaveMeCost, selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.FirstCurrency);
        //}
#else
        if (gameplayManager.FirstTimeSaveMeShown)
        {
            counter = 0;
            currButtonSaveMeComp.SetItemValues(coinsValues[selectedItemIndex], selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.FirstCurrency);
        }
        else
        {
            //currButtonSaveMeComp.SetItemValues(baseDiamondsValues[selectedItemIndex] + counter, selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.SecondCurrency);
            currButtonSaveMeComp.SetItemValues(CurrentSaveMeCost, selectedButton.type, secondsValues[selectedItemIndex], PriceData.CurrencyType.SecondCurrency);
        }
#endif

        if (OnTheRunIngameHiScoreCheck.Instance.IsInGameplay && !OnTheRunIngameHiScoreCheck.Instance.HasReachedTopOfTheLeaderboard)
            currButtonSaveMeComp.SetOpponentValues(true, OnTheRunIngameHiScoreCheck.Instance.NextOpponentsPicture, OnTheRunIngameHiScoreCheck.Instance.NextOpponentsName, uiRoot.FormatTextNumber((int)OnTheRunIngameHiScoreCheck.Instance.NextOpponentsScore), OnTheRunIngameHiScoreCheck.Instance.MetersToBeatNextOpponent.ToString());
        else
            currButtonSaveMeComp.SetOpponentValues(false);

        //OnLaunchBestMetersFlyer();
        //OnLaunchYourMetersFlyer();
    }

    void OnLaunchBestMetersFlyer()
    {
        if (bestMeters != null)
            return;

        bestMeters = GameObject.Instantiate(bestObj) as GameObject;
        bestMeters.transform.position = new Vector3(-4.8f, -1.5f, 0.0f);

        UITextField tfText = bestMeters.transform.FindChild("TextLabel").GetComponentInChildren<UITextField>();
        tfText.text = OnTheRunDataLoader.Instance.GetLocaleString("best_meters") + "  " + uiRoot.FormatTextNumber(PlayerPersistentData.Instance.GetBestMeters());
        tfText.ApplyParameters();

        tfText.gameObject.GetComponent<Animation>().Play();
    }

    void OnLaunchYourMetersFlyer()
    {
        if (yourMeters != null)
            return;

        yourMeters = GameObject.Instantiate(metersObj) as GameObject;
        yourMeters.transform.position = new Vector3(4.8f, -0.8f, 0.0f);

        UITextField tfText = yourMeters.transform.FindChild("TextLabel").GetComponentInChildren<UITextField>();
        tfText.text = OnTheRunDataLoader.Instance.GetLocaleString("your_meters") + "  " + uiRoot.FormatTextNumber(uiSharedData.InterfaceDistance);
        //tfText.size = tfText.size + (int)(tfText.size * 0.35f);
        tfText.ApplyParameters();
        tfText.gameObject.GetComponent<Animation>().Play();
    }

    void Update()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;
        
        timer -= dt;
        float timePerc = Mathf.Clamp01(timer / duration);
        timeBar.localScale = new Vector3(timePerc, 1.0f, 1.0f);
        if (timer < 0)
           SkipPopup();
    }

    void SkipPopup()
    {
#if !UNITY_WEBPLAYER
        if (Mathf.Clamp01(timer / duration) >= 0.25f)
        {
            timer -= duration * 0.25f;
            Vector3 scale = timeBar.localScale - Vector3.right * 0.25f;
            scale.x = Mathf.Clamp01(scale.x);
            timeBar.localScale = scale;
        }
        else
#endif
        {
            OnTheRunOmniataManager.Instance.TrackSaveMe(false, currentItem.GetSaveMeParam(), gameplayManager.SaveMeCounter, duration, 0);
            gameplayManager.GetComponent<OnTheRunFBFriendsSpawner>().DestroyAll();

            gameplayManager.OnChangeCarEvent(Vector3.zero);
            gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Offgame);

            DestroyAnimatedTexts();
            UIManager uiManager = Manager<UIManager>.Get();

            gameplayManager.MainCamera.SendMessage("OnFinalCameraEvent");
            uiManager.RemovePopupFromStack("SaveMePopup");
            uiManager.PopPopup();

            uiRoot.IsMissionPageFromIngameFlag = false;
            uiRoot.ActivateBackground(true);
            OnTheRunSingleRunMissions.Instance.CanActivatePassedMissions = false;
            uiManager.GoToPage("RewardPage");
            OnTheRunUITransitionManager.Instance.OnPageChanged("RewardPage", "");

            if (OnTheRunIngameHiScoreCheck.Instance)
                OnTheRunIngameHiScoreCheck.Instance.OnGameplayFinished();

            timer = 1.0f;
        }
    }

    void DestroyAnimatedTexts()
    {
        if(bestMeters != null)
            Destroy(bestMeters);
        if(yourMeters != null)
            Destroy(yourMeters);
    }

    void OnSpacePressed()
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        SkipPopup();
    }

    void ResetSaveMeTimer()
    {
        timer = duration;
    }

    void OnBackButtonAction()
    {
        Signal_OnGoToOffgame(null);
    }
}