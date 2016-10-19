Shader "User/Gesture" {
	Properties {
		_Trail ("Trail Texture", 2D) = "white" {}
		_Head ("Head Texture", 2D) = "white" {}
		_Fill ("Fill Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader {
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord", texcoord
			Bind "Color", color
		}

		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest LEqual
			AlphaTest Off

			Fog {
				Color (0,0,0,0)
			}

			SetTexture [_Trail] {
				constantColor [_Color]
				combine texture lerp(texture) constant
			}

			SetTexture [_Head] {
				combine texture lerp(texture) previous
			}

			SetTexture [_Fill] {
				combine texture lerp(texture) previous
			}
		}
	}

	FallBack Off
}