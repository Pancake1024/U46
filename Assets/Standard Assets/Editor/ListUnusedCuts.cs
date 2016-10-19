
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.IO;
//using System.Collections.Generic;

public class ListUnusedCuts : MonoBehaviour
{
    static string spritesPath = "Assets/UI/graphic/";
    static string spritesExt = "*.png";
    static string uiPrefabPath = "Assets/Prefabs/UI/";


    [MenuItem("Tools/List Unused Cuts")]
    public static void FindSpritesInPrefabsAssets()
    {
        HashSet<string> sprites = new HashSet<string>();
        HashSet<string> spritesInPrefabs = new HashSet<string>();
        HashSet<string> unusedSprites = new HashSet<string>();

        //SPRITES
        string[] spritesPaths = Directory.GetFiles(spritesPath, spritesExt, SearchOption.AllDirectories);
        foreach (string s in spritesPaths)
        {
            int startIndex = s.LastIndexOf('/') + 1, endIndex = s.LastIndexOf('.');
            string sn = s.Substring(startIndex, endIndex - startIndex);
            sprites.Add(sn);
        }
        
        //PREFABS
        string[] prefabsPaths = Directory.GetFiles(uiPrefabPath, "*.prefab", SearchOption.AllDirectories);

        //Debug.Log("PREFAB 0 NAME: " + prefabsPaths[0]);
        foreach (string pref in prefabsPaths)
        {
            //Debug.Log("Prefab: " + pref);
            GameObject p = (GameObject)AssetDatabase.LoadAssetAtPath(pref, typeof(GameObject));

            GameObject instP = Instantiate(p) as GameObject;

            SpriteRenderer[] sr = instP.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < sr.Length; i++)
            {
                if(sr[i]!=null && sr[i].sprite != null)
                {
                    if (!spritesInPrefabs.Contains(sr[i].sprite.texture.name))
                        spritesInPrefabs.Add(sr[i].sprite.texture.name);
                }
            }

            UINineSlice[] ns = instP.GetComponentsInChildren<UINineSlice>(true);
            for (int i = 0; i < ns.Length; i++)
            {
                if (ns[i] != null && ns[i].sprite != null)
                {
                    if (!spritesInPrefabs.Contains(ns[i].sprite.texture.name))
                        spritesInPrefabs.Add(ns[i].sprite.texture.name);
                }
            }

            DestroyImmediate(instP);
        }

        
        foreach (var s in sprites)
            Debug.Log("* sprite: " + s);

        foreach (var s in spritesInPrefabs)
            Debug.Log("** spritesInPrefabs: " + s);
        

        sprites.ExceptWith(spritesInPrefabs);
        foreach (var s in sprites)
            Debug.Log("UNUSED sprite " + s);

        Debug.Log("DONE!!!!!");
    }
}
