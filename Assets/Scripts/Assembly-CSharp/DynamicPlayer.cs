using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class DynamicPlayer : MonoBehaviour
{
	public bool playOnStart = true;

	public float fadeTime = 1f;

	public MusicSet musicSet;

	public AudioMixerGroup mixerGroupOutput;

	private AudioSource[] channels;

	private void Awake()
	{
		Setup();
		if (playOnStart && (bool)musicSet)
		{
			PlayMusic();
		}
	}

	private void Setup()
	{
		if (!musicSet)
		{
			return;
		}
		channels = new AudioSource[musicSet.audioClips.Length];
		for (int i = 0; i < musicSet.audioClips.Length; i++)
		{
			channels[i] = base.gameObject.AddComponent<AudioSource>();
			channels[i].playOnAwake = false;
			channels[i].loop = true;
			channels[i].clip = musicSet.audioClips[i];
			if ((bool)mixerGroupOutput)
			{
				channels[i].outputAudioMixerGroup = mixerGroupOutput;
			}
			if (musicSet.partSet && i > 0)
			{
				channels[i].volume = 0f;
			}
		}
	}

	public void PlayMusic(MusicSet newMusicSet = null)
	{
		if ((bool)newMusicSet)
		{
			musicSet = newMusicSet;
			for (int i = 0; i < musicSet.audioClips.Length; i++)
			{
				channels[i].clip = musicSet.audioClips[i];
			}
		}
		AudioSource[] array = channels;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].PlayScheduled(AudioSettings.dspTime + 0.20000000298023224);
		}
	}

	public void StopMusic()
	{
		AudioSource[] array = channels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	public void SetSourceVolume(int sourceID, float newVolume)
	{
		channels[sourceID].volume = newVolume;
	}

	public void SwitchParts(int partID)
	{
		StopAllCoroutines();
		for (int i = 0; i < channels.Length; i++)
		{
			StartCoroutine(FadeChannel(channels[i], (i == partID) ? 1 : 0));
		}
	}

	private IEnumerator FadeChannel(AudioSource channel, float targetVolume)
	{
		float time = 0f;
		float start = channel.volume;
		while (time < fadeTime)
		{
			time += Time.deltaTime;
			channel.volume = Mathf.Lerp(start, targetVolume, time / fadeTime);
			yield return null;
		}
		channel.volume = targetVolume;
	}
}
