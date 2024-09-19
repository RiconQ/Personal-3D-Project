using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ParticleSystem))]
public class PooledEffect : MonoBehaviour
{
	public Vector2 pitchMinMax = new Vector2(0.8f, 1.2f);

	public Transform t;

	public AudioSource source { get; private set; }

	public ParticleSystem particle { get; private set; }

	private void Awake()
	{
		t = base.transform;
		source = GetComponent<AudioSource>();
		particle = GetComponent<ParticleSystem>();
	}

	public void Play(float value = -1f, int emit = 0)
	{
		source.pitch = ((value == -1f) ? Random.Range(pitchMinMax.x, pitchMinMax.y) : Mathf.Lerp(0.8f, 1.2f, value));
		source.Play();
		if (emit == 0)
		{
			particle.Play();
		}
		else
		{
			particle.Emit(emit);
		}
	}
}
