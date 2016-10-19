using UnityEngine;
using System.Collections;

public class AnimationRandomTime : MonoBehaviour 
{
	void Start () 
	{
		Animation pAnim = GetComponent<Animation>();

		if( pAnim == null )
		{
			Debug.LogWarning("AnimationRandomTime -> animation component not found!");
			return;
		}

		pAnim[pAnim.clip.name].normalizedTime = Random.Range( 0.0f , 0.99f );
		pAnim.Play();
	}
}
