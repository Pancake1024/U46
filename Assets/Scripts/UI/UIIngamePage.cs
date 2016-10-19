using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Globalization;
using System;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIIngamePage")]
public class UIIngamePage : MonoBehaviour
{
    public static bool HORIZONTAL_PROGRESS_BAR = true;

    public GameObject coinsFlyer;
    public GameObject fastCoinsFlyer;
    public GameObject timeSavedFlyer;
    public GameObject timeStoppedFlyer;
    public GameObject liveNewsFlyer;
    public GameObject comboPlayerFlyer;
    public GameObject missionStatus;

    public UINineSlice comboBarFill;
    public UINineSlice comboBarBg;
    public UINineSlice comboBarGrayBg;
    public UITextField scoreMultiplier;
    public UITextField scoreMultiplierStroke;
    public UITextField tfMissionTitle;
    public UITextField tfMissionSubTitle;
    public UITextField tfMissionPartial;
    public UITextField tfMissionTotal;
    public GameObject missionStatusPassed;
    
    public UIButton leftAllScreenButton;
    public UIButton rightAllScreenButton;

    public UITextField fbAdviseText;
    public UITextField fbAdviseText2;

    public GameObject progressBarVertical;
    public GameObject progressBarHorizontal;
    public GameObject TutorialVertical;
    public GameObject TutorialHorizontal;

    protected ParticleSystem progressBarFx;
    protected UIRoot uiRoot;

    protected GameObject currentProgressBar;
    protected GameObject currentTutorialText;

    protected int turboIndicatorsNumber = 3;

    protected float startButtonsAlpha = 1.0f;
    protected float finishButtonAlpha = 0.1f;
    protected float alphaChangingPeriod = 5.0f;
    protected float currentButtonsAlpha = 1.0f;
    protected float startChangeAlphaTime = -1.0f;

    protected UITextField cpDistance;
    protected UITextField meters;
    protected UITextField coins;
	private Animation coinsAnim = null;
    //protected UITextField score;
    protected UITextField time;
    protected UITextField specialCarText;
    protected UITextField specialCarTextKeys;
    protected Transform timeBar;
	private GameObject redTimeBar = null;
	private Animation iconClockAnim = null;
	private Animation timeTextAnim = null;
    protected Transform cpProgressBar;
    protected Transform progressPointer;
    protected Animation multiplierAnim;

    protected OnTheRunInterfaceSounds interfaceSounds;
    protected OnTheRunGameplay gameplayManager;

    protected GameObject player;
    protected PlayerKinematics playerKinematics;

    protected UISharedData uiSharedData;

    protected Transform turbo;
    protected TurboObject[] turboIndicators;
    protected Transform turboBar;
    protected UINineSlice turboBarFill;
    protected Transform turboBarSpark;
    protected bool turboBarActive = false;
    protected float fullBarWidth = 0.0f;
    protected float fullCpBarWidth = 0.0f;
    protected float currenCpBarWidth = 0.0f;

    protected GameObject comboBar;
    protected PlayerDraft playerDraftRef;
    protected float fullComboBarWidth;

    protected GameObject reverseMultiplier;
    protected GameObject timeStopped;
    protected GameObject liveNewsFlyerRef;
    protected GameObject comboPlayerFlyerRef;
    protected SpriteRenderer[] arrowLeft;
    protected SpriteRenderer[] arrowRight;
    protected GameObject tutorialBlinkingGO;
    protected GameObject tapToContinueGO;
    protected GameObject allScreenButton;
    protected GameObject arrowLeftGO;
    protected GameObject arrowRightGO;
    
    protected UIButton pauseButton;
    protected UIButton resumeButton;

    protected float tutorialBLinkingTimer = -1;
    protected bool animateArrowLeft = false;
    protected bool animateArrowRight = false;
    protected int animateLeftArrowDirection = 0;
    protected int animateRightArrowDirection = 0;
    protected float animateLeftArrowTimer = -1;
    protected float animateRightArrowTimer = -1;

    protected Transform[] blinkTarget;
    protected float startBlinkTime = -1.0f;
    protected float blinkScale = 1.0f;
    protected float startScale = 0.9f;
    protected float scaleChange = 0.15f;
    protected float blinkInterval = 0.3f;
    
    protected float fbAdviseTimer = -1.0f;
    protected float fbAdviseDuration = 5.0f;
    protected bool fbAdviseIsEntering = false;
    protected Animation fbAdviseAnim;
	// below this time various feedbacks triggers to warn that
	// time is running out
	private static readonly float TIME_RED_TIMEOUT = 5f;
	private static readonly Vector3 DEFAULT_SCALE = new Vector3(1f,1f,1f);

    protected OnTheRunSingleRunMissions.DriverMission currentMission;

    public struct TurboObject
    {
        public Transform parent;
        public Transform ghost;
        public Transform active;
    }

    public bool IsComboBarActive()
    {
        return comboBar.activeInHierarchy;
    }
        
    public void ActivateTapToContinue(bool value)
    {
#if !UNITY_WEBPLAYER
        ActivateArrows(value, 0);
#endif

        tapToContinueGO.SetActive(value);
    }

    public void DectivateTapToContinue( )
    {
#if !UNITY_WEBPLAYER
        ActivateArrows(false, 0);
#else
        blinkTarget[0].gameObject.SetActive(false);
        blinkTarget[1].gameObject.SetActive(false);
#endif

        tapToContinueGO.SetActive(false);
    }

    public void ActivateArrows(bool value, int side)
    {
        if (side < 0)
        { 
#if UNITY_WEBPLAYER
            blinkTarget[0].gameObject.SetActive(value);
#else
            arrowLeftGO.gameObject.SetActive(value);
#endif
        }
        if (side > 0)
        {
#if UNITY_WEBPLAYER
            blinkTarget[1].gameObject.SetActive(value);
#else
            arrowRightGO.gameObject.SetActive(value);
#endif
        }
        if (side == 0)
        {
#if UNITY_WEBPLAYER
            blinkTarget[0].gameObject.SetActive(value);
            blinkTarget[1].gameObject.SetActive(value);
#else
            allScreenButton.gameObject.SetActive(value);
#endif
        }
    }

    public void ActivateArrowBlink(bool value, int side)
    {
        if (value)
        {
            if (side < 0)
            {
                animateArrowLeft = true;
                animateLeftArrowDirection = 1;
                animateLeftArrowTimer = 0.0f;
            }
            else
            {
                animateArrowRight = true;
                animateRightArrowDirection = 1;
                animateRightArrowTimer = 0.0f;
            }
        }
        else
        {
            animateArrowLeft = false;
            animateArrowRight = false;
            animateLeftArrowTimer = 0.0f;
            animateRightArrowTimer = 0.0f;
        }
    }

