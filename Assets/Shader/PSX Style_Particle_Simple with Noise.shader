Shader "PSX Style/Particle/Simple with Noise" {
	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
		_NoiseTex ("Particle Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Vector) = (0.5,0.5,0.5,0.5)
		_VertexSnapStep ("Vertex Step", Range(1, 128)) = 32
		_Scale ("Scale", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}