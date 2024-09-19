using System;
using UnityEngine;

public class BowController : WeaponController
{
	private const float _SHOT_MIN_HOLDING = 0.3f;

	private const float _APEXSHOT_MIN_HOLDING = 0.1f;

	private const float _QUICKSHOT_MIN_HOLDING = 0.1f;

	public GameObject objTrigger;

	public LayerMask quickshotMask;

	private int quickshotCount;

	[Header("DamageTypes")]
	public DamageType dmg_Shot;

	public DamageType dmg_Fireshot;

	public DamageType dmg_Quickshot;

	public DamageType dmg_Powershot;

	[Header("Prefabs")]
	public GameObject _pooledArrow;

	public GameObject _pooledQuickshotTrail;

	public GameObject _pooledSkyshot;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip sfxShot;

	public AudioClip sfxApexshot;

	public AudioClip sfxCharge;

	public AudioClip sfxThrowCharge;

	public AudioClip sfxSwitch;

	public AudioClip sfxQuickshot;

	public AudioClip sfxQuickshotCharge;

	public static float accuracy { get; private set; }

	public bool apexShot { get; private set; }

	public int arrowType { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		objTrigger.SetActive(value: false);
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Grounded));
		PlayerController.OnEnemyJump = (Action<BaseEnemy>)Delegate.Combine(PlayerController.OnEnemyJump, new Action<BaseEnemy>(EnemyJump));
		PlayerController.OnParkourMove = (Action)Delegate.Combine(PlayerController.OnParkourMove, new Action(OnParkour));
	}

	public override bool DamageReaction()
	{
		if (base.DamageReaction())
		{
			Invoke("DropBow", 0.1f);
			return true;
		}
		return false;
	}

	private void DropBow()
	{
		manager.DropCurrentWeapon(player.tHead.forward);
	}

	private void OnDestroy()
	{
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Grounded));
		PlayerController.OnEnemyJump = (Action<BaseEnemy>)Delegate.Remove(PlayerController.OnEnemyJump, new Action<BaseEnemy>(EnemyJump));
		PlayerController.OnParkourMove = (Action)Delegate.Remove(PlayerController.OnParkourMove, new Action(OnParkour));
	}

	private void EnemyJump(BaseEnemy e)
	{
		if (base.gameObject.activeInHierarchy && base.attackState == 0 && !manager.voidshotTargets.Contains(e))
		{
			base.animator.Play("Voidshot", -1, 0f);
			(QuickPool.instance.Get(_pooledSkyshot, base.t.position) as SkyShot).Setup(e);
			manager.voidshotTargets.Add(e);
		}
	}

	private void Grounded()
	{
		if (base.isActiveAndEnabled)
		{
			Game.time.StopSlowmo();
			base.midAirAction = false;
			apexShot = false;
		}
	}

	private void OnParkour()
	{
		apexShot = false;
	}

	private void OnEnable()
	{
		if (base.attackState != 0)
		{
			base.attackState = 0;
		}
		arrowType = 0;
		accuracy = 80f;
		if (!objWeapon.activeInHierarchy)
		{
			objWeapon.SetActive(value: true);
		}
		player.sway.Sway(0f, -5f, 0f, 2f);
	}

	public void SetArrowtype(int i = 0)
	{
		if (arrowType != i)
		{
			arrowType = i;
		}
	}

	public void Sway()
	{
		switch (base.attackIndex)
		{
		case 0:
			if (arrowType == 2 && (bool)BubbleTrap.lastEnabled)
			{
				BubbleTrap.lastEnabled.Explode();
			}
			break;
		case 1:
			player.sway.Sway(0f, 0f, 5f, 3f);
			break;
		case 2:
			player.sway.Sway(0f, 0f, 5f, 3f);
			break;
		case 3:
			player.sway.Sway(0f, 0f, 5f, 4f);
			break;
		}
	}

	public void PowerShot(Vector3 pos, Vector3 dir, float angle = 15f, StylePoint stylePoint = null)
	{
		dir = (damage.dir = CrowdControl.instance.GetClosestDirectionToNormal(pos, dir, angle, 30f));
		Physics.Raycast(pos, dir, out hit, 30f, quickshotMask);
		if (hit.distance != 0f)
		{
			if (hit.collider.gameObject.layer != 0)
			{
				damage.amount = Mathf.Lerp(60f, 100f, base.holding);
				damage.dir = dir;
				damage.newType = dmg_Powershot;
				damage.knockdown = true;
				damage.stylePoint = stylePoint;
				hit.collider.GetComponent<IDamageable<DamageData>>().Damage(damage);
				Debug.DrawLine(pos, hit.point, Color.blue, 2f);
				if (hit.collider.gameObject.layer == 10)
				{
					CameraController.shake.Shake(2);
				}
				source.PlayClip(sfxApexshot);
				QuickEffectsPool.Get("Arrow Hit", hit.point, Quaternion.LookRotation(hit.normal)).Play();
				(QuickPool.instance.Get(_pooledQuickshotTrail, pos) as ShotTrail).Setup(hit.point);
			}
			else
			{
				QuickEffectsPool.Get((arrowType == 0) ? "Arrow Hit" : "Arrow Hit Fire", hit.point, Quaternion.LookRotation(hit.normal)).Play(5f);
				(QuickPool.instance.Get(_pooledQuickshotTrail, player.tHead) as ShotTrail).Setup(hit.point);
			}
		}
		else
		{
			(QuickPool.instance.Get(_pooledQuickshotTrail, pos) as ShotTrail).Setup(pos + dir * 24f);
		}
	}

	public void Strike()
	{
		manager.Blink(0f);
		switch (base.attackIndex)
		{
		case 0:
			base.attackState = 0;
			base.animator.SetInteger("Attack Index", -1);
			player.sway.Sway(-3f, 0f, 0f, 5f);
			source.PlayClip(sfxShot);
			switch (arrowType)
			{
			case 0:
				damage.newType = dmg_Shot;
				damage.knockdown = base.holding == 1f;
				break;
			case 1:
				damage.newType = dmg_Fireshot;
				damage.knockdown = true;
				break;
			}
			damage.amount = (int)Mathf.Lerp(10f, 50f, base.holding / 0.7f);
			(QuickPool.instance.Get(_pooledArrow, player.tHead) as ArrowFlying).Setup(damage, arrowType);
			accuracy = Mathf.Clamp(accuracy - 10f, 10f, 80f);
			arrowType = 0;
			break;
		case 2:
			base.attackState = 0;
			base.animator.SetInteger("Attack Index", -1);
			player.sway.Sway(-3f, 0f, 0f, 5f);
			PowerShot(player.tHead.position, player.tHead.forward);
			base.midAirAction = false;
			break;
		case 1:
		{
			quickshotCount++;
			if (!player.grounder.grounded && player.airControlBlock == 0f)
			{
				Vector3 velocity = player.rb.velocity;
				velocity.y = Mathf.Clamp(velocity.y, 10f, float.PositiveInfinity);
				player.rb.velocity = velocity;
			}
			player.sway.Sway(0f, 0f, -2.5f, 4f);
			Vector3 vector = (damage.dir = CrowdControl.instance.GetClosestDirectionToNormal(player.tHead.position, player.tHead.forward, 20f));
			Physics.Raycast(player.tHead.position, vector, out hit, 20f, quickshotMask);
			if (hit.distance != 0f)
			{
				if (hit.collider.gameObject.layer != 0)
				{
					damage.amount = 20f;
					damage.dir = vector;
					damage.newType = dmg_Quickshot;
					damage.knockdown = quickshotCount == 3;
					hit.collider.GetComponent<IDamageable<DamageData>>().Damage(damage);
					if (quickshotCount == 3)
					{
						Game.time.SlowMotion(0.25f, 0.2f, 0.1f);
					}
				}
				QuickEffectsPool.Get((arrowType == 0) ? "Arrow Hit" : "Arrow Hit Fire", hit.point, Quaternion.LookRotation(hit.normal)).Play(5f);
				(QuickPool.instance.Get(_pooledQuickshotTrail, player.tHead) as ShotTrail).Setup(hit.point);
			}
			else
			{
				(QuickPool.instance.Get(_pooledQuickshotTrail, player.tHead) as ShotTrail).Setup(player.tHead.position + vector * 20f);
				source.PlayClip(sfxQuickshot);
			}
			if (quickshotCount == 3)
			{
				base.attackState = 0;
			}
			break;
		}
		case 3:
		{
			Vector3 vector = player.tHead.forward;
			if (player.slide.slideState != 0)
			{
				vector = CrowdControl.instance.GetClosestDirectionToNormal(player.tHead.position, vector, 7.5f);
			}
			(QuickPool.instance.Get(data.prefabThrowed, player.tHead.position, Quaternion.LookRotation(vector)) as ThrowedWeapon).dmg.amount = 40 + ((!Game.player.grounder.grounded) ? 20 : 0) + Game.player.parkourActionsCount * 20;
			objWeapon.SetActive(value: false);
			base.attackState++;
			break;
		}
		}
		base.holding = 0f;
	}

	private void ChargeAttack(int i)
	{
		base.attackIndex = i;
		base.animator.SetInteger("Attack Index", base.attackIndex);
		base.animator.SetTrigger("Charge");
		base.attackState = 1;
		objTrigger.SetActive(value: true);
		if (WeaponController.OnCharge != null)
		{
			WeaponController.OnCharge();
		}
	}

	private void ReleaseAttack()
	{
		base.animator.SetTrigger("Release");
		base.attackState = 2;
		objTrigger.SetActive(value: false);
	}

	public override void Tick()
	{
		switch (base.attackState)
		{
		case 0:
			accuracy = Mathf.MoveTowards(accuracy, 80f, Time.deltaTime * 40f);
			if (Game.player.AttackPressed())
			{
				if (player.slide.slideState > 0)
				{
					arrowType = 0;
					ChargeAttack(1);
					quickshotCount = 0;
					source.PlayClip(sfxQuickshotCharge);
					break;
				}
				if (player.parkourActionsCount > 0 && !apexShot)
				{
					ChargeAttack(2);
					base.midAirAction = true;
					Game.time.SlowMotion(0.1f, 0.5f, 0.2f);
				}
				else
				{
					ChargeAttack(0);
				}
				arrowType = 0;
				source.PlayClip(sfxCharge);
				if (WeaponController.OnRangedAttack != null)
				{
					WeaponController.OnRangedAttack();
				}
			}
			else if (Game.player.AltPressed() && !CheckDaggerTargets())
			{
				ChargeAttack(3);
				source.PlayClip(sfxThrowCharge);
			}
			break;
		case 1:
			base.holding = Mathf.MoveTowards(base.holding, 1f, Time.deltaTime * (float)((!player.slide.isSliding) ? 1 : 2));
			switch (base.attackIndex)
			{
			case 0:
				if (!Game.player.AttackHolded() && base.holding > 0.3f)
				{
					Game.time.StopSlowmo();
					ReleaseAttack();
				}
				if (player.slide.slideState != 0 && arrowType == 0)
				{
					ChargeAttack(1);
					source.PlayClip(sfxSwitch);
					player.sway.Sway(-2.5f, 0f, 0f, 4f);
				}
				break;
			case 1:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
					Game.time.StopSlowmo();
					quickshotCount = 0;
					ReleaseAttack();
				}
				break;
			case 2:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
					Game.time.StopSlowmo();
					ReleaseAttack();
				}
				if (player.grounder.grounded)
				{
					ChargeAttack(0);
					source.PlayClip(sfxSwitch);
					player.sway.Sway(-2.5f, 0f, 0f, 4f);
				}
				break;
			case 3:
				if ((base.holding == 1f || !CheckDaggerTargets()) && !Game.player.AltHolded() && base.holding > 0.2f)
				{
					ReleaseAttack();
				}
				break;
			case 99:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
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
