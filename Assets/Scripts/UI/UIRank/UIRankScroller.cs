using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System;

[AddComponentMenu("OnTheRun/UI/UIRankScroller")]
public class UIRankScroller : MonoBehaviour
{
    public UIScroller scroller;
    public GameObject baseElementRef;
    public GameObject unlockableElementRef;
    public GameObject friendElementRef;
    public GameObject playerElementRef;
    public GameObject bonusConsumableRef;

    private List<GameObject> scrollerItems;
    private float initXPosition = 0.0f;
    private float deltaXPosition;
    private float offsetXPosition;
    private int baseSortingOrder = 22;
    protected int defaultLevelWindow = 35;
    protected int maxLevel = 35;

    private float startLevel;
    private float endLevel;

    private List<int> alreadyUsedIndex;

    private bool updatePlayerFlag = false;

    [NonSerialized] public float tapTimer = -1.0f;


    #region Unity callbacks
    void Awake()
    {
        scrollerItems = new List<GameObject>();
        maxLevel = OnTheRunDataLoader.Instance.GetMaxPlayerLevel();
    }

    void OnDisable()
    {
        tapTimer = -1.0f;
        updatingPlayer = false;
    }
    #endregion

    #region Initialization functions
    void InitRankScroller(bool isPopup)
    {
        updatePlayerFlag = !isPopup;

        DestroyScroller();

        float aspectScale = (Manager<UIManager>.Get().baseScreenHeight * Manager<UIManager>.Get().UICamera.aspect) / Manager<UIManager>.Get().baseScreenWidth;

        GameObject tmpElement = Instantiate(baseElementRef) as GameObject;
        float elementBaseWidth = tmpElement.transform.FindChild("grid/horizontal_bar").GetComponent<SpriteRenderer>().bounds.size.x * 0.985f;
        //Debug.Log("--> " + elementBaseWidth);
        Destroy(tmpElement);

        startLevel = Mathf.Clamp(Mathf.FloorToInt(OnTheRunRankManager.Instance.CurrentRank), 0, int.MaxValue);
        int totalElementCounter = 0;
        if (maxLevel<0)
        {
            endLevel = startLevel + defaultLevelWindow + 1;
            totalElementCounter = defaultLevelWindow;
        }
        else
        {
            endLevel = maxLevel + 1;
            totalElementCounter = maxLevel - (int)startLevel;
        }

        if (aspectScale < 1.0f)
            deltaXPosition = elementBaseWidth / aspectScale;// 1.185f * (1.0f / scroller.transform.parent.localScale.x);
        else
            deltaXPosition = elementBaseWidth;

        deltaXPosition /= transform.localScale.x;

        offsetXPosition = (startLevel - 1) * deltaXPosition;

        transform.FindChild("Scroller/Mask").GetComponent<SpriteRenderer>().sortingOrder = 0; // baseSortingOrder;

        alreadyUsedIndex = new List<int>();

        scroller.BeginAddElements();

        //cheat
        //AddFakeFriendsListElements();

        AddUnlockableElements(startLevel, endLevel);
        AddBonusConsumables(startLevel, endLevel);
        AddPlayerElement();

        for (int i = 0; i < totalElementCounter; ++i)
        {
            GameObject newElement = Instantiate(baseElementRef) as GameObject;
            float xPos = initXPosition + i * deltaXPosition;
            newElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos, 0.0f, 0.0f));
            int currLevel = (int)(startLevel + i + 1);
            bool starAlreadyPlaced = alreadyUsedIndex.IndexOf(currLevel) >= 0;
            newElement.GetComponent<UIRankElement>().Initialize(currLevel.ToString(), starAlreadyPlaced);
            scroller.AddElement(newElement.transform);
            scrollerItems.Add(newElement);
        }

        GameObject fakeElement = new GameObject();
        scroller.AddElement(fakeElement.transform);
        scroller.EndAddElements();

        Destroy(fakeElement);

        if (OnTheRunFacebookManager.Instance.IsLoggedIn)
            StartCoroutine(WaitForFriendsReady());

        //transform.FindChild("Scroller/RankingScroller/offsetNode").transform.localScale = transform.localScale;
    }

    public void ResetRankScroller( )
    {
        transform.FindChild("Scroller/RankingScroller/offsetNode").transform.localScale = Vector3.one;
    }

    IEnumerator WaitForFriendsReady()
    {
        while (!McSocialApiManager.Instance.friendsPicturesForRankBarReady || !McSocialApiManager.Instance.friendsRequestForRankBarReady)
            yield return new WaitForEndOfFrame();

        scroller.BeginAddElements();
        AddFriendsListElements(startLevel, endLevel);
        scroller.EndAddElements();
    }

    void DestroyScroller()
    {
        if (scrollerItems != null)
        {
            for (int i = 0; i < scrollerItems.Count; i++)
            {
                DestroyImmediate(scrollerItems[i]);
            }
        }

        scrollerItems.Clear();
        scroller.Offset = Vector2.zero;
        scroller.OffsetVelocity = Vector2.zero;
        scroller.UpdateScroller(0.0f, false);
    }

    void AddUnlockableElements(float startLevel, float endLevel)
    {
        List<OnTheRunRankManager.Unlockable> unlockableList = OnTheRunRankManager.Instance.UnlockableItems;
        for (int i = unlockableList.Count-1; i >= 0; --i)
        {
            OnTheRunRankManager.Unlockable currUnlockable = unlockableList[0];
            if (currUnlockable.rankLevel > startLevel && currUnlockable.rankLevel < endLevel)
            {
                float xPos = initXPosition + (currUnlockable.rankLevel - 1) * deltaXPosition - offsetXPosition;
                AddSingleUnlockable(unlockableList[0], xPos);
                unlockableList.RemoveAt(0);
                alreadyUsedIndex.Add(currUnlockable.rankLevel);
            }
            else
                unlockableList.RemoveAt(0);
        }
    }

    void AddFakeFriendsListElements( )
    {
        float lastLev = -1.0f;
        float[] fakeFrinedsLevels = { 1.0f, 1.13f, 1.5f, 1.65f };
        sortingOffset = 0;
        for(int i=0; i<fakeFrinedsLevels.Length; ++i)
        {
            float friendLevel = PlayerPersistentData.Instance.Level + fakeFrinedsLevels[i];
            float xPos = initXPosition + (friendLevel - 1) * deltaXPosition - offsetXPosition;
            bool increaseLayer = false;
            if (lastLev > 0 && friendLevel - lastLev < 0.3f)
                increaseLayer = true;
            lastLev = friendLevel;
            AddFriendsElement(xPos, null, increaseLayer);
        }
    }

    void AddFriendsListElements(float startLevel, float endLevel)
    {
        Dictionary<string, float> friendsDictionary = McSocialApiManager.Instance.LastFriendsExperienceData;
        if (friendsDictionary == null)
            return;

        Sprite friendPicture = OnTheRunMcSocialApiData.Instance.defaultUserPicture;

        List<McSocialApiUtils.FriendData> socialApiFriends = McSocialApiManager.Instance.Friends;
        sortingOffset = 0;
        float lastLev = -1.0f;
        foreach (McSocialApiUtils.FriendData data in socialApiFriends)
        {
            float friendLevel = -1;
            if (friendsDictionary.ContainsKey(data.Id))
                friendLevel = friendsDictionary[data.Id];

            if (data.Type == McSocialApiUtils.LoginType.Facebook)
                friendPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(data.LoginTypeId);

            if (friendLevel > startLevel && friendLevel < endLevel)
            {
                float xPos = initXPosition + (friendLevel - 1) * deltaXPosition - offsetXPosition;
                bool increaseLayer = false;
                if (lastLev > 0 && friendLevel - lastLev < 0.3f)
                    increaseLayer = true;
                lastLev = friendLevel;
                AddFriendsElement(xPos, friendPicture, increaseLayer);
            }
        }
    }

    void AddSingleUnlockable(OnTheRunRankManager.Unlockable unlockableElement, float xPos)
    {
        GameObject newElement = Instantiate(unlockableElementRef) as GameObject;
        newElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos, 0.0f, 0.0f));

        UIRankUnlockable unlockable = newElement.GetComponent<UIRankUnlockable>();
        unlockable.scrollerGo = gameObject;
        unlockable.Initialize(unlockableElement.rankLevel.ToString(), unlockableElement.sprite);

        scroller.AddElement(newElement.transform);
        scrollerItems.Add(newElement);
    }

    int sortingOffset = 1;
    void AddFriendsElement(float xPos, Sprite friendSprite, bool increaseLayer)
    {
        GameObject newElement = Instantiate(friendElementRef) as GameObject;
        newElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos, 0.06f, 0.0f));

        if (increaseLayer)
        {
            SpriteRenderer[] rndrs = newElement.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < rndrs.Length; i++)
                rndrs[i].sortingOrder += sortingOffset * 2;
        }

        UIRankPlayer unlockable = newElement.GetComponent<UIRankPlayer>();
        unlockable.Initialize(friendSprite);
        scroller.AddElement(newElement.transform);
        scrollerItems.Add(newElement);

        if (increaseLayer)
            sortingOffset++;
        else
            sortingOffset = 1;
    }

    Transform playerBar, playerElement;//playerCar, playerText, playerBar;
    float startBarScaleX, xpGained;
    float rankOffset = 5.0f;
    void AddPlayerElement()
    {
        GameObject newElement = Instantiate(playerElementRef) as GameObject;
        float xPos = initXPosition + (OnTheRunRankManager.Instance.CurrentRank - 1) * deltaXPosition - offsetXPosition;

        newElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos, 0.0f, 0.0f));
        //playerCar = newElement.transform.FindChild("Car");
        //playerText = newElement.transform.FindChild("text");
        playerElement = newElement.transform;
        playerBar = newElement.transform.FindChild("Bar");

        //playerBar.localScale = new Vector3(rankOffset + OnTheRunRankManager.Instance.CurrentRank * 1.2f, playerBar.localScale.y, playerBar.localScale.z);
        startBarScaleX = playerBar.localScale.x;

        UIRankPlayer unlockable = newElement.GetComponent<UIRankPlayer>();
        unlockable.Initialize(null, OnTheRunDataLoader.Instance.GetLocaleString("rank_bar_player"));
        scroller.AddElement(newElement.transform);
        scrollerItems.Add(newElement);

        updatingPlayer = true;

        if (updatePlayerFlag)
            unlockable.StartFX();
    }

    public bool IsAnimationPlaying()
    {
        return playerElement.GetComponent<UIRankPlayer>().fxPlaying;
    }

    bool updatingPlayer = false;

	float lastXPos = -99f;

    public void UpdatePlayerElement()
    {
        if (null == playerBar)
            return;

        if (true) //updatingPlayer)
        {
            float perc = Manager<UIRoot>.Get().UIExperienceBar.GetComponent<UILevelBar>().GetLevelBarProgress();
            float currLevelUpdated = updatePlayerFlag ? (PlayerPersistentData.Instance.Level + perc) : OnTheRunRankManager.Instance.CurrentRank;

            float deltaScaled = deltaXPosition;// *transform.localScale.x;
            float xPos = initXPosition + (currLevelUpdated - 1) * deltaScaled - offsetXPosition;

			if( xPos == lastXPos )
			{
				// clear particle efc
				playerElement.GetComponent<UIRankPlayer>().StopFX();
			}
			else
			{
				// clear particle efc
				playerElement.GetComponent<UIRankPlayer>().StartFX();
			}

			lastXPos = xPos;

            playerElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos + scroller.Offset.x, 0.0f, 0.0f));
            float barScale = playerElement.transform.localPosition.x;

            float rewardCoeff = 9.35f;
