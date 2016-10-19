using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIPlatformsPopup")]
public class UIPlatformsPopup : MonoBehaviour
{
    #region Singleton instance
    protected static UIPlatformsPopup instance = null;

    public static UIPlatformsPopup Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Protected static members
    protected int maxShownPerSession = 1;
    protected static int thresholdIncrement = 5;
    #endregion

    #region Public members
    public string gameVersion = "1.0.0";
    #endregion

    OnTheRunInterfaceSounds interfaceSounds;

    #region Generic Popup stuff
    void OnEnable()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        OnTheRunUITransitionManager.Instance.InitializeBackground(transform.FindChild("background/bg_black_gradient"));
        transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("ok");

        thresholdIncrement = OnTheRunDataLoader.Instance.GetPopupStoreData()[0];
        maxShownPerSession = OnTheRunDataLoader.Instance.GetPopupStoreData()[1];
    }

    void Signal_OnOkButtonRelease(UIButton button)
    {
        Manager<UIRoot>.Get().PauseExperienceBarAnimation(false);
        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        OnTheRunUITransitionManager.Instance.ClosePopup();

        Manager<UIManager>.Get().ActivePage.SendMessage("OnPlatformsPopupClosed");
    }
    #endregion

    #region Protected members
    protected int playsCounter = 0;
    protected int playsThreshold = 1; // shows first reward
    protected int shownCounter = 0;

    protected bool showPopup = false;
    #endregion
    
    #region Protected methods
    protected void SaveData()
    {
        EncryptedPlayerPrefs.SetInt("bip_playsCounter_" + gameVersion, playsCounter);
        EncryptedPlayerPrefs.SetInt("bip_playsThreshold_" + gameVersion, playsThreshold);
        EncryptedPlayerPrefs.SetInt("bip_shownCounter_" + gameVersion, shownCounter);
        EncryptedPlayerPrefs.SetInt("bip_showPopup_" + gameVersion, showPopup ? 1 : 0);
    }
    #endregion

    #region Public methods
    public bool WillShow()
    {
        return (playsCounter + 1) >= playsThreshold && shownCounter < maxShownPerSession;
    }

    public void ShowPopupCheck()
    {
        ++playsCounter;
        if (playsCounter >= playsThreshold)
            showPopup = (shownCounter < maxShownPerSession);
        //this.SaveData();

        Debug.Log("BUYIT data: " + playsCounter + "/" + playsThreshold + ", " + shownCounter + "/" + maxShownPerSession);

        if (showPopup)
            OnShowPopup();
    }
    #endregion

    #region Protected methods
    protected void OnShowPopup()
    {
        OnTheRunUITransitionManager.Instance.OpenPopup("PlatformsPopup");

        Asserts.Assert(showPopup);
        showPopup = false;

        ++shownCounter;
        playsThreshold = playsCounter + thresholdIncrement;// (shownCounter * thresholdIncrement);
        //this.SaveData();
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        Asserts.Assert(null == instance);
        instance = this;

        transform.FindChild("content/popupTextField").GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_mobilepromo_tag");
        transform.FindChild("content/OkButton").GetComponentInChildren<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("skip");

        playsCounter = EncryptedPlayerPrefs.GetInt("bip_playsCounter_" + gameVersion, playsCounter);
        playsThreshold = EncryptedPlayerPrefs.GetInt("bip_playsThreshold_" + gameVersion, playsThreshold);
        shownCounter = EncryptedPlayerPrefs.GetInt("bip_shownCounter_" + gameVersion, shownCounter);

        showPopup = 1 == EncryptedPlayerPrefs.GetInt("bip_showPopup_" + gameVersion, showPopup ? 1 : 0);
    }

    void OnDestroy()
    {
        Asserts.Assert(this == instance);
        instance = null;
    }
    #endregion
}