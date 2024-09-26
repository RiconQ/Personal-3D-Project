Shader "cptnsigh/Particles/Multiply Distortion Framed" {
	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
		_Normal ("Normal", 2D) = "white" {}
		_Frame ("Frame", 2D) = "white" {}
		_Power ("Power", Float) = 0.05
		_XSpeed ("X Speed", Float) = 0.1
		_YSpeed ("Y Speed", Float) = 0.1
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