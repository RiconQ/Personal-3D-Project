using System;
using UnityEngine;

public class ShieldController : WeaponController
{
	public static Action OnPush = delegate
	{
	};

	private const float _SLASH_MIN_HOLDING = 0.1f;

	private const float _SLAMSLASH_MIN_HOLDING = 0.2f;

	private const float _THROW_MIN_HOLDING = 0.3f;

	public TrailScript trail;

	public AnimationCurve damageCurve;

	public Transform tFireslideFX;

	public Vector3 absorbBounds = new Vector3(2f, 0.5f, 2.5f);

	[Header("Prefabs")]
	public GameObject _pooledGroundpound;

	public GameObject _pooledPhantomslash;

	public GameObject _pooledPush;

	public GameObject shieldA;

	public GameObject shieldB;

	public DamageType stuggerType;

	public bool blocked;

	public Collider playerTrigger;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip sfxSlash;

	public AudioClip sfxSlamslash;

	public AudioClip sfxSwitch;

	public AudioClip sfxSlashCharge;

	public AudioClip sfxSlamslashCharge;

	public AudioClip sfxBlock;

	public AudioClip sfxWallhit;

	private float cooldown;

	private int blockedCount;

	private void OnEnable()
	{
		trail.Reset();
		if (!objWeapon.activeInHierarchy)
		{
			objWeapon.SetActive(value: true);
		}
		player.sway.Sway(0f, 0f, -5f, 2f);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		playerTrigger.enabled = false;
		tFireslideFX.gameObject.SetActive(value: false);
	}

