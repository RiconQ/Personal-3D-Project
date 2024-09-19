using System;
using UnityEngine;

public class SkyShot : PooledMonobehaviour
{
	private BaseEnemy e;

	private RaycastHit hit;

	private float timer;

	private float delay = 0.5f;

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
		if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)e)
		{
			Game.player.weapons.voidshotTargets.Remove(e);
			e = null;
		}
	}

	public void Setup(BaseEnemy enemy)
	{
		e = enemy;
		timer = 0f;
	}

	private void LateUpdate()
	{
		if (!e)
		{
			return;
		}
		if (e.dead)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.t.position = e.GetActualPosition();
		if (!e.gameObject.activeInHierarchy && !e.body.rb.isKinematic)
		{
			timer += Time.deltaTime;
		}
		if (timer >= 0.5f)
		{
			(Game.player.weapons.weaponControllers[1] as BowController).PowerShot(base.t.position - Vector3.up * 2f, Vector3.up, 6f, Game.style.BowVoidShot);
			CameraController.shake.Shake(2);
			base.gameObject.SetActive(value: false);
		}
	}
}
