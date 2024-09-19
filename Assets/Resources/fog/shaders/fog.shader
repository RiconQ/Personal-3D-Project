Shader "Hidden/Kino/Fog" {
	Properties {
		_MainTex ("-", 2D) = "" {}
		_FogColor ("-", Vector) = (0,0,0,0)
		_SkyTint ("-", Vector) = (0.5,0.5,0.5,0.5)
		[Gamma] _SkyExposure ("-", Range(0, 8)) = 1
		[NoScaleOffset] _SkyCubemap ("-", Cube) = "" {}
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