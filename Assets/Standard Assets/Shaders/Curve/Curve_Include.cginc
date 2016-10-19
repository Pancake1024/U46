// Upgrade NOTE: commented out 'half4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Curve shaders
#include "UnityCG.cginc"
#include "AutoLight.cginc"

#ifndef CURVE_INCLUDED
#	define CURVE_INCLUDED

#	if defined(SHADER_API_D3D11_9X)
#		define SHADER_API_WP8
#	endif

//Curve parameters
half4 _CurveOffset;
half _CurveDist;

//Diffuse texture
sampler2D _MainTex;

//Specular texture
sampler2D _SpecularTex;

//Specular power
half _SpecularPower;

//Lighting Ramp
sampler2D _Ramp;

//Highlight/Shadow Colors
fixed4 _Color;
fixed4 _SColor;

//Rim light
half _RimPower;
fixed4 _RimColor;

half4 _LightColor0;

#	ifdef SHADER_API_WP8
half4 unity_FogColor;
half4 unity_FogStart;
half4 unity_FogEnd;
#		define VERTEX_FOG(v, o) \
    half4 hpos = mul(UNITY_MATRIX_MV, v.vertex); \
    o.fog = clamp((unity_FogEnd.x - length(hpos)) / (unity_FogEnd.x - unity_FogStart.x), 0.0, 1.0);
#		define FRAG_FOG(i, c) \
	lerp(unity_FogColor, c, i.fog);
#		define PARTICLE_FRAG_FOG(i, c) \
	fixed4((c).rgb, (c).a * i.fog);
#	else
#		define VERTEX_FOG(v, o)
#		define FRAG_FOG(i, c) c
#		define PARTICLE_FRAG_FOG(i, c) c
#	endif

struct appdata_particle {
    float4 vertex   : POSITION;
    float4 texcoord : TEXCOORD0;
	fixed4 color    : COLOR;
};

#	ifdef LIGHTMAP_ON
// sampler2D unity_Lightmap;
// half4 unity_LightmapST;

struct appdata_base_LM {
    float4 vertex    : POSITION;
    float3 normal    : NORMAL;
    float4 texcoord  : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};
#	endif

struct v2f_unlit
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef LIGHTMAP_ON
    half2  uvLM      : TEXCOORD1;
#	endif
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD2;
#	endif
};

struct v2f_particle
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD1;
#	endif
	fixed4 color     : COLOR;
};

struct v2f_diffuse
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef LIGHTMAP_ON
    half2  uvLM      : TEXCOORD1;
    half3  lightDirT : TEXCOORD2;
	half3  normal    : TEXCOORD3;
    LIGHTING_COORDS(4,5)
#	else
    half3  lightDirT : TEXCOORD1;
	half3  normal    : TEXCOORD2;
    LIGHTING_COORDS(3,4)
#	endif
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD5;
#	endif
};

struct v2f_specular
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef LIGHTMAP_ON
    half2  uvLM      : TEXCOORD1;
    half3  lightDirT : TEXCOORD2;
    half3  halfVecT  : TEXCOORD3;
	half3  normal    : TEXCOORD3;
    LIGHTING_COORDS(4,5)
#	else
    half3  lightDirT : TEXCOORD1;
    half3  halfVecT  : TEXCOORD2;
	half3  normal    : TEXCOORD2;
    LIGHTING_COORDS(3,4)
#	endif
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD5;
#	endif
};

struct v2f_diffuse_rim
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef LIGHTMAP_ON
    half2  uvLM      : TEXCOORD1;
    half3  lightDirT : TEXCOORD2;
	half3  normal    : TEXCOORD3;
	half3  viewDirT  : TEXCOORD4;
    LIGHTING_COORDS(5,6)
#	else
    half3  lightDirT : TEXCOORD1;
	half3  normal    : TEXCOORD2;
	half3  viewDirT  : TEXCOORD3;
    LIGHTING_COORDS(4,5)
#	endif
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD6;
#	endif
};

struct v2f_specular_rim
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
#	ifdef LIGHTMAP_ON
    half2  uvLM      : TEXCOORD1;
    half3  lightDirT : TEXCOORD2;
	half3  normal    : TEXCOORD3;
	half3  viewDirT  : TEXCOORD4;
	half3  halfVecT  : TEXCOORD5;
    LIGHTING_COORDS(6,7)
#	else
    half3  lightDirT : TEXCOORD1;
	half3  normal    : TEXCOORD2;
	half3  viewDirT  : TEXCOORD3;
	half3  halfVecT  : TEXCOORD4;
    LIGHTING_COORDS(5,6)
#	endif
#	ifdef SHADER_API_WP8
	half   fog       : TEXCOORD7;
#	endif
};

