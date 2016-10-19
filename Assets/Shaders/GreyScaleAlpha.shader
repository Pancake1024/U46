Shader "Unlit/GreyScale Transparent" {
    Properties {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
    SubShader {
	    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    LOD 100
 
 	    ZWrite Off
	    Blend SrcAlpha OneMinusSrcAlpha 
	    
        Pass {
	
	    CGPROGRAM
	    #pragma vertex vert
	    #pragma fragment frag

	    #include "UnityCG.cginc"

	    sampler2D _MainTex;
	    fixed4 _Color;

	    struct v2f {
	        float4  pos : SV_POSITION;
	        float2  uv : TEXCOORD0;
	    };

	    float4 _MainTex_ST;

	    v2f vert (appdata_base v)
	    {
	        v2f o;
	        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	        o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	        return o;
	    }

	    half4 frag (v2f i) : COLOR
	    {
	        half4 texcol = tex2D (_MainTex, i.uv);
	        texcol.rgb = dot(texcol.rgb, _Color.rgb);
	        return texcol;
	    }
	    ENDCG
        }
    }
    Fallback "VertexLit"
 } 