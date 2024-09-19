using UnityEngine;

public class EnemyAnimatorEvents : MonoBehaviour
{
	private BaseEnemy enemy;

	private void Awake()
	{
		enemy = GetComponentInParent<BaseEnemy>();
	}

	public void EnemyEvent(int i)
	{
		enemy.AnimationEvent(i);
	}
}
