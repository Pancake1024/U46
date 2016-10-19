// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - Normalmap uses Tiling/Offset of the Base texture
// - no Deferred Lighting support
// - no Lightmap support
// - supports ONLY 1 directional light. Other lights are completely ignored.
Shader "Mobile/Reflective/Bumped Specular (1 Directional Light, Separate Gloss Map)" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	//_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Gloss ("Gloss (R)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf MobileBlinnPhong vertex:vert exclude_path:prepass nolightmap noforwardadd halfasview novertexlights

#include "UnityCG.cginc"

inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	fixed nh = max (0, dot (s.Normal, halfDir));
	fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec);// * (atten);//*2);
	c.a = 1.0;
	return c;
}

sampler2D _MainTex;
sampler2D _Gloss;
sampler2D _BumpMap;
samplerCUBE _Cube;
half _Shininess;
//fixed4 _ReflectColor;
float4x4 _CubeMatrix;

struct Input {
	float2 uv_MainTex;
	float3 normal;
	float3 color;
};

void vert(inout appdata_full v, out Input o)
{
	o.uv_MainTex = v.texcoord.xy;
    //o.normal = mul(_CubeMatrix, float4(v.normal, 1)).xyz;
	o.normal = reflect(normalize(ObjSpaceViewDir(v.vertex)), mul(_CubeMatrix, float4(v.normal, 1)).xyz);
	//o.normal = reflect(normalize(WorldSpaceViewDir(v.vertex)), mul(_CubeMatrix, float4(v.normal, 1)).xyz);
	o.color = ShadeSH9(half4(v.normal, 1.0)) * 0.5;
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	tex.a = tex2D(_Gloss, IN.uv_MainTex).r;

	o.Albedo = tex.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

	//fixed4 reflcol = texCUBE(_Cube, IN.normal) * (IN.normal.y * IN.normal.y);
	fixed4 reflcol = texCUBE(_Cube, IN.normal);
	//fixed4 reflcol2 = texCUBE(_Cube, half3(-IN.normal.y, IN.normal.x, IN.normal.z)) * (IN.normal.x * IN.normal.x);
	//o.Emission = reflcol.rgb * _ReflectColor.rgb * tex.a;
	//o.Emission = (reflcol.rgb + reflcol2.rgb) * tex.a;
	//o.Emission = reflcol2.rgb * tex.a;
	//o.Emission = reflcol.rgb * tex.a + ShadeSH9(half4(IN.normal, 1.0)) * tex.rgb * 0.5;
	o.Emission = reflcol.rgb * tex.a + IN.color * tex.rgb;
}
ENDCG
}

FallBack "Mobile/VertexLit"
}
