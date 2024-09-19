using System;
using UnityEngine;

public class EnemyFollowState : BaseState
{
	public BaseEnemy enemy;

	private bool following;

	private float timer;

	public EnemyFollowState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
		timer = 0.25f;
		following = enemy.agent.velocity.sqrMagnitude > 0.25f;
		if (enemy.animator.GetBool("Running") != following)
		{
			enemy.animator.SetBool("Running", following);
		}
		if (!enemy.agent.enabled)
		{
			enemy.TryDirectJump();
		}
	}

	public override void LastCall()
	{
		enemy.animator.speed = 1f;
		enemy.animator.SetBool("Running", value: false);
		if (enemy.isActiveAndEnabled && enemy.agent.enabled)
		{
			enemy.agent.ResetPath();
		}
	}

	public override Type Tick()
	{
		float sqrMagnitude = enemy.agent.velocity.sqrMagnitude;
		enemy.agent.SetDestination(enemy.targetPosition);
		enemy.RunningUpdate();
		if (following != sqrMagnitude > 0.25f)
		{
			following = sqrMagnitude > 0.25f;
			enemy.animator.SetBool("Running", following);
		}
		if (!following)
		{
			if (timer <= 0f)
			{
				enemy.FollowBreak();
				timer = UnityEngine.Random.Range(0.5f, 1.5f);
			}
			else
			{
				timer -= Time.deltaTime;
			}
		}
		return null;
	}
}