    void UpdateTutorialBlinking()
    {
        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            float dt = Manager<UIManager>.Get().UITimeSource.DeltaTime;

            if (Manager<UIManager>.Get().IsPopupInStack("PausePopup"))
                dt = 0.0f;

            tutorialBLinkingTimer -= dt;
            if (tutorialBLinkingTimer < 0.0f)
            {
                tutorialBLinkingTimer = 0.3f;
                tutorialBlinkingGO.SetActive(!tutorialBlinkingGO.activeInHierarchy);
            }
        }
        else if (tutorialBlinkingGO.activeInHierarchy)
            tutorialBlinkingGO.SetActive(false);
    }

    void UpdateArrowsButtonsBlink()
    {
        if (!OnTheRunTutorialManager.Instance.CanBeSkipped && OnTheRunTutorialManager.Instance.TutorialActive)
            return;

        if (!OnTheRunTutorialManager.Instance.TutorialActive)
            return;

        float dt = Manager<UIManager>.Get().UITimeSource.DeltaTime * 0.35f;

        if (Manager<UIManager>.Get().IsPopupInStack("PausePopup"))
            dt = 0.0f;

        float maxSize = 0.1f;
        if (animateArrowLeft)
        {
            if (animateLeftArrowDirection == 1)
            {
                if (animateLeftArrowTimer <= maxSize)
                    animateLeftArrowTimer += dt;
                else
                    animateLeftArrowDirection = -1;
            }
            else if (animateLeftArrowDirection == -1)
            {
                if (animateLeftArrowTimer >= 0.0f)
                    animateLeftArrowTimer -= dt;
                else
                    animateLeftArrowDirection = 1;
            }
            arrowLeft[1].transform.localScale = new Vector3(1.0f + animateLeftArrowTimer, 1.0f + animateLeftArrowTimer, 1.0f);
        }
        
        if (animateArrowRight)
        {
            if (animateRightArrowDirection == 1)
            {
                if (animateRightArrowTimer <= maxSize)
                    animateRightArrowTimer += dt;
                else
                    animateRightArrowDirection = -1;
            }
            else if (animateRightArrowDirection == -1)
            {
                if (animateRightArrowTimer >= 0.0f)
                    animateRightArrowTimer -= dt;
                else
                    animateRightArrowDirection = 1;
            }
            arrowRight[1].transform.localScale = new Vector3(-1.0f - animateRightArrowTimer, 1.0f + animateRightArrowTimer, 1.0f);
        }
    }

    void UpdateArrowsButtonsAlpha()
    {
        if (!gameplayManager.Gameplaystarted)
            return;

        if (startChangeAlphaTime<0)
            startChangeAlphaTime = TimeManager.Instance.MasterSource.TotalTime;

        if(arrowLeft == null || arrowRight == null)
            return;

        if(currentButtonsAlpha <= finishButtonAlpha)
            return;

        float now = TimeManager.Instance.MasterSource.TotalTime;

        currentButtonsAlpha = SBSEasing.EaseNone(now - startChangeAlphaTime, startButtonsAlpha, finishButtonAlpha - startButtonsAlpha, alphaChangingPeriod);

        Color c = arrowLeft[0].color;
        arrowRight[0].color = arrowLeft[0].color = new Color(c.r, c.g, c.b, currentButtonsAlpha);
        
        c = arrowLeft[1].color;
        arrowRight[1].color = arrowLeft[1].color = new Color(c.r, c.g, c.b, currentButtonsAlpha);
    }

    void UpdateBlink( )
    {
        float now = Manager<UIManager>.Get().UITimeSource.TotalTime;

        if (null == blinkTarget[0] || null == blinkTarget[1])
            return;

        if (startBlinkTime < 0.0f)
            return;

        float blinkTime = Mathf.Clamp(now - startBlinkTime, 0.0f, blinkInterval);
        blinkScale = SBSEasing.EaseOutSine(blinkTime, startScale, scaleChange, blinkInterval);

        if (blinkTime >= blinkInterval)
        {
            scaleChange = scaleChange == 0.15f ? -0.15f : 0.15f;
            blinkInterval = scaleChange == 0.2f ? 0.7f : 0.2f;
            startScale = blinkScale;
            startBlinkTime = now;
        }
        else
        {
            blinkTarget[0].transform.localScale = Vector3.one * blinkScale;
            blinkTarget[1].transform.localScale = Vector3.one * blinkScale;
        }
    }

    void Awake()
    {
        if (UIIngamePage.HORIZONTAL_PROGRESS_BAR)
        {
            currentProgressBar = progressBarHorizontal;
            currentTutorialText = TutorialHorizontal;
            if (progressBarVertical != null)
            {
                Destroy(progressBarVertical);
                Destroy(TutorialVertical);
            }
        }
        else
        {
            currentProgressBar = progressBarVertical;
            currentTutorialText = TutorialVertical;
            if (progressBarHorizontal != null)
            {
                Destroy(progressBarHorizontal);
                Destroy(TutorialHorizontal);
            }
        }

        progressBarFx = currentProgressBar.transform.FindChild("FxProgressBar").GetComponent<ParticleSystem>();

        leftAllScreenButton.shouldDisablePressSignal = false;
        leftAllScreenButton.shouldDisableReleaseSignal = false;
        rightAllScreenButton.shouldDisablePressSignal = false;
        rightAllScreenButton.shouldDisableReleaseSignal = false;

        //transform.FindChild("Status/CameraTest").gameObject.SetActive(false);

        pauseButton = transform.FindChild("ButtonsBar/PauseButton").GetComponent<UIButton>();
        resumeButton = transform.FindChild("ButtonsBar/ResumeButton").GetComponent<UIButton>();

        tutorialBlinkingGO = currentTutorialText.gameObject;
        tutorialBlinkingGO.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_title");
        tutorialBlinkingGO.SetActive(false);
        tapToContinueGO = transform.FindChild("TapToContinueText").gameObject;
#if UNITY_WEBPLAYER
        tapToContinueGO.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("web_pressspacetocontinue");
#else
        tapToContinueGO.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("popup_tutorial_tap");
        tapToContinueGO.transform.FindChild("bg").gameObject.SetActive(false);
#endif
        tapToContinueGO.SetActive(false);

        allScreenButton = transform.FindChild("AllScreenButton").gameObject;
        allScreenButton.SetActive(false);
        arrowLeftGO = transform.FindChild("ArrowLeftButton").gameObject;
        arrowRightGO = transform.FindChild("ArrowRightButton").gameObject;
        arrowLeft = new SpriteRenderer[2];
        arrowRight = new SpriteRenderer[2];
        arrowLeft[0] = transform.FindChild("ArrowLeftButton/arrow_big_dn_left").GetComponent<SpriteRenderer>();
        arrowLeft[1] = transform.FindChild("ArrowLeftButton/arrow_big_up_left").GetComponent<SpriteRenderer>();
        arrowRight[0] = transform.FindChild("ArrowRightButton/arrow_big_dn_right").GetComponent<SpriteRenderer>();
        arrowRight[1] = transform.FindChild("ArrowRightButton/arrow_big_up_right").GetComponent<SpriteRenderer>();

        blinkTarget = new Transform[2];
        blinkTarget[0] = transform.FindChild("ArrowLeftButtonWeb");
        blinkTarget[1] = transform.FindChild("ArrowRightButtonWeb");

#if UNITY_WEBPLAYER
        startBlinkTime = Manager<UIManager>.Get().UITimeSource.TotalTime;
        arrowLeftGO.SetActive(false);
        arrowRightGO.SetActive(false);
#else
        blinkTarget[0].gameObject.SetActive(false);
        blinkTarget[1].gameObject.SetActive(false);
#endif

        turbo = transform.FindChild("Turbo");
        turboBar = turbo.transform.Find("TurboBar");
        turboBarFill = turboBar.FindChild("bar").GetComponent<UINineSlice>();
        turboBarSpark = turbo.transform.Find("TurboBar/spark");
        fullBarWidth = turboBar.FindChild("barBG").GetComponent<UINineSlice>().width;
        comboBar = transform.FindChild("Status/ComboBar").gameObject;
        comboBarFill = comboBar.transform.FindChild("bar_yellow_bg").GetComponent<UINineSlice>();
        fullComboBarWidth = comboBar.transform.FindChild("bar_gray_bg").GetComponent<UINineSlice>().width * 0.96f;
        fullCpBarWidth = currentProgressBar.transform.FindChild("bar_bg").GetComponent<UINineSlice>().width;// transform.FindChild("Status/CpProgressBar/bar_bg").GetComponent<UINineSlice>().width;
        turboIndicators = new TurboObject[turboIndicatorsNumber];
        reverseMultiplier = transform.FindChild("Status/ReverseMultiplier").gameObject;
        reverseMultiplier.SetActive(false);
        
        for (int i = 0; i < turboIndicatorsNumber; i++)
        {
            turboIndicators[i].parent = turbo.transform.FindChild("BonusTurbo" + (i + 1));
            turboIndicators[i].ghost = turbo.transform.FindChild("BonusTurbo" + (i + 1).ToString() + "/Ghost");
            turboIndicators[i].active = turbo.transform.FindChild("BonusTurbo" + (i + 1).ToString() + "/Normal");
            turboIndicators[i].active.gameObject.SetActive(false);
        }
                
        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.SetUIReferences(this, transform.Find("Ranks").gameObject.GetComponent<UIIngameRanks>());
    }

    SpriteRenderer[] renderer2D;
    UITextField[] textfields;
    Color[] renderersColors;
    Color[] textfieldsColors;
    bool graphics2DEnabled = true;
    
    [NonSerialized]
    public Vector3 gOffset = new Vector3(0.0f, 0.0f, 0.0f);

    Vector3 ingamePosition;
    void Disable2DElements()
    {
        //renderer2D = transform.GetComponentsInChildren<SpriteRenderer>();
        //renderersColors = new Color[renderer2D.Length];

        //textfields = transform.GetComponentsInChildren<UITextField>();
        //textfieldsColors = new Color[textfields.Length];

        //for (int i = 0; i < renderer2D.Length; i++)
        //{
        //    renderersColors[i] = renderer2D[i].color;
        //    renderer2D[i].color = new Color(renderersColors[i].r, renderersColors[i].g, renderersColors[i].b, 0.0f);
        //}

        //for (int i = 0; i < renderer2D.Length; i++)
        //{
        //    textfieldsColors[i] = textfields[i].color;
        //    textfields[i].color = new Color(textfieldsColors[i].r, textfieldsColors[i].g, textfieldsColors[i].b, 0.0f);
        //}
        ingamePosition = this.transform.localPosition;
        this.transform.localPosition = new Vector3(0.0f, 10.0f, 0.0f);
        transform.parent.FindChild("fpsText").GetComponent<UITextField>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        graphics2DEnabled = false;
        gOffset = new Vector3(0.0f, 10.0f, 0.0f);
    }

    void Enable2dElements()
    {
        //if (renderer2D == null)
        //    return;

        //for (int i = 0; i < renderer2D.Length; i++)
        //    renderer2D[i].color = renderersColors[i];

        //for (int i = 0; i < textfields.Length; i++)
        //    textfields[i].color = textfieldsColors[i];

        this.transform.localPosition = ingamePosition;
        graphics2DEnabled = true;
        gOffset = new Vector3(0.0f, 0.0f, 0.0f);
        transform.parent.FindChild("fpsText").GetComponent<UITextField>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            if (graphics2DEnabled)
                Disable2DElements();
            else
                Enable2dElements();
        }
