Shader "Curve/Unlit" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
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
	return Curve_OutputColorUnlit(i);
}

ENDCG

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

	Pass {
            Name "ContentBase"
            //Tags {"LightMode" = "ForwardBase"}
			Lighting Off

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
