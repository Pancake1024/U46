using UnityEngine;
using SBS.Core;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIRewardBar")]
public class UIRewardBar : MonoBehaviour
{
    public GameObject playButton;
    public GameObject continueButton;
    public GameObject continueButtonWeb;

    public GameObject RewardRankingItem;
    public GameObject RewardWheelItem;


    void Awake()
    {
        continueButton.transform.FindChild("TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btContinue");
        playButton.transform.FindChild("TextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("btPlay");

        continueButtonWeb.SetActive(false);
        continueButtonWeb.SetActive(true);
        continueButtonWeb.SetActive(false);
#if UNITY_WEBPLAYER
        continueButtonWeb.SetActive(true);
        continueButton.SetActive(false);
#endif

    }

    public void ActivateContinueButton(bool activate)
    {
        playButton.SetActive(!activate);

#if UNITY_WEBPLAYER
        continueButtonWeb.SetActive(activate);
        RewardRankingItem.SetActive(false);
        RewardWheelItem.SetActive(false);
#else
        continueButton.SetActive(activate);
#endif
    }

    public void RefreshSpinWheelButton()
    {
        UpdateSpinWheelButton();
        Manager<UIRoot>.Get().SetupWheelButtonBounce(transform.FindChild("BottomLeftAnchor/SpinWheelButton").gameObject);
    }

    public void UpdateSpinWheelButton()
    {
        int remainingSpinds = PlayerPersistentData.Instance.ExtraSpin;
        if (remainingSpinds > 0)
        {
            transform.FindChild("BottomLeftAnchor/SpinWheelButton/Remaining").gameObject.SetActive(true);
            transform.FindChild("BottomLeftAnchor/SpinWheelButton/Remaining/tfRemaining").GetComponent<UITextField>().text = remainingSpinds.ToString();
        }
        else
            transform.FindChild("BottomLeftAnchor/SpinWheelButton/Remaining").gameObject.SetActive(false);
    }

    void Singal_SpinButton(UIButton button)
    {
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        StartCoroutine(GoToNextPage("WheelPopup", false));
    }

    //-------------------------------------------------------//
    void Signal_OnContinueRelease(UIButton button)
    {
    }

    void Signal_OnRankingsRelease(UIButton button)
    {
        Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().DisableFloatingStuff();
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIRoot>.Get().lastPageVisited = Manager<UIManager>.Get().ActivePageName;
        Manager<UIRoot>.Get().lastPageShown = Manager<UIManager>.Get().ActivePageName;
        StartCoroutine(GoToRankingPage());
    }

    void Signal_OnHallRelease(UIButton button)
    {
        Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().DisableFloatingStuff();
        OnTheRunInterfaceSounds.Instance.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        this.StartCoroutine(this.GoToGaragePage());
    }     

    IEnumerator GoToNextPage(string nextPage, bool fadeOutBg)
    {
        if (OnTheRunUITransitionManager.Instance.ButtonsCantWork)
            yield break;

        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", nextPage);

        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        if (nextPage == "WheelPopup")
            Manager<UIManager>.Get().PushPopup(nextPage);
        else
            Manager<UIManager>.Get().GoToPage(nextPage);
        OnTheRunUITransitionManager.Instance.OnPageChanged(nextPage, "RewardPage");
        Manager<UIRoot>.Get().lastPageShown = "StartPage";
    }

    //-------------------------------------------------------//
    IEnumerator GoToGaragePage()
    {
        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", "GaragePage");

        //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        Manager<UIManager>.Get().GoToPage("GaragePage");

        OnTheRunUITransitionManager.Instance.OnPageChanged("GaragePage", "RewardPage");
    }

    IEnumerator GoToRankingPage()
    {
        OnTheRunUITransitionManager.Instance.OnPageExiting("RewardPage", "RankingsPage");

        //yield return new WaitForSeconds(OnTheRunUITransitionManager.changePageDelay);
        while (UIEnterExitAnimations.activeAnimationsCounter > 0)
        {
            yield return null;
        }

        Manager<UIManager>.Get().GoToPage("RankingsPage");

        OnTheRunUITransitionManager.Instance.OnPageChanged("RankingsPage", "RewardPage");
    }

    void Signal_OnPlayRelease(UIButton button)
    {
        UIRoot rootManager = Manager<UIRoot>.Get();
        rootManager.SendMessage("OnMissionPageAdvance");
    }

    void OnSpacePressed()
    {
        Signal_OnPlayRelease(null);
    }
}