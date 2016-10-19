using System;
using UnityEngine;
using SBS.Core;
using SBS.Miniclip;

public class OnTheRunNewsIntegration : MonoBehaviour
{
	#region Singleton instance
	protected static OnTheRunNewsIntegration instance = null;
	
	public static OnTheRunNewsIntegration Instance
	{
		get
		{
			return instance;
		}
	}
	#endregion

    public int UnreadNews { get { return unreadNews; } }
    public int TotalNews { get { return totalNews; } }

    int unreadNews = 0;
    int totalNews = 0;

	void Awake()
	{
		Asserts.Assert(null == instance);
		instance = this;
		
		GameObject.DontDestroyOnLoad(gameObject);
		RegisterDispatcherEvents();
#if UNITY_EDITOR
        EditorTest_ResetNumNews();
#endif
	}
	
	void OnDestroy()
	{
		Asserts.Assert(this == instance);
		instance = null;
	}

    void RegisterDispatcherEvents()
    {
        ObjCDispatcher.Instance.RegisterEvent("MCUtilsAvailabilityChanged", onAvailabilityChanged);
        ObjCDispatcher.Instance.RegisterEvent("MCUtilsNewMessages", onNewMessagesChanged);
        ObjCDispatcher.Instance.RegisterEvent("MCUtilsNewAppAvailable", onNewApplicationVersionAvailable);

        //ObjCDispatcher.Instance.RegisterEvent("MCUtilsShouldShowUrgentMessage", onShouldShowUrgentMessage);
    }

	public void onAvailabilityChanged(string param)
	{
		UpdateMessagesCount();
        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>().UpdateNewsfeedButton();
	}
	
	public void onNewMessagesChanged(string param)
	{
        UpdateMessagesCount();
        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>().UpdateNewsfeedButton();
	}

	public void onNewApplicationVersionAvailable(string param)
	{
	}

	/*public void onShouldShowUrgentMessage(string param)
    {
        Debug.Log("onShouldShowUrgentMessage: " + param);
    }*/
	
	public void UpdateMessagesCount()
	{
		int a = -1, b = -1;
		SBS.Miniclip.MCUtilsBindings.GetMessagesCount(ref a, ref b);
		
		unreadNews = a;
		totalNews = b;
	}

#if UNITY_EDITOR
    public void EditorTest_ResetNumNews()
    {
        unreadNews = 10;
        totalNews = 10;

        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>().UpdateNewsfeedButton();
    }

    public void EditorTest_ReadNews()
    {
        unreadNews--;
        if (unreadNews < 0)
            unreadNews = 0;

        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>().UpdateNewsfeedButton();
    }

    public void EditorTest_SetZeroNews()
    {
        unreadNews = 0;
        totalNews = 0;

        if (Manager<UIManager>.Get().ActivePageName == "StartPage")
            Manager<UIManager>.Get().ActivePage.gameObject.GetComponent<UIStartPage>().UpdateNewsfeedButton();
    }
#endif
}