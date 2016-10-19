using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIDailyBonusPopup")]
public class UIDailyBonusPopup : MonoBehaviour
{
    public GameObject itemRef;
    public UITextField title;
    public UITextField subTitle;
    public Sprite[] dailyBonusIcons;
    public Sprite misteryItemIcon;
    public Sprite[] dailyBonusBoosterIcons;
    public List<UIWheelPopup.CarSprite> carIcons;
    public GameObject collectButton;
    
    protected List<Transform> daysObjects;
    protected UIManager uiManager;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected int currentDayIdx = -1;
    protected Dictionary<OnTheRunBooster.BoosterType, Sprite> dailyBonusBoosterIconsDictionary;

    protected float closeDailyBonusPopupTimer = -1.0f;

    //protected List<int> misteryGiftIndexes;
    protected GameObject itemAnimated;
    protected int alphaFade = 0;
    protected float animationDelay = 1.0f;
    protected float alphaAnimationDelay;
    protected float alphaValue = -1.0f;
    protected float alphaSpeed = 1.5f;

    protected OnTheRunDailyBonusManager.DailyBonusData misteryReward;
    //protected List<OnTheRunDailyBonusManager.DailyBonusData> misteryRewardList;


    #region Unity callbacks
    void OnEnable()
    {
        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        //if (misteryRewardList == null)
        //{
        //    misteryRewardList = OnTheRunDataLoader.Instance.GetDailyBonusMisteryReward();
        //}

        dailyBonusBoosterIconsDictionary = new Dictionary<OnTheRunBooster.BoosterType, Sprite>();
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.Turbo, dailyBonusBoosterIcons[0]);
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.Shield, dailyBonusBoosterIcons[1]);
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.SpecialCar, dailyBonusBoosterIcons[2]);
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.DoubleCoins, dailyBonusBoosterIcons[3]);
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.DoubleExp, dailyBonusBoosterIcons[4]);
        dailyBonusBoosterIconsDictionary.Add(OnTheRunBooster.BoosterType.MoreCheckpointTime, dailyBonusBoosterIcons[5]);

        closeDailyBonusPopupTimer = -1.0f;

        //Items initialization
        if (daysObjects == null)
        {
            daysObjects = new List<Transform>();
            daysObjects.Add(itemRef.transform);
            float[] xPositions = { -1.75f, 0.0f, 1.75f, 3.5f, -3.5f, -1.75f, 0.0f, 1.75f, 3.5f };
            float[] yPositions_normal = { 0.815f, 0.815f, 0.815f, 0.815f, 0.815f, 0.815f, 0.815f, 0.815f, 0.815f, 0.815f };
            float[] yPositions_central = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };//{ -1.05f, 0.56f, -1.05f, 0.56f, -1.05f };
            float[] yPositions = yPositions_normal;
            if (OnTheRunDailyBonusManager.MaxDailyBonusDays == 5)
            {
                itemRef.transform.localPosition = new Vector3(-3.5f, 0.0f, 0.0f);
                yPositions = yPositions_central;
            }
            //for (int i = 0; i < xPositions.Length; ++i)
            for (int i = 0; i < OnTheRunDailyBonusManager.MaxDailyBonusDays - 1; ++i)
            {
                float currYPos = yPositions[i];
                if(i>=4)
                    currYPos = -1.3f;
                GameObject curr = Instantiate(itemRef, Vector3.zero, Quaternion.identity) as GameObject;
                curr.transform.parent = gameObject.transform.FindChild("content/Items");
                curr.transform.localPosition = new Vector3(xPositions[i], currYPos, 0.0f);

                if (i == OnTheRunDailyBonusManager.MaxDailyBonusDays - 2)
                {
                    //day 10
                    curr.transform.FindChild("dayten_bg").gameObject.SetActive(true);
                    curr.transform.FindChild("fireworks").gameObject.SetActive(true);
                } 
                else if (i == 3)
                {
                    //day 5
                    curr.transform.FindChild("dayfive_bg").gameObject.SetActive(true);
                    curr.transform.FindChild("normal_bg").gameObject.SetActive(false);
                }
                daysObjects.Add(curr.transform);
            }
        }
    }

    void Start()
    {
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("Backgrounds/bg_black_gradient"));
    }
    #endregion

    #region Signals
    void OnShow(UIPopup popup)
    {
        closeDailyBonusPopupTimer = -1.0f;

        //if (misteryGiftIndexes==null)
        //    misteryGiftIndexes = OnTheRunDataLoader.Instance.GetDailyBonusMisteryDays();

        OnTheRunFireworks.Instance.ClearFireworks();
        
        collectButton.SetActive(false);
        collectButton.SetActive(true);
        collectButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("collect");
        collectButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().ApplyParameters();

        title.text = OnTheRunDataLoader.Instance.GetLocaleString("daily_bonus_title");
        subTitle.text = OnTheRunDataLoader.Instance.GetLocaleString("daily_bonus_descr");

        for (int i = 0; i < daysObjects.Count; ++i)
        {
            UIDailyBonusDay currDay = daysObjects[i].GetComponent<UIDailyBonusDay>();
            currDay.dayTextField.text = OnTheRunDataLoader.Instance.GetLocaleString("day") + " " + (i+1);
            OnTheRunDailyBonusManager.DailyBonusData data = OnTheRunDataLoader.Instance.GetBonusDataById(i);//OnTheRunDailyBonusManager.Instance.GetDailyBonusReward((i+1));
            currDay.prizeTextField.text = Manager<UIRoot>.Get().FormatTextNumber(data.quantity);
            currDay.todayTextField.text = OnTheRunDataLoader.Instance.GetLocaleString("today");

            int dayInARowPassed = OnTheRunDailyBonusManager.Instance.DaysInARow;
            bool dayPassed = dayInARowPassed > (i + 1);
            bool dayToday = dayInARowPassed == (i + 1);
            bool dayToBe = dayInARowPassed < (i + 1);
            if (dayToday)
            {
                currentDayIdx = i;
                currDay.dayTextField.text = "";
            }
            currDay.passedGraphic.gameObject.SetActive(dayPassed);
            currDay.todayGraphic.gameObject.SetActive(dayToday);
            currDay.normalGraphic.gameObject.SetActive(dayToBe || dayPassed);

            /*if (misteryGiftIndexes.Contains(i))
            {
                data.bonusType = OnTheRunDailyBonusManager.DailyBonus.Mistery;
                data.quantity = -1;
                currDay.prizeTextField.text = "";
            }*/
            if (dayPassed)
                currDay.prizeTextField.text = "";

            if (data.bonusType == OnTheRunDailyBonusManager.DailyBonus.Mistery)
                currDay.prizeTextField.text = "";
            else
                misteryReward = null;

            if (dayToday && data.bonusType == OnTheRunDailyBonusManager.DailyBonus.Mistery) //misteryGiftIndexes.Contains(currentDayIdx))
            {
                collectButton.SetActive(false);
                itemAnimated = currDay.icon.gameObject;
                alphaAnimationDelay = animationDelay;
                alphaFade = -1;
                alphaValue = 1.0f;
                currDay.transform.FindChild("fireworks").gameObject.SetActive(true);
            }

            SetDayIcon(currDay.icon.GetComponent<SpriteRenderer>(), data, !dayPassed);

            //text color............
            if (dayToBe)
            {
                currDay.dayTextField.color = Color.white;
                currDay.prizeTextField.color = Color.yellow;
            }

            if (dayToday)
            {
                currDay.dayTextField.color = Color.white;
                currDay.prizeTextField.color = Color.black;
            }

            if (dayPassed)
            {
                currDay.dayTextField.color = Color.yellow;
                currDay.prizeTextField.color = Color.yellow;
            }

            currDay.dayTextField.ApplyParameters();
            currDay.prizeTextField.ApplyParameters();

        }

        if(OnTheRunDailyBonusManager.Instance.DaysInARow==5)
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COME_BACK_MEDIUM);
        else if(OnTheRunDailyBonusManager.Instance.DaysInARow==10)
            OnTheRunAchievements.Instance.AchievementEvent(OnTheRunAchievements.AchievementType.COME_BACK_HARD);

        iTween.ScaleTo(daysObjects[currentDayIdx].gameObject, iTween.Hash("scale", new Vector3(1.1f, 1.1f, 1.1f), "loopType", "pingpong", "time", 0.3f));

        if (!collectButton.activeInHierarchy)
        {
            List<OnTheRunDailyBonusManager.DailyBonusData> misteryRewardList = OnTheRunDataLoader.Instance.GetBonusDataById(currentDayIdx).misteryRewards;
            misteryReward = misteryRewardList[UnityEngine.Random.Range(0, misteryRewardList.Count)];
        }

        OnTheRunDailyBonusManager.Instance.AssignDailyBonusReward(currentDayIdx, misteryReward);
    }

    #region Mistery animation functions
    void FadeOutCompleted()
    {
        SetDayIcon(itemAnimated.GetComponent<SpriteRenderer>(), misteryReward, true);
        itemAnimated.transform.parent.transform.FindChild("tfPrize").GetComponent<UITextField>().text = misteryReward.quantity.ToString();
        alphaFade = 1;
        alphaValue = 0.0f;
    }

    void FadeInCompleted()
    {
        alphaValue = -1.0f;
        alphaFade = 0;
        collectButton.SetActive(true);
        itemAnimated.transform.parent.transform.FindChild("fireworks").gameObject.SetActive(false);
        collectButton.transform.FindChild("tfTextfield").GetComponent<UITextField>().ApplyParameters();
    }

    public void setAlpha(float newAlpha)
    {
        foreach (Material mObj in itemAnimated.GetComponent<SpriteRenderer>().materials)
        {
            mObj.color = new Color(mObj.color.r, mObj.color.g, mObj.color.b, newAlpha);
        }
    }
    #endregion

    void Signal_OnExit(UIPopup popup)
    {
        OnTheRunFireworks.Instance.ClearFireworks();
    }

    void Signal_OnOkRelease(UIButton button)
    {
        if (collectButton.gameObject.activeInHierarchy)
        {
            iTween.Stop();
            daysObjects[currentDayIdx].transform.localScale = Vector3.one;

            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            Manager<UIRoot>.Get().RefreshSpinWheelButton();
            //Manager<UIRoot>.Get().RefreshMissionButton(true);

            OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("content/fireworks"));
            collectButton.gameObject.SetActive(false);
            closeDailyBonusPopupTimer = TimeManager.Instance.MasterSource.TotalTime + 2.0f;

            if (OnTheRunOmniataManager.Instance != null)
                OnTheRunOmniataManager.Instance.TrackDailyBonus(currentDayIdx + 1, OnTheRunDailyBonusManager.Instance.streakType, OnTheRunDailyBonusManager.Instance.streakDays);

            UIGaragePage garagePage = Manager<UIManager>.Get().ActivePage.GetComponent<UIGaragePage>();
            if (garagePage != null)
            {
                garagePage.CheckForLastCarUnlockedTracking(null);
            }
        }
    }

    void Signal_OnDailyItemPressed(UIButton button)
    {
        if (button.gameObject.GetComponent<UIDailyBonusDay>().todayGraphic.gameObject.activeInHierarchy && collectButton.gameObject.activeInHierarchy)
        {
            iTween.Stop();
            daysObjects[currentDayIdx].transform.localScale = Vector3.one;

            interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

            OnTheRunFireworks.Instance.StartFireworksEffect(25, transform.FindChild("content/fireworks"));
            collectButton.gameObject.SetActive(false);
            closeDailyBonusPopupTimer = TimeManager.Instance.MasterSource.TotalTime + 2.0f;
        }
    }
    #endregion

    void SetDayIcon(SpriteRenderer icon, OnTheRunDailyBonusManager.DailyBonusData data, bool active)
    {
        icon.gameObject.SetActive(active);

        if (!active)
            return;

        icon.transform.localScale = Vector3.one;
        switch (data.bonusType)
        { 
            case OnTheRunDailyBonusManager.DailyBonus.Boost:
                //icon.sprite = dailyBonusIcons[5];
                icon.sprite = dailyBonusBoosterIconsDictionary[OnTheRunDailyBonusManager.Instance.CurrentRewardBooster];
                break;
            case OnTheRunDailyBonusManager.DailyBonus.Coin:
                icon.transform.localScale = Vector3.one * 0.7f;
                if (data.quantity <= 100)
                { icon.sprite = dailyBonusIcons[0];}
                else if (data.quantity <= 500)
                { icon.sprite = dailyBonusIcons[3];}
                else if (data.quantity <= 5000)
                { icon.sprite = dailyBonusIcons[8];}
                break;
            case OnTheRunDailyBonusManager.DailyBonus.Diamond: 
                if (data.quantity <= 5)
                {
                    icon.sprite = dailyBonusIcons[4];
                    icon.transform.localScale = Vector3.one * 0.9f;
                }
                else if (data.quantity <= 15)
                {
                    icon.sprite = dailyBonusIcons[9];
                    icon.transform.localScale = Vector3.one * 0.7f;
                }
                else
                {
                    icon.sprite = dailyBonusIcons[9];
                    icon.transform.localScale = Vector3.one * 0.7f;
                }
                break;
            case OnTheRunDailyBonusManager.DailyBonus.ExtraSpin:
                if (data.quantity < 2)
                { icon.sprite = dailyBonusIcons[2]; }
                else 
                { icon.sprite = dailyBonusIcons[7]; }
                break;
            case OnTheRunDailyBonusManager.DailyBonus.Fuel:
                if (data.quantity < 3)
                { icon.sprite = dailyBonusIcons[1]; }
                else
                { icon.sprite = dailyBonusIcons[6]; }
                break;
            case OnTheRunDailyBonusManager.DailyBonus.SpecialCar:
                if (OnTheRunDailyBonusManager.Instance.CurrentRewardCar != OnTheRunGameplay.CarId.None)
                {
                    /*int currIndx = -1;
                    for(int k = 0; k<carIcons.Count; ++k)
                    {
                        if(carIcons[k].type == OnTheRunDailyBonusManager.Instance.CurrentRewardCar)
                        {
                            currIndx = k;
                            break;
                        }
                    }
                    icon.sprite = carIcons[currIndx].icon;*/
                    icon.sprite = GetCarSprite();
                }
                else
                {
                    icon.sprite = dailyBonusIcons[9];
                    icon.transform.localScale = Vector3.one * 0.7f;
                }
                break;
            case OnTheRunDailyBonusManager.DailyBonus.Mistery:
                icon.sprite = misteryItemIcon;
                break;
        }
    }

    public Sprite GetCarSprite()
    {
        int currIndx = -1;
        for (int k = 0; k < carIcons.Count; ++k)
        {
            if (carIcons[k].type == OnTheRunDailyBonusManager.Instance.CurrentRewardCar)
            {
                currIndx = k;
                break;
            }
        }

        Sprite returnValue = dailyBonusIcons[9];
        if (currIndx>=0)
            returnValue = carIcons[currIndx].icon;
        return returnValue;
    }

    void Update()
    {
        if (alphaFade != 0)
        {
            float dt = TimeManager.Instance.MasterSource.DeltaTime;
            if (alphaAnimationDelay >= 0.0f)
            {
                alphaAnimationDelay -= dt;
            }
            else
            {
                alphaValue += dt * alphaFade * alphaSpeed;
                if (alphaValue <= 0.0f)
                {
                    alphaValue = 0.0f;
                    FadeOutCompleted();
                }
                else if (alphaValue >= 1.0f)
                {
                    alphaValue = 1.0f;
                    FadeInCompleted();
                }
                else
                    setAlpha(alphaValue);
            }
        }

        if (closeDailyBonusPopupTimer > 0.0f)
        {
            if (TimeManager.Instance.MasterSource.TotalTime > closeDailyBonusPopupTimer)
            {
                if (uiManager == null)
                    uiManager = Manager<UIManager>.Get();

                if (uiManager.FrontPopup == GetComponent<UIPopup>())
                {
                    OnTheRunUITransitionManager.Instance.ClosePopup();
                    closeDailyBonusPopupTimer = -1.0f;
                }
            }
        }
    }

    void OnBackButtonAction()
    {
        Transform collectButton = transform.FindChild("content/CollectButton");
        if (collectButton != null && collectButton.gameObject.activeInHierarchy)
        {
            UIButton collectButtonComponent = collectButton.GetComponent<UIButton>();
            collectButtonComponent.onReleaseEvent.Invoke(collectButtonComponent);
        }
    }
}