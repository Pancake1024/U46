using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class OnTheRunInterstitialsManager : Manager<OnTheRunInterstitialsManager>
{
    #region Singleton instance
    protected static OnTheRunInterstitialsManager instance = null;

    public static OnTheRunInterstitialsManager Instance
    {
        get
        {
            return Manager<OnTheRunInterstitialsManager>.Get();
        }
    }
    #endregion

    [Flags]
    public enum TriggerPoint
    {
        MissionCompleted = (1 << 0),
        BackToMainScreen = (1 << 1),
        LevelUp = (1 << 2)
    }

    const bool LOGS_ARE_ENABLED = false;

    public bool InterstitialCanBePresented
    {
        get
        {
            Log("InterstitialCanBePresented - FLAGS: " + interstitialsAreEnabled + " " + PassedSessionsToWait + " " + (!OnTheRunInAppManager.Instance.UserIsPurchaser) + " " + (!ReachedMaxPerDay) + " " + (!ReachedMaxPerSession));

            return interstitialsAreEnabled &&
                   PassedSessionsToWait &&
                  !OnTheRunInAppManager.Instance.UserIsPurchaser &&
                  !ReachedMaxPerDay &&
                  !ReachedMaxPerSession;
        }
    }

    bool PassedSessionsToWait { get { return OnTheRunSessionsManager.Instance.GetSessionCounter() >= numSessionsToSkip; } } // Android? first session is
    bool ReachedMaxPerDay { get { return OnTheRunDaysCounterForAdvertising.Instance.InterstitialsShownToday >= maxPerDay; } }
    bool ReachedMaxPerSession { get { return interstitialsInSessionCounter >= maxPerSession; } }
    
    bool paramsParsingIsFinished;
    bool interstitialsAreEnabled;
    int numSessionsToSkip;
    int maxPerSession;
    int maxPerDay;

    int interstitialsInSessionCounter;

    string[] triggerPointNames;
    bool isPurchaser;
    TriggerPoint chosenTriggerPoint;
    TriggerPoint enabledTriggerPoints;

    bool isInitialized = false;

    List<string> pagesNoInterstitialPermitted;

    new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Initialize()
    {
        if (isInitialized)
            return;

        Log("Initialize");

        triggerPointNames = Enum.GetNames(typeof(TriggerPoint));
        for (int i = 0, c = triggerPointNames.Length; i < c; ++i)
            triggerPointNames[i] = triggerPointNames[i].ToLower();

        SetupPagesNoInterstitialPermitted();

        paramsParsingIsFinished = false;
        enabledTriggerPoints = 0;

        interstitialsInSessionCounter = 0;

        isInitialized = true;
    }

    void SetupPagesNoInterstitialPermitted()
    {
        pagesNoInterstitialPermitted = new List<string>();
        pagesNoInterstitialPermitted.Add("IngamePage");
        //pagesNoInterstitialPermitted.Add("RewardPage");
        //pagesNoInterstitialPermitted.Add("InAppPage");
    }


    void InterstitialsSessionBegin()
    {
        Log("InterstitialsSessionBegin");

        interstitialsInSessionCounter = 0;
        this.ChooseTriggerPoint();
    }

    void ChooseTriggerPoint()
    {
        Log("ChooseTriggerPoint - begin");

        if (0 == enabledTriggerPoints)
        {
            chosenTriggerPoint = 0;
            return;
        }

        int numTriggerPoints = triggerPointNames.Length;
        int randomIndex = UnityEngine.Random.Range(0, numTriggerPoints), counter = 0;
        while (0 == ((TriggerPoint)(1 << randomIndex) & enabledTriggerPoints) && counter++ < numTriggerPoints)
            randomIndex = (randomIndex + 1) % numTriggerPoints;

        chosenTriggerPoint = (TriggerPoint)(1 << randomIndex);

        Log("ChooseTriggerPoint - chosenTriggerPoint: " + chosenTriggerPoint);

        //OnTheRunMoPubManager.Instance.RequestInterstitial();
    }

    #region Public Configuration Methods

    public void SetConfigParameters(bool isEnabled, int numSessionsToSkip, int maxPerSession, int maxPerDay)
    {
        Initialize();

        Log("SetConfigParameters - isEnabled: " + isEnabled + " - numSessionsToSkip: " + numSessionsToSkip + " - maxPerSession: " + maxPerSession + " - maxPerDay: " + maxPerDay);

        this.interstitialsAreEnabled = isEnabled;
        this.numSessionsToSkip = numSessionsToSkip;
        this.maxPerSession = maxPerSession;
        this.maxPerDay = maxPerDay;
    }

    public void SetTriggerPointEnabled(string triggerPointId, bool isEnabled)
    {
        Log("SetTriggerPointEnabled - triggerPoint: " + triggerPointId + " - isEnabled: " + isEnabled);

        int index = Array.IndexOf<string>(triggerPointNames, triggerPointId.ToLower());
        if (-1 == index)
            return;

        if (isEnabled)
            enabledTriggerPoints |= (TriggerPoint)(1 << index);
        else
            enabledTriggerPoints &= ~(TriggerPoint)(1 << index);
    }

    public void OnConfigParametersParsed()
    {
        Log("OnConfigParametersParsed");
        paramsParsingIsFinished = true;

        this.ChooseTriggerPoint();
    }

    #endregion

    public void TriggerInterstitial(TriggerPoint triggerPoint)
    {
        Log("TriggerInterstitial - requested: " + triggerPoint + " - chosenTriggerPoint: " + chosenTriggerPoint + " - isInitialized: " + isInitialized + " - paramsParsingIsFinished: " + paramsParsingIsFinished + " - InterstitialCanBePresented: " + InterstitialCanBePresented);

        if (!InterstitialCanBePresented || triggerPoint != chosenTriggerPoint || !paramsParsingIsFinished || !isInitialized)
            return;

        Log("TriggerInterstitial - did not skip");

        OnTheRunMoPubManager.Instance.ShowInterstitial();
    }

    public void OnInterstitialDismissed()
    {
        OnTheRunDaysCounterForAdvertising.Instance.OnInterstitialShown();
        interstitialsInSessionCounter++;
        this.ChooseTriggerPoint();

        Log("OnInterstitialDismissed - interstitialsInSessionCounter: " + interstitialsInSessionCounter + " - interstitialsToday: " + OnTheRunDaysCounterForAdvertising.Instance.InterstitialsShownToday);
    }

    public bool DoesCurrentPagePermitInterstitials()
    {
        if (!Manager<UIManager>.Get())
            return false;

        return DoesPagePermitInterstitial(Manager<UIManager>.Get().ActivePageName);
    }

    bool DoesPagePermitInterstitial(string pageName)
    {
        if (pagesNoInterstitialPermitted == null)
            return true;

        bool returnValue = true;
        foreach (var noInterstitialPage in pagesNoInterstitialPermitted)
            if (noInterstitialPage.ToLowerInvariant().Equals(pageName.ToLowerInvariant()))
                returnValue = false;

        Log("DoesPagePermitInterstitial - " + pageName + " - " + returnValue);

        return returnValue;
    }

    void Log(string logStr)
    {
        if (LOGS_ARE_ENABLED)
            Debug.Log("### xxxxxxxxxxxxxxxxxxxxxxxxxxxxx Interstitials Manager - " + logStr);
    }
}