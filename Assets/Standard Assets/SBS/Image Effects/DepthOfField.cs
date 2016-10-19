using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("SBS/Image Effects/Depth Of Field")]
public class DepthOfField : MonoBehaviour
{
    public Shader blurShader;
    public Shader dofShader;

    public DownsampleSize downsample = DownsampleSize.x2;
    public GaussianBlurKernelSize blurKernelSize = GaussianBlurKernelSize.x5;

    public float focalForegroundSmooth = 20.0f;
    public float focalBackgroundSmooth = 20.0f;
    public float focalSize = 2.0f;

    public Transform focalPoint;

    public float strength = 1.0f;

    protected bool isSupported = false;
    protected Material blurMat = null;
    protected Material dofMat = null;

    protected float focalSize01;
    protected float focalDistance01;

	protected DepthTextureMode prevDepthTexMode;

    protected bool CheckResources()
    {
		if (!Helper.CheckSupport(GetComponent<Camera>(), true))
            return false;

        blurMat = Helper.CreateMaterial(blurShader, blurMat);
        if (null == blurMat)
            return false;

        dofMat = Helper.CreateMaterial(dofShader, dofMat);
        if (null == dofMat)
            return false;

        return true;
    }

    protected void UpdateVariables()
    {
        float oof = 1.0f / GetComponent<Camera>().farClipPlane;
        focalSize01 = Mathf.Clamp01(focalSize * oof);
        focalDistance01 = null == focalPoint ? 0.5f : Mathf.Clamp01(GetComponent<Camera>().WorldToViewportPoint(focalPoint.position).z * oof);
    }

    void Start()
    {
        isSupported = this.CheckResources();
    }

    void OnEnable()
    {
		prevDepthTexMode = GetComponent<Camera>().depthTextureMode;

		isSupported = this.CheckResources();
    }

	void OnDisable()
	{
		GetComponent<Camera>().depthTextureMode = prevDepthTexMode;
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!isSupported && !this.CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }

        int div = 1 << (int)downsample;

        RenderTexture tmp0 = RenderTexture.GetTemporary(source.width / div, source.height / div, 0),
                      tmp1 = RenderTexture.GetTemporary(source.width / div, source.height / div, 0);

        Graphics.Blit(source, tmp0);

        if (blurKernelSize > GaussianBlurKernelSize.x0)
        {
            Graphics.Blit(tmp0, tmp1, blurMat, (int)blurKernelSize);
#if UNITY_IPHONE
			RenderTexture.active = tmp0;
			GL.Clear(false, true, Color.black);
#endif
            Graphics.Blit(tmp1, tmp0, blurMat, (int)blurKernelSize + 3);
        }

        //Graphics.Blit(tmp0, destination);
        this.UpdateVariables();
        float f0 = focalDistance01 - focalSize01 * 0.5f, f1 = focalDistance01 + focalSize01 * 0.5f;
        dofMat.SetVector("_CurveParams", new Vector4(focalForegroundSmooth, focalBackgroundSmooth, f0, f1));
        dofMat.SetFloat("_Strength", strength);
        dofMat.SetTexture("_Blurred", tmp0);

        Graphics.Blit(source, destination, dofMat, 0);

        RenderTexture.ReleaseTemporary(tmp0);
        RenderTexture.ReleaseTemporary(tmp1);
    }
}
