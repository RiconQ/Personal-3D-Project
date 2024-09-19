using System;
using System.Collections.Generic;
using UnityEngine;

public class Archer : BaseEnemy
{
	public GameObject _prefabArrowMine;

	public GameObject projectile;

	private int sideRunSign = 1;

	private float timer;

	private float altFireTimer;

	private float kickTimer;

	private float strafeTimer;

	private bool lookForTarget;

	protected override void Awake()
	{
		base.Awake();
		base.agent.updateRotation = false;
		Dictionary<Type, BaseState> states = new Dictionary<Type, BaseState>
		{
			{
				typeof(EnemyIdleState),
				new EnemyIdleState(this)
			},
			{
				typeof(EnemyFollowState),
				new EnemyFollowState(this)
			},
			{
				typeof(EnemyJumpState),
				new EnemyJumpState(this)
			},
			{
				typeof(EnemyActionState),
				new EnemyActionState(this)
			},
			{
				typeof(EnemyDashState),
				new EnemyDashState(this)
			}
		};
		base.stateMachine.SetStates(states);
		WeaponController.OnRangedAttack = (Action)Delegate.Combine(WeaponController.OnRangedAttack, new Action(Strafe));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WeaponController.OnRangedAttack = (Action)Delegate.Remove(WeaponController.OnRangedAttack, new Action(Strafe));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		strafeTimer = 0f;
		altFireTimer = UnityEngine.Random.Range(0.2f, 0.4f);
		timer = UnityEngine.Random.Range(0.35f, 1.75f);
		actionTime = 0.5f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
		targetPosition = base.t.position;
	}

