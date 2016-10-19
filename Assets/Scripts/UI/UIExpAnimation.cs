using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIExpAnimation")]
public class UIExpAnimation: MonoBehaviour
{
    public enum FloatingObjectsUpdateTypes
    {
        LERP,
        SLERP,
        MOVE_TOWARDS
    }
    public FloatingObjectsUpdateTypes FloatingObjectsUpdateType = FloatingObjectsUpdateTypes.LERP;

    protected OnTheRunGameplay gameplayManager;
    protected OnTheRunInterfaceSounds interfaceSounds;

    private GameObject[] m_aCoins = null;
    private GameObject[] m_aFuels = null;
    private GameObject[] m_aStars = null;
    private GameObject[] m_aStars_fake = null;
    private GameObject[] m_aDiamonds = null;
    private Animation m_pMetersAnim = null;
    private GameObject m_pPlaceholderMeters = null;
    private GameObject m_pPlaceholderFake = null;
    private GameObject m_pPlaceholderFuels = null;
    private GameObject m_pPlaceholderCoins = null;
    private GameObject m_pPlaceholderDiamonds = null;
    private UITextField m_pCoinsText = null;
    private UITextField m_pMetersText = null;
    private UITextField m_pDiamondsText = null;
    private UITextField m_pCoinsCollectedText = null;
    private Animation m_pFuelsAnim = null;
    private Animation m_pCoinsAnim = null;
    private Animation m_pDiamondsAnim = null;
    private UIRoot m_pUIRoot = null;
    //---------------------------------//
    private int m_iDiamondsCollected = 0;
    private int m_iDiamondsToCollect = 0;
    private int m_iMetersCollected = 0;
    private int m_iMetersToCollect = 0;
    private int m_iFakeExpCollected = 0;
    private int m_iFakeExpToCollect = 0;
    private int m_iCoinsCollected = 0;
    private int m_iCoinsToCollect = 0;
    private int m_iFuelsCollected = 0;
    private int m_iFuelsToCollect = 0;
    private int m_iOldPlayerCoins = 0;
    private int m_iOldPlayerDiamonds = 0;
    private int m_finalExperienceValue = 0;
    //---------------------------------//
    private float m_fUpdateTimer = 0f;
    private float m_fGeneralTimer = 0f;
    //---------------------------------//

    // use these only to tweak values
    private static readonly int POOL_SIZE = 10;
    public float UPDATE_TIMEOUT = 0.06f;
	public float EXP_COEFF = 0.03f;
    public float FUELS_COEFF = 1.0f;
	public float COINS_COEFF = 0.02f;
    public float FLOATING_FUELS_SPEED = 12f;
    public float FLOATING_COINS_SPEED = 11f;
    public float FLOATING_STARS_SPEED = 10f;
    public float SPAWN_OFFSET = 0.4f;
    public float FUELS_ALPHA = 1.0f;
    public float COINS_ALPHA = 0.7f;
    public float STARS_ALPHA = 0.8f;
    public float Delay = 0.4f;

    protected bool pauseAnimations = false;
    protected bool metersAnimationEnded = false;
    protected bool updateTextfield = false;
    protected bool meterAnim = false;

