using UnityEngine;
using com.miniclip;
using System;

[AddComponentMenu("SBS/Miniclip/MiniclipAPIManager")]
public class MiniclipAPIManager : MonoBehaviour
{
    #region Internal Structs
    public struct HighscoreData
    {
        public uint level;
        public string levelName;

        public HighscoreData(uint l, string ln)
        {
            level = l;
            levelName = ln;
        }
    }
    #endregion

    protected MiniclipAPI miniclipAPI;

    protected string url_store_IOS = "https://itunes.apple.com/ca/app/strawberry-shortcake-berry/id870423065"; //TODO STORE LINKS
    protected string url_store_Google = "https://play.google.com/store/apps/details?id=com.miniclip.berryrush";
    protected string url_store_Windows = "http://www.windowsphone.com/s?appId=8d430f1a-2739-4f22-87fa-eb1dcf75a899";
    protected string url_store_Amazon = "http://www.amazon.com/gp/mas/dl/android?p=com.miniclip.berryrushamazon";

    #region Public Methods
    public string Url_store_IOS
    {
        get { return url_store_IOS; }
    }

    public string Url_store_Google
    {
        get { return url_store_Google; }
    }

    public string Url_store_Windows
    {
        get { return url_store_Windows; }
    }

    public string Url_store_Amazon
    {
        get { return url_store_Amazon; }
    }
    #endregion

    #region Unity Callbacks
    void Start ()
    {
        GameObject go = GameObject.Find("MiniclipAPI");
        miniclipAPI = (null == go) ? null : GameObject.Find("MiniclipAPI").GetComponent<MiniclipAPI>();

        if (miniclipAPI != null)
        {
            miniclipAPI.HighscoresClosed += OnHighscoreClose;
            //miniclipAPI.ParametersReceived += OnParametersReceived; API 0.0.94 -> 0.1.27
            miniclipAPI.EmbedVariablesReceived += OnEmbedVariablesReceived;

            GetParameters();
        }
	}
	/*
	void Update ()
    {

    }
    */
    #endregion

    #region Miniclip Callbacks
    public void OnHighscoreClose(object sender, EventArgs args)
    {
        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("ShowContinueButtons", SendMessageOptions.DontRequireReceiver);
        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("UnlockInput", SendMessageOptions.DontRequireReceiver);
    }

    public void OnEmbedVariablesReceived(object sender, EmbedVariablesEventArgs args)
    {
        //string lang = args.EmbedVariables.Dictionary["mc_lang"] as string;

        Manager<UIManager>.Get().ActivePage.GetComponent<UIPreloader>().canProcede = true;

		//LocalizationsManager.ForcedLang = lang;
		//LocalizationsManager.CurrentLanguage = lang;

        string url_IOS_embed = args.EmbedVariables.Dictionary["4187_ios_url"] as string; //TODO STORE LINKS
        if (url_IOS_embed.Length > 0)
            url_store_IOS = url_IOS_embed;

        string url_Google_embed = args.EmbedVariables.Dictionary["4187_google_url"] as string;
        if (url_Google_embed.Length > 0)
            url_store_Google = url_Google_embed;

        string url_Windows_embed = args.EmbedVariables.Dictionary["4187_windows_url"] as string;
        if (url_Windows_embed.Length > 0)
            url_store_Windows = url_Windows_embed;

        string url_Amazon_embed = args.EmbedVariables.Dictionary["4187_amazon_url"] as string;
        if (url_Amazon_embed.Length > 0)
            url_store_Amazon = url_Amazon_embed;
    }
    #endregion

    #region Messages
    public void ShowHighscores(HighscoreData hd)
    {
        Debug.Log("ShowHighscores");
        if (!IsOnMiniclip())
            return;

        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("LockInput", SendMessageOptions.DontRequireReceiver);

        if (hd.level > 0)
        {
            if (hd.levelName.Length > 0)
            {
                miniclipAPI.ShowHighscores(hd.level, hd.levelName);
            }
            else
            {
                miniclipAPI.ShowHighscores(hd.level);
            }
        }
        else
        {
            miniclipAPI.ShowHighscores();
        }
    }

    public void SaveHighscore(int score)
    {
        if (!IsOnMiniclip())
            return;

        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("LockInput", SendMessageOptions.DontRequireReceiver);
        miniclipAPI.SaveHighscore(score);
    }

    public void SaveHighscore(int score, uint level)
    {
        if (!IsOnMiniclip())
            return;

        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("LockInput", SendMessageOptions.DontRequireReceiver);
        miniclipAPI.SaveHighscore(score, level);
    }

    public void SaveHighscore(int score, uint level, string levelName)
    {
        if (!IsOnMiniclip())
            return;

        GameObject.FindGameObjectWithTag("UICameraUnity").BroadcastMessage("LockInput", SendMessageOptions.DontRequireReceiver);
        miniclipAPI.SaveHighscore(score, level, levelName);
    }

    public void GetParameters()
    {
        if (!IsOnMiniclip())
            return;

        miniclipAPI.GetEmbedVariables();
    }
    #endregion

    #region Utilities
    public static bool IsOnMiniclip()
    {
        bool domainCheck = !Application.absoluteURL.Contains("silentbaystudios.com") && !Application.absoluteURL.Contains("testaluna.it") && !Application.absoluteURL.StartsWith("file:");
        bool editorCheck = !Application.isEditor;

        return (domainCheck && editorCheck);
    }
    #endregion
}