inline half4 Curve_OutputPosition(float4 vertex)
{
	half4 p = mul(UNITY_MATRIX_MV, vertex);
	half z = p.z / _CurveDist;
	p += _CurveOffset * z * z;
	return mul(UNITY_MATRIX_P, p);
}

inline fixed4 Curve_OutputColorUnlit(v2f_unlit i)
{
	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * lm;
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb;
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 Curve_OutputColorDiffuse(v2f_diffuse i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT));
	fixed atten = LIGHT_ATTENUATION(i);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (NdotL * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (NdotL * atten * 2 * _LightColor0));
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 Curve_OutputColorColorDiffuse(v2f_diffuse i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT));
	fixed atten = LIGHT_ATTENUATION(i);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (NdotL * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (NdotL * atten * 2 * _LightColor0)) * _Color;
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 Curve_OutputColorSpecular(v2f_specular i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT));
	fixed atten = LIGHT_ATTENUATION(i);

	half3 specularity = pow(saturate(dot(i.normal, i.halfVecT)), _SpecularPower) * _LightColor0;

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (NdotL * atten * 2 * _LightColor0)) + specularity;
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (NdotL * atten * 2 * _LightColor0)) + specularity;
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 Curve_OutputColorSpecularWithTex(v2f_specular i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT));
	fixed atten = LIGHT_ATTENUATION(i);

	half3 specularity = pow(saturate(dot(i.normal, i.halfVecT)), tex2D(_SpecularTex, i.uv).g * 200) * tex2D(_SpecularTex, i.uv).r * _LightColor0;

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (NdotL * atten * 2 * _LightColor0)) + specularity;
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (NdotL * atten * 2 * _LightColor0)) + specularity;
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 CurveToon_OutputColorDiffuse(v2f_diffuse i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT)) * .5 + .5;
	fixed atten = LIGHT_ATTENUATION(i);

	//Ramp shading
	fixed3 ramp = tex2D(_Ramp, fixed2(NdotL, NdotL));
	
	//Gooch shading
	ramp = lerp(_SColor, _Color, ramp);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (ramp * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (ramp * atten * 2 * _LightColor0));
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 CurveToon_OutputColorSpecular(v2f_specular i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT)) * .5 + .5;
	fixed atten = LIGHT_ATTENUATION(i);

	//Ramp shading
	fixed3 ramp = tex2D(_Ramp, fixed2(NdotL, NdotL));
	
	//Gooch shading
	ramp = lerp(_SColor, _Color, ramp);

	//Specular
	half NdotH = saturate(dot(i.normal, i.halfVecT));
	half spec = pow(NdotH, _SpecularPower);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	fixed specOccl = saturate(dot(lm, fixed3(1, 1, 1)));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + ((ramp + fixed3(spec, spec, spec) * specOccl) * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + ((ramp + fixed3(spec, spec, spec)) * atten * 2 * _LightColor0));
#	endif
	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 CurveToonRim_OutputColorDiffuse(v2f_diffuse_rim i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT)) * .5 + .5;
	fixed atten = LIGHT_ATTENUATION(i);

	//Ramp shading
	fixed3 ramp = tex2D(_Ramp, fixed2(NdotL, NdotL));
	
	//Gooch shading
	ramp = lerp(_SColor, _Color, ramp);

	//Rim light
	half rim = pow(1.0f - saturate(dot(i.normal, i.viewDirT)), _RimPower);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + (ramp * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + (ramp * atten * 2 * _LightColor0));
#	endif

	result.rgb += (rim * _RimColor.rgb);

	result.a = 1;

	return FRAG_FOG(i, result);
}

inline fixed4 CurveToonRim_OutputColorSpecular(v2f_specular_rim i)
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT)) * .5 + .5;
	fixed atten = LIGHT_ATTENUATION(i);

	//Ramp shading
	fixed3 ramp = tex2D(_Ramp, fixed2(NdotL, NdotL));
	
	//Gooch shading
	ramp = lerp(_SColor, _Color, ramp);

	//Rim light
	half rim = pow(1.0f - saturate(dot(i.normal, i.viewDirT)), _RimPower);

	//Specular
	half NdotH = saturate(dot(i.normal, i.halfVecT));
	half spec = pow(NdotH, _SpecularPower);

	fixed4 result;

#	ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw));
	fixed specOccl = saturate(dot(lm, fixed3(1, 1, 1)));
	result.rgb = tex2D(_MainTex, i.uv).rgb * (lm + ((ramp + fixed3(spec, spec, spec) * specOccl) * atten * 2 * _LightColor0));
#	else
	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + ((ramp + fixed3(spec, spec, spec)) * atten * 2 * _LightColor0));
#	endif

	result.rgb += (rim * _RimColor.rgb);

	result.a = 1;

	return FRAG_FOG(i, result);
}

#endif
