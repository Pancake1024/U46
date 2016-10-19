Shader "User/Sky" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord", texcoord
			Bind "Color", color
		}

		Pass {
			Lighting Off
			AlphaTest Off

			Fog
			{
				Mode Off
			}

			SetTexture [_MainTex]
			{
				combine texture
			}
		}
	}

	FallBack Off
}
