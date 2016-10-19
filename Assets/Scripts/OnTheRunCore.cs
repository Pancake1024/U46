using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Math;
using SBS.UI;
using System.Collections;

public class OnTheRunCore : MonoBehaviour
{
    //protected BitmapFontText fpsText;
    //protected BitmapFontText testText;
    public UITextField fpsText;

    protected int frameCount = 0;
    protected float totalTime = 0.0f;
    protected float fps = 0.0f;
    /*
    public UICollectionNode test;
    */
    protected Scroller scroller;
/*
    void OnGUI()
    {
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(Screen.width * 0.6f, Screen.height * 0.016f, 440, 20), "FPS: " + fps);
    }
*/

    void Awake()
    {
        PlayerPersistentData.Instance.Load();
    }

    void Start()
    {
#if (UNITY_IPHONE)
		if (iPhoneGeneration.iPodTouch1Gen == iPhone.generation ||
			iPhoneGeneration.iPodTouch2Gen == iPhone.generation ||
			iPhoneGeneration.iPhone3G == iPhone.generation)
        	Application.targetFrameRate = 30;
		else
        	Application.targetFrameRate = 60;
#else
        Application.targetFrameRate = 60;
#endif

        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;

        LevelRoot.Instance.Initialize();/*
        LevelObject[] levObjs = LevelRoot.Instance.Root.gameObject.GetComponentsInChildren<LevelObject>();
        foreach (LevelObject obj in levObjs)
            obj.Initialize();*/
        
        /*
        testText = new BitmapFontText();
        testText.Font = UIManager.Instance.GetFont("duality_small");
        testText.Position = new Vector2(0.0f, Screen.height - testText.Font.lineHeight);
        testText.Text = "bla bla";
        */
        /*fpsText = new BitmapFontText();
        fpsText.Font = UIManager.Instance.GetFont("duality_small");
        fpsText.Position = new Vector2(0.0f, Screen.height - fpsText.Font.lineHeight);
         * */
        /*
        test.Collection.AddElement("fps", fpsText);
        test.Collection.AddButton("play", UIHelper.Instance.CreateButton("btn_play", new UIRect(0.256f, 0.642f, 0.488f, 0.174f), "TEST"));
        test.transform.GetChild(0).gameObject.GetComponent<UICollectionNode>().Collection.AddElement("bla", testText);
        */

        /*
        scroller = UIManager.Instance.AddScroller("test_scroller", new UIRect(0.25f, 0.3f, 0.6f, 0.4f));
        scroller.ScrollX = false;
        for (int i = 0; i < 40; ++i)
            scroller.AddElement(i.ToString(), UIHelper.Instance.CreateText("duality_small", BitmapFontText.TextAlign.MiddleCenter, new UIRect(0.25f, i * 0.03f, 0.5f, 0.035f), "riga " + i));
        */

        //OnTheRunPreloader preloader = FindObjectOfType(typeof(OnTheRunPreloader)) as OnTheRunPreloader;
        //if (preloader != null)
        //    preloader.OnNewLevelLoaded();

        /*
        SBS.Flurry.FlurryBinding.logEvent("test");
        SBS.Flurry.FlurryBinding.logEvent("timedTest", true);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.Add("key0", "param0");
        dict.Add("key1", "param1");
        dict.Add("key2", "param2");
        SBS.Flurry.FlurryBinding.logEvent("testWithParams", dict);*/
        //ObjCDispatcher.Instance.RegisterEvent("alertButtonClicked", (btnName) => { Debug.Log("Button clicked: " + btnName); });
        //SBS.Flurry.FlurryBinding.logEvent("timedTest", true);
#if UNITY_ANDROID
        Debug.Log("App Version: " + AndroidUtils.GetVersionName());
#elif UNITY_WP8
        Debug.Log("App Version: " + SBS.Miniclip.WP8Bindings.VersionNumber;);
#else
        Debug.Log("App Version: " + SBS.Core.iOSUtils.GetAppVersion());
#endif

        if (!OnTheRunDataLoader.Instance.GetFpsIsEnabled())
            fpsText.gameObject.SetActive(false);
    }

    /*public void test(string num)
    {
        Debug.Log(num);
    }*/

    void Update()
    {
        if (OnTheRunDataLoader.Instance.GetFpsIsEnabled())
        {
            ++frameCount;
            totalTime += Time.deltaTime;
            if (totalTime > 1.0f)
            {
                //if (fpsText != null)
                //  fpsText.Text = Mathf.Round((float)frameCount / totalTime).ToString();//((float)frameCount / totalTime).ToString("0.0");
                fps = Mathf.Round((float)frameCount / totalTime);
                frameCount = 0;
                totalTime = 0.0f;

                fpsText.text = "FPS: " + fps;
                fpsText.ApplyParameters();
            }
        }
    }

    /*void OnPause()
    {
        if (RunnerInterface.Instance != null)
            RunnerInterface.Instance.OnPause();
    }

    void OnResume()
    {
        if (RunnerInterface.Instance != null)
            RunnerInterface.Instance.OnResume();
    }
    */
    void OnApplicationPause(bool paused)
    {
        Screen.sleepTimeout = paused ? (int)SleepTimeout.SystemSetting : (int)SleepTimeout.NeverSleep;

        if (paused)
        {
            PlayerPersistentData.Instance.Save();
            PlayerPersistentData.Instance.SaveCars();
            //EncryptedPlayerPrefs.Save();
        }
        else
        {
            PlayerPersistentData.Instance.Load();
            /*if (OnTheRunSoundsManager.Instance != null)
                OnTheRunSoundsManager.Instance.UpdateDeviceMusicPlaying();*/

			StartCoroutine(UpdateDeviceMusicPlayingDelayed());
        }
        
        UIManager uiManager = Manager<UIManager>.Get();
        if (paused && uiManager.ActivePageName.Equals("IngamePage"))
        {
            UIPopup frontPopup = uiManager.FrontPopup;
            if (null == frontPopup) // || frontPopup.name != "RewardPopup")
            {
                if (!uiManager.BringPopupToFront("PausePopup"))
                {
                    OnTheRunGameplay onTheRunGameplay = GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>();
                    if (onTheRunGameplay != null)
                        onTheRunGameplay.ChangeState(OnTheRunGameplay.GameplayStates.Paused);
                    uiManager.PushPopup("PausePopup");
                }
            }
        }
    }

	IEnumerator UpdateDeviceMusicPlayingDelayed()
	{
		yield return new WaitForEndOfFrame();

		if (OnTheRunSoundsManager.Instance != null)
		{
			OnTheRunSoundsManager.Instance.UpdateDeviceMusicPlaying();
			//OnTheRunSoundsManager.Instance.gameObject.SendMessage("OnResume");
		}
	}
    
    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        OnApplicationPause(true);
        //PlayerPersistentData.Instance.Save();
#endif
    }
}
