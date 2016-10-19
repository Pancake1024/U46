using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIOptionsPopup")]
public class UIOptionsPopup : MonoBehaviour
{
    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;

    public UIButton btSounds;
    public UIButton btMusics;
    public UIButton btTutorial;
    public UIScroller creditsScroller;

    //public UITextField tfCopyright;
    public UITextField tfDevelopedBy;
    public UITextField tfProduction;
    public UITextField tfDesign;
    public UITextField tfProgramming;
    public UITextField tf3DArt;
    public UITextField tf2DArt;
    public UITextField tfSound;
    public UITextField tfQa;
    public UITextField tfSpecial;
    public UITextField tfLast;

    GameObject soundsOn, soundsOff;
    GameObject musicsOn, musicsOff;
    GameObject tutorialOn, tutorialOff;
    protected bool initialized = false;


    void FillCreditsScroller()
    {
        tfProduction.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_production");
        tfDesign.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_design");
        tfProgramming.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_programming");
        tf3DArt.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_3dart");
        tf2DArt.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_2dart");
        tfSound.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_sound");
        tfQa.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_qa");
        tfSpecial.text = OnTheRunDataLoader.Instance.GetLocaleString("credits_specialthanks");
        tfLast.text = OnTheRunDataLoader.Instance.GetLocaleString("The rest of Miniclip staff");

        //Transform elem = creditsScroller.transform.parent.FindChild("tfCreditsText");
        //creditsScroller.BeginAddElements();
        //creditsScroller.AddElement(elem);
        //creditsScroller.EndAddElements();

        creditsScroller.OffsetVelocity = Vector3.zero;
        creditsScroller.Offset = new Vector2(0.0f, -19.0f);
        creditsScroller.UpdateScroller(0.0f, false);
    }

    void Start()
    {
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        //btTutorial = transform.FindChild("TutorialObjects/btTutorial").GetComponent<UIButton>();
        tutorialOff = btTutorial.transform.FindChild("icon_check_no").gameObject;
        tutorialOn = btTutorial.transform.FindChild("icon_check_yes").gameObject;
        tutorialOff.SetActive(!OnTheRunTutorialManager.Instance.TutorialActive);
        tutorialOn.SetActive(OnTheRunTutorialManager.Instance.TutorialActive);

        //btSounds = transform.FindChild("SoundButton").GetComponent<UIButton>();
        soundsOff = btSounds.transform.FindChild("icon_sound_off").gameObject;
        soundsOn = btSounds.transform.FindChild("icon_sound_on").gameObject;
        soundsOff.SetActive(!OnTheRunSoundsManager.Instance.SoundsActive);
        soundsOn.SetActive(OnTheRunSoundsManager.Instance.SoundsActive);

        //btMusics = transform.FindChild("btMusics").GetComponent<UIButton>();
        musicsOff = btMusics.transform.FindChild("icon_music_off").gameObject;
        musicsOn = btMusics.transform.FindChild("icon_music_on").gameObject;
        musicsOff.SetActive(!OnTheRunSoundsManager.Instance.MusicActive);
        musicsOn.SetActive(OnTheRunSoundsManager.Instance.MusicActive);

        string versionNumber = iOSUtils.GetAppVersion();
#if UNITY_ANDROID
        versionNumber = AndroidUtils.GetVersionName();
#elif UNITY_WP8
        versionNumber = SBS.Miniclip.WP8Bindings.VersionNumber;
#endif

        transform.FindChild("content/tfVersionNum").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("version") + versionNumber;
        //tfCopyright.text = OnTheRunDataLoader.Instance.GetLocaleString("copyright");
        tfDevelopedBy.text = OnTheRunDataLoader.Instance.GetLocaleString("developed_by");
        //transform.FindChild("content/OptionsObjects/CreditsObjects/tfRightsReserved").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("all_rights_reserved");
        transform.FindChild("content/OptionsObjects/TutorialObjects/tfTutorialLabel").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("tutorial");

        OnTheRunUITransitionManager.Instance.InitializeBackground( transform.FindChild("background/bg_gradient") );

#if UNITY_WEBPLAYER || UNITY_WP8
        transform.FindChild("content/OptionsObjects/infoButton").gameObject.SetActive(false);
        transform.FindChild("content/OptionsObjects/GameCenterButton").gameObject.SetActive(false);
        btSounds.transform.localPosition = new Vector3(2.5f, btSounds.transform.localPosition.y, btSounds.transform.localPosition.z);
        btMusics.transform.localPosition = new Vector3(0.06f, btMusics.transform.localPosition.y, btMusics.transform.localPosition.z);
#endif

/*#if UNITY_ANDROID
        if (transform.FindChild("content/OptionsObjects/infoButton") != null)
            Destroy(transform.FindChild("content/OptionsObjects/infoButton").gameObject);
#endif*/

    }

    void Signal_OnPageEnter(UIPopup popup)
    {
        //Manager<UIRoot>.Get().ShowOffgameBG(true);
        //Manager<UIRoot>.Get().ShowPageBorders(true);
        //Manager<UIRoot>.Get().ShowCommonPageElements(true, true, true, true, false);

        this.Start();

        FillCreditsScroller();
    }

    void Signal_OnPageExit(UIPopup popup)
    {

        creditsScroller.OffsetVelocity = Vector3.zero;
        creditsScroller.Offset = new Vector2(0.0f, -19.0f);
        creditsScroller.UpdateScroller(0.0f, false);
        //Manager<UIRoot>.Get().ShowOffgameBG(false);
        //Manager<UIRoot>.Get().ShowPageBorders(false);
        //Manager<UIRoot>.Get().ShowCommonPageElements(false, false, false, false, false);
    }

    void Signal_OnSoundsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunSoundsManager.Instance.SoundsActive = !OnTheRunSoundsManager.Instance.SoundsActive;
        soundsOff.SetActive(!OnTheRunSoundsManager.Instance.SoundsActive);
        soundsOn.SetActive(OnTheRunSoundsManager.Instance.SoundsActive);
    }

    void Signal_OnMusicsRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunSoundsManager.Instance.MusicActive = !OnTheRunSoundsManager.Instance.MusicActive;
        musicsOff.gameObject.SetActive(!OnTheRunSoundsManager.Instance.MusicActive);
        musicsOn.gameObject.SetActive(OnTheRunSoundsManager.Instance.MusicActive);
    }

    void Signal_OnInfoRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Debug.Log("INFO: Signal_OnInfoRelease");

#if UNITY_IPHONE && !UNITY_EDITOR
		SBS.Miniclip.MCUtilsBindings.ShowCorporateInfo();
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass jc = new AndroidJavaClass("com.miniclip.ontherun.OnTheRunActivity"))
            jc.CallStatic("showHtmlDialog", "http://www.miniclip.com/smartphone-information");
#endif
    }

    void Signal_OnGameCenterRelease(UIButton button)
    {
		// Just for testing
		/*OnTheRunFyberManager.Instance.RequestRewardedVideo ();
		return;*/

        OnTheRunSocial.Instance.ShowAchievements();
    }

    void Signal_OnTutorialRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunTutorialManager.Instance.TutorialActive = !OnTheRunTutorialManager.Instance.TutorialActive;
        tutorialOff.SetActive(!OnTheRunTutorialManager.Instance.TutorialActive);
        tutorialOn.SetActive(OnTheRunTutorialManager.Instance.TutorialActive);
    }

    void Signal_OnCloseRelease(UIButton button)
    {
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();
    }
}