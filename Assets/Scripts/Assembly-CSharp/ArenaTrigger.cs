using System;
using UnityEngine;

public class ArenaTrigger : MonoBehaviour
{
	private void Awake()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: true);
	}

	private void OnTriggerEnter()
	{
		Game.mission.SetState(1);
		ArenaSpawner.instanse.Activate();
		CameraController.shake.Shake();
		QuickEffectsPool.Get("ArenaTriggerExp", base.transform.position, Quaternion.identity).Play();
		base.gameObject.SetActive(value: false);
	}
}