#if !UNITY_WEBPLAYER
            if((UnityEngine.iOS.Device.generation.ToString()).IndexOf("iPad") > -1)
                rewardCoeff = 8.6f;
#endif
            float scaleCoeff = updatePlayerFlag ? rewardCoeff : 12.0f;
            playerBar.localScale = new Vector3(-barScale * scaleCoeff, playerBar.localScale.y, playerBar.localScale.z);

//            if (updatePlayerFlag && !Manager<UIRoot>.Get().UIExperienceBar.GetComponent<UILevelBar>().startedAnimation)
//                playerElement.SendMessage("StopFX");
            
        }
    }

    void Update()
    { 
        UpdatePlayerElement(); 
    }

    void AddBonusConsumables(float startLevel, float endLevel)
    {
        List<OnTheRunConsumableBonusManager.ConsumableBonus> consumablesList = OnTheRunConsumableBonusManager.Instance.GetConsumableBonusList();
        if (consumablesList == null)
            return;

        for (int i = 0; i < consumablesList.Count; ++i)
        {
            bool avoidConsumableBonus = consumablesList[i].type == OnTheRunConsumableBonusManager.ConsumableType.None || consumablesList[i].type == OnTheRunConsumableBonusManager.ConsumableType.Other || consumablesList[i].type == OnTheRunConsumableBonusManager.ConsumableType.OnlyDiamonds || consumablesList[i].type == OnTheRunConsumableBonusManager.ConsumableType.Car;
            if ( !avoidConsumableBonus && consumablesList[i].level > startLevel && consumablesList[i].level < endLevel)
            {
                float xPos = initXPosition + (consumablesList[i].level - 1) * deltaXPosition - offsetXPosition;
                //Debug.Log("AddBonusConsumables " + consumablesList[i].type);
                AddBonusConsumableElement(xPos, consumablesList[i]);
                alreadyUsedIndex.Add(consumablesList[i].level);
            }
        }
    }

    void AddBonusConsumableElement(float xPos, OnTheRunConsumableBonusManager.ConsumableBonus cons)
    {
        GameObject newElement = Instantiate(bonusConsumableRef) as GameObject;
        newElement.transform.position = scroller.transform.TransformPoint(new Vector3(xPos, 0.0f, 0.0f));

        UIRankBonus bonus = newElement.GetComponent<UIRankBonus>();
        bonus.Initialize(cons);
        scroller.AddElement(newElement.transform);
        scrollerItems.Add(newElement);
    }
    #endregion

    public void HideRank()
    {
        this.Signal_OnTapScrollBar(null);
        updatingPlayer = false;
    }

    Vector2 tapMousePos;
    void Signal_OnTapPress(UIButton button)
    {
        tapMousePos = Manager<UIManager>.Get().UICamera.transform.worldToLocalMatrix * Manager<UIManager>.Get().UICamera.ScreenToWorldPoint(Input.mousePosition);
    }
    
    protected void GoToSecondStep()
    {
        Manager<UIRoot>.Get().ShowRewardBar(true);
        UIManager.Instance.ActivePage.SendMessage("SetRankPanelVisibility");
        UIManager.Instance.ActivePage.BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.NextPageStepEnter);
        Manager<UIRoot>.Get().GetComponentInChildren<UIRewardBar>().BroadcastMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.NextPageStepEnter);
        UIRewardPage.firstStep = false;
        tapTimer = -1.0f;
    }

    void Signal_OnTapScrollBar(UIButton button)
    {
        if (Manager<UIManager>.Get().ActivePageName == "RewardPage")
        {
            GoToSecondStep();
        }
        else
        {
            Vector2 mousePos = Manager<UIManager>.Get().UICamera.transform.worldToLocalMatrix * Manager<UIManager>.Get().UICamera.ScreenToWorldPoint(Input.mousePosition);
            bool insideScroller = scroller.ScrollArea.Contains(mousePos - (Vector2)(Manager<UIManager>.Get().UICamera.transform.worldToLocalMatrix * scroller.transform.position));
            bool isPopup = Manager<UIManager>.Get().FrontPopup != null && Manager<UIManager>.Get().FrontPopup.name == "RankBarPopup";

            if (isPopup && insideScroller && (tapMousePos - mousePos).sqrMagnitude < float.Epsilon)
                OnTheRunUITransitionManager.Instance.ClosePopup();

            //tapTimer = (insideScroller && !isPopup) ? 5.0f : -1.0f;

            if (!insideScroller && Mathf.Abs(scroller.OffsetVelocity.x) < 5.0f)
            {
                if (isPopup)
                    OnTheRunUITransitionManager.Instance.ClosePopup();
                //else if (UIRewardPage.canSkip)
                //    GoToSecondStep();
            }
        }
    }
}