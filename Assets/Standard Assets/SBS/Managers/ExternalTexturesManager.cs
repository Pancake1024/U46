using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBS.Core;

[AddComponentMenu("SBS/Managers/ExternalTexturesManager")]
public class ExternalTexturesManager : MonoBehaviour
{
    #region Internal Data Structure
    [System.Serializable]
    public class ExternalTextureItem
    {
        public string url;
        public Texture2D texture;
        public Material material;
    }

    [System.Serializable]
    public class ReloadedImageDefinition
    {
        public string description;
        public string baseURL;
        public int zoneId;
        public string location;
        public int billboardMaxNum;
        public Material sourceMaterial;
    }

    public enum DebugOptions
    {
        NO_ADS = 0,
        MINICLIP_ADS,
        RELOADED_ADS
    }

    public enum SponsorTags
    {
        Sponsor = 0,
        Sponsor2,
        Sponsor3
    }
    #endregion

    #region Public Members
    public bool enableReloadedSDK = false;
    public bool preloadReloadedADV = false;
    public SponsorTags[] tagsEnable;
    public DebugOptions debugOption;
    public bool isReady = false;
    public Texture2D[] defaultImages;
    public List<ExternalTextureItem> texturesList;
    public List<ReloadedImageDefinition> reloadedImages;
    
    public void LoadTextures()
    {
        if (enableReloadedSDK)
            InitializeReloadedSDK();
        else
            StartCoroutine(LoadTexturesCoroutine());
    }
    #endregion

    #region Protected methods
    protected void LoadDefaultTextures()
    {
        for (int i = 0; i < texturesList.Count; i++)
        {
            texturesList[i].material.mainTexture = defaultImages[i % defaultImages.Length];
        }
    }
    #endregion

    #region Couroutines
    IEnumerator LoadTexturesCoroutine()
    {
        foreach (ExternalTextureItem item in texturesList)
        {
            WWW www = new WWW(CoreUtils.GetFullPath(item.url, true));
            yield return www;
            if (www.error == null)
            {
                item.material.mainTexture = www.texture;
            }
            
        }
        isReady = true;
    }
    #endregion

    #region Messages
    void OnLevelStarted()
    {
        if (enableReloadedSDK)
        {
            if (preloadReloadedADV)
                AssignMaterials();
            else
                InitializeReloadedSDK();
        }
    }
    #endregion

    #region Unity Callbacks
    void Start ()
    {
        isReady = false;
        LoadTextures();
	}
    #endregion

    #region Reloaded SDK
    protected List<Material> materialList;
    protected Dictionary<string, ReloadedImageDefinition> imgDefinitionCache;
    protected GameObject parentObj;

    protected void InitializeReloadedSDK()
    {

        materialList = new List<Material>();
        imgDefinitionCache = new Dictionary<string, ReloadedImageDefinition>();
        LoadDefaultTextures();

#if UNITY_EDITOR
        switch (debugOption)
        {
            case (DebugOptions.MINICLIP_ADS):
                StartCoroutine(LoadTexturesCoroutine());
                break;
            case (DebugOptions.RELOADED_ADS):
                if (preloadReloadedADV)
                    PreloadReloadedTextures();
                else
                    FillMaterialList();
                break;
        }
#else
        string[] parameters = Application.srcValue.Split('?');
        if (parameters.Length >= 2)
        {
            if (parameters[1].StartsWith("RELOADED_ADS"))
            {
                var attr = parameters[1].Split('&');
                if (attr.Length >= 2)
                {
                    switch (attr[1])
                    {
                        case ("SPONSOR1"):
                            tagsEnable = new SponsorTags[] { SponsorTags.Sponsor };
                            break;
                        case ("SPONSOR2"):
                            tagsEnable = new SponsorTags[] { SponsorTags.Sponsor, SponsorTags.Sponsor2 };
                            break;
                        case ("SPONSOR3"):
                            tagsEnable = new SponsorTags[] { SponsorTags.Sponsor, SponsorTags.Sponsor2, SponsorTags.Sponsor3 };
                            break;
                    }
                }
                FillMaterialList();
            }
            else if (parameters[1].StartsWith("MINICLIP_ADS"))
            {
                StartCoroutine(LoadTexturesCoroutine());
            }
        }
#endif
    }

    #region Preload Textures
    protected List<Material> preloadedMaterial;
    protected bool preloadCoroutineStarted = false;
    protected void PreloadReloadedTextures()
    {
        preloadedMaterial = new List<Material>();
        StartCoroutine(PreloadCoroutine());
        preloadCoroutineStarted = true;
    }

