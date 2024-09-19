using System.Collections.Generic;
using UnityEngine;

public class Explosion : PooledMonobehaviour
{
	public DamageData damage = new DamageData();

	public int count = 3;

	public float range = 8f;

	private Collider[] colliders = new Collider[3];

	private List<BaseEnemy> enemies = new List<BaseEnemy>(3);

	private Vector3 dir;

	private float timer;

	private bool exploded;

	public Transform[] trails;

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		CrowdControl.instance.GetEnemiesInRange(enemies, base.t.position, range, count);
		timer = 0f;
		exploded = false;
		for (int i = 0; i < trails.Length; i++)
		{
			if (i < enemies.Count)
			{
				trails[i].SetPositionAndRotation(base.t.position, Quaternion.LookRotation(base.t.position.DirTo(enemies[i].GetActualPosition())));
				trails[i].gameObject.SetActive(value: true);
			}
			else
			{
				trails[i].gameObject.SetActive(value: false);
			}
		}
		if (enemies.Count > 1)
		{
			Game.time.SlowMotion(0.4f, 0.3f, 0.1f);
		}
	}

	private void Update()
	{
		if (exploded)
		{
			return;
		}
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 10f);
			for (int i = 0; i < enemies.Count; i++)
			{
				trails[i].position = Vector3.LerpUnclamped(base.t.position, enemies[i].GetActualPosition(), timer * 1.2f);
			}
			return;
		}
		if (enemies.Count > 0)
		{
			CameraController.shake.Shake(1);
			for (int j = 0; j < enemies.Count; j++)
			{
				damage.dir = (trails[j].forward + Vector3.up).normalized;
				enemies[j].Damage(damage);
			}
		}
		Physics.OverlapSphereNonAlloc(base.t.position, 1f, colliders, 16384);
		for (int k = 0; k < colliders.Length; k++)
		{
			if ((bool)colliders[k])
			{
				damage.dir = Vector3.up;
				colliders[k].GetComponent<IDamageable<DamageData>>().Damage(damage);
				colliders[k] = null;
			}
		}
		exploded = true;
	}
}
