using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UIAtlasImporter
{
    [MenuItem("Assets/Interfaces/Import Atlas", validate = true)]
    static bool Validate()
    {
        return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets).Length > 0;
    }

    static void GetAtlasPrefab(string atlasPath, out GameObject obj, out Object prefab)
    {
        // Find prefab for storing atlas data
        string prefabPath = atlasPath.Substring(0, atlasPath.LastIndexOf(".")) + ".prefab";
        prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
        if (prefab == null)
            prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

        // Create prefab if it doesnt exist
        if (prefab as GameObject != null)
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        else
            obj = new GameObject();
        obj.name = System.IO.Path.GetFileNameWithoutExtension(atlasPath);

        // Make sure there's a BitmapFont component on it
        UIAtlas atlas = obj.GetComponent<UIAtlas>();
        if (atlas == null)
            atlas = obj.AddComponent<UIAtlas>();
    }

    [MenuItem("Assets/Interfaces/Import Atlas")]
    static void Import()
    {
        Object[] cuts = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        List<Texture2D> cutsList = new List<Texture2D>();
        string basePath = null,
               dirName = null;
        foreach (Texture2D cut in cuts)
        {
            if (null == cut)
                continue;
            cutsList.Add(cut);
            string path = AssetDatabase.GetAssetPath(cut);

            if (null == basePath)
            {
                basePath = path.Substring(0, path.LastIndexOf("/"));
                int index = basePath.LastIndexOf("/");
                dirName = basePath.Substring(index + 1);
                basePath = basePath.Substring(0, index);
            }

            // setup import settings
            TextureImporter texImp = TextureImporter.GetAtPath(path) as TextureImporter;
            texImp.textureType = TextureImporterType.Advanced;
            texImp.npotScale = TextureImporterNPOTScale.None;
            texImp.mipmapEnabled = false;
            texImp.isReadable = true;
            texImp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        Texture2D[] cutsArray = cutsList.ToArray();

        Texture2D atlas = new Texture2D(0, 0);
        int[] sizes = { 256, 512, 1024, 2048, 4096 };
        int sizeCounter = 0, lastMaxSize = 0, i;
        Rect[] cutsImageRect = null;
        bool packOk = false;
        while (!packOk)
        {
            if (sizeCounter == sizes.Length)
                break;

            lastMaxSize = sizes[sizeCounter++];
            cutsImageRect = atlas.PackTextures(cutsArray, 0, lastMaxSize);

            packOk = true;
            for (i = 0; i < cutsArray.Length; ++i)
            {
                int dw = Mathf.RoundToInt(cutsImageRect[i].width * atlas.width) - cutsArray[i].width,
                    dh = Mathf.RoundToInt(cutsImageRect[i].height * atlas.height) - cutsArray[i].height;

                packOk = packOk && (0 == dw && 0 == dh);

                if (!packOk)
                    break;
            }

            if (packOk)
                break;
        }

        if (!packOk)
        {
            Debug.LogError("Coudn't create atlas, too many/big textures");
            return;
        }

        byte[] pngData = atlas.EncodeToPNG();
        string atlasPath = basePath + "/" + dirName + ".png";// "_atlas.png";

        System.IO.File.WriteAllBytes(atlasPath, pngData);
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter atlasImp = TextureImporter.GetAtPath(atlasPath) as TextureImporter;
        atlasImp.textureType = TextureImporterType.Advanced;
        atlasImp.mipmapEnabled = false;
        atlasImp.isReadable = false;
        atlasImp.linearTexture = true;
        atlasImp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        atlasImp.wrapMode = TextureWrapMode.Clamp;
        atlasImp.filterMode = FilterMode.Point;
        atlasImp.maxTextureSize = lastMaxSize;
        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport);

        GameObject obj;
        Object prefab;
        GetAtlasPrefab(atlasPath, out obj, out prefab);

        UIAtlas atlasComp = obj.GetComponent<UIAtlas>();
        atlasComp.atlas = AssetDatabase.LoadAssetAtPath(atlasPath, typeof(Texture2D)) as Texture2D;

        Debug.Log("Atlas Size: " + atlasComp.atlas.width + "x" + atlasComp.atlas.height);

        atlasComp.capacity = 256; // DEFAULT
        atlasComp.cuts = new UIAtlas.CutData[cutsImageRect.Length];
        for (i = 0; i < cutsArray.Length; ++i)
        {
            atlasComp.cuts[i] = new UIAtlas.CutData();
            atlasComp.cuts[i].name = cutsArray[i].name;
            atlasComp.cuts[i].rect = new Rect(
                Mathf.RoundToInt(cutsImageRect[i].x * atlasComp.atlas.width),
                Mathf.RoundToInt((1.0f - cutsImageRect[i].y - cutsImageRect[i].height) * atlasComp.atlas.height),
                Mathf.RoundToInt(cutsImageRect[i].width * atlasComp.atlas.width),
                Mathf.RoundToInt(cutsImageRect[i].height * atlasComp.atlas.height));
        }

        PrefabUtility.ReplacePrefab(obj, prefab);
        GameObject.DestroyImmediate(obj);
    }
}
