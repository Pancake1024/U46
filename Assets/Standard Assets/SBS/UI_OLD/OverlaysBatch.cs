using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using SBS.UI;

[AddComponentMenu("SBS/UI/OverlaysBatch")]
public class OverlaysBatch : MonoBehaviour
{
    #region Protected enums
    [Flags]
    protected enum DirtyFlags
    {
        None = 0,
        Indices = 1 << 0,
        Vertices = 1 << 1,
        UVs = 1 << 2,
        Colors = 1 << 3
    }
    #endregion

    #region Protected members
    protected OverlaysLayer layer;
    protected int capacity;
    protected Material baseMaterial;
    protected Texture image;
    protected SBSMatrix4x4 baseTransform;
    protected Dictionary<int, Material> materials;
    protected int baseDepth;
    protected Mesh mesh;

    protected Mesh mesh2;
    protected bool meshSwap;
    protected MeshFilter meshFilter;

    protected Vector3[] vertexBuffer;
    protected Vector2[] uvBuffer;
    protected Color[] colorBuffer;
    protected bool[] visibility;
    //protected bool[] clipVisibility;
    protected int[] subMeshIndices;
    protected int[] subMeshQuadsCount;
#if UNITY_FLASH
    protected AS3Stack<int> freeIndices;
#else
    protected Stack<int> freeIndices;
#endif
    protected List<int> usedIndices;
    protected DirtyFlags dirtyFlags;
    protected DirtyFlags dirtyFlags2;
    protected bool isOpenGL;/*
    protected List<Rect> scissorRects;
    protected List<string> scissorRectNames;*/
    protected MeshRenderer meshRenderer;
    #endregion

    #region Public properties
    public OverlaysLayer Layer
    {
        get
        {
            return layer;
        }
        set
        {
            layer = value;
        }
    }

    public int BaseDepth
    {
        get
        {
            return baseDepth;
        }
    }
    /*
    public Material Material
    {
        get
        {
            return material;
        }
    }

    public Mesh Mesh
    {
        get
        {
            return mesh;
        }
    }
    */
    public bool IsDirty
    {
        get
        {
            return meshSwap ? (dirtyFlags != 0) : (dirtyFlags2 != 0);//!(DirtyFlags.None == dirtyFlags);
        }
    }

    public Texture Image
    {
        get
        {
            return image;
        }
    }

    public SBSMatrix4x4 Transform
    {
        get
        {
            return baseTransform;
        }
    }

    public int Capacity
    {
        get
        {
            return capacity;
        }
    }
    #endregion

    #region Protected Methods
    protected void Reserve(int newCapacity)
    {
        if (newCapacity < capacity)
            return;

        Vector3[] newVertexBuffer = new Vector3[newCapacity << 2];
        Vector2[] newUvBuffer = new Vector2[newCapacity << 2];
        Color[] newColorBuffer = new Color[newCapacity << 2];
        bool[] newVisibility = new bool[newCapacity];
        //bool[] newClipVisibility = new bool[newCapacity];
        int[] newSubMeshIndices = new int[newCapacity];

        if (capacity > 0)
        {
            vertexBuffer.CopyTo(newVertexBuffer, 0);
            uvBuffer.CopyTo(newUvBuffer, 0);
            colorBuffer.CopyTo(newColorBuffer, 0);
            visibility.CopyTo(newVisibility, 0);
            //clipVisibility.CopyTo(newClipVisibility, 0);
            subMeshIndices.CopyTo(newSubMeshIndices, 0);
        }

        for (int i = capacity; i < newCapacity; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                int k = (i << 2) + j;

                newVertexBuffer[k] = Vector3.zero;
                newUvBuffer[k] = Vector2.zero;
                newColorBuffer[k] = Color.white;
            }

            newVisibility[i] = true;
            //newClipVisibility[i] = true;
            newSubMeshIndices[i] = 0;

            freeIndices.Push(i);
        }

        vertexBuffer = newVertexBuffer;
        uvBuffer = newUvBuffer;
        colorBuffer = newColorBuffer;
        visibility = newVisibility;
        //clipVisibility = newClipVisibility;
        subMeshIndices = newSubMeshIndices;

