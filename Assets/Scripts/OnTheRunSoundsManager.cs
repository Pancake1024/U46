using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Miniclip;
using System.Collections;

public class OnTheRunSoundsManager : MonoBehaviour
{
	#region Singleton instance
	protected static OnTheRunSoundsManager instance = null;
	
	public static OnTheRunSoundsManager Instance
	{
		get
		{
			return instance;
		}
	}
	#endregion
	
	#region Public members
	[ System.NonSerialized ]
	public AudioClip musicClip = null;

	bool pauseMusic = true;
	bool useLevelRootToFindSources = true;
	#endregion
	
	
	public float MusicVolume
	{
		get{ return musicVol; }
		set
		{
			musicVol = value;
			UpdateMusicOnOff();
		}
	}
	float musicVol = 1f;

	#region Protected members
	protected bool musicOn = true;
	protected bool soundsOn = true;
	protected bool deviceMusicPlaying = false;
	protected Dictionary<AudioSource, bool> wasPlaying = new Dictionary<AudioSource,bool>();
	#endregion
	
	#region Public properties
	public bool MusicActive
	{
		get
		{
			return musicOn;
		}
		set
		{
			if (value != musicOn)
				this.ToggleMusicOnOff();
		}
	}
	
	public bool SoundsActive
	{
		get
		{
			return soundsOn;
		}
		set
		{
			if (value != soundsOn)
				this.ToggleSoundsOnOff();
		}
	}
	#endregion
	
	#region Virtual functions
	protected virtual void ToggleMusicOnOff()
	{
		musicOn = !musicOn;
		PlayerPrefs.SetInt("sm_mu", musicOn ? 1 : 0);
		
		UpdateMusicOnOff();
	}
	
	protected void UpdateMusicOnOff()
	{
		if (null == musicClip)// || (pauseMusic && TimeManager.Instance.MasterSource.IsPaused))
			return;
		
		IEnumerable<AudioSource> sources = null;
		if (LevelRoot.Instance != null && useLevelRootToFindSources)
		{
			sources = LevelRoot.Instance.gameObject.GetComponentsInChildren<AudioSource>(true);
		}
		else
		{
			#if UNITY_4_3
			Transform[] transforms = (Transform[])Transform.FindObjectsOfType(typeof(Transform));
			#else
			Transform[] transforms = (Transform[])Transform.FindSceneObjectsOfType(typeof(Transform));
			#endif
			List<AudioSource> sourceList = new List<AudioSource>();
			sources = sourceList;
			foreach (Transform tr in transforms)
			{
				if (null == tr.parent)
				{
					AudioSource[] subSources = tr.gameObject.GetComponentsInChildren<AudioSource>(true);
					foreach (AudioSource source in subSources)
						sourceList.Add(source);
				}
			}
		}
		
		foreach (AudioSource source in sources)
		{
			if (source.clip == musicClip)
			{
				source.volume = musicVol;
				#if UNITY_IPHONE && !UNITY_EDITOR
				source.mute = !musicOn || deviceMusicPlaying;
				#else
				source.mute = !musicOn;
				#endif
			}
		}
	}
	
	protected virtual void ToggleSoundsOnOff()
	{
		soundsOn = !soundsOn;
		PlayerPrefs.SetInt("sm_sf", soundsOn ? 1 : 0);
		
		IEnumerable<AudioSource> sources = null;
		if (LevelRoot.Instance != null && useLevelRootToFindSources)
		{
			sources = LevelRoot.Instance.gameObject.GetComponentsInChildren<AudioSource>(true);
		}
		else
		{
			#if UNITY_4_3
			Transform[] transforms = (Transform[])Transform.FindObjectsOfType(typeof(Transform));
			#else
			Transform[] transforms = (Transform[])Transform.FindSceneObjectsOfType(typeof(Transform));
			#endif
			List<AudioSource> sourceList = new List<AudioSource>();
			sources = sourceList;
			foreach (Transform tr in transforms)
			{
				if (null == tr.parent)
				{
					AudioSource[] subSources = tr.gameObject.GetComponentsInChildren<AudioSource>(true);
					foreach (AudioSource source in subSources)
						sourceList.Add(source);
				}
			}
		}
		
		foreach (AudioSource source in sources)
		{
			if (null == source.clip || source.clip != musicClip)
			{
				source.mute = !soundsOn;
			}
		}
	}
	#endregion

	AudioSource oldMusic = null;
	float lastMusicTime = 0f;

	#region Public functions
	public void PlayMusicSource(AudioSource source)
	{
		if( oldMusic != null )
			oldMusic.Stop();

		source.time = 0f;
		source.Play();
		oldMusic = source;
		lastMusicTime = 0f;

		if (TimeManager.Instance.MasterSource.IsPaused)
		{
			if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame == false )
				if (pauseMusic)
					source.Pause();
		}

