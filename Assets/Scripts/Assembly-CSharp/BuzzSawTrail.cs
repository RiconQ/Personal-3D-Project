using UnityEngine;

public class BuzzSawTrail : MonoBehaviour
{
	public float xOffsetSpeed = 8f;

	public float yOffsetSpeed;

	public Vector4 tillingOffset = new Vector4(1f, 1f, 0f, 0f);

	private MeshRenderer rend;

	private MaterialPropertyBlock block;

	private void Awake()
	{
		block = new MaterialPropertyBlock();
		rend = GetComponent<MeshRenderer>();
		rend.GetPropertyBlock(block);
	}

	private void Update()
	{
		block.SetVector("_MainTex_ST", tillingOffset);
		if (xOffsetSpeed != 0f)
		{
			tillingOffset.z = ((tillingOffset.z != 1f) ? Mathf.MoveTowards(tillingOffset.z, 1f, Time.deltaTime * xOffsetSpeed) : 0f);
		}
		if (yOffsetSpeed != 0f)
		{
			tillingOffset.w = ((tillingOffset.w != 1f) ? Mathf.MoveTowards(tillingOffset.w, 1f, Time.deltaTime * yOffsetSpeed) : 0f);
		}
		rend.SetPropertyBlock(block);
	}
}
