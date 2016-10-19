using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface OmniataImplementation
{
    bool ProcessEvents { get; set; }

    void Initialize(string apiKey, string userId);
    
    void TrackSessionBegin();
	void TrackSessionBegin(string idfaKey, string idFaValue);

    void TrackEvent(string eventName, Dictionary<string, string> parameters);
}