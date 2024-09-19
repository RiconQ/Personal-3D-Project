using System;
using UnityEngine;

public class TheOrb : BaseBreakable
{
	private const float delay = 3f;

	private new Transform t;

	private DamageData dmgInfo = new DamageData();

	private Collider[] colliders = new Collider[3];

	private Animator animator;

	public ParticleSystem particle;

	public GameObject objOrb;

	public override void Awake()
	{
		base.Awake();
		t = base.transform;
		animator = GetComponentInChildren<Animator>();
		dmgInfo.amount = 1000f;
		dmgInfo.newType = Game.style.basicMill;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void Reset()
	{
		objOrb.SetActive(value: true);
		particle.Play();
		Game.mission.SetState(0);
		base.gameObject.SetActive(value: true);
	}

	public override void Damage(DamageData damage)
	{
		base.Damage(damage);
		if (damage.newType == Game.player.weapons.daggerController.dmg_Pull)
		{
			Game.player.PullTo(t.position);
		}
		DestroyTheOrb();
	}

	public void DestroyTheOrb()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		Physics.OverlapSphereNonAlloc(t.position, 6f, colliders, 1024);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				colliders[i].GetComponent<IKickable<Vector3>>().Kick(t.position.DirTo(colliders[i].transform.position).With(null, 0f));
				colliders[i] = null;
			}
		}
		CameraController.shake.Shake();
		Game.player.sway.Sway(2f, 0f, 5f, 3f);
		Game.mission.SetState(2);
		QuickEffectsPool.Get("Orb Explosion", t.position, Quaternion.identity).Play();
		particle.Stop();
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		t.localPosition = new Vector3(0f, 3f + Mathf.Sin(Time.time) * 0.5f, 0f);
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case 9:
		{
			Vector3 vector = t.position.DirTo(Game.player.t.position);
			vector.y += 0.5f;
			vector.Normalize();
			Game.player.Damage(vector);
			Game.player.rb.AddForce(vector * 10f, ForceMode.Impulse);
			Game.player.airControlBlock = 0.2f;
			QuickEffectsPool.Get("Orb Hit", Game.player.t.position, Quaternion.LookRotation(vector)).Play();
			break;
		}
		case 10:
			dmgInfo.dir = t.position.DirTo(other.bounds.center);
			other.GetComponent<IDamageable<DamageData>>().Damage(dmgInfo);
			break;
		case 14:
			other.attachedRigidbody.velocity = t.position.DirTo(other.bounds.center) * 10f;
			DestroyTheOrb();
			break;
		}
	}
}
