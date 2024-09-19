using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class LevelEnvoroment : MonoBehaviour
{
	public int vertexColorsOffset = 100;

	[Header("Default")]
	public GradientCubemap Skybox;

	public GradientCubemap Enviroment;

	public GradientCubemap Fog;

	public AmbientColors Ambient;

	public GameObject postProcess;

	private void Start()
	{
		if ((bool)Game.player)
		{
			HubData currentHub = LevelsData.instance.GetCurrentHub();
			if ((bool)currentHub && (bool)currentHub.envPreset)
			{
				currentHub.envPreset.Apply();
				if ((bool)currentHub.envPreset.postProcess)
				{
					Object.Instantiate(Resources.Load(currentHub.envPreset.postProcess.name));
				}
				return;
			}
		}
		if ((bool)postProcess)
		{
			Object.Instantiate(Resources.Load(postProcess.name));
		}
		if ((bool)Skybox)
		{
			Shader.SetGlobalTexture("_Skybox", Skybox.Cubemap);
		}
		if ((bool)Fog)
		{
			Shader.SetGlobalTexture("_Fog", Fog.Cubemap);
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