#endif
        if (uiRoot == null)
            uiRoot = Manager<UIRoot>.Get();

        UpdateFBAdvise();
        UpdateDraft();

#if UNITY_WEBPLAYER
        UpdateBlink( );
#else
        UpdateArrowsButtonsAlpha();
        UpdateArrowsButtonsBlink();
#endif
        UpdateTutorialBlinking();
    }

    void OnEnable()
    {
        if (Manager<UIManager>.Get().ActivePage.gameObject != gameObject)
            return;

        fbAdviseDuration = OnTheRunDataLoader.Instance.GetFBAdviseDuration();

        uiRoot = Manager<UIRoot>.Get();

		ChartboostBinding.LockInterstitials();

        fbAdviseText.gameObject.SetActive(false);

        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>(); 
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerKinematics = player.GetComponent<PlayerKinematics>();

        uiSharedData = uiRoot.GetComponent<UISharedData>();
        //if(!playerKinematics.PlayerIsDead)
        //    uiSharedData.OnRestartRace();

        fbAdviseAnim = fbAdviseText.transform.GetComponent<Animation>();
        cpDistance = currentProgressBar.transform.FindChild("pointer/CpDistance").gameObject.GetComponent<UITextField>(); //transform.FindChild("Status/CpProgressBar/pointer/CpDistance").gameObject.GetComponent<UITextField>();
        
        //transform.FindChild("Status/CpProgressBar/pointer/CpMeters").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
        currentProgressBar.transform.FindChild("pointer/CpMeters").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
        
        meters = transform.FindChild("Status/MetersValue").gameObject.GetComponent<UITextField>();
        transform.FindChild("Status/Meters").gameObject.GetComponent<UITextField>().text = OnTheRunDataLoader.Instance.GetLocaleString("meters_short");
        coins = transform.FindChild("Status/Coins").gameObject.GetComponent<UITextField>();
        coinsAnim = transform.FindChild("Status/CoinsIcon").gameObject.GetComponent<Animation>();
        time = transform.FindChild("Time/TextField").gameObject.GetComponent<UITextField>();
        //score = transform.FindChild("Status/Score").gameObject.GetComponent<UITextField>();
        scoreMultiplier = transform.FindChild("Status/Multiplier").gameObject.GetComponent<UITextField>();
        specialCarText = transform.FindChild("SpecialCarText").gameObject.GetComponent<UITextField>();
        specialCarTextKeys = transform.FindChild("SpecialCarTextWeb").gameObject.GetComponent<UITextField>();

        multiplierAnim = transform.FindChild("Status/Multiplier").gameObject.GetComponent<Animation>();

        timeBar = transform.FindChild("Time/Bar/bar_orange_bg");
        timeBar.localScale = Vector3.one;

		redTimeBar = timeBar.transform.GetChild(0).gameObject;
		redTimeBar.SetActive( false );

		iconClockAnim = transform.FindChild("Time/icon_clock_hud").gameObject.GetComponent<Animation>();
		iconClockAnim.Rewind();
		iconClockAnim.enabled = false;

		timeTextAnim = time.gameObject.GetComponent<Animation>();
		timeTextAnim.enabled = false;
        
        progressPointer = currentProgressBar.transform.FindChild("pointer"); //transform.FindChild("Status/CpProgressBar/pointer");
        progressPointer.localPosition = Vector3.zero;
        cpProgressBar = currentProgressBar.transform.FindChild("bar_progress"); //transform.FindChild("Status/CpProgressBar/bar_progress");
        cpProgressBar.localScale = Vector3.one;

        meters.text = "0";
        cpDistance.text = uiRoot.FormatTextNumber((int)((float)uiSharedData.MaxCheckpointDistance * OnTheRunUtils.ToOnTheRunMeters));
        coins.text = uiRoot.FormatTextNumber(gameplayManager.CoinsCollected);
        //score.text = gameplayManager.CurrentRunScore.ToString();
        scoreMultiplier.text = OnTheRunDataLoader.Instance.GetLocaleString("combo") + " 1";

        comboBar.SetActive(false);
        scoreMultiplier.gameObject.SetActive(false);
        specialCarText.gameObject.SetActive(false);
        specialCarTextKeys.gameObject.SetActive(false);

        ResetTurboIndicators();
        //turboBar.gameObject.SetActive(false);

		this.UpdateCpDistanceData();

        this.UpdateTime();
        //Debug.Log("UIIngamePage ONENABLE");

        playerDraftRef = player.GetComponent<PlayerDraft>();

        currentButtonsAlpha = startButtonsAlpha;
        Color c = arrowLeft[0].color;
        arrowRight[0].color = arrowLeft[0].color = new Color(c.r, c.g, c.b, currentButtonsAlpha);
        c = arrowLeft[1].color;
        arrowRight[1].color = arrowLeft[1].color = new Color(c.r, c.g, c.b, currentButtonsAlpha);
        
        startComboFadeOut = false;

#if UNITY_WEBPLAYER
        Transform cheatBtn = transform.FindChild("ButtonsBar/CheatButton");
        if (cheatBtn != null)
            cheatBtn.localPosition = new Vector3(0.43f, -2.38f, 0.0f);
#endif
    }

	void OnDisable()
	{
		ChartboostBinding.UnlockInterstitials();
	}

    protected void UpdateTime()
    {
        time.text = MillisecondsToSeconds(gameplayManager.GameplayTime)+"''";

        if(OnTheRunTutorialManager.Instance.TutorialActive)
            timeBar.localScale = Vector3.one;

        if (gameplayManager.InitGameplayTime > 0.0f)
        {
            float timePerc = Mathf.Clamp01(gameplayManager.GameplayTime / gameplayManager.InitGameplayTime);
            timeBar.localScale = new Vector3(timePerc, 1.0f, 1.0f);
            
			int iRemainingSeconds = Mathf.FloorToInt(gameplayManager.GameplayTime) % 60;

			if( iRemainingSeconds < TIME_RED_TIMEOUT && redTimeBar.activeSelf == false )
			{
				redTimeBar.SetActive( true );
				iconClockAnim.enabled = true;
				timeTextAnim.enabled = true;
			}
			else if( iRemainingSeconds > TIME_RED_TIMEOUT )
			{
				redTimeBar.SetActive( false );
				iconClockAnim.Rewind();
				iconClockAnim.enabled = false;
				timeTextAnim.Rewind();
				timeTextAnim.enabled = false;

				time.color = Color.white;
				iconClockAnim.gameObject.transform.localScale = DEFAULT_SCALE;
			}
        }
        else
        {
			timeTextAnim.Rewind();
			timeTextAnim.enabled = false;
			iconClockAnim.Rewind();
			iconClockAnim.enabled = false;

			time.color = Color.white;
			iconClockAnim.gameObject.transform.localScale = DEFAULT_SCALE;

			redTimeBar.SetActive( false );
            timeBar.localScale = Vector3.one;
        }
    }

    public void StartProgressBarAnimation()
    {
        progressBarFx.gameObject.SetActive(true);
        progressBarFx.Play(true);
        StartCoroutine(StopProgressBarAnimation(3.0f));
    }

    IEnumerator StopProgressBarAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);

        progressBarFx.Stop(true);
        progressBarFx.gameObject.SetActive(false);
    }

    protected void UpdateCpDistanceData()
    {
        if (progressBarFx.isPlaying)
        {
            cpDistance.text = "-0";
            return;
        }

        float width = 0.0f;
        if (uiSharedData.BaseCheckPointDistance != 0.0f)
            width = (uiSharedData.BaseCheckPointDistance - uiSharedData.CheckpointDistance) * fullCpBarWidth / uiSharedData.BaseCheckPointDistance;

        cpDistance.text = "-" + uiRoot.FormatTextNumber((int)((float)(uiSharedData.CheckpointDistance) * OnTheRunUtils.ToOnTheRunMeters));
        float perc = 0.0f;
        if ((float)uiSharedData.BaseCheckPointDistance != 0.0f)
        {
            float playerDistanceFromCheckpoint = (float)uiSharedData.CheckpointDistance;

            //Patch for starnge issue with progress bar (very rare)
            if (playerDistanceFromCheckpoint > (float)uiSharedData.BaseCheckPointDistance && gameplayManager.Gameplaystarted)
            {
                int rest =  Mathf.FloorToInt(playerDistanceFromCheckpoint / (float)uiSharedData.BaseCheckPointDistance);
                playerDistanceFromCheckpoint = playerDistanceFromCheckpoint - rest * (float)uiSharedData.BaseCheckPointDistance;
            }

            perc = playerDistanceFromCheckpoint / (float)uiSharedData.BaseCheckPointDistance;
        }

        cpProgressBar.localScale = new Vector3(Mathf.Clamp01(1.0f - perc), 1.0f, 1.0f);

        //progressPointer.localPosition = new Vector3(width - 0.04f, 0.15f, 0.0f);
        width = cpProgressBar.transform.localPosition.x + cpProgressBar.localScale.x * (1.86f - cpProgressBar.transform.localPosition.x);
        progressPointer.localPosition = new Vector3(width, 0.15f, 0.0f);
    }

    public static string MillisecondsToSeconds(float time)
    {
        time = SBSMath.Max(time, 0.0f);

        int secs = Mathf.FloorToInt(time) % 60;
        
        return string.Format(CultureInfo.InvariantCulture, "{0:D2}", secs);
    }

    protected float updateTimer = 0.1f;

	int oldCoins = 0;
	int oldShownPartial = -1;

	public Animation tfPartialBounceAnim;

    void LateUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.Keypad5))
        //    OnTheRunSoundsManager.Instance.MusicActive = !OnTheRunSoundsManager.Instance.MusicActive;

        uiSharedData.UpdateSharedData();

        updateTimer -= TimeManager.Instance.MasterSource.DeltaTime;
        if (updateTimer < 0.0f)
        {
            updateTimer = 0.12f;
            this.UpdateTime();
            if (!OnTheRunTutorialManager.Instance.TutorialActive)
                this.UpdateCpDistanceData();

            meters.text = uiRoot.FormatTextNumber(uiSharedData.InterfaceDistance);

            if (null != currentMission)
            {
                int currPartial = Mathf.FloorToInt(currentMission.Counter);
                int currGoal = Mathf.RoundToInt(currentMission.checkCounter);
                int shownPartial = currPartial > currGoal ? currGoal : currPartial;

				if( oldShownPartial != shownPartial )
				{
               	 	tfMissionPartial.text = shownPartial.ToString();

//					Debug.Log("MISSIONE "+currentMission.category.ToString());

//					// AVOID SHOWING THE BOUNCE FOR THESE MISSION TYPES
					if( currentMission.category != OnTheRunSingleRunMissions.MissionCategory.RunFor &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.WrongDirection &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.AvoidBarriers &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.CentralLaneFor &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.RightLaneFor &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.LeftLaneFor &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.CentralLaneUSAFor &&
					   currentMission.category != OnTheRunSingleRunMissions.MissionCategory.UseSpecialVehicleFor )
					{ 
						tfPartialBounceAnim.gameObject.SetActive( false );
						tfPartialBounceAnim.gameObject.SetActive( true );
						tfPartialBounceAnim.Play();
					}
				}

				oldShownPartial = shownPartial;

                tfMissionTotal.text = currentMission.checkCounter.ToString() + currentMission.GoalUnitIngame;
            }
        }

		// WTF set text in Update ?? use setters !  // TODO someone pls
		/*if( oldCoins != gameplayManager.CoinsCollected )
		{
			oldCoins = gameplayManager.CoinsCollected;
            coins.text = uiRoot.FormatTextNumber(gameplayManager.CoinsCollected);
			//coinsAnim.Rewind();
			coinsAnim.Play();
		}*/

        if (player != GameObject.FindGameObjectWithTag("Player"))
        {
            player = GameObject.FindGameObjectWithTag("Player");
            playerKinematics = player.GetComponent<PlayerKinematics>();
            playerDraftRef = player.GetComponent<PlayerDraft>();
        }

        if (playerKinematics.WrongDirection && !reverseMultiplier.activeInHierarchy)
        {
            if (!gameplayManager.IsSpecialCarActive)
            {
                reverseMultiplier.SetActive(true);
                reverseMultiplier.GetComponent<Animation>().Play();
            }
        }

        if (!playerKinematics.WrongDirection && reverseMultiplier.activeInHierarchy)
            reverseMultiplier.SetActive(false);
    }

    public void ShowFBAdvise(bool logged)
    {
        //Debug.Log("SHOWFBADVISE: " + logged + ", " + fbAdviseDuration);
        if (fbAdviseDuration <= 0)
            return;

        fbAdviseText.gameObject.SetActive(true);
        fbAdviseText.text = OnTheRunDataLoader.Instance.GetLocaleString(logged ? "no_friends_ingame" : "not_logged_in_ingame");
        fbAdviseText2.text = fbAdviseText.text;
        fbAdviseAnim.clip = fbAdviseAnim["FbAdviseTextAnimEnter"].clip;
        fbAdviseAnim["FbAdviseTextAnimEnter"].time = 0.0f;
        fbAdviseAnim.Rewind();
        fbAdviseAnim.Play();
        fbAdviseTimer = TimeManager.Instance.MasterSource.TotalTime;
        fbAdviseIsEntering = true;
    }

    public void HideFBAdvise()
    {
        fbAdviseAnim.clip = fbAdviseAnim["FbAdviseTextAnimExit"].clip;
        fbAdviseAnim["FbAdviseTextAnimExit"].time = 0.0f;
        fbAdviseAnim.Rewind();
        fbAdviseAnim.Play();
        fbAdviseIsEntering = false;
    }

    void UpdateFBAdvise()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            ShowFBAdvise(false);
        }
