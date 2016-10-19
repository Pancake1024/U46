using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.ImageEffects
{
    public enum GaussianBlurKernelSize
    {
        x0 = -1,
        x3 = 0,
        x5,
        x7
    }

    public enum DownsampleSize
    {
        x1 = 0,
        x2,
        x4,
        x8
    }

    public class Helper
    {
        static public Material CreateMaterial(Shader shader, Material material)
        {
            if (null == shader)
                return null;

            if (material != null && material.shader == shader && shader.isSupported)
                return material;

            if (!shader.isSupported)
                return null;

            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;

            return material;
        }

        static public bool CheckSupport(Camera camera, bool needDepth)
        {
            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
                return false;

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
                return false;

            if (needDepth)
                camera.depthTextureMode = DepthTextureMode.Depth;

            return true;
        }

        static public bool CheckSupport(Camera camera, bool needDepth, bool needNormals)
        {
            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
                return false;

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
                return false;

            if (needDepth && !needNormals)
                camera.depthTextureMode = DepthTextureMode.Depth;
            else if (needDepth || needNormals)
                camera.depthTextureMode = DepthTextureMode.DepthNormals;
            else
                camera.depthTextureMode = DepthTextureMode.None;

            return true;
        }

        static public bool CheckSupport(Camera camera, bool needDepth, bool needNormals, bool needHdr)
        {
            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
                return false;

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
                return false;

            if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                return false;

            if (needDepth && !needNormals)
                camera.depthTextureMode = DepthTextureMode.Depth;
            else if (needDepth || needNormals)
                camera.depthTextureMode = DepthTextureMode.DepthNormals;
            else
                camera.depthTextureMode = DepthTextureMode.None;

            return true;
        }

        static public void DrawFsQuadRecPos(Camera camera)
        {
            if (camera.orthographic)
                return;

            float t = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad),
                  x = t * camera.aspect * camera.farClipPlane,
                  y = t * camera.farClipPlane,
                  z = camera.farClipPlane;

            GL.PushMatrix();
            GL.LoadIdentity();

            //GL.LoadPixelMatrix(0.0f, Screen.width, 0.0f, Screen.height);
            GL.LoadProjectionMatrix(Matrix4x4.identity);
            //GL.LoadPixelMatrix(-1.0f, 1.0f, -1.0f, 1.0f);

            GL.Begin(GL.QUADS);

            GL.TexCoord3(-x, -y, -z);
            GL.Vertex3(-1.0f, -1.0f, 0.0f);
            GL.TexCoord3(x, -y, -z);
            GL.Vertex3(1.0f, -1.0f, 0.0f);
            GL.TexCoord3(x, y, -z);
            GL.Vertex3(1.0f, 1.0f, 0.0f);
            GL.TexCoord3(-x, y, -z);
            GL.Vertex3(-1.0f, 1.0f, 0.0f);

            GL.End();

            GL.PopMatrix();
        }

        static public void DrawFsQuad(Camera camera)
        {
            if (camera.orthographic)
                return;

            GL.PushMatrix();
            GL.LoadIdentity();

            GL.LoadProjectionMatrix(Matrix4x4.identity);

            GL.Begin(GL.QUADS);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-1.0f, -1.0f, 0.0f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(1.0f, -1.0f, 0.0f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 0.0f);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, 0.0f);

            GL.End();

            GL.PopMatrix();
        }
    }
}
