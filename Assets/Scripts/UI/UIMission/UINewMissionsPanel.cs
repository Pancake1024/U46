using UnityEngine;
using SBS.Core;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UINewMissionsPanel")]
public class UINewMissionsPanel : MonoBehaviour
{
	public Animation Anim;
	public UITextField NewMissionText;
	public UITextField titleText;
    public UITextField descriptionText;
    public UITextField rewardText;
    public UITextField rewardValue;
    public GameObject coinsRewardItem;
    public GameObject expRewardItem;
    public GameObject gemsRewardItem;
    public GameObject missionCompletedItem;
    public UITextField progressTextPrefix;
    public UITextField progressTextGoal;
    public UITextField progressTextPostfix;
    public GameObject progressGroup;
    protected GameObject diamondIcon;
    protected UITextField specialText;


    public Sprite missionFailedSprite;
    public Sprite missionCompletedSprite;

    protected string xpPostfix;

    void Awake()
    {
        xpPostfix = OnTheRunDataLoader.Instance.GetLocaleString("xp_postfix");

		if( NewMissionText != null )
		{
			string strLocalized = OnTheRunDataLoader.Instance.GetLocaleString("new_mission"); 

			strLocalized = strLocalized.ToUpper();

			if( strLocalized.Contains("_") )
				strLocalized = "NEW MISSION !"; // debug text

			NewMissionText.text = strLocalized;
		}

        string val = PlayerPrefs.GetString(kOldMissKey_base + "_" + OnTheRunEnvironment.Environments.Europe, "");
        m_kOldMissions.Add(OnTheRunEnvironment.Environments.Europe, val);
        val = PlayerPrefs.GetString(kOldMissKey_base + "_" + OnTheRunEnvironment.Environments.NY, "");
        m_kOldMissions.Add(OnTheRunEnvironment.Environments.NY, null);
        val = PlayerPrefs.GetString(kOldMissKey_base + "_" + OnTheRunEnvironment.Environments.Asia, "");
        m_kOldMissions.Add(OnTheRunEnvironment.Environments.Asia, null);
        val = PlayerPrefs.GetString(kOldMissKey_base + "_" + OnTheRunEnvironment.Environments.USA, "");
        m_kOldMissions.Add(OnTheRunEnvironment.Environments.USA, null);

		/*m_kOldMissions.Add( OnTheRunEnvironment.Environments.Europe , null );
		m_kOldMissions.Add( OnTheRunEnvironment.Environments.NY , null );
		m_kOldMissions.Add( OnTheRunEnvironment.Environments.Asia , null );
		m_kOldMissions.Add( OnTheRunEnvironment.Environments.USA , null );*/
    }

    private Dictionary<OnTheRunEnvironment.Environments, string> m_kOldMissions = new Dictionary<OnTheRunEnvironment.Environments, string>();
    private string kOldMissKey_base = "koldm_id";

	public Transform[] MissionPanelActiveStuff;
	public Transform[] MissionPanelNewActiveStuff;

	public void SkipNewMissionAnim()
	{
		if( Anim != null )
		{
			Anim["MissionPanel@"].normalizedTime = 0.9f;
			Anim.Play("MissionPanel@");

			foreach( Transform t in MissionPanelActiveStuff )
				t.gameObject.SetActive( true );

			foreach( Transform t in MissionPanelNewActiveStuff )
				t.gameObject.SetActive( false );
		}
	}

	void OnDisable()
	{
		if( Anim != null )
		{
			Anim.Stop();

			foreach( Transform t in MissionPanelActiveStuff )
				t.gameObject.SetActive( false );
			
			foreach( Transform t in MissionPanelNewActiveStuff )
				t.gameObject.SetActive( false );
		}
	}

