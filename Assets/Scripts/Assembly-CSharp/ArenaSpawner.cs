using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
	public static Action OnReachedNumber = delegate
	{
	};

	public static ArenaSpawner instanse;

	public int targetCount = 9;

	public int maxAtOnce = 3;

	public int maxBuffedAtOnce = 2;

	public List<ArenaSpawnerEntry> entries = new List<ArenaSpawnerEntry>();

	private List<BaseEnemy> enemies = new List<BaseEnemy>();

	public List<Vector3> spawns = new List<Vector3>();

	private bool activated;

	private int enemyIndex;

	private int lastSpawnIndex = -1;

	private int currentCount;

	private int deadCount;

	private int currentBuffedCount;

	private float delay = 2f;

	public AnimationCurve curve;

	public GameObject tPivot;

	public void Activate()
	{
		activated = true;
		delay = 0f;
	}

	private void OnDrawGizmos()
	{
		if (spawns.Count != 0)
		{
			Gizmos.color = Color.black / 2f;
			for (int i = 0; i < spawns.Count; i++)
			{
				Gizmos.DrawSphere(spawns[i], 0.5f);
			}
		}
	}

	private void Start()
	{
		instanse = this;
		int num = 0;
		for (int i = 0; i < 10; i++)
		{
			enemies.Add(UnityEngine.Object.Instantiate(entries[num].enemy, spawns[0], Quaternion.identity).GetComponent<BaseEnemy>());
			num = num.Next(entries.Count);
			enemies[i].ManualReset = true;
			enemies[i].gameObject.SetActive(value: false);
		}
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(Count));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(Count));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Count(BaseEnemy enemy)
	{
		if (enemies.Contains(enemy))
		{
			if (enemy.buffed)
			{
				currentBuffedCount--;
			}
			currentCount--;
			deadCount++;
			delay += 0.5f;
			if (deadCount == targetCount)
			{
				delay = 3f;
				maxAtOnce = 5;
				StartCoroutine(DestroyingCage());
			}
			else if (deadCount > 30)
			{
				Game.mission.SetState(2);
				CrowdControl.instance.KillTheRest();
			}
		}
	}

	private IEnumerator DestroyingCage()
	{
		Game.wideMode.Show();
		Game.player.MakeInvinsible(value: true);
		Game.time.SetDefaultTimeScale(0.5f);
		float timer = 0f;
		while (true)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime * 1.2f);
			if (timer == 1f && !Game.player.weapons.IsAttacking() && !Game.player.rb.isKinematic && Game.player.inputActive)
			{
				break;
			}
			yield return null;
		}
		Vector3 oldVel = Game.player.rb.velocity;
		Vector3 aPos = Game.player.tHead.position;
		Vector3 bPos = tPivot.transform.position;
		Game.player.Deactivate();
		Game.player.SetKinematic(value: true);
		Game.player.weapons.gameObject.SetActive(value: false);
		CameraController.shake.Shake(2);
		timer = 0f;
		float aFov = Game.player.fov.cam.fieldOfView;
		float height = Vector3.Distance(aPos, bPos) / 6f;
		bool reached = false;
		while (timer != 1f)
		{
			Vector3 position = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			position.y += Mathf.Sin(curve.Evaluate(timer) * (float)Math.PI) * height;
			Game.player.tHead.position = position;
			Game.player.mouseLook.LookAtSmooth(ChainBarrier.instance.transform.position, 3f, unscaled: true);
			Game.player.fov.cam.fieldOfView = Mathf.Lerp(aFov, 100f, curve.Evaluate(timer));
			Game.player.camController.Angle(curve.Evaluate(timer) * 5f);
			timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime * 0.25f);
			if (timer > 0.3333f && !reached)
			{
				reached = true;
				if (OnReachedNumber != null)
				{
					OnReachedNumber();
				}
			}
			yield return null;
		}
		Game.time.SetDefaultTimeScale(1f);
		Game.player.tHead.localPosition = new Vector3(0f, 0.75f, 0f);
		Game.player.Activate();
		Game.player.SetKinematic(value: false);
		Game.player.rb.velocity = oldVel;
		Game.player.weapons.gameObject.SetActive(value: true);
		Game.player.MakeInvinsible(value: false);
		Game.wideMode.Hide();
	}

	private void Reset()
	{
		StopAllCoroutines();
		activated = false;
		currentCount = (currentBuffedCount = (deadCount = 0));
		delay = 0f;
		maxAtOnce = 3;
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].tTarget = null;
			enemies[i].ResetPositionAndRotation();
			enemies[i].DeactivateEnemy();
		}
	}

	private void Update()
	{
		if (!activated)
		{
			return;
		}
		if (delay > 0f)
		{
			delay -= Time.deltaTime;
		}
		else
		{
			if (currentCount >= maxAtOnce)
			{
				return;
			}
			enemies[enemyIndex].Reset();
			enemies[enemyIndex].DeactivateEnemy();
			if (deadCount >= targetCount)
			{
				if (currentBuffedCount < maxBuffedAtOnce)
				{
					enemies[enemyIndex].Buff(value: true);
					currentBuffedCount++;
				}
				else
				{
					enemies[enemyIndex].Buff(value: false);
				}
			}
			else
			{
				enemies[enemyIndex].Buff(value: false);
			}
			int num = UnityEngine.Random.Range(0, spawns.Count);
			if (num == lastSpawnIndex)
			{
				num = num.Next(spawns.Count);
			}
			Vector3 pos = spawns[num];
			pos += MyRandom.DirXZ(4f);
			(QuickPool.instance.Get("EnemySpawnPoint", pos) as EnemySpawnPoint).SetEnemyToSpawn(enemies[enemyIndex], onMyPosition: true);
			lastSpawnIndex = num;
			currentCount++;
			enemyIndex = enemyIndex.Next(enemies.Count);
			delay = 1f;
		}
	}
}
