using System;
using System.Collections.Generic;
using UnityEngine;

public class Shieldman : BaseEnemy
{
	private float strafeTimer;

	private float fireTimer;

	private float attackTimer;

	private TrailScript trail;

	protected override void Awake()
	{
		base.Awake();
		trail = GetComponentInChildren<TrailScript>();
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
				typeof(EnemyActionState),
				new EnemyActionState(this)
			},
			{
				typeof(EnemyDashState),
				new EnemyDashState(this)
			},
			{
				typeof(EnemyJumpState),
				new EnemyJumpState(this)
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

	private void Strafe()
	{
		if ((bool)tTarget && !base.stateMachine.CurrentIs(typeof(EnemyActionState)) && !base.stateMachine.CurrentIs(typeof(EnemyJumpState)) && !(Game.player.ViewAnglePlane(base.t.position) > 60f) && base.agent.enabled)
		{
			CheckViewPoints();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		trail.Reset();
		strafeTimer = 0f;
		actionTime = 0.75f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		switch (index)
		{
		case 0:
			base.animator.speed = 0f;
			break;
		case 1:
			UpdateTargetDirections();
			if (base.dist > 4f)
			{
				Physics.Raycast(tTarget.position - targetDirGrounded * 2f, Vector3.down, out hit, 10f, 1);
				if (hit.distance != 0f && CheckWarpPos(hit.point))
				{
					targetPosition = hit.point;
					base.stateMachine.SwitchState(typeof(EnemyDashState));
				}
			}
			else
			{
				base.animator.Play("Slash", -1, 0f);
				trail.Play();
				AttackWithTrailBounds(trail);
				PlaySound(sounds.Hit);
			}
			break;
		case 2:
			if (Physics.CheckSphere(base.t.position + base.t.up + base.t.forward * 2f, 2.5f, 512))
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
			break;
		}
	}

	public override void Stun()
	{
		base.Stun();
		ActionStateWithTrigger("Stun", 3.5f);
	}

	public override void OnDashed()
	{
		base.OnDashed();
		if (!base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			actionTime = 0.5f;
			base.stateMachine.SwitchState(typeof(EnemyActionState));
			trail.Play();
			AttackWithTrailBounds(trail);
			PlaySound(sounds.Hurt);
		}
	}

	public override void OnLanded()
	{
		base.OnLanded();
		strafeTimer = 0f;
	}

	private void Update()
	{
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || base.stateMachine.CurrentIs(typeof(EnemyJumpState)) || CheckForOffMeshLinks())
		{
			return;
		}
		UpdateTargetDistances();
		if (buffed)
		{
			UpdateReposition();
		}
		targetPosition = tTarget.position + base.t.right * (Mathf.Sin((float)base.randomSeed + Time.timeSinceLevelLoad) * (base.distGrounded / 12f)) * 4f;
		if (strafeTimer > 0f)
		{
			strafeTimer = Mathf.MoveTowards(strafeTimer, 0f, Time.deltaTime);
			if (strafeTimer == 0f)
			{
				Vector3 vector = targetDirGrounded;
				vector = Quaternion.Euler(0f, 60 * MyRandom.Sign(), 0f) * vector;
				if (CheckJumpPosInDirection(ref targetPosition, vector, 12f))
				{
					base.stateMachine.SwitchState(typeof(EnemyJumpState));
				}
			}
		}
		if (attackTimer != 0f)
		{
			attackTimer = Mathf.MoveTowards(attackTimer, 0f, Time.deltaTime);
		}
		if (attackTimer != 0f)
		{
			return;
		}
		if (base.dist < 4f)
		{
			if (UnityEngine.Random.Range(0f, 1f) < 0.6f && CheckJumpPosInDirection(ref targetPosition, -base.t.position.DirToXZ(tTarget.position), 10f, 1f, 8f))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				lockJumpRotation = true;
				base.stateMachine.SwitchState(typeof(EnemyJumpState));
				attackTimer = UnityEngine.Random.Range(0.25f, 0.75f);
			}
			else
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				ActionStateWithAnim("Push");
				attackTimer = UnityEngine.Random.Range(0.25f, 0.6f);
			}
		}
		else
		{
			if (!(base.dist < 16f) || !(base.distVertical < 5f) || !Game.player.grounder.grounded)
			{
				return;
			}
			Physics.Raycast(tTarget.position - targetDirGrounded * 2f, Vector3.down, out hit, 10f, 1);
			if (hit.distance != 0f && CheckWarpPos(hit.point) && CrowdControl.instance.GetToken(this))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				ActionStateWithTrigger("Attack", 1.5f);
				PlaySound(sounds.Spawn);
				if ((bool)tHead)
				{
					QuickEffectsPool.Get("Warning", tHead.position, Quaternion.LookRotation(base.t.forward)).Play();
				}
				else
				{
					QuickEffectsPool.Get("Warning", base.clldr.bounds.center, Quaternion.LookRotation(base.t.forward)).Play();
				}
			}
			attackTimer = UnityEngine.Random.Range(0.5f, 1f);
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		if (c.gameObject.layer == 14 && (bool)tTarget && !base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			actionTime = 1f;
			base.animator.SetTrigger("Stun");
			base.stateMachine.SwitchState(typeof(EnemyActionState));
		}
	}
}
