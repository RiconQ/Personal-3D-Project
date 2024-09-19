using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class Spider : BaseEnemy
{
	private float jumpTimer;

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
				typeof(EnemyJumpState),
				new EnemyJumpState(this)
			},
			{
				typeof(EnemyActionState),
				new EnemyActionState(this)
			}
		};
		base.stateMachine.SetStates(states);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		trail.Reset();
		actionTime = 0.3f;
		jumpTimer = UnityEngine.Random.Range(0.25f, 1.5f);
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		if (index == 0)
		{
			trail.Play();
			AttackWithTrailBounds(trail);
			PlaySound(sounds.Hit);
		}
	}

	public override void RunningUpdate()
	{
		base.RunningUpdate();
		float sqrMagnitude = base.agent.velocity.sqrMagnitude;
		if (sqrMagnitude > 0.25f)
		{
			base.animator.speed = Mathf.Clamp(sqrMagnitude / (base.agent.speed * base.agent.speed), 0.5f, 10f);
			base.t.rotation = Quaternion.Slerp(base.t.rotation, Quaternion.LookRotation(base.agent.velocity.normalized), Time.deltaTime * base.agent.speed * 2f);
		}
		else
		{
			base.t.rotation = Quaternion.Slerp(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(tTarget.position).With(null, 0f)), Time.deltaTime * base.agent.speed);
			base.animator.speed = Mathf.Lerp(base.animator.speed, 1f, Time.deltaTime);
		}
	}

	private void Update()
	{
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || CheckForOffMeshLinks())
		{
			return;
		}
		UpdateTargetDistances();
		if (Game.player.grounder.grounded && !Game.player.rb.isKinematic && Game.player.inputActive)
		{
			targetPosition = tTarget.position;
		}
		if (base.stateMachine.CurrentIs(typeof(EnemyActionState)) || !base.rb.isKinematic || !base.agent.enabled)
		{
			return;
		}
		jumpTimer = Mathf.MoveTowards(jumpTimer, 0f, Time.deltaTime);
		if (buffed && base.dist < 4f && Game.player.weapons.IsAttacking() && CheckJumpPosInDirection(ref targetPosition, -base.t.position.DirToXZ(tTarget.position), 10f, 1f, 8f))
		{
			base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			lockJumpRotation = true;
			base.stateMachine.SwitchState(typeof(EnemyJumpState));
		}
		if (base.dist < 3f)
		{
			if (CrowdControl.instance.GetToken(this))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				ActionStateWithAnim("Attack", 0.8f);
				PlaySound(sounds.Attack);
				QuickEffectsPool.Get("Warning", tHead.position, Quaternion.LookRotation(base.t.forward)).Play();
			}
		}
		else if (base.dist.InRange(7f, buffed ? 20 : 16) && jumpTimer == 0f && Game.player.grounder.grounded && CrowdControl.instance.GetToken(this))
		{
			TryDirectJump();
			jumpTimer = UnityEngine.Random.Range(0.25f, 1.5f);
		}
	}
}
