using UnityEngine;

[ExecuteInEditMode]
[CreateAssetMenu(menuName = "Enviroment/Clounds Preset")]
public class CloudsPreset : ScriptableObject
{
	public Color Color = new Color(1f, 0.6f, 0.6f, 0.6f);

	public Texture2D Texture;

	public float ScrollSpeed;

	public Vector2 Size = new Vector2(1f, 1f);

	public Vector2 Offset = new Vector2(0f, 0f);

	public void OnValidate()
	{
		Apply();
	}

	public void Apply()
	{
		Shader.SetGlobalColor("CloudsColor", Color);
		Shader.SetGlobalTexture("CloudsTexture", Texture);
		Shader.SetGlobalFloat("CloundsSpeed", ScrollSpeed);
		Shader.SetGlobalVector("CloundsSize", Size);
		Shader.SetGlobalVector("CloundsOffset", Offset);
	}
}
