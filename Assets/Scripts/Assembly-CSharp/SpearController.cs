using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearController : WeaponController
{
	private const float _SLASH_MIN_HOLDING = 0.3f;

	private const float _SLAMSLASH_MIN_HOLDING = 0.2f;

	public TrailScript trail;

	public AnimationCurve damageCurve;

	public GameObject _pooledSpearLaunch;

	[Header("DamageTypes")]
	public DamageType dmgt_Sweep;

	public DamageType dmgt_Launch;

	public DamageType dmgt_Dash;

	public DamageType dmgt_Throw;

	public DamageType dmgt_Block;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip sfxSlash;

	public AudioClip sfxSlamslash;

	public AudioClip sfxSwitch;

	public AudioClip sfxSlashCharge;

	public AudioClip sfxSlamslashCharge;

	public AudioClip sfxBlock;

	private float cooldown;

	private bool jumpStrikeUsed;

	private Vector3 slashBox = new Vector3(3f, 0.5f, 3.5f);

	private Vector3 slamSlashBox = new Vector3(1f, 1f, 4f);

	private BaseEnemy phantomSlashTarget;

	private List<BaseEnemy> groundPoundTargets = new List<BaseEnemy>(5);

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

	private void Grounded()
	{
		if (base.attackIndex == 2 && base.attackState == 1)
		{
			Game.time.StopSlowmo();
			source.PlayClip(sfxSwitch);
			ChargeAttackWithIndex(0);
		}
		jumpStrikeUsed = false;
	}

	private void OnEnable()
	{
		trail.Reset();
		int num2 = (base.attackIndex = 0);
		float num4 = (base.holding = num2);
		cooldown = num4;
		if (!objWeapon.activeInHierarchy)
		{
			objWeapon.SetActive(value: true);
		}
		player.sway.Sway(0f, 0f, 5f, 2f);
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
			source.PlayClip(sfxSlash);
			player.sway.Sway(0f, Mathf.Lerp(10f, 30f, base.holding), 0f, 4f);
			trail.t.SetPositionAndRotation(player.tHead.position - player.tHead.up * 0.2f, Quaternion.LookRotation(player.tHead.forward, manager.t.up * ((base.attackIndex != 0) ? 1 : (-1))));
			trail.gameObject.SetActive(value: false);
			trail.gameObject.SetActive(value: true);
			trail.SetColor(manager.color.Evaluate(base.holding));
			break;
		case 1:
			source.PlayClip(sfxSlash);
			player.sway.Sway(0f - Mathf.Lerp(10f, 20f, base.holding), 0f, 0f, 3f);
			break;
		case 2:
			source.PlayClip(sfxSlamslash);
			player.sway.Sway(Mathf.Lerp(10f, 30f, base.holding), 0f, 0f, 5f);
			trail.SetColor(manager.color.Evaluate(base.holding));
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
		{
			base.attackState = 0;
			damage.newType = dmgt_Sweep;
			damage.dir = (player.slide.isSliding ? Vector3.up : ((player.tHead.forward + player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1))) / 2f).normalized);
			damage.amount = (int)Mathf.Lerp(0f, 20f, damageCurve.Evaluate(base.holding / 0.7f));
			damage.knockdown = true;
			damage.stylePoint = (player.slide.isSliding ? Game.style.SwordSlideSlash : null);
			if (Slash2(slashBox, dirPerTarget: true))
			{
				QuickEffectsPool.Get("Slash Hit", player.tHead.position + player.tHead.forward * 1.25f, Quaternion.LookRotation(player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1)))).Play(base.holding);
				break;
			}
			vector = Quaternion.AngleAxis(40f, player.tHead.up) * vector;
			for (int i = 0; i < 3; i++)
			{
				Debug.DrawRay(player.tHead.position, vector * 3f, Color.red, 2f);
				if (Physics.Raycast(player.tHead.position, vector, out hit, 2.5f, 16385))
				{
					QuickEffectsPool.Get("Slash Hit B", hit.point, Quaternion.LookRotation(player.tHead.right * ((base.attackIndex == 0) ? 1 : (-1)))).Play(base.holding);
					break;
				}
				vector = Quaternion.AngleAxis(-40f, player.tHead.up) * vector;
			}
			break;
		}
		case 1:
			base.attackState = 0;
			QuickPool.instance.Get(_pooledSpearLaunch, player.t.position, Quaternion.LookRotation(Vector3.ProjectOnPlane(Game.player.tHead.forward, Game.player.grounder.gNormal)));
			break;
		case 2:
		{
			base.attackState = 0;
			damage.newType = dmgt_Dash;
			damage.dir = ((player.tHead.forward + player.tHead.up / 2f) / 2f).normalized;
			damage.amount = (int)Mathf.Lerp(20f, 85f, damageCurve.Evaluate(base.holding / 0.8f));
			damage.stylePoint = null;
			damage.knockdown = base.holding > 0.85f;
			bool flag = Slash2(slamSlashBox);
			CameraController.shake.Shake((!flag) ? 1 : 2);
			Physics.Raycast(player.tHead.position, player.tHead.forward, out hit, 8f, 1);
			if (hit.distance != 0f)
			{
				QuickEffectsPool.Get("Arrow Hit", hit.point, Quaternion.LookRotation(hit.normal)).Play();
				if (!flag)
				{
					player.rb.velocity = player.rb.velocity.With(null, 0f);
					player.rb.velocity *= 1.1f;
					player.rb.AddForce(hit.normal * 25f, ForceMode.Impulse);
					player.ParkourMove();
				}
			}
			break;
		}
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
			obj.dmg.amount = ((Game.player.parkourActionsCount > 0) ? 150 : 80);
			obj.charged = player.parkourActionsCount > 0;
			objWeapon.SetActive(value: false);
			break;
		}
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

	private IEnumerator Dashing()
	{
		Vector3 oldVel = player.rb.velocity;
		Vector3 aPos = player.t.position;
		Vector3 bPos = player.t.position + player.tHead.forward * 12f;
		player.SetKinematic(value: true);
		player.sway.Sway(0f, 0f, 6f, 3f);
		player.fov.kinematicFOV = 0f;
		player.headPosition.ChangeYPosition(0f);
		Physics.Raycast(player.t.position, player.tHead.forward, out var hit, 20f, 16384);
		if (CrowdControl.instance.GetClosestEnemyToNormal(player.t.position, player.tHead.forward, 30f, 20f, out var target))
		{
			bPos = target.GetActualPosition() + target.GetActualPosition().DirTo(player.t.position) * 2f;
			target.staggered = true;
		}
		else if (hit.distance != 0f)
		{
			bPos = hit.point + hit.normal;
		}
		if (hit.distance != 0f || target != null)
		{
			source.PlayClip(sfxSlamslashCharge);
			base.animator.Play("Dashing", -1, 0f);
			float timer = 0f;
			while (timer != 1f)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 8f);
				player.t.position = Vector3.LerpUnclamped(aPos, bPos, timer);
				player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * -5f);
				player.fov.kinematicFOV = Mathf.Sin(timer * (float)Math.PI) * 25f;
				yield return null;
			}
			if ((bool)target)
			{
				target.staggered = false;
			}
		}
		else
		{
			yield return null;
		}
		base.animator.SetTrigger("Release");
		player.SetKinematic(value: false);
		player.headPosition.ChangeYPosition(0.75f);
		if ((bool)target)
		{
			player.airControlBlock = 0.1f;
			player.rb.velocity = (-aPos.DirTo(bPos).With(null, 0f).normalized + Vector3.up * 2f).normalized * 20f;
		}
		else if (hit.distance != 0f)
		{
			player.airControlBlock = 0.1f;
			player.rb.velocity = Vector3.ProjectOnPlane(aPos.DirTo(bPos), hit.normal).normalized * 25f;
		}
		else
		{
			player.rb.velocity = oldVel;
		}
		CameraController.shake.Shake(1);
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
				if (player.grounder.grounded || jumpStrikeUsed)
				{
					ChargeAttackWithIndex(0);
				}
				else
				{
					ChargeAttackWithIndex(2);
					if (player.parkourActionsCount > 0)
					{
						Game.time.SlowMotion(0.1f, 0.5f, 0.2f);
					}
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
			base.holding = Mathf.MoveTowards(base.holding, 1f, Time.deltaTime * (float)((!player.slide.isSliding) ? 1 : 2));
			switch (base.attackIndex)
			{
			case 0:
				if (player.slide.isSliding)
				{
					ChargeAttackWithIndex(1);
				}
				if (Game.player.JumpPressed() && !jumpStrikeUsed)
				{
					source.PlayClip(sfxSwitch);
					ChargeAttackWithIndex(2);
				}
				if (!Game.player.AttackHolded() && base.holding > 0.5f)
				{
					Game.time.StopSlowmo();
					base.animator.SetTrigger("Release");
					base.attackState++;
				}
				break;
			case 1:
				if (Game.player.JumpPressed() && !jumpStrikeUsed)
				{
					source.PlayClip(sfxSwitch);
					ChargeAttackWithIndex(2);
				}
				if (!Game.player.AttackHolded() && base.holding > 0.3f)
				{
					Game.time.StopSlowmo();
					base.animator.SetTrigger("Release");
					base.attackState++;
				}
				break;
			case 2:
				if (!Game.player.AttackHolded() && base.holding > 0.1f)
				{
					Game.time.StopSlowmo();
					jumpStrikeUsed = true;
					StartCoroutine(Dashing());
					base.attackState++;
				}
				break;
			case 3:
				if (base.holding == 1f || !CheckDaggerTargets())
				{
					if (!Game.player.AltHolded() && base.holding > 0.2f)
					{
						base.animator.SetTrigger("Release");
						Game.time.StopSlowmo();
						base.attackState++;
					}
					if (player.SlidePressed())
					{
						base.animator.Play("Throw Boost", -1, 0f);
					}
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
