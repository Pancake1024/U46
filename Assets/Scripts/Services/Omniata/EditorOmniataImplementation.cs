using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorOmniataImplementation : MonoBehaviour, OmniataImplementation
{
    public bool ProcessEvents
    {
        get
        {
            return true;
        }
        set
        { }
    }

    public void Initialize(string apiKey, string userId)
    {
        Debug.Log("###################################################################################### Editor Omniata - Initialize - apiKey: " + apiKey + " - userId: " + userId);
    }

    public void TrackSessionBegin()
    {
        Debug.Log("###################################################################################### Editor Omniata - Session Begin - om_load");
    }
	
	public void TrackSessionBegin(string idfaKey, string idFaValue)
	{
		Debug.Log("###################################################################################### Editor Omniata - Session Begin - om_load - idfaKey: " + idfaKey + " - idFaValue: " + idFaValue);
	}

    public void TrackEvent(string eventName, Dictionary<string, string> parameters)
    {
        string logStr = "###################################################################################### Editor Omniata - TrackEvent - name: " + eventName;

        if (parameters != null)
        {
            foreach (var par in parameters)
                logStr += "\n - " + par.Key + ": " + par.Value;
        }

        Debug.Log(logStr);
    }
}