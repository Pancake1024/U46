using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;
using System.Collections.Generic;

class OnTheRunPackerPolicy : IPackerPolicy
{
    protected class Entry
    {
        public Sprite sprite;
        public AtlasSettings settings;
        public string atlasName;
        public SpritePackingMode packingMode;
    }

    protected enum Category
    {
        StartPage = 0,
		Splash,
        Web,
		Opaque,
        Other
    }

    public virtual int GetVersion()
    {
        return 1;
    }

    private Category GetCategoryFromTag(string tag)
    {
        if (tag.Equals("ontherun_main"))
			return Category.StartPage;
		else if (tag.Equals("splash"))
            return Category.Splash;
		else if (tag.Equals("ontherun_small"))
		     return Category.Opaque;
        else if (tag.IndexOf("_web") >= 0)
            return Category.Web;
        return Category.Other;
    }

#if UNITY_4_5 || UNITY_4_6
	public void OnGroupAtlases(BuildTarget target, PackerJob job, int[] textureImportersIDs)
#else
    public virtual void OnGroupAtlases(BuildTarget target, PackerJob job, int[] textureImporters)
#endif
    {
//        List<Entry> entries = new List<Entry>();

//        TextureFormat //format32 = TextureFormat.RGB24,
//                      //format16 = TextureFormat.RGB565,
//                      formatCompressed = TextureFormat.DXT1;

//        TextureFormat //formatAlpha32 = TextureFormat.RGBA32,
//                      //formatAlpha16 = TextureFormat.RGBA4444,
//                      formatCompressedAlpha = TextureFormat.DXT5;

//        switch (target)
//        {
//            case BuildTarget.WP8Player:
//                formatCompressed = TextureFormat.DXT1;
//                formatCompressedAlpha = TextureFormat.DXT5;
//                break;

//            case BuildTarget.iOS:
//                formatCompressed = TextureFormat.PVRTC_RGB4;
//                formatCompressedAlpha = TextureFormat.PVRTC_RGBA4;
//                break;

//            default:
//                break;
//        }

//#if UNITY_4_5 || UNITY_4_6
//        foreach (int tiID in textureImportersIDs)
//#else
//        foreach (TextureImporter ti in textureImporters)
//#endif
//        {
//#if UNITY_4_5 || UNITY_4_6
//            TextureImporter ti = EditorUtility.InstanceIDToObject(tiID) as TextureImporter;
//#endif
//            TextureImportInstructions ins = new TextureImportInstructions();
//            ti.ReadTextureImportInstructions(ins, target);

//            TextureImporterSettings tis = new TextureImporterSettings();
//            ti.ReadTextureSettings(tis);

//            Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(ti.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();
//            foreach (Sprite sprite in sprites)
//            {
//                Entry entry = new Entry();

//                entry.sprite = sprite;
//                entry.atlasName = ti.spritePackingTag;

//                Category cat = this.GetCategoryFromTag(ti.spritePackingTag);

//                switch (target)
//                {
//                    case BuildTarget.WP8Player:
//                        if (cat == Category.Web)
//                            continue;

//                        entry.packingMode = SpritePackingMode.Rectangle;
//                        entry.settings.format = ti.alphaIsTransparency ? formatCompressedAlpha : formatCompressed;
//                        entry.settings.usageMode = TextureUsageMode.Default;
//                        entry.settings.colorSpace = ColorSpace.Gamma;
//                        entry.settings.compressionQuality = (int)TextureCompressionQuality.Best;
//                        entry.settings.filterMode = FilterMode.Bilinear;
//                        entry.settings.maxWidth = 1024;
//                        entry.settings.maxHeight = 1024;
//                        break;

//                    case BuildTarget.WebPlayer:
//                    case BuildTarget.WebPlayerStreamed:
//                        entry.packingMode = SpriteMeshType.Tight == tis.spriteMeshType ? SpritePackingMode.Tight : SpritePackingMode.Rectangle;
//                        if (cat == Category.StartPage || cat == Category.Splash || cat == Category.Opaque)
//                            entry.settings.format = ins.desiredFormat;
//                        else
//                            entry.settings.format = ti.DoesSourceTextureHaveAlpha() ? ins.desiredFormat/*formatCompressedAlpha*/ : formatCompressed;	//ti.alphaIsTransparency ? ins.desiredFormat : formatCompressed;
//                        entry.settings.usageMode = ins.usageMode;
//                        entry.settings.colorSpace = ins.colorSpace;
//                        entry.settings.compressionQuality = ins.compressionQuality;
//                        entry.settings.filterMode = Enum.IsDefined(typeof(FilterMode), ti.filterMode) ? ti.filterMode : FilterMode.Bilinear;
//                        entry.settings.maxWidth = 2048;
//                        entry.settings.maxHeight = 2048;
//                        break;

//                    default:
//                        if (cat == Category.Web)
//                            continue;

//                        entry.packingMode = SpriteMeshType.Tight == tis.spriteMeshType ? SpritePackingMode.Tight : SpritePackingMode.Rectangle;
//                        if (cat == Category.StartPage || cat == Category.Splash || cat == Category.Opaque)
//                            entry.settings.format = ins.desiredFormat;
//                        else
//                            entry.settings.format = ti.DoesSourceTextureHaveAlpha() ? ins.desiredFormat : formatCompressed;	//ti.alphaIsTransparency ? ins.desiredFormat : formatCompressed;
//                        entry.settings.usageMode = ins.usageMode;
//                        entry.settings.colorSpace = ins.colorSpace;
//                        entry.settings.compressionQuality = ins.compressionQuality;
//                        entry.settings.filterMode = Enum.IsDefined(typeof(FilterMode), ti.filterMode) ? ti.filterMode : FilterMode.Bilinear;
//                        entry.settings.maxWidth = 2048;
//                        entry.settings.maxHeight = 2048;
//                        break;
//                }

//                entries.Add(entry);
//            }

//            Resources.UnloadAsset(ti);
//        }

//        // First split sprites into groups based on atlas name
//        var atlasGroups =
//            from e in entries
//            group e by e.atlasName;
//        foreach (var atlasGroup in atlasGroups)
//        {
//            int page = 0;
//            // Then split those groups into smaller groups based on texture settings
//            var settingsGroups =
//                from t in atlasGroup
//                group t by t.settings;
//            foreach (var settingsGroup in settingsGroups)
//            {
//                string atlasName = atlasGroup.Key;
//                if (settingsGroups.Count() > 1)
//                    atlasName += string.Format(" (Group {0})", page);

//                job.AddAtlas(atlasName, settingsGroup.Key);
//                foreach (Entry entry in settingsGroup)
//                {
//                    job.AssignToAtlas(atlasName, entry.sprite, entry.packingMode, SpritePackingRotation.None);
//                }

//                ++page;
//            }
//        }
	}
}
