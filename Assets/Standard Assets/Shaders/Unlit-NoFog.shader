// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
// - no fog

Shader "Unlit/Texture No Fog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {
		Lighting Off
		Fog { Mode Off }
		SetTexture [_MainTex] { combine texture } 
	}
}
}
