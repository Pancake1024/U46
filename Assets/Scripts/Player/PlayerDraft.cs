using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDraft : MonoBehaviour
{
    #region Public Members
    [HideInInspector]
    public float draftDuration = 1.2f;
    protected float forwardDistance = 15.0f;
    public AudioClip draftSound;
    public AudioClip draftStartSound;

    public float draftDeltaSpeed;
    public float draftAcceleration;
    #endregion

    #region Protected Members
    protected OnTheRunEnvironment environmentManager;
    protected UISharedData uiSharedData;
    protected OnTheRunGameplay gameplayManager;
    protected UIIngamePage ingamePage;
    protected AudioSource draftStartSource;
    protected PlayerKinematics kinematics;
    protected bool isInDraft = false;
    protected bool isInUSADraft = false;
    protected float maxDraftSoundVolume = 0.7f;
    protected float endDraftTimer = -1.0f;
    protected Collider lastHitCollider = null;
    protected LayerMask ignoreLayers = 0;
    protected bool readyForPlayerFlyier = false;

    protected float[] draftDeltaSpeedList;
    protected float[] draftAccelerationList;

    protected float waitForDraftDuration = 0.0f;
    protected float waitForDraftTimer = -1.0f;
    #endregion

    public bool SlipstreamFlyierCanBeShown
    {
        get { return readyForPlayerFlyier; }
        set { readyForPlayerFlyier = value; }
    }

    public float DraftTimer
    {
        get { return endDraftTimer; }
    }

    public bool IsInDraft
    {
        get { return isInDraft; }
    }

    public bool DraftUSAActive
    {
        get { return isInUSADraft; }
    }

    public float currentDeltaSpeedAdvanced
    {
        get
        {
            int comboIndex = uiSharedData.ScoreDistanceMultiplier - 1;
            if (comboIndex < 0) comboIndex = 0;
            float currentDeltaSpeed = draftDeltaSpeed;
            if (draftDeltaSpeedList!=null)
                currentDeltaSpeed = comboIndex < draftDeltaSpeedList.Length ? draftDeltaSpeedList[comboIndex] : draftDeltaSpeedList[draftDeltaSpeedList.Length - 1];
            return currentDeltaSpeed; 
        }
    }

    public float currentAccelerationAdvanced
    {
        get
        {
            int comboIndex = uiSharedData.ScoreDistanceMultiplier - 1;
            if (comboIndex < 0) comboIndex = 0;
            float currentAcceleration = draftAcceleration;
            if (draftAccelerationList != null)
            {
                currentAcceleration = comboIndex < draftAccelerationList.Length ? draftAccelerationList[comboIndex] : draftAccelerationList[draftAccelerationList.Length - 1];
            }
            return currentAcceleration; 
        }
    }

    #region Unity Callbacks
    void Awake()
    {
		ignoreLayers |= (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Blocks")) | (1 << LayerMask.NameToLayer("Bonus")) | (1 << LayerMask.NameToLayer("Player"));

		//SetReferences();
	}

	void SetReferences()
	{
		gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        ingamePage = Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>();
        environmentManager = gameplayManager.GetComponent<OnTheRunEnvironment>();
    }

    void Start() 
    {
		SetReferences();

        isInDraft = false;
        isInUSADraft = false;
        uiSharedData = Manager<UIRoot>.Get().GetComponent<UISharedData>();
        kinematics = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerKinematics>();

        draftDuration = OnTheRunDataLoader.Instance.GetSlipstreamData("duration");
        forwardDistance = OnTheRunDataLoader.Instance.GetSlipstreamData("distance");

        if (OnTheRunDataLoader.Instance.HasSlipstreamData("wait_for_activation"))
            waitForDraftDuration = OnTheRunDataLoader.Instance.GetSlipstreamData("wait_for_activation");

        if (OnTheRunDataLoader.Instance.HasSlipstreamData("acceleration"))
            draftAcceleration = OnTheRunDataLoader.Instance.GetSlipstreamData("acceleration");

        if (OnTheRunDataLoader.Instance.HasSlipstreamData("deltaSpeed"))
            draftDeltaSpeed = OnTheRunDataLoader.Instance.GetSlipstreamData("deltaSpeed");

        if(OnTheRunDataLoader.Instance.SlipstreamAdvancedDataValid())
        {
            draftAccelerationList = OnTheRunDataLoader.Instance.GetSlipstreamAdvancedData("acceleration_list");
            draftDeltaSpeedList = OnTheRunDataLoader.Instance.GetSlipstreamAdvancedData("deltaSpeed_list");
        }

        draftStartSource = gameObject.AddComponent<AudioSource>();
        draftStartSource.volume = 0.9f;
        draftStartSource.rolloffMode = AudioRolloffMode.Linear;
        draftStartSource.minDistance = 1.0f;
        draftStartSource.maxDistance = 30.0f;
        draftStartSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
        draftStartSource.playOnAwake = false;

        draftStartSource.clip = draftStartSound;

        if (gameplayManager.IsSpecialCarActive && !kinematics.PlayerIsStopping && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Plane && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Ufo)
            ActivateDraftParticles(true);
        else
            ActivateDraftParticles(false);

        waitForDraftTimer = waitForDraftDuration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (gameplayManager == null || ingamePage == null || environmentManager == null)
            SetReferences();

        if (((!gameplayManager.Gameplaystarted && !kinematics.PlayerIsStopping) || gameplayManager.IsSpecialCarActive) && !OnTheRunTutorialManager.Instance.TutorialActive) // || gameplayManager.IsSpecialCarActive
        {
            if (isInDraft)
            {
                ingamePage.DestroyPlayerComboFlyier();
                ActivateDraftParticles(false);
                DeactivateSlipstreamEffect();
                LevelRoot.Instance.BroadcastMessage("OnDraftEnd");
                isInDraft = false;
                gameObject.GetComponent<PlayerKinematics>().DraftOn = false;
                lastHitCollider = null;
            }
            return;
        }

        if (!kinematics.PlayerIsDead && !kinematics.TurboOn && !gameplayManager.IsSpecialCarActive && !kinematics.IsOnCollisionEffect) //&& !kinematics.PlayerIsStopping) // && !gameplayManager.IsSpecialCarActive
        {
            RaycastHit hit;
            float checkDistance = kinematics.WrongDirection ? 30.0f : 3.0f;
            Vector3 startPos = gameObject.transform.position + Vector3.forward * checkDistance + Vector3.up * 1.0f;
            // Pietro
            Vector3 endPos = gameObject.transform.position + Vector3.forward * (OnTheRunDataLoader.ABTesting_Flag ? forwardDistance : 10.0f) + Vector3.up * 1.0f;
            //Debug.Log((OnTheRunDataLoader.ABTesting_Flag ? forwardDistance : 10.0f));
            //Physics.Raycast(startPos, endPos, out hit, forwardDistance); //to improve: layer for traffic ??
            //~(1 << LayerMask.NameToLayer("Ignore Raycast")));
            // Pietro
            Physics.Raycast(startPos, endPos, out hit, (OnTheRunDataLoader.ABTesting_Flag ? forwardDistance : 10.0f), ~ignoreLayers);

            if (hit.collider && lastHitCollider != hit.collider)
            {
                if (waitForDraftTimer >= 0.0f)
                    waitForDraftTimer -= dt;
                else
                {
                    if (!isInDraft)
                        ActivateDraftParticles(true);
                    isInDraft = true;
                    gameObject.GetComponent<PlayerKinematics>().DraftOn = true;
                    if (lastHitCollider != hit.collider)
                    {
                        LevelRoot.Instance.BroadcastMessage("OnDraft");
                        endDraftTimer = -1.0f;
                        ActivateSlipstreamEffect();
                        gameplayManager.UpdateRunParameter(OnTheRunGameplay.RunParameters.Slipstream, 1);
                        uiSharedData.IncreaseScoreMultiplier();
                        readyForPlayerFlyier = true;
                        lastHitCollider = hit.collider;
                        OnTheRunSoundsManager.Instance.PlaySource(draftStartSource);//.Play();
                    }
                }
            }
            else
            {
                if (isInDraft && endDraftTimer < 0.0f)
                    endDraftTimer = draftDuration;
                waitForDraftTimer = waitForDraftDuration;
            }

            if (endDraftTimer >= 0.0f)
            {
                endDraftTimer -= dt;
                if (endDraftTimer < 0.0f)
                {
                    endDraftTimer = -1.0f;
                    if (isInDraft)
                    {
                        waitForDraftTimer = waitForDraftDuration;
                        readyForPlayerFlyier = false;
                        ActivateDraftParticles(false);
                        LevelRoot.Instance.BroadcastMessage("OnDraftEnd");
                    }
                    uiSharedData.ResetScoreMultiplier();
                    isInDraft = false;
                    DeactivateSlipstreamEffect();
                    gameObject.GetComponent<PlayerKinematics>().DraftOn = false;
                    lastHitCollider = null;
                }
            }
        }
        else if (isInDraft)
        {
            LevelRoot.Instance.BroadcastMessage("OnDraftEnd");
            isInDraft = false;
            DeactivateSlipstreamEffect();
            gameObject.GetComponent<PlayerKinematics>().DraftOn = false;
            lastHitCollider = null;
        }

    }
    #endregion

    #region Functions
    void PalyerHasCollided()
    {
        endDraftTimer = -1.0f;
        readyForPlayerFlyier = false;
        ActivateDraftParticles(false);
        LevelRoot.Instance.BroadcastMessage("OnDraftEnd");
        uiSharedData.ResetScoreMultiplier();
        isInDraft = false;
        DeactivateSlipstreamEffect();
        gameObject.GetComponent<PlayerKinematics>().DraftOn = false;
        //lastHitCollider = null;
    }

    void ActivateDraftParticles(bool active)
    {
		if (gameplayManager == null)
			SetReferences();

        if (gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Plane && gameplayManager.CurrentSpecialCar != OnTheRunGameplay.CarId.Ufo)
        {
            if (readyForPlayerFlyier)
                readyForPlayerFlyier = false;
            gameObject.SendMessage("ActivateDraftFx", active);
        }
    }

    void OnTurboActive(bool active)
    {
        if (active)
            ActivateDraftParticles(false);
        else if (gameplayManager.IsSpecialCarActive)
            ActivateDraftParticles(true);

    }

    void OnSlipstreamActive(bool active)
    {
        if (active)
            ActivateDraftParticles(false);
    }

    void OnStartRunning()
    {
        ActivateDraftParticles(false);
        lastHitCollider = null;
    }

    void OnStartGame()
    {
        ActivateDraftParticles(false);
    }

    void OnTimeEnded()
    {
        ActivateDraftParticles(false);
    }

    void StartUSADraftAction()
    {
        if (!isInDraft)
        {
            ActivateDraftParticles(true);
            LevelRoot.Instance.BroadcastMessage("OnDraft");
            isInDraft = true;
            isInUSADraft = true;
            gameObject.GetComponent<PlayerKinematics>().DraftOn = true;
            OnTheRunSoundsManager.Instance.PlaySource(draftStartSource);
            endDraftTimer = draftDuration * 1.2f;
        }
    }

    void StopUSADraftAction()
    {
        bool isInDraftCheck = ingamePage.IsComboBarActive();
        if (!isInDraftCheck)//isInDraft)
        {
            endDraftTimer = -1.0f;
            ActivateDraftParticles(false);
            LevelRoot.Instance.BroadcastMessage("OnDraftEnd");
            isInDraft = false;
            isInUSADraft = false;
            gameObject.GetComponent<PlayerKinematics>().DraftOn = false;
        }
    }
    #endregion

    #region Slipstream Effect
    protected int maxFXLevel = 3;
    protected int currentSlipsteamFXLevel = -1;
    protected float[] slipstreamFXMultipliers = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

    protected ParticleSystem slipstreamFX_type_1;
    protected ParticleSystem slipstreamFX_type_2;
    protected ParticleSystem slipstreamFX_type_3;
    protected ParticleSystem slipstreamFX_type_4;
    protected ParticleSystem slipstreamFX_type_5;
    protected ParticleSystem slipstreamFX_type_6;

    protected List<ParticleSystem> currentFxs = null; 

    protected float[] horizontalOffsetEU = { 0.65f, 0.65f, 0.65f, 0.65f, 0.65f };
    protected float[] horizontalOffsetAS = { 0.65f, 0.65f, 0.65f, 0.65f, 0.65f };
    protected float[] horizontalOffsetNY = { 0.65f, 0.65f, 0.65f, 0.65f, 0.65f };
    protected float[] horizontalOffsetUS = { 0.65f, 0.65f, 0.65f, 0.65f, 0.65f };

    #region Initializations
    protected void InitializeEffects()
    {
        currentFxs = new List<ParticleSystem>();
        currentSlipsteamFXLevel = -1;
        InitializeSlipstreamEffect(out slipstreamFX_type_1, gameplayManager.slipstreamFX_1, "RearSlipstream_pos_1");
        InitializeSlipstreamEffect(out slipstreamFX_type_2, gameplayManager.slipstreamFX_2, "RearSlipstream_pos_1");
        InitializeSlipstreamEffect(out slipstreamFX_type_3, gameplayManager.slipstreamFX_3, "RearSlipstream_pos_1");
        InitializeSlipstreamEffect(out slipstreamFX_type_4, gameplayManager.slipstreamFX_4, "RearSlipstream_pos_2");
        InitializeSlipstreamEffect(out slipstreamFX_type_5, gameplayManager.slipstreamFX_5, "RearSlipstream_pos_1");
        InitializeSlipstreamEffect(out slipstreamFX_type_6, gameplayManager.slipstreamFX_6, "RearSlipstream_pos_2");
    }

    public void DeactivateSlipstreamEffect()
    {
        if (gameObject.transform.Find("RearSlipstream_pos_1") == null || currentFxs == null)
            return;

        currentSlipsteamFXLevel = -1;

        ClearCurrentFxs();
    }

    protected void InitializeSlipstreamEffect(out ParticleSystem effect, ParticleSystem effectRef, string attachNodeName)
    {
        Transform parentToAttach = gameObject.transform.Find(attachNodeName).transform;
        effect = EffectAlredyCreated(parentToAttach, effectRef);
        if (effect == null)
            effect = (ParticleSystem)Instantiate(effectRef, Vector3.zero, effectRef.transform.rotation);
        effect.transform.parent = parentToAttach;
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = effectRef.transform.localRotation;
        effect.Stop(true);

        ChangeHorizontalOffset(effect);
    }
    #endregion

    public void ActivateSlipstreamEffect()
    {
        if (gameObject.transform.Find("RearSlipstream_pos_1") == null)
            return;

        if (currentFxs == null)
            InitializeEffects();

        currentSlipsteamFXLevel = Mathf.Clamp(++currentSlipsteamFXLevel, 0, uiSharedData.MaxComboIndex-1);

        ClearCurrentFxs();

        switch (currentSlipsteamFXLevel)
        {
            case 0:
                AddCurrentFxs(slipstreamFX_type_1, 1);
                break;
            case 1:
                AddCurrentFxs(slipstreamFX_type_1, 2);
                break;
            case 2:
                AddCurrentFxs(slipstreamFX_type_1, 2);
                AddCurrentFxs(slipstreamFX_type_2, 1);
                break;
            case 3:
                AddCurrentFxs(slipstreamFX_type_5, 1);
                break;
            case 4:
                AddCurrentFxs(slipstreamFX_type_5, 2);
                break;
            case 5:
                AddCurrentFxs(slipstreamFX_type_3, 1);
                break;
            case 6:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                break;
            case 7:
                AddCurrentFxs(slipstreamFX_type_4, 1);
                break;
            case 8:
                AddCurrentFxs(slipstreamFX_type_6, 1);
                break;
            case 9:
                AddCurrentFxs(slipstreamFX_type_1, 2);
                AddCurrentFxs(slipstreamFX_type_2, 1);
                AddCurrentFxs(slipstreamFX_type_6, 1);
                break;
            case 10:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                AddCurrentFxs(slipstreamFX_type_4, 1);
                break;
            case 11:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                AddCurrentFxs(slipstreamFX_type_4, 1);
                //ChangeColorBySlipstreamLevel(0);
                break;
            case 12:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                AddCurrentFxs(slipstreamFX_type_4, 1);
                //ChangeColorBySlipstreamLevel(1);
                break;
            case 13:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                AddCurrentFxs(slipstreamFX_type_4, 1);
                //ChangeColorBySlipstreamLevel(2);
                break;
            case 14:
                AddCurrentFxs(slipstreamFX_type_3, 2);
                AddCurrentFxs(slipstreamFX_type_4, 1);
                //ChangeColorBySlipstreamLevel(3);
                break;
        }
    }

    #region Current Active FX
    public void ClearCurrentFxs()
    {
        for (int i = 0; i < currentFxs.Count; ++i)
        {
            ResetSlipstreamEffect(currentFxs[i]);
        }
        currentFxs.Clear();
    }

    public void AddCurrentFxs(ParticleSystem effect, int level=1)
    {
        ChangeParticleEffectByLevel(effect, level);
        currentFxs.Add(effect);
        effect.Play(true);
    }
    #endregion
    
    #region Particle System Modifiers
    protected Color currentColor = Color.white;
    protected Color[] colorsPerLevel = { Color.white, 
                                         new Color(255.0f / 255.0f, 181.0f / 255.0f, 0.0f / 255.0f), //yellow
                                         new Color(255.0f / 255.0f, 75.0f / 255.0f, 0.0f / 255.0f), //orange
                                         new Color(255.0f / 255.0f, 11.0f / 255.0f, 0.0f / 255.0f), //red
                                         new Color(255.0f / 255.0f, 0.0f / 255.0f, 248.0f / 255.0f), //fucsia
                                         new Color(40.0f / 255.0f, 0.0f / 255.0f, 255.0f / 255.0f), //purple
                                         new Color(0.0f / 255.0f, 23.0f / 255.0f, 255.0f / 255.0f), //blu
                                         new Color(0.0f / 255.0f, 97.0f / 255.0f, 255.0f / 255.0f), //azure
                                         new Color(40.0f / 255.0f, 255.0f / 255.0f, 0.0f / 255.0f), //green
                                         new Color(255.0f / 255.0f, 181.0f / 255.0f, 0.0f / 255.0f), //yellow
                                         new Color(255.0f / 255.0f, 75.0f / 255.0f, 0.0f / 255.0f), //orange
                                         new Color(255.0f / 255.0f, 11.0f / 255.0f, 0.0f / 255.0f) //red
                                         };

    void ChangeColorBySlipstreamLevel(int colorIndex)
    {
        currentColor = colorsPerLevel[colorIndex];
        
        for (int i = 0; i < currentFxs.Count; ++i)
        {
            if (currentFxs[i].gameObject.transform.FindChild("fx_1") != null)
            {
                currentFxs[i].gameObject.transform.FindChild("fx_1").GetComponent<ParticleSystem>().startColor = currentColor;
                currentFxs[i].gameObject.transform.FindChild("fx_2").GetComponent<ParticleSystem>().startColor = currentColor;
                if (currentFxs[i] == slipstreamFX_type_3)
                {
                    currentFxs[i].gameObject.transform.FindChild("fx_1/Fx_Blue_Strobo").GetComponent<ParticleSystem>().startColor = currentColor;
                    currentFxs[i].gameObject.transform.FindChild("fx_2/Fx_Blue_Strobo").GetComponent<ParticleSystem>().startColor = currentColor;
                    currentFxs[i].gameObject.transform.FindChild("fx_1/Fx_Iceball").GetComponent<ParticleSystem>().startColor = currentColor;
                    currentFxs[i].gameObject.transform.FindChild("fx_2/Fx_Iceball").GetComponent<ParticleSystem>().startColor = currentColor;
                }
            }
            else
            {
                currentFxs[i].gameObject.GetComponent<ParticleSystem>().startColor = currentColor;
                currentFxs[i].gameObject.GetComponent<ParticleSystem>().startColor = currentColor;
            }
        }

        /*int level = slipstreamFXLevels[currentSlipsteamFXLevel];
        int cumulativeLevel = 0;
        for (int i = 0; i < slipstreamFXLevels.Length; ++i)
        {
            if (slipstreamFXLevels[i] >= 0)
                cumulativeLevel += slipstreamFXLevels[i]+1;
        }

        if (cumulativeLevel <= colorsPerLevel.Length)
            currentColor = colorsPerLevel[cumulativeLevel - 1];

        if (cumulativeLevel>0)
        {
            slipstreamFX_base.gameObject.transform.FindChild("fx_1").GetComponent<ParticleSystem>().startColor = currentColor;
            slipstreamFX_base.gameObject.transform.FindChild("fx_2").GetComponent<ParticleSystem>().startColor = currentColor;
        }
        if (cumulativeLevel>3)
        {
            slipstreamFX_medium.startColor = currentColor;
            slipstreamFX_medium.gameObject.transform.FindChild("rays").GetComponent<ParticleSystem>().startColor = currentColor;
        }
        if (cumulativeLevel > 6)
        {
            slipstreamFX_strong.gameObject.transform.FindChild("fx_1").GetComponent<ParticleSystem>().startColor = currentColor;
            slipstreamFX_strong.gameObject.transform.FindChild("fx_2").GetComponent<ParticleSystem>().startColor = currentColor;
        }*/
    }

    void ChangeHorizontalOffset(ParticleSystem effect)
    {
        if (effect.name.StartsWith("Fx_Slipstream1"))
        {
            float[] horOffset = { 0, 0, 0, 0, 0 };
            switch (environmentManager.currentEnvironment)
            {
                case OnTheRunEnvironment.Environments.Europe:
                    horizontalOffsetEU.CopyTo(horOffset, 0);
                    break;
                case OnTheRunEnvironment.Environments.Asia:
                    horizontalOffsetAS.CopyTo(horOffset, 0);
                    break;
                case OnTheRunEnvironment.Environments.NY:
                    horizontalOffsetNY.CopyTo(horOffset, 0);
                    break;
                case OnTheRunEnvironment.Environments.USA:
                    horizontalOffsetUS.CopyTo(horOffset, 0);
                    break;
            }

            Transform currFx = effect.gameObject.transform.FindChild("fx_1").transform;
            currFx.localPosition = new Vector3(horOffset[0], currFx.localPosition.y, currFx.localPosition.z);
            currFx = effect.gameObject.transform.FindChild("fx_2").transform;
            currFx.localPosition = new Vector3(-horOffset[0], currFx.localPosition.y, currFx.localPosition.z);
        }
    }
    
    protected void ChangeParticleEffectByLevel(ParticleSystem effect, int level)
    {
        ParticleSystem[] objects = effect.GetComponentsInChildren<ParticleSystem>(true);

        if (effect == slipstreamFX_type_1)
        {
            slipstreamFXMultipliers[0] = level * 1.3f;
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].startSpeed *= slipstreamFXMultipliers[0];
                objects[i].startSize *= slipstreamFXMultipliers[0];
            }
        }
        else if (effect == slipstreamFX_type_2)
        {
            slipstreamFXMultipliers[1] = level * 2.6f;
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].startSpeed *= slipstreamFXMultipliers[1];
                objects[i].startSize *= slipstreamFXMultipliers[1];
            }
        }
        else if (effect == slipstreamFX_type_3)
        {
            if(level==2)
                slipstreamFXMultipliers[2] = 1.3f;
            for (int i = 0; i < objects.Length; ++i)
            {
                //objects[i].startSpeed *= slipstreamFXMultipliers[2];
                objects[i].startSize *= slipstreamFXMultipliers[2];
            }
        }
        else if (effect == slipstreamFX_type_5)
        {
            if (level == 2)
                slipstreamFXMultipliers[4] = 1.3f;
            for (int i = 0; i < objects.Length; ++i)
            {
                objects[i].startSpeed *= slipstreamFXMultipliers[4];
                objects[i].startSize *= slipstreamFXMultipliers[4];
            }
        }
    }

    protected void ResetSlipstreamEffect(ParticleSystem effect)
    {
        if (effect != null && effect.isPlaying)
        {
            effect.Clear();
            effect.Stop(true);
        }

        if (effect != null)
        {
            ParticleSystem[] objects = effect.GetComponentsInChildren<ParticleSystem>(true);
            if (effect == slipstreamFX_type_1)
            {
                for (int i = 0; i < objects.Length; ++i)
                {
                    objects[i].startSpeed /= slipstreamFXMultipliers[0];
                    objects[i].startSize /= slipstreamFXMultipliers[0];
                }
            }
            else if (effect == slipstreamFX_type_2)
            {
                for (int i = 0; i < objects.Length; ++i)
                {
                    objects[i].startSpeed /= slipstreamFXMultipliers[1];
                    objects[i].startSize /= slipstreamFXMultipliers[1];
                }
            }
            else if (effect == slipstreamFX_type_3)
            {
                for (int i = 0; i < objects.Length; ++i)
                {
                    //objects[i].startSpeed /= slipstreamFXMultipliers[2];
                    objects[i].startSize /= slipstreamFXMultipliers[2];
                }
            }
            else if (effect == slipstreamFX_type_5)
            {
                for (int i = 0; i < objects.Length; ++i)
                {
                    objects[i].startSpeed /= slipstreamFXMultipliers[4];
                    objects[i].startSize /= slipstreamFXMultipliers[4];
                }
            }
        }
    }
    #endregion


    ParticleSystem EffectAlredyCreated(Transform parentToAttach, ParticleSystem effectRef)
    {
        ParticleSystem retValue = null;
        for (int i = 0; i < parentToAttach.childCount; ++i)
        {
            Transform currChild = parentToAttach.GetChild(i);
            if (currChild.name.StartsWith(effectRef.name))
            {
                retValue = currChild.GetComponent<ParticleSystem>();
                break;
            }
        }

        return retValue;
    }
    #endregion
}
