// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "Curve/Particles/Multiply" {

Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
}

CGINCLUDE

#include "Curve_Include.cginc"

#ifdef LIGHTMAP_ON
v2f_unlit vert1(appdata_base_LM v)
#else
v2f_unlit vert1(appdata_base v)
#endif
{
    v2f_unlit o;

	o.pos = Curve_OutputPosition(v.vertex);
    o.uv = v.texcoord.xy;
#ifdef LIGHTMAP_ON
	o.uvLM.xy = v.texcoord1.xy;
#endif

	VERTEX_FOG(v, o);

    return o;
}

fixed4 frag1(v2f_unlit i) : COLOR
{
#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	lm *= tex2D(_MainTex, i.uv);
	return PARTICLE_FRAG_FOG(i, fixed4(lm, 1));
#	else
	return PARTICLE_FRAG_FOG(i, tex2D(_MainTex, i.uv));
#	endif
}

ENDCG

SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Pass {

		Blend Zero SrcColor
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Color (1,1,1,1) }

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

}
