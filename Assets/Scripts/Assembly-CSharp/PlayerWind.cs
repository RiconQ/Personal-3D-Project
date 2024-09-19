using UnityEngine;

public class PlayerWind : MonoBehaviour
{
	private AudioSource source;

	private PlayerController player;

	public float vel = 8f;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
		player = GetComponentInParent<PlayerController>();
	}

	private void Update()
	{
		if (player.grounder.grounded)
		{
			if (source.volume != 0f)
			{
				source.volume = Mathf.MoveTowards(source.volume, 0f, Time.deltaTime * 8f);
			}
			else if (source.isPlaying)
			{
				source.Pause();
			}
		}
		else
		{
			if (!source.isPlaying)
			{
				source.UnPause();
			}
			source.pitch = Mathf.Clamp01(player.rb.velocity.magnitude / vel);
			source.volume = Mathf.Lerp(source.volume, Mathf.Clamp01(source.pitch / 4f), Time.deltaTime);
		}
	}
}
