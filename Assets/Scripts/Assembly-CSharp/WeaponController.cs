using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
	public static Action OnCharge = delegate
	{
	};

	public static Action OnRangedAttack = delegate
	{
	};

	public Weapon data;

	public bool alwaysBlock;

	public GameObject objWeapon;

	public DamageType block;

	protected PlayerController player;

	protected PlayerWeapons manager;

	protected BaseBreakable daggerTarget;

	protected DamageData damage = new DamageData();

	protected RaycastHit hit;

	protected Collider[] colliders = new Collider[3];

	public Transform t { get; private set; }

	public bool midAirAction { get; protected set; }

	public int attackIndex { get; protected set; }

	public int attackState { get; protected set; }

	public float holding { get; protected set; }

	public Animator animator { get; private set; }

	private IEnumerator Parrying(BaseEnemy enemy)
	{
		yield return new WaitForSeconds(0.1f);
		if (player.AttackHolded())
		{
			damage.dir = player.tHead.forward;
			damage.amount = 0f;
			damage.newType = block;
			damage.knockdown = true;
			damage.stylePoint = null;
			enemy.Damage(damage);
		}
		else
		{
			manager.DropCurrentWeapon(Game.player.tHead.forward);
		}
	}

	public virtual bool DamageReaction()
	{
		if (player.AttackHolded() && attackState == 1 && (holding < 0.5f || attackIndex == 99 || alwaysBlock))
		{
			if (attackIndex != 99)
			{
				animator.SetTrigger("Damage");
				if ((bool)CrowdControl.lastAttacked)
				{
					StartCoroutine(Parrying(CrowdControl.lastAttacked));
				}
				holding = 0f;
				attackState = 1;
				attackIndex = 99;
				animator.SetInteger("Attack Index", attackIndex);
				CameraController.shake.Shake(2);
				return true;
			}
			attackState = 0;
			attackIndex = -1;
			animator.SetInteger("Attack Index", attackIndex);
			CameraController.shake.Shake(2);
			manager.DropCurrentWeapon(player.tHead.forward);
			return true;
		}
		return false;
	}

	public bool CheckDaggerTargets()
	{
		if (PullableControl.GetCurrent(out var transform))
		{
			manager.SwitchToDagger(transform.GetComponent<BaseBreakable>());
			return true;
		}
		return false;
	}

	public void Drop()
	{
		attackState = 0;
		manager.Pick(-1);
	}

	public void Stun(Vector3 slashBoxSize)
	{
		bool flag = false;
		Physics.OverlapBoxNonAlloc(player.tHead.position + player.tHead.forward * slashBoxSize.z / 2f, slashBoxSize, colliders, player.tHead.rotation, 17408);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				damage.dir = (player.tHead.forward.With(null, 0f) + Vector3.up).normalized;
				colliders[i].GetComponent<IDamageable<DamageData>>().Damage(damage);
				colliders[i] = null;
				flag = true;
				StyleRanking.instance.AddStylePoint(StylePointTypes.GroundPound);
			}
		}
	}

	public bool Slash2(Vector3 slashBoxSize, bool dirPerTarget = false)
	{
		bool result = false;
		Physics.OverlapBoxNonAlloc(player.tHead.position + player.tHead.forward * slashBoxSize.z / 2f, slashBoxSize, colliders, player.tHead.rotation, 17408);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				if (dirPerTarget)
				{
					damage.dir = player.tHead.position.DirTo(colliders[i].bounds.center);
				}
				colliders[i].GetComponent<IDamageable<DamageData>>().Damage(damage);
				colliders[i] = null;
				damage.amount *= 0.75f;
				result = true;
			}
		}
		return result;
	}

	public bool Slash3(Vector3 slashBoxSize)
	{
		bool result = false;
		Physics.OverlapBoxNonAlloc(player.tHead.position + player.tHead.forward * slashBoxSize.z / 2f, slashBoxSize, colliders, player.tHead.rotation, 17409);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (!(colliders[i] != null))
			{
				continue;
			}
			if (colliders[i].gameObject.layer != 0)
			{
				colliders[i].GetComponent<IDamageable<DamageData>>().Damage(damage);
				damage.amount *= 0.9f;
				if (colliders[i].gameObject.layer == 14)
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
			colliders[i] = null;
		}
		return result;
	}

	public abstract void Tick();

	protected virtual void Awake()
	{
		attackIndex = -1;
		attackState = 0;
		t = base.transform;
		animator = GetComponent<Animator>();
		player = GetComponentInParent<PlayerController>();
		manager = GetComponentInParent<PlayerWeapons>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	protected virtual void Reset()
	{
		attackIndex = -1;
		attackState = 0;
	}

	protected virtual void CancelAttack()
	{
		attackIndex = -1;
		animator.SetInteger("Attack Index", attackIndex);
		animator.SetTrigger("Cancel");
		attackState = 0;
	}

	protected virtual void OnDisable()
	{
		holding = 0f;
		attackState = 0;
		attackIndex = -1;
		animator.SetInteger("Attack Index", attackIndex);
	}
}
