using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Enviroment/Ambient Colors")]
public class AmbientColors : ScriptableObject
{
	[ColorUsage(false, true)]
	public Color SkyColor;

	[ColorUsage(false, true)]
	public Color EquatorColor;

	[ColorUsage(false, true)]
	public Color GroundColor;

	public void SetFromScene()
	{
		SkyColor = RenderSettings.ambientSkyColor;
		EquatorColor = RenderSettings.ambientEquatorColor;
		GroundColor = RenderSettings.ambientGroundColor;
	}

	public void Apply()
	{
		RenderSettings.ambientMode = AmbientMode.Trilight;
		RenderSettings.ambientSkyColor = SkyColor;
		RenderSettings.ambientEquatorColor = EquatorColor;
		RenderSettings.ambientGroundColor = GroundColor;
	}
}
