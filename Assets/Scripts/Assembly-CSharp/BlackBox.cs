using UnityEngine;

public class BlackBox : MonoBehaviour
{
	public static BlackBox instance;

	public Transform tPlayerPosition;

	public ParticleSystem particle;

	public AudioSource source;

	private void Awake()
	{
		instance = this;
		base.gameObject.SetActive(value: false);
	}
}
