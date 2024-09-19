using UnityEngine;

public class HeavyDashState : EnemyDashState
{
	private BaseEnemy enemy;

	public HeavyDashState(BaseEnemy e)
		: base(e)
	{
		enemy = e;
	}

	public override void LastCall()
	{
		enemy.agent.enabled = true;
		enemy.clldr.isTrigger = false;
		enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
		enemy.tMesh.gameObject.SetActive(value: true);
		enemy.particleDash.Stop();
		if (enemy.agent.isOnOffMeshLink)
		{
			enemy.agent.CompleteOffMeshLink();
		}
		enemy.animator.SetTrigger("Damage");
	}
}
