Shader "Mobile/Car Paint/Fill Depth" {
Properties {
}

CGINCLUDE

#include "UnityCG.cginc"

struct v2f_depth
{
    half4 pos: SV_POSITION;
};

v2f_depth vert_depth(appdata_base v)
{
	v2f_depth o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	return o;
}

fixed4 frag_depth(v2f_depth i): COLOR
{
	return fixed4(1, 1, 1, 1);
}

ENDCG

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

	Pass {
		Name "DepthFill"

		Lighting Off
		Fog { Mode Off }

		ColorMask 0

		Cull Back
		ZWrite On
		ZTest LEqual

CGPROGRAM

#pragma exclude_renderers flash
#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert_depth
#pragma fragment frag_depth

ENDCG

	}
}

Fallback Off
}
