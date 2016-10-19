Shader "Mobile/Self-Illumin/Diffuse" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Illum ("Illumin (A)", 2D) = "white" {}
	_EmissionLM ("Emission (Lightmapper)", Float) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	
CGPROGRAM
#pragma surface surf MobileLambert vertex:vert exclude_path:prepass noforwardadd halfasview novertexlights

#include "UnityCG.cginc"

inline fixed4 LightingMobileLambert (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff * atten * 2);
	c.a = 1.0;

	return c;
}

sampler2D _MainTex;
sampler2D _Illum;

struct Input {
	float2 uv_MainTex;
};

void vert(inout appdata_full v, out Input o)
{
	o.uv_MainTex = v.texcoord.xy;
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb;
	o.Emission = tex.rgb * tex2D(_Illum, IN.uv_MainTex).a;
	o.Alpha = tex.a;
}
ENDCG
} 
FallBack "Self-Illumin/VertexLit"
}
