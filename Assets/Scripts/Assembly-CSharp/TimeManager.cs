using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
	private bool timeIsStopped;

	private float slowmoScale;

	private float slowmoDuration;

	private float slowmoDelay;

	private float cachedTimeScale;

	public float sinTime { get; private set; }

	public float defaultTimeScale { get; private set; }

	public void SlowMotion(float scale = 0.1f, float duration = 0.1f, float delay = 0f)
	{
		slowmoScale = scale;
		slowmoDuration = duration;
		slowmoDelay = delay;
	}

	public void SetDefaultTimeScale(float newTimeScale)
	{
		if (defaultTimeScale != newTimeScale)
		{
			defaultTimeScale = newTimeScale;
		}
	}

	public void StopSlowmo()
	{
		slowmoDuration = 0f;
	}

	public void Stop()
	{
		timeIsStopped = true;
		cachedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
	}

	public void Play()
	{
		timeIsStopped = false;
		Time.timeScale = cachedTimeScale;
	}

	private void Update()
	{
		Shader.SetGlobalFloat("_UnscaledTime", Time.unscaledTime);
		sinTime = Mathf.MoveTowards(sinTime, (float)Math.PI * 2f, Time.deltaTime);
		if (sinTime == (float)Math.PI * 2f)
		{
			sinTime = 0f;
		}
		if (Game.debug && Input.GetKeyDown(KeyCode.F1))
		{
			Time.timeScale = defaultTimeScale;
			timeIsStopped = false;
		}
		if (timeIsStopped)
		{
			return;
		}
		if (slowmoDelay > 0f)
		{
			slowmoDelay -= Time.unscaledDeltaTime;
		}
		else if (slowmoDuration > 0f)
		{
			if (Time.timeScale != slowmoScale)
			{
				Time.timeScale = slowmoScale;
			}
			slowmoDuration -= Time.unscaledDeltaTime;
		}
		else if (Time.timeScale != defaultTimeScale)
		{
			Time.timeScale = defaultTimeScale;
		}
	}
}
