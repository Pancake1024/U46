using UnityEngine;
using System.Collections.Generic;
using SBS.Core;
using SBS.Math;

[AddComponentMenu("SBS/UI/OverlaysLayer")]
public class OverlaysLayer : MonoBehaviour
{
    public class ScissorRectLayer
    {
        public GameObject go = null;
        public Camera camera = null;
        public Rect rect;
        public Vector2 offset = Vector2.zero;
        public Vector2 scale = Vector2.one;
        public int layer;
        public int refCount = 1;
        public List<OverlaysBatch> batches = new List<OverlaysBatch>();
    }

    #region Public members
    public Material baseMaterial;
    public LayerMask overlaysLayerMask;
    #endregion

    #region Protected members
    protected Camera _camera;
    protected int overlaysLayer;
    protected List<OverlaysBatch> batches;
    protected GameObject orthoCamera;
    protected Dictionary<string, ScissorRectLayer> scissorRectLayers = new Dictionary<string, ScissorRectLayer>();
#if UNITY_FLASH
    protected AS3Stack<int> scissorRectMasks = new AS3Stack<int>();
#else
    protected Stack<int> scissorRectMasks = new Stack<int>();
#endif
    protected bool dontDestroyOnLoad = true;
    #endregion

    #region Public properties
    public Camera Camera
    {
        get
        {
            return _camera;
        }
    }

    public Camera LayerCamera
    {
        get
        {
            return orthoCamera.GetComponent<Camera>();
        }
    }
    #endregion

    #region Public methods
    public void Initialize(bool _dontDestroyOnLoad)
    {
        dontDestroyOnLoad = _dontDestroyOnLoad;

        _camera = this.GetComponent<Camera>();
        if (null == _camera)
            Asserts.Assert(false, "OverlaysLayer behavior must be attached to a camera");

        Asserts.Assert(Mathf.ClosestPowerOfTwo(overlaysLayerMask) == overlaysLayerMask, "Overlays layer mask must be just one layer");

        _camera.cullingMask &= ~overlaysLayerMask;

        overlaysLayer = -1;
        int mask = overlaysLayerMask;
        while (mask > 0)
        {
            ++overlaysLayer;
            mask >>= 1;
        }
		
		for (int freeLayer = 31; freeLayer > 16; --freeLayer)
			scissorRectMasks.Push(freeLayer);
		
        batches = new List<OverlaysBatch>();

        orthoCamera = GameObject.Find("__OverlaysLayer");
        if (null == orthoCamera)
        {
            orthoCamera = new GameObject("__OverlaysLayer");
            Camera ortho = orthoCamera.AddComponent<Camera>();
            orthoCamera.transform.parent = gameObject.transform;
            orthoCamera.transform.localPosition = Vector3.zero;
            orthoCamera.transform.localRotation = Quaternion.identity;

            ortho.clearFlags = CameraClearFlags.Nothing;
            ortho.depth = _camera.depth + 0.1f;
            ortho.orthographic = true;
            ortho.orthographicSize = 0.5f;
            ortho.nearClipPlane = 0.0f;
            ortho.farClipPlane = 1.0f;
            ortho.cullingMask = overlaysLayerMask;

            if (dontDestroyOnLoad)
                Object.DontDestroyOnLoad(orthoCamera);
        }
        else
        {
            orthoCamera.transform.parent = gameObject.transform;
            orthoCamera.transform.localPosition = Vector3.zero;
            orthoCamera.transform.localRotation = Quaternion.identity;

			int iChilds = 0;

#if UNITY_4_3
            iChilds = orthoCamera.transform.childCount;
#else
			iChilds =orthoCamera.transform.GetChildCount();
#endif
			for (int i = iChilds- 1; i >= 0; --i)
            {
                OverlaysBatch batch = orthoCamera.transform.GetChild(i).GetComponent<OverlaysBatch>();
                batch.Layer = this;
                batches.Add(batch);
            }
        }
        /*
        GameObject[] scissorRects = GameObject.FindGameObjectsWithTag("ScissorRect");
        foreach (GameObject go in scissorRects)
        {
            if (go.name.StartsWith("__SR_"))
            {
                ScissorRectLayer srLayer = new ScissorRectLayer();
                srLayer.go = go;
                srLayer.camera = go.camera;
                srLayer.rect = srLayer.camera.pixelRect;
                srLayer.refCount = 1;//ToDo

                srLayer.camera.transform.parent = gameObject.transform;
                srLayer.camera.transform.localPosition = Vector3.zero;
                srLayer.camera.transform.localRotation = Quaternion.identity;

                string name = go.name.Substring(5);
                scissorRectLayers.Add(name, srLayer);
            }
        }*/
    }

