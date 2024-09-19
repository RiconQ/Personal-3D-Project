using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeightTrigger : MonoBehaviour
{
	public List<BaseEnemy> enemiesToSpawn = new List<BaseEnemy>();

	private Transform t;

	private bool spawned;

	private void Start()
	{
		t = base.transform;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			item.ManualReset = true;
			item.DeactivateEnemy();
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		if (spawned)
		{
			spawned = false;
		}
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			item.ResetPositionAndRotation();
			item.DeactivateEnemy();
		}
	}

	private void Update()
	{
		Check();
	}

	private void Check()
	{
		if (spawned || !((t.position.y - Game.player.t.position.y).Abs() < 2f))
		{
			return;
		}
		CameraController.shake.Shake();
		spawned = true;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			ThreatsUI.instance.SetTarget(item.t);
			(QuickPool.instance.Get("EnemySpawnPoint", item.t.position) as EnemySpawnPoint).enemyToActivate = item;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "EnemyFloorTrigger.png");
		Gizmos.color = Color.yellow;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			Gizmos.DrawLine(item.transform.position, base.transform.position);
		}
	}
}
