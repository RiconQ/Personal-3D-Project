using UnityEngine;

[CreateAssetMenu(menuName = "Enviroment/Enviroment Colors")]
public class EnviromentColors : ScriptableObject
{
	[ColorUsage(false, true)]
	public Color SkyColor;

	[ColorUsage(false, true)]
	public Color EquatorColor;

	[ColorUsage(false, true)]
	public Color GroundColor;
}
