using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class Dummy : BaseEnemy
{
	private bool strafe;

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
		jumpTimer = UnityEngine.Random.Range(0f, 1f);
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void Stun()
	{
		base.Stun();
		base.animator.SetTrigger("Stun");
		actionTime = 3.5f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		switch (index)
		{
		case 0:
			trail.Play();
			AttackWithTrailBounds(trail);
			PlaySound(sounds.Hit);
			break;
		case 1:
			Warp();
			break;
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
		if ((bool)tTarget && base.agent.enabled && !base.stateMachine.CurrentIs(typeof(EnemyActionState)) && !CheckForOffMeshLinks())
		{
			targetPosition = tTarget.position;
			UpdateTargetDistances();
			if (base.dist < 3f && CrowdControl.instance.GetToken(this))
			{
				PlaySound(sounds.Attack);
				base.animator.SetTrigger("Attack");
				actionTime = 1f;
				base.stateMachine.SwitchState(typeof(EnemyActionState));
			}
		}
	}
}
