Shader "Hidden/SBS/MotionBlur" {
    Properties {
        _MainTex ("", 2D) = "" {}
		_VelocityBuffer ("", 2D) = "" {}
    }

CGINCLUDE

#include "UnityCG.cginc"

struct vb_input {
    float4 vertex: POSITION;
    float3 texcoord: TEXCOORD0;
};

struct vb_v2f {
    float4 pos: POSITION;
    float3 uv : TEXCOORD0;
	float4 cp : TEXCOORD1;
	float2 sp : TEXCOORD2;
};

struct blur_v2f {
    half4 pos: POSITION;
    half2 uv : TEXCOORD0;
};

sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;

sampler2D _VelocityBuffer;
sampler2D _CameraDepthTexture;

float4x4 _PrevViewProj;
float4x4 _ObjPrevMVP;
float _VelocityMult;
float _BlurStrength;

vb_v2f vb_vert(vb_input v) {
	vb_v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv.xyz = v.texcoord.xyz;
	o.cp = o.pos;
	o.sp = (o.cp.xy / o.cp.w) * half2(0.5, 0.5) + half2(0.5, 0.5);
	return o;
}

float4 vb_frag(vb_v2f i) : COLOR {
	float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, float2(i.sp.x, 1.0 - i.sp.y)));
	d = Linear01Depth(d);

	float4 viewPos = float4(i.uv.x * d, -i.uv.y * d, i.uv.z * d, 1.0);
	float4 prevClipPos = mul(_PrevViewProj, viewPos);

	float2 c0 = prevClipPos.xy / prevClipPos.w,
	       c1 = i.cp.xy / i.cp.w;
	float2 dx = c1 - c0;

	float2 v = dx * _VelocityMult;

	v = v * 0.5 + half2(0.5, 0.5);
	v = saturate(v);

	return float4(v.x, v.y, 0.0, 0.0);
}

blur_v2f blur_vert(appdata_img v) {
	blur_v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv.xy = v.texcoord.xy;
	return o;
}

float4 blur_frag_8(blur_v2f i) : COLOR {
	float2 v = tex2D(_VelocityBuffer, i.uv) * 2.0f - float2(1.0, 1.0);
	float2 hp = _MainTex_TexelSize * half2(0.5, -0.5);
	v = float2(v.x, -v.y) * _MainTex_TexelSize.xy * _BlurStrength;
	return (tex2D(_MainTex, i.uv + hp) +
			tex2D(_MainTex, i.uv + hp + v) +
			tex2D(_MainTex, i.uv + hp + v * 2.0) +
			tex2D(_MainTex, i.uv + hp + v * 3.0) +
			tex2D(_MainTex, i.uv + hp + v * 4.0) +
			tex2D(_MainTex, i.uv + hp + v * 5.0) +
			tex2D(_MainTex, i.uv + hp + v * 6.0) +
			tex2D(_MainTex, i.uv + hp + v * 7.0)) * 0.125;
}

float4 blur_frag_6(blur_v2f i) : COLOR {
	float2 v = tex2D(_VelocityBuffer, i.uv) * 2.0f - float2(1.0, 1.0);
	float2 hp = _MainTex_TexelSize * half2(0.5, -0.5);
	v = float2(v.x, -v.y) * _MainTex_TexelSize.xy * _BlurStrength;
	return (tex2D(_MainTex, i.uv + hp) +
			tex2D(_MainTex, i.uv + hp + v) +
			tex2D(_MainTex, i.uv + hp + v * 2.0) +
			tex2D(_MainTex, i.uv + hp + v * 3.0) +
			tex2D(_MainTex, i.uv + hp + v * 4.0) +
			tex2D(_MainTex, i.uv + hp + v * 5.0)) * 0.16667;
}

float4 blur_frag_4(blur_v2f i) : COLOR {
	float2 v = tex2D(_VelocityBuffer, i.uv) * 2.0f - float2(1.0, 1.0);
	float2 hp = _MainTex_TexelSize * half2(0.5, -0.5);
	v = float2(v.x, -v.y) * _MainTex_TexelSize.xy * _BlurStrength;
	return (tex2D(_MainTex, i.uv + hp) +
			tex2D(_MainTex, i.uv + hp + v) +
			tex2D(_MainTex, i.uv + hp + v * 2.0) +
			tex2D(_MainTex, i.uv + hp + v * 3.0)) * 0.25;
}

ENDCG

	Subshader {
		// Velocity Buffer
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vb_vert
			#pragma fragment vb_frag
			ENDCG
		}

		// Motion Blur - 4
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex blur_vert
			#pragma fragment blur_frag_4
			ENDCG
		}

		// Motion Blur - 6
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex blur_vert
			#pragma fragment blur_frag_6
			ENDCG
		}

		// Motion Blur - 8
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex blur_vert
			#pragma fragment blur_frag_8
			ENDCG
		}
	}

	Fallback Off
}
