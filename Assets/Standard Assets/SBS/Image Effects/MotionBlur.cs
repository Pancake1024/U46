using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("SBS/Image Effects/Motion Blur")]
public class MotionBlur : MonoBehaviour
{
    static string blendShader = @"
Shader ""Hidden/MotionBlurBlend""  {
	Properties { _MainTex (""Texture"", any) = """" {} } 

    SubShader { 

		Tags { ""ForceSupported""=""True"" ""RenderType""=""Overlay"" } 

		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Fog { Mode Off }
		ZTest Always
        ColorMask RGB

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

    public enum UpdateFunction
    {
        Update = 0,
        LateUpdate,
        FixedUpdate
    }

    public enum Quality
    {
        Low = 0,
        Medium,
        High
    }

    public float velocityMultiplier = 2.0f;
    public float blurStrength = 12.0f;
    public LayerMask noBlurMask;
    public UpdateFunction updateFunction = UpdateFunction.FixedUpdate;
    public Quality quality = Quality.Low;

    protected bool isSupported = false;
    protected Material blurMat = null;
    protected Shader objBlurShader = null;

    protected Matrix4x4 prevViewProj;
    protected Matrix4x4 matrix;

    protected Matrix4x4 objPrevViewProj;

    protected Camera noBlurCamera;
    protected RenderTexture noBlurVelBuffer;
    protected Material noBlurVelBlendMat;

    protected bool CheckResources()
    {
        if (!Helper.CheckSupport(GetComponent<Camera>(), true))
            return false;

        blurMat = Helper.CreateMaterial(Shader.Find("Hidden/SBS/MotionBlur"), blurMat);
        if (null == blurMat)
            return false;

        objBlurShader = Shader.Find("Hidden/SBS/MotionBlurObject");
        if (null == objBlurShader)
            return false;

        if (null == noBlurCamera)
        {
            GameObject go = GameObject.Find("__noBlurCamera");
            if (go != null)
                noBlurCamera = go.GetComponent<Camera>();

            if (null == noBlurCamera)
            {
                noBlurCamera = new GameObject("__noBlurCamera", typeof(Camera)).GetComponent<Camera>();
                noBlurCamera.enabled = false;
                noBlurCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        noBlurVelBlendMat = new Material(blendShader);
        if (null == noBlurVelBlendMat)
            return false;

        return true;
    }

    void Start()
    {
        isSupported = this.CheckResources();
    }

    void OnEnable()
    {
        isSupported = this.CheckResources();
    }

    void FixedUpdate()
    {
        if (updateFunction != UpdateFunction.FixedUpdate)
            return;

        matrix = prevViewProj * GetComponent<Camera>().cameraToWorldMatrix;
        prevViewProj = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
        objPrevViewProj = GetComponent<Camera>().worldToCameraMatrix * GetComponent<Camera>().projectionMatrix;
    }

    void Update()
    {
        if (updateFunction != UpdateFunction.Update)
            return;

        matrix = prevViewProj * GetComponent<Camera>().cameraToWorldMatrix;
        prevViewProj = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
        objPrevViewProj = GetComponent<Camera>().worldToCameraMatrix * GetComponent<Camera>().projectionMatrix;
    }

    void LateUpdate()
    {
        if (updateFunction != UpdateFunction.LateUpdate)
            return;

        matrix = prevViewProj * GetComponent<Camera>().cameraToWorldMatrix;
        prevViewProj = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
        objPrevViewProj = GetComponent<Camera>().worldToCameraMatrix * GetComponent<Camera>().projectionMatrix;
    }

    void OnPostRender()
    {
        if (null == noBlurVelBuffer)
            noBlurVelBuffer = new RenderTexture(Screen.width, Screen.height, 16);

        noBlurCamera.CopyFrom(GetComponent<Camera>());
        noBlurCamera.targetTexture = noBlurVelBuffer;
        noBlurCamera.transform.position = transform.position;
        noBlurCamera.transform.rotation = transform.rotation;
        noBlurCamera.depthTextureMode = DepthTextureMode.None;
        noBlurCamera.cullingMask = noBlurMask;
        noBlurCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        noBlurCamera.clearFlags = CameraClearFlags.Color;
        noBlurCamera.RenderWithShader(objBlurShader, "RenderType");
        noBlurCamera.targetTexture = null;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!isSupported && !this.CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        blurMat.SetMatrix("_PrevViewProj", matrix);

        blurMat.SetFloat("_VelocityMult", velocityMultiplier);

        RenderTexture velBuffer = RenderTexture.GetTemporary(source.width, source.height);//, 0);
        RenderTexture.active = velBuffer;
        blurMat.SetPass(0);
        Helper.DrawFsQuadRecPos(GetComponent<Camera>());
        RenderTexture.active = null;

        RenderTexture.active = velBuffer;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, velBuffer.width, velBuffer.height, 0);
        Graphics.DrawTexture(new Rect(0, 0, velBuffer.width, velBuffer.height), noBlurVelBuffer, noBlurVelBlendMat);
        GL.PopMatrix();
        RenderTexture.active = null;

        blurMat.SetTexture("_VelocityBuffer", velBuffer);
        blurMat.SetFloat("_BlurStrength", blurStrength);
        Graphics.Blit(source, destination, blurMat, 1 + (int)quality);

        RenderTexture.ReleaseTemporary(velBuffer);
    }
}
