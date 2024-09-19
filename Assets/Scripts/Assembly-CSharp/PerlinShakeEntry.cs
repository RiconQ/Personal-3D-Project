using System;
using UnityEngine;

[Serializable]
public class PerlinShakeEntry
{
	public float time;

	public Vector3 result;

	public PerlinShakePreset preset;

	private float amp;

	private Vector3 offset;

	public void Reset()
	{
		if (time != 0f)
		{
			time = 0f;
		}
	}

	public void Setup(PerlinShakePreset p)
	{
		preset = p;
		time = preset.duration;
		offset.x = UnityEngine.Random.Range(0f, 1f);
		offset.y = UnityEngine.Random.Range(0f, 1f);
		offset.z = UnityEngine.Random.Range(0f, 1f);
	}

	public Vector3 GetShake(bool unscaled)
	{
		time = Mathf.MoveTowards(time, 0f, unscaled ? Time.unscaledDeltaTime : Time.deltaTime);
		amp = preset.curve.Evaluate(1f - time / preset.duration) * preset.amplitude;
		result.x = (-0.5f + Mathf.PerlinNoise((offset.x + time) * preset.speed, 0f)) * amp;
		result.y = (-0.5f + Mathf.PerlinNoise((offset.y + time) * preset.speed, 0.5f)) * amp;
		result.z = (-0.5f + Mathf.PerlinNoise((offset.z + time) * preset.speed, 1f)) * amp;
		return result;
	}
}
