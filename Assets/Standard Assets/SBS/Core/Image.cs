using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
    public enum BlitOpType
    {
        Copy = 0, // Blend Off
        Blend,    // Blend SrcAlpha OneMinusSrcAlpha
        Multiply, // Blend Zero SrcColor
        Add,      // Blend One One
        CopyAlpha,// Blend Off, ColorMask A
        Count
    }

    public struct BlitOp
    {
        public BlitOpType type;
        public Texture source;
        public Rect sourceRect;
        public Rect destRect;
    }

    internal class ImageUtils
    {
        static string shader = @"
Shader ""Hidden/<name>""  {
	Properties { _MainTex (""Texture"", any) = """" {} } 

    SubShader { 

		Tags { ""ForceSupported""=""True"" ""RenderType""=""Overlay"" } 

		Lighting Off
		<blend>
		Cull Off
		ZWrite Off
		Fog { Mode Off }
		ZTest Always
        <colormask>

		BindChannels { 
			Bind ""vertex"", vertex 
			Bind ""color"", color 
			Bind ""TexCoord"", texcoord 
		} 

		Pass { 
			SetTexture [_MainTex] {
				combine primary * texture DOUBLE, primary * texture DOUBLE
			} 
		} 
	} 

	Fallback off 
}";

        static Material[] blitMaterials = new Material[(int)BlitOpType.Count];

        internal static Material GetMaterial(BlitOpType opType)
        {
            Material mat = blitMaterials[(int)opType];

            if (null == mat)
            {
                string shd = null;

                switch (opType)
                {
                    case BlitOpType.Copy:
                        shd = shader.Replace("<name>", "Image-Copy").Replace("<blend>", "Blend Off").Replace("<colormask>", "ColorMask RGBA");
                        break;
                    case BlitOpType.Blend:
                        shd = shader.Replace("<name>", "Image-Blend").Replace("<blend>", "Blend SrcAlpha OneMinusSrcAlpha").Replace("<colormask>", "ColorMask RGB");
                        break;
                    case BlitOpType.Multiply:
                        shd = shader.Replace("<name>", "Image-Multiply").Replace("<blend>", "Blend Zero SrcColor").Replace("<colormask>", "ColorMask RGB");
                        break;
                    case BlitOpType.Add:
                        shd = shader.Replace("<name>", "Image-Add").Replace("<blend>", "Blend One One").Replace("<colormask>", "ColorMask RGB");
                        break;
                    case BlitOpType.CopyAlpha:
                        shd = shader.Replace("<name>", "Image-CopyAlpha").Replace("<blend>", "Blend Off").Replace("<colormask>", "ColorMask A");
                        break;
                    default:
                        Asserts.Assert(false);
                        break;
                }

                mat = new Material(shd);
                blitMaterials[(int)opType] = mat;
            }

            return mat;
        }

        internal static RenderTexture BeginRenderOn(RenderTexture dest)
        {
            RenderTexture prevActiveRt = RenderTexture.active;

            RenderTexture.active = dest;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, dest.width, dest.height, 0);

            return prevActiveRt;
        }

        internal static void Blit(BlitOp op)
        {
            Rect uvSourceRect = op.sourceRect;

            uvSourceRect.xMin = (uvSourceRect.xMin - 0.5f) * op.source.texelSize.x;
            uvSourceRect.xMax = (uvSourceRect.xMax + 0.5f) * op.source.texelSize.x;
            uvSourceRect.yMin = 1.0f - ((uvSourceRect.yMin + 0.5f) * op.source.texelSize.y);
            uvSourceRect.yMax = 1.0f - ((uvSourceRect.yMax - 0.5f) * op.source.texelSize.y);

            Graphics.DrawTexture(op.destRect, op.source, uvSourceRect, 0, 0, 0, 0, GetMaterial(op.type));
        }

        internal static Texture2D ReadPixels(TextureFormat format, bool useMipMaps, bool compressed)
        {
            RenderTexture rt = RenderTexture.active;
            Texture2D t = new Texture2D(rt.width, rt.height, format, useMipMaps);

            t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            t.Apply(useMipMaps, !compressed);

            if (compressed)
            {
                t.Compress(false);
                t.Apply(useMipMaps, true);
            }

            return t;
        }

        internal static void EndRender(RenderTexture prevActiveRt)
        {
            GL.PopMatrix();

            RenderTexture.active = prevActiveRt;
        }
    }

    public class Image : IDisposable
    {
        protected RenderTexture rt;
        protected Rect screenRect;

        public Rect rect
        {
            get
            {
                return screenRect;
            }
        }

        public Texture texture
        {
            get
            {
                return rt;
            }
        }
		
		public bool useMipMap
		{
			get
			{
				return rt.useMipMap;
			}
			set
			{
				rt.useMipMap = value;
			}
		}
		
        public Image(int width, int height)
        {
#if UNITY_IPHONE
            rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB4444, RenderTextureReadWrite.Default);
#else
            rt = new RenderTexture(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif
            rt.useMipMap = false;
            screenRect = new Rect(0, 0, width, height);
        }

        public Image(int width, int height, BlitOp[] blitOps)
            : this(width, height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            ImageUtils.EndRender(prevActiveRt);
        }

        public Image(int width, int height, Color clearColor, BlitOp[] blitOps)
            : this(width, height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            GL.Clear(false, true, clearColor);
            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            ImageUtils.EndRender(prevActiveRt);
        }

        public Image(Texture source)
            : this(source.width, source.height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            Graphics.DrawTexture(screenRect, source);

            ImageUtils.EndRender(prevActiveRt);
        }

        public Image(Texture source, BlitOp[] blitOps)
            : this(source.width, source.height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            Graphics.DrawTexture(screenRect, source, ImageUtils.GetMaterial(BlitOpType.Copy));
            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            ImageUtils.EndRender(prevActiveRt);
        }
        
        private Image(Texture source, BlitOp[] blitOps, TextureFormat format, bool useMipMaps, bool compressed, out Texture2D tex)
            : this(source.width, source.height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            Graphics.DrawTexture(screenRect, source, ImageUtils.GetMaterial(BlitOpType.Copy));
            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            tex = ImageUtils.ReadPixels(format, useMipMaps, compressed);

            ImageUtils.EndRender(prevActiveRt);
        }

        private Image(int width, int height, Color clearColor, BlitOp[] blitOps, TextureFormat format, bool useMipMaps, bool compressed, out Texture2D tex)
            : this(width, height)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            GL.Clear(false, true, clearColor);
            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            tex = ImageUtils.ReadPixels(format, useMipMaps, compressed);

            ImageUtils.EndRender(prevActiveRt);
        }
        
        public void Clear(Color color)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            GL.Clear(false, true, color);

            ImageUtils.EndRender(prevActiveRt);
        }

        public void Blit(BlitOp blitOp)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            ImageUtils.Blit(blitOp);

            ImageUtils.EndRender(prevActiveRt);
        }

        public void Blit(BlitOp[] blitOps)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            ImageUtils.EndRender(prevActiveRt);
        }

        public void ClearAndBlit(Color color, BlitOp[] blitOps)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            GL.Clear(false, true, color);
            foreach (BlitOp op in blitOps)
                ImageUtils.Blit(op);

            ImageUtils.EndRender(prevActiveRt);
        }

        public Texture2D Finalize(TextureFormat format, bool useMipMaps, bool compressed)
        {
            RenderTexture prevActiveRt = ImageUtils.BeginRenderOn(rt);

            Texture2D t = ImageUtils.ReadPixels(format, useMipMaps, compressed);

            ImageUtils.EndRender(prevActiveRt);

            return t;
        }

        public Texture2D Finalize(TextureFormat format, bool useMipMaps)
        {
            return this.Finalize(format, useMipMaps, true);
        }

        public Texture2D Finalize(TextureFormat format)
        {
            return this.Finalize(format, true, true);
        }

        public void Dispose()
        {
            RenderTexture.DestroyImmediate(rt);
        }

        public static Texture2D CreateTexture(Texture source, BlitOp[] blitOps, TextureFormat format, bool useMipMaps, bool compressed)
        {
            Texture2D tex;
            using (new Image(source, blitOps, format, useMipMaps, compressed, out tex))
                return tex;
        }

        public static Texture2D CreateTexture(int width, int height, Color clearColor, BlitOp[] blitOps, TextureFormat format, bool useMipMaps, bool compressed)
        {
            Texture2D tex;
            using (new Image(width, height, clearColor, blitOps, format, useMipMaps, compressed, out tex))
                return tex;
        }
        
        public static Texture2D CreateTexture(Texture source, BlitOp[] blitOps, TextureFormat format, bool useMipMaps)
        {
            return CreateTexture(source, blitOps, format, useMipMaps, true);
        }

        public static Texture2D CreateTexture(int width, int height, Color clearColor, BlitOp[] blitOps, TextureFormat format, bool useMipMaps)
        {
            return CreateTexture(width, height, clearColor, blitOps, format, useMipMaps, true);
        }

        public static Texture2D CreateTexture(Texture source, BlitOp[] blitOps, TextureFormat format)
        {
            return CreateTexture(source, blitOps, format, true, true);
        }

        public static Texture2D CreateTexture(int width, int height, Color clearColor, BlitOp[] blitOps, TextureFormat format)
        {
            return CreateTexture(width, height, clearColor, blitOps, format, true, true);
        }
    }
}
