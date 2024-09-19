using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Ambience : MonoBehaviour
{
	private AudioSource source;

	private float speed = 4f;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
		source.volume = 0f;
		source.volume = 0.75f;
	}

	public void PlayClip(AudioClip new_clip, float volume)
	{
		StopAllCoroutines();
		StartCoroutine(Switching(new_clip, volume));
	}

	private IEnumerator Switching(AudioClip new_clip, float volume)
	{
		if (source.isPlaying)
		{
			while (source.volume != 0f)
			{
				source.volume = Mathf.MoveTowards(source.volume, 0f, Time.deltaTime * speed);
				yield return null;
			}
		}
		source.volume = volume;
		source.clip = new_clip;
		if (!source.isPlaying)
		{
			source.Play();
		}
		while (source.volume != 1f)
		{
			source.volume = Mathf.MoveTowards(source.volume, 1f, Time.deltaTime);
			yield return null;
		}
	}
}
