using UnityEngine;

public class PooledEffectB : PooledMonobehaviour
{
	private ParticleSystem[] particles;

	protected override void Awake()
	{
		base.Awake();
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		ParticleSystem[] array = particles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}
}
