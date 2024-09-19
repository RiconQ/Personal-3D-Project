using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
	public static BackgroundMusic instance;

	public static Dictionary<string, float> tracks = new Dictionary<string, float>(10);

	public float defaultVolume = 1f;

	public bool startOver = true;

	public bool playAtStart;

	public bool stopOnReset;

	private AudioSource source;

	private float volume = 1f;

	private float time;

	private void Awake()
	{
		if (instance != this)
		{
			UnityEngine.Object.Destroy(instance);
		}
		instance = this;
		source = GetComponent<AudioSource>();
		if (!playAtStart)
		{
			source.Stop();
		}
		else
		{
			source.Play();
		}
		PlayerController.OnPlayerDie = (Action)Delegate.Combine(PlayerController.OnPlayerDie, new Action(Stop));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		if (!tracks.ContainsKey(source.clip.name))
		{
			tracks.Add(source.clip.name, 0f);
		}
		else if (!startOver)
		{
			tracks.TryGetValue(source.clip.name, out time);
			source.time = time;
		}
	}

	private void OnDestroy()
	{
		PlayerController.OnPlayerDie = (Action)Delegate.Remove(PlayerController.OnPlayerDie, new Action(Stop));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		if (tracks.ContainsKey(source.clip.name))
		{
			tracks[source.clip.name] = time;
		}
	}

	public void Setup(AudioClip musicClip)
	{
		source.clip = musicClip;
		if (!startOver)
		{
			tracks.TryGetValue(source.clip.name, out time);
			source.time = time;
		}
		Play();
	}

	public void Stop()
	{
		volume = 0f;
	}

	public void Play()
	{
		source.Play();
		volume = defaultVolume;
	}

	public void Stop(bool instant = false)
	{
		if ((bool)instance)
		{
			source.volume = 0f;
		}
		volume = 0f;
	}

	public void Reset()
	{
		if (Game.mission.state != MissionState.MissionStates.Complete)
		{
			volume = 1f;
		}
		if (stopOnReset)
		{
			Stop();
		}
	}

	private void Update()
	{
		if (source.isPlaying)
		{
			time = source.time;
		}
		if (source.volume != volume * defaultVolume)
		{
			source.volume = Mathf.Lerp(source.volume, volume * defaultVolume, Time.deltaTime);
		}
	}
}
