using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class CheatsManager : MonoBehaviour
{

#if UNITY_EDITOR

	[MenuItem("GameUtils/Break Prefab #&b")]
	public static void BreakPrefab ()
	{
		GameObject[] aObjects = Selection.gameObjects;
		int iLen = aObjects.Length;
		
		GameObject pSelection = null;
		
		for( int i = 0 ; i < iLen ; ++i )
		{
			GameObject kOld = aObjects[i];
			
			Transform pParent = kOld.transform.parent;
			kOld.transform.parent = null;
			
			GameObject kNew = GameObject.Instantiate( kOld ) as GameObject;
			
			if( pSelection == null )
				pSelection = kNew;
			
			kNew.name = kOld.name;
			kNew.SetActive( kOld.activeInHierarchy );
			
			GameObject.DestroyImmediate(kOld);
			kNew.transform.parent = pParent;
		}
		Selection.activeGameObject = pSelection;
	}

    [MenuItem("PlayerPrefs/ClearData")]
    public static void ClearDataEditor()
    {
        Clear();
    }

    //----------------------------------------------//
    public static void Clear()
    {
        Debug.LogWarning("[ CLEARED ALL GAME DATA ]\n");
        PlayerPrefs.DeleteAll();
    }
#endif

    #region Protected members

    protected float prevFixedDeltaTime;

    #endregion


    #region Public properties


    #endregion

    #region Unity Callbacks

    int nextDaily = 0;
    void HandleCheats()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            OnTheRunNewsIntegration.Instance.EditorTest_SetZeroNews();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            OnTheRunNewsIntegration.Instance.EditorTest_ResetNumNews();
        }

        if (Input.GetKeyDown("1"))
        {
            TimeManager.Instance.MasterSource.TimeMultiplier = 0.05f;
            prevFixedDeltaTime = Time.fixedDeltaTime;
            Time.fixedDeltaTime *= 0.05f;
        }

        if (Input.GetKeyDown("2"))
        {
            TimeManager.Instance.MasterSource.TimeMultiplier = 1.0f;
            Time.fixedDeltaTime = prevFixedDeltaTime;
        }

        if (Input.GetKeyDown("3"))
        {
            Manager<UIManager>.Get().PushPopup("VideoFeedbackPopup");

			string s1 = OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_congrat");
			string s2 = OnTheRunDataLoader.Instance.GetLocaleString("popup_free_coins_not_available");

			Manager<UIManager>.Get().FrontPopup.GetComponent<UIVideoFeedbackPopup>().SetupPopup(1, s1, s2);

            //PlayerPersistentData.Instance.UpdateExperiencePoints(poinToNextLevel);
            //Manager<UIRoot>.Get().UpdateCurrenciesItem();

            //Debug.Log("AAAAAAAA " + PlayerPersistentData.Instance.Experience + " " + PlayerPersistentData.Instance.NextExperienceLevelThreshold);
            //PlayerPersistentData.Instance.Level += PlayerPersistentData.Instance.Experience;
            //Debug.Log("LEVEL: " + PlayerPersistentData.Instance.Level);

            //if(UIFacebookLoggedPopup.CanBeShown(true))
            //    OnTheRunUITransitionManager.Instance.OpenPopup("FacebookLoggedPopup");
//             SkipToReward();

            //Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartMissionPassedAnimation();
            
            //OnTheRunUITransitionManager.Instance.OpenPopup("SkipMissionPopup");
            //Manager<UIManager>.Get().FrontPopup.GetComponent<UISkipMissionPopup>().SetupPopup(1);

            //OnTheRunUITransitionManager.Instance.OpenPopup("RankBarPopup");
            //Manager<UIManager>.Get().FrontPopup.BroadcastMessage("InitRankScroller", true);

            //OnTheRunUITransitionManager.Instance.OpenPopup("SkipMissionPopup");
            //Manager<UIManager>.Get().FrontPopup.GetComponent<UISkipMissionPopup>().SetupPopup(1, 10);
            
            /*
            DateTime now = DateTime.Now;
            System.TimeSpan tSpan = new System.TimeSpan(0, 19, 0, 0);
            DateTime lastTime = now - tSpan;
            Debug.Log("DAYS PASSED: float -> " + (now - lastTime).TotalDays+" int -> " + (int)(now - lastTime).TotalDays);
            */

            //GameObject cameraRef = GameObject.FindGameObjectWithTag("MainCamera");
            //cameraRef.SendMessage("OnTurboCameraEvent", true);
            //cameraRef.SendMessage("OnFinalCameraEvent");

            //OnTheRunGameplay gameplay = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
            //gameplay.gameObject.AddComponent<InterpolatedTimeEffects>();

            //InterpolatedTimeEffects.Instance.AddInterpEffect(0.25f, 1.0f, 0.2f);
            //interpTimeEffects.AddInterpEffect(0.1f, 0.3f, 1.0f);

            /*OnTheRunGameplay gameplay = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
            gameplay.gameObject.AddComponent<SlowMotionController>();
            gameplay.gameObject.GetComponent<SlowMotionController>().PlaySloMo(0.1f, 0.2f);*/

            //Manager<UIManager>.Get().ActivePage.gameObject.SendMessage("OnBackButtonAction", SendMessageOptions.DontRequireReceiver);

            //Manager<UIManager>.Get().ActivePage.GetComponent<UIRewardPage>().StartNewRecordAnimation(); 
            
            //Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().StartProgressBarAnimation(); 

            //OnTheRunUITransitionManager.Instance.OpenPopup("FacebookLoginPopupWithReward");
            //int poinToNextLevel = PlayerPersistentData.Instance.NextExperienceLevelThreshold - PlayerPersistentData.Instance.Experience;
            //PlayerPersistentData.Instance.UpdateExperiencePoints(poinToNextLevel);

            /*OnTheRunUITransitionManager.Instance.OpenPopup("RankFeedbackPopup");
            string newLevelReachedText = OnTheRunDataLoader.Instance.GetLocaleString("level") + " 2";
            UIPopup frontPopup = Manager<UIManager>.Get().FrontPopup;
            frontPopup.GetComponent<UIRankFeedbackPopup>().SetText(newLevelReachedText, OnTheRunDataLoader.Instance.GetLocaleString("you_win"), "1");
            frontPopup.SendMessage("StartFireworks");*/

            //GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GetComponent<OnTheRunFBFriendsSpawner>().PlaceFriendMarker("adsas");

            /*OnTheRunDailyBonusManager.Instance.ForceActiveMissionsUpdate = true;
            OnTheRunDailyBonusManager.Instance.NewsBadgdesActive = true;*/

            //OnTheRunUITransitionManager.Instance.OpenPopup("FBFeedbackPopup");

            //OnTheRunNotificationManager.Instance.ResetGiftAndInviteDone();


            //OnTheRunUITransitionManager.Instance.OpenPopup("FBFriendsPopup");
            
            //Manager<UIManager>.Get().ActivePage.SendMessage("CheckForAdvicePopup");
            //OnTheRunUITransitionManager.Instance.OpenPopup("TrucksAdvisePopup");

            //OnTheRunFBLoginPopupManager.Instance.SendMessage("ShowFBPopup");

            //PlayerPersistentData.Instance.Coins += 5000;

            //PlayerPersistentData.Instance.BuyItem(PriceData.CurrencyType.FirstCurrency, 200, true, OnTheRunDataLoader.Instance.GetLocaleString("popup_amazing"), OnTheRunDataLoader.Instance.GetLocaleString("popup_unlocked_car"), UIBuyFeedbackPopup.ItemBought.Car);
          
            //GameObject.FindGameObjectWithTag("Player").BroadcastMessage("ActivateSlipstreamEffect");

            //GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GameplayTime += 10.0f; 

            //OnTheRunUITransitionManager.Instance.OpenPopup("DailyBonusSequencePopup");
            //OnTheRunUITransitionManager.Instance.OpenPopup("SingleButtonFeedbackPopup");
                        
            //OnTheRunUITransitionManager.Instance.OnPageChanged("RewardPage", "");

            //GameObject.FindGameObjectWithTag("Player").SendMessage("OnShieldActive", true);
            //LevelRoot.Instance.Root.BroadcastMessage("CreatePlayerCar", 1);
            //GameObject.FindGameObjectWithTag("Player").gameObject.SendMessage("ActivateMagnetFx", true);

            /*BonusBehaviour.BonusData data = new BonusBehaviour.BonusData();
            data.type = OnTheRunObjectsPool.ObjectType.BonusMagnet;
            data.coinsToGain = 0;
            GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("OnBonusCollected", data);*/

            //Debug.Log(Manager<UIManager>.Get().gameObject.name);
            //PlayerPersistentData.Instance.Level++;
            //Manager<UIManager>.Get().ActivePage.SendMessage("OnTimeSaved", 5);
            //Manager<UIManager>.Get().ActivePage.SendMessage("OnScoreForCollision", GameObject.FindGameObjectWithTag("Player").rigidbody.transform.position);
            //Manager<UIManager>.Get().ActivePage.SendMessage("OnTimeSaved", 30.0f);

            //GameObject.FindGameObjectWithTag("GameplayManagers").SendMessage("SaveDistancesForSpecialCar");
            
            //Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().OnCoinsForCollision(GameObject.FindGameObjectWithTag("Player").rigidbody.transform.position, 2);

            //Manager<UIManager>.Get().ActivePage.GetComponent<UIIngamePage>().SendMessage("OnLiveNews",4.0f);//GameObject.FindGameObjectWithTag("Player").rigidbody.transform.position, 1);
            //Manager<UIRoot>.Get().UpdateExperienceBarImmediatly(0);

            //GameObject.FindGameObjectWithTag("Player").SendMessage("TestFlying");
        }
        if (Input.GetKeyDown("4"))
        {
            //PlayerPersistentData.Instance.Level = 3;
            //Manager<UIManager>.Get().ActivePage.SendMessage("UnlockCarsByXP");
            //PlayerPersistentData.Instance.GetPlayerData(OnTheRunGameplay.CarId.Car_3_europe).UnlockCar();
            //OnTheRunUITransitionManager.Instance.OpenPopup("SkipMissionPopup");
            //Manager<UIManager>.Get().FrontPopup.GetComponent<UISkipMissionPopup>().SetupPopup(2, 10);
            GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("ActivateTurbo");
        }

		/*if (Input.GetKeyDown("5"))
			OnTheRunInAppManager.Instance.OnUserHackedPurchases();*/

        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GameplayTime = 2.0f; 
        }
        
        /*if (Input.GetKeyDown("v"))
        {
            LevelRoot.Instance.Root.Find("Track").SendMessage("UpdateAmbient");
        }*/
        
        if (Input.GetKeyDown("f"))
        {
            LevelRoot.Instance.Root.Find("Track").SendMessage("switchAmbient");
        }

        //Reset missions data only from start page
        if (Manager<UIManager>.Get().ActivePageName == "StartPage" || Manager<UIManager>.Get().ActivePageName == "GaragePage")
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                LevelRoot.Instance.Root.BroadcastMessage("ResetMissionsData", SendMessageOptions.DontRequireReceiver);
            } 
            if (Input.GetKeyDown(KeyCode.D))
            {
                nextDaily++;
                if (nextDaily > 10)
                    nextDaily = 10;
                OnTheRunDailyBonusManager.Instance.TestDailyBonus(nextDaily);
            }
        }


        if (Manager<UIManager>.Get().ActivePageName == "GaragePage")
        {

            if (Input.GetKeyDown(KeyCode.S))
            {
                Manager<UIManager>.Get().ActivePage.SendMessage("ScaleCurrentCar", false);
            }
        }

        if (Manager<UIManager>.Get().ActivePageName == "IngamePage")
        {

            if (Input.GetKeyDown(KeyCode.G))
            {
                GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("OnSaveMeHelpingHandPressed");
                //LevelRoot.Instance.Root.BroadcastMessage("ForceSpawn");
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                LevelRoot.Instance.Root.BroadcastMessage("CheatPassMission");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                LevelRoot.Instance.Root.BroadcastMessage("SpawnEnemy", OnTheRunEnemiesManager.EnemyType.PoliceSingle);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                LevelRoot.Instance.Root.BroadcastMessage("SpawnEnemy", OnTheRunEnemiesManager.EnemyType.PoliceTriple);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                LevelRoot.Instance.Root.BroadcastMessage("SpawnEnemy", OnTheRunEnemiesManager.EnemyType.RoadBlock);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                LevelRoot.Instance.Root.BroadcastMessage("SpawnEnemy", OnTheRunEnemiesManager.EnemyType.Helicopter);
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().GameplayTime += 10.0f;
            }


            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                LevelRoot.Instance.Root.BroadcastMessage("BigfootCheat");
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                LevelRoot.Instance.Root.BroadcastMessage("TankCheat");
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                LevelRoot.Instance.Root.BroadcastMessage("FiretruckCheat");
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                LevelRoot.Instance.Root.BroadcastMessage("UfoCheat");
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                LevelRoot.Instance.Root.BroadcastMessage("PlaneCheat");//"FiretruckCheat" //("BigfootCheat");
            }
            
            if (Input.GetKeyDown(KeyCode.T))
            {
                LevelRoot.Instance.BroadcastMessage("StartRoadWorks", GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().transform.position.z);
                LevelRoot.Instance.BroadcastMessage("StartGuardRail", GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().transform.position.z);
            }

        }
        else if (Manager<UIManager>.Get().ActivePageName == "GaragePage")
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                PlayerPersistentData.Instance.IncrementCurrency(PriceData.CurrencyType.FirstCurrency, 1000);
                OnTheRunSounds.Instance.PlayBonusCollected(OnTheRunObjectsPool.ObjectType.BonusMoneyBig);
                Manager<UIRoot>.Get().UpdateCurrenciesItem();
            }
        }
