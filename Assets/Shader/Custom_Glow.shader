Shader "Custom/Glow" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaPower ("Alpha", Float) = 1
		_Power ("Power", Float) = 1
		_Scale ("Scale", Float) = 1
		_Offset ("Offset", Float) = 0
		_RimColor ("Rim Color", Vector) = (0,0,0,1)
		_RimColorB ("Rim Color B", Vector) = (1,1,1,1)
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