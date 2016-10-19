using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoidOmniataImplementation : MonoBehaviour, OmniataImplementation
{
    public bool ProcessEvents { get { return false; } set { } }
    public void Initialize(string apiKey, string userId) { }
	public void TrackSessionBegin() { }
	public void TrackSessionBegin(string idfaKey, string idFaValue) { }
    public void TrackEvent(string eventName, Dictionary<string, string> parameters) { }
}