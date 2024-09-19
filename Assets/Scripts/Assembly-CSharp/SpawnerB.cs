using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerB : MonoBehaviour
{
	public int maxAtOnce;

	public float spawnDelay = 0.1f;

	public bool floorIsTrigger;

	public List<BaseEnemy> enemies = new List<BaseEnemy>(20);

	public GameObject barrier;

	private bool triggered;

	private int spawnedCount;

	private int deadCount;

	private int lastAlive;

	private int state;

	private float timer;

	private void Start()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].ManualReset = true;
			enemies[i].gameObject.SetActive(value: false);
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		if (floorIsTrigger)
		{
			Grounder grounder = PlayerController.instance.grounder;
			grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Grounded));
		}
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		if (floorIsTrigger)
		{
			Grounder grounder = PlayerController.instance.grounder;
			grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Grounded));
		}
	}

	private void Grounded()
	{
		if (!triggered && (base.transform.position.y - (PlayerController.instance.t.position.y - 1f)).Abs() < 1f)
		{
			triggered = true;
		}
	}

	private void Reset()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].ResetPositionAndRotation();
			enemies[i].DeactivateEnemy();
		}
		triggered = false;
		state = 0;
		timer = 0f;
		spawnedCount = 0;
		if ((bool)barrier && !barrier.activeInHierarchy)
		{
			barrier.gameObject.SetActive(value: true);
		}
		base.gameObject.SetActive(value: true);
	}

	private void Update()
	{
		if (!triggered)
		{
			return;
		}
		switch (state)
		{
		case 0:
			if (timer > 0f)
			{
				timer -= Time.deltaTime;
			}
			else
			{
				(QuickPool.instance.Get("EnemySpawnPoint", enemies[spawnedCount].t.position) as EnemySpawnPoint).enemyToActivate = enemies[spawnedCount];
				spawnedCount++;
				timer = spawnDelay;
			}
			if (spawnedCount == enemies.Count)
			{
				state++;
			}
			break;
		case 1:
		{
			deadCount = 0;
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].dead)
				{
					deadCount++;
				}
				else if (i < spawnedCount)
				{
					lastAlive = i;
				}
			}
			if (deadCount == enemies.Count)
			{
				Game.time.SlowMotion(0.2f, 0.6f, 0.1f);
				timer = 0f;
				state++;
			}
			break;
		}
		case 2:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			if (timer == 1f)
			{
				state++;
			}
			break;
		case 3:
			if ((bool)barrier)
			{
				barrier.GetComponent<ITriggerable>().Trigger();
				CameraController.shake.Shake(2);
				QuickEffectsPool.Get("Chain Gate Explosion", barrier.transform.position, barrier.transform.rotation).Play();
			}
			base.gameObject.SetActive(value: false);
			break;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!triggered)
		{
			triggered = true;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.position, 1f);
		for (int i = 0; i < enemies.Count; i++)
		{
			Gizmos.DrawLine(base.transform.position, enemies[i].transform.position);
		}
	}
}
