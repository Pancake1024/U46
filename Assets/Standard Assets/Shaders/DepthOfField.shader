Shader "Hidden/SBS/DepthOfField" {
    Properties {
        _MainTex ("", 2D) = "" {}
		_Blurred ("", 2D) = "" {}
    }

CGINCLUDE

#include "UnityCG.cginc"

struct v2f {
    half4 pos: POSITION;
    half2 uv : TEXCOORD0;
};

sampler2D _MainTex;
sampler2D _CameraDepthTexture;
sampler2D _Blurred;

half4 _CurveParams;
half _Strength;

v2f vert(appdata_img v) {
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv.xy = v.texcoord.xy;
	return o;
}

half4 frag(v2f i) : COLOR {
	float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv.xy));
	d = Linear01Depth(d);

	half coc = saturate((_CurveParams.z - d) * _CurveParams.x) +
               saturate((d - _CurveParams.w) * _CurveParams.y);

	return lerp(tex2D(_MainTex, i.uv.xy), tex2D(_Blurred, i.uv.xy), saturate(coc * _Strength));
}

ENDCG

	Subshader {
		// Simple DoF
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}

	Fallback Off
}
