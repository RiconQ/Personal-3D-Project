using UnityEngine;

[CreateAssetMenu(menuName = "Enviroment/Gradient Cubemap")]
public class GradientCubemap : ScriptableObject
{
	[GradientUsage(true)]
	public Gradient Gradient = new Gradient();

	public Cubemap Cubemap;

	public float Power = 1f;

	private int _textureSize = 64;
}
