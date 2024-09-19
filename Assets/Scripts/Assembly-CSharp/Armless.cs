using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Flammable))]
[RequireComponent(typeof(StateMachine))]
public class Armless : BaseEnemy
{
	private float fireTimer;

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
				typeof(EnemyActionState),
				new EnemyActionState(this)
			},
			{
				typeof(EnemyDashState),
				new EnemyDashState(this)
			}
		};
		base.stateMachine.SetStates(states);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		actionTime = 0.75f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		if (index == 0)
		{
			QuickEffectsPool.Get("Armless Jump", base.t.position).Play();
			CameraController.shake.Shake(2);
			DeactivateEnemy();
		}
	}

	public override void Stun()
	{
		base.Stun();
		ActionStateWithTrigger("Stun", 3.5f);
	}

	public override void RunningUpdate()
	{
		base.RunningUpdate();
		float sqrMagnitude = base.agent.velocity.sqrMagnitude;
		if (sqrMagnitude > 4f)
		{
			base.animator.speed = 0.1f + sqrMagnitude / (base.agent.speed * base.agent.speed);
			base.t.rotation = Quaternion.Slerp(base.t.rotation, Quaternion.LookRotation(base.agent.velocity.normalized), Time.deltaTime * base.agent.speed);
		}
		else
		{
			base.animator.speed = Mathf.Lerp(base.animator.speed, 1f, Time.deltaTime);
		}
	}

	public override void Damage(DamageData damage)
	{
		if (ActionStateWithAnim("Block", 1.75f))
		{
			QuickEffectsPool.Get("Block", base.clldr.bounds.center, base.t.rotation).Play();
			CameraController.shake.Shake(1);
		}
	}

	public override void FollowBreak()
	{
		base.FollowBreak();
		if ((bool)tTarget)
		{
			float num = tTarget.position.y - 1f - base.t.position.y;
			if (num < 0f && num >= -14f)
			{
				JumpOff();
			}
		}
	}

	private void JumpOff()
	{
		Vector3 normalized = base.t.position.DirTo(tTarget.position).With(null, 0f).normalized;
		Physics.Raycast(base.t.position + normalized * 4f, Vector3.down, out hit, 18f, 1);
		if (hit.distance != 0f)
		{
			Kick((normalized + Vector3.down / 2f) / 2f);
		}
	}

	public void Leave()
	{
		base.animator.Play("PreJump", -1, 0f);
	}

	private void Update()
	{
		if (!tTarget)
		{
			return;
		}
		UpdateTargetDistances();
		if (buffed)
		{
			UpdateReposition();
		}
		if (Vector3.Angle(base.t.forward, base.t.position.DirTo(tTarget.position.With(null, base.t.position.y))) > 85f)
		{
			float num = Vector3.Dot(base.t.right, base.t.position.DirTo(tTarget.position.With(null, base.t.position.y))).Sign();
			ActionStateWithAnim((num < 0f) ? "Turn Left" : "Turn Right");
			base.t.Rotate(0f, 90f * num, 0f);
			actionTime = 0.5f;
		}
		targetPosition = base.t.position;
		if (!base.stateMachine.CurrentIs(typeof(EnemyActionState)) && base.rb.isKinematic && base.agent.enabled && base.dist < 3f && CrowdControl.instance.GetToken(this))
		{
			base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			ActionStateWithTrigger("Attack", 1.5f);
			PlaySound(sounds.Attack);
			if ((bool)tHead)
			{
				QuickEffectsPool.Get("Warning", tHead.position, Quaternion.LookRotation(base.t.forward)).Play();
			}
			else
			{
				QuickEffectsPool.Get("Warning", base.clldr.bounds.center, Quaternion.LookRotation(base.t.forward)).Play();
			}
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
