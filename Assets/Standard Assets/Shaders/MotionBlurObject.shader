Shader "Hidden/SBS/MotionBlurObject" {
Properties {
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	
	Fog { Mode off }

CGPROGRAM
#pragma surface surf NoLight vertex:vert exclude_path:prepass nolightmap noforwardadd halfasview novertexlights

#include "UnityCG.cginc"

inline fixed4 LightingNoLight(SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed4 c;
	c.rgb = float3(0.5, 0.5, 0.0);
	c.a = 1.0;
	return c;
}

struct Input {
	float4 clipPos;
};

void vert(inout appdata_full v, out Input o)
{
	o.clipPos = mul(UNITY_MATRIX_MVP, v.vertex);
}

void surf (Input IN, inout SurfaceOutput o)
{ }
ENDCG
}
}
