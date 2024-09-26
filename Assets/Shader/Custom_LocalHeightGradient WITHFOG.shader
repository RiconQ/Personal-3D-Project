Shader "Custom/LocalHeightGradient WITHFOG" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Scale ("Scale", Float) = 1
		_Offset ("Offset", Float) = 0
		_ColorA ("Color A", Vector) = (1,1,1,1)
		_ColorB ("Color B", Vector) = (0,0,0,1)
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