using UnityEngine;

public class LevelProgress : MonoBehaviour
{
	private MaterialPropertyBlock block;

	private MeshRenderer rend;

	public Transform t { get; private set; }

	private void Awake()
	{
		t = base.transform;
		rend = GetComponent<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
	}

	public void Set(float alpha)
	{
		block.SetFloat("_AlphaPower", alpha);
		rend.SetPropertyBlock(block);
	}
}
