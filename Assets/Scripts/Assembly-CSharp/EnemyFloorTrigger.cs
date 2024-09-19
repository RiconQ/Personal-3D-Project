using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFloorTrigger : MonoBehaviour
{
	public List<BaseEnemy> enemiesToSpawn = new List<BaseEnemy>();

	private Transform t;

	private bool spawned;

	private float checkDelay;

	private Vector3 temp;

	private RaycastHit hit;

	private void Start()
	{
		t = base.transform;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			item.ManualReset = true;
			item.DeactivateEnemy();
		}
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Check));
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

	private void Check()
	{
		if (!spawned && t.position.y - Game.player.t.position.y < 0f)
		{
			temp = t.position;
			temp.y += 0.5f;
			Physics.Raycast(t.position, temp.DirTo(Game.player.t.position), out hit, 18f, 513);
			if (hit.distance != 0f && hit.collider.gameObject.layer == 9)
			{
				Debug.DrawLine(temp, hit.point, Color.green, 2f);
				SpawnEnemies();
			}
		}
	}

	private void SpawnEnemies()
	{
		CameraController.shake.Shake();
		spawned = true;
		StartCoroutine(SpawningEnemies());
	}

	private IEnumerator SpawningEnemies()
	{
		float timer = 0f;
		int index = 0;
		while (index != enemiesToSpawn.Count)
		{
			if (timer == 0f)
			{
				ThreatsUI.instance.SetTarget(enemiesToSpawn[index].t);
				(QuickPool.instance.Get("EnemySpawnPoint", enemiesToSpawn[index].t.position) as EnemySpawnPoint).enemyToActivate = enemiesToSpawn[index];
				index++;
				timer = 0.2f;
			}
			else
			{
				timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
				yield return null;
			}
		}
	}

	private void Update()
	{
		if (spawned || !(t.position.y - Game.player.t.position.y < 0f))
		{
			return;
		}
		if (checkDelay == 0f)
		{
			temp = t.position;
			temp.y += 0.5f;
			Physics.Raycast(t.position, temp.DirTo(Game.player.t.position), out hit, 18f, 513);
			if (hit.distance != 0f && hit.collider.gameObject.layer == 9)
			{
				Debug.DrawLine(temp, hit.point, Color.green, 2f);
				SpawnEnemies();
			}
			checkDelay = 0.1f;
		}
		else
		{
			checkDelay = Mathf.MoveTowards(checkDelay, 0f, Time.deltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "EnemyFloorTrigger.png");
		Gizmos.color = Color.yellow;
		foreach (BaseEnemy item in enemiesToSpawn)
		{
			if (item != null)
			{
				Gizmos.DrawLine(item.transform.position, base.transform.position);
			}
		}
	}
}
