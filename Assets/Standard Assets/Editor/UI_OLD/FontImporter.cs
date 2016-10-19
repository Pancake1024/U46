using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FontImporter
{
    [MenuItem("Assets/Interfaces/Import Fonts", validate = true)]
    static bool Validate()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
            if (path.ToLower().EndsWith(".fnt"))
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Assets/Interfaces/Import Fonts")]
    static void Import()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            //Look for assets with the .fnt extension
            string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
            if (path.ToLower().EndsWith(".fnt"))
            {
                //Find prefab for storing bitmap font
                string basePath = path.Substring(0, path.LastIndexOf("."));
                string prefabPath = basePath;// +"_font";

                string oldName = null;
                int oldCapacityPerPage = -1;
                float oldExtraSpacing = -1.0f;
                
                Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath + ".prefab", typeof(GameObject));
                if (prefab == null)
                {
                    prefab = PrefabUtility.CreateEmptyPrefab(prefabPath + ".prefab");
                }

                //Create prefab if it doesnt exist
                GameObject obj = null;
                if (prefab as GameObject != null)
                {
                    obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    BitmapFont oldFont = obj.GetComponent<BitmapFont>();
                    if (oldFont != null)
                    {
                        oldName = oldFont.fontName;
                        oldCapacityPerPage = oldFont.capacityPerPage;
                        oldExtraSpacing = oldFont.extraSpacing;
                    }
                }
                else
                {
                    obj = new GameObject();
                }
                obj.name = prefabPath.Substring(prefabPath.LastIndexOf("/") + 1);

                //Make sure there's a BitmapFont component on it
                BitmapFont fnt = obj.GetComponent<BitmapFont>();
                if (fnt == null)
                {
                    fnt = obj.AddComponent<BitmapFont>();
                }

                //Read BitmapFont info from .fnt file
                UpdateBitmapFont(path, obj.GetComponent<BitmapFont>());

                if (oldName != null)
                {
                    BitmapFont newFont = obj.GetComponent<BitmapFont>();

                    newFont.fontName = oldName;
                    newFont.capacityPerPage = oldCapacityPerPage;
                    newFont.extraSpacing = oldExtraSpacing;
                }

                PrefabUtility.ReplacePrefab(obj, prefab);
                GameObject.DestroyImmediate(obj);
            }
        }
    }

    /* .fnt parser taken from 
     * 
     */
    static BitmapFont UpdateBitmapFont(string path, BitmapFont fnt)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        //Read font info 
        XmlNode info = doc.SelectSingleNode("/font/info");
        fnt.fontName = fnt.gameObject.name;
        fnt.fontFace = info.Attributes["face"].Value;
        fnt.padding = ReadRectOffsetAttribute(info, "padding");
        //fnt.Size = ReadFloatAttribute(info, "size");

        XmlNode common = doc.SelectSingleNode("/font/common");
        fnt.fontSize = ReadFloatAttribute(common, "base");
        fnt.lineHeight = ReadFloatAttribute(common, "lineHeight");
        /*fnt.Base = ReadFloatAttribute(common, "base");
        fnt.ScaleH = ReadFloatAttribute(common, "scaleW");
        fnt.ScaleW = ReadFloatAttribute(common, "scaleH");*/

        //Read character info
        XmlNodeList chars = doc.SelectNodes("/font/chars/char");
        fnt.chars = new BitmapFont.CharacterDesc[chars.Count];
        int index = 0;
        foreach (XmlNode char_node in chars)
        {
            fnt.chars[index] = new BitmapFont.CharacterDesc();
            fnt.chars[index].code = ReadIntAttribute(char_node, "id");/*
            fnt.chars[index].Position = ReadVector2Attributes(char_node, "x", "y");
            fnt.chars[index].Size = ReadVector2Attributes(char_node, "width", "height");*/
            fnt.chars[index].rect = new Rect(
                ReadFloatAttribute(char_node, "x"), ReadFloatAttribute(char_node, "y"),
                ReadFloatAttribute(char_node, "width") + 1, ReadFloatAttribute(char_node, "height") + 1);
            fnt.chars[index].offset = ReadVector2Attributes(char_node, "xoffset", "yoffset");
            fnt.chars[index].xAdvance = ReadFloatAttribute(char_node, "xadvance") + 1.0f;
            fnt.chars[index].batchIndex = ReadIntAttribute(char_node, "page");

            index++;
        }

        //Load texture pages and convert to distance fields
        XmlNodeList pages = doc.SelectNodes("/font/pages/page");
        fnt.pages = new Texture2D[pages.Count];
        index = 0;
        foreach (XmlNode page in pages)
        {
            //Find original font texture
            string imagePath = System.IO.Path.GetDirectoryName(path) + "/" + page.Attributes["file"].Value;
            fnt.pages[index++] = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath, typeof(Texture2D));
        }

        fnt.capacityPerPage = 256;//DEFAULT
        fnt.extraSpacing = 0.0f;//DEFAULT

        //Create texture atlas
/*      Texture2D pageAtlas = new Texture2D(0, 0);
        fnt.PageOffsets = pageAtlas.PackTextures(texturePages, 0);
        
        //Save atlas as png
        byte[] pngData = pageAtlas.EncodeToPNG();
        string outputPath = path.Substring(0, path.LastIndexOf('.')) + "_dist.png";
        System.IO.File.WriteAllBytes(outputPath, pngData);
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

        //Set correct texture format
        TextureImporter texImp = (TextureImporter)TextureImporter.GetAtPath(outputPath);
        texImp.textureType = TextureImporterType.Advanced;
        texImp.isReadable = true;
        texImp.textureFormat = TextureImporterFormat.Alpha8;
        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

        //Load the saved texture atlas
        fnt.PageAtlas = (Texture2D)AssetDatabase.LoadAssetAtPath(outputPath, typeof(Texture2D));
        */
        //Load kerning info
        XmlNodeList kernings = doc.SelectNodes("/font/kernings/kerning");
        fnt.kernings = new BitmapFont.CharsKerning[kernings.Count];
        index = 0;
        foreach (XmlNode kerning in kernings)
        {
            BitmapFont.CharsKerning krn = new BitmapFont.CharsKerning();
            krn.firstChar = ReadIntAttribute(kerning, "first");
            krn.secondChar = ReadIntAttribute(kerning, "second");
            krn.amount = ReadFloatAttribute(kerning, "amount");
            fnt.kernings[index] = krn;
            index++;
        }

        return fnt;
    }

    static int ReadIntAttribute(XmlNode node, string attribute)
    {
        return int.Parse(node.Attributes[attribute].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
    }
    static float ReadFloatAttribute(XmlNode node, string attribute)
    {
        return float.Parse(node.Attributes[attribute].Value, System.Globalization.NumberFormatInfo.InvariantInfo);
    }
    static Vector2 ReadVector2Attributes(XmlNode node, string attributeX, string attributeY)
    {
        return new Vector2(ReadFloatAttribute(node, attributeX), ReadFloatAttribute(node, attributeY));
    }
    static RectOffset ReadRectOffsetAttribute(XmlNode node, string attribute)
    {
        string[] numbers = node.Attributes[attribute].Value.Split(',');
        return new RectOffset(int.Parse(numbers[0]), int.Parse(numbers[1]), int.Parse(numbers[3]), int.Parse(numbers[2]));
    }
}
