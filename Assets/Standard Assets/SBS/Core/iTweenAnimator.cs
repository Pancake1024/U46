using System;
using UnityEngine;

public class iTweenAnimator : MonoBehaviour
{
	//public delegate void Callback(Vector3 v);
	
	public Action<object> onStart;
	public Action<object> onUpdate;
	public Action<object> onComplete;

    public Action<object, object> onStart2;
    public Action<object, object> onUpdate2;
    public Action<object, object> onComplete2;
}

