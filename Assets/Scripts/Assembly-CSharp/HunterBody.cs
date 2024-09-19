using UnityEngine;

public class HunterBody : Body
{
	private float jumpTimer;

	public override void OnEnable()
	{
		base.OnEnable();
		jumpTimer = Random.Range(0.5f, 1f);
	}

	public override void ExtraUpdate()
	{
		base.ExtraUpdate();
		if (base.rb.isKinematic)
		{
			return;
		}
		jumpTimer = Mathf.MoveTowards(jumpTimer, 0f, Time.deltaTime);
		if (jumpTimer == 0f && base.rb.velocity.sqrMagnitude > 16f)
		{
			if ((base.enemy as Hunter).CheckSneakJumpPosition(out var pos, out var normal))
			{
				ActivateEnemy(base.enemy.GetActualPosition());
				base.enemy.targetPosition = pos;
				base.enemy.targetNormal = normal;
				base.enemy.lockJumpRotation = true;
				base.enemy.stateMachine.SwitchState(typeof(HunterEnemyState));
			}
			jumpTimer = Random.Range(0.25f, 0.75f);
		}
	}
}