#endif

    }

    void SkipToReward()
    {
        OnTheRunGameplay gameplayManager = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();

        Manager<UIRoot>.Get().SetupBackground(OnTheRunEnvironment.Environments.Europe);
        Manager<UIRoot>.Get().GetComponent<UISharedData>().AddScore(40000);
        gameplayManager.CoinsCollected = 2000;

        gameplayManager.GetComponent<OnTheRunFBFriendsSpawner>().DestroyAll();
        
        gameplayManager.OnChangeCarEvent(Vector3.zero);
        gameplayManager.ChangeState(OnTheRunGameplay.GameplayStates.Offgame);

        //DestroyAnimatedTexts();
        UIManager uiManager = Manager<UIManager>.Get();

        gameplayManager.MainCamera.SendMessage("OnFinalCameraEvent");
        uiManager.RemovePopupFromStack("SaveMePopup");
        uiManager.PopPopup();

        Manager<UIRoot>.Get().IsMissionPageFromIngameFlag = false;
        Manager<UIRoot>.Get().ActivateBackground(true);
        OnTheRunSingleRunMissions.Instance.CanActivatePassedMissions = false;
        uiManager.GoToPage("RewardPage");
        OnTheRunUITransitionManager.Instance.OnPageChanged("RewardPage", "");
               
        if (OnTheRunIngameHiScoreCheck.Instance)
            OnTheRunIngameHiScoreCheck.Instance.OnGameplayFinished();
    }

    void Update()
    {
#if UNITY_EDITOR
        this.HandleCheats();
#endif
    }
    #endregion
}
