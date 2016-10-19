Shader "Curve/Toon/Rim" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}

	_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}

	//COLORS
	_Color ("Highlight Color", Color) = (0.8,0.8,0.8,1)
	_SColor ("Shadow Color", Color) = (0.0,0.0,0.0,1)

	//RIM LIGHT
	_RimColor ("Rim Color", Color) = (0.8,0.8,0.8,0.6)
	_RimPower ("Rim Power", Range(-2,10)) = 0.5
}

CGINCLUDE

#include "Curve_Include.cginc"

#ifdef LIGHTMAP_ON
v2f_diffuse_rim vert1(appdata_base_LM v)
#else
v2f_diffuse_rim vert1(appdata_base v)
#endif
{
    v2f_diffuse_rim o;

	o.pos = Curve_OutputPosition(v.vertex);

    o.uv = v.texcoord.xy;
#ifdef LIGHTMAP_ON
	o.uvLM.xy = v.texcoord1.xy;
#endif

	o.lightDirT = ObjSpaceLightDir(v.vertex);
	o.viewDirT = normalize(ObjSpaceViewDir(v.vertex));

	o.normal = v.normal;

	VERTEX_FOG(v, o);
    TRANSFER_VERTEX_TO_FRAGMENT(o);

    return o;
}

fixed4 frag1(v2f_diffuse_rim i) : COLOR
{
	return CurveToonRim_OutputColorDiffuse(i);
}

ENDCG

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

	Pass {
            Name "ContentBase"
            Tags {"LightMode" = "ForwardBase"}

CGPROGRAM

#pragma exclude_renderers flash
#pragma fragmentoption ARB_precision_hint_fastest
#pragma glsl_no_auto_normalization
#pragma multi_compile_fwdbase
#pragma vertex vert1
#pragma fragment frag1

ENDCG

	}
}

Fallback "Mobile/VertexLit"
}
