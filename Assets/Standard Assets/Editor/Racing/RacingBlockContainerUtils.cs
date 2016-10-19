using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class RacingBlockContainerUtils : EditorWindow
{
    public string[] blockNames;
    public RacingBlock[] blocks;

    [MenuItem("SBS/Racing Blocks")]
    static void Init()
    {
        EditorWindow.GetWindow<RacingBlockContainerUtils>("Racing Blocks", true);
    }

    protected void RefreshBlocks()
    {
        string[] files = Directory.GetFiles(Application.dataPath + "/Prefabs", "*.prefab", SearchOption.AllDirectories);

        List<RacingBlock> blocksList = new List<RacingBlock>();
        foreach (string file in files)
        {
            string assetPath = file.Substring(file.IndexOf("Assets"));
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
            if (null == obj)
                continue;
            RacingBlock block = (obj as GameObject).GetComponent<RacingBlock>();
            if (null == block)
                continue;
            blocksList.Add(block);
        }

        int count = blocksList.Count;
        blockNames = new string[count];
        blocks = new RacingBlock[count];
        for (int i = 0; i < count; ++i)
        {
            blockNames[i] = blocksList[i].name;
            blocks[i] = blocksList[i];
        }
    }

    void OnEnable()
    {
        this.RefreshBlocks();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Refresh Blocks List"))
            this.RefreshBlocks();
    }
}
