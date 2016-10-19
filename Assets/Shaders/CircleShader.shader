Shader "User/Circle" {
	Properties {
		_Image ("Image", 2D) = "white" {}
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
			Lighting Off
			Cull Off
			ZWrite Off
			ZTest LEqual
			AlphaTest Off
			Blend SrcAlpha OneMinusSrcAlpha

			Fog {
				Mode Off
			}

			SetTexture [_Image] {
				constantColor [_Color]
				combine texture * constant
			}
		}
	}

	FallBack Off
}