        capacity = newCapacity;
    }
    /*
    protected int ReserveExtraQuad(int subMeshIndex)
    {
        if (0 == freeIndices.Count)
        {
            this.Reserve(capacity << 1);
            Asserts.Assert(freeIndices.Count > 0);
        }

        int freeIndex = freeIndices.Pop();
        usedIndices.Add(freeIndex);

        visibility[freeIndex] = true;
        //clipVisibility[freeIndex] = true;
        subMeshIndices[freeIndex] = subMeshIndex;
        subMeshQuadsCount[subMeshIndex] += 1;

        dirtyFlags |= DirtyFlags.Indices;

        return freeIndex;
    }

    protected void FreeExtraQuad(int index1)
    {
        freeIndices.Push(index1);
        usedIndices.Remove(index1);//ToDo: optimize

        dirtyFlags |= DirtyFlags.Indices;
    }
    */
    protected int GetSubMeshIndex(int depth)
    {
        Material mat = null;
        if (materials.TryGetValue(depth, out mat))
        {
            int subMeshIndex = 0;

            Material[] rndMats = GetComponent<Renderer>().sharedMaterials;
            foreach (Material meshMat in rndMats)
            {
                if (meshMat == mat)
                    break;
                ++subMeshIndex;
            }

            Asserts.Assert(subMeshIndex < rndMats.Length);
            return subMeshIndex;
        }
        else
        {
            mat = new Material(baseMaterial);
            mat.SetTexture("_Image", image);
            mat.SetMatrix("_Transform", baseTransform);
            mat.renderQueue += depth;

            materials.Add(depth, mat);

            Material[] newSharedMaterials = new Material[GetComponent<Renderer>().sharedMaterials.Length + 1];
            GetComponent<Renderer>().sharedMaterials.CopyTo(newSharedMaterials, 0);
            newSharedMaterials[newSharedMaterials.Length - 1] = mat;
            GetComponent<Renderer>().sharedMaterials = newSharedMaterials;

            int[] newQuadsCount = new int[subMeshQuadsCount.Length + 1];
            subMeshQuadsCount.CopyTo(newQuadsCount, 0);
            //newQuadsCount[subMeshQuadsCount.Length] = 0;
            subMeshQuadsCount = newQuadsCount;

            return GetComponent<Renderer>().sharedMaterials.Length - 1;
        }
    }
    #endregion

    #region Public methods
    public void Initialize(OverlaysLayer _layer, int _capacity, Material baseMat, Texture _image, SBSMatrix4x4 _transform, int _baseDepth, bool dontDestroyOnLoad)
    {
        layer = _layer;
        capacity = 0;
        baseMaterial = baseMat;
        image = _image;
        baseDepth = _baseDepth;
        baseTransform = _transform;

        Material material = new Material(baseMat);
        material.SetTexture("_Image", image);
        material.SetMatrix("_Transform", baseTransform);
        material.renderQueue += baseDepth;

        materials = new Dictionary<int, Material>();
        materials.Add(baseDepth, material);

        mesh = new Mesh();
        mesh2 = new Mesh();

#if UNITY_FLASH
        freeIndices = new AS3Stack<int>();
#else
        freeIndices = new Stack<int>();
#endif
        usedIndices = new List<int>();

        isOpenGL = SystemInfo.graphicsDeviceVersion.Contains("OpenGL");
        /*
        scissorRects = new List<Rect>();
        scissorRectNames = new List<string>();
        */

        this.Reserve(_capacity);

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.enabled = false;

        subMeshQuadsCount = new int[1] { 0 };

        if (dontDestroyOnLoad)
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    public int AddOverlay(SBSMatrix4x4 transform, Rect scrRect, Rect imgRect, Color color)
    {
        if (0 == freeIndices.Count)
        {
        	Debug.Log("capacity <<= 1 for batch " + image.name);
            this.Reserve(capacity << 1);
            Asserts.Assert(freeIndices.Count > 0);
        }

        int freeIndex = freeIndices.Pop();
        usedIndices.Add(freeIndex);

        visibility[freeIndex] = true;
        //clipVisibility[freeIndex] = true;
        subMeshIndices[freeIndex] = 0;

        subMeshQuadsCount[0] += 1;

        this.FillVertices(freeIndex, transform, scrRect, layer.Camera.pixelRect);
        this.FillUVs(freeIndex, imgRect);
        this.FillColors(freeIndex, color);

        dirtyFlags |= DirtyFlags.Indices;
        dirtyFlags2 |= DirtyFlags.Indices;

        return freeIndex;
    }

    public void FillVertices(int index, SBSMatrix4x4 transform, Rect scrRect, Rect pixelRect)
    {
        int baseIndex = index << 2;

        SBSVector3[] v = {
            transform.MultiplyPoint3x4(scrRect.xMin, scrRect.yMin, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMax, scrRect.yMin, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMax, scrRect.yMax, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMin, scrRect.yMax, 0.0f)
        };

        //Rect pixelRect = layer.Camera.pixelRect;
        float aspect = pixelRect.width / pixelRect.height;
        SBSVector3 pixelSize = new SBSVector3(aspect / pixelRect.width, -1.0f / pixelRect.height),
                   halfPixelSize = isOpenGL ? SBSVector3.zero : pixelSize * 0.5f,
                   screenCenter = new SBSVector3(aspect * 0.5f, -0.5f);

        for (int i = 0; i < 4; ++i)
            vertexBuffer[baseIndex + i] = new SBSVector3(pixelSize.x * (v[i].x - pixelRect.xMin), pixelSize.y * (v[i].y - pixelRect.yMin), 0.01f) -
                                            screenCenter -
                                            halfPixelSize;

        dirtyFlags |= DirtyFlags.Vertices;
        dirtyFlags2 |= DirtyFlags.Vertices;
    }

    public void FillUVs(int index, Rect imgRect)
    {
        int baseIndex = index << 2;

        Vector2 texelSize = image.texelSize;

        uvBuffer[baseIndex + 0] = new Vector2(texelSize.x * imgRect.xMin + texelSize.x * 0.5f, 1.0f - texelSize.y * imgRect.yMin - texelSize.y * 0.5f);
        uvBuffer[baseIndex + 1] = new Vector2(texelSize.x * imgRect.xMax - texelSize.x * 0.5f, 1.0f - texelSize.y * imgRect.yMin - texelSize.y * 0.5f);
        uvBuffer[baseIndex + 2] = new Vector2(texelSize.x * imgRect.xMax - texelSize.x * 0.5f, 1.0f - texelSize.y * imgRect.yMax + texelSize.y * 0.5f);
        uvBuffer[baseIndex + 3] = new Vector2(texelSize.x * imgRect.xMin + texelSize.x * 0.5f, 1.0f - texelSize.y * imgRect.yMax + texelSize.y * 0.5f);
/*      uvBuffer[baseIndex + 0] = new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMin);
        uvBuffer[baseIndex + 1] = new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMin);
        uvBuffer[baseIndex + 2] = new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMax);
        uvBuffer[baseIndex + 3] = new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMax);
*/
        dirtyFlags |= DirtyFlags.UVs;
        dirtyFlags2 |= DirtyFlags.UVs;
    }
    /*
    public void FillClippedVerticesAndUVs(int index0, ref int index1, SBSMatrix4x4 transform, Rect scrRect, Rect imgRect, int scissorRectIndex)
    {
        if (scissorRectIndex >= scissorRects.Count || null == scissorRectNames[scissorRectIndex])
        {
            if (index1 >= 0)
            {
                this.FreeExtraQuad(index1);
                index1 = -1;
            }

            this.FillVertices(index0, transform, scrRect);
            this.FillUVs(index0, imgRect);
            return;
        }

        Rect scissorRect = scissorRects[scissorRectIndex];

        SBSVector3[] v = {
            transform.MultiplyPoint3x4(scrRect.xMin, scrRect.yMin, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMax, scrRect.yMin, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMax, scrRect.yMax, 0.0f),
            transform.MultiplyPoint3x4(scrRect.xMin, scrRect.yMax, 0.0f)
        };

        float xMin = float.MaxValue, xMax = -float.MaxValue, yMin = float.MaxValue, yMax = -float.MaxValue;
        foreach (SBSVector3 vv in v)
        {
            xMin = SBSMath.Min(xMin, vv.x);
            yMin = SBSMath.Min(yMin, vv.y);
            xMax = SBSMath.Max(xMax, vv.x);
            yMax = SBSMath.Max(yMax, vv.y);
        }

        SBSMath.ClipStatus clipStatus = SBSMath.GetClipStatus(scissorRect, Rect.MinMaxRect(xMin, yMin, xMax, yMax));
        if (SBSMath.ClipStatus.Outside == clipStatus)
        {
            bool wasVisible = clipVisibility[index0] || (index1 >= 0 ? clipVisibility[index1] : false);

            clipVisibility[index0] = false;
            if (index1 >= 0)
                clipVisibility[index1] = false;

            if (wasVisible)
                dirtyFlags |= DirtyFlags.Indices;
            return;
        }
        else if (SBSMath.ClipStatus.Inside == clipStatus)
        {
            bool updIndices = !clipVisibility[index0] || (index1 >= 0 ? clipVisibility[index1] : false);

            clipVisibility[index0] = true;
            if (index1 >= 0)
                clipVisibility[index1] = false;

            this.FillVertices(index0, transform, scrRect);
            this.FillUVs(index0, imgRect);

            if (updIndices)
                dirtyFlags |= DirtyFlags.Indices;
            return;
        }

        SBSVector3 pixelSize = new SBSVector3(layer.Camera.aspect / layer.Camera.pixelWidth, -1.0f / layer.Camera.pixelHeight),
                   halfPixelSize = isOpenGL ? SBSVector3.zero : pixelSize * 0.5f,
                   screenCenter = new SBSVector3(layer.Camera.aspect * 0.5f, -0.5f);

        Vector2 texelSize = image.texelSize;

        List<SBSClip.Vertex> output = new List<SBSClip.Vertex>();

        SBSPlane[] clipPlanes = {
                                    new SBSPlane(SBSVector3.right, new SBSVector3(scissorRect.xMin, scissorRect.yMin)),
                                    new SBSPlane(SBSVector3.left, new SBSVector3(scissorRect.xMax, scissorRect.yMin)),
                                    new SBSPlane(SBSVector3.up, new SBSVector3(scissorRect.xMin, scissorRect.yMin)),
                                    new SBSPlane(SBSVector3.down, new SBSVector3(scissorRect.xMin, scissorRect.yMax))
                                };

        int baseIndex0 = index0 << 2, baseIndex1 = 0, i;

        if (!SBSClip.ClipPolygon(v, clipPlanes, output))
        { // not visible
            bool wasVisible = clipVisibility[index0] || (index1 >= 0 ? clipVisibility[index1] : false);

            clipVisibility[index0] = false;
            if (index1 >= 0)
                clipVisibility[index1] = false;

            if (wasVisible)
                dirtyFlags |= DirtyFlags.Indices;
            return;
        }
        else
        {
            bool wasVisible = clipVisibility[index0] || (index1 >= 0 ? clipVisibility[index1] : false);

            clipVisibility[index0] = true;
            if (index1 >= 0)
                clipVisibility[index1] = true;

            if (!wasVisible)
                dirtyFlags |= DirtyFlags.Indices;
        }

        if (output.Count > 4 && index1 < 0)
        {
            index1 = this.ReserveExtraQuad(subMeshIndices[index0]);
            baseIndex1 = index1 << 2;
        }

        int count = output.Count;
        if (count > 0)
        {
            Asserts.Assert(count > 3 && count <= 6);

            Vector2[] uv = {
                               new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMin),
                               new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMin),
                               new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMax),
                               new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMax)
                           };

            SBSVector3 v0 = output[0].Sample<SBSVector3, SBSVector3Mix>(v);
            Vector2 uv0 = output[0].Sample<Vector2, Vector2Mix>(uv);
            for (i = 3; i < count; i += 3)
            {
                int baseIndex = (i < 4 ? baseIndex0 : baseIndex1);

                SBSVector3 v1 = output[i - 2].Sample<SBSVector3, SBSVector3Mix>(v),
                           v2 = output[i - 1].Sample<SBSVector3, SBSVector3Mix>(v),
                           v3 = output[i - 0].Sample<SBSVector3, SBSVector3Mix>(v);

                Vector2 uv1 = output[i - 2].Sample<Vector2, Vector2Mix>(uv),
                        uv2 = output[i - 1].Sample<Vector2, Vector2Mix>(uv),
                        uv3 = output[i - 0].Sample<Vector2, Vector2Mix>(uv);

                vertexBuffer[baseIndex + 0] = new SBSVector3(pixelSize.x * v0.x, pixelSize.y * v0.y, 0.01f) -
                                                screenCenter -
                                                halfPixelSize;
                vertexBuffer[baseIndex + 1] = new SBSVector3(pixelSize.x * v1.x, pixelSize.y * v1.y, 0.01f) -
                                                screenCenter -
                                                halfPixelSize;
                vertexBuffer[baseIndex + 2] = new SBSVector3(pixelSize.x * v2.x, pixelSize.y * v2.y, 0.01f) -
                                                screenCenter -
                                                halfPixelSize;
                vertexBuffer[baseIndex + 3] = new SBSVector3(pixelSize.x * v3.x, pixelSize.y * v3.y, 0.01f) -
                                                screenCenter -
                                                halfPixelSize;

                uvBuffer[baseIndex + 0] = uv0;
                uvBuffer[baseIndex + 1] = uv1;
                uvBuffer[baseIndex + 2] = uv2;
                uvBuffer[baseIndex + 3] = uv3;
            }
        }
        else
        {
            for (i = 0; i < 4; ++i)
                vertexBuffer[baseIndex0 + i] = new SBSVector3(pixelSize.x * v[i].x, pixelSize.y * v[i].y, 0.01f) -
                                                screenCenter -
                                                halfPixelSize;

            uvBuffer[baseIndex0 + 0] = new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMin);
            uvBuffer[baseIndex0 + 1] = new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMin);
            uvBuffer[baseIndex0 + 2] = new Vector2(texelSize.x * imgRect.xMax, 1.0f - texelSize.y * imgRect.yMax);
            uvBuffer[baseIndex0 + 3] = new Vector2(texelSize.x * imgRect.xMin, 1.0f - texelSize.y * imgRect.yMax);
        }

        dirtyFlags |= (DirtyFlags.Vertices | DirtyFlags.UVs);
    }
    */
    public void FillColors(int index, Color color)
    {
        int baseIndex = index << 2;

        colorBuffer[baseIndex + 0] = color;
        colorBuffer[baseIndex + 1] = color;
        colorBuffer[baseIndex + 2] = color;
        colorBuffer[baseIndex + 3] = color;

        dirtyFlags |= DirtyFlags.Colors;
        dirtyFlags2 |= DirtyFlags.Colors;
    }
    /*
    public bool IsQuadClipped(int index)
    {
        return !clipVisibility[index];
    }
    */
    public void SetVisibility(int index, bool flag)
    {
        visibility[index] = flag;

        dirtyFlags |= DirtyFlags.Indices;
        dirtyFlags2 |= DirtyFlags.Indices;
    }

    public void SetDepth(int index, int depth)
    {
        subMeshQuadsCount[subMeshIndices[index]] -= 1;
#if UNITY_FLASH
        int newIndex = this.GetSubMeshIndex(depth);
        subMeshIndices[index] = newIndex;
#else
        int newIndex = subMeshIndices[index] = this.GetSubMeshIndex(depth);
#endif
        subMeshQuadsCount[newIndex] += 1;

        dirtyFlags |= DirtyFlags.Indices;
        dirtyFlags2 |= DirtyFlags.Indices;
    }
    /*
    public void SetDepth(int index0, int index1, int depth)
    {
        subMeshQuadsCount[subMeshIndices[index0]] -= 1;
        int newIndex = subMeshIndices[index0] = this.GetSubMeshIndex(depth);
        subMeshQuadsCount[newIndex] += 1;

        subMeshQuadsCount[subMeshIndices[index1]] -= 1;
        subMeshIndices[index1] = subMeshIndices[index0];
        subMeshQuadsCount[newIndex] += 1;

        dirtyFlags |= DirtyFlags.Indices;
    }
    */
    public void RemoveOverlay(int index)
    {
        freeIndices.Push(index);
        usedIndices.Remove(index);//ToDo: optimize
        subMeshQuadsCount[subMeshIndices[index]] -= 1;

        dirtyFlags |= DirtyFlags.Indices;
        dirtyFlags2 |= DirtyFlags.Indices;
    }
    /*
    public void RemoveOverlay(int index0, int index1)
    {
        freeIndices.Push(index0);
        freeIndices.Push(index1);
        usedIndices.Remove(index0);//ToDo: optimize
        usedIndices.Remove(index1);//ToDo: optimize
        subMeshQuadsCount[subMeshIndices[index0]] -= 1;
        subMeshQuadsCount[subMeshIndices[index1]] -= 1;

        dirtyFlags |= DirtyFlags.Indices;
    }*/
    /*
    public int GetScissorRectIndex(string name)
    {
        int i = 0;
        foreach (string srName in scissorRectNames)
        {
            if (srName == name)
                return i;

            ++i;
        }
        return -1;
    }

    public string GetScissorRectName(int index)
    {
        return scissorRectNames[index];
    }

    public int SetScissorRect(string name, Rect scissorRect)
    {
        int i = 0;
        foreach (string srName in scissorRectNames)
        {
            if (null == srName || name == srName)
            {
                scissorRectNames[i] = name;
                scissorRects[i] = scissorRect;
                return i;
            }

            ++i;
        }

        scissorRectNames.Add(name);
        scissorRects.Add(scissorRect);

        return scissorRects.Count - 1;
    }

    public bool RemoveScissorRect(string name)
    {
        int index = this.GetScissorRectIndex(name);
        if (index >= 0)
        {
            scissorRectNames[index] = null;
            return true;
        }
        return false;
    }

    public bool RemoveScissorRect(int index)
    {
        if (index >= 0 && index < scissorRectNames.Count)
        {
            scissorRectNames[index] = null;
            return true;
        }
        return false;
    }
    */
    public void Flush()
    {
        bool recalcBounds = false;

        Mesh dstMesh = meshSwap ? mesh : mesh2;/*,
             othMesh = meshSwap ? mesh2 : mesh;*/
        DirtyFlags dFlags = meshSwap ? dirtyFlags : dirtyFlags2;
        meshSwap = !meshSwap;

        if ((dFlags & DirtyFlags.Vertices) > 0)
        {
            dstMesh.vertices = vertexBuffer;
            recalcBounds = true;
            dFlags &= ~DirtyFlags.Vertices;
        }

        if ((dFlags & DirtyFlags.UVs) > 0)
        {
            dstMesh.uv = uvBuffer;
            dFlags &= ~DirtyFlags.UVs;
        }

        if ((dFlags & DirtyFlags.Colors) > 0)
        {
            dstMesh.colors = colorBuffer;
            dFlags &= ~DirtyFlags.Colors;
        }

        if ((dFlags & DirtyFlags.Indices) > 0)
        {/*
            int numIndices = usedIndices.Count * 6;
            int[] indexBuffer = new int[numIndices];
            int counter = 0;

            foreach (int index in usedIndices)
            {
                if (!visibility[index] || !clipVisibility[index])
                    continue;

                int baseIndex = index << 2;

                indexBuffer[counter++] = baseIndex + 0;
                indexBuffer[counter++] = baseIndex + 1;
                indexBuffer[counter++] = baseIndex + 2;
                indexBuffer[counter++] = baseIndex + 2;
                indexBuffer[counter++] = baseIndex + 3;
                indexBuffer[counter++] = baseIndex + 0;
            }

            mesh.SetTriangles(indexBuffer, 0);*/
            int subMeshesCount = subMeshQuadsCount.Length;
            int[][] indexBuffers = new int[subMeshesCount][];

            int counter = 0;
            foreach (int quadsCount in subMeshQuadsCount)
                indexBuffers[counter++] = new int[Mathf.Max(0, quadsCount) * 6];

            int[] ibCounters = new int[subMeshesCount];
            foreach (int index in usedIndices)
            {
                if (!visibility[index])// || !clipVisibility[index])
                    continue;

                int baseIndex = index << 2,
                    subMeshIndex = subMeshIndices[index];

                int[] indexBuffer = indexBuffers[subMeshIndex];
                counter = ibCounters[subMeshIndex];

                indexBuffer[counter++] = baseIndex + 0;
                indexBuffer[counter++] = baseIndex + 1;
                indexBuffer[counter++] = baseIndex + 2;
                indexBuffer[counter++] = baseIndex + 2;
                indexBuffer[counter++] = baseIndex + 3;
                indexBuffer[counter++] = baseIndex + 0;

                ibCounters[subMeshIndex] = counter;
            }

            dstMesh.subMeshCount = subMeshesCount;
            for (counter = 0; counter < subMeshesCount; ++counter)
                dstMesh.SetTriangles(indexBuffers[counter], counter);

            recalcBounds = true;

            meshRenderer.enabled = (usedIndices.Count > 0);

            dFlags &= ~DirtyFlags.Indices;
        }

        if (recalcBounds)
            dstMesh.RecalculateBounds();

        meshFilter.mesh = dstMesh;

        if (meshSwap)
            dirtyFlags2 = dFlags;
        else
            dirtyFlags = dFlags;
    }

    public void Destroy()
    {
//      UnityEngine.Object.DestroyImmediate(material);
#if UNITY_FLASH
        foreach (KeyValuePair<int, Material> matItem in materials)
            UnityEngine.Object.DestroyImmediate(matItem.Value);
#else
        foreach (Material mat in materials.Values)
            UnityEngine.Object.DestroyImmediate(mat);
#endif
        materials.Clear();

        UnityEngine.Object.DestroyImmediate(mesh);
//      UnityEngine.Object.DestroyImmediate(gameObject);
    }
    #endregion
}
