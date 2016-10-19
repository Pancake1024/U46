using UnityEngine;
using System.Collections;

public class AdvRootBehaviour : MonoBehaviour {

	void Awake ()
    {
        DontDestroyOnLoad(gameObject);
	}
}