    public void UpdatePanel(OnTheRunSingleRunMissions.DriverMission currentMission)
    {
        if (diamondIcon == null)
        {
            diamondIcon = transform.FindChild("RightSide/diamond").gameObject;
            specialText = transform.FindChild("RightSide/diamond/tape_new_mission/special_text").GetComponent<UITextField>();
        }

        transform.FindChild("RightSide/diamond/tape_new_mission").gameObject.SetActive(false);
        rewardValue.text = "";

		OnTheRunEnvironment.Environments eEnv = OnTheRunGameplay.Instance.GetComponent<OnTheRunEnvironment>().currentEnvironment;
		
		if( Anim != null )
		{
			if( m_kOldMissions[eEnv] != currentMission.id && currentMission.IsPassed == false )
			{
				Anim.Play("MissionPanelNewMission@");
				Anim.PlayQueued("MissionPanel@");
			}
			else
			{
				foreach( Transform t in MissionPanelActiveStuff )
					t.gameObject.SetActive( true );
				
				foreach( Transform t in MissionPanelNewActiveStuff )
					t.gameObject.SetActive( false );

				Anim.Sample();
				Anim.Play("MissionPanel@");
			}
		}
		m_kOldMissions[eEnv] = currentMission.id;
        PlayerPrefs.SetString(kOldMissKey_base + "_" + eEnv, currentMission.id);

        titleText.text = OnTheRunDataLoader.Instance.GetLocaleString("run_goal");

		titleText.color.a = 1f;
//		titleText.ApplyParameters();

        descriptionText.text = currentMission.description;
//        titleText.ApplyParameters();
//        descriptionText.ApplyParameters();
        SetRewardItem(currentMission, false);
        specialText.text = "";
//        specialText.ApplyParameters();
        diamondIcon.SetActive(currentMission.DiamondRewardActive);
        if (currentMission.DiamondRewardActive)
            specialText.text = OnTheRunDataLoader.Instance.GetLocaleString("special_mission");
//        specialText.ApplyParameters();

        diamondIcon.GetComponent<SpriteRenderer>().enabled = !currentMission.Done;
    }

    public void UpdateRewardPanel(OnTheRunSingleRunMissions.DriverMission currentMission, bool isOnReward)
    {
        if (diamondIcon == null)
        {
            diamondIcon = transform.FindChild("diamond").gameObject;
            specialText = transform.FindChild("diamond/tape_new_mission/special_text").GetComponent<UITextField>();
        }
        //OnTheRunSingleRunMissions.Instance.SetRewardTaken(currentMission);
        titleText.text = currentMission.description;
        descriptionText.text = currentMission.Done ? OnTheRunDataLoader.Instance.GetLocaleString("mission_completed") : OnTheRunDataLoader.Instance.GetLocaleString("mission_failed");
//        titleText.ApplyParameters();
//        descriptionText.ApplyParameters();

        rewardValue.text = "";
        rewardText.text = "";
        progressTextPrefix.text = "";
        progressTextGoal.text = "";
        progressTextPostfix.text = "";
        SetRewardItem(currentMission, isOnReward);
        specialText.text = "";
//        specialText.ApplyParameters();
        diamondIcon.SetActive(currentMission.DiamondRewardActive && currentMission.Done);
        if (currentMission.DiamondRewardActive)
            specialText.text = OnTheRunDataLoader.Instance.GetLocaleString("special_mission");
//        if (currentMission.Done)
//            specialText.ApplyParameters();
    }

    public void UpdateRewardPanelForcePassed(OnTheRunSingleRunMissions.DriverMission currentMission)
    {
        OnTheRunFireworks.Instance.StartFireworksEffect(5, Manager<UIManager>.Get().ActivePage.gameObject.transform.FindChild("fireworks_mission"));
        rewardText.text = "";
        rewardValue.text = "";
//        rewardValue.ApplyParameters();
//        rewardText.ApplyParameters();
        expRewardItem.SetActive(false);
        gemsRewardItem.SetActive(false);
        coinsRewardItem.SetActive(false);
        missionCompletedItem.SetActive(false);
        missionCompletedItem.SetActive(true);
        missionCompletedItem.GetComponent<SpriteRenderer>().sprite = missionCompletedSprite;
        //if (currentMission.Done)
        //    OnTheRunSingleRunMissions.Instance.EvaluateMissionPassed(currentMission);
    }
    
    public void ResetRewardDiamond()
    {
        diamondIcon.SetActive(false);
    }

