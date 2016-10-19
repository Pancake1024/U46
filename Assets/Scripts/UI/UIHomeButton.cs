using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIHomeButton")]
public class UIHomeButton : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void Signal_OnHomeRelease(UIButton button)
    {
        //if (OnTheRunGameplay.Instance.GameState == OnTheRunGameplay.GameplayStates.ReadyToRace && !Manager<UIManager>.Get().IsPopupInStack("CurrencyPopup"))
        //    GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().ChangeState(OnTheRunGameplay.GameplayStates.Offgame);

        Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().DisableFloatingStuff();

        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork && Manager<UIManager>.Get().FrontPopup.name!="CurrencyPopup")
            return;

        this.StartCoroutine(this.GoToNextPage());
    }

    IEnumerator GoToNextPage( )
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

        UIManager uiManager = Manager<UIManager>.Get();

        string currentPage = uiManager.ActivePageName,
               nextPage = Manager<UIRoot>.Get().lastPageShown;
        bool wheelPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("WheelPopup");
        bool optionsPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("OptionsPopup");
        bool currencyPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("CurrencyPopup");
        bool facebookFriendsPopupActive = uiManager.FrontPopup != null && uiManager.FrontPopup.name.Equals("FBFriendsPopup");
        bool skipMissionPopupActive = uiManager.IsPopupInStack("SkipMissionPopup");
        
        if (currentPage == "RewardPage")
            nextPage = "RewardPage";

        if (wheelPopupActive)
        {
            currentPage = "WheelPopup";
            if (nextPage == "GaragePage")
                Manager<UIRoot>.Get().lastPageShown = "StartPage";
        }
        else if (facebookFriendsPopupActive)
        {
            if (currentPage == "GaragePage")
            {
                Manager<UIRoot>.Get().lastPageShown = "StartPage";
                nextPage = "GaragePage"; //Manager<UIRoot>.Get().lastPageShown = "StartPage";
            }
            /*else
            {
                nextPage = "GaragePage";
            }*/
            currentPage = "FBFriendsPopup";
        }
        else if (optionsPopupActive)
        {
            currentPage = "OptionsPopup";
            nextPage = Manager<UIManager>.Get().ActivePageName;
        }
        else if (currencyPopupActive)
        {
            UICurrencyPopup.isFromNotEnoughCoins = false;

            currentPage = "CurrencyPopup";
            if (uiManager.IsPopupInStack("WheelPopup"))
                nextPage = "WheelPopup";
            else if (uiManager.IsPopupInStack("SaveMePopup"))
                nextPage = "SaveMePopup";
            else if (uiManager.IsPopupInStack("OptionsPopup"))
                nextPage = "OptionsPopup";
            else if (uiManager.IsPopupInStack("DailyBonusSequencePopup"))
                nextPage = "DailyBonusSequencePopup";
            else if (uiManager.IsPopupInStack("FBFriendsPopup"))
                nextPage = "FBFriendsPopup";
            else
                nextPage = Manager<UIRoot>.Get().lastPageBeforePopup;
        }
        else if (currentPage == "InAppPage")
            nextPage = "GaragePage";
        
        if (currentPage == "GaragePage" && nextPage == "GaragePage")
            nextPage = "StartPage";

        OnTheRunUITransitionManager.Instance.OnPageExiting(currentPage, nextPage);

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        if (OnTheRunGameplay.Instance.GameState == OnTheRunGameplay.GameplayStates.ReadyToRace && !Manager<UIManager>.Get().IsPopupInStack("CurrencyPopup"))
             GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().ChangeState(OnTheRunGameplay.GameplayStates.Offgame);

        if (wheelPopupActive)
        {
            Manager<UIManager>.Get().FrontPopup.SendMessage("OnExitWheelPopup");
            Manager<UIManager>.Get().PopPopup();
            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
        }
        else if (facebookFriendsPopupActive)
        {
            Manager<UIManager>.Get().PopPopup();
            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
        }
        else if (optionsPopupActive)
        {
            //Manager<UIManager>.Get().FrontPopup.SendMessage("OnExitWheelPopup");
            Manager<UIManager>.Get().PopPopup();
            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
        }
        else if (currencyPopupActive)
        {
            Manager<UIManager>.Get().FrontPopup.SendMessage("OnExitCurrencyPopup");
            Manager<UIManager>.Get().PopPopup();
            if (nextPage == "WheelPopup" || nextPage == "SaveMePopup" || nextPage == "OptionsPopup" || nextPage == "FBFriendsPopup")
                Manager<UIManager>.Get().PushPopup(nextPage);
            else if (nextPage == "DailyBonusSequencePopup")
            {
                Manager<UIManager>.Get().RemovePopupFromStack(nextPage);
                OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusSequencePopup");
            }

            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);
        }
        else
        {
            Manager<UIManager>.Get().GoToPage(nextPage);

            OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, currentPage);

            Manager<UIRoot>.Get().lastPageShown = "StartPage";
            //Manager<UIRoot>.Get().lastPageShown = currentPage;
        }

        if (currentPage == "InAppPage")
        {
           GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("ResetEnvironment");
        }

        if (skipMissionPopupActive)
            Manager<UIManager>.Get().PushPopup("SkipMissionPopup");
    }

    void OnBackButtonAction()
    {
        UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;
        if (frontPopup != null && frontPopup.name.Equals("FBFriendsPopup"))
        {
            if(frontPopup.GetComponent<UIFBFriendsPopup>().stillLoading.activeInHierarchy)
                McSocialApiManager.Instance.StopAnyRequest();
        }

        UIButton backButtonComponent = GetComponent<UIButton>();
        backButtonComponent.onReleaseEvent.Invoke(backButtonComponent);

        //Signal_OnHomeRelease(gameObject.GetComponent<UIButton>());
    }
}