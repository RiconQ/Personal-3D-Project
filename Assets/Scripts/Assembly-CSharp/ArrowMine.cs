using System;
using UnityEngine;

public class ArrowMine : PooledMonobehaviour
{
	public GameObject _pooledQuickshotTrail;

	public DamageData dmg = new DamageData();

	private float lifetime;

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		lifetime = 0f;
	}

	private void Update()
	{
		lifetime = Mathf.MoveTowards(lifetime, 1f, Time.deltaTime * 0.25f);
		if (lifetime == 1f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

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

	public void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case 9:
			(QuickPool.instance.Get(_pooledQuickshotTrail, Game.player.t.position) as ShotTrail).Setup(Game.player.t.position + Vector3.up * 10f);
			CameraController.shake.Shake(2);
			Game.player.Damage(new Vector4(0f, 1f, 0f, 1f));
			base.gameObject.SetActive(value: false);
			break;
		case 10:
			if (!other.attachedRigidbody.isKinematic)
			{
				(QuickPool.instance.Get(_pooledQuickshotTrail, other.bounds.center) as ShotTrail).Setup(other.bounds.center + Vector3.up * 10f);
				CameraController.shake.Shake(2);
				dmg.dir = base.t.position.DirTo(other.bounds.center);
				other.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				base.gameObject.SetActive(value: false);
			}
			break;
		}
	}
}
