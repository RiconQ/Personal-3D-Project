using System;
using UnityEngine;

public class ShieldPush : PooledMonobehaviour
{
	private BaseEnemy e;

	private RaycastHit hit;

	private float timer;

	private float delay = 0.5f;

	public DamageData dmg;

	protected override void Awake()
	{
		base.Awake();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		ShieldController.OnPush = (Action)Delegate.Combine(ShieldController.OnPush, new Action(Push));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		ShieldController.OnPush = (Action)Delegate.Remove(ShieldController.OnPush, new Action(Push));
	}

	private void Reset()
	{
		if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Push()
	{
		if (base.isActiveAndEnabled)
		{
			QuickEffectsPool.Get("Push Explosion", e.GetActualPosition()).Play();
			CrowdControl.instance.GetClosestEnemy(base.t.position, out var enemy, e, 20f);
			dmg.dir = (enemy ? (base.t.position.DirTo(enemy.GetActualPosition()) + Vector3.up).normalized : Vector3.up);
			e.Damage(dmg);
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		e = null;
	}

	public void Setup(BaseEnemy enemy)
	{
		e = enemy;
		timer = 0f;
	}

	private void LateUpdate()
	{
		if ((bool)e)
		{
			if (e.dead)
			{
				e = null;
				base.gameObject.SetActive(value: false);
			}
			else
			{
				base.t.position = e.GetActualPosition();
			}
		}
	}
}
