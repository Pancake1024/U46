using UnityEngine;
using System.Collections;
//------------------------------------------------//
public class StopAnimOnButtonDisable : MonoBehaviour
{
	public UIButton Button;
	public Animation[] Animations;
	//------------------------------------------------//
	public bool ResumeOnEnable = true;
	public bool RewindOnStop = true;
	public bool RewindOnEnable = true;
	//------------------------------------------------//
	void Start () 
	{
		if( Button == null )
		{
			Debug.LogError("Missing button reference!");
		}

		if( Animations == null || Animations.Length == 0  )
		{
			Debug.LogError("No Animations found!");
		}

		Button.onDisableEvent.AddTarget( gameObject , "StopAnim" );
		Button.onEnableEvent.AddTarget( gameObject , "ResumeAnim" );
	}
	//------------------------------------------------//
	void StopAnim( UIButton pButton )
	{
		int iLen =  Animations.Length;
		
		for( int i = 0 ; i< iLen ; ++i )
		{
			Animation pAnim = Animations[i];

			if( RewindOnStop )
			{
				pAnim.Rewind();
				pAnim.Stop();
				pAnim[pAnim.clip.name].normalizedTime = 0f;
				pAnim.Sample();
			}
			else
				pAnim.Stop();
		}
	}
	//------------------------------------------------//
	void ResumeAnim( UIButton pButton )
	{
		int iLen =  Animations.Length;

		for( int i = 0 ; i< iLen ; ++i )
		{
			Animation pAnim = Animations[i];

			if( RewindOnEnable )
			{
				pAnim.Rewind();
				pAnim.Play();
				pAnim[pAnim.clip.name].normalizedTime = 0f;
				pAnim.Sample();
			}
			else
				pAnim.Play();
		}
	}
	//------------------------------------------------//
}
