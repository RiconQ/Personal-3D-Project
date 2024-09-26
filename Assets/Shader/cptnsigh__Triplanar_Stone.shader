Shader "cptnsigh/_Triplanar/Stone" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Main ("Main", 2D) = "white" {}
		_Mask ("Mask", 2D) = "" {}
		_MaskScale ("_MaskScale", Float) = 4
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