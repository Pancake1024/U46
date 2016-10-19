using UnityEngine;
using System.Collections;
using omniata;
using System.Collections.Generic;

#if !UNITY_WEBPLAYER && !UNITY_WP8

public class UnitySdkOmniataImplementation : MonoBehaviour, OmniataImplementation
{
    OmniataComponent omniataComp;

    void Awake()
    {
        omniataComp = gameObject.AddComponent<OmniataComponent>();
    }

    public bool ProcessEvents
    {
        get
        {
            return omniataComp.ProcessEvents;
        }

        set
        {
            omniataComp.ProcessEvents = value;
        }
    }

    public void Initialize(string apiKey, string userId)
    {
		bool debug = false;

        omniataComp.LogLevel(LogLevel.ERROR);
        omniataComp.Initialize(apiKey, userId, debug, true, new DefaultReachability());
    }

    public void TrackSessionBegin()
    {
        omniataComp.TrackLoad();
    }

	public void TrackSessionBegin(string idfaKey, string idFaValue)
	{
		Dictionary<string, string> paramsDict = new Dictionary<string, string>();
		paramsDict.Add(idfaKey, idFaValue);
		omniataComp.TrackLoad(paramsDict);
	}

    public void TrackEvent(string eventName, Dictionary<string, string> parameters)
    {
        omniataComp.Track(eventName, parameters);
    }
}

#endif