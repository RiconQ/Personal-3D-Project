using System;
using UnityEngine;

public class ShieldTrap : PooledMonobehaviour
{
	public static ShieldTrap instance;

	public DamageData dmg;

	private bool triggered;

	private float timer;

	private float actionTimer;

	private Collider[] clldrs = new Collider[5];

	protected override void Awake()
	{
		base.Awake();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnActualEnable()
	{
		timer = (actionTimer = 0f);
		triggered = false;
		base.OnActualEnable();
		if (instance != null)
		{
			instance.gameObject.SetActive(value: false);
		}
		instance = this;
	}

	private void Update()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
		}
		if (triggered)
		{
			actionTimer = Mathf.MoveTowards(actionTimer, 1f, Time.deltaTime * 4f);
			if (actionTimer == 1f)
			{
				Explode();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.layer == 10 && !(timer < 0.6f) && !triggered)
		{
			triggered = true;
		}
	}

	private void Explode()
	{
		int num = 0;
		Physics.OverlapCapsuleNonAlloc(base.t.position, base.t.position + Vector3.up * 4f, 6f, clldrs, 1024);
		for (int i = 0; i < clldrs.Length; i++)
		{
			if (clldrs[i] != null)
			{
				num++;
				dmg.dir = (base.t.position.DirTo(clldrs[i].bounds.center) + Vector3.up * 2f).normalized;
				clldrs[i].GetComponent<IDamageable<DamageData>>().Damage(dmg);
				clldrs[i] = null;
			}
		}
		if (num > 0)
		{
			CameraController.shake.Shake();
		}
		QuickEffectsPool.Get("Shield Trap Explosion", base.t.position).Play();
		base.gameObject.SetActive(value: false);
	}
}