#endif

        if (fbAdviseTimer < 0)
            return;

        float now = TimeManager.Instance.MasterSource.TotalTime;
        if (now - fbAdviseTimer >= fbAdviseDuration)
        {
            if (fbAdviseIsEntering)
            {
                HideFBAdvise();
                //fbAdviseDuration = now;
            }

            if (!fbAdviseIsEntering && !fbAdviseAnim.isPlaying)
            {
                fbAdviseText.gameObject.SetActive(false);
                fbAdviseTimer = -1.0f;
            }
        }
    }

    void OnSetAlphaOnCombo(float _currAlpha)
    {
        float currAlpha = _currAlpha;
        currAlpha = Mathf.Clamp01(currAlpha);

        comboBarBg.color.a = currAlpha;
        comboBarBg.ApplyParameters();
        comboBarFill.color.a = currAlpha;
        comboBarFill.ApplyParameters();
        comboBarGrayBg.color.a = currAlpha;
        comboBarGrayBg.ApplyParameters();
        scoreMultiplier.color.a = currAlpha;
        scoreMultiplier.ApplyParameters();
        scoreMultiplierStroke.color.a = currAlpha;
        scoreMultiplierStroke.ApplyParameters();
    }

    float comboAlpha = 0.0f;
    bool startComboFadeOut = false;
    void OnScoreMultiplierChange()
    {
        if (uiSharedData.ScoreDistanceMultiplier > 0)
        {
            startComboFadeOut = false;
            comboBar.SetActive(true); 
            comboAlpha = 1.0f;
            OnSetAlphaOnCombo(comboAlpha);
            scoreMultiplier.text = OnTheRunDataLoader.Instance.GetLocaleString("combo") + " " + uiSharedData.ScoreDistanceMultiplier.ToString();
            scoreMultiplier.gameObject.SetActive(true);
            multiplierAnim.Play();
        }
        else
            startComboFadeOut = true;

        //scoreMultiplier.gameObject.SetActive(uiSharedData.ScoreDistanceMultiplier > 0);
    }

    void UpdateIngameCoinsText()
    {
        //oldCoins = gameplayManager.CoinsCollected;
        coins.text = uiRoot.FormatTextNumber(gameplayManager.CoinsCollected);
        coinsAnim.Play();
    }

    /*void OnCoinsCollected(int coinsValue)
    {
        if (coinsValue > 0)
        {
            UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
            data.text = uiRoot.FormatTextNumber(coinsValue);
            data.initPosition = new Vector3(0.2f, 0.2f, 0.0f) + gOffset;
            data.goalPosition = new Vector3(0.91f, 0.93f, 0.0f) + gOffset;
            data.timeInIdle = 0.5f;
            data.scaleSpeed = 2.5f;
            data.coinsGained = coinsValue;

            GameObject flyer = Instantiate(coinsFlyer, Vector3.zero+gOffset, Quaternion.identity) as GameObject;
            flyer.SendMessage("InitializeWithPercentage", data);
        }
    }*/

    void OnScoreForCollision(Vector3 worldPos)
    {
        Vector3 screenPos = gameplayManager.MainCamera.WorldToViewportPoint(worldPos);
        screenPos.z = 0.0f;
        screenPos = Manager<UIManager>.Get().UICamera.ViewportToWorldPoint(screenPos);
        float orthoWidth = Manager<UIManager>.Get().UICamera.aspect * Manager<UIManager>.Get().UICamera.orthographicSize;
        screenPos.x = Mathf.Clamp(screenPos.x, -orthoWidth * 0.85f, orthoWidth * 0.85f);
        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text = uiRoot.FormatTextNumber((int)(uiSharedData.scoreForCollision * uiSharedData.ScoreDistanceMultiplier));
        data.initPosition = screenPos + gOffset;
        data.goalPosition = data.initPosition + new Vector3(0.0f, 1.0f, 0.0f) + gOffset;
        data.timeInIdle = 0.0f;
        data.scaleSpeed = 7.0f;
        data.scoreGained = uiSharedData.scoreForCollision * uiSharedData.ScoreDistanceMultiplier;

        GameObject flyer = Instantiate(fastCoinsFlyer, Vector3.zero+gOffset, Quaternion.identity) as GameObject;
        flyer.SendMessage("Initialize", data);
    }

    public void OnCoinsForCollision(Vector3 worldPos, int howManyCoins, bool waterfallEffect=false)
    {
        howManyCoins *= gameplayManager.MoneyMultiplier;

        Vector3 screenPos = gameplayManager.MainCamera.WorldToViewportPoint(worldPos);
        screenPos.z = 0.0f;
        screenPos = Manager<UIManager>.Get().UICamera.ViewportToWorldPoint(screenPos);
        float orthoWidth = Manager<UIManager>.Get().UICamera.aspect * Manager<UIManager>.Get().UICamera.orthographicSize;
        screenPos.x = Mathf.Clamp(screenPos.x, -orthoWidth * 0.85f, orthoWidth * 0.85f);
        screenPos.y = Mathf.Clamp(screenPos.y, -orthoWidth * 0.625f, orthoWidth * 0.625f);
        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text =  uiRoot.FormatTextNumber(howManyCoins);
        data.initPosition = screenPos + gOffset;
        data.goalPosition = data.initPosition + Vector3.down * 2.0f + gOffset; //1.0f
        if (waterfallEffect)
        {
            float coinSide = UnityEngine.Random.Range(-0.75f, 0.75f);
            data.goalPosition += Vector3.right * coinSide;
        }
        data.timeInIdle = 0.0f;
        data.scaleSpeed = 7.0f;
        data.timeToExit = 0.85f;
        //data.scoreGained = uiSharedData.scoreForCollision * uiSharedData.ScoreDistanceMultiplier;
        data.coinsGained = howManyCoins;

        GameObject flyer = Instantiate(coinsFlyer, Vector3.zero+gOffset, Quaternion.identity) as GameObject;
        flyer.SendMessage("Initialize", data);
    }

    
    void OnLiveNews( float duration )
    {
        DestroyLiveNewsFlyer();

        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text = "";
        data.initPosition = new Vector3(-2.42f, 0.87f, 0.0f) + gOffset;
        data.goalPosition = new Vector3(-2.0f, 0.87f, 0.0f) + gOffset;
        data.timeInIdle = duration;
        data.scaleSpeed = 4.0f;
        data.timeToExit = 0.85f;
        data.flyerWidth = 1.8f;

        liveNewsFlyerRef = Instantiate(liveNewsFlyer, Vector3.zero+gOffset, Quaternion.identity) as GameObject;
        if (UIIngamePage.HORIZONTAL_PROGRESS_BAR)
        {
            float yPos = currentMission.Done ? 0.5f : 0.07f;
            data.initPosition = new Vector3(-2.42f, yPos, 0.0f) + gOffset;
            data.goalPosition = new Vector3(-2.2f, yPos, 0.0f) + gOffset;
            liveNewsFlyerRef.SendMessage("InitializeWithPercentageY2", data);
        }
        else
            liveNewsFlyerRef.SendMessage("InitializeWithPercentageY", data);

        //OnTheRunIngameHiScoreCheck.Instance.HideRanksTemporarily();
    }

    public void DestroyLiveNewsFlyer( )
    {
        if (liveNewsFlyerRef!=null)
            Destroy(liveNewsFlyerRef);

        //OnTheRunIngameHiScoreCheck.Instance.ShowRanksBackAgain();
    }

    public void StartPlayerComboFlyier(Vector3 direction)
    {
        if (uiSharedData.ScoreDistanceMultiplier < 1)
            return;

        Vector3 screenPos = gameplayManager.MainCamera.WorldToViewportPoint(player.transform.position + Vector3.back * 2.5f);
        screenPos.z = 0.0f;
        screenPos = Manager<UIManager>.Get().UICamera.ViewportToWorldPoint(screenPos);
        float orthoWidth = Manager<UIManager>.Get().UICamera.aspect * Manager<UIManager>.Get().UICamera.orthographicSize;
        screenPos.x = Mathf.Clamp(screenPos.x, -orthoWidth * 0.85f, orthoWidth * 0.85f);
        screenPos.y = Mathf.Clamp(screenPos.y, -orthoWidth * 0.625f, orthoWidth * 0.625f);
        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text = OnTheRunDataLoader.Instance.GetLocaleString("combo") + " " + uiSharedData.ScoreDistanceMultiplier.ToString();
        data.initPosition = screenPos + gOffset;
        data.goalPosition = data.initPosition + direction * 2.0f; //1.0f
        data.timeInIdle = 0.6f;
        data.scaleSpeed = 7.0f;
        data.fadeAfter = 0.2f;
        data.fadeSpeed = 10.0f;
        data.flashAfter = 0.0f;//0.3f;
        data.flashTime = 0.5f;

        //DestroyPlayerComboFlyier();
        comboPlayerFlyerRef = Instantiate(comboPlayerFlyer, Vector3.zero + gOffset, Quaternion.identity) as GameObject;
        comboPlayerFlyerRef.SendMessage("Initialize", data);
    }

    public void DestroyPlayerComboFlyier( )
    {
        if (comboPlayerFlyerRef != null)
            Destroy(comboPlayerFlyerRef); 
    }

    void OnTutorialPopupShown()
    {
        DestroyPlayerComboFlyier();
    }

    void OnPoliceDestroyed( )
    {
        LaunchTurboFlyer(OnTheRunDataLoader.Instance.GetLocaleString("police_destroyed"), gOffset + Vector3.up);
        int coinsValue = 100;

        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text = uiRoot.FormatTextNumber(coinsValue);
        data.initPosition = new Vector3(0.2f, 1.2f, 0.0f)+gOffset;
        data.goalPosition = new Vector3(0.91f, 1.93f, 0.0f)+gOffset;
        data.timeInIdle = 0.5f;
        data.scaleSpeed = 2.5f;
        data.coinsGained = coinsValue;

        GameObject flyer = Instantiate(coinsFlyer, Vector3.zero+gOffset, Quaternion.identity) as GameObject;
        flyer.SendMessage("InitializeWithPercentage", data);
    }

    void OnTimeSaved(int time)
    {
        UICoinsFlyer.CoinsFlyerData data = new UICoinsFlyer.CoinsFlyerData();
        data.text = OnTheRunDataLoader.Instance.GetLocaleString("time_saved") + ": " + time.ToString();
        data.initPosition = new Vector3(0.0f, 0.8f, 0.0f)+gOffset;
        data.goalPosition = new Vector3(0.0f, 3.0f, 0.0f)+gOffset;
        data.timeInIdle = 0.5f;
        data.scaleSpeed = 2.5f;

        GameObject flyer = Instantiate(timeSavedFlyer, Vector3.zero, Quaternion.identity) as GameObject;
        flyer.SendMessage("InitializeWithPercentage", data);
    }

    void OnLaunchTimeStoppedFlyer()
    {
        timeStopped = Instantiate(timeStoppedFlyer) as GameObject;
        timeStopped.GetComponent<Animation>().Play();
        timeStopped.transform.position += gOffset;
    }

    void OnDestroyTimeStoppedFlyer()
    {
        Destroy(timeStopped);
    }

    /*public void ActivateSpecialPowerText(bool active, string textToDisplay="")
    {
        if (active)
            specialCarText.text = textToDisplay;
        specialCarText.gameObject.SetActive(active);
    }*/

    public void ActivateSpecialPowerText(bool active, TruckBehaviour.TrasformType type = TruckBehaviour.TrasformType.None)
    {
        specialCarText.gameObject.SetActive(false);
        specialCarTextKeys.gameObject.SetActive(false);

        if (active)
        {
            string textToDisplay = "";
            switch (type)
            {
                case TruckBehaviour.TrasformType.Bigfoot:
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("destroy_them_all");
                    break;
                case TruckBehaviour.TrasformType.Firetruck:
#if UNITY_WEBPLAYER
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("web_tap_to_cannon");
#else
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("tap_to_cannon");
#endif
                    break;
                case TruckBehaviour.TrasformType.Plane:
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("plane_active");
                    break;
                case TruckBehaviour.TrasformType.Tank:
#if UNITY_WEBPLAYER
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("web_tap_to_shoot");
#else
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("tap_to_shoot");
#endif
                    break;
                case TruckBehaviour.TrasformType.Ufo:
                    textToDisplay = OnTheRunDataLoader.Instance.GetLocaleString("ufo_active");
                    break;
            }
            specialCarText.text = textToDisplay;
            specialCarTextKeys.text = textToDisplay;
        }

#if UNITY_WEBPLAYER
        if (type == TruckBehaviour.TrasformType.Firetruck || type == TruckBehaviour.TrasformType.Tank)
            specialCarTextKeys.gameObject.SetActive(active);
        else
            specialCarText.gameObject.SetActive(active);
#else
        specialCarText.gameObject.SetActive(active);
#endif
    }

    public void ActivateSpecialVehicleTextForTutorial(bool active)
    {
#if !UNITY_WEBPLAYER
        if (gameplayManager.IsSpecialCarActive)
        {
            specialCarText.gameObject.SetActive(active);
        }
#endif
    }

    protected void ActivateResumeButton(bool activate)
    {
        pauseButton.gameObject.SetActive(!activate);
        resumeButton.gameObject.SetActive(activate);
    }

    #region Signals
    void Signal_OnEnter(UIPage page)
    {
        if (TimeManager.Instance.MasterSource.IsPaused)
            TimeManager.Instance.MasterSource.Resume();

        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        if (aspectRatio > 1.4f)
        {
            progressBarHorizontal.transform.localScale = Vector3.one * 1.2f;
            progressBarFx.startLifetime = 1.2f;
            progressBarFx.transform.FindChild("finalExpl").GetComponent<ParticleSystem>().startDelay = progressBarFx.startLifetime;
        }

        startChangeAlphaTime = -1.0f;
        ActivateResumeButton(false);
        gameplayManager.SendMessage("OnInitGameplayTime");
        ActivateArrowBlink(false, -1);
        ActivateArrowBlink(false, 1);

        //reset progress bar
        if (playerKinematics.Distance == 0.0f)
        {
            uiSharedData.OnRestartRace();
            cpDistance.text = "";
            float perc = 1.0f;
            cpProgressBar.localScale = new Vector3(1.0f - perc, 1.0f, 1.0f);
            float width = cpProgressBar.transform.localPosition.x + cpProgressBar.localScale.x * (1.86f - cpProgressBar.transform.localPosition.x);
            progressPointer.localPosition = new Vector3(width, 0.15f, 0.0f);
        }

        if (OnTheRunTutorialManager.Instance.TutorialActive)
        {
            ActivateArrows(false, -1);
            ActivateArrows(false, 1);
            ActivateArrowBlink(true, 1);
            ActivateArrowBlink(true, -1);
            ActivateTapToContinue(false);

            SetProgressBarVisibility(false);
        }
        else
        {
            ActivateArrows(true, -1);
            ActivateArrows(true, 1);

            SetProgressBarVisibility(true);
        }


        missionStatus.SetActive(false);
        missionStatusPassed.SetActive(false);
        UpdateMissionLateralPanel();
        //missionStatus.SetActive(!OnTheRunTutorialManager.Instance.TutorialActive);
    }

    void Signal_OnExit(UIPage page)
    {
        DestroyPlayerComboFlyier();

        uiRoot.ActivateBackground(true);

        DestroyLiveNewsFlyer();

        gameplayManager.GetComponent<OnTheRunFBFriendsSpawner>().DestroyAll();

		timeTextAnim.Rewind();
		timeTextAnim.enabled = false;
		iconClockAnim.Rewind();
		iconClockAnim.enabled = false;
		redTimeBar.SetActive( false );

		time.color = Color.white;
		iconClockAnim.gameObject.transform.localScale = DEFAULT_SCALE;

        OnDestroyTimeStoppedFlyer();
        LevelRoot.Instance.BroadcastMessage("OnReset", SendMessageOptions.DontRequireReceiver);
    }

    void UpdateMissionLateralPanel()
    {
        currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        if (currentMission != null)
        {
            if (currentMission.Done)
                missionStatus.SetActive(false);
            else
                missionStatus.SetActive(true);

            if (missionStatus.activeInHierarchy)
            {
                tfMissionSubTitle.text = " ";
                tfMissionTitle.text = currentMission.MissionPrefix;
                tfMissionSubTitle.text = currentMission.MissionPostfix;
                tfMissionSubTitle.ApplyParameters();
            }
        }

        if (OnTheRunTutorialManager.Instance.TutorialActive)
            missionStatus.SetActive(false);
    }

    void SetProgressBarVisibility(bool visible)
    {
        progressBarHorizontal.SetActive(visible);

        if (currentMission==null)
            currentMission = OnTheRunSingleRunMissions.Instance.CurrentTierMission;
        
        if (visible && currentMission != null)
        {
            if (!currentMission.Done)
                missionStatus.SetActive(visible);
        }
    }

    void Signal_OnTestCameraRelease(UIButton button)
    {
        button.transform.FindChild("tfTextfield").GetComponent<UITextField>().text = gameplayManager.MainCamera.GetComponent<FollowCharacter>().ChangeCamera();
    }

    void Signal_OnCheatRelease(UIButton button)
    {
        GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GameplayTime = 2.0f;
    }


    void Signal_OnPauseRelease(UIButton button)
    {
       	StartCoroutine( PauseGameTask() );
    }

	IEnumerator PauseGameTask()
	{
		interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);

		yield return new WaitForEndOfFrame();

		meters.text = uiRoot.FormatTextNumber(uiSharedData.InterfaceDistance);

		gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Paused);
		ActivateResumeButton(true);
		Manager<UIManager>.Get().PushPopup("PausePopup");
		
		if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame )
		{
			//			Debug.LogWarning("devo interropmere musica");
			OnTheRunSoundsManager.Instance.PauseMusicForced();
		}
	}

    void Signal_OnResumeRelease(UIButton button)
    {
        ActivateResumeButton(false);

        interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Click);
        Manager<UIManager>.Get().PopPopup();

        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Racing);

		
		if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame )
		{
//			Debug.LogWarning("devo resumare musica");
			OnTheRunSoundsManager.Instance.ResumeForced();
		}
    }

    void Signal_OnLeftPressed(UIButton button)
    {
        gameplayManager.PlayerKinematics.SendMessage("OnLeftInputDown");
    }

    void Signal_OnLeftReleased(UIButton button)
    {
        OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapLeft);
        gameplayManager.PlayerKinematics.SendMessage("OnLeftInputUp");
    }

    void Signal_OnRightPressed(UIButton button)
    {
        gameplayManager.PlayerKinematics.SendMessage("OnRightInputDown");
    }

    void Signal_OnRightReleased(UIButton button)
    {
        OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapRight);
        gameplayManager.PlayerKinematics.SendMessage("OnRightInputUp");
    }

    void Signal_OnAllScreenReleased(UIButton button)
    {
        OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapEverywhere);
    }
    #endregion

    public void Test_LaunchHiScoreFlyer(string text, Sprite opponentsPicture)
    {
        UIFlyer pictureflyer = Manager<UIManager>.Get().PlayFlyer("SimpleFlyerWithPictures"/*"SimpleFlyer"*/, Vector3.zero + gOffset, Quaternion.identity);
        UIFlyer flyer = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + gOffset, Quaternion.identity);
        pictureflyer.transform.Find("my_avatar/fb_user").GetComponent<SpriteRenderer>().sprite = OnTheRunMcSocialApiData.Instance.GetPicture();
        pictureflyer.transform.Find("opponents_avatar/fb_user").GetComponent<SpriteRenderer>().sprite = opponentsPicture;

        UITextField tf = flyer.gameObject.GetComponentInChildren<UITextField>();
        tf.text = text;
        tf.ApplyParameters();
    }

    #region Turbo
    public void LaunchTurboFlyer(string text, Vector3 offset)
    {
        UIFlyer turboFlyer = Manager<UIManager>.Get().PlayFlyer("SimpleFlyer", Vector3.zero + offset + Vector3.up, Quaternion.identity);

        UITextField tf = turboFlyer.gameObject.GetComponentInChildren<UITextField>();
        tf.text = text;
        tf.ApplyParameters();
    }

    public void OnTurboBonusCollected(int count)
    {
        if (!OnTheRunTutorialManager.Instance.TutorialActive)
        {
            if (count == 1)
                LaunchTurboFlyer(OnTheRunDataLoader.Instance.GetLocaleString("two_more"), gOffset);
            else if (count == 2)
                LaunchTurboFlyer(OnTheRunDataLoader.Instance.GetLocaleString("one_more"), gOffset);
            //else
            //    interfaceSounds.SendMessage("PlayGeneralInterfaceSound", OnTheRunInterfaceSounds.InterfaceSoundsType.Turbo);
        }
        SetIndicatorActive(count - 1, true, true);
    }

    public void ResetTurboIndicators()
    {
        for (int i = 0; i < turboIndicatorsNumber; i++)
            SetIndicatorActive(i, (gameplayManager.TurboCounter >= i + 1));

        turboBarFill.gameObject.SetActive(false);
        turboBarSpark.gameObject.SetActive(false);
    }

    public void OnTurboActive(bool active)
    {
        //for (int i = 0; i < turboIndicatorsNumber; i++)
        //    turboIndicators[i].parent.gameObject.SetActive(!active);

        turboBarFill.gameObject.SetActive(active);
        turboBarSpark.gameObject.SetActive(active);
        if (active)
            turboBarFill.width = fullBarWidth;
    }

    public void OnTurboEnd()
    {
        //for (int i = 0; i < turboIndicatorsNumber; i++)
        //    turboIndicators[i].parent.gameObject.SetActive(true);

        turboBarSpark.gameObject.SetActive(false);
        turboBarFill.gameObject.SetActive(false);
    }

    public void OnTurboBarUpdate(float ratio)
    {
        float currentWidth = fullBarWidth * ratio;
        currentWidth = Mathf.Clamp(currentWidth, 0.0f, fullBarWidth);
        turboBarFill.width = currentWidth;
        turboBarSpark.localPosition = Vector3.zero + Vector3.right * currentWidth + Vector3.up * 0.07f;
    }

    public void SetIndicatorActive(int id, bool active)
    { 
        SetIndicatorActive(id, active, false);
    }

    public void SetIndicatorActive(int id, bool active, bool withAnim)
    {
        turboIndicators[id].ghost.gameObject.SetActive(!active);
        turboIndicators[id].active.gameObject.SetActive(active);
        if (active && withAnim)
        {
            turboIndicators[id].active.GetComponent<Animation>().Rewind();
            turboIndicators[id].active.GetComponent<Animation>().Play();
        }
        else if (active)
        {
            turboIndicators[id].active.GetComponent<Animation>().Play();
            turboIndicators[id].active.GetComponent<Animation>()["TurboBolt@"].time = turboIndicators[id].active.GetComponent<Animation>()["TurboBolt@"].length;
            turboIndicators[id].active.GetComponent<Animation>().Sample();
            turboIndicators[id].active.GetComponent<Animation>().Stop();
        }
    }
    #endregion

    #region Draft
    void UpdateDraft()
    {
        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        /*if (!comboBar.activeInHierarchy && !playerDraftRef.DraftUSAActive && playerDraftRef.DraftTimer > 0.0f)
        {
            startComboFadeOut = false;
            comboBar.SetActive(true);
            scoreMultiplier.gameObject.SetActive(true);
            comboAlpha = 1.0f;
            OnSetAlphaOnCombo(1.0f);
        }*/

        if (comboBar.activeInHierarchy && !playerDraftRef.DraftUSAActive)
        {
            float currentWidth = fullComboBarWidth;
            currentWidth = Mathf.Clamp(currentWidth, 0.0f, fullComboBarWidth);
            if (playerDraftRef.IsInDraft)
            {
                float tempDraftTimer = playerDraftRef.DraftTimer < 0.0f ? fullComboBarWidth : ((currentWidth * playerDraftRef.DraftTimer) / playerDraftRef.draftDuration);
                comboBarFill.width = tempDraftTimer;
            }
            else
                comboBarFill.width = currentWidth * playerDraftRef.DraftTimer / playerDraftRef.draftDuration;
            comboBarFill.width = Mathf.Clamp(comboBarFill.width, 0.0f, fullComboBarWidth) * 2;
            if (comboBarFill.width > 0.0f)
                comboAlpha = 1.0f;

            if (comboAlpha <= 0.0f)
                comboBar.SetActive(false);
            //Debug.Log("playerDraftRef.DraftTimer: " + playerDraftRef.DraftTimer + " playerDraftRef.DraftTimer / playerDraftRef.draftDuration " +(playerDraftRef.DraftTimer / playerDraftRef.draftDuration));
        }

        if (startComboFadeOut)
        { 
            comboAlpha -= dt * 2.0f;
            comboAlpha = Mathf.Clamp01(comboAlpha);
            OnSetAlphaOnCombo(comboAlpha);
            if (comboAlpha <= 0.0f)
            {
                startComboFadeOut = false;
                comboBar.SetActive(false);
                scoreMultiplier.gameObject.SetActive(false);
            }
            //Debug.Log("alpha = " + comboAlpha);
        }
    }
    #endregion

    void OnBackButtonAction()
    {
        pauseButton.onReleaseEvent.Invoke(pauseButton);
    }

    public void StartMissionPassedAnimation()
    {
        OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.MissionCompleted);
        missionStatusPassed.SetActive(true);
        missionStatusPassed.GetComponent<Animation>().Play();
        missionStatusPassed.GetComponentInChildren<ParticleSystem>().Play();

        missionStatus.GetComponent<UIEnterExitAnimations>().SendMessage("StartEnterExitAnimation", UIEnterExitAnimations.AnimationType.ExitPage);
    }

    void OnApplicationPause(bool paused)
    {
#if UNITY_ANDROID
        if (!paused)
            ForceLeftRightButtonsReleaseIfPressed();
#endif
    }

    void ForceLeftRightButtonsReleaseIfPressed()
    {
        if (leftAllScreenButton != null && (leftAllScreenButton.State == UIButton.StateType.MouseOver || leftAllScreenButton.State == UIButton.StateType.Pressed))
            leftAllScreenButton.State = UIButton.StateType.Normal;

        if (rightAllScreenButton != null && (rightAllScreenButton.State == UIButton.StateType.MouseOver || rightAllScreenButton.State == UIButton.StateType.Pressed))
            rightAllScreenButton.State = UIButton.StateType.Normal;

    }
}
