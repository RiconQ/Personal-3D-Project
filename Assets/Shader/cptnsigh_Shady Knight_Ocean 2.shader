Shader "cptnsigh/Shady Knight/Ocean 2" {
	Properties {
		_Color ("Texture Color", Vector) = (1,1,1,1)
		_Main ("Texture", 2D) = "white" {}
		_Normal ("Distortion", 2D) = "white" {}
		_ScaleA ("Normal Scale A", Float) = 0.03
		_ScaleB ("Normal Scale B", Float) = 0.005
		_PatternScale ("Pattern Scale", Float) = 0.025
		_PatternDstrStrengh ("Pattern Distortion Strengh", Float) = 0.04
		_Depth ("Depth", Float) = 18
		_DepthPower ("Depth Power", Float) = 4
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}