using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("OnTheRun/UI/UIPreloader")]
public class UIPreloader : MonoBehaviour
{
    public string sceneToLoad = "scene";
    public string pageToLoad = "StartPage";
    public Animation loader;

    protected string loaderClip;
    protected bool firstFrame = true;
    protected AsyncOperation asyncLoad = null;

    protected MiniclipAPIManager mcApiManager = null;
    public bool canProcede = false;

    protected bool sessionHasBegun = false;

    void Awake()
    {
        loaderClip = loader.clip.name;
        DontDestroyOnLoad(TimeManager.Instance.gameObject);

#if UNITY_WEBPLAYER
        GameObject mcGo = new GameObject("MiniclipAPI");
        mcGo.AddComponent<MiniclipAPI>();
        mcApiManager = mcGo.AddComponent<MiniclipAPIManager>();
        mcGo.tag = "MiniclipAPI";

        if (!MiniclipAPIManager.IsOnMiniclip())
            canProcede = true;
#endif
    }

    void Start()
    {
        if (!McSocialApiManager.Instance.IsLoggedIn)
            McSocialApiManager.Instance.LoginAsGuest();

#if !UNITY_WEBPLAYER
        OnTheRunFacebookManager.Instance.InitFacebook();
#endif

        firstFrame = true;
        //this.StartCoroutine(this.LoadLevel());

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- UIPreloader Start");
    }

    void Update()
    {
        if (firstFrame)
        {
            this.StartCoroutine("LoadLevel");

            firstFrame = false;
        }
    }

    void PreloaderSessionBegin()
    {
        sessionHasBegun = true;
    }

    IEnumerator LoadLevel()
    {

#if UNITY_WEBPLAYER
        while ( !canProcede ||
                !Application.CanStreamedLevelBeLoaded(sceneToLoad) )
            yield return new WaitForEndOfFrame();
#endif

        // HIDE LOADING BAR - For Taking the preloader snapshot
        //transform.Find("bar").gameObject.SetActive(false);

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- UIPreloader LoadLevel");
        loader.Play(loaderClip);
        loader[loaderClip].speed = 0.0f;
        loader[loaderClip].normalizedTime = 0.0f;

        asyncLoad = Application.LoadLevelAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            loader[loaderClip].normalizedTime = asyncLoad.progress * 0.5f;

            yield return new WaitForEndOfFrame();
        }
        asyncLoad = null;
        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- UIPreloader Level Loaded");
		
		SBS.Miniclip.UtilsBindings.ConsoleLog("***** SCENE LOADED!!!! ");

        asyncLoad = Application.LoadLevelAdditiveAsync("scene_eu");
        while (!asyncLoad.isDone)
        {
            loader[loaderClip].normalizedTime = 0.5f + asyncLoad.progress * 0.5f;

            yield return new WaitForEndOfFrame();
        }
        asyncLoad = null;

        GameObject.FindGameObjectWithTag("GameplayManagers").GetComponent<OnTheRunGameplay>().SendMessage("LoadNewLevel", true);

        loader[loaderClip].normalizedTime = 1.0f;

        while (!sessionHasBegun)
            yield return new WaitForEndOfFrame();
		
		OnTheRunChartboostManager.Instance.SetConfigParameters(OnTheRunDataLoader.Instance.GetChartboostEnabled());
        //GameObject.Destroy(gameObject);

        //SBS.Miniclip.UtilsBindings.ConsoleLog("DEBUG LOG -- UIPreloader GoToPage " + pageToLoad);
        Manager<UIManager>.Get().GoToPage(pageToLoad);
    }
}
