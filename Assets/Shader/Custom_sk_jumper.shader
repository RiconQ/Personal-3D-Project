Shader "Custom/sk_jumper" {
	Properties {
		_Color ("Tint Color", Vector) = (1,1,1,1)
		_Main ("Texture", 2D) = "white" {}
		_Normal ("Distortion", 2D) = "white" {}
		_Reaction ("Reaction", Range(0, 1)) = 0
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