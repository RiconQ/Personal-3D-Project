using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Deadman : BaseEnemy
{
	private float fireTimer;

	private TrailScript trail;

	public GameObject projectile;

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
				typeof(EnemyJumpState),
				new EnemyJumpState(this)
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
		trail.Reset();
		fireTimer = MyRandom.Range(0.5f, 1f);
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
			trail.Play();
			AttackWithTrailBounds(trail);
			PlaySound(sounds.Hit);
			break;
		case 2:
		{
			Collider[] array = new Collider[1];
			Physics.OverlapSphereNonAlloc(base.t.position + base.t.forward + base.t.up, 1.5f, array, 16384);
			if (array[0] != null && (bool)array[0].attachedRigidbody)
			{
				array[0].attachedRigidbody.AddForceAndTorque((base.t.forward + base.t.up).normalized * 25f, new Vector3(90f, 360f, 0f));
				QuickEffectsPool.Get("Poof", array[0].ClosestPoint(base.t.position));
			}
			break;
		}
		case 3:
			UpdateTargetDirections();
			if (Vector3.Angle(base.t.forward, targetDir) < attackCorrectionRadius)
			{
				base.t.rotation = Quaternion.LookRotation(targetDirGrounded, Vector3.up);
			}
			CameraController.shake.Shake(2);
			QuickPool.instance.Get(projectile, base.t);
			break;
		}
	}

	public override void Stun()
	{
		base.Stun();
		ActionStateWithTrigger("Stun", 3.5f);
	}

	public override void FollowBreak()
	{
		base.FollowBreak();
		if ((bool)tTarget)
		{
			UpdateTargetDirections();
			float num = tTarget.position.y - 1f - base.t.position.y;
			if (num < 0f && num >= -16f && CheckJumpPosInDirection(ref targetPosition, targetDirGrounded, 8f, 1f, 16f))
			{
				Debug.DrawRay(targetPosition, targetDirGrounded, Color.magenta, 2f);
				base.gameObject.SetActive(value: false);
				SpawnBody();
				base.body.PushBody(targetDirGrounded.normalized);
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

	private void ReposOld()
	{
		if (repositionTimer == 0f)
		{
			if (base.agent.enabled && !isTeleporting && !base.flammable.onFire && !base.stateMachine.CurrentIs(typeof(EnemyActionState)) && base.distVertical.Abs() < 2f && base.distGrounded > 6f && Game.player.ViewAnglePlane(base.t.position) < 30f)
			{
				Vector3 vector = GetTargetDirGrounded();
				vector = Quaternion.Euler(0f, UnityEngine.Random.Range((0f - dashAngle) / 2f, dashAngle / 2f), 0f) * vector;
				if (NavMesh.SamplePosition(base.t.position + vector * ((float)dashSign * dashDist), out navHit, 1f, -1))
				{
					if (!Physics.Linecast(navHit.position + Vector3.up, base.t.position + Vector3.up, 1))
					{
						targetPosition = navHit.position;
						base.stateMachine.SwitchState(typeof(EnemyDashState));
						Debug.DrawLine(navHit.position, base.t.position, Color.green, 2f);
					}
					else
					{
						Debug.DrawLine(navHit.position, base.t.position, Color.red, 2f);
					}
				}
				else
				{
					Debug.DrawLine(navHit.position, base.t.position, Color.magenta, 2f);
				}
			}
			repositionTimer = MyRandom.Range(0.2f, 1f);
		}
		else
		{
			repositionTimer = Mathf.MoveTowards(repositionTimer, 0f, Time.deltaTime);
		}
	}

	private void Update()
	{
		if (!tTarget || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || !base.rb.isKinematic || !base.agent.enabled)
		{
			return;
		}
		UpdateTargetDistances();
		if (buffed && base.distGrounded.InRange(6f, 20f) && base.distVertical.Abs() < 2f)
		{
			if (fireTimer != 0f)
			{
				fireTimer = Mathf.MoveTowards(fireTimer, 0f, Time.deltaTime);
			}
			else
			{
				if (CrowdControl.instance.GetToken(this))
				{
					Physics.Raycast(GetActualPosition(), GetActualPosition().DirTo(Game.player.tHead.position), out hit, 22f, 1537);
					if (hit.distance != 0f && hit.collider.gameObject.layer == 9)
					{
						base.t.LookAt(tTarget.position.With(null, base.t.position.y));
						ActionStateWithAnim("Attack 2", 2.5f);
						PlaySound(sounds.AltAttack);
						Warning();
					}
				}
				fireTimer = MyRandom.Range(1f, 2f);
			}
		}
		if (!(base.t.position.y - tTarget.position.y < 0f) || !CheckForOffMeshLinks())
		{
			if (Game.player.grounder.grounded && !Game.player.rb.isKinematic)
			{
				targetPosition = tTarget.position + base.t.right * (Mathf.Sin((float)base.randomSeed + Time.timeSinceLevelLoad) * (base.distGrounded / 8f)) * 3f;
			}
			if (!base.stateMachine.CurrentIs(typeof(EnemyActionState)) && base.rb.isKinematic && base.agent.enabled && base.dist < 3f && CrowdControl.instance.GetToken(this))
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				ActionStateWithTrigger("Attack", 1.5f);
				PlaySound(sounds.Attack);
				Warning();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 14 && !other.attachedRigidbody.isKinematic && !base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			base.t.rotation = Quaternion.LookRotation(base.t.position.DirTo(other.transform.position.With(null, base.t.position.y)), Vector3.up);
			actionTime = 0.5f;
			base.animator.SetTrigger("Stun");
			base.stateMachine.SwitchState(typeof(EnemyActionState));
		}
	}
}
