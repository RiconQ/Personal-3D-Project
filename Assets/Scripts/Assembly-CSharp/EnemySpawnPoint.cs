using System;
using UnityEngine;

public class EnemySpawnPoint : PooledMonobehaviour
{
	public ParticleSystem loopedFX;

	public ParticleSystem burstFX;

	public AudioClip sfxSpawn;

	public AudioClip sfxSpawnEnd;

	public BaseEnemy enemyToActivate;

	public float speed = 2f;

	private float timer;

	private AudioSource source;

	public void SetEnemyToSpawn(BaseEnemy enemy, bool onMyPosition = false)
	{
		enemyToActivate = enemy;
		if (onMyPosition)
		{
			enemy.t.position = base.t.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		source = GetComponent<AudioSource>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnEnable()
	{
		timer = 0f;
		loopedFX.Play();
		source.PlayClip(sfxSpawn);
	}

	private void Reset()
	{
		enemyToActivate = null;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			if (timer == 1f && Game.mission.state == MissionState.MissionStates.InProcess)
			{
				loopedFX.Stop();
				burstFX.Play();
				source.PlayClip(sfxSpawnEnd);
				if ((bool)enemyToActivate)
				{
					enemyToActivate.ActivateEnemy();
					enemyToActivate.Teleport(base.t.position);
					ThreatsUI.instance.SetTarget(enemyToActivate.t);
				}
			}
		}
		else if (!burstFX.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
