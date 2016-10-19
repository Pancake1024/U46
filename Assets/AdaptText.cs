using UnityEngine;
using System.Collections;

public class AdaptText : MonoBehaviour 
{
	public int CharCount;
	public int WantedSize;
	//----------------------------------------//
	int defaultSize;
	UITextField textfield = null;
	//----------------------------------------//
	void Awake()
	{
		textfield = GetComponent<UITextField>();
		defaultSize = textfield.size;
	}
	//----------------------------------------//
	void Update () 
	{
        int targetSize = textfield.text.Length > CharCount ? WantedSize : defaultSize;
        if (targetSize != textfield.size)
        {
            textfield.size = targetSize;
            textfield.ApplyParameters(true);
        }

		/*if( textfield.text.Length > CharCount )
		{
			textfield.size = WantedSize;
			textfield.ApplyParameters();
		}
		else
		{
			textfield.size = defaultSize;
			textfield.ApplyParameters();
		}*/
	}
	//----------------------------------------//
}
