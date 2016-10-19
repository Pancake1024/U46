using UnityEngine;
using System.Collections;

public class UIGenericEvents : MonoBehaviour 
{
	// used in lock ui garage page when 
	// lock is unlocked 
	void UnlockAndShakeCam()
	{
		GameObject pCam = GameObject.FindGameObjectWithTag("MainCamera");

		// shake cam
		FollowCharacter.ShakeData sData = new FollowCharacter.ShakeData(0.2f, 0.4f, 0.5f);
		pCam.SendMessage("StartShakeCamera", sData);

		// play sound
		OnTheRunInterfaceSounds.Instance.PlayUnlockSound();
	}

	void PlayNewMissionSound()
	{
		OnTheRunSounds.Instance.PlayGeneralGameSound(OnTheRunSounds.GeneralGameSounds.MissionCompleted);
	}
}
