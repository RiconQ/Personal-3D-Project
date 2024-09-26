Shader "PSX Style/Unlit Rim" {
	Properties {
		_RimColor ("Rim Color", Vector) = (1,0,0,1)
		_RimPower ("Rim Power", Range(0, 10)) = 4
		_RimExtra ("Rim Brithness", Range(1, 10)) = 4
		_VertexSnapStep ("Vertex Step", Range(1, 128)) = 32
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}