using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ChangeFontUtils : EditorWindow
{
    string fileName = "FileName";
    public Font fontSelected;

    [MenuItem("SBS/Change Font")]
    static void Init()
    {
        EditorWindow.GetWindow<ChangeFontUtils>("Change Font", true);
    }

    protected void SetupFontWindow()
    {
        UnityEngine.Object[] components = Resources.FindObjectsOfTypeAll(typeof(UITextField));
        for (int i = 0; i < components.Length; ++i)
        {
            UITextField currTxtFld = (components[i] as UITextField);
            currTxtFld.font = fontSelected;
            currTxtFld.size = (int)(currTxtFld.size * 0.9f);
        }
    }

    void OnEnable()
    {
        //this.SetupFontWindow();
    }

    void OnGUI()
    {
        fontSelected = (Font)EditorGUILayout.ObjectField("Select Font", fontSelected, typeof(Font));
        if (GUILayout.Button("Swap font"))
        {
            if (fontSelected != null)
            {
                this.SetupFontWindow();
            }
            else
            {
                Debug.Log("********** YOU MUST CHOOSE A FONT");
            }
        }
    }
}
