using UnityEngine;
using UnityEngine.Audio;

public class SoundsManager : MonoBehaviour
{
	public AudioMixerGroup mixerGroup;

	public AudioSource[] sources;

	public AudioSource source;

	private int index;

	private int count = 20;

	public SoundGroup[] Groups;

	[Button]
	public void Setup()
	{
		sources = new AudioSource[count];
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = new GameObject($"Audio Source {i}");
			gameObject.transform.SetParent(base.transform);
			sources[i] = gameObject.AddComponent<AudioSource>();
			sources[i].minDistance = 3f;
			sources[i].maxDistance = 18f;
			sources[i].outputAudioMixerGroup = mixerGroup;
		}
		for (int j = 0; j < Groups.Length; j++)
		{
			Groups[j].Setup(base.transform);
		}
	}

	public void PlaySound(AudioClip clip, int groupIndex)
	{
		Groups[groupIndex].PlayClip(clip);
	}

	public void PlayClip(AudioClip clip, float volume = 1f)
	{
		source = sources[index];
		source.spatialBlend = 0f;
		source.pitch = Random.Range(0.9f, 1.1f);
		source.clip = clip;
		if (source.volume != volume)
		{
			source.volume = volume;
		}
		source.Play();
		index = index.Next(sources.Length);
	}

	public void PlayClipAtPosition(AudioClip clip, float volume, Vector3 pos)
	{
		source = sources[index];
		source.transform.position = pos;
		source.spatialBlend = 1f;
		source.pitch = Random.Range(0.75f, 1.1f);
		source.clip = clip;
		if (source.volume != volume)
		{
			source.volume = volume;
		}
		source.Play();
		index = index.Next(sources.Length);
	}
}
