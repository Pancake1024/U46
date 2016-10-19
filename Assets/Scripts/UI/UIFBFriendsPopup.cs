using UnityEngine;
using SBS.Core;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIFBFriendsPopup")]
public class UIFBFriendsPopup : MonoBehaviour
{
    public GameObject friendRowGO;
    public UIScroller inviteScroller;
    public UIScroller friendsScroller;
    public GameObject stillLoading;
    public GameObject loadingIcon;
    public GameObject noScrollerItem;
    protected UITextField stillLoadingText;
    public UINineSlice popupBG;
    public UINineSlice inviteTab;
    public UINineSlice friendsTab;
    public UITextField tfInvite;
    public UITextField tfFriends;
    public GameObject InviteSection;
    public GameObject FriendsSection;
    public Transform TitleGroup;
    public GameObject TitleInvites;
    public GameObject TitleGifts;

    public class FacebookInviteData : MonoBehaviour
    {
        public OnTheRunFacebookManager.InvitableFriend invitableFriend;
    }
    
    public class FuelShareData : MonoBehaviour
    {
        public McSocialApiUtils.FriendData friendData;
    }

    public enum FBPopupTab
    {
        Invite = 0,
        Friends
    }

    protected UIManager uiManager;
    protected FBPopupTab currentActiveTab;
    public UITextField titleInviteText;
    public UITextField subtitleInviteText;
    public UITextField titleGiftText1;
    public UITextField titleGiftText2;

    List<GameObject> rankElements;
    List<GameObject> friendsElements;
    List<Transform> cullingElements;

    protected float zRotation = 0.0f;
    UIButton buttonInvitePressed;
    string friendInvitedId;

    bool stopInvitableFriendsCoroutineFlag = true;

    bool updateScrollerCulling = true; //false;

    #region Unity Callbacks
    void Awake()
    {
        currentActiveTab = FBPopupTab.Invite;
        uiManager = Manager<UIManager>.Get();

        stillLoadingText = stillLoading.transform.FindChild("popupTextField").GetComponent<UITextField>();
        stillLoadingText.text = OnTheRunDataLoader.Instance.GetLocaleString("");//"loading");
    }

    void Update()
    {
        if (stillLoading.activeInHierarchy)
        {
            zRotation += TimeManager.Instance.MasterSource.DeltaTime * 150;
            loadingIcon.transform.eulerAngles = new Vector3(0.0f, 0.0f, zRotation);
        }

        ScrollerCullingUpdate();

        //hack
        //Debug.Log("inviteScroller.Offset.y " + inviteScroller.Offset.y);
        //if (inviteScroller.Offset.y < 6.45f)
        //    inviteScroller.Offset = new Vector2(0.0f, 6.45f);
    }

    void ScrollerCullingUpdate()
    {
        if (cullingElements != null && updateScrollerCulling)
        {
            UIScroller scroller = null;
            if (currentActiveTab == FBPopupTab.Invite)
                scroller = inviteScroller;
            else
                scroller = friendsScroller;

            if (scroller != null)
            {
                var tr = scroller.transform;
                float scrollAreaMin = scroller.ScrollArea.yMin,
                      scrollAreaMax = scroller.ScrollArea.yMax;
                for (int i = 0, c = cullingElements.Count; i < c; ++i)
                {
                    var elem = cullingElements[i];
                    Vector3 v = tr.InverseTransformPoint(elem.position);
                    float elemMin = v.y - 0.5f, elemMax = v.y + 0.5f;
                    if ((elemMin - scrollAreaMax) > 0.0f ||
                        (scrollAreaMin - elemMax) > 0.0f)
                        elem.gameObject.SetActive(false);
                    else
                        elem.gameObject.SetActive(true);
                }
            }
        }
    }
    #endregion

    #region Signals

    void Signal_OnShow(UIPopup popup)
    {
        SetupLoadingItem(true, OnTheRunDataLoader.Instance.GetLocaleString(""));//"loading"));
        SetupNoScrollerItem(false);

        stopInvitableFriendsCoroutineFlag = true;
        DestroyScroller();

        currentActiveTab = FBPopupTab.Invite;
        SwitchTab();

        if (OnTheRunNotificationManager.Instance.PendingNotifications > 0)
            OnTheRunNotificationManager.Instance.ShowGiftReceivedPopup();

    }

