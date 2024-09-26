Shader "Shady Knight/Rank Letter" {
	Properties {
		_MainTex ("Dirt", 2D) = "white" {}
		_Normal ("Normal", 2D) = "white" {}
		_Fill ("Fill", Range(0, 8)) = 0
		_Blink ("Blink", Range(0, 2)) = 0
		_Speed ("Speed", Float) = 4
		_Alpha ("Alpha", Range(-1, 2)) = 2
		_RimPower ("Rim Power", Float) = 6
		_ColorRimA ("Color Rim A", Vector) = (0.1,0,0.8,1)
		_ColorRimB ("Color Rim B", Vector) = (0,1,0.2,1)
		_ColorRimC ("Color Rim C", Vector) = (0.8,0,0.05,1)
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