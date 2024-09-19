using UnityEngine;

[ExecuteInEditMode]
public class CustomTrails : MonoBehaviour
{
	private static Material m_Material;

	public Shader shader;

	private CustomRenderTexture _pastFrame;

	[SerializeField]
	[Range(0f, 0.75f)]
	private float _maxTransparency = 0.75f;

	protected Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = new Material(Shader.Find("Hidden/ClearFlagsImageEffect"));
				m_Material.hideFlags = HideFlags.DontSave;
			}
			return m_Material;
		}
	}

	protected void OnDisable()
	{
		if ((bool)m_Material)
		{
			Object.DestroyImmediate(m_Material);
		}
	}

	private void Start()
	{
		_maxTransparency = 0.75f;
		_pastFrame = new CustomRenderTexture(Screen.width / 4, Screen.height / 4);
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		material.SetTexture("_PrevFrame", _pastFrame);
		material.SetFloat("_MaxTransparency", Mathf.Clamp(_maxTransparency, 0f, 1f));
		Graphics.Blit(src, dst, material);
		Graphics.Blit(RenderTexture.active, _pastFrame);
	}
}