	public override void RunningUpdate()
	{
		float sqrMagnitude = base.agent.velocity.sqrMagnitude;
		if (sqrMagnitude > 1f)
		{
			base.animator.speed = 0.1f + sqrMagnitude / (base.agent.speed * base.agent.speed);
			base.t.rotation = Quaternion.Slerp(base.t.rotation, Quaternion.LookRotation(base.agent.velocity.normalized), Time.deltaTime * base.agent.speed);
		}
		else
		{
			base.animator.speed = Mathf.Lerp(base.animator.speed, 1f, Time.deltaTime);
			base.t.rotation = Quaternion.Slerp(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(tTarget.position).With(null, 0f)), Time.deltaTime * base.agent.speed);
		}
	}

	public override void Damage(DamageData damage)
	{
		base.Damage(damage);
		timer -= 0.3f;
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		switch (index)
		{
		case 0:
			Fire();
			break;
		case 1:
			Warp();
			break;
		case 2:
			projectile = QuickPool.instance.Get(_prefabArrowMine, hit.point).gameObject;
			break;
		case 3:
			if (Physics.CheckSphere(base.t.position + base.t.up + base.t.forward, 2f, 512))
			{
				Game.player.grounder.Ungrounded(forced: true);
				Vector3 vector = base.t.position.DirToXZ(tTarget.position);
				vector.y += 1f;
				vector.Normalize();
				Game.player.airControlBlock = 0.2f;
				Game.player.rb.velocity = Vector3.zero;
				Game.player.rb.AddForce(vector * 22f, ForceMode.Impulse);
				Game.player.sway.Sway(5f, 0f, 5f, 4f);
				CameraController.shake.Shake(1);
				QuickEffectsPool.Get("ArcherKick", base.t.position + base.t.up + base.t.forward, Quaternion.LookRotation(base.t.forward)).Play();
			}
			else
			{
				CheckViewPoints();
			}
			break;
		case 4:
			CheckViewPoints();
			break;
		}
	}

	private void Fire()
	{
		if (!tTarget)
		{
			return;
		}
		if (Vector3.Angle(base.t.forward, base.t.position.DirTo(tTarget.position)) < 90f)
		{
			UpdateTargetDistances();
			if (KickRule())
			{
				Kick();
				return;
			}
			base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			QuickPool.instance.Get("Projectile", base.t.position + Vector3.up * 1.5f + base.t.forward, Quaternion.LookRotation((base.t.position + Vector3.up * 1.5f).DirTo(tTarget.position)));
		}
		else
		{
			repositionTimer *= 0.25f;
		}
	}

	private bool KickRule()
	{
		if (base.dist < 4f && !Game.player.rb.isKinematic && Game.player.grounder.grounded)
		{
			return kickTimer == 0f;
		}
		return false;
	}

	private void Kick()
	{
		kickTimer = UnityEngine.Random.Range(1.5f, 2.5f);
		base.t.rotation = Quaternion.LookRotation(base.t.position.DirToXZ(tTarget.position), Vector3.up);
		base.animator.Play("Kick", -1, 0f);
		actionTime = 0.5f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	private void Strafe()
	{
		if (buffed && (bool)tTarget && !(strafeTimer > 0f))
		{
			strafeTimer = 0.2f + Mathf.Sin(Time.timeSinceLevelLoad + base.t.position.x + base.t.position.z) * 0.1f;
		}
	}

	private void Strafing()
	{
		if (base.agent.enabled && (bool)tTarget && !(PlayerController.instance.ViewAngle(base.t.position) > 30f))
		{
			int num = -PlayerController.instance.tHead.forward.SideFromDirection(GetTargetDirGrounded(), Vector3.up);
			base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			if (CheckWarpPos(base.t.position - base.t.right * (num * 4)))
			{
				QuickEffectsPool.Get("Strafe", base.t.position, Quaternion.LookRotation(base.t.right * -num)).Play();
				base.animator.SetTrigger((num == 1) ? "Strafe Right" : "Strafe Left");
				actionTime = 1f;
				base.stateMachine.SwitchState(typeof(EnemyActionState));
				targetPosition = base.agent.nextPosition;
				base.isStrafing = 1f;
			}
		}
	}

	private void Update()
	{
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || CheckForOffMeshLinks())
		{
			return;
		}
		UpdateTargetDistances();
		if (strafeTimer != 0f && !strafeTimer.MoveTowards(0f))
		{
			Strafing();
		}
		if (buffed)
		{
			if ((bool)projectile && !projectile.activeInHierarchy)
			{
				projectile = null;
			}
			if (altFireTimer != 0f)
			{
				altFireTimer = Mathf.MoveTowards(altFireTimer, 0f, Time.deltaTime);
			}
			else
			{
				if (base.dist > 12f && !projectile && Game.player.grounder.grounded && Game.player.ViewAngle(base.t.position) < 60f)
				{
					Physics.Raycast(Vector3.Lerp(Game.player.t.position, GetActualPosition(), 0.5f), Vector3.down, out hit, 6f, 1);
					if (hit.distance != 0f && !Physics.CheckSphere(hit.point, 2f, 1024) && !Physics.CheckSphere(hit.point + Vector3.up * 2f, 0.9f, 1))
					{
						Warning();
						base.t.LookAt(tTarget.position.With(null, base.t.position.y));
						ActionStateWithAnim("Alt Attack");
						targetPosition = base.t.position;
					}
				}
				altFireTimer = UnityEngine.Random.Range(0.2f, 0.4f);
			}
		}
		if (kickTimer != 0f)
		{
			kickTimer = Mathf.MoveTowards(kickTimer, 0f, Time.deltaTime);
		}
		else if (KickRule())
		{
			Kick();
		}
		if (Game.player.grounder.grounded && !Game.player.rb.isKinematic)
		{
			if (!lookForTarget)
			{
				if (base.distGrounded > 24f)
				{
					targetPosition = tTarget.position;
				}
				else if (base.distGrounded < 18f && base.distVertical < 2f && Game.player.grounder.grounded)
				{
					targetPosition = base.t.position + Vector3.Cross(tTarget.position.DirTo(base.t.position), Vector3.up) * 4f * sideRunSign;
				}
				else
				{
					targetPosition = base.t.position;
				}
			}
			else
			{
				targetPosition = tTarget.position;
			}
		}
		if (timer >= 0f)
		{
			timer -= Time.deltaTime * (float)((!(base.distGrounded > 16f)) ? 1 : 2);
			return;
		}
		if (!base.stateMachine.CurrentIs(typeof(EnemyJumpState)) && !base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			if (base.distVertical > 8f && Game.player.grounder.grounded && base.distGrounded < 18f)
			{
				lookForTarget = true;
			}
			else
			{
				Physics.Raycast(GetActualPosition(), GetActualPosition().DirTo(Game.player.tHead.position), out hit, 22f, 513);
				if (hit.distance != 0f && hit.collider.gameObject.layer == 9)
				{
					if (CrowdControl.instance.GetToken(this))
					{
						Warning();
						base.t.LookAt(tTarget.position.With(null, base.t.position.y));
						ActionStateWithAnim("Attack");
						targetPosition = base.t.position;
					}
					lookForTarget = false;
				}
				else if (hit.distance != 0f && CheckViewPoints())
				{
					lookForTarget = false;
				}
				else
				{
					lookForTarget = true;
				}
			}
		}
		timer = UnityEngine.Random.Range(0.35f, 1.75f);
		sideRunSign *= UnityEngine.Random.Range(-1f, 1f).Sign();
	}
}