    public OverlaysBatch AddBatch(int capacity, Texture image, SBSMatrix4x4 transform)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = orthoCamera.transform;

        float z;/*
        if (batches.Count > 0)
            z = batches[batches.Count - 1].transform.localPosition.z + 0.01f;
        else*/
            z = 0.0f;

        obj.transform.localPosition = new Vector3(0.0f, 0.0f, z);
        obj.transform.localRotation = Quaternion.identity;
        obj.layer = overlaysLayer;

        OverlaysBatch batch = obj.AddComponent<OverlaysBatch>();
        batch.Initialize(this, capacity, baseMaterial, image, transform, 3200 - batches.Count * 100, dontDestroyOnLoad);
        batches.Add(batch);

        return batch;
    }

    public OverlaysBatch InsertBatch(int capacity, Texture image, SBSMatrix4x4 transform, int index)
    {
        index = Mathf.Clamp(index, 0, batches.Count);
        GameObject obj = new GameObject();
        obj.transform.parent = orthoCamera.transform;

        float z;/*
        if (index > 0 && batches.Count > 0)
            z = batches[index - 1].transform.localPosition.z + 0.01f;
        else*/
            z = 0.0f;

        obj.transform.localPosition = new Vector3(0.0f, 0.0f, z);
        obj.transform.localRotation = Quaternion.identity;
        obj.layer = overlaysLayer;

        OverlaysBatch batch = obj.AddComponent<OverlaysBatch>();
        int baseDepth = 0;
        if (index > 0)
            baseDepth = (batches[index - 1].BaseDepth + batches[index].BaseDepth) / 2;
        else
            baseDepth = batches[0].BaseDepth - 100;
        batch.Initialize(this, capacity, baseMaterial, image, transform, baseDepth, dontDestroyOnLoad);
        batches.Insert(index, batch);
        /*
        for (int i = index + 1; i < batches.Count; ++i)
        {
            z =  batches[i - 1].transform.localPosition.z + 0.01f;
            batches[i].transform.localPosition = new Vector3(0.0f, 0.0f, z);
        }
        */
        return batch;
    }

    public void RemoveBatch(OverlaysBatch batch)
    {
        batch.Destroy();

        batches.Remove(batch);
    }

    public OverlaysBatch AddScissorRectRef(OverlaysBatch source, string name, Rect scissorRect)
    {
        ScissorRectLayer srLayer = null;
        Camera ortho = null;
        if (!scissorRectLayers.TryGetValue(name, out srLayer))
        {
            //Debug.Log("created " + name);
            srLayer = new ScissorRectLayer();
            srLayer.go = new GameObject("__SR_" + name);
            ortho = srLayer.camera = srLayer.go.AddComponent<Camera>();
            srLayer.go.transform.parent = gameObject.transform;
            srLayer.go.transform.localPosition = Vector3.zero;
            srLayer.go.transform.localRotation = Quaternion.identity;
			
			Asserts.Assert(scissorRectMasks.Count > 0);
            srLayer.layer = scissorRectMasks.Pop();

#if UNITY_FLASH
            ortho.clearFlags = CameraClearFlags.Nothing;
#else
            ortho.clearFlags = CameraClearFlags.Depth;// Nothing;
#endif
            ortho.depth = orthoCamera.GetComponent<Camera>().depth + (32 - srLayer.layer) * 0.1f;
            ortho.orthographic = true;
            ortho.orthographicSize = 0.5f;
            ortho.nearClipPlane = 0.0f;
            ortho.farClipPlane = 1.0f;

            ortho.cullingMask = (1 << srLayer.layer);

            scissorRectLayers.Add(name, srLayer);

            //Object.DontDestroyOnLoad(srLayer.go);
        }
        else
        {
            srLayer.refCount++;

            ortho = srLayer.camera;
        }

        srLayer.rect = scissorRect;

        Rect pixelRect = scissorRect;
        pixelRect.yMin = Screen.height - scissorRect.yMax;
        pixelRect.yMax = Screen.height - scissorRect.yMin;

        float ratio = 1.0f;
        if (pixelRect.yMin < 0.0f)
            ratio = pixelRect.yMax / pixelRect.height;
        else if (pixelRect.yMax > Screen.height)
            ratio = (Screen.height - pixelRect.yMin) / pixelRect.height;

        ortho.pixelRect = pixelRect;
        ortho.orthographicSize = 0.5f * ratio;

        foreach (OverlaysBatch batch in srLayer.batches)
        {
            if (batch.Image == source.Image)
                return batch;
        }

        OverlaysBatch newBatch = this.AddBatch(source.Capacity, source.Image, source.Transform);
        newBatch.gameObject.layer = srLayer.layer;

        batches.Remove(newBatch);
        srLayer.batches.Add(newBatch);

        return newBatch;
    }

    public OverlaysBatch ReleaseScissorRectRef(OverlaysBatch source, string name)
    {
        ScissorRectLayer srLayer = null;
        OverlaysBatch oldBatch = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
        {
            foreach (OverlaysBatch batch in batches)
            {
                if (batch.Image == source.Image)
                {
                    oldBatch = batch;
                    break;
                }
            }

            Asserts.Assert(srLayer.refCount > 0);
            --srLayer.refCount;
/*
            if (0 == --srLayer.refCount)
            {
                Debug.Log("destroyed " + name);
                foreach (OverlaysBatch batch in srLayer.batches)
                    batch.Destroy();
				
				int cullingMask = srLayer.camera.cullingMask,
				    freeLayer = 31;
				while (0 == ((1 << freeLayer) & cullingMask))
					--freeLayer;
				scissorRectMasks.Push(freeLayer);

				Object.DestroyImmediate(srLayer.go);

                srLayer.batches.Clear();
                srLayer.batches = null;

                srLayer.go = null;
                srLayer.camera = null;
                srLayer = null;

                scissorRectLayers.Remove(name);
            }*/
        }
        return oldBatch;
    }

    protected void ClearScissorRects()
    {
        List<string> srLayerNames = new List<string>();

        foreach (string srName in scissorRectLayers.Keys)
        {
            ScissorRectLayer srLayer = scissorRectLayers[srName];

            if (0 == srLayer.refCount)
            {
                //Debug.Log("destroyed " + srName);
                foreach (OverlaysBatch batch in srLayer.batches)
                {
                    batch.Destroy();
                    Object.DestroyImmediate(batch.gameObject);
                }

                int cullingMask = srLayer.camera.cullingMask,
                    freeLayer = 31;
                while (0 == ((1 << freeLayer) & cullingMask))
                    --freeLayer;
                scissorRectMasks.Push(freeLayer);

                Object.DestroyImmediate(srLayer.go);

                srLayer.batches.Clear();
                srLayer.batches = null;

                srLayer.go = null;
                srLayer.camera = null;

                srLayerNames.Add(srName);
            }
        }

        foreach (string name in srLayerNames)
            scissorRectLayers.Remove(name);
    }

    public bool GetScissorRect(string name, ref Rect rect)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
        {
            rect = srLayer.rect;
            return true;
        }
        else
            return false;
    }

    public int GetScissorRectFlag(string name)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
            return srLayer.camera.cullingMask;
        else
            return -1;
    }

    public float GetScissorRectDepthOffset(string name)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
            return srLayer.camera.depth - orthoCamera.GetComponent<Camera>().depth;
        else
            return 0.0f;
    }

    public void SetScissorRectDepthOffset(string name, float offset)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
            srLayer.camera.depth = orthoCamera.GetComponent<Camera>().depth + offset;
    }

    public void SetScissorRectOffset(string name, Vector2 offset)
    {
        Vector3 uiOffset = new Vector3(offset.x, -offset.y, 0.0f);

        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
        {
            srLayer.offset = offset;

            Vector3 sOffset = new Vector3(0.0f, 0.0f, 0.0f);

            if (srLayer.camera.pixelWidth > SBSMath.Epsilon)
                uiOffset.x /= srLayer.camera.pixelWidth;
            else
                uiOffset.x = 0.0f;
            if (srLayer.camera.pixelHeight > SBSMath.Epsilon)
            {
                uiOffset.y /= srLayer.camera.pixelHeight;

                float aspect = srLayer.camera.pixelWidth / srLayer.camera.pixelHeight;
                uiOffset.x *= aspect;

                sOffset.x -= 0.5f * aspect * (1.0f - srLayer.scale.x);
                sOffset.y += 0.5f * (1.0f - srLayer.scale.y);
            }
            else
                uiOffset.y = 0.0f;

            foreach (OverlaysBatch batch in srLayer.batches)
            {
                uiOffset.z = batch.transform.localPosition.z;
                batch.transform.localPosition = uiOffset + sOffset;
            }
        }
    }

    public void SetScissorRectScale(string name, Vector2 scale)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
        {
            srLayer.scale = scale;

            Vector3 uiOffset = new Vector3(srLayer.offset.x, -srLayer.offset.y, 0.0f);

            if (srLayer.camera.pixelWidth > SBSMath.Epsilon)
                uiOffset.x /= srLayer.camera.pixelWidth;
            else
                uiOffset.x = 0.0f;

            if (srLayer.camera.pixelHeight > SBSMath.Epsilon)
            {
                uiOffset.y /= srLayer.camera.pixelHeight;

                float aspect = srLayer.camera.pixelWidth / srLayer.camera.pixelHeight;
                uiOffset.x *= aspect;

                uiOffset.x -= 0.5f * aspect * (1.0f - srLayer.scale.x);
                uiOffset.y += 0.5f * (1.0f - srLayer.scale.y);
            }
            else
                uiOffset.y = 0.0f;

            foreach (OverlaysBatch batch in srLayer.batches)
            {
                uiOffset.z = batch.transform.localPosition.z;
                batch.transform.localPosition = uiOffset;
                batch.transform.localScale = new Vector3(scale.x, scale.y, 1.0f);
            }
        }
    }

    public bool GetScissorRectOffset(string name, ref Vector2 offset)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
        {
            offset = srLayer.offset;
            return true;
        }
        else
            return false;
    }

    public void SetScissorRectActive(string name, bool active)
    {
        ScissorRectLayer srLayer = null;
        if (scissorRectLayers.TryGetValue(name, out srLayer))
            srLayer.camera.enabled = active;
    }

    public void OnLoadNewLevel()
    {
        orthoCamera.transform.parent = null;
/*
#if UNITY_FLASH
        foreach (KeyValuePair<string, ScissorRectLayer> srLayerItem in scissorRectLayers)
            srLayerItem.Value.go.transform.parent = null;
#else
        foreach (ScissorRectLayer srLayer in scissorRectLayers.Values)
            srLayer.go.transform.parent = null;
#endif*/

        foreach (KeyValuePair<string, ScissorRectLayer> item in scissorRectLayers)
            item.Value.refCount = 0;

        this.ClearScissorRects();
    }
    #endregion

    #region Unity callbacks
    void Update()
    {
        foreach (OverlaysBatch batch in batches)
        {
            if (batch.IsDirty)
                batch.Flush();
        }

#if UNITY_FLASH
        foreach (KeyValuePair<string, ScissorRectLayer> srLayerItem in scissorRectLayers)
        {
            foreach (OverlaysBatch batch in srLayerItem.Value.batches)
#else
        foreach (ScissorRectLayer srLayer in scissorRectLayers.Values)
        {
            foreach (OverlaysBatch batch in srLayer.batches)
#endif
            {
                if (batch.IsDirty)
                    batch.Flush();
            }
        }
    }

    void LateUpdate()
    {
        this.ClearScissorRects();
    }
    #endregion
};
