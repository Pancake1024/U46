using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFx : MonoBehaviour
{
    #region Public Members
    //public TrailRenderer[] trails;
    public GameObject shadow;
    public ParticleSystem TurboExplosionRef;
    public ParticleSystem RearTurboBoostRef;
    public ParticleSystem FrontTurboBoostRef;
    public ParticleSystem RearDraftRef;
    public ParticleSystem FrontDraftRef;
    public ParticleSystem SlipStreamRef;
    public ParticleSystem LandExplosionRef;
    public ParticleSystem DirtDustRef;
	public GameObject ShieldRef,BrokenShieldRef;
    public GameObject MagnetFxRef;
    public GameObject[] blinkingParts;
    #endregion

    #region Protected Members
    protected GameObject playerCar;
    protected PlayerKinematics kinematics;
    protected bool trailsActive = false;
    protected bool lightsActive = false;

    protected ParticleSystem TurboExplosion;
    protected ParticleSystem RearTurboBoost;
    protected ParticleSystem FrontTurboBoost;
    protected ParticleSystem RearDraft;
    protected ParticleSystem FrontDraft;
    protected ParticleSystem SlipStream;
    protected ParticleSystem LandExplosion;
    protected ParticleSystem dirtDust;
	protected GameObject Shield,BrokenShield;
    protected GameObject MagnetFx;
    protected OnTheRunGameplay gameplayManager;
    protected AudioSource dirtLoopSound;

    protected List<GameObject> tunnelLights;
    protected bool blinkingActive = false;
    #endregion

    #region Public Properties
    #endregion

    #region Functions
    bool EffectAlredyCreated(Transform parentToAttach, ParticleSystem effectRef)
    {
        bool retValue = false;
        for (int i = 0; i < parentToAttach.childCount; ++i)
        {
            Transform currChild = parentToAttach.GetChild(i);
            if (currChild.name.Equals(effectRef.name))
            {
                retValue = true;
                break;
            }
        }

        return retValue;
    }

    bool EffectAlredyCreated(Transform parentToAttach, GameObject effectRef)
    {
        bool retValue = false;
        for (int i = 0; i < parentToAttach.childCount; ++i)
        {
            Transform currChild = parentToAttach.GetChild(i);
            if (currChild.name.Equals(effectRef.name))
            {
                retValue = true;
                break;
            }
        }

        return retValue;
    }

    Transform GetExistingEffect(Transform parentToAttach, string effectRefName)
    {
        Transform retValue = parentToAttach.GetChild(0).transform;
        for (int i = 0; i < parentToAttach.childCount; ++i)
        {
            Transform currChild = parentToAttach.GetChild(i);
            if (currChild.name.Equals(effectRefName))
            {
                retValue = currChild.transform;
                break;
            }
        }

        return retValue;
    }

    void InitializeEffect(out ParticleSystem effect, ParticleSystem effectRef, string placeHolderName, bool attachToPlayer = true)
    {
        Transform parentToAttach = gameObject.transform.Find(placeHolderName).transform;
        if (!attachToPlayer || !EffectAlredyCreated(parentToAttach, effectRef))
        {
            effect = (ParticleSystem)Instantiate(effectRef, Vector3.zero, effectRef.transform.rotation);
            if (attachToPlayer)
                effect.transform.parent = parentToAttach;
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = effectRef.transform.localRotation;
            effect.transform.name = effectRef.name;
        }
        else
            effect = GetExistingEffect(parentToAttach, effectRef.name).GetComponent<ParticleSystem>();
    }

    void InitializeEffect(out GameObject effect, GameObject effectRef, string placeHolderName, bool attachToPlayer = true)
    {
        Transform parentToAttach = gameObject.transform.Find(placeHolderName).transform;
        if (!attachToPlayer || !EffectAlredyCreated(parentToAttach, effectRef))
        {
            effect = (GameObject)Instantiate(effectRef, Vector3.zero, effectRef.transform.rotation);
            if (attachToPlayer)
                effect.transform.parent = gameObject.transform.Find(placeHolderName).transform;
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = effectRef.transform.localRotation;
            effect.transform.name = effectRef.name;
        }
        else
            effect = GetExistingEffect(parentToAttach, effectRef.name).gameObject;
    }

    private void SetupShieldSize()
    {
        OnTheRunGameplay.CarId id = gameObject.GetComponent<PlayerKinematics>().carId;

        // DEFAULT SIZE
        BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.one;// new Vector3(1.1f, 1.1f, 1.1f);

        // RED CARS 
        if (id == OnTheRunGameplay.CarId.Car_1_muscle ||
           id == OnTheRunGameplay.CarId.Car_2_muscle ||
           id == OnTheRunGameplay.CarId.Car_3_muscle ||
           id == OnTheRunGameplay.CarId.Car_4_muscle ||
           id == OnTheRunGameplay.CarId.Car_5_muscle)
        {
            //BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 1.3f,1.3f,1.3f );
        }
        else if (id == OnTheRunGameplay.CarId.Bigfoot)
        {
            BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.zero;
            //			BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 1.34f,1.63f,1.24f );
        }
        else if (id == OnTheRunGameplay.CarId.Firetruck)
        {
            BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.zero;
            //			BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 1.48f,1.99f,1.51f );
        }
        else if (id == OnTheRunGameplay.CarId.Plane)
        {
            BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.zero;
            //			BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 2.28f,1.23f,1.62f );
        }
        else if (id == OnTheRunGameplay.CarId.Tank)
        {
            BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.zero;
            //			BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 1.69f,1.55f,1.62f );
        }
        else if (id == OnTheRunGameplay.CarId.Ufo)
        {
            BrokenShield.transform.localScale = Shield.transform.localScale = Vector3.zero;
            //			BrokenShield.transform.localScale = Shield.transform.localScale = new Vector3( 1.75f,1.04f,0.98f );
        }
    }
    #endregion

    #region Messages    
    void ActivateBlink(bool forceVisible=false)
    {
        if (blinkingActive || forceVisible)
        {
            blinkingActive = false;
            //playerCar.SetActive(true);
            //shadow.SetActive(true);
            foreach (GameObject go in blinkingParts)
            {
                if (go != null)
                    go.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        else
        {
            blinkingActive = true;
            //playerCar.SetActive(false);
            //shadow.SetActive(false);
            foreach (GameObject go in blinkingParts)
            {
                if (go != null)
                    go.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

	static GameObject CameraTurboFX = null;

    void ActivateTurboFx(bool active)
    {
        FrontTurboBoost.gameObject.SetActive(active);
        RearTurboBoost.gameObject.SetActive(active);
		
		if( CameraTurboFX != null )
			CameraTurboFX.SetActive( active );
    }

    void ActivateDraftFx(bool active)
    {
        FrontDraft.gameObject.SetActive(active);
        RearDraft.gameObject.SetActive(active);
    }
   
    void ActivateSlipstreamFx(bool active)
    {
        SlipStream.gameObject.SetActive(active);
    }

    void ActivateAsphaltCrackFx( )
    {
        OnTheRunObjectsPool.Instance.RequestEffect(gameObject.transform, gameObject.transform.forward * 2.0f + Vector3.up * 0.2f, OnTheRunObjectsPool.ObjectType.AsphaltCrack, false);
    }

    void ActivateShieldFx(bool active)
    {
        Shield.SetActive(active);
        if(active)
            Shield.GetComponentsInChildren<ParticleSystem>(true)[0].Play(true);
        else
            Shield.GetComponentsInChildren<ParticleSystem>(true)[0].Stop(true);

		BrokenShield.SetActive(active == false);

		SetupShieldSize();

    }

    void ActivateMagnetFx(bool active)
    {
        MagnetFx.transform.localPosition = Vector3.zero + Vector3.up * 0.1f;
        MagnetFx.SetActive(active);
    }

    void ActivateDirtDust(bool active)
    {
        if (dirtDust != null)
        {

            if (active)
            {
                if (dirtLoopSound != null)
                    OnTheRunSoundsManager.Instance.PlaySource(dirtLoopSound); //dirtLoopSound.Play();
                dirtDust.Clear();
                dirtDust.Play();
            }
            else
            {
                if (dirtLoopSound != null)
                    OnTheRunSoundsManager.Instance.StopSource(dirtLoopSound); //dirtLoopSound.Stop();
                dirtDust.Stop();
            }
        }
    }
    
    void OnPlayerLand( )
    {
        ActivateAsphaltCrackFx( );
        LandExplosion.Play();
    }

    void OnTurboActive(bool active)
    {
        if (active)
            TurboExplosion.Play();
    }

    void OnSlipstreamActive(bool active)
    {
        if (active)
            SlipStream.Play();
    }

    void OnChangePlayerEffect()
    {
        TurboExplosion.Play();
    }
    
    void PlayerIsDead()
    {
        //shadow.SetActiveRecursively(false);
        //Smoke.Play();
    }

    void OnStartGame()
    {
        if (shadow!=null)
            shadow.SetActive(true);
    }

    void OnActivateLights(bool active)
    {
        lightsActive = active;
        if (tunnelLights!=null)
        {
            foreach (GameObject light in tunnelLights)
                light.SetActive(active);
        }
    }
    #endregion

    #region Unity Callback
    void Awake()
    {
        InitializeEffect(out TurboExplosion, TurboExplosionRef, "TurboExplosion_pos");
        InitializeEffect(out RearTurboBoost, RearTurboBoostRef, "RearTurbo_pos");
        InitializeEffect(out FrontTurboBoost, FrontTurboBoostRef, "FrontTurbo_pos");
        InitializeEffect(out RearDraft, RearDraftRef, "RearDraft_pos");
        InitializeEffect(out FrontDraft, FrontDraftRef, "FrontDraft_pos");
        InitializeEffect(out SlipStream, SlipStreamRef, "SlipStream_pos");
        InitializeEffect(out Shield, ShieldRef, "Shield_pos");
		InitializeEffect(out BrokenShield, BrokenShieldRef, "PlayerCar");
        InitializeEffect(out LandExplosion, LandExplosionRef, "PlayerCar");
        InitializeEffect(out MagnetFx, MagnetFxRef, "PlayerCar");
	}

	void Init()
	{
		if( CameraTurboFX == null )
		CameraTurboFX = GameObject.Find("CameraTurboFX").gameObject;

        if (DirtDustRef != null)
            InitializeEffect(out dirtDust, DirtDustRef, "DirtDust_pos");

        dirtLoopSound = gameObject.GetComponent<AudioSource>();
        if (dirtLoopSound!=null)
            OnTheRunSoundsManager.Instance.StopSource(dirtLoopSound); //dirtLoopSound.Stop();
    }

    void Start() 
    {
		Init();

        playerCar = gameObject.transform.FindChild("PlayerCar").gameObject;
        kinematics = gameObject.GetComponent<PlayerKinematics>();
        gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
        
        /*
        foreach (TrailRenderer tr in trails)
        {
            tr.enabled = false;
        }
        */

        ActivateTurboFx(false);
        ActivateDraftFx(false);
        ActivateSlipstreamFx(false);
        ActivateMagnetFx(false);
        ActivateDirtDust(false);
        //if (!gameplayManager.Gameplaystarted)
            ActivateShieldFx(false);

		BrokenShield.SetActive( false );

        tunnelLights = new List<GameObject>();//GameObject.FindGameObjectsWithTag("TunnelLights");
        foreach(Transform child in transform)
        {
            if (child.gameObject.tag == "TunnelLights")
                tunnelLights.Add(child.gameObject);
        }
        
        OnActivateLights(false);
        string page = Manager<UIManager>.Get().ActivePageName;
        if (gameplayManager.GetComponent<OnTheRunEnvironment>().currentEnvironment == OnTheRunEnvironment.Environments.Asia && (page == "InAppPage" || page == "IngamePage") && !gameplayManager.IsSpecialCar(kinematics.carId))
        {
            OnActivateLights(true);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(lightsActive && !tunnelLights[0].activeInHierarchy)
            OnActivateLights(true);

        if (dirtDust != null)
        {
            if (dirtDust.gameObject.activeInHierarchy)
            {
                if (kinematics.Speed < 1.0f)
                    ActivateDirtDust(false);
                else if (!dirtDust.isPlaying && kinematics.IsOnDirt)
                    ActivateDirtDust(true);
            }
        }
/*
        if (LandExplosion)
            InitializeEffect(out LandExplosion, LandExplosionRef, "PlayerCar");
*/
        /*
        if (kinematics.Speed > 0.0f && !trailsActive && !kinematics.PlayerIsDead)
        {
            trailsActive = true;
            foreach (TrailRenderer tr in trails)
            {
                tr.enabled = true;
            }
        }

        if (trailsActive && kinematics.PlayerIsDead)
        {
            trailsActive = false;
            foreach (TrailRenderer tr in trails)
            {
                tr.enabled = false;
            }
        }

        if (trailsActive && kinematics.Speed > 0.0f)
        {
            //Debug.Log("speed = " + kinematics.Speed * dt + "trails.time = " + trails[0].time );
            foreach (TrailRenderer tr in trails)
            {
                //tr.time = Mathf.Clamp((kinematics.Speed * 0.05f *dt), 0f, 0.03f); // X GIAMBA: codice SBAGLIATO
                tr.time = Mathf.Clamp((kinematics.Speed * 0.05f * 0.016f), 0.0f, 0.03f);
            }
        }
        */
    }
    #endregion

}

