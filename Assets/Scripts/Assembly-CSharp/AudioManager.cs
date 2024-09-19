using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public AnimationCurve volumeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AudioMixer mixer;

	[SerializeField]
	private string[] mixerGroupNames = new string[2] { "Sounds Volume", "Music Volume" };

	private float[] volumes = new float[2];

	private float temp;

	private float _musicLowpass = 22000f;

	private float _musicGain = 1f;

	private void Start()
	{
		for (int i = 0; i < mixerGroupNames.Length; i++)
		{
			int value = 0;
			if (GamePrefs.cached.ContainsKey(mixerGroupNames[i]))
			{
				GamePrefs.cached.TryGetValue(mixerGroupNames[i], out value);
			}
			else
			{
				value = PlayerPrefs.GetInt(mixerGroupNames[i]);
				GamePrefs.cached.Add(mixerGroupNames[i], value);
			}
			SetVolume01(mixerGroupNames[i], (float)value / 10f);
		}
	}

	public void Gain(float value = 1f)
	{
		_musicGain = 1f;
	}

	private void Update()
	{
		mixer.GetFloat("Master Volume", out temp);
		temp = Mathf.Lerp(0f, -20f, Game.fading.cg.alpha);
		mixer.SetFloat("Master Volume", temp);
		mixer.GetFloat("Sounds Pitch", out temp);
		temp = Mathf.Lerp(temp, Time.timeScale, Time.unscaledDeltaTime * 4f);
		mixer.SetFloat("Sounds Pitch", temp);
		if (_musicGain != 0f)
		{
			_musicGain = Mathf.MoveTowards(_musicGain, 0f, Time.deltaTime * 0.75f);
			mixer.SetFloat("Music Gain", 1f - Mathf.Sin(_musicGain * (float)Math.PI) * 0.5f);
		}
		float num = (Game.player ? Mathf.Clamp01(Game.player.rb.velocity.y / -20f) : 0f);
		_musicLowpass = Mathf.Lerp(_musicLowpass, Mathf.Lerp(200f, 22000f, Time.timeScale - num * 0.75f), Time.unscaledDeltaTime * 10f);
		mixer.SetFloat("Music Lowpass", _musicLowpass);
	}

	public float GetVolume01(string groupName)
	{
		float result = 0f;
		for (int i = 0; i < mixerGroupNames.Length; i++)
		{
			if (mixerGroupNames[i] == groupName)
			{
				result = volumes[i];
			}
		}
		return result;
	}

	public void SetVolume01(string groupName, float value01)
	{
		float value2 = Mathf.Lerp(-80f, 0f, volumeCurve.Evaluate(value01));
		for (int i = 0; i < mixerGroupNames.Length; i++)
		{
			if (mixerGroupNames[i] == groupName)
			{
				volumes[i] = value01;
			}
		}
		mixer.SetFloat(groupName, value2);
	}
}
