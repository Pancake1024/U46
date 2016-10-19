using UnityEngine;
using System.Collections;

public class ChangeParticleSystemSortingOrder : MonoBehaviour 
{
	public int NewSortingOrder;

	// Use this for initialization
	void Start () 
	{
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = NewSortingOrder;
	}
}