		if (!musicOn)
			source.mute = true;
		else
		{
			#if UNITY_IPHONE && !UNITY_EDITOR
			if (deviceMusicPlaying)
				source.mute = true;
			#endif
		}
	}

	public void PauseMusicForced()
	{
		oldMusic.Pause();
		lastMusicTime = oldMusic.time;

		AudioSource[] sources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach (AudioSource source in sources)
		{
			if (source.clip == musicClip)
			{
				if (pauseMusic)
					source.Pause();
				
				lastMusicTime = source.time;
			}
			else
			{
				if (wasPlaying.ContainsKey(source))
				{
					m_kTimes[source] = source.time;
					wasPlaying[source] = source.isPlaying;
				}
				else
				{
					m_kTimes.Add(source , source.time );
					wasPlaying.Add(source, source.isPlaying);
				}

				source.Pause();
			}
		}
	}

	public void ResumeForced()
	{
		OnResume();
	}

	public void StopMusicSource()
	{
		if( oldMusic != null )
			lastMusicTime = oldMusic.time;

		if (null == musicClip)
			return;

		if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame )
			return;
		
		IEnumerable<AudioSource> sources = null;
		if (LevelRoot.Instance != null && useLevelRootToFindSources)
		{
			sources = LevelRoot.Instance.gameObject.GetComponentsInChildren<AudioSource>(true);
		}
		else
		{
			#if UNITY_4_3
			Transform[] transforms = (Transform[])Transform.FindObjectsOfType(typeof(Transform));
			#else
			Transform[] transforms = (Transform[])Transform.FindSceneObjectsOfType(typeof(Transform));
			#endif
			List<AudioSource> sourceList = new List<AudioSource>();
			sources = sourceList;
			foreach (Transform tr in transforms)
			{
				if (null == tr.parent)
				{
					AudioSource[] subSources = tr.gameObject.GetComponentsInChildren<AudioSource>(true);
					foreach (AudioSource source in subSources)
						sourceList.Add(source);
				}
			}
		}

		foreach (AudioSource source in sources)
		{
			if (source.clip == musicClip)
			{
				source.Stop();
			}
		}

		musicClip = null;
	}
	
	public void PlaySource(AudioSource source, bool bPlayInPause = false )
	{
		if( TimeManager.Instance.MasterSource.IsPaused == false )
		{
			source.time = 0f;
			source.Play();
		}
		else
		{
			if( bPlayInPause && soundsOn )
			{
				float fOld = Time.timeScale;
				
				Time.timeScale = 1;
				AudioSource.PlayClipAtPoint(source.clip, Camera.main.transform.position, 1.0f ); 
				Time.timeScale = fOld;
			}
		}

		if (!soundsOn)
			source.mute = true;
	}

	
	public void StopSource(AudioSource source)
	{
		if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame == false )
		    source.Stop();

		if (TimeManager.Instance.MasterSource.IsPaused)
		{
			if (wasPlaying.ContainsKey(source))
			{
				m_kTimes[source] = source.time;
				wasPlaying[source] = source.isPlaying;
			}
			else
			{
				m_kTimes.Add(source , source.time );
				wasPlaying.Add(source, source.isPlaying);
			}
		}
	}
	
	public void UpdateDeviceMusicPlaying()
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		deviceMusicPlaying = UtilsBindings.IsMusicPlaying();
		#endif
		UpdateMusicOnOff();
	}
	#endregion
	
	#region Messages
	void OnPause()
	{
		if( oldMusic != null )
			lastMusicTime = oldMusic.time;

		AudioSource[] sources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach (AudioSource source in sources)
		{
			if (source.clip == musicClip )
			{
				if (pauseMusic && OnTheRunTutorialManager.Instance.TutorialActiveAndIngame == false )// && musicOn)
					source.Pause();
			}
			else/* if (soundsOn)*/
			{
				if (wasPlaying.ContainsKey(source))
				{
					m_kTimes[source] = source.time;
					wasPlaying[source] = source.isPlaying;
				}
				else
				{
					m_kTimes.Add(source , source.time );
					wasPlaying.Add(source, source.isPlaying);
				}

				if( OnTheRunTutorialManager.Instance.TutorialActiveAndIngame == false )
				source.Pause();
			}
		}
	}

	Dictionary<AudioSource, float> m_kTimes = new Dictionary<AudioSource, float>();

	void OnResume()
	{
		AudioSource[] sources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		foreach (AudioSource source in sources)
		{
			if (source.clip == musicClip)
			{
				if( source.isPlaying == false )
				{
					source.time = lastMusicTime;

					if (pauseMusic)// && musicOn)
						source.Play();
				}
			}
			else/* if (soundsOn)*/
			{
				bool playAgain = true;
				wasPlaying.TryGetValue(source, out playAgain);
				if (playAgain)
				{
					source.time = m_kTimes[source];
					source.Play();
				}
			}
		}
	}
	#endregion
	
	#region Unity Callbacks
	void Awake()
	{
		Asserts.Assert(null == instance);
		instance = this;
		
		#if UNITY_IPHONE && !UNITY_EDITOR
		deviceMusicPlaying = UtilsBindings.IsMusicPlaying();
		#endif
		musicOn = PlayerPrefs.GetInt("sm_mu", 1) == 1;
		soundsOn = PlayerPrefs.GetInt("sm_sf", 1) == 1;
	}
	
	void OnDestroy()
	{
		Asserts.Assert(this == instance);
		instance = null;
	}
	
	
	void OnApplicationPause(bool paused)
	{
		if ( paused || TimeManager.Instance.MasterSource.IsPaused )
		{
			AudioSource[] sources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
			foreach (AudioSource source in sources)
			{
				if (source.clip == musicClip)
				{
					if (pauseMusic)
						source.Pause();

					lastMusicTime = source.time;
				}
				else
				{
					source.Pause();
				}
			}
		}
	}
	#endregion
}
