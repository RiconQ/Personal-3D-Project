using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
	public List<BaseEnemy> enemiesToKill = new List<BaseEnemy>();

	public List<BaseEnemy> enemiesToSpawn = new List<BaseEnemy>();

	private bool spawned;

	private void Start()
	{
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			item.ManualReset = true;
			item.DeactivateEnemy();
		}
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		spawned = false;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			item.ResetPositionAndRotation();
			item.DeactivateEnemy();
		}
	}

	private void Check(BaseEnemy enemy)
	{
		if (spawned)
		{
			return;
		}
		bool flag = true;
		foreach (BaseEnemy item in enemiesToKill)
		{
			if (!item.dead)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		spawned = true;
		foreach (BaseEnemy item2 in enemiesToSpawn)
		{
			ThreatsUI.instance.SetTarget(item2.t);
			(QuickPool.instance.Get("EnemySpawnPoint", item2.t.position) as EnemySpawnPoint).enemyToActivate = item2;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "EnemyTrigger.png");
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		foreach (BaseEnemy item in enemiesToKill)
		{
			if ((bool)item)
			{
				Gizmos.DrawLine(item.transform.position, base.transform.position);
			}
		}
		Gizmos.color = Color.grey;
		foreach (BaseEnemy item2 in enemiesToSpawn)
		{
			if ((bool)item2)
			{
				Gizmos.DrawLine(item2.transform.position, base.transform.position);
			}
		}
	}
}
