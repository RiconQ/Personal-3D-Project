using UnityEngine;

public class BarrierChain : MonoBehaviour
{
	public Transform t;

	public GameObject mesh;

	public ParticleSystem particle;

	public void Hide()
	{
		mesh.SetActive(value: false);
		particle.Play();
	}

	public void Reset()
	{
		mesh.SetActive(value: true);
		particle.Clear();
	}
}
