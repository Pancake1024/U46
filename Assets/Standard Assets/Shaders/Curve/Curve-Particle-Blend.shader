Shader "Curve/Particles/Blend" {

Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
}

CGINCLUDE

#include "Curve_Include.cginc"

v2f_particle vert1(appdata_particle v)
{
    v2f_particle o;

	o.pos = Curve_OutputPosition(v.vertex);
    o.uv = v.texcoord.xy;
	o.color = v.color;

	VERTEX_FOG(v, o);

    return o;
}

fixed4 frag1(v2f_particle i) : COLOR
{
	return PARTICLE_FRAG_FOG(i, i.color * tex2D(_MainTex, i.uv));
}

ENDCG

SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Pass {

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Color (1,1,1,1) }

CGPROGRAM

#pragma exclude_renderers flash
#pragma fragmentoption ARB_precision_hint_fastest
#pragma glsl_no_auto_normalization
#pragma multi_compile_fwdbase
#pragma vertex vert1
#pragma fragment frag1

ENDCG

	}
}

}
