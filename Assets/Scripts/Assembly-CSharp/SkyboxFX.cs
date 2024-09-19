using System;
using UnityEngine;

public class SkyboxFX : MonoBehaviour
{
	private void Awake()
	{
		MissionState.OnMissionCompleted = (Action)Delegate.Combine(MissionState.OnMissionCompleted, new Action(Play));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		MissionState.OnMissionCompleted = (Action)Delegate.Remove(MissionState.OnMissionCompleted, new Action(Play));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		GetComponent<ParticleSystem>().Clear();
	}

	private void Play()
	{
		GetComponent<ParticleSystem>().Play();
	}
}
