using System;
using UnityEngine;

public class EnemyTeleport : PooledMonobehaviour
{
	private BaseEnemy enemyToSpawn;

	private ParticleSystem particle;

	private AudioSource source;

	private float timer;

	private Vector3 spawnPos;

	public void Setup(BaseEnemy e, Vector3 spawnPos)
	{
		enemyToSpawn = e;
		this.spawnPos = spawnPos;
	}

	protected override void Awake()
	{
		base.Awake();
		particle = GetComponent<ParticleSystem>();
		source = GetComponent<AudioSource>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		timer = 0f;
		enemyToSpawn = null;
		base.gameObject.SetActive(value: false);
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		particle.Play();
		if (source.isPlaying)
		{
			source.Stop();
		}
		source.Play();
		timer = 0f;
	}

	private void Update()
	{
		if ((bool)enemyToSpawn && !timer.MoveTowards(1f, 2f))
		{
			enemyToSpawn.gameObject.SetActive(value: true);
			Debug.DrawLine(spawnPos, enemyToSpawn.body.recoverPos, Color.magenta, 2f);
			Transform obj = enemyToSpawn.t;
			Vector3 position = (base.t.position = spawnPos);
			obj.position = position;
			enemyToSpawn.targetPosition = enemyToSpawn.body.recoverPos;
			enemyToSpawn.stateMachine.SwitchState(typeof(EnemyJumpState));
			QuickEffectsPool.Get("EnemyTeleport 1", base.t.position, Quaternion.LookRotation(base.t.position.DirTo(enemyToSpawn.targetPosition))).Play();
			ThreatsUI.instance.SetTarget(enemyToSpawn.t);
			enemyToSpawn.PlaySound(enemyToSpawn.sounds.Spawn);
			enemyToSpawn = null;
		}
	}
}
