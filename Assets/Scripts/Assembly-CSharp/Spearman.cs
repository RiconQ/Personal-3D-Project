using System;
using System.Collections.Generic;
using UnityEngine;

public class Spearman : BaseEnemy
{
	public TrailScript hTrail;

	public TrailScript vTrail;

	private int attackIndex;

	private float jumpTimer;

	private float strafeTimer;

	private float seed;

	private float seedDist;

	protected override void Awake()
	{
		base.Awake();
		base.agent.updateRotation = false;
		jumpTimer = UnityEngine.Random.Range(0f, 1.5f);
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
			},
			{
				typeof(EnemyWalljumpState),
				new EnemyWalljumpState(this)
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
		if ((bool)tTarget && !(strafeTimer > 0f))
		{
			strafeTimer = 0.2f + Mathf.Sin(Time.timeSinceLevelLoad + base.t.position.x + base.t.position.z) * 0.1f;
		}
	}

	private void Strafing()
	{
		if (base.agent.enabled && (bool)tTarget && !base.stateMachine.CurrentIs(typeof(EnemyActionState)) && !(PlayerController.instance.ViewAngle(base.t.position) > 30f))
		{
			int num = -PlayerController.instance.tHead.forward.SideFromDirection(GetTargetDirGrounded(), Vector3.up);
			base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			if (CheckWarpPos(base.t.position - base.t.right * (num * 3)))
			{
				QuickEffectsPool.Get("Strafe", base.t.position, Quaternion.LookRotation(base.t.right * -num)).Play();
				base.animator.SetTrigger((num == 1) ? "Strafe Right" : "Strafe Left");
				actionTime = 0.4f;
				base.stateMachine.SwitchState(typeof(EnemyActionState));
				targetPosition = base.agent.nextPosition;
				base.isStrafing = 1f;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		hTrail.Reset();
		vTrail.Reset();
		seed = UnityEngine.Random.Range(0f, 10f);
		seedDist = UnityEngine.Random.Range(8f, 16f);
		attackIndex = UnityEngine.Random.Range(0, 3);
		jumpTimer = UnityEngine.Random.Range(0f, 1.5f);
		strafeTimer = 0f;
		actionTime = 0.6f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		switch (index)
		{
		case 0:
			switch (attackIndex)
			{
			case 0:
				vTrail.Play();
				AttackWithTrailBounds(vTrail);
				break;
			case 1:
				hTrail.Play();
				AttackWithTrailBounds(hTrail);
				break;
			}
			break;
		case 1:
			Warp();
			break;
		}
	}

	public override void OnLanded()
	{
		base.OnLanded();
		attackIndex = 0;
	}

	public override void FollowBreak()
	{
		base.FollowBreak();
		if (!TryDirectJump())
		{
			base.t.rotation = Quaternion.LookRotation(GetTargetDirGrounded());
		}
	}

	private static float WrapAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	private void LateUpdate()
	{
		float num = WrapAngle(Quaternion.LookRotation(base.t.InverseTransformDirection(tHead.position.DirTo(Game.player.tHead.position))).eulerAngles.y);
		float z = Mathf.LerpAngle(tHeadMesh.localEulerAngles.z, (num.Abs() > 75f) ? 0f : num, Time.deltaTime * 8f);
		tHeadMesh.localEulerAngles = new Vector3(0f, 0f, z);
	}

	private bool WallJumpCheck()
	{
		Vector3 position = base.t.position;
		position.y += 1f;
		Vector3 forward = base.t.forward;
		forward.y += 0.5f;
		forward.Normalize();
		for (int i = 0; i < 2; i++)
		{
			Vector3 direction = Quaternion.Euler(0f, (i == 0) ? (-70) : 70, 0f) * forward;
			if (!Physics.SphereCast(position, 1f, direction, out hit, 14f, 1))
			{
				continue;
			}
			Debug.DrawLine(position, hit.point, (hit.distance > 4f) ? Color.yellow : Color.red, 2f);
			if (!(hit.distance > 4f) || !(hit.normal.y.Abs() < hit.normal.x.Abs()))
			{
				continue;
			}
			position = hit.point + hit.normal;
			Physics.Raycast(position, position.DirTo(tTarget.position), out var hitInfo, 20f, 513);
			if (hitInfo.distance > 6f && hitInfo.collider.gameObject.layer == 9)
			{
				Debug.DrawLine(base.t.position, hit.point, Color.blue, 2f);
				Physics.Raycast(hitInfo.point, Vector3.down, out hitInfo, 4f, 1);
				if (hitInfo.distance != 0f && CheckNavMeshPos(ref targetPosition, hitInfo.point))
				{
					base.agent.enabled = false;
					targetNormal = position;
					base.t.rotation = Quaternion.LookRotation(position.DirToXZ(tTarget.position));
					base.stateMachine.SwitchState(typeof(EnemyWalljumpState));
					jumpTimer = UnityEngine.Random.Range(1f, 2f);
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	private void Update()
	{
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			return;
		}
		if (staggered)
		{
			targetPosition = base.t.position;
		}
		else
		{
			if (CheckForOffMeshLinks())
			{
				return;
			}
			if (strafeTimer != 0f)
			{
				strafeTimer = Mathf.MoveTowards(strafeTimer, 0f, Time.deltaTime);
				if (strafeTimer == 0f)
				{
					Strafing();
				}
			}
			if (Game.player.grounder.grounded && !Game.player.rb.isKinematic)
			{
				targetPosition = tTarget.position + base.t.right * (Mathf.Sin(seed + Time.timeSinceLevelLoad) * (1f + base.distGrounded / 2f));
			}
			else
			{
				targetPosition = base.t.position;
			}
			UpdateTargetDistances();
			if (buffed)
			{
				UpdateReposition();
			}
			if (base.dist < 3f)
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
				if (attackIndex != 2 && Game.player.weapons.kickController.isCharging && !Game.player.slide.isSliding)
				{
					attackIndex = 2;
				}
				else
				{
					attackIndex = 1;
				}
				switch (attackIndex)
				{
				case 0:
				case 1:
					if (CrowdControl.instance.GetToken(this))
					{
						base.animator.SetInteger("AttackIndex", attackIndex);
						base.animator.SetTrigger("Attack");
						Warning();
						actionTime = 0.8f;
						base.stateMachine.SwitchState(typeof(EnemyActionState));
					}
					break;
				case 2:
					QuickStrafe();
					break;
				}
			}
			else
			{
				if (!(base.dist > 7f))
				{
					return;
				}
				if (jumpTimer <= 0f)
				{
					if (Game.player.ViewAngle(base.rb.worldCenterOfMass) < 60f && CrowdControl.instance.GetToken(this))
					{
						if (!WallJumpCheck())
						{
							if (CheckJumpPosAtPosition(ref targetPosition, tTarget.position - base.t.position.DirToXZ(tTarget.position) * 2f, 1f, 8f))
							{
								Warning();
								base.stateMachine.SwitchState(typeof(EnemyJumpState));
								jumpTimer = UnityEngine.Random.Range(1f, 2f);
							}
						}
						else
						{
							jumpTimer = UnityEngine.Random.Range(0.5f, 1.5f);
						}
					}
					else
					{
						jumpTimer = 0.5f;
					}
				}
				else
				{
					jumpTimer -= Time.deltaTime;
				}
			}
		}
	}

	private bool QuickStrafe()
	{
		int num = UnityEngine.Random.Range(-1f, 1f).Sign();
		if (CheckWarpPos(base.t.position - base.t.right * (3 * num)))
		{
			QuickEffectsPool.Get("Strafe", base.t.position, Quaternion.LookRotation(base.t.right * -num)).Play();
			base.animator.SetTrigger((num == 1) ? "Strafe Right" : "Strafe Left");
			actionTime = 0.5f;
			base.stateMachine.SwitchState(typeof(EnemyActionState));
			return true;
		}
		return false;
	}
}
