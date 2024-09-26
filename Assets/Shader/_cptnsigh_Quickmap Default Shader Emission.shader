Shader "_cptnsigh/Quickmap Default Shader Emission" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_GlossinessTex ("Glossiness", 2D) = "white" {}
		_VignetteTex ("Vignette", 2D) = "white" {}
		_Emission ("Emission", 2D) = "white" {}
		_EmissionColor ("Emission Color", Vector) = (1,0,0,1)
		_Glossiness ("Smoothness", Range(0, 10)) = 3
		_Metallic ("Metallic", Range(0, 1)) = 0
		_EdgeBlendWidth ("Edge Blend Width", Range(0, 1)) = 0.2
		_SmoothnessTexScale ("_SmoothnessTexScale", Float) = 0.1
		_MainTexScale ("Main Tex Scale", Float) = 0.1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}