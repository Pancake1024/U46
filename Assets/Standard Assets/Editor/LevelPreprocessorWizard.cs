using System;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEditor;
using SBS.Level;

class LevelPreprocessorWizard : ScriptableWizard
{
    #region Public members
    public GameObject levelRoot = null;
    public TextAsset levelXml = null;
    #endregion

    #region Wizard callback
    [MenuItem("SBS/Preprocess level")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Level preprocessor", typeof(LevelPreprocessorWizard), "OK");
    }
    #endregion

    #region Unity callbacks
    void OnWizardUpdate()
    {
        helpString = "Optimize level geometry and apply components";
        if (null == levelRoot)
        {
            errorString = "Select level root node";
            isValid = false;
            return;
        }
        else if (levelRoot.transform.parent != null)
        {
            errorString = "Node must be a ROOT node (no parent)";
            isValid = false;
            return;
        }

        LevelRoot comp = levelRoot.GetComponent<LevelRoot>();
        if (null == comp)
        {
            errorString = "Node must have a LevelRoot component attached";
            isValid = false;
            return;
        }

        if (null == levelXml)
        {
            errorString = "Select level XML";
            isValid = false;
            return;
        }

        errorString = "";
        isValid = true;
    }

    void OnWizardCreate()
    {
        LevelPreprocessor preprocessor = new LevelPreprocessor();
        LevelRoot root = levelRoot.GetComponent<LevelRoot>();

        preprocessor.levelRoot = levelRoot;
        preprocessor.levelXml = levelXml;

        preprocessor.Run();

        Debug.Log("level bounds: " + preprocessor.WorldBounds);

        root.categories = preprocessor.Categories;
        root.layersName = preprocessor.LayersName;
        root.worldBoundsMin = preprocessor.WorldBounds.min;
        root.worldBoundsMax = preprocessor.WorldBounds.max;

        // map layers
        foreach (LevelPreprocessor.MapUnityLayerDesc desc in preprocessor.UnityLayersMaps)
            preprocessor.Layers.MapUnityLayer(desc.unityLayerName, desc.layersMask);
    }
    #endregion
}
