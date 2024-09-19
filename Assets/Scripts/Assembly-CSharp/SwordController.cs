using System;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : WeaponController
{
	private const float _SLASH_MIN_HOLDING = 0.3f;

	private const float _SLAMSLASH_MIN_HOLDING = 0.2f;

	public TrailScript trailSlideSlash;

	public TrailScript trail;

	public AnimationCurve damageCurve;

	[Header("Prefabs")]
	public GameObject _pooledGroundpound;

	public GameObject _pooledPhantomslash;

	[Header("DamageTypes")]
	public DamageType dmg_Slash;

	public DamageType dmg_SlamSlash;

	public DamageType dmg_SlideSlash;

	public DamageType dmg_Throw;

	public DamageType dmg_Block;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip sfxSlash;

	public AudioClip sfxSlamslash;

	public AudioClip sfxSlamSlashCharged;

	public AudioClip sfxSwitch;

	public AudioClip sfxSlashCharge;

	public AudioClip sfxSlamslashCharge;

	public AudioClip sfxBlock;

	public AudioClip sfxThrow;

	private bool flippedSlash;

	private float flippedSlashTimer;

	private float cooldown;

	private int poundTargetsCount;

	private Vector3 slashBounds = new Vector3(2f, 0.5f, 2.5f);

	private Vector3 slideSlashBounds = new Vector3(2.5f, 0.5f, 4f);

	private Vector3 slamSlashBounds = new Vector3(0.6f, 4f, 4f);

	private List<BaseEnemy> groundPoundTargets = new List<BaseEnemy>(5);

	private void OnEnable()
	{
		trail.Reset();
		int num2 = (base.attackIndex = 0);
		float num4 = (base.holding = (flippedSlashTimer = num2));
		cooldown = num4;
		flippedSlash = false;
		if (!objWeapon.activeInHierarchy)
		{
			objWeapon.SetActive(value: true);
		}
		player.sway.Sway(0f, 0f, -5f, 2f);
	}

	protected override void Awake()
	{
		base.Awake();
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Grounded));
	}

	private void OnDestroy()
	{
		Grounder grounder = player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Grounded));
	}

	private void GroundPoundDetection()
	{
		poundTargetsCount = 0;
		CrowdControl.instance.GetClosestEnemies(groundPoundTargets, player.tHead.position, player.tHead.forward.With(null, 0f).normalized, 24f);
		if (groundPoundTargets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < groundPoundTargets.Count; i++)
		{
			if (groundPoundTargets[i].isActiveAndEnabled && groundPoundTargets[i].agent.enabled)
			{
				(QuickPool.instance.Get(_pooledGroundpound, groundPoundTargets[i].GetActualPosition()) as GroundPound).Setup(groundPoundTargets[i]);
				poundTargetsCount++;
			}
		}
		groundPoundTargets.Clear();
	}

	private void Grounded()
	{
		if (base.isActiveAndEnabled && base.animator.GetInteger("Attack Index") == 2 && base.attackState > 0)
		{
			base.animator.SetTrigger("Release");
			cooldown = 0.1f;
			if (player.parkourActionsCount > 0)
			{
				GroundPoundDetection();
			}
		}
	}

	public void Charge()
	{
		switch (base.attackIndex)
		{
		case 0:
		case 1:
		case 4:
			source.PlayClip(sfxSlashCharge);
			player.sway.Sway(0f, 0f, flippedSlash ? 5 : (-5), 2f);
			break;
		case 2:
			source.PlayClip(sfxSlamslashCharge);
			player.sway.Sway(-6f, 0f, 0f, 2f);
			break;
		case 3:
			break;
		}
	}

	public void Sway()
	{
		switch (base.attackIndex)
		{
		case 0:
		case 1:
			source.PlayClip(sfxSlash);
			player.sway.Sway(0f, Mathf.Lerp(10f, 30f, base.holding) * (float)((base.attackIndex == 0) ? 1 : (-1)), 0f, 5f);
			trail.t.SetPositionAndRotation(player.tHead.position - player.tHead.up * 0.2f, Quaternion.LookRotation(player.tHead.forward, manager.t.up * ((base.attackIndex != 0) ? 1 : (-1))));
			trail.gameObject.SetActive(value: false);
			trail.gameObject.SetActive(value: true);
			trail.SetColor(manager.color.Evaluate(base.holding));
			break;
		case 4:
			source.PlayClip(sfxSlash);
			player.sway.Sway(0f, Mathf.Lerp(10f, 30f, base.holding), 0f, 5f);
			trailSlideSlash.t.SetPositionAndRotation(player.tHead.position + player.tHead.forward, Quaternion.LookRotation(player.tHead.forward));
			trailSlideSlash.gameObject.SetActive(value: false);
			trailSlideSlash.gameObject.SetActive(value: true);
			trail.SetColor(manager.color.Evaluate(base.holding));
			break;
		case 2:
			source.PlayClip(sfxSlamslash);
			player.sway.Sway(Mathf.Lerp(10f, 30f, base.holding), 0f, 0f, 5f);
			trail.t.SetPositionAndRotation(player.tHead.position, Quaternion.LookRotation(player.tHead.forward, -manager.t.right));
			trail.gameObject.SetActive(value: true);
			trail.SetColor(manager.color.Evaluate(base.holding));
			break;
		case 3:
			player.sway.Sway(10f, 0f, 0f, 5f);
			Game.sounds.PlayClip(sfxThrow);
			break;
		}
	}

	private void WallSlashCheck(Vector3 dir)
	{
		dir = Quaternion.AngleAxis(40f, player.tHead.up) * dir;
		for (int i = 0; i < 3; i++)
		{
			Debug.DrawRay(player.tHead.position, dir * 3f, Color.red, 2f);
			if (Physics.Raycast(player.tHead.position, dir, out hit, 2.5f, 1))
			{
				QuickEffectsPool.Get("Slash Hit B", hit.point, Quaternion.LookRotation(player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1)))).Play(base.holding);
				break;
			}
			dir = Quaternion.AngleAxis(-40f, player.tHead.up) * dir;
		}
	}

	public void Strike()
	{
		Vector3 vector = player.tHead.forward;
		manager.Blink(0f);
		switch (base.attackIndex)
		{
		case 0:
		case 1:
			base.attackState = 0;
			damage.newType = dmg_Slash;
			damage.dir = ((player.tHead.forward + player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1))) / 2f).normalized;
			damage.amount = (int)Mathf.Lerp(30f, 65f, damageCurve.Evaluate(base.holding / 0.7f));
			damage.knockdown = base.holding == 1f;
			damage.stylePoint = null;
			if (Slash2(slashBounds))
			{
				QuickEffectsPool.Get("Slash Hit", player.tHead.position + player.tHead.forward * 1.25f, Quaternion.LookRotation(player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1)))).Play(base.holding);
			}
			else
			{
				WallSlashCheck(vector);
			}
			flippedSlash = !flippedSlash;
			if (flippedSlash)
			{
				flippedSlashTimer = 1f;
			}
			break;
		case 4:
			base.attackState = 0;
			damage.newType = dmg_SlideSlash;
			damage.dir = Vector3.up;
			damage.amount = (int)Mathf.Lerp(30f, 65f, damageCurve.Evaluate(base.holding / 0.7f));
			damage.knockdown = true;
			damage.stylePoint = Game.style.SwordSlideSlash;
			if (Slash2(slideSlashBounds))
			{
				Game.time.SlowMotion(0.25f, 0.2f, 0.1f);
				QuickEffectsPool.Get("Slash Hit", player.tHead.position + player.tHead.forward * 1.25f, Quaternion.LookRotation(-player.tHead.right)).Play(base.holding);
			}
			else
			{
				WallSlashCheck(vector);
			}
			break;
		case 2:
		{
			base.attackState = 0;
			damage.newType = dmg_SlamSlash;
			damage.dir = ((player.tHead.forward + player.tHead.up / 2f) / 2f).normalized;
			damage.amount = (int)Mathf.Lerp(50f, 125f, base.holding / 0.8f);
			damage.stylePoint = null;
			damage.knockdown = base.holding / 0.8f > 0.25f;
			if (poundTargetsCount > 0)
			{
				Game.time.SlowMotion(0.3f, 0.3f);
			}
			poundTargetsCount = 0;
			bool flag = Slash2(slamSlashBounds);
			CameraController.shake.Shake((!flag) ? 1 : 2);
			QuickEffectsPool.Get(flag ? "Slash Hit" : "Slash Hit B", player.t.position + player.tHead.forward * 2f, Quaternion.LookRotation(player.tHead.forward.With(null, 0f))).Play(base.holding);
			break;
		}
		case 3:
			base.attackState++;
			if (Game.player.KickHolded())
			{
				manager.kickController.QuickKick();
			}
			if (player.slide.slideState != 0)
			{
				vector = CrowdControl.instance.GetClosestDirectionToNormal(player.tHead.position, vector, 7.5f);
			}
			(QuickPool.instance.Get(data.prefabThrowed, player.tHead.position, Quaternion.LookRotation(vector)) as ThrowedWeapon).dmg.amount = 80 + ((!Game.player.grounder.grounded) ? 20 : 0) + Game.player.parkourActionsCount * 20;
			objWeapon.SetActive(value: false);
			break;
		}
		base.holding = 0f;
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
		switch (base.attackState)
		{
		case 0:
			if (!objWeapon.activeInHierarchy)
			{
				break;
			}
			if (Game.player.AttackPressed())
			{
				if (player.grounder.grounded)
				{
					if (player.slide.isSliding)
					{
						ChargeAttackWithIndex(4);
					}
					else
					{
						ChargeAttackWithIndex(flippedSlash ? 1 : 0);
					}
				}
				else
				{
					ChargeAttackWithIndex(2);
				}
				base.attackState = 1;
			}
			else if (Game.player.AltPressed())
			{
				if (!CheckDaggerTargets())
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
			}
			else if (flippedSlashTimer > 0f)
			{
				flippedSlashTimer -= Time.deltaTime;
			}
			else if (flippedSlash)
			{
				flippedSlash = false;
			}
			break;
		case 1:
			base.holding = Mathf.MoveTowards(base.holding, 1f, Time.deltaTime * (float)((!player.slide.isSliding) ? 1 : 2));
			switch (base.attackIndex)
			{
			case 0:
			case 1:
				if (Game.player.JumpPressed())
				{
					source.PlayClip(sfxSwitch);
					ChargeAttackWithIndex(2);
				}
				if (player.slide.isSliding)
				{
					ChargeAttackWithIndex(4, base.holding);
				}
				if (!Game.player.AttackHolded() && base.holding > 0.3f)
				{
					base.animator.SetTrigger("Release");
					base.attackState++;
				}
				break;
			case 2:
				if (Game.player.AttackReleased())
				{
					player.rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
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
				if ((base.holding == 1f || !CheckDaggerTargets()) && !Game.player.AltHolded() && base.holding > 0.2f)
				{
					base.animator.SetTrigger("Release");
					Game.time.StopSlowmo();
					base.attackState++;
				}
				break;
			case 4:
				if (!player.slide.isSliding)
				{
					ChargeAttackWithIndex(flippedSlash ? 1 : 0, base.holding);
				}
				if (!Game.player.AttackHolded() && base.holding > 0.3f)
				{
					base.animator.SetTrigger("Release");
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
