using System;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
	private float timer;

	private MeshRenderer rend;

	private MaterialPropertyBlock block;

	private AudioSource source;

	public Transform t { get; private set; }

	public void Setup()
	{
		t = base.transform;
		rend = GetComponent<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		block.SetFloat("_Reaction", 0f);
		rend.SetPropertyBlock(block);
		source = GetComponent<AudioSource>();
	}

	public void DamageReaction()
	{
		timer = 1f;
		rend.GetPropertyBlock(block);
		block.SetFloat("_Reaction", Mathf.Sin(timer * (float)Math.PI));
		rend.SetPropertyBlock(block);
		if (source.isPlaying)
		{
			source.Stop();
		}
		source.Play();
	}

	public void Tick()
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * 2f);
			rend.GetPropertyBlock(block);
			block.SetFloat("_Reaction", Mathf.Sin(timer * (float)Math.PI));
			rend.SetPropertyBlock(block);
		}
	}
}
