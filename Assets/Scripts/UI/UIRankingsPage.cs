using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIRankingsPage")]
public class UIRankingsPage : MonoBehaviour
{
    public static int RankingsPageWorldId = 0;

    public Transform[] locationIcons;
    //public UIScroller scroller;
    public GameObject scrollerElement;
    public Transform ToggleArea;
    public Transform ToggleData;
    public UIButton friendsButton;
    public UIButton weeklyButton;
    public GameObject[] comingSoonObjects;
    public GameObject loginPanel;
    public GameObject scrollerMask;
    public GameObject FBIcon;
    public GameObject FBIconGhost;
    public GameObject loginButton;
    //Sprite defaultUserPicture;

    protected bool awakeDone = false;
    protected UIRoot uiRoot;
    protected int currentLocation = 0;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected UIGaragePage garagePage;
    protected UIManager uiManager;
    UIButton[] buttonsArea;
    UIButton[] buttonsData;
    UIScroller scroller;
    List<GameObject> rankElements;

    McSocialApiUtils.ScoreType currentScoreType;
    McSocialApiUtils.ScoreFilter currentScoreFilter;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        buttonsArea = ToggleArea.GetComponentsInChildren<UIButton>();
        buttonsData = gameObject.transform.FindChild("TopCenterButtons/ToggleData").GetComponentsInChildren<UIButton>();
        scroller = transform.FindChild("Scroller/RankingScroller").GetComponent<UIScroller>();
        //defaultUserPicture = scrollerElement.transform.Find("frame_avatar_facebook/fb_user").GetComponent<SpriteRenderer>().sprite;

        uiManager = Manager<UIManager>.Get();
        SetScoreTypeAndFilterToDefault();

