using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CustomSkybox : MonoBehaviour
{
	public GradientCubemap Skybox;

	public GradientCubemap Enviroment;

	public AmbientColors Ambient;

	public void Start()
	{
		Apply();
	}

	public void OnValidate()
	{
		Apply();
	}

	public void Apply()
	{
		if ((bool)Skybox)
		{
			Shader.SetGlobalTexture("_Skybox", Skybox.Cubemap);
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
}