    void ShowInvitableFriends_old()
    {
        OnTheRunFacebookManager.Instance.RequestInvitableFriends(invitableFriends =>
        {
            int count = 0;

            if (invitableFriends != null)
                count = invitableFriends.Count;

            InitInviteScroller(count);

            int i = 0;
            for (; i < count; i++)
            {
                setupSingleElementForInvite(rankElements[i], invitableFriends[i]);
            }

            SetupLoadingItem(false);
            if (count > 0)
            {
                SetupNoScrollerItem(false);
                StartCoroutine(RequestInvitableFriendsCoroutine());
            }
            else
                SetupNoScrollerItem(true, OnTheRunDataLoader.Instance.GetLocaleString("no_FB_friends"));

            updateScrollerCulling = true;
        });
    }

    void ShowInvitableFriends()
    {
        OnTheRunFacebookManager.Instance.RequestInvitableFriends(invitableFriends =>
        {
            SetupLoadingItem(false);

            StartCoroutine(InitInviteScrollerCoroutine(invitableFriends));

            if (invitableFriends.Count > 0)
                SetupNoScrollerItem(false);
            else
                SetupNoScrollerItem(true, OnTheRunDataLoader.Instance.GetLocaleString("no_FB_friends"));
        });
    }

    IEnumerator RequestInvitableFriendsCoroutine()
    {
        bool areInvitablePicturesReady = false;
        stopInvitableFriendsCoroutineFlag = false;

        List<bool> pictureAlreadySetFlags = new List<bool>(rankElements.Count);
        for (int i = 0; i < rankElements.Count; i++)
            pictureAlreadySetFlags.Add(false);

        do
        {
            bool allPicturesAvailable = true;

            if (stopInvitableFriendsCoroutineFlag)
                yield break;

            for (int i = 0; i < rankElements.Count; i++)
            {
                if (rankElements[i] == null || pictureAlreadySetFlags[i])
                    continue;

                FacebookInviteData currentData = rankElements[i].GetComponent<FacebookInviteData>();
                if (currentData.invitableFriend.Picture == null)
                {
                    allPicturesAvailable = false;
                    continue;
                }

                SpriteRenderer spriteRenderer = rankElements[i].transform.Find("frame_avatar_facebook/fb_user").GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = currentData.invitableFriend.Picture;

                McSocialApiManager.ScaleAvatarSpriteRenderer(spriteRenderer);
                pictureAlreadySetFlags[i] = true;
            }

            if (!allPicturesAvailable)
            {
                areInvitablePicturesReady = OnTheRunFacebookManager.Instance.AreInvitablePicturesReady;
                yield return new WaitForSeconds(0.5f);
            }
            else
                yield break;
        } while (!areInvitablePicturesReady);
    }

    void ShowFuelShareFriends()
    {
        stopInvitableFriendsCoroutineFlag = true;

        McSocialApiManager.Instance.GetFriends(success =>
        {
            bool noFriends = true;
            if (success)
            {
                List<McSocialApiUtils.FriendData> friends = McSocialApiManager.Instance.Friends;

                if (friends != null)
                {
                    if (friends.Count > 0)
                        noFriends = false;
                    List<string> fbIds = new List<string>();
                    List<string> googleIds = new List<string>();
                    foreach (var friend in friends)
                    {
                        string id = friend.LoginTypeId;

                        if (friend.Type == McSocialApiUtils.LoginType.Facebook)
                            fbIds.Add(id);
                        else if (friend.Type == McSocialApiUtils.LoginType.Google)
                            googleIds.Add(id);
                    }

                    McSocialApiManager.Instance.RequestPictures(fbIds, googleIds, () =>
                    {
                        InitFriendsScroller(friends.Count);

                        for (int i = 0; i < friends.Count; i++)
                            setupSingleElementForFuelShare(friendsElements[i], friends[i]);

                        SetupLoadingItem(false);
                        if (noFriends)
                            SetupNoScrollerItem(true, OnTheRunDataLoader.Instance.GetLocaleString("no_FB_friends_playing"));
                        else
                            SetupNoScrollerItem(false);

                        updateScrollerCulling = true;
                    });
                }
            }
            else
                SetupLoadingItem(false);
        });
    }

