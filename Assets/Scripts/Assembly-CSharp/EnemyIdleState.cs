using System;

public class EnemyIdleState : BaseState
{
	private BaseEnemy enemy;

	public EnemyIdleState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
	}

	public override void LastCall()
	{
	}

	public override Type Tick()
	{
		if (!enemy.tTarget)
		{
			return null;
		}
		if (enemy.agent.enabled)
		{
			return typeof(EnemyFollowState);
		}
		enemy.TryJumpOff();
		return null;
	}
}
