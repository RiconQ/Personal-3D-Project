using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
	[Header("Components")]
	public Transform t;

	public Transform tPivot;

	public PlayerController p;

	public KickController kickController;

	public DaggerController daggerController;

	public WeaponController[] weaponControllers;

	[Header("Other")]
	public Material playerMat;

	public Gradient color;

	[Header("Damage Types")]
	public DamageType unarmedEnemyJump;

	public DamageType unarmedEnemyKnockedJump;

	public DamageType basicBurn;

	public DamageType basicBluntHit;

	public DamageType basicBodyHit;

	public StylePoint kickBounce;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip pick;

	public AudioClip liftSounds;

	public AudioClip sfxPickObject;

	public AudioClip sfxThrowLifted;

	public AudioClip sfxDropObject;

	private bool isHolding;

	private int liftCheckCount;

	private float blink;

	private float liftCheckTimer;

	private float liftTimer;

	private float liftCooldown;

	private Transform tLifted;

	private Vector3 weaponPos;

	private Vector3 offset;

	private DamageData dmgInfo = new DamageData();

	private Collider[] colliders = new Collider[1];

	public List<BaseEnemy> voidshotTargets = new List<BaseEnemy>(5);

	public int currentWeapon { get; private set; }

	public int hashedWeapon { get; private set; }

	public Rigidbody rbLifted { get; private set; }

	private void Awake()
	{
		int num2 = (hashedWeapon = -1);
		currentWeapon = num2;
		for (int i = 0; i < weaponControllers.Length; i++)
		{
			weaponControllers[i].gameObject.SetActive(value: false);
		}
		Deactivate();
		PlayerSlide.OnSlideStart = (Action<Vector3>)Delegate.Combine(PlayerSlide.OnSlideStart, new Action<Vector3>(OnSlide));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		Grounder grounder = p.grounder;
		grounder.OnFakeGrounded = (Action)Delegate.Combine(grounder.OnFakeGrounded, new Action(OnGrounded));
	}

	private void OnDestroy()
	{
		PlayerSlide.OnSlideStart = (Action<Vector3>)Delegate.Remove(PlayerSlide.OnSlideStart, new Action<Vector3>(OnSlide));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		Grounder grounder = p.grounder;
		grounder.OnFakeGrounded = (Action)Delegate.Remove(grounder.OnFakeGrounded, new Action(OnGrounded));
	}

	private void Reset()
	{
		hashedWeapon = -1;
	}

	private void OnSlide(Vector3 pos)
	{
		if (currentWeapon != -1 && weaponControllers[currentWeapon].attackState == 0)
		{
			weaponControllers[currentWeapon].animator.Play("Slide", -1, 0f);
		}
	}

	private void OnGrounded()
	{
		if (currentWeapon != -1)
		{
			_ = weaponControllers[currentWeapon].attackState;
		}
	}

	public void OnKickHolded()
	{
		if (currentWeapon != -1)
		{
			weaponControllers[currentWeapon].animator.Play("Kick Holded", -1, 0f);
		}
	}

	public void OnKickReleased()
	{
		if (currentWeapon != -1)
		{
			weaponControllers[currentWeapon].animator.Play("Kick Released", -1, 0f);
		}
	}

	private void BlinkUpdate()
	{
		if (blink > 0f)
		{
			blink -= Time.deltaTime * 4f;
		}
		else
		{
			blink = 0f;
		}
		playerMat.SetFloat("_Blink", blink * 2f);
	}

	public void Blink(float value = 1f)
	{
		blink = value;
		playerMat.SetFloat("_Blink", blink * 2f);
	}

	public void ThrowLifted()
	{
		rbLifted.gameObject.layer = 14;
		rbLifted.isKinematic = false;
		rbLifted.AddForce(p.tHead.forward * Mathf.Lerp(20f, 60f, liftTimer), ForceMode.Impulse);
		rbLifted.AddTorque(p.tHead.right * (180f * liftTimer), ForceMode.VelocityChange);
		rbLifted.GetComponent<BreakableB>().damage.stylePoint = (p.grounder.grounded ? StyleData.instance.ObjectThrowed : StyleData.instance.ObjectThrowed);
		rbLifted = null;
		tLifted = null;
		liftTimer = 0f;
		liftCooldown = 0.3f;
		p.sway.Sway(10f, 0f, 0f, 2.5f);
		source.PlayClip(sfxThrowLifted);
		if (currentWeapon > -1)
		{
			weaponControllers[currentWeapon].gameObject.SetActive(value: true);
		}
		Game.time.StopSlowmo();
	}

	public bool DropLifted(bool andBreak = false)
	{
		if (!rbLifted)
		{
			return false;
		}
		rbLifted.isKinematic = false;
		rbLifted.gameObject.layer = 14;
		if (!andBreak)
		{
			rbLifted.AddForce(p.tHead.forward * 5f, ForceMode.Impulse);
			rbLifted.AddTorque(rbLifted.transform.right, ForceMode.Impulse);
		}
		else
		{
			rbLifted.GetComponent<BreakableB>().Break(p.tHead.forward);
		}
		rbLifted = null;
		tLifted = null;
		liftTimer = 0f;
		liftCooldown = 0.3f;
		p.sway.Sway(-5f, 0f, 5f, 4f);
		source.PlayClip(sfxDropObject);
		if (currentWeapon > -1)
		{
			weaponControllers[currentWeapon].gameObject.SetActive(value: true);
		}
		return true;
	}

	public void PickWeapon(int index)
	{
		if ((bool)rbLifted)
		{
			DropLifted();
		}
		if (currentWeapon > -1)
		{
			QuickPool.instance.Get(weaponControllers[currentWeapon].name, p.tHead.position + p.tHead.forward).rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
		}
		currentWeapon = index;
		Refresh();
		source.PlayClip(pick);
	}

	private void Update()
	{
		if (Game.debug && Input.GetKeyDown(KeyCode.X) && !IsAttacking())
		{
			currentWeapon = currentWeapon.Next(2);
			Refresh();
		}
		kickController.Tick();
		if (p.dash.isDashing)
		{
			if ((bool)tLifted)
			{
				DropLifted();
			}
			return;
		}
		if (currentWeapon > -1)
		{
			weaponPos.x = 0f;
			weaponPos.y = Mathf.Lerp(weaponPos.y, -0.2f, Time.deltaTime * 2f);
			weaponPos.z = Mathf.Lerp(weaponPos.z, kickController.isCharging ? (-0.1f) : 0f, Time.deltaTime * 4f);
			weaponControllers[currentWeapon].t.localPosition = weaponPos;
			weaponControllers[currentWeapon].Tick();
		}
		else
		{
			weaponPos.x = (weaponPos.z = 0f);
			weaponPos.y = -0.2f;
			daggerController.Tick();
		}
		liftCooldown = Mathf.MoveTowards(liftCooldown, 0f, Time.deltaTime);
		if ((bool)tLifted)
		{
			if (p.AttackHolded())
			{
				liftTimer = Mathf.MoveTowards(liftTimer, 1f, Time.deltaTime * 1.75f);
				tPivot.localPosition = tPivot.localPosition.With(null, Mathf.Lerp(0.25f, 0.65f, liftTimer));
			}
			if (liftTimer != 0f && !p.AttackHolded())
			{
				ThrowLifted();
			}
		}
		else if (liftCooldown == 0f)
		{
			if (!isHolding)
			{
				if (daggerController.state != 1 && Game.player.AttackPressed() && !TryToLift())
				{
					isHolding = true;
					liftCheckTimer = 0f;
					liftCheckCount = 0;
				}
			}
			else
			{
				liftCheckTimer = Mathf.MoveTowards(liftCheckTimer, 0.2f, Time.deltaTime);
				if (liftCheckTimer == 0.2f)
				{
					if (TryToLift())
					{
						isHolding = false;
					}
					else
					{
						liftCheckCount++;
					}
				}
				if (!Game.player.AttackHolded() || liftCheckCount > 20 || IsAttacking())
				{
					isHolding = false;
				}
			}
		}
		if (blink != 0f)
		{
			BlinkUpdate();
		}
	}

	private void LateUpdate()
	{
		if ((bool)tLifted)
		{
			tLifted.SetPositionAndRotation(tPivot.position + tPivot.TransformDirection(offset), Quaternion.LookRotation(p.tHead.forward));
		}
	}

	public void SwitchFromDagger()
	{
		if (hashedWeapon != -1)
		{
			SetWeapon(hashedWeapon);
			hashedWeapon = -1;
		}
	}

	public void SwitchToDagger(BaseBreakable target)
	{
		hashedWeapon = currentWeapon;
		SetWeapon(-1);
		daggerController.ThrowDagger(target);
	}

	public void DropCurrentWeapon(Vector3 dir)
	{
		if (currentWeapon != -1)
		{
			((PooledWeapon)QuickPool.instance.Get(weaponControllers[currentWeapon].name, p.tHead.position, Quaternion.LookRotation(p.tHead.right))).Drop((dir + Vector3.up / 4f).normalized * 14f, -1f);
			currentWeapon = -1;
			Refresh();
		}
	}

	public bool ReadyToLift()
	{
		if (!rbLifted)
		{
			return isHolding;
		}
		return false;
	}

	public void Lift(Collider c)
	{
		if (currentWeapon > -1)
		{
			weaponControllers[currentWeapon].gameObject.SetActive(value: false);
		}
		QuickEffectsPool.Get("Poof", c.attachedRigidbody.worldCenterOfMass).Play();
		p.sway.Sway(-5f, 0f, 2f, 3f);
		c.gameObject.layer = 11;
		rbLifted = c.attachedRigidbody;
		rbLifted.isKinematic = true;
		tLifted = rbLifted.transform;
		offset = tLifted.GetComponent<MeshFilter>().sharedMesh.bounds.extents;
		offset.x = 0f;
		offset.z *= -1f;
		isHolding = false;
		liftTimer = 0f;
		liftCheckCount = 4;
		tPivot.localPosition = tPivot.localPosition.With(null, 0.25f);
		if (!Game.player.grounder.grounded)
		{
			Game.time.SlowMotion(0.75f, 0.6f, 0.05f);
		}
		else
		{
			Game.player.rb.AddForce(Vector3.ProjectOnPlane(Game.player.tHead.forward, Game.player.grounder.gNormal) * 10f);
		}
		source.PlayClip(liftSounds);
	}

	private bool TryToLift()
	{
		colliders[0] = null;
		Physics.OverlapCapsuleNonAlloc(p.tHead.position, p.tHead.position + p.tHead.forward * 2f, 0.7f, colliders, 16384);
		if ((bool)colliders[0])
		{
			switch (colliders[0].gameObject.layer)
			{
			case 14:
				if (!colliders[0].attachedRigidbody.isKinematic && !colliders[0].CompareTag("Target"))
				{
					Lift(colliders[0]);
					return true;
				}
				return false;
			case 13:
				if (currentWeapon == -1 && !colliders[0].isTrigger)
				{
					colliders[0].GetComponent<IInteractable>().Interact();
					return true;
				}
				return false;
			default:
				return false;
			}
		}
		return false;
	}

	public float KickingOrHolding()
	{
		if ((bool)tLifted)
		{
			return Mathf.Lerp(rbLifted.mass, rbLifted.mass * 0.2f, liftTimer);
		}
		if (kickController.isCharging)
		{
			return p.grounder.grounded ? 3 : 0;
		}
		return 0f;
	}

	public float Holding()
	{
		if (currentWeapon <= -1)
		{
			return liftTimer;
		}
		return weaponControllers[currentWeapon].holding;
	}

	public bool IsAttacking()
	{
		if (currentWeapon >= 0)
		{
			return weaponControllers[currentWeapon].attackState != 0;
		}
		return (bool)tLifted | (daggerController.state != 0);
	}

	public void SetWeapon(int i)
	{
		currentWeapon = i;
		Refresh();
	}

	public bool Pick(int index)
	{
		if (currentWeapon != index)
		{
			currentWeapon = index;
			Refresh();
			return true;
		}
		return false;
	}

	public bool ReactToDamage()
	{
		if (currentWeapon != -1)
		{
			return weaponControllers[currentWeapon].DamageReaction();
		}
		return false;
	}

	public void Refresh()
	{
		daggerController.gameObject.SetActive(currentWeapon == -1);
		for (int i = 0; i < weaponControllers.Length; i++)
		{
			weaponControllers[i].gameObject.SetActive(i == currentWeapon);
		}
	}

	public void Deactivate()
	{
		for (int i = 0; i < weaponControllers.Length; i++)
		{
			weaponControllers[i].gameObject.SetActive(value: false);
		}
	}
}