    void OnEnable()
    {
        if (gameplayManager == null)
            gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        if (interfaceSounds == null)
            interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        if (m_pUIRoot == null)
            m_pUIRoot = Manager<UIRoot>.Get();

        if (m_pFuelsAnim == null)
            m_pFuelsAnim = m_pUIRoot.transform.FindChild("CurrencyBar/FuelItem/FuelNormal/fuel").gameObject.GetComponent<Animation>();

        if (m_pCoinsAnim == null)
        {
            Transform coinAnimTr = m_pUIRoot.transform.FindChild("CurrencyBar/CoinsItem/GreenButtonSquare/Normal/coin");
            if(coinAnimTr == null)
                coinAnimTr = m_pUIRoot.transform.FindChild("CurrencyBar/CoinsItem/coin");
            m_pCoinsAnim = coinAnimTr.gameObject.GetComponent<Animation>();
        }

        if (m_pMetersAnim == null)
            m_pMetersAnim = m_pUIRoot.transform.FindChild("TopLeftObjects/LevelBar/star_fb_user").gameObject.GetComponent<Animation>();

        if (m_pDiamondsAnim == null)
            m_pDiamondsAnim = m_pUIRoot.transform.FindChild("CurrencyBar/DiamondsItem/GreenButtonSquare/Normal/diamond").gameObject.GetComponent<Animation>();
        
        if (m_pCoinsText == null)
            m_pCoinsText = m_pUIRoot.transform.FindChild("CurrencyBar/CoinsItem/ValueTextField").gameObject.GetComponent<UITextField>();

		if( m_pMetersText == null )
//#if UNITY_WEBPLAYER
//			m_pMetersText = GameObject.Find("RewardPage").transform.FindChild("CenterScreenAnchorWeb/Meters/ValueTextField").gameObject.GetComponent<UITextField>();
//#else
			m_pMetersText = GameObject.Find("RewardPage").transform.FindChild("CenterScreenAnchor/AnimAnchor/Meters/ValueTextField").gameObject.GetComponent<UITextField>();
//#endif

        if (m_pDiamondsText == null)
            m_pDiamondsText = m_pUIRoot.transform.FindChild("CurrencyBar/DiamondsItem/ValueTextField").gameObject.GetComponent<UITextField>();
        
		if( m_pCoinsCollectedText == null )
//#if UNITY_WEBPLAYER
//            m_pCoinsCollectedText = GameObject.Find("RewardPage").transform.FindChild("CenterScreenAnchorWeb/Coins/ValueTextField").gameObject.GetComponent<UITextField>();
//#else
            m_pCoinsCollectedText = GameObject.Find("RewardPage").transform.FindChild("CenterScreenAnchor/AnimAnchor/Coins/ValueTextField").gameObject.GetComponent<UITextField>();
//#endif

        m_pCoinsCollectedText.text = "0";
        if(updateTextfield)
		    m_pMetersText.text = "0";

        if (m_aCoins == null)
            CreatePools();
    }
    //-------------------------------------------------------//
    void AddFloatingFakeStar()
    {
        GameObject pNewStar = GetValidFakeStar();

        if (pNewStar == null)
            return;

        pNewStar.SetActive(true);

        pNewStar.transform.position = m_pPlaceholderFake.transform.position;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.up * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.down * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.right * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.left * SPAWN_OFFSET;

        pNewStar.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        pNewStar.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, STARS_ALPHA);
    }
	//-------------------------------------------------------//
    void AddFloatingStar()
    {
        GameObject pNewStar = GetValidStar();

        if (pNewStar == null)
            return;

        pNewStar.SetActive(true);

		pNewStar.transform.position = m_pPlaceholderMeters.transform.position;
		
		if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.up * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.down * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.right * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewStar.transform.position += Vector3.left * SPAWN_OFFSET;

        pNewStar.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        pNewStar.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, STARS_ALPHA);
    }
    //-------------------------------------------------------//
    void AddFloatingFuel()
    {
        GameObject pNewFuel = GetValidFuel();

        if (pNewFuel == null)
            return;

        pNewFuel.SetActive(true);

        pNewFuel.transform.position = m_pPlaceholderFuels.transform.position;

        if (Random.Range(0, 2) == 0)
            pNewFuel.transform.position += Vector3.up * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewFuel.transform.position += Vector3.down * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewFuel.transform.position += Vector3.right * SPAWN_OFFSET;

        //if (Random.Range(0, 2) == 0)
        //    pNewFuel.transform.position += Vector3.left * SPAWN_OFFSET;

        pNewFuel.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        pNewFuel.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, FUELS_ALPHA);
    }
    //-------------------------------------------------------//
    void AddFloatingCoin()
    {
        GameObject pNewCoin = GetValidCoin();

        if (pNewCoin == null)
            return;

        pNewCoin.SetActive(true);

        pNewCoin.transform.position = m_pPlaceholderCoins.transform.position;

        if (Random.Range(0, 2) == 0)
            pNewCoin.transform.position += Vector3.up * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewCoin.transform.position += Vector3.down * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewCoin.transform.position += Vector3.right * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewCoin.transform.position += Vector3.left * SPAWN_OFFSET;

        pNewCoin.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        pNewCoin.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, COINS_ALPHA);
    }
    //-------------------------------------------------------//
    void AddFloatingDiamonds()
    {
        GameObject pNewDiamond = GetValidDiamond();

        if (pNewDiamond == null)
            return;

        pNewDiamond.SetActive(true);

        pNewDiamond.transform.position = m_pPlaceholderDiamonds.transform.position;

        if (Random.Range(0, 2) == 0)
            pNewDiamond.transform.position += Vector3.up * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewDiamond.transform.position += Vector3.down * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewDiamond.transform.position += Vector3.right * SPAWN_OFFSET;

        if (Random.Range(0, 2) == 0)
            pNewDiamond.transform.position += Vector3.left * SPAWN_OFFSET;

        pNewDiamond.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
        pNewDiamond.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, COINS_ALPHA);
    }
    //-------------------------------------------------------//
	public GameObject CoinEffect;
	void AddCoinEffect( Vector3 kPos )
	{
		GameObject pEffect = Instantiate( CoinEffect , kPos , Quaternion.identity ) as GameObject;
		Destroy(pEffect, 1f);
	}
    //-------------------------------------------------------//
    void UpdateFloatingCoins()
    {
        if (m_aCoins == null)
            return;

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            GameObject kCoin = m_aCoins[i];

            if (kCoin.activeSelf == false)
                continue;

            if (Vector3.Distance(kCoin.transform.position, m_pCoinsAnim.transform.position) < 0.15f)
            {
                kCoin.SetActive(false);

                int iAdd = (int)(((float)m_iCoinsToCollect) * COINS_COEFF);
                //Debug.Log("############# (float)m_iCoinsToCollect): " + ((float)m_iCoinsToCollect).ToString() + " COINS_COEFF " + COINS_COEFF);
                //Debug.Log("§§§§§§§§§§§§§ iAdd: " + iAdd + " m_iCoinsToCollect : " + m_iCoinsToCollect);
                iAdd = Mathf.Clamp(iAdd, 1, m_iCoinsToCollect);
                m_iCoinsCollected += iAdd;
                //Debug.Log("@@@@@@@@@@@@@@ m_iCoinsCollected " + m_iCoinsCollected + " m_iCoinsToCollect " + m_iCoinsToCollect + " iAdd " + iAdd);
                m_iCoinsCollected = Mathf.Clamp(m_iCoinsCollected, 0, m_iCoinsToCollect);

                m_pCoinsText.text = m_pUIRoot.FormatTextNumber(m_iOldPlayerCoins + m_iCoinsCollected);

                // play the bounce animation
                m_pCoinsAnim.Rewind();
                m_pCoinsAnim.Play();
				AddCoinEffect(m_pCoinsAnim.transform.position);
                interfaceSounds.CounterCoin();
                continue;
            }

            // move the coins towards the coin in the upper right corner of the screen
            if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.LERP)
            {
                kCoin.transform.position = Vector3.Lerp(kCoin.transform.position, m_pCoinsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
                kCoin.transform.localScale = Vector3.Lerp(kCoin.transform.localScale, m_pCoinsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.SLERP)
            {
                kCoin.transform.position = Vector3.Slerp(kCoin.transform.position, m_pCoinsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
                kCoin.transform.localScale = Vector3.Slerp(kCoin.transform.localScale, m_pCoinsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.MOVE_TOWARDS)
            {
                kCoin.transform.position = Vector3.MoveTowards(kCoin.transform.position, m_pCoinsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
                kCoin.transform.localScale = Vector3.MoveTowards(kCoin.transform.localScale, m_pCoinsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_COINS_SPEED);
            }
        }
    }
    //-------------------------------------------------------//
    void UpdateFloatingFuels()
    {
        if (m_aFuels == null)
            return;

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            GameObject kFuel = m_aFuels[i];

            if (kFuel.activeSelf == false)
                continue;

            if (Vector3.Distance(kFuel.transform.position, m_pFuelsAnim.transform.position) < 0.15f)
            {
                kFuel.SetActive(false);

                int iAdd = (int)(((float)m_iFuelsToCollect) * FUELS_COEFF);

                iAdd = Mathf.Clamp(iAdd, 1, m_iFuelsToCollect);
                m_iFuelsCollected += iAdd;

                m_iFuelsCollected = Mathf.Clamp(m_iFuelsCollected, 0, m_iFuelsToCollect);
                Manager<UIRoot>.Get().transform.FindChild("CurrencyBar").GetComponent<UICurrencyBarItem>().SetFuelValue();
                // play the bounce animation
                m_pFuelsAnim.Rewind();
                m_pFuelsAnim.Play();
                interfaceSounds.CounterCoin();
                continue;
            }

            // move the coins towards the coin in the upper right corner of the screen
            if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.LERP)
            {
                kFuel.transform.position = Vector3.Lerp(kFuel.transform.position, m_pFuelsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
                kFuel.transform.localScale = Vector3.Lerp(kFuel.transform.localScale, m_pFuelsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.SLERP)
            {
                kFuel.transform.position = Vector3.Slerp(kFuel.transform.position, m_pFuelsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
                kFuel.transform.localScale = Vector3.Slerp(kFuel.transform.localScale, m_pFuelsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.MOVE_TOWARDS)
            {
                kFuel.transform.position = Vector3.MoveTowards(kFuel.transform.position, m_pFuelsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
                kFuel.transform.localScale = Vector3.MoveTowards(kFuel.transform.localScale, m_pFuelsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_FUELS_SPEED);
            }
        }
    }
    //-------------------------------------------------------//
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            m_fUpdateTimer = TimeManager.Instance.MasterSource.TotalTime;
            m_iMetersCollected = 0;
            m_iCoinsCollected = 0;
            m_iFuelsCollected = 0;
            m_iFakeExpCollected = 0;
        }

        if (pauseAnimations && m_iDiamondsToCollect!=1)
            return;

        float fNow = TimeManager.Instance.MasterSource.TotalTime;

		// delay 
		if (fNow - m_fGeneralTimer < Delay)
            return;

        UpdateFloatingFuels();
        UpdateFloatingCoins();
        UpdateFloatingStars();
        UpdateFloatingDiamonds();
        UpdateFloatingFakeStars();

        if (fNow - m_fUpdateTimer < UPDATE_TIMEOUT )
            return;

        m_fUpdateTimer = fNow;

        // Fake EXP
        if (m_iFakeExpToCollect > 0 && m_iFakeExpCollected < m_iFakeExpToCollect)
        {
            //m_pMetersText.text = m_pUIRoot.FormatTextNumber(m_iMetersCollected);
            AddFloatingFakeStar();
        }

        // Meters
        if (m_iMetersToCollect > 0 && m_iMetersCollected < m_iMetersToCollect)
        {
            if (updateTextfield)
		        m_pMetersText.text = m_pUIRoot.FormatTextNumber(m_iMetersCollected);
            AddFloatingStar();
        }

        // Fuels
        if (m_iFuelsToCollect > 0 && m_iFuelsCollected < m_iFuelsToCollect)
        {
            //Debug.Log("§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§ m_iCoinsCollected: " + m_iCoinsCollected);
            AddFloatingFuel();
        }

        // Coins
        if (m_iCoinsToCollect > 0 && m_iCoinsCollected < m_iCoinsToCollect)
        {
            m_pCoinsCollectedText.text = m_pUIRoot.FormatTextNumber(m_iCoinsCollected/*+1*/);
            //Debug.Log("§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§ m_iCoinsCollected: " + m_iCoinsCollected);
            AddFloatingCoin();
        }
        else if (m_iCoinsToCollect > 0)
        {
            m_pCoinsCollectedText.text = m_pUIRoot.FormatTextNumber(m_iCoinsToCollect/*+1*/);
        }

        // Diamonds
        if (m_iDiamondsToCollect > 0 && m_iDiamondsCollected < m_iDiamondsToCollect)
        {
            //m_pCoinsCollectedText.text = (m_iDiamondsCollected + 1).ToString();
            if (m_iDiamondsToCollect == 1)
                m_iDiamondsCollected = m_iDiamondsToCollect;
            AddFloatingDiamonds();
        }
    }
	//-------------------------------------------------------//
    void UpdateFloatingFakeStars()
    {
        if (m_aStars_fake == null)
            return;

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            GameObject kStar = m_aStars_fake[i];

            if (kStar.activeSelf == false)
                continue;

            if (Vector3.Distance(kStar.transform.position, m_pMetersAnim.transform.position) < 0.15f)
            {
                kStar.SetActive(false);

                int iAdd = (int)(dt * EXP_COEFF * m_iFakeExpToCollect); //(int)(((float)m_iMetersToCollect) * EXP_COEFF);
                iAdd = Mathf.Clamp(iAdd, 1, m_iFakeExpToCollect);

                m_iFakeExpCollected += iAdd;

                /*if (m_iMetersCollected <= m_iMetersToCollect)
                {
                    //do nothing
                }
                else if (m_finalExperienceValue > 0)
                {
                    //m_pUIRoot.SetExperienceBarValue(m_finalExperienceValue);
                    m_finalExperienceValue = 0;
                }*/

                m_iFakeExpCollected = Mathf.Clamp(m_iFakeExpCollected, 0, m_iFakeExpToCollect);
                
                // play the bounce animation
                /*m_pMetersAnim.Rewind();
                m_pMetersAnim.Play();
                interfaceSounds.CounterStar();*/
                continue;
            }

            // move the coins towards the coin in the upper right corner of the screen
            if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.LERP)
            {
                kStar.transform.position = Vector3.Lerp(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.Lerp(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.SLERP)
            {
                kStar.transform.position = Vector3.Slerp(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.Slerp(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.MOVE_TOWARDS)
            {
                kStar.transform.position = Vector3.MoveTowards(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.MoveTowards(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
        }
    }

    void UpdateFloatingStars()
    {
        if (m_aStars == null)
            return;

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            GameObject kStar = m_aStars[i];

            if (kStar.activeSelf == false)
                continue;

            if (Vector3.Distance(kStar.transform.position, m_pMetersAnim.transform.position) < 0.15f)
            {
                kStar.SetActive(false);

                int iAdd = (int)(dt * EXP_COEFF * m_iMetersToCollect); //(int)(((float)m_iMetersToCollect) * EXP_COEFF);
                iAdd = Mathf.Clamp(iAdd, 1, m_iMetersToCollect);

                m_iMetersCollected += iAdd;

                if (m_iMetersCollected <= m_iMetersToCollect)
                {
                    //do nothing
                }
                else if (m_finalExperienceValue > 0)
                {
                    //m_pUIRoot.SetExperienceBarValue(m_finalExperienceValue);
                    m_finalExperienceValue = 0;
                }

                m_iMetersCollected = Mathf.Clamp(m_iMetersCollected, 0, m_iMetersToCollect);
                if (updateTextfield)
                    m_pMetersText.text = m_pUIRoot.FormatTextNumber(m_iMetersCollected);

                if (Manager<UIManager>.Get().ActivePageName == "RewardPage" && m_iMetersCollected == m_iMetersToCollect && !metersAnimationEnded && updateTextfield)
                {
                    //Debug.Log("ASDASDASDas " + m_pMetersText.text + " " + m_iMetersToCollect);
                    metersAnimationEnded = true;
                    if (meterAnim)
                        Manager<UIManager>.Get().ActivePage.SendMessage("StartNewRecordAnimation");
                }

                // play the bounce animation
                m_pMetersAnim.Rewind();
                m_pMetersAnim.Play();
                interfaceSounds.CounterStar();
                continue;
            }

            // move the coins towards the coin in the upper right corner of the screen
            if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.LERP)
            {
                kStar.transform.position = Vector3.Lerp(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.Lerp(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.SLERP)
            {
                kStar.transform.position = Vector3.Slerp(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.Slerp(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.MOVE_TOWARDS)
            {
                kStar.transform.position = Vector3.MoveTowards(kStar.transform.position, m_pMetersAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kStar.transform.localScale = Vector3.MoveTowards(kStar.transform.localScale, m_pMetersAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
        }
    }

    void UpdateFloatingDiamonds()
    {
        if (m_aDiamonds == null)
            return;

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            GameObject kDiamond = m_aDiamonds[i];

            if (kDiamond.activeSelf == false)
                continue;

            if (Vector3.Distance(kDiamond.transform.position, m_pDiamondsAnim.transform.position) < 0.15f)
            {
                kDiamond.SetActive(false);

                int iAdd = (int)(dt * EXP_COEFF * m_iDiamondsToCollect); //(int)(((float)m_iMetersToCollect) * EXP_COEFF);
                iAdd = Mathf.Clamp(iAdd, 1, m_iDiamondsToCollect);

                m_iDiamondsCollected += iAdd;

                if (m_iDiamondsCollected <= m_iDiamondsToCollect)
                {
                    //do nothing
                }
                else if (m_finalExperienceValue > 0)
                {
                    m_finalExperienceValue = 0;
                }

                m_iDiamondsCollected = Mathf.Clamp(m_iDiamondsCollected, 0, m_iDiamondsToCollect);
                m_pDiamondsText.text = m_pUIRoot.FormatTextNumber(m_iOldPlayerDiamonds + m_iDiamondsCollected);

                // play the bounce animation
                m_pDiamondsAnim.Rewind();
                m_pDiamondsAnim.Play();
                interfaceSounds.CounterStar();
                continue;
            }

            // move the coins towards the coin in the upper right corner of the screen
            if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.LERP)
            {
                kDiamond.transform.position = Vector3.Lerp(kDiamond.transform.position, m_pDiamondsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kDiamond.transform.localScale = Vector3.Lerp(kDiamond.transform.localScale, m_pDiamondsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.SLERP)
            {
                kDiamond.transform.position = Vector3.Slerp(kDiamond.transform.position, m_pDiamondsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kDiamond.transform.localScale = Vector3.Slerp(kDiamond.transform.localScale, m_pDiamondsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
            else if (FloatingObjectsUpdateType == FloatingObjectsUpdateTypes.MOVE_TOWARDS)
            {
                kDiamond.transform.position = Vector3.MoveTowards(kDiamond.transform.position, m_pDiamondsAnim.transform.position, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
                kDiamond.transform.localScale = Vector3.MoveTowards(kDiamond.transform.localScale, m_pDiamondsAnim.transform.localScale, TimeManager.Instance.MasterSource.DeltaTime * FLOATING_STARS_SPEED);
            }
        }
    }
    
    //-------------------------------------------------------//
    void CreatePools()
    {
        m_aFuels = new GameObject[POOL_SIZE];
        m_aCoins = new GameObject[POOL_SIZE];
        m_aStars = new GameObject[POOL_SIZE];
        m_aStars_fake = new GameObject[POOL_SIZE];
        m_aDiamonds = new GameObject[POOL_SIZE];

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            m_aFuels[i] = Instantiate(m_pFuelsAnim.gameObject) as GameObject;
            m_aFuels[i].SetActive(false);

            m_aCoins[i] = Instantiate(m_pCoinsAnim.gameObject) as GameObject;
            m_aCoins[i].SetActive(false);

            m_aStars[i] = Instantiate(m_pMetersAnim.gameObject) as GameObject;
            m_aStars[i].SetActive(false);

            m_aStars_fake[i] = Instantiate(m_pMetersAnim.gameObject) as GameObject;
            m_aStars_fake[i].SetActive(false);

            m_aDiamonds[i] = Instantiate(m_pDiamondsAnim.gameObject) as GameObject;
            m_aDiamonds[i].GetComponent<SpriteRenderer>().sortingOrder = 25;
            m_aDiamonds[i].SetActive(false);
        }
    }
    //-------------------------------------------------------//
    GameObject GetValidFuel()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
            if (m_aFuels[i].activeSelf == false)
                return m_aFuels[i];

        //Debug.LogWarning("Pool size too small for fuel!");
        return null;
    }
    //-------------------------------------------------------//
    GameObject GetValidCoin()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
            if (m_aCoins[i].activeSelf == false)
                return m_aCoins[i];

        //Debug.LogWarning("Pool size too small for coins!");
        return null;
    }
    //-------------------------------------------------------//
    GameObject GetValidFakeStar()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
            if (m_aStars_fake[i].activeSelf == false)
                return m_aStars_fake[i];

        //Debug.LogWarning("Pool size too small for stars!");
        return null;
    }
    //-------------------------------------------------------//
    GameObject GetValidStar()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
            if (m_aStars[i].activeSelf == false)
                return m_aStars[i];

        //Debug.LogWarning("Pool size too small for stars!");
        return null;
    }
    //-------------------------------------------------------//
    GameObject GetValidDiamond()
    {
        for (int i = 0; i < POOL_SIZE; ++i)
            if (m_aDiamonds[i].activeSelf == false)
                return m_aDiamonds[i];

        //Debug.LogWarning("Pool size too small for diamonds!");
        return null;
    }
    //-------------------------------------------------------//
    public void StartCoinsAnimation(int coinsToAdd, GameObject startPositionObj, float coinsCoeff = 0.02f, float delay = 0.4f)
    {
        Delay = delay;
        COINS_COEFF = coinsCoeff;
        m_fGeneralTimer = m_fUpdateTimer = TimeManager.Instance.MasterSource.TotalTime;
        //m_iMetersCollected = 0;
        m_iCoinsCollected = 0;
        m_pPlaceholderCoins = startPositionObj;
        m_iOldPlayerCoins = (int)PlayerPersistentData.Instance.Coins;
        m_iCoinsToCollect = coinsToAdd;
        PlayerPersistentData.Instance.Coins += m_iCoinsToCollect;
        PlayerPersistentData.Instance.Save();
    }
    //-------------------------------------------------------//
    public void StartFuelsAnimation(int fuelsToAdd, GameObject startPositionObj, float fuelsCoeff = 1.0f, float delay = 0.4f)
    {
        Delay = delay;
        FUELS_COEFF = fuelsCoeff;
        m_fGeneralTimer = m_fUpdateTimer = TimeManager.Instance.MasterSource.TotalTime;
        //m_iMetersCollected = 0;
        m_iFuelsCollected = 0;
        m_pPlaceholderFuels = startPositionObj;
        m_iFuelsToCollect = fuelsToAdd;
    }
	//-------------------------------------------------------//
    public void StartExpAnimation(int expToAdd, int valueToDisplay, GameObject startPositionObj, float expCoeff = 0.03f, float delay = 0.4f, bool updateText = true, bool isMetersAnim = false)
    {
		Delay = delay;
		EXP_COEFF = expCoeff;
        m_fGeneralTimer = m_fUpdateTimer = TimeManager.Instance.MasterSource.TotalTime;
        m_iMetersCollected = 0;
        //m_iCoinsCollected = 0;
        m_iOldPlayerDiamonds = (int)PlayerPersistentData.Instance.Diamonds;
		m_pPlaceholderMeters = startPositionObj;
        m_iMetersToCollect = valueToDisplay; // expToAdd;
        metersAnimationEnded = false;
        updateTextfield = updateText;
        meterAnim = isMetersAnim;

        float animationSpeed = expToAdd / (8000.0f * (5.0f / expCoeff)); //expToAdd / (8000.0f * (5.0f / expCoeff)); //14500.0f

        m_pUIRoot.UpdateExperienceBarAnimated(delay, expToAdd, animationSpeed);// * 0.15f

        m_finalExperienceValue = PlayerPersistentData.Instance.Experience + expToAdd * gameplayManager.ExperienceMultiplier;
        if (m_finalExperienceValue > PlayerPersistentData.Instance.NextExperienceLevelThreshold)
        {
            m_finalExperienceValue -= PlayerPersistentData.Instance.NextExperienceLevelThreshold;
        }
        //PlayerPersistentData.Instance.Experience = experience < PlayerPersistentData.Instance.NextExperienceLevelThreshold ? experience : experience - PlayerPersistentData.Instance.NextExperienceLevelThreshold;
    }

    public void StartExpAnimationFake(int starsToDisplay, GameObject startPositionObj, float expCoeff = 0.03f, float delay = 0.4f)
    {
        Delay = delay;
        m_pPlaceholderFake = startPositionObj;
        m_iFakeExpToCollect = starsToDisplay;
        m_iFakeExpCollected = 0;
    }
    //-------------------------------------------------------//
    public void StartDiamondsAnimation(int diamondsToAdd, GameObject startPositionObj, float diamodsCoeff = 0.02f, float delay = 0.4f)
    {
        Delay = delay;
        EXP_COEFF = diamodsCoeff;
        m_fGeneralTimer = m_fUpdateTimer = TimeManager.Instance.MasterSource.TotalTime;
        m_iDiamondsCollected = 0;
        m_iOldPlayerDiamonds = (int)PlayerPersistentData.Instance.Diamonds;
        m_pPlaceholderDiamonds = startPositionObj;
        m_iDiamondsToCollect = diamondsToAdd;
    }
	//-------------------------------------------------------//
    public void PauseAnimations( )
    {
        pauseAnimations = true;
    }
	//-------------------------------------------------------//
    public void ResumeAnimations()
    {
        pauseAnimations = false;
    }
	//-------------------------------------------------------//
    public void DisableFloatingStuff()
    {
        if (m_iMetersToCollect > 0 || m_iCoinsToCollect > 0 || m_iFakeExpToCollect>0)
        {
            m_iMetersCollected = 0;
            m_iCoinsCollected = 0;
            m_iFuelsCollected = 0;
            m_iMetersToCollect = 0;
            m_iCoinsToCollect = 0;
            m_iFuelsToCollect = 0;
            m_iFakeExpToCollect = 0;
            m_iFakeExpCollected = 0;

            for (int i = 0; i < POOL_SIZE; ++i)
            {
                m_aCoins[i].SetActive(false);
                m_aFuels[i].SetActive(false);
                m_aStars[i].SetActive(false);
                m_aStars_fake[i].SetActive(false);
            }

            if (m_finalExperienceValue > 0)
            {
                //m_pUIRoot.SetExperienceBarValue(m_finalExperienceValue);
                m_finalExperienceValue = 0;
            }

            m_pMetersAnim.Rewind();
            m_pMetersAnim.Stop();

            m_pCoinsAnim.Rewind();
            m_pCoinsAnim.Stop();

            m_pFuelsAnim.Rewind();
            m_pFuelsAnim.Stop();

            m_pDiamondsAnim.Rewind();
            m_pDiamondsAnim.Stop();
        }
    }
	//-------------------------------------------------------//
}