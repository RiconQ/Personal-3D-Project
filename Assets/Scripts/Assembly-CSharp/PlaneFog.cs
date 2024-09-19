using UnityEngine;

public class PlaneFog : MonoBehaviour
{
	public float dot;

	public MeshRenderer rend;

	public MaterialPropertyBlock block;

	private void Awake()
	{
		rend = GetComponent<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
	}

	private void Update()
	{
		dot = Vector3.Dot(base.transform.up, LastActiveCamera.tCam.position.DirTo(base.transform.position).normalized).Abs() * 2f - 0.25f;
		block.SetFloat("_Alpha", Mathf.Clamp01(dot));
		rend.SetPropertyBlock(block);
	}
}
