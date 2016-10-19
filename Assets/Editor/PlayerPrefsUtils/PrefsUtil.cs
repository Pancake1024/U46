using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class PrefsUtil : EditorWindow
{
	[MenuItem("SBS/Player Prefs/Clear All")]
	public static void ShowWindow()
	{
		PlayerPrefs.DeleteAll();
	}
}

