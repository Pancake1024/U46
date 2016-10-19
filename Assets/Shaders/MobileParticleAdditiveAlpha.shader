// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask
            
Shader "Mobile/Particles/SBS_AdditiveAlpha" {
	Properties {
		_Color ("Main Color", Color) = (1, 1, 1, 0)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}
            
	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
                
		BindChannels {
				Bind "Vertex", vertex
				Bind "TexCoord", texcoord
		}
                
		SubShader {
				Pass {
						SetTexture [_MainTex] {
								constantColor[_Color]
								combine texture * constant
						}
				}
		}
	}
}