using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(LightTrailsRenderer), PostProcessEvent.AfterStack, "cptnsigh/LightTrails", true)]
public sealed class LightTrails : PostProcessEffectSettings
{
	[Range(0f, 1f)]
	[Tooltip("Light Trails effect intensity")]
	public FloatParameter blend = new FloatParameter
	{
		value = 0.5f
	};

	public TextureParameter tex = new TextureParameter
	{
		value = null,
		defaultState = TextureParameterDefault.Black
	};
}
