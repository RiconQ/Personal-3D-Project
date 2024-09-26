Shader "cptnsigh/Triplanar/Wood" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Pattern ("Pattern", 2D) = "white" {}
		_PatternColor ("Pattern Color", Vector) = (1,1,1,1)
		_PatternScale ("Pattern Scale", Float) = 2
		_Mask ("Mask", 2D) = "white" {}
		_MaskScale ("_SmoothnessTexScale", Float) = 4
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
	Fallback "Diffuse"
}