    void Signal_OnExit(UIPopup popup)
    {
        stopInvitableFriendsCoroutineFlag = true;
    }

    void Signal_OnInviteRelease(UIButton button)//Signal_OnGiftFuelRelease
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        if (currentActiveTab == FBPopupTab.Invite)
        {
            //SetupTitle();
            
            buttonInvitePressed = button;
            FacebookInviteData rowData = button.transform.parent.GetComponent<FacebookInviteData>();

#if UNITY_EDITOR
            if (OnTheRunNotificationManager.Instance.InvitesRemaining > 0 && button.transform.FindChild("icon_gem").gameObject.activeInHierarchy)
            {
                int diamondCount = OnTheRunDataLoader.Instance.GetFacebookDiamondsForInvite();
                Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(diamondCount, button.gameObject, 5.0f, 0f);
                PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, diamondCount);
            }

            List<string> testIds = new List<string>
            {
                "100008464654504",
                "1379057299051874",
                "1378267922463798",
                "100008468193383",
                "1378312189126404",
                "277929219054101"
            };
            List<string> testNames = new List<string>
            {
                "Lisa Amhdfdfedejd Narayananwitz",
                "Donna Amhdbhejgfia Narayananberg",
                "Bob Amhdahjiffcc Shepardwitz",
                "Linda Amhdfhaicchc Baoberg",
                "Margaret Chen",
                "John Bharam"
            };
            /*for (int i = 0; i < testIds.Count; i++)
            {
                string friendId = testIds[i];
                string friendName = testNames[i];
                OnTheRunNotificationManager.Instance.AddToInvitedList(friendId, friendName);
            }*/

            string invitedPlayerName = button.transform.parent.FindChild("player_name").GetComponent<UITextField>().text;
            OnTheRunNotificationManager.Instance.AddToInvitedList(invitedPlayerName, invitedPlayerName); 

            PlayCheckAnimationOnButton(button);
			OnTheRunOmniataManager.Instance.TrackFacebookInvite();
#else
            OnTheRunFacebookManager.Instance.InviteFriend(rowData.invitableFriend, (success, recipientId, recipientName) =>
            {
                if (success)
                {
                    if (OnTheRunNotificationManager.Instance.InvitesRemaining > 0 && button.transform.FindChild("icon_gem").gameObject.activeInHierarchy)
                    {
                        int diamondCount = OnTheRunDataLoader.Instance.GetFacebookDiamondsForInvite();
                        Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(diamondCount, button.gameObject, 5.0f, 0f);
                        PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, diamondCount);
                    }

					OnTheRunOmniataManager.Instance.TrackFacebookInvite();
                    OnTheRunNotificationManager.Instance.AddToInvitedList(recipientId, recipientName);
                    PlayCheckAnimationOnButton(button);
                    SetupTitle();
                }
            });