    IEnumerator PreloadCoroutine()
    {
        for (int k = 0; k < reloadedImages.Count; k++)
        {
            int matNum = reloadedImages[k].billboardMaxNum;
            for (int i = 0; i < matNum; i++)
            {
                Material newMat = new Material(reloadedImages[k].sourceMaterial);
                newMat.name = "sponsor" + i;
                preloadedMaterial.Add(newMat);
            }
        }

        CreatePREReloadedGO();
        yield return new WaitForEndOfFrame();
    }

    protected void CreatePREReloadedGO()
    {
        if (null != parentObj)
        {
            Destroy(parentObj);
            parentObj = null;
        }

        parentObj = new GameObject("ReloadedObjects");
        if (preloadReloadedADV)
            parentObj.AddComponent<AdvRootBehaviour>();

        for (int i = 0; i < preloadedMaterial.Count; i++)
        {
            GameObject advObj = new GameObject("Adv" + (i + 1));
            advObj.transform.parent = parentObj.transform;

            string key = preloadedMaterial[i].mainTexture.width.ToString() + "x" + preloadedMaterial[i].mainTexture.height.ToString();
            ReloadedImageDefinition imgDef = GetReloadedImageDefinition(key);

            if (null != imgDef)
            {
                RIGA.AdTagManager advComp = advObj.AddComponent<RIGA.AdTagManager>();
                advComp.BaseUrl = imgDef.baseURL;
                advComp.ZoneId = imgDef.zoneId;
                advComp.Location = imgDef.location;
                advComp.AdMaterial = preloadedMaterial[i];
            }
        }
    }

    protected void AssignMaterials()
    {
        int materialCounter = 0;

        for (int k = 0; k < tagsEnable.Length; k++)
        {
            GameObject[] goList = GameObject.FindGameObjectsWithTag(tagsEnable[k].ToString());
            for (int i = 0; i < goList.Length; i++)
            {
                goList[i].GetComponent<MeshRenderer>().materials = new Material[] { preloadedMaterial[materialCounter] };
                materialCounter = (materialCounter + 1) % preloadedMaterial.Count;
            }
        }
    }
    #endregion

    protected bool CheckSponsorMaterial(string name)
    {
        for (int i = 0; i < texturesList.Count; i++)
            if (name.StartsWith(texturesList[i].material.name))
                return true;
        
        return false;
    }

    protected void FillMaterialList()
    {
        for (int k = 0; k < tagsEnable.Length; k++)
        {
            GameObject[] goList = GameObject.FindGameObjectsWithTag(tagsEnable[k].ToString());
            for (int i = 0; i < goList.Length; i++)
            {
                Material[] materials = goList[i].GetComponent<MeshRenderer>().materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    if (CheckSponsorMaterial(materials[j].name))
                        materialList.Add(materials[j]);
                }
            }
        }
        StartCoroutine(CreateReloadedGO());
    }

    protected ReloadedImageDefinition GetReloadedImageDefinition(string key)
    {
        if (imgDefinitionCache.ContainsKey(key))
            return imgDefinitionCache[key];

        for (int i = 0; i < reloadedImages.Count; i++)
        {
            if (reloadedImages[i].description.Equals(key))
            {
                imgDefinitionCache.Add(key, reloadedImages[i]);
                return reloadedImages[i];
            }
        }

        Debug.LogWarning("*** NO RELOADED IMAGE DEFINED FOR " + key);
        return null;
    }

    IEnumerator CreateReloadedGO()
    {
        if (null != parentObj)
        {
            Destroy(parentObj);
            parentObj = null;
        }

        parentObj = new GameObject("ReloadedObjects");
        for (int i = 0; i < materialList.Count; i++)
        {
            GameObject advObj = new GameObject("Adv" + (i + 1));
            advObj.transform.parent = parentObj.transform;

            string key = materialList[i].mainTexture.width.ToString() + "x" + materialList[i].mainTexture.height.ToString();
            ReloadedImageDefinition imgDef = GetReloadedImageDefinition(key);

            if (null != imgDef)
            {
                RIGA.AdTagManager advComp = advObj.AddComponent<RIGA.AdTagManager>();
                advComp.BaseUrl = imgDef.baseURL;
                advComp.ZoneId = imgDef.zoneId;
                advComp.Location = imgDef.location;
                advComp.AdMaterial = materialList[i];
            }
        }
        yield return new WaitForEndOfFrame();
    }
    #endregion
}
