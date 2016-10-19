// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Reflective/Bumped Specular RC" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
	_MainTex ("Base (RGB) RefStrGloss (A)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 400
CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert exclude_path:prepass
#pragma target 3.0

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
samplerCUBE _Cube;

fixed4 _Color;
fixed4 _ReflectColor;
half _Shininess;

float4x4 _CubeMatrix;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float3 xyz_Cube;
	/*float3 localNormal;
	float3 localView;*/
	//INTERNAL_DATA
};

void vert(inout appdata_full v, out Input o)
{
	o.uv_MainTex = v.texcoord.xy;
	o.uv_BumpMap = v.texcoord.xy;
	o.xyz_Cube = mul(
		float3x3(unity_ObjectToWorld),
		mul(
			float3x3(_CubeMatrix),
			-reflect(normalize(ObjSpaceViewDir(v.vertex)), v.normal)
		)
	);
	/*o.xyz_Cube = 
		mul(
			float3x3(_Object2World),
			-reflect(normalize(ObjSpaceViewDir(v.vertex)), mul(float3x3(_CubeMatrix), v.normal))
		);*/
	/*o.localNormal = v.normal;
	o.localView = normalize(ObjSpaceViewDir(v.vertex));*/
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex * _Color;
	o.Albedo = c.rgb;
	
	o.Gloss = tex.a;
	o.Specular = _Shininess;
	
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	
	//float3 worldRefl = WorldReflectionVector (IN, o.Normal);
	//float3 refl = mul(float3x3(_Object2World), mul(float3x3(_CubeMatrix), -reflect(IN.localView, IN.localNormal)));

	fixed4 reflcol = texCUBE (_Cube, IN.xyz_Cube);//float3(-IN.xyz_Cube.y, IN.xyz_Cube.x, IN.xyz_Cube.z));//refl);//worldRefl);
	reflcol *= tex.a;
	o.Emission = reflcol.rgb * _ReflectColor.rgb;
	o.Alpha = reflcol.a * _ReflectColor.a;
}
ENDCG
}

FallBack "Reflective/Bumped Specular"
}
