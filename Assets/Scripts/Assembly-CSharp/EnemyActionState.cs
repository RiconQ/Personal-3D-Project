using System;
using UnityEngine;

public class EnemyActionState : BaseState
{
	private float timer;

	private BaseEnemy enemy;

	public EnemyActionState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
		enemy.targetPosition = enemy.t.position;
		timer = ((enemy.actionTime == 0f) ? 0.5f : enemy.actionTime);
	}

	public override void LastCall()
	{
	}

	public override Type Tick()
	{
		if (enemy.staggered)
		{
			return typeof(EnemyIdleState);
		}
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
			return null;
		}
		return typeof(EnemyIdleState);
	}
}
