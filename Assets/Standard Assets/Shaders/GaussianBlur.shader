Shader "Hidden/SBS/GaussianBlur" {
    Properties {
        _MainTex ("", 2D) = "" {}
    }

CGINCLUDE

#include "UnityCG.cginc"

struct v2f_3 {
    half4 pos   : POSITION;
    half4 uv[2] : TEXCOORD0;
};

struct v2f_5 {
    half4 pos   : POSITION;
    half4 uv[3] : TEXCOORD0;
};

struct v2f_7 {
    half4 pos   : POSITION;
    half4 uv[4] : TEXCOORD0;
};

#define return_vert_3 \
    v2f_3 o; \
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex); \
    o.uv[0].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[0]; \
    o.uv[0].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[1]; \
    o.uv[1].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[2]; \
    return o;

#define return_vert_5 \
    v2f_5 o; \
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex); \
    o.uv[0].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[0]; \
    o.uv[0].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[1]; \
    o.uv[1].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[2]; \
    o.uv[1].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[3]; \
    o.uv[2].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[4]; \
    return o;

#define return_vert_7 \
    v2f_7 o; \
    o.pos = mul(UNITY_MATRIX_MVP, v.vertex); \
    o.uv[0].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[0]; \
    o.uv[0].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[1]; \
    o.uv[1].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[2]; \
    o.uv[1].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[3]; \
    o.uv[2].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[4]; \
    o.uv[2].zw = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[5]; \
    o.uv[3].xy = v.texcoord.xy + _MainTex_TexelSize.xy * blurOffsets[6]; \
    return o;

sampler2D _MainTex;

uniform float4 _MainTex_TexelSize;

v2f_3 vert_3_Horiz(appdata_img v) {
    const half2 blurOffsets[3] = {
        half2(-1.5, +0.5),
        half2(-0.5, +0.5),
        half2(+0.5, +0.5)
    };

    return_vert_3;
}

v2f_3 vert_3_Vert(appdata_img v) {
    const half2 blurOffsets[3] = {
        half2(+0.5, -0.5),
        half2(+0.5, +0.5),
        half2(+0.5, +1.5)
    };

    return_vert_3;
}

v2f_5 vert_5_Horiz(appdata_img v) {
    const half2 blurOffsets[5] = {
        half2(-2.5, +0.5),
        half2(-1.5, +0.5),
        half2(-0.5, +0.5),
        half2(+0.5, +0.5),
        half2(+1.5, +0.5)
    };

    return_vert_5;
}

v2f_5 vert_5_Vert(appdata_img v) {
    const half2 blurOffsets[5] = {
        half2(+0.5, -1.5),
        half2(+0.5, -0.5),
        half2(+0.5, +0.5),
        half2(+0.5, +1.5),
        half2(+0.5, +2.5)
    };

    return_vert_5;
}

v2f_7 vert_7_Horiz(appdata_img v) {
    const half2 blurOffsets[7] = {
        half2(-3.5, +0.5),
        half2(-2.5, +0.5),
        half2(-1.5, +0.5),
        half2(-0.5, +0.5),
        half2(+0.5, +0.5),
        half2(+1.5, +0.5),
        half2(+2.5, +0.5)
    };

    return_vert_7;
}

v2f_7 vert_7_Vert(appdata_img v) {
    const half2 blurOffsets[7] = {
        half2(+0.5, -2.5),
        half2(+0.5, -1.5),
        half2(+0.5, -0.5),
        half2(+0.5, +0.5),
        half2(+0.5, +1.5),
        half2(+0.5, +2.5),
        half2(+0.5, +3.5)
    };

    return_vert_7;
}
/*
#if SHADER_API_D3D9 || SHADER_API_D3D11 || SHADER_API_XBOX360
     if (_MainTex_TexelSize.y < 0)
         i.uv.xy = i.uv.xy * half2(1,-1)+half2(0,1);
#endif
*/
half4 frag_3(v2f_3 i): COLOR {
    const half g_3[3] = {
        0.0833333325572312,
        0.833333334885538,
        0.0833333325572312
    };

    return tex2D(_MainTex, i.uv[0].xy) * g_3[0] +
           tex2D(_MainTex, i.uv[0].zw) * g_3[1] +
           tex2D(_MainTex, i.uv[1].xy) * g_3[2];
}

half4 frag_5(v2f_5 i): COLOR {
    const half g_5[5] = {
        0.0430166238378208,
        0.241900254569334,
        0.430166243185691,
        0.241900254569334,
        0.0430166238378208
    };

    return tex2D(_MainTex, i.uv[0].xy) * g_5[0] +
           tex2D(_MainTex, i.uv[0].zw) * g_5[1] +
           tex2D(_MainTex, i.uv[1].xy) * g_5[2] +
           tex2D(_MainTex, i.uv[1].zw) * g_5[3] +
           tex2D(_MainTex, i.uv[2].xy) * g_5[4];
}

half4 frag_7(v2f_7 i): COLOR {
    const half g_7[7] = {
        0.028840966944021,
        0.103649061724298,
        0.223305134999961,
        0.288409672663439,
        0.223305134999961,
        0.103649061724298,
        0.028840966944021
    };

    return tex2D(_MainTex, i.uv[0].xy) * g_7[0] +
           tex2D(_MainTex, i.uv[0].zw) * g_7[1] +
           tex2D(_MainTex, i.uv[1].xy) * g_7[2] +
           tex2D(_MainTex, i.uv[1].zw) * g_7[3] +
           tex2D(_MainTex, i.uv[2].xy) * g_7[4] +
           tex2D(_MainTex, i.uv[2].zw) * g_7[5] +
           tex2D(_MainTex, i.uv[3].xy) * g_7[6];
}

ENDCG

	Subshader {
		// Horizontal Blur 3
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_3_Horiz
			#pragma fragment frag_3
			ENDCG
		}

		// Horizontal Blur 5
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_5_Horiz
			#pragma fragment frag_5
			ENDCG
		}

		// Horizontal Blur 7
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_7_Horiz
			#pragma fragment frag_7
			ENDCG
		}

		// Vertical Blur 3
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_3_Vert
			#pragma fragment frag_3
			ENDCG
		}

		// Vertical Blur 5
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_5_Vert
			#pragma fragment frag_5
			ENDCG
		}

		// Vertical Blur 7
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma exclude_renderers flash
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_7_Vert
			#pragma fragment frag_7
			ENDCG
		}
	}

	Fallback Off
}
