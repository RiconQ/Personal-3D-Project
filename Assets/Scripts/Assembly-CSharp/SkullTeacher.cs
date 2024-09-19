using System;
using UnityEngine;

public class SkullTeacher : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	public bool kick;

	public bool damageThreshold;

	public float minDamage;

	public float maxDamage = 100f;

	public DamageType damageType;

	public AudioClip sfxHit;

	private Transform t;

	private Transform tMesh;

	private Rigidbody rb;

	private Animator animator;

	public void Damage(DamageData damage)
	{
		if (!kick && damage.newType == damageType && (!damageThreshold || (damage.amount > minDamage && damage.amount < maxDamage)))
		{
			QuickEffectsPool.Get("Orb Explosion", t.position, t.rotation).Play();
			base.gameObject.SetActive(value: false);
			return;
		}
		animator.SetTrigger("Damage");
		rb.AddForce(damage.dir * 10f, ForceMode.Impulse);
		if (damage.amount > 0f)
		{
			QuickEffectsPool.Get("Poof", t.position, t.rotation).Play();
		}
		Game.sounds.PlayClip(sfxHit);
	}

	public void Kick(Vector3 dir)
	{
		if (kick)
		{
			QuickEffectsPool.Get("Orb Explosion", t.position, t.rotation).Play();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			animator.SetTrigger("Damage");
			rb.AddForce(dir * 10f, ForceMode.Impulse);
			Game.sounds.PlayClip(sfxHit);
		}
	}

	private void Awake()
	{
		t = base.transform;
		tMesh = t.Find("Mesh").transform;
		rb = GetComponentInChildren<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void LateUpdate()
	{
		if ((bool)PlayerController.instance)
		{
			t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(t.position.DirTo(PlayerController.instance.t.position)), Time.deltaTime * 4f);
		}
	}
}