    void SetRewardItem(OnTheRunSingleRunMissions.DriverMission currentMission, bool isOnReward)
    {
        rewardText.text = OnTheRunDataLoader.Instance.GetLocaleString("win");
        expRewardItem.SetActive(false);
        gemsRewardItem.SetActive(false);
        coinsRewardItem.SetActive(false);

        if (isOnReward)
        {
            if (currentMission.Done)
            {
                rewardValue.text = currentMission.Reward.GetRewardPassed().ToString();
                if (currentMission.DiamondRewardActive)
                    rewardValue.text += " +"; 
                switch (currentMission.Reward.type)
                {
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Exp:
                        //expRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Gems:
                        //gemsRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Coins:
                        //coinsRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.None:
                        rewardValue.text = "0 " + xpPostfix;
                        break;
                }
            }
            else
            {
                diamondIcon.SetActive(false);
                rewardText.text = "";
                rewardValue.text = "";
            }
        }
        else
        {
            if (!currentMission.Done)
            {
                rewardValue.text = currentMission.Reward.GetRewardPassed().ToString() + " " + xpPostfix;
                if (currentMission.DiamondRewardActive)
                    rewardValue.text += "+"; 
                switch (currentMission.Reward.type)
                {
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Exp:
                        //expRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Gems:
                        //gemsRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Coins:
                        //coinsRewardItem.SetActive(true);
                        break;
                    case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.None:
                        rewardValue.text = "0 " + xpPostfix;
                        break;
                }
            }
            else
            {
                rewardText.text = rewardValue.text = "";
            }
        }

        if (isOnReward)
        {
            int currentMeters = currentMission.Done ? Mathf.RoundToInt(currentMission.checkCounter) : Mathf.RoundToInt(currentMission.Counter);
            progressTextPrefix.text = "[ " + currentMeters + " / ";
            progressTextGoal.text = currentMission.checkCounter.ToString();
            progressTextPostfix.text = "]";
            progressTextPrefix.ApplyParameters();
            progressTextGoal.ApplyParameters();
            progressTextPostfix.ApplyParameters();
            float xPos = progressTextGoal.transform.localPosition.x + progressTextGoal.GetTextBounds().width + 0.2f;
            progressTextPostfix.transform.localPosition = new Vector3(xPos, progressTextPostfix.transform.localPosition.y, progressTextPostfix.transform.localPosition.z);
            xPos = -progressTextGoal.GetTextBounds().width * 0.5f - 0.1f;
            progressGroup.transform.localPosition = new Vector3(progressGroup.transform.localPosition.x + xPos, progressGroup.transform.localPosition.y, progressGroup.transform.localPosition.z);
        }

//        rewardText.ApplyParameters();
//        rewardValue.ApplyParameters();
        missionCompletedItem.SetActive(isOnReward ? false : currentMission.Done);
        
        //if (isOnReward)
        //    StartCoroutine(MissionRewardAnimation(currentMission));
    }

    void StartMissionRewardAnimation(OnTheRunSingleRunMissions.DriverMission currentMission)
    {
        StartCoroutine(MissionRewardAnimation(currentMission));
    }

    IEnumerator MissionRewardAnimation(OnTheRunSingleRunMissions.DriverMission currentMission)
    {
        if (currentMission.Done)
            OnTheRunSingleRunMissions.Instance.SetRewardTaken(currentMission);

        //yield return new WaitForSeconds(2.0f);
        yield return new WaitForSeconds(0.0f);

        switch (currentMission.Reward.type)
        {
            case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Exp:
                if (currentMission.Reward.ExpForMission >= 0)
                    Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartExpAnimation(currentMission.Reward.ExpForMission, currentMission.Reward.metersReward, expRewardItem, 2f, 0f);
                break;
            case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Gems:
                break;
            case OnTheRunSingleRunMissions.SingleRunMissionsRewardType.Coins:
                Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartCoinsAnimation(currentMission.Reward.quantity, coinsRewardItem, 0.1f, 0.4f);
                break;
        }

        if (currentMission.Done)
        {
            OnTheRunFireworks.Instance.StartFireworksEffect(5, Manager<UIManager>.Get().ActivePage.gameObject.transform.FindChild("fireworks_mission"));
            int rewardInDiamonds = (int)OnTheRunDataLoader.Instance.GetMissionsRewardData("diamonds", -1.0f);
            if (rewardInDiamonds > 0)
            {
                if (currentMission.DiamondRewardActive)
                {
                    Manager<UIRoot>.Get().GetComponent<UIExpAnimation>().StartDiamondsAnimation(rewardInDiamonds, expRewardItem, 5.0f, 0.0f);
                    PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.SecondCurrency, 1.0f);
                    Manager<UIManager>.Get().ActivePage.SendMessage("ResetRewardDiamond", SendMessageOptions.DontRequireReceiver);
                }
            }
        }


        //expText.gameObject.SetActive(false);
        //expTextUnderline.SetActive(false);
        rewardText.text = "";
        rewardValue.text = "";
//        rewardValue.ApplyParameters();
//        rewardText.ApplyParameters();
//        rewardText.ApplyParameters();
        expRewardItem.SetActive(false);
        gemsRewardItem.SetActive(false);
        coinsRewardItem.SetActive(false);
        missionCompletedItem.SetActive(true);
        missionCompletedItem.GetComponent<SpriteRenderer>().sprite = currentMission.Done ? missionCompletedSprite : missionFailedSprite;

        if (currentMission.Done)
            OnTheRunSingleRunMissions.Instance.EvaluateMissionPassed(currentMission);

        yield return new WaitForSeconds(2.0f);

        Manager<UIManager>.Get().ActivePage.GetComponent<UIRewardPage>().AnimationsEnded();
    }
}