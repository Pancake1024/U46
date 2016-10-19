using UnityEngine;
using SBS.Core;
using System.Collections.Generic;

[AddComponentMenu("OnTheRun/UI/UIRankPlayer")]
public class UIRankPlayer : MonoBehaviour
{
    public UITextField rankTagTextfield;
    public SpriteRenderer icon;
    public ParticleSystem fx;

    #region Unity callbacks
    public void Initialize(Sprite iconSprite, string text=null)
	{
        if (rankTagTextfield != null && text!=null)
        {
            rankTagTextfield.text = text;
            rankTagTextfield.ApplyParameters();
        }
        if (iconSprite!=null)
            icon.sprite = iconSprite;

        if (fx != null)
        {
            fx.playOnAwake = true;
            fx.gameObject.SetActive(false);
        }
    }

	[ System.NonSerialized ]
	public bool fxPlaying = false;

    public void StartFX()
    {
		if (fx != null && fxPlaying == false )
		{
//			Debug.LogWarning("---> START FX");
			fxPlaying = true;
			fx.gameObject.SetActive(true);
			fx.Play();
		}
    }

    public void StopFX()
    {
		if (fx != null && fxPlaying )
		{
//			Debug.LogWarning("---> STOP FX");
			fxPlaying = false;
            fx.Stop();
		}
    }
    #endregion

}
