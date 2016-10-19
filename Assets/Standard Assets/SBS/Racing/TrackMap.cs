using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SBS.Core;
using SBS.Math;
using SBS.Racing;

[AddComponentMenu("SBS/Racing/TrackMap")]
public class TrackMap : MonoBehaviour
{
    #region Public members
    public RectOffset padding;
    public Bounds bounds;
    public Texture2D image;
    public int imageWidth;
    public int imageHeight;
    #endregion

    #region Public methods
#if UNITY_EDITOR
    public byte[] CreateTexture(int width, int height, string imagePath)
    {
        bounds.SetMinMax(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), new Vector3(float.MinValue, float.MinValue, float.MinValue));

        Mesh mesh = null;

#if UNITY_4_3
        Token[] tokens = GameObject.FindObjectsOfType(typeof(Token)) as Token[];
#else
		Token[] tokens = GameObject.FindSceneObjectsOfType(typeof(Token)) as Token[];
#endif
		foreach (Token token in tokens)
        {
            SBSBounds tokenBounds = token.ComputeBounds();
            tokenBounds.min.y = token.transform.position.y - 5.0f;
            tokenBounds.max.y = token.transform.position.y + 5.0f;
            bounds.Encapsulate(tokenBounds);
            token.CreateMesh(token.type == Token.TokenType.Rect ? 1 : 10, ref mesh, true);
        }

        float aspect      = height / (float)width,
              orthoHeight = Mathf.Max(bounds.size.x * aspect, bounds.size.z);

        Vector3 pixelSize = new Vector3(orthoHeight * aspect, 0.0f, orthoHeight);
        pixelSize.x /= (float)width;
        pixelSize.z /= (float)height;

        bounds.min -= padding.left * pixelSize.x * Vector3.right;
        bounds.max += padding.right * pixelSize.x * Vector3.right;
        bounds.min -= padding.bottom * pixelSize.z * Vector3.forward;
        bounds.max += padding.top * pixelSize.z * Vector3.forward;

        orthoHeight = Mathf.Max(bounds.size.x * aspect, bounds.size.z);

        Camera cam = new GameObject("mapRender", typeof(Camera)).GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthoHeight * 0.5f;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
        cam.transform.position = bounds.center + (1.0f + bounds.size.y * 0.5f) * Vector3.up;
        cam.nearClipPlane = 1.0f;
        cam.farClipPlane = 1.0f + bounds.size.y;
        cam.transform.LookAt(bounds.center, Vector3.forward);

        RenderTexture map = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        cam.targetTexture = map;

        Material tokensMaterial = new Material(Shader.Find("Unlit/Texture"));

        GameObject go = new GameObject("mapTokens", typeof(MeshFilter), typeof(MeshRenderer));
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterial = tokensMaterial;
        go.layer = 31;
        cam.cullingMask = (1 << go.layer);

        cam.Render();

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false, true);
        RenderTexture.active = map;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(map);
        GameObject.DestroyImmediate(cam.gameObject);
        GameObject.DestroyImmediate(go);
        /*
        byte[] pngData = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(imagePath, pngData);
        AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter imageImp = TextureImporter.GetAtPath(imagePath) as TextureImporter;
        imageImp.textureType = TextureImporterType.Advanced;
        imageImp.npotScale = TextureImporterNPOTScale.None;
        imageImp.mipmapEnabled = false;
        imageImp.isReadable = false;
        imageImp.linearTexture = true;
        imageImp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        imageImp.wrapMode = TextureWrapMode.Clamp;
        imageImp.filterMode = FilterMode.Point;
        AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);

        image = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath, typeof(Texture2D));
        imageWidth = width;
        imageHeight = height;*/
        return screenShot.EncodeToPNG();
    }
#endif
    #endregion
}