#endif

        }
        else
        { 
            McSocialApiUtils.FriendData data = button.transform.parent.GetComponent<FuelShareData>().friendData;
            OnTheRunNotificationManager.Instance.SendFuelGift(data.Id);

            PlayCheckAnimationOnButton(button);

            StartCoroutine(UpdateFuelGiftTitle());
        }
    }

    void PlayCheckAnimationOnButton(UIButton button)
    {
        button.State = UIButton.StateType.Disabled;

        GameObject check = button.transform.parent.FindChild("iconCheck").gameObject;
        check.SetActive(button.State == UIButton.StateType.Disabled);
        check.GetComponent<Animation>().Play();
        button.gameObject.SetActive(!check.activeInHierarchy);
    }

    /*void LogCallback(bool success)
    {
        if (success)
        {
            buttonInvitePressed.State = UIButton.StateType.Disabled;
            OnTheRunNotificationManager.Instance.AddToInvitedList(friendInvitedId);
        }
    }*/

    void GoToTab(FBPopupTab type)
    {
        currentActiveTab = type;
        SwitchTab();
    }

    void Signal_OnTabInviteRelease(UIButton button)
    {
        if (currentActiveTab != FBPopupTab.Invite)
        {
            OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
            currentActiveTab = FBPopupTab.Invite;
            SwitchTab();
        }
    }

    void Signal_OnTabFriendsRelease(UIButton button)
    {
        if (currentActiveTab != FBPopupTab.Friends)
        {
            OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
            currentActiveTab = FBPopupTab.Friends;
            SwitchTab();
        }
    }
    #endregion

    #region Functions    
    void SetupLoadingItem(bool visible, string text="")
    {
        stillLoading.SetActive(visible);
    }

    void SetupNoScrollerItem(bool visible, string text="")
    {
        noScrollerItem.SetActive(visible);
        if (noScrollerItem.activeInHierarchy)
            noScrollerItem.GetComponent<UITextField>().text = text;
    }

    void SwitchTab()
    {
        updateScrollerCulling = false;

        SetupLoadingItem(true, OnTheRunDataLoader.Instance.GetLocaleString(""));//"loading"));
        SetupNoScrollerItem(false);

        StopAllCoroutines();
        DestroyScroller();
        Color backColor = new Color(0.75f, 0.75f, 0.75f, 1.0f);

        InviteSection.SetActive(currentActiveTab == FBPopupTab.Invite);
        FriendsSection.SetActive(currentActiveTab == FBPopupTab.Friends);
        SetupTitle();

        tfFriends.text = OnTheRunDataLoader.Instance.GetLocaleString("friends_tab");
        tfInvite.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_tab");
        tfFriends.color = currentActiveTab == FBPopupTab.Invite ? backColor : Color.black;
        tfInvite.color = currentActiveTab == FBPopupTab.Invite ? Color.black : backColor;
        tfFriends.ApplyParameters();
        tfInvite.ApplyParameters();

        if (currentActiveTab == FBPopupTab.Invite)
        {
            inviteTab.color = Color.white;
            inviteTab.sortingOrder = popupBG.sortingOrder + 1;
            friendsTab.color = backColor;
            friendsTab.sortingOrder = popupBG.sortingOrder - 1;

            ShowInvitableFriends();
        }
        else
        {
            inviteTab.color = backColor;
            inviteTab.sortingOrder = popupBG.sortingOrder - 1;
            friendsTab.color = Color.white;
            friendsTab.sortingOrder = popupBG.sortingOrder + 1;

            ShowFuelShareFriends();
        }

        inviteTab.ApplyParameters();
        friendsTab.ApplyParameters();
    }

    void UpdateSubtitleInvite()
    {
        if (OnTheRunNotificationManager.Instance.InvitesRemaining > 0)
            subtitleInviteText.text = OnTheRunDataLoader.Instance.GetLocaleString("invitefriends_maxremaining") + " " + OnTheRunNotificationManager.Instance.InvitesRemaining;
        else
            subtitleInviteText.text = "";
        titleInviteText.ApplyParameters();
    }

    void SetupTitle()
    {
        //titleText.text = currentActiveTab == FBPopupTab.Invite ? OnTheRunDataLoader.Instance.GetLocaleString("invite_title") : OnTheRunDataLoader.Instance.GetLocaleString("friends_title")+" "+OnTheRunNotificationManager.Instance.GiftRemaining;
        if (currentActiveTab == FBPopupTab.Invite)
        {
            if (OnTheRunNotificationManager.Instance.InvitesRemaining > 0)
                titleInviteText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_title");
            else
                titleInviteText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_5_comeback").ToUpper();

            UpdateSubtitleInvite();

            TitleInvites.SetActive(true);
            TitleGifts.SetActive(false);

        }
        else
        {
            titleGiftText1.text = OnTheRunNotificationManager.Instance.GiftRemaining.ToString();
            titleGiftText2.text = OnTheRunDataLoader.Instance.GetLocaleString("friends_title");

            TitleInvites.SetActive(false);
            TitleGifts.SetActive(true);
        }

        TitleGroup.FindChild("icon_gem").gameObject.SetActive(currentActiveTab == FBPopupTab.Invite && OnTheRunNotificationManager.Instance.InvitesRemaining > 0);
        TitleGroup.FindChild("icon_tank").gameObject.SetActive(currentActiveTab == FBPopupTab.Friends);

        GameObject currentIcon = TitleGroup.FindChild("icon_gem").gameObject;
        if (currentActiveTab == FBPopupTab.Friends)
            currentIcon = TitleGroup.FindChild("icon_tank").gameObject;

        if (currentActiveTab == FBPopupTab.Invite)
        {
            float iconXPos = titleInviteText.transform.position.x + titleInviteText.GetTextBounds().width * 0.5f + currentIcon.GetComponent<SpriteRenderer>().bounds.size.x * 0.7f;
            currentIcon.transform.localPosition = new Vector3(iconXPos, currentIcon.transform.localPosition.y, currentIcon.transform.localPosition.z);
        }
    }
    
    IEnumerator UpdateFuelGiftTitle()
    {
        yield return new WaitForEndOfFrame( );

        titleGiftText1.text = OnTheRunNotificationManager.Instance.GiftRemaining.ToString();

        if (OnTheRunNotificationManager.Instance.GiftRemaining == 0)
        {
            List<McSocialApiUtils.FriendData> friends = McSocialApiManager.Instance.Friends;
            if (friends != null)
            {
                for (int i = 0; i < friends.Count; i++)
                    setupSingleElementForFuelShare(friendsElements[i], friends[i]);
            }
        }
    }
    #endregion

    #region Scroller
    void InitFriendsScroller(int howMany)
    {
        float initYPosition = 0.0f,
              deltaYPosition = -0.822f; //-0.85f;

        friendsElements = new List<GameObject>();
        cullingElements = new List<Transform>();

        friendsScroller.BeginAddElements();
        for (int i = 0; i < howMany; ++i)
        {
            GameObject newElement = Instantiate(friendRowGO) as GameObject;
            float yPos = initYPosition + i * deltaYPosition;
            newElement.transform.position = friendsScroller.transform.TransformPoint(new Vector3(0.0f, yPos, 0.0f));
            friendsScroller.AddElement(newElement.transform, Vector3.one);
            friendsElements.Add(newElement);
            cullingElements.Add(newElement.transform);
        }
        friendsScroller.EndAddElements();
        friendsScroller.Offset = new Vector2(friendsScroller.Offset.x, 1.0f);
        //friendsScroller.snapY = -2;
    }

    void InitInviteScroller(int howMany)
    {
        float initYPosition = 0.0f,
              deltaYPosition = -0.822f; //-0.85f;

        rankElements = new List<GameObject>();
        cullingElements = new List<Transform>();

        inviteScroller.BeginAddElements();
        for (int i = 0; i < howMany; ++i)
        {
            GameObject newElement = Instantiate(friendRowGO) as GameObject;
            float yPos = initYPosition + i * deltaYPosition;
            newElement.transform.position = inviteScroller.transform.TransformPoint(new Vector3(0.0f, yPos, 0.0f));
            inviteScroller.AddElement(newElement.transform, Vector3.one);
            rankElements.Add(newElement);
            cullingElements.Add(newElement.transform);
        }
        inviteScroller.EndAddElements();
        inviteScroller.Offset = new Vector2(inviteScroller.Offset.x, 1.0f);
        //inviteScroller.snapY = -2;
    }

    IEnumerator InitInviteScrollerCoroutine(List<OnTheRunFacebookManager.InvitableFriend> invitableFriends)
    {
        updateScrollerCulling = true;

        int howMany = invitableFriends.Count;

        float initYPosition = 0.0f, // -5.287864f, //0.0f, //
              deltaYPosition = -0.822f; //-0.85f;

        rankElements = new List<GameObject>();
        cullingElements = new List<Transform>();

        GameObject lastAddedElement = null;

        bool addWithAddElement = true;
        inviteScroller.BeginAddElements();
        for (int i = 0; i < howMany; ++i)
        {
            GameObject newElement = Instantiate(friendRowGO) as GameObject;
            float yPos = initYPosition + i * deltaYPosition;
            
            //if (lastAddedElement != null)
            //    yPos = lastAddedElement.transform.localPosition.y + deltaYPosition;

            newElement.transform.position = inviteScroller.transform.TransformPoint(new Vector3(0.0f, yPos, 0.0f));

            if (addWithAddElement)
                inviteScroller.AddElement(newElement.transform, Vector3.one);
            else
                newElement.transform.parent = inviteScroller.transform;

            newElement.transform.localScale = Vector3.one;
            rankElements.Add(newElement);
            cullingElements.Add(newElement.transform);

            setupSingleElementForInvite(newElement, invitableFriends[i]);

            lastAddedElement = newElement;

            if (i % 1 == 0 && i != 0) //50
            {
                if (addWithAddElement)
                {
                    inviteScroller.EndAddElements();
                    addWithAddElement = false;
                }
                yield return new WaitForEndOfFrame(); // WaitForSeconds(0.00025f); // 0.5f // WaitForEndOfFrame();
                //inviteScroller.BeginAddElements();
            }
        }

        if (addWithAddElement)
        {
            inviteScroller.EndAddElements();
        }
        
        if (howMany > 0)
            StartCoroutine(RequestInvitableFriendsCoroutine());

        updateScrollerCulling = true;

        //inviteScroller.TotalArea = new Rect(inviteScroller.TotalArea.x, Mathf.Min(6.45f, inviteScroller.TotalArea.y), inviteScroller.TotalArea.width, inviteScroller.TotalArea.height);
    }

    void setupSingleElementForInvite(GameObject currentRow, OnTheRunFacebookManager.InvitableFriend invitableFriend)
    {
        currentRow.transform.Find("player_name").GetComponent<UITextField>().text = OnTheRunMcSocialApiData.Instance.TrimStringAtMaxChars(invitableFriend.Name, 33);
        UIButton currentRowInviteButton = currentRow.transform.Find("InviteButton").GetComponent<UIButton>();
        currentRowInviteButton.State = UIButton.StateType.Normal;
        currentRowInviteButton.onReleaseEvent.RemoveTarget(gameObject);
        currentRowInviteButton.onReleaseEvent.AddTarget(gameObject, "Signal_OnInviteRelease");
        currentRowInviteButton.inputLayer = 7;

        UITextField buttonText = currentRowInviteButton.transform.FindChild("tfTextfield").GetComponent<UITextField>();
        buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("invite_button");

        bool alreadyInvited = OnTheRunNotificationManager.Instance.IsAlreadyInvited(invitableFriend.Name);
        currentRowInviteButton.transform.FindChild("icon_gem").gameObject.SetActive(!alreadyInvited);
        currentRowInviteButton.transform.FindChild("icon_gem_gh").gameObject.SetActive(alreadyInvited);

        currentRowInviteButton.transform.FindChild("icon_tank").gameObject.SetActive(false);
        currentRowInviteButton.transform.FindChild("icon_tank_gh").gameObject.SetActive(false);

        if (alreadyInvited)
            currentRowInviteButton.State = UIButton.StateType.Disabled;

        bool reInvited = OnTheRunNotificationManager.Instance.IsReInvited(invitableFriend.Name);
        if (reInvited)
        {
            buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("invitefriends_reinvite");
            currentRowInviteButton.transform.FindChild("icon_gem").gameObject.SetActive(false);
            currentRowInviteButton.transform.FindChild("icon_gem_gh").gameObject.SetActive(false);
        }

        bool showGem = !alreadyInvited && OnTheRunNotificationManager.Instance.InvitesRemaining > 0;
        if (!showGem)
        {
            currentRowInviteButton.transform.FindChild("icon_gem").gameObject.SetActive(false);
            currentRowInviteButton.transform.FindChild("icon_gem_gh").gameObject.SetActive(false);
        }

        GameObject check = currentRow.transform.FindChild("iconCheck").gameObject;
        check.SetActive(currentRowInviteButton.State == UIButton.StateType.Disabled);
        currentRowInviteButton.gameObject.SetActive(!check.activeInHierarchy);

        if (check.activeInHierarchy)
        {
            Animation checkAnim = check.GetComponent<Animation>();
            checkAnim["FBCheckAnim"].time = checkAnim["FBCheckAnim"].length;
            checkAnim.Sample();
            checkAnim.Stop();
        }

        currentRow.AddComponent<FacebookInviteData>();
        FacebookInviteData currentData = currentRow.GetComponent<FacebookInviteData>();
        currentData.invitableFriend = invitableFriend;

        SpriteRenderer spriteRenderer = currentRow.transform.Find("frame_avatar_facebook/fb_user").GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = invitableFriend.Picture;

        McSocialApiManager.ScaleAvatarSpriteRenderer(spriteRenderer);
    }

    void setupSingleElementForFuelShare(GameObject currentRow, McSocialApiUtils.FriendData friendData)
    {
        currentRow.transform.Find("player_name").GetComponent<UITextField>().text = OnTheRunMcSocialApiData.Instance.TrimStringAtMaxChars(friendData.Name, 33);
        UIButton currentRowInviteButton = currentRow.transform.Find("InviteButton").GetComponent<UIButton>();
        if (currentRowInviteButton != null)
        {
            currentRowInviteButton.gameObject.SetActive(true);
            currentRowInviteButton.State = UIButton.StateType.Normal;
            currentRowInviteButton.onReleaseEvent.RemoveTarget(gameObject);
            currentRowInviteButton.onReleaseEvent.AddTarget(gameObject, "Signal_OnInviteRelease");
            currentRowInviteButton.inputLayer = 7;

            GameObject check = currentRow.transform.FindChild("iconCheck").gameObject;

            UITextField buttonText = currentRowInviteButton.transform.FindChild("tfTextfield").GetComponent<UITextField>();
            buttonText.text = OnTheRunDataLoader.Instance.GetLocaleString("gift_button");

            bool alreadyGifted = OnTheRunNotificationManager.Instance.GiftAlreadyDone(friendData.Id);
            if (alreadyGifted || OnTheRunNotificationManager.Instance.GiftRemaining == 0)
                currentRowInviteButton.State = UIButton.StateType.Disabled;

            currentRowInviteButton.transform.FindChild("icon_gem").gameObject.SetActive(false);
            currentRowInviteButton.transform.FindChild("icon_gem_gh").gameObject.SetActive(false);

            currentRowInviteButton.transform.FindChild("icon_tank").gameObject.SetActive(currentRowInviteButton.State != UIButton.StateType.Disabled);
            currentRowInviteButton.transform.FindChild("icon_tank_gh").gameObject.SetActive(currentRowInviteButton.State == UIButton.StateType.Disabled);

            check.SetActive(alreadyGifted);
            currentRowInviteButton.gameObject.SetActive(!alreadyGifted);//!check.activeInHierarchy);

            if (alreadyGifted)//check.activeInHierarchy)
            {
                Animation checkAnim = check.GetComponent<Animation>();
                checkAnim["FBCheckAnim"].time = checkAnim["FBCheckAnim"].length;
                checkAnim.Sample();
                checkAnim.Stop();
            }
            currentRow.AddComponent<FuelShareData>();
            FuelShareData currentData = currentRow.GetComponent<FuelShareData>();
            currentData.friendData = friendData;

            SpriteRenderer spriteRenderer = currentRow.transform.Find("frame_avatar_facebook/fb_user").GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = OnTheRunMcSocialApiData.Instance.defaultUserPicture;
            if (friendData.Type == McSocialApiUtils.LoginType.Facebook)
                spriteRenderer.sprite = OnTheRunFacebookManager.Instance.GetUsersPicture(friendData.LoginTypeId);

            McSocialApiManager.ScaleAvatarSpriteRenderer(spriteRenderer);
        }
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

        if (friendsElements != null)
        {
            for (int i = 0; i < friendsElements.Count; i++)
            {
                DestroyImmediate(friendsElements[i]);
                friendsElements[i] = null;
            }
        }

		if (cullingElements != null)
		{
			for (int i = 0; i < cullingElements.Count; i++)
			{
				DestroyImmediate(cullingElements[i]);
				cullingElements[i] = null;
			}
		}

        inviteScroller.Offset = Vector2.zero;
        inviteScroller.OffsetVelocity = Vector2.zero;
        inviteScroller.UpdateScroller(0.0f, false);

        friendsScroller.Offset = Vector2.zero;
        friendsScroller.OffsetVelocity = Vector2.zero;
        friendsScroller.UpdateScroller(0.0f, false);
    }
    #endregion
}