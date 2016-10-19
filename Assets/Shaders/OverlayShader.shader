Shader "User/Overlay" {
	Properties {
		_Image ("Image", 2D) = "white" {}
	}

	SubShader {
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord", texcoord
			Bind "Color", color
		}

		Tags {
			"Queue"="Overlay"
			"IgnoreProjector"="True"
			"RenderType"="Overlay"
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
				combine texture * primary
				Matrix [_Transform]
			}
		}
	}

	FallBack Off
}