using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundGroup
{
	public int Count = 10;

	public AudioMixerGroup MixerGroup;

	public int _index;

	public AudioSource[] _sources;

	private AudioSource source;

	public void Setup(Transform tParent)
	{
		_sources = new AudioSource[Count];
		for (int i = 0; i < Count; i++)
		{
			GameObject gameObject = new GameObject($"{MixerGroup.name} Source {i}");
			gameObject.transform.SetParent(tParent);
			_sources[i] = gameObject.AddComponent<AudioSource>();
			_sources[i].minDistance = 3f;
			_sources[i].maxDistance = 18f;
			_sources[i].outputAudioMixerGroup = MixerGroup;
		}
	}

	public void PlayClip(AudioClip clip)
	{
		source = _sources[_index];
		if (source.isPlaying)
		{
			source.Stop();
		}
		source.pitch = MyRandom.Range(0.9f, 1.1f);
		source.clip = clip;
		source.Play();
		_index = _index.Next(_sources.Length);
	}
}
