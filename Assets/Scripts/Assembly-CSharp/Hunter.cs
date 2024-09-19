using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class Hunter : BaseEnemy
{
	public GameObject ProjectilePrefab;

	private bool lookForTarget;

	private bool strafe;

	private int sign;

	private float jumpTimer;

	private float attackTimer;

	private TrailScript trail;

	protected override void Awake()
	{
		base.Awake();
		trail = GetComponentInChildren<TrailScript>(includeInactive: true);
		trail.gameObject.SetActive(value: true);
		base.agent.updateRotation = false;
		sign = UnityEngine.Random.Range(-1f, 1f).Sign();
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
				typeof(HunterEnemyState),
				new HunterEnemyState(this)
			}
		};
		base.stateMachine.SetStates(states);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		trail.Reset();
		actionTime = 0.3f;
		sign = UnityEngine.Random.Range(-1f, 1f).Sign();
		jumpTimer = UnityEngine.Random.Range(0.2f, 0.4f);
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
		case 2:
			if (base.distGrounded < 6f && CheckJumpPosInDirection(ref targetPosition, -base.t.position.DirToXZ(tTarget.position), 10f, 1f, 8f))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				lockJumpRotation = true;
				jumpTimer = UnityEngine.Random.Range(0.3f, 0.6f);
				base.stateMachine.SwitchState(typeof(EnemyJumpState));
			}
			else
			{
				Vector3 forward = (base.t.position + base.t.up).DirTo(tTarget.position);
				QuickPool.instance.Get(ProjectilePrefab, base.t.position + base.t.up, Quaternion.LookRotation(forward));
				PlaySound(sounds.Hit);
			}
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
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || base.stateMachine.CurrentIs(typeof(HunterEnemyState)) || CheckForOffMeshLinks())
		{
			return;
		}
		UpdateTargetDistances();
		targetPosition = tTarget.position + base.t.right * (Mathf.Sin((float)base.randomSeed + Time.timeSinceLevelLoad) * (base.distGrounded / 7f)) * 3f;
		jumpTimer = Mathf.MoveTowards(jumpTimer, 0f, Time.deltaTime);
		attackTimer = Mathf.MoveTowards(attackTimer, 0f, Time.deltaTime);
		if (base.dist.InRange(6f, 14f))
		{
			if (jumpTimer == 0f)
			{
				if (CheckSneakJumpPosition(out var pos, out var normal))
				{
					targetPosition = pos;
					targetNormal = normal;
					base.stateMachine.SwitchState(typeof(HunterEnemyState));
					jumpTimer = 3f;
				}
				else
				{
					jumpTimer = 1f;
				}
			}
		}
		else if (base.dist < 6f)
		{
			if (jumpTimer > 0.4f)
			{
				jumpTimer = 0.4f;
			}
			if (Game.player.weapons.IsAttacking() && jumpTimer == 0f && CheckJumpPosInDirection(ref targetPosition, -base.t.position.DirToXZ(tTarget.position), 10f, 1f, 8f))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				lockJumpRotation = true;
				jumpTimer = UnityEngine.Random.Range(0.2f, 0.4f);
				base.stateMachine.SwitchState(typeof(EnemyJumpState));
			}
			else if (base.dist < 3f && Vector3.Angle(base.t.forward, base.t.position.DirTo(tTarget.position)) < 90f)
			{
				ActionStateWithAnim("Bite");
			}
		}
	}

	public bool CheckSneakJumpPosition(out Vector3 pos, out Vector3 normal)
	{
		pos = (normal = Vector3.zero);
		Vector3 vector = (-base.t.forward + base.t.up * UnityEngine.Random.Range(0.25f, 1f)).normalized;
		for (int i = 0; i < 3; i++)
		{
			vector = Quaternion.Euler(0f, 120f, 0f) * vector;
			Debug.DrawRay(GetActualPosition() + Vector3.up, vector, Color.yellow, 2f);
			if (Physics.SphereCast(GetActualPosition() + Vector3.up, 0.5f, vector, out hit, 14f, 147457) && hit.distance > 3f && hit.collider.gameObject.layer == 0 && hit.normal.y.InRange(-0.5f, 0.5f))
			{
				Physics.Raycast(hit.point + hit.normal, (hit.point + hit.normal).DirTo(tTarget.position), out var hitInfo, 30f, 513);
				if (hitInfo.distance != 0f && hitInfo.collider.gameObject.layer == 9)
				{
					Debug.DrawLine(GetActualPosition(), hit.point, Color.blue, 2f);
					pos = hit.point;
					normal = hit.normal;
					return true;
				}
			}
		}
		return false;
	}
}
