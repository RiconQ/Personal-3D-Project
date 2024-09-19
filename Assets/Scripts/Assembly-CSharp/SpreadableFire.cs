using UnityEngine;

public class SpreadableFire : MonoBehaviour
{
	private ParticleSystem[] particles;

	public bool onFire;

	private void Awake()
	{
		particles = GetComponentsInChildren<ParticleSystem>();
		if (!onFire)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Stop();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!onFire && other.CompareTag("Fire") && other.GetComponent<SpreadableFire>().onFire)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Play();
			}
			onFire = true;
		}
	}
}
