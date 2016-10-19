// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Mobile/Car Paint/Specular Alpha (1 Directional Light)" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_FresnelBase ("Fresnel Base", Range(0, 1)) = 0.0
	_FresnelPower ("Fresnel Power", Range(0.03, 10)) = 4.0
	_FresnelFading ("Fresnel Fading", Range(0, 2)) = 1.0
	_LightingBase ("Lighting Base", Range(0, 0.5)) = 0.0
	_LightingMul ("Lighting Mul", Range(0.5, 1.0)) = 1.0
	_ReflectionPower ("Reflection Power", Range(0.1, 10.0)) = 1.0
	_Alpha ("Alpha", Range(0, 1)) = 0.5
}

CGINCLUDE

#include "UnityCG.cginc"
#include "AutoLight.cginc"

half4 _LightColor0;

sampler2D _MainTex;
samplerCUBE _Cube;

half _Shininess;
half _FresnelBase, _FresnelPower, _FresnelFading;
half _LightingBase, _LightingMul;
half _ReflectionPower;
half _Alpha;

struct v2f_specular_fres
{
    half4  pos       : SV_POSITION;
    half2  uv        : TEXCOORD0;
    half3  lightDirT : TEXCOORD1;
	half3  normal    : TEXCOORD2;
	half3  viewDirT  : TEXCOORD3;
	half3  halfVecT  : TEXCOORD4;
	half3  reflVecT  : TEXCOORD5;
    LIGHTING_COORDS(6,7)
};

v2f_specular_fres vert(appdata_base v)
{
    v2f_specular_fres o;

	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.pos.z -= o.pos.w * 0.0001;

    o.uv = v.texcoord.xy;

	o.lightDirT = ObjSpaceLightDir(v.vertex);
	o.viewDirT  = normalize(ObjSpaceViewDir(v.vertex));
	o.halfVecT  = normalize(o.lightDirT + o.viewDirT);
	o.reflVecT  = normalize(mul(unity_ObjectToWorld, half4(-reflect(o.viewDirT, v.normal), 0.0)).xyz);

	o.normal = v.normal;

    TRANSFER_VERTEX_TO_FRAGMENT(o);

    return o;
}

fixed4 frag(v2f_specular_fres i) : COLOR
{
	fixed NdotL = saturate(dot(i.normal, i.lightDirT) * _LightingMul + _LightingBase);
	fixed atten = LIGHT_ATTENUATION(i);

	//Fresnel
	half fres = _FresnelBase + pow(1.0f - saturate(dot(i.normal, i.viewDirT)), _FresnelPower) * (1.0 - _FresnelBase);

	//Specular
	half NdotH = saturate(dot(i.normal, i.halfVecT));
	half spec = pow(NdotH, _Shininess * 128.0);

	//Reflection
	fixed3 refl = texCUBE(_Cube, i.reflVecT).rgb * _ReflectionPower;

	fixed4 result;

	result.rgb = tex2D(_MainTex, i.uv).rgb * (UNITY_LIGHTMODEL_AMBIENT + ((lerp(NdotL, NdotL * _FresnelFading, fres) + spec) * atten * 2 * _LightColor0));
	result.rgb += (fres * refl);
	result.rgb *= _Alpha;

	result.a = _Alpha;

	return result;
}

ENDCG

SubShader {
	Tags { "RenderType"="Transparent" "Queue"="Overlay" }
	LOD 150

	Pass {
        Name "ContentBase"
        Tags {"LightMode" = "ForwardBase"}

		Blend One OneMinusSrcAlpha
		Cull Back
		ZWrite Off
		ZTest LEqual
		//Offset -1, -1

CGPROGRAM

#pragma exclude_renderers flash
#pragma fragmentoption ARB_precision_hint_fastest
#pragma glsl_no_auto_normalization
#pragma multi_compile_fwdbase nolightmap nodirlightmap
#pragma vertex vert
#pragma fragment frag

ENDCG

	}
}

Fallback "Mobile/VertexLit"
}
