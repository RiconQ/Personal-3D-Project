using UnityEngine;

public class ShotTrail : PooledMonobehaviour
{
	public MeshRenderer rend;

	public Color colorA = Color.white;

	public Color colorB = Color.clear;

	private float timer;

	private float dist;

	private MaterialPropertyBlock block;

	protected override void Awake()
	{
		base.Awake();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		timer = 0f;
		block.SetColor("_TintColor", Color.Lerp(colorA, colorB, 0f));
		block.SetFloat("_Dissolve", 0f);
		block.SetFloat("_DeformScale", Random.Range(2f, 4f));
		rend.SetPropertyBlock(block);
	}

	public void Setup(Vector3 endPos)
	{
		dist = Vector3.Distance(base.t.position, endPos);
		dist /= 12f;
		timer = 0f;
		base.t.localScale = new Vector3(1f, 1f, dist);
		base.t.rotation = Quaternion.LookRotation(base.t.position.DirTo(endPos));
	}

	private void Update()
	{
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.8f);
		block.SetColor("_TintColor", Color.Lerp(colorA, colorB, timer * timer));
		block.SetFloat("_Dissolve", timer);
		rend.SetPropertyBlock(block);
		if (timer == 1f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
