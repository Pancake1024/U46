using UnityEngine;
using System.Collections;
//------------------------------------------------------------//
public class TextStroke : MonoBehaviour 
{
	public UITextField RealText;
	public UITextField Stroke;

	private bool m_bInitialized = false;
	private bool m_bUpdateAlpha = false;
	private Color StrokeColor = Color.black;
	//------------------------------------------------------------//
	void Awake ()
	{
		if( Stroke == null || RealText == null )
		{
			Debug.LogWarning("TextStroke on object [ "+gameObject.name+" ] is missing references!");
			return;
		}
		Stroke.text = "";
		m_bUpdateAlpha = RealText.ApplyParametersType == UITextField.ApplyTypes.ALWAYS_APPLY;
	}
	//------------------------------------------------------------//
	void Start ()
	{
		Stroke.text = RealText.text;
	}
	//------------------------------------------------------------//
	void Update()
	{
		Profiler.BeginSample("TextStroke::Update()");

		if( Stroke == null || RealText == null )
		{
			Debug.LogWarning("TextStroke on object [ "+gameObject.name+" ] is missing references!");
			return;
		}

		if( m_bInitialized == false )
		{
			m_bInitialized = true;
			Stroke.text = RealText.text;
		}

        if (m_bInitialized && StrokeColor.a > 0 && !Stroke.text.Equals(RealText.text))
        {
            Stroke.text = RealText.text;
            Stroke.ApplyParameters();
        }

		if( m_bInitialized && m_bUpdateAlpha )
        {
            bool alphaCheck = StrokeColor.a != RealText.color.a;
            bool colorCheck = Stroke.color != StrokeColor;
            bool sizeCheck = Stroke.size != RealText.size;

            if (alphaCheck)
                StrokeColor.a = RealText.color.a;
            if (colorCheck)
                Stroke.color = StrokeColor;
            if (sizeCheck)
                Stroke.size = RealText.size;
            if (alphaCheck || colorCheck || sizeCheck)
                Stroke.ApplyParameters();
		}

		Profiler.EndSample();
	}
	//------------------------------------------------------------//
	void OnLanguageChanged( string strLang )
	{
		Stroke.text = RealText.text;
	}
	//------------------------------------------------------------//
	void OnDisable()
	{
		if( Stroke != null )
		Stroke.text = "";
	}
	//------------------------------------------------------------//
}
