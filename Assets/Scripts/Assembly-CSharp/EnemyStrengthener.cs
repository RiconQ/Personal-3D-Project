using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStrengthener : MonoBehaviour
{
	private Transform t;

	private List<BaseEnemy> affectedEnemies = new List<BaseEnemy>(10);

	private void Awake()
	{
		t = base.transform;
	}

	private void Start()
	{
		BuffEnemies();
		if ((bool)QuickmapScene.instance)
		{
			QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(BuffEnemies));
		}
	}

	private void OnDestroy()
	{
		if ((bool)QuickmapScene.instance)
		{
			DebuffEnemies();
			QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(BuffEnemies));
		}
	}

	private void BuffEnemies()
	{
		affectedEnemies.Clear();
		foreach (BaseEnemy allEnemy in CrowdControl.allEnemies)
		{
			if ((t.position.y - allEnemy.t.position.y).Abs() < 1f)
			{
				affectedEnemies.Add(allEnemy);
				allEnemy.Buff(value: true);
			}
		}
	}

	private void DebuffEnemies()
	{
		foreach (BaseEnemy affectedEnemy in affectedEnemies)
		{
			affectedEnemy.Buff(value: false);
		}
		affectedEnemies.Clear();
	}
}