	protected override void Awake()
	{
		base.Awake();
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Grounded));
		PlayerController.OnParkourMove = (Action)Delegate.Combine(PlayerController.OnParkourMove, new Action(OnParkour));
	}

	private void OnDestroy()
	{
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Grounded));
		PlayerController.OnParkourMove = (Action)Delegate.Remove(PlayerController.OnParkourMove, new Action(OnParkour));
	}

	private void OnWallHit(ContactPoint cp)
	{
		if (base.attackState == 1 && base.attackIndex == 0 && !player.grounder.grounded)
		{
			blocked = true;
			blockedCount++;
			base.animator.Play("Absorb Hit", -1, 0f);
			Game.sounds.PlayClip(sfxWallhit);
			Vector3 vector = Vector3.Reflect(player.oldVel.With(null, 0f).normalized, cp.normal);
			player.rb.velocity = (vector + Vector3.up * 2f).normalized * 22f;
			player.airControlBlock = 0.2f;
			CameraController.shake.Shake(2);
			player.sway.Sway(4f, 0f, 8f, 4f);
		}
	}

	private void OnParkour()
	{
		if (base.attackState == 1 && base.attackIndex != 2)
		{
			ChargeAttackWithIndex(2);
		}
	}

	private void OnSlideBash()
	{
		if (base.attackState == 1 && (base.attackIndex == 0 || base.attackIndex == 1))
		{
			blocked = true;
			blockedCount++;
			base.animator.Play("Absorb Hit", -1, 0f);
		}
	}

	public override bool DamageReaction()
	{
		if (player.AttackHolded() && base.attackState == 1 && base.attackIndex == 0)
		{
			blocked = true;
			blockedCount++;
			base.animator.Play("Absorb Hit", -1, 0f);
			Game.sounds.PlayClip(sfxSlamslashCharge);
			if ((bool)CrowdControl.lastAttacked && base.holding < 1f)
			{
				damage.dir = (player.t.position.DirTo(CrowdControl.lastAttacked.t.position.With(null, player.t.position.y)) + Vector3.up / 2f).normalized;
				damage.amount = 0f;
				damage.newType = block;
				damage.knockdown = true;
				damage.stylePoint = null;
				CrowdControl.lastAttacked.Damage(damage);
				ChargeAttackWithIndex(3);
			}
			CameraController.shake.Shake(2);
			if (blockedCount <= 3)
			{
				_ = base.holding;
				_ = 1f;
			}
			return true;
		}
		return false;
	}

	private void Grounded()
	{
		if (base.isActiveAndEnabled)
		{
			if (base.animator.GetInteger("Attack Index") == 2 && base.attackState > 0)
			{
				base.animator.SetTrigger("Release");
				cooldown = 0.1f;
			}
			if (base.attackIndex == 0)
			{
				_ = base.attackState;
				_ = 1;
			}
		}
	}

	public void Charge()
	{
		switch (base.attackIndex)
		{
		case 0:
		case 1:
			source.PlayClip(sfxSlashCharge);
			player.sway.Sway(0f, 0f, -5f, 2f);
			break;
		case 2:
			source.PlayClip(sfxSlamslashCharge);
			player.sway.Sway(-6f, 0f, 0f, 2f);
			break;
		}
	}

	public void Sway()
	{
		switch (base.attackIndex)
		{
		case 0:
		case 1:
			player.sway.Sway(0f, -20f, 5f, 3f);
			trail.t.SetPositionAndRotation(player.tHead.position - player.tHead.up * 0.2f, Quaternion.LookRotation(player.tHead.forward, manager.t.up));
			trail.gameObject.SetActive(value: false);
			trail.gameObject.SetActive(value: true);
			trail.SetColor(manager.color.Evaluate(base.holding));
			break;
		case 2:
			source.PlayClip(sfxSlamslash);
			player.sway.Sway(Mathf.Lerp(10f, 30f, base.holding), 0f, 0f, 5f);
			break;
		case 3:
			player.sway.Sway(10f, 0f, 0f, 5f);
			break;
		case 4:
			player.sway.Sway(0f, 0f, 5f, 4f);
			break;
		}
	}

	public void Strike()
	{
		Vector3 vector = player.tHead.forward;
		manager.Blink(0f);
		switch (base.attackIndex)
		{
		case 0:
			damage.newType = Game.style.basicBluntHit;
			damage.dir = Game.player.tHead.forward;
			damage.amount = 5f;
			damage.knockdown = false;
			damage.stylePoint = null;
			if (Slash2(absorbBounds))
			{
				QuickEffectsPool.Get("Slash Hit", player.tHead.position + player.tHead.forward * 1.25f, Quaternion.LookRotation(player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1)))).Play(base.holding);
			}
			base.attackState = 0;
			blocked = false;
			blockedCount = 0;
			break;
		case 1:
			base.attackState = 0;
			damage.newType = stuggerType;
			damage.dir = player.tHead.forward;
			damage.amount = 0f;
			damage.knockdown = false;
			damage.stylePoint = (player.slide.isSliding ? Game.style.SwordSlideSlash : null);
			break;
		case 2:
			base.attackState = 0;
			damage.dir = ((player.tHead.forward + player.tHead.up / 2f) / 2f).normalized;
			damage.amount = (int)Mathf.Lerp(45f, 85f, damageCurve.Evaluate(base.holding / 0.8f));
			damage.stylePoint = null;
			damage.knockdown = true;
			Physics.Raycast(base.t.position + player.tHead.forward.With(null, 0f).normalized, Vector3.down, out hit, 4f, 1);
			if (hit.distance != 0f)
			{
				QuickEffectsPool.Get("sh Trap", hit.point, Quaternion.LookRotation(hit.normal)).Play();
				QuickPool.instance.Get(_pooledPhantomslash, hit.point, Quaternion.LookRotation(Vector3.ProjectOnPlane(player.tHead.forward, hit.normal)));
				CameraController.shake.Shake(2);
			}
			break;
		case 3:
		{
			base.attackState++;
			if (Game.player.KickHolded())
			{
				manager.kickController.QuickKick();
			}
			if (player.slide.slideState != 0)
			{
				vector = CrowdControl.instance.GetClosestDirectionToNormal(player.tHead.position, vector, 7.5f);
			}
			ThrowedWeapon obj = QuickPool.instance.Get(data.prefabThrowed, player.tHead.position, Quaternion.LookRotation(vector)) as ThrowedWeapon;
			obj.dmg.amount = ((Game.player.parkourActionsCount > 0) ? 60 : 40);
			obj.charged = blocked;
			objWeapon.SetActive(value: false);
			blocked = false;
			break;
		}
		case 4:
			base.attackState = 0;
			damage.newType = stuggerType;
			damage.dir = player.tHead.forward;
			damage.amount = 0f;
			damage.knockdown = false;
			damage.stylePoint = (player.slide.isSliding ? Game.style.SwordSlideSlash : null);
			CameraController.shake.Shake(2);
			player.sway.Sway(0f, 10f, 0f, 4f);
			blocked = false;
			blockedCount = 0;
			break;
		}
		base.holding = 0f;
	}

	public void FinishAttack()
	{
		base.attackState = 0;
	}

	private void ChargeAttackWithIndex(int i, float newHolding = 0f)
	{
		base.attackIndex = i;
		base.animator.SetInteger("Attack Index", base.attackIndex);
		base.animator.SetTrigger("Charge");
		base.holding = newHolding;
		if (WeaponController.OnCharge != null)
		{
			WeaponController.OnCharge();
		}
	}

	public override void Tick()
	{
		cooldown.MoveTowards(0f);
		if (playerTrigger.enabled != ((base.attackIndex == 0) & (base.attackState == 1)))
		{
			playerTrigger.enabled = !playerTrigger.enabled;
		}
		if (Game.player.slide.isSliding)
		{
			tFireslideFX.rotation = Quaternion.LookRotation(player.gVel, player.grounder.gNormal);
		}
		if (base.attackState > 0 && (base.attackIndex == 1 || base.attackIndex == 2))
		{
			if (shieldA.activeInHierarchy)
			{
				shieldA.SetActive(value: false);
			}
			if (!shieldB.activeInHierarchy)
			{
				shieldB.SetActive(value: true);
			}
		}
		else
		{
			if (shieldB.activeInHierarchy)
			{
				shieldB.SetActive(value: false);
			}
			if (!shieldA.activeInHierarchy)
			{
				shieldA.SetActive(value: true);
			}
		}
		switch (base.attackState)
		{
		case 0:
			if (!objWeapon.activeInHierarchy)
			{
				break;
			}
			if (Game.player.AttackPressed())
			{
				if (player.grounder.grounded || player.parkourActionsCount == 0)
				{
					ChargeAttackWithIndex(0);
				}
				else
				{
					ChargeAttackWithIndex(2);
				}
				base.attackState = 1;
			}
			else if (Game.player.AltPressed() && !CheckDaggerTargets())
			{
				ChargeAttackWithIndex(3);
				player.sway.Sway(0f, 0f, 5f, 3f);
				base.attackState = 1;
				source.PlayClip(sfxSlamslashCharge);
				if (WeaponController.OnRangedAttack != null)
				{
					WeaponController.OnRangedAttack();
				}
			}
			break;
		case 1:
			base.holding = Mathf.MoveTowards(base.holding, 1f, Time.deltaTime * 2f);
			switch (base.attackIndex)
			{
			case 0:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
					base.animator.Play("Absorb", -1, 0f);
					base.attackState++;
				}
				break;
			case 4:
				if (!Game.player.AttackHolded())
				{
					base.animator.SetTrigger("Release");
					base.attackState++;
				}
				break;
			case 1:
				if (!Game.player.slide.isSliding)
				{
					ChargeAttackWithIndex(0);
					tFireslideFX.gameObject.SetActive(value: false);
				}
				if (!Game.player.AttackHolded())
				{
					_ = base.holding;
					_ = 0.1f;
				}
				break;
			case 2:
				Game.player.AltPressed();
				if (Game.player.AttackReleased())
				{
					player.rb.velocity *= 0.5f;
					player.rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
					base.animator.Play("Slamslash Fall", -1, 0f);
					base.attackState++;
				}
				else if (Game.player.grounder.grounded)
				{
					base.animator.SetTrigger("Release");
					base.attackState++;
				}
				break;
			case 3:
				if ((base.holding == 1f || !CheckDaggerTargets()) && !Game.player.AltHolded() && !Game.player.AttackHolded() && base.holding > 0.3f)
				{
					base.animator.SetTrigger("Release");
					Game.time.StopSlowmo();
					base.attackState++;
				}
				break;
			case 99:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
					player.sway.Sway(0f, 0f, -3f, 2.5f);
					source.PlayClip(sfxBlock);
					base.animator.SetTrigger("Release");
					Game.time.StopSlowmo();
					base.attackState = 0;
				}
				break;
			}
			break;
		case 2:
			break;
		}
	}
}