        UITextField loginButtonText = loginPanel.transform.FindChild("btAccept/tfTextfield").GetComponent<UITextField>();
        loginButtonText.onResize.AddTarget(gameObject, "OnResizeDone");
    }

    void Start()
    {
        uiRoot = Manager<UIRoot>.Get();

        UIToggleButton uiToggle = ToggleArea.GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(uiToggle.firstActive.GetComponent<UIButton>()); 
        ChangeButtonColor(uiToggle.firstActive, Color.yellow);
        uiToggle = gameObject.transform.FindChild("TopCenterButtons/ToggleData").GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(uiToggle.firstActive.GetComponent<UIButton>()); 
        ChangeButtonColor(uiToggle.firstActive, Color.yellow);

        SetScoreTypeAndFilterToDefault();

        transform.FindChild("TopCenterButtons/ToggleArea/CountryButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("country");
        transform.FindChild("TopCenterButtons/ToggleArea/FriendsButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("friends");
        transform.FindChild("TopCenterButtons/ToggleArea/GlobalButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("global");

        transform.FindChild("TopCenterButtons/ToggleData/AllTimeButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("all_time");
        transform.FindChild("TopCenterButtons/ToggleData/MonthlyButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("monthly");
        transform.FindChild("TopCenterButtons/ToggleData/WeeklyButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("weekly");

        awakeDone = true;
    }

    #region Login Panel
    void SetupLoginPanel(int friendsShownCount)
    {
        bool loginPanelWasActive = loginPanel.activeInHierarchy;
        bool showFriends = (currentScoreFilter == McSocialApiUtils.ScoreFilter.Friends);
        if (showFriends)
        {
            loginPanel.SetActive(true);
            UITextField text = loginPanel.transform.FindChild("LabelTextField").GetComponent<UITextField>();
            UIButton button = loginPanel.transform.FindChild("btAccept").GetComponent<UIButton>();
            UITextField buttonText = loginPanel.transform.FindChild("btAccept/tfTextfield").GetComponent<UITextField>();
            button.onReleaseEvent.RemoveAllTargets();

            if (OnTheRunFacebookManager.Instance.IsLoggedIn)
            {
                int playerFBFriendsCount = OnTheRunFacebookManager.Instance.FriendsDictionary.Count;
                if (playerFBFriendsCount < 1)
                {
                    FBIcon.SetActive(false);
                    FBIconGhost.SetActive(false);
                    loginPanel.SetActive(true);
                    if (friendsShownCount <= 1)
                    {
                        text.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_facebook_ranking");
                        buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_button");
                        button.onReleaseEvent.AddTarget(gameObject, "OnInviteRequested");
                    }
                    else
                        loginPanel.SetActive(false);
                }
                else
                    loginPanel.SetActive(false);
            }
            else
            {
                FBIcon.SetActive(true);
                FBIconGhost.SetActive(false);
                loginPanel.SetActive(true);
                text.text = OnTheRunDataLoader.Instance.GetLocaleString("login_facebook_ranking");
                buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("login");
                button.onReleaseEvent.AddTarget(gameObject, "OnLoginRequested");
            }
        }
        else
            loginPanel.SetActive(false);

        if (loginPanel.activeInHierarchy && !loginPanelWasActive)
            loginPanel.SendMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.EnterPage);
    }

    void OnInviteRequested(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        scrollerMask.SetActive(false);
        StartCoroutine(GoToNextPage("FBFriendsPopup", false));
    }

    void OnLoginRequested(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        ShowLoadingPopup();

        OnTheRunFacebookManager.Instance.Login(
            () =>
            {
                OnTheRunMcSocialApiData.Instance.OnFacebookPictureAvailable();
                McSocialApiManager.Instance.LoginWithFacebook(OnTheRunFacebookManager.Instance.Token, SuccessCallback);
            },
            () => { HideLoadingPopup(); },
            () => { HideLoadingPopup(); });
    }

    public void ShowLoadingPopup()
    {
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
            uiManager.PushPopup("LoadingPopup");
            if (uiManager.FrontPopup != null)
                uiManager.FrontPopup.GetComponent<UILoadingPopup>().SetText("");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
        }
    }

    public void HideLoadingPopup()
    {
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");
    }

    void SuccessCallback(bool success)
    {
        HideLoadingPopup();

        if (success)
            RequestScores();
    }

    IEnumerator GoToNextPage(string nextPage, bool fadeOutBg, string prevPage = "RankingsPage")
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            yield break;

        OnTheRunUITransitionManager.Instance.OnPageExiting("RankingsPage", nextPage);

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        uiManager.PushPopup(nextPage);

        OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, prevPage);
        Manager<UIRoot>.Get().lastPageBeforeFriendsPopup = Manager<UIRoot>.Get().lastPageShown;
        Manager<UIRoot>.Get().lastPageShown = prevPage;
    }

    void ResetPageBeforeFriendsPopup()
    {
        if (Manager<UIRoot>.Get().lastPageBeforeFriendsPopup != "")
        {
            Manager<UIRoot>.Get().lastPageShown = Manager<UIRoot>.Get().lastPageBeforeFriendsPopup;
            Manager<UIRoot>.Get().lastPageBeforeFriendsPopup = "";
        }

        scrollerMask.SetActive(true);
    }
    #endregion

    void ChangeButtonColor(UIButton button, Color newColor)
    {
        UITextField[] texts = button.transform.GetComponentsInChildren<UITextField>();
        for (int j = 0; j < texts.Length; ++j)
        {
            UITextField currTxtFld = texts[j];
            if (!currTxtFld.gameObject.name.Contains("stroke"))
            {
                currTxtFld.color = newColor;
                currTxtFld.ApplyParameters();
            }
        }
    }

    void SetScoreTypeAndFilterToDefault()
    {
        currentScoreType = McSocialApiUtils.ScoreType.Weekly;
        currentScoreFilter = McSocialApiUtils.ScoreFilter.Friends;
    }

    #region Scroller
    void InitScroller(int howMany)
    {
        float initYPosition = 0.0f,
              deltaYPosition = -0.822f; //-0.85f;

        rankElements = new List<GameObject>();

        scroller.BeginAddElements();
        for (int i = 0; i < howMany; ++i)
        {
            GameObject newElement = Instantiate(scrollerElement) as GameObject;
            float yPos = initYPosition + i * deltaYPosition;
            newElement.transform.position = scroller.transform.TransformPoint(new Vector3(0.0f, yPos, 0.0f));
            scroller.AddElement(newElement.transform);
            rankElements.Add(newElement);
        }
        scroller.EndAddElements();
        scroller.snapY = -2;

        SetupLoginPanel(howMany);
    }

    void DestroyScroller()
    {
        if (rankElements != null)
        {
            for (int i = 0; i < rankElements.Count; i++)
            {
                DestroyImmediate(rankElements[i]);
                rankElements[i] = null;
            }
        }

        scroller.Offset = Vector2.zero;
        scroller.OffsetVelocity = Vector2.zero;
        scroller.UpdateScroller(0.0f, false);
    }
    #endregion

    #region Signals
    void OnEnable()
    {
        //UIButton weeklyButton = transform.FindChild("TopCenterButtons/ToggleData/WeeklyButton").GetComponentInChildren<UIButton>();
        //UIButton friendsButton = transform.FindChild("TopCenterButtons/ToggleArea/FriendsButton").GetComponentInChildren<UIButton>();
        if (!awakeDone)
            return;


        UIButton scoreTypeButton = null;
        UIButton scoreFilterButton = null;
        switch (currentScoreType)
        {
            case McSocialApiUtils.ScoreType.Weekly: scoreTypeButton = transform.FindChild("TopCenterButtons/ToggleData/WeeklyButton").GetComponent<UIButton>(); break;
            case McSocialApiUtils.ScoreType.Monthly: scoreTypeButton = transform.FindChild("TopCenterButtons/ToggleData/MonthlyButton").GetComponent<UIButton>(); break;
            case McSocialApiUtils.ScoreType.Latest: scoreTypeButton = transform.FindChild("TopCenterButtons/ToggleData/AllTimeButton").GetComponent<UIButton>(); break;
        }
        switch (currentScoreFilter)
        {
            case McSocialApiUtils.ScoreFilter.Friends: scoreFilterButton = transform.FindChild("TopCenterButtons/ToggleArea/FriendsButton").GetComponent<UIButton>(); break;
            case McSocialApiUtils.ScoreFilter.Country: scoreFilterButton = transform.FindChild("TopCenterButtons/ToggleArea/CountryButton").GetComponent<UIButton>(); break;
            case McSocialApiUtils.ScoreFilter.Global: scoreFilterButton = transform.FindChild("TopCenterButtons/ToggleArea/GlobalButton").GetComponent<UIButton>(); break;
        }

        UIToggleButton uiToggle = ToggleArea.GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(scoreFilterButton);
        //ChangeButtonColor(uiToggle.firstActive, Color.yellow);
        uiToggle = gameObject.transform.FindChild("TopCenterButtons/ToggleData").GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(scoreTypeButton);
        //ChangeButtonColor(uiToggle.firstActive, Color.yellow);


        /*
        UIToggleButton uiToggle = ToggleArea.GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(friendsButton);//uiToggle.firstActive.GetComponent<UIButton>());
        ChangeButtonColor(uiToggle.firstActive, Color.yellow);
        uiToggle = gameObject.transform.FindChild("TopCenterButtons/ToggleData").GetComponent<UIToggleButton>();
        uiToggle.SetActiveButton(weeklyButton);
        ChangeButtonColor(uiToggle.firstActive, Color.yellow);
        //this.Signal_OnFriendsRankRelease(friendsButton);
        //this.Signal_OnWeeklyRankRelease(weeklyButton);

        SetScoreTypeAndFilterToDefault();
        */
    }


    void Signal_OnEnter(UIPage page)
    {
        currentLocation = RankingsPageWorldId;

        loginPanel.SetActive(false);

        Manager<UIRoot>.Get().ShowOffgameBG(true);
        Manager<UIRoot>.Get().ShowUpperPageBorders(true);
        Manager<UIRoot>.Get().ShowCommonPageElements(true, true, true, false, false);

        //if (scroller.NumberOfElements <= 0)
        //    InitScroller(10);

        DestroyScroller();
        
        RequestScores();

        ResetIcons();


        if (garagePage == null)
            garagePage = Manager<UIManager>.Get().GetPage("GaragePage").GetComponent<UIGaragePage>();

        for (int i = 0; i < comingSoonObjects.Length; ++i)
            comingSoonObjects[i].SetActive(garagePage.IsComingSoonActive(i) ? true : false);

        transform.FindChild("TopCenterButtons/ToggleArea/CountryButton").GetComponentInChildren<UITextField>().ApplyParameters();
         transform.FindChild("TopCenterButtons/ToggleArea/FriendsButton").GetComponentInChildren<UITextField>().ApplyParameters();
         transform.FindChild("TopCenterButtons/ToggleArea/GlobalButton").GetComponentInChildren<UITextField>().ApplyParameters();
         transform.FindChild("TopCenterButtons/ToggleData/AllTimeButton").GetComponentInChildren<UITextField>().ApplyParameters();
         transform.FindChild("TopCenterButtons/ToggleData/MonthlyButton").GetComponentInChildren<UITextField>().ApplyParameters();
         transform.FindChild("TopCenterButtons/ToggleData/WeeklyButton").GetComponentInChildren<UITextField>().ApplyParameters();
     }

     void Signal_OnExit(UIPage page)
     {
         McSocialApiManager.Instance.StopAnyRequest();

         Manager<UIRoot>.Get().ShowOffgameBG(false);
         Manager<UIRoot>.Get().ShowPageBorders(false);
         Manager<UIRoot>.Get().ShowCommonPageElements(false, false, false, false, false);
     }

     void Signal_OnFriendsRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("FRIENDS RANK: Signal_OnFriendsRankRelease");

         currentScoreFilter = McSocialApiUtils.ScoreFilter.Friends;
         RequestScores();
     }

     void Signal_OnCountryRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("COUNTRY RANK: Signal_OnCountryRankRelease");

         currentScoreFilter = McSocialApiUtils.ScoreFilter.Country;
         RequestScores();
     }

     void Signal_OnGlobalRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("GLOBAL RANK: Signal_OnGlobalRankRelease");

         currentScoreFilter = McSocialApiUtils.ScoreFilter.Global;
         RequestScores();
     }

     void Signal_OnAllTimeRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("ALL TIME RANK: Signal_OnAllTimeRankRelease");

         currentScoreType = McSocialApiUtils.ScoreType.Latest;
         RequestScores();
     }

     void Signal_OnMonthlyRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("MONTHLY RANK: Signal_OnMonthlyRankRelease");

         currentScoreType = McSocialApiUtils.ScoreType.Monthly;
         RequestScores();
     }

     void Signal_OnWeeklyRankRelease(UIButton button)
     {
         interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
         Debug.Log("WEEKLY RANK: Signal_OnWeeklyRankRelease");

         currentScoreType = McSocialApiUtils.ScoreType.Weekly;
         RequestScores();
     }

     void Signal_OnChangeAreaButton(UIButton button)
     {
         for (int i = 0; i < buttonsArea.Length; ++i)
         {
             UIButton currButt = buttonsArea[i];
             if (currButt == button)
             {
                 ChangeButtonColor(currButt, Color.yellow);
             }
             else
             {
                 ChangeButtonColor(currButt, Color.white);
             }
         }
     }

     void Signal_OnChangeDataButton(UIButton button)
     {
         for (int i = 0; i < buttonsData.Length; ++i)
         {
             UIButton currButt = buttonsData[i];
             if (currButt == button)
             {
                 ChangeButtonColor(currButt, Color.yellow);
             }
             else
             {
                 ChangeButtonColor(currButt, Color.white);
             }
         }
     }

     void OnLocationReleased(int locationIndex)
     {
         interfaceSounds.ChangeLocation();
         currentLocation = locationIndex-1;
         ResetIcons();

         RequestScores();
     }
     #endregion

     void ResetIcons()
     {
         for (int i = 0; i < locationIcons.Length; ++i)
         {
             locationIcons[i].localScale = Vector3.one * 0.85f;
             locationIcons[i].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
         }

         locationIcons[currentLocation].localScale = Vector3.one * 1.2f;
         locationIcons[currentLocation].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
     }

     void OnBackButtonAction()
     {
         McSocialApiManager.Instance.StopAnyRequest();

         UIManager uiManager = Manager<UIManager>.Get();
         if (uiManager.IsPopupInStack("LoadingPopup") && uiManager.FrontPopup.name.Equals("LoadingPopup"))
             uiManager.PopPopup();

         DestroyScroller();
     }
    
     void RequestScores()
     {
         /*const bool DONT_USE_API = false;
         if (DONT_USE_API)
         {
             OnRankingsAvailable(new Dictionary<string, int>());
             return;
         }*/

        McSocialApiManager.Instance.StopAnyRequest();

        string country = HttpMcSocialApiImplementation.ALL_COUNTRIES;
        if (currentScoreFilter == McSocialApiUtils.ScoreFilter.Country)
            country = HttpMcSocialApiImplementation.GEOIP_COUNTRY;

        //Debug.Log("----------------------- country: " + country);
        
        bool showFriends = (currentScoreFilter == McSocialApiUtils.ScoreFilter.Friends);

        UIManager uiManager = Manager<UIManager>.Get();
        if (!uiManager.IsPopupInStack("LoadingPopup"))
        {
            uiManager.PushPopup("LoadingPopup");
            uiManager.FrontPopup.SendMessage("SetText", "");//OnTheRunDataLoader.Instance.GetLocaleString("loading"));
            //scroller.yDragSets = UIScroller.DragActions.Locked;
            DestroyScroller();
        }

        long scoresSpread = OnTheRunDataLoader.Instance.GetNumScoresOffgameRanks() - 1; //49;
        //McSocialApiManager.Instance.GetScores(true, showFriends, currentScoreType, spread, OnTheRunMcSocialApiData.Instance.leaderboardId, country);
        McSocialApiManager.Instance.GetScoresForRankingPage(true, showFriends, currentScoreType, scoresSpread, OnTheRunMcSocialApiData.Instance.GetLeaderboardId(currentLocation), country);

    }

    public void OnRankingsAvailable(Dictionary<string, int> levels)
    {
        UIManager uiManager = Manager<UIManager>.Get();
        //if (uiManager.FrontPopup.name.Equals("LoadingPopup"))
        //    uiManager.PopPopup();
        if (uiManager.IsPopupInStack("LoadingPopup"))
            uiManager.RemovePopupFromStack("LoadingPopup");

        //scroller.yDragSets = UIScroller.DragActions.Free;
        //return;

		DestroyScroller();

		List<McSocialApiUtils.ScoreData> scores = null;

		if (McSocialApiManager.Instance.LastRequestedScores != null)
		{
			scores = new List<McSocialApiUtils.ScoreData>(McSocialApiManager.Instance.LastRequestedScores);
            scores.Sort(
                delegate(McSocialApiUtils.ScoreData score1, McSocialApiUtils.ScoreData score2)
                {
                    return score1.Rank.CompareTo(score2.Rank);
                });
        }

        InitScroller(scores != null ? scores.Count : 0);
        SetRankingsData(scores, levels);
    }

    void SetRankingsData(List<McSocialApiUtils.ScoreData> scores, Dictionary<string, int> levels)
    {
        if (scores == null)
            return;

        for (int i = 0; i < rankElements.Count; i++)
        {   
            if (i >= scores.Count)
            {
                rankElements[i].SetActive(false);
                continue;
            }
            
            int level = -1;
            if (levels.ContainsKey(scores[i].Id))
                level = levels[scores[i].Id];

            rankElements[i].SetActive(true);
            SetRankingDataForElement(rankElements[i], scores[i], level);
        }
    }

    void SetRankingDataForElement(GameObject element, McSocialApiUtils.ScoreData score, int level)
    {
        string playerName = OnTheRunMcSocialApiData.Instance.TrimStringAtMaxChars(score.Name, 24);

        string playerLevel = level > 0 ? uiRoot.FormatTextNumber(level) : OnTheRunDataLoader.Instance.GetLocaleString("no_level");
        string bestMeters = uiRoot.FormatTextNumber((int)score.Score);
        //string description = OnTheRunDataLoader.Instance.GetLocaleString("description");
        string rank = uiRoot.FormatTextNumber((int)score.Rank);
        element.transform.Find("meters").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("meters_short");

        element.transform.Find("player_name").GetComponent<UITextField>().text = playerName;
        element.transform.Find("player_level").GetComponent<UITextField>().text = playerLevel;
        element.transform.Find("player_level/stroke").GetComponent<UITextField>().text =  playerLevel;
        element.transform.Find("best_meters").GetComponent<UITextField>().text = bestMeters;
        element.transform.Find("position/best_meters").GetComponent<UITextField>().text = rank;
        //element.transform.Find("description").GetComponent<UITextField>().text = description;

        bool currentElementIsUser = score.Id == McSocialApiManager.Instance.UserLoginData.Id;
        element.transform.Find("backgrounds/bg_you").gameObject.SetActive(currentElementIsUser);
        element.transform.Find("backgrounds/bg_normal").gameObject.SetActive(!currentElementIsUser);

        SpriteRenderer spriteRenderer = element.transform.Find("frame_avatar_facebook/fb_user").GetComponent<SpriteRenderer>();
        Sprite spriteToUSe = spriteRenderer.sprite = OnTheRunMcSocialApiData.Instance.defaultUserPicture; //defaultUserPicture;

        if (score.Id == McSocialApiManager.Instance.UserLoginData.Id && OnTheRunFacebookManager.Instance.IsLoggedIn)
        {
            /*
            Texture2D picture = OnTheRunFacebookManager.Instance.GetUsersPicture(OnTheRunFacebookManager.Instance.UserId);
            if (picture != null)
                spriteToUSe = Sprite.Create(picture, new Rect(0, 0, picture.width, picture.height), new Vector2(0.5f, 0.5f));
            */
            spriteToUSe = OnTheRunMcSocialApiData.Instance.GetPicture();
       }
        else if (score.Id == McSocialApiManager.Instance.UserLoginData.Id && GooglePlusManager.Instance != null && GooglePlusManager.Instance.IsLoggedIn)
        {
            if (GooglePlusManager.Instance.UserPicture != null)
                spriteToUSe = GooglePlusManager.Instance.UserPicture;
        }
        else if (score.LoginType == McSocialApiUtils.LoginType.Facebook)
        {
            //Texture2D picture = OnTheRunFacebookManager.Instance.GetUsersPicture(score.LoginTypeId);
            //spriteToUSe = Sprite.Create(picture, new Rect(0, 0, picture.width, picture.height), new Vector2(0.5f, 0.5f));

            Sprite fbUserPicture = OnTheRunFacebookManager.Instance.GetUsersPicture(score.LoginTypeId);
            if (fbUserPicture != null)
                spriteToUSe = fbUserPicture;
        }
        else if (score.LoginType == McSocialApiUtils.LoginType.Google)
        {
            if (GooglePlusManager.Instance != null && GooglePlusManager.Instance.OtherUsersPicturesByUserId.ContainsKey(score.LoginTypeId))
            {
                if (GooglePlusManager.Instance.OtherUsersPicturesByUserId[score.LoginTypeId] != null)
                    spriteToUSe = GooglePlusManager.Instance.OtherUsersPicturesByUserId[score.LoginTypeId];
            }
        }

        spriteRenderer.sprite = spriteToUSe;
        McSocialApiManager.ScaleAvatarSpriteRenderer(spriteRenderer);
    }

    void OnResizeDone(UITextField tf)
    {
        float xPos = loginButton.transform.position.x - loginButton.GetComponent<BoxCollider2D>().size.x * 0.4f;
        Vector3 newPos = new Vector3(xPos, FBIcon.transform.position.y, FBIcon.transform.position.z);
        FBIcon.transform.position = newPos;
        FBIconGhost.transform.position = newPos;
    }
}