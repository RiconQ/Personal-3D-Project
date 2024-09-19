using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Enviroment Preset", menuName = "New EnviromentPreset", order = 1)]
public class EnviromentPreset : ScriptableObject
{
	public GameObject postProcess;

	public Cubemap Reflections;

	public GradientCubemap Skybox;

	public GradientCubemap Enviroment;

	public GradientCubemap Fog;

	public AmbientColors Ambient;

	public LightingSettings lighting;

	public void Apply()
	{
		RenderSettings.ambientMode = AmbientMode.Trilight;
		if ((bool)Reflections)
		{
			Shader.SetGlobalTexture("_Armor", Reflections);
		}
		if ((bool)Skybox)
		{
			Shader.SetGlobalTexture("_Skybox", Skybox.Cubemap);
		}
		if ((bool)Fog)
		{
			Shader.SetGlobalTexture("_Fog", Fog.Cubemap);
		}
		else
		{
			Shader.SetGlobalTexture("_Fog", Skybox.Cubemap);
		}
		if ((bool)Enviroment)
		{
			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
			RenderSettings.customReflection = Enviroment.Cubemap;
		}
		if ((bool)Ambient)
		{
			Ambient.Apply();
		}
	}

	public void Fill()
	{
	}
}
