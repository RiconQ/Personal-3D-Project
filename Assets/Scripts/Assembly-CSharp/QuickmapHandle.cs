using UnityEngine;

public class QuickmapHandle : MonoBehaviour
{
	public Color color = Color.green;

	private Color colorSelected = new Color(0.8f, 0.7f, 0.6f, 0.75f);

	private MeshRenderer[] rends;

	private MaterialPropertyBlock block;

	[Button]
	public void Setup()
	{
		rends = GetComponentsInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rends[0].GetPropertyBlock(block);
		block.SetColor("_Color", color);
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	public void Select(bool value)
	{
		block.SetColor("_Color", value ? colorSelected : color);
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].SetPropertyBlock(block);
		}
	}

	private void Awake()
	{
		Setup();
	}
}
