using UnityEngine;
using SBS.Core;
using System;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIFacebookLoggedPopup")]
public class UIFacebookLoggedPopup : MonoBehaviour
{
    public UITextField title;
    public UITextField description;
    public UITextField gettext;
    public UITextField quantity;
    public UITextField skipButtonText;
    public UITextField inviteButtonText;
    public GameObject FBIcon;

    protected UIManager uiManager;
    protected static DateTime lastDayShown;
    protected OnTheRunInterfaceSounds interfaceSounds;
    protected const string kLastDayKeyShown = "ldks_pop";

    protected GameObject loginButton;

    public static bool CanBeShown(bool force = false)
    {
        DateTime sessionStart = OnTheRunSessionsManager.Instance.GetCurrentSessionBegin();
        TimeSpan durationTimeSpan;
        OnTheRunSessionsManager.Instance.GetSessionTimeSpan(out durationTimeSpan);

        OnTheRunDataLoader dataLoader = OnTheRunDataLoader.Instance;
        UIFacebookLoggedPopup.lastDayShown = EncryptedPlayerPrefs.GetUtcDate(kLastDayKeyShown);

        Debug.Log("last day ---> " + UIFacebookLoggedPopup.lastDayShown + " :: " + dataLoader.GetFacebookLoggedPopupData("days"));
        Debug.Log("daysCheck ---> " + (int)(sessionStart - UIFacebookLoggedPopup.lastDayShown).TotalDays);
        Debug.Log("minutes ---> " + durationTimeSpan.TotalMinutes + " :: " + dataLoader.GetFacebookLoggedPopupData("minutes"));
        Debug.Log("friends ---> " + OnTheRunFacebookManager.Instance.FriendsDictionary.Count + " :: " + dataLoader.GetFacebookLoggedPopupData("friends"));

        bool loggedCheck = OnTheRunFacebookManager.Instance.IsLoggedIn;
        bool daysCheck = (int)(sessionStart - UIFacebookLoggedPopup.lastDayShown).TotalDays > dataLoader.GetFacebookLoggedPopupData("days");
        bool minutesCheck = durationTimeSpan.TotalMinutes > dataLoader.GetFacebookLoggedPopupData("minutes");
        bool friendsCheck = OnTheRunFacebookManager.Instance.FriendsDictionary.Count > dataLoader.GetFacebookLoggedPopupData("friends");

        Debug.Log("loggedCheck " + loggedCheck + " daysCheck " + daysCheck + " minutesCheck " + minutesCheck + " friendsCheck " + friendsCheck);

        return force || (loggedCheck && daysCheck && minutesCheck && friendsCheck);
    }

    void OnEnable()
    {
        uiManager = Manager<UIManager>.Get();
        OnTheRunDataLoader dataLoader = OnTheRunDataLoader.Instance;
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        title.text = dataLoader.GetLocaleString("invitefriends_reminder_title");
        description.text = dataLoader.GetLocaleString("invitefriends_reminder_payoff");
        gettext.text = dataLoader.GetLocaleString("facebook_login_popup_get");
        quantity.text = dataLoader.GetInviteDiamondsReward().ToString();
        skipButtonText.text = dataLoader.GetLocaleString("skip");
        inviteButtonText.text = dataLoader.GetLocaleString("invite_button");

        loginButton = inviteButtonText.transform.parent.gameObject;
    }

    void Signal_OnShow(UIPopup button)
    {
        EncryptedPlayerPrefs.SetUtcDate(kLastDayKeyShown, OnTheRunSessionsManager.Instance.GetCurrentSessionBegin());

        if (FBIcon != null)
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

    void Signal_OnInviteRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        GoToFriendsPopup();
        //OnTheRunUITransitionManager.Instance.ClosePopup(GoToFriendsPopup);
    }

    public void GoToFriendsPopup()
    {
        //Manager<UIRoot>.Get().lastPageShown = "StartPage";
        StartCoroutine(GoToNextPage("FBFriendsPopup"));
    }

    IEnumerator GoToNextPage(string nextPage)
    {
        string currentPage = Manager<UIManager>.Get().ActivePageName;
        OnTheRunUITransitionManager.Instance.OnPageExiting(currentPage, nextPage);

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        Manager<UIManager>.Get().PopPopup();
        Manager<UIManager>.Get().PushPopup(nextPage);

        OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);

        Manager<UIManager>.Get().FrontPopup.SendMessage("GoToTab", UIFBFriendsPopup.FBPopupTab.Invite);
    }

    void OnBackButtonAction()
    {
        Signal_OnSkipRelease(null);
    }
}