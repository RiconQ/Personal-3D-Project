using UnityEngine;

[RequireComponent(typeof(Projector))]
public class ProjectorMaterial : MonoBehaviour
{
	public Color color;

	public Texture2D texture;

	private Projector projector;

	private Material material;

	private void Start()
	{
		projector = GetComponent<Projector>();
		material = new Material(projector.material);
		if (color != Color.clear)
		{
			material.color = color;
		}
		if ((bool)texture)
		{
			material.mainTexture = texture;
		}
		projector.material = material;
	}
}
