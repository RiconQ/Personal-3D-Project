using System;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeMap : MonoBehaviour
{
	public List<ArenaWave> waves = new List<ArenaWave>();

	public int count;

	public Transform tTarget;

	public Transform tHead;

	public PlayerController player;

	public GameObject results;

	private int deadCount;

	private int deadInCurrentWave;

	private int lastStandingInCurrentWave;

	private int waveIndex;

	private float timer;

	private void Start()
	{
		for (int i = 0; i < waves.Count; i++)
		{
			for (int j = 0; j < waves[i].enemies.Count; j++)
			{
				if (i > 0)
				{
					waves[i].enemies[j].ManualReset = true;
					waves[i].enemies[j].gameObject.SetActive(value: false);
				}
			}
		}
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnenyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnenyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void Reset()
	{
		deadCount = 0;
		waveIndex = 0;
		timer = 0f;
		tTarget = null;
		for (int i = 0; i < waves.Count; i++)
		{
			for (int j = 0; j < waves[i].enemies.Count; j++)
			{
				waves[i].enemies[j].ResetPositionAndRotation();
				if (i > 0)
				{
					waves[i].enemies[j].DeactivateEnemy();
				}
			}
		}
	}

	private void OnEnenyDie(BaseEnemy enemy)
	{
		if (Game.mission.state != MissionState.MissionStates.Complete)
		{
			deadCount++;
			if (CrowdControl.allEnemies.Count == deadCount)
			{
				Game.mission.SetState(2);
				CameraController.shake.Shake();
			}
		}
	}

	private void LateUpdate()
	{
		if (waveIndex >= waves.Count)
		{
			return;
		}
		deadInCurrentWave = 0;
		for (int i = 0; i < waves[waveIndex].enemies.Count; i++)
		{
			if (waves[waveIndex].enemies[i].dead)
			{
				deadInCurrentWave++;
			}
			else
			{
				lastStandingInCurrentWave = i;
			}
		}
		if (deadInCurrentWave != waves[waveIndex].enemies.Count && (deadInCurrentWave != waves[waveIndex].enemies.Count - 1 || !(waves[waveIndex].enemies[lastStandingInCurrentWave].GetHealthPercentage() < 0.25f)))
		{
			return;
		}
		waveIndex++;
		if (waveIndex < waves.Count)
		{
			for (int j = 0; j < waves[waveIndex].enemies.Count; j++)
			{
				(QuickPool.instance.Get("EnemySpawnPoint", waves[waveIndex].enemies[j].transform.position) as EnemySpawnPoint).enemyToActivate = waves[waveIndex].enemies[j];
			}
		}
	}
